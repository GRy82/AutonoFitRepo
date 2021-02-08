using AutonoFit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutonoFit.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Equipment> Equipment { get; set; }

        public DbSet<Goals> Goals { get; set; }

        public DbSet<ClientEquipment> ClientEquipment { get; set; }

        public DbSet<ClientExercise> ClientExercise { get; set; }

        public DbSet<ClientWeek> ClientWeek { get; set; }

        public DbSet<ClientWorkout> ClientWorkout { get; set; }

        public DbSet<PeriodGoals> PeriodGoals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                new IdentityRole
                {
                    Name = "Client",
                    NormalizedName = "CLIENT"
                });

            modelBuilder.Entity<Equipment>()
             .HasData(
             new Equipment
             {
                 EquipmentId = 1,
                 Name = "Barbell"
             },
             new Equipment
             {
                 EquipmentId = 2,
                 Name = "SZ-Bar"
             },
             new Equipment
             {
                 EquipmentId = 3,
                 Name = "Dumbbell"
             },
             new Equipment
             {
                 EquipmentId = 4,
                 Name = "Gym mat"
             },
             new Equipment
             {
                 EquipmentId = 5,
                 Name = "Swiss Ball"
             },
             new Equipment
             {
                 EquipmentId = 6,
                 Name = "Pull-up Bar"
             },
             new Equipment
             {
                 EquipmentId = 8,
                 Name = "Bench"
             },
             new Equipment
             {
                 EquipmentId = 9,
                 Name = "Incline Bench"
             },
             new Equipment
             {
                 EquipmentId = 10,
                 Name = "Kettlebell"

             });

            modelBuilder.Entity<Goals>()
            .HasData(
            new Goals
            {
                GoalId = 1,
                Name = "Strength"

            },
            new Goals
            {
                GoalId = 2,
                Name = "Hypertrophy"

            },
            new Goals
            {
                GoalId = 3,
                Name = "Muscular Endurance"

            },
            new Goals
            {
                GoalId = 4,
                Name = "Cardiovascular Endurance"

            },
            new Goals
            {
                GoalId = 5,
                Name = "Weightloss"

            });
        }

        public DbSet<AutonoFit.Models.Client> Client { get; set; }

        public DbSet<AutonoFit.Models.ClientProgram> ClientProgram { get; set; }
    }
}
