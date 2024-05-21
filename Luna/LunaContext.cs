using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Luna;

public partial class LunaContext : DbContext
{
    public LunaContext()
    {
    }

    public LunaContext(DbContextOptions<LunaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<HotelOrder> HotelOrders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomImage> RoomImages { get; set; }

    public virtual DbSet<RoomOrder> RoomOrders { get; set; }

    public virtual DbSet<RoomPromotion> RoomPromotions { get; set; }

    public virtual DbSet<RoomType> RoomTypes { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<UseService> UseServices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Discriminator)
                .HasMaxLength(21)
                .HasDefaultValue("");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.Wallet)
                .HasDefaultValue(0.0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D887208891");

            entity.ToTable("Customer");

            entity.Property(e => e.CustomerId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.CusName).HasMaxLength(50);

            entity.HasOne(d => d.RoomOrder).WithMany(p => p.Customers)
                .HasForeignKey(d => new { d.OrderId, d.RoomId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Customer__6FB49575");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.OrderId }).HasName("PK__Feedback__DE2DE9BBCDF01B05");

            entity.ToTable("Feedback");

            entity.Property(e => e.Message).HasMaxLength(500);

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Id__7A3223E8");

            entity.HasOne(d => d.Order).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__OrderI__7B264821");
        });

        modelBuilder.Entity<HotelOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__HotelOrd__C3905BCF6D3C364C");

            entity.ToTable("HotelOrder");

            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(6)
                .IsUnicode(false);

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.HotelOrders)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HotelOrder__Id__65370702");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.TypeId, e.OrderId }).HasName("PK__OrderDet__BD560609328102FC");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_OD1");

            entity.HasOne(d => d.Type).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_OD");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42FCF72F5132D");

            entity.ToTable("Promotion");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("isActive");
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Room__32863939F856D8C2");

            entity.ToTable("Room");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("isActive");
            entity.Property(e => e.RoomStatus)
                .HasMaxLength(2)
                .IsUnicode(false);

            entity.HasOne(d => d.Type).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK__Room__TypeId__6166761E");
        });

        modelBuilder.Entity<RoomImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoomImag__3214EC27A92E3800");

            entity.ToTable("RoomImage");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Link).HasMaxLength(1000);

            entity.HasOne(d => d.Type).WithMany(p => p.RoomImages)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK__RoomImage__TypeI__5D95E53A");
        });

        modelBuilder.Entity<RoomOrder>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.RoomId }).HasName("pk_rO");

            entity.ToTable("RoomOrder");

            entity.Property(e => e.CheckIn).HasColumnName("checkIn");
            entity.Property(e => e.CheckOut).HasColumnName("checkOut");

            entity.HasOne(d => d.Order).WithMany(p => p.RoomOrders)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomOrder__Order__6BE40491");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomOrders)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomOrder__RoomI__6CD828CA");
        });

        modelBuilder.Entity<RoomPromotion>(entity =>
        {
            entity.HasKey(e => new { e.TypeId, e.PromotionId }).HasName("PK__RoomProm__B44341491CCA6A42");

            entity.ToTable("RoomPromotion");

            entity.HasOne(d => d.Promotion).WithMany(p => p.RoomPromotions)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomPromo__Promo__59C55456");

            entity.HasOne(d => d.Type).WithMany(p => p.RoomPromotions)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomPromo__TypeI__5AB9788F");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__RoomType__516F03B5F662C7BE");

            entity.ToTable("RoomType");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.TypeName).HasMaxLength(30);
            entity.Property(e => e.TypePrice).HasColumnType("decimal(10, 0)");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__C51BB00A470A2D6F");

            entity.ToTable("Service");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("isActive");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ServicePrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<UseService>(entity =>
        {
            entity.HasKey(e => e.UseServiceId).HasName("PK__UseServi__AB2F497861A388DC");

            entity.ToTable("UseService");

            entity.Property(e => e.DateUseService).HasColumnType("datetime");
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.UseServices)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UseService__Id__756D6ECB");

            entity.HasOne(d => d.Service).WithMany(p => p.UseServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UseServic__Servi__76619304");

            entity.HasOne(d => d.RoomOrder).WithMany(p => p.UseServices)
                .HasForeignKey(d => new { d.OrderId, d.RoomId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UseService__7755B73D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
