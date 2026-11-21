using Microsoft.EntityFrameworkCore;
using OfficerService.Entities;

namespace OfficerService.Data;

public class OfficerDbContext : DbContext
{
    public OfficerDbContext(DbContextOptions<OfficerDbContext> options) : base(options)
    {
    }

    public DbSet<Officer> Officers => Set<Officer>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<ComplianceCheck> ComplianceChecks => Set<ComplianceCheck>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<ComplianceRecord> ComplianceRecords => Set<ComplianceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Officer>(e =>
        {
            e.Property(o => o.EmployeeNumber).IsRequired().HasMaxLength(20);
            e.Property(o => o.FullName).IsRequired().HasMaxLength(100);
            e.Property(o => o.Email).IsRequired().HasMaxLength(150);
            e.Property(o => o.Phone).IsRequired().HasMaxLength(20);
            e.Property(o => o.Region).IsRequired().HasMaxLength(100);
            e.Property(o => o.PasswordHash).IsRequired().HasMaxLength(500);

            e.HasIndex(o => o.EmployeeNumber).IsUnique();
            e.HasIndex(o => o.Email).IsUnique();
        });

        modelBuilder.Entity<VerificationRequest>(e =>
        {
            e.Property(v => v.ReferenceNumber).IsRequired().HasMaxLength(30);
            e.Property(v => v.FarmerName).IsRequired().HasMaxLength(150);
            e.Property(v => v.Region).IsRequired().HasMaxLength(100);
            e.Property(v => v.ItemName).IsRequired().HasMaxLength(200);
            e.Property(v => v.Quantity).HasColumnType("decimal(18,2)");
            e.Property(v => v.DeliveryLocation).IsRequired().HasMaxLength(300);
            e.Property(v => v.DecisionRemarks).HasMaxLength(1000);
            e.Property(v => v.LastCallbackError).HasMaxLength(500);

            e.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(v => v.CallbackStatus).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(v => v.ReferenceNumber).IsUnique();
            e.HasIndex(v => v.SourceRequestId).IsUnique();
            e.HasIndex(v => new { v.AssignedOfficerId, v.Status });

            e.HasOne(v => v.AssignedOfficer)
                .WithMany(o => o.VerificationRequests)
                .HasForeignKey(v => v.AssignedOfficerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Quotation>(e =>
        {
            e.Property(q => q.SupplierName).IsRequired().HasMaxLength(150);
            e.Property(q => q.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(q => q.StockQuantity).HasColumnType("decimal(18,2)");
            e.Property(q => q.Notes).HasMaxLength(1000);
            e.Property(q => q.SuitabilityNotes).HasMaxLength(1000);
            e.Property(q => q.Suitability).HasConversion<string>().HasMaxLength(30);

            e.HasIndex(q => q.SourceQuotationId).IsUnique();
            e.HasIndex(q => new { q.VerificationRequestId, q.SupplierId }).IsUnique();

            e.HasOne(q => q.VerificationRequest)
                .WithMany(v => v.Quotations)
                .HasForeignKey(q => q.VerificationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComplianceCheck>(e =>
        {
            e.Property(c => c.Criterion).IsRequired().HasMaxLength(300);
            e.Property(c => c.Remarks).HasMaxLength(1000);

            e.HasOne(c => c.VerificationRequest)
                .WithMany(v => v.Checks)
                .HasForeignKey(c => c.VerificationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict avoids SQL Server's "multiple cascade paths" error.
            e.HasOne(c => c.Quotation)
                .WithMany()
                .HasForeignKey(c => c.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Recommendation>(e =>
        {
            e.Property(r => r.Title).IsRequired().HasMaxLength(150);
            e.Property(r => r.Details).IsRequired().HasMaxLength(4000);
            e.Property(r => r.Priority).HasConversion<string>().HasMaxLength(20);

            e.HasOne(r => r.Officer)
                .WithMany(o => o.Recommendations)
                .HasForeignKey(r => r.OfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.VerificationRequest)
                .WithMany(v => v.Recommendations)
                .HasForeignKey(r => r.VerificationRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComplianceRecord>(e =>
        {
            e.Property(c => c.SubjectName).IsRequired().HasMaxLength(150);
            e.Property(c => c.Standard).IsRequired().HasMaxLength(200);
            e.Property(c => c.Findings).IsRequired().HasMaxLength(2000);
            e.Property(c => c.CorrectiveAction).HasMaxLength(1000);
            e.Property(c => c.SubjectType).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(c => new { c.SubjectType, c.SubjectId });

            e.HasOne(c => c.Officer)
                .WithMany(o => o.ComplianceRecords)
                .HasForeignKey(c => c.OfficerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}