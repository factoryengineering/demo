using Microsoft.EntityFrameworkCore;
using Festify.Api.Models;

namespace Festify.Api.Data;

public class FestifyDbContext(DbContextOptions<FestifyDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
}
