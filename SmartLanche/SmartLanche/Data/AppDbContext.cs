using Microsoft.EntityFrameworkCore;
using SmartLanche.Models;

namespace SmartLanche.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

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
                entity.Property(product => product.StockQuantity).HasDefaultValue(0);
                entity.Property(product => product.MinStockLevel).HasDefaultValue(5);
            });

            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.ToTable("StockMovements");
                entity.HasKey(sm => sm.Id);
                entity.Property(sm => sm.Reason).HasMaxLength(200);
                entity.Property(sm => sm.Date).IsRequired(); 
                
                entity.HasOne(sm => sm.Product)
                      .WithMany()
                      .HasForeignKey(sm => sm.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(client => client.OutstandingBalance).HasColumnType("decimal(10,2)");
                entity.Property(client => client.Name).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(order => order.TotalAmount).HasColumnType("decimal(10,2)");

                entity.HasOne(order => order.Client)
                      .WithMany()
                      .HasForeignKey(order => order.ClientId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(orderItem => orderItem.UnitPrice).HasColumnType("decimal(10,2)");

                entity.HasOne(orderItem => orderItem.Order)
                      .WithMany(order => order.OrderItems)
                      .HasForeignKey(orderItem => orderItem.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(orderItem => orderItem.Product)
                          .WithMany()
                          .HasForeignKey(orderItem => orderItem.ProductId)
                          .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
