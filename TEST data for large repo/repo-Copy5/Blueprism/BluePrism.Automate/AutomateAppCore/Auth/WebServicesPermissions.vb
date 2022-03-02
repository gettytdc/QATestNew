Imports BluePrism.AutomateProcessCore.Processes

Namespace Auth

    Public Class WebServicesPermissions

        Private ReadOnly mServer As IServer = Nothing

        Public Sub New(server As IServer)
            mServer = server
        End Sub

        Public Function CanExecuteWebService(
                processType As DiagramType,
                authenticatedUser As IUser,
                processId As Guid) As (Success As Boolean, ErrorMessage As String)

            If authenticatedUser Is Nothing Then Return (False, My.Resources.WebServicesPermissions_InvalidOperation)

            Dim requiredPermission =
                    If(processType = DiagramType.Process,
                       Permission.ProcessStudio.ExecuteProcessAsWebService,
                       Permission.ObjectStudio.ExecuteBusinessObjectAsWebService)

            Dim hasPermission =
                    mServer.GetEffectiveMemberPermissionsForProcess(processId).
                    HasPermission(authenticatedUser, requiredPermission)

            If Not hasPermission Then
                Return (False, My.Resources.WebServicesPermissions_YouDoNotHavePermissionToExecuteThisProcessAsAWebService)
            End If

            Return (True, String.Empty)
        End Function

    End Class

End Namespace