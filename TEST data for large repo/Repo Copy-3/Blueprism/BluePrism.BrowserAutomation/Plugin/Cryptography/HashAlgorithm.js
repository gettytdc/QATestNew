class HashAlgorithm {
    constructor(hashFunction) {
        this.hashFunction = hashFunction;
    }

    computeHash(data) {
        return this.hashFunction(data);
    }
}

try {
    module.exports = HashAlgorithm;
}
catch (e) { /* Used for unit testing purposes */ }