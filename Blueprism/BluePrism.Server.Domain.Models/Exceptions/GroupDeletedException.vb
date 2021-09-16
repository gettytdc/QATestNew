Imports System.Runtime.Serialization

<Serializable()> _
Public Class GroupDeletedException : Inherits BluePrismException

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class
