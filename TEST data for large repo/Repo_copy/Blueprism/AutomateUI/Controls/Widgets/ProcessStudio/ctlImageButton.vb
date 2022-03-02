Public Class ctlRolloverButton


    ''' <summary>
    ''' The text to be displayed in a tooltip when the user
    ''' hovers on this button, if any.
    ''' </summary>
    Public Property TooltipText() As String
        Get
            Return mToolTip.GetToolTip(Me.pbButtonImage)
        End Get
        Set(ByVal value As String)
            mToolTip.SetToolTip(Me.pbButtonImage, value)
        End Set
    End Property

    ''' <summary>
    ''' The title of the tooltip displayed when the user
    ''' hovers on the button, if any.
    ''' </summary>
    Public Property TooltipTitle() As String
        Get
            Return mToolTip.ToolTipTitle
        End Get
        Set(ByVal value As String)
            mToolTip.ToolTipTitle = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property 
    ''' DefaultImage
    ''' </summary>
    ''' <remarks>The control will be size automatically
    ''' to be 2 pixels larger in each dimension. This
    ''' allows for zooming on mouse rollover.</remarks>
    Private mDefaultImage As Image
    ''' <summary>
    ''' The image to be displayed as default.
    ''' </summary>
    Public Property DefaultImage() As Image
        Get
            Return Me.mDefaultImage
        End Get
        Set(ByVal value As Image)
            Me.mDefaultImage = value
            If Me.Enabled Then
                Me.pbButtonImage.Image = value
                Me.pbButtonImage.BackgroundImageLayout = ImageLayout.Center
            End If
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property RolloverImage()
    ''' </summary>
    Public mRolloverImage As Image
    ''' <summary>
    ''' The image to be displayed when the mouse
    ''' rolls over the button, or nothing if this
    ''' image is to be the same as the DefaultImage
    ''' </summary>
    Public Property RolloverImage() As Image
        Get
            Return mRolloverImage
        End Get
        Set(ByVal value As Image)
            mRolloverImage = value
            If Me.Enabled AndAlso Me.pbButtonImage.Bounds.Contains(System.Windows.Forms.Cursor.Position) Then
                Me.pbButtonImage.Image = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property DisabledImage()
    ''' </summary>
    Public mDisabledImage As Image
    ''' <summary>
    ''' The image to be displayed when the button
    ''' is disabled.
    ''' </summary>
    Public Property DisabledImage() As Image
        Get
            Return mDisabledImage
        End Get
        Set(ByVal value As Image)
            mDisabledImage = value
            If Not Me.Enabled Then
                Me.pbButtonImage.Image = value
            End If
        End Set
    End Property

    Private Sub ctlDocumentationButton_EnabledChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.EnabledChanged
        Me.pbButtonImage.Enabled = Me.Enabled
        If Enabled Then
            If (RolloverImage IsNot Nothing) AndAlso pbButtonImage.Bounds.Contains(System.Windows.Forms.Cursor.Position) Then
                Me.pbButtonImage.Image = RolloverImage
            Else
                Me.pbButtonImage.Image = DefaultImage
            End If
        Else
            Me.pbButtonImage.Image = DisabledImage
        End If
    End Sub

    Private Sub ctlDocumentationButton_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbButtonImage.MouseEnter
        FormatRolloverButtonImage()
    End Sub

    Private Sub ctlDocumentationButton_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbButtonImage.MouseLeave
        If Me.Enabled Then
            Me.AdjustPBSize(-1)
            Me.pbButtonImage.Image = DefaultImage
        End If
    End Sub

    Private Sub btnDetails_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles pbButtonImage.Click
        MyBase.OnClick(e)
    End Sub

    Private Sub ctlRolloverButton_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbButtonImage.MouseDown
        Me.AdjustPBSize(-2)
    End Sub

    Private Sub pbButtonImage_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbButtonImage.MouseUp
        Me.AdjustPBSize(2)
    End Sub

    ''' <summary>
    ''' Adjusts the size of the picture box by the 
    ''' specified amount.
    ''' </summary>
    ''' <param name="Value">The value by which to increase
    ''' the size of the picture box. The increase will be
    ''' two times this value in each direction. Eg for a
    ''' value of 1, the width and height will each be
    ''' increased by 2.</param>
    Private Sub AdjustPBSize(ByVal Value As Integer)
        Me.pbButtonImage.Left -= Value
        Me.pbButtonImage.Top -= Value
        Me.pbButtonImage.Width += 2 * Value
        Me.pbButtonImage.Height += 2 * Value
    End Sub


    Private Sub ctlRolloverButton_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged
        Me.pbButtonImage.Location = New Point(1, 1)
        Me.pbButtonImage.Size = New Size(Me.Width - 2, Me.Height - 2)
    End Sub

    Private Sub ctlRolloverButton_Enter(sender As Object, e As EventArgs) Handles MyBase.Enter
        FormatRolloverButtonImage()
    End Sub

    Private Sub FormatRolloverButtonImage()
        If Me.Enabled AndAlso Me.RolloverImage IsNot Nothing Then
            Me.pbButtonImage.Dock = DockStyle.None
            Me.AdjustPBSize(1)
            Me.pbButtonImage.SizeMode = PictureBoxSizeMode.StretchImage
            Me.pbButtonImage.Image = Me.RolloverImage
        End If
    End Sub

    Private Sub ctlRolloverButton_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Enter Then
            MyBase.OnClick(e)
        End If
    End Sub

    Private Sub ctlRolloverButton_Leave(sender As Object, e As EventArgs) Handles MyBase.Leave
        If Me.Enabled Then
            Me.AdjustPBSize(-1)
            Me.pbButtonImage.Image = DefaultImage
        End If
    End Sub

End Class
