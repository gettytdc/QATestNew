Imports System.Threading.Tasks
Imports BluePrism.ClientServerResources.Core.Enums

Namespace Resources
    Public Interface IResourceConnection
        ReadOnly Property ConnectionState As ResourceConnectionState
        ReadOnly Property ProcessesPending As Integer
        ReadOnly Property ProcessesRunning As Integer
        ReadOnly Property ResourceId As Guid
        Sub Terminate()
        Function GetSessions() As IDictionary(Of Guid, RunnerStatus)
        Function RefreshResource() As Task(Of Boolean)
        Function AwaitValidConnection(timeoutMillis As Integer, inuseIsValid As Boolean, ByRef state As ResourceConnectionState) As Boolean
    End Interface

End Namespace
