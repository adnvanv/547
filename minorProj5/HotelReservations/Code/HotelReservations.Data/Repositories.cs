using Microsoft.Data.SqlClient;
using static HotelReservations.Data.SqlDatabase;

namespace HotelReservations.Data;

// Data layer: plain CRUD per table (SelectAll, SelectById, Insert, Update, Delete).
// Business rules and friendlier method names live in HotelReservations.Business.

public class RoomTypeRepository(SqlDatabase db)
{
    private const string Select =
        "SELECT RoomTypeId, Name, NightlyRate, MaxOccupancy, Description, IsActive FROM dbo.RoomTypes";

    public Task<List<RoomType>> SelectAllAsync() => db.QueryAsync(Select + " ORDER BY NightlyRate, Name", Map);

    public Task<RoomType?> SelectByIdAsync(int id) =>
        db.QuerySingleAsync(Select + " WHERE RoomTypeId = @Id", Map, P("@Id", id));

    public Task<int> InsertAsync(RoomType t) => db.InsertAsync("""
        INSERT INTO dbo.RoomTypes (Name, NightlyRate, MaxOccupancy, Description, IsActive)
        OUTPUT INSERTED.RoomTypeId
        VALUES (@Name, @NightlyRate, @MaxOccupancy, @Description, @IsActive)
        """, "The room type has a value out of range.", Parameters(t));

    public Task<int> UpdateAsync(RoomType t) => db.ExecuteAsync("""
        UPDATE dbo.RoomTypes
        SET Name = @Name, NightlyRate = @NightlyRate, MaxOccupancy = @MaxOccupancy,
            Description = @Description, IsActive = @IsActive
        WHERE RoomTypeId = @RoomTypeId
        """, "The room type has a value out of range.", [.. Parameters(t), P("@RoomTypeId", t.RoomTypeId)]);

    public Task<int> DeleteAsync(int id) => db.ExecuteAsync(
        "DELETE FROM dbo.RoomTypes WHERE RoomTypeId = @Id",
        "Rooms still use this room type. Change those rooms first, or mark this type inactive instead.",
        P("@Id", id));

    private static SqlParameter[] Parameters(RoomType t) =>
    [
        P("@Name", t.Name, 50), P("@NightlyRate", t.NightlyRate), P("@MaxOccupancy", t.MaxOccupancy),
        P("@Description", t.Description, 250), P("@IsActive", t.IsActive),
    ];

    private static RoomType Map(SqlDataReader r) => new()
    {
        RoomTypeId = r.Int("RoomTypeId"),
        Name = r.Str("Name"),
        NightlyRate = r.Dec("NightlyRate"),
        MaxOccupancy = r.Int("MaxOccupancy"),
        Description = r.GetNullableString("Description"),
        IsActive = r.Bool("IsActive"),
    };
}

public class RoomRepository(SqlDatabase db)
{
    private const string Select =
        "SELECT RoomId, RoomNumber, Floor, RoomTypeId, IsSmoking, IsOutOfService, Notes FROM dbo.Rooms";

    public Task<List<Room>> SelectAllAsync() => db.QueryAsync(Select + " ORDER BY Floor, RoomNumber", Map);

    public Task<Room?> SelectByIdAsync(int id) =>
        db.QuerySingleAsync(Select + " WHERE RoomId = @Id", Map, P("@Id", id));

    public Task<int> InsertAsync(Room room) => db.InsertAsync("""
        INSERT INTO dbo.Rooms (RoomNumber, Floor, RoomTypeId, IsSmoking, IsOutOfService, Notes)
        OUTPUT INSERTED.RoomId
        VALUES (@RoomNumber, @Floor, @RoomTypeId, @IsSmoking, @IsOutOfService, @Notes)
        """, "The selected room type does not exist, or a value is out of range.", Parameters(room));

    public Task<int> UpdateAsync(Room room) => db.ExecuteAsync("""
        UPDATE dbo.Rooms
        SET RoomNumber = @RoomNumber, Floor = @Floor, RoomTypeId = @RoomTypeId,
            IsSmoking = @IsSmoking, IsOutOfService = @IsOutOfService, Notes = @Notes
        WHERE RoomId = @RoomId
        """, "The selected room type does not exist, or a value is out of range.",
        [.. Parameters(room), P("@RoomId", room.RoomId)]);

    public Task<int> DeleteAsync(int id) => db.ExecuteAsync(
        "DELETE FROM dbo.Rooms WHERE RoomId = @Id",
        "This room has reservations. Delete those reservations first, or mark the room out of service.",
        P("@Id", id));

    private static SqlParameter[] Parameters(Room room) =>
    [
        P("@RoomNumber", room.RoomNumber, 10), P("@Floor", room.Floor), P("@RoomTypeId", room.RoomTypeId),
        P("@IsSmoking", room.IsSmoking), P("@IsOutOfService", room.IsOutOfService), P("@Notes", room.Notes, 250),
    ];

    private static Room Map(SqlDataReader r) => new()
    {
        RoomId = r.Int("RoomId"),
        RoomNumber = r.Str("RoomNumber"),
        Floor = r.Int("Floor"),
        RoomTypeId = r.Int("RoomTypeId"),
        IsSmoking = r.Bool("IsSmoking"),
        IsOutOfService = r.Bool("IsOutOfService"),
        Notes = r.GetNullableString("Notes"),
    };
}

public class GuestRepository(SqlDatabase db)
{
    private const string Select =
        "SELECT GuestId, FirstName, LastName, Email, Phone, City, Country, CreatedOn FROM dbo.Guests";

    public Task<List<Guest>> SelectAllAsync() => db.QueryAsync(Select + " ORDER BY LastName, FirstName", Map);

    public Task<Guest?> SelectByIdAsync(int id) =>
        db.QuerySingleAsync(Select + " WHERE GuestId = @Id", Map, P("@Id", id));

    public Task<int> InsertAsync(Guest g) => db.InsertAsync("""
        INSERT INTO dbo.Guests (FirstName, LastName, Email, Phone, City, Country, CreatedOn)
        OUTPUT INSERTED.GuestId
        VALUES (@FirstName, @LastName, @Email, @Phone, @City, @Country, @CreatedOn)
        """, "This guest could not be saved.", Parameters(g));

    public Task<int> UpdateAsync(Guest g) => db.ExecuteAsync("""
        UPDATE dbo.Guests
        SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone,
            City = @City, Country = @Country, CreatedOn = @CreatedOn
        WHERE GuestId = @GuestId
        """, "This guest could not be saved.", [.. Parameters(g), P("@GuestId", g.GuestId)]);

    public Task<int> DeleteAsync(int id) => db.ExecuteAsync(
        "DELETE FROM dbo.Guests WHERE GuestId = @Id",
        "This guest still has reservations. Delete or reassign those reservations first.", P("@Id", id));

    private static SqlParameter[] Parameters(Guest g) =>
    [
        P("@FirstName", g.FirstName, 50), P("@LastName", g.LastName, 50), P("@Email", g.Email, 100),
        P("@Phone", g.Phone, 20), P("@City", g.City, 50), P("@Country", g.Country, 50),
        P("@CreatedOn", g.CreatedOn),
    ];

    private static Guest Map(SqlDataReader r) => new()
    {
        GuestId = r.Int("GuestId"),
        FirstName = r.Str("FirstName"),
        LastName = r.Str("LastName"),
        Email = r.Str("Email"),
        Phone = r.GetNullableString("Phone"),
        City = r.GetNullableString("City"),
        Country = r.Str("Country"),
        CreatedOn = r.Date("CreatedOn"),
    };
}

public class ReservationRepository(SqlDatabase db)
{
    private const string Select = """
        SELECT ReservationId, GuestId, RoomId, CheckInDate, CheckOutDate, NumberOfGuests, Status, Notes
        FROM dbo.Reservations
        """;

    private const string InvalidReference = "The selected guest or room does not exist, or a value is out of range.";

    public Task<List<Reservation>> SelectAllAsync() =>
        db.QueryAsync(Select + " ORDER BY CheckInDate, ReservationId", Map);

    public Task<Reservation?> SelectByIdAsync(int id) =>
        db.QuerySingleAsync(Select + " WHERE ReservationId = @Id", Map, P("@Id", id));

    public Task<int> InsertAsync(Reservation res) => db.InsertAsync("""
        INSERT INTO dbo.Reservations (GuestId, RoomId, CheckInDate, CheckOutDate, NumberOfGuests, Status, Notes)
        OUTPUT INSERTED.ReservationId
        VALUES (@GuestId, @RoomId, @CheckInDate, @CheckOutDate, @NumberOfGuests, @Status, @Notes)
        """, InvalidReference, Parameters(res));

    public Task<int> UpdateAsync(Reservation res) => db.ExecuteAsync("""
        UPDATE dbo.Reservations
        SET GuestId = @GuestId, RoomId = @RoomId, CheckInDate = @CheckInDate, CheckOutDate = @CheckOutDate,
            NumberOfGuests = @NumberOfGuests, Status = @Status, Notes = @Notes
        WHERE ReservationId = @ReservationId
        """, InvalidReference, [.. Parameters(res), P("@ReservationId", res.ReservationId)]);

    public Task<int> DeleteAsync(int id) => db.ExecuteAsync(
        "DELETE FROM dbo.Reservations WHERE ReservationId = @Id", "This reservation could not be deleted.", P("@Id", id));

    private static SqlParameter[] Parameters(Reservation res) =>
    [
        P("@GuestId", res.GuestId), P("@RoomId", res.RoomId), P("@CheckInDate", res.CheckInDate),
        P("@CheckOutDate", res.CheckOutDate), P("@NumberOfGuests", res.NumberOfGuests),
        P("@Status", res.Status, 20), P("@Notes", res.Notes, 250),
    ];

    private static Reservation Map(SqlDataReader r) => new()
    {
        ReservationId = r.Int("ReservationId"),
        GuestId = r.Int("GuestId"),
        RoomId = r.Int("RoomId"),
        CheckInDate = r.Date("CheckInDate"),
        CheckOutDate = r.Date("CheckOutDate"),
        NumberOfGuests = r.Int("NumberOfGuests"),
        Status = r.Str("Status"),
        Notes = r.GetNullableString("Notes"),
    };
}
