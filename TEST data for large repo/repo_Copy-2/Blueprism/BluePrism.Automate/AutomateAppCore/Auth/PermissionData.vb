Imports System.Runtime.Serialization

Namespace Auth

    ''' <summary>
    ''' Class to group the permission data together when initialising an environment
    ''' </summary>
    <DataContract([Namespace]:="bp")>
    <Serializable()> _
 Public Class PermissionData

        ' The permissions, keyed on their IDs
        <DataMember>
        Private mPerms As IDictionary(Of Integer, Permission)

        ' The permission groups, keyed on their IDs
        <DataMember>
        Public mGroups As IDictionary(Of Integer, PermissionGroup)

        ''' <summary>
        ''' Creates a new PermissionData object wrapping prebuilt permission data.
        ''' </summary>
        ''' <param name="perms">The permissions to contain in this data</param>
        ''' <param name="groups">The permission groups to contain in this data.
        ''' </param>
        Public Sub New( _
         ByVal perms As IDictionary(Of Integer, Permission), _
         ByVal groups As IDictionary(Of Integer, PermissionGroup))
            mPerms = perms
            mGroups = groups
        End Sub

        ''' <summary>
        ''' Gets the permissions held in this data, keyed on their IDs
        ''' </summary>
        Public ReadOnly Property Permissions() As IDictionary(Of Integer, Permission)
            Get
                Return mPerms
            End Get
        End Property

        ''' <summary>
        ''' Gets the permission groups held in this data, keyed on their IDs
        ''' </summary>
        Public ReadOnly Property PermissionGroups() As IDictionary(Of Integer, PermissionGroup)
            Get
                Return mGroups
            End Get
        End Property
    End Class

End Namespace
