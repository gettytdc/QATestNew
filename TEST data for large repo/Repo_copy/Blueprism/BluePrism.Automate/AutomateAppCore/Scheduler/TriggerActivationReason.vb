''' Project  : AutomateAppCore
''' Class    : TriggerActivationReason
''' <summary>
''' The reason for the trigger activation - this is used when creating
''' a log for a trigger instance to determine the type of log created.
''' </summary>
Public Enum TriggerActivationReason

    ''' <summary>
    ''' The trigger indicates the execution of a schedule
    ''' </summary>
    Execute = 0

    ''' <summary>
    ''' The trigger indicates the suppression of a schedule
    ''' </summary>
    Suppress = 1

    ''' <summary>
    ''' The trigger mode was indeterminate
    ''' </summary>
    Indeterminate = 2

    ''' <summary>
    ''' After the scheduler was resumed and the schedule indicated by the
    ''' trigger was executed once, this instance was skipped.
    ''' </summary>
    MissedWhileNotRunning = 3

    ''' <summary>
    ''' Misfired due to the timezone changing, usually due to Daylight Savings
    ''' Time coming into or going out of effect.
    ''' </summary>
    TimeZoneMisfire = 4

End Enum
