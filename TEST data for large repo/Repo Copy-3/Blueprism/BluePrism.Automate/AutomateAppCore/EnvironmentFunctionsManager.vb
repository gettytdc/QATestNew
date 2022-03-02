Imports BluePrism.AutomateAppCore.EnvironmentFunctions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.DependencyInjection


''' <summary>
''' Registers environment functions defined in AutomateAppCore with APC. These
''' functions provide access to the runtime environment of the process making the
''' function call and are specific to the Automate runtime environment. All other 
''' functions are defined in APC.
''' </summary>
Public Class EnvironmentFunctionsManager

    Public Shared Sub Register(isDigitalWorker As Boolean)
        Dim functionFactories = DependencyResolver.Resolve(Of IEnumerable(Of Func(Of Boolean, EnvironmentFunction)))
        functionFactories.ToList().ForEach(Sub(factory) clsAPC.AddFunction(factory(isDigitalWorker)))
    End Sub
End Class
