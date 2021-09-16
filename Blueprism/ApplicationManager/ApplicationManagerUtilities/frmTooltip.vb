
Imports system.windows.forms
Imports System.Drawing

Public Class frmToolTip
    Inherits System.Windows.Forms.Form

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
    Private WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents myMagnifyingGlass As BluePrism.ApplicationManager.ApplicationManagerUtilities.ctlMagnifyingGlass

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private lblMessage As Label
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmToolTip))
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.myMagnifyingGlass = New BluePrism.ApplicationManager.ApplicationManagerUtilities.ctlMagnifyingGlass()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblMessage
        '
        resources.ApplyResources(Me.lblMessage, "lblMessage")
        Me.lblMessage.Name = "lblMessage"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'myMagnifyingGlass
        '
        Me.myMagnifyingGlass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.myMagnifyingGlass, "myMagnifyingGlass")
        Me.myMagnifyingGlass.Name = "myMagnifyingGlass"
        Me.myMagnifyingGlass.PixelSize = 5
        Me.myMagnifyingGlass.PosAlign = System.Drawing.ContentAlignment.TopLeft
        Me.myMagnifyingGlass.PosFormat = "[#xg, #yg], (#xl, #yl)"
        Me.myMagnifyingGlass.ShowPixel = True
        Me.myMagnifyingGlass.ShowPosition = True
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "info.gif")
        Me.ImageList1.Images.SetKeyName(1, "warning.gif")
        Me.ImageList1.Images.SetKeyName(2, "error.gif")
        '
        'frmToolTip
        '
        resources.ApplyResources(Me, "$this")
        Me.BackColor = System.Drawing.SystemColors.Info
        Me.Controls.Add(Me.myMagnifyingGlass)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmToolTip"
        Me.ShowInTaskbar = False
        Me.TopMost = True
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private WithEvents mobjTimer As Windows.Forms.Timer
    ''' <summary>
    ''' Private member to store public property AutoPosition()
    ''' </summary>
    Private mbAutoPosition As Boolean = True
    ''' <summary>
    ''' Determines whether the form should be auto positioned
    ''' to avoid the current mouse cursor.
    ''' </summary>
    ''' <remarks>Defaults to true.</remarks>
    Public Property AutoPosition() As Boolean
        Get
            Return mbAutoPosition
        End Get
        Set(ByVal value As Boolean)
            mbAutoPosition = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property ShowMagnifyingGlass
    ''' </summary>
    Private mbShowMagnifyingGlass As Boolean = False
    ''' <summary>
    ''' Determines whether the form display the magnifying glass
    ''' control.
    ''' </summary>
    ''' <remarks>Defaults to false.</remarks>
    Public Property ShowMagnifyingGlass() As Boolean
        Get
            Return mbShowMagnifyingGlass
        End Get
        Set(ByVal value As Boolean)
            mbShowMagnifyingGlass = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property TooltipIcon
    ''' </summary>
    ''' <remarks></remarks>
    Private mIcon As ToolTipIcon
    ''' <summary>
    ''' The icon being used on the tooltip.
    ''' </summary>
    Public Property ToolTipIcon() As ToolTipIcon
        Get
            Return mIcon
        End Get
        Set(ByVal value As ToolTipIcon)
            If Not mIcon = value Then
                mIcon = value
                If mIcon = Windows.Forms.ToolTipIcon.None Then
                    Me.PictureBox1.Image = Nothing
                Else
                    Me.PictureBox1.Image = Me.ImageList1.Images(CInt(mIcon) - 1)
                End If
            End If
        End Set
    End Property


    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.BackColor = Me.TooltipBackColors(0)
        Me.ToolTipIcon = Windows.Forms.ToolTipIcon.Info
        Me.Show()
        Me.mobjTimer = New Timer
        Me.mobjTimer.Interval = 1000
        mobjTimer.Start()
    End Sub

    ''' <summary>
    ''' '''Handles mouse entering the tooltip.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub HandleMouseEnter(ByVal sender As Object, ByVal e As EventArgs) Handles Me.MouseEnter
        Me.PositionTooltip()
    End Sub

    ''' <summary>
    ''' Refresh the magnifying glass
    ''' </summary>>
    ''' <param name="GlobalCursorPos">The location of the mouse cursor, relative to the screen.</param>
    ''' <param name="LocalCursorPos">The location of the mouse cursor, relative to its containing window.</param>
    Public Sub RefreshMagnifyingGlass(ByVal GlobalCursorPos As Point, ByVal LocalCursorPos As Point)
        If mbShowMagnifyingGlass = True Then
            myMagnifyingGlass.RefreshImage(GlobalCursorPos, LocalCursorPos)
        End If
    End Sub

    ''' <summary>
    ''' Chooses a new location for the tooltip, based on
    ''' the location of the mouse cursor's location.
    ''' </summary>
    Public Sub PositionTooltip()
        If mbAutoPosition Then

        'Gets the bounds of the desktop
        Dim DesktopBounds As System.Drawing.Rectangle = Screen.GetWorkingArea(DesktopBounds)

            'Potential locations for the tooltip
            Dim TopLeft As Point = New Point(0, 0)
            Dim TopRight As Point = New Point(DesktopBounds.Width - Me.Width, 0)

            If Me.Bounds.Contains(Windows.Forms.Cursor.Position) Then
                Select Case True
                    Case Me.Location.Equals(TopLeft)
                        Me.Location = TopRight
                    Case Me.Location.Equals(TopRight)
                        Me.Location = TopLeft
                End Select
            End If
        End If
    End Sub


    ''' <summary>
    ''' Sets the text on the tooltip.
    ''' </summary>
    ''' <param name="Text">The new text for the tooltip.
    ''' </param>
    Public Sub SetToolTip(ByVal Title As String, ByVal Text As String)
        'Find out how big the message is
        Dim MaxSize As Size = New Size(400, 250)
        Dim g As Graphics = Me.CreateGraphics
        Dim newStringFormat As New StringFormat
        Dim chars, lines As Integer
        Dim MessageSize As SizeF = g.MeasureString(Text, Me.lblMessage.Font, MaxSize, newStringFormat, chars, lines)

        'Adjust the size of the form to match
        Me.Width += CInt(MessageSize.Width - lblMessage.Width)
        Me.Height += CInt(MessageSize.Height - lblMessage.Height)

        'Update text and colours
        Me.lblMessage.Text = Text
        Me.Label1.Text = Title

        'Update magnifying glass size and position
        If mbShowMagnifyingGlass Then
            Me.myMagnifyingGlass.Visible = True
            Me.Height += 80
            Me.myMagnifyingGlass.Visible = True
            Me.myMagnifyingGlass.Top = Me.Height - 79
            Me.myMagnifyingGlass.Left = 2
            Me.myMagnifyingGlass.Width = Me.Width - 4
            If Me.myMagnifyingGlass.Width Mod 2 = 1 Then Me.myMagnifyingGlass.Width -= 1
        Else
            Me.myMagnifyingGlass.Visible = False
        End If
    End Sub

    ''' <summary>
    ''' The possible different backcolours that we use.
    ''' </summary>
    ''' <remarks></remarks>
    Private TooltipBackColors As Color() = {SystemColors.Info, Color.BlanchedAlmond, Color.Salmon}

    ''' <summary>
    ''' Changes the backcolor of the tooltip to something
    ''' different from its current color
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ChangeTooltipBackColor(ByVal NewColor As Color)
        If NewColor = Color.Empty Then
            Dim CurrentColour As Integer = System.Array.IndexOf(TooltipBackColors, Me.BackColor)
            CurrentColour = (CurrentColour + 1 + TooltipBackColors.Length) Mod TooltipBackColors.Length

            Me.BackColor = TooltipBackColors(CurrentColour)
        Else
            Me.BackColor = NewColor
        End If
    End Sub

    Public Sub ChangeTooltipBackColor()
        ChangeTooltipBackColor(Color.Empty)
    End Sub


    ''' <summary>
    ''' Determines whether the tooltip is visible
    ''' to the user.
    ''' </summary>
    Public Property ToolTipActive() As Boolean
        Get
            Return Me.Visible
        End Get
        Set(ByVal value As Boolean)
            Me.Visible = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the timer tick event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mobjTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles mobjTimer.Tick
        Me.PositionTooltip()
    End Sub
End Class
