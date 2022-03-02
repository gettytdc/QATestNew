Option Strict Off

Public Class ARTerminal
    Inherits AbstractTerminal

    ''' <summary>
    ''' Reference to the reflections COM object.
    ''' </summary>
    Private mobjReflections As Object


    Public Sub New()
        mobjReflections = CreateObject("Reflection2.Session")
    End Sub

    Public Overrides Function ConnectToHostOrSession(ByVal SessionProfile As String, ByVal SessionShortName As String, ByRef sErr As String) As Boolean
        Try
            With mobjReflections
                .OpenSettings(SessionProfile)
                .Visible = True
                If Not IsConnected() Then
                    .Connect()
                End If
            End With
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean
        Try
            If IsConnected() Then
                mobjReflections.Disconnect()
                mobjReflections = Nothing
            End If
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
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

    Public Overrides Function GetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Length As Integer, ByRef Value As String, ByRef sErr As String) As Boolean
        Try
            Dim EndRow As Integer = StartRow
            Dim EndColumn As Integer = StartColumn + (Length - 1)
            Value = mobjReflections.GetText(StartRow - 1, StartColumn - 1, EndRow - 1, EndColumn - 1)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendKeystroke(ByVal Key As String, ByRef sErr As String) As Boolean
        Try
            Dim keyid As Integer = 0
            mobjReflections.TransmitTerminalKey(keyid)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function SetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Value As String, ByRef sErr As String) As Boolean
        Try
            SetCursorPosition(StartRow, StartColumn, sErr)
            mobjReflections.Transmit(Value)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
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
        Try
            With mobjReflections
                col = .CursorColumn
                row = .CursorRow
            End With
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean
        Try
            With mobjReflections
                If .CursorColumn <> col OrElse .CursorRow <> row Then
                    SelectArea(row, col, row, col, SelectionType.Continuous, sErr)
                End If
            End With
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function RunEmulatorMacro(macroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function GetSessionSize(ByRef NumRows As Integer, ByRef NumColumns As Integer, ByRef sErr As String) As Boolean
        Try
            With mobjReflections
                NumColumns = .DisplayColumns
                NumRows = .DisplayRows
            End With
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function IsConnected() As Boolean
        Try
            Return mobjReflections.Connected
        Catch
            Return False
        End Try
    End Function

    Public Overrides Function SelectArea(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal EndRow As Integer, ByVal EndColumn As Integer, ByVal Type As AbstractTerminal.SelectionType, ByRef sErr As String) As Boolean
        Try
            mobjReflections.SelectText(StartRow, StartColumn, EndRow, EndColumn)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function


    Public Overrides Property SleepTime() As Integer
        Get
            'do nothing - not relevant to attachmate terminals
        End Get
        Set(ByVal value As Integer)
            'do nothing - not relevant to attachmate terminals
        End Set
    End Property


    Public Overrides Property WaitTimeout() As Integer
        Get
            '.WaitTimeoutNumHalfSeconds * 500
        End Get
        Set(ByVal value As Integer)
            '.WaitTimeoutNumHalfSeconds = CInt(Math.Ceiling(value / 500))
        End Set
    End Property

End Class
