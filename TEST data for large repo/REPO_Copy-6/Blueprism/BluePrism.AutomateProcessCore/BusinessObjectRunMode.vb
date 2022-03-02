
''' <summary>
''' The different run-modes available to business objects.
''' </summary>
Public Enum BusinessObjectRunMode
    ''' <summary>
    ''' No run mode - used to indicate that nothing else can run
    ''' </summary>
    None = -1

    ''' <summary>
    ''' Exclusive business objects can only be run when no other business objects
    ''' are in use, whatever the type.
    ''' </summary>
    Exclusive = 0

    ''' <summary>
    ''' Background business objects can run with other business objects at the
    ''' same time, as long as the other objects allow simultaneous operation.
    ''' </summary>
    Background = 1

    ''' <summary>
    ''' Foreground business objects can run with other business objects at the
    ''' same time, as long as there is no other foreground business object in
    ''' use.
    ''' </summary>
    Foreground = 2

End Enum
