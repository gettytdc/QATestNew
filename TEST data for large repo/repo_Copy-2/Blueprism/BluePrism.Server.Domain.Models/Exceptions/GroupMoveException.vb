Imports System.Runtime.Serialization

''' <summary>
''' Exception which is generated if a group move is invalid
''' </summary>
<Serializable()> _
Public Class GroupMoveException : Inherits BluePrismException
    
    'effected groups
    Public Property SourceId As Guid
    Public Property DestinationId As Guid


    ''' <summary>
    ''' Deserializes an exception indicating that a lock was requested for archiving
    ''' when there is a machine which currently has the archive lock.
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
        SourceId = Guid.Parse(info.getstring("SourceId"))
        DestinationId = Guid.Parse(info.getstring("DestinationId"))
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message
    ''' </summary>
    ''' <param name="msg">The message detailing the error</param>
    Public Sub New(msg As String)
        MyBase.New(msg)
    End Sub

    Public Sub New (msg As String, sourceId As Guid, destId as Guid)
        MyBase.New(msg)
        Me.SourceId = sourceId
        Me.DestinationId = destId
    End Sub
    ''' <summary>
    ''' Allow the serialisation of the member variables 
    ''' </summary>
    ''' <param name="info"></param>
    ''' <param name="ctx"></param>
    Public Overrides Sub GetObjectData(info As SerializationInfo, ctx As StreamingContext)
        MyBase.GetObjectData(info, ctx)
        info.AddValue("SourceId",SourceId.ToString)
        info.AddValue("DestinationId",SourceId.ToString)
    End Sub

End Class
