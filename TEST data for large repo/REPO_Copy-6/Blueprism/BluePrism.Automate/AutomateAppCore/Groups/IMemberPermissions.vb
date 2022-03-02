Imports BluePrism.AutomateAppCore.Auth

Public Interface IMemberPermissions
    ReadOnly Property IsEmpty As Boolean
    ReadOnly Property IsRestricted As Boolean
    Property State As PermissionState
    Function HasAnyPermissions(user As IUser) As Boolean
    Function HasPermission(u As IUser, perms As ICollection(Of Permission)) As Boolean
    Function HasPermission(u As IUser, perms As ICollection(Of String)) As Boolean
    Function HasPermission(u As IUser, ParamArray perms() As Permission) As Boolean
    Function HasPermission(u As IUser, ParamArray perms() As String) As Boolean
End Interface
