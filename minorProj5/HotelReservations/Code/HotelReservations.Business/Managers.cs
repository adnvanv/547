using System.Net.Mail;
using HotelReservations.Data;

namespace HotelReservations.Business;

// Business layer: wraps the data layer's Insert/Update/Delete in friendlier operations
// (Save = insert or update, Cancel, Search, GetAvailable...) and enforces the hotel's rules.

internal static class Guard
{
    /// <summary>Runs a data layer call and rewords its errors as business errors.</summary>
    public static async Task<T> DataCall<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (DataLayerException ex)
        {
            throw new BusinessRuleException(ex.Message, ex);
        }
    }

    public static void Require(bool condition, string message)
    {
        if (!condition)
            throw new BusinessRuleException(message);
    }

    public static void FoundOne(int rows, string what, int id)
    {
        if (rows == 0)
            throw new NotFoundException(what, id);
    }

    public static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}

public class RoomTypeManager(RoomTypeRepository roomTypes, RoomRepository rooms)
{
    public async Task<List<RoomTypeDetails>> GetAllAsync(bool activeOnly = false)
    {
        var roomCounts = (await rooms.SelectAllAsync()).CountBy(r => r.RoomTypeId).ToDictionary();
        return (await roomTypes.SelectAllAsync())
            .Where(t => !activeOnly || t.IsActive)
            .Select(t => new RoomTypeDetails(t, roomCounts.GetValueOrDefault(t.RoomTypeId)))
            .ToList();
    }

    public async Task<RoomTypeDetails?> GetAsync(int id)
    {
        var type = await roomTypes.SelectByIdAsync(id);
        if (type is null)
            return null;
        return new RoomTypeDetails(type, (await rooms.SelectAllAsync()).Count(r => r.RoomTypeId == id));
    }

    /// <summary>Inserts when RoomTypeId is 0, otherwise updates. Returns the room type's id.</summary>
    public async Task<int> SaveAsync(RoomType type)
    {
        type.Name = type.Name.Trim();
        type.Description = Guard.Clean(type.Description);
        Guard.Require(type.Name.Length is > 0 and <= 50, "Room type name is required (50 characters max).");
        Guard.Require(type.NightlyRate is >= 0 and < 1_000_000, "Nightly rate must be between $0 and $999,999.99.");
        Guard.Require(type.MaxOccupancy is >= 1 and <= 12, "Max occupancy must be between 1 and 12.");

        if (type.RoomTypeId == 0)
            return await Guard.DataCall(() => roomTypes.InsertAsync(type));

        Guard.FoundOne(await Guard.DataCall(() => roomTypes.UpdateAsync(type)), "Room type", type.RoomTypeId);
        return type.RoomTypeId;
    }

    public async Task DeleteAsync(int id) =>
        Guard.FoundOne(await Guard.DataCall(() => roomTypes.DeleteAsync(id)), "Room type", id);
}

public class RoomManager(RoomRepository rooms, RoomTypeRepository roomTypes, ReservationRepository reservations)
{
    public async Task<List<RoomDetails>> GetAllAsync()
    {
        var types = (await roomTypes.SelectAllAsync()).ToDictionary(t => t.RoomTypeId);
        var bookingCounts = (await reservations.SelectAllAsync()).CountBy(r => r.RoomId).ToDictionary();
        return (await rooms.SelectAllAsync())
            .Select(r => new RoomDetails(r, types[r.RoomTypeId], bookingCounts.GetValueOrDefault(r.RoomId)))
            .ToList();
    }

    public async Task<RoomDetails?> GetAsync(int id) => (await GetAllAsync()).FirstOrDefault(r => r.Room.RoomId == id);

    /// <summary>Rooms in service that can hold the party and have no active booking overlapping the dates.</summary>
    public async Task<List<RoomDetails>> GetAvailableAsync(DateOnly checkIn, DateOnly checkOut, int numberOfGuests = 1)
    {
        Guard.Require(checkOut > checkIn, "Check-out must be after check-in.");
        var booked = (await reservations.SelectAllAsync())
            .Where(r => r.Status != ReservationStatus.Cancelled && r.CheckInDate < checkOut && checkIn < r.CheckOutDate)
            .Select(r => r.RoomId)
            .ToHashSet();
        return (await GetAllAsync())
            .Where(r => !r.Room.IsOutOfService && r.RoomType.MaxOccupancy >= numberOfGuests && !booked.Contains(r.Room.RoomId))
            .ToList();
    }

    /// <summary>Inserts when RoomId is 0, otherwise updates. Returns the room's id.</summary>
    public async Task<int> SaveAsync(Room room)
    {
        room.RoomNumber = room.RoomNumber.Trim();
        room.Notes = Guard.Clean(room.Notes);
        Guard.Require(room.RoomNumber.Length is > 0 and <= 10, "Room number is required (10 characters max).");
        Guard.Require(room.Floor is >= 1 and <= 50, "Floor must be between 1 and 50.");

        var type = await roomTypes.SelectByIdAsync(room.RoomTypeId);
        Guard.Require(type is not null, $"Room type {room.RoomTypeId} does not exist.");
        var existing = room.RoomId == 0 ? null : await rooms.SelectByIdAsync(room.RoomId);
        // A retired room type can stay on rooms that already have it, but can't be given to new ones.
        Guard.Require(type!.IsActive || existing?.RoomTypeId == type.RoomTypeId,
                      $"Room type \"{type.Name}\" is inactive and can't be assigned to rooms.");

        if (room.RoomId == 0)
            return await Guard.DataCall(() => rooms.InsertAsync(room));

        Guard.FoundOne(await Guard.DataCall(() => rooms.UpdateAsync(room)), "Room", room.RoomId);
        return room.RoomId;
    }

    public async Task DeleteAsync(int id) =>
        Guard.FoundOne(await Guard.DataCall(() => rooms.DeleteAsync(id)), "Room", id);
}

public class GuestManager(GuestRepository guests, ReservationRepository reservations)
{
    public async Task<List<GuestDetails>> GetAllAsync()
    {
        var bookingCounts = (await reservations.SelectAllAsync()).CountBy(r => r.GuestId).ToDictionary();
        return (await guests.SelectAllAsync())
            .Select(g => new GuestDetails(g, bookingCounts.GetValueOrDefault(g.GuestId)))
            .ToList();
    }

    public async Task<GuestDetails?> GetAsync(int id) => (await GetAllAsync()).FirstOrDefault(g => g.Guest.GuestId == id);

    /// <summary>Case-insensitive match on name, email, phone or city. Blank text returns everyone.</summary>
    public async Task<List<GuestDetails>> SearchAsync(string? text)
    {
        var all = await GetAllAsync();
        if (string.IsNullOrWhiteSpace(text))
            return all;
        text = text.Trim();
        return all.Where(d =>
            new[] { d.FullName, d.Guest.Email, d.Guest.Phone, d.Guest.City }
                .Any(field => field?.Contains(text, StringComparison.OrdinalIgnoreCase) == true))
            .ToList();
    }

    /// <summary>Inserts when GuestId is 0, otherwise updates. Returns the guest's id.</summary>
    public async Task<int> SaveAsync(Guest guest)
    {
        guest.FirstName = guest.FirstName.Trim();
        guest.LastName = guest.LastName.Trim();
        guest.Email = guest.Email.Trim().ToLowerInvariant();
        guest.Phone = Guard.Clean(guest.Phone);
        guest.City = Guard.Clean(guest.City);
        guest.Country = Guard.Clean(guest.Country) ?? "USA";
        Guard.Require(guest.FirstName.Length is > 0 and <= 50, "First name is required (50 characters max).");
        Guard.Require(guest.LastName.Length is > 0 and <= 50, "Last name is required (50 characters max).");
        Guard.Require(MailAddress.TryCreate(guest.Email, out _) && guest.Email.Length <= 100,
                      "A valid email address is required.");

        if (guest.GuestId == 0)
        {
            if (guest.CreatedOn == default)
                guest.CreatedOn = DateOnly.FromDateTime(DateTime.Today);
            return await Guard.DataCall(() => guests.InsertAsync(guest));
        }

        // "Guest since" never changes after the guest is created.
        var existing = await guests.SelectByIdAsync(guest.GuestId) ?? throw new NotFoundException("Guest", guest.GuestId);
        guest.CreatedOn = existing.CreatedOn;
        Guard.FoundOne(await Guard.DataCall(() => guests.UpdateAsync(guest)), "Guest", guest.GuestId);
        return guest.GuestId;
    }

    public async Task DeleteAsync(int id) =>
        Guard.FoundOne(await Guard.DataCall(() => guests.DeleteAsync(id)), "Guest", id);
}

public class ReservationManager(ReservationRepository reservations, GuestRepository guests,
                                RoomRepository rooms, RoomTypeRepository roomTypes)
{
    public async Task<List<ReservationDetails>> GetAllAsync(bool includeCancelled = true)
    {
        var guestsById = (await guests.SelectAllAsync()).ToDictionary(g => g.GuestId);
        var roomsById = (await rooms.SelectAllAsync()).ToDictionary(r => r.RoomId);
        var typesById = (await roomTypes.SelectAllAsync()).ToDictionary(t => t.RoomTypeId);
        return (await reservations.SelectAllAsync())
            .Where(r => includeCancelled || r.Status != ReservationStatus.Cancelled)
            .Select(r =>
            {
                var room = roomsById[r.RoomId];
                return new ReservationDetails(r, guestsById[r.GuestId], room, typesById[room.RoomTypeId]);
            })
            .ToList();
    }

    public async Task<ReservationDetails?> GetAsync(int id) =>
        (await GetAllAsync()).FirstOrDefault(r => r.Reservation.ReservationId == id);

    public async Task<List<ReservationDetails>> GetForGuestAsync(int guestId) =>
        (await GetAllAsync()).Where(r => r.Reservation.GuestId == guestId).ToList();

    /// <summary>
    /// Inserts when ReservationId is 0, otherwise updates. Checks dates, status, the guest and room exist,
    /// the room is in service and big enough, and the room isn't double booked. Returns the reservation's id.
    /// </summary>
    public async Task<int> SaveAsync(Reservation res)
    {
        res.Status = string.IsNullOrWhiteSpace(res.Status) ? ReservationStatus.Booked : res.Status.Trim();
        res.Notes = Guard.Clean(res.Notes);
        Guard.Require(ReservationStatus.All.Contains(res.Status),
                      "Status must be one of: " + string.Join(", ", ReservationStatus.All) + ".");
        Guard.Require(res.CheckOutDate > res.CheckInDate, "Check-out must be after check-in.");
        Guard.Require(res.NumberOfGuests >= 1, "A reservation needs at least one guest.");
        Guard.Require(await guests.SelectByIdAsync(res.GuestId) is not null, $"Guest {res.GuestId} does not exist.");
        var room = await rooms.SelectByIdAsync(res.RoomId);
        Guard.Require(room is not null, $"Room {res.RoomId} does not exist.");

        if (res.Status != ReservationStatus.Cancelled)
        {
            var type = (await roomTypes.SelectByIdAsync(room!.RoomTypeId))!;
            Guard.Require(res.NumberOfGuests <= type.MaxOccupancy,
                          $"Room {room.RoomNumber} sleeps at most {type.MaxOccupancy} guest(s).");
            Guard.Require(!room.IsOutOfService || res.ReservationId != 0,
                          $"Room {room.RoomNumber} is out of service.");

            var conflict = (await reservations.SelectAllAsync()).FirstOrDefault(r =>
                r.RoomId == res.RoomId && r.ReservationId != res.ReservationId &&
                r.Status != ReservationStatus.Cancelled &&
                r.CheckInDate < res.CheckOutDate && res.CheckInDate < r.CheckOutDate);
            Guard.Require(conflict is null,
                          $"Room {room.RoomNumber} is already booked by reservation #{conflict?.ReservationId} " +
                          $"({conflict?.CheckInDate:yyyy-MM-dd} to {conflict?.CheckOutDate:yyyy-MM-dd}).");
        }

        if (res.ReservationId == 0)
            return await Guard.DataCall(() => reservations.InsertAsync(res));

        Guard.FoundOne(await Guard.DataCall(() => reservations.UpdateAsync(res)), "Reservation", res.ReservationId);
        return res.ReservationId;
    }

    /// <summary>Marks a reservation cancelled (keeps the history, frees the room).</summary>
    public async Task CancelAsync(int id)
    {
        var res = await reservations.SelectByIdAsync(id) ?? throw new NotFoundException("Reservation", id);
        Guard.Require(res.Status != ReservationStatus.CheckedOut, "A checked-out reservation can't be cancelled.");
        res.Status = ReservationStatus.Cancelled;
        await Guard.DataCall(() => reservations.UpdateAsync(res));
    }

    public async Task DeleteAsync(int id) =>
        Guard.FoundOne(await Guard.DataCall(() => reservations.DeleteAsync(id)), "Reservation", id);
}
