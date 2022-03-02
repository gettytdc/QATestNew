Imports BluePrism.CharMatching
Imports BluePrism.ApplicationManager.AMI

Namespace ProcessLoading

    Public Enum CacheRefreshBehaviour
        CheckForUpdatesOnce
        CheckForUpdatesEveryTime
        NeverCheckForUpdates
    End Enum

    ''' <summary>
    ''' The interface for a process loader. The application should
    ''' implement its own process loader with the required
    ''' functionality.
    ''' 
    ''' A process loader is optional, but required for some advanced clsProcess
    ''' functionality, in particular the ability to run sub-processes.
    ''' </summary>
    Public Interface IProcessLoader
        Inherits IFontStore

        ''' <summary>
        ''' Get the process XML from the data source for a given process.
        ''' </summary>
        ''' <param name="gProcessID">The ID of the process</param>
        ''' <param name="sXML">On success, the process XML</param>
        ''' <param name="lastmod">The date/time the process was loaded.</param>
        ''' <param name="sErr">On failure, an error message</param>
        ''' <returns>True if successful, otherwise False</returns>
        Function GetProcessXML(gProcessID As Guid, ByRef sXML As String, ByRef lastmod As DateTime, ByRef sErr As String) As Boolean

        Function GetEffectiveRunMode(ByVal processId As Guid) As BusinessObjectRunMode

        ''' <summary>
        ''' Gets the attributes of the specified process from the database.
        ''' </summary>
        ''' <param name="gProcessID">ID of the process.</param>
        ''' <param name="Attributes">Attributes of the process.</param>
        ''' <param name="serr">Error message when return value is false.</param>
        ''' <returns>Returns true unless error occurs; otherwise false.</returns>
        Function GetProcessAtrributes(gProcessID As Guid, ByRef Attributes As ProcessAttributes, ByRef serr As String) As Boolean


        ''' <summary>
        ''' Get the full set environment variables.
        ''' </summary>
        ''' <returns>A Dictionary(Of String,clsArgument) containing all the known variables
        ''' and their values.</returns>
        ''' <remarks>This should always return a new Dictionaryu, which the caller then owns.
        ''' </remarks>
        Function GetEnvVars() As Dictionary(Of String, clsArgument)

        ''' <summary>
        ''' Get the full set environment variables with refresh from server
        ''' </summary>
        ''' <param name="freshFromDatabase"></param>
        ''' <returns></returns>
        Function GetEnvVars(freshFromDatabase As Boolean) As Dictionary(Of String, clsArgument)

        Function GetEnvVarSingle(name As String, updateCache As Boolean) As clsArgument


        Function GetAMIInfo() As clsGlobalInfo

        Property CacheBehaviour As CacheRefreshBehaviour

    End Interface

End Namespace
