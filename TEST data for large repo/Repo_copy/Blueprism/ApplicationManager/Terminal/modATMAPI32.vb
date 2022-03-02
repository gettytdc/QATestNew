
Imports System.Runtime.InteropServices

Public Module modATMAPI32

    ' Standard EAL Functions
    Declare Function ATMConnectSession Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String) As Integer
    Declare Function ATMDisconnectSession Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMGetConnectionStatus Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal StatusType As ATMTerminal.ConnectionStatusCalls) As Integer
    Declare Function ATMGetCursorLocation Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMGetError Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal errstring As String, ByVal Length As Integer) As Integer
    Declare Function ATMGetFieldInfo Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal InfoType As Integer) As Integer
    Declare Function ATMGetFieldLength Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal field As Integer) As Integer
    Declare Function ATMGetFieldPosition Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal field As Integer) As Integer
    Declare Function ATMGetParameter Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Index As Integer) As Integer
    Declare Function ATMGetSessions Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal SessionList As String, ByVal Length As Integer, ByVal state As Integer) As Integer
    Declare Function ATMGetSessionSize Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMGetSessionStatus Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal status As Integer) As Integer
    Declare Function ATMGetString Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal GetString As String, ByVal Length As Integer) As Integer
    Declare Function ATMGetStringFromField Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal GetString As String, ByVal Length As Integer) As Integer
    Declare Function ATMListSessions Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal Length As Integer, ByVal title As String, ByVal status As Integer) As Integer
    Declare Function ATMLockKeyboard Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMPause Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Pause As Integer) As Integer
    Declare Function ATMReceiveFile Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal receive As String, ByVal Length As Integer) As Integer
    Declare Function ATMResetSystem Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMRowColumn Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Position As Integer, ByVal Toggle As Integer) As Integer
    Declare Function ATMSearchField Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal SearchString As String) As Integer
    Declare Function ATMSearchSession Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal SearchString As String, ByVal SearchOption As Integer) As Integer
    Declare Function ATMSendAndWait Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nRow As Integer, ByVal nColumn As Integer, ByVal SendString As String, ByVal WaitString As String, ByVal nTimeout As Integer) As Integer
    Declare Function ATMSendFile Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Send As String, ByVal Length As Integer) As Integer
    Declare Function ATMSendKey Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Key As String) As Integer
    Declare Function ATMSendString Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal SendString As String) As Integer
    Declare Function ATMSendStringToField Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer, ByVal SendString As String) As Integer
    Declare Function ATMSetCursorLocation Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Row As Integer, ByVal Column As Integer) As Integer
    Declare Function ATMSetParameter Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Index As Integer, ByVal Setting As Integer, ByVal Escape As String) As Integer
    Declare Function ATMShowLastError Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMUnlockKeyboard Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer

    Declare Function ATMWait Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMWaitForCursor Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nRow As Integer, ByVal nColumn As Integer, ByVal nTimeout As Integer) As Integer
    Declare Function ATMWaitForString Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nRow As Integer, ByVal nColumn As Integer, ByVal SearchString As String, ByVal nTimeout As Integer) As Integer
    Declare Function ATMWaitHostQuiet Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal SettleTime As Integer, ByVal Timeout As Integer) As Integer

    Declare Function ATMWaitForCursorMove Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTimeout As Integer) As Integer
    Declare Function ATMWaitForHostConnect Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Timeout As Integer) As Integer
    Declare Function ATMWaitForHostDisconnect Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Timeout As Integer) As Integer
    Declare Function ATMWaitForKey Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Key As String, ByVal nTimeout As Integer) As Integer

    Declare Function ATMGetKeystroke Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal GetStroke As String, ByVal Bufferlength As Integer) As Integer
    Declare Function ATMStartKeystrokeIntercept Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal Intercept As Integer) As Integer
    Declare Function ATMStopKeystrokeIntercept Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String) As Integer

    Declare Function ATMRegisterClient Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nType As Integer) As Integer
    Declare Function ATMUnregisterClient Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer


    ' Emulator Specific - WorkStation Control Functions (WsCtrl)
    Declare Function ATMAllowUpdates Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMBlockUpdates Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer

    Declare Function ATMGetEmulatorPath Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Path As String, ByVal Length As Integer) As Integer
    Declare Function ATMGetEmulatorVersion Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Buffer As String, ByVal Length As Integer) As Integer
    Declare Function ATMGetATMAPIVersion Lib "ATMAPI32.DLL" (ByVal Buffer As String, ByVal Length As Integer) As Integer

    Declare Function ATMGetSessionHandle Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String) As Integer

    Declare Function ATMCloseConfiguration Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMGetConfiguration Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal config As String, ByVal Length As Integer) As Integer
    Declare Function ATMOpenConfiguration Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal config As String) As Integer

    Declare Function ATMGetLayoutName Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal layout As String, ByVal Length As Integer) As Integer
    Declare Function ATMOpenLayout Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal layout As String) As Integer

    Declare Function ATMRunEmulatorMacro Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal Macro As String) As Integer

    Declare Function ATMSessionOff Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMSessionOn Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer

    Declare Function ATMStartSession Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal WindowMode As String) As Integer
    Declare Function ATMStopSession Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer


    ' Functions specific to EXTRA!  (obsoleted)
    Declare Function ATMGetEXTRAPath Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Path As String, ByVal Length As Integer) As Integer
    Declare Function ATMGetEXTRAVersion Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Buffer As String, ByVal Length As Integer) As Integer
    Declare Function ATMRunEXTRAMacro Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal Macro As String) As Integer


    ' Functions specific to the Async (KEA!) platform
    Declare Function ATMAddWait Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer) As Integer
    Declare Function ATMAddWaitForCursor Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer, ByVal nRow As Integer, ByVal nColumn As Integer) As Integer
    Declare Function ATMAddWaitForCursorMove Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer) As Integer
    Declare Function ATMAddWaitForHostConnect Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer) As Integer
    Declare Function ATMAddWaitForHostDisconnect Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer) As Integer
    Declare Function ATMAddWaitForKey Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer, ByVal lpKey As String) As Integer
    Declare Function ATMAddWaitForString Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer, ByVal nRow As Integer, ByVal nColumn As Integer, ByVal lpString As String) As Integer
    Declare Function ATMAddWaitForStringNotAt Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer, ByVal nRow As Integer, ByVal nColumn As Integer, ByVal lpString As String) As Integer
    Declare Function ATMAddWaitHostQuiet Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer, ByVal nIdleMilliseconds As Integer) As Integer

    Declare Function ATMClearEventTable Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer) As Integer
    Declare Function ATMDeleteEvent Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nEventID As Integer) As Integer
    Declare Function ATMExecute Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal MacroCode As String, ByVal nTimeout As Integer) As Integer
    Declare Function ATMHoldHost Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer
    Declare Function ATMResumeHost Lib "ATMAPI32.DLL" (ByVal hwnd As Integer) As Integer

    Declare Function ATMWaitForEvent Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nTable As Integer, ByVal nTimeout As Integer) As Integer


    '  Undocumented (unsupported) Functions
    Declare Function ATMPasswordDlg Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Password As String, ByVal Length As Integer) As Integer
    Declare Function ATMFileTransferDlg Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal A1 As String, ByVal A2 As String, ByVal A3 As String, ByVal A4 As String, ByVal Num1 As Integer, ByVal A5 As String, ByVal Num2 As Integer) As Integer
    Declare Function ATMWaitForStringNotAt Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal nRow As Integer, ByVal nColumn As Integer, ByVal SearchString As String, ByVal nTimeout As Integer) As Integer
    Declare Function ATMRunEXTRAMacroAsync Lib "ATMAPI32.DLL" (ByVal hwnd As Integer, ByVal Session As String, ByVal Macro As String) As Integer


    ' Global Constants and Global Variables
    Public Timeout As Integer   ' time out period (half seconds)
    Public Settle As Integer ' settle time (mili seconds)

    ' Global Variables for connection
    Public Handle As Integer
    Public SessionID As String

    ' Global Constants for session status functions
    Public Const ATM_ISCONFIGURED As Integer = 1
    Public Const ATM_ISOPENED As Integer = 2
    Public Const ATM_ISPOWERED As Integer = 3

    ' Global Constants for ATMRowColumn
    Public Const ATM_GETROW As Integer = 0
    Public Const ATM_GETCOLUMN As Integer = 1

    ' Global Constants for field functions
    Public Const ATM_THISFIELD As Integer = 0
    Public Const ATM_NEXTFIELD As Integer = 1
    Public Const ATM_PREVIOUSFIELD As Integer = 2
    Public Const ATM_NEXTPROTECTEDFIELD As Integer = 3
    Public Const ATM_NEXTUNPROTECTEDFIELD As Integer = 4
    Public Const ATM_PREVIOUSPROTECTEDFIELD As Integer = 5
    Public Const ATM_PREVIOUSUNPROTECTEDFIELD As Integer = 6

    ' Global Constants for ATMSeachSession option
    Public Const ATM_SEARCHALL As Integer = 1
    Public Const ATM_SEARCHFROM As Integer = 2
    Public Const ATM_SEARCHAT As Integer = 3
    Public Const ATM_SEARCHBACK As Integer = 4

    ' Global Constants for Get/SetParameters
    Public Const ATM_ATTRIB As Integer = 1
    Public Const ATM_AUTORESET As Integer = 2
    Public Const ATM_CONNECTTYPE As Integer = 3
    Public Const ATM_EAB As Integer = 4
    Public Const ATM_PAUSE As Integer = 5
    Public Const ATM_SEARCHORG As Integer = 6
    Public Const ATM_SEARCHDIRECTION As Integer = 7
    Public Const ATM_TIMEOUT As Integer = 8
    Public Const ATM_TRACE As Integer = 9
    Public Const ATM_WAIT As Integer = 10
    Public Const ATM_XLATE As Integer = 11
    Public Const ATM_ESCAPE As Integer = 12

    ' New Global Constants for ATMGetConnectionStatus
    Public Const ATM_XSTATUS As Integer = 1
    Public Const ATM_CONNECTION As Integer = 2
    Public Const ATM_ERROR As Integer = 3
    Public Const ATM_CASEMODE As Integer = 4

    ' New Global Constants for ATMGetFieldInfo
    Public Const ATM_ISFIELDPROTECTED As Integer = 1
    Public Const ATM_ISFIELDNUMERIC As Integer = 2
    Public Const ATM_ISFIELDSELECTORPENDETECTABLE As Integer = 3
    Public Const ATM_ISFIELDBOLD As Integer = 4
    Public Const ATM_ISFIELDHIDDEN As Integer = 5
    Public Const ATM_ISFIELDMODIFIED As Integer = 6

    ' New Global Constants for ATMGetSessions and ATMListSessions
    Public Const ATM_GETCONFIGURED As Integer = 1
    Public Const ATM_GETOPENED As Integer = 2
    Public Const ATM_GETPOWERED As Integer = 3
    Public Const ATM_GETEMULATED As Integer = 11
    Public Const ATM_GETEMULATEDPOWERED As Integer = 12

    ' New Global Constants for ATMGetSessions
    Public Const ATM_GETCONFIGUREDCOUNT As Integer = 4
    Public Const ATM_GETOPENEDCOUNT As Integer = 5
    Public Const ATM_GETPOWEREDCOUNT As Integer = 6
    Public Const ATM_GETEMULATEDCOUNT As Integer = 14
    Public Const ATM_GETEMULATEDPOWEREDCOUNT As Integer = 15

    ' Settings for GetSessionStatus
    Public Const ATM_ISEMULATED As Integer = 14
    Public Const ATM_ISCONNECTED As Integer = 15
    Public Const ATM_ISFILETRANSFER As Integer = 16

    ' Return codes
    Public Const ATM_SUCCESS As Integer = 1
    Public Const ATM_NOTFOUND As Integer = 0
    Public Const ATM_NOTCONNECTED As Integer = -1
    Public Const ATM_INVALIDPARAMETER As Integer = -2
    Public Const ATM_TIMEDOUT As Integer = -4
    Public Const ATM_SESSIONOCCUPIED As Integer = -4
    Public Const ATM_SESSIONLOCKED As Integer = -5
    Public Const ATM_PROTECTED As Integer = -5
    Public Const ATM_FIELDSIZEMISMATCH As Integer = -6
    Public Const ATM_DATATRUNCATED As Integer = -6
    Public Const ATM_INVALIDPOSITION As Integer = -7
    Public Const ATM_NOPRIORSTARTKEYSTROKE As Integer = -8
    Public Const ATM_NOPRIORSTARTHOSTNOTIFY As Integer = -8
    Public Const ATM_SYSTEMERROR As Integer = -9
    Public Const ATM_FUNCTIONNOTAVAILABLE As Integer = -10
    Public Const ATM_RESOURCEUNAVAILABLE As Integer = -11
    Public Const ATM_SEARCHSTRINGNOTFOUND As Integer = -24
    Public Const ATM_UNFORMATTEDHOSTPS As Integer = -24
    Public Const ATM_NOSUCHFIELD As Integer = -24
    Public Const ATM_NOHOSTSESSIONUPDATE As Integer = -24
    Public Const ATM_KEYSTROKESNOTAVAILABLE As Integer = -25
    Public Const ATM_HOSTSESSIONUPDATE As Integer = -26
    Public Const ATM_KEYSTROKEQUEUEOVERFLOW As Integer = -31
    Public Const ATM_MEMORYUNAVAILABLE As Integer = -101
    Public Const ATM_DELAYENDEDBYCLIENT As Integer = -102
    Public Const ATM_UNCONFIGUREDPSID As Integer = -103
    Public Const ATM_NOEMULATORATTACHED As Integer = -104
    Public Const ATM_WSCTRLFAILURE As Integer = -105
    Public Const ATM_NOMATCHINGPSID As Integer = -200
    Public Const ATM_SESSIONOPEN As Integer = -201
    Public Const ATM_CONFIGOPEN As Integer = -202
    Public Const ATM_LIBLOADERROR As Integer = -203
    Public Const ATM_EVENTALREADYSET As Integer = -301
    Public Const ATM_EVENTMAXEXCEEDED As Integer = -302
    Public Const ATM_TABLEMAXEXCEEDED As Integer = -303
    Public Const ATM_TABLENOTSET As Integer = -304
    Public Const ATM_INDEXNOTSET As Integer = -305
    Public Const ATM_INVALIDROW As Integer = -306
    Public Const ATM_INVALIDCOLUMN As Integer = -307
    Public Const ATM_STRINGTOOLONG As Integer = -308

    ' These new constants support the ATMGetConnectionStatus and ATMGetFieldInfo functions.
    Public Const ATM_NOTATTRIBUTE As Integer = 0
    Public Const ATM_INVALIDNUM As Integer = 1
    Public Const ATM_NUMONLY As Integer = 2
    Public Const ATM_PROTFIELD As Integer = 3
    Public Const ATM_PASTEOF As Integer = 4
    Public Const ATM_BUSY As Integer = 5
    Public Const ATM_INVALIDFUNC As Integer = 6
    Public Const ATM_UNAUTHORIZED As Integer = 7
    Public Const ATM_SYSTEM As Integer = 8
    Public Const ATM_INVALIDCHAR As Integer = 9
    Public Const ATM_APPOWNED As Integer = 1
    Public Const ATM_SSCP As Integer = 2
    Public Const ATM_UNOWNED As Integer = 3
    Public Const ATM_NONE As Integer = 4
    Public Const ATM_UNKNOWN As Integer = 5
    Public Const ATM_PROGCHECK As Integer = 1
    Public Const ATM_COMMCHECK As Integer = 2
    Public Const ATM_MACHINECHECK As Integer = 3
    Public Const ATM_UPPER As Integer = 1
    Public Const ATM_MIXED As Integer = 2

    ' Global constants for ATMStartKeyIntercept function
    Public Const ATM_AIDKeys As Integer = 1
    Public Const ATM_AllKeys As Integer = 2

    ' Session Constants
    Public Const SESSION_A As Integer = 0
    Public Const SESSION_B As Integer = 1
    Public Const SESSION_C As Integer = 2
    Public Const SESSION_D As Integer = 3
    Public Const SESSION_E As Integer = 4
    Public Const SESSION_F As Integer = 5
    Public Const SESSION_G As Integer = 6
    Public Const SESSION_H As Integer = 7
    Public Const SESSION_I As Integer = 8
    Public Const SESSION_J As Integer = 9
    Public Const SESSION_K As Integer = 10
    Public Const SESSION_L As Integer = 11
    Public Const SESSION_M As Integer = 12
    Public Const SESSION_N As Integer = 13
    Public Const SESSION_O As Integer = 14
    Public Const SESSION_P As Integer = 15
    Public Const SESSION_Q As Integer = 16
    Public Const SESSION_R As Integer = 17
    Public Const SESSION_S As Integer = 18
    Public Const SESSION_T As Integer = 19
    Public Const SESSION_U As Integer = 20
    Public Const SESSION_V As Integer = 21
    Public Const SESSION_W As Integer = 22
    Public Const SESSION_X As Integer = 23
    Public Const SESSION_Y As Integer = 24
    Public Const SESSION_Z As Integer = 25


    'Windows API functions and declaration needed to create window/handle
    Public Declare Function DestroyWindow Lib "User32" (ByVal hwnd As Integer) As Integer

    <DllImport("user32.dll", CharSet:=CharSet.Auto)> _
    Public Function CreateWindowEx( _
      ByVal dwExStyle As Integer, _
      ByVal lpClassName As String, _
      ByVal lpWindowName As String, _
      ByVal dwStyle As Integer, _
      ByVal x As Integer, _
      ByVal y As Integer, _
      ByVal nWidth As Integer, _
      ByVal nHeight As Integer, _
      ByVal hWndParent As IntPtr, _
      ByVal hMenut As IntPtr, _
      ByVal hInstancet As IntPtr, _
      ByVal lpParamt As IntPtr) As IntPtr
    End Function

    Public Const WS_CHILD As Integer = &H40000000

    Public Declare Function FreeLibrary Lib "kernel32.dll" (ByVal hModule As IntPtr) As Boolean
    Public Declare Function LoadLibraryA Lib "kernel32.dll" (ByVal hModule As String) As Boolean
    Public Declare Function GetModuleHandleExA Lib "kernel32.dll" (ByVal dwFlags As Integer, ByVal ModuleName As String, ByRef phModule As IntPtr) As Boolean

End Module
