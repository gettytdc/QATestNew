Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessDate
''' 
''' <summary>
''' A date input control.
''' </summary>
Friend Class ctlProcessDate
    Inherits ctlProcessDateTime

    ''' <summary>
    ''' Creates a new date control
    ''' </summary>
    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Gets the type of value handled by this control. This implementation
    ''' handles <see cref="DataType.time"/> values.
    ''' </summary>
    Protected Overrides ReadOnly Property ValueType() As DataType
        Get
            Return DataType.date
        End Get
    End Property

    ''' <summary>
    ''' Initializes the component - created by the windows form designer
    ''' </summary>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessDate))
        Me.SuspendLayout()
        '
        'ctlProcessDate
        '
        Me.Name = "ctlProcessDate"
        resources.ApplyResources(Me, "$this")
        Me.TimeButtonVisible = False
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
End Class
