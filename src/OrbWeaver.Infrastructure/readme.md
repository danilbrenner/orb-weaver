## EF Core Migrations

This solution uses:

* **Startup project:** `OrbWeaver.Host`
* **Data / migrations project:** `OrbWeaver.Data`

### Create a migration for a specific DbContext

```bash
dotnet ef migrations add <MigratioName> \
         --context OrbWeaverDbContext \
         --project ./src/OrbWeaver.Data/OrbWeaver.Data.csproj \
         --startup-project ./src/OrbWeaver.Host/OrbWeaver.Host.csproj
```

### Apply migrations to the database

```bash
dotnet ef database update --project ./src/OrbWeaver.Data/OrbWeaver.Data.csproj --connection "<CONNECTION_STRING>"
```

### Prerequisites

Ensure EF Core CLI tools are installed:

```bash
dotnet tool restore
```

