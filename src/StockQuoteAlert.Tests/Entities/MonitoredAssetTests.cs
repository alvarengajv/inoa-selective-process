using FluentAssertions;
using StockQuoteAlert.Domain.Entities;
using Xunit;

namespace StockQuoteAlert.Tests.Entities;

public class MonitoredAssetTests
{
    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidParameters_ReturnsInstance()
    {
        // Arrange
        const string ticker = "petr4";
        const decimal buyThreshold = 10m;
        const decimal sellThreshold = 20m;

        // Act
        var asset = MonitoredAsset.Create(ticker, buyThreshold, sellThreshold);

        // Assert
        asset.Ticker.Should().Be("PETR4");
        asset.BuyThreshold.Should().Be(buyThreshold);
        asset.SellThreshold.Should().Be(sellThreshold);
        asset.CurrentPrice.Should().Be(0);
    }

    [Fact]
    public void Create_EmptyTicker_ThrowsArgumentException()
    {
        // Arrange / Act
        var act = () => MonitoredAsset.Create("", 10m, 20m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("ticker");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_BlankOrNullTicker_ThrowsArgumentException(string? ticker)
    {
        // Arrange / Act
        var act = () => MonitoredAsset.Create(ticker!, 10m, 20m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("ticker");
    }

    [Fact]
    public void Create_InvertedThresholds_ThrowsArgumentException()
    {
        // Arrange / Act
        var act = () => MonitoredAsset.Create("PETR4", buyThreshold: 20m, sellThreshold: 10m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sellThreshold");
    }

    [Fact]
    public void Create_EqualThresholds_ThrowsArgumentException()
    {
        // Arrange / Act
        var act = () => MonitoredAsset.Create("PETR4", buyThreshold: 15m, sellThreshold: 15m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("sellThreshold");
    }

    // ── UpdatePrice ──────────────────────────────────────────────────────────

    [Fact]
    public void UpdatePrice_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", 10m, 20m);

        // Act
        var act = () => asset.UpdatePrice(-1m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("newPrice");
    }

    [Fact]
    public void UpdatePrice_ZeroPrice_ThrowsArgumentException()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", 10m, 20m);

        // Act
        var act = () => asset.UpdatePrice(0m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("newPrice");
    }

    [Fact]
    public void UpdatePrice_ValidPrice_UpdatesCurrentPrice()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", 10m, 20m);

        // Act
        asset.UpdatePrice(15m);

        // Assert
        asset.CurrentPrice.Should().Be(15m);
    }

    // ── ShouldTriggerBuyAlert ────────────────────────────────────────────────

    [Fact]
    public void ShouldTriggerBuyAlert_PriceZero_ReturnsFalse()
    {
        // Arrange — CurrentPrice == 0 após Create
        var asset = MonitoredAsset.Create("PETR4", 10m, 20m);

        // Act / Assert
        asset.ShouldTriggerBuyAlert().Should().BeFalse();
    }

    [Fact]
    public void ShouldTriggerBuyAlert_PriceAtExactBuyThreshold_ReturnsTrue()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", buyThreshold: 10m, sellThreshold: 20m);
        asset.UpdatePrice(10m);

        // Act / Assert
        asset.ShouldTriggerBuyAlert().Should().BeTrue();
    }

    [Fact]
    public void ShouldTriggerBuyAlert_PriceBelowBuyThreshold_ReturnsTrue()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", buyThreshold: 10m, sellThreshold: 20m);
        asset.UpdatePrice(9m);

        // Act / Assert
        asset.ShouldTriggerBuyAlert().Should().BeTrue();
    }

    [Fact]
    public void ShouldTriggerBuyAlert_PriceAboveBuyThreshold_ReturnsFalse()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", buyThreshold: 10m, sellThreshold: 20m);
        asset.UpdatePrice(11m);

        // Act / Assert
        asset.ShouldTriggerBuyAlert().Should().BeFalse();
    }

    // ── ShouldTriggerSellAlert ───────────────────────────────────────────────

    [Fact]
    public void ShouldTriggerSellAlert_PriceZero_ReturnsFalse()
    {
        // Arrange — CurrentPrice == 0 após Create
        var asset = MonitoredAsset.Create("PETR4", 10m, 20m);

        // Act / Assert
        asset.ShouldTriggerSellAlert().Should().BeFalse();
    }

    [Fact]
    public void ShouldTriggerSellAlert_PriceAtExactSellThreshold_ReturnsTrue()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", buyThreshold: 10m, sellThreshold: 20m);
        asset.UpdatePrice(20m);

        // Act / Assert
        asset.ShouldTriggerSellAlert().Should().BeTrue();
    }

    [Fact]
    public void ShouldTriggerSellAlert_PriceAboveSellThreshold_ReturnsTrue()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", buyThreshold: 10m, sellThreshold: 20m);
        asset.UpdatePrice(21m);

        // Act / Assert
        asset.ShouldTriggerSellAlert().Should().BeTrue();
    }

    [Fact]
    public void ShouldTriggerSellAlert_PriceBelowSellThreshold_ReturnsFalse()
    {
        // Arrange
        var asset = MonitoredAsset.Create("PETR4", buyThreshold: 10m, sellThreshold: 20m);
        asset.UpdatePrice(19m);

        // Act / Assert
        asset.ShouldTriggerSellAlert().Should().BeFalse();
    }
}
