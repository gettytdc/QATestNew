Imports System.Runtime.Serialization


''' <summary>
''' Exception thrown when a dashboard name is required and that which has been
''' specified does not exist.
''' </summary>
<Serializable>
Public Class NoSuchDashboardException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception indicating that no dashboard with the specified name
    ''' was found.
    ''' </summary>
    Public Sub New(id As Guid)
        MyBase.New(My.Resources.NoSuchDashboardException_DashboardWithID0NotFound, id)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
