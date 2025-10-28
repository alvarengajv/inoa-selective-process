using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Threading;
using StockQuoteAlert.Domain.Interfaces;

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

        public async Task SendAlertAsync(string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("O assunto não pode ser vazio.", nameof(subject));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("O corpo do e-mail não pode ser vazio.", nameof(body));

            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_username),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(_recipientEmail);

                using var smtpClient = new SmtpClient(_smtpServer)
                {
                    Port = _smtpPort,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_username, _password),
                    EnableSsl = _enableSsl
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao enviar e-mail: {ex.Message}", ex);
            }
        }
    }
}