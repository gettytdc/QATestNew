try {
    if(!CryptoJS) var CryptoJS = require("../crypto-js");
}
catch (e) { /* Used for unit testing purposes */ }

class CryptographyProvider {
    constructor(hashAlgorithm, symmetricAlgorithm, ivGenerator) {
        this.hashAlgorithm = hashAlgorithm;
        this.symmetricAlgorithm = symmetricAlgorithm;
        this.ivGenerator = ivGenerator;
    }

    encrypt(data, key) {
        const ivSizeInBytes = 128 / 8;
        const iv = this.ivGenerator(ivSizeInBytes);

        const encryptedMessage = this.symmetricAlgorithm.encrypt(
            data,
            this.getHashedKey(key),
            this.getCryptographySettings(iv));
        const messageAndIv = [...encryptedMessage.ciphertext.words, ...iv.words];

        return CryptoJS.enc.Base64.stringify(new CryptoJS.lib.WordArray.init(messageAndIv));
    }

    decrypt(data, key) {
        var rawData = atob(data);
        var encryptedText = rawData.substring(0, rawData.length - 16);
        var iv = rawData.substring(rawData.length-16);

        var plaintextArray = this.symmetricAlgorithm.decrypt(
            { ciphertext: CryptoJS.enc.Latin1.parse(encryptedText) },
            this.getHashedKey(key),
            this.getCryptographySettings(CryptoJS.enc.Latin1.parse(iv)));

        return CryptoJS.enc.Utf8.stringify(plaintextArray);
    }

    getHashedKey(key) {
        return this.hashAlgorithm.computeHash(key);
    }

    getCryptographySettings(iv) {
        return {
            iv: iv,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        };
    }
}

try {
    module.exports = CryptographyProvider;
}
catch (e) { /* Used for unit testing purposes */ }
