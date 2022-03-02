Imports BluePrism.AutomateAppCore


''' <summary>
''' Radio button which represents a conflict option for a component
''' </summary>
Public Class ctlConflictOptionRadioButton : Inherits RadioButton

    ''' <summary>
    ''' Event fired when this conflict option is chosen (ie. checked)
    ''' </summary>
    Public Event ConflictOptionChosen(ByVal sender As Object, ByVal e As ConflictEventArgs)

    ' The option represented by this radio button
    Private mOption As ConflictOption

    ''' <summary>
    ''' Creates a new conflict option radio button representing the specified option
    ''' for the given component.
    ''' </summary>
    ''' <param name="comp">The component that the conflict is on.</param>
    ''' <param name="opt">The option to resolve the conflict.</param>
    Public Sub New(ByVal comp As PackageComponent, ByVal opt As ConflictOption)
        Me.AutoSize = True
        Me.ConflictOption = opt
        Me.AccessibleName = ctlConflict.GetAccessibleName(comp, opt)
    End Sub

    ''' <summary>
    ''' The conflict option represented by this radio button
    ''' </summary>
    Public Property ConflictOption() As ConflictOption
        Get
            Return mOption
        End Get
        Set(ByVal opt As ConflictOption)
            mOption = opt
            Text = opt.Text
        End Set
    End Property

    ''' <summary>
    ''' Handles the radio button being checked. This just ensures that the event is
    ''' bubbled out via the <see cref="ConflictOptionChosen"/> event.
    ''' </summary>
    Protected Overrides Sub OnCheckedChanged(ByVal e As EventArgs)
        MyBase.OnCheckedChanged(e)
        If Checked Then
            RaiseEvent ConflictOptionChosen(Me, New ConflictEventArgs(Me, Nothing, mOption))
        End If
    End Sub


End Class
