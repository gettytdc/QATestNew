using BluePrism.BPCoreLib;
using BluePrism.Core.Encryption;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests
{
    public class DatabaseDataEncryptionTests
    {
        [Test]
        public void EncryptDecrypt_RowIdentifierAndDataAsExpected()
        {
            string rowidentifier = "123";
            string data = "testdata";
            var encrypted = DatabaseDataEncryption.EncryptWithRowIdentifier(rowidentifier, data);

            var decrypted = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(rowidentifier, encrypted);

            Assert.AreEqual(data, decrypted);
        }

        [Test]
        public void EncryptDecrypt_RowIdentiferDoesntMatch_ThrowsException()
        {
            string rowidentifier = "123";
            string data = "testdata";
            var encrypted = DatabaseDataEncryption.EncryptWithRowIdentifier(rowidentifier, data);

            Assert.Throws(typeof(BluePrismException), () => DatabaseDataEncryption.DecryptAndVerifRowIdentifier("456", encrypted));
        }


    }
}
