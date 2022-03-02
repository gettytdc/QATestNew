Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessNumber
''' 
''' <summary>
''' Allows a clsProcessValue of datatype number to be edited.
''' </summary>
Public Class ctlProcessUnknown
    Inherits AutomateControls.Textboxes.StyledTextBox
    Implements IProcessValue

    Public Event Changed As EventHandler Implements IProcessValue.Changed

    ''' <summary>
    ''' Creates a new unknown data control
    ''' </summary>
    Public Sub New()
        Me.ReadOnly = True
        Me.Text = My.Resources.ctlProcessUnknown_UnknownDataType
        Me.Font = New Font(Me.Font, FontStyle.Bold)
        Me.BackColor = SystemColors.GrayText
    End Sub

    ''' <summary>
    ''' The default width applied to this control.
    ''' </summary>
    Private DefaultWidth As Integer = 100

    ''' <summary>
    ''' The default height applied to this
    ''' control.
    ''' </summary>
    Private DefaultHeight As Integer = 24

    Public Overrides Function GetPreferredSize(ByVal proposedSize As System.Drawing.Size) As System.Drawing.Size
        Return New Size(Math.Max(DefaultWidth, proposedSize.Width), DefaultHeight)
    End Function

    ''' <summary>
    ''' The underlying clsProcessValue to be edited
    ''' </summary>
    Private mValue As clsProcessValue = New clsProcessValue(DataType.unknown)


    ''' <summary>
    ''' The property that allows access to the underlying clsProcessValue stored in 
    ''' mobjValue , on set me.text to a hidden password string.
    ''' </summary>
    ''' <value></value>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return mValue
        End Get
        Set(ByVal Value As clsProcessValue)
            'do nothing
        End Set
    End Property


    Public Sub SelectControl() Implements IProcessValue.SelectControl
        Me.Select()
    End Sub

    Public Shadows Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return True
        End Get
        Set(ByVal value As Boolean)
            'Deliberately make this control always readonly
            MyBase.ReadOnly = True
        End Set
    End Property

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        ' This control is readonly, so there's no work to do here
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
End Class
