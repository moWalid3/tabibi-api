using MailKit.Net.Smtp;
using MimeKit;

namespace Tabibi.EmailService
{
    public class EmailSender(EmailConfiguration emailConfig) : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig = emailConfig;

        public async Task SendEmailAsync(Message message)
        {
            var mailMessage = CreateEmailMessage(message);
            await SendAsync(mailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(MailboxAddress.Parse(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            BodyBuilder bodyBuilder = new()
            {
                HtmlBody = $@"
                    <body style='font-family: Arial, sans-serif; margin: 15px 0; padding: 15px 0; background-color: #f4f4f7;'>
                        <div style='width: 100%; max-width: 580px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); overflow: hidden; border: 1px solid #e0e0e0;'>
        
                            <!-- Header -->
                            <div style='background-color: #007bff; color: #ffffff; padding: 25px 35px; text-align: center;'>
                                <h1 style='margin: 0; font-size: 28px; font-weight: 600;'>Tabibi</h1>
                                <p style='margin: 5px 0 0; font-size: 16px; opacity: 0.9;'>Your Health Companion</p>
                            </div>
        
                            <!-- Content -->
                            <div style='padding: 35px 40px; color: #333333; text-align: center;'>
            
                                <!-- Title from Subject -->
                                <h2 style='margin-top: 0; margin-bottom: 20px; font-size: 22px; font-weight: 600;'>{message.Subject}</h2>
            
                                <!-- [MODIFIED] The entire message.Content is injected here as HTML -->
                                {message.Content}

                                <p style='font-size: 14px; color: #777777; margin-top: 20px;'>
                                    If you did not request this, you can safely ignore this email.
                                </p>
            
                            </div>
        
                            <!-- Footer -->
                            <div style='background-color: #f9f9f9; color: #888888; padding: 25px 35px; text-align: center; border-top: 1px solid #eeeeee;'>
                                <p style='margin: 0; font-size: 13px;'>© {DateTime.UtcNow.Year} Tabibi. All rights reserved.</p>
                                <p style='margin: 5px 0 0; font-size: 13px;'>Find your doctor today.</p>
                            </div>
                        </div>
                    </body>
                    "
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                await client.SendAsync(mailMessage);
            }
            catch
            {
                //log an error message or throw an exception, or both.
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}
