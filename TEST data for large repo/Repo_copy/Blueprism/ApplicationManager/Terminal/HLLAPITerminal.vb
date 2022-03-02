Imports System.Text
Imports BluePrism.BPCoreLib
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Public Class HLLAPITerminal
    Inherits AbstractTerminal

    ''' <summary>
    ''' The hllapi dll wrapper
    ''' </summary>
    Private ReadOnly mHLLAPI As HLLAPIWrapper

    ''' <summary>
    ''' The type of session (either normal or enhanced)
    ''' </summary>
    Private ReadOnly mSessionType As SessionStartInfo.SessionTypes

    ''' <summary>
    ''' The text encoding to use when sending or recieving data to the terminal
    ''' </summary>
    Private ReadOnly mTerminalEncoding As Encoding

    ''' <summary>
    ''' Indicates whether the terminal is connected
    ''' </summary>
    Private mIsConnected As Boolean

    ''' <summary>
    ''' Holds the currently connected host
    ''' </summary>
    Private mConnectedHost As String

    Private Shared mKeyCodeMap As IDictionary(Of KeyCodeMappings, String)

    Shared Sub New()
        mKeyCodeMap = New Dictionary(Of KeyCodeMappings, String)
        For Each k As KeyCodeMappings In [Enum].GetValues(GetType(KeyCodeMappings))
            Dim attr = KeyCode.GetAttribute(Of KeyCodeAttribute)(k)
            If attr IsNot Nothing Then
                mKeyCodeMap.Add(k, attr.Code)
            End If
        Next
    End Sub

    Public Class KeyCodeAttribute : Inherits Attribute
        Public Property Code As String
        Public Sub New(code As String)
            Me.Code = code
        End Sub
    End Class
    ''' <summary>
    ''' Helper class to hold the columns rows and session longname
    ''' </summary>
    Private Class SessionInformation
        Public Property Columns As Integer
        Public Property Rows As Integer
        Public Property LongName As String
    End Class

    ''' <summary>
    ''' (E)HLLAPI function code definitions
    ''' </summary>
    Private Enum Fn
        ConnectPresentationSpace = 1
        DisconnectPresentationSpace = 2
        SendKey = 3
        QueryCursorLocation = 7
        CopyPresentationSpaceToString = 8
        QuerySessions = 10
        CopyStringToPresentationSpace = 15
        QuerySessionStatus = 22
        SetCursor = 40
    End Enum

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="dllName">The name of the HLLAPI dll to use.</param>
    ''' <param name="entryPoint">The name of the entryPoiny in the dll.</param>
    ''' <param name="convention">The calling convention</param>
    ''' <param name="encoding">The text encoding</param>
    ''' <param name="stype">The session type.</param>
    Public Sub New(dllName As String, entryPoint As String, stype As SessionStartInfo.SessionTypes,
                   convention As CallingConvention, encoding As Encoding)
        mSessionType = stype
        Select Case convention
            Case CallingConvention.Winapi
                mHLLAPI = New WinHLLAPIWrapper(dllName, entryPoint)
            Case CallingConvention.StdCall
                mHLLAPI = New StdHLLAPIWrapper(dllName, entryPoint)
            Case CallingConvention.Cdecl
                mHLLAPI = New CdeclHLLAPIWrapper(dllName, entryPoint)
        End Select
        mTerminalEncoding = encoding
        mSessionInfo = Nothing
        mIsConnected = False
    End Sub

    Private Sub HLLAPI(ByRef func As Integer, data As Byte(), ByRef len As Integer, ByRef rc As Integer)
        mHLLAPI.HLLAPI(func, data, len, rc)
    End Sub

    Private Sub HLLAPI(ByRef func As Integer, data As Byte(), ByRef rc As Integer)
        mHLLAPI.HLLAPI(func, data, data.Length, rc)
    End Sub

    Public Overrides Function ConnectToHostOrSession(sessionProfile As String, shortName As String, ByRef sErr As String) As Boolean
        Try
            mConnectedHost = shortName
            ConnectPresentationSpace(shortName)
            mSessionInfo = Nothing
            mIsConnected = True
            Return True
        Catch ex As Exception
            sErr = ex.Message()
            mIsConnected = False
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The Connect Presentation Space function establishes a connection between your
    ''' EHLLAPI application program and the host presentation space.
    ''' </summary>
    ''' <param name="idSession">1-character short name of the host presentation space</param>
    Private Sub ConnectPresentationSpace(idSession As String)
        Dim rc = 0
        'Session short name is always encoded in ASCII
        Dim b = Encoding.ASCII.GetBytes(idSession)
        HLLAPI(fn.ConnectPresentationSpace, b, rc)
        Select Case rc
            Case 0
                'The Connect Presentation Space function was successful; the host presentation space is unlocked and ready for input.
                Return
            Case 1
                Throw New HLLAPIException(rc, My.Resources.AnIncorrectHostPresentationSpaceIDWasSpecifiedTheSpecifiedSessionEitherDoesNotE)
            Case 4
                Throw New HLLAPIException(rc, My.Resources.SuccessfulConnectionWasAchievedButTheHostPresentationSpaceIsBusy)
            Case 5
                Throw New HLLAPIException(rc, My.Resources.SuccessfulConnectionWasAchievedButTheHostPresentationSpaceIsLockedInputInhibited)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case 11
                Throw New HLLAPIException(rc, My.Resources.ThisResourceIsUnavailableTheHostPresentationSpaceIsAlreadyBeingUsedByAnotherSys)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Sub

    ''' <summary>
    ''' Returns the number of sessions
    ''' </summary>
    Private Function GetNumSessions() As Integer
        Dim rc = 0
        Dim len = 0
        HLLAPI(Fn.QuerySessions, {}, len, rc)
        '"If an application program receives RC=2 or RC=0,
        ' the number of the active sessions is returned in the length field. The application program can recognize the minimum string length by this number."
        Select Case rc
            Case 0, 2
                Return len
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Function

    ''' <summary>
    ''' Returns a list of session short names for existing sessions
    ''' </summary>
    Private Function QuerySessions() As List(Of String)

        Dim sessions As New List(Of String)

        Dim n = GetNumSessions()
        If n <= 0 Then Return sessions

        Dim size As Integer
        Select Case mSessionType
            Case SessionStartInfo.SessionTypes.Enhanced
                size = 16
            Case Else
                size = 12
        End Select
        Dim len = size * n
        Dim sessionData(len - 1) As Byte
        Dim rc = 0
        HLLAPI(Fn.QuerySessions, sessionData, len, rc)
        Select Case rc
            Case 0
                For i = 0 To sessionData.Length - 1 Step size
                    Dim shortName = Encoding.ASCII.GetString(sessionData, i, 1)
                    sessions.Add(shortName)
                Next
                Return sessions
            Case 2
                Throw New HLLAPIException(rc, My.Resources.AnIncorrectStringLengthWasMade)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Function

    ''' <summary>
    ''' Returns true if the given session is launched
    ''' </summary>
    ''' <param name="sessionShortName">The shortname of the session</param>
    Private Function AlreadyLaunched(sessionShortName As String) As Boolean
        If QuerySessions.Contains(sessionShortName) Then
            Return True
        End If
        Return False
    End Function

    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean
        Try
            DisconnectPresentationSpace(mConnectedHost)
            mSessionInfo = Nothing
            mIsConnected = False
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
        Try

            If Not AlreadyLaunched(sessionShortName) Then
                sErr = String.Format(My.Resources.Session0DoesnTNotExistYouMustLaunchFirst, sessionShortName)
                Return False
            End If

            ConnectPresentationSpace(sessionShortName)
            mConnectedHost = sessionShortName
            mSessionInfo = Nothing
            mIsConnected = True
            Return True

        Catch ex As Exception
            sErr = ex.Message()
            mIsConnected = False
            Return False
        End Try
    End Function

    Public Overrides Function Detach(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Terminate(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' The Disconnect Presentation Space function drops the connection between your
    ''' EHLLAPI application program and the host presentation space. Also, if a host
    ''' presentation space is reserved using the Reserve (11) function, it is released
    ''' upon execution of the Disconnect Presentation Space function.
    ''' </summary>
    ''' <param name="idSession">Not applicable</param>
    Private Sub DisconnectPresentationSpace(idSession As String)
        Dim rc = 0
        'Session short name is always encoded in ASCII
        Dim b = Encoding.ASCII.GetBytes(idSession)
        HLLAPI(Fn.DisconnectPresentationSpace, b, rc)
        Select Case rc
            Case 0
                'The Disconnect Presentation Space function was successful.
                Return
            Case 1
                Throw New HLLAPIException(rc, My.Resources.YourProgramWasNotCurrentlyConnectedToTheHostPresentationSpace)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Sub


    Public Overrides Function SetCursorPosition(row As Integer, col As Integer, ByRef sErr As String) As Boolean

        Try
            Dim startPos As Integer = GetStartPosition(row, col)
            SetCursor(startPos)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' The Set Cursor function is used to set the position of the cursor within the 
    ''' host presentation space. Before using the Set Cursor function, a workstation
    ''' application must be connected to the host presentation space.
    ''' </summary>
    ''' <param name="iNewPos">Desired cursor position in the connected host presentation
    ''' space</param>
    Private Sub SetCursor(iNewPos As Integer)
        Dim rc = iNewPos
        HLLAPI(Fn.SetCursor, {}, rc)
        Select Case rc
            Case 0
                'Cursor was successfully located at the specified position.
                Return
            Case 1
                Throw New HLLAPIException(rc, My.Resources.YourProgramIsNotConnectedToAHostSession)
            Case 4
                Throw New HLLAPIException(rc, My.Resources.TheSessionIsBusy)
            Case 7
                Throw New HLLAPIException(rc, My.Resources.ACursorLocationLessThan1OrGreaterThanTheSizeOfTheConnectedHostPresentationSpace)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorOccurred)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Sub

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean

        Try
            Dim pos = QueryCursorLocation()

            Dim sessionColumns = SessionInfo.Columns

            row = (pos - 1) \ sessionColumns + 1
            col = pos - ((row - 1) * sessionColumns) + 1

            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' The Query Cursor Location function indicates the position of the cursor in the
    ''' host-connected presentation space by returning the cursor position.
    ''' </summary>
    ''' <return>This function returns the position as a length</return>
    Private Function QueryCursorLocation() As Integer
        Dim rc = 0
        Dim pos = 0
        HLLAPI(Fn.QueryCursorLocation, {}, pos, rc)
        Select Case rc
            Case 0
                'The Query Cursor Location function was successful.
                Return pos
            Case 1
                Throw New HLLAPIException(rc, My.Resources.YourProgramIsNotCurrentlyConnectedToAHostSession)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Function

    Public Overrides Function GetText(row As Integer, col As Integer, length As Integer, ByRef text As String, ByRef sErr As String) As Boolean

        Try
            Dim startPos As Integer = GetStartPosition(row, col)

            text = CopyPresentationSpaceToString(length, startPos)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' The Copy String to Presentation Space function copies an ASCII data string
    ''' directly into the host presentation space at the location specified by the PS
    ''' position calling parameter.
    ''' </summary>
    ''' <param name="str">String of ASCII data to be copied into the host
    ''' presentation space.</param>
    ''' <param name="pos">  Position in the host presentation space to begin the copy,
    ''' a value between 1 and the configured size of your host presentation space.</param>
    Private Sub CopyStringToPresentationSpace(str As String, pos As Integer)
        Dim rc = pos

        Dim b = mTerminalEncoding.GetBytes(str)
        HLLAPI(Fn.CopyStringToPresentationSpace, b, rc)

        Select Case rc
            Case 0
                'The Copy String to Presentation Space function was successful.
                Return
            Case 1
                Throw New HLLAPIException(rc, My.Resources.YourProgramIsNotConnectedToAHostSession)
            Case 2
                Throw New HLLAPIException(rc, My.Resources.ParameterErrorOrZeroLengthForCopy)
            Case 5
                Throw New HLLAPIException(rc, My.Resources.TheTargetPresentationSpaceIsProtectedOrInhibitedOrIncorrectDataWasSentToTheTarg)
            Case 6
                Throw New HLLAPIException(rc, My.Resources.TheCopyWasCompletedButTheDataWasTruncated)
            Case 7
                Throw New HLLAPIException(rc, My.Resources.TheHostPresentationSpacePositionIsNotValid)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Sub

    ''' <summary>
    ''' The Copy Presentation Space to String function is used to copy all or part of
    ''' the host-connected presentation space into a data string that you define in
    ''' your EHLLAPI application program. The input PS position is the offset into
    ''' the host presentation space. This offset is based on a layout in which the
    ''' upper-left corner (row 1/column 1) is location 1 and the bottom-right corner
    ''' is 3564, which is the maximum screen size for the host presentation space.
    ''' The value of PS Position + (Length - 1) cannot exceed the configured size of
    ''' your host presentation space. The Copy Presentation Space to String function
    ''' translates the characters in the host source presentation space into ASCII.
    ''' Attribute bytes and other characters not represented in ASCII normally are
    ''' translated into blanks. If you do not want the attribute bytes translated into
    ''' blanks, you can override this translation with the ATTRB option under the
    ''' Set Session Parameters (9) function.
    ''' </summary>
    ''' <param name="maxLen">Length of the target data string.</param>
    ''' <param name="pos">Position within the host presentation space of the first
    ''' byte in your target data string.</param>
    ''' <returns>Preallocated target string the size of your host
    ''' presentation space. When the Set Session Parameters (9) function with the EAB
    ''' option is issued, the length of the data string must be at least twice the
    ''' length of the presentation space. DBCS Only: When the EAD option is specified,
    ''' the length of the data string must be at least three times the length of the
    ''' presentation space. When both the EAB and EAD options are specified, the
    ''' length of the data string must be at least four times the length of the
    ''' presentation space.</returns>
    Private Function CopyPresentationSpaceToString(maxLen As Integer, pos As Integer) As String

        Dim rc = pos
        Dim b(maxLen - 1) As Byte
        mHLLAPI.HLLAPI(Fn.CopyPresentationSpaceToString, b, maxLen, rc)

        Select Case rc
            Case 0, 4, 5
                '0  The host presentation space contents were copied to the application program. The target presentation space was active, and the keyboard was unlocked.
                '4  The host presentation space contents were copied. The host presentation space was waiting for host response.
                '5  The host presentation space was copied. The keyboard was locked.
                Return mTerminalEncoding.GetString(b)
            Case 1
                Throw New HLLAPIException(rc, My.Resources.YourProgramIsNotConnectedToAHostSession)
            Case 2
                Throw New HLLAPIException(rc, My.Resources.AnErrorWasMadeInSpecifyingStringLengthOrTheSumOfLength1PSPositionIsGreaterThanT)
            Case 7
                Throw New HLLAPIException(rc, My.Resources.TheHostPresentationSpacePositionIsNotValid)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select

    End Function

    Public Overrides Function GetSessionSize(ByRef numRows As Integer, ByRef numCols As Integer, ByRef sErr As String) As Boolean

        Try
            numRows = SessionInfo.Rows
            numCols = SessionInfo.Columns
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' The Query Session Status function is used to obtain session-specific information.
    ''' </summary>
    ''' <param name="idSession">An 18/20-byte string consisting of a 1-byte short name of
    ''' the target presentation space</param>
    ''' <param name="longName">17 bytes for returned data.</param>
    ''' <param name="rows">Number of rows in the host presentation space</param>
    ''' <param name="columns">Number of columns in the host presentation space</param>
    Private Sub QuerySessionStatus(idSession As String, ByRef longName As String, ByRef rows As Integer, ByRef columns As Integer)

        Dim rc = 0
        Dim len As Integer
        Select Case mSessionType
            Case SessionStartInfo.SessionTypes.Enhanced
                len = 20
            Case SessionStartInfo.SessionTypes.Normal
                len = 18
            Case SessionStartInfo.SessionTypes.NotImplemented
                longName = idSession
                rows = 24
                columns = 80
                Return
            Case Else
                Throw New InvalidOperationException(My.Resources.InvalidSessionStatusType)
        End Select
        Dim b() As Byte = Encoding.ASCII.GetBytes(idSession.PadRight(len, Chr(0)))
        HLLAPI(Fn.QuerySessionStatus, b, len, rc)

        'Return bytes are as follows
        '
        'Standard   Enhanced
        '0          0           A 1-character presentation space short name (PSID)
        '           1-3         Reserved
        '1-8        4-11        Session long name (same as profile name; or, if profile not set, same as short name)
        '9          12          Session Type
        '                           D  3270 display
        '                           E  3270 printer
        '                           F  5250 display
        '                           G  5250 printer
        '                           H  ASCII VT
        '
        '10         13          Session characteristics expressed by a binary number including the following session-characteristics bits
        '                       Bit 0
        '                           EAB  0: Session has the basic attribute.  1: Session has the extended attribute
        '                       Bit 1
        '                           PSS  0: Session does not support the programmed symbols  1: Session supports the programmed symbols
        '                       Bits 2-7
        '                           Reserved
        '11-12      14-15       Number of rows in the host presentation space, expressed as a binary number
        '13-14      16-17       Number of columns in the host presentation space, expressed as a binary number
        '15-16      18-19       Host code page expressed as a binary number
        '17         Reserved

        Select Case rc
            Case 0
                'The Query Session Status function was successful.
                If mSessionType = SessionStartInfo.SessionTypes.Enhanced Then
                    longName = Encoding.ASCII.GetString(b, 4, 7)
                    rows = BitConverter.ToInt16(b, 14)
                    columns = BitConverter.ToInt16(b, 16)
                Else
                    longName = Encoding.ASCII.GetString(b, 1, 7)
                    rows = BitConverter.ToInt16(b, 11)
                    columns = BitConverter.ToInt16(b, 13)
                End If
                Return
            Case 1
                Throw New HLLAPIException(rc, My.Resources.AnIncorrectHostPresentationSpaceWasSpecified)
            Case 2
                Throw New HLLAPIException(rc, My.Resources.AnIncorrectStringLengthWasMade)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select

    End Sub


    Public Overrides Function SetText(row As Integer, col As Integer, text As String, ByRef sErr As String) As Boolean

        Try
            Dim startPos = GetStartPosition(row, col)

            CopyStringToPresentationSpace(text, startPos)

            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    <Obsolete>
    Public Overrides Function SendKeyStroke(keyStroke As String, ByRef sErr As String) As Boolean
        Try
            SendKey(keyStroke)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Try
            Dim sequence = Terminal.ParseKeySequence(keys)

            For Each k In sequence
                If k.IsCharacter Then
                    SendKey(k.Character)
                Else
                    Dim c = mKeyCodeMap(k.Code)
                    SendKey(c)
                End If
            Next

            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The Send Key function is used to send either a keystroke or a string of keystrokes
    ''' to the host presentation space. You define the string of keystrokes to be sent with
    ''' the calling data string parameter. The keystrokes appear to the target session as
    ''' though they were entered by the terminal operator. You can also send all attention
    ''' identifier (AID) keys such as Enter and so on. All host fields that are input
    ''' protected or are numeric only must be treated accordingly.
    ''' </summary>
    ''' <param name="keyString">A string of keystrokes, maximum 255. Uppercase and lowercase
    ''' ASCII characters are represented literally. Function keys and shifted function key
    ''' are represented by mnemonics. See Keyboard Mnemonics.</param>
    Private Sub SendKey(keyString As String)
        Dim rc = 0
        'KeyStrokes and menomics are ASCII encoded.
        Dim b = Encoding.ASCII.GetBytes(keyString)
        HLLAPI(Fn.SendKey, b, rc)
        Select Case rc
            Case 0
                'The keystrokes were sent; status is normal.
                Return
            Case 1
                Throw New HLLAPIException(rc, My.Resources.YourProgramIsNotConnectedToAHostSession)
            Case 2
                Throw New HLLAPIException(rc, My.Resources.AnIncorrectParameterWasPassedToEHLLAPI)
            Case 4
                Throw New HLLAPIException(rc, My.Resources.TheHostSessionWasBusyAllOfTheKeystrokesCouldNotBeSent)
            Case 5
                Throw New HLLAPIException(rc, My.Resources.InputToTheTargetSessionWasInhibitedOrRejectedAllOfTheKeystrokesCouldNotBeSent)
            Case 9
                Throw New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case Else
                Throw GeneralReturnCodeError(rc)
        End Select
    End Sub


    Private mSessionInfo As SessionInformation

    Private ReadOnly Property SessionInfo As SessionInformation
        Get
            If mSessionInfo Is Nothing Then
                Dim longName As String = Nothing
                Dim rows As Integer
                Dim cols As Integer
                QuerySessionStatus(mConnectedHost, longName, rows, cols)
                mSessionInfo = New SessionInformation With {.LongName = longName, .Rows = rows, .Columns = cols}
            End If
            Return mSessionInfo
        End Get
    End Property

    ''' <summary>
    ''' Determine required start position
    ''' </summary>
    ''' <param name="row"></param>
    ''' <param name="col"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetStartPosition(row As Integer, col As Integer) As Integer
        Return ((row - 1) * SessionInfo.Columns) + col
    End Function

    ''' <summary>
    ''' Get a general exception based on the return code.
    ''' </summary>
    ''' <param name="rc">The return code for which to get an exception</param>
    ''' <returns>A HLLAPIException for the given return code</returns>
    Private Function GeneralReturnCodeError(rc As Integer) As Exception
        Select Case rc
            Case 0
                'The function successfully executed, or no update since the last call was issued.
                Return New HLLAPIException(rc, My.Resources.SuccessCodeTreatedAsError) 'This should never happen 
            Case 1
                Return New HLLAPIException(rc, My.Resources.AnIncorrectHostPresentationSpaceIDWasSpecifiedTheSpecifiedSessionEitherWasNotCo)
            Case 2
                Return New HLLAPIException(rc, My.Resources.AParameterErrorWasEncounteredOrAnIncorrectFunctionNumberWasSpecifiedReferToTheI)
            Case 4
                Return New HLLAPIException(rc, My.Resources.TheExecutionOfTheFunctionWasInhibitedBecauseTheTargetPresentationSpaceWasBusyIn)
            Case 5
                Return New HLLAPIException(rc, My.Resources.TheExecutionOfTheFunctionWasInhibitedForSomeReasonOtherThanThoseStatedInReturnC)
            Case 6
                Return New HLLAPIException(rc, My.Resources.ADataErrorWasEncounteredDueToSpecificationOfAnIncorrectParameterForExampleALeng)
            Case 7
                Return New HLLAPIException(rc, My.Resources.TheSpecifiedPresentationSpacePositionWasNotValid)
            Case 8
                Return New HLLAPIException(rc, My.Resources.AFunctionalProcedureErrorWasEncounteredForExampleUseOfConflictingFunctionsOrMis)
            Case 9
                Return New HLLAPIException(rc, My.Resources.ASystemErrorWasEncountered)
            Case 10
                Return New HLLAPIException(rc, My.Resources.ThisFunctionIsNotAvailableForEHLLAPI)
            Case 11
                Return New HLLAPIException(rc, My.Resources.ThisResourceIsNotAvailable)
            Case 12
                Return New HLLAPIException(rc, My.Resources.ThisSessionStopped)
            Case 24
                Return New HLLAPIException(rc, My.Resources.TheStringWasNotFoundOrThePresentationSpaceIsUnformatted)
            Case 25
                Return New HLLAPIException(rc, My.Resources.KeystrokesWereNotAvailableOnInputQueue)
            Case 26
                Return New HLLAPIException(rc, My.Resources.AHostEventOccurredSeeQueryHostUpdate24ForDetails)
            Case 27
                Return New HLLAPIException(rc, My.Resources.FileTransferWasEndedByACtrlBreakCommand)
            Case 28
                Return New HLLAPIException(rc, My.Resources.FieldLengthWas0)
            Case 31
                Return New HLLAPIException(rc, My.Resources.KeystrokeQueueOverflowKeystrokesWereLost)
            Case 32
                Return New HLLAPIException(rc, My.Resources.AnApplicationHasAlreadyConnectedToThisSessionForCommunications)
            Case 33
                Return New HLLAPIException(rc, My.Resources.Reserved)
            Case 34
                Return New HLLAPIException(rc, My.Resources.TheMessageSentToTheHostWasCanceled)
            Case 35
                Return New HLLAPIException(rc, My.Resources.TheMessageSentFromTheHostWasCanceled)
            Case 36
                Return New HLLAPIException(rc, My.Resources.ContactWithTheHostWasLost)
            Case 37
                Return New HLLAPIException(rc, My.Resources.InboundCommunicationHasBeenDisabled)
            Case 38
                Return New HLLAPIException(rc, My.Resources.TheRequestedFunctionHasNotCompletedItsExecution)
            Case 39
                Return New HLLAPIException(rc, My.Resources.AnotherDDMSessionIsAlreadyConnected)
            Case 40
                Return New HLLAPIException(rc, My.Resources.TheDisconnectionAttemptWasSuccessfulButThereWereAsynchronousRequestsThatHadNotB)
            Case 41
                Return New HLLAPIException(rc, My.Resources.TheBufferYouRequestedIsBeingUsedByAnotherApplication)
            Case 42
                Return New HLLAPIException(rc, My.Resources.ThereAreNoOutstandingRequestsThatMatch)
            Case 43
                Return New HLLAPIException(rc, My.Resources.TheAPIWasAlreadyLockedByAnotherEHLLAPIApplicationOnLOCKOrAPINotLockedOnUNLOCK)
            Case Else
                Return New HLLAPIException(rc, My.Resources.UnknownError)
        End Select
    End Function

    Public Overrides Function GetWindowTitle(ByRef value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function SetWindowTitle(value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function IsConnected() As Boolean
        Return mIsConnected
    End Function

    Public Overrides Function RunEmulatorMacro(macroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.NotImplementedForEHLLAPIBasedTerminals
        Return False
    End Function

    Public Overrides Function SelectArea(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer, type As SelectionType, ByRef sErr As String) As Boolean
        sErr = My.Resources.SelectionsAreNotPossibleOnAEHLLAPIBasedTerminals
        Return False
    End Function

    Public Overrides Property SleepTime As Integer

    Public Overrides Property WaitTimeout As Integer

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If mHLLAPI IsNot Nothing Then mHLLAPI.Dispose()
        End If
    End Sub

    <Serializable>
    Private Class HLLAPIException : Inherits Exception
        Private ReadOnly mReturnCode As Integer
        Public Sub New(rc As Integer, msg As String)
            MyBase.New(msg)
            mReturnCode = rc
        End Sub

        Public Overrides ReadOnly Property Message() As String
            Get
                Return String.Format(My.Resources.x0ReturnCode1, MyBase.Message, mReturnCode)
            End Get
        End Property
    End Class
End Class
