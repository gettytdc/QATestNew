Option Strict Off

Public Class IBMTerminal
    Inherits AbstractTerminal

    Private Enum ConnectionStyle
        Normal = 1
        Maximized = 2
        Minimized = 3
        Hidden = 4
    End Enum

    ''' <summary>
    ''' Return values from InputInhibited method
    ''' </summary>
    Private Enum InhibitReasons
        NotInhibited = 0
        SystemWait = 1
        CommCheck = 2
        ProgCheck = 3
        MachCheck = 4
        OtherInhibit = 5
    End Enum

    Private mobjPCOMConnMgr As Object
    Private mobjPCOMConnList As Object
    Private mobjPCOMSession As Object
    Private mobjPCOMPS As Object
    Private mobjPCOMOIA As Object
    Private msSessionShortName As String

    ''' <summary>
    ''' The number of milliseconds for which to sleep
    ''' between polls of the API when waiting for idle host.
    ''' </summary>
    Private mWaitSleepTime As Integer = 1000

    ''' <summary>
    ''' The total waiting time in milliseconds before a request should
    ''' timeout
    ''' </summary>
    Private mWaitTimeout As Integer = 30000

    Private Property ClientHandle As Integer = -1 'default this to -1 as no handle will ever have -1 and so makes it easier to catch the fact that it has not been set

    Public Overrides Function ConnectToHostOrSession(ByVal SessionProfile As String, ByVal SessionShortName As String, ByRef sErr As String) As Boolean
        Try
            If SessionShortName = "" Then SessionShortName = "A"
            If Not StartConnection(SessionProfile, SessionShortName, ConnectionStyle.Normal, sErr) Then Return False
            If Not StartSession(sErr) Then Return False

            mobjPCOMSession.StartCommunication()

            Return WaitForIdleHost(sErr)

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean

        Try
            If Not mobjPCOMSession Is Nothing Then
                mobjPCOMSession.StopCommunication()
            End If

            If mobjPCOMConnMgr Is Nothing Then
                mobjPCOMConnMgr = CreateObject("PCOMM.autECLConnMgr")
            End If
            mobjPCOMConnMgr.StopConnection(ClientHandle)
            Return True

        Catch e As Exception
            sErr = e.ToString
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
            If mobjPCOMPS Is Nothing Then
                mobjPCOMPS = CreateObject("PCOMM.autECLPS")
                mobjPCOMPS.SetConnectionByHandle(ClientHandle)
            End If

            Value = mobjPCOMPS.GetText(StartRow, StartColumn, Length)
            If Value IsNot Nothing Then
                'Corrects strings that are null terminated
                Value = ChopNullTerminated(Value)
            End If
            Return True
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function SendKeystroke(ByVal Key As String, ByRef sErr As String) As Boolean

        Try

            mobjPCOMPS = CreateObject("PCOMM.autECLPS")
            mobjPCOMPS.SetConnectionByHandle(ClientHandle)
            mobjPCOMPS.SendKeys(Key)

            Return WaitForIdleHost(sErr)

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function SetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Value As String, ByRef sErr As String) As Boolean

        Try
            If mobjPCOMPS Is Nothing Then
                mobjPCOMPS = CreateObject("PCOMM.autECLPS")
                mobjPCOMPS.SetConnectionByHandle(ClientHandle)
            End If
            mobjPCOMPS.SetText(Value, StartRow, StartColumn)

            Return WaitForIdleHost(sErr)

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function GetWindowTitle(ByRef Value As String, ByRef sErr As String) As Boolean

        Try
            Dim mobjPCOMWinMetrics As Object
            mobjPCOMWinMetrics = CreateObject("PCOMM.autECLWinMetrics")

            Dim objConnList As Object
            objConnList = CreateObject("Pcomm.autECLConnList")

            objConnList.refresh()
            mobjPCOMWinMetrics.SetConnectionByHandle(ClientHandle)

            Value = mobjPCOMWinMetrics.WindowTitle
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function SetWindowTitle(ByVal Value As String, ByRef sErr As String) As Boolean

        Try
            Dim mobjPcomWinMetrics As Object
            mobjPcomWinMetrics = CreateObject("PCOMM.autECLWinMetrics")

            Dim objConnList As Object
            objConnList = CreateObject("Pcomm.autECLConnList")

            objConnList.refresh()
            mobjPcomWinMetrics.SetConnectionByHandle(ClientHandle)

            mobjPcomWinMetrics.WindowTitle = Value
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean

        Try
            Dim ps = CreateObject("PCOMM.autECLPS")

            Dim objConnList As Object
            objConnList = CreateObject("Pcomm.autECLConnList")

            objConnList.refresh()
            ps.SetConnectionByHandle(ClientHandle)

            row = ps.CursorPosRow
            col = ps.CursorPosCol
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean

        Try
            Dim ps = CreateObject("PCOMM.autECLPS")

            Dim objConnList As Object
            objConnList = CreateObject("Pcomm.autECLConnList")

            objConnList.refresh()
            ps.SetConnectionByHandle(ClientHandle)

            ps.SetCursorPos(row, col)
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function GetSessionSize(ByRef NumRows As Integer, ByRef NumColumns As Integer, ByRef sErr As String) As Boolean

        If mobjPCOMPS Is Nothing Then Return False

        Try
            NumRows = mobjPCOMPS.NumRows
            NumColumns = mobjPCOMPS.NumCols

            Return True
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function IsConnected() As Boolean

        Try
            If mobjPCOMConnList IsNot Nothing Then
                Dim connection As Object = mobjPCOMConnList.FindConnectionByHandle(Me.ClientHandle)
                If connection IsNot Nothing Then
                    Return connection.CommStarted
                End If
            End If
        Catch
            Return False
        End Try

        Return False
    End Function

    Public Overrides Function SelectArea(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal EndRow As Integer, ByVal EndColumn As Integer, ByVal Type As AbstractTerminal.SelectionType, ByRef sErr As String) As Boolean
        sErr = My.Resources.SelectionsAreNotPossibleOnAnIBMTerminal
        Return False
    End Function

    Public Overrides Function RunEmulatorMacro(ByVal MacroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.NotImplementedForIBMTerminals
        Return False
    End Function

    Public Overrides Property SleepTime() As Integer
        Get
            Return mWaitSleepTime
        End Get
        Set(ByVal value As Integer)
            mWaitSleepTime = value
        End Set
    End Property

    Public Overrides Property WaitTimeout() As Integer
        Get
            Return mWaitTimeout
        End Get
        Set(ByVal value As Integer)
            mWaitTimeout = value
        End Set
    End Property


    ''' <summary>
    ''' Begins a connection using the specified configuration options. This is
    ''' equivalent to opening a graphical window manually.
    ''' </summary>
    ''' <param name="sProfile">A profile string. This should be the path
    ''' to a file with extension .WS on a readable filesystem. Do not enclose
    ''' path in quote marks as this is done automatically before passing
    ''' to the API call.</param>
    ''' <param name="sSessionShortName">The host short name (ie a letter "A".."Z").
    ''' If not supplied then the next available letter will be assigned
    ''' automatically.</param>
    ''' <param name="style">Style of the window when opened.</param>
    ''' <param name="sErr">String to carry an error message.</param>
    ''' <returns>Returns true if call is successful; false otherwise.</returns>
    Private Function StartConnection(ByVal sProfile As String, ByVal sSessionShortName As String, ByVal style As ConnectionStyle, ByRef sErr As String) As Boolean


        msSessionShortName = sSessionShortName

        Dim sStyle As String = ""
        Select Case style
            Case ConnectionStyle.Normal
                sStyle = "RESTORE"
            Case ConnectionStyle.Maximized
                sStyle = "MAX"
            Case ConnectionStyle.Minimized
                sStyle = "MIN"
            Case ConnectionStyle.Hidden
                sStyle = "HIDE"
            Case Else
                sErr = My.Resources.FailedToStartIBMPersonalCommunicationsConnectionUnknownConnectionStyleSpecified
                Return False
        End Select

        Dim sConnectionString As String = ""
        sConnectionString = "profile=""" & sProfile & """ connname=" & sSessionShortName & " winstate=" & sStyle

        Try
            mobjPCOMConnMgr = CreateObject("PCOMM.autECLConnMgr")

            ' Before we start a new session see if one already exists
            ' if a sesssion alerady exists with the same name then we cannot start a new one
            ' but we could attach to one if needed.
            ' if one already exists then the process must stop
            If mobjPCOMConnList Is Nothing Then
                mobjPCOMConnList = CreateObject("PCOMM.autECLConnList")
            End If


            mobjPCOMConnList.Refresh()

            Dim obj As Object

            Try
                obj = mobjPCOMConnList.FindConnectionByName(msSessionShortName)
                ClientHandle = obj.handle
            Catch ex As Exception
                ClientHandle = 0
            End Try

            If ClientHandle = 0 Then
                ' no connections current exist so simply start it
                mobjPCOMConnMgr.StartConnection(sConnectionString)

                'wait 120s for session to appear
                Dim iWait As Integer = 0

                Do While ClientHandle <= 0
                    System.Threading.Thread.Sleep(250)
                    iWait = iWait + 250

                    mobjPCOMConnList.Refresh()

                    Try
                        ClientHandle = mobjPCOMConnList.FindConnectionByName(msSessionShortName).handle
                        Exit Do
                    Catch ex As Exception
                        ClientHandle = -1
                    End Try

                    If iWait > 120000 Then
                        sErr = String.Format(My.Resources.FailedToOpenANewConnectionToHost0WithinTheTimeoutPeriod, sSessionShortName)
                        Return False
                    End If
                Loop
            End If

            Return True

        Catch e As Exception
            sErr = e.Message
            Return False
        End Try
    End Function

    Public Function StartSession(ByRef sErr As String) As Boolean


        Try
            If mobjPCOMConnList Is Nothing Then
                mobjPCOMConnList = CreateObject("PCOMM.autECLConnList")
            End If

            If mobjPCOMSession Is Nothing Then
                mobjPCOMSession = CreateObject("PCOMM.autECLSession")
            End If

            If mobjPCOMPS Is Nothing Then
                mobjPCOMPS = CreateObject("PCOMM.autECLPS")
            End If

            ' Initialize the session
            mobjPCOMConnList.Refresh()

            Dim iCounter As Integer = 0

            Do While ClientHandle = 0 'up to 60 secs
                Try
                    ClientHandle = mobjPCOMConnList.FindConnectionByName(msSessionShortName).Handle
                Catch ex As Exception
                    sErr = String.Format(My.Resources.StartSessionFailedToConnectClientHandle0, ClientHandle)
                    Return False
                End Try
                If iCounter > 60 Then
                    sErr = My.Resources.StartSessionClientHandleNotFoundAfter60Seconds
                    Return False
                End If

                iCounter = iCounter + 1
                Threading.Thread.Sleep(1000)

            Loop

            iCounter = 0


            mobjPCOMSession.SetConnectionByHandle(ClientHandle)
            '   AddHandler mobjPCOMSession.NotifyCommEvent, AddressOf Me.HandleCommsEvent
            '   mobjPCOMSession.RegisterCommEvent()

            Do

                If mobjPCOMSession.Ready Then
                    Exit Do
                End If

                If iCounter > 60 Then
                    sErr = My.Resources.StartSessionSessionNotReadyAfter60Seconds
                    Return False
                End If

                iCounter = iCounter + 1
                Threading.Thread.Sleep(1000)
            Loop While True

            mobjPCOMPS.SetConnectionByHandle(ClientHandle)

            Do

                If mobjPCOMPS.Ready Then
                    Exit Do
                End If

                If iCounter > 60 Then
                    sErr = My.Resources.StartSessionPresentationSpaceIsNotReadyAfter60Seconds
                    Return False
                End If

                iCounter = iCounter + 1
                Threading.Thread.Sleep(1000)
            Loop While True


            Return True
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Given a string with null characters, chops it up and returns the portion of
    ''' the string before the first null character.
    ''' </summary>
    ''' <param name="s">The string to chop, should not be Nothing</param>
    ''' <returns>The chopped string</returns>
    Private Function ChopNullTerminated(s As String) As String
        Dim i = s.IndexOf(Chr(0))
        If i > 0 Then
            Return s.Remove(i)
        End If
        Return s
    End Function

    Public Function WaitForIdleHost(ByRef sErr As String) As Boolean

        Dim sleepTime As Integer = sleepTime
        If mobjPCOMOIA Is Nothing Then
            mobjPCOMOIA = CreateObject("PCOMM.autECLOIA")
            mobjPCOMOIA.SetConnectionByHandle(ClientHandle)
        End If

        Dim iAttempts As Integer = 0
        Dim iMaxAttempts As Integer = 1

        Dim LastInhibitReason As Integer
        Try
            Dim StartTime As DateTime = DateTime.Now
            Do
                LastInhibitReason = mobjPCOMOIA.InputInhibited
                If LastInhibitReason = 0 Then
                    iAttempts = iAttempts + 1
                End If

                If iAttempts = iMaxAttempts Then
                    Return True
                End If
                System.Threading.Thread.Sleep(sleepTime)

                If DateTime.Now.Subtract(StartTime).TotalMilliseconds >= Me.WaitTimeout Then
                    sErr = String.Format(My.Resources.TheHostNeverBecameIdleWithinTheLimitedTimeoutPeriodOf0SecondsLastInhibitReasonW, ((WaitTimeout) / 1000), CType(LastInhibitReason, InhibitReasons).ToString)
                    Return False
                End If

            Loop While True

        Catch e As Exception
            sErr = e.ToString
            Return False
        Finally
            mobjPCOMOIA = Nothing
        End Try
    End Function


End Class

