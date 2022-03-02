
Imports System.Globalization
Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : frmTime
''' 
''' <summary>
''' A form enabling users to select time values.
''' </summary>
Friend Class frmTime
    Inherits frmForm

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'fr and de - do not use AM/PM format
        Dim parentUICulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        If parentUICulture.Equals("fr") OrElse parentUICulture.Equals("de") Then
            btnAMPM.Hide()
            Dim width = Me.Size.Width
            Dim height = Me.Size.Height
            Me.Size = New Size(CType(width + width / 4, Integer), CType(height + height / 4, Integer))
            maxHoursClock = TwentyFourHourClock
        End If

        'Add any initialization after the InitializeComponent() call

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
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents btnAMPM As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtTime As MaskedTextBox
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTime))
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.txtTime = New System.Windows.Forms.MaskedTextBox()
        Me.btnAMPM = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        CType(Me.PictureBox1,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.BackColor = System.Drawing.SystemColors.Control
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = false
        '
        'txtTime
        '
        resources.ApplyResources(Me.txtTime, "txtTime")
        Me.txtTime.BackColor = System.Drawing.Color.White
        Me.txtTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtTime.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals
        Me.txtTime.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite
        Me.txtTime.Name = "txtTime"
        Me.txtTime.SkipLiterals = false
        Me.txtTime.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals
        '
        'btnAMPM
        '
        resources.ApplyResources(Me.btnAMPM, "btnAMPM")
        Me.btnAMPM.BackColor = System.Drawing.Color.White
        Me.btnAMPM.Name = "btnAMPM"
        Me.btnAMPM.UseVisualStyleBackColor = false
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.BackColor = System.Drawing.Color.White
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = false
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.BackColor = System.Drawing.Color.White
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = false
        '
        'frmTime
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnAMPM)
        Me.Controls.Add(Me.txtTime)
        Me.Controls.Add(Me.PictureBox1)
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "frmTime"
        Me.ShowInTaskbar = false
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        CType(Me.PictureBox1,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

#End Region

    ''' <summary>
    ''' The hands of the clock
    ''' </summary>
    Private Enum Hand
        None
        Hour
        Min
        Secs
    End Enum

    ''' <summary>
    ''' The hand which is currently selected in the user interface.
    ''' </summary>
    Private Selected As Hand
    ''' <summary>
    ''' The pen used to render the Hour hand.
    ''' </summary>
    Private hourpen As New Pen(Color.Black, 2)
    ''' <summary>
    ''' The pen used to render the minute hand.
    ''' </summary>
    Private minpen As New Pen(Color.Black, 2)
    ''' <summary>
    ''' The pen used to render the seconds hand.
    ''' </summary>
    Private secpen As New Pen(Color.LightBlue, 1)
    Private mbOKSelected As Boolean
    ''' <summary>
    ''' Record of the time before the user had the opportunity to make changes.
    ''' </summary>
    Private mdOriginalTime As TimeSpan
    ''' <summary>
    ''' The number of seconds in twelve hours
    ''' </summary>
    Private Const iNoonSeconds As Integer = 12 * 60 * 60

    Private Sub PictureBox1_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBox1.Paint

        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        'cx and cy are the centre of the circle
        Dim EdgeBorder As Integer = 2        'recommend that this be at least 2 since when halving it we want it to be at least 1
        Dim cx As Single = EdgeBorder + (PictureBox1.Width - 2 * EdgeBorder) \ 2
        Dim cy As Single = EdgeBorder + (PictureBox1.Height - 2 * EdgeBorder) \ 2

        'an estimate of the numeral sizes resulting under the fonts below
        Const WidthOfNumerals As Integer = 10
        'The radius of the circle formed by the inner edges of the numerals
        Dim InnerRadius As Single = (Math.Min(PictureBox1.Width, PictureBox1.Height) - 2 * EdgeBorder) \ 2 - WidthOfNumerals
        'the radius of the whole clockface
        Dim OuterRadius As Integer = CInt(InnerRadius + WidthOfNumerals)


        'prepare circle of clock face
        Dim path As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
        path.AddEllipse(cx - OuterRadius, cy - OuterRadius, 2 * OuterRadius, 2 * OuterRadius)
        Dim pathGradient As Drawing2D.PathGradientBrush = New Drawing2D.PathGradientBrush(path)
        pathGradient.CenterColor = System.Drawing.Color.Silver
        pathGradient.SurroundColors = New Color() {System.Drawing.Color.SlateGray}
        'fill the clockface with a pattern
        g.FillPath(pathGradient, path)
        'draw the outer circle
        g.DrawPath(Pens.Black, path)
        'draw the inner circle
        g.DrawEllipse(Pens.LightBlue, cx - InnerRadius, cy - InnerRadius, 2 * InnerRadius, 2 * InnerRadius)



        'draw the numbers on
        Dim F As Font = New Font("Segoe UI", 10, FontStyle.Bold)
        Dim NumeralSize As SizeF

        For i As Integer = 1 To maxHoursClock
            Dim angle As Double = i * Rad(360 / maxHoursClock)

            Dim strHour As String = Nothing
            If i = TwentyFourHourClock Then
                strHour = 0.ToString("00")
            Else
                strHour = i.ToString()
            End If

            NumeralSize = g.MeasureString(strHour, F)
            Dim X As Double = InnerRadius * System.Math.Sin(angle) - CInt(NumeralSize.Width) \ 2
            Dim Y As Double = InnerRadius * System.Math.Cos(angle) + CInt(NumeralSize.Height) \ 2
            g.DrawString(strHour, F, Brushes.Black, cx + CSng(X), cy - CSng(Y))
        Next

        'figure out the lengths and positions of the hands
        Dim LengthofHourHand As Single = InnerRadius - 25
        Dim LengthofMinHand As Single = InnerRadius - 10
        Dim LengthofSecHand As Single = InnerRadius - 5
        Dim hourRadian As Double = (mTime.Hours Mod maxHoursClock) * Rad(360 / maxHoursClock)
        Dim minRadian As Double = mTime.Minutes * Rad(360 / 60)
        Dim secRadian As Double = mTime.Seconds * Rad(360 / 60)


        'now draw the hands
        hourpen.StartCap = Drawing2D.LineCap.Round
        hourpen.EndCap = Drawing2D.LineCap.Round
        minpen.StartCap = Drawing2D.LineCap.Round
        minpen.EndCap = Drawing2D.LineCap.Round
        DrawHand(e.Graphics, hourpen, LengthofHourHand, hourRadian)
        DrawHand(e.Graphics, minpen, LengthofMinHand, minRadian)
        DrawHand(e.Graphics, secpen, LengthofSecHand, secRadian)
    End Sub


    Private Sub DrawHand(ByVal g As Graphics, ByVal p As Pen, ByVal length As Double, ByVal angle As Double)
        Dim X As Double = length * System.Math.Sin(angle)
        Dim Y As Double = length * System.Math.Cos(angle)

        Dim cx As Single = PictureBox1.Width \ 2
        Dim cy As Single = PictureBox1.Height \ 2

        g.DrawLine(p, cx, cy, cx + CSng(X), cy - CSng(Y))
        'g.FillEllipse(Brushes.White, cx + CSng(X) - 5, cy - CSng(Y) - 5, 10, 10)
        'g.DrawEllipse(Pens.Black, cx + CSng(X) - 5, cy - CSng(Y) - 5, 10, 10)
    End Sub

    Private Sub PictureBox1_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDown
        Dim aRadian As Double = ConvertBackToPolar(e.X, e.Y)
        GetNearHand(sender, e, Selected)
    End Sub


    Private Function ConvertBackToPolar(ByVal x As Integer, ByVal y As Integer) As Double
        Dim cx As Double = PictureBox1.Width / 2
        Dim cy As Double = PictureBox1.Height / 2
        Dim a As Double = x - cx
        Dim o As Double = y - cy
        Dim aRadian As Double
        If a = 0 And o = 0 Then
            aRadian = 0
        Else

            aRadian = Math.Atan(o / a)

            'If adjacent is positive then add 180degree offset
            'This effectivly mirrors the clock
            If a >= 0 Then
                aRadian += Rad(180)
            End If

            '0degrees on a polar coordinate system is at 90degrees on the clock
            'so remove 90degree offset
            aRadian -= Rad(90)

            'We don't want negative angles like -90degrees what we actually want is 
            '270degrees
            If aRadian <= 0 Then
                aRadian = Rad(360) + aRadian
            End If
        End If
        Return aRadian

    End Function

    Private Sub PictureBox1_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseMove

        hourpen.Color = AutomateControls.ColourScheme.Default.ClockHourHand
        minpen.Color = AutomateControls.ColourScheme.Default.ClockMinuteHand
        secpen.Color = AutomateControls.ColourScheme.Default.ClockSecondHand
        Dim aRadian As Double = ConvertBackToPolar(e.X, e.Y)
        Select Case Selected
            Case Hand.Hour
                hourpen.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                Dim iHour As Integer
                iHour = CInt(aRadian / Rad(360 / maxHoursClock))
                If maxHoursClock = TwelveHourClock Then
                    If Not mbAM Then
                        iHour = (iHour + 12) Mod 24
                        If iHour = 0 Then iHour = 12
                    Else
                        If iHour = 12 Then iHour = 0
                    End If
                End If
                mTime = New TimeSpan(iHour, mTime.Minutes, mTime.Seconds)
            Case Hand.Min
                minpen.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                Dim iMins As Integer
                iMins = CInt(aRadian / Rad(360 / 60)) Mod 60
                mTime = New TimeSpan(mTime.Hours, iMins, mTime.Seconds)
            Case Hand.Secs
                secpen.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                Dim iSecs As Integer
                iSecs = CInt(aRadian / Rad(360 / 60)) Mod 60
                mTime = New TimeSpan(mTime.Hours, mTime.Minutes, iSecs)
            Case Hand.None
                Dim NearHand As AutomateUI.frmTime.Hand
                GetNearHand(sender, e, NearHand)
                Select Case NearHand
                    Case Hand.Hour
                        hourpen.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                    Case Hand.Min
                        minpen.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                    Case Hand.Secs
                        secpen.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                End Select
        End Select

        mbsettingtext = True
        'The time string must be formatted this way to fit with the masked textbox mask value
        txtTime.Text = (New DateTime).Add(mTime).ToString("HH:mm:ss")
        mbsettingtext = False
        PictureBox1.Invalidate()
    End Sub



    ''' <summary>
    ''' Convert an angle from Degrees into radians.
    ''' </summary>
    ''' <param name="angle"></param>
    ''' <returns></returns>
    Private Function Rad(ByVal angle As Double) As Double
        Return angle * Math.PI / 180
    End Function

    'Private Sub frmTime_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    '   If mTime = Nothing Then
    '       SelectedDate = Now
    '   End If
    'End Sub

    Private Sub PictureBox1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseUp
        Selected = Hand.None
    End Sub

    Private mTime As TimeSpan

    ''' <summary>
    ''' Gets or sets the selected data value.
    ''' </summary>
    ''' <value>The date</value>
    Public Property SelectedTime() As TimeSpan
        Get
            Return mTime
        End Get
        Set(ByVal Value As TimeSpan)
            mTime = Value
            mdOriginalTime = Value
            mbsettingtext = True
            Dim d As DateTime = (New DateTime).Add(Value)
            'The time string must be formatted this way to fit with the masked textbox mask value
            txtTime.Text = d.ToString("HH:mm:ss")
            mbsettingtext = False
            If d.TimeOfDay.TotalSeconds >= iNoonSeconds Then
                mbAM = False
                btnAMPM.Text = My.Resources.PM
            Else
                mbAM = True
                btnAMPM.Text = My.Resources.AM
            End If
            Me.PictureBox1.Invalidate()           'causes us to redraw time onto clock face
        End Set
    End Property

    Private mbAM As Boolean = True
    Private mbsettingtext As Boolean

    Private Const TwelveHourClock = 12
    Private Const TwentyFourHourClock = 24
    Private maxHoursClock As Integer = TwelveHourClock

    Private Sub btnAMPM_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAMPM.Click
        If mbAM Then
            mbAM = False
            btnAMPM.Text = My.Resources.PM
        Else
            mbAM = True
            btnAMPM.Text = My.Resources.AM
        End If

        Dim iHour As Integer = mTime.Hours
        iHour = (iHour + 12) Mod 24
        mTime = New TimeSpan(iHour, mTime.Minutes, mTime.Seconds)

        mbsettingtext = True
        'The time string must be formatted this way to fit with the masked textbox mask value
        txtTime.Text = (DateTime.MinValue).Add(mTime).ToString("HH:mm:ss")
        mbsettingtext = False
    End Sub

    ''' <summary>
    ''' Uses mouse movement arguments to provide the hand currently near the mouse
    ''' position
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e">mouseeventargs</param>
    ''' <param name="NearHand">AutomateUI.frmTime.Hand</param>
    ''' <remarks>
    ''' This code was originally in the mousedown event but moved into its own 
    ''' routine as I also needed to use it in the mousemove event.
    ''' </remarks>
    Private Sub GetNearHand(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs, ByRef NearHand As AutomateUI.frmTime.Hand)
        Dim aRadian As Double = ConvertBackToPolar(e.X, e.Y)

        'see if we have hit the seconds hand with the mouse exactly
        Dim iSecsProximity As Integer = clsUtility.MathsUtil.ModularAbsDifference(CInt(aRadian / Rad(360 / 60)), mTime.Seconds, 60)
        If iSecsProximity = 0 Then
            NearHand = Hand.Secs
            Exit Sub
        End If

        'see if we have hit the minutes hand with the mouse exactly
        Dim iMinsProximity As Integer = clsUtility.MathsUtil.ModularAbsDifference(CInt(aRadian / Rad(360 / 60)), mTime.Minutes, 60)
        If iMinsProximity = 0 Then
            NearHand = Hand.Min
            Exit Sub
        End If

        'see if we have hit the hour hand with the mouse exactly
        Dim iHourProximity As Integer = clsUtility.MathsUtil.ModularAbsDifference(CInt(aRadian / Rad(360 / maxHoursClock)), mTime.Hours Mod maxHoursClock, 60)
        If iHourProximity = 0 Then
            NearHand = Hand.Hour
            Exit Sub
        End If

        'if we didn't hit anything exactly, choose the nearest hand if it is sufficienltly close
        If NearHand = Hand.None Then
            Dim MinDistanceAway As Integer = CInt(clsUtility.MathsUtil.Min(iSecsProximity, iMinsProximity, iHourProximity))
            If MinDistanceAway <= 2 Then
                Select Case MinDistanceAway
                    Case iSecsProximity
                        NearHand = Hand.Secs
                    Case iMinsProximity
                        NearHand = Hand.Min
                    Case iHourProximity
                        NearHand = Hand.Hour
                End Select
            End If
        End If
    End Sub

    Private Sub frmTime_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        If Not mbOKSelected Then
            mTime = mdOriginalTime
        End If
    End Sub

    Private Sub btnOK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOK.Click
        mbOKSelected = True
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub txtTime_KeyPress(ByVal sender As Object, ByVal e As EventArgs) Handles txtTime.TextChanged
        If Not mbsettingtext Then
            Dim dt As DateTime = Nothing
            If DateTime.TryParse(txtTime.Text, dt) Then
                Me.SelectedTime = New TimeSpan(dt.Hour, dt.Minute, dt.Second)
            End If
        End If
    End Sub
End Class
