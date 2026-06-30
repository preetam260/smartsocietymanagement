namespace SmartSociety.Repository.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T item);
    Task DeleteAsync(T item);
    Task UpdateAsync(T item);
    Task<bool> ExistsAsync(Guid id);
}