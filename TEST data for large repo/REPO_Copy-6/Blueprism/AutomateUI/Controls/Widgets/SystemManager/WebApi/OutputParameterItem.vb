Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.WebApis

Namespace Controls.Widgets.SystemManager.WebApi
    Friend Class ResponseDataGridRowValues : Implements IParameter

        Public ReadOnly Property Index As Integer
        Public ReadOnly Property ParamName As String Implements IParameter.Name
        Public ReadOnly Property Description As String
        Public ReadOnly Property Type As DataType Implements IParameter.DataType
        Public ReadOnly Property MethodType As OutputMethodType
        Public ReadOnly Property JsonPath As String
        Public ReadOnly Property Direction As ParameterDirection = ParameterDirection.Out Implements IParameter.Direction

        Sub New(index As Integer, paramName As String, description As String, dataType As DataType,
            methodType As OutputMethodType, jsonPath As String)

            Me.Index = index
            Me.ParamName = paramName
            Me.Description = description
            Me.Type = dataType
            Me.MethodType = methodType
            Me.JsonPath = jsonPath
        End Sub
    End Class
End Namespace
