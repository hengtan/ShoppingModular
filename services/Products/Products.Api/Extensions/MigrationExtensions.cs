using Microsoft.EntityFrameworkCore;

namespace Products.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        const int maxRetries = 10;
        int retry = 0;

        while (retry < maxRetries)
        {
            try
            {
                var pending = await dbContext.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    Console.WriteLine($"📦 Applying migrations for {typeof(TContext).Name}...");
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine($"✅ Migrations applied for {typeof(TContext).Name}.");
                }
                else
                {
                    Console.WriteLine($"✔️ No pending migrations for {typeof(TContext).Name}.");
                }

                break;
            }
            catch (Exception ex)
            {
                retry++;
                Console.WriteLine($"⏳ Retry {retry}/{maxRetries} - Waiting for DB... Error: {ex.Message}");
                await Task.Delay(3000);
            }
        }

        if (retry == maxRetries)
            throw new Exception("❌ Failed to apply migrations after multiple retries.");
    }
}