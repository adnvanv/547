namespace VetClinic.Data;

public sealed class Owner
{
    public int OwnerId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int PetCount { get; set; }

    public string FullName => $"{FirstName} {LastName}";
    public override string ToString() => $"{LastName}, {FirstName}";
}

public sealed class Pet
{
    public int PetId { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = "";
    public string Species { get; set; } = "";
    public string? Breed { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal? WeightKg { get; set; }
    public string OwnerName { get; set; } = "";

    public override string ToString() => $"{Name} ({Species}) - {OwnerName}";
}

public sealed class Appointment
{
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = AppointmentStatus.Scheduled;
    public decimal? Cost { get; set; }
    public string PetName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public string VeterinarianName { get; set; } = "";
}

public static class AppointmentStatus
{
    public const string Scheduled = "Scheduled";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All = [Scheduled, Completed, Cancelled];
}

public sealed class Veterinarian
{
    public int VeterinarianId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Specialty { get; set; }
    public bool IsActive { get; set; } = true;
    public int AppointmentCount { get; set; }

    public override string ToString() =>
        $"Dr. {FirstName} {LastName}" + (IsActive ? "" : " (inactive)");
}

/// <summary>Owner columns the user can search on.</summary>
public enum OwnerSearchField
{
    AnyField,
    Id,
    FirstName,
    LastName,
    Email,
    Phone,
    City,
}
