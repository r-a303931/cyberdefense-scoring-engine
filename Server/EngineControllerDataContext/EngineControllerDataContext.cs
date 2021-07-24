using Microsoft.EntityFrameworkCore;
using EngineController.Models;

namespace EngineController.Data
{
    public class EngineControllerContext : DbContext
    {
        public EngineControllerContext(DbContextOptions<EngineControllerContext> options)
            : base(options)
        {
        }

        public DbSet<CompetitionTask> CompetitionTask { get; set; }
        public DbSet<CompetitionPenalty> CompetitionPenalty { get; set; }


        public DbSet<AppliedCompetitionPenalty> AppliedCompetitionPenalties { get; set; }
        public DbSet<CompletedCompetitionTask> CompletedCompetitionTasks { get; set; }


        public DbSet<Team> Teams { get; set; }


        public DbSet<CompetitionSystem> CompetitionSystems { get; set; }

        public DbSet<RegisteredVirtualMachine> RegisteredVirtualMachines { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompetitionTask>()
                .ToTable("CompetitionTasks");
            modelBuilder.Entity<CompetitionPenalty>()
                .ToTable("CompetitionPenalties");

            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .ToTable("AppliedCompetitionPenalties")
                .HasKey(p => new { p.CompetitionPenaltyID, p.TeamID, p.VmId });
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
            modelBuilder.Entity<AppliedCompetitionPenalty>()
                .HasOne(p => p.AppliedVirtualMachine)
                .WithMany()
                .HasForeignKey(p => new { p.TeamID, p.VmId })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletedCompetitionTask>()
                .ToTable("CompletedCompetitionTasks")
                .HasKey(p => new { p.CompetitionTaskID, p.TeamID, p.VmId });
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
            modelBuilder.Entity<CompletedCompetitionTask>()
                .HasOne(t => t.AppliedVirtualMachine)
                .WithMany()
                .HasForeignKey(t => new { t.TeamID, t.VmId })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>()
                .ToTable("Teams");
            modelBuilder.Entity<Team>()
                .HasMany(t => t.AppliedCompetitionPenalties)
                .WithOne();
            modelBuilder.Entity<Team>()
                .HasMany(t => t.CompletedCompetitionTasks)
                .WithOne();
            modelBuilder.Entity<Team>()
                .HasMany(t => t.RegisteredVirtualMachines)
                .WithOne()
                .HasForeignKey(r => r.TeamID);

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

            modelBuilder.Entity<RegisteredVirtualMachine>()
                .ToTable("RegisteredVirtualMachines")
                .HasKey(r => new { r.TeamID, r.VmId });
            modelBuilder.Entity<RegisteredVirtualMachine>()
                .HasOne(r => r.Team)
                .WithMany()
                .HasForeignKey(r => r.TeamID);
            modelBuilder.Entity<RegisteredVirtualMachine>()
                .HasOne(r => r.CompetitionSystem)
                .WithMany()
                .HasForeignKey(r => r.SystemIdentifier);
            modelBuilder.Entity<RegisteredVirtualMachine>()
                .HasMany(vm => vm.CompetitionPenalties)
                .WithOne()
                .HasForeignKey(p => new { p.TeamID, p.VmId });
            modelBuilder.Entity<RegisteredVirtualMachine>()
                .HasMany(vm => vm.CompetitionTasks)
                .WithOne()
                .HasForeignKey(t => new { t.TeamID, t.VmId });
        }
    }
}
