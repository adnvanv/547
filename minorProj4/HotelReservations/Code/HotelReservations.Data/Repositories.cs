using Microsoft.Data.SqlClient;
using static HotelReservations.Data.SqlDatabase;

namespace HotelReservations.Data;

public enum GuestSearchField { Any, FirstName, LastName, Email, City }

public enum MatchMode { Contains, StartsWith, EndsWith }

public class GuestRepository(SqlDatabase db)
{
    private const string SelectGuests = """
        SELECT g.GuestId, g.FirstName, g.LastName, g.Email, g.Phone, g.City, g.Country, g.CreatedOn,
               (SELECT COUNT(*) FROM dbo.Reservations r WHERE r.GuestId = g.GuestId) AS ReservationCount
        FROM dbo.Guests g
        """;

    private static readonly Dictionary<GuestSearchField, string> Columns = new()
    {
        [GuestSearchField.FirstName] = "g.FirstName",
        [GuestSearchField.LastName] = "g.LastName",
        [GuestSearchField.Email] = "g.Email",
        [GuestSearchField.City] = "g.City",
    };

    /// <summary>
    /// Searches guests by ID (when the text is a number and "Any" is selected) or by first name,
    /// last name, email or city with a parameterized SQL LIKE (contains / starts with / ends with).
    /// </summary>
    public Task<List<Guest>> SearchAsync(string? text, GuestSearchField field = GuestSearchField.Any,
                                         MatchMode mode = MatchMode.Contains)
    {
        text = text?.Trim();
        if (string.IsNullOrEmpty(text))
            return db.QueryAsync(SelectGuests + " ORDER BY g.LastName, g.FirstName", Map);

        if (field == GuestSearchField.Any && int.TryParse(text, out var id))
            return db.QueryAsync(SelectGuests + " WHERE g.GuestId = @Id", Map, P("@Id", id));

        // Only column names from the fixed dictionary above are put into the SQL text;
        // the user's text always goes in the @Pattern parameter.
        var columns = field == GuestSearchField.Any ? Columns.Values.ToList() : [Columns[field]];
        var where = string.Join(" OR ", columns.Select(c => $"{c} LIKE @Pattern ESCAPE '\\'"));
        return db.QueryAsync($"{SelectGuests} WHERE {where} ORDER BY g.LastName, g.FirstName", Map,
                             P("@Pattern", LikePattern(text, mode), 110));
    }

    /// <summary>Escapes LIKE wildcards in the user's text, then adds % for the chosen match mode.</summary>
    public static string LikePattern(string text, MatchMode mode)
    {
        var escaped = text.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[");
        return mode switch
        {
            MatchMode.StartsWith => escaped + "%",
            MatchMode.EndsWith => "%" + escaped,
            _ => "%" + escaped + "%",
        };
    }

    public Task<List<Guest>> GetAllAsync() => SearchAsync(null);

    public Task<Guest?> GetAsync(int id) =>
        db.QuerySingleAsync(SelectGuests + " WHERE g.GuestId = @Id", Map, P("@Id", id));

    public Task<int> AddAsync(Guest g) => db.InsertAsync("""
        INSERT INTO dbo.Guests (FirstName, LastName, Email, Phone, City, Country, CreatedOn)
        OUTPUT INSERTED.GuestId
        VALUES (@FirstName, @LastName, @Email, @Phone, @City, @Country, @CreatedOn)
        """, "This guest could not be saved.", Parameters(g));

    public async Task UpdateAsync(Guest g) => EnsureOneRow(await db.ExecuteAsync("""
        UPDATE dbo.Guests
        SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone,
            City = @City, Country = @Country, CreatedOn = @CreatedOn
        WHERE GuestId = @GuestId
        """, "This guest could not be saved.", [.. Parameters(g), P("@GuestId", g.GuestId)]));

    public async Task DeleteAsync(int id) => EnsureOneRow(await db.ExecuteAsync(
        "DELETE FROM dbo.Guests WHERE GuestId = @Id",
        "This guest still has reservations. Delete or reassign those reservations first.", P("@Id", id)));

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
        ReservationCount = r.Int("ReservationCount"),
    };
}

public class RoomRepository(SqlDatabase db)
{
    private const string SelectRooms = """
        SELECT rm.RoomId, rm.RoomNumber, rm.Floor, rm.RoomTypeId, rm.IsSmoking, rm.IsOutOfService, rm.Notes,
               t.Name AS RoomTypeName, t.NightlyRate, t.MaxOccupancy,
               (SELECT COUNT(*) FROM dbo.Reservations r WHERE r.RoomId = rm.RoomId) AS ReservationCount
        FROM dbo.Rooms rm
        JOIN dbo.RoomTypes t ON t.RoomTypeId = rm.RoomTypeId
        """;

    public Task<List<Room>> GetAllAsync() =>
        db.QueryAsync(SelectRooms + " ORDER BY rm.Floor, rm.RoomNumber", Map);

    public Task<Room?> GetAsync(int id) =>
        db.QuerySingleAsync(SelectRooms + " WHERE rm.RoomId = @Id", Map, P("@Id", id));

    public Task<int> AddAsync(Room room) => db.InsertAsync("""
        INSERT INTO dbo.Rooms (RoomNumber, Floor, RoomTypeId, IsSmoking, IsOutOfService, Notes)
        OUTPUT INSERTED.RoomId
        VALUES (@RoomNumber, @Floor, @RoomTypeId, @IsSmoking, @IsOutOfService, @Notes)
        """, "The selected room type does not exist, or a value is out of range.", Parameters(room));

    public async Task UpdateAsync(Room room) => EnsureOneRow(await db.ExecuteAsync("""
        UPDATE dbo.Rooms
        SET RoomNumber = @RoomNumber, Floor = @Floor, RoomTypeId = @RoomTypeId,
            IsSmoking = @IsSmoking, IsOutOfService = @IsOutOfService, Notes = @Notes
        WHERE RoomId = @RoomId
        """, "The selected room type does not exist, or a value is out of range.",
        [.. Parameters(room), P("@RoomId", room.RoomId)]));

    public async Task DeleteAsync(int id) => EnsureOneRow(await db.ExecuteAsync(
        "DELETE FROM dbo.Rooms WHERE RoomId = @Id",
        "This room has reservations. Delete those reservations first, or mark the room out of service.",
        P("@Id", id)));

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
        RoomTypeName = r.Str("RoomTypeName"),
        NightlyRate = r.Dec("NightlyRate"),
        MaxOccupancy = r.Int("MaxOccupancy"),
        ReservationCount = r.Int("ReservationCount"),
    };
}

public class ReservationRepository(SqlDatabase db)
{
    private const string SelectReservations = """
        SELECT r.ReservationId, r.GuestId, r.RoomId, r.CheckInDate, r.CheckOutDate, r.NumberOfGuests,
               r.Status, r.Notes, g.FirstName + ' ' + g.LastName AS GuestName,
               rm.RoomNumber, t.Name AS RoomTypeName, t.NightlyRate
        FROM dbo.Reservations r
        JOIN dbo.Guests g ON g.GuestId = r.GuestId
        JOIN dbo.Rooms rm ON rm.RoomId = r.RoomId
        JOIN dbo.RoomTypes t ON t.RoomTypeId = rm.RoomTypeId
        """;

    private const string InvalidReference = "The selected guest or room does not exist, or a value is out of range.";

    public Task<List<Reservation>> GetAllAsync(bool includeCancelled = true) => db.QueryAsync(
        SelectReservations + " WHERE @IncludeCancelled = 1 OR r.Status <> N'Cancelled' ORDER BY r.CheckInDate, rm.RoomNumber",
        Map, P("@IncludeCancelled", includeCancelled));

    public Task<List<Reservation>> GetForGuestAsync(int guestId) => db.QueryAsync(
        SelectReservations + " WHERE r.GuestId = @GuestId ORDER BY r.CheckInDate", Map, P("@GuestId", guestId));

    public Task<Reservation?> GetAsync(int id) =>
        db.QuerySingleAsync(SelectReservations + " WHERE r.ReservationId = @Id", Map, P("@Id", id));

    public async Task<int> AddAsync(Reservation res)
    {
        await CheckBookingRulesAsync(res);
        return await db.InsertAsync("""
            INSERT INTO dbo.Reservations (GuestId, RoomId, CheckInDate, CheckOutDate, NumberOfGuests, Status, Notes)
            OUTPUT INSERTED.ReservationId
            VALUES (@GuestId, @RoomId, @CheckInDate, @CheckOutDate, @NumberOfGuests, @Status, @Notes)
            """, InvalidReference, Parameters(res));
    }

    public async Task UpdateAsync(Reservation res)
    {
        await CheckBookingRulesAsync(res);
        EnsureOneRow(await db.ExecuteAsync("""
            UPDATE dbo.Reservations
            SET GuestId = @GuestId, RoomId = @RoomId, CheckInDate = @CheckInDate, CheckOutDate = @CheckOutDate,
                NumberOfGuests = @NumberOfGuests, Status = @Status, Notes = @Notes
            WHERE ReservationId = @ReservationId
            """, InvalidReference, [.. Parameters(res), P("@ReservationId", res.ReservationId)]));
    }

    public async Task DeleteAsync(int id) => EnsureOneRow(await db.ExecuteAsync(
        "DELETE FROM dbo.Reservations WHERE ReservationId = @Id", "This reservation could not be deleted.", P("@Id", id)));

    /// <summary>Rejects double bookings and parties larger than the room type allows.</summary>
    private async Task CheckBookingRulesAsync(Reservation res)
    {
        if (res.Status == "Cancelled")
            return;

        var maxOccupancy = await db.ScalarAsync("""
            SELECT t.MaxOccupancy FROM dbo.Rooms rm JOIN dbo.RoomTypes t ON t.RoomTypeId = rm.RoomTypeId
            WHERE rm.RoomId = @RoomId
            """, P("@RoomId", res.RoomId));
        if (maxOccupancy is int max && res.NumberOfGuests > max)
            throw new DataLayerException($"This room sleeps at most {max} guest(s).");

        var conflict = await db.ScalarAsync("""
            SELECT TOP 1 CONCAT(N'#', ReservationId, N' (', FORMAT(CheckInDate, 'yyyy-MM-dd'), N' to ',
                                FORMAT(CheckOutDate, 'yyyy-MM-dd'), N')')
            FROM dbo.Reservations
            WHERE RoomId = @RoomId AND ReservationId <> @ReservationId AND Status <> N'Cancelled'
              AND CheckInDate < @CheckOutDate AND @CheckInDate < CheckOutDate
            """, P("@RoomId", res.RoomId), P("@ReservationId", res.ReservationId),
            P("@CheckInDate", res.CheckInDate), P("@CheckOutDate", res.CheckOutDate));
        if (conflict is string other)
            throw new DataLayerException($"That room is already booked for those dates by reservation {other}.");
    }

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
        GuestName = r.Str("GuestName"),
        RoomNumber = r.Str("RoomNumber"),
        RoomTypeName = r.Str("RoomTypeName"),
        NightlyRate = r.Dec("NightlyRate"),
    };
}

/// <summary>Room types. Only the Administration controller calls the write methods.</summary>
public class RoomTypeRepository(SqlDatabase db)
{
    private const string SelectRoomTypes = """
        SELECT t.RoomTypeId, t.Name, t.NightlyRate, t.MaxOccupancy, t.Description, t.IsActive,
               (SELECT COUNT(*) FROM dbo.Rooms rm WHERE rm.RoomTypeId = t.RoomTypeId) AS RoomCount
        FROM dbo.RoomTypes t
        """;

    public Task<List<RoomType>> GetAllAsync(bool activeOnly = false) => db.QueryAsync(
        SelectRoomTypes + " WHERE @ActiveOnly = 0 OR t.IsActive = 1 ORDER BY t.NightlyRate, t.Name",
        Map, P("@ActiveOnly", activeOnly));

    public Task<RoomType?> GetAsync(int id) =>
        db.QuerySingleAsync(SelectRoomTypes + " WHERE t.RoomTypeId = @Id", Map, P("@Id", id));

    public Task<int> AddAsync(RoomType t) => db.InsertAsync("""
        INSERT INTO dbo.RoomTypes (Name, NightlyRate, MaxOccupancy, Description, IsActive)
        OUTPUT INSERTED.RoomTypeId
        VALUES (@Name, @NightlyRate, @MaxOccupancy, @Description, @IsActive)
        """, "This room type could not be saved.", Parameters(t));

    public async Task UpdateAsync(RoomType t) => EnsureOneRow(await db.ExecuteAsync("""
        UPDATE dbo.RoomTypes
        SET Name = @Name, NightlyRate = @NightlyRate, MaxOccupancy = @MaxOccupancy,
            Description = @Description, IsActive = @IsActive
        WHERE RoomTypeId = @RoomTypeId
        """, "This room type could not be saved.", [.. Parameters(t), P("@RoomTypeId", t.RoomTypeId)]));

    public async Task DeleteAsync(int id) => EnsureOneRow(await db.ExecuteAsync(
        "DELETE FROM dbo.RoomTypes WHERE RoomTypeId = @Id",
        "Rooms still use this room type. Change those rooms first, or mark this type inactive instead.",
        P("@Id", id)));

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
        RoomCount = r.Int("RoomCount"),
    };
}
