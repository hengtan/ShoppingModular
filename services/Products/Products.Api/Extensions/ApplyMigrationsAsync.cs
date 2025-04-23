using Microsoft.EntityFrameworkCore;

namespace Products.Api.Extensions;

public static class WebAppExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        await db.Database.MigrateAsync();
    }
}