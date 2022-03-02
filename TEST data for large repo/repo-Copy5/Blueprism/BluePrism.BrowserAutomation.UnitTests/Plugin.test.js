const CryptographyProvider = require("../BluePrism.BrowserAutomation/Plugin/Cryptography/CryptographyProvider.js");
const CryptoJS = require("../BluePrism.BrowserAutomation/Plugin/crypto-js");
const HashAlgorithm = require("../BluePrism.BrowserAutomation/Plugin/Cryptography/HashAlgorithm.js");
const SymmetricAlgorithm = require("../BluePrism.BrowserAutomation/Plugin/Cryptography/SymmetricAlgorithm.js");

test('decrypt When given encrypted string then returns decrypted value.',
    () => {
        const classUnderTest = new CryptographyProvider(
            new HashAlgorithm(CryptoJS.SHA256),
            new SymmetricAlgorithm(CryptoJS.AES),
            CryptoJS.lib.WordArray.random);

        const testEncryptedMessage = "HriSogEMBFaubQNMH4F2EJHuC4HZ5zjOtMr8gI2UYlU=";
        const testDecryptedMessage = "This is a test";
        const testKey = "Test";

        const result = classUnderTest.decrypt(testEncryptedMessage, testKey);

        expect(result).toBe(testDecryptedMessage);
    });

test('encrypt When given normal string then returns correct value.',
    () => {
        const classUnderTest = new CryptographyProvider(
            new HashAlgorithm(CryptoJS.SHA256),
            new SymmetricAlgorithm(CryptoJS.AES),
            (c) => new CryptoJS.enc.Hex.parse("91EE0B81D9E738CEB4CAFC808D946255"));

        const testMessage = "This is a test";
        const testKey = "Test";
        const testEncryptedMessage = "HriSogEMBFaubQNMH4F2EJHuC4HZ5zjOtMr8gI2UYlU=";

        const result = classUnderTest.encrypt(testMessage, testKey);

        expect(result).toBe(testEncryptedMessage);
    });