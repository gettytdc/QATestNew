
''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessDebugHook
''' 
''' <summary>
''' A base class defining the interface for a process debug hook.
''' The application should derive its own process debug hook class from this one,
''' implementing the required functionality.
''' 
''' The debug hook is optional, only required if debugging support is needed.
''' </summary>
Public MustInherit Class clsProcessDebugHook

    ''' <summary>
    ''' This property is queried by APC to determine whether to use the ProcessDebugHook
    ''' method or not.
    ''' </summary>
    ''' <returns>True if ProcessDebugHook should be called when relevant</returns>
    Public MustOverride ReadOnly Property UseDebugHook() As Boolean

    ''' <summary>
    ''' This is called during execution when a subprocess (only a Process, not a
    ''' Business Object) is about to be prepared for execution. It gives the
    ''' application the opportunity to provide a clsProcess object for the subprocess
    ''' instead of letting APC load and create one.
    ''' If a process is provided, it must be reset to its starting state.
    ''' </summary>
    ''' <param name="procid">The ID of the process about to be created.</param>
    ''' <returns>A clsProcess object that APC should use for execution, or Nothing
    ''' if one is not available.</returns>
    Public MustOverride Function GetDebugSubprocess(ByVal procid As Guid) As clsProcess


    ''' <summary>
    ''' This is called during execution when an sub-process (either a real 'Process'
    ''' or a Business Object of course) is about to be executed.
    ''' </summary>
    ''' <param name="objProcess">The process to be executed.</param>
    ''' <param name="gProcessID">The local process ID of this process</param>
    ''' <param name="parentProcess">The parent process.</param>
    ''' <param name="loadedDate">The date/time the process was loaded.</param>
    ''' <param name="sErr">In the event of failure, contains an error description
    ''' </param>
    ''' <returns>True if successful, False otherwise</returns>
    Public MustOverride Function ProcessDebugHook(ByVal objProcess As clsProcess, ByVal gProcessID As Guid, ByVal parentProcess As clsProcess, ByVal loadedDate As DateTime, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Called when debugging has returned from a child process.
    ''' </summary>
    ''' <param name="proc">The child process that was executed.</param>
    ''' <param name="parentProc">The parent process now being returned to.</param>
    ''' <returns>True if the application has taken (or retained) ownership of the
    ''' child process object. It can only do this for a Process. Not, currently, for
    ''' a Business Object. False if it thinks APC owns it.</returns>
    Public MustOverride Function ProcessDebugReturn(ByVal proc As clsProcess, ByVal parentProc As clsProcess) As Boolean

End Class
