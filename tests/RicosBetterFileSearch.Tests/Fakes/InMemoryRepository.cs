using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Tests.Fakes;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _items = new();

    public Task<T?> GetByIdAsync(Guid id) =>
        Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

    public Task<IEnumerable<T>> GetAllAsync() =>
        Task.FromResult<IEnumerable<T>>(_items.ToList());

    public Task AddAsync(T entity)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        _items.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        var index = _items.FindIndex(x => x.Id == entity.Id);
        if (index >= 0) _items[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task DeleteWhereAsync(Func<T, bool> predicate)
    {
        _items.RemoveAll(x => predicate(x));
        return Task.CompletedTask;
    }
}
