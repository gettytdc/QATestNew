Imports System.Reflection

Namespace Commands

    Public Class CommandFactory : Implements ICommandFactory

        Private Shared ReadOnly CommandFactories As IReadOnlyCollection(Of Func(Of IListenerClient, IListener, IServer, Func(Of IGroupPermissions, IMemberPermissions), ICommand))

        Private ReadOnly mClient As ListenerClient
        Private ReadOnly mListener As clsListener

        ''' <summary>
        ''' A dictionary of Command objects which contains all the available telnet commands.
        ''' </summary>
        Public ReadOnly Property Commands As IReadOnlyDictionary(Of String, ICommand) Implements ICommandFactory.Commands

        ''' <summary>
        ''' Shared constructor - initialises the telnet command list.
        ''' </summary>
        Shared Sub New()

            CommandFactories =
                Assembly.GetExecutingAssembly().
                    GetTypes().
                    Where(Function(x) Not x.IsAbstract).
                    Where(AddressOf GetType(CommandBase).IsAssignableFrom).
                    Select(Function(x) x.GetConstructor({GetType(IListenerClient), GetType(IListener), GetType(IServer), GetType(Func(Of IGroupPermissions, IMemberPermissions))})).
                    Select(Function(x) DirectCast(
                        Function(
                              client As IListenerClient,
                              listener As IListener,
                              server As IServer,
                              memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions)) _
                        DirectCast(x.Invoke({client, listener, server, memberPermissionsFactory}), ICommand),
                        Func(Of IListenerClient, IListener, IServer, Func(Of IGroupPermissions, IMemberPermissions), ICommand))).
                    ToList()

        End Sub

        Public Sub New(client As ListenerClient, listener As clsListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))

            mClient = client
            mListener = listener

            Commands =
                CommandFactories.
                    Select(Function(x) x(client, listener, server, memberPermissionsFactory)).
                    ToDictionary(Function(k) k.Name, Function(v) v)

        End Sub

        ''' <summary>
        ''' Execute a command
        ''' </summary>
        ''' <param name="commandString">The command text.</param>
        ''' <returns>The output of the command</returns>
        Public Function ExecCommand(commandString As String) _
            As (output as String, clientsToRemove As IReadOnlyCollection(Of IListenerClient)) _
            Implements ICommandFactory.ExecCommand

            If commandString = "help" Then
                Return (ProcessHelpCommand(), {})
            ElseIf commandString.StartsWith("help ") Then
                Return (ProcessHelpTopicCommand(commandString.Substring(5)), {})
            Else
                Return ProcessExternalCommand(commandString)
            End If

        End Function

        Private Function ProcessHelpCommand() As String
            Dim output As String = My.Resources.CommandFactory_AvailableCommands & vbCrLf & vbCrLf
            output &= " help            "
            Dim column = 1
            For Each key As String In Commands.Keys
                If column = 4 Then
                    column = 0
                    output &= vbCrLf & " "
                End If
                Dim name As String = Commands.Item(key).Name
                output &= name
                output &= New String(" "c, 16 - name.Length)
                column += 1
            Next
            output &= vbCrLf & vbCrLf & My.Resources.CommandFactory_UseHelpCommandForMoreInformation & vbCrLf
            Return output
        End Function

        Private Function ProcessHelpTopicCommand(command As String) As String

            Return _
                If(Commands.ContainsKey(command),
                   My.Resources.CommandFactory_Command & command.ToLower() & vbCrLf & Commands(command).Help & vbCrLf,
                   My.Resources.CommandFactory_ThereIsNoSuchCommand & vbCrLf)

        End Function

        Private Function ProcessExternalCommand(commandString As String) As (String, IReadOnlyCollection(Of IListenerClient))

            Dim command As ICommand

            Try
                Dim verb = commandString
                Dim firstSpaceIndex = verb.IndexOf(" ", StringComparison.Ordinal)
                If firstSpaceIndex <> -1 Then
                    verb = verb.Substring(0, firstSpaceIndex)
                End If
                command = Commands.Item(verb)

                If command.CommandAuthenticationRequired <> CommandAuthenticationMode.Any Then
                    If Not (command.CommandAuthenticationRequired = CommandAuthenticationMode.AuthedOrLocal AndAlso mClient.IsLocal()) Then
                        If Not mClient.UserSet Then
                            Return ("USER NOT SET" & vbCrLf, New IListenerClient() {})
                        End If

                        Dim perm = command.CheckPermissions(mClient.AuthenticatedUser, mListener.ResourceId)
                        If Not perm.allowed Then
                            Return (perm.errorMessage & vbCrLf, New IListenerClient() {})
                        End If
                    End If
                End If

                If Not command.ValidRunStates.Contains(mListener.RunState) Then
                    Return ("COMMAND NOT VALID AT THIS TIME - RESOURCE IS " + GetRunStateDescription(mListener.RunState).ToUpper() & vbCrLf, New ListenerClient() {})
                End If
                Dim result = command.Execute(commandString)
                ' Ensure that we finish on a newline
                If Not result.output.EndsWith(vbCrLf) Then result.output &= vbCrLf
                Return result
            Catch e As Exception
                Return ("INVALID COMMAND - use 'help'" & vbCrLf, New IListenerClient() {})
            End Try

        End Function

        ''' <summary>
        ''' Gets description of a ResourcePcRunState value in format suitable for 
        ''' use in response to client
        ''' </summary>
        Private Function GetRunStateDescription(runState As ResourcePcRunState) As String
            Select Case runState

                Case ResourcePcRunState.WaitingToStop
                    Return My.Resources.CommandFactory_StoppingWaitingForRunningSessionsToComplete
                Case Else
                    Return [Enum].GetName(GetType(ResourcePcRunState), runState)

            End Select



        End Function

    End Class
End Namespace