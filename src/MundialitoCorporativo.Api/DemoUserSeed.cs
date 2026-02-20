using Microsoft.EntityFrameworkCore;
using MundialitoCorporativo.Api.Services;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Entities;

namespace MundialitoCorporativo.Api;

/// <summary>
/// Crea el usuario demo (demo / Demo123!) si no existe ningún usuario en la BD.
/// </summary>
public static class DemoUserSeed
{
    public const string DemoUserName = "demo";
    public const string DemoPassword = "Demo123!";

    public static async Task EnsureDemoUserAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        if (await db.Users.AnyAsync()) return;

        var (hash, salt) = PasswordHasher.HashPassword(DemoPassword);
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = DemoUserName,
            Email = "demo@mundialito.local",
            PasswordHash = hash,
            Salt = salt,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
