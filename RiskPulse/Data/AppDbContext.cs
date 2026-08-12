using Microsoft.EntityFrameworkCore;
using RiskPulse.Models.DbModel.AccessControl;
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

    }
}
