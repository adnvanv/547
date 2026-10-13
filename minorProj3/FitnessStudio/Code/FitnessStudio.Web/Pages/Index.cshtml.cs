using FitnessStudio.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudio.Web.Pages;

public class IndexModel(FitnessStudioContext db, FitnessClassService classes) : PageModel
{
    public int MemberCount { get; private set; }
    public int InstructorCount { get; private set; }
    public int ClassCount { get; private set; }
    public int PlanCount { get; private set; }
    public List<FitnessClass> TodaysClasses { get; private set; } = [];
    public string Today { get; } = DateTime.Today.DayOfWeek.ToString();

    public async Task OnGetAsync()
    {
        MemberCount = await db.Members.CountAsync();
        InstructorCount = await db.Instructors.CountAsync();
        ClassCount = await db.FitnessClasses.CountAsync();
        PlanCount = await db.MembershipPlans.CountAsync(p => p.IsActive);
        TodaysClasses = (await classes.GetAllAsync()).Where(c => c.DayOfWeek == Today).ToList();
    }
}
