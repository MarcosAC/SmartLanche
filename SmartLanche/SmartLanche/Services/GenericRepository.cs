using Microsoft.EntityFrameworkCore;
using SmartLanche.Data;

namespace SmartLanche.Services
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GenericRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(T entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var dbSet = context.Set<T>();
            var entity = await dbSet.FindAsync(id);

            if (entity == null) return;

            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();            
            return await context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }
    }
}
