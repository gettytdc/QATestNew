#Region "ColorButtonEventArgs"
''' Project  : Automate
''' Class    : clsColorButtonEventArgs
''' 
''' <summary>
''' An EventArgs subclass with added colour capabilities.
''' </summary>
Public Class clsColorButtonEventArgs
    Inherits EventArgs

    Private mColor As Color

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="c"></param>
    Public Sub New(ByVal c As Color)
        mColor = c
    End Sub

    ''' <summary>
    ''' The colour.
    ''' </summary>
    ''' <value>The colour</value>
    Public Property Color() As Color
        Get
            Return mColor
        End Get
        Set(ByVal Value As Color)
            mColor = Value
        End Set
    End Property
End Class
#End Region


''' Project  : Automate
''' Class    : clsColorButton
''' 
''' <summary>
''' A Button subclass with added colour capabilities.
''' </summary>
Public Class clsColorButton
    Inherits ToolStripSplitButton
    Private mobjCurrentColor As Color
    Private maiCustomColors As Integer()


    Public Sub New()
        MyBase.New()

        'sets the default colour and makes sure we have an image
        Me.CurrentColor = System.Drawing.Color.Black

        Me.mobjDialog = New clsColorPaletteDialog
        Me.mobjDialog.Size = New Size(DialogWidth, DialogHeight)
        Me.mobjDialog.SetParentControl(Me)

        Dim Host As New ToolStripControlHost(mobjDialog)
        Host.Size = mobjDialog.Size
        Host.Dock = DockStyle.Fill

        Me.DropDownItems.Add(Host)
    End Sub
    ''' <summary>
    ''' The colour dialog that pops up when you click this button.
    ''' </summary>
    Private WithEvents mobjDialog As clsColorPaletteDialog

    Public Event ColorChanged(ByVal sender As Object, ByVal e As clsColorButtonEventArgs)

    ''' <summary>
    ''' The width of the popup form.
    ''' </summary>
    ''' <remarks></remarks>
    Const DialogWidth As Integer = 158
    ''' <summary>
    ''' The height of the popup form.
    ''' </summary>
    ''' <remarks></remarks>
    Const DialogHeight As Integer = 132


    ''' <summary>
    ''' The colour of the button centre.
    ''' </summary>
    ''' <value>The colour</value>
    Public Property CurrentColor() As Color
        Get
            Return mobjCurrentColor
        End Get
        Set(ByVal Value As Color)
            If Not Me.mobjCurrentColor = Value Then
                mobjCurrentColor = Value
                Dim b As New Bitmap(16, 16)
                Dim g As Graphics = Graphics.FromImage(b)
                g.Clear(Value)
                Me.Image = b
                Me.Invalidate()
            End If
        End Set
    End Property

    ''' <summary>
    ''' The button's custom colours.
    ''' </summary>
    ''' <value>The colours</value>
    Public Property CustomColors() As Integer()
        Get
            Return maiCustomColors
        End Get
        Set(ByVal Value As Integer())
            maiCustomColors = Value
        End Set
    End Property

    ''' <summary>
    ''' Hides the drop-down portion of the control, which
    ''' reveals the color palette.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub HidePalette()
        MyBase.HideDropDown()
    End Sub

    Private Sub PaletteColourChosen(ByVal c As Color) Handles mobjDialog.ColourChosen
        CurrentColor = c
        RaiseEvent ColorChanged(Me, New clsColorButtonEventArgs(c))
    End Sub

End Class
