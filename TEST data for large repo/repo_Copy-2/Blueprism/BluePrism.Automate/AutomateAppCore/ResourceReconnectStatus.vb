
''' <summary>
''' Flag denoting the different reconnect statuses for a resource connection
''' </summary>
<Flags()> _
Public Enum ResourceReconnectStatus
    ''' <summary>
    ''' The resource connection will not attempt to connect again if the connection
    ''' fails.
    ''' </summary>
    None = 0
    ''' <summary>
    ''' The resource connection will attempt to connect again if the connection fails.
    ''' </summary>
    ReconnectOnFail = 1
    ''' <summary>
    ''' The resource connection is currently attempting to reconnect.
    ''' </summary>
    AttemptingReconnect = 2


End Enum