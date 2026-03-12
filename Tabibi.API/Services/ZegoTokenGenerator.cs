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
                stream_id_list = Array.Empty<string>()
            };

            // 2. The payload field in Zego tokens MUST be a JSON string
            string payloadJson = JsonConvert.SerializeObject(payloadData);

            // 3. Construct the main Token Info object
            var nonce = new Random().Next();
            var tokenInfo = new
            {
                app_id = appId,
                user_id = userId,
                nonce = nonce,
                ctime = createTime,
                expire = effectiveTimeInSeconds,
                payload = payloadJson // roomId is now included here!
            };

            string plainText = JsonConvert.SerializeObject(tokenInfo);

            // 4. Handle Secret (Production Check)
            var secretBytes = Encoding.UTF8.GetBytes(serverSecret);
            if (secretBytes.Length != 32)
            {
                throw new InvalidOperationException("Zego Server Secret must be exactly 32 characters/bytes.");
            }

            // 5. Encryption (AES-256-CBC based on 32-byte secret)
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

            // =========================================================================
            // 6. THE FIX: Construct Final Byte Array (Official Zego Token04 Format)
            // Structure: ExpireTime (8) + IV Length (2) + IV (16) + Crypto Length (2) + Crypto Data
            // =========================================================================
            var finalBytes = new byte[8 + 2 + 16 + 2 + encryptedBytes.Length];
            int offset = 0;

            // Expire Time (8 bytes, Big Endian)
            var expireBytes = BitConverter.GetBytes((long)expireTime);
            if (BitConverter.IsLittleEndian) Array.Reverse(expireBytes); // Ensure Big Endian
            Buffer.BlockCopy(expireBytes, 0, finalBytes, offset, 8);
            offset += 8;

            // IV Length (2 bytes, Big Endian) - Value is always 16
            finalBytes[offset++] = 0;
            finalBytes[offset++] = 16;

            // IV Data (16 bytes)
            Buffer.BlockCopy(iv, 0, finalBytes, offset, 16);
            offset += 16;

            // Encrypted Data Length (2 bytes, Big Endian)
            finalBytes[offset++] = (byte)(encryptedBytes.Length >> 8);
            finalBytes[offset++] = (byte)(encryptedBytes.Length);

            // Encrypted Data Bytes
            Buffer.BlockCopy(encryptedBytes, 0, finalBytes, offset, encryptedBytes.Length);

            // 7. Return with Version Prefix "04"
            return "04" + Convert.ToBase64String(finalBytes);
        }
    }
}