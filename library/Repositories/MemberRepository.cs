using Npgsql;
using library.Models;

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
}