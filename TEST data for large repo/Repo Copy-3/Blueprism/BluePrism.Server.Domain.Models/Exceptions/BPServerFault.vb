Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.ServiceModel

''' <summary>
''' A generic fault which is used to wrap any type of exception
''' </summary>
<DataContract([Namespace]:="bp")>
Public Class BPServerFault

    ''' <summary>
    ''' Creates a FaultException(Of BPServerFault) from the given exception's detail
    ''' </summary>
    ''' <param name="e">The exception whose detail should populate the fault
    ''' exception returned.</param>
    ''' <returns>A fault exception containing the detail from the given exception.
    ''' </returns>
    Public Shared Function CreateFaultException(e As Exception) _
        As FaultException(Of BPServerFault)
        Return New FaultException(Of BPServerFault)(New BPServerFault(e), e.Message)
    End Function

    ' The exception that is wrapped by this server fault
    Private mException As Exception

    ''' <summary>
    ''' Creates a new server fault instance
    ''' </summary>
    ''' <param name="ex">The exception that the fault represents</param>
    Private Sub New(ex As Exception)
        mException = ex
    End Sub

    ''' <summary>
    ''' Gets or sets the serialized exception for this fault.
    ''' This is a <em>binary</em> serialization of the exception held within this
    ''' server fault.
    ''' </summary>
    <DataMember>
    Private Property SerializedException As String
        Get
            Return SerializeToString()
        End Get
        Set(value As String)
            mException = DeserializeFromString(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets the exception wrapped inside this server fault, or null if it has no
    ''' exception.
    ''' </summary>
    Public ReadOnly Property InnerException As Exception
        Get
            Return mException
        End Get
    End Property

    ''' <summary>
    ''' Rethrows the exception held within this server fault. Does nothing if it
    ''' has no exception held within it.
    ''' </summary>
    Public Sub Rethrow()
        If mException IsNot Nothing Then mException.RethrowWithStackTrace()
    End Sub

    ''' <summary>
    ''' Serializes the local exception to a base64-encoded string
    ''' </summary>
    ''' <returns>A string containing the serialized exception</returns>
    Private Function SerializeToString() As String
        If mException Is Nothing Then Return Nothing

        Using ms = New MemoryStream()
            Call New BinaryFormatter().Serialize(ms, mException)
            Return Convert.ToBase64String(ms.ToArray())
        End Using
    End Function

    ''' <summary>
    ''' Deserializes the given base64-encoded string into an Exception.
    ''' </summary>
    ''' <param name="base64Exception">The base64-encoded exception string</param>
    Private Function DeserializeFromString(base64Exception As String) As Exception
        If String.IsNullOrEmpty(base64Exception) Then Return Nothing

        Dim data As Byte() = Convert.FromBase64String(base64Exception)
        Using ms = New MemoryStream(data)
            Return DirectCast(New BinaryFormatter().Deserialize(ms), Exception)
        End Using
    End Function

End Class
