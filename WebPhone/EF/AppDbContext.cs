using Microsoft.EntityFrameworkCore;

namespace WebPhone.EF
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillInfo> BillInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasDefaultValueSql("NEWID()");
                entity.Property(u => u.UserName).HasMaxLength(200);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).HasMaxLength(15);
                entity.Property(u => u.Address).HasMaxLength(500);
                entity.Property(u => u.PasswordHash).HasMaxLength(200);
                entity.Property(u => u.CreateAt).HasDefaultValueSql("(sysdatetime())");
                entity.HasIndex(u => u.UserName);
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
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.Property(b => b.Id).HasDefaultValueSql("NEWID()");
                entity.Property(b => b.CustomerName).HasMaxLength(200);
                entity.Property(b => b.EmploymentName).HasMaxLength(200);
                entity.Property(b => b.EmploymentName).HasMaxLength(200);
                entity.Property(b => b.CreateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(b => b.Customer)
                    .WithMany(u => u.CustomerBills)
                    .HasForeignKey(b => b.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Employment)
                    .WithMany(u => u.EmploymentBills)
                    .HasForeignKey(b => b.EmploymentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BillInfo>(entity =>
            {
                entity.Property(bi => bi.Id).HasDefaultValueSql("NEWID()");
                entity.Property(bi => bi.ProductName).HasMaxLength(500);
                entity.Property(bi => bi.CreateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(bi => bi.Bill)
                    .WithMany(b => b.BillInfos)
                    .HasForeignKey(bi => bi.BillId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bi => bi.Product)
                    .WithMany(p => p.BillInfos)
                    .HasForeignKey(bi => bi.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        //public override int SaveChanges()
        //{
        //    return base.SaveChanges();
        //}

        //public override Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
        //{
        //    return base.SaveChangesAsync(cancellationToken);
        //}

        //private void UpdateTimestamps()
        //{
        //    var entries = ChangeTracker
        //        .Entries()
        //        .Where(e => e.Entity is User && ())
        //}
    }
}
