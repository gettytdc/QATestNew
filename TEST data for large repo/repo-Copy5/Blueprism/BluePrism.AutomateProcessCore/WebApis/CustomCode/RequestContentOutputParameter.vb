Namespace WebApis.CustomCode
    ''' <summary>
    ''' Output parameter used in a <see cref="MethodType.RequestContent"/> custom code 
    ''' method
    ''' </summary>
    Public Class RequestContentOutputParameter : Implements IParameter

        Public ReadOnly Property Name As String Implements IParameter.Name
        Public ReadOnly Property DataType As DataType Implements IParameter.DataType

        Public ReadOnly Property Direction As ParameterDirection Implements IParameter.Direction
            Get
                Return ParameterDirection.Out
            End Get
        End Property

        Public Sub New(name As String)
            Me.Name = name
            Me.DataType = DataType.text
        End Sub
    End Class
End Namespace
