Namespace Resources

    ''' <summary>
    ''' Class to hold a log entry with a log level
    ''' </summary>
    Public Class ResourceNotification
        Public Sub New(ByVal notificationLevel As ResourceNotificationLevel, ByVal txt As String, at As DateTime)
            Level = notificationLevel
            Text = String.Format("[{0:u}] {1}", at, txt)
        End Sub

        ''' <summary>
        ''' The logging level of this log entry
        ''' </summary>
        Public ReadOnly Property Level() As ResourceNotificationLevel
        ''' <summary>
        ''' The text detailing the message for this log entry
        ''' </summary>
        Public ReadOnly Property Text() As String
    End Class
End NameSpace