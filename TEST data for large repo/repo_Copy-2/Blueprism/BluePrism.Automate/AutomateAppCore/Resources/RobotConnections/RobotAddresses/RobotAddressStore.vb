Imports System.Collections.Concurrent
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Resources
Imports NLog

Namespace Resources
    Public Class RobotAddressStore
        Implements IRobotAddressStore

        Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()
        Private Shared ReadOnly Lock As New Object

        Private ReadOnly mAddressStore As ConcurrentDictionary(Of String, RobotAddress) = New ConcurrentDictionary(Of String, RobotAddress)
        Private ReadOnly mRefreshPeriodSeconds As Integer = 120

        Public Property Server As IServer Implements IRobotAddressStore.Server
        Public Property RequireSecureResourceConnection As Boolean Implements IRobotAddressStore.RequireSecureResourceConnection
        Public Property ResourceRegistrationMode As ResourceRegistrationMode Implements IRobotAddressStore.ResourceRegistrationMode

        Private mNextUpdate As Date = Date.MinValue

        Public Sub New(serverConnection As IServer, refreshPeriodSeconds As Integer)
            Server = serverConnection
            mRefreshPeriodSeconds = refreshPeriodSeconds
            RefreshResourceAddressLookup()
        End Sub

        Private Sub RefreshResourceAddressLookup()
            Dim timer = Stopwatch.StartNew()

            RequireSecureResourceConnection = Server.GetRequireSecuredResourceConnections()
            ResourceRegistrationMode = Server.GetResourceRegistrationMode()

            Dim dt = Server.GetResources(ResourceAttribute.None, ResourceAttribute.Retired Or ResourceAttribute.Debug, Nothing)

            If dt IsNot Nothing Then
                For Each dr As DataRow In dt.Rows
                    Dim record = CreateRobotAddressRecord(Guid.Parse(dr("resourceid").ToString()),
                                                          dr("name").ToString(),
                                                          IIf(Convert.IsDBNull(dr("FQDN")), "", dr("FQDN")).ToString(),
                                                          CBool(dr("ssl")))
                    mAddressStore.AddOrUpdate(record.ResourceName, record, Function(key, value) record)
                Next
                Log.Debug($"Refreshed Robot Address lookup store population in {timer.ElapsedMilliseconds}")
            End If
        End Sub

        Private Function CreateRobotAddressRecord(id As Guid, resourceName As String, fqdn As String, ssl As Boolean) As RobotAddress
            Dim hostName As String
            Dim portNo As Integer
            Dim index As Integer = resourceName.IndexOf(":")

            If index = -1 Then
                hostName = resourceName
                portNo = ResourceMachine.DefaultPort
            Else
                hostName = resourceName.Left(index)
                portNo = CInt(resourceName.Mid(index + 2))
            End If

            Return New RobotAddress(id, resourceName, hostName, portNo, ssl, fqdn)
        End Function

        Public Function GetRobotAddress(robotName As String) As RobotAddress Implements IRobotAddressStore.GetRobotAddress

            ' Cache misses are unlikely, it's possible a newly started robot is not in the cache, let's check
            ' and force an in process query for the missing details.
            If Not mAddressStore?.ContainsKey(robotName) OrElse mNextUpdate < Date.UtcNow Then
                SyncLock Lock
                    Try
                        ' Re-check it hasn't been updated by another thread while we were synclocked
                        If mNextUpdate < Date.UtcNow OrElse Not mAddressStore?.ContainsKey(robotName) Then
                            Log.Debug("Refreshing Robot Address Store")
                            RefreshResourceAddressLookup()
                            mNextUpdate = Date.UtcNow.AddSeconds(mRefreshPeriodSeconds)
                        End If
                    Catch ex As Exception
                        Log.Error(ex, "Unable to refresh Robot Address Store")
                    End Try
                End SyncLock
            End If

            Dim address As RobotAddress = Nothing
            mAddressStore.TryGetValue(robotName, address)

            ' Apply logic that was original in db layer
            If address Is Nothing Then
                Log.Warn($"Counldn't find record for resource {robotName}")
            ElseIf ResourceRegistrationMode <> ResourceRegistrationMode.MachineMachine Then
                If String.IsNullOrWhiteSpace(address.FQDN) Then
                    Throw New InvalidOperationException(String.Format("Resource {0} has no FQDN", address.ResourceName))
                End If
                address = New RobotAddress(address.ResourceId, address.ResourceName, address.FQDN, address.Port, address.UseSsl, address.FQDN)
            End If

            Return address
        End Function

    End Class

End Namespace
