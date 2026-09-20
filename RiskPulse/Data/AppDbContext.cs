using Microsoft.EntityFrameworkCore;
using RiskPulse.Data.Entries;

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

            //UnitGroup relationships (many-to-many Units ↔ Groups)
            modelBuilder.Entity<UnitGroup>(entity =>
            {
                entity.HasOne(ug => ug.Group)
                    .WithMany(g => g.UnitGroups)
                    .HasForeignKey(ug => ug.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Unit)
                    .WithMany(u => u.UnitGroups)
                    .HasForeignKey(ug => ug.UnitId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ug => new { ug.GroupId, ug.UnitId })
                    .IsUnique();
            });

            //SaqStatus enum to string conversion
            modelBuilder.Entity<SaqHeader>(entity =>
            {
                entity.Property(s => s.SaqStatus)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.Property(s => s.SaqCode)
                    .HasMaxLength(50);

                entity.HasIndex(s => s.SaqCode)
                    .IsUnique();

                entity.HasOne(h => h.Group)
                    .WithMany(g => g.SaqHeaders)
                    .HasForeignKey(h => h.GroupId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Unit)
                    .WithMany(u => u.SaqHeaders)
                    .HasForeignKey(h => h.UnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(h => h.SaqQuestions)
                    .WithOne(q => q.SaqHeader)
                    .HasForeignKey(q => q.SaqHeaderId)
                    .OnDelete(DeleteBehavior.Cascade);
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

                entity.Property(h => h.KriCode)
                    .HasMaxLength(50);

                entity.HasIndex(h => h.KriCode)
                    .IsUnique();

                entity.HasOne(h => h.Group)
                    .WithMany(g => g.KriHeaders)
                    .HasForeignKey(h => h.GroupId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Unit)
                    .WithMany(u => u.KriHeaders)
                    .HasForeignKey(h => h.UnitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //ScheduleStatus/ScheduleType enum to string conversion
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(s => s.ScheduleStatus)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.Property(s => s.ScheduleType)
                    .HasConversion<string>()
                    .HasMaxLength(32);
            });

            //ScheduleItem polymorphic links (Schedule ↔ SaqHeader/KriHeader by ItemType + ItemId; ItemId has no FK)
            modelBuilder.Entity<ScheduleItem>(entity =>
            {
                entity.Property(si => si.ItemType)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.HasOne(si => si.Schedule)
                    .WithMany(s => s.ScheduleItems)
                    .HasForeignKey(si => si.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(si => new { si.ScheduleId, si.ItemType, si.ItemId })
                    .IsUnique();

                entity.HasIndex(si => new { si.ItemType, si.ItemId });
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
            });
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Unit> Units { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<UnitGroup> UnitGroups { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<SaqHeader> SaqHeaders { get; set; }

        public DbSet<SaqQuestion> SaqQuestions { get; set; }

        public DbSet<SaqQuestionOption> SaqQuestionOptions { get; set; }

        public DbSet<KriHeader> KriHeaders { get; set; }

        public DbSet<Kri> Kris { get; set; }

        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<ScheduleItem> ScheduleItems { get; set; }

    }
}
