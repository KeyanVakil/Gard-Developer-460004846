using GardPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace GardPortal.Data;

public class GardDbContext : DbContext
{
    public GardDbContext(DbContextOptions<GardDbContext> options) : base(options) { }

    public DbSet<Vessel> Vessels => Set<Vessel>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimHistory> ClaimHistories => Set<ClaimHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vessel
        modelBuilder.Entity<Vessel>(e =>
        {
            e.HasKey(v => v.Id);
            e.HasIndex(v => v.ImoNumber).IsUnique();
            e.Property(v => v.Name).HasMaxLength(200).IsRequired();
            e.Property(v => v.ImoNumber).HasMaxLength(7).IsRequired();
            e.Property(v => v.FlagState).HasMaxLength(100).IsRequired();
            e.Property(v => v.GrossTonnage).HasColumnType("decimal(18,2)");
            e.Property(v => v.Notes).HasMaxLength(500);
        });

        // Policy
        modelBuilder.Entity<Policy>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.PolicyNumber).IsUnique();
            e.Property(p => p.PolicyNumber).HasMaxLength(50).IsRequired();
            e.Property(p => p.InsuredValue).HasColumnType("decimal(18,2)");
            e.Property(p => p.AnnualPremium).HasColumnType("decimal(18,2)");
            e.Property(p => p.Notes).HasMaxLength(1000);
            e.HasOne(p => p.Vessel)
             .WithMany(v => v.Policies)
             .HasForeignKey(p => p.VesselId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Claim
        modelBuilder.Entity<Claim>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.ClaimNumber).IsUnique();
            e.Property(c => c.ClaimNumber).HasMaxLength(50).IsRequired();
            e.Property(c => c.Description).HasMaxLength(2000).IsRequired();
            e.Property(c => c.EstimatedAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.SettledAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.Notes).HasMaxLength(500);
            e.HasOne(c => c.Policy)
             .WithMany(p => p.Claims)
             .HasForeignKey(c => c.PolicyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ClaimHistory
        modelBuilder.Entity<ClaimHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Notes).HasMaxLength(1000);
            e.Property(h => h.ChangedBy).HasMaxLength(200);
            e.HasOne(h => h.Claim)
             .WithMany(c => c.History)
             .HasForeignKey(h => h.ClaimId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
