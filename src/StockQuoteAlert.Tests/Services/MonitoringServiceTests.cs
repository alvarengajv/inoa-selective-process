using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StockQuoteAlert.Application.DTOs;
using StockQuoteAlert.Application.Interfaces;
using StockQuoteAlert.Application.Services;
using Xunit;

namespace StockQuoteAlert.Tests.Services;

public class MonitoringServiceTests
{
    // BuyThreshold = 10, SellThreshold = 20
    // Buy zone  : preço <= 10
    // Normal    : 10 < preço < 20
    // Sell zone : preço >= 20

    private static MonitoringService CreateSut(
        Mock<IQuoteService> quoteServiceMock,
        Mock<IEmailService> emailServiceMock)
    {
        var loggerMock = new Mock<ILogger<MonitoringService>>();
        return new MonitoringService(
            emailServiceMock.Object,
            quoteServiceMock.Object,
            monitoringIntervalSeconds: 1,
            loggerMock.Object);
    }

    private static MonitoredAssetDto CreateDto() => new()
    {
        Ticker = "PETR4",
        BuyThreshold = 10m,
        SellThreshold = 20m
    };

    // ── Happy Path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task HappyPath_PriceBelowBuyThreshold_SendsBuyAlert()
    {
        // Arrange
        var quoteServiceMock = new Mock<IQuoteService>();
        var emailServiceMock = new Mock<IEmailService>();
        var cts = new CancellationTokenSource();

        quoteServiceMock
            .Setup(q => q.GetQuoteAsync(It.IsAny<string>()))
            .ReturnsAsync(8m)
            .Callback(cts.Cancel);

        emailServiceMock
            .Setup(e => e.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(quoteServiceMock, emailServiceMock);

        // Act
        await sut.StartMonitoringAsync(CreateDto(), cts.Token);

        // Assert
        emailServiceMock.Verify(
            e => e.SendAlertAsync(
                It.Is<string>(s => s.Contains("Compra")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task HappyPath_PriceAboveSellThreshold_SendsSellAlert()
    {
        // Arrange
        var quoteServiceMock = new Mock<IQuoteService>();
        var emailServiceMock = new Mock<IEmailService>();
        var cts = new CancellationTokenSource();

        quoteServiceMock
            .Setup(q => q.GetQuoteAsync(It.IsAny<string>()))
            .ReturnsAsync(25m)
            .Callback(cts.Cancel);

        emailServiceMock
            .Setup(e => e.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(quoteServiceMock, emailServiceMock);

        // Act
        await sut.StartMonitoringAsync(CreateDto(), cts.Token);

        // Assert
        emailServiceMock.Verify(
            e => e.SendAlertAsync(
                It.Is<string>(s => s.Contains("Venda")),
                It.IsAny<string>()),
            Times.Once);
    }

    // ── Anti-Spam ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DoesNotSendDuplicateAlerts_BuyAlertStaysActive_SingleEmail()
    {
        // Arrange
        var quoteServiceMock = new Mock<IQuoteService>();
        var emailServiceMock = new Mock<IEmailService>();
        var cts = new CancellationTokenSource();
        var callCount = 0;

        // 3 ticks consecutivos na buy zone — alerta só deve disparar uma vez
        quoteServiceMock
            .Setup(q => q.GetQuoteAsync(It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount >= 3)
                    cts.Cancel();
                return Task.FromResult(callCount switch
                {
                    1 => 8m,
                    2 => 7m,
                    _ => 6m
                });
            });

        emailServiceMock
            .Setup(e => e.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(quoteServiceMock, emailServiceMock);

        // Act
        await sut.StartMonitoringAsync(CreateDto(), cts.Token);

        // Assert
        emailServiceMock.Verify(
            e => e.SendAlertAsync(
                It.Is<string>(s => s.Contains("Compra")),
                It.IsAny<string>()),
            Times.Once);
    }

    // ── Reset de Alerta ──────────────────────────────────────────────────────

    [Fact]
    public async Task AlertReset_PriceNormalizesAndRetriggers_TwoEmailsSent()
    {
        // Arrange
        var quoteServiceMock = new Mock<IQuoteService>();
        var emailServiceMock = new Mock<IEmailService>();
        var cts = new CancellationTokenSource();
        var callCount = 0;

        // Tick 1: buy zone  → alerta enviado, buyAlertSent = true
        // Tick 2: zona normal → flags resetadas (buyAlertSent = false)
        // Tick 3: buy zone  → alerta enviado novamente
        quoteServiceMock
            .Setup(q => q.GetQuoteAsync(It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount >= 3)
                    cts.Cancel();
                return Task.FromResult(callCount switch
                {
                    1 => 8m,   // buy zone
                    2 => 15m,  // zona normal — reseta flags
                    _ => 8m    // buy zone novamente
                });
            });

        emailServiceMock
            .Setup(e => e.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(quoteServiceMock, emailServiceMock);

        // Act
        await sut.StartMonitoringAsync(CreateDto(), cts.Token);

        // Assert
        emailServiceMock.Verify(
            e => e.SendAlertAsync(
                It.Is<string>(s => s.Contains("Compra")),
                It.IsAny<string>()),
            Times.Exactly(2));
    }

    // ── Resiliência ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Resilience_QuoteServiceThrowsException_LoopContinues()
    {
        // Arrange
        // Nota: este teste demora ~1 segundo devido ao Task.Delay no bloco catch.
        var quoteServiceMock = new Mock<IQuoteService>();
        var emailServiceMock = new Mock<IEmailService>();
        var cts = new CancellationTokenSource();
        var callCount = 0;

        // Call 1: lança exceção genérica → catch(Exception) → loop não crasha
        // Call 2: retorna preço na buy zone → alerta enviado → loop encerrado
        quoteServiceMock
            .Setup(q => q.GetQuoteAsync(It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    return Task.FromException<decimal>(new Exception("Erro de rede"));
                cts.Cancel();
                return Task.FromResult(8m);
            });

        emailServiceMock
            .Setup(e => e.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(quoteServiceMock, emailServiceMock);

        // Act
        await sut.StartMonitoringAsync(CreateDto(), cts.Token);

        // Assert — o loop não crashou; o alerta foi enviado na iteração seguinte
        emailServiceMock.Verify(
            e => e.SendAlertAsync(
                It.Is<string>(s => s.Contains("Compra")),
                It.IsAny<string>()),
            Times.Once);
    }
}
