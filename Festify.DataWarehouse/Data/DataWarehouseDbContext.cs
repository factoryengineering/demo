using Festify.DataWarehouse.Models;
using Microsoft.EntityFrameworkCore;

namespace Festify.DataWarehouse.Data;

public class DataWarehouseDbContext(DbContextOptions<DataWarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");
            entity.HasKey(e => e.EventId);
            entity.HasIndex(e => e.EventGuid).IsUnique();
            entity.Property(e => e.EventType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Payload).HasColumnType("jsonb").IsRequired();
        });
    }
}
