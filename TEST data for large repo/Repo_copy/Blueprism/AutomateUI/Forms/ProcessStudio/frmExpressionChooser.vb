Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmExpressionChooser : Inherits Forms.HelpButtonForm
    Implements IHelp, IChild, IEnvironmentColourManager

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.objBlueBar.Title = My.Resources.frmExpressionChooser_CreateAnExpressionUsingDragDropOperations
    End Sub

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
    Friend WithEvents mExpressionBuilder As AutomateUI.ctlProcessExpressionBuilder
    Friend WithEvents objBlueBar As AutomateControls.TitleBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmExpressionChooser))
        Me.objBlueBar = New AutomateControls.TitleBar()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.mExpressionBuilder = New AutomateUI.ctlProcessExpressionBuilder()
        Me.SuspendLayout()
        '
        'objBlueBar
        '
        resources.ApplyResources(Me.objBlueBar, "objBlueBar")
        Me.objBlueBar.Name = "objBlueBar"
        Me.objBlueBar.Title = "Create an expression using drag drop operations"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'mExpressionBuilder
        '
        resources.ApplyResources(Me.mExpressionBuilder, "mExpressionBuilder")
        Me.mExpressionBuilder.ExpressionText = ""
        Me.mExpressionBuilder.Name = "mExpressionBuilder"
        Me.mExpressionBuilder.StoreInText = ""
        Me.mExpressionBuilder.StoreInVisible = True
        '
        'frmExpressionChooser
        '
        Me.AcceptButton = Me.btnOK
        resources.ApplyResources(Me, "$this")
        Me.CancelButton = Me.btnCancel
        Me.Controls.Add(Me.mExpressionBuilder)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.objBlueBar)
        Me.HelpButton = True
        Me.Name = "frmExpressionChooser"
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' The expression displayed on the form.
    ''' </summary>
    ''' <value>Value.</value>
    Public Property Expression() As String
        Get
            Return mExpressionBuilder.ExpressionText
        End Get
        Set(ByVal Value As String)
            mExpressionBuilder.ExpressionText = Value
        End Set
    End Property

    ''' <summary>
    ''' The stage to use as scope etc when building expression. Corresponds to
    ''' setstage method on expression builder control.
    ''' </summary>
    ''' <value></value>
    Public WriteOnly Property Stage() As BluePrism.AutomateProcessCore.clsProcessStage
        Set(ByVal Value As BluePrism.AutomateProcessCore.clsProcessStage)
            Me.mExpressionBuilder.SetStage(Value)
        End Set
    End Property

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Overrides Function GetHelpFile() As String
        Return "helpCalculationsAndDecisions.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        DialogResult = System.Windows.Forms.DialogResult.OK
        Close()
    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objBlueBar.BackColor
        End Get
        Set(value As Color)
            objBlueBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objBlueBar.TitleColor
        End Get
        Set(value As Color)
            objBlueBar.TitleColor = value
        End Set
    End Property
End Class
