﻿using DAL.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<MedicalSpeciality> MedicalSpecialities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleDetail> ScheduleDetails { get; set; }

        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor();
        }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasOne(p => p.Doctor)
                .WithMany(b => b.Appointments)
                .HasForeignKey(p => p.DoctorId);

            modelBuilder.Entity<Appointment>()
                .HasOne(p => p.ApplicationUser)
                .WithMany(b => b.Appointments)
                .HasForeignKey(p => p.ApplicationUserId);

            modelBuilder.Entity<Doctor>()
                .HasOne(p => p.MedicalSpeciality)
                .WithMany(b => b.Doctors)
                .HasForeignKey(p => p.MedicalSpecialityId);

            modelBuilder.Entity<Schedule>()
                .HasOne(p => p.Doctor)
                .WithMany(b => b.Schedules)
                .HasForeignKey(p => p.DoctorId);

            modelBuilder.Entity<Schedule>()
                .HasOne(p => p.Institution)
                .WithMany(b => b.Schedules)
                .HasForeignKey(p => p.InstitutionId);

            modelBuilder.Entity<ScheduleDetail>()
               .HasOne(p => p.Schedule)
               .WithMany(b => b.ScheduleDetails)
               .HasForeignKey(p => p.ScheduleId);

            //define unique indexes
            modelBuilder.Entity<Doctor>()
                .HasIndex(u => new { u.FirstName, u.LastName })
                .IsUnique();

            modelBuilder.Entity<MedicalSpeciality>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<Schedule>()
                .HasIndex(u => new { u.InstitutionId, u.DoctorId })
                .IsUnique();

            modelBuilder.Entity<Institution>()
                .HasIndex(u => u.Address)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
