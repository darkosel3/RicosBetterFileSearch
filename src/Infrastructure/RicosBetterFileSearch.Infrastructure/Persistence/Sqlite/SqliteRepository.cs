using System.Text.Json;
using Microsoft.Data.Sqlite;
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Infrastructure.Persistence.Sqlite;

public class SqliteRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqliteRepository(string dbPath)
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        _connectionString = $"Data Source={dbPath}";
        _tableName = typeof(T).Name;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        InitializeTable();
    }

    private void InitializeTable()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
            CREATE TABLE IF NOT EXISTS {_tableName} (
                Id TEXT PRIMARY KEY,
                Data TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            )";
        cmd.ExecuteNonQuery();
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT Data FROM {_tableName} WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id.ToString());

        var json = cmd.ExecuteScalar() as string;
        if (json is null) return Task.FromResult<T?>(null);

        return Task.FromResult(JsonSerializer.Deserialize<T>(json, _jsonOptions));
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        var items = new List<T>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT Data FROM {_tableName}";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var json = reader.GetString(0);
            var item = JsonSerializer.Deserialize<T>(json, _jsonOptions);
            if (item is not null) items.Add(item);
        }

        return Task.FromResult<IEnumerable<T>>(items);
    }

    public Task AddAsync(T entity)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var json = JsonSerializer.Serialize(entity, _jsonOptions);
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"INSERT OR REPLACE INTO {_tableName} (Id, Data, CreatedAt) VALUES (@id, @data, @created)";
        cmd.Parameters.AddWithValue("@id", entity.Id.ToString());
        cmd.Parameters.AddWithValue("@data", json);
        cmd.Parameters.AddWithValue("@created", entity.CreatedAt.ToString("O"));
        cmd.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        foreach (var entity in entities)
        {
            var json = JsonSerializer.Serialize(entity, _jsonOptions);
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"INSERT OR REPLACE INTO {_tableName} (Id, Data, CreatedAt) VALUES (@id, @data, @created)";
            cmd.Parameters.AddWithValue("@id", entity.Id.ToString());
            cmd.Parameters.AddWithValue("@data", json);
            cmd.Parameters.AddWithValue("@created", entity.CreatedAt.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return AddAsync(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id.ToString());
        cmd.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task DeleteWhereAsync(Func<T, bool> predicate)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var allItems = new List<(string Id, T Item)>();
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = $"SELECT Id, Data FROM {_tableName}";

        using var reader = selectCmd.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetString(0);
            var json = reader.GetString(1);
            var item = JsonSerializer.Deserialize<T>(json, _jsonOptions);
            if (item is not null) allItems.Add((id, item));
        }
        reader.Close();

        foreach (var (id, item) in allItems.Where(x => predicate(x.Item)))
        {
            var delCmd = connection.CreateCommand();
            delCmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";
            delCmd.Parameters.AddWithValue("@id", id);
            delCmd.ExecuteNonQuery();
        }

        transaction.Commit();
        return Task.CompletedTask;
    }
}
