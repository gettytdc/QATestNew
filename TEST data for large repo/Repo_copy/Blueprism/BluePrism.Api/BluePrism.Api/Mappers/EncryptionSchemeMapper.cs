namespace BluePrism.Api.Mappers
{
    using System;

    public static class EncryptionSchemeMapper
    {
        public static Models.EncryptionSchemeModel ToModelObject(this Domain.EncryptionScheme encryptionScheme) =>
            new Models.EncryptionSchemeModel
            {
                Name = encryptionScheme.Name,
                Algorithm = encryptionScheme.Algorithm.ToModel(),
                KeyLocation = encryptionScheme.KeyLocation.ToModel(),
                IsAvailable = encryptionScheme.IsAvailable,
            };

        public static Models.EncryptionKeyLocation ToModel(this Domain.EncryptionKeyLocation encryptionKeyLocation)
        {
            switch (encryptionKeyLocation)
            {
                case Domain.EncryptionKeyLocation.Database:
                    return Models.EncryptionKeyLocation.Database;
                case Domain.EncryptionKeyLocation.Server:
                    return Models.EncryptionKeyLocation.Server;
                default:
                    throw new ArgumentException("Unexpected encryption key location type", nameof(encryptionKeyLocation));
            }
        }

        public static Models.EncryptionAlgorithm ToModel(this Domain.EncryptionAlgorithm encryptionAlgorithm)
        {
            switch (encryptionAlgorithm)
            {
                case Domain.EncryptionAlgorithm.None:
                    return Models.EncryptionAlgorithm.None;
                case Domain.EncryptionAlgorithm.AES256:
                    return Models.EncryptionAlgorithm.AES256;
                case Domain.EncryptionAlgorithm.Rijndael256:
                    return Models.EncryptionAlgorithm.Rijndael256;
                case Domain.EncryptionAlgorithm.TripleDES:
                    return Models.EncryptionAlgorithm.TripleDES;
                default:
                    throw new ArgumentException("Unexpected encryption algorithm type", nameof(encryptionAlgorithm));
            }
        }
    }
}
