using Microsoft.EntityFrameworkCore;
using RiskPulse.Models.DbModel.AccessControl;
using RiskPulse.Models.DbModel.Kri;
using RiskPulse.Models.DbModel.Saq;

namespace RiskPulse.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("riskpulse");

            //UnitType enum to string conversion
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(u => u.UnitType)
                    .HasConversion<string>()
                    .HasMaxLength(32);
            });

            //SaqStatus enum to string conversion
            modelBuilder.Entity<SaqHeader>(entity =>
            {
                entity.Property(s => s.SaqStatus)
                    .HasConversion<string>()
                    .HasMaxLength(32);
            });

            //QuestionType enum to string conversion
            modelBuilder.Entity<SaqQuestion>(entity =>
            {
                entity.Property(q => q.QuestionType)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.Property(q => q.AllowComment)
                    .HasDefaultValue(true);

                //QuestionId is the real FK to SaqHeader's questions (replaces shadow SaqQuestionQuestionId)
                entity.HasMany(q => q.SaqQuestionOptions)
                    .WithOne(o => o.SaqQuestion)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //KriStatus enum to string conversion
            modelBuilder.Entity<KriHeader>(entity =>
            {
                entity.Property(h => h.KriStatus)
                    .HasConversion<string>()
                    .HasMaxLength(32);
            });

            //Kri relationships
            modelBuilder.Entity<Kri>(entity =>
            {
                entity.Property(k => k.AllowComment)
                    .HasDefaultValue(true);

                entity.HasOne(k => k.KriHeader)
                    .WithMany(h => h.Kris)
                    .HasForeignKey(k => k.KriHeaderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(k => k.KriThresholdGroup)
                    .WithMany(g => g.Kris)
                    .HasForeignKey(k => k.KriThresholdGroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //KriThreshold relationships
            modelBuilder.Entity<KriThreshold>(entity =>
            {
                entity.HasOne(t => t.KriThresholdGroup)
                    .WithMany(g => g.KriThresholds)
                    .HasForeignKey(t => t.KriThresholdGroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Color)
                    .WithMany(c => c.KriThresholds)
                    .HasForeignKey(t => t.ColorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Unit> Units { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<SaqHeader> SaqHeaders { get; set; }

        public DbSet<SaqQuestion> SaqQuestions { get; set; }

        public DbSet<SaqQuestionOption> SaqQuestionOptions { get; set; }

        public DbSet<KriHeader> KriHeaders { get; set; }

        public DbSet<Kri> Kris { get; set; }

        public DbSet<KriThresholdGroup> KriThresholdGroups { get; set; }

        public DbSet<KriThresholdColor> KriThresholdColors { get; set; }

        public DbSet<KriThreshold> KriThresholds { get; set; }

    }
}
