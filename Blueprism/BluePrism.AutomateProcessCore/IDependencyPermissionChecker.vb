
''' <summary>
''' Performs permission checks on a processes dependencies
''' </summary>
Public Interface IDependencyPermissionChecker

    ''' <summary>
    ''' Test for any dependencies of the process the current user does not have permission to access, which could cause execution of the process to fail.
    ''' </summary>
    ''' <param name="process">The process to check</param>
    ''' <returns>A dictionary containing invalid references, where: 
    ''' The key is the ID of the referencing stage, and the value is a list of processes names reference directly or indirectly by the stage, 
    ''' which contain items the user doesn't have permission to use.</returns>
    Function GetInaccessibleDependenciesInProcessDependencyTree(process As clsProcess) As Dictionary(Of Guid, ICollection(Of String))

End Interface
