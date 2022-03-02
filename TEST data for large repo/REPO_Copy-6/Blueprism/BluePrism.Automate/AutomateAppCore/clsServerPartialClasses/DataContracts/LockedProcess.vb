Imports System.Runtime.Serialization

Namespace clsServerPartialClasses.DataContracts

    ''' <summary>
    ''' Contains information about a locked process
    ''' </summary>
    <DataContract([Namespace]:="bp")>
    <Serializable>
    Public Class LockedProcess
        ''' <summary>
        ''' The identifier
        ''' </summary>
        <DataMember>
        public Id As Guid

        ''' <summary>
        ''' The name
        ''' </summary>
        <DataMember>
        public Name As String

        ''' <summary>
        ''' The lock date
        ''' </summary>
        <DataMember>
        public LockDate As DateTime

        ''' <summary>
        ''' The username
        ''' </summary>
        <DataMember>
        public Username As String

        ''' <summary>
        ''' The machine name
        ''' </summary>
        <DataMember>
        public MachineName As String
    End Class
End NameSpace