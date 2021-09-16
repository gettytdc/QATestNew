Imports System.Runtime.Serialization
Namespace Auth

<Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class RefreshADUserList

        <DataMember>
        public Property DeactivatedUsers As IEnumerable(Of User)
        <DataMember>
        public Property ActivatedUsers As IEnumerable(Of User)
        <DataMember>
        public Property AddedUsers As IEnumerable(Of User)
        <DataMember>
        public Property RolesNotMapped As IEnumerable(Of String)
        <DataMember>
        public Property GroupErrors as String
    End Class
End Namespace
