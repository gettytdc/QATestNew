Imports System.Reflection
Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib

<DataContract([Namespace]:="bp"), Serializable, KnownType(NameOf(SessionIdentifier.GetAllKnownTypes))>
Public MustInherit Class SessionIdentifier

    <DataMember>
    Private mId As Guid

    Public ReadOnly Property Id As Guid
        Get
            Return mId
        End Get
    End Property

    Public Sub New(id As Guid)
        mId = id
    End Sub

    Public MustOverride ReadOnly Property SessionIdentifierType As SessionIdentifierType

    Public Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
        Return Assembly.GetExecutingAssembly().GetConcreteSubclasses(Of SessionIdentifier)()
    End Function
End Class
