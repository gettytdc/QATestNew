Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.DependencyInjection

''' Project  : AutomateAppCore
''' Class    : clsAppCore
''' 
''' <summary>
''' Core functionality for the AutomateAppCore project.
''' </summary>
Public Class clsAppCore
    ''' <summary>
    ''' Perform any initialisation required to configure APC to with with the
    ''' main application. This should be called once at startup before APC is
    ''' used.
    ''' </summary>
    ''' <param name="isDigitalWorker">Is the calling instance a digital worker (not a run time resource)</param>
    Public Shared Sub InitAPC(Optional isDigitalWorker As Boolean = False)
        clsAPC.ProcessLoader = New clsAutomateProcessLoader()
        clsAPC.ObjectLoader = DependencyResolver.Resolve(Of IObjectLoader)
        EnvironmentFunctionsManager.Register(isDigitalWorker)
    End Sub

End Class
