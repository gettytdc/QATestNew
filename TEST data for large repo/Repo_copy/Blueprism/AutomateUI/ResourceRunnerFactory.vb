Imports BluePrism
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.DigitalWorker

Public Class ResourceRunnerFactory
    Implements IResourceRunnerFactory

    Public Function Create(options As IResourceRunnerStartUpOptions) As ResourceRunnerComponents Implements IResourceRunnerFactory.Create

        Dim view As IResourcePCView
        Dim runner As IResourceRunner

        Dim digitalWorkerOptions = TryCast(options, DigitalWorkerStartUpOptions)
        Dim resourcePCOptions = TryCast(options, ResourcePCStartUpOptions)

        If digitalWorkerOptions IsNot Nothing
            view = New frmDigitalWorker(digitalWorkerOptions)
            Dim factory = DependencyResolver.Resolve(Of Func(Of IResourcePCView, DigitalWorker))
            runner = factory(view)
        ElseIf resourcePCOptions IsNot Nothing Then
            view = New frmResourcePC(resourcePCOptions)
            runner = New ListenerResourceRunner(resourcePCOptions, view)
        Else
            Throw New ArgumentException("Unsupported IResourceRunnerStartUpOptions type", NameOf(options))
        End If

        Return New ResourceRunnerComponents(view, runner)
    End Function

End Class