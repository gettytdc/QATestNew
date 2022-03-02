Option Strict Off

Public Class clsPassportAutomation

#Region "PASSOBJLIB.dll"

    'PASSOBJLIB.dll
    'Private moPassport As PASSOBJLib.System
    'Private moSession As PASSOBJLib.Session
    'Private moScreen As PASSOBJLib.Screen

    Private moPassport As Object
    Private moSession As Object
    Private moScreen As Object
    ''PASSHIOLib.dll
    'Private oOhioSessions As PASSHIOLib.OhioSessions
    'Private oOhioSession As PASSHIOLib.OhioSession
    'Private oOhioScreen As PASSHIOLib.OhioScreen

#End Region

#Region "New"

    Public Sub New()
        'moPassport = CType(CreateObject("PASSPORT.System"), PASSOBJLib.System)
        moPassport = CreateObject("PASSPORT.System")
        moPassport.TimeoutValue = 30000
    End Sub

    Public Sub New(ByVal TimeOut As Integer)
        ' moPassport = CType(CreateObject("PASSPORT.System"), PASSOBJLib.System)
        moPassport = CreateObject("PASSPORT.System")
        moPassport.TimeoutValue = TimeOut
    End Sub

#End Region

#Region "TimeOut"

    Public Property TimeOut() As Integer
        Get
            Return moPassport.TimeoutValue
        End Get
        Set(ByVal value As Integer)
            moPassport.TimeoutValue = value
        End Set
    End Property

#End Region

#Region "ConnectToHost"

    Public Function ConnectToHost(ByVal sProfile As String, ByRef sError As String) As Boolean

        Try
            Dim PreviousCount As Integer = moPassport.sessions.count
            moPassport.Sessions.Open(sProfile)
            If moPassport.sessions.count > PreviousCount Then
                moSession = moPassport.sessions.item(PreviousCount + 1)
                moScreen = moSession.Screen
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            sError = ex.Message
            Return False
        End Try

    End Function

#End Region

#Region "DisonnectFromHost"

    Public Function DisonnectFromHost(ByRef sError As String) As Boolean

        Try
            moPassport.Quit()
            Return True
        Catch ex As Exception
            sError = ex.Message
            Return False
        End Try

    End Function

#End Region

    Public Function IsConnected() As Boolean
        Try
            If (moScreen IsNot Nothing) AndAlso (moScreen.OIA IsNot Nothing) Then
                Dim ConnectionStatus As Integer = moScreen.oia.connectionstatus
                If ConnectionStatus = 1 Then Return True
                If ConnectionStatus = 2 Then Return True
            End If

            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

#Region "WaitForIdleHost"

    Public Function WaitForIdleHost(ByVal iTimeOut As Integer, ByRef sError As String) As Boolean

        'Dim oScreenWait As PASSOBJLib.ScreenWait
        Dim oScreenWait As Object
        Dim iResult, iTemp As Integer
        Dim sMessage As String = My.Resources.FailedToWaitForIdleHost

        Try
            If iTimeOut >= 0 Then
                iTemp = moPassport.TimeoutValue
                moPassport.TimeoutValue = iTimeOut
            End If

            oScreenWait = moScreen.WaitHostQuiet()
            iResult = oScreenWait.Wait()
            If oScreenWait.Value = -1 Then
                Return True
            Else
                sError = sMessage
                Return False
            End If
        Catch ex As Exception
            sError = String.Format(My.Resources.x01, sMessage, ex.Message)
            Return False
        Finally
            If iTimeOut >= 0 Then
                moPassport.TimeoutValue = iTemp
            End If
        End Try

    End Function

    Public Function WaitForIdleHost(ByRef sError As String) As Boolean
        Return WaitForIdleHost(-1, sError)
    End Function

#End Region

#Region "WaitForText"

    Public Function WaitForText(ByVal sText As String, ByVal iRow As Integer, ByVal iColumn As Integer, ByVal iTimeOut As Integer, ByRef sError As String) As Boolean

        'Dim oScreenWait As PASSOBJLib.ScreenWait
        Dim oScreenWait As Object
        Dim iResult, iTemp As Integer
        Dim sMessage As String = String.Format(My.Resources.FailedToWaitForText0, sText)

        Try
            If iTimeOut >= 0 Then
                iTemp = moPassport.TimeoutValue
                moPassport.TimeoutValue = iTimeOut
            End If

            If iColumn < 1 Or iRow < 1 Then
                oScreenWait = moScreen.WaitForString(sText)
            Else
                oScreenWait = moScreen.WaitForString(sText, iRow, iColumn)
            End If
            iResult = oScreenWait.Wait()
            If oScreenWait.Value = -1 Then
                Return True
            Else
                sError = sMessage
                Return False
            End If
        Catch ex As Exception
            sError = String.Format(My.Resources.x01, sMessage, ex.Message)
            Return False
        Finally
            If iTimeOut >= 0 Then
                moPassport.TimeoutValue = iTemp
            End If
        End Try

    End Function

    Public Function WaitForText(ByVal sText As String, ByVal iRow As Integer, ByVal iColumn As Integer, ByRef sError As String) As Boolean

        Return WaitForText(sText, iRow, iColumn, -1, sError)

    End Function

    Public Function WaitForText(ByVal sText As String, ByVal iTimeOut As Integer, ByRef sError As String) As Boolean

        Return WaitForText(sText, -1, -1, iTimeOut, sError)

    End Function

    Public Function WaitForText(ByVal sText As String, ByRef sError As String) As Boolean

        Return WaitForText(sText, -1, -1, -1, sError)

    End Function

#End Region

#Region "GetSessionSize"

    Public Function GetSessionSize(ByRef iRows As Integer, ByRef iColumns As Integer, ByRef sError As String) As Boolean

        Try
            iRows = moScreen.Rows
            iColumns = moScreen.Cols
            Return True
        Catch ex As Exception
            sError = My.Resources.FailedToGetSessionSize & ex.Message
            Return False
        End Try

    End Function

#End Region

    Public Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean

        Try
            Dim pos As Integer = moSession.Screen.Cursor()
            row = (pos - 1) \ moSession.Screen.Cols + 1
            col = pos - ((row - 1) * moSession.Screen.Cols) + 1
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean

        Try

            moSession.Screen.CursorRC(row, col)
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function


#Region "GetText"

    Public Function GetText(ByRef sText As String, ByVal iRow As Integer, ByVal iColumn As Integer, ByVal iLength As Integer, ByRef sError As String) As Boolean

        Try
            sText = moSession.Screen.GetString(CShort(iRow), CShort(iColumn), CShort(iLength))
            Return True
        Catch ex As Exception
            sError = My.Resources.FailedToGetText & ex.Message
            Return False
        End Try

    End Function


#End Region

#Region "SetText"

    Public Function SetText(ByVal sText As String, ByRef sError As String) As Boolean

        Try
            moSession.Screen.PutString(sText)
            Return True
        Catch ex As Exception
            sError = String.Format(My.Resources.FailedToSetText01, sText, ex.Message)
            Return False
        End Try

    End Function

    Public Function SetText(ByVal sText As String, ByVal iRow As Integer, ByVal iColumn As Integer, ByRef sError As String) As Boolean

        Try
            moSession.Screen.PutString(sText, CInt(iRow), CInt(iColumn))
            Return True
        Catch ex As Exception
            sError = String.Format(My.Resources.FailedToSetText01, sText, ex.Message)
            Return False
        End Try

    End Function


#End Region

#Region "SendKeystroke"

    Public Function SendKeystroke(ByVal sKey As String, ByRef sError As String) As Boolean

        Try
            moSession.Screen.SendKeys(sKey)
            Return True
        Catch ex As Exception
            sError = String.Format(My.Resources.FailedToSendKeyStroke01, sKey, ex.Message)
            Return False
        End Try

    End Function

#End Region

    Public Enum SelectionType
        Block
        Continuous
    End Enum

    Public Function SelectArea(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal EndRow As Integer, ByVal EndColumn As Integer, ByVal Type As SelectionType, ByRef sErr As String) As Boolean
        Try
            Dim a As Object = moScreen.area(StartRow, StartColumn, EndRow, EndColumn)
            a.select()

            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function


#Region "GetFields"

    Public Function GetFields(ByRef Fields As Field(), ByRef sError As String) As Boolean

        Try
            Fields = New Field() {}
            Return True
        Catch ex As Exception
            sError = String.Format(My.Resources.FailedToGetFields0, ex.Message)
            Return False
        End Try

    End Function


#End Region

#Region "Class Field"

    Public Class Field

        Private miPosition As Integer
        Private miLength As Integer

        Public ReadOnly Property Position() As Integer
            Get
                Return miPosition
            End Get
        End Property

        Public ReadOnly Property Length() As Integer
            Get
                Return miLength
            End Get
        End Property

        Public Sub New(ByVal p As Integer, ByVal l As Integer)
            miPosition = p
            miLength = l
        End Sub

    End Class

#End Region

End Class
