Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Friend Class frmStagePropertiesException
    ''' <summary>
    ''' Constructor for the form.
    ''' </summary>
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        CtlDataTypeTips1.ShowTip(My.Resources.frmStagePropertiesException_Exceptions, String.Format(My.Resources.frmStagePropertiesException_0ExceptionTypeCanBeUsedToGeneraliseTheCauseOfTheException, BPUtil.PasswordChar) & vbCrLf & vbCrLf & String.Format(My.Resources.frmStagePropertiesException_0ExceptionDetailCanBeUsedToGetAnyAdditionalValuesFromTheProcess, BPUtil.PasswordChar))

    End Sub


    ''' <summary>
    ''' Holds a reference to the stage being edited.
    ''' </summary>
    Private mExceptionStage As Stages.clsExceptionStage

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesException.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        Try
            Dim tp As String = cmbExceptionType.Text
            If tp.Length > 30 Then Throw New FieldLengthException(
             My.Resources.frmStagePropertiesException_ExceptionTypeCannotExceed30Characters)

            mExceptionStage.ExceptionType = tp
            mExceptionStage.ExceptionDetailForLocalizationSetting = objExceptionDetailExpressionEdit.Text
            mExceptionStage.UseCurrentException = chkSameException.Checked
            mExceptionStage.SaveScreenCapture = chkSaveScreenCapture.Checked

            Return MyBase.ApplyChanges()

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmStagePropertiesException_FailedToAddExceptionType0, ex.Message), ex)
        End Try

    End Function

    ''' <summary>
    ''' Populates the form with data from the stage.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()
        Try
            Dim exceptionTypes = gSv.GetExceptionTypes()
            For Each s As String In exceptionTypes
                cmbExceptionType.Items.Add(s)
            Next
        Catch ex As Exception
            UserMessage.Err(ex.Message)
        End Try

        cmbExceptionType.Text = mExceptionStage.ExceptionType
        objExceptionDetailExpressionEdit.Text = mExceptionStage.ExceptionDetailForLocalizationSetting
        objExceptionDetailExpressionEdit.Stage = mExceptionStage
        objExceptionDetailExpressionEdit.ProcessViewer = ProcessViewer
        chkSameException.Checked = mExceptionStage.UseCurrentException
        chkSaveScreenCapture.Checked = mExceptionStage.SaveScreenCapture

        MyBase.PopulateStageData()
    End Sub

    ''' <summary>
    ''' Override of base class implementation so that we can collect
    '''  a strongly typed copy of stage reference.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    Public Overrides Property ProcessStage() As BluePrism.AutomateProcessCore.clsProcessStage
        Get
            Return MyBase.ProcessStage
        End Get
        Set(ByVal value As BluePrism.AutomateProcessCore.clsProcessStage)
            Me.mExceptionStage = CType(value, Stages.clsExceptionStage)
            MyBase.ProcessStage = value
        End Set
    End Property

    Private Sub chkSameException_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSameException.CheckedChanged
        cmbExceptionType.Enabled = Not chkSameException.Checked
        objExceptionDetailExpressionEdit.Enabled = Not chkSameException.Checked
    End Sub

    Private Sub cmbExceptionType_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbExceptionType.GotFocus
        Me.CtlDataTypeTips1.ShowTip(My.Resources.frmStagePropertiesException_ExceptionType, String.Format(My.Resources.frmStagePropertiesException_0ChooseAnExistingExceptionTypeFromTheDropdown, BPUtil.PasswordChar) & vbCrLf & vbCrLf & String.Format(My.Resources.frmStagePropertiesException_0CreateANewExceptionTypeByEnteringText, BPUtil.PasswordChar))
    End Sub

    Private Sub objExceptionDetailExpressionEdit_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles objExceptionDetailExpressionEdit.Click
        Me.CtlDataTypeTips1.ShowTip(My.Resources.frmStagePropertiesException_ExceptionDetail, String.Format(My.Resources.frmStagePropertiesException_0ExceptionDetailCanBeAnyValidAutomateExpression, BPUtil.PasswordChar) & vbCrLf & vbCrLf & String.Format(My.Resources.frmStagePropertiesException_0IfYouJustNeedSomeTextRememberToEncloseTheTextInQuoteMarks, BPUtil.PasswordChar))
    End Sub
End Class
