''' Project  : Automate
''' Class    : clsColorPaletteDialog
''' 
''' <summary>
''' 
''' </summary>
Public Class clsColorPaletteDialog
    Inherits UserControl

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        Me.InitilizeComponent()
    End Sub

    Private WithEvents btnMoreColorsButton As New AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnCancelButton As New AutomateControls.Buttons.StandardStyledButton

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

    <System.Diagnostics.DebuggerStepThrough()> Public Sub InitilizeComponent()

        'btnMoreColorsButton
        btnMoreColorsButton.Text = My.Resources.MoreColors
        btnMoreColorsButton.Size = New Size(142, 22)
        btnMoreColorsButton.Location = New Point(5, 99)
        btnMoreColorsButton.FlatStyle = FlatStyle.Popup

        'btnCancelButon

        btnCancelButton.TabIndex = 0
        btnCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnCancelButton.Size = New Size(5, 5)
        btnCancelButton.Location = New Point(-10, -10)

        'clsColorPalateDialog
        Controls.Add(btnCancelButton)
        Controls.Add(btnMoreColorsButton)
        Size = New Size(158, 132)
    End Sub
#End Region

    Private Const constMax As Byte = 40
    Private maobjPanel(constMax) As Panel

    Private mobjColor() As Color = {Drawing.Color.FromArgb(0, 0, 0), Drawing.Color.FromArgb(153, 51, 0), Drawing.Color.FromArgb(51, 51, 0), Drawing.Color.FromArgb(0, 51, 0), Drawing.Color.FromArgb(0, 51, 102), Drawing.Color.FromArgb(0, 0, 128), Drawing.Color.FromArgb(51, 51, 153), Drawing.Color.FromArgb(51, 51, 51), Drawing.Color.FromArgb(128, 0, 0), Drawing.Color.FromArgb(255, 102, 0), Drawing.Color.FromArgb(128, 128, 0), Drawing.Color.FromArgb(0, 128, 0), Drawing.Color.FromArgb(0, 128, 128), Drawing.Color.FromArgb(0, 0, 255), Drawing.Color.FromArgb(102, 102, 153), Drawing.Color.FromArgb(128, 128, 128), Drawing.Color.FromArgb(255, 0, 0), Drawing.Color.FromArgb(255, 153, 0), Drawing.Color.FromArgb(153, 204, 0), Drawing.Color.FromArgb(51, 153, 102), Drawing.Color.FromArgb(51, 204, 204), Drawing.Color.FromArgb(51, 102, 255), Drawing.Color.FromArgb(128, 0, 128), Drawing.Color.FromArgb(153, 153, 153), Drawing.Color.FromArgb(255, 0, 255), Drawing.Color.FromArgb(255, 204, 0), Drawing.Color.FromArgb(255, 255, 0), Drawing.Color.FromArgb(0, 255, 0), Drawing.Color.FromArgb(0, 255, 255), Drawing.Color.FromArgb(0, 204, 255), Drawing.Color.FromArgb(153, 51, 102), Drawing.Color.FromArgb(192, 192, 192), Drawing.Color.FromArgb(255, 153, 204), Drawing.Color.FromArgb(255, 204, 153), Drawing.Color.FromArgb(255, 255, 153), Drawing.Color.FromArgb(204, 255, 204), Drawing.Color.FromArgb(204, 255, 255), Drawing.Color.FromArgb(153, 204, 255), Drawing.Color.FromArgb(204, 153, 255), Drawing.Color.FromArgb(255, 255, 255)}
    Private msColorName() As String = {My.Resources.Black, My.Resources.Brown, My.Resources.OliveGreen, My.Resources.DarkGreen, My.Resources.DarkTeal, My.Resources.DarkBlue, My.Resources.Indigo, My.Resources.Gray80, My.Resources.DarkRed, My.Resources.Orange, My.Resources.DarkYellow, My.Resources.Green, My.Resources.Teal, My.Resources.Blue, My.Resources.BlueGray, My.Resources.Gray50, My.Resources.Red, My.Resources.LightOrange, My.Resources.Lime, My.Resources.SeaGreen, My.Resources.Aqua, My.Resources.LightBlue, My.Resources.Violet, My.Resources.Gray40, My.Resources.Pink, My.Resources.Gold, My.Resources.Yellow, My.Resources.BrightGreen, My.Resources.Turquoise, My.Resources.SkyBlue, My.Resources.Plum, My.Resources.Gray25, My.Resources.Rose, My.Resources.Tan, My.Resources.LightYellow, My.Resources.LightGreen, My.Resources.LightTurquoise, My.Resources.PaleBlue, My.Resources.Lavender, My.Resources.White}

    Private mobjSelectedColor As Color
    Private mobjParentControl As clsColorButton

    Delegate Sub OnColourChosen(ByVal c As Color)
    Public Event ColourChosen As OnColourChosen

    Public Sub SetParentControl(ByVal objParentControl As clsColorButton)
        mobjParentControl = objParentControl
    End Sub


    Public ReadOnly Property Color() As Color
        Get
            Return mobjSelectedColor
        End Get
    End Property

    Private Sub BuildPalette()
        Dim num5 As Byte = 16
        Dim num4 As Byte = 16
        Dim num3 As Byte = 2
        Dim num1 As Byte = 5
        Dim num6 As Integer = num1
        Dim num7 As Integer = num1
        Dim tip1 As New ToolTip
        Dim num8 As Integer = (constMax - 1)
        Dim num2 As Integer = 0
        For num2 = 0 To constMax - 1
            maobjPanel(num2) = New Panel
            maobjPanel(num2).Height = num5
            maobjPanel(num2).Width = num4
            Dim point1 As New Point(num6, num7)
            maobjPanel(num2).Location = point1
            tip1.SetToolTip(maobjPanel(num2), msColorName(num2))
            Controls.Add(maobjPanel(num2))
            If (num6 < (7 * CType((num5 + num3), Byte))) Then
                num6 = (num6 + CType((num5 + num3), Byte))
            Else
                num6 = num1
                num7 = (num7 + CType((num4 + num3), Byte))
            End If
            maobjPanel(num2).BackColor = mobjColor(num2)
            AddHandler maobjPanel(num2).MouseEnter, AddressOf OnMouseEnterPanel
            AddHandler maobjPanel(num2).MouseLeave, AddressOf OnMouseLeavePanel
            AddHandler maobjPanel(num2).MouseDown, AddressOf OnMouseDownPanel
            AddHandler maobjPanel(num2).MouseUp, AddressOf OnMouseUpPanel
            AddHandler maobjPanel(num2).Paint, AddressOf OnPanelPaint
        Next
    End Sub

    Private Sub btnMoreColorsButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnMoreColorsButton.Click
        Dim dialog1 As New ColorDialog
        dialog1.FullOpen = True
        dialog1.Color = mobjSelectedColor
        dialog1.Color = mobjParentControl.CurrentColor
        dialog1.CustomColors = mobjParentControl.CustomColors
        If (dialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK) Then
            mobjSelectedColor = dialog1.Color
            RaiseEvent ColourChosen(mobjSelectedColor)
        End If
        mobjParentControl.CustomColors = dialog1.CustomColors
        dialog1.Dispose()
        mobjParentControl.HidePalette()
    End Sub

    Private Sub btnCancelButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancelButton.Click
        CType(mobjParentControl, clsColorButton).HidePalette()
    End Sub

    Private Sub OnMouseEnterPanel(ByVal sender As Object, ByVal e As EventArgs)
        DrawPanel(sender, 1)
    End Sub

    Private Sub OnMouseLeavePanel(ByVal sender As Object, ByVal e As EventArgs)
        DrawPanel(sender, 0)
    End Sub

    Private Sub OnMouseDownPanel(ByVal sender As Object, ByVal e As MouseEventArgs)
        DrawPanel(sender, 2)
    End Sub

    Private Sub OnMouseUpPanel(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim panel1 As Panel = CType(sender, Panel)
        mobjSelectedColor = panel1.BackColor
        If (Not ColourChosenEvent Is Nothing) Then
            ColourChosenEvent.Invoke(mobjSelectedColor)
        End If
        mobjParentControl.HidePalette()
    End Sub

    Private Sub DrawPanel(ByVal sender As Object, ByVal state As Byte)
        Dim pen1 As Pen
        Dim pen2 As Pen
        Dim panel1 As Panel = CType(sender, Panel)
        Dim graphics1 As Graphics = panel1.CreateGraphics
        If (state = 1) Then
            pen1 = New Pen(SystemColors.ControlLightLight)
            pen2 = New Pen(SystemColors.ControlDarkDark)
        Else
            If (state = 2) Then
                pen1 = New Pen(SystemColors.ControlDarkDark)
                pen2 = New Pen(SystemColors.ControlLightLight)
            Else
                pen1 = New Pen(SystemColors.ControlDark)
                pen2 = New Pen(SystemColors.ControlDark)
            End If
        End If
        Dim rectangle1 As Rectangle = panel1.ClientRectangle
        Dim point1 As New Point(rectangle1.Left, rectangle1.Top)
        Dim point2 As New Point((rectangle1.Right - 1), rectangle1.Top)
        Dim point3 As New Point(rectangle1.Left, (rectangle1.Bottom - 1))
        Dim point4 As New Point((rectangle1.Right - 1), (rectangle1.Bottom - 1))
        graphics1.DrawLine(pen1, point1, point2)
        graphics1.DrawLine(pen1, point1, point3)
        graphics1.DrawLine(pen2, point2, point4)
        graphics1.DrawLine(pen2, point3, point4)
    End Sub

    Private Sub OnPanelPaint(ByVal sender As [Object], ByVal e As PaintEventArgs)
        DrawPanel(sender, 0)
    End Sub

    Private Sub clsColorPaletteDialog_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        BuildPalette()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'clsColorPaletteDialog
        '
        Me.Name = "clsColorPaletteDialog"
        Me.ResumeLayout(False)

    End Sub
End Class

