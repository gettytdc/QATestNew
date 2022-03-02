Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore.Processes
Imports Actions = BluePrism.AutomateAppCore.clsValidationInfo.Actions

Friend Class frmValidateAndSaveError : Implements IEnvironmentColourManager

    ''' <summary>
    ''' Creates a validate and save error form
    ''' </summary>
    ''' <param name="procType">The type of process - object or process</param>
    ''' <param name="restrictedAction">The most restricted action set in this
    ''' environment (I think)</param>
    ''' <param name="userAction">The action which the user is attempting to perform
    ''' </param>
    ''' <returns>A form, prepared for displaying to the user, which handles the
    ''' action that they wish to perform given the most restrictive action in the
    ''' system</returns>
    Friend Shared Function CreateForm(
     ByVal procType As DiagramType,
     ByVal restrictedAction As clsValidationInfo.Actions,
     ByVal userAction As ctlProcessViewer.DoValidateAction) As frmValidateAndSaveError
        Return CreateForm(Nothing, procType, restrictedAction, userAction)
    End Function

    ''' <summary>
    ''' Creates a validate and save error form
    ''' </summary>
    ''' <param name="owner">The owner of the form being created. Necessary in order
    ''' to handle the Validate() button being pressed.</param>
    ''' <param name="procType">The type of process - object or process</param>
    ''' <param name="restrictedAction">The most restricted action set in this
    ''' environment (I think)</param>
    ''' <param name="userAction">The action which the user is attempting to perform
    ''' </param>
    ''' <returns>A form, prepared for displaying to the user, which handles the
    ''' action that they wish to perform given the most restrictive action in the
    ''' system</returns>
    Friend Shared Function CreateForm(
     ByVal owner As Form,
     ByVal procType As DiagramType,
     ByVal restrictedAction As clsValidationInfo.Actions,
     ByVal userAction As ctlProcessViewer.DoValidateAction) As frmValidateAndSaveError

        Dim f As New frmValidateAndSaveError()
        f.Owner = owner
        f.StartPosition = FormStartPosition.CenterParent
        Select Case restrictedAction
            Case clsValidationInfo.Actions.PreventSave
                If userAction <> ctlProcessViewer.DoValidateAction.Close Then
                    f.btnDiscardOrUnpublish.Visible = False
                End If
                Select Case procType
                    Case DiagramType.Process
                        f.BlueBar.Title = My.Resources.CannotSaveProcess
                    Case DiagramType.Object
                        f.BlueBar.Title = My.Resources.CannotSaveObject
                End Select
            Case clsValidationInfo.Actions.PreventPublication
                Select Case userAction
                    Case ctlProcessViewer.DoValidateAction.PublishProcess
                        f.BlueBar.Title = My.Resources.CannotPublishProcessWhileValidationErrorsExist
                        f.btnDiscardOrUnpublish.Visible = False
                    Case ctlProcessViewer.DoValidateAction.PublishAction
                        f.BlueBar.Title = My.Resources.CannotPublishObjectPageWhileValidationErrorsExist
                        f.btnDiscardOrUnpublish.Visible = False
                    Case Else
                        f.btnDiscardOrUnpublish.Text = My.Resources.Unpublish
                End Select
        End Select

        Dim returnToStudioStringDescription = If(procType = DiagramType.Process, My.Resources.ReturnToProcessStudio, My.Resources.ReturnToObjectStudio)

       
        Dim localisedDiagramType = TreeDefinitionAttribute.GetLocalizedFriendlyName(procType.ToString())

        If (restrictedAction = Actions.PreventSave) Then
            Select Case procType
                Case DiagramType.Process
                    f.lblBody.Text = String.Format(
                        My.Resources.ThisProcessCannotBeSavedInItsCurrentStateDueToDesignControls22PleaseSelectValidateToV,
                        returnToStudioStringDescription,
                        vbCrLf
                        )
                Case DiagramType.Object
                    f.lblBody.Text = String.Format(
                        My.Resources.ThisObjectCannotBeSavedInItsCurrentStateDueToDesignControls22PleaseSelectValidateToV,
                        returnToStudioStringDescription,
                        vbCrLf
                        )
                Case Else
                    f.lblBody.Text = String.Format(
                        My.Resources.This1CannotBeSavedInItsCurrentStateDueToDesignControls22PleaseSelectValidateToV,
                        returnToStudioStringDescription,
                        localisedDiagramType.ToLowerInvariant(),
                        vbCrLf
                        )
            End Select

        Else
            Select Case procType
                Case DiagramType.Process
                    f.lblBody.Text = String.Format(
                        My.Resources.ThisProcessCannotBePublishedInItsCurrentStateDueToDesignControls22PleaseSelectValidat,
                        returnToStudioStringDescription,
                        vbCrLf
                        )
                Case DiagramType.Object
                    f.lblBody.Text = String.Format(
                            My.Resources.ThisObjectCannotBePublishedInItsCurrentStateDueToDesignControls22PleaseSelectValidat,
                            returnToStudioStringDescription,
                            vbCrLf
                            )
                Case Else
                    f.lblBody.Text = String.Format(
             My.Resources.This1CannotBePublishedInItsCurrentStateDueToDesignControls22PleaseSelectValidat,
             returnToStudioStringDescription,
             localisedDiagramType.ToLowerInvariant(),
             vbCrLf
            )
            End Select
        End If

        f.btnReturnToStudio.Text = returnToStudioStringDescription
        Return f
    End Function

    Private Sub btnDiscardOrUnpublish_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDiscardOrUnpublish.Click
        DialogResult = DialogResult.Yes
        Me.Close()
    End Sub

    Private Sub btnProcessValidation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnProcessValidation.Click
        Dim p As frmProcess = TryCast(Me.Owner, frmProcess)
        If p IsNot Nothing Then
            p.ShowProcessValidation()
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        End If
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReturnToStudio.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

#Region "IEnvironmentColourManager"
    <Browsable(False),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return BlueBar.BackColor
        End Get
        Set(value As Color)
            BlueBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return BlueBar.TitleColor
        End Get
        Set(value As Color)
            BlueBar.TitleColor = value
        End Set
    End Property
#End Region
End Class
