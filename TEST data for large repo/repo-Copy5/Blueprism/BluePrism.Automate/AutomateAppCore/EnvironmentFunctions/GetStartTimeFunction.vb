Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    ''' <summary>
    ''' Function to get the start time of a process instance
    ''' </summary>
    Public Class GetStartTimeFunction : Inherits GetStartTimeFunctionBase
        
        Private ReadOnly mServerFactory As Func(Of IServer)

        Public Sub New(serverFactory As Func(Of IServer))
            mServerFactory = serverFactory
        End Sub

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            If parameters.Count <> 0 Then
                Throw GetStartTimeFunctionShouldHaveNoParametersException()
            End If

            If process.Session Is Nothing Then
                Throw GetStartTimeFunctionSessionNotRunningException()
            End If

            Dim sessions = mServerFactory().GetActualSessions(process.Session.ID)
            If sessions.Count <> 1 Then
                Throw MissingSessionException()
            End If

            Dim sessionStart = sessions.First().SessionStart.ToUniversalTime().DateTime

            Return New clsProcessValue(DataType.datetime, sessionStart, False)
        End Function
    End Class
End NameSpace