namespace BluePrism.Api.Domain
{
    using System;

    public struct ItemsPerPage
    {
        public const int MinValue = 1;
        public const int MaxValue = int.MaxValue;

        private readonly uint _value;

        private ItemsPerPage(uint value)
        {
            if (value > MaxValue)
            {
                throw new ArgumentException($"Value of {nameof(value)} cannot be greater than {MaxValue}", nameof(value));
            }

            if (value < MinValue)
            {
                throw new ArgumentException($"Value of {nameof(value)} cannot be less than {MinValue}", nameof(value));
            }

            _value = value;
        }

        public static implicit operator uint(ItemsPerPage value) => value._value;
        public static implicit operator int(ItemsPerPage value) => (int)value._value;
        public static implicit operator ItemsPerPage(uint value) => new ItemsPerPage(value);

        public static implicit operator ItemsPerPage(int value)
        {
            var unsignedValue =
                value < uint.MinValue
                    ? uint.MinValue
                    : (uint)value;

            return new ItemsPerPage(unsignedValue);
        }
    }
}
