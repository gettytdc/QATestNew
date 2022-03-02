Imports System.Threading
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Core.Resources

Namespace Resources
    Public Interface IResourceConnectionManager
        Inherits IDisposable
        ReadOnly Property Mode As Modes
        ReadOnly Property MaxRefreshInterval As TimeSpan
        Property MonitorSessionVariables As Boolean
        ReadOnly Property QueryCapabilities As Boolean
        ReadOnly Property SessionVariables As IDictionary(Of String, clsSessionVariable)
        ReadOnly Property ServerName As String
        ReadOnly Property UsingAppServerConnection As Boolean
        Event ResourceStatusChanged As ResourcesChangedEventHandler
        Event SessionCreate As SessionCreateEventHandler
        Event SessionDelete As SessionDeleteEventHandler
        Event SessionEnd As SessionEndEventHandler
        Event SessionStart As SessionStartEventHandler
        Event SessionStop As SessionStopEventHandler
        Event SessionVariablesUpdated As SessionVariableUpdatedHandler
        Event ShowUserMessage As EventHandler(Of String)
        Sub GetLatestDBResourceInfo()
        Sub GetLatestDBResourceInfo(excludedResources As ResourceAttribute)

        Function GetActiveResources(connectedOnly As Boolean) As List(Of IResourceMachine)
        Function GetControllingResource(name As String) As IResourceMachine
        Function GetResource(resourceID As Guid) As IResourceMachine
        Function GetResource(name As String) As IResourceMachine
        Function GetResources() As Dictionary(Of Guid, IResourceMachine)
        Function GetResources(connectedOnly As Boolean) As Dictionary(Of Guid, IResourceMachine)

        Function SendCreateSession(sessions As IEnumerable(Of CreateSessionData)) As Guid()
        Sub SendDeleteSession(sessions As IEnumerable(Of DeleteSessionData))
        Sub SendStartSession(sessions As IEnumerable(Of StartSessionData))
        Sub SendStopSession(sessions As IEnumerable(Of StopSessionData))

        Sub SendSetSessionVariable(resourceID As Guid, vars As Queue(Of clsSessionVariable))
        Sub SendGetSessionVariables(resourceId As Guid, sessionID As Guid, processId As Guid, ByRef sErr As String)
        Sub ToggleShowSessionVariables(monitorSessionVars As Boolean)

        Function TryGetNextUserMessage(ByRef msg As String) As Boolean
        Sub RefreshResourceConnection(resourceId As Guid)
        Function CheckInstructionalClientStatus() As Boolean

        Enum Modes
            ''' <summary>
            ''' Normal mode maintains connections to all Resource PCs, except those
            ''' that are members of a pool. This is the standard mode used by Control
            ''' Room.
            ''' </summary>
            Normal

            ''' <summary>
            ''' In controller mode, connections are maintained to all Resource PCs
            ''' that are members of a particular pool, except the controller itself.
            ''' </summary>
            Controller

            ''' <summary>
            ''' In pool member mode, a connection is maintained only to the controller
            ''' of the pool. This is used for proxying incoming requests to it.
            ''' </summary>
            PoolMember

        End Enum


        ReadOnly Property RateLimiter() As AutoResetEvent

        Property ConnectionUser() As IUser

        ReadOnly Property UserId() As Guid

        ReadOnly Property PoolId() As Guid

        ReadOnly Property IsDisposed As Boolean

    End Interface

End Namespace
