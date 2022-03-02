Imports AutomateControls
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Images

''' Project  : Automate
''' Class    : frmSessionVariables
''' 
''' <summary>
''' Allows editing of a set of session variables.
''' </summary>
Friend Class frmSessionVariables
    Inherits frmForm
    Implements IHelp
    Implements IEnvironmentColourManager

#Region "Windows Forms Designer Generated Code"
    Private WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlMain As System.Windows.Forms.Panel
    Friend WithEvents mobjBlueIconBar As AutomateControls.TitleBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Splitter1 As System.Windows.Forms.Splitter
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents btnModify As AutomateControls.Buttons.StandardStyledButton
    Private components As System.ComponentModel.IContainer


    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSessionVariables))
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.mobjBlueIconBar = New AutomateControls.TitleBar()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Splitter1 = New System.Windows.Forms.Splitter()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnModify = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Name = "btnHelp"
        '
        'mobjBlueIconBar
        '
        resources.ApplyResources(Me.mobjBlueIconBar, "mobjBlueIconBar")
        Me.mobjBlueIconBar.Name = "mobjBlueIconBar"
        '
        'pnlMain
        '
        resources.ApplyResources(Me.pnlMain, "pnlMain")
        Me.pnlMain.Controls.Add(Me.Panel1)
        Me.pnlMain.Controls.Add(Me.Splitter1)
        Me.pnlMain.Name = "pnlMain"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Name = "Panel1"
        '
        'Splitter1
        '
        resources.ApplyResources(Me.Splitter1, "Splitter1")
        Me.Splitter1.Name = "Splitter1"
        Me.Splitter1.TabStop = False
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        '
        'btnModify
        '
        resources.ApplyResources(Me.btnModify, "btnModify")
        Me.btnModify.Name = "btnModify"
        '
        'frmSessionVariables
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnModify)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.mobjBlueIconBar)
        Me.Controls.Add(Me.btnHelp)
        Me.Name = "frmSessionVariables"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.pnlMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
#End Region

    ' The icon to be used to label indeterminate images.
    Private mIndeterminateIcon As Image

    ' The variables being edited
    Private mVars As ICollection(Of clsSessionVariable)

    ''' <summary>
    ''' The variables being edited. This should be set before the form is shown, and
    ''' can then be read when the form has been closed with an 'OK' result.
    ''' </summary>
    Public Property Vars() As ICollection(Of clsSessionVariable)
        Get
            Return mVars
        End Get
        Set(ByVal value As ICollection(Of clsSessionVariable))
            mVars = value
        End Set
    End Property

    ''' <summary>
    ''' We initialise the form, and create new collection objects to hold the sessions
    ''' and processes
    ''' </summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' This returns the help file for the form
    ''' </summary>
    ''' <returns></returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmSessionVariables.htm"
    End Function

    ''' <summary>
    ''' On for load we get the unique processes populate the treeview and populate
    ''' the parameters panel.
    ''' </summary>
    Private Sub frmSessionVariables_Load( _
     ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        mIndeterminateIcon = ToolImages.Notebook_Warning_16x16
        PopulateValues()
    End Sub

    ''' <summary>
    ''' Populate the values panel.
    ''' </summary>
    Private Sub PopulateValues()

        Panel1.Controls.Clear()
        Panel1.SuspendLayout()
        Panel1.Visible = False

        If mVars IsNot Nothing Then

            For Each var As clsSessionVariable In mVars
                Dim c As New ctlProcessValueEdit( _
                 var.Name, var.Value.DataType, mIndeterminateIcon, False)
                c.Indeterminate = var.Indeterminate
                c.Value = var.Value
                c.Tag = var
                AddHandler c.Changed, AddressOf ValueEdited
                Panel1.Controls.Add(c)
            Next

            Dim i As Integer = 8
            For Each c As Control In Me.Panel1.Controls
                c.Top = i
                c.Left = 8
                c.Width = Me.Panel1.ClientSize.Width - 16
                c.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
                i += c.Height + 8
            Next

            'add a dummy padding space to make a gap at the bottom of the list
            Dim pnl As New Panel
            pnl.Height = 0
            pnl.Top = i
            pnl.Left = 8
            pnl.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
            pnl.Width = Me.Panel1.ClientSize.Width - 16
            Me.Panel1.Controls.Add(pnl)

        End If

        If Panel1.Controls.Count > 0 Then Panel1.Controls(0).Focus()

        Panel1.ResumeLayout(True)
        Panel1.Visible = True

    End Sub


    ''' <summary>
    ''' Event handler for the cancel button (closes the form)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
    End Sub

    ''' <summary>
    ''' Callback used by the ctlProcessValueEdit controls to set the value in the
    ''' session
    ''' </summary>
    Private Sub ValueEdited(ByVal sender As Object, ByVal e As EventArgs)
        ' Get the name/value from the edited value
        Dim editCtl As ctlProcessValueEdit = CType(sender, ctlProcessValueEdit)
        Dim val As clsProcessValue = editCtl.Value.Clone()
        Dim sName As String = editCtl.Title
        ' Find the corresponding session var and update it
        For Each var In Me.Vars
            ' This isn't the variable we're looking for
            If var.Name <> sName Then Continue For
            ' Update the description with the one from the current variable
            If var.Value IsNot Nothing Then val.Description = var.Value.Description
            ' And replace the value in the variable with the new one
            var.Value = val
        Next
    End Sub

    ''' <summary>
    ''' The event handler for the save button.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnModify_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnModify.Click
        DialogResult = DialogResult.OK
    End Sub

    ''' <summary>
    ''' The event handler for the help button
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnHelp_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return mobjBlueIconBar.BackColor
        End Get
        Set(value As Color)
            mobjBlueIconBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return mobjBlueIconBar.TitleColor
        End Get
        Set(value As Color)
            mobjBlueIconBar.TitleColor = value
        End Set
    End Property
End Class
