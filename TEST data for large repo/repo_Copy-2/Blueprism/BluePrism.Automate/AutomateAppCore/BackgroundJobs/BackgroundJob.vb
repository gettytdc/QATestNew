Imports System.Runtime.Serialization

''' <summary>
''' Contains data about a background job that has started running
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class BackgroundJob
    <DataMember>
    Private ReadOnly [mId] As Guid

    ''' <summary>
    ''' Creates a new BackgroundJob
    ''' </summary>
    ''' <param name="id">The job identifier</param>
    Public Sub New(id As Guid)
        mID = id
    End Sub

    ''' <summary>
    ''' The job identifier
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Id as Guid
        Get
            return mId
        End Get
    End Property

End Class