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

            //AssessmentStatus enum to string conversion + Schedule → AssessmentHeader (Restrict)
            modelBuilder.Entity<AssessmentHeader>(entity =>
            {
                entity.Property(h => h.AssessmentStatus)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.Property(h => h.AssessmentCode)
                    .HasMaxLength(50);

                entity.HasIndex(h => h.AssessmentCode)
                    .IsUnique();

                entity.HasOne(h => h.Schedule)
                    .WithMany(s => s.AssessmentHeaders)
                    .HasForeignKey(h => h.ScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Workflow/WorkflowStep dictionary (statuses are DB-driven, not enums)
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.Property(w => w.WorkflowCode)
                    .HasMaxLength(64);

                entity.Property(w => w.WorkflowName)
                    .HasMaxLength(200);

                entity.HasIndex(w => w.WorkflowCode)
                    .IsUnique();
            });

            modelBuilder.Entity<WorkflowStep>(entity =>
            {
                entity.Property(s => s.StepCode)
                    .HasMaxLength(64);

                entity.Property(s => s.StepLabel)
                    .HasMaxLength(128);

                entity.HasOne(s => s.Workflow)
                    .WithMany(w => w.WorkflowSteps)
                    .HasForeignKey(s => s.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => new { s.WorkflowId, s.StepCode })
                    .IsUnique();
            });

            //AssessmentUnit relationships (Header → Unit Cascade, Unit/WorkflowStep/User Restrict)
            modelBuilder.Entity<AssessmentUnit>(entity =>
            {
                entity.HasOne(u => u.AssessmentHeader)
                    .WithMany(h => h.AssessmentUnits)
                    .HasForeignKey(u => u.AssessmentHeaderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(u => u.Unit)
                    .WithMany()
                    .HasForeignKey(u => u.UnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.WorkflowStep)
                    .WithMany()
                    .HasForeignKey(u => u.WorkflowStepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.AuthorizedBy)
                    .WithMany()
                    .HasForeignKey(u => u.AuthorizedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(u => new { u.AssessmentHeaderId, u.UnitId })
                    .IsUnique();

                entity.HasIndex(u => u.UnitId);
            });

            //AssessmentItem polymorphic links (Unit → Item Cascade, workflow/User Restrict)
            modelBuilder.Entity<AssessmentItem>(entity =>
            {
                entity.Property(i => i.ItemType)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.HasOne(i => i.AssessmentUnit)
                    .WithMany(u => u.AssessmentItems)
                    .HasForeignKey(i => i.AssessmentUnitId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.WorkflowStep)
                    .WithMany()
                    .HasForeignKey(i => i.WorkflowStepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.SubmittedBy)
                    .WithMany()
                    .HasForeignKey(i => i.SubmittedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.ApprovedBy)
                    .WithMany()
                    .HasForeignKey(i => i.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(i => new { i.AssessmentUnitId, i.ItemType, i.ItemId })
                    .IsUnique();

                entity.HasIndex(i => new { i.ItemType, i.ItemId });
            });

            //SAQ answers (Item → answer Cascade, Question/Option Restrict)
            modelBuilder.Entity<SaqAssessmentAnswer>(entity =>
            {
                entity.HasOne(a => a.AssessmentItem)
                    .WithMany(i => i.SaqAssessmentAnswers)
                    .HasForeignKey(a => a.AssessmentItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Question)
                    .WithMany()
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Option)
                    .WithMany()
                    .HasForeignKey(a => a.OptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => new { a.AssessmentItemId, a.QuestionId })
                    .IsUnique();
            });

            //KRI values (Item → value Cascade, Kri Restrict)
            modelBuilder.Entity<KriAssessmentValue>(entity =>
            {
                entity.HasOne(v => v.AssessmentItem)
                    .WithMany(i => i.KriAssessmentValues)
                    .HasForeignKey(v => v.AssessmentItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Kri)
                    .WithMany()
                    .HasForeignKey(v => v.KriId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(v => new { v.AssessmentItemId, v.KriId })
                    .IsUnique();
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

        public DbSet<AssessmentHeader> AssessmentHeaders { get; set; }

        public DbSet<AssessmentUnit> AssessmentUnits { get; set; }

        public DbSet<AssessmentItem> AssessmentItems { get; set; }

        public DbSet<Workflow> Workflows { get; set; }

        public DbSet<WorkflowStep> WorkflowSteps { get; set; }

        public DbSet<SaqAssessmentAnswer> SaqAssessmentAnswers { get; set; }

        public DbSet<KriAssessmentValue> KriAssessmentValues { get; set; }

    }
}
