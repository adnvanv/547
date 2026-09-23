using System.Data;
using Microsoft.Data.SqlClient;
using static VetClinic.Data.SqlDatabase;

namespace VetClinic.Data;

/// <summary>
/// Veterinarian staff records. The rest of the app only reads these;
/// inserts, updates and deletes are exposed on the Administration page.
/// </summary>
public sealed class VeterinarianRepository(SqlDatabase db)
{
    private const string SelectSql = """
        SELECT v.VeterinarianId, v.FirstName, v.LastName, v.Specialty, v.IsActive,
               (SELECT COUNT(*) FROM dbo.Appointments AS a WHERE a.VeterinarianId = v.VeterinarianId) AS AppointmentCount
        FROM dbo.Veterinarians AS v
        """;

    public List<Veterinarian> GetAll() => db.Query(SelectSql + " ORDER BY v.LastName, v.FirstName;", Map);

    public Veterinarian? GetById(int veterinarianId) =>
        db.Query(SelectSql + " WHERE v.VeterinarianId = @VeterinarianId;", Map,
            Param("@VeterinarianId", SqlDbType.Int, veterinarianId)).FirstOrDefault();

    public int Insert(Veterinarian vet)
    {
        const string sql = """
            INSERT INTO dbo.Veterinarians (FirstName, LastName, Specialty, IsActive)
            OUTPUT INSERTED.VeterinarianId
            VALUES (@FirstName, @LastName, @Specialty, @IsActive);
            """;
        vet.VeterinarianId = db.Scalar<int>(sql, Parameters(vet));
        return vet.VeterinarianId;
    }

    public bool Update(Veterinarian vet)
    {
        const string sql = """
            UPDATE dbo.Veterinarians
            SET FirstName = @FirstName, LastName = @LastName, Specialty = @Specialty, IsActive = @IsActive
            WHERE VeterinarianId = @VeterinarianId;
            """;
        return db.Execute(sql,
            [.. Parameters(vet), Param("@VeterinarianId", SqlDbType.Int, vet.VeterinarianId)]) == 1;
    }

    /// <summary>Fails with a <see cref="DataLayerException"/> if the vet still has appointments.</summary>
    public bool Delete(int veterinarianId) =>
        db.Execute("DELETE FROM dbo.Veterinarians WHERE VeterinarianId = @VeterinarianId;",
            Param("@VeterinarianId", SqlDbType.Int, veterinarianId)) == 1;

    private static SqlParameter[] Parameters(Veterinarian vet) =>
    [
        Text("@FirstName", vet.FirstName, 50),
        Text("@LastName", vet.LastName, 50),
        Text("@Specialty", vet.Specialty, 100),
        Param("@IsActive", SqlDbType.Bit, vet.IsActive),
    ];

    private static Veterinarian Map(SqlDataReader r) => new()
    {
        VeterinarianId = r.GetInt32(r.GetOrdinal("VeterinarianId")),
        FirstName = r.GetString(r.GetOrdinal("FirstName")),
        LastName = r.GetString(r.GetOrdinal("LastName")),
        Specialty = NullableString(r, "Specialty"),
        IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
        AppointmentCount = r.GetInt32(r.GetOrdinal("AppointmentCount")),
    };
}
