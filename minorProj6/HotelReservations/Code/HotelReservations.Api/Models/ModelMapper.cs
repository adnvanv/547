using HotelReservations.Business;
using HotelReservations.Data;

namespace HotelReservations.Api.Models;

/// <summary>Converts between business-layer records and the service models sent over HTTP.</summary>
public static class ModelMapper
{
    public static RoomTypeModel ToModel(this RoomTypeDetails d) => new()
    {
        Id = d.RoomType.RoomTypeId,
        Name = d.RoomType.Name,
        NightlyRate = d.RoomType.NightlyRate,
        MaxOccupancy = d.RoomType.MaxOccupancy,
        Description = d.RoomType.Description,
        IsActive = d.RoomType.IsActive,
        RoomCount = d.RoomCount,
    };

    public static RoomType ToEntity(this RoomTypeModel m) => new()
    {
        RoomTypeId = m.Id,
        Name = m.Name,
        NightlyRate = m.NightlyRate,
        MaxOccupancy = m.MaxOccupancy,
        Description = m.Description,
        IsActive = m.IsActive,
    };

    public static RoomModel ToModel(this RoomDetails d) => new()
    {
        Id = d.Room.RoomId,
        RoomNumber = d.Room.RoomNumber,
        Floor = d.Room.Floor,
        RoomTypeId = d.Room.RoomTypeId,
        IsSmoking = d.Room.IsSmoking,
        IsOutOfService = d.Room.IsOutOfService,
        Notes = d.Room.Notes,
        RoomType = d.RoomType.Name,
        NightlyRate = d.RoomType.NightlyRate,
        MaxOccupancy = d.RoomType.MaxOccupancy,
        ReservationCount = d.ReservationCount,
    };

    public static Room ToEntity(this RoomModel m) => new()
    {
        RoomId = m.Id,
        RoomNumber = m.RoomNumber,
        Floor = m.Floor,
        RoomTypeId = m.RoomTypeId,
        IsSmoking = m.IsSmoking,
        IsOutOfService = m.IsOutOfService,
        Notes = m.Notes,
    };

    public static GuestModel ToModel(this GuestDetails d) => new()
    {
        Id = d.Guest.GuestId,
        FirstName = d.Guest.FirstName,
        LastName = d.Guest.LastName,
        Email = d.Guest.Email,
        Phone = d.Guest.Phone,
        City = d.Guest.City,
        Country = d.Guest.Country,
        FullName = d.FullName,
        GuestSince = d.Guest.CreatedOn,
        ReservationCount = d.ReservationCount,
    };

    public static Guest ToEntity(this GuestModel m) => new()
    {
        GuestId = m.Id,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Email = m.Email,
        Phone = m.Phone,
        City = m.City,
        Country = m.Country,
    };

    public static ReservationModel ToModel(this ReservationDetails d) => new()
    {
        Id = d.Reservation.ReservationId,
        GuestId = d.Reservation.GuestId,
        RoomId = d.Reservation.RoomId,
        CheckIn = d.Reservation.CheckInDate,
        CheckOut = d.Reservation.CheckOutDate,
        NumberOfGuests = d.Reservation.NumberOfGuests,
        Status = d.Reservation.Status,
        Notes = d.Reservation.Notes,
        GuestName = $"{d.Guest.FirstName} {d.Guest.LastName}",
        RoomNumber = d.Room.RoomNumber,
        RoomType = d.RoomType.Name,
        NightlyRate = d.RoomType.NightlyRate,
        Nights = d.Nights,
        TotalCost = d.TotalCost,
    };

    public static Reservation ToEntity(this ReservationModel m) => new()
    {
        ReservationId = m.Id,
        GuestId = m.GuestId,
        RoomId = m.RoomId,
        CheckInDate = m.CheckIn,
        CheckOutDate = m.CheckOut,
        NumberOfGuests = m.NumberOfGuests,
        Status = m.Status,
        Notes = m.Notes,
    };
}
