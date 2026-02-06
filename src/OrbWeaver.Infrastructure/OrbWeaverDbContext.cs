using Microsoft.EntityFrameworkCore;
using OrbWeaver.Infrastructure.DataModel;

namespace OrbWeaver.Infrastructure;

public class OrbWeaverDbContext(DbContextOptions<OrbWeaverDbContext> options) : DbContext(options)
{
    public DbSet<AlertData> Alerts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrbWeaverDbContext).Assembly);
    }
}

