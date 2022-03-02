
Imports System.Globalization
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessDateTime
''' 
''' <summary>
''' This control allows a user to edit an automate datetime.
''' Features to note: every 100ms the text that the user has typed into
''' the textbox gets parsed to see if it is a valid date time.
''' This allows the user to quickly type in some text, representing a date
''' in their current locale setting.
''' </summary>
Friend Class ctlProcessDateTime : Inherits UserControl : Implements IProcessValue

    Public Event Changed As EventHandler Implements IProcessValue.Changed

    ' The last value set or committed by this control
    Private mLastValue As New clsProcessValue(DataType.datetime)

    ''' <summary>
    ''' Gets the type of value handled by this control. The base implementation
    ''' handles <see cref="DataType.datetime"/> values.
    ''' </summary>
    Protected Overridable ReadOnly Property ValueType() As DataType
        Get
            Return DataType.datetime
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets whether the date button is visible in this control
    ''' </summary>
    Public Property DateButtonVisible() As Boolean
        Get
            Return btnDate.Visible
        End Get
        Set(ByVal value As Boolean)
            btnDate.Visible = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets whether the time button is visible in this control
    ''' </summary>
    Public Property TimeButtonVisible() As Boolean
        Get
            Return btnTime.Visible
        End Get
        Set(ByVal value As Boolean)
            btnTime.Visible = value
        End Set
    End Property


#Region "Windows Forms designer Generated code"

    Friend WithEvents txtDate As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnTime As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDate As AutomateControls.Buttons.StandardStyledButton

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessDateTime))
        Me.txtDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnTime = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDate = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'txtDate
        '
        resources.ApplyResources(Me.txtDate, "txtDate")
        Me.txtDate.Name = "txtDate"
        '
        'btnTime
        '
        resources.ApplyResources(Me.btnTime, "btnTime")
        Me.btnTime.Image = Global.AutomateUI.My.Resources.ToolImages.Task_Schedule_16x16
        Me.btnTime.Name = "btnTime"
        Me.btnTime.TabStop = True
        '
        'btnDate
        '
        resources.ApplyResources(Me.btnDate, "btnDate")
        Me.btnDate.Image = Global.AutomateUI.My.Resources.ComponentImages.Calendar_16x16
        Me.btnDate.Name = "btnDate"
        Me.btnDate.TabStop = True
        '
        'ctlProcessDateTime
        '
        Me.Controls.Add(Me.txtDate)
        Me.Controls.Add(Me.btnTime)
        Me.Controls.Add(Me.btnDate)
        Me.Name = "ctlProcessDateTime"
        resources.ApplyResources(Me, "$this")
        Me.Tag = "204,20"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

    ''' <summary>
    ''' We start the timer here
    ''' </summary>
    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Gets or sets the date value of this control
    ''' </summary>
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Protected Overridable Property DateValue() As Nullable(Of Date)
        Get
            Dim txt As String = txtDate.Text.Trim()
            If txt = "" Then Return Nothing
            Dim dt As Date
            If Date.TryParse(txt, Nothing, DateTimeStyles.NoCurrentDateDefault, dt) _
             Then Return dt _
             Else Return Nothing
        End Get
        Set(ByVal value As Nullable(Of Date))
            If Not value.HasValue Then txtDate.Text = "" : Commit() : Return
            Dim pval As New clsProcessValue(ValueType, value.Value)
            txtDate.Text = pval.FormattedValue
        End Set
    End Property

    Public Property MinValue() As Nullable(Of Date)
    Public Property MaxValue() As Nullable(Of Date)

    ''' <summary>
    ''' The property that allows access to the underlying clsProcessValue stored in 
    ''' mValue. On set we also store the date in mDate
    ''' </summary>
    <Browsable(False), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Dim dt As Nullable(Of Date) = DateValue
            If Not dt.HasValue Then Return New clsProcessValue(ValueType)
            Return New clsProcessValue(ValueType, dt.Value)
        End Get
        Set(ByVal value As clsProcessValue)
            ' Clear the last value to inhibit checks against it
            mLastValue = Nothing

            ' Replace null values with values with null values
            If value Is Nothing Then value = New clsProcessValue(ValueType)

            ' Update the textbox with the value
            txtDate.Text = value.FormattedValue

            ' Keep the last value for later changed checks
            mLastValue = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the validation event on the text field by ensuring that the UI text
    ''' is committed to the interested listeners
    ''' </summary>
    Private Sub HandleTextValidated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDate.Validated
        Commit()
    End Sub

    ''' <summary>
    ''' The event handler for the date button, that then shows a frmCalendar, to set
    ''' the date using a calendar control.
    ''' </summary>
    Private Sub btnDate_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDate.Click
        Using calForm As New frmCalendar()
            calForm.ShowInTaskbar = False
            If MinValue IsNot Nothing Then
                calForm.cal2.MinDate = CDate(MinValue)
            End If
            If MaxValue IsNot Nothing Then
                calForm.cal2.MaxDate = CDate(MaxValue)
            End If

            calForm.Location =
             PointToScreen(btnDate.Location - New Size(calForm.Width, 0))
            Dim dt As Date
            With DateValue
                If .HasValue Then dt = .Value Else dt = Date.Now
            End With
            calForm.SelectedDate = dt
            calForm.ShowInTaskbar = False
            If calForm.ShowDialog(Parent) = DialogResult.OK Then
                DateValue = (calForm.SelectedDate + dt.TimeOfDay)
                Commit()
            End If
        End Using
    End Sub

    ''' <summary>
    ''' When we reset the text of the control we clear the underlying value
    ''' </summary>
    Public Overrides Sub ResetText()
        txtDate.Text = ""
    End Sub

    ''' <summary>
    ''' This just ensures that the date text box is focused by default when the control
    ''' is focused.
    ''' </summary>
    Protected Overrides Sub OnGotFocus(ByVal e As EventArgs)
        MyBase.OnGotFocus(e)
        txtDate.Focus()
    End Sub

    ''' <summary>
    ''' The event handler for the time button. This shows a frmTime, and then sets the 
    ''' underlying time value to the one selected on the time control.
    ''' </summary>
    Private Sub btnTime_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnTime.Click
        Using timeForm As New frmTime()
            timeForm.ShowInTaskbar = False
            timeForm.Location =
             PointToScreen(btnTime.Location - New Size(timeForm.Width, 0))
            Dim dt As Date
            With DateValue
                If .HasValue Then dt = .Value Else dt = Date.Today
            End With
            timeForm.SelectedTime = dt.TimeOfDay
            timeForm.ShowInTaskbar = False
            If timeForm.ShowDialog(Parent) = DialogResult.OK Then
                DateValue = dt.Date + timeForm.SelectedTime
                Commit()
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Selects this control
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        txtDate.Select()
    End Sub

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return txtDate.ReadOnly
        End Get
        Set(ByVal value As Boolean)
            txtDate.ReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Raises the <see cref="Changed"/> event
    ''' </summary>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control.
    ''' This is called when we have either typed in a date, and lost focus of the
    ''' textbox, or when we are finished using the frmCalendar or frmTime Controls.
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        If Me.ReadOnly Then Exit Sub
        If mLastValue Is Nothing Then Return ' It's currently setting the value
        Dim val As clsProcessValue = Me.Value
        If val.Equals(mLastValue) Then
            ' Reset to the last value display - an equal value could just mean that
            ' it's testing an invalid value against an empty value
            txtDate.Text = mLastValue.FormattedValue
        Else
            mLastValue = val
            txtDate.Text = val.FormattedValue
            OnChanged(EventArgs.Empty)
        End If
    End Sub

End Class
