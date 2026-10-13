using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessStudio.Data;

/// <summary>Admin-only table: only the Administration page can change these rows.</summary>
public class MembershipPlan
{
    [Key]
    public int PlanId { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = "";

    [Display(Name = "Monthly price")]
    [Range(0, 999999.99), Column(TypeName = "decimal(8,2)"), DataType(DataType.Currency)]
    public decimal MonthlyPrice { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<Member> Members { get; set; } = [];
}

public class Member
{
    [Key]
    public int MemberId { get; set; }

    [Required, StringLength(50), Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50), Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = "";

    [StringLength(20), Phone]
    public string? Phone { get; set; }

    [Display(Name = "Join date"), DataType(DataType.Date)]
    public DateOnly JoinDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Plan")]
    public int PlanId { get; set; }

    public MembershipPlan? Plan { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}

public class Instructor
{
    [Key]
    public int InstructorId { get; set; }

    [Required, StringLength(50), Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50), Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = "";

    [Required, StringLength(50)]
    public string Specialty { get; set; } = "";

    [Display(Name = "Hire date"), DataType(DataType.Date)]
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<FitnessClass> Classes { get; set; } = [];

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}

public class FitnessClass
{
    public static readonly string[] Days =
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    [Key]
    public int ClassId { get; set; }

    [Required, StringLength(80)]
    public string Title { get; set; } = "";

    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }

    public Instructor? Instructor { get; set; }

    [Required, StringLength(10), Display(Name = "Day")]
    public string DayOfWeek { get; set; } = "Monday";

    [Display(Name = "Start time"), DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; } = new(9, 0);

    [Display(Name = "Duration (min)"), Range(15, 180)]
    public int DurationMinutes { get; set; } = 60;

    [Range(1, 100)]
    public int Capacity { get; set; } = 20;

    [Required, StringLength(30)]
    public string Room { get; set; } = "";
}
