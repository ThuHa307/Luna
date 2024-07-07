using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Luna.Models;
using Microsoft.Data.SqlClient;
namespace Luna.Data
{
    public partial class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public virtual IEnumerable<SendEmail> GetBills(int orderId)
        {
            return Database.SqlQueryRaw<SendEmail>("BillCustomer @OrderId", new SqlParameter("@OrderId", orderId)).ToList();
        }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
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
        public virtual DbSet<ChatMessages> ChatMessages { get; set; }

        public virtual DbSet<WishList> WishLists { get; set; }
        //  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //=> optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Wallet)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);
            });
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D8E7F7C3F2");

                entity.ToTable("Customer");

                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Cccd)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CCCD");
                entity.Property(e => e.CusName).HasMaxLength(50);
                entity.Property(e => e.Genre).HasMaxLength(3);
                entity.HasOne(d => d.RoomOrder).WithMany(p => p.Customers)
                    .HasForeignKey(d => new { d.OrderId, d.RoomId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Customer__68D28DBC");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.OrderId });

                entity.ToTable("Feedback");

                entity.Property(e => e.Message).HasMaxLength(500);

                // Thêm thuộc tính "Show" kiểu boolean
                entity.Property(e => e.Show).HasColumnName("Show").HasColumnType("bit");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            modelBuilder.Entity<HotelOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId).HasName("PK__HotelOrd__C3905BCFD6FCDAE8");

                entity.ToTable("HotelOrder");

                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.OrderStatus)
                    .HasMaxLength(6)
                    .IsUnicode(false);
                entity.HasOne(d => d.User).WithMany(p => p.HotelOrders)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HotelOrder__Id__19AACF41");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => new { e.TypeId, e.OrderId }).HasName("PK__OrderDet__BD56060911624E76");

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
                entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42FCF7035BB19");

                entity.ToTable("Promotion");

                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(false)
                    .HasColumnName("isActive");
                entity.Property(e => e.Title).HasMaxLength(50);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId).HasName("PK__Room__32863939FE41443F");

                entity.ToTable("Room");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(false)
                    .HasColumnName("isActive");
                entity.Property(e => e.RoomStatus)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.HasOne(d => d.Type).WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Room__TypeId__15DA3E5D");
            });

            modelBuilder.Entity<RoomImage>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__RoomImag__3214EC2741B227EB");

                entity.ToTable("RoomImage");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Link).HasMaxLength(1000);

                entity.HasOne(d => d.Type).WithMany(p => p.RoomImages)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__RoomImage__TypeI__1209AD79");
            });

            modelBuilder.Entity<RoomOrder>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.RoomId }).HasName("pk_rO");

                entity.ToTable("RoomOrder");

                entity.Property(e => e.CheckIn).HasColumnName("checkIn");
                entity.Property(e => e.CheckOut).HasColumnName("checkOut");
                entity.Property(e => e.ConfirmCheckIn).HasColumnName("ConfirmCheckIn");
                entity.Property(e => e.ConfirmCheckOut).HasColumnName("ConfirmCheckOut");

                entity.HasOne(d => d.Order).WithMany(p => p.RoomOrders)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RoomOrder__Order__2057CCD0");

                entity.HasOne(d => d.Room).WithMany(p => p.RoomOrders)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RoomOrder__RoomI__214BF109");
            });

            modelBuilder.Entity<RoomPromotion>(entity =>
            {
                entity.HasKey(e => new { e.TypeId, e.PromotionId }).HasName("PK__RoomProm__B44341498017BB2A");

                entity.ToTable("RoomPromotion");

                entity.HasOne(d => d.Promotion).WithMany(p => p.RoomPromotions)
                    .HasForeignKey(d => d.PromotionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RoomPromo__Promo__0E391C95");

                entity.HasOne(d => d.Type).WithMany(p => p.RoomPromotions)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RoomPromo__TypeI__0F2D40CE");
            });

            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.TypeId).HasName("PK__RoomType__516F03B53F159747");

                entity.ToTable("RoomType");

                entity.Property(e => e.Description).HasMaxLength(100);
                entity.Property(e => e.TypeName).HasMaxLength(30);
                entity.Property(e => e.TypePrice).HasColumnType("decimal(10, 0)");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.ServiceId).HasName("PK__Service__C51BB00AE21E6BFE");

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
                entity.HasKey(e => e.UseServiceId).HasName("PK__UseServi__AB2F49784AA81E1C");

                entity.ToTable("UseService");

                entity.Property(e => e.DateUseService).HasColumnType("datetime");
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.HasOne(d => d.Service).WithMany(p => p.UseServices)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UseServic__Servi__2AD55B43");

                entity.HasOne(d => d.RoomOrder).WithMany(p => p.UseServices)
                    .HasForeignKey(d => new { d.OrderId, d.RoomId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UseService__2BC97F7C");
                entity.HasOne(d => d.User).WithMany(p => p.UseServices)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UseService__Id__29E1370A");

            });
            modelBuilder.Entity<ChatMessages>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC0778271F6D");

                entity.Property(e => e.ReceiverId).HasMaxLength(450);
                entity.Property(e => e.SenderId).HasMaxLength(450);
                entity.Property(e => e.Timestamp).HasColumnType("datetime");
                entity.Property(e => e.IsSeen)
                    .HasDefaultValue(false)
                    .HasColumnName("isSeen");
                entity.HasOne(d => d.Sender).WithMany(p => p.SentMessages)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChatMessa__Sende__41B8C09B");
                entity.HasOne(d => d.Receiver).WithMany(p => p.ReceivedMessages)
                    .HasForeignKey(d => d.ReceiverId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChatMessa__Recei__42ACE4D4");
            });
            modelBuilder.Entity<WishList>(entity =>
            {
                entity.HasKey(e => new { e.TypeId, e.UserId }).HasName("PK__WishList__80178F71997B6B52");
                entity.HasOne(d => d.User).WithMany(p => p.WishLists)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__WishList__UserId__4830B400");
                entity.HasOne(d => d.RoomType).WithMany(p => p.WishLists)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__WishList__TypeId__473C8FC7");
                entity.ToTable("WishList");
            });
            //OnModelCreatingPartial(modelBuilder);
        }
        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

