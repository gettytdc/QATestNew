''' Project  : Automate
''' Class    : clsPanel
''' 
''' <summary>
''' A Panel subclass with added border capabilities.
''' </summary>
Public Class clsPanel
    Inherits System.Windows.Forms.Panel

#Region "Member variables"
    ''' <summary>
    ''' Border mode switch.
    ''' </summary>
    Public Enum BorderMode
        [On]
        Off
    End Enum

    Private miBorderMode As BorderMode
    Private mBorderColor As Color
    Private mBorderWidth As Integer
#End Region

#Region "Properties"
    ''' <summary>
    ''' Border style, On or Off.
    ''' </summary>
    ''' <value>The border style</value>
    Public Shadows Property BorderStyle() As BorderMode
        Get
            Return miBorderMode
        End Get
        Set(ByVal Value As BorderMode)
            miBorderMode = Value
        End Set
    End Property

    ''' <summary>
    ''' The border colour.
    ''' </summary>
    ''' <value>The border colour</value>
    Public Property BorderColor() As Color
        Get
            Return mBorderColor
        End Get
        Set(ByVal Value As Color)
            mBorderColor = Value
        End Set
    End Property

    ''' <summary>
    ''' The border width.
    ''' </summary>
    ''' <value>The border width</value>
    Public Property BorderWidth() As Integer
        Get
            Return mBorderWidth
        End Get
        Set(ByVal Value As Integer)
            mBorderWidth = Value
        End Set
    End Property
#End Region

#Region "Constructors"
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        Me.New(System.Drawing.Color.Black, 1, BorderMode.Off)
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or _
           ControlStyles.UserPaint Or _
           ControlStyles.DoubleBuffer, True)
    End Sub

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="borderColor">The border colour</param>
    ''' <param name="borderWidth">The border width</param>
    ''' <param name="mode">The border style</param>
    Public Sub New(ByVal borderColor As Color, ByVal borderWidth As Integer, ByVal mode As BorderMode)
        MyBase.New()
        MyBase.BorderStyle = System.Windows.Forms.BorderStyle.None
        mBorderColor = borderColor
        mBorderWidth = borderWidth
        miBorderMode = mode
    End Sub
#End Region

#Region "DrawBorder"
    ''' <summary>
    ''' Paint event handler to draw the border.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Public Sub DrawBorder(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles MyBase.Paint
        If miBorderMode = BorderMode.On Then
            Dim pen As New Pen(mBorderColor, mBorderWidth)
            e.Graphics.DrawRectangle(pen, New Rectangle(mBorderWidth \ 2, mBorderWidth \ 2, Width - mBorderWidth, Height - mBorderWidth))
        End If
    End Sub

    ''' <summary>
    ''' Resize event handler.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub clsPanel_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Me.Invalidate()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
#End Region


End Class
