using Microsoft.EntityFrameworkCore;

namespace WebPhone.EF
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasDefaultValueSql("NEWID()");
                entity.Property(u => u.UserName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.PasswordHash).HasMaxLength(200);
                entity.Property(u => u.CreateAt).HasDefaultValueSql("(sysdatetime())");
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<CategoryProduct>(entity =>
            {
                entity.Property(cp => cp.Id).HasDefaultValueSql("NEWID()");
                entity.Property(cp => cp.CategoryName).HasMaxLength(100);
                entity.Property(cp => cp.CreateAt).HasDefaultValueSql("(sysdatetime())");
                entity.HasIndex(cp => cp.CategoryName).IsUnique();

                entity.HasOne(cp => cp.CateProductParent)
                    .WithMany(cpc => cpc.CateProductChildren)
                    .HasForeignKey(cp => cp.IdParent)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Id).HasDefaultValueSql("NEWID()");
                entity.Property(p => p.ProductName).HasMaxLength(500);
                entity.Property(p => p.CreateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasIndex(p => p.ProductName).IsUnique();

                entity.HasOne(p => p.CategoryProduct)
                    .WithMany(ct => ct.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
