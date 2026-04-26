using DigiClinicApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.AppDbContext
{
    public class ContextDb : DbContext
    {
        public ContextDb(DbContextOptions<ContextDb> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<DoctorService> DoctorServices { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.TelegramChatId)
                .IsUnique();

            modelBuilder.Entity<Specialization>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<DoctorProfile>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<PatientProfile>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .HasIndex(x => x.TimeSlotId)
                .IsUnique();

            modelBuilder.Entity<DoctorService>()
                .HasKey(x => new { x.DoctorProfileId, x.ServiceId });

            modelBuilder.Entity<DoctorService>()
                .HasOne(x => x.DoctorProfile)
                .WithMany(x => x.DoctorServices)
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorService>()
                .HasOne(x => x.Service)
                .WithMany(x => x.DoctorServices)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(x => x.DoctorProfile)
                .WithOne(x => x.User)
                .HasForeignKey<DoctorProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(x => x.PatientProfile)
                .WithOne(x => x.User)
                .HasForeignKey<PatientProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeSlot>()
                .HasOne(x => x.Appointment)
                .WithOne(x => x.TimeSlot)
                .HasForeignKey<Appointment>(x => x.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(x => x.PatientProfile)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.PatientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(x => x.Service)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeSlot>()
                .HasOne(x => x.DoctorProfile)
                .WithMany(x => x.TimeSlots)
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorProfile>()
                .HasOne(x => x.Specialization)
                .WithMany(x => x.DoctorProfiles)
                .HasForeignKey(x => x.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrivateMessage>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrivateMessage>()
                .HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Referral>()
            .HasOne(x => x.PatientProfile)
            .WithMany()
            .HasForeignKey(x => x.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Referral>()
                .HasOne(x => x.CreatedByDoctorProfile)
                .WithMany()
                .HasForeignKey(x => x.CreatedByDoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Referral>()
                .HasOne(x => x.SourceAppointment)
                .WithMany()
                .HasForeignKey(x => x.SourceAppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Referral>()
                .HasOne(x => x.Service)
                .WithMany()
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(x => x.Referral)
                .WithMany()
                .HasForeignKey(x => x.ReferralId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
