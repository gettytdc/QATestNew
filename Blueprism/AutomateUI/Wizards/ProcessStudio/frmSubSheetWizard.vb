
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : frmSubSheetWizard
''' 
''' <summary>
''' A wizard for creating a process page.
''' </summary>
Friend Class frmSubSheetWizard
    Inherits frmWizard


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
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents lstSubsheets As System.Windows.Forms.ListBox
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnNew As AutomateControls.StyledRadioButton
    Friend WithEvents btnExisting As AutomateControls.StyledRadioButton
    Friend WithEvents btnNewUnreferenced As AutomateControls.StyledRadioButton
    Friend WithEvents Label1 As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSubSheetWizard))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lstSubsheets = New System.Windows.Forms.ListBox()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnExisting = New AutomateControls.StyledRadioButton()
        Me.btnNew = New AutomateControls.StyledRadioButton()
        Me.btnNewUnreferenced = New AutomateControls.StyledRadioButton()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
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
        Me.Panel1.Controls.Add(Me.btnNewUnreferenced)
        Me.Panel1.Controls.Add(Me.btnNew)
        Me.Panel1.Controls.Add(Me.btnExisting)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.Label1)
        Me.Panel2.Controls.Add(Me.lstSubsheets)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'lstSubsheets
        '
        resources.ApplyResources(Me.lstSubsheets, "lstSubsheets")
        Me.lstSubsheets.Name = "lstSubsheets"
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.txtName)
        Me.Panel3.Controls.Add(Me.Label2)
        resources.ApplyResources(Me.Panel3, "Panel3")
        Me.Panel3.Name = "Panel3"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'btnExisting
        '
        resources.ApplyResources(Me.btnExisting, "btnExisting")
        Me.btnExisting.ButtonHeight = 21
        Me.btnExisting.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.btnExisting.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnExisting.FocusDiameter = 16
        Me.btnExisting.FocusThickness = 3
        Me.btnExisting.FocusYLocation = 9
        Me.btnExisting.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.btnExisting.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.btnExisting.MouseLeaveColor = System.Drawing.Color.White
        Me.btnExisting.Name = "btnExisting"
        Me.btnExisting.RadioButtonDiameter = 12
        Me.btnExisting.RadioButtonThickness = 2
        Me.btnExisting.RadioYLocation = 7
        Me.btnExisting.StringYLocation = 1
        Me.btnExisting.TabStop = True
        Me.btnExisting.TextColor = System.Drawing.Color.Black
        Me.btnExisting.UseVisualStyleBackColor = True
        '
        'btnNew
        '
        resources.ApplyResources(Me.btnNew, "btnNew")
        Me.btnNew.ButtonHeight = 21
        Me.btnNew.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.btnNew.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnNew.FocusDiameter = 16
        Me.btnNew.FocusThickness = 3
        Me.btnNew.FocusYLocation = 9
        Me.btnNew.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.btnNew.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.btnNew.MouseLeaveColor = System.Drawing.Color.White
        Me.btnNew.Name = "btnNew"
        Me.btnNew.RadioButtonDiameter = 12
        Me.btnNew.RadioButtonThickness = 2
        Me.btnNew.RadioYLocation = 7
        Me.btnNew.StringYLocation = 1
        Me.btnNew.TabStop = True
        Me.btnNew.TextColor = System.Drawing.Color.Black
        Me.btnNew.UseVisualStyleBackColor = True
        '
        'btnNewUnreferenced
        '
        resources.ApplyResources(Me.btnNewUnreferenced, "btnNewUnreferenced")
        Me.btnNewUnreferenced.ButtonHeight = 21
        Me.btnNewUnreferenced.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.btnNewUnreferenced.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnNewUnreferenced.FocusDiameter = 16
        Me.btnNewUnreferenced.FocusThickness = 3
        Me.btnNewUnreferenced.FocusYLocation = 9
        Me.btnNewUnreferenced.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.btnNewUnreferenced.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.btnNewUnreferenced.MouseLeaveColor = System.Drawing.Color.White
        Me.btnNewUnreferenced.Name = "btnNewUnreferenced"
        Me.btnNewUnreferenced.RadioButtonDiameter = 12
        Me.btnNewUnreferenced.RadioButtonThickness = 2
        Me.btnNewUnreferenced.RadioYLocation = 7
        Me.btnNewUnreferenced.StringYLocation = 1
        Me.btnNewUnreferenced.TabStop = True
        Me.btnNewUnreferenced.TextColor = System.Drawing.Color.Black
        Me.btnNewUnreferenced.UseVisualStyleBackColor = True
        '
        'frmSubSheetWizard
        '
        Me.CancelButton = Nothing
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Panel3)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "frmSubSheetWizard"
        Me.Title = "Add a Page"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.Panel1, 0)
        Me.Controls.SetChildIndex(Me.Panel2, 0)
        Me.Controls.SetChildIndex(Me.Panel3, 0)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private mobjProcess As clsProcess
    Private mobjRadionButton As RadioButton

#Region "constructor"

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mobjProcess = Nothing
    End Sub

#End Region

#Region "SetProcess"

    ''' <summary>
    ''' Sets the parent process.
    ''' </summary>
    ''' <param name="objProcess">The process</param>
    Public Sub SetProcess(ByVal objProcess As clsProcess)
        mobjProcess = objProcess
    End Sub

#End Region

#Region "frmSubSheetWizard_Load"

    Private Sub frmSubSheetWizard_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If mobjProcess Is Nothing Then
            Close()
            Exit Sub
        End If
        SetMaxSteps(1)
        btnExisting.Checked = True
    End Sub

#End Region
#Region "UpdatePage"

    ''' <summary>
    ''' 
    ''' </summary>
    Protected Overrides Sub UpdatePage()

        Dim iPage As Integer
        iPage = GetStep()

        Select Case iPage
            Case 0
                ShowPage(Panel1)
                If Not mobjProcess.HasExtraSheets Then
                    btnExisting.Enabled = False
                    mobjRadionButton = btnNew
                End If
                mobjRadionButton.Checked = True
                Me.Title = My.Resources.frmSubSheetWizard_CreateANewPageOrAReferenceToAnExistingPage
            Case 1
                If btnExisting.Checked Then
                    lstSubsheets.Items.Clear()
                    For Each sh As clsProcessSubSheet In mobjProcess.GetNormalSheets
                        lstSubsheets.Items.Add(sh.Name)
                    Next
                    ShowPage(Panel2)
                    Me.Title = My.Resources.frmSubSheetWizard_ChooseThePageYouWantToReference
                ElseIf btnNew.Checked Or btnNewUnreferenced.Checked Then
                    ShowPage(Panel3)
                    Me.Title = My.Resources.frmSubSheetWizard_CreateANewPage
                    txtName.Focus()
                End If

            Case 2
                If btnExisting.Checked And lstSubsheets.SelectedItems.Count = 0 Then
                    UserMessage.Show(My.Resources.frmSubSheetWizard_YouMustSelectAPageToReference)
                    Rollback()
                    Exit Select
                End If

                If Not btnExisting.Checked Then
                    If txtName.Text = "" Then
                        UserMessage.Show(My.Resources.frmSubSheetWizard_YouMustGiveTheNewPageAName)
                        Rollback()
                        Exit Select
                    End If
                    If txtName.Text = "Main Page" Then
                        UserMessage.Show(My.Resources.frmSubSheetWizard_YouCannotUseMainPageAsANameForANewPage)
                        Rollback()
                        Exit Select
                    End If
                    If Not mobjProcess.GetSubSheetID(txtName.Text).Equals(Guid.Empty) Then
                        UserMessage.Show(My.Resources.frmSubSheetWizard_APageWithThatNameAlreadyExistsPleaseChooseAnother)
                        Rollback()
                        Exit Select
                    End If
                End If

                'DialogResult.Yes has been used to indicate that a new sub-sheet  
                'is to be created but without creating any reference to it
                If btnNewUnreferenced.Checked Then
                    DialogResult = System.Windows.Forms.DialogResult.Yes
                Else
                    DialogResult = System.Windows.Forms.DialogResult.OK
                End If
                Close()
        End Select

    End Sub

#End Region

#Region "GetExisting"

    ''' <summary>
    ''' Indicates if an existing subsheet was selected.
    ''' </summary>
    ''' <returns>True if an existing subsheet was selected, or False
    ''' for a new one</returns>
    Public Function GetExisting() As Boolean
        Return btnExisting.Checked
    End Function

#End Region

#Region "GetName"

    ''' <summary>
    ''' Gets the name selected for a new subsheet, or the name of the existing subsheet.
    ''' </summary>
    ''' <returns>The name</returns>
    Public Function GetName() As String
        If btnExisting.Checked Then
            Return CStr(lstSubsheets.SelectedItem)
        Else
            Return txtName.Text
        End If
    End Function

#End Region

    Private Sub btnExisting_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExisting.CheckedChanged
        If btnExisting.Checked Then
            mobjRadionButton = btnExisting
        End If
    End Sub

    Private Sub btnNew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNew.CheckedChanged
        If btnNew.Checked Then
            mobjRadionButton = btnNew
        End If
    End Sub

    Private Sub btnNewUnreferenced_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNewUnreferenced.CheckedChanged
        If btnNewUnreferenced.Checked Then
            mobjRadionButton = btnNewUnreferenced
        End If
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "HELPMISSING"
    End Function

    Private Sub txtName_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtName.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                Me.NextPage()
        End Select
    End Sub

End Class
