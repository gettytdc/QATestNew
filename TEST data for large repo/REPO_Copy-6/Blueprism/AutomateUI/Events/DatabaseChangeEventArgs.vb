
''' <summary>
''' Class describing the event arguments for a database change.
''' </summary>
Public Class DatabaseChangeEventArgs : Inherits EventArgs

    ' Flag indicating if the database connection was successful
    Private mSuccess As Boolean

    ' The long message to given to the users
    Private mLongMessage As String

    ' The shorter message to give to the users regarding the connection
    Private mShortMessage As String

    'The exception information about this event.
    Private mException As Exception

    ''' <summary>
    ''' Indicates whether any changes were made. This can be used to signify that
    ''' nothing in the UI needs updating.
    ''' </summary>
    Public Property ChangesMade As Boolean

    ''' <summary>
    ''' Constructor that simply creates a DatabaseChangeEventArgs and
    ''' indicates that no changes were made.
    ''' </summary>
    Public Sub New(success As Boolean)
        mSuccess = success
        ChangesMade = False
    End Sub

    ''' <summary>
    ''' Creates a new DatabaseChangeEventArgs with the given success flag and the
    ''' specified message for the user.
    ''' </summary>
    ''' <param name="success">True to indicate success; False to indicate failure.
    ''' </param>
    ''' <param name="exception">The exception information about this event.</param>
    ''' <param name="shortMsg">The short (no more than one line) message with
    ''' optional placeholders to display to the user to provide information about
    ''' this event.</param>
    ''' <param name="args">The arguments with which to format the supplied messages
    ''' </param>
    Public Sub New(ByVal success As Boolean, _
                   ByVal exception As Exception, _
                   ByVal longMsg As String, _
                   ByVal shortMsg As String, _
                   ByVal ParamArray args() As Object)
        mSuccess = success
        mException = exception
        If longMsg IsNot Nothing Then mLongMessage = String.Format(longMsg, args)
        If shortMsg IsNot Nothing Then mShortMessage = String.Format(shortMsg, args)
        ChangesMade = True
    End Sub

    ''' <summary>
    ''' Flag indicating whether the database connection attempt was a success (True)
    ''' or a failure (False).
    ''' </summary>
    Public ReadOnly Property Success() As Boolean
        Get
            Return mSuccess
        End Get
    End Property

    ''' <summary>
    ''' The detailed user message providing information to the user about this
    ''' event.
    ''' </summary>
    Public ReadOnly Property LongMessage() As String
        Get
            If mLongMessage Is Nothing Then Return String.Empty
            Return mLongMessage
        End Get
    End Property


    ''' <summary>
    ''' The user message providing information to the user about this event.
    ''' This should be a single line only
    ''' </summary>
    Public ReadOnly Property ShortMessage() As String
        Get
            If mShortMessage Is Nothing Then Return String.Empty
            Return mShortMessage
        End Get
    End Property

    ''' <summary>
    ''' If an exception occoured this provides access to the exception information.
    ''' </summary>
    Public ReadOnly Property Exception As Exception
        Get
            Return mException
        End Get
    End Property
End Class
