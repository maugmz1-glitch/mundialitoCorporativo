using Xunit;
using MundialitoCorporativo.Domain.Common;
using MundialitoCorporativo.Application.Common;

namespace MundialitoCorporativo.Tests;

/// <summary>
/// Result Pattern: Success, Failure, mensajes de error, códigos de error opcionales.
/// </summary>
public class ResultPatternTests
{
    [Fact]
    public void Success_ReturnsIsSuccessTrue_AndData()
    {
        var data = "test";
        var result = Result.Success(data);
        Assert.True(result.IsSuccess);
        Assert.Equal(data, result.Data);
        Assert.Equal(string.Empty, result.Message);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void Success_Generic_ReturnsCorrectType()
    {
        var value = 42;
        var result = Result<int>.Success(value);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Data);
    }

    [Fact]
    public void Failure_ReturnsIsSuccessFalse_AndMessage()
    {
        var msg = "Equipo no encontrado.";
        var result = Result.Failure<string>(msg);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(msg, result.Message);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void Failure_WithErrorCode_StoresErrorCode()
    {
        var result = Result.Failure<string>("Equipo no encontrado.", ErrorCodes.NotFound);
        Assert.False(result.IsSuccess);
        Assert.Equal("Equipo no encontrado.", result.Message);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    [Theory]
    [InlineData(ErrorCodes.NotFound)]
    [InlineData(ErrorCodes.Validation)]
    [InlineData(ErrorCodes.Conflict)]
    [InlineData(ErrorCodes.Duplicate)]
    public void Failure_ErrorCodes_AreStored(string errorCode)
    {
        var result = Result.Failure<int>("Error", errorCode);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorCode, result.ErrorCode);
    }

    [Fact]
    public void Failure_WithoutErrorCode_ErrorCodeIsNull()
    {
        var result = Result.Failure<string>("Solo mensaje");
        Assert.Null(result.ErrorCode);
    }
}
