Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore.Utility

''' Project  : Automate
''' Class    : frmStagePropertiesSubSheetInfo
''' 
''' <summary>
''' The SubSheetInfo properties form.
''' </summary>
Friend Class frmStagePropertiesSubSheetInfo
    Inherits frmProperties



#Region " Windows Form Designer generated code "



    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Friend WithEvents chkPublish As System.Windows.Forms.CheckBox

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents mPrePostConds As ctlPreconditionsEndconditions
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesSubSheetInfo))
        Me.mPrePostConds = New AutomateUI.ctlPreconditionsEndconditions()
        Me.chkPublish = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'mPrePostConds
        '
        resources.ApplyResources(Me.mPrePostConds, "mPrePostConds")
        Me.mPrePostConds.Name = "mPrePostConds"
        Me.mPrePostConds.PostConditions = CType(resources.GetObject("mPrePostConds.PostConditions"), System.Collections.Generic.ICollection(Of String))
        Me.mPrePostConds.PreConditions = CType(resources.GetObject("mPrePostConds.PreConditions"), System.Collections.Generic.ICollection(Of String))
        Me.mPrePostConds.ReadOnly = False
        '
        'chkPublish
        '
        resources.ApplyResources(Me.chkPublish, "chkPublish")
        Me.chkPublish.Name = "chkPublish"
        Me.chkPublish.UseVisualStyleBackColor = True
        '
        'frmStagePropertiesSubSheetInfo
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.chkPublish)
        Me.Controls.Add(Me.mPrePostConds)
        Me.Name = "frmStagePropertiesSubSheetInfo"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.mPrePostConds, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.chkPublish, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region


    Private msName As String

    ''' <summary>
    ''' The start stage of the current subsheet
    ''' </summary>
    Private mStartStage As clsProcessStage



    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing
        Me.LogInhibitVisible = False

    End Sub


    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()
        If Not mProcessStage Is Nothing Then
            msName = txtName.Text

            mStartStage = mProcessStage.Process.GetStage(mProcessStage.Process.GetSubSheetStartStage(mProcessStage.GetSubSheetID))
            mPrePostConds.PreConditions = mStartStage.Preconditions
            mPrePostConds.PostConditions = mStartStage.PostConditions

            chkPublish.Visible = (mProcessStage.Process.ProcessType = DiagramType.Object AndAlso
                                  mProcessStage.SubSheet.SheetType = SubsheetType.Normal)
            chkPublish.Checked = mProcessStage.Process.GetSubSheetByID(mProcessStage.GetSubSheetID).Published
        Else
            UserMessage.Show(My.Resources.frmStagePropertiesSubSheetInfo_InternalErrorEmptyStagePassedToPropertiesForm)
        End If
    End Sub

    ''' <summary>
    ''' Sets the name field value.
    ''' </summary>
    ''' <param name="s">The value</param>
    Public Sub SetName(ByVal s As String)
        txtName.Text = s
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub


    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False

        'if the name hasn't changed then we don't have a lot to worry about;
        'set the information and exit.
        If txtName.Text = msName Then
            SetData()
            Return True
        End If

        'check for witty users
        If txtName.Text = My.Resources.frmStagePropertiesSubSheetInfo_MainPage Then
            UserMessage.Show(My.Resources.frmStagePropertiesSubSheetInfo_YouCanTUseMainPageAsTheNameForAPage)
            Return False
        End If

        'check for duplicate name
        If Not mProcessStage.Process.GetSubSheetID(txtName.Text).Equals(Guid.Empty) Then
            UserMessage.Show(My.Resources.frmStagePropertiesSubSheetInfo_APageWithThatNameAlreadyExistsPleaseChooseAnother)
            Return False
        End If

        'finally, go ahead and make the change to the name
        Dim sErr As String = Nothing
        Dim objProcess As clsProcess = mProcessStage.Process
        If objProcess.RenameSubSheet(mProcessStage.GetSubSheetID, txtName.Text, sErr) Then
            If TypeOf mParentForm Is frmProcess Then
                CType(mParentForm, frmProcess).ProcessViewer.UpdateSheetReferenceNames(msName, txtName.Text, mProcessStage.GetSubSheetID)
            End If
            SetData()
        Else
            UserMessage.Show(String.Format(My.Resources.frmStagePropertiesSubSheetInfo_RenameFailed0, sErr))
            Return False
        End If

        If TypeOf mParentForm Is frmProcess Then
            CType(mParentForm, frmProcess).ProcessViewer.UpdateSheetTabs()
            CType(mParentForm, frmProcess).ProcessViewer.InvalidateView()
        End If

        Return True
    End Function


    Private Sub SetData()
        'save preconditions/postconditions
        mStartStage.Preconditions = mPrePostConds.PreConditions
        mStartStage.PostConditions = mPrePostConds.PostConditions

        If mProcessStage.Process.ProcessType = DiagramType.Object Then
            Dim Subsheet As clsProcessSubSheet = mProcessStage.Process.GetSubSheetByID(mProcessStage.GetSubSheetID)
            Subsheet.Published = Me.chkPublish.Checked
        End If
    End Sub


    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            OpenHelpFile(Me, Name)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesSubSheetInfo.htm"
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
End Class
