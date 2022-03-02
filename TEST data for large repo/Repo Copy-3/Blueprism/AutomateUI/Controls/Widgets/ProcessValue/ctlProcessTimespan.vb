Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessTimespan
''' 
''' <summary>
''' This control allows a user to edit an automate timespan.
''' </summary>
Public Class ctlProcessTimespan : Inherits UserControl : Implements IProcessValue

#Region "Windows Forms Designer Generated Code"


    Friend WithEvents lSecs As System.Windows.Forms.Label
    Friend WithEvents lMins As System.Windows.Forms.Label
    Friend WithEvents lHours As System.Windows.Forms.Label
    Friend WithEvents lDays As System.Windows.Forms.Label

    Friend WithEvents txtDays As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtMins As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtSecs As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtHours As AutomateControls.Textboxes.StyledTextBox

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessTimespan))
        Me.lSecs = New System.Windows.Forms.Label()
        Me.lMins = New System.Windows.Forms.Label()
        Me.lHours = New System.Windows.Forms.Label()
        Me.lDays = New System.Windows.Forms.Label()
        Me.txtDays = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtHours = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtMins = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtSecs = New AutomateControls.Textboxes.StyledTextBox()
        Me.SuspendLayout()
        '
        'lSecs
        '
        resources.ApplyResources(Me.lSecs, "lSecs")
        Me.lSecs.Name = "lSecs"
        '
        'lMins
        '
        resources.ApplyResources(Me.lMins, "lMins")
        Me.lMins.Name = "lMins"
        '
        'lHours
        '
        resources.ApplyResources(Me.lHours, "lHours")
        Me.lHours.Name = "lHours"
        '
        'lDays
        '
        resources.ApplyResources(Me.lDays, "lDays")
        Me.lDays.Name = "lDays"
        '
        'txtDays
        '
        resources.ApplyResources(Me.txtDays, "txtDays")
        Me.txtDays.Name = "txtDays"
        '
        'txtHours
        '
        resources.ApplyResources(Me.txtHours, "txtHours")
        Me.txtHours.Name = "txtHours"
        '
        'txtMins
        '
        resources.ApplyResources(Me.txtMins, "txtMins")
        Me.txtMins.Name = "txtMins"
        '
        'txtSecs
        '
        resources.ApplyResources(Me.txtSecs, "txtSecs")
        Me.txtSecs.Name = "txtSecs"
        '
        'ctlProcessTimespan
        '
        Me.Controls.Add(Me.txtSecs)
        Me.Controls.Add(Me.txtMins)
        Me.Controls.Add(Me.txtHours)
        Me.Controls.Add(Me.txtDays)
        Me.Controls.Add(Me.lSecs)
        Me.Controls.Add(Me.lMins)
        Me.Controls.Add(Me.lHours)
        Me.Controls.Add(Me.lDays)
        Me.Name = "ctlProcessTimespan"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

    Public Event Changed As EventHandler Implements IProcessValue.Changed

    ' Flag to indicate whether this control is readonly or not
    Private mReadOnly As Boolean

    ' Field to hold the underlying clsProcessValue
    ' Initialise with a null timespan value so that Commit() works correctly
    Private mLastValue As New clsProcessValue(DataType.timespan)

    Public Sub New()
        MyBase.New()

        InitializeComponent()

    End Sub

    ''' <summary>
    ''' Overrides the control onGotFocus method, to make sure the days textbox is 
    ''' fovused by default.
    ''' </summary>
    Protected Overrides Sub OnGotFocus(ByVal e As EventArgs)
        txtDays.Focus()
    End Sub

    ''' <summary>
    ''' The current value held within this control
    ''' </summary>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return New TimeSpan( _
             CastText(txtDays), _
             CastText(txtHours, 23), CastText(txtMins, 59), CastText(txtSecs, 59))
        End Get
        Set(ByVal val As clsProcessValue)
            mLastValue = Nothing ' A null last-value inhibits event firing
            ' Make sure we have a non-null value
            If val Is Nothing Then val = New clsProcessValue(DataType.timespan)
            Dim span As TimeSpan = CType(val, TimeSpan)
            If span = TimeSpan.Zero Then
                ResetText()
            Else
                txtDays.Text = CStr(span.Days)
                txtHours.Text = CStr(span.Hours)
                txtMins.Text = CStr(span.Minutes)
                txtSecs.Text = CStr(span.Seconds)
            End If
            mLastValue = val
        End Set
    End Property

    ''' <summary>
    ''' Casts the text from the given text box into an integer. If the text could not
    ''' be converted into an int, the text is reset and zero is returned
    ''' </summary>
    ''' <param name="box">The textbox whose text should be cast</param>
    ''' <returns>The integer representing the value found in the textbox; or
    ''' zero if the textbox's text could not be converted into an integer
    ''' </returns>
    Private Function CastText(ByVal box As TextBox) As Integer
        Return CastText(box, Integer.MaxValue)
    End Function

    ''' <summary>
    ''' Casts the text from the given text box into an integer, ensuring that it
    ''' does not exceed a maximum value. If it does, the it sets the text to that
    ''' max value and returns that. If the text could not be converted into an int,
    ''' the text is reset and zero is returned
    ''' </summary>
    ''' <param name="box">The textbox whose text should be cast</param>
    ''' <param name="max">The maximum value of the returned integer</param>
    ''' <returns>The integer representing the value found in the textbox; or
    ''' <paramref name="max"/> if the value in the textbox was greater than the max,
    ''' or zero if the textbox's text could not be converted into an integer
    ''' </returns>
    Private Function CastText(ByVal box As TextBox, ByVal max As Integer) As Integer
        Dim val As Integer = 0
        If Not Integer.TryParse(box.Text, val) Then box.Text = "" : Return 0
        If val > max Then val = max : box.Text = max.ToString()
        Return val
    End Function

    ''' <summary>
    ''' This function validates the users input when any of the textboxes change
    ''' </summary>
    Public Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the text changing for any of the text fields
    ''' </summary>
    Private Sub HandleTextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDays.TextChanged, txtHours.TextChanged, _
      txtMins.TextChanged, txtSecs.TextChanged
        Commit()
    End Sub

    ''' <summary>
    ''' override the controls resettext method, so that all the text boxes get reset.
    ''' </summary>
    Public Overrides Sub ResetText()
        txtDays.ResetText()
        txtHours.ResetText()
        txtMins.ResetText()
        txtSecs.ResetText()
    End Sub

    ''' <summary>
    ''' Selects this control
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        txtDays.Select()
    End Sub

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Private Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            mReadOnly = value
            txtDays.ReadOnly = value
            txtHours.ReadOnly = value
            txtMins.ReadOnly = value
            txtSecs.ReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        ' If we have no last value, the value is currently being set
        If mLastValue Is Nothing Then Return

        ' We have a last value - if it's equal to the current value, no change
        Dim currVal As clsProcessValue = Value
        If mLastValue.Equals(currVal) Then Return

        ' We have a last value, different to the current value.
        ' Save the current value as the last value
        mLastValue = currVal
        ' And fire the changed event
        OnChanged(EventArgs.Empty)
    End Sub

End Class
