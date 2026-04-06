using System;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using StockQuoteAlert.Application.Interfaces;

namespace StockQuoteAlert.Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _enableSsl;
        private readonly string _recipientEmail;

        public EmailService(
            string smtpServer,
            int smtpPort,
            string username,
            string password,
            bool enableSsl,
            string recipientEmail)
        {
            _smtpServer = smtpServer ?? throw new ArgumentNullException(nameof(smtpServer));
            _smtpPort = smtpPort > 0 ? smtpPort : throw new ArgumentException("Porta SMTP inválida.", nameof(smtpPort));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _enableSsl = enableSsl;
            _recipientEmail = recipientEmail ?? throw new ArgumentNullException(nameof(recipientEmail));
        }

        public async Task ValidateSmtpConfigurationAsync()
        {
            await ExecuteWithSmtpClientAsync(_ => Task.CompletedTask);
        }

        public async Task SendAlertAsync(string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("O assunto não pode ser vazio.", nameof(subject));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("O corpo do e-mail não pode ser vazio.", nameof(body));

            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_username));
                message.To.Add(MailboxAddress.Parse(_recipientEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                await ExecuteWithSmtpClientAsync(client => client.SendAsync(message));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao enviar e-mail: {ex.Message}", ex);
            }
        }

        private async Task ExecuteWithSmtpClientAsync(Func<SmtpClient, Task> action)
        {
            using var client = new SmtpClient();
            var secureSocketOptions = _enableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_smtpServer, _smtpPort, secureSocketOptions);
            await client.AuthenticateAsync(_username, _password);
            await action(client);
            await client.DisconnectAsync(true);
        }
    }
}