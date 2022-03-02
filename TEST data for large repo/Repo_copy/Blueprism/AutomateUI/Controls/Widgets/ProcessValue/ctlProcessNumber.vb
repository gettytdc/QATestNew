Imports System.Globalization

Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessNumber
''' 
''' <summary>
''' Allows a clsProcessValue of datatype number to be edited.
''' </summary>
Public Class ctlProcessNumber : Inherits AutomateControls.Textboxes.StyledTextBox : Implements IProcessValue

#Region " Constants "

    ''' <summary>
    ''' The default colour to use for the <see cref="InvalidBackColor"/> property
    ''' </summary>
    Private Shared ReadOnly DefaultInvalidBackColor As Color = Color.BlanchedAlmond

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event fired when the value in this control has been validated and differs
    ''' from when the value was set or the last time that this event was fired,
    ''' whichever was the most recent activity.
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

#End Region

#Region " Member Variables "

    ' The decimal separator char for the thread which created this control
    Private ReadOnly mDecimalSeparator As String = _
     CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator

    ' The negative sign string for the thread which creates this control
    Private ReadOnly mNegativeSign As String = _
     CultureInfo.CurrentCulture.NumberFormat.NegativeSign

    ' The colour to use to indicate an invalid value
    Private mInvalidBackColor As Color = DefaultInvalidBackColor

    ' The back colour saved when the invalid back colour was applied
    Private mSavedBackColor As Color

    ' The value last used in this control - either when the value was set, or when
    ' the last Changed event was fired, whichever was most recent.
    ' Initialise with a null number value so that Commit() works correctly
    Private mLastValue As New clsProcessValue(DataType.number)

#End Region

#Region " Properties "

    ''' <summary>
    ''' The colour used to indicate that the current value of the text box could not
    ''' be parsed into a valid process value.
    ''' </summary>
    <Browsable(True), Category("Appearance"), _
     Description("The background colour to indicate an invalid number")> _
    Public Property InvalidBackColor() As Color
        Get
            Return mInvalidBackColor
        End Get
        Set(ByVal Value As Color)
            mInvalidBackColor = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets the decimal value of the text in this control, or a null value if this
    ''' text field is empty or the contents could not be parsed into a decimal
    ''' using current culture formatting.
    ''' </summary>
    Public Property DecimalValue() As Nullable(Of Decimal)
        Get
            Dim txt As String = Text.Trim()
            If txt = "" Then Return Nothing
            Dim dec As Decimal
            ' Strip out any chars except numeric and decimal separators; we only
            ' allow a single decimal separator - stop processing if we find another
            Dim sb As New StringBuilder()
            Dim foundDecSep As Boolean = False
            For Each c As Char In txt
                ' If it's numeric, append it and continue
                If "0123456789".IndexOf(c) >= 0 Then sb.Append(c) : Continue For

                ' If it's a negative sign
                If mNegativeSign.IndexOf(c) >= 0 Then
                    ' We allow them at the beginning of the number
                    If sb.Length = 0 Then sb.Append(c)
                    ' Otherwise, we just ignore them as if they were any other
                    ' garbage character
                    Continue For
                End If

                ' Otherwise, if it's not a decimal point, skip over it and continue
                If mDecimalSeparator.IndexOf(c) = -1 Then Continue For

                ' At this point, it's a decimal separator; if we've already found
                ' one, then we stop processing now. We ignore anything from here on
                If foundDecSep Then Exit For

                ' If we're here, we've found a first decimal separator, so set
                ' the flag, append it and move on
                foundDecSep = True
                sb.Append(c)
            Next
            txt = sb.ToString()
            If Decimal.TryParse(txt, dec) Then Return dec Else Return Nothing
        End Get
        Set(ByVal value As Nullable(Of Decimal))
            If Not value.HasValue Then Text = "" : Return
            Text = value.Value.ToString()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the current value held in this control
    ''' </summary>
    <Browsable(False)> _
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Dim dec As Nullable(Of Decimal) = DecimalValue
            If Not dec.HasValue Then Return New clsProcessValue(DataType.number)
            Return dec.Value
        End Get
        Set(ByVal value As clsProcessValue)
            mLastValue = Nothing
            ' Treat 'null' as a null numeric process value
            If value Is Nothing Then value = New clsProcessValue(DataType.number)
            Text = value.FormattedValue
            mLastValue = value
        End Set
    End Property

    ''' <summary>
    ''' Thin cover for the 'ReadOnly' property to implement the specified property
    ''' in the IProcessValue interface.
    ''' </summary>
    Private Property IsReadOnly() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return MyBase.ReadOnly
        End Get
        Set(ByVal value As Boolean)
            MyBase.ReadOnly = value
        End Set
    End Property

#End Region

#Region " Event Handler Invokers/Overrides "

    ''' <summary>
    ''' Handles the text being changed by changing the back color to indicate an
    ''' invalid value if it cannot be parsed into a number.
    ''' </summary>
    Protected Overrides Sub OnTextChanged(ByVal e As EventArgs)
        MyBase.OnTextChanged(e)

        If Decimal.TryParse(Text, Nothing) Then
            BackColor = mSavedBackColor
        Else
            mSavedBackColor = BackColor
            BackColor = mInvalidBackColor
        End If
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        mSavedBackColor = BackColor
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        mSavedBackColor = BackColor
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Private Sub Commit() Implements IProcessValue.Commit
        ' We have no last value - we're amidst change, no committing here
        If mLastValue Is Nothing Then Return

        ' We have a last value; if it's equal to the current value, no change
        Dim val As clsProcessValue = Value
        Me.Text = val.FormattedValue
        If mLastValue.Equals(val) Then Return

        ' We have a last value; it has changed; save it and fire the event
        mLastValue = val
        OnChanged(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Handles the post-validation of this control by ensuring that the back color
    ''' is reset to 'valid' and the <see cref="Changed"/> event is fired, if
    ''' appropriate
    ''' </summary>
    Protected Overrides Sub OnValidated(ByVal e As EventArgs)
        MyBase.OnValidated(e)
        If Not Me.ReadOnly Then Commit()
    End Sub

    ''' <summary>
    ''' Raises the <see cref="Changed"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Sets the value in this control and returns the resultant process value.
    ''' Currently, this only changes the held process value reference if it is
    ''' unset in this control. Otherwise, it sets the value in the existing value
    ''' object and returns it.
    ''' </summary>
    ''' <param name="dec">The decimal value to set in the value held in this control.
    ''' </param>
    ''' <returns>The process value held by this control with this value set in it.
    ''' </returns>
    Private Function SetValue(ByVal dec As Decimal) As clsProcessValue
        Value = dec
        Return Value
    End Function

    ''' <summary>
    ''' Thin cover for the 'Select' method to implement the specified method in
    ''' the IProcessValue interface.
    ''' </summary>
    Private Sub DoSelect() Implements IProcessValue.SelectControl
        Me.Select()
    End Sub


    ''' <summary>
    ''' Tests if the <see cref="InvalidBackColor"/> property should be serialized
    ''' by the forms designer.
    ''' </summary>
    ''' <returns>True if the currently set invalid back color is different to the
    ''' default; False if it matches the default</returns>
    Private Function ShouldSerializeInvalidBackColor() As Boolean
        Return (mInvalidBackColor <> DefaultInvalidBackColor)
    End Function

    ''' <summary>
    ''' Resets the <see cref="InvalidBackColor"/> property to its default
    ''' </summary>
    Private Sub ResetInvalidBackColor()
        mInvalidBackColor = DefaultInvalidBackColor
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

End Class
