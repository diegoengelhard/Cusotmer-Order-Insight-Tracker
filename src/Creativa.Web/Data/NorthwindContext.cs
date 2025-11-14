using Microsoft.EntityFrameworkCore;
using Creativa.Web.Models;

namespace Creativa.Web.Data
{
    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions<NorthwindContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.CustomerID);
                entity.Property(e => e.CustomerID).HasMaxLength(5).IsRequired();
                entity.Property(e => e.CompanyName).HasMaxLength(40).IsRequired();
                entity.Property(e => e.ContactName).HasMaxLength(30).IsRequired(false);
                entity.Property(e => e.Phone).HasMaxLength(24).IsRequired(false);
                entity.Property(e => e.Fax).HasMaxLength(24).IsRequired(false);
                entity.Property(e => e.Country).HasMaxLength(15).IsRequired(false);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.OrderID);
                entity.Property(e => e.CustomerID).HasMaxLength(5).IsRequired(false);
                entity.Property(e => e.OrderDate).IsRequired(false);
                entity.Property(e => e.ShippedDate).IsRequired(false);
            });
        }
    }
}
