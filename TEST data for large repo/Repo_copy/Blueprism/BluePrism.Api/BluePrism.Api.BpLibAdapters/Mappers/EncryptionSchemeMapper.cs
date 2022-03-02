namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using AutomateAppCore;
    using Domain;

    public static class EncryptionSchemeMapper
    {
        public static EncryptionScheme ToDomain(this clsEncryptionScheme @this) =>
            new EncryptionScheme
            {
                Name = @this.Name,
                KeyLocation = (Domain.EncryptionKeyLocation)@this.KeyLocation,
                Algorithm = (Domain.EncryptionAlgorithm)@this.Algorithm,
                IsAvailable = @this.IsAvailable,
            };
    }
}
