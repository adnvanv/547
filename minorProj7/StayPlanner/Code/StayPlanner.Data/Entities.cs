namespace StayPlanner.Data;

/// <summary>A hotel room, loaded from the embedded rooms.json seed file.</summary>
public sealed record Room(int Id, string Number, string Type, int Beds, int MaxGuests, decimal NightlyRate, int Floor);

/// <summary>A booking saved to the local reservations file.</summary>
public sealed record Reservation(
    int Id,
    int RoomId,
    string GuestName,
    int Guests,
    DateOnly CheckIn,
    DateOnly CheckOut,
    decimal Total,
    DateTime CreatedUtc,
    string CreatedOn);
