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


        public DbSet<CompetitionSystem> CompetitionSystems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            modelBuilder.Entity<CompetitionTask>()
                .ToTable("CompetitionTasks");
            modelBuilder.Entity<CompetitionPenalty>()
                .ToTable("CompetitionPenalties");

            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .ToTable("AppliedCompetitionPenalties")
                .HasKey(p => new { p.CompetitionPenaltyID, p.TeamID });
            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .HasOne(p => p.Team)
                .WithMany()
                .HasForeignKey(p => p.TeamID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .HasOne(p => p.CompetitionPenalty)
                .WithMany()
                .HasForeignKey(p => p.CompetitionPenaltyID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletedCompetitionTask>()
                .ToTable("CompletedCompetitionTasks")
                .HasKey(p => new { p.CompetitionTaskID, p.TeamID });
            modelBuilder.Entity<CompletedCompetitionTask>()
                .HasOne(t => t.Team)
                .WithMany()
                .HasForeignKey(t => t.TeamID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CompletedCompetitionTask>()
                .HasOne(t => t.CompetitionTask)
                .WithMany()
                .HasForeignKey(t => t.CompetitionTaskID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>()
                .ToTable("Teams");
            modelBuilder.Entity<Team>()
                .HasMany(t => t.AppliedCompetitionPenalties)
                .WithOne();
			modelBuilder.Entity<Team>()
				.HasMany(t => t.CompletedCompetitionTasks)
				.WithOne();

            modelBuilder.Entity<CompetitionSystem>()
                .ToTable("CompetitionSystem");
            modelBuilder.Entity<CompetitionSystem>()
                .HasMany(s => s.CompetitionPenalties)
                .WithOne()
                .HasForeignKey(t => t.SystemIdentifier);
            modelBuilder.Entity<CompetitionSystem>()
                .HasMany(s => s.CompetitionTasks)
                .WithOne()
                .HasForeignKey(t => t.SystemIdentifier);

            modelBuilder.Entity<RegisteredVirtualMachines>()
                .ToTable("RegisteredVirtualMachines");
            modelBuilder.Entity<RegisteredVirtualMachines>()
                .HasOne(r => r.Team)
                .WithMany()
                .HasForeignKey(r => r.TeamID);
            modelBuilder.Entity<RegisteredVirtualMachines>()
                .HasOne(r => r.CompetitionSystem)
                .WithMany()
                .HasForeignKey(r => r.SystemIdentifier);
		}
    }
}
