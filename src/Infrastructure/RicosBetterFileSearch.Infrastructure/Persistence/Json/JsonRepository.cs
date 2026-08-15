using System.Text.Json;
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Infrastructure.Persistence.Json;

public class JsonRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;
    private List<T> _items;
    private readonly object _lock = new();

    public JsonRepository(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, $"{typeof(T).Name}.json");
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        _items = LoadFromFile();
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(item);
        }
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<T>>(_items.ToList());
        }
    }

    public Task AddAsync(T entity)
    {
        lock (_lock)
        {
            _items.Add(entity);
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        lock (_lock)
        {
            _items.AddRange(entities);
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                _items[index] = entity;
                SaveToFile();
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            _items.RemoveAll(x => x.Id == id);
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteWhereAsync(Func<T, bool> predicate)
    {
        lock (_lock)
        {
            _items.RemoveAll(x => predicate(x));
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    private List<T> LoadFromFile()
    {
        if (!File.Exists(_filePath))
            return new List<T>();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    private void SaveToFile()
    {
        var json = JsonSerializer.Serialize(_items, _options);
        File.WriteAllText(_filePath, json);
    }
}
