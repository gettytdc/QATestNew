Imports System.Collections.Concurrent
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore

Public Interface IListener
    ReadOnly Property IsLoginAgent As Boolean
    ReadOnly Property ResourceConnections As IResourceConnectionManager
    ReadOnly Property ResourceId As Guid
    Property Availability As BusinessObjectRunMode
    Property Runners As RunnerRecordList
    Property Clients As List(Of ListenerClient)
    ReadOnly Property IsController As Boolean
    Property PoolId As Guid
    Property ResourceOptions As ResourcePCStartUpOptions
    Property UserId As Guid
    Property UserName As String
    Property ResourceName As String
    Property RunState As ResourcePcRunState
    Event Err(message As String)
    Event Failed(sReason As String)
    Event Info(message As String)
    Event ShutdownResource()
    Event Verbose(message As String)
    Event Warn(message As String)
    Sub AddNotification(msg As String)
    Sub AddNotification(formattedMsg As String, ParamArray args() As Object)
    Sub InitiateShutdown(reason As String, waitForSessions As Boolean)
    Sub NotifyStatus()
    Sub RaiseError(message As String)
    Sub RaiseError(msg As String, ParamArray args() As Object)
    Sub RaiseInfo(message As String)
    Sub RaiseInfo(msg As String, ParamArray args() As Object)
    Function CreateRunner(runnerRequest As RunnerRequest) As ListenerRunnerRecord
    Function FindRunner(sessId As Guid) As ListenerRunnerRecord
    Function GetActiveConnections() As Integer
    Function GetActiveSessionCount() As Integer
    Function GetPendingSessionCount() As Integer
    Function Shutdown(ByRef sErr As String) As Boolean
    Function Startup(startupOptions As ResourcePCStartUpOptions, ByRef sErr As String) As Boolean
End Interface
