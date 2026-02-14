using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Tabibi.API.Services
{
    public static class ZegoTokenGenerator
    {
        public static string GenerateToken(
            long appId,
            string serverSecret,
            string userId,
            string roomId,
            long effectiveTimeInSeconds)
        {
            var createTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expireTime = createTime + effectiveTimeInSeconds;

            // 1. Create the inner payload specifically for the Room
            // 1 = Login Privilege, 2 = Publish Privilege. Setting both to 1 (enabled).
            var payloadData = new
            {
                room_id = roomId,
                privilege = new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 1 }
                },
                stream_id_list = new string[] { }
            };

            // 2. The payload field in Zego tokens MUST be a JSON string
            string payloadJson = JsonConvert.SerializeObject(payloadData);

            // 3. Construct the main Token Info object
            var nonce = new Random().NextInt64();
            var tokenInfo = new
            {
                app_id = appId,
                user_id = userId,
                nonce = nonce,
                ctime = createTime,
                expire = expireTime,
                payload = payloadJson // roomId is now included here!
            };

            string plainText = JsonConvert.SerializeObject(tokenInfo);

            // 4. Handle Secret (Production Check)
            // Zego secrets are 32-char Hex strings. We need exactly 32 bytes.
            var secretBytes = Encoding.UTF8.GetBytes(serverSecret);
            if (secretBytes.Length != 32)
            {
                throw new InvalidOperationException("Zego Server Secret must be exactly 32 characters/bytes.");
            }

            // 5. Encryption (AES-128-CBC)
            var iv = new byte[16];
            RandomNumberGenerator.Fill(iv); // More secure than 'new Random()' for production

            using var aes = Aes.Create();
            aes.Key = secretBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // 6. Construct Final Byte Array: [IV (16) + Len (2) + Content]
            var finalBytes = new byte[16 + 2 + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, finalBytes, 0, 16);

            // Content Length (Big Endian)
            finalBytes[16] = (byte)(encryptedBytes.Length >> 8);
            finalBytes[17] = (byte)(encryptedBytes.Length);

            Buffer.BlockCopy(encryptedBytes, 0, finalBytes, 18, encryptedBytes.Length);

            // 7. Return with Version Prefix "04"
            return "04" + Convert.ToBase64String(finalBytes);
        }
    }
}