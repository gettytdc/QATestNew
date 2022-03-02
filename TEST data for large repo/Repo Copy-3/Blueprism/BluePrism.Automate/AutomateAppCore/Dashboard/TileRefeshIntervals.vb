Imports BluePrism.BPCoreLib

Public Enum TileRefreshIntervals
    <FriendlyName("Never")> Never = 0
    <FriendlyName("Every minute")> EveryMinute = 60
    <FriendlyName("Every 5 minutes")> EveryFiveminutes = 300
    <FriendlyName("Every 10 minutes")> EveryTenMinutes = 600
    <FriendlyName("Every 30 minutes")> EveryHalfHour = 1800
End Enum
