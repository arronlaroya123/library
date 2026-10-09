using library.Models;
using Npgsql;

namespace library.Repositories;

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

    public async Task<List<Member>> GetAllAsync()
    {
        const string sql = """
            SELECT member_id, full_name, email, member_type
            FROM lending.member
            ORDER BY full_name;
            """;

        var members = new List<Member>();

        await using var command = _db.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            members.Add(ReadMember(reader));
        }

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
}