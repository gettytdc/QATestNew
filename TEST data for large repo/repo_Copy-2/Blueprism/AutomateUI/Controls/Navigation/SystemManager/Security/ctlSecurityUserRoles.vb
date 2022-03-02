Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

''' <summary>
''' Panel used to display the user roles in the 'Security' section of System Manager.
''' </summary>
Public Class ctlSecurityUserRoles
    Implements IHelp, IStubbornChild, IPermission

    Private mUnsavedPromptActioned As Boolean = False

    ''' <summary>
    ''' The application form which ultimately 'owns' this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm

    ''' <summary>
    ''' The permission(s) required to view this control.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.Security.UserRoles)
        End Get
    End Property

    ''' <summary>
    ''' Handles the 'Apply' button being clicked
    ''' </summary>
    Private Sub HandleApply(sender As Object, e As EventArgs) Handles btnApply.Click
        SaveChanges()
    End Sub

    ''' <summary>
    ''' Saves the changes made in this form to the database and ensures that the
    ''' current system roles are updated with those changes.
    ''' </summary>
    ''' <returns>True on success; False on failure (after a message has been
    ''' displayed to the user)</returns>
    Private Function SaveChanges() As Boolean
        Try
            ' Commit the changes
            Dim roles As RoleSet = gSv.UpdateRoles(ctlRoles.Roles)

            ' And force an update of the system roles
            SystemRoleSet.SystemCurrent.Poll()

            ' Synchronize users if SSO enabled
            If User.IsLoggedInto(DatabaseType.SingleSignOn) AndAlso
             User.Current.HasPermission(Permission.SystemManager.Security.Users) Then
                Try
                    Dim msg = RefreshADUserListMessageBuilder.Build(gSv.RefreshADUserList())

                    If msg IsNot Nothing Then UserMessage.Show(msg)

                Catch ex As Exception
                    UserMessage.Err(ex,
                     My.Resources.ctlSecurityUserRoles_ErrorSynchronizingActiveDirectoryUsers0, ex.Message)
                    Return False
                End Try
            End If

            Return True

        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ctlSecurityUserRoles_AnErrorOccurredWhileApplyingTheChangesToTheUserRoles0,
             ex.Message)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Gets the help file which documents the usage of this control
    ''' </summary>
    ''' <returns>The path to the help file in the compiled held which details this
    ''' control.</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpUserPermissions.htm"
    End Function

    ''' <summary>
    ''' Checks if it is safe to leave this control. This checks to see if any roles
    ''' have changed, prompting the user to see how to deal with the situation where
    ''' they have changed the roles in some way, but not yet saved them.
    ''' </summary>
    ''' <returns>True if the user is happy to leave this control; False if it should
    ''' remain open on this control, refusing to move away to another view.
    ''' </returns>
    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave

        If mUnsavedPromptActioned Then Return True

        ' Check the roles in this control against the system roles
        Dim sysRoles = SystemRoleSet.Current
        Dim displayedRoles = ctlRoles.Roles

        ' If they're the same, this control is safe to leave
        If displayedRoles.Equals(sysRoles) Then Return True

        ' A report of the changes might be nice, but RoleSet.GetChangeReport() is
        ' not geared toward a user message box at the moment and would require quite
        ' a bit of rework to make it work in such a context. So for now... vagueness
        Dim msg As String =
         My.Resources.ctlSecurityUserRoles_UserMessage_ThereAreUnsavedChangesToTheUserRolesDoYouWantToSaveTheChanges

        ' And see what they want to do
        Dim resp = UserMessage.YesNoCancel(msg, True)
        Dim result As Boolean
        Select Case resp
            Case MsgBoxResult.No : result = True
            Case MsgBoxResult.Yes : result = SaveChanges()
            Case Else : result = False
        End Select

        mUnsavedPromptActioned = result

        Return result

    End Function

End Class
