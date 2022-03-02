using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SkillPackager.Cryptography
{
    /// This class is not going to be used in this application, it will be used in the main Blue Prism application.
    /// Once the import .bpskill functionality has been added to the main application it will then be moved.
    internal static class Decryptor
    {
        private const int KeySize = 128;
        private const int IvSize = 16;

        private const string SymmetricKey = "n7b2oZn4WT4tRHunsoklIA==";
        private const string PublicKey = "<RSAKeyValue><Modulus>wg15NzTPf7kEcxJA0wu3SqR0Iq+eVWcCNh3e87/+Y0wEPapPFzc8n9kbxgkRjcd1AoqWBuD17R7DWys0ebF5rn/M4vxPx76/O7PM3F7XZwU0LOAjLAr6/glmZ4awfcFAuii9ZhpKOcHWQfA8ws6xSb8ueA+6/vJQ97wISfxyZ6E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private static string DecryptAndVerify(string encryptedStuff)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedStuff);
            var unencryptedBytes = DecryptSymmetrically(encryptedBytes, Convert.FromBase64String(SymmetricKey));
            var signature = unencryptedBytes.Skip(unencryptedBytes.Length - KeySize).ToArray();
            var content = unencryptedBytes.Take(unencryptedBytes.Length - KeySize).ToArray();

            Verify(signature, content, PublicKey);

            return Encoding.UTF8.GetString(content);
        }

        private static void Verify(byte[] signature, byte[] content, string publicKey)
        {
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            using (var shaCryptoServiceProvider = new SHA256CryptoServiceProvider())
            {
                rsaCryptoServiceProvider.FromXmlString(publicKey);
                if (!rsaCryptoServiceProvider.VerifyData(content, shaCryptoServiceProvider, signature))
                    throw new CryptographicException("Unable to verify signature.");
            }
        }

        private static byte[] DecryptSymmetrically(byte[] stuff, byte[] key)
        {
            var ivStringBytes = stuff.Take(IvSize).ToArray();
            var cipherTextBytes = stuff.Skip(IvSize).ToArray();

            using (var symmetricKey = new RijndaelManaged()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            using (var decryptor = symmetricKey.CreateDecryptor(key, ivStringBytes))
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                return plainTextBytes.Take(decryptedByteCount).ToArray();
            }
        }
    }
}
