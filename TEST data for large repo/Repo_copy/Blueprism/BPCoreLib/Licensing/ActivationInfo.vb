Imports System.Runtime.Serialization

<Serializable>
<DataContract(Namespace:="bp")>
Public NotInheritable Class ActivationInfo

    Public Sub New(requestId As Integer, reference As Guid)
        mRequestId = requestId
        mReference = reference
    End Sub

    <DataMember>
    Private mRequestId As Integer

    <DataMember>
    Private mReference As Guid

    Public ReadOnly Property RequestId As Integer
        Get
            Return mRequestId
        End Get
    End Property

    Public ReadOnly Property Reference As Guid
        Get
            Return mReference
        End Get
    End Property
End Class
