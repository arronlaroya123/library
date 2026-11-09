
using library.Models;
using Npgsql;
using System.Data.Common;

namespace library.Data;

public class MemberRepository
{
    private readonly NpgsqlDataSource _db;

    public MemberRepository(NpgsqlDataSource db)
    {
        _db = db;
    }

    private static Member ReadMember(NpgsqlDataReader reader)
    {
        return new Member
        {
            MemberId = reader.GetInt64(reader.GetOrdinal("member_id")),
            FullName = reader.GetString(reader.GetOrdinal("full_name")),
            Email = reader.IsDBNull(reader.GetOrdinal("email"))
                ? null
                : reader.GetString(reader.GetOrdinal("email")),
            MemberType = reader.GetString(reader.GetOrdinal("member_type"))
        };
    }

    // READ: every member, with how many books each has out now.
    public async Task<List<Member>> GetAllAsync()
    {
        // 1. The SQL: every member, joined to their open loans, which are counted.
        const string sql =
            "SELECT m.member_id, m.full_name, m.email, m.member_type, COUNT(l.loan_id) " +
            "FROM lending.member m " +
            "LEFT JOIN lending.loan l ON l.member_id = m.member_id AND l.returned_on IS NULL " +
            "GROUP BY m.member_id " +
            "ORDER BY m.full_name;";

        // 2. An empty list, to fill.
        var members = new List<Member>();

        // 3. A command that will run the SQL.
        await using var command = _db.CreateCommand(sql);

        // 4. Run it, and get a reader for the rows.
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) // 5. there is another row to read
        {
            // 6. Copy the row into a Member.
            var member = ReadMember(reader);

            // 7. Add the count from the fifth column.
            member.OpenLoans = reader.GetInt64(4);

            // 8. Add the member to the list.
            members.Add(member);
        }

        // 9. Give the list back.
        return members;
    }

    public async Task AddAsync(Member member)
    {
        const string sql = """
            INSERT INTO lending.member (full_name, email, member_type)
            VALUES (@full_name, @email, @member_type);
            """;

        await using var command = _db.CreateCommand(sql);

        command.Parameters.AddWithValue("full_name", member.FullName);
        command.Parameters.AddWithValue(
            "email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("member_type", member.MemberType);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Member?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT member_id, full_name, email, member_type
            FROM lending.member
            WHERE member_id = @id;
            """;

        await using var command = _db.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadMember(reader);
        }

        return null;
    }

    public async Task UpdateAsync(Member member)
    {
        const string sql = """
            UPDATE lending.member
            SET full_name = @full_name,
                email = @email,
                member_type = @member_type
            WHERE member_id = @id;
            """;

        await using var command = _db.CreateCommand(sql);

        command.Parameters.AddWithValue("full_name", member.FullName);
        command.Parameters.AddWithValue(
            "email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("member_type", member.MemberType);
        command.Parameters.AddWithValue("id", member.MemberId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        const string sql = """
            DELETE FROM lending.member
            WHERE member_id = @id;
            """;

        await using var command = _db.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
