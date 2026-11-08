namespace HotelReservations.Data;

// Data objects: one class per table, one property per column (same names and nullability).

/// <summary>dbo.RoomTypes</summary>
public class RoomType
{
    public int RoomTypeId { get; set; }
    public string Name { get; set; } = "";
    public decimal NightlyRate { get; set; }
    public int MaxOccupancy { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>dbo.Rooms</summary>
public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = "";
    public int Floor { get; set; }
    public int RoomTypeId { get; set; }
    public bool IsSmoking { get; set; }
    public bool IsOutOfService { get; set; }
    public string? Notes { get; set; }
}

/// <summary>dbo.Guests</summary>
public class Guest
{
    public int GuestId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "USA";
    public DateOnly CreatedOn { get; set; }
}

/// <summary>dbo.Reservations</summary>
public class Reservation
{
    public int ReservationId { get; set; }
    public int GuestId { get; set; }
    public int RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string Status { get; set; } = "Booked";
    public string? Notes { get; set; }
}
