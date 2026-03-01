using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Festify.DataWarehouse.Data;

// Used by dotnet-ef at design time (migrations, database updates).
// Not used at runtime — the host registers the context via DI.
public class DataWarehouseDbContextFactory : IDesignTimeDbContextFactory<DataWarehouseDbContext>
{
    public DataWarehouseDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DataWarehouseDbContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("FESTIFY_DW_CONNECTION_STRING")
                ?? "Host=localhost;Database=festify_dw;Username=postgres;Password=postgres")
            .Options;

        return new DataWarehouseDbContext(options);
    }
}
