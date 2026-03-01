using Festify.DataWarehouse.Data;
using Festify.DataWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Festify.DataWarehouse.Services;

// Runs on a periodic schedule, picking up unprocessed events and executing
// any SQL scripts registered for their event type.
public class SqlScriptProcessingService(
    IServiceScopeFactory scopeFactory,
    ILogger<SqlScriptProcessingService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessPendingEventsAsync(stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DataWarehouseDbContext>();

        var pending = await db.Events
            .Where(e => !e.Processed)
            .OrderBy(e => e.ReceivedAt)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        logger.LogInformation("Processing {Count} pending event(s).", pending.Count);

        foreach (var ev in pending)
        {
            await ProcessEventAsync(ev, db, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessEventAsync(Event ev, DataWarehouseDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            var scripts = GetScriptsForEventType(ev.EventType);

            foreach (var script in scripts)
            {
                logger.LogDebug("Executing script {Script} for event {EventGuid}.", script, ev.EventGuid);
                await db.Database.ExecuteSqlRawAsync(
                    await File.ReadAllTextAsync(script, cancellationToken),
                    cancellationToken);
            }

            ev.Processed = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process event {EventGuid} of type {EventType}.", ev.EventGuid, ev.EventType);
        }
    }

    // Returns the ordered list of SQL script paths registered for a given event type.
    // Scripts live in the Scripts/ directory and are named <EventType>/<nn>-<description>.sql.
    private static IReadOnlyList<string> GetScriptsForEventType(string eventType)
    {
        var scriptsDir = Path.Combine(AppContext.BaseDirectory, "Scripts", eventType);

        if (!Directory.Exists(scriptsDir))
            return [];

        return Directory.GetFiles(scriptsDir, "*.sql")
            .OrderBy(f => f)
            .ToList();
    }
}
