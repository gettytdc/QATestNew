Imports BluePrism.AutomateProcessCore

''' <summary>
''' Performs permission checks on a processes dependencies
''' </summary>
Public Class DependencyPermissionChecker
    Implements IDependencyPermissionChecker

    Private mServer As IServer

    Public Sub New(server As IServer)
        mServer = server
    End Sub

    ''' <summary>
    ''' Test for any dependencies of the process the current user does not have permission to access, which could cause execution of the process to fail.
    ''' </summary>
    ''' <param name="process">The process to check</param>
    ''' <returns>A dictionary containing invalid references, where: 
    ''' The key is the ID of the referencing stage, and the value is a list of processes names referenced directly or indirectly by the stage, 
    ''' which contain items the user doesn't have permission to use.</returns>
    Public Function GetInaccessibleDependenciesInProcessDependencyTree(process As clsProcess) As Dictionary(Of Guid, ICollection(Of String)) _
        Implements IDependencyPermissionChecker.GetInaccessibleDependenciesInProcessDependencyTree

        ' Get external dependencies of the process. Only interested in dependencies 
        ' which are processes or vbos, since they are the only type which may have dependencies of their own that we need to check user permissions on.
        Dim dependencies = process.GetDependencies(False)
        Dim vboDependencies = dependencies.Dependencies.OfType(Of clsProcessNameDependency) ' VBO dependencies referenced through Action stages.
        Dim processDependencies = dependencies.Dependencies.OfType(Of clsProcessIDDependency) ' Process dependencies referenced though Process stages.


        Dim results = New Dictionary(Of Guid, ICollection(Of String))

        '' Check the dependencies of the vbos referenced through action stages - refactored to single server hit
        Dim depedencyLookUp = vboDependencies.ToDictionary(Of String, clsProcessNameDependency)(Function(x) x.RefProcessName, Function(x) x)
        Dim badReferences = gSv.GetInaccessibleReferencesByProcessNames(depedencyLookUp.Keys.ToList())

        For Each processName In badReferences.Keys
            Dim dependancy = depedencyLookUp(processName)
            results(dependancy.StageIDs.First) = CType(badReferences(processName), ICollection(Of String))
        Next

        ' Check the dependencies of the processes referenced through process stages.
        For Each processDependency In processDependencies
            ' Get the names of objects referenced by the vbo which contain references to items the user cannot use.
            Dim inaccessibleRefs = gSv.GetInaccessibleReferencesByProcessID(processDependency.RefProcessID)
            If inaccessibleRefs.Any() Then
                results(processDependency.StageIDs.First) = inaccessibleRefs
            End If
        Next

        Return results
    End Function
End Class
