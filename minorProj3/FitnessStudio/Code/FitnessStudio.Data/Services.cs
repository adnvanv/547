using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudio.Data;

/// <summary>Database errors worded for the user (e.g. a delete blocked by a foreign key).</summary>
public sealed class DataLayerException(string message, Exception? inner = null) : Exception(message, inner);

public enum MemberSearchField { Any, FirstName, LastName, Email, Phone }

public enum MatchMode { Contains, StartsWith, EndsWith }

/// <summary>Shared save/delete logic that turns SQL Server errors into <see cref="DataLayerException"/>s.</summary>
public abstract class ServiceBase(FitnessStudioContext db)
{
    protected FitnessStudioContext Db { get; } = db;

    protected async Task SaveAsync(string inUseMessage)
    {
        try
        {
            await Db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql)
        {
            Db.ChangeTracker.Clear();
            throw sql.Number switch
            {
                547 => new DataLayerException(inUseMessage, ex),               // FK / CHECK constraint
                2627 or 2601 => new DataLayerException(                       // UNIQUE constraint
                    "Another record already uses that value (email addresses and plan names must be unique).", ex),
                _ => new DataLayerException("The database rejected the change: " + sql.Message, ex),
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Db.ChangeTracker.Clear();
            throw new DataLayerException("That record no longer exists. It may have been deleted already.", ex);
        }
    }

    protected async Task DeleteAsync<T>(int id, string inUseMessage) where T : class
    {
        var entity = await Db.Set<T>().FindAsync(id)
                     ?? throw new DataLayerException("That record no longer exists. It may have been deleted already.");
        Db.Remove(entity);
        await SaveAsync(inUseMessage);
    }
}

public class MemberService(FitnessStudioContext db) : ServiceBase(db)
{
    /// <summary>
    /// Searches members by ID (when the text is a number) or by name, email or phone,
    /// using EF's Contains / StartsWith / EndsWith (translated to SQL LIKE).
    /// </summary>
    public Task<List<Member>> SearchAsync(string? text, MemberSearchField field = MemberSearchField.Any,
                                          MatchMode mode = MatchMode.Contains)
    {
        IQueryable<Member> query = Db.Members.AsNoTracking().Include(m => m.Plan);
        text = text?.Trim();

        if (!string.IsNullOrEmpty(text))
        {
            query = field switch
            {
                MemberSearchField.FirstName => query.Where(Match(m => m.FirstName, text, mode)),
                MemberSearchField.LastName => query.Where(Match(m => m.LastName, text, mode)),
                MemberSearchField.Email => query.Where(Match(m => m.Email, text, mode)),
                MemberSearchField.Phone => query.Where(Match(m => m.Phone!, text, mode)),
                _ => int.TryParse(text, out var id)
                    ? query.Where(m => m.MemberId == id)
                    : query.Where(Match(m => m.FirstName, text, mode))
                        .Union(query.Where(Match(m => m.LastName, text, mode)))
                        .Union(query.Where(Match(m => m.Email, text, mode)))
                        .Union(query.Where(Match(m => m.Phone!, text, mode))),
            };
        }

        return query.OrderBy(m => m.LastName).ThenBy(m => m.FirstName).ToListAsync();
    }

    // Builds m => selector(m).Contains(text) (or StartsWith/EndsWith) so EF can translate it to LIKE.
    private static System.Linq.Expressions.Expression<Func<Member, bool>> Match(
        System.Linq.Expressions.Expression<Func<Member, string>> selector, string text, MatchMode mode)
    {
        var method = typeof(string).GetMethod(mode.ToString(), [typeof(string)])!;
        var body = System.Linq.Expressions.Expression.AndAlso(
            System.Linq.Expressions.Expression.NotEqual(selector.Body,
                System.Linq.Expressions.Expression.Constant(null, typeof(string))),
            System.Linq.Expressions.Expression.Call(selector.Body, method,
                System.Linq.Expressions.Expression.Constant(text)));
        return System.Linq.Expressions.Expression.Lambda<Func<Member, bool>>(body, selector.Parameters);
    }

    public Task<Member?> GetAsync(int id) =>
        Db.Members.AsNoTracking().Include(m => m.Plan).FirstOrDefaultAsync(m => m.MemberId == id);

    public async Task AddAsync(Member member)
    {
        Db.Members.Add(member);
        await SaveAsync("The selected membership plan does not exist.");
    }

    public async Task UpdateAsync(Member member)
    {
        Db.Members.Update(member);
        await SaveAsync("The selected membership plan does not exist.");
    }

    public Task DeleteAsync(int id) => DeleteAsync<Member>(id, "This member could not be deleted.");
}

public class InstructorService(FitnessStudioContext db) : ServiceBase(db)
{
    public Task<List<Instructor>> GetAllAsync() =>
        Db.Instructors.AsNoTracking().Include(i => i.Classes)
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToListAsync();

    public Task<Instructor?> GetAsync(int id) =>
        Db.Instructors.AsNoTracking().Include(i => i.Classes).FirstOrDefaultAsync(i => i.InstructorId == id);

    public async Task AddAsync(Instructor instructor)
    {
        Db.Instructors.Add(instructor);
        await SaveAsync("This instructor could not be saved.");
    }

    public async Task UpdateAsync(Instructor instructor)
    {
        Db.Instructors.Update(instructor);
        await SaveAsync("This instructor could not be saved.");
    }

    public Task DeleteAsync(int id) => DeleteAsync<Instructor>(id,
        "This instructor still teaches classes. Reassign or delete those classes first.");
}

public class FitnessClassService(FitnessStudioContext db) : ServiceBase(db)
{
    public async Task<List<FitnessClass>> GetAllAsync()
    {
        var classes = await Db.FitnessClasses.AsNoTracking().Include(c => c.Instructor).ToListAsync();
        // Order Monday..Sunday, then by start time.
        return classes.OrderBy(c => Array.IndexOf(FitnessClass.Days, c.DayOfWeek))
                      .ThenBy(c => c.StartTime).ToList();
    }

    public Task<FitnessClass?> GetAsync(int id) =>
        Db.FitnessClasses.AsNoTracking().Include(c => c.Instructor).FirstOrDefaultAsync(c => c.ClassId == id);

    public async Task AddAsync(FitnessClass fitnessClass)
    {
        Db.FitnessClasses.Add(fitnessClass);
        await SaveAsync("The selected instructor does not exist, or a value is out of range.");
    }

    public async Task UpdateAsync(FitnessClass fitnessClass)
    {
        Db.FitnessClasses.Update(fitnessClass);
        await SaveAsync("The selected instructor does not exist, or a value is out of range.");
    }

    public Task DeleteAsync(int id) => DeleteAsync<FitnessClass>(id, "This class could not be deleted.");
}

/// <summary>Membership plans. Only the Administration page calls the write methods.</summary>
public class MembershipPlanService(FitnessStudioContext db) : ServiceBase(db)
{
    public Task<List<MembershipPlan>> GetAllAsync(bool activeOnly = false) =>
        Db.MembershipPlans.AsNoTracking().Include(p => p.Members)
            .Where(p => !activeOnly || p.IsActive)
            .OrderBy(p => p.MonthlyPrice).ToListAsync();

    public Task<MembershipPlan?> GetAsync(int id) =>
        Db.MembershipPlans.AsNoTracking().Include(p => p.Members).FirstOrDefaultAsync(p => p.PlanId == id);

    public async Task AddAsync(MembershipPlan plan)
    {
        Db.MembershipPlans.Add(plan);
        await SaveAsync("This plan could not be saved.");
    }

    public async Task UpdateAsync(MembershipPlan plan)
    {
        Db.MembershipPlans.Update(plan);
        await SaveAsync("This plan could not be saved.");
    }

    public Task DeleteAsync(int id) => DeleteAsync<MembershipPlan>(id,
        "Members are still on this plan. Move them to another plan, or mark this plan inactive instead.");
}
