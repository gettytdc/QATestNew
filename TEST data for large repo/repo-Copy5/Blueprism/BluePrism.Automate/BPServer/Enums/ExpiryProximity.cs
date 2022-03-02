namespace BluePrism.BPServer.Enums
{
    enum ExpiryProximity
    {
        [Expiry(interval: 1, logLevel: LoggingLevel.Information)]
        None,
        [Expiry(interval: 14, logLevel: LoggingLevel.Information)]
        SixMonths,
        [Expiry(interval: 7, logLevel: LoggingLevel.Warning)]
        ThreeMonths,
        [Expiry(interval: 1, logLevel: LoggingLevel.Warning)]
        OneMonth,
        [Expiry(interval: (double)1 / 24, logLevel: LoggingLevel.Error)]
        OneDay
    }
}
