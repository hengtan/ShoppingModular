using Microsoft.EntityFrameworkCore;
using ShoppingModular.Infrastructure.Orders;

namespace Orders.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task<IApplicationBuilder> ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        const int maxRetries = 10;
        int retry = 0;

        while (retry < maxRetries)
        {
            try
            {
                var pending = await dbContext.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    Console.WriteLine("📦 Pending migrations found. Applying...");
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("✅ Migrations applied successfully.");
                }
                else
                {
                    Console.WriteLine("✔️ No pending migrations. Database is up-to-date.");
                }

                break; // se tudo deu certo, sai do loop
            }
            catch (Exception ex)
            {
                retry++;
                Console.WriteLine($"⏳ Database not ready yet. Retrying {retry}/{maxRetries}...");
                Console.WriteLine($"Error: {ex.Message}");

                await Task.Delay(3000); // espera 3 segundos antes de tentar de novo
            }
        }

        if (retry == maxRetries)
        {
            throw new Exception("❌ Failed to connect to database after several retries.");
        }

        return app;
    }
}