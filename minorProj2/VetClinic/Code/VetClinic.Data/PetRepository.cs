using System.Data;
using Microsoft.Data.SqlClient;
using static VetClinic.Data.SqlDatabase;

namespace VetClinic.Data;

public sealed class PetRepository(SqlDatabase db)
{
    private const string SelectSql = """
        SELECT p.PetId, p.OwnerId, p.Name, p.Species, p.Breed, p.BirthDate, p.WeightKg,
               CONCAT(o.FirstName, N' ', o.LastName) AS OwnerName
        FROM dbo.Pets AS p
        JOIN dbo.Owners AS o ON o.OwnerId = p.OwnerId
        """;

    public List<Pet> GetAll() => db.Query(SelectSql + " ORDER BY p.Name;", Map);

    public Pet? GetById(int petId) =>
        db.Query(SelectSql + " WHERE p.PetId = @PetId;", Map,
            Param("@PetId", SqlDbType.Int, petId)).FirstOrDefault();

    public int Insert(Pet pet)
    {
        const string sql = """
            INSERT INTO dbo.Pets (OwnerId, Name, Species, Breed, BirthDate, WeightKg)
            OUTPUT INSERTED.PetId
            VALUES (@OwnerId, @Name, @Species, @Breed, @BirthDate, @WeightKg);
            """;
        pet.PetId = db.Scalar<int>(sql, Parameters(pet));
        return pet.PetId;
    }

    public bool Update(Pet pet)
    {
        const string sql = """
            UPDATE dbo.Pets
            SET OwnerId = @OwnerId, Name = @Name, Species = @Species, Breed = @Breed,
                BirthDate = @BirthDate, WeightKg = @WeightKg
            WHERE PetId = @PetId;
            """;
        return db.Execute(sql, [.. Parameters(pet), Param("@PetId", SqlDbType.Int, pet.PetId)]) == 1;
    }

    /// <summary>Deletes the pet; its appointments are removed by ON DELETE CASCADE.</summary>
    public bool Delete(int petId) =>
        db.Execute("DELETE FROM dbo.Pets WHERE PetId = @PetId;",
            Param("@PetId", SqlDbType.Int, petId)) == 1;

    private static SqlParameter[] Parameters(Pet pet) =>
    [
        Param("@OwnerId", SqlDbType.Int, pet.OwnerId),
        Text("@Name", pet.Name, 50),
        Text("@Species", pet.Species, 30),
        Text("@Breed", pet.Breed, 50),
        Param("@BirthDate", SqlDbType.Date, pet.BirthDate?.Date),
        new SqlParameter("@WeightKg", SqlDbType.Decimal)
            { Precision = 6, Scale = 2, Value = (object?)pet.WeightKg ?? DBNull.Value },
    ];

    private static Pet Map(SqlDataReader r) => new()
    {
        PetId = r.GetInt32(r.GetOrdinal("PetId")),
        OwnerId = r.GetInt32(r.GetOrdinal("OwnerId")),
        Name = r.GetString(r.GetOrdinal("Name")),
        Species = r.GetString(r.GetOrdinal("Species")),
        Breed = NullableString(r, "Breed"),
        BirthDate = NullableDate(r, "BirthDate"),
        WeightKg = NullableDecimal(r, "WeightKg"),
        OwnerName = r.GetString(r.GetOrdinal("OwnerName")),
    };
}
