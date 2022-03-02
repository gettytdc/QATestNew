Imports System.Runtime.InteropServices

Public Class SessionStartInfo

    ''' <summary>
    ''' The session profile path
    ''' </summary>
    Public SessionFile As String

    ''' <summary>
    ''' The session name
    ''' </summary>
    Public SessionID As String

    ''' <summary>
    ''' The terminal type
    ''' </summary>
    Public TerminalType As TerminalTypes

    ''' <summary>
    ''' The amount of time for which to sleep between
    ''' individual polls of the mainframe API during wait operations.
    ''' If zero then the mainframe's own default value will be used.
    ''' </summary>
    Public WaitSleepTime As Integer

    ''' <summary>
    ''' The maximum waiting time during mainframe wait
    ''' operations. If zero then the mainframe's own default value will be used.
    ''' </summary>
    Public WaitTimeout As Integer

    ''' <summary>
    ''' Information specific to the session
    ''' type. For Attachmate, this is the emulation type. For others, nothing is
    ''' defined yet - pass an empty string.
    ''' </summary>
    Public MainframeSpecificInfo As String

    ''' <summary>
    ''' The name of the DLL to use for Generic HLLAPI
    ''' </summary>
    Public SessionDLLName As String

    ''' <summary>
    ''' The name of the DLL entry point to use for Generic HLLAPI
    ''' </summary>
    Public SessionDLLEntryPoint As String

    ''' <summary>
    ''' The session type for HLLAPI
    ''' </summary>
    Public SessionType As SessionTypes

    ''' <summary>
    ''' The calling convention for HLLAPI
    ''' </summary>
    Public Convention As CallingConvention

    ''' <summary>
    ''' The text encoding used to convert HLLAPI data to unicode strings
    ''' </summary>
    Public Encoding As Encoding

    ''' <summary>
    ''' Describes the type of session implementation we expect the target (E)HLLAPI
    ''' to be using.
    ''' </summary>
    Public Enum SessionTypes

        ''' <summary>
        ''' 'Normal' implementation - an 18 byte data block.
        ''' </summary>
        Normal

        ''' <summary>
        ''' Enhanced implementation, per the EHLLAPI spec - a 20 byte data block.
        ''' </summary>
        Enhanced

        ''' <summary>
        ''' No implementation at all, or a broken one - i.e. don't even bother
        ''' trying to call it. For example, NDL Conductor's implementation returns
        ''' a 'System Error' response, no matter what.
        ''' </summary>
        NotImplemented

    End Enum

    ''' <summary>
    ''' The type of terminal session - ie the API to use when communicating.
    ''' </summary>
    Public Enum TerminalTypes
        ''' <summary>
        ''' Corresponds to a Generic terminal (HLLAPI)
        ''' </summary>
        GEN
        ''' <summary>
        ''' Corresponds to an IBM Personal Communications terminal
        ''' </summary>
        IBM
        ''' <summary>
        ''' Corresponds to an Attachmate terminal.
        ''' </summary>
        ATM
        ''' <summary>
        ''' Corresponds to a HummingBird HostExplorer terminal.
        ''' </summary>
        HUM
        ''' <summary>
        ''' Corresponds to a Passport HostExplorer terminal.
        ''' </summary>
        PSS
        ''' <summary>
        ''' Corresponds to a Rumba terminal.
        ''' </summary>
        RUM
        ''' <summary>
        ''' Corresponds to a TeemTalk terminal.
        ''' </summary>
        TMT
        ''' <summary>
        ''' Corresponds to an IBM Personal Communicator terminal using HLLAPI.
        ''' </summary>
        PCH
        ''' <summary>
        ''' Corresponds to an IBM iAccess terminal using EHLLAPI.
        ''' </summary>
        IAC
        ''' <summary>
        ''' Corresponds to an Ericom PowerTerm lite terminal using HLLAPI.
        ''' </summary>
        PWT
        ''' <summary>
        ''' Corresponds to an NDL Conductor terminal using HLLAPI.
        ''' </summary>
        CON
        ''' <summary>
        ''' Corresponds to an RMD emulator (TCP communication)
        ''' </summary>
        RMD
        ''' <summary>
        ''' Corresponds to an HostAccess emulator (TCP communication)
        ''' </summary>
        HAT
        ''' <summary>
        ''' Corresponds to a HostExplorer EHLLAPI emulator
        ''' </summary>
        HEE
        ''' <summary>
        ''' Corresponds to a Attachmate Reflections emulator
        ''' </summary>
        ART
        ''' <summary>
        ''' Corresponds to a Attachmate Reflections emulator using .NET
        ''' </summary>
        ARN
        ''' <summary>
        ''' Corresponds to a InfoConnect WinHLLAPI emulator
        ''' </summary>
        INF
    End Enum
End Class
