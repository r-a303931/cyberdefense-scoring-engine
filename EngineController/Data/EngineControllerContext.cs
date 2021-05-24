using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EngineController.Models;

namespace EngineController.Data
{
    public class EngineControllerContext : DbContext
    {
        public EngineControllerContext (DbContextOptions<EngineControllerContext> options)
            : base(options)
        {
        }

        public DbSet<CompetitionTask> CompetitionTask { get; set; }
        public DbSet<CompetitionPenalty> CompetitionPenalty { get; set; }


        public DbSet<AppliedCompetitionPenalty> AppliedCompetitionPenalties { get; set; }
        public DbSet<CompletedCompetitionTask> CompletedCompetitionTasks { get; set; }


        public DbSet<Team> Teams { get; set; }


        public DbSet<Readme> Readme { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            modelBuilder.Entity<CompetitionTask>().ToTable("CompetitionTasks");
            modelBuilder.Entity<CompetitionPenalty>().ToTable("CompetitionPenalties");

            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .ToTable("AppliedCompetitionPenalties")
                .HasKey(p => new { p.CompetitionPenaltyID, p.TeamID });
            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .HasOne(p => p.Team)
                .WithMany()
                .HasForeignKey(p => p.TeamID);
            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .HasOne(p => p.CompetitionPenalty)
                .WithMany()
                .HasForeignKey(p => p.CompetitionPenaltyID);

            modelBuilder.Entity<CompletedCompetitionTask>()
                .ToTable("CompletedCompetitionTasks")
                .HasKey(p => new { p.CompetitionTaskID, p.TeamID });
            modelBuilder.Entity<CompletedCompetitionTask>()
                .HasOne(t => t.Team)
                .WithMany()
                .HasForeignKey(t => t.TeamID);
            modelBuilder.Entity<CompletedCompetitionTask>()
                .HasOne(t => t.CompetitionTask)
                .WithMany()
                .HasForeignKey(t => t.CompetitionTaskID);

            modelBuilder.Entity<Team>()
                .ToTable("Teams");
            modelBuilder.Entity<Team>()
                .HasMany(t => t.AppliedCompetitionPenalties)
                .WithOne();
			modelBuilder.Entity<Team>()
				.HasMany(t => t.CompletedCompetitionTasks)
				.WithOne();

            modelBuilder.Entity<Readme>()
                .ToTable("Readme")
                .HasKey(r => r.ID);
		}
    }
}
