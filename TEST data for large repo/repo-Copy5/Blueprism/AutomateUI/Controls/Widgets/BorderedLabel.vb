Partial Public Class BorderedLabel
    Inherits Label

    Private _borderColor As Color = Color.Black
    Private _borderWidth As Integer =2
    Private _borderStyle As ButtonBorderStyle  = ButtonBorderStyle.solid


    Public Property BorderColor() As Color
        Get
            Return _borderColor
        End Get
        Set
            _borderColor = Value
        End Set
    End Property

    Public Property BorderWidth() As Integer
        Get
            Return _borderWidth
        End Get
        Set
            _borderWidth = value
        End Set
    End Property

    Public Overloads Property BorderStyle As ButtonBorderStyle
        Get
            Return _borderStyle
        End Get
        Set
            _borderStyle = value
        End Set
    End Property

    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)

        ControlPaint.DrawBorder(e.Graphics, Me.DisplayRectangle,   _borderColor,
                                _borderWidth,
                                _borderStyle,
                                _borderColor,
                                _borderWidth,
                                _borderStyle,
                                _borderColor,
                                _borderWidth,
                                _borderStyle,
                                _borderColor,
                                _borderWidth,
                                _borderStyle)
        
    End Sub

End Class
