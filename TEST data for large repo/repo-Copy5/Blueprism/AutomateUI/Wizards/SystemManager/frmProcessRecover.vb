
Imports BluePrism.AutomateAppCore

''' -----------------------------------------------------------------------------
''' Project  : Automate
''' Class    : frmProcessRecover
''' 
''' -----------------------------------------------------------------------------
''' <summary>
''' A wizard to recover process back ups.
''' </summary>
''' -----------------------------------------------------------------------------
Public Class frmProcessRecover
    Inherits Automate.frmWizard
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
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents lvProcess As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim ListViewItem1 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem(New String() {"Some process", "31 May 05 14:23", "31 May 05 14:23", "Back up"}, -1)
        Me.Panel1 = New System.Windows.Forms.Panel
        Me.Label2 = New System.Windows.Forms.Label
        Me.lvProcess = New System.Windows.Forms.ListView
        Me.ColumnHeader1 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader3 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader2 = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader4 = New System.Windows.Forms.ColumnHeader
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.lvProcess)
        Me.Panel1.Location = New System.Drawing.Point(16, 72)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(456, 216)
        Me.Panel1.TabIndex = 106
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(8, 8)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(440, 16)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "Select a process to recover"
        '
        'lvProcess
        '
        Me.lvProcess.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
           Or System.Windows.Forms.AnchorStyles.Left) _
           Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lvProcess.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader3, Me.ColumnHeader2, Me.ColumnHeader4})
        Me.lvProcess.FullRowSelect = True
        ListViewItem1.StateImageIndex = 0
        Me.lvProcess.Items.AddRange(New System.Windows.Forms.ListViewItem() {ListViewItem1})
        Me.lvProcess.Location = New System.Drawing.Point(8, 24)
        Me.lvProcess.MultiSelect = False
        Me.lvProcess.Name = "lvProcess"
        Me.lvProcess.Size = New System.Drawing.Size(440, 184)
        Me.lvProcess.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.lvProcess.TabIndex = 0
        Me.lvProcess.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Name"
        Me.ColumnHeader1.Width = 150
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Version"
        Me.ColumnHeader3.Width = 100
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Restored"
        Me.ColumnHeader2.Width = 100
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Type"
        Me.ColumnHeader4.Width = 200
        '
        'frmProcessRecover
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(784, 562)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "frmProcessRecover"
        Me.Text = "Restore Process"
        Me.Controls.SetChildIndex(Me.Panel1, 0)
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "New"

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Default constructor. Creates process rather than object.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public Sub New()
        Me.New(WizardType.Selection)
        msThing = "process"
    End Sub

    Public Sub New(ByVal SelectionType As WizardType)
        MyBase.New(SelectionType)

        'form designer requires this
        Me.InitializeComponent()


        If miWizardType = WizardType.BusinessObject Then
            msThing = "business object"
        Else
            msThing = "process"
        End If

    End Sub

#End Region

#Region "Members"

    Private Const csSession As String = "Unrestored edit session back up"
    Private Const csOriginal As String = "Original version"
    Private Const csRestored As String = "Restored edit session back up"

    Private msThing As String

#End Region

#Region "Permission"
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    ''' -----------------------------------------------------------------------------
    Public ReadOnly Property Permission() As Long Implements IPermission.Permission
        Get
            If miWizardType = WizardType.BusinessObject Then
                Return clsAuthorisation.GetPermissionIDFromActionName("Create/Clone Business Object") _
                  + clsAuthorisation.GetPermissionIDFromActionName("Edit Business Object") _
                   + clsAuthorisation.GetPermissionIDFromActionName("View Business Object")
            Else
                Return clsAuthorisation.GetPermissionIDFromActionName("Create/Clone Process") _
                  + clsAuthorisation.GetPermissionIDFromActionName("Edit Process") _
                   + clsAuthorisation.GetPermissionIDFromActionName("View Process")
            End If
        End Get
    End Property

#End Region

#Region "UpdatePage"
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Protected Overrides Sub UpdatePage()
        Dim iPage As Integer = MyBase.GetStep
        Dim bRestored As Boolean = False
        Dim bRollBack As Boolean = False
        Dim sMessage As String = ""
        Dim sType As String
        Dim sProcessId As String = ""

        Select Case iPage

            Case 0
                SetTitleBar(miWizardType, Panel1)
                ShowPage(Panel1)
                btnNext.Text = "Restore"
                Try
                    If miWizardType = WizardType.BusinessObject Then
                        PopulateProcesses(clsAutoSaver.GetRecoverableBusinessObjects())
                    Else
                        PopulateProcesses(clsAutoSaver.GetRecoverableProcesses())
                    End If
                Catch ex As Exception
                    UserMessage.Show("The " & msThing & " back ups could not be accessed because an error occurred. Please contact Blue Prism for technical support if this problem persists. The error message was '" & ex.Message & "'")
                End Try

            Case 1
                If lvProcess.View = View.Details AndAlso lvProcess.SelectedItems.Count > 0 Then

                    Try
                        sProcessId = lvProcess.SelectedItems(0).Tag.ToString
                        sType = lvProcess.SelectedItems.Item(0).SubItems(3).Text
                        Windows.Forms.Cursor.Current = Cursors.WaitCursor
                        If sType = csSession Or sType = csRestored Then
                            If clsAutoSaver.RestoreBackUp(sProcessId) Then
                                sMessage = String.Format("The {0} has been successfully recovered from the back up.", msThing)
                            Else
                                sMessage = String.Format("The {0} could not be recovered from the back up.", msThing)
                            End If
                        ElseIf sType = csOriginal Then
                            If clsAutoSaver.RestoreOriginal(sProcessId) Then
                                sMessage = String.Format("The {0} has been successfully restored to its original state.", msThing)
                            Else
                                sMessage = String.Format("The {0} could not be restored to its original state.", msThing)
                            End If
                        Else
                            sMessage = String.Format("The {0} could not be recovered.", msThing)
                        End If

                    Catch ex As Exception
                        UserMessage.Show(String.Format("The {0} could not be recovered because an error occurred. Please contact Blue Prism for technical support if this problem persists. The error message was '{1}'", msThing, ex.Message))

                    Finally
                        Windows.Forms.Cursor.Current = Cursors.Default
                        UserMessage.OK(sMessage)
                        clsAutoSaver.UnLockProcess(sProcessId)
                        btnCancel.Text = "Close"
                        'Me.Close()
                        bRollBack = True
                    End Try

                Else

                    If lvProcess.View = View.Details Then
                        UserMessage.OK(String.Format("Please select a {0} to recover.", msThing))
                        bRollBack = True
                    Else
                        DialogResult = Windows.Forms.DialogResult.Cancel
                        Me.Close()
                    End If

                End If
        End Select
        If bRollBack Then
            MyBase.Rollback()
        End If
    End Sub

#End Region

#Region "PopulateProcesses"

    Private Sub PopulateProcesses(ByVal dataTable As DataTable)
        Dim row As DataRow
        Dim item As ListViewItem

        lvProcess.Items.Clear()
        If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
            lvProcess.View = View.List
            If miWizardType = WizardType.Process Then
                lvProcess.Items.Add("No recoverable processes were found")
            Else
                lvProcess.Items.Add("No recoverable business objects were found")
            End If
        Else
            lvProcess.View = View.Details
            For Each row In dataTable.Rows
                item = New ListViewItem(row("name").ToString)

                If row("backupdate").GetType.Name = GetType(DBNull).Name Then
                    item.SubItems.Add("")
                Else
                    item.SubItems.Add(CDate(row("backupdate")).ToString)
                End If

                If row("restoredate").GetType.Name = GetType(DBNull).Name Then
                    item.SubItems.Add("")
                Else
                    item.SubItems.Add((CDate(row("restoredate")).ToString))
                End If

                'When a process is restored from back up, the current version
                'effectively swaps places with the version in the back up table.
                'The administrator can 'undo' a restore and revert back to the
                'original version if necessary.
                Select Case CType(row("backuplevel"), clsAutoSaver.BackUpLevel)
                    Case clsAutoSaver.BackUpLevel.Session
                        item.SubItems.Add(csSession)
                    Case clsAutoSaver.BackUpLevel.Original
                        item.SubItems.Add(csOriginal)
                    Case clsAutoSaver.BackUpLevel.Restored
                        item.SubItems.Add(csRestored)
                    Case Else
                        item.SubItems.Add("")
                End Select

                item.Tag = row("processid").ToString
                lvProcess.Items.Add(item)
            Next
        End If
    End Sub

#End Region

#Region "Events"

    Private Sub frmRecoverProcess_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetMaxSteps(0)
        Me.Title = "Restore Process from Back Up"
        Me.SubTitle = "Recovers a process backed up in the database"

    End Sub

    Private Sub lvProcess_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvProcess.DoubleClick
        UpdatePage()
    End Sub

#End Region

#Region "Help"

    Public Overrides Function GetHelpFile() As String
        Return "frmProcessRecover.htm"
    End Function

#End Region

End Class