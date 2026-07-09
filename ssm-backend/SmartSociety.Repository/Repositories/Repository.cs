using Microsoft.EntityFrameworkCore;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Repository.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly SmartSocietyDbContext _context; // connection manager 
    protected readonly DbSet<T> _dbSet; // table manager

    public Repository(SmartSocietyDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    
        => await _dbSet.ToListAsync();
    

    public virtual async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        

    public async Task<bool> ExistsAsync(Guid id)

        => await _dbSet.AnyAsync(e => e.Id == id);

    public async Task AddAsync(T entity)

        => await _dbSet.AddAsync(entity);

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);

        return Task.CompletedTask;
    }
}