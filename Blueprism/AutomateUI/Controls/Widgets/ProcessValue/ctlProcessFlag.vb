Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessFlag
''' 
''' <summary>
''' This control lets the user edit a automate flag
''' </summary>
Public Class ctlProcessFlag
    Inherits UserControl
    Implements IProcessValue

#Region " Class Scope Declarations "

    ''' <summary>
    ''' The enumeration here is used to signify tristate nature of automate flags
    ''' </summary>
    Private Enum TriState
        [True]
        [False]
        NotSet
    End Enum

#End Region

#Region " Published Events "

    Public Event Changed As EventHandler Implements IProcessValue.Changed

#End Region

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Friend WithEvents rdoTrue As AutomateControls.StyledRadioButton
    Friend WithEvents rdoFalse As AutomateControls.StyledRadioButton

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessFlag))
        Me.rdoTrue = New AutomateControls.StyledRadioButton()
        Me.rdoFalse = New AutomateControls.StyledRadioButton()
        Me.SuspendLayout()
        '
        'rdoTrue
        '
        Me.rdoTrue.AutoCheck = False
        Me.rdoTrue.ButtonHeight = 21
        Me.rdoTrue.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoTrue.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoTrue.FocusDiameter = 16
        Me.rdoTrue.FocusThickness = 3
        Me.rdoTrue.FocusYLocation = 9
        Me.rdoTrue.ForceFocus = True
        Me.rdoTrue.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoTrue.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        resources.ApplyResources(Me.rdoTrue, "rdoTrue")
        Me.rdoTrue.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoTrue.Name = "rdoTrue"
        Me.rdoTrue.RadioButtonDiameter = 12
        Me.rdoTrue.RadioButtonThickness = 2
        Me.rdoTrue.RadioYLocation = 7
        Me.rdoTrue.StringYLocation = 2
        Me.rdoTrue.TabStop = True
        Me.rdoTrue.TextColor = System.Drawing.Color.Black
        Me.rdoTrue.UseVisualStyleBackColor = True
        '
        'rdoFalse
        '
        Me.rdoFalse.AutoCheck = False
        Me.rdoFalse.ButtonHeight = 21
        Me.rdoFalse.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoFalse.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoFalse.FocusDiameter = 16
        Me.rdoFalse.FocusThickness = 3
        Me.rdoFalse.FocusYLocation = 9
        Me.rdoFalse.ForceFocus = True
        Me.rdoFalse.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoFalse.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        resources.ApplyResources(Me.rdoFalse, "rdoFalse")
        Me.rdoFalse.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoFalse.Name = "rdoFalse"
        Me.rdoFalse.RadioButtonDiameter = 12
        Me.rdoFalse.RadioButtonThickness = 2
        Me.rdoFalse.RadioYLocation = 7
        Me.rdoFalse.StringYLocation = 2
        Me.rdoFalse.TabStop = True
        Me.rdoFalse.TextColor = System.Drawing.Color.Black
        Me.rdoFalse.UseVisualStyleBackColor = True
        '
        'ctlProcessFlag
        '
        Me.Controls.Add(Me.rdoFalse)
        Me.Controls.Add(Me.rdoTrue)
        Me.Name = "ctlProcessFlag"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Member Variables "

    ' readonly flag for this control
    Private mReadOnly As Boolean

    ' The last value set in this control of the process value.
    ' Checked on a value setting to see if the Changed() event should occur
    Private mLastValue As clsProcessValue

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the value in this control.
    ''' </summary>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            If rdoTrue.Checked Then Return True
            If rdoFalse.Checked Then Return False
            Return New clsProcessValue(DataType.flag)
        End Get
        Set(ByVal val As clsProcessValue)
            ' Treat a null process value as, er, a process value of null
            If val Is Nothing Then val = New clsProcessValue(DataType.flag)
            Dim bool As Boolean = CBool(val)
            rdoTrue.Checked = bool
            rdoFalse.Checked = Not val.IsNull AndAlso Not bool
            mLastValue = val
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            mReadOnly = value
            rdoFalse.Enabled = Not value
            rdoTrue.Enabled = Not value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' We override the resetText function to clear the status of the radios
    ''' </summary>
    Public Overrides Sub ResetText()
        rdoTrue.Checked = False
        rdoFalse.Checked = False
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        Dim currValue As clsProcessValue = Me.Value
        If currValue.Equals(mLastValue) Then Return
        mLastValue = currValue
        OnChanged(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Selects this control
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        rdoTrue.Focus()
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles one of the raio buttons being clicked
    ''' </summary>
    Private Sub HandleRadioClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles rdoTrue.Click, rdoFalse.Click
        rdoTrue.Checked = (sender Is rdoTrue)
        rdoFalse.Checked = (sender Is rdoFalse)
        Commit()
    End Sub

    ''' <summary>
    ''' instead of focusing the control we want to focus the true radio
    ''' </summary>
    Protected Overrides Sub OnGotFocus(ByVal e As EventArgs)
        MyBase.OnGotFocus(e)
        ' Focus the currently checked radio button - focus neither if neither are
        ' checked because setting the focus on a radio button apparently checks it,
        ' regardless, incidentally, of the AutoCheck property, which is false for
        ' both of these radio buttons
        If rdoTrue.Checked Then rdoTrue.Focus() : Return
        If rdoFalse.Checked Then rdoFalse.Focus() : Return
    End Sub

    ''' <summary>
    ''' Checks to see if the process value has changed since it was last set, or
    ''' since the last Changed event. Raises the Changed event if it has.
    ''' </summary>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

#End Region

End Class
