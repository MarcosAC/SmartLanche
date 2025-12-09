using Microsoft.EntityFrameworkCore;
using SmartLanche.Models;

namespace SmartLanche.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Client> Clients => Set<Client>();
        //public DbSet<Order> Orders => Set<Order>();
        //public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        //public DbSet<StockItem> StockItems => Set<StockItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(product => product.Id);
                entity.Property(product => product.Name).IsRequired().HasMaxLength(100);
                entity.Property(product => product.Category).HasMaxLength(50);
                entity.Property(product => product.Price).HasColumnType("decimal(10,2)");
                entity.Property(product => product.Description).HasMaxLength(250);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(client => client.OutstandingBalance).HasColumnType("decimal(10,2)");
                entity.Property(client => client.Name).IsRequired().HasMaxLength(150);
            });                
        }
    }
}
