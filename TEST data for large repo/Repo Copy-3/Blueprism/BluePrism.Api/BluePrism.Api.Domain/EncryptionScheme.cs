namespace BluePrism.Api.Domain
{
    using System.Diagnostics.CodeAnalysis;

    public class EncryptionScheme
    {
        public string Name { get; set; }
        public EncryptionAlgorithm Algorithm { get; set; }
        public EncryptionKeyLocation KeyLocation { get; set; }
        public bool IsAvailable { get; set; }

        public override bool Equals(object obj) =>
            obj is EncryptionScheme e
            && Name == e.Name
            && Algorithm == e.Algorithm
            && KeyLocation == e.KeyLocation
            && IsAvailable == e.IsAvailable;

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Algorithm.GetHashCode();
                hashCode = (hashCode * 397) ^ KeyLocation.GetHashCode();
                hashCode = (hashCode * 397) ^ IsAvailable.GetHashCode();
                return hashCode;
            }
        }
    }
}
