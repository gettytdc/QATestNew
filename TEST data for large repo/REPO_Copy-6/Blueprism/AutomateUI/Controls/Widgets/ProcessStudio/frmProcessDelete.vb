Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Wizard used to delete a process
''' </summary>
Friend Class frmProcessDelete

    ' The group member to be deleted
    Private mDeleteTarget As ProcessBackedGroupMember

    ''' <summary>
    ''' Creates a new delete form to delete the given group member
    ''' </summary>
    ''' <param name="mem">The group member to delete</param>
    Public Sub New(mem As ProcessBackedGroupMember)

        ' This call is required by the designer.
        InitializeComponent()

        ' Set the delete target - anything other than a process-backed member
        ' will cause this to fail (and, indeed, so it should)
        mDeleteTarget = mem

        ' Add any initialization after the InitializeComponent() call.
        SetMaxSteps(0)

    End Sub

    ''' <summary>
    ''' Handles the page preparation and execution
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        Select Case GetStep()
            Case 0
                If mDeleteTarget Is Nothing Then
                    Me.Title = My.Resources.DeleteAProcessObject
                Else
                    Dim friendlyName As String

                    Select Case mDeleteTarget.MemberType
                        Case GroupMemberType.Object
                            friendlyName = My.Resources.ProcessType_businessobjectL
                        Case GroupMemberType.Process
                            friendlyName = My.Resources.ProcessType_processL
                        Case Else
                            friendlyName = mDeleteTarget.MemberType.GetLocalizedFriendlyName().ToLower()
                    End Select

                    Me.Title = String.Format(
                        My.Resources.DeleteThe01, friendlyName, mDeleteTarget.Name)
                End If
                'Display warning if the process/object is referenced elsewhere
                Try
                    Dim dep As clsProcessDependency
                    If mDeleteTarget.MemberType = GroupMemberType.Object Then
                        dep = New clsProcessNameDependency(mDeleteTarget.Name)
                    Else
                        dep = New clsProcessIDDependency(mDeleteTarget.IdAsGuid)
                    End If
                    If gSv.IsReferenced(dep) Then
                        pbWarning.Visible = True
                        lblWarning.Visible = True
                    End If
                Catch ex As Exception
                    UserMessage.Err(ex,
                     My.Resources.AnErrorOccurredWhileRetrievingReferences, ex.Message)
                End Try
            Case 1
                If mDeleteTarget Is Nothing Then Return
                Try

                    If (String.IsNullOrEmpty(gSv.GetProcessNameByID(mDeleteTarget.IdAsGuid))) Then
                        UserMessage.OK(My.Resources.ProcessAlreadyDeleted)
                        Me.DialogResult = DialogResult.OK
                        Close()
                    Else
                        ' do the deletion and record it in audit log
                        gSv.DeleteProcess(mDeleteTarget.IdAsGuid, txtDeleteReason.Text)
                        Me.DialogResult = DialogResult.OK
                        Close()
                    End If

                Catch aofe As AuditOperationFailedException
                    UserMessage.Err(aofe, aofe.Message)

                Catch ex As Exception
                    UserMessage.Err(ex,
                     My.Resources.AnErrorOccurredWhileDeletingTheProcess0, ex.Message)

                End Try

        End Select

    End Sub
End Class
