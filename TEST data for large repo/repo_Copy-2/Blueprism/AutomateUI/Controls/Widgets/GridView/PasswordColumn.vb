''' <summary>
''' A DataGridView column designed for displaying passwords
''' </summary>
Public Class PasswordColumn
    Inherits DataGridViewColumn

    ' Time interval before characters are masked
    Public Property DisplayInterval As Integer


    Public Sub New()
        MyBase.New(New PasswordCell())
        DisplayInterval = 1500
    End Sub


    ''' <summary>
    ''' Set the default cell template to PasswordCell
    ''' </summary>
    Public Overrides Property CellTemplate As DataGridViewCell
        Get
            Return New PasswordCell()
        End Get
        Set(value As DataGridViewCell)
            If value IsNot Nothing AndAlso Not value.GetType().IsAssignableFrom(GetType(PasswordCell)) Then
                Throw New InvalidCastException(My.Resources.PasswordColumn_CellTypeIsNotBasedUponThePasswordCell)
            End If
            MyBase.CellTemplate = value
        End Set
    End Property
End Class
