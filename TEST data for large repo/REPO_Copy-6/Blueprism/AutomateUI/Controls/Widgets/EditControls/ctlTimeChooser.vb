Imports BluePrism.AutomateAppCore.clsUtility

''' Project  : Automate
''' Class    : ctlTimeChooser
''' 
''' <summary>
''' Implements the TimeChooser form as a control, meaning it can be added to other
''' forms/dialogs. Slightly reworked as a result.
''' </summary>
Public Class ctlTimeChooser

#Region "Enums and Constants"

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
    ''' The pen used to render the Hour hand.
    ''' </summary>
    Private Shared ReadOnly PenHour As New Pen(Color.Black, 2)
    ''' <summary>
    ''' The pen used to render the minute hand.
    ''' </summary>
    Private Shared ReadOnly PenMinute As New Pen(Color.Black, 2)
    ''' <summary>
    ''' The pen used to render the seconds hand.
    ''' </summary>
    Private Shared ReadOnly PenSecond As New Pen(Color.LightBlue, 1)

    ''' <summary>
    ''' The font used to draw the numbers on the clock.
    ''' </summary>
    Private Shared ReadOnly FontNumbers As New Font("Segoe UI", 10, FontStyle.Bold)

#End Region

#Region "Member Variables"

    ''' <summary>
    ''' The hand which is currently selected in the user interface.
    ''' </summary>
    Private Selected As Hand

    ''' <summary>
    ''' Record of the time before the user had the opportunity to make changes.
    ''' </summary>
    Private mdOriginalTime As TimeSpan

    ''' <summary>
    ''' Is this time in the morning?
    ''' </summary>
    Private mbAM As Boolean = True

    ''' <summary>
    ''' Don't know yet. I'll find out. Wait there.
    ''' </summary>
    Private mbsettingtext As Boolean

    ''' <summary>
    ''' The currently set time in this chooser
    ''' </summary>
    Private mTime As TimeSpan

    ''' <summary>
    ''' The default time to use as the selected time if the current value cannot 
    ''' be converted into a valid time.
    ''' </summary>
    Private mDefaultTime As TimeSpan

#End Region

#Region "Constructors"

    ''' <summary>
    ''' Create a new time chooser control, with the current time set as the time to
    ''' be displayed and used as a default time if the user enters an invalid time.
    ''' </summary>
    Public Sub New()
        Me.New(CType(Nothing, TimeSpan))
    End Sub

    ''' <summary>
    ''' Create a new time chooser control, using the time part of the given datetime
    ''' to provide a time to represent and a default time.
    ''' </summary>
    ''' <param name="showtime">The time to represent on this chooser control</param>
    Public Sub New(ByVal showtime As DateTime)
        Me.New(showtime.TimeOfDay)
    End Sub

    ''' <summary>
    ''' Creates a new time chooser control, using the given time span as the time
    ''' it represents, and the default time.
    ''' </summary>
    ''' <param name="showtime"></param>
    Public Sub New(ByVal showtime As TimeSpan)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        If showtime = Nothing Then
            DefaultTime = DateTime.Now().TimeOfDay
            mTime = DefaultTime
        Else
            DefaultTime = showtime ' Use as default
            mTime = showtime
        End If

        ' Add any initialization after the InitializeComponent() call.
        AddHandler Me.TimeChanging, AddressOf ChosenTimeChanged

    End Sub

#End Region

#Region "Accessors and Properties"

    ''' <summary>
    ''' Sets the time to display for this control using the given time span
    ''' </summary>
    ''' <param name="span">The time to use for this control</param>
    Public Sub SetTime(ByVal span As TimeSpan)
        RaiseEvent TimeChanging(span)
    End Sub

    ''' <summary>
    ''' Sets the time to display for this control using the (time portion of) the
    ''' given datetime
    ''' </summary>
    ''' <param name="time">The date time to use for the time this control should
    ''' represent - note that the date portion of the date time is ignored</param>
    Public Sub SetTime(ByVal time As DateTime)
        RaiseEvent TimeChanging(time.TimeOfDay)
    End Sub

    ''' <summary>
    ''' Gets the time that this time chooser control currently represents.
    ''' </summary>
    ''' <returns>The time span representing the time that this control currently
    ''' represents.</returns>
    Public Function GetTime() As TimeSpan
        Return mTime
    End Function

    ''' <summary>
    ''' Applies the time set in this control to the given date, returning the
    ''' resultant composite datetime.
    ''' </summary>
    ''' <param name="dt">The date time containing the date component which should
    ''' be used.</param>
    ''' <returns>The date time with the date component provided and the time
    ''' currently set in this control</returns>
    Public Function ApplyTime(ByVal dt As DateTime) As DateTime
        Return New DateTime(dt.Year, dt.Month, dt.Day, mTime.Hours, mTime.Minutes, mTime.Seconds)
    End Function



    ''' <summary>
    ''' Sets the default time to use when an invalid time is currently set in this
    ''' control
    ''' </summary>
    Public Property DefaultTime() As TimeSpan
        Get
            Return mDefaultTime
        End Get
        Set(ByVal value As TimeSpan)
            mDefaultTime = value
        End Set
    End Property

#End Region

#Region "Event Handling"

    ''' <summary>
    ''' Ensures that the clock is invalidated (and thus redrawn... or, more
    ''' correctly, the old clock is removed) when the control is resized.
    ''' </summary>
    ''' <param name="e">The arguments detailing the resize event</param>
    Protected Overrides Sub OnResize(ByVal e As EventArgs)
        MyBase.OnResize(e)
        PBClock.Invalidate()
    End Sub

    ''' <summary>
    ''' Event fired to indicate that the time is changing on this chooser.
    ''' </summary>
    ''' <param name="time">The time that will be represented by this
    ''' chooser when the event is fully consumed.</param>
    Public Event TimeChanging(ByVal time As TimeSpan)

    ''' <summary>
    ''' Event Handler to process a changing time.
    ''' This ensures that the synchronizing flag is set as the text box has the time
    ''' set within it; that the AM/PM flag and corresponding button are correctly set;
    ''' that the instance variable is set and that the clock is redrawn to represent
    ''' the new time.
    ''' </summary>
    ''' <param name="time">The new time that should be displayed by this control</param>
    Private Sub ChosenTimeChanged(ByVal time As TimeSpan) Handles Me.TimeChanging
        mTime = time
        mbsettingtext = True
        'The time string must be formatted this way to fit with the masked textbox mask value
        txtTime.Text = DateTime.MinValue.Add(time).ToString("HH:mm:ss")
        mbsettingtext = False
        mbAM = time.TotalHours < 12.0
        btnAMPM.Text = CStr(IIf(mbAM, My.Resources.ctlTimeChooser_AM, My.Resources.ctlTimeChooser_PM))
        PBClock.Invalidate()
    End Sub

    ''' <summary>
    '''  Paints the clock onto the picture box within the control
    ''' </summary>
    ''' <param name="sender">The object which acts as the source of the event</param>
    ''' <param name="e">The arguments which comprise the event.</param>
    Private Sub PBClock_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) _
     Handles PBClock.Paint

        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        ' cx and cy are the centre of the circle
        ' recommend that EdgeBorder be at least 2 since when halving it we want it to be at least 1
        Dim EdgeBorder As Integer = 2
        Dim cx As Single = EdgeBorder + (PBClock.Width - 2 * EdgeBorder) \ 2
        Dim cy As Single = EdgeBorder + (PBClock.Height - 2 * EdgeBorder) \ 2

        'an estimate of the numeral sizes resulting under the fonts below
        Const WidthOfNumerals As Integer = 10
        'The radius of the circle formed by the inner edges of the numerals
        Dim InnerRadius As Single = (Math.Min(PBClock.Width, PBClock.Height) - 2 * EdgeBorder) \ 2 - WidthOfNumerals
        'the radius of the whole clockface
        Dim OuterRadius As Integer = CInt(InnerRadius + WidthOfNumerals)


        'prepare circle of clock face
        Dim path As Drawing2D.GraphicsPath = New Drawing2D.GraphicsPath
        path.AddEllipse(cx - OuterRadius, cy - OuterRadius, 2 * OuterRadius, 2 * OuterRadius)
        Dim pathGradient As Drawing2D.PathGradientBrush = New Drawing2D.PathGradientBrush(path)
        pathGradient.CenterColor = Color.Silver
        pathGradient.SurroundColors = New Color() {Color.SlateGray}
        'fill the clockface with a pattern
        g.FillPath(pathGradient, path)
        'draw the outer circle
        g.DrawPath(Pens.Black, path)
        'draw the inner circle
        g.DrawEllipse(Pens.LightBlue, cx - InnerRadius, cy - InnerRadius, 2 * InnerRadius, 2 * InnerRadius)

        'draw the numbers on
        Dim NumeralSize As SizeF
        For i As Integer = 1 To 12
            Dim angle As Double = i * Rad(360 / 12)
            NumeralSize = g.MeasureString(CStr(i), FontNumbers)
            Dim X As Double = InnerRadius * System.Math.Sin(angle) - CInt(NumeralSize.Width) \ 2
            Dim Y As Double = InnerRadius * System.Math.Cos(angle) + CInt(NumeralSize.Height) \ 2
            g.DrawString(CStr(i), FontNumbers, Brushes.Black, cx + CSng(X), cy - CSng(Y))
        Next

        'figure out the lengths and positions of the hands
        Dim LengthofHourHand As Single = InnerRadius - 25
        Dim LengthofMinHand As Single = InnerRadius - 10
        Dim LengthofSecHand As Single = InnerRadius - 5
        Dim hourRadian As Double = (mTime.Hours Mod 12) * Rad(360 / 12)
        Dim minRadian As Double = mTime.Minutes * Rad(360 / 60)
        Dim secRadian As Double = mTime.Seconds * Rad(360 / 60)


        'now draw the hands
        PenHour.StartCap = Drawing2D.LineCap.Round
        PenHour.EndCap = Drawing2D.LineCap.Round
        PenMinute.StartCap = Drawing2D.LineCap.Round
        PenMinute.EndCap = Drawing2D.LineCap.Round
        DrawHand(e.Graphics, PenHour, LengthofHourHand, hourRadian)
        DrawHand(e.Graphics, PenMinute, LengthofMinHand, minRadian)
        DrawHand(e.Graphics, PenSecond, LengthofSecHand, secRadian)
    End Sub


    ''' <summary>
    ''' Handles the mouse button being pressed on the clock picture box.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The arguments detailing the event</param>
    Private Sub PBClock_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles PBClock.MouseDown
        Selected = GetNearHand(e)
    End Sub

    ''' <summary>
    ''' Deal with the mouse being moved on the clock face.
    ''' If a hand is currently 'gripped', this will ensure that the clock
    ''' hand moves with the mouse and the time is changed to reflect the
    ''' movement.
    ''' If no hand is selected, this will highlight the nearest one in 
    ''' order to indicate which hand will be gripped when the mouse button
    ''' is clicked.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The event arguments</param>
    Private Sub PBClock_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles PBClock.MouseMove

        PenHour.Color = AutomateControls.ColourScheme.Default.ClockHourHand
        PenMinute.Color = AutomateControls.ColourScheme.Default.ClockMinuteHand
        PenSecond.Color = AutomateControls.ColourScheme.Default.ClockSecondHand
        Dim aRadian As Double = ConvertBackToPolar(e.X, e.Y)
        Select Case Selected
            Case Hand.Hour
                PenHour.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                Dim iHour As Integer
                iHour = CInt(aRadian / Rad(360 / 12))
                If Not mbAM Then
                    iHour = (iHour + 12) Mod 24
                    If iHour = 0 Then iHour = 12
                Else
                    If iHour = 12 Then iHour = 0
                End If
                mTime = New TimeSpan(iHour, mTime.Minutes, mTime.Seconds)
            Case Hand.Min
                PenMinute.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                Dim iMins As Integer
                iMins = CInt(aRadian / Rad(360 / 60)) Mod 60
                mTime = New TimeSpan(mTime.Hours, iMins, mTime.Seconds)
            Case Hand.Secs
                PenSecond.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                Dim iSecs As Integer
                iSecs = CInt(aRadian / Rad(360 / 60)) Mod 60
                mTime = New TimeSpan(mTime.Hours, mTime.Minutes, iSecs)
            Case Hand.None
                Dim NearHand As Hand = GetNearHand(e)
                Select Case NearHand
                    Case Hand.Hour
                        PenHour.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                    Case Hand.Min
                        PenMinute.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                    Case Hand.Secs
                        PenSecond.Color = AutomateControls.ColourScheme.Default.ClockHighlightHand
                End Select
        End Select
        RaiseEvent TimeChanging(mTime)

    End Sub

    ''' <summary>
    ''' Handler for lifting the mouse button up on the clock picture box.
    ''' This just ensures that no clock hand is currently selected.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub PBClock_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles PBClock.MouseUp
        Selected = Hand.None
    End Sub


    ''' <summary>
    ''' Toggles between AM and PM for this time.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The arguments determining the event.</param>
    Private Sub btnAMPM_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAMPM.Click

        RaiseEvent TimeChanging( _
         New TimeSpan((mTime.Hours + 12) Mod 24, mTime.Minutes, mTime.Seconds))

    End Sub

    ''' <summary>
    ''' Handles keypresses within the time text-box - this ensures that the underlying 
    ''' model is updated and thus that the clock is redrawn with the new time (if the
    ''' text in the field represents a valid time).
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The arguments detailing the event.</param>
    Private Sub txtTime_KeyPress(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtTime.TextChanged
        If Not mbsettingtext Then
            Dim dt As DateTime
            If DateTime.TryParse(Me.txtTime.Text, dt) Then
                RaiseEvent TimeChanging(dt.TimeOfDay)
            End If
        End If
    End Sub

#End Region

#Region "Helper Methods"
    ''' <summary>
    ''' Uses mouse movement arguments to provide the hand currently near the mouse
    ''' position
    ''' </summary>
    ''' <param name="e">mouseeventargs</param>
    ''' <returns>AutomateUI.frmTime.Hand</returns>
    ''' <remarks>
    ''' This code was originally in the mousedown event but moved into its own 
    ''' routine as I also needed to use it in the mousemove event.
    ''' </remarks>
    Private Function GetNearHand(ByVal e As MouseEventArgs) As Hand

        Dim aRadian As Double = ConvertBackToPolar(e.X, e.Y)

        'see if we have hit the seconds hand with the mouse exactly
        Dim iSecsProximity As Integer = _
         MathsUtil.ModularAbsDifference(CInt(aRadian / Rad(360 / 60)), mTime.Seconds, 60)

        If iSecsProximity = 0 Then
            Return Hand.Secs
        End If

        'see if we have hit the minutes hand with the mouse exactly
        Dim iMinsProximity As Integer = _
         MathsUtil.ModularAbsDifference(CInt(aRadian / Rad(360 / 60)), mTime.Minutes, 60)
        If iMinsProximity = 0 Then
            Return Hand.Min
        End If

        ' see if we have hit the hour hand with the mouse exactly
        ' here we equate each hour to five minutes of rotation round the clock for equal comparison
        Dim iHourProximity As Integer = _
         MathsUtil.ModularAbsDifference(CInt(aRadian / Rad(360 / 60)), 5 * mTime.Hours, 60)
        If iHourProximity = 0 Then
            Return Hand.Hour
        End If

        'if we didn't hit anything exactly, choose the nearest hand if it is sufficienltly close
        Dim MinDistanceAway As Integer = _
         CInt(MathsUtil.Min(iSecsProximity, iMinsProximity, iHourProximity))

        If MinDistanceAway <= 2 Then
            Select Case MinDistanceAway
                Case iSecsProximity
                    Return Hand.Secs
                Case iMinsProximity
                    Return Hand.Min
                Case iHourProximity
                    Return Hand.Hour
            End Select
        End If

        Return Hand.None

    End Function

    ''' <summary>
    ''' Gets the (radian) angle from the centre of the clock which is represented
    ''' by the given co-ordinates.
    ''' </summary>
    ''' <param name="x">The X co-ordinate to find the angle from</param>
    ''' <param name="y">The Y co-ordinate to find the angle from</param>
    ''' <returns>The angle that the given co-ordinates represent.</returns>
    Private Function ConvertBackToPolar(ByVal x As Integer, ByVal y As Integer) As Double
        Dim cx As Double = PBClock.Width / 2
        Dim cy As Double = PBClock.Height / 2
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

    ''' <summary>
    ''' Convert an angle from Degrees into radians.
    ''' </summary>
    ''' <param name="angle">The angle in degrees</param>
    ''' <returns>The angle in radians</returns>
    Private Function Rad(ByVal angle As Double) As Double
        Return angle * Math.PI / 180
    End Function

    ''' <summary>
    ''' Draws the required hand using the given constraints
    ''' </summary>
    ''' <param name="g">The graphics context to draw on</param>
    ''' <param name="p">The pen to use to draw the hand</param>
    ''' <param name="length">The length of the hand which should be drawn</param>
    ''' <param name="angle">The angle at which the angle should be drawn</param>
    Private Sub DrawHand(ByVal g As Graphics, ByVal p As Pen, ByVal length As Double, ByVal angle As Double)
        Dim X As Double = length * System.Math.Sin(angle)
        Dim Y As Double = length * System.Math.Cos(angle)

        Dim cx As Single = PBClock.Width \ 2
        Dim cy As Single = PBClock.Height \ 2

        g.DrawLine(p, cx, cy, cx + CSng(X), cy - CSng(Y))
    End Sub

#End Region

End Class
