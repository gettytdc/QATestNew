Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore.Utility

''' Project  : Automate
''' Class    : frmStagePropertiesSubSheet
''' 
''' <summary>
''' The process page properties form.
''' </summary>
Friend Class frmStagePropertiesSubSheet
    Inherits frmProperties
    Implements IDataItemTreeRefresher


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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents cmbSheet As System.Windows.Forms.ComboBox
    Friend WithEvents mIOConditions As AutomateUI.ctlInputsOutputsConditions
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents objDataItemTreeView As AutomateUI.ctlDataItemTreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesSubSheet))
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmbSheet = New System.Windows.Forms.ComboBox()
        Me.mIOConditions = New AutomateUI.ctlInputsOutputsConditions()
        Me.objDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.Name = "Label3"
        '
        'cmbSheet
        '
        resources.ApplyResources(Me.cmbSheet, "cmbSheet")
        Me.cmbSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSheet.Name = "cmbSheet"
        '
        'mIOConditions
        '
        resources.ApplyResources(Me.mIOConditions, "mIOConditions")
        Me.mIOConditions.Name = "mIOConditions"
        '
        'objDataItemTreeView
        '
        Me.objDataItemTreeView.CheckBoxes = False
        resources.ApplyResources(Me.objDataItemTreeView, "objDataItemTreeView")
        Me.objDataItemTreeView.IgnoreScope = False
        Me.objDataItemTreeView.Name = "objDataItemTreeView"
        Me.objDataItemTreeView.Stage = Nothing
        Me.objDataItemTreeView.StatisticsMode = False
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label3)
        Me.SplitContainer1.Panel1.Controls.Add(Me.cmbSheet)
        Me.SplitContainer1.Panel1.Controls.Add(Me.mIOConditions)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.objDataItemTreeView)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        '
        'frmStagePropertiesSubSheet
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesSubSheet"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.SplitContainer1, 0)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.PerformLayout()
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region "constructor"

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing
        mObjectRefs = Nothing

    End Sub

#End Region

#Region "Members"

    ''' <summary>
    ''' Used for when we maximise and then restore form to original size. We need 
    ''' to know what size it was before any resizing took place so that we can
    ''' restore the original size.
    ''' </summary>
    Private OriginalDataItemsTreeviewWidth As Integer

    ''' <summary>
    ''' Used for when we maximise and then restore form to original size. We need 
    ''' to know what location it had before any resizing took place so that we can
    ''' restore the original location.
    ''' </summary>
    Private OriginalDataItemsTreeviewLeft As Integer

#End Region

#Region " Properties "

    Public ReadOnly Property SubSheetRefStage() As clsSubSheetRefStage
        Get
            Return DirectCast(mProcessStage, clsSubSheetRefStage)
        End Get
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

#End Region

    Private Sub frmStageProperties_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Make sure we have a valid index...
        If mProcessStage Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesSubSheet_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If
        Dim sheetRefStg As clsSubSheetRefStage = SubSheetRefStage

        'prepare the inputs/outputs/conditions control and populate it
        mIOConditions.SetStage(mProcessStage, Me.ProcessViewer, mObjectRefs)
        Dim stg As clsProcessStage = mProcessStage.Process.GetStage(mProcessStage.Process.GetSubSheetStartStage(sheetRefStg.ReferenceId))
        If Not stg Is Nothing Then _
         mIOConditions.UpdatePreconditionsAndPostconditions(stg.Preconditions, stg.PostConditions)


        UpdateProcessList()

        'Fill in all the fields...
        MyBase.txtName.Text = mProcessStage.GetName()
        MyBase.txtDescription.Text = mProcessStage.GetNarrative()

        'Select correct process...
        For Each objSub As clsProcessSubSheet In mProcessStage.Process.SubSheets
            If sheetRefStg.ReferenceId.Equals(objSub.ID) Then
                cmbSheet.Text = objSub.Name
                Exit For
            End If
        Next
        If cmbSheet.Text = "" Then cmbSheet.Text = My.Resources.frmStagePropertiesSubSheet_None

        Me.mIOConditions.Treeview = Me.objDataItemTreeView
        Me.mIOConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)

        objDataItemTreeView.Populate(mProcessStage)

        Me.OriginalDataItemsTreeviewWidth = Me.objDataItemTreeView.Width
        Me.OriginalDataItemsTreeviewLeft = Me.objDataItemTreeView.Left

        Me.objDataItemTreeView.ProcessViewer = Me.ProcessViewer
    End Sub


    Private Sub UpdateProcessList()
        cmbSheet.Items.Clear()
        cmbSheet.Sorted = True
        For Each sh As clsProcessSubSheet In mProcessStage.Process.GetNormalSheets
            cmbSheet.Items.Add(sh.Name)
        Next
        'after adding all of the processes in a sorted list
        'we stop sorting and insert the text "None" at the top
        'of the list.
        cmbSheet.Sorted = False
        cmbSheet.Items.Insert(0, My.Resources.frmStagePropertiesSubSheet_None)
    End Sub

#Region "btnOk_Click"

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If MyBase.ApplyChanges Then

            'Set the parameters
            Me.mProcessStage.ClearParameters()
            Me.mProcessStage.AddParameters(Me.mIOConditions.GetInputParameters)
            Me.mProcessStage.AddParameters(Me.mIOConditions.GetOutputParameters)


            Dim objSheetRef As clsSubSheetRefStage = CType(mProcessStage, clsSubSheetRefStage)
            'Get the process ID for the selected process and save it back
            'to the stage object...
            Dim gProcID As Guid = Guid.Empty
            For Each objSub As clsProcessSubSheet In mProcessStage.Process.SubSheets
                If objSub.Name = CStr(cmbSheet.SelectedItem) Then
                    gProcID = objSub.ID
                    Exit For
                End If
            Next
            objSheetRef.ReferenceId = gProcID

            Return True
        Else
            Return False
        End If
    End Function


#End Region

#Region "btnCancel_Click"

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

#End Region

#Region "RewriteParameters"

    ''' <summary>
    ''' Called when the process is changed - all inputs and outputs
    ''' for the stage are rewritten accordingly. If any parameters
    ''' are common between the previous and new actions, the values
    ''' are retained.
    ''' </summary>
    Private Sub RewriteParameters()

        Dim objParm As clsProcessParameter, objParm2 As clsProcessParameter
        Dim ii As Integer, jj As Integer

        'Get the subsheet ID for the selected process...
        Dim subsheetId As Guid = Guid.Empty
        For Each objSub As clsProcessSubSheet In mProcessStage.Process.SubSheets
            If objSub.Name = CStr(cmbSheet.SelectedItem) Then
                subsheetId = objSub.ID
                Exit For
            End If
        Next
        'If there isn't a valid subsheet selected, clear parameters
        'and bail out...
        If subsheetId.Equals(Guid.Empty) Then
            Do While mProcessStage.GetNumParameters > 0
                objParm = mProcessStage.GetParameter(0)
                mProcessStage.RemoveParameter(objParm)
            Loop
            Exit Sub
        End If

        'Find the start stage, and get the parameters.
        Dim objStartStage As clsProcessStage
        objStartStage = mProcessStage.Process.GetStage(mProcessStage.Process.GetSubSheetStartStage(subsheetId))
        If Not objStartStage Is Nothing Then

            'Stage 1 - remove any parameters from the process that
            '          should not be there
InStage1:
            For ii = 0 To mProcessStage.GetNumParameters() - 1
                objParm = mProcessStage.GetParameter(ii)
                If objParm.Direction = ParamDirection.In And objStartStage.GetParameter(objParm.Name, objParm.Direction) Is Nothing Then
                    'Remove the parameter, then start this stage
                    'again...
                    mProcessStage.RemoveParameter(objParm.Name, objParm.Direction)
                    GoTo InStage1
                End If
            Next

            'Stage 2 - add any parameters that aren't present, and
            '          update properties for those that are
            For jj = 0 To objStartStage.GetNumParameters() - 1
                objParm = objStartStage.GetParameter(jj)
                If objParm.Direction = ParamDirection.In Then
                    Dim bFound As Boolean
                    bFound = False
                    For ii = 0 To mProcessStage.GetNumParameters() - 1
                        objParm2 = mProcessStage.GetParameter(ii)
                        If objParm2.Name = objParm.Name And _
                         objParm2.Direction = objParm.Direction Then
                            'Update the data type if it changed...
                            If objParm2.GetDataType() <> objParm.GetDataType() Then
                                objParm2.SetDataType(objParm.GetDataType())
                            End If
                            If objParm2.Narrative <> objParm.Narrative Then
                                objParm2.Narrative = objParm.Narrative
                            End If
                            If objParm2.FriendlyName <> objParm.FriendlyName Then
                                objParm2.FriendlyName = objParm.FriendlyName
                            End If
                            bFound = True
                        End If
                    Next
                    If Not bFound Then
                        mProcessStage.AddParameter(objParm.Direction, objParm.GetDataType(), objParm.Name, objParm.Narrative, MapType.Expr, "", Nothing, objParm.FriendlyName)
                    End If
                End If

            Next

        End If

        'Find the end stage, and get the parameters.
        Dim objEndStage As clsProcessStage
        objEndStage = mProcessStage.Process.GetStage(mProcessStage.Process.GetSubSheetEndStage(subsheetId))
        If Not objEndStage Is Nothing Then

            'Stage 1 - remove any parameters from the process that
            '          should not be there
OutStage1:
            For ii = 0 To mProcessStage.GetNumParameters() - 1
                objParm = mProcessStage.GetParameter(ii)
                If objParm.Direction = ParamDirection.Out And objEndStage.GetParameter(objParm.Name, objParm.Direction) Is Nothing Then
                    'Remove the parameter, then start this stage
                    'again...
                    mProcessStage.RemoveParameter(objParm.Name, objParm.Direction)
                    GoTo OutStage1
                End If
            Next

            'Stage 2 - add any parameters that aren't present, and
            '          update properties for those that are
            For jj = 0 To objEndStage.GetNumParameters() - 1
                objParm = objEndStage.GetParameter(jj)
                If objParm.Direction = ParamDirection.Out Then
                    Dim bFound As Boolean
                    bFound = False
                    For ii = 0 To mProcessStage.GetNumParameters() - 1
                        objParm2 = mProcessStage.GetParameter(ii)
                        If objParm2.Name = objParm.Name And _
                         objParm2.Direction = objParm.Direction Then
                            'Update the data type if it changed...
                            If objParm2.GetDataType() <> objParm.GetDataType() Then
                                objParm2.SetDataType(objParm.GetDataType())
                            End If
                            If objParm2.Narrative <> objParm.Narrative Then
                                objParm2.Narrative = objParm.Narrative
                            End If
                            If objParm2.FriendlyName <> objParm.FriendlyName Then
                                objParm2.FriendlyName = objParm.FriendlyName
                            End If
                            bFound = True
                        End If
                    Next
                    If Not bFound Then
                        mProcessStage.AddParameter(objParm.Direction, objParm.GetDataType(), objParm.Name, objParm.Narrative, MapType.Stage, "", Nothing, objParm.FriendlyName)
                    End If
                End If

            Next

        End If


    End Sub

#End Region

#Region "btnHelp_Click"

    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            OpenHelpFile(Me, Name)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

#End Region

#Region "cmbProcess_SelectedIndexChanged"

    Private Sub cmbProcess_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbSheet.SelectedIndexChanged
        RewriteParameters()
        Me.mIOConditions.RefreshControls(mProcessStage.GetInputs, mProcessStage.GetOutputs)
    End Sub

#End Region



    Private Sub UpdatePreconditionsAndEndpoints()

    End Sub

    Private Sub ShiftComponents(ByVal ShiftDistance As Integer, ByVal ShiftRight As Boolean)
        Dim HalfWidth As Integer = ShiftDistance \ 2
        Dim iTemp As Integer = 1
        If Not ShiftRight Then iTemp = -1

        Me.objDataItemTreeView.Left += ShiftDistance * iTemp
        Me.mIOConditions.Width += ShiftDistance * iTemp
        Me.cmbSheet.Width += ShiftDistance * iTemp
        Me.txtName.Width = Me.mIOConditions.Left + Me.mIOConditions.Width - Me.txtName.Left
        Me.txtDescription.Width = Me.txtName.Width
    End Sub

    Private Sub frmStagePropertiesCalculation_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Static LastWindowState As FormWindowState
        If Not (MyBase.WindowState = FormWindowState.Minimized OrElse LastWindowState = FormWindowState.Minimized) Then
            MyBase.SuspendLayout()
            Static WidthReduction As Integer
            If Me.objDataItemTreeView.Width > 350 AndAlso Me.objDataItemTreeView.Left <= Me.OriginalDataItemsTreeviewLeft + 5 Then
                WidthReduction = Me.objDataItemTreeView.Width - 350
                Me.objDataItemTreeView.Width -= WidthReduction
                ShiftComponents(WidthReduction, True)
            Else
                If Not WidthReduction = 0 Then       'we must be restoring to default size after minimising
                    Me.objDataItemTreeView.Width = Me.OriginalDataItemsTreeviewWidth
                    ShiftComponents(WidthReduction, False)
                End If
            End If
            MyBase.ResumeLayout(True)

        End If

        LastWindowState = MyBase.WindowState
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesSubSheet.htm"
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

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        objDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        objDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub
End Class
