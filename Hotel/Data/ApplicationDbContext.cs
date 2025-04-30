using System;
using System.Collections.Generic;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<Mealorder> Mealorders { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Roomcategory> Roomcategories { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Serviceorder> Serviceorders { get; set; }

    public virtual DbSet<Spaservice> Spaservices { get; set; }

    public virtual DbSet<Spaserviceorder> Spaserviceorders { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workschedule> Workschedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=HotelDB;Username=postgres;Password=1");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("booking_pkey");

            entity.ToTable("booking");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CheckInDate).HasColumnName("check_in_date");
            entity.Property(e => e.CheckOutDate).HasColumnName("check_out_date");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");

            entity.HasOne(d => d.Guest).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.GuestId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_booking_guest");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_booking_room");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employee_pkey");

            entity.ToTable("employee");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(100)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(15)
                .HasColumnName("contact_phone");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .HasColumnName("position");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestId).HasName("guest_pkey");

            entity.ToTable("guest");

            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(100)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(15)
                .HasColumnName("contact_phone");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PassportData)
                .HasMaxLength(50)
                .HasColumnName("passport_data");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("invoice_pkey");

            entity.ToTable("invoice");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasComputedColumnSql("(((total_room_cost + total_service_cost) + total_meal_cost) + total_spa_cost)", true)
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalMealCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_meal_cost");
            entity.Property(e => e.TotalRoomCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_room_cost");
            entity.Property(e => e.TotalServiceCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_service_cost");
            entity.Property(e => e.TotalSpaCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_spa_cost");

            entity.HasOne(d => d.Booking).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_invoice_booking");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.MealId).HasName("meal_pkey");

            entity.ToTable("meal");

            entity.HasIndex(e => new { e.MealName, e.MealType }, "unique_meal_name_type").IsUnique();

            entity.Property(e => e.MealId).HasColumnName("meal_id");
            entity.Property(e => e.MealName)
                .HasMaxLength(100)
                .HasColumnName("meal_name");
            entity.Property(e => e.MealType)
                .HasMaxLength(10)
                .HasColumnName("meal_type");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
        });

        modelBuilder.Entity<Mealorder>(entity =>
        {
            entity.HasKey(e => e.MealOrderId).HasName("mealorder_pkey");

            entity.ToTable("mealorder");

            entity.HasIndex(e => new { e.BookingId, e.MealId, e.MealDate, e.MealTime }, "uniq_meal_booking").IsUnique();

            entity.Property(e => e.MealOrderId).HasColumnName("meal_order_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.MealDate).HasColumnName("meal_date");
            entity.Property(e => e.MealId).HasColumnName("meal_id");
            entity.Property(e => e.MealTime).HasColumnName("meal_time");

            entity.HasOne(d => d.Booking).WithMany(p => p.Mealorders)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_mealorder_booking");

            entity.HasOne(d => d.Meal).WithMany(p => p.Mealorders)
                .HasForeignKey(d => d.MealId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_mealorder_meal");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("review_pkey");

            entity.ToTable("review");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("review_date");
            entity.Property(e => e.ReviewText).HasColumnName("review_text");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_review_booking");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("room_pkey");

            entity.ToTable("room");

            entity.HasIndex(e => e.RoomNumber, "uniq_room_number").IsUnique();

            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(10)
                .HasColumnName("room_number");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasColumnName("status");

            entity.HasOne(d => d.Category).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_room_category");
        });

        modelBuilder.Entity<Roomcategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("roomcategory_pkey");

            entity.ToTable("roomcategory");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PricePerNight)
                .HasPrecision(10, 2)
                .HasColumnName("price_per_night");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("service_pkey");

            entity.ToTable("service");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
            entity.Property(e => e.ServiceType)
                .HasMaxLength(250)
                .HasColumnName("service_type");
        });

        modelBuilder.Entity<Serviceorder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("serviceorder_pkey");

            entity.ToTable("serviceorder");

            entity.HasIndex(e => new { e.BookingId, e.ServiceId, e.ServiceDate, e.ServiceTime }, "uniq_service_booking").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ServiceDate).HasColumnName("service_date");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ServiceTime).HasColumnName("service_time");

            entity.HasOne(d => d.Booking).WithMany(p => p.Serviceorders)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_serviceorder_booking");

            entity.HasOne(d => d.Service).WithMany(p => p.Serviceorders)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_serviceorder_service");
        });

        modelBuilder.Entity<Spaservice>(entity =>
        {
            entity.HasKey(e => e.SpaServiceId).HasName("spaservice_pkey");

            entity.ToTable("spaservice");

            entity.HasIndex(e => e.ServiceName, "unique_spa_service_name").IsUnique();

            entity.Property(e => e.SpaServiceId).HasColumnName("spa_service_id");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<Spaserviceorder>(entity =>
        {
            entity.HasKey(e => e.SpaOrderId).HasName("spaserviceorder_pkey");

            entity.ToTable("spaserviceorder");

            entity.HasIndex(e => new { e.BookingId, e.SpaServiceId, e.ServiceDate, e.ServiceTime }, "uniq_spa_service_booking").IsUnique();

            entity.Property(e => e.SpaOrderId).HasColumnName("spa_order_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ServiceDate).HasColumnName("service_date");
            entity.Property(e => e.ServiceTime).HasColumnName("service_time");
            entity.Property(e => e.SpaServiceId).HasColumnName("spa_service_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.Spaserviceorders)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_spaserviceorder_booking");

            entity.HasOne(d => d.SpaService).WithMany(p => p.Spaserviceorders)
                .HasForeignKey(d => d.SpaServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_spaserviceorder_service");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.GuestId, "users_guest_id_key").IsUnique();

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");

            entity.HasOne(d => d.Guest).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.GuestId)
                .HasConstraintName("fk_user_guest");
        });

        modelBuilder.Entity<Workschedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("workschedule_pkey");

            entity.ToTable("workschedule");

            entity.HasIndex(e => new { e.EmployeeId, e.WorkDate }, "uniq_employee_date").IsUnique();

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");

            entity.HasOne(d => d.Employee).WithMany(p => p.Workschedules)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk_employee");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
