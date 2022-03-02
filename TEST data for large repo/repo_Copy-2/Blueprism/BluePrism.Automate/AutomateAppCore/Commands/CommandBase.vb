Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

Namespace Commands
    ''' <summary>
    ''' Base class that deals with all the various commands executed by the listener.
    ''' </summary>
    Public MustInherit Class CommandBase
        Implements ICommand

        Protected ReadOnly Client As IListenerClient
        Protected ReadOnly Listener As IListener
        Protected ReadOnly Server As IServer

        Private ReadOnly mMemberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions)

        Protected Sub New(
                           client As IListenerClient,
                           listener As IListener,
                           server As IServer,
                           memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            Me.Client = client
            Me.Listener = listener
            Me.Server = server
            mMemberPermissionsFactory = memberPermissionsFactory
        End Sub

        ''' <summary>
        ''' The name of the command
        ''' </summary>
        Public MustOverride ReadOnly Property Name As String Implements ICommand.Name

        ''' <summary>
        ''' The help text for the command.
        ''' </summary>
        Public MustOverride ReadOnly Property Help As String Implements ICommand.Help

        ''' <summary>
        ''' The authentication level required to use this command.
        ''' </summary>
        Public MustOverride ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode Implements ICommand.CommandAuthenticationRequired

        ''' <summary>
        ''' Determines whether the user requires the Control Resource permission to execute this command. (Only if AuthMode is not equal to AuthMode.Any)
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property RequiresControlResourcePermission As Boolean Implements ICommand.RequiresControlResourcePermission
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Abstract method that forces the command to check if the user has permissions for this action
        ''' </summary>
        ''' <param name="authUser">User object that is requesting the action</param>
        ''' <param name="resourceId">Identity of the resource</param>
        ''' <returns>result and error text.</returns>
        Public Overridable Function CheckPermissions(authUser As IUser, resourceId As Guid) As (errorMessage As String, allowed As Boolean) _
                        Implements ICommand.CheckPermissions
            Return (String.Empty, True)
        End Function

        Public Function CheckUserHasPermissionOnResource(user As IUser, resourceId As Guid, permissions As String()) As Boolean

            ' Authen user is a system level user
            If user.AuthType = AuthMode.System Then Return True

            ' Get the permissions for the resource 
            Dim recPerms = Server.GetEffectiveGroupPermissionsForResource(resourceId)
            Dim memberPerms = mMemberPermissionsFactory(recPerms)

            Return memberPerms.HasPermission(user, permissions)

        End Function

        Public Function CheckUserHasPermissionOnResource(user As IUser, resourceId As Guid, permissions As String) As Boolean
            Return CheckUserHasPermissionOnResource(user, resourceId, New String() {permissions})
        End Function

        ''' <summary>
        ''' Shared value returned by base implementation of ValidRunStates.
        ''' </summary>
        Private Shared ReadOnly DefaultValidRunStates As IList(Of ResourcePcRunState) =
                                    New List(Of ResourcePcRunState)() From {ResourcePcRunState.Running}.AsReadOnly()

        ''' <summary>
        ''' Convenience field used for subclasses overriding ValidRunStates 
        ''' </summary>
        Protected Shared ReadOnly AllRunStates As IList(Of ResourcePcRunState) =
                                      (New List(Of ResourcePcRunState) From
            {ResourcePcRunState.Stopped,
            ResourcePcRunState.Running,
            ResourcePcRunState.WaitingToStop,
            ResourcePcRunState.Stopping}) _
            .AsReadOnly()

        ''' <summary>
        ''' Gets the run states for which this command is valid. The base class 
        ''' implementation defaults to Running state only as this applies to most 
        ''' commands, but property can be overriden to enable the command for 
        ''' different run states.
        ''' </summary>
        Public Overridable ReadOnly Property ValidRunStates As IEnumerable(Of ResourcePcRunState) Implements ICommand.ValidRunStates
            Get
                Return DefaultValidRunStates
            End Get
        End Property

        ''' <summary>
        ''' Gets the nearest GUID to the given stub ID, using a specified function
        ''' delegate.
        ''' </summary>
        ''' <param name="stub">The stub of the ID for which the nearest actual ID
        ''' is required.</param>
        ''' <param name="getter">A delegate to the method which will actually take
        ''' the stub and compare it to the required set of IDs.</param>
        ''' <returns>A GUID containing the nearest ID of the type required, or
        ''' <see cref="Guid.Empty"/> if the stub did not match any ID or if it was
        ''' ambiguous.</returns>
        Private Function GetNearestId(stub As String, getter As Func(Of String, Guid)) As Guid
            ' The string is a complete GUID
            ' FIXME: This doesn't even check if it's a valid session ID 
            If stub.Length = Guid.Empty.ToString().Length Then Return New Guid(stub)

            ' The GUID is incomplete, so get the best match from the database.
            Return getter(stub)

        End Function


        ''' <summary>
        ''' Get the session ID that matches the given 'stub' text. In other words,
        ''' the stub text is the user input, specifying as few characters as
        ''' necessary to uniquely identify the process.
        ''' </summary>
        ''' <param name="stub">The stub session ID that the full ID is required for.
        ''' </param>
        ''' <returns>The nearest session ID to the given stub, or
        ''' <see cref="Guid.Empty"/> if the stub didn't match any session ID, or was
        ''' ambiguous
        ''' </returns>
        ''' <exception cref="FormatException">If the format of the GUID is invalid.
        ''' </exception>
        Protected Function GetNearestSessionID(stub As String) As Guid
            Return GetNearestId(stub, AddressOf Server.GetCompleteSessionID)
        End Function

        ''' <summary>
        ''' Get the pool ID that matches the given 'stub' text. In other words, the
        ''' stub text is the user input, specifying as few characters as necessary to
        ''' uniquely identify the process.
        ''' </summary>
        ''' <param name="stub">The 'stub' text</param>
        ''' <returns>The nearest pool ID to the given stub, or
        ''' <see cref="Guid.Empty"/> if the stub didn't match any pool ID, or was
        ''' ambiguous
        ''' </returns>
        ''' <exception cref="FormatException">If the format of the GUID is invalid.
        ''' </exception>
        Protected Function GetNearestPoolID(stub As String) As Guid
            Return GetNearestId(stub, AddressOf Server.GetCompletePoolID)
        End Function

        ''' <summary>
        ''' Get the process ID that matches the given 'stub' text. In other words,
        ''' the stub text is the user input, specifying as few characters as
        ''' necessary to uniquely identify the process.
        ''' </summary>
        ''' <param name="stub">The 'stub' text</param>
        ''' <returns>The nearest process ID to the given stub, or
        ''' <see cref="Guid.Empty"/> if the stub didn't match any process ID, or was
        ''' ambiguous
        ''' </returns>
        ''' <exception cref="FormatException">If the format of the GUID is invalid.
        ''' </exception>
        Protected Function GetNearestProcID(stub As String) As Guid
            Return GetNearestId(stub, AddressOf Server.GetCompleteProcessID)
        End Function

        ''' <summary>
        ''' Get the user ID that matches the given 'stub' text. In
        ''' other words, the stub text is the user input, specifying
        ''' as few characters as necessary to uniquely identify the
        ''' user.
        ''' </summary>
        ''' <param name="stub">The initial part of the ID</param>
        ''' <returns>The nearest user ID to the given stub, or
        ''' <see cref="Guid.Empty"/> if the stub didn't match any user ID, or was
        ''' ambiguous
        ''' </returns>
        ''' <exception cref="FormatException">If the format of the GUID is invalid.
        ''' </exception>
        Protected Function GetNearestUserID(stub As String) As Guid
            Return GetNearestId(stub, AddressOf Server.GetCompleteUserID)
        End Function

        ''' <summary>
        ''' Get the Resource ID that matches the given 'stub' text. In
        ''' other words, the stub text is the user input, specifying
        ''' as few characters as necessary to uniquely identify the
        ''' id of the resource.
        ''' </summary>
        ''' <param name="stub">The short text supplied. We hope
        ''' to match this against an existing Resource ID from the
        ''' database.</param>
        ''' <returns>The nearest resource ID to the given stub, or
        ''' <see cref="Guid.Empty"/> if the stub didn't match any resource ID, or was
        ''' ambiguous
        ''' </returns>
        ''' <exception cref="FormatException">If the format of the GUID is invalid.
        ''' </exception>
        Protected Function GetNearestResourceID(stub As String) As Guid
            Return GetNearestId(stub, AddressOf Server.GetCompleteResourceID)
        End Function

        ''' <summary>
        ''' Execute the command.
        ''' </summary>
        ''' <param name="command">The command text.</param>
        ''' <returns>The output of the command</returns>
        Public Function Execute(command As String) As (output as String, clientsToRemove as IReadOnlyCollection(Of IListenerClient)) Implements ICommand.Execute
            Try
                Return ExecRemoveClients(command)
            Catch ex As Exception
                Return (ex.Message, New IListenerClient() {})
            End Try
        End Function

        ''' <summary>
        ''' Execute the command.
        ''' </summary>
        ''' <param name="command">The command text.</param>
        ''' <returns>The output of the command</returns>
        Protected Overridable Function Exec(command As String) As String

            ' Ensures that at least one of Exec or ExecRemoveClients is overridden
            Throw New NotImplementedException()

        End Function

        Protected Overridable Function ExecRemoveClients(command As String) _
            As (output as String, clientsToRemove as IReadOnlyCollection(Of IListenerClient))

            Return (Exec(command), New IListenerClient() {})

        End Function

        Protected Function ReturnMessageOnly(message As String) As (String, IReadOnlyCollection(Of IListenerClient))
            Return (message, New IListenerClient() {})
        End Function

    End Class
End Namespace
