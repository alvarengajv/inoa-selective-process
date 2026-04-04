using FluentAssertions;
using StockQuoteAlert.Infrastructure.ExternalServices;
using Xunit;

namespace StockQuoteAlert.Tests.Services;

public class EmailServiceTests
{
    private static EmailService CreateSut() =>
        new(
            smtpServer: "localhost",
            smtpPort: 1025,
            username: "test@localhost",
            password: "test",
            enableSsl: false,
            recipientEmail: "recipient@localhost");

    // ── Construtor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullSmtpServer_ThrowsArgumentNullException()
    {
        var act = () => new EmailService(null!, 1025, "user", "pass", false, "to@test.com");
        act.Should().Throw<ArgumentNullException>().WithParameterName("smtpServer");
    }

    [Fact]
    public void Constructor_InvalidPort_ThrowsArgumentException()
    {
        var act = () => new EmailService("localhost", 0, "user", "pass", false, "to@test.com");
        act.Should().Throw<ArgumentException>().WithParameterName("smtpPort");
    }

    [Fact]
    public void Constructor_NullUsername_ThrowsArgumentNullException()
    {
        var act = () => new EmailService("localhost", 1025, null!, "pass", false, "to@test.com");
        act.Should().Throw<ArgumentNullException>().WithParameterName("username");
    }

    [Fact]
    public void Constructor_NullPassword_ThrowsArgumentNullException()
    {
        var act = () => new EmailService("localhost", 1025, "user", null!, false, "to@test.com");
        act.Should().Throw<ArgumentNullException>().WithParameterName("password");
    }

    [Fact]
    public void Constructor_NullRecipientEmail_ThrowsArgumentNullException()
    {
        var act = () => new EmailService("localhost", 1025, "user", "pass", false, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("recipientEmail");
    }

    // ── SendAlertAsync – Validação de argumentos ─────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendAlertAsync_EmptySubject_ThrowsArgumentException(string? subject)
    {
        var sut = CreateSut();
        var act = () => sut.SendAlertAsync(subject!, "corpo válido");
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("subject");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendAlertAsync_EmptyBody_ThrowsArgumentException(string? body)
    {
        var sut = CreateSut();
        var act = () => sut.SendAlertAsync("assunto válido", body!);
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("body");
    }

    // ── SendAlertAsync – Falha de conexão SMTP ──────────────────────────────

    [Fact]
    public async Task SendAlertAsync_NoSmtpServer_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var act = () => sut.SendAlertAsync("Alerta", "Corpo do alerta");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro ao enviar e-mail:*");
    }

    // ── ValidateSmtpConfigurationAsync – Falha de conexão ───────────────────

    [Fact]
    public async Task ValidateSmtpConfigurationAsync_NoSmtpServer_Throws()
    {
        var sut = CreateSut();
        var act = () => sut.ValidateSmtpConfigurationAsync();
        await act.Should().ThrowAsync<Exception>();
    }
}
