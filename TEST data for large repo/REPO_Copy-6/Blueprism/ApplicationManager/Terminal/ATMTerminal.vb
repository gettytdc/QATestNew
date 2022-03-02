Imports System.Reflection
Imports System.Runtime.InteropServices

Public Class ATMTerminal
    Inherits AbstractTerminal

    Public Enum SessionStyle
        Normal = 1
        Maximized = 2
        Iconized = 3
        Hidden = 4
    End Enum

    '  Global constants for session connection types
    Public Enum HostAccessValues
        ATM_EXTRA = 1 ' Extra! 16 bit and EPC6.x (32 bit)
        ATM_RALLY = 2 ' Rally! 16 bit products
        ATM_KEA = 3 ' KEA 16 and 32-bit DEC VT products
        ATM_IRMA = 4 ' Irma 16-bit products
        ATM_ICONN = 5 ' INFOConnect 16 and 32-bit products
        ATM_HP = 6 ' KEA! HP
    End Enum

    Private Enum ConnectionStatus
        [Default] = 0 'None of the following ATM_CONNECTION states hold
        ATM_APPOWNED = 1 'Session is connected to an application.
        ATM_SSCP = 2 'Control program owns session.
        ATM_UNOWNED = 3 'Session not connected to an application.
        ATM_NONE = 4 'None of the foregoing conditions applies. For async, not all connection statuses apply.
        ATM_UNKNOWN = 5
    End Enum

    Public Enum ConnectionStatusCalls
        ATM_XSTATUS = 1 'Returns an integer indicating the status of the XCLOCK .Not applicable for async sessions.
        ATM_CONNECTION = 2 'Returns an integer indicating the status of the host connection.
        ATM_ERROR = 3 'Returns the integer value of any existing error condition. Not applicable for async sessions.
    End Enum

    Private Enum ConnectionStatusErrors
        None = 0 'None of the following ATM_ERROR states apply
        ATM_PROGCHECK = 1 'XPROG756 Configuration mismatch occurred.
        ATM_COMMCHECK = 2 '-+Z_510 Communications hardware problem occurred.
        ATM_MACHINECHECK = 3 'x0211 Problem with the physical connection occurred.
        ATM_NONE = 4 'Status does not apply because it's an async connection
        ATM_UNKNOWN = 5 ' Unknown status
    End Enum

    ''' <summary>
    ''' The session shortname. Eg "A", "B", etc
    ''' </summary>
    Private mSessionName As String
    Private mRegisteredClient As Boolean = False
    Private mClientHandle As Integer = 0

    Private Shared ReadOnly mRegistrationLock As New Object()

    Public Sub New()
        Dim sErr As String = Nothing
        If Not CreateUniqueSessionHandle(sErr) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToCreateSessionHandle0, sErr))
        End If
    End Sub

    Public Overrides Function ConnectToHostOrSession(sessionProfile As String, sessionShortName As String, ByRef sErr As String) As Boolean
        'The parameter SessionProfile is not used. It appears because it is part of
        'the union of interfaces with other similar APIs
        Try
            If Not RegisterClient(CInt(mVariantType), sErr) Then Return False

            'The ATMConnectSession function specifies the host session with which your application will interact.
            Dim result = ATMConnectSession(mClientHandle, sessionShortName)

            Select Case result
                Case ATM_SUCCESS
                    Exit Select 'Success so exit the select

                Case ATM_INVALIDPARAMETER
                    sErr = String.Format(My.Resources.FailedToConnectToAttachmateHost0DueToAnInvalidParameter, sessionShortName)
                    Return False
                Case ATM_SESSIONLOCKED
                    sErr = String.Format(My.Resources.FailedToConnectToAttachmateHost0DueToTheSessionBeingLocked, sessionShortName)
                    Return False
                Case ATM_SYSTEMERROR
                    sErr = String.Format(My.Resources.FailedToConnectToAttachmateHost0, sessionShortName)
                    Return False
                Case ATM_NOMATCHINGPSID
                    sErr = String.Format(My.Resources.FailedToConnectToAttachmateHost0DueToNoMatchingPresentationSpaceIDPSID, sessionShortName)
                    Return False
                Case Else
                    sErr = String.Format(My.Resources.FailedToConnectToAttachmateHost0WithAnUnknownErrorCodeTheErrorCodeWas1, sessionShortName, result.ToString)
                    Return False
            End Select

            If Not StartSession(sessionShortName, SessionStyle.Normal, sErr) Then Return False

            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean
        Try
            If Not StopSession(sErr) Then Return False

            Dim result = ATMDisconnectSession(mClientHandle)
            If Not result = ATM_SUCCESS Then
                sErr = GetErrorMessage(result, My.Resources.Disconnect)
                Return False
            End If

            If Not UnRegisterClient(sErr) Then Return False

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

    Public Overrides Function GetText(startRow As Integer, startColumn As Integer, length As Integer, ByRef value As String, ByRef sErr As String) As Boolean

        If length < 0 Then
            sErr = My.Resources.LengthOfTextToRetrieveMustBeAtLeastZero
            Return False
        End If

        value = Space(length)

        Try
            Dim result = ATMGetString(mClientHandle, startRow, startColumn, value, length)
            If result = ATM_SUCCESS Then Return True
            sErr = GetErrorMessage(result, My.Resources.GetText)
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function SendKeystroke(key As String, ByRef sErr As String) As Boolean

        Try
            Dim result = ATMSendKey(mClientHandle, key)

            Select Case result
                Case ATM_SUCCESS
                    Return True
                Case ATM_NOTCONNECTED
                    sErr = My.Resources.FailedToSendKeystrokeToAttachmateHostAsNoConnectionCanBeFound
                    Return False
                Case ATM_INVALIDPARAMETER
                    sErr = My.Resources.FailedToSendKeystrokeToAttachmateHostDueToAnInvalidParameter
                    Return False
                Case ATM_SYSTEMERROR
                    sErr = My.Resources.FailedToSendKeystrokeToAttachmateHost
                    Return False
                Case ATM_SESSIONOCCUPIED
                    sErr = My.Resources.FailedToSendKeystrokeToAttachmateHostDueToTheSessionBeingOccupied
                    Return False
                Case ATM_SESSIONLOCKED
                    sErr = My.Resources.FailedToSendKeystrokeToAttachmateHostDueToTheSessionBeingLocked
                    Return False
                Case Else
                    sErr = My.Resources.FailedToSendKeystrokeToAttachmateHostWithAnUnknownErrorCode
                    Return False
            End Select

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function SetText(startRow As Integer, startColumn As Integer, value As String, ByRef sErr As String) As Boolean
        'If either or both of the row and column parameters are zero, the string will be copied to the present host-cursor position.
        Try
            Dim result = ATMSendString(mClientHandle, startRow, startColumn, value)

            Select Case result
                Case ATM_SUCCESS
                    Return True
                Case ATM_NOTCONNECTED
                    sErr = My.Resources.FailedToSetTextInAttachmateHostAsNoConnectionCanBeFound
                    Return False
                Case ATM_INVALIDPARAMETER
                    sErr = My.Resources.FailedToSetTextInAttachmateHostDueToAnInvalidParameter
                    Return False
                Case ATM_SYSTEMERROR
                    sErr = My.Resources.FailedToSetTextInAttachmateHost
                    Return False
                Case ATM_INVALIDPOSITION
                    sErr = My.Resources.FailedToSetTextInAttachmateHostDueToAnInvalidPosition
                    Return False
                Case ATM_SESSIONLOCKED
                    sErr = My.Resources.FailedToSetTextInAttachmateHostDueToTheSessionBeingLocked
                    Return False
                Case Else
                    sErr = My.Resources.FailedToSetTextInAttachmateHostWithAnUnknownErrorCode
                    Return False
            End Select
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Overrides Function GetWindowTitle(ByRef value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function SetWindowTitle(value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean
        Try
            Dim pos As Integer = ATMGetCursorLocation(mClientHandle)
            If (pos < 0) Then
                sErr = GetErrorMessage(pos, My.Resources.GetCursorLocation)
                Return False
            End If
            row = ATMRowColumn(mClientHandle, pos, ATM_GETROW)
            col = ATMRowColumn(mClientHandle, pos, ATM_GETCOLUMN)
            Return True

        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SetCursorPosition(row As Integer, col As Integer, ByRef sErr As String) As Boolean

        Try
            Dim result = ATMSetCursorLocation(mClientHandle, row, col)

            Select Case result
                Case Is > 0
                    Return True
                Case ATM_INVALIDPARAMETER
                    sErr = My.Resources.FailedToSetCursorLocationOfAttachmateHostDueToAnInvalidParameter
                    Return False
                Case ATM_SYSTEMERROR
                    sErr = My.Resources.FailedToSetCursorLocationOfAttachmateHost
                    Return False
                Case ATM_INVALIDPOSITION
                    sErr = My.Resources.FailedToSetCursorLocationOfAttachmateHostDueToAnInvalidPosition
                    Return False
                Case Else
                    sErr = My.Resources.FailedToSetCursorLocationOfAttachmateHostWithAnUnknownErrorCode
                    Return False
            End Select
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Overrides Function RunEmulatorMacro(macroPath As String, ByRef sErr As String) As Boolean
        Try
            If String.IsNullOrEmpty(mSessionName) Then
                sErr = My.Resources.NoValidConnectionFoundNoSessionName
                Return False
            End If

            Dim result As Integer = ATMRunEmulatorMacro(mClientHandle, mSessionName, macroPath)
            Select Case result
                Case ATM_SUCCESS
                    Return True
                Case ATM_INVALIDPARAMETER
                    sErr = My.Resources.InvalidParameterCheckYourMacroName
                    Return False
                Case Else
                    sErr = String.Format(My.Resources.UnknownErrorCode0, result.ToString)
                    Return False
            End Select
        Catch ex As Exception
            sErr = String.Format(My.Resources.UnexpectedException0, ex.Message)
            Return False
        End Try
    End Function

    Public Overrides Function GetSessionSize(ByRef numRows As Integer, ByRef numColumns As Integer, ByRef sErr As String) As Boolean
        Try
            Dim result As Integer = ATMGetSessionSize(mClientHandle)

            If result < 0 Then
                sErr = GetErrorMessage(result, My.Resources.DetermineSessionSize)
                Return False
            End If

            numRows = ATMRowColumn(mClientHandle, result, ATM_GETROW)
            numColumns = ATMRowColumn(mClientHandle, result, ATM_GETCOLUMN)
            If numRows < 0 OrElse numColumns < 0 Then
                sErr = String.Format(My.Resources.FailedToDetermineSessionSideAtATMRowColumn01, numRows, numColumns)
                Return False
            End If
            Return True
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Overrides Function IsConnected() As Boolean

        If mClientHandle = 0 Then Return False
        Try
            Dim result = ATMGetConnectionStatus(mClientHandle, ConnectionStatusCalls.ATM_CONNECTION)

            Select Case CType(result, ConnectionStatus)
                Case ConnectionStatus.ATM_APPOWNED, ConnectionStatus.ATM_SSCP, ConnectionStatus.ATM_UNOWNED
                    Return True
                Case Else
                    result = ATMGetConnectionStatus(mClientHandle, ConnectionStatusCalls.ATM_ERROR)
                    Select Case CType(result, ConnectionStatusErrors)
                        Case ConnectionStatusErrors.ATM_NONE, ConnectionStatusErrors.None
                            'It's an asynchronous session, so none of the normal status codes
                            'apply - we must assume that it is connected in order to fix
                            'bug #2789
                            Return True
                        Case Else
                            Return False
                    End Select
            End Select

        Catch e As Exception
            Return False
        End Try
    End Function

    Public Overrides Function SelectArea(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer, type As SelectionType, ByRef sErr As String) As Boolean
        sErr = My.Resources.CanNotSelectTextOnAnAttachmateTerminal
        Return False
    End Function


    Public Overrides Property SleepTime() As Integer
        Get
            'do nothing - not relevant to attachmate terminals
        End Get
        Set(value As Integer)
            'do nothing - not relevant to attachmate terminals
        End Set
    End Property

    Public WaitTimeoutNumHalfSeconds As Integer = 60

    Public Overrides Property WaitTimeout() As Integer
        Get
            Return WaitTimeoutNumHalfSeconds * 500
        End Get
        Set(value As Integer)
            WaitTimeoutNumHalfSeconds = CInt(Math.Ceiling(value / 500))
        End Set
    End Property

    Private mVariantType As HostAccessValues
    Public Property VariantType() As HostAccessValues
        Get
            Return mVariantType
        End Get
        Set(value As HostAccessValues)
            mVariantType = value
        End Set
    End Property

    ''' <summary>
    ''' Get a friendly error message.
    ''' </summary>
    ''' <param name="code">An Attachmate return code (ATM_xxx)</param>
    ''' <param name="action">A short description of the thing we couldn't do, e.g.
    ''' "disconnect", "get text"</param>
    ''' <returns>The error message</returns>
    Private Function GetErrorMessage(code As Integer, action As String) As String
        Select Case code
            Case ATM_SUCCESS
                'Shouldn't have called it with this, it's not an error!
                Debug.Assert(False, "Bad error handling in clsAttachmateAutomation")
                Return My.Resources.GettingErrorMessageForSuccessfulAction
            Case ATM_NOTCONNECTED
                Return String.Format(My.Resources.AttachmateFailedTo0NotConnectedATM_NOTCONNECTED, action)
            Case ATM_INVALIDPARAMETER
                Return String.Format(My.Resources.AttachmateFailedTo0InvalidParameterATM_INVALIDPARAMETER, action)
            Case ATM_SYSTEMERROR
                Return String.Format(My.Resources.AttachmateFailedTo0SystemErrorATM_SYSTEMERROR, action)
            Case ATM_RESOURCEUNAVAILABLE
                Return String.Format(My.Resources.AttachmateFailedTo0UnavailableResourceATM_RESOURCEUNAVAILABLE, action)
            Case ATM_MEMORYUNAVAILABLE
                Return String.Format(My.Resources.AttachmateFailedTo0MemoryUnavailableATM_MEMORYUNAVAILABLE, action)
            Case ATM_WSCTRLFAILURE
                Return String.Format(My.Resources.AttachmateFailedTo0WorkstationControlFailureATM_WSCTRLFAILURE, action)
            Case ATM_NOMATCHINGPSID
                Return String.Format(My.Resources.AttachmateFailedTo0NoMatchingPresentationSpaceIdATM_NOMATCHINGPSID, action)
            Case ATM_NOEMULATORATTACHED
                Return String.Format(My.Resources.AttachmateFailedTo0NoEmulatorAttachedATM_NOEMULATORATTACHED, action)
            Case ATM_TIMEDOUT
                Return String.Format(My.Resources.AttachmateTimedOutWhenTryingTo0ATM_TIMEDOUT, action)
            Case Else
                Return String.Format(My.Resources.AttachmateFailedTo0WithErrorCode1, action, code)
        End Select
    End Function


    ''' <summary>
    ''' Register the client. Must be the first EAL function call within the application.
    ''' </summary>
    ''' <param name="iEmulationType">The required emulation type.</param>
    ''' <param name="sErr">In the event of failure, contains an error description on
    ''' return.</param>
    ''' <returns>True if sucessful, False Otherwise</returns>
    Public Function RegisterClient(iEmulationType As Integer, ByRef sErr As String) As Boolean

        Try
            SyncLock mRegistrationLock
                If mRegisteredClient Then Return True
                'Load the library manually, in case we have already freed it, because if
                'that's happened, pinvoke will not know to load it again.
                LoadLibraryA("ATMAPI32.DLL")
                Dim result = ATMRegisterClient(mClientHandle, iEmulationType)
                Select Case result
                    Case ATM_SUCCESS
                        mRegisteredClient = True
                        Return True
                    Case ATM_SYSTEMERROR
                        sErr = My.Resources.ATM_SYSTEMERROR
                    Case ATM_NOTFOUND
                        sErr = My.Resources.ATM_NOTFOUND
                    Case Else
                        sErr = String.Format(My.Resources.ErrorCode0, result)
                End Select
            End SyncLock
        Catch e As Exception
            sErr = String.Format(My.Resources.FailedToRegisterAttachmateClient0, e)
            Return False
        End Try

    End Function

    Public Function UnRegisterClient(ByRef sErr As String) As Boolean
        'The ATMUnregisterClient function disassociates the client's window handle
        'from the EAL. It also does an internal ATMResetSystem.
        Try
            SyncLock mRegistrationLock
                If Not mRegisteredClient Then Return True

                Dim result = ATMUnregisterClient(mClientHandle)
                If result = ATM_SUCCESS Then
                    'Free the ATMAPI32 library as many times as is necessary to get
                    'rid of it, bearing in mind that pinvoke will have created an
                    'extra reference after we loaded it manually.
                    Dim m As IntPtr
                    If GetModuleHandleExA(0, "ATMAPI32", m) Then
                        While FreeLibrary(m)
                        End While
                    End If
                    mRegisteredClient = False
                    Return True
                End If
                sErr = GetErrorMessage(result, My.Resources.UnregisterClient)
            End SyncLock
        Catch e As Exception
            sErr = e.ToString()
        End Try
        Return False

    End Function

    Public Function StartSession(sSession As String, sessionStyle As SessionStyle, ByRef sErr As String) As Boolean
        'The ATMStartSession function starts a terminal session, and defines how the session will appear.
        'ATMConnectSession must be called prior to using this function.

        Dim sStyle As String = ""

        Select Case sessionStyle
            Case SessionStyle.Normal
                sStyle = "N"
            Case SessionStyle.Maximized
                sStyle = "M"
            Case SessionStyle.Iconized
                sStyle = "I"
            Case SessionStyle.Hidden
                sStyle = "H"
            Case Else
                sErr = String.Format(My.Resources.UnknownSessionStyle0UnableToStartSession, sessionStyle)
                Return False
        End Select

        Try
            Dim result = ATMStartSession(mClientHandle, sSession, sStyle)

            If result = ATM_SUCCESS Then
                mSessionName = sSession
                Return True
            End If
            sErr = GetErrorMessage(result, My.Resources.StartSession)
        Catch e As Exception
            sErr = e.ToString
        End Try
        Return False

    End Function

    Public Function StopSession(ByRef sErr As String) As Boolean
        'ATMConnectSession must be called prior to using this function.

        Try
            Dim result = ATMStopSession(mClientHandle)

            If result = ATM_SUCCESS Then Return True
            sErr = GetErrorMessage(result, My.Resources.StopSession)
        Catch e As Exception
            sErr = e.ToString
        End Try
        Return False

    End Function

    Public Declare Function GetDesktopWindow Lib "User32" () As Integer

    Public Function CreateUniqueSessionHandle(ByRef sErr As String) As Boolean

        Try
            mClientHandle = CreateWindowEx(0, "Static", "Blue Prism Terminal Automation", WS_CHILD, 1, 1, 2, 2, New IntPtr(GetDesktopWindow()), IntPtr.Zero, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly.GetModules()(0)), IntPtr.Zero).ToInt32
            If mClientHandle = 0 Then
                sErr = My.Resources.FailedToCreateWindowForAttachmateSession
                Return False
            End If
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
        Return True

    End Function

    Public Sub DestroyUniqueSessionHandle()
        If mClientHandle <> 0 Then
            DestroyWindow(mClientHandle)
            mClientHandle = 0
        End If
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Dim sErr As String = Nothing
        If disposing Then
            UnRegisterClient(sErr)
            DestroyUniqueSessionHandle()
        End If
    End Sub
End Class
