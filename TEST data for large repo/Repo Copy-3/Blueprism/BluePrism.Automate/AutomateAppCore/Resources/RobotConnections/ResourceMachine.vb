Imports System.Drawing
Imports System.Runtime.Serialization
Imports System.Threading.Tasks
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Core.Resources

Namespace Resources

    <DebuggerDisplay("Name: {Name}")>
    Public Class ResourceMachine
        Implements IResourceMachine

        ''' <summary>
        ''' Event raised when the DBStatus property changes
        ''' </summary>
        Public Event DbStatusChanged As EventHandler Implements IResourceMachine.DbStatusChanged

        ''' <summary>
        ''' Event raised when the Attributes property changes
        ''' </summary>
        Public Event AttributesChanged As EventHandler Implements IResourceMachine.AttributesChanged

        ''' <summary>
        ''' The default port that a resource machine listens on
        ''' </summary>
        Public Const DefaultPort As Integer = 8181

        ''' <summary>
        ''' Returns whether the resource machine has ever successfully connected to app server.
        ''' </summary>
        Public Property SuccessfullyConnectedToAppServer As Boolean Implements IResourceMachine.SuccessfullyConnectedToAppServer


        ''' <summary>
        ''' Gets the resource name for a resource on this machine running on the given
        ''' port.
        ''' </summary>
        ''' <param name="portNo">The port number on which the resource is listening. If
        ''' not specified then the default port is assumed.</param>
        ''' <returns>The resource name for a resource listening on the given port number
        ''' on the current machine.</returns>
        Public Shared Function GetName(Optional portNo As Integer = DefaultPort) As String

            Static resourceRegistrationMode As ResourceRegistrationMode
            Static rrmNextUpdate As DateTime = DateTime.UtcNow.AddHours(-1)

            If rrmNextUpdate < DateTime.UtcNow Then
                resourceRegistrationMode = gSv.GetResourceRegistrationMode()
                rrmNextUpdate = DateTime.UtcNow.AddMinutes(1)
            End If


            Dim name As String
            If resourceRegistrationMode = ResourceRegistrationMode.FQDNFQDN Then
                name = clsUtility.GetFQDN()
            Else
                name = Environment.MachineName
            End If
            If portNo <> DefaultPort Then
                name &= ":" & portNo
            End If
            Return name
        End Function

        ''' <summary>
        ''' Extracts the port number from the resource name.
        ''' </summary>
        ''' <param name="name">The resource name</param>
        ''' <returns>The port number</returns>
        Public Shared Function GetPortFromName(name As String) As Integer
            If String.IsNullOrEmpty(name) Then Return 0
            Dim info As String() = Split(name, ":")
            If info.Length = 1 Then
                Return DefaultPort
            Else
                Return CInt(info(1))
            End If
        End Function

        ''' <summary>
        ''' Helper function for checking Resource PC status. Determines if it is possible
        ''' to communicate with the Resource PC at this point in time.
        ''' </summary>
        ''' <param name="resourceName">The name of the Resource PC - used only for formulating
        ''' error messages.</param>
        ''' <param name="errorMessage">If communication is not possible, on return this contains
        ''' a message giving the reason.</param>
        ''' <returns>True if communication is possible, False otherwise.</returns>
        Public Overridable Function CheckResourcePCStatus(ByVal resourceName As String, ByRef errorMessage As String) As Boolean Implements IResourceMachine.CheckResourcePCStatus
            If Me IsNot Nothing Then
               If ConnectionState = ResourceConnectionState.Connected Then
                  If DBStatus = ResourceMachine.ResourceDBStatus.Ready Then Return True
                Else
                    errorMessage = String.Format(My.Resources.clsResourceMachine_Resource0IsNotConnected, resourceName)
                End If
            Else
                errorMessage = String.Format(My.Resources.clsResourceMachine_FailedToGetStatusOf0ResourceNotFound, resourceName)
            End If
            errorMessage = FormatDefaultResourcePcConnectionErrorMessage(resourceName, errorMessage)

            Return False
        End Function

        Protected Function FormatDefaultResourcePcConnectionErrorMessage(resourceName As String, errorMessage As String) As String

            errorMessage =
                If(errorMessage = String.Empty,
                   String.Format(My.Resources.clsResourceMachine_0IsCurrently1AndIsNotAvailable, resourceName, GetDBStatusFriendlyName(DBStatus)),
                   errorMessage)
            Return errorMessage
        End Function

        Public Function ProvideConnectionState() As ResourceConnectionState Implements IResourceMachine.ProvideConnectionState
            If ConnectionState = ResourceConnectionState.Server OrElse ConnectionState = ResourceConnectionState.Disconnected Then
                If Not SuccessfullyConnectedToAppServer AndAlso IsConnected  _
                   AndAlso Not HasAttribute(ResourceAttribute.DefaultInstance) _
                   AndAlso Not IsInPool Then
                    Return ResourceConnectionState.Disconnected
                End If
            End If

            Return ConnectionState
        End Function

        ''' <summary>
        ''' Class to represent the combined configuration for a number of resource PCs
        ''' </summary>
        <Serializable()>
        <DataContract(Name:="cc", [Namespace]:="bp")>
        Public Class CombinedConfig

            ''' <summary>
            ''' The combined state of the resource configs represented by a single
            ''' CombinedConfig object.
            ''' </summary>
            ''' <remarks>Note that these values correspond precisely to the
            ''' <see cref="System.Windows.Forms.CheckState"/> enumeration. That is not
            ''' directly used, simply to avoid references to forms within AutomateAppCore
            ''' </remarks>
            <DataContract([Namespace]:="bp", Name:="c")>
            Public Enum CombinedState
                ''' <summary>
                ''' All resources represented in this config have a state for a given
                ''' property of disabled.
                ''' </summary>
                ''' <remarks>Represents 
                ''' <see cref="System.Windows.Forms.CheckState.Unchecked"/></remarks>
                <EnumMember(Value:="D")> Disabled = 0

                ''' <summary>
                ''' All resources represented in this config have a state for a given
                ''' property of enabled.
                ''' </summary>
                ''' <remarks>Represents 
                ''' <see cref="System.Windows.Forms.CheckState.Checked"/></remarks>
                <EnumMember(Value:="E")> Enabled = 1

                ''' <summary>
                ''' The resources represented in this config have different state for
                ''' the affected property (or the property was never set
                ''' </summary>
                ''' <remarks>Represents 
                ''' <see cref="System.Windows.Forms.CheckState.Indeterminate"/></remarks>
                <EnumMember(Value:="I")> Indeterminate = 2

            End Enum

            <DataMember(Name:="D", EmitDefaultValue:=False)>
            Public LoggingDefault As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="A", EmitDefaultValue:=False)>
            Public LoggingAllOverride As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="K", EmitDefaultValue:=False)>
            Public LoggingKeyOverride As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="E", EmitDefaultValue:=False)>
            Public LoggingErrorsOnlyOverride As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="M", EmitDefaultValue:=False)>
            Public LoggingMemory As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="G", EmitDefaultValue:=False)>
            Public LoggingForceGC As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="L", EmitDefaultValue:=False)>
            Public LoggingToEventLog As CombinedState = CombinedState.Indeterminate

            <DataMember(Name:="W", EmitDefaultValue:=False)>
            Public LoggingWebServices As CombinedState = CombinedState.Indeterminate

            ''' <summary>
            ''' Compounds the given state with the specified flag value
            ''' </summary>
            ''' <param name="state">The current state</param>
            ''' <param name="value">The value to compound into the given state.</param>
            ''' <returns>Indeterminate if the current state is already indeterminate or
            ''' if the specified value does not align with the current state (ie. if the
            ''' flag is False and the current state is Enabled, or if the flag is true
            ''' and the current state is Disabled)</returns>
            Public Function CompoundState(ByVal state As CombinedState, ByVal value As Boolean) As CombinedState
                Select Case state
                    Case CombinedState.Indeterminate : Return CombinedState.Indeterminate
                    Case CombinedState.Enabled
                        If value Then Return CombinedState.Enabled
                        Return CombinedState.Indeterminate
                    Case CombinedState.Disabled
                        If Not value Then Return CombinedState.Disabled
                        Return CombinedState.Indeterminate
                End Select
                ' Really shouldn't reach this point, but for the sake of the compiler, assume 'unknown'
                Return CombinedState.Indeterminate
            End Function

            ''' <summary>
            ''' Sets the logging states for all of the resource config options to enabled.
            ''' </summary>
            Friend Sub EnableAllLogging()
                LoggingDefault = CombinedState.Enabled
                LoggingAllOverride = CombinedState.Enabled
                LoggingKeyOverride = CombinedState.Enabled
                LoggingErrorsOnlyOverride = CombinedState.Enabled
                LoggingMemory = CombinedState.Enabled
                LoggingForceGC = CombinedState.Enabled
                LoggingToEventLog = CombinedState.Enabled
                LoggingWebServices = CombinedState.Enabled
            End Sub

            ''' <summary>
            ''' Sets the compound states for all of the resource config options to enabled or
            ''' disabled, based on the given diags.
            ''' </summary>
            Friend Sub SetLoggingStates(diags As clsAPC.Diags, logToEventLog As Boolean)

                LoggingDefault = GetCombinedState(ShouldApplyDefaultLogging(diags))
                LoggingAllOverride = GetCombinedState(diags.HasFlag(clsAPC.Diags.LogOverrideAll))
                LoggingKeyOverride = GetCombinedState(diags.HasFlag(clsAPC.Diags.LogOverrideKey))
                LoggingErrorsOnlyOverride = GetCombinedState(diags.HasFlag(clsAPC.Diags.LogOverrideErrorsOnly))
                LoggingMemory = GetCombinedState(diags.HasFlag(clsAPC.Diags.LogMemory))
                LoggingForceGC = GetCombinedState(diags.HasFlag(clsAPC.Diags.ForceGC))
                LoggingToEventLog = GetCombinedState(logToEventLog)
                LoggingWebServices = GetCombinedState(diags.HasFlag(clsAPC.Diags.LogWebServices))
            End Sub

            Private Function GetCombinedState(shouldLog As Boolean) As CombinedState
                Return If(shouldLog, CombinedState.Enabled, CombinedState.Disabled)
            End Function

            Private Function ShouldApplyDefaultLogging(diags As clsAPC.Diags) As Boolean
                Return Not diags.HasAnyFlag(clsAPC.Diags.LogOverrideKey _
                        Or clsAPC.Diags.LogOverrideAll _
                        Or clsAPC.Diags.LogOverrideErrorsOnly)
            End Function

            ''' <summary>
            ''' Compounds the given diags into the current state of the resource config
            ''' for all config options.
            ''' </summary>
            Friend Sub CompoundLoggingStates(diags As clsAPC.Diags, logToEventLog As Boolean)

                LoggingDefault = CompoundState(LoggingDefault,
                                            ShouldApplyDefaultLogging(diags))
                LoggingAllOverride = CompoundState(LoggingAllOverride,
                                            diags.HasFlag(clsAPC.Diags.LogOverrideAll))
                LoggingKeyOverride = CompoundState(LoggingKeyOverride,
                                            diags.HasFlag(clsAPC.Diags.LogOverrideKey))
                LoggingErrorsOnlyOverride = CompoundState(LoggingErrorsOnlyOverride,
                                            diags.HasFlag(clsAPC.Diags.LogOverrideErrorsOnly))
                LoggingMemory = CompoundState(LoggingMemory,
                                            diags.HasFlag(clsAPC.Diags.LogMemory))
                LoggingForceGC = CompoundState(LoggingForceGC,
                                            diags.HasFlag(clsAPC.Diags.ForceGC))
                LoggingToEventLog = CompoundState(LoggingToEventLog, logToEventLog)
                LoggingWebServices = CompoundState(LoggingWebServices,
                                            diags.HasFlag(clsAPC.Diags.LogWebServices))
            End Sub
        End Class

        ''' <summary>
        ''' Constructor for an instance that is owned by a clsResourceConnection. This
        ''' kind of instance represents a real live connection!
        ''' </summary>
        ''' <param name="owningConnection">The clsResoureConnection that owns this
        ''' instance.</param>
        ''' <param name="dbstatus">The Resource's database status</param>
        ''' <param name="name">The Resource's name</param>
        ''' <param name="id">The ID of the Resource</param>
        ''' <param name="attribs">The attributes of the Resource</param>
        Friend Sub New(ByVal owningConnection As IResourceConnection, ByVal dbstatus As ResourceDBStatus, ByVal name As String, ByVal id As Guid, ByVal attribs As ResourceAttribute)
            Me.New(owningConnection, dbstatus, name, id, attribs, Guid.Empty)
        End Sub

        ''' <summary>
        ''' Constructor for an instance that contains a static representation of a
        ''' connection state. This kind of instance is NOT owned by a
        ''' clsResourceConnection.
        ''' </summary>
        ''' <param name="constate">The status of the connection to the resource</param>
        ''' <param name="name">The Resource's name</param>
        ''' <param name="ID">The ID of the Resource</param>
        ''' <param name="attribs">The attributes of the Resource</param>
        Public Sub New(ByVal constate As ResourceConnectionState, ByVal name As String, ByVal ID As Guid, ByVal attribs As ResourceAttribute)
            Me.New(Nothing, ResourceDBStatus.Unknown, name, ID, attribs, Guid.Empty)

            mConnectionState = constate
        End Sub

        Friend Sub New(ByVal owningConnection As IResourceConnection, ByVal dbstatus As ResourceDBStatus, ByVal name As String, ByVal id As Guid, ByVal attribs As ResourceAttribute, ByVal userId As Guid)

            mConnection = owningConnection
            Me.Name = name
            Me.Id = id
            mAttributes = attribs
            Me.DBStatus = dbstatus
            Me.UserID = userId
        End Sub

        ''' <summary>
        ''' The status of this resource according to the database.
        ''' Corresponds to the column Status in the BPAResource table.
        ''' </summary>
        <DataContract([Namespace]:="bp")>
        Public Enum ResourceDBStatus
            <EnumMember(Value:="U")> Unknown = 0
            <EnumMember(Value:="R")> Ready = 1
            <EnumMember(Value:="O")> Offline = 2
            <EnumMember(Value:="P")> Pending = 3
        End Enum

        ''' <summary>
        ''' The name of this resource
        ''' </summary>
        ''' <value>The Name.</value>
        Public Property Name As String Implements IResourceMachine.Name

        ''' <summary>
        ''' The ID of this resource
        ''' </summary>
        Public Property Id As Guid Implements IResourceMachine.Id

        ''' <summary>
        ''' The number of child resources this resource machine has. This can only be
        ''' relevant if the resource is a Pool. Otherwise, the value will be 0.
        ''' </summary>
        Public ReadOnly Property ChildResourceCount As Integer Implements IResourceMachine.ChildResourceCount
            Get
                Return If(ChildResources?.Count(), 0)
            End Get
        End Property

        ''' <summary>
        ''' A List of the names of child resources.
        ''' </summary>
        Public ReadOnly Property ChildResourceNames As List(Of String) Implements IResourceMachine.ChildResourceNames
            Get
                Return Me.ChildResources?.Select(Function(c) c.Name).ToList()
            End Get
        End Property


        ''' <summary>
        ''' Details of children in the pool.
        ''' </summary>
        Public Property ChildResources As List(Of IResourceMachine) Implements IResourceMachine.ChildResources

        ''' <summary>
        ''' Holds and informational string pertinent to the resource
        ''' </summary>
        Public Property Info As String Implements IResourceMachine.Info

        ''' <summary>
        ''' Holds the appropriate display state for the resource
        ''' </summary>
        Public Property DisplayStatus As ResourceStatus Implements IResourceMachine.DisplayStatus

        ''' <summary>
        ''' Holds a setting for the colour of the info text
        ''' </summary>
        Public Property InfoColour As Color Implements IResourceMachine.InfoColour

        ''' <summary>
        ''' Holds whether or not this resource is in a pool 
        ''' </summary>
        Public Property IsInPool As Boolean Implements IResourceMachine.IsInPool

        ''' <summary>
        ''' Gets the last connection error encountered on this Resource.
        ''' </summary>
        Public Property LastError As String Implements IResourceMachine.LastError
            Get
                Return mLastError
            End Get
            Set(value As String)
                mLastError = value
            End Set
        End Property
        Private mLastError As String = ""

        ''' <summary>
        ''' The number of processes the remote Resource PC is currently running. Known
        ''' only if the machine has an active connection - otherwise 0 will be returned.
        ''' </summary>
        Public ReadOnly Property ProcessesRunning As Integer Implements IResourceMachine.ProcessesRunning
            Get
                If mConnection Is Nothing Then Return 0
                Return mConnection.ProcessesRunning
            End Get
        End Property

        ''' <summary>
        ''' The number of processes the remote Resource PC has pending.Known only if the
        ''' machine has an active connection - otherwise 0 will be returned.
        ''' </summary>
        Public ReadOnly Property ProcessesPending As Integer Implements IResourceMachine.ProcessesPending
            Get
                If mConnection Is Nothing Then Return 0
                Return mConnection.ProcessesPending
            End Get
        End Property

        ''' <summary>
        ''' Gets the sessions registered on the resource machine represented by this
        ''' object
        ''' </summary>
        ''' <returns>A map of session statuses against the IDs of the sessions that are
        ''' registered on this resource machine</returns>
        Friend Function GetSessions() As IDictionary(Of Guid, RunnerStatus)
            If mConnection Is Nothing Then _
         Return GetEmpty.IDictionary(Of Guid, RunnerStatus)()

            Return mConnection.GetSessions()
        End Function

        ''' <summary>
        ''' Determine whether this Resource has a pool member with the given name. It
        ''' may not be a pool at all, in which case the answer is obviously no.
        ''' </summary>
        ''' <param name="name">The name of the member to check for.</param>
        ''' <returns>True if the Resource is a Pool, and has a member with the given
        ''' name. False otherwise.</returns>
        Public Function HasPoolMember(ByVal name As String) As Boolean Implements IResourceMachine.HasPoolMember
            If ChildResourceCount = 0 Then Return False
            Return ChildResourceNames.Contains(name)
        End Function

        Public Function ContainsResource(resourceid As Guid) As Boolean
            Return mConnection.ResourceId = resourceid
        End Function

        ''' <summary>
        ''' True if this resource is operating in local machine only mode.
        ''' </summary>
        Public ReadOnly Property Local As Boolean Implements IResourceMachine.Local
            Get
                Return (mAttributes And ResourceAttribute.Local) <> 0
            End Get
        End Property

        ''' <summary>
        ''' The attributes possesed by this resource.
        ''' </summary>
        Public Property Attributes As ResourceAttribute Implements IResourceMachine.Attributes
            Get
                Return mAttributes
            End Get
            Set(ByVal value As ResourceAttribute)

                If value = mAttributes Then Return ' Shortcut - no work required...

                Dim wasRetired As Boolean = (Me.mAttributes And ResourceAttribute.Retired) > 0
                mAttributes = value
                If mConnection Is Nothing Then Exit Property
                Dim isNowRetired As Boolean = (Me.mAttributes And ResourceAttribute.Retired) > 0

                If isNowRetired AndAlso (Not wasRetired) AndAlso Me.mConnection IsNot Nothing Then
                    Me.mConnection.Terminate()
                End If

                'just changed from retired non-retired so revive connection
                If (Not isNowRetired) AndAlso wasRetired AndAlso Me.mConnection IsNot Nothing Then
                    'wake up the resource
                    mConnection.RefreshResource()
                End If

                RaiseEvent AttributesChanged(Me, EventArgs.Empty)
            End Set
        End Property
        Private mAttributes As ResourceAttribute

        ''' <summary>
        ''' Checks if this resource machine has the attributes specified.
        ''' </summary>
        ''' <param name="attr">The attributes to check for in this machine.</param>
        ''' <returns>True if this resource machine has all of the attributes specified in
        ''' the argument; False otherwise.</returns>
        Public Function HasAttribute(attr As ResourceAttribute) As Boolean Implements IResourceMachine.HasAttribute
            Return Attributes.HasFlag(attr)
        End Function

        ''' <summary>
        ''' Checks if this resource machine has any of the attributes specified.
        ''' </summary>
        ''' <param name="attr">The attributes to check for in this machine.</param>
        ''' <returns>True if this resource machine has any of the attributes specified in
        ''' the argument; False otherwise.</returns>
        Public Function HasAnyAttribute(attr As ResourceAttribute) As Boolean Implements IResourceMachine.HasAnyAttribute
            Return Attributes.HasAnyFlag(attr)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        Public Property CapabilitiesFriendly As String() Implements IResourceMachine.CapabilitiesFriendly

        ''' <summary>
        ''' The connection owning this resource object, if any.
        ''' </summary>
        Private mConnection As IResourceConnection

        ''' <summary>
        ''' Returns the connection status of the machine. This may either be pre-stored,
        ''' or live from the associated connection if there is one.
        ''' </summary>
        Public ReadOnly Property ConnectionState As ResourceConnectionState Implements IResourceMachine.ConnectionState
            Get
                If Not mConnection Is Nothing Then
                    Return mConnection.ConnectionState
                Else
                    Return mConnectionState
                End If
            End Get
        End Property
        Private mConnectionState As ResourceConnectionState = ResourceConnectionState.Disconnected

        ''' <summary>
        ''' Checks if this machine object is connected, according to its
        ''' <see cref="ConnectionState"/>
        ''' </summary>
        Public Overridable ReadOnly Property IsConnected As Boolean Implements IResourceMachine.IsConnected
            Get
                Return ConnectionState = ResourceConnectionState.Connected
            End Get
        End Property

        ''' <summary>
        ''' The DB status of this resource.
        ''' </summary>
        Public Property DBStatus As ResourceDBStatus Implements IResourceMachine.DBStatus
            Get
                Return mDbStatus
            End Get
            Set(value As ResourceDBStatus)
                Dim changing = value <> mDbStatus
                mDbStatus = value
                If changing Then
                    RaiseEvent DbStatusChanged(Me, EventArgs.Empty)
                End If
            End Set
        End Property
        Private mDbStatus As ResourceDBStatus

        ''' <summary>
        ''' Determines of the resource machine is a controller.
        ''' </summary>
        Public Property IsController As Boolean Implements IResourceMachine.IsController

        Public Property UserID() As Guid Implements IResourceMachine.UserID

        ''' <summary>
        ''' Converts dbstatus to string suitable for display on screen.
        ''' </summary>
        ''' <param name="DBStatus">The status</param>
        ''' <returns>A string representing the status.</returns>
        Public Shared Function GetDBStatusFriendlyName(ByVal DBStatus As ResourceDBStatus) As String
            Select Case DBStatus
                Case ResourceDBStatus.Offline
                    Return My.Resources.clsResourceMachine_Offline
                Case ResourceDBStatus.Ready
                    Return My.Resources.clsResourceMachine_Ready
                Case ResourceDBStatus.Pending
                    Return My.Resources.clsResourceMachine_Pending
                Case ResourceDBStatus.Unknown
                    Return My.Resources.clsResourceMachine_Unknown
                Case Else
                    Return My.Resources.clsResourceMachine_InternalErrorStatusMissing
            End Select
        End Function


        Public Function RefreshResourceConnection() As Task(Of Boolean) Implements IResourceMachine.RefreshResourceConnection
            Return mConnection.RefreshResource()
        End Function

        ''' <summary>
        ''' Awaits a connection becoming available on this connection, blocking the
        ''' current thread until one enters a 'valid' state.
        ''' This treats a connection in a state of "InUse" as a valid connection.
        ''' This will wait indefinitely for a 'connecting' connection to become valid.
        ''' </summary>
        ''' <param name="resultantState">The last state found before this method
        ''' returned. This will normally be a more fine-grained state than 'valid' or
        ''' 'not valid' so that calling methods can handle the states in a more specific
        ''' way.</param>
        ''' <returns>True if the underlying connection was valid ie. Connected or InUse;
        ''' False if it was invalid - ie. the thread is not alive or the connection state
        ''' becomes Error, Offline or Unavailable
        ''' </returns>
        Public Function AwaitValidConnection(
     ByVal timeoutMillis As Integer,
     ByVal inuseIsValid As Boolean,
     ByRef resultantState As ResourceConnectionState) As Boolean Implements IResourceMachine.AwaitValidConnection

            Dim result = mConnection.AwaitValidConnection(timeoutMillis, inuseIsValid, resultantState)
            Return result

        End Function


    End Class

End Namespace
