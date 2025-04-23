using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

namespace ShoppingModular.Infrastructure.Extensions;

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