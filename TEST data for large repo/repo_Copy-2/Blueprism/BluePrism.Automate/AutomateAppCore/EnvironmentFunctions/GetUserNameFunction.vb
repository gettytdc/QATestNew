Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    Public Class GetUserNameFunction : Inherits GetUserNameFunctionBase
        
        Private ReadOnly mServerFactory As Func(Of IServer)

        Public Sub New(serverFactory As Func(Of IServer))
            mServerFactory = serverFactory
        End Sub

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            If parameters.Count <> 0 Then
                Throw New clsFunctionException(
                 My.Resources.GetUserNameFunction_GetUserNameFunctionShouldNotHaveAnyParameters)
            End If

            'Get the username, if there's a session to get it from
            Dim user As String = ""
            If process.Session IsNot Nothing Then
                user = mServerFactory().GetSessionDetails(process.Session.ID).UserName
            End If

            Return New clsProcessValue(DataType.text, user)

        End Function      
    End Class
End NameSpace