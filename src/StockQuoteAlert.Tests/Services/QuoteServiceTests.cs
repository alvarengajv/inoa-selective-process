using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StockQuoteAlert.Infrastructure.ExternalServices;
using Xunit;
using YahooFinanceApi;

namespace StockQuoteAlert.Tests.Services;

public class QuoteServiceTests
{
    private static QuoteService CreateSut(Mock<IYahooFinanceClient> clientMock)
        => new QuoteService(clientMock.Object);

    private static Security CreateSecurity(string ticker, double price)
    {
        var fields = new Dictionary<string, dynamic>
        {
            ["Symbol"] = ticker,
            ["RegularMarketPrice"] = price
        };
        return (Security)Activator.CreateInstance(
            typeof(Security),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { fields },
            null)!;
    }

    private static Security CreateSecurityWithNullPrice(string ticker)
    {
        var fields = new Dictionary<string, dynamic>
        {
            ["Symbol"] = ticker,
            ["RegularMarketPrice"] = null!
        };
        return (Security)Activator.CreateInstance(
            typeof(Security),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { fields },
            null)!;
    }

    // ── Happy Path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetQuoteAsync_ValidTicker_ReturnsPositiveDecimal()
    {
        // Arrange
        const string ticker = "PETR4";
        var security = CreateSecurity(ticker, 28.5);
        var securities = new Dictionary<string, Security> { [ticker] = security };

        var clientMock = new Mock<IYahooFinanceClient>();
        clientMock
            .Setup(c => c.QueryAsync(ticker))
            .ReturnsAsync(securities);

        var sut = CreateSut(clientMock);

        // Act
        var result = await sut.GetQuoteAsync(ticker);

        // Assert
        result.Should().Be(28.5m);
        result.Should().BePositive();
    }

    // ── Validação de entrada ─────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetQuoteAsync_EmptyOrNullTicker_ThrowsArgumentException(string? ticker)
    {
        // Arrange
        var clientMock = new Mock<IYahooFinanceClient>();
        var sut = CreateSut(clientMock);

        // Act
        var act = async () => await sut.GetQuoteAsync(ticker!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("ticker");
    }

    // ── Ticker não encontrado ────────────────────────────────────────────────

    [Fact]
    public async Task GetQuoteAsync_TickerNotInResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        const string ticker = "INVALID";
        var clientMock = new Mock<IYahooFinanceClient>();
        clientMock
            .Setup(c => c.QueryAsync(ticker))
            .ReturnsAsync(new Dictionary<string, Security>());

        var sut = CreateSut(clientMock);

        // Act
        var act = async () => await sut.GetQuoteAsync(ticker);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ticker}*");
    }

    // ── Preço inválido ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetQuoteAsync_NullPrice_ThrowsInvalidOperationException()
    {
        // Arrange
        const string ticker = "PETR4";
        var security = CreateSecurityWithNullPrice(ticker);
        var securities = new Dictionary<string, Security> { [ticker] = security };

        var clientMock = new Mock<IYahooFinanceClient>();
        clientMock
            .Setup(c => c.QueryAsync(ticker))
            .ReturnsAsync(securities);

        var sut = CreateSut(clientMock);

        // Act
        var act = async () => await sut.GetQuoteAsync(ticker);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{ticker}*");
    }

    // ── Exceção externa encapsulada ──────────────────────────────────────────

    [Fact]
    public async Task GetQuoteAsync_ClientThrowsException_WrapsAsInvalidOperationException()
    {
        // Arrange
        const string ticker = "PETR4";
        var clientMock = new Mock<IYahooFinanceClient>();
        clientMock
            .Setup(c => c.QueryAsync(ticker))
            .ThrowsAsync(new HttpRequestException("Rede indisponível"));

        var sut = CreateSut(clientMock);

        // Act
        var act = async () => await sut.GetQuoteAsync(ticker);

        // Assert
        var assertion = await act.Should().ThrowAsync<InvalidOperationException>();
        assertion.WithMessage($"*{ticker}*");
        assertion.Which.InnerException.Should().BeOfType<HttpRequestException>();
    }
}
