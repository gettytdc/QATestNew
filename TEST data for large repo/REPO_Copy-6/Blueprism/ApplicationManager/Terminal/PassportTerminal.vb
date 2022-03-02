Public Class PassportTerminal
    Inherits AbstractTerminal

    Private moPassportAutomation As clsPassportAutomation

    Public Sub New()
        moPassportAutomation = New clsPassportAutomation
    End Sub

    Public Overrides Function ConnectToHostOrSession(ByVal sProfile As String, ByVal sShortName As String, ByRef sError As String) As Boolean
        Return moPassportAutomation.ConnectToHost(sProfile, sError)
    End Function

    Public Overrides Function DisconnectFromHost(ByRef sError As String) As Boolean
        Return moPassportAutomation.DisonnectFromHost(sError)
    End Function

    Public Overrides Function Launch(sessionProfile As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Attach(sessionShortName As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Detach(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Terminate(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function GetSessionSize(ByRef iRows As Integer, ByRef iColumns As Integer, ByRef sError As String) As Boolean
        Return moPassportAutomation.GetSessionSize(iRows, iColumns, sError)
    End Function

    Public Overrides Function GetText(ByVal iRow As Integer, ByVal iColumn As Integer, ByVal Length As Integer, ByRef Value As String, ByRef sError As String) As Boolean
        Return moPassportAutomation.GetText(Value, iRow, iColumn, Length, sError)
    End Function

    Public Overrides Function SendKeystroke(ByVal sKey As String, ByRef sError As String) As Boolean
        Return moPassportAutomation.SendKeystroke(sKey, sError)
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function SetText(ByVal iRow As Integer, ByVal iColumn As Integer, ByVal Value As String, ByRef sError As String) As Boolean
        Return moPassportAutomation.SetText(Value, iRow, iColumn, sError)
    End Function

    Public Overrides Function IsConnected() As Boolean
        Return moPassportAutomation.IsConnected
    End Function

    Public Overrides Function GetWindowTitle(ByRef Value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function SetWindowTitle(ByVal Value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean
        Return moPassportAutomation.GetCursorPosition(row, col, sErr)
    End Function

    Public Overrides Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean
        Return moPassportAutomation.SetCursorPosition(row, col, sErr)
    End Function

    Public Overrides Function RunEmulatorMacro(ByVal MacroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.NotImplementedForPassportTerminals
        Return False
    End Function

    Public Overrides Function SelectArea(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal EndRow As Integer, ByVal EndColumn As Integer, ByVal Type As AbstractTerminal.SelectionType, ByRef sErr As String) As Boolean
        Dim AreaType As clsPassportAutomation.SelectionType
        Select Case Type
            Case SelectionType.Block
                AreaType = clsPassportAutomation.SelectionType.Block
            Case SelectionType.Continuous
                AreaType = clsPassportAutomation.SelectionType.Continuous
        End Select

        Return moPassportAutomation.SelectArea(StartRow, StartColumn, EndRow, EndColumn, AreaType, sErr)
    End Function

    Public Overrides Property SleepTime() As Integer
        Get
            'do nothing - not relevant to passport terminals
        End Get
        Set(ByVal value As Integer)
            'do nothing - not relevant to passport terminals
        End Set
    End Property

    Public Overrides Property WaitTimeout() As Integer
        Get
            Return Me.moPassportAutomation.TimeOut
        End Get
        Set(ByVal value As Integer)
            Me.moPassportAutomation.TimeOut = value
        End Set
    End Property

End Class
