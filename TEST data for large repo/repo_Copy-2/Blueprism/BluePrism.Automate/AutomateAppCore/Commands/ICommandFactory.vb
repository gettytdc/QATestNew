Namespace Commands

    Public Interface ICommandFactory
        ReadOnly Property Commands As IReadOnlyDictionary(Of String, ICommand)
        Function ExecCommand(commandString As String) As (output as String, clientsToRemove As IReadOnlyCollection(Of IListenerClient))
    End Interface
End NameSpace