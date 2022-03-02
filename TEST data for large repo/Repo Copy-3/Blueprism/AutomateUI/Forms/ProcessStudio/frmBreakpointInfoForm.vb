Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : frmBreakpointInfoForm
''' 
''' <summary>
''' Gives info about a breakpoint, including condition etc.
''' </summary>
Friend Class frmBreakpointInfoForm
    Inherits frmForm


#Region " Windows Form Designer generated code "

    Public Sub New(ByVal BPI As clsProcessBreakpointInfo)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.Text = My.Resources.frmBreakpointInfoForm_BreakpointReached
        Me.Label2.Text = BPI.Message
        Me.StartPosition = FormStartPosition.Manual
        Select Case BPI.BreakPointType
            Case clsProcessBreakpoint.BreakEvents.WhenConditionMet, clsProcessBreakpoint.BreakEvents.WhenDataValueChanged
                Me.rtbExpression.Text = clsExpression.NormalToLocal(BPI.Condition)
                Me.rtbExpression.ColourText()
            Case Else
                Me.rtbExpression.Visible = False
                Me.Label1.Visible = False
        End Select

        'make sure that caret (blinking cursor) does not appear on textbox
        Me.HideRichTextBoxCaret()
        AddHandler rtbExpression.TextChanged, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.GotFocus, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.LostFocus, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.Enter, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.Leave, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.MouseEnter, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.MouseLeave, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.MouseDown, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.MouseUp, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.SelectionChanged, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.TextChanged, AddressOf HideRichTextBoxCaret
        AddHandler rtbExpression.VisibleChanged, AddressOf HideRichTextBoxCaret

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
    Friend WithEvents rtbExpression As ctlExpressionRichTextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBreakpointInfoForm))
        Me.rtbExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'rtbExpression
        '
        Me.rtbExpression.AllowDrop = True
        resources.ApplyResources(Me.rtbExpression, "rtbExpression")
        Me.rtbExpression.BackColor = System.Drawing.Color.AliceBlue
        Me.rtbExpression.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.rtbExpression.DetectUrls = False
        Me.rtbExpression.HideSelection = False
        Me.rtbExpression.HighlightingEnabled = True
        Me.rtbExpression.Name = "rtbExpression"
        Me.rtbExpression.PasswordChar = ChrW(0)
        Me.rtbExpression.ReadOnly = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'frmBreakpointInfoForm
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.rtbExpression)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.Name = "frmBreakpointInfoForm"
        Me.ResumeLayout(False)

    End Sub

#End Region


#Region "Caret hiding"

    ''' <summary>
    ''' External call. Hides the caret in a text box.
    ''' </summary>
    ''' <param name="hwnd">handle of the textbox.</param>
    ''' <returns>Returns 0.</returns>
    Private Declare Function HideCaret Lib "user32" _
    (ByVal hwnd As IntPtr) As Integer

    ''' <summary>
    ''' Hides the caret in the rich text box
    ''' </summary>
    Private Sub HideRichTextBoxCaret()
        Try
            HideCaret(Me.rtbExpression.Handle)
        Catch
            'never mind.
        End Try
    End Sub

    Private Sub HideRichTextBoxCaret(ByVal sender As Object, ByVal e As EventArgs)
        Me.HideRichTextBoxCaret()
    End Sub

    Private Sub HideRichTextBoxCaret(ByVal sender As Object, ByVal e As MouseEventArgs)
        Me.HideRichTextBoxCaret()
    End Sub

    Private Sub frmBreakpointInfoForm_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.HideRichTextBoxCaret()
    End Sub

#End Region


End Class
