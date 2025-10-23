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
        public DbSet<MedicalCenterRegistration.Models.Doctor> Doctor { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.Patient> Patient { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.Visit> Visit { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.VisitSchedule> VisitSchedule { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.Specialization> Specialization { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.DoctorSpecialization> DoctorSpecialization { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.Image> Image { get; set; } = default!;
        public DbSet<MedicalCenterRegistration.Models.VisitSummary> VisitSummary { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Visit>()
                .HasOne(v => v.Patient)
                .WithMany()
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

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
        }


    }
}
