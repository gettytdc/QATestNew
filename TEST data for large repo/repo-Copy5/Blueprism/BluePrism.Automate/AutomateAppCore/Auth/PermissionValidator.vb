Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

Public Class PermissionValidator : Implements IPermissionValidator

    Public Sub EnsurePermissions(context As ServerPermissionsContext) Implements IPermissionValidator.EnsurePermissions

        If context.AllowAnyLocalCalls AndAlso context.IsLocal Then Exit Sub

        If context.User Is Nothing Then _
            Throw New PermissionException(My.Resources.PermissionValidator_UnauthorizedUserNotLoggedIn)

        If context.User.AuthType = AuthMode.System Then Exit Sub

        If context.Permissions Is Nothing OrElse context.Permissions.Count = 0 Then Exit Sub

        If context.Permissions.Any(Function(p) context.User.HasPermission(p)) Then Exit Sub

        Throw New PermissionException(
            My.Resources.PermissionValidator_UnauthorizedUserDoesNotHavePermission & vbCrLf & My.Resources.PermissionValidator_Requires & vbCrLf & string.Join(" || ", context.Permissions))

    End Sub
End Class
