using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MundialitoCorporativo.Application.Interfaces;
using MundialitoCorporativo.Domain.Entities;
using MundialitoCorporativo.Infrastructure.Persistence;

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
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Asegurar que PasswordHash y Salt sean nvarchar(max) con conexión propia (evita truncado si la migración no se aplicó).
        var connStr = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connStr))
        {
            try
            {
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "ALTER TABLE [dbo].[Users] ALTER COLUMN [PasswordHash] nvarchar(max) NOT NULL";
                    cmd.CommandTimeout = 30;
                    await cmd.ExecuteNonQueryAsync();
                }
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "ALTER TABLE [dbo].[Users] ALTER COLUMN [Salt] nvarchar(max) NOT NULL";
                    cmd.CommandTimeout = 30;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception)
            {
                // Tabla no existe o ya está bien; seguimos.
            }
        }

        if (await db.Users.AnyAsync()) return;

        var (hash, salt) = passwordHasher.HashPassword(DemoPassword);
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
