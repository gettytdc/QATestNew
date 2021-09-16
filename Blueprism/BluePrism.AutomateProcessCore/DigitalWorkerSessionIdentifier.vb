Imports System.Runtime.Serialization

<DataContract([Namespace]:="bp")>
<Serializable()>
Public Class DigitalWorkerSessionIdentifier
    Inherits SessionIdentifier

    Public Sub New(id As Guid)
        MyBase.New(id)
    End Sub

    Public Overrides ReadOnly Property SessionIdentifierType As SessionIdentifierType
        Get
            Return SessionIdentifierType.DigitalWorker
        End Get
    End Property
End Class
