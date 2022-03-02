class SymmetricAlgorithm {
    constructor(algorithm) {
        this.algorithm = algorithm;
    }

    encrypt(data, key, settings) {
        return this.algorithm.encrypt(data, key, settings);
    }

    decrypt(data, key, settings) {
        return this.algorithm.decrypt(data, key, settings);
    }
}

try {
    module.exports = SymmetricAlgorithm;
}
catch (e) { /* Used for unit testing purposes */ }