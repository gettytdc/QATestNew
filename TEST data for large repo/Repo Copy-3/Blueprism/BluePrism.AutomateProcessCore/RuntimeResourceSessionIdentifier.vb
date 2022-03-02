Imports System.Runtime.Serialization

<DataContract([Namespace]:="bp")>
<Serializable()>
Public Class RuntimeResourceSessionIdentifier
    Inherits SessionIdentifier

    <DataMember>
    Private mSessionNumber As Integer
    Public ReadOnly Property SessionNumber As Integer
        Get
            Return mSessionNumber
        End Get
    End Property

    Public Overrides ReadOnly Property SessionIdentifierType As SessionIdentifierType
        Get
            Return SessionIdentifierType.RuntimeResource
        End Get
    End Property

    Public Sub New(id As Guid, sessionNumber As Integer)
        MyBase.New(id)
        mSessionNumber = sessionNumber
    End Sub

End Class
