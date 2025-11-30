using Microsoft.EntityFrameworkCore;
using SmartLanche.Models;

namespace SmartLanche.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        //public DbSet<Client> Clients => Set<Client>();
        //public DbSet<Order> Orders => Set<Order>();
        //public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        //public DbSet<StockItem> StockItems => Set<StockItem>();

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<Order>().Property(order => order.Status).HasConversion<string>();
        //}

    }
}
