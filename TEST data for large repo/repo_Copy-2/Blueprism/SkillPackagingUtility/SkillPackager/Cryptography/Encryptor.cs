using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SkillPackager.Cryptography
{
    internal static class Encryptor
    {
        private const int ivSize = 16;
        private const string symmetricKey = "n7b2oZn4WT4tRHunsoklIA==";
        private const string privateKey = "<RSAKeyValue><Modulus>wg15NzTPf7kEcxJA0wu3SqR0Iq+eVWcCNh3e87/+Y0wEPapPFzc8n9kbxgkRjcd1AoqWBuD17R7DWys0ebF5rn/M4vxPx76/O7PM3F7XZwU0LOAjLAr6/glmZ4awfcFAuii9ZhpKOcHWQfA8ws6xSb8ueA+6/vJQ97wISfxyZ6E=</Modulus><Exponent>AQAB</Exponent><P>7KyLesVc8D0UKjysR9UOV3d9nUGk0hGWpzgIn6bf/Oc9EDCkj7m7esL8zF8Qu4ps5NxA1tGfbWdgi+1TAYyzXw==</P><Q>0eX3si3EHUBJ41DpncNgUAOj7T3ZHUcx0UvVFkGmlIes/ShMqXPbKNBwAnlUBZhR+08T5v4V6Wpy9oqRmmnE/w==</Q><DP>F6o1FCSR42+oCYUhkNkr4vEOvV+n9F1P3A6NRjFwaiBRCcJjYf+nUGIY1vKWgLoZo1SmoxQ4xb61d6hWSWxhLw==</DP><DQ>RjrT7eIyRDdGgbCI+ihtCViueKrBAnLX0Fe3LIM64WekEfBx9iC1q6pSUAGYg2a7x4Jl/lv1qOvdG3Cx1yCcsQ==</DQ><InverseQ>kBis5SpxrY2PynJedysqkDdJHiNx4ah1tUaKrmYpCB8Z2FAKhi1T4pUqem5afZVbyPtSZ7uwiTT6N6qf+xMgYw==</InverseQ><D>VEKXdMJIq1QCO5kcbsdykkWwa/NH1BuDWU5FJiAi7KjwWnpeF12TxHD4X1hGuAJ4q17T4C7Eem7WMvsisQjAJaRNnZajy3nCeOUcKpSxJBVGKUbJaOt01r+PrnnqqkmoWC3cw+ePkMCuEm+KblCCqYhGi4qhmPedEHSyX2KyzeU=</D></RSAKeyValue>";


        internal static string Encrypt(string text)
        {
            var encryptedText = EncryptSymmetrically(text);
            return Convert.ToBase64String(encryptedText);
        }

        private static byte[] EncryptSymmetrically(string plainText)
        {
            var ivBytes = GenerateRandomByteArray(ivSize);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var signedBytes = SignText(plainTextBytes, privateKey);

            using (var encryptor = GenerateEncryptor(Convert.FromBase64String(symmetricKey), ivBytes))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                var combinedBytes = plainTextBytes.Concat(signedBytes).ToArray();
                cryptoStream.Write(combinedBytes, 0, combinedBytes.Length);
                cryptoStream.FlushFinalBlock();
                return ivBytes.Concat(memoryStream.ToArray()).ToArray();
            }
        }

        private static byte[] SignText(byte[] plainTextBytes, string privateKey)
        {
            using (var cryptoServiceProvider = new RSACryptoServiceProvider())
            using (var hashAlgprithmProvider = new SHA256CryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(privateKey);
                return cryptoServiceProvider.SignData(plainTextBytes, hashAlgprithmProvider);
            }
        }

        private static ICryptoTransform GenerateEncryptor(byte[] symmetricKey, byte[] ivBytes)
        {
            return new RijndaelManaged()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            }.CreateEncryptor(symmetricKey, ivBytes);
        }

        private static byte[] GenerateRandomByteArray(int size)
        {
            var randomBytes = new byte[size]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
