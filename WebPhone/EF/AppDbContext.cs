﻿using Microsoft.EntityFrameworkCore;

namespace WebPhone.EF
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<CategoryProduct> CategoryProducts { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<BillInfo> BillInfos { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<LogHistory> LogHistories { get; set; }
        public virtual DbSet<PaymentLog> PaymentLogs { get; set; }

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
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(b => b.Employment)
                    .WithMany(u => u.EmploymentBills)
                    .HasForeignKey(b => b.EmploymentId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
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

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Id).HasDefaultValueSql("NEWID()");
                entity.Property(r => r.RoleName).HasMaxLength(200);
                entity.Property(r => r.CreateAt).HasDefaultValueSql("(sysdatetime())");
                entity.HasIndex(r => r.RoleName).IsUnique();
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId })
                        .HasName("PK_UserRole_UserId_RoleId");

                entity.HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId)
                        .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.User)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.Property(w => w.Id).HasDefaultValueSql("NEWID()");
                entity.Property(w => w.WarehouseName).HasMaxLength(200);
                entity.Property(w => w.Address).HasMaxLength(200);
                entity.Property(w => w.CreateAt).HasDefaultValueSql("(sysdatetime())");
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property(i => i.Id).HasDefaultValueSql("NEWID()");
                entity.Property(i => i.CreateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(i => i.Product)
                    .WithMany(p => p.Inventories)
                    .HasForeignKey(i => i.ProductId);

                entity.HasOne(i => i.Warehouse)
                    .WithMany(p => p.Inventories)
                    .HasForeignKey(i => i.WarehouseId);
            });

            modelBuilder.Entity<LogHistory>(entity =>
            {
                entity.Property(lh => lh.Id).HasDefaultValueSql("NEWID()");
                entity.Property(lh => lh.EntityName).HasMaxLength(200);
                entity.Property(lh => lh.UpdateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(lh => lh.Employment)
                    .WithMany(u => u.LogHistories)
                    .HasForeignKey(lh => lh.EmploymentId);
            });

            modelBuilder.Entity<PaymentLog>(entity =>
            {
                entity.Property(p => p.Id).HasDefaultValueSql("NEWID()");
                entity.Property(p => p.CreateAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(p => p.Customer)
                    .WithMany(c => c.PaymentLogs)
                    .HasForeignKey(p => p.CustomerId);

                entity.HasOne(p => p.Bill)
                    .WithMany(c => c.PaymentLogs)
                    .HasForeignKey(p => p.BillId);
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
