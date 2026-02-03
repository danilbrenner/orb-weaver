using Microsoft.EntityFrameworkCore;
using OrbWeaver.Data.DataModel;

namespace OrbWeaver.Data;

public class OrbWeaverDbContext(DbContextOptions<OrbWeaverDbContext> options) : DbContext(options)
{
    public DbSet<AlertData> Alerts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrbWeaverDbContext).Assembly);
    }
}

