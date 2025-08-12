using Microsoft.EntityFrameworkCore;
using AgentStateAPI.Models;

namespace AgentStateAPI.Data;

public class AgentStateDbContext : DbContext
{
    public AgentStateDbContext(DbContextOptions<AgentStateDbContext> options) 
        : base(options) { }

    public DbSet<Agent> Agents { get; set; }
    public DbSet<AgentSkill> AgentSkills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.State).HasConversion<string>();
        });

        modelBuilder.Entity<AgentSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Agent)
                  .WithMany(a => a.Skills)
                  .HasForeignKey(e => e.AgentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.AgentId, e.QueueId }).IsUnique();
        });
    }
}