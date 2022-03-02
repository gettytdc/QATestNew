Imports System.Runtime.Serialization

Namespace clsServerPartialClasses.DataContracts

    ''' <summary>
    ''' Contains the result of a request for locked processes
    ''' </summary>
    <DataContract([Namespace]:="bp")>
    <Serializable>
    Public Class LockedProcessesResult
        ''' <summary>
        ''' The locked processes
        ''' </summary>
        <DataMember>
        public LockedProcesses As List(Of LockedProcess) = New List(Of LockedProcess)()
    End Class
End NameSpace