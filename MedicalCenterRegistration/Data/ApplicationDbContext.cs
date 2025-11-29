// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctor { get; set; } = default!;
        public DbSet<Patient> Patient { get; set; } = default!;
        public DbSet<Visit> Visit { get; set; } = default!;
        public DbSet<VisitSchedule> VisitSchedule { get; set; } = default!;
        public DbSet<Specialization> Specialization { get; set; } = default!;
        public DbSet<DoctorSpecialization> DoctorSpecialization { get; set; } = default!;
        public DbSet<PublicImage> PublicImage { get; set; } = default!;
        public DbSet<VisitSummary> VisitSummary { get; set; } = default!;
        public DbSet<DoctorRating> DoctorRating { get; set; } = default!;
        public DbSet<UserFile> UserFile { get; set; } = default!;
        public DbSet<UserFileOwner> UserFileOwner { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Visit>()
                .HasOne(v => v.Patient)
                .WithMany()
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Visit>()
                .HasOne(v => v.Doctor)
                .WithMany()
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Visit>()
                .HasOne(v => v.VisitSchedule)
                .WithMany(vs => vs.Visits)
                .HasForeignKey(v => v.VisitScheduleId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<VisitSummary>()
                .HasOne(vs => vs.Visit)
                .WithOne(v => v.VisitSummary)
                .HasForeignKey<VisitSummary>(vs => vs.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFileOwner>()
                .HasKey(ufo => ufo.Id);

            modelBuilder.Entity<UserFileOwner>()
                .HasOne(ufo => ufo.File)
                .WithMany(f => f.Owners)
                .HasForeignKey(ufo => ufo.FileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFileOwner>()
                .HasOne(ufo => ufo.User)
                .WithMany()
                .HasForeignKey(ufo => ufo.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorRating>()
                .HasOne(dr => dr.Patient)
                .WithMany()
                .HasForeignKey(dr => dr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorRating>()
                .HasOne(dr => dr.Doctor)
                .WithMany(d => d.Ratings)
                .HasForeignKey(dr => dr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

        }


        public override int SaveChanges()
        {
            ConvertDatesToUtc();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDatesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDatesToUtc()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime) &&
                        property.CurrentValue != null)
                    {
                        var date = (DateTime)property.CurrentValue;

                        if (date.Kind == DateTimeKind.Local)
                        {
                            property.CurrentValue = date.ToUniversalTime();
                        }
                        else if (date.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }
    }
}
