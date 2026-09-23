using System.Data;
using Microsoft.Data.SqlClient;
using static VetClinic.Data.SqlDatabase;

namespace VetClinic.Data;

public sealed class OwnerRepository(SqlDatabase db)
{
    private const string SelectSql = """
        SELECT o.OwnerId, o.FirstName, o.LastName, o.Email, o.Phone, o.City,
               (SELECT COUNT(*) FROM dbo.Pets AS p WHERE p.OwnerId = o.OwnerId) AS PetCount
        FROM dbo.Owners AS o
        """;

    private const string OrderBy = " ORDER BY o.LastName, o.FirstName;";

    public List<Owner> GetAll() => db.Query(SelectSql + OrderBy, Map);

    public Owner? GetById(int ownerId) =>
        db.Query(SelectSql + " WHERE o.OwnerId = @OwnerId;", Map,
            Param("@OwnerId", SqlDbType.Int, ownerId)).FirstOrDefault();

    /// <summary>
    /// Searches owners with LIKE '%term%' on one text column, or on all of them.
    /// Searching by <see cref="OwnerSearchField.Id"/> requires a whole number.
    /// </summary>
    public List<Owner> Search(OwnerSearchField field, string term)
    {
        term = term.Trim();
        if (term.Length == 0)
            return GetAll();

        if (field == OwnerSearchField.Id)
        {
            return int.TryParse(term, out var id) && GetById(id) is { } owner ? [owner] : [];
        }

        var where = field switch
        {
            OwnerSearchField.FirstName => "o.FirstName LIKE @Pattern",
            OwnerSearchField.LastName => "o.LastName LIKE @Pattern",
            OwnerSearchField.Email => "o.Email LIKE @Pattern",
            OwnerSearchField.Phone => "o.Phone LIKE @Pattern",
            OwnerSearchField.City => "o.City LIKE @Pattern",
            _ => "(o.FirstName LIKE @Pattern OR o.LastName LIKE @Pattern OR o.Email LIKE @Pattern " +
                 "OR o.Phone LIKE @Pattern OR o.City LIKE @Pattern " +
                 "OR CONCAT(o.FirstName, N' ', o.LastName) LIKE @Pattern)",
        };

        return db.Query(SelectSql + " WHERE " + where + OrderBy, Map,
            Param("@Pattern", SqlDbType.NVarChar, "%" + EscapeLike(term) + "%", 110));
    }

    public int Insert(Owner owner)
    {
        const string sql = """
            INSERT INTO dbo.Owners (FirstName, LastName, Email, Phone, City)
            OUTPUT INSERTED.OwnerId
            VALUES (@FirstName, @LastName, @Email, @Phone, @City);
            """;
        owner.OwnerId = db.Scalar<int>(sql, Parameters(owner));
        return owner.OwnerId;
    }

    public bool Update(Owner owner)
    {
        const string sql = """
            UPDATE dbo.Owners
            SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone, City = @City
            WHERE OwnerId = @OwnerId;
            """;
        return db.Execute(sql, [.. Parameters(owner), Param("@OwnerId", SqlDbType.Int, owner.OwnerId)]) == 1;
    }

    /// <summary>Deletes the owner; their pets and appointments are removed by ON DELETE CASCADE.</summary>
    public bool Delete(int ownerId) =>
        db.Execute("DELETE FROM dbo.Owners WHERE OwnerId = @OwnerId;",
            Param("@OwnerId", SqlDbType.Int, ownerId)) == 1;

    private static SqlParameter[] Parameters(Owner owner) =>
    [
        Text("@FirstName", owner.FirstName, 50),
        Text("@LastName", owner.LastName, 50),
        Text("@Email", owner.Email, 100),
        Text("@Phone", owner.Phone, 25),
        Text("@City", owner.City, 50),
    ];

    /// <summary>Escapes LIKE wildcards so the user's text is matched literally.</summary>
    private static string EscapeLike(string value) =>
        value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    private static Owner Map(SqlDataReader r) => new()
    {
        OwnerId = r.GetInt32(r.GetOrdinal("OwnerId")),
        FirstName = r.GetString(r.GetOrdinal("FirstName")),
        LastName = r.GetString(r.GetOrdinal("LastName")),
        Email = NullableString(r, "Email"),
        Phone = NullableString(r, "Phone"),
        City = NullableString(r, "City"),
        PetCount = r.GetInt32(r.GetOrdinal("PetCount")),
    };
}
