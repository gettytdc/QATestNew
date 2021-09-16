Imports BluePrism.AutomateAppCore
''' Project  : Automate
''' Class    : frmCalendar
''' 
''' <summary>
''' A calendar form.
''' </summary>
Friend Class frmCalendar
    Inherits frmForm

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
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents flowPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents cal2 As CustomControls.MonthCalendar
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCalendar))
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.cal2 = New CustomControls.MonthCalendar()
        Me.flowPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.flowPanel.SuspendLayout
        Me.SuspendLayout
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'cal2
        '
        Me.cal2.ColorTable.Border = System.Drawing.SystemColors.ControlDark
        Me.cal2.ColorTable.DayActiveGradientBegin = System.Drawing.SystemColors.GradientInactiveCaption
        Me.cal2.ColorTable.DayActiveGradientEnd = System.Drawing.SystemColors.GradientInactiveCaption
        Me.cal2.ColorTable.DaySelectedGradientBegin = System.Drawing.SystemColors.GradientActiveCaption
        Me.cal2.ColorTable.DaySelectedGradientEnd = System.Drawing.SystemColors.GradientActiveCaption
        Me.cal2.ColorTable.HeaderActiveGradientBegin = System.Drawing.Color.White
        Me.cal2.ColorTable.MonthSeparator = System.Drawing.SystemColors.ActiveBorder
        Me.cal2.ColorTable.WeekHeaderText = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.cal2, "cal2")
        Me.cal2.Name = "cal2"
        Me.cal2.ShowFooter = false
        '
        'flowPanel
        '
        Me.flowPanel.Controls.Add(Me.btnCancel)
        Me.flowPanel.Controls.Add(Me.btnOK)
        resources.ApplyResources(Me.flowPanel, "flowPanel")
        Me.flowPanel.Name = "flowPanel"
        '
        'frmCalendar
        '
        Me.AcceptButton = Me.btnOK
        resources.ApplyResources(Me, "$this")
        Me.CancelButton = Me.btnCancel
        Me.Controls.Add(Me.flowPanel)
        Me.Controls.Add(Me.cal2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "frmCalendar"
        Me.ShowInTaskbar = false
        Me.flowPanel.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub

#End Region

#Region " Member Variables "
    Private mOrigDate As Date

    Private Const CalendarHeaderHeight As Integer = 37
    Private Const CalendarFirstColumnWidth As Integer = 18
    Private const DaysInWeek As Integer = 7
    Private const RowsInCalendar As Integer = 6

    Private ReadOnly mColumnWidth As Integer
    Private ReadOnly mRowHeight As Integer
    Private ReadOnly mBorderWidth As Integer
    Private ReadOnly mHeaderSize As Integer

    ' The location of the cursor when a date was last selected
    Private mLastClickLocn As Point

    ' The currently selected date
    Private mDate As Date

    ' The date last explicitly set by the 'SelectedDate' property
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, empty calendar form
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        'I18n Calendar System.Windows.Forms.MonthCalendar does not support a locale; defaults to LOCALE_USER_DEFAULT
        'https://stackoverflow.com/questions/16099594/change-language-of-a-monthcalender-control
        'https://support.microsoft.com/af-za/help/889834/the-datetimepicker-and-monthcalendar-control-do-not-reflect-the-curren
        'When needed we use the following culture aware month calendar
        'https://www.codeproject.com/Articles/45684/Culture-Aware-Month-Calendar-and-Datepicker

        'Get the forms border and header width from the clientSize
        mBorderWidth = (Width - ClientSize.Width) \ 2
        mHeaderSize = Height - ClientSize.Height - mBorderWidth * 2
        
        Width = cal2.Width + (mBorderWidth * 2)
        mColumnWidth = (cal2.Width - CalendarFirstColumnWidth) \ DaysInWeek
        mRowHeight = (cal2.Height - CalendarHeaderHeight) \ RowsInCalendar
    End Sub

#End Region

#Region " Event Handling methods / overrides "

    Private mFirstClickClicked As Boolean = False

    ''' <summary>
    ''' Handles the DateSelected event.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Public Sub SelectDate(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cal2.DateSelected

        ' Get the selected date
        mDate = cal2.SelectionStart
        
        'We have to use measurements relative to the Left of the form, the calendar control size is fixed
        mLastClickLocn = New Point(MousePosition.X - Left - mBorderWidth - CalendarFirstColumnWidth, MousePosition.Y - Top - (mHeaderSize + mBorderWidth) - CalendarHeaderHeight)
        mNumberRectangle = GetBoundsOfClickedNumber(mLastClickLocn)
        mFirstClickClicked = True
    End Sub
    Dim mNumberRectangle As Rectangle
    Private Function GetBoundsOfClickedNumber(clickedPoint As Point) As Rectangle
        Dim topOfBounds = New Point(RoundDownToSignificance(clickedPoint.X, mColumnWidth), RoundDownToSignificance(clickedPoint.Y, mRowHeight))
        Return New Rectangle(topOfBounds, New Size(mColumnWidth, mRowHeight))
    End Function
    Private Function RoundDownToSignificance(number As Integer, significance As Integer) As Integer

        If number = 0 Then Return 0
        
        'Round number down to the nearest multiple of significance
        Dim d As Double
        d = number / significance
        d = (Math.Round(d, 0))  * significance

        'Have we overshot the lower bound?
        If d >= significance AndAlso d > number Then
            d -= significance
        End If

        Return CType(d, Integer)
    End Function
    Private Sub CalendarDoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles cal2.DoubleClick
        'Where was the mouse when the mouse was clicked
        Dim clickLocation As Point = New Point(CType(e, MouseEventArgs).Location.X - CalendarFirstColumnWidth, CType(e, MouseEventArgs).Location.Y - CalendarHeaderHeight) 

        If mNumberRectangle.Contains(clickLocation) Then
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

    ''' <summary>
    ''' The date selected on the calendar.
    ''' </summary>
    ''' <value>The date</value>
    Public Property SelectedDate() As Date
        Get
            Return mDate
        End Get
        Set
            If Value >= cal2.MinDate Then
                cal2.SelectionStart = Value
                cal2.SelectionEnd = Value
                mOrigDate = Value
            Else
                cal2.SelectionStart = Today
                cal2.SelectionEnd = Today
            End If
        End Set
    End Property

    Private Sub btnOK_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnOK.Click 
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        If Not e.Cancel Then
            If DialogResult <> DialogResult.OK Then
                mDate = mOrigDate
            Else
                mDate = cal2.SelectionStart
            End If
        End If
    End Sub

    Private Sub frmCalendar_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetClickAreaOfNumberByDate()
    End Sub

    Private Sub cal2_DateChanged(sender As Object, e As DateRangeEventArgs) Handles cal2.DateChanged
            SetClickAreaOfNumberByDate()
    End Sub

    Private Sub SetClickAreaOfNumberByDate()
        If cal2.SelectionStart >= cal2.RealStartDate AndAlso cal2.SelectionStart <= cal2.ViewEnd Then
            Dim distanceFromFirstDate = (cal2.SelectionStart - cal2.RealStartDate).Days
            Dim row = distanceFromFirstDate \ DaysInWeek 
            Dim column = distanceFromFirstDate Mod DaysInWeek
            Dim topOfBounds = New Point((column * mColumnWidth), (row * mRowHeight))
            mNumberRectangle = New Rectangle(topOfBounds, New Size(mColumnWidth, mRowHeight))
        End If
    End Sub
    #End Region
End Class

