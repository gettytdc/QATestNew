namespace BluePrism.Api.Domain
{
    using System;

    public struct PageNumber
    {
        public const int MinValue = 1;
        public const int MaxValue = int.MaxValue;

        private readonly uint _value;

        private PageNumber(uint value)
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

        public static implicit operator uint(PageNumber value) => value._value;
        public static implicit operator int(PageNumber value) => (int)value._value;
        public static implicit operator PageNumber(uint value) => new PageNumber(value);

        public static implicit operator PageNumber(int value)
        {
            var unsignedValue =
                value < uint.MinValue
                    ? uint.MinValue
                    : (uint)value;

            return new PageNumber(unsignedValue);
        }
    }
}
