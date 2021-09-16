''' <summary>
''' The running states of a ResourcePC instance
''' </summary>
''' <remarks></remarks>
Public Enum ResourcePcRunState
    ''' <summary>
    ''' ResourcePC is not running
    ''' </summary>
    Stopped = 0
    ''' <summary>
    ''' ResourcePC is in its normal running state
    ''' </summary>
    Running = 1
    ''' <summary>
    ''' ResourcePC is shutting down and is waiting for any running sessions to complete 
    ''' before stopping. No new sessions can be added or existing sessions started. Waiting 
    ''' for sessions to stop is an optional behaviour when shutting down.
    ''' </summary>
    WaitingToStop = 2
    ''' <summary>
    ''' The ResourcePC is stopping
    ''' </summary>
    Stopping = 3
End Enum

