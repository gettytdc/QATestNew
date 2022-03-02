Namespace WebApis.CustomCode
    ''' <summary>
    ''' Input parameter used in a <see cref="MethodType.OutputParameter"/> custom code 
    ''' method
    ''' </summary>
    Public Class ResponseContentInputParameter : Implements IParameter

        Public ReadOnly Property Name As String Implements IParameter.Name
        Public ReadOnly Property DataType As DataType Implements IParameter.DataType
        Public ReadOnly Property Direction As ParameterDirection = ParameterDirection.In Implements IParameter.Direction

        Public Sub New(name As String)
            Me.Name = name
            Me.DataType = DataType.text
        End Sub

    End Class
End Namespace
