Imports System.Runtime.Serialization

Namespace Resources

    <Serializable, DataContract([Namespace]:="bp")>
    Public Class DigitalWorkerStartUpOptions
        Implements IResourceRunnerStartUpOptions
        
        Public ReadOnly Property ResourceRunnerType As ResourceRunnerType = ResourceRunnerType.DigitalWorker _
            Implements IResourceRunnerStartUpOptions.ResourceRunnerType

        <DataMember>
        Public Property Name As DigitalWorkerName

    End Class

End Namespace
