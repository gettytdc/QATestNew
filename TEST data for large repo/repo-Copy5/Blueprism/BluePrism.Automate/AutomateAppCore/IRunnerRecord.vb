Imports BluePrism.AutomateProcessCore

Public Interface IRunnerRecord

    Sub RunnerMethod()

    Sub StopProcess(userName As String, resName As String,
        Optional reason As String = "")

    ReadOnly Property SessionStarted As DateTimeOffset

    Function GetSessionVariables() As IDictionary(Of String, clsProcessValue)

End Interface
