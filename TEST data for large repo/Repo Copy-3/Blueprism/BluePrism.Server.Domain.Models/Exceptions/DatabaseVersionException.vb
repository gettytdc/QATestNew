''' <summary>
''' Exception thrown when the database version differs from the version supported
''' by the code.
''' </summary>
<Serializable>
Public Class DatabaseVersionException
    Inherits Exception

    ' The version that this code is expecting.
    Private mCodeVersion As Integer = -1

    ' The version found on the database.
    Private mDbVersion As Integer = -1

    ''' <summary>
    ''' The version supported by this code.
    ''' </summary>
    Public ReadOnly Property CodeVersion() As Integer
        Get
            Return mCodeVersion
        End Get
    End Property

    ''' <summary>
    ''' The version found on the database being connected to
    ''' </summary>
    Public ReadOnly Property DatabaseVersion() As Integer
        Get
            Return mDbVersion
        End Get
    End Property

    ''' <summary>
    ''' Creates a new DatabaseVersionException indicating the 
    ''' </summary>
    ''' <param name="reqdVersion">The version supported by this code</param>
    ''' <param name="currVersion">The version found on the database being
    ''' connected to</param>
    ''' <param name="message">The formatted message with placeholders for the
    ''' arguments.</param>
    ''' <param name="args">The arguments for the message</param>
    ''' <remarks></remarks>
    Public Sub New(
                   ByVal currVersion As Integer, ByVal reqdVersion As Integer,
                   ByVal message As String, ByVal ParamArray args() As Object)

        MyBase.New(String.Format(message, args) & " " &
                   String.Format("Database version:{0}, Required version:{1}.", currVersion, reqdVersion))

    End Sub

End Class
