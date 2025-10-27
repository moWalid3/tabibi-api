namespace Tabibi.EmailService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
