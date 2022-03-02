Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessTime
''' 
''' <summary>
''' This control allows a user to edit an automate Time
''' the control is related to a ctlProcessDateTime, and uses most of the functionality
''' from there
''' </summary>
Friend Class ctlProcessTime : Inherits ctlProcessDateTime

    ''' <summary>
    ''' Gets the type of value handled by this control. This implementation
    ''' handles <see cref="DataType.time"/> values.
    ''' </summary>
    Protected Overrides ReadOnly Property ValueType() As DataType
        Get
            Return DataType.time
        End Get
    End Property

    ''' <summary>
    ''' Creates a new time control
    ''' </summary>
    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Initializes the component - created by the windows form designer
    ''' </summary>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessTime))
        Me.SuspendLayout()
        '
        'ctlProcessTime
        '
        Me.DateButtonVisible = False
        Me.Name = "ctlProcessTime"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
End Class
