using System.ComponentModel.DataAnnotations;

namespace HotelReservations.Data;

/// <summary>A kind of room and its price. Admin-only table.</summary>
public class RoomType
{
    public int RoomTypeId { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = "";

    [Display(Name = "Nightly rate"), Range(0, 99999.99), DataType(DataType.Currency)]
    public decimal NightlyRate { get; set; }

    [Display(Name = "Max occupancy"), Range(1, 12)]
    public int MaxOccupancy { get; set; } = 2;

    [StringLength(250)]
    public string? Description { get; set; }

    [Display(Name = "Active (can be assigned to rooms)")]
    public bool IsActive { get; set; } = true;

    /// <summary>Filled by the list query; not a column.</summary>
    public int RoomCount { get; set; }
}

public class Room
{
    public int RoomId { get; set; }

    [Display(Name = "Room number"), Required, StringLength(10)]
    public string RoomNumber { get; set; } = "";

    [Range(1, 50)]
    public int Floor { get; set; } = 1;

    [Display(Name = "Room type"), Range(1, int.MaxValue, ErrorMessage = "Choose a room type.")]
    public int RoomTypeId { get; set; }

    [Display(Name = "Smoking room")]
    public bool IsSmoking { get; set; }

    [Display(Name = "Out of service")]
    public bool IsOutOfService { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    // Joined from RoomTypes by the read queries.
    public string? RoomTypeName { get; set; }
    public decimal NightlyRate { get; set; }
    public int MaxOccupancy { get; set; }
    public int ReservationCount { get; set; }
}

public class Guest
{
    public int GuestId { get; set; }

    [Display(Name = "First name"), Required, StringLength(50)]
    public string FirstName { get; set; } = "";

    [Display(Name = "Last name"), Required, StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";

    [Phone, StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [Required, StringLength(50)]
    public string Country { get; set; } = "USA";

    [Display(Name = "Guest since"), DataType(DataType.Date)]
    public DateOnly CreatedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public int ReservationCount { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public class Reservation : IValidatableObject
{
    public static readonly string[] Statuses = ["Booked", "Checked In", "Checked Out", "Cancelled"];

    public int ReservationId { get; set; }

    [Display(Name = "Guest"), Range(1, int.MaxValue, ErrorMessage = "Choose a guest.")]
    public int GuestId { get; set; }

    [Display(Name = "Room"), Range(1, int.MaxValue, ErrorMessage = "Choose a room.")]
    public int RoomId { get; set; }

    [Display(Name = "Check-in"), DataType(DataType.Date)]
    public DateOnly CheckInDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Check-out"), DataType(DataType.Date)]
    public DateOnly CheckOutDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Display(Name = "Guests"), Range(1, 12)]
    public int NumberOfGuests { get; set; } = 1;

    [Required]
    public string Status { get; set; } = "Booked";

    [StringLength(250)]
    public string? Notes { get; set; }

    // Joined from Guests / Rooms / RoomTypes by the read queries.
    public string? GuestName { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomTypeName { get; set; }
    public decimal NightlyRate { get; set; }

    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;
    public decimal TotalCost => Nights * NightlyRate;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOutDate <= CheckInDate)
            yield return new ValidationResult("Check-out must be after check-in.", [nameof(CheckOutDate)]);
        if (!Statuses.Contains(Status))
            yield return new ValidationResult("Choose a valid status.", [nameof(Status)]);
    }
}
