using Microsoft.EntityFrameworkCore;

namespace FitnessStudio.Data;

/// <summary>
/// EF Core context mapped onto the tables created by Scripts/01_FitnessStudioDB_DDL.sql.
/// The schema is owned by the SQL scripts, so there are no EF migrations.
/// </summary>
public class FitnessStudioContext(DbContextOptions<FitnessStudioContext> options) : DbContext(options)
{
    public const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=FitnessStudioDB;Integrated Security=True;TrustServerCertificate=True;";

    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<FitnessClass> FitnessClasses => Set<FitnessClass>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MembershipPlan>().ToTable("MembershipPlans");
        modelBuilder.Entity<Member>().ToTable("Members");
        modelBuilder.Entity<Instructor>().ToTable("Instructors");
        modelBuilder.Entity<FitnessClass>().ToTable("FitnessClasses");

        // Deleting a parent row must never silently delete its children;
        // the services turn the resulting FK error into a friendly message.
        modelBuilder.Entity<Member>()
            .HasOne(m => m.Plan).WithMany(p => p.Members)
            .HasForeignKey(m => m.PlanId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FitnessClass>()
            .HasOne(c => c.Instructor).WithMany(i => i.Classes)
            .HasForeignKey(c => c.InstructorId).OnDelete(DeleteBehavior.Restrict);
    }
}
