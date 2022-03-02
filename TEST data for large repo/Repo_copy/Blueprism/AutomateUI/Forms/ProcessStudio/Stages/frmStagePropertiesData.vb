

Imports AutomateControls
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.Server.Domain.Models
Imports System.Resources

''' Project  : Automate
''' Class    : frmStagePropertiesData
''' 
''' <summary>
''' The data item properties form.
''' </summary>
Friend Class frmStagePropertiesData
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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents chkAlwaysInit As System.Windows.Forms.CheckBox
    Friend WithEvents lblInitialisation As System.Windows.Forms.Label
    Friend WithEvents cmbExposure As System.Windows.Forms.ComboBox
    Friend WithEvents lblDataType As System.Windows.Forms.Label
    Friend WithEvents cmbDataType As System.Windows.Forms.ComboBox
    Friend WithEvents lblInitialValue As System.Windows.Forms.Label
    Friend WithEvents lblExposure As System.Windows.Forms.Label
    Friend WithEvents txtCurrentValue As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblCurrent As System.Windows.Forms.Label
    Friend WithEvents txtInitialValue As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblVisibility As System.Windows.Forms.Label
    Friend WithEvents chkPrivate As System.Windows.Forms.CheckBox
    Friend WithEvents cmbEnvName As AutomateControls.StyledComboBox
    Friend WithEvents ctlDataTypeTips As AutomateUI.ctlDataTypeTips
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesData))
        Me.lblDataType = New System.Windows.Forms.Label()
        Me.cmbDataType = New System.Windows.Forms.ComboBox()
        Me.lblInitialValue = New System.Windows.Forms.Label()
        Me.lblExposure = New System.Windows.Forms.Label()
        Me.txtCurrentValue = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblCurrent = New System.Windows.Forms.Label()
        Me.txtInitialValue = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblVisibility = New System.Windows.Forms.Label()
        Me.chkPrivate = New System.Windows.Forms.CheckBox()
        Me.ctlDataTypeTips = New AutomateUI.ctlDataTypeTips()
        Me.chkAlwaysInit = New System.Windows.Forms.CheckBox()
        Me.lblInitialisation = New System.Windows.Forms.Label()
        Me.cmbExposure = New System.Windows.Forms.ComboBox()
        Me.cmbEnvName = New AutomateControls.StyledComboBox()
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
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        '
        'lblDataType
        '
        resources.ApplyResources(Me.lblDataType, "lblDataType")
        Me.lblDataType.Name = "lblDataType"
        '
        'cmbDataType
        '
        resources.ApplyResources(Me.cmbDataType, "cmbDataType")
        Me.cmbDataType.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDataType.DropDownWidth = 488
        Me.cmbDataType.Name = "cmbDataType"
        '
        'lblInitialValue
        '
        resources.ApplyResources(Me.lblInitialValue, "lblInitialValue")
        Me.lblInitialValue.Name = "lblInitialValue"
        '
        'lblExposure
        '
        resources.ApplyResources(Me.lblExposure, "lblExposure")
        Me.lblExposure.Name = "lblExposure"
        '
        'txtCurrentValue
        '
        resources.ApplyResources(Me.txtCurrentValue, "txtCurrentValue")
        Me.txtCurrentValue.Name = "txtCurrentValue"
        '
        'lblCurrent
        '
        resources.ApplyResources(Me.lblCurrent, "lblCurrent")
        Me.lblCurrent.Name = "lblCurrent"
        '
        'txtInitialValue
        '
        resources.ApplyResources(Me.txtInitialValue, "txtInitialValue")
        Me.txtInitialValue.Name = "txtInitialValue"
        '
        'lblVisibility
        '
        resources.ApplyResources(Me.lblVisibility, "lblVisibility")
        Me.lblVisibility.Name = "lblVisibility"
        '
        'chkPrivate
        '
        resources.ApplyResources(Me.chkPrivate, "chkPrivate")
        Me.chkPrivate.ForeColor = System.Drawing.Color.Black
        Me.chkPrivate.Name = "chkPrivate"
        '
        'ctlDataTypeTips
        '
        resources.ApplyResources(Me.ctlDataTypeTips, "ctlDataTypeTips")
        Me.ctlDataTypeTips.BackColor = System.Drawing.Color.White
        Me.ctlDataTypeTips.Name = "ctlDataTypeTips"
        Me.ctlDataTypeTips.TabStop = False
        '
        'chkAlwaysInit
        '
        resources.ApplyResources(Me.chkAlwaysInit, "chkAlwaysInit")
        Me.chkAlwaysInit.ForeColor = System.Drawing.Color.Black
        Me.chkAlwaysInit.Name = "chkAlwaysInit"
        '
        'lblInitialisation
        '
        resources.ApplyResources(Me.lblInitialisation, "lblInitialisation")
        Me.lblInitialisation.Name = "lblInitialisation"
        '
        'cmbExposure
        '
        resources.ApplyResources(Me.cmbExposure, "cmbExposure")
        Me.cmbExposure.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbExposure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbExposure.DropDownWidth = 550
        Me.cmbExposure.Name = "cmbExposure"
        '
        'cmbEnvName
        '
        resources.ApplyResources(Me.cmbEnvName, "cmbEnvName")
        Me.cmbEnvName.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbEnvName.DisplayMember = "Value"
        Me.cmbEnvName.DropDownWidth = 488
        Me.cmbEnvName.Name = "cmbEnvName"
        Me.cmbEnvName.ValueMember = "Value"
        '
        'frmStagePropertiesData
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.cmbEnvName)
        Me.Controls.Add(Me.cmbExposure)
        Me.Controls.Add(Me.chkAlwaysInit)
        Me.Controls.Add(Me.lblInitialisation)
        Me.Controls.Add(Me.ctlDataTypeTips)
        Me.Controls.Add(Me.chkPrivate)
        Me.Controls.Add(Me.lblVisibility)
        Me.Controls.Add(Me.txtInitialValue)
        Me.Controls.Add(Me.lblCurrent)
        Me.Controls.Add(Me.txtCurrentValue)
        Me.Controls.Add(Me.lblExposure)
        Me.Controls.Add(Me.lblInitialValue)
        Me.Controls.Add(Me.cmbDataType)
        Me.Controls.Add(Me.lblDataType)
        Me.Name = "frmStagePropertiesData"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.lblDataType, 0)
        Me.Controls.SetChildIndex(Me.cmbDataType, 0)
        Me.Controls.SetChildIndex(Me.lblInitialValue, 0)
        Me.Controls.SetChildIndex(Me.lblExposure, 0)
        Me.Controls.SetChildIndex(Me.txtCurrentValue, 0)
        Me.Controls.SetChildIndex(Me.lblCurrent, 0)
        Me.Controls.SetChildIndex(Me.txtInitialValue, 0)
        Me.Controls.SetChildIndex(Me.lblVisibility, 0)
        Me.Controls.SetChildIndex(Me.chkPrivate, 0)
        Me.Controls.SetChildIndex(Me.ctlDataTypeTips, 0)
        Me.Controls.SetChildIndex(Me.lblInitialisation, 0)
        Me.Controls.SetChildIndex(Me.chkAlwaysInit, 0)
        Me.Controls.SetChildIndex(Me.cmbExposure, 0)
        Me.Controls.SetChildIndex(Me.cmbEnvName, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Member Variables "

    ' The control used to model the initial value of the data stage
    Private mInitialValueControl As Control

    ' The control used to model the current value of the data stage
    Private mCurrentValueControl As Control

    Private ReadOnly mResourceManager As ResourceManager = My.Resources.ResourceManager

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Dialog which displays and accepts changes to the properties of a data stage,
    ''' including its name, description, exposure type (environment, session etc),
    ''' type and intial and current values.
    ''' </summary>
    Public Sub New()
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing
        Me.LogInhibitVisible = False

        mInitialValueControl = txtInitialValue
        mCurrentValueControl = txtCurrentValue

        ' Add possible data types to combo box...
        For Each dt As clsDataTypeInfo In clsProcessDataTypes.GetPublicScalars()
            cmbDataType.Items.Add(dt)
        Next

        With cmbExposure.Items
            For Each d As StageExposureType In [Enum].GetValues(GetType(StageExposureType))
                .Add(New ComboBoxItem(GetExposureFriendlyName(d), d))
            Next
        End With

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the stage that this properties form is modelling as a data stage.
    ''' </summary>
    Protected ReadOnly Property DataStage() As clsDataStage
        Get
            Return DirectCast(mProcessStage, clsDataStage)
        End Get
    End Property

    ''' <summary>
    ''' The selected exposure type.
    ''' </summary>
    Protected ReadOnly Property SelectedExposureType() As StageExposureType
        Get
            Dim el As ComboBoxItem = TryCast(cmbExposure.SelectedItem, ComboBoxItem)
            If el IsNot Nothing Then Return DirectCast(el.Tag, StageExposureType)
            Return StageExposureType.None
        End Get
    End Property

    ''' <summary>
    ''' The currently selected environment variable name.
    ''' </summary>
    Protected ReadOnly Property SelectedEnvVarName() As String
        Get
            Dim item As ComboBoxItem = TryCast(cmbEnvName.SelectedItem, ComboBoxItem)
            If item IsNot Nothing Then Return TryCast(item.Text, String)
            Return cmbEnvName.Text
        End Get
    End Property

#End Region

#Region " Event Handlers / Event Overrides "

    ''' <summary>
    ''' Handles the loading of this form, ensuring that the controls are populated
    ''' from the stage data.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)

        Dim stg As clsDataStage = Me.DataStage
        ' Make sure we have a valid stage...
        If stg Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesData_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        'Fill in all the fields...
        txtName.Text = mProcessStage.GetName()
        txtDescription.Text = mProcessStage.GetNarrative()

        cmbExposure.Text = GetExposureFriendlyName(stg.Exposure)
        chkPrivate.Checked = stg.IsPrivate
        chkAlwaysInit.Checked = stg.AlwaysInit

        Dim dtDataType As DataType = stg.GetDataType()
        If dtDataType <> DataType.unknown Then
            cmbDataType.Text = clsProcessDataTypes.GetFriendlyName(dtDataType)
        Else
            SwapControls(dtDataType)
        End If

        ctlDataTypeTips.ShowTipForType(dtDataType)

        UpdateControlsEnabled()
        cmbDataType.Focus()
    End Sub

    ''' <summary>
    ''' Handles the data type dropdown having its index changed
    ''' </summary>
    Private Sub cmbDataType_SelectedIndexChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbDataType.SelectedIndexChanged
        Dim dt As clsDataTypeInfo = CType(cmbDataType.SelectedItem, clsDataTypeInfo)
        If dt Is Nothing Then Exit Sub
        DataStage.SetDataType(dt.Value)
        SwapControls(dt.Value)
        UpdateControlsEnabled()
        ctlDataTypeTips.ShowTipForType(dt.Value)
    End Sub

    ''' <summary>
    ''' Handles the exposure dropdown having its index changed
    ''' </summary>
    Private Sub cmbExposure_SelectedIndexChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbExposure.SelectedIndexChanged
        Select Case SelectedExposureType
            Case StageExposureType.Environment
                PopulateVariables(txtName.Text)
                mInitialValueControl.Enabled = False
                cmbEnvName.Visible = True
                txtName.Visible = False
            Case StageExposureType.None, StageExposureType.Session, StageExposureType.Statistic
                mInitialValueControl.Enabled = True
                cmbEnvName.Visible = False
                txtName.Visible = True
                Dim selectedDataTypeInfo As clsDataTypeInfo = CType(cmbDataType.SelectedItem, clsDataTypeInfo)
                If selectedDataTypeInfo?.Value = DataType.password Then
                    mInitialValueControl.ResetText()
                End If
        End Select
        UpdateControlsEnabled()
    End Sub

    ''' <summary>
    ''' Handles the environment variable name have its index changed
    ''' </summary>
    Private Sub cmbEnvName_SelectedIndexChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbEnvName.SelectedIndexChanged
        txtName.Text = SelectedEnvVarName

        Dim name = txtName.Text
        clsAPC.ProcessLoader.GetEnvVarSingle(name, True)

        Dim arg As clsArgument = Nothing
        If Not Process.EnvVars.TryGetValue(txtName.Text, arg) Then Return

        With arg.Value
            cmbDataType.Text = clsProcessDataTypes.GetFriendlyName(.DataType)
            mInitialValueControl.Text = .FormattedValue
            mCurrentValueControl.Text = .FormattedValue
        End With
        mInitialValueControl.Enabled = False
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Gets the user-readable name and description of the given exposure type.
    ''' </summary>
    ''' <param name="exp">The exposure type for which the user-friendly name
    ''' is required</param>
    ''' <returns>The name and a short description of the given exposure type.
    ''' </returns>
    Private Function GetExposureFriendlyName(ByVal exp As StageExposureType) As String
        Dim et As String = exp.ToLocalizedString(mResourceManager)
        Select Case exp
            Case StageExposureType.None
                Return My.Resources.frmStagePropertiesData_None
            Case StageExposureType.Environment
                Return String.Format(My.Resources.frmStagePropertiesData_0ReadTheCorrespondingEnvironmentVariableFromSystemManager, et)
            Case StageExposureType.Session
                Return String.Format(My.Resources.frmStagePropertiesData_0ExposeTheDataItemToControlRoom, et)
            Case StageExposureType.Statistic
                Return String.Format(My.Resources.frmStagePropertiesData_0StoreTheContentsOfThisDataItemInTheDatabaseForReportingPurposes, et)
            Case Else
                Return et
        End Select
    End Function

    ''' <summary>
    ''' Swaps the existing Current and Initial Value controls for new ones matching
    ''' the given datatype
    ''' </summary>
    Private Sub SwapControls(ByVal dt As DataType)
        With Controls
            .Remove(mInitialValueControl)
            .Remove(mCurrentValueControl)
            mInitialValueControl = GetValueControl(True, txtInitialValue, dt)
            mCurrentValueControl = GetValueControl(False, txtCurrentValue, dt)
            .Add(mInitialValueControl)
            .Add(mCurrentValueControl)
        End With
    End Sub

    ''' <summary>
    ''' Makes validation checks used prior to closing the form.
    ''' </summary>
    ''' <returns></returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False
        Dim stg As clsDataStage = Me.DataStage

        'Check for illegal characters in chosen name
        Dim sErr As String = Nothing
        If Not clsDataStage.IsValidDataName(txtName.Text, sErr) Then
            UserMessage.Show(String.Format(My.Resources.frmStagePropertiesData_TheChosenNameForThisStageIsInvalid0, sErr))
            txtName.Focus()
            Return False
        End If

        Dim exp As StageExposureType = SelectedExposureType
        stg.Exposure = exp

        'Set private so that scope resolution works as intended
        stg.IsPrivate = chkPrivate.Checked

        'Check uniqueness of data item name
        Dim conflictStage As clsProcessStage =
         CollectionUtil.First(stg.FindNamingConflicts(txtName.Text))
        If conflictStage IsNot Nothing Then
            UserMessage.Show(String.Format(
             My.Resources.frmStagePropertiesData_TheChosenNameForThisStageConflictsWithTheStage0OnPage1PleaseChooseAnother22Alte,
             conflictStage.Name, conflictStage.SubSheet.Name, vbCrLf))

            txtName.Focus()
            Return False
        End If

        If exp = StageExposureType.Environment Then
            txtName.Text = SelectedEnvVarName
            If txtName.Text = "" Then
                UserMessage.Show(My.Resources.frmStagePropertiesData_YouHaveNotSelectedAnEnvironmentVariable)
                cmbEnvName.Focus()
                Return False
            End If
            If Not Process.EnvVars.ContainsKey(txtName.Text) Then
                UserMessage.Show(My.Resources.frmStagePropertiesData_TheEnvironmentVariableYouSelectedDoesNotExist)
                cmbEnvName.Focus()
                Return False
            End If
        Else

            stg.AlwaysInit = chkAlwaysInit.Checked

            'Save the changes...
            If exp = StageExposureType.Statistic Then
                Dim dtype As DataType = stg.DataType
                If Not clsProcessDataTypes.IsStatisticCompatible(dtype) Then
                    UserMessage.Err(
                     My.Resources.frmStagePropertiesData_DataItemOfType0CannotBeUsedAsAStatistic,
                     clsProcessDataTypes.GetFriendlyName(dtype))
                    cmbExposure.Text = My.Resources.frmStagePropertiesData_None
                    Return False
                End If
            End If

            If mCurrentValueControl IsNot Nothing Then
                Dim currVal As clsProcessValue = _
                 DirectCast(mCurrentValueControl, IProcessValue).Value
                If currVal IsNot Nothing Then
                    stg.SetValue(currVal)
                Else
                    stg.SetDataType(DataType.unknown)
                End If
            End If

            If mInitialValueControl IsNot Nothing Then
                Dim initVal As clsProcessValue = _
                 DirectCast(mInitialValueControl, IProcessValue).Value
                If initVal IsNot Nothing Then stg.SetInitialValue(initVal)
            End If
        End If


        Return True
    End Function

    ''' <summary>
    ''' Returns a control based on the given data type, using the physical size and
    ''' location from the given placeholder control
    ''' </summary>
    ''' <param name="initial">True to indicate initial value; False to indicate
    ''' current value.</param>
    ''' <param name="placeholderCtl">A dummy control marking the position and size of
    ''' the desired control</param>
    ''' <param name="dt">The data type for which a control is required</param>
    ''' <returns>The input control object</returns>
    Private Function GetValueControl(ByVal initial As Boolean, _
     ByVal placeHolderCtl As Control, ByVal dt As DataType) As Control
        Dim ctl As Control = clsProcessValueControl.GetControl(dt)
        ctl.Location = placeHolderCtl.Location
        ctl.TabIndex = placeHolderCtl.TabIndex
        ctl.Anchor = placeholderCtl.Anchor
        ' Radio.Top gets corrupted if you try and set its size, so don't do that
        If dt <> DataType.flag Then ctl.Size = placeHolderCtl.Size

        ' Set its readonly state depending on this dialog's editable state
        Dim valueControl As IProcessValue = DirectCast(ctl, IProcessValue)
        valueControl.ReadOnly = Not IsEditable

        Dim val As clsProcessValue
        Try
            If initial _
             Then val = DataStage.GetInitialValue() Else val = DataStage.GetValue()
        Catch nsee As NoSuchElementException
            ' Thrown for undefined env-vars, Just use a blank value.
            val = New clsProcessValue(dt)
        End Try
        valueControl.Value = val
        Return ctl
    End Function

    ''' <summary>
    ''' Gets the help file associated with this form.
    ''' </summary>
    ''' <returns>The name of the help file which details this dialog</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesData.htm"
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
    ''' Populates the environment variables combo box with the environment variables
    ''' from the data stage's process.
    ''' </summary>
    ''' <param name="current">The currently selected environment variable</param>
    Private Sub PopulateVariables(ByVal current As String)
        cmbEnvName.Items.Clear()
        mProcessStage.Process.GetEnvironmentVars(True)
        Dim found As Boolean = False
        For Each arg In Process.EnvVars
            cmbEnvName.Items.Add(New ComboBoxItem(arg.Key))
            If current = arg.Key Then found = True
        Next
        If found Then
            cmbEnvName.Text = current
        Else
            Dim missing As String = My.Resources.frmStagePropertiesData_MISSING & current
            cmbEnvName.Items.Add(New ComboBoxItem(missing, Color.Red, current))
            cmbEnvName.Text = missing
        End If
    End Sub

    ''' <summary>
    ''' Updates the enabled state of the controls dependent on the current state of
    ''' the process and values.
    ''' </summary>
    Private Sub UpdateControlsEnabled()

        Dim current As IProcessValue = TryCast(mCurrentValueControl, IProcessValue)
        Dim initial As IProcessValue = TryCast(mInitialValueControl, IProcessValue)

        If current Is Nothing OrElse initial Is Nothing Then Exit Sub

        Try
            If ShouldShowCurrentValue() AndAlso DataStage.Value IsNot Nothing Then
                mCurrentValueControl.Text = DataStage.Value.FormattedValue
            Else
                mCurrentValueControl.Text = ""
            End If
        Catch nsee As NoSuchElementException
            ' Thrown if the data stage is an env var which is currently undefined
            mCurrentValueControl.Text = ""
        End Try

        ' If we're not running, current value has no meaning - full disablement
        ' Note that this is separate from the 'ReadOnly'ness of the control, which
        ' is ascertained by the nature of the data item
        mCurrentValueControl.Enabled = mProcessStage.Process.IsRunning()

        If IsEditable Then
            Select Case SelectedExposureType
                Case StageExposureType.Environment
                    initial.ReadOnly = True
                    current.ReadOnly = True
                    cmbDataType.Enabled = False
                    chkAlwaysInit.Enabled = False
                Case Else
                    initial.ReadOnly = False
                    current.ReadOnly = False
                    cmbDataType.Enabled = True
                    chkAlwaysInit.Enabled = True
            End Select
            chkPrivate.Enabled = True

        Else
            cmbDataType.Enabled = False
            chkAlwaysInit.Enabled = False
            chkPrivate.Enabled = False

            initial.ReadOnly = True
            current.ReadOnly = True

        End If
    End Sub

#End Region

End Class
