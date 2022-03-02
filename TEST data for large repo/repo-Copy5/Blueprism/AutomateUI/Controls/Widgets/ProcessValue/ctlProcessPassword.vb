Imports BluePrism.AutomateProcessCore
Imports BluePrism.Common.Security

''' Project  : Automate
''' Class    : ctlProcessPassword
''' 
''' <summary>
''' This control allows the user to edit a Password, Note that the password is 
''' never shown to the user.
''' </summary>
Public Class ctlProcessPassword
    Inherits ctlAutomateSecurePassword
    Implements IProcessValue

    ''' <summary>
    ''' The default width applied to this control.
    ''' </summary>
    Private Const DefaultWidth As Integer = 100

    ''' <summary>
    ''' The default height applied to this
    ''' control.
    ''' </summary>
    Private Const DefaultHeight As Integer = 24

    ''' <summary>
    ''' Event raised when the value has been changed by the user
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

    ''' <summary>
    ''' This stores the underlying clsprocessValue
    ''' </summary>
    Private mValue As clsProcessValue

    ''' <summary>
    ''' The initial value for this control
    ''' </summary>
    Private mInitValue As clsProcessValue

    ''' <summary>
    ''' We get the passwordchar here.
    ''' </summary>
    Public Sub New()
        Me.Size = New Size(DefaultWidth, DefaultHeight)
    End Sub

    ''' <summary>
    ''' Gets the preferred size of this control
    ''' </summary>
    ''' <param name="proposedSize">The size that the layout engine is proposing for
    ''' this control</param>
    ''' <returns>The preferred size of this control, given its current content.
    ''' </returns>
    Public Overrides Function GetPreferredSize(ByVal proposedSize As Size) As Size
        Return New Size(Math.Max(DefaultWidth, proposedSize.Width), DefaultHeight)
    End Function

    ''' <summary>
    ''' The property that allows access to the underlying clsProcessValue stored in 
    ''' mobjValue , on set me.text to a hidden password string.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            ' Check we have the latest text from the inner password dialog
            Commit()
            Return mValue
        End Get
        Set(ByVal value As clsProcessValue)
            mValue = value
            If value Is Nothing Then
                SecurePassword = New SafeString()
                mInitValue = Nothing
            Else
                SecurePassword = CType(value, SafeString)
                mInitValue = value.Clone()
            End If
        End Set
    End Property

    ''' <summary>
    ''' we override the onLostfocus event then we check the current value against the
    ''' initial value. if the current value does not match the inital value, then the 
    ''' value has changed.
    ''' </summary>
    Protected Overrides Sub OnLostFocus(ByVal e As EventArgs)
        Commit()
    End Sub

    ''' <summary>
    ''' Raises the <see cref="Changed"/> event
    ''' </summary>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

    ''' <summary>
    ''' we override the resetTextMethod, because we want to clear the undelying
    ''' value as well as clearing the text.
    ''' </summary>
    Public Overrides Sub ResetText()
        mValue = New clsProcessValue(DataType.password)
        MyBase.ResetText()
    End Sub

    ''' <summary>
    ''' Selects the content in this control
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        Me.Select()
    End Sub

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Public Shadows Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return MyBase.ReadOnly
        End Get
        Set(ByVal value As Boolean)
            MyBase.ReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        Dim currVal As New clsProcessValue(SecurePassword)
        If Not currVal.Equals(mValue) Then
            mValue = currVal
            OnChanged(EventArgs.Empty)
        End If
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
End Class
