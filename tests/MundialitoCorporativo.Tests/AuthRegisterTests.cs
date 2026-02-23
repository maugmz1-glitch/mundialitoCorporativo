using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MundialitoCorporativo.Application.Auth.Commands;
using MundialitoCorporativo.Application.Common;
using MundialitoCorporativo.Domain.Entities;
using MundialitoCorporativo.Infrastructure.Persistence;
using MundialitoCorporativo.Infrastructure.Services;

namespace MundialitoCorporativo.Tests;

/// <summary>
/// Pruebas del flujo de registro de usuario (RegisterCommandHandler).
/// </summary>
public class AuthRegisterTests
{
    private static AppDbContext CreateInMemoryDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        await using var db = CreateInMemoryDb(nameof(Register_ValidUser_ReturnsSuccess));
        var hasher = new PasswordHasherService();
        var handler = new RegisterCommandHandler(db, hasher);

        var result = await handler.Handle(
            new RegisterCommand("newuser", "new@example.com", "Password123!"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("newuser", result.Data!.UserName);
        Assert.Contains("Cuenta creada", result.Data.Message);

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == "newuser");
        Assert.NotNull(user);
        Assert.Equal("new@example.com", user.Email);
        Assert.NotNull(user.PasswordHash);
        Assert.NotNull(user.Salt);
    }

    [Fact]
    public async Task Register_ValidUser_WithoutEmail_ReturnsSuccess()
    {
        await using var db = CreateInMemoryDb(nameof(Register_ValidUser_WithoutEmail_ReturnsSuccess));
        var hasher = new PasswordHasherService();
        var handler = new RegisterCommandHandler(db, hasher);

        var result = await handler.Handle(
            new RegisterCommand("nouser", null, "Pass1234!"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("nouser", result.Data!.UserName);
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == "nouser");
        Assert.NotNull(user);
        Assert.Null(user.Email);
    }

    [Fact]
    public async Task Register_UserNameTooShort_ReturnsValidationFailure()
    {
        await using var db = CreateInMemoryDb(nameof(Register_UserNameTooShort_ReturnsValidationFailure));
        var hasher = new PasswordHasherService();
        var handler = new RegisterCommandHandler(db, hasher);

        var result = await handler.Handle(
            new RegisterCommand("ab", "a@b.com", "Password123!"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
        Assert.Contains("3 caracteres", result.Message);
    }

    [Fact]
    public async Task Register_PasswordTooShort_ReturnsValidationFailure()
    {
        await using var db = CreateInMemoryDb(nameof(Register_PasswordTooShort_ReturnsValidationFailure));
        var hasher = new PasswordHasherService();
        var handler = new RegisterCommandHandler(db, hasher);

        var result = await handler.Handle(
            new RegisterCommand("validuser", null, "12345"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
        Assert.Contains("6 caracteres", result.Message);
    }

    [Fact]
    public async Task Register_DuplicateUserName_ReturnsConflict()
    {
        await using var db = CreateInMemoryDb(nameof(Register_DuplicateUserName_ReturnsConflict));
        var hasher = new PasswordHasherService();
        var (hash, salt) = hasher.HashPassword("FirstPass1!");
        db.Users.Add(new User
        {
            Id = System.Guid.NewGuid(),
            UserName = "duplicate",
            Email = null,
            PasswordHash = hash,
            Salt = salt,
            CreatedAtUtc = System.DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db, hasher);
        var result = await handler.Handle(
            new RegisterCommand("duplicate", "other@example.com", "OtherPass1!"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
        Assert.Contains("Ya existe un usuario", result.Message);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        await using var db = CreateInMemoryDb(nameof(Register_DuplicateEmail_ReturnsConflict));
        var hasher = new PasswordHasherService();
        var (hash, salt) = hasher.HashPassword("FirstPass1!");
        db.Users.Add(new User
        {
            Id = System.Guid.NewGuid(),
            UserName = "firstuser",
            Email = "same@example.com",
            PasswordHash = hash,
            Salt = salt,
            CreatedAtUtc = System.DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db, hasher);
        var result = await handler.Handle(
            new RegisterCommand("seconduser", "same@example.com", "OtherPass1!"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
        Assert.Contains("correo", result.Message);
    }

    [Fact]
    public async Task Register_EmptyUserName_ReturnsValidationFailure()
    {
        await using var db = CreateInMemoryDb(nameof(Register_EmptyUserName_ReturnsValidationFailure));
        var hasher = new PasswordHasherService();
        var handler = new RegisterCommandHandler(db, hasher);

        var result = await handler.Handle(
            new RegisterCommand("   ", "a@b.com", "Password123!"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
    }
}
