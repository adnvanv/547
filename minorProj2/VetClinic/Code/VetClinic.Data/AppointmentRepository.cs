using System.Data;
using Microsoft.Data.SqlClient;
using static VetClinic.Data.SqlDatabase;

namespace VetClinic.Data;

public sealed class AppointmentRepository(SqlDatabase db)
{
    private const string SelectSql = """
        SELECT a.AppointmentId, a.PetId, a.VeterinarianId, a.ScheduledAt, a.Reason, a.Status, a.Cost,
               p.Name AS PetName,
               CONCAT(o.FirstName, N' ', o.LastName) AS OwnerName,
               CONCAT(N'Dr. ', v.FirstName, N' ', v.LastName) AS VeterinarianName
        FROM dbo.Appointments AS a
        JOIN dbo.Pets AS p ON p.PetId = a.PetId
        JOIN dbo.Owners AS o ON o.OwnerId = p.OwnerId
        JOIN dbo.Veterinarians AS v ON v.VeterinarianId = a.VeterinarianId
        """;

    public List<Appointment> GetAll() => db.Query(SelectSql + " ORDER BY a.ScheduledAt DESC;", Map);

    public Appointment? GetById(int appointmentId) =>
        db.Query(SelectSql + " WHERE a.AppointmentId = @AppointmentId;", Map,
            Param("@AppointmentId", SqlDbType.Int, appointmentId)).FirstOrDefault();

    public int Insert(Appointment appointment)
    {
        const string sql = """
            INSERT INTO dbo.Appointments (PetId, VeterinarianId, ScheduledAt, Reason, Status, Cost)
            OUTPUT INSERTED.AppointmentId
            VALUES (@PetId, @VeterinarianId, @ScheduledAt, @Reason, @Status, @Cost);
            """;
        appointment.AppointmentId = db.Scalar<int>(sql, Parameters(appointment));
        return appointment.AppointmentId;
    }

    public bool Update(Appointment appointment)
    {
        const string sql = """
            UPDATE dbo.Appointments
            SET PetId = @PetId, VeterinarianId = @VeterinarianId, ScheduledAt = @ScheduledAt,
                Reason = @Reason, Status = @Status, Cost = @Cost
            WHERE AppointmentId = @AppointmentId;
            """;
        return db.Execute(sql,
            [.. Parameters(appointment), Param("@AppointmentId", SqlDbType.Int, appointment.AppointmentId)]) == 1;
    }

    public bool Delete(int appointmentId) =>
        db.Execute("DELETE FROM dbo.Appointments WHERE AppointmentId = @AppointmentId;",
            Param("@AppointmentId", SqlDbType.Int, appointmentId)) == 1;

    private static SqlParameter[] Parameters(Appointment a) =>
    [
        Param("@PetId", SqlDbType.Int, a.PetId),
        Param("@VeterinarianId", SqlDbType.Int, a.VeterinarianId),
        Param("@ScheduledAt", SqlDbType.DateTime2, a.ScheduledAt),
        Text("@Reason", a.Reason, 200),
        Text("@Status", a.Status, 20),
        new SqlParameter("@Cost", SqlDbType.Decimal)
            { Precision = 8, Scale = 2, Value = (object?)a.Cost ?? DBNull.Value },
    ];

    private static Appointment Map(SqlDataReader r) => new()
    {
        AppointmentId = r.GetInt32(r.GetOrdinal("AppointmentId")),
        PetId = r.GetInt32(r.GetOrdinal("PetId")),
        VeterinarianId = r.GetInt32(r.GetOrdinal("VeterinarianId")),
        ScheduledAt = r.GetDateTime(r.GetOrdinal("ScheduledAt")),
        Reason = r.GetString(r.GetOrdinal("Reason")),
        Status = r.GetString(r.GetOrdinal("Status")),
        Cost = NullableDecimal(r, "Cost"),
        PetName = r.GetString(r.GetOrdinal("PetName")),
        OwnerName = r.GetString(r.GetOrdinal("OwnerName")),
        VeterinarianName = r.GetString(r.GetOrdinal("VeterinarianName")),
    };
}
