using FirebaseAdmin.Messaging;

namespace Tabibi.API.Services
{
    public sealed class FcmNotificationService
        (ILogger<FcmNotificationService> logger) : IFcmNotificationService
    {
        public async Task<bool> SendPushNotificationAsync(
            string fcmToken,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                return false;
            }

            try
            {
                Message message = new()
                {
                    Token = fcmToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? []
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                logger.LogInformation($"Successfully sent message: {response}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending FCM notification");
                return false;
            }
        }
    }
}
