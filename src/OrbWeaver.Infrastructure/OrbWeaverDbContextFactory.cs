using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrbWeaver.Infrastructure;

public class OrbWeaverDbContextFactory : IDesignTimeDbContextFactory<OrbWeaverDbContext>
{
    public OrbWeaverDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrbWeaverDbContext>();
        optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();

        return new OrbWeaverDbContext(optionsBuilder.Options);
    }
}

