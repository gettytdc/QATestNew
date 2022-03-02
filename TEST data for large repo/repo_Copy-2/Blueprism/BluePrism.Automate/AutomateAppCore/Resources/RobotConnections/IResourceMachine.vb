Imports System.Drawing
Imports System.Threading.Tasks
Imports BluePrism.Core.Resources

Namespace Resources
    Public Interface IResourceMachine
        Property Attributes As ResourceAttribute
        Property CapabilitiesFriendly As String()
        ReadOnly Property ChildResourceCount As Integer
        ReadOnly Property ChildResourceNames As List(Of String)
        Property ChildResources As List(Of IResourceMachine)
        ReadOnly Property ConnectionState As ResourceConnectionState
        Property DBStatus As ResourceMachine.ResourceDBStatus
        Property DisplayStatus As ResourceStatus
        Property Id As Guid
        Property Info As String
        Property InfoColour As Color
        ReadOnly Property IsConnected As Boolean
        Property IsController As Boolean
        Property IsInPool As Boolean
        Property LastError As String
        ReadOnly Property Local As Boolean
        Property Name As String
        ReadOnly Property ProcessesPending As Integer
        ReadOnly Property ProcessesRunning As Integer
        Event AttributesChanged As EventHandler
        Event DbStatusChanged As EventHandler
        Function CheckResourcePCStatus(resourceName As String, ByRef errorMessage As String) As Boolean
        Function HasAnyAttribute(attr As ResourceAttribute) As Boolean
        Function HasAttribute(attr As ResourceAttribute) As Boolean
        Function HasPoolMember(name As String) As Boolean
        Function RefreshResourceConnection() As Task(Of Boolean)

        Function AwaitValidConnection(timeoutMillis As Integer, inuseIsValid As Boolean, ByRef resultantState As ResourceConnectionState) As Boolean
        Function ProvideConnectionState() As ResourceConnectionState
        Property UserID() As Guid
        Property SuccessfullyConnectedToAppServer As Boolean
    End Interface

End Namespace
