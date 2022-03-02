
''' <summary>
''' Contains reply info for query commands.
''' </summary>
Public Class Reply

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Used to internally represent types of reply available.
    ''' </summary>
    Private Enum Type
        OK
        RESULT
        WARNING
        WINDOW
        AAELEMENT
        UIAELEMENT
        WEBELEMENT
        JAB
        HTML
        TERMINALFIELD
        CANCEL
        BITMAP
        SAP
        MAX
        NONE
        RECT
        TIMEOUT
    End Enum

    ''' <summary>
    ''' Reply value returned to indicate an 'OK' response
    ''' </summary>
    Public Shared ReadOnly Ok As Reply = New Reply(Type.OK)

    ''' <summary>
    ''' Reply value returned to indicate a 'Cancel' response.
    ''' </summary>
    Public Shared ReadOnly Cancel As Reply = New Reply(Type.CANCEL)

    ''' <summary>
    ''' Reply value returned to indicate a 'True' response.
    ''' </summary>
    Public Shared ReadOnly [True] As Reply = Result(True)

    ''' <summary>
    ''' Reply value returned to indicate a 'False' response.
    ''' </summary>
    Public Shared ReadOnly [False] As Reply = Result(False)

    ''' <summary>
    ''' Reply value returned to indicate a 'Max' value has been reached.
    ''' </summary>
    Public Shared ReadOnly Max As Reply = New Reply(Type.MAX)

    ''' <summary>
    ''' Reply value returned to indicate a 'None' response - typically indicating
    ''' that nothing was found in a search.
    ''' </summary>
    Public Shared ReadOnly None As Reply = New Reply(Type.NONE)

    ''' <summary>
    ''' Reply value returned to indicate a 'Timeout' condition was hit.
    ''' </summary>
    Public Shared ReadOnly Timeout As Reply = New Reply(Type.TIMEOUT)

    ''' <summary>
    ''' Creates a Reply value used to convey a string message
    ''' </summary>
    ''' <param name="message">The message to wrap into a reply</param>
    ''' <returns>The reply with the given message</returns>
    Public Shared Function Result(ByVal message As String) As Reply
        Return New Reply(Type.RESULT, message)
    End Function

    ''' <summary>
    ''' Reply value used to convey a string message
    ''' </summary>
    ''' <param name="formatMsg">The message, with format placeholders, to wrap into a
    ''' reply.</param>
    ''' <param name="args">The arguments to populate the format message with.</param>
    ''' <returns>The reply with the given formatted message</returns>
    Public Shared Function Result(
     formatMsg As String, ParamArray args() As Object) As Reply
        Return New Reply(Type.RESULT, String.Format(formatMsg, args))
    End Function

    ''' <summary>
    ''' Creates a Reply value used to convey a flag value
    ''' </summary>
    ''' <param name="flag">The flag value to wrap into a reply</param>
    ''' <returns>The reply with the given flag value</returns>
    Public Shared Function Result(ByVal flag As Boolean) As Reply
        Return New Reply(Type.RESULT, flag.ToString())
    End Function

    ''' <summary>
    ''' Creates a Reply value used to convey an integer value
    ''' </summary>
    ''' <param name="number">The integer value to wrap into a reply</param>
    ''' <returns>The reply with the given integer value</returns>
    Public Shared Function Result(ByVal number As Integer) As Reply
        Return New Reply(Type.RESULT, number.ToString())
    End Function

    ''' <summary>
    ''' Formulates a reply containing a terminal field and its data
    ''' </summary>
    ''' <param name="message">The data representing the terminal field; typically a
    ''' collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function TerminalField(ByVal message As String) As Reply
        Return New Reply(Type.TERMINALFIELD, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a window and its data
    ''' </summary>
    ''' <param name="message">The data representing the window; typically a
    ''' collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Window(ByVal message As String) As Reply
        Return New Reply(Type.WINDOW, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a bitmap and its data
    ''' </summary>
    ''' <param name="message">The data representing the bitmap; typically a
    ''' base64-encoded PNG-format image.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Bitmap(ByVal message As String) As Reply
        Return New Reply(Type.BITMAP, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a SAP field and its data
    ''' </summary>
    ''' <param name="message">The data representing the SAP field; typically a
    ''' collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Sap(ByVal message As String) As Reply
        Return New Reply(Type.SAP, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing an Accessibility element and its data
    ''' </summary>
    ''' <param name="message">The data representing the AA element; typically a
    ''' collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function AAElement(ByVal message As String) As Reply
        Return New Reply(Type.AAELEMENT, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a UIAutomation field and its data
    ''' </summary>
    ''' <param name="message">The data representing the UIAutomation field; typically
    ''' a collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function UIAElement(ByVal message As String) As Reply
        Return New Reply(Type.UIAELEMENT, message)
    End Function


    Public Shared Function WebElement(ByVal message As String) As Reply
        Return New Reply(Type.WEBELEMENT, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing an HTML element and its data
    ''' </summary>
    ''' <param name="message">The data representing the HTML element; typically a
    ''' collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Html(ByVal message As String) As Reply
        Return New Reply(Type.HTML, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a Java element and its data
    ''' </summary>
    ''' <param name="message">The data representing the Java element ; typically a
    ''' collection of identifiers and their values.</param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Jab(ByVal message As String) As Reply
        Return New Reply(Type.JAB, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a warning message
    ''' </summary>
    ''' <param name="message">The warning message to be returned from a query.
    ''' </param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Warning(ByVal message As String) As Reply
        Return New Reply(Type.WARNING, message)
    End Function

    ''' <summary>
    ''' Formulates a reply containing a rectangle definition 
    ''' </summary>
    ''' <param name="message">The data representing the rectangle definition;
    ''' typically a rectangle encoded as per
    ''' <see cref="BPCoreLib.RECT.ToString"/></param>
    ''' <returns>The reply object to return from a query</returns>
    Public Shared Function Rect(ByVal message As String) As Reply
        Return New Reply(Type.RECT, message)
    End Function

#End Region

#Region " Member Variables "

    ' The type of this reply
    Private mType As Type

    ' The message within this reply
    Private mMessage As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new reply of the given type with no message.
    ''' </summary>
    ''' <param name="type">The type of reply to create</param>
    Private Sub New(ByVal type As Type)
        mType = type
        mMessage = Nothing
    End Sub

    ''' <summary>
    ''' Creates a new reply of a specified type and message.
    ''' </summary>
    ''' <param name="type">The type of reply to create</param>
    ''' <param name="message">The detail message for the reply</param>
    Private Sub New(ByVal type As Type, ByVal message As String)
        mType = type
        mMessage = message
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Checks if this reply represents an 'OK' reply.
    ''' </summary>
    ''' <returns>True if this reply represents a reply of type 'OK'; False otherwise
    ''' (note that this will return false if a 'text' reply was created with a
    ''' message of "OK" - only reply objects created with the <see cref="Ok"/>
    ''' method will return true here)</returns>
    Public ReadOnly Property IsOk As Boolean
        Get
            Return (mType = Type.OK)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this reply represents a 'RECT' reply
    ''' </summary>
    ''' <returns>True if this reply object is a rect reply; False otherwise.
    ''' </returns>
    Public ReadOnly Property IsRect As Boolean
        Get
            Return (mType = Type.RECT)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this reply represents a result reply.
    ''' </summary>
    ''' <returns>True if this reply represents a result; False otherwise.</returns>
    Public ReadOnly Property IsResult As Boolean
        Get
            Return (mType = Type.RESULT)
        End Get
    End Property

    ''' <summary>
    ''' Gets the message associated with this reply
    ''' </summary>
    Public ReadOnly Property Message As String
        Get
            Return mMessage
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Gets a string representation of this reply.
    ''' This is in the format "{TYPE}:{MESSAGE}".
    ''' </summary>
    Public Overrides Function ToString() As String
        Dim sType As String = mType.ToString().ToUpperInvariant()
        If mMessage Is Nothing Then
            Return sType
        End If
        Return sType & ":" & mMessage
    End Function

End Class
