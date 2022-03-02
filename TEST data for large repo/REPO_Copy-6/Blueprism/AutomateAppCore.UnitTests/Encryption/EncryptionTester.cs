using BluePrism.AutomateAppCore;
using BluePrism.Common.Security;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Encryption
{

    /// <summary>
    /// Performs some basic encryption/decryption tests with using the various algorthims
    /// currently supported.
    /// </summary>
    [TestFixture]
    public class EncryptionTester
    {
        #region 'Encryption Keys'
        // A TripleDES encryption key
        private static readonly SafeString tripleDESKey = new SafeString("vxFWKhALiTFeqVBag9o4uYP5UWC/eR4n");

        // An AES Rijndael encryption key
        private static readonly SafeString aesRijndaelKey = new SafeString("iygnimqoKTFy8ULDesK7aOLd5Ovo019+88hbsaNgfOI=");

        // An AES Crypto encryption key
        private static readonly SafeString aesCryptoKey = new SafeString("YLh6i4XtkKVd3K6vboAW3WBpOt4movmsewr/ofl0D6Q=");

        // An example key in external file format with legacy obfuscation
        private const string legacyFileFormatKey = "2:3,65,50,118,88,27,39,28,24,27,18,124,85,85,26,52,115,25,3,91,21,67,38,48,13,0,62,83,10,0,27,80,23,36,54,95,32,90,4,85,118,47,23,13";

        // An example key in external file format with cipher obfuscation
        private const string cipherFileFormatKey = "3:aCpGKl8qTyp6KlgqQypZKkcqBCppKkUqWCpPKgQqeSpPKkkqXypYKk8qBCppKkMqWipCKk8qWCplKkgqTCpfKlkqSSpLKl4qRSpYKg==:cRknSQZbOAETQxB6C1IZUAleP1M3DVswCS8hVV9DJCstDTFKP3EXIyccQA0=";

        // An example key in external file format with encrypting obfuscation
        private const string encryptedFileFormatKey = "2:aCpGKl8qTyp6KlgqQypZKkcqBCppKkUqWCpPKgQqeSpPKkkqXypYKk8qBCpvKkQqSSpYKlMqWipeKkMqRCpNKmUqSCpMKl8qWSpJKksqXipFKlgq:NLfc8PahETHq+EMVuTSGlg==:6UGVtx2cnGfm1B5cLg3eO++6p7uPmnWo/CL3I9jKD2LnNw0j3H5vLk/+5Lnc9/s5";

        #endregion  
        /// <summary>
        /// Tests for encryption and decryption of data
        /// </summary>
        [Test]
        public void TestEncryptDecrypt()
        {
            // Create some encryption schemes of different types
            var scheme3D = new clsEncryptionScheme("TripleDES")
            {
                Algorithm = EncryptionAlgorithm.TripleDES,
                Key = tripleDESKey
            };
            var schemeRijndael = new clsEncryptionScheme("Rijndael")
            {
                Algorithm = EncryptionAlgorithm.Rijndael256,
                Key = aesRijndaelKey
            };
            var schemeAES = new clsEncryptionScheme("AES256")
            {
                Algorithm = EncryptionAlgorithm.AES256,
                Key = aesCryptoKey
            };

            // Check that data can be encrypted/decrypted with the 3DES algorithm...
            var cipher = scheme3D.Encrypt("HelloTripleDES");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(scheme3D.Decrypt(cipher), Is.EqualTo("HelloTripleDES"));
            // ...including empty strings
            cipher = scheme3D.Encrypt("");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(scheme3D.Decrypt(cipher), Is.Empty);

            // Check that data can be encrypted/decrypted with the Rijndael algorithm...
            cipher = schemeRijndael.Encrypt("HelloRijndael");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(schemeRijndael.Decrypt(cipher), Is.EqualTo("HelloRijndael"));
            // ...including empty strings
            cipher = schemeRijndael.Encrypt("");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(schemeRijndael.Decrypt(cipher), Is.Empty);

            // Check that data can be encrypted/decrypted with the AES algorithm...
            cipher = schemeAES.Encrypt("HelloAES256");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(schemeAES.Decrypt(cipher), Is.EqualTo("HelloAES256"));
            // ...including empty strings
            cipher = schemeAES.Encrypt("");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(schemeAES.Decrypt(cipher), Is.Empty);

            // Check that data encrypted with one scheme fails to be decrypted with another
            string plainText = "Some text to encrypt";
            cipher = scheme3D.Encrypt(plainText);
            try
            {
                Assert.That(schemeRijndael.Decrypt(cipher), Is.Not.EqualTo(plainText));
            }
            catch (OperationFailedException)
            {
            } // Correct Behaviour

            cipher = schemeRijndael.Encrypt(plainText);
            try
            {
                Assert.That(schemeAES.Decrypt(cipher), Is.Not.EqualTo(plainText));
            }
            catch (OperationFailedException)
            {
            } // Correct Behaviour

            cipher = schemeAES.Encrypt(plainText);
            try
            {
                Assert.That(scheme3D.Decrypt(cipher), Is.Not.EqualTo(plainText));
            }
            catch (OperationFailedException)
            {
            } // Correct Behaviour

            // Check that decryption fails if data is in wrong format
            // (Note that the same code is used for all algorithms)
            Assert.That(() => schemeAES.Decrypt("asdasdfsafdsfs8787wef23"), Throws.InstanceOf<InvalidEncryptedDataException>());

            // Check that encryption/decryption fails when invalid key lengths are used
            // (Note that Rijndael & AES have the same key length)
            scheme3D.Key = aesRijndaelKey;
            Assert.That(() => scheme3D.Encrypt("More data to encrypt"), Throws.InstanceOf<InvalidValueException>());
            Assert.That(() => scheme3D.Decrypt(cipher), Throws.InstanceOf<InvalidValueException>());
            schemeAES.Key = tripleDESKey;
            Assert.That(() => schemeAES.Encrypt("More data to encrypt"), Throws.InstanceOf<InvalidValueException>());
            Assert.That(() => schemeAES.Decrypt(cipher), Throws.InstanceOf<InvalidValueException>());

            // Check that encryption/decryption fails with an invalid algorithm
            scheme3D.Algorithm = default(EncryptionAlgorithm);
            Assert.That(() => scheme3D.Encrypt("More data to encrypt"), Throws.InstanceOf<InvalidValueException>());
            Assert.That(() => scheme3D.Decrypt(cipher), Throws.InstanceOf<InvalidValueException>());
        }

        [Test]
        public void TestEncryptDecryptWithMultiByteChars()
        {
            // Create some encryption schemes of different types
            var schemeAES = new clsEncryptionScheme("AES256")
            {
                Algorithm = EncryptionAlgorithm.AES256,
                Key = aesCryptoKey
            };

            string testInput = "🙃 ⊕ ⊖ ⊗ ⊝ ば ぶ ぺ " + System.Environment.NewLine + " 〄 ° ઊ હ ஞ ଲ ⊙ ﹏ 🕴";

            // Check that data can be encrypted/ decrypted with the AES algorithm...
            var cipher = schemeAES.Encrypt(testInput);
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            var s = schemeAES.Decrypt(cipher);
            Assert.AreEqual(testInput, s, "Check decrypt gives us the same string back");
            var ss = schemeAES.DecryptToSafeString(cipher);
            Assert.AreEqual(testInput, ss.AsString(), "Test decrypt to safe string gives us the same string back");

            // ...including empty strings
            cipher = schemeAES.Encrypt("");
            Assert.That(clsEncryptionScheme.DefaultDecrypterRegex.IsMatch(cipher), Is.True);
            Assert.That(schemeAES.Decrypt(cipher), Is.Empty);
        }
        /// <summary>
        /// Tests for key validity
        /// </summary>
        [Test]
        public void TestKeyValidity()
        {
            // Check that appropriate keys are created...
            var sch = new clsEncryptionScheme("SchemeA");
            sch.Algorithm = EncryptionAlgorithm.TripleDES;
            sch.GenerateKey();
            Assert.That(sch.HasValidKey, Is.True);
            // ...and the generated key encrypts/decrypts ok
            Assert.That(sch.Decrypt(sch.Encrypt("HelloWorld")), Is.EqualTo("HelloWorld"));

            // Repeat with Rijndael
            sch.Algorithm = EncryptionAlgorithm.Rijndael256;
            sch.GenerateKey();
            Assert.That(sch.HasValidKey, Is.True);
            Assert.That(sch.Decrypt(sch.Encrypt("HelloWorld")), Is.EqualTo("HelloWorld"));

            // And again with AES
            sch.Algorithm = EncryptionAlgorithm.AES256;
            sch.GenerateKey();
            Assert.That(sch.HasValidKey, Is.True);
            Assert.That(sch.Decrypt(sch.Encrypt("HelloWorld")), Is.EqualTo("HelloWorld"));

            // Also check that invalid keys are detected
            sch.Key = new SafeString("abc");
            Assert.That(sch.HasValidKey, Is.False);
        }

        /// <summary>
        /// Test that keys held in external file format can be read correctly, whether
        /// held in one of the legacy obfuscation formats or the current one (encrypted).
        /// </summary>
        [Test]
        public void TestEncryptionKeyFileFormats()
        {
            // Legacy obfuscated format
            var scheme = new clsEncryptionScheme("LegacyObfuscatedKey");
            scheme.FromExternalFileFormat(legacyFileFormatKey);
            Assert.That(scheme.HasValidKey);

            // Cipher obfuscated format
            scheme = new clsEncryptionScheme("CipherObfuscatedKey");
            scheme.FromExternalFileFormat(cipherFileFormatKey);
            Assert.That(scheme.HasValidKey);

            // Encrypted obfuscated format
            scheme = new clsEncryptionScheme("EncryptingObfuscatedKey");
            scheme.FromExternalFileFormat(encryptedFileFormatKey);
            Assert.That(scheme.HasValidKey);
        }
    }
}
