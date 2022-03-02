
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Data

Friend Class ProcessDependencyPermissionLogic

    Private ReadOnly mServer As IServerPrivate

    Public Sub New(server As IServerPrivate)
        mServer = server
    End Sub

    Public Function GetInaccessibleReferences(connection As IDatabaseConnection, processID As Guid, user As IUser,
                                              processIDLookup As Func(Of String, Guid)) As ICollection(Of String)

        ' Traverses the dependency tree, checking permissions on process dependencies.
        Dim processesToCheck = New Queue(Of Guid)
        Dim processesVisited = New List(Of Guid)

        Dim inaccessibleDependencies = New List(Of String)

        processesToCheck.Enqueue(processID)

        While processesToCheck.Any()

            Dim processIdToCheck = processesToCheck.Dequeue()

            For Each dependancy In mServer.GetExternalDependencies(connection, processIdToCheck).Dependencies

                ' Check if this dependency is accessible to the current user. If not add process to the list to be returned.
                If Not HasUserGotPermissionToUseDependency(connection, dependancy, user, processIDLookup) Then
                    inaccessibleDependencies.Add(mServer.GetProcessNameById(connection, processIdToCheck))
                    processesVisited.Add(processIdToCheck)
                    Continue While
                End If

                ' If this is a dependency on a process / object, add that to the processes to check queue
                ' (if we haven't seen it before)
                Dim referencedProcessID = GetReferencedProcess(dependancy, processIDLookup)
                If referencedProcessID <> Guid.Empty AndAlso
                    Not processesVisited.Contains(referencedProcessID) AndAlso
                    Not processesToCheck.Contains(referencedProcessID) Then
                    processesToCheck.Enqueue(referencedProcessID)
                End If

            Next

            processesVisited.Add(processIdToCheck)
        End While

        Return inaccessibleDependencies
    End Function

    ''' <summary>
    ''' Checks if the user has permission to use the item referenced by the dependency.
    ''' </summary>
    ''' <param name="dependency"></param>
    ''' <returns></returns>
    Private Function HasUserGotPermissionToUseDependency(connection As IDatabaseConnection, dependency As clsProcessDependency,
                                                         user As IUser, processIDLookup As Func(Of String, Guid)) As Boolean
        ' Just check process / object type dependencies for now, since we haven't yet enforced group permissions on other dependencies such as credentials / work queues etc.
        ' They will need to be checked here once group permissions are enforced.

        ' Test user has 'Execute' permission on any VBO dependencies.
        If dependency.GetType() Is GetType(clsProcessNameDependency) Then
            Dim processNameDependency = CType(dependency, clsProcessNameDependency)
            Dim referencedProcessId = processIDLookup(processNameDependency.RefProcessName)
            Dim perms = mServer.GetEffectiveMemberPermissionsForProcess(connection, referencedProcessId)
            Return Not perms.IsRestricted OrElse perms.HasPermission(user, Permission.ObjectStudio.ImpliedExecuteBusinessObject)
        End If

        ' Test user has 'Execute' permission on any Process dependencies.
        If dependency.GetType() Is GetType(clsProcessIDDependency) Then
            Dim processIdDependency = CType(dependency, clsProcessIDDependency)
            Dim referencedProcessId = processIdDependency.RefProcessID
            Dim perms = mServer.GetEffectiveMemberPermissionsForProcess(connection, referencedProcessId)
            Return Not perms.IsRestricted OrElse perms.HasPermission(user, Permission.ProcessStudio.ImpliedExecuteProcess)
        End If

        Return True
    End Function

    ''' <summary>
    ''' Returns the id of the process referenced by a dependency (Only clsProcessIDDependency or clsProcessNameDependency)
    ''' Returns Guid.Empty for other dependencies
    ''' </summary>
    ''' <param name="processDependency"></param>
    ''' <returns></returns>

    Private Function GetReferencedProcess(processDependency As clsProcessDependency, processIdLookup As Func(Of String, Guid)) As Guid

        Dim referencedProcessId = Guid.Empty

        Dim processIdDependency = TryCast(processDependency, clsProcessIDDependency)
        If processIdDependency IsNot Nothing Then
            referencedProcessId = processIdDependency.RefProcessID
        End If

        Dim processNameDependency = TryCast(processDependency, clsProcessNameDependency)
        If processNameDependency IsNot Nothing Then
            referencedProcessId = processIdLookup(processNameDependency.RefProcessName)
        End If

        Return referencedProcessId
    End Function
End Class
