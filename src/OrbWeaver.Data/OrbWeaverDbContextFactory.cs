using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrbWeaver.Data;

public class OrbWeaverDbContextFactory : IDesignTimeDbContextFactory<OrbWeaverDbContext>
{
    public OrbWeaverDbContext CreateDbContext(string[] args)
    {
        // Build DbContextOptions with a hardcoded connection string for design-time
        // EF Core migrations will use this when running commands
        var optionsBuilder = new DbContextOptionsBuilder<OrbWeaverDbContext>();
        optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();

        return new OrbWeaverDbContext(optionsBuilder.Options);
    }
}

