Imports BluePrism.AutomateAppCore.Auth

Public Interface IGroupPermissions : Inherits IList(Of GroupLevelPermissions), ICloneable
    Property InheritedAncestorID As Guid
    ReadOnly Property IsRestricted As Boolean
    Property MemberId As Guid
    Property State As PermissionState
    Sub Merge(m As IGroupPermissions)
    Function HasPermission(u As IUser, perms As ICollection(Of Permission)) As Boolean
    Function HasPermission(u As IUser, perms As ICollection(Of String)) As Boolean
    Function HasPermission(u As IUser, ParamArray perms() As Permission) As Boolean
    Function HasPermission(u As IUser, ParamArray perms() As String) As Boolean
    Function GetGroupLevelPermission(id As Integer) As GroupLevelPermissions
End Interface
