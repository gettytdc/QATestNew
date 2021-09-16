
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServer
Imports BluePrism.Server.Domain.Models


''' Project  : Automate
''' Class    : frmSystemUnlock
''' 
''' <summary>
''' A wizard to iunlock processes.
''' </summary>
Friend Class frmSystemUnlock
    Inherits frmWizard
    Implements IPermission

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
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents lvLockedProcesses As AutomateControls.FlickerFreeListView
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents optSelect As AutomateControls.StyledRadioButton
    Friend WithEvents optAll As AutomateControls.StyledRadioButton
    Friend WithEvents llSelectAll As System.Windows.Forms.LinkLabel
    Friend WithEvents llSelectNone As System.Windows.Forms.LinkLabel
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSystemUnlock))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.optSelect = New AutomateControls.StyledRadioButton()
        Me.optAll = New AutomateControls.StyledRadioButton()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.lvLockedProcesses = New AutomateControls.FlickerFreeListView()
        Me.llSelectNone = New System.Windows.Forms.LinkLabel()
        Me.llSelectAll = New System.Windows.Forms.LinkLabel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.optSelect)
        Me.Panel1.Controls.Add(Me.optAll)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'optSelect
        '
        resources.ApplyResources(Me.optSelect, "optSelect")
        Me.optSelect.Checked = True
        Me.optSelect.Name = "optSelect"
        Me.optSelect.TabStop = True
        '
        'optAll
        '
        resources.ApplyResources(Me.optAll, "optAll")
        Me.optAll.Name = "optAll"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.lvLockedProcesses)
        Me.Panel2.Controls.Add(Me.llSelectNone)
        Me.Panel2.Controls.Add(Me.llSelectAll)
        Me.Panel2.Controls.Add(Me.Label2)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'lvLockedProcesses
        '
        resources.ApplyResources(Me.lvLockedProcesses, "lvLockedProcesses")
        Me.lvLockedProcesses.CheckBoxes = True
        Me.lvLockedProcesses.Name = "lvLockedProcesses"
        Me.lvLockedProcesses.UseCompatibleStateImageBehavior = False
        Me.lvLockedProcesses.View = System.Windows.Forms.View.Details
        '
        'llSelectNone
        '
        resources.ApplyResources(Me.llSelectNone, "llSelectNone")
        Me.llSelectNone.Name = "llSelectNone"
        Me.llSelectNone.TabStop = True
        '
        'llSelectAll
        '
        resources.ApplyResources(Me.llSelectAll, "llSelectAll")
        Me.llSelectAll.Name = "llSelectAll"
        Me.llSelectAll.TabStop = True
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'frmSystemUnlock
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "frmSystemUnlock"
        Me.Title = My.Resources.frmSystemUnlock_AllowUsersToRegainAccessToProcessesThatAreLocked
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.Panel1, 0)
        Me.Controls.SetChildIndex(Me.Panel2, 0)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Public Sub New()
        Me.New(WizardType.Selection)
    End Sub

    Public Sub New(ByVal Type As WizardType)
        MyBase.New(Type)

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
    End Sub

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property

    Private objSorter As clsListViewSorter

    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        Dim iPage As Integer = MyBase.GetStep
        Select Case iPage
            Case 0
                MyBase.UpdatePanelTextToProcessOrObject(mWizardType, Me.Panel1)
                ShowPage(Panel1)
            Case 1
                If optAll.Checked Then
                    Dim i As Integer
                    Dim sErr As String = Nothing
                    For i = 0 To lvLockedProcesses.Items.Count - 1
                        Try
                            gSv.UnlockProcess(New Guid(lvLockedProcesses.Items(i).SubItems(5).Text()), True)
                        Catch ex As Exception
                            UserMessage.Err(
                                ex, String.Format(My.Resources.frmSystemUnlock_ErrorUnlockingProcess0, ex.Message))
                            Exit For
                        End Try

                        Try
                            gSv.AuditRecordProcessEvent(ProcessEventCode.UnlockProcess, CType(lvLockedProcesses.Items(i).Tag, Guid), Nothing, Nothing, Nothing)
                        Catch ex As Exception
                            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_WarningAuditRecordingFailedWithError0, ex.Message))  
                        End Try
                    Next

                    Me.Close()
                Else
                    MyBase.UpdatePanelTextToProcessOrObject(mWizardType, Me.Panel2)
                    ShowPage(Panel2)
                End If
            Case 2
                Dim i As Integer
                Dim sErr As String = Nothing
                For i = 0 To lvLockedProcesses.Items.Count - 1
                    If lvLockedProcesses.Items(i).Checked = True Then
                        Try
                            gSv.UnlockProcess(New Guid(lvLockedProcesses.Items(i).SubItems(5).Text), True)
                        Catch ex As Exception
                            UserMessage.Err(
                                ex, String.Format(My.Resources.frmSystemUnlock_ErrorUnlockingProcess0, ex.Message))
                            Exit For
                        End Try

                        Try
                            gSv.AuditRecordProcessEvent(ProcessEventCode.UnlockProcess, CType(lvLockedProcesses.Items(i).Tag, Guid), Nothing, Nothing, Nothing)
                        Catch ex As Exception
                            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_WarningAuditRecordingFailedWithError0, ex.Message))  
                        End Try
                    End If
                Next
                Me.Close()
        End Select
    End Sub

    Private Sub PopulateLocks()
        Dim errorMessage As String = Nothing
        lvLockedProcesses.Items.Clear()
        Try
            Dim useBusinessObjects As Boolean = (mWizardType = WizardType.BusinessObject)
            Dim lockedProcesses = gSv.GetLockedProcesses(useBusinessObjects)
            If lockedProcesses Is Nothing Then
                UserMessage.Show(errorMessage)
                Close()
                Exit Sub
            End If

            For Each process In lockedProcesses.LockedProcesses
                Dim item = lvLockedProcesses.Items.Add("")
                With item.SubItems
                    .Add(process.Name)
                    .Add(process.LockDate.ToString())
                    .Add(process.Username)
                    .Add(process.MachineName)
                    .Add(process.Id.ToString())
                End With
                item.Tag = process.Id
            Next
        Catch mdex As MissingDataException
            UserMessage.Show(mdex.Message)
        Catch ex As Exception
            UserMessage.Show(ex.Message)
        End Try


    End Sub

    Private Sub frmSystemUnlock_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetMaxSteps(1)


        objSorter = New clsListViewSorter(lvLockedProcesses) With {
            .ColumnDataTypes = {GetType(String), GetType(String), GetType(Date), GetType(String), GetType(String), GetType(String)},
            .Order = SortOrder.Ascending
        }

        Me.lvLockedProcesses.ListViewItemSorter = objSorter

        lvLockedProcesses.Columns.Add("", 20, HorizontalAlignment.Left)
        lvLockedProcesses.Columns.Add(My.Resources.frmSystemUnlock_Name, 180, HorizontalAlignment.Left)
        lvLockedProcesses.Columns.Add(My.Resources.frmSystemUnlock_LockDate, 120, HorizontalAlignment.Left)
        lvLockedProcesses.Columns.Add(My.Resources.frmSystemUnlock_UserName, 70, HorizontalAlignment.Left)
        lvLockedProcesses.Columns.Add(My.Resources.frmSystemUnlock_ResourceName, 100, HorizontalAlignment.Left)
        PopulateLocks()
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmSystemUnlock.htm"
    End Function

    Private Sub llSelectAll_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llSelectAll.LinkClicked
        For Each lvi As ListViewItem In Me.lvLockedProcesses.Items
            lvi.Checked = True
        Next
    End Sub

    Private Sub llSelectNone_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llSelectNone.LinkClicked
        For Each lvi As ListViewItem In Me.lvLockedProcesses.Items
            lvi.Checked = False
        Next
    End Sub

    Private Sub optAll_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optAll.CheckedChanged
        If Me.optAll.Checked Then
            Me.btnNext.Text = My.Resources.frmSystemUnlock_Finish
        Else
            Me.btnNext.Text = My.Resources.frmSystemUnlock_Next
        End If
    End Sub
End Class
