using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelReservations.Api.Models;

// Service models: the JSON shapes the client sends and receives. Read-only properties
// (marked "filled by the service") are computed or joined on the way out and ignored on POST.

public class RoomTypeModel
{
    /// <summary>0 (or omitted) on POST inserts a new room type; an existing id updates it.</summary>
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = "";

    [Range(0, 999999.99)]
    public decimal NightlyRate { get; set; }

    [Range(1, 12)]
    public int MaxOccupancy { get; set; } = 2;

    [StringLength(250)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Filled by the service.</summary>
    public int RoomCount { get; set; }
}

public class RoomModel
{
    public int Id { get; set; }

    [Required, StringLength(10)]
    public string RoomNumber { get; set; } = "";

    [Range(1, 50)]
    public int Floor { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Choose a room type.")]
    public int RoomTypeId { get; set; }

    public bool IsSmoking { get; set; }
    public bool IsOutOfService { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    // Filled by the service.
    public string? RoomType { get; set; }
    public decimal NightlyRate { get; set; }
    public int MaxOccupancy { get; set; }
    public int ReservationCount { get; set; }
}

public class GuestModel
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string Country { get; set; } = "USA";

    // Filled by the service.
    public string? FullName { get; set; }
    public DateOnly GuestSince { get; set; }
    public int ReservationCount { get; set; }
}

public class ReservationModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a guest.")]
    public int GuestId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a room.")]
    public int RoomId { get; set; }

    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }

    [Range(1, 12)]
    public int NumberOfGuests { get; set; } = 1;

    /// <summary>Booked, Checked In, Checked Out or Cancelled. Defaults to Booked.</summary>
    [StringLength(20)]
    public string Status { get; set; } = "Booked";

    [StringLength(250)]
    public string? Notes { get; set; }

    // Filled by the service.
    public string? GuestName { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomType { get; set; }
    public decimal NightlyRate { get; set; }
    public int Nights { get; set; }
    public decimal TotalCost { get; set; }
}

/// <summary>Returned by POST: whether a record was created or updated, plus the saved record.</summary>
public record SaveResult<T>(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("record")] T Record);
