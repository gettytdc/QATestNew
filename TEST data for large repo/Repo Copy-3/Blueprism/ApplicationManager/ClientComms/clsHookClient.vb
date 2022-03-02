Option Strict On

Imports System.Threading
Imports System.IO
Imports System.IO.Pipes
Imports System.Collections.Generic
Imports System.ComponentModel
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports System.Runtime.InteropServices

''' Project  : ClientComms
''' Class    : ClientComms.clsHookClient
''' <summary>
''' This class provides a consistent encapsulated interface for communicating
''' with and managing the Injector and BPInjAgent components of the API
''' Hooking functionality.
''' </summary>
Public Class clsHookClient

    ''' <summary>
    ''' Events raised by this class when things happen on the connection.
    ''' </summary>
    Public Event Terminated()
    Public Event Failed(ByVal sMsg As String)
    Public Event LineReceived(ByVal sLine As String)

    ''' <summary>
    ''' True when connected.
    ''' </summary>
    Private mbConnected As Boolean

    ''' <summary>
    ''' The thread managing the connection currently.
    ''' </summary>
    Private mtCommsThread As Thread

    ''' <summary>
    ''' Set to True to terminate the Comms thread.
    ''' </summary>
    Private mbTerminate As Boolean

    ''' <summary>
    ''' The directory where the Injector resources can be found. See
    ''' documentation on constructor for more information.
    ''' </summary>
    Private msInjectorFolder As String

    ''' <summary>
    ''' When connecting/connected, these contain the PID of the target application
    ''' and the ID of its main thread respectively.
    ''' miMainThread can be 0 - see the documentation for Connect().
    ''' </summary>
    Private miTargetPID As Integer
    Private miMainThread As Integer

    Private mPipeServer As NamedPipeServerStream = Nothing

    ''' <summary>
    ''' This event is set when the pipe is connected.
    ''' </summary>
    Private mPipeReady As New ManualResetEvent(False)

    ''' <summary>
    ''' The response to the most recent command sent to the injected DLL. The
    ''' CommsThread sets this when the response is received. The SendCommand
    ''' function reads and clears it.
    ''' </summary>
    ''' <remarks></remarks>
    Private msCommandResponse As String = Nothing

    ''' <summary>
    ''' Used in conjunction with msCommandResponse. Determinates whether the
    ''' response there indicates a failure or success.
    ''' </summary>
    Private mbCommandSuccess As Boolean

    ''' <summary>
    ''' Used in conjunction with msCommandResponse. Determinates if we are currently
    ''' waiting for a response from a command we sent.
    ''' </summary>
    Private mbCommandWaiting As Boolean


    ''' <summary>
    ''' Communications thread - handles everything!
    ''' </summary>
    Private Sub CommsThread()

        Dim sErr As String = Nothing

        Try

            'Initialise...
            mPipeServer = New NamedPipeServerStream("BluePrismInjectorPipe" & miTargetPID.ToString("X8"), PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 65536, 8192)

            'Start waiting for the client (target app) to connect...
            Dim connectres As IAsyncResult = mPipeServer.BeginWaitForConnection(Nothing, Nothing)

            'Inject...
            clsConfig.LogHook(My.Resources.HOOKStartingInjection)
            If Not StartInjection(sErr) Then
                Throw New InvalidOperationException(String.Format(My.Resources.CouldntStartInjection0, sErr))
            End If

            'Finish waiting for the connection...
            clsConfig.LogHook(My.Resources.HOOKWaitingForPipeConnection)
            While True
                If connectres.AsyncWaitHandle.WaitOne(500, False) Then Exit While
                If mbTerminate Then
                    RaiseEvent Terminated()
                    Exit Sub
                End If
            End While
            mPipeServer.EndWaitForConnection(connectres)

            'Signal that the pipe is connected.
            mPipeReady.Set()
            clsConfig.LogHook(My.Resources.HOOKPipeConnectionEstablished)

            Dim buffer(8191) As Byte
            Dim pipemsg(32767) As Byte
            Dim pipemsgoffset As Integer
            Dim chars(32767) As Char
            Dim decoder As Decoder = Encoding.UTF8.GetDecoder()

            'Keep going until we're told to terminate...
            While Not mbTerminate

                'Read the next message....
                Dim message As String = ""
                decoder.Reset()
                While True
                    pipemsgoffset = 0
                    Do
                        connectres = mPipeServer.BeginRead(buffer, 0, buffer.Length, Nothing, Nothing)
                        While True
                            If connectres.AsyncWaitHandle.WaitOne(100, False) Then Exit While
                            If mbTerminate Then
                                RaiseEvent Terminated()
                                Exit Sub
                            End If
                        End While
                        Dim num As Integer = mPipeServer.EndRead(connectres)
                        If num = 0 Then
                            clsConfig.LogHook(My.Resources.HOOKCommsThreadTerminated0BytesRead)
                            RaiseEvent Terminated()
                            Exit Sub
                        End If
                        Array.Copy(buffer, 0, pipemsg, pipemsgoffset, num)
                        pipemsgoffset += num
                    Loop Until mPipeServer.IsMessageComplete
                    If pipemsg(0) = 0 Then
                        'Large messages are sent in several chunks (i.e. individual messages), with the
                        'first byte being a '\0' in all except the final chunk. We have received one of
                        'these chunks here, so keep it and continue adding to the message.
                        Dim numchars As Integer = decoder.GetChars(pipemsg, 1, pipemsgoffset - 1, chars, 0)
                        message &= New String(chars, 0, numchars)
                    Else
                        'This is either an entire 'normal' message, or the final chunk of a very large
                        'message that has been split up.
                        Dim numchars As Integer = decoder.GetChars(pipemsg, 0, pipemsgoffset, chars, 0)
                        message &= New String(chars, 0, numchars)
                        Exit While
                    End If
                End While

                If mbCommandWaiting Then
                    If message.StartsWith(My.Resources.RESPONSE) Then
                        mbCommandSuccess = True
                        msCommandResponse &= message.Substring(9)
                        mbCommandWaiting = False
                    ElseIf message.StartsWith(My.Resources.FAILURE) Then
                        mbCommandSuccess = False
                        msCommandResponse = message.Substring(8)
                        mbCommandWaiting = False
                    End If
                End If
                clsConfig.LogHook(My.Resources.HOOKCS & message)
                RaiseEvent LineReceived(message)

            End While

        Catch e As Exception
            sErr = e.Message & " - " & e.StackTrace
            clsConfig.LogHook(My.Resources.HOOKCommsThreadfailedD & sErr)
            RaiseEvent Failed(sErr)

        Finally
            mPipeReady.Reset()
            If Not mPipeServer Is Nothing Then
                mPipeServer.Close()
                mPipeServer.Dispose()
                mPipeServer = Nothing
            End If
        End Try

        RaiseEvent Terminated()

    End Sub


    ''' <summary>
    ''' Create a new instance of this class. Does not connect.
    ''' </summary>
    ''' <param name="sInjectorFolder">The directory where BPInjAgent.dll can be
    ''' found.</param>
    Public Sub New(ByVal sInjectorFolder As String)
        mbConnected = False
        mbTerminate = False
        msInjectorFolder = sInjectorFolder
    End Sub


    ''' <summary>
    ''' Connect to the target application. This involves both injecting
    ''' BPInjAgent, and establishing a communications channel with it.
    ''' </summary>
    ''' <param name="iPID">The process ID of the target</param>
    ''' <param name="iMainThread">The thread ID of the main thread, which should be
    ''' suspended before calling this, and will be resumed at the appropriate time.
    ''' If this is 0, we create a brand new thread for the purposes of injection.
    ''' This method is used when attaching, since the target's main thread may be
    ''' in a suspended (wait) state at the time.
    ''' </param>
    ''' <param name="sErr">In the event of failure, this contains an error
    ''' description.</param>
    ''' <returns>True if successful.</returns>
    Public Function Connect(ByVal iPID As Integer, ByVal iMainThread As Int32, ByRef sErr As String) As Boolean

        'Can't connect if already connected...
        If mbConnected Then
            sErr = My.Resources.AlreadyConnected
            Return False
        End If

        miTargetPID = iPID
        miMainThread = iMainThread

        mbTerminate = False
        mtCommsThread = New Thread(New ThreadStart(AddressOf CommsThread))
        mtCommsThread.Start()

        'Wait for the pipe to be connected, which means that everything is up and running
        If Not mPipeReady.WaitOne(20000, False) Then
            sErr = My.Resources.TimedOutWaitingForPipeConnection
            mbTerminate = True
            Return False
        End If

        mbConnected = True
        Return True

    End Function


    ''' <summary>
    ''' Disconnect.
    ''' </summary>
    Public Sub Disconnect()

        If Not mbConnected Then Return

        Debug.WriteLine("Sending quit command...")
        SendCommand("quit", Nothing, False)
        mbTerminate = True
        Debug.WriteLine("Ending injection...")
        Dim sErr As String = Nothing
        If Not EndInjection(sErr) Then
            Debug.WriteLine(sErr)
        End If
        Debug.WriteLine("Waiting for comms thread to terminate...")
        If Not mtCommsThread Is Nothing Then
            mtCommsThread.Join()
            mtCommsThread = Nothing
        End If
        Debug.WriteLine("Disconnection complete")
        mbConnected = False

    End Sub


    ''' <summary>
    ''' Send a command to the target application's injection agent, wait for the
    ''' response, and return it.
    ''' </summary>
    ''' <param name="sCommand">The command to send.</param>
    ''' <param name="sResult">On return (successful or otherwise) contains a
    ''' response (assuming one was required - see bResponseRequired)
    ''' </param>
    ''' <param name="bResponseRequired">True if a response is required</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function SendCommand(ByVal sCommand As String, ByRef sResult As String, ByVal bResponseRequired As Boolean) As Boolean
        Try
            'Something is out of sync if there is already a response waiting, so
            'just return an error immediately.
            If mbCommandWaiting Then
                sResult = My.Resources.OutOfSync
                Return False
            End If

            'Don't try and do anything if the pipe is not connected...
            If Not mPipeReady.WaitOne(1000, False) Then
                sResult = My.Resources.PipeNotConnected
                Return False
            End If

            Using sw As New StreamWriter(
                mPipeServer, New UTF8Encoding(False, True), 1024, True)

                sw.AutoFlush = True
                If bResponseRequired Then
                    mbCommandWaiting = True
                    msCommandResponse = ""
                End If
                sw.Write(sCommand)

            End Using

            'Exit now if a result is not expected.
            If Not bResponseRequired Then Return True
            Dim iWaitCount As Integer = 0
            While mbCommandWaiting
                iWaitCount += 1
                If iWaitCount > (1000 * 60 * 5) / 25 Then
                    sResult = My.Resources.Timeout
                    Return False
                End If
                Thread.Sleep(25)
            End While
            sResult = msCommandResponse
            Return mbCommandSuccess

        Catch ex As Exception
            Return False
        End Try
    End Function



    ''' <summary>
    ''' A miniature specialised inline assembly generator that abstracts the
    ''' differences between x86 and x64, supporting only the few things we are
    ''' using here!
    ''' </summary>
    Private Class MiniCodeGenerator

        Private mx64 As Boolean
        Private mCode As New List(Of Byte)

        Public Sub New(ByVal x64 As Boolean)
            mx64 = x64
        End Sub

        Public Function GetCode() As Byte()
            Return mCode.ToArray()
        End Function

        Public Sub PushAddress(addr As IntPtr)
            If mx64 Then
                'For 64 bit, there's no push immediate for a QWORD, and nor can
                'you push a DWORD without the stack pointer advancing 8 bytes
                'anyway, so it has to be done in this roundabout way...
                '1. Push low-dword...
                mCode.Add(&H68)
                AddDWORD(Convert.ToUInt32(addr.ToInt64() And &HFFFFFFFFL))
                '2. Use  mov [rsp+4], high-dword
                mCode.Add(&HC7)
                mCode.Add(&H44)
                mCode.Add(&H24)
                mCode.Add(&H4)
                AddDWORD(Convert.ToUInt32(addr.ToInt64() >> 32))
            Else
                'On the other hand, 32 bit is a simple immediate push...
                mCode.Add(&H68)
                AddDWORD(CUInt(addr.ToInt32()))
            End If
        End Sub
        Public Sub AddressToAx(address As IntPtr)
            AddressToReg(address, &HB8)
        End Sub
        Public Sub AddressToCx(address As IntPtr)
            AddressToReg(address, &HB9)
        End Sub
        Public Sub AddressToDx(address As IntPtr)
            AddressToReg(address, &HBA)
        End Sub
        Private Sub AddressToReg(address As IntPtr, opCode As Byte)
            If mx64 Then
                mCode.Add(&H48)
                mCode.Add(opCode)
                AddDWORD(Convert.ToUInt32(address.ToInt64() And &HFFFFFFFFL))
                AddDWORD(Convert.ToUInt32(address.ToInt64() >> 32))
            Else
                mCode.Add(opCode)
                AddDWORD(CUInt(address.ToInt32()))
            End If
        End Sub
        Public Sub CallAx()
            mCode.Add(&HFF)
            mCode.Add(&HD0)
        End Sub
        ''' <summary>
        ''' Call the given address, with a single pointer parameter.
        ''' </summary>
        ''' <param name="address">The address to call</param>
        ''' <param name="p1">The only parameter, a pointer</param>
        Public Sub CallWithPtr(address As IntPtr, p1 As IntPtr)
            If mx64 Then
                AddressToCx(p1)
            Else
                PushAddress(p1)
            End If
            AddressToAx(address)
            CallAx()
        End Sub
        ''' <summary>
        ''' Call the given address, with eax/rax as the first parameter, and
        ''' a pointer as the second.
        ''' </summary>
        ''' <param name="address">The address to call</param>
        ''' <param name="p2">The second parameter, a pointer</param>
        Public Sub CallWithAxAndPtr(address As IntPtr, p2 As IntPtr)
            If mx64 Then
                AddressToDx(p2)
                AxToCx()
            Else
                PushAddress(p2)
                PushAx()
            End If
            AddressToAx(address)
            CallAx()
        End Sub
        Public Sub AxToCx()
            If mx64 Then
                mCode.Add(&H48)
            End If
            mCode.Add(&H89)
            mCode.Add(&HC1)
        End Sub
        Public Sub AddRsp(amount As Byte)
            mCode.Add(&H48)
            mCode.Add(&H83)
            mCode.Add(&HC4)
            mCode.Add(amount)
        End Sub
        Public Sub SubRsp(amount As Byte)
            mCode.Add(&H48)
            mCode.Add(&H83)
            mCode.Add(&HEC)
            mCode.Add(amount)
        End Sub
        Public Sub PushAx()
            mCode.Add(&H50)
        End Sub
        ''' <summary>
        ''' Push all registers and flags to save state. For x64, also adjusts
        ''' stack pointer as per MS x64 calling convention requirements in
        ''' relation to the calls we're going to make.
        ''' </summary>
        Public Sub Pushall()
            mCode.Add(&H9C) 'Flags
            If mx64 Then
                mCode.Add(&H50)     'rax
                mCode.Add(&H53)     'rbx
                mCode.Add(&H51)     'rcx
                mCode.Add(&H52)     'rdx
                mCode.Add(&H55)     'rbp
                mCode.Add(&H57)     'rdi
                mCode.Add(&H56)     'rsi
                For r As Byte = &H50 To &H57 'r8 thru r15
                    mCode.Add(&H41)
                    mCode.Add(r)
                Next
                mCode.Add(&H48)     'mov rbp,rsp
                mCode.Add(&H89)
                mCode.Add(&HE5)
                SubRsp(&H30)
                mCode.Add(&H48)     'and rsp, 0xFFFFFFFFFFFFFFF0
                mCode.Add(&H83)     'to ensure stack alignment
                mCode.Add(&HE4)
                mCode.Add(&HF0)
            Else
                mCode.Add(&H60)
            End If
        End Sub
        ''' <summary>
        ''' The opposite of Pushall()
        ''' </summary>
        Public Sub Popall()
            If mx64 Then
                mCode.Add(&H48)     'mov rsp, rbp
                mCode.Add(&H89)
                mCode.Add(&HEC)
                For r As Integer = &H5F To &H58 Step -1 'r15 thru r8
                    mCode.Add(&H41)
                    mCode.Add(CByte(r))
                Next
                mCode.Add(&H5E)     'rsi
                mCode.Add(&H5F)     'rdi
                mCode.Add(&H5D)     'rbp
                mCode.Add(&H5A)     'rdx
                mCode.Add(&H59)     'rcx
                mCode.Add(&H5B)     'rbx
                mCode.Add(&H58)     'rax 
            Else
                mCode.Add(&H61)
            End If
            mCode.Add(&H9D) 'Flags
        End Sub
        Public Sub Ret()
            mCode.Add(&HC3)
        End Sub
        Private Sub AddDWORD(dWord As UInt32)
            mCode.Add(CByte(dWord And &HFF))
            mCode.Add(CByte((dWord >> 8) And &HFF))
            mCode.Add(CByte((dWord >> 16) And &HFF))
            mCode.Add(CByte((dWord >> 24) And &HFF))
        End Sub
    End Class

    ''' <summary>
    ''' Inject into our target application which has been started in a suspended
    ''' state.
    ''' </summary>
    ''' <param name="sErr">If an error occurs, this contains an error
    ''' description.</param>
    ''' <returns>True if successful, False otherwise</returns>
    Private Function StartInjection(ByRef sErr As String) As Boolean
        Dim contextMem As IntPtr = IntPtr.Zero
        Dim contextAligned As IntPtr
        Try
            'Open the remote process...
            Dim hProcess As IntPtr =
             OpenProcess(ProcessAccess.HOOK_RIGHTS, False, miTargetPID)

            Dim bits As String = ""
            Dim x64 As Boolean = False
            If BPUtil.Is64BitProcess(hProcess) Then
                bits = "64"
                x64 = True
            End If
            Dim sFilename As String = msInjectorFolder
            If clsConfig.AgentDiags Then
                sFilename &= String.Format("\BPInjAgent{0}D.dll", bits)
            Else
                sFilename &= String.Format("\BPInjAgent{0}.dll", bits)
            End If

            If Not File.Exists(sFilename) Then
                sErr = My.Resources.InvalidPathToInjectorAgent
                Return False
            End If

            'Two buffers, which correspond to what we're going to stick in the target
            'process memory space, in this order. The first is the path to the DLL we
            'want to load (i.e. BPInjAgent.dll), the second the name of the Init function
            'we want to call after the library is loaded. Directly following these will
            'be some code to do the loading, calling, and then jump to the real application
            'code.
            Dim baFilename As Byte() = Text.Encoding.Unicode.GetBytes(sFilename & ChrW(0))
            Dim baInitName As Byte() = Text.Encoding.ASCII.GetBytes("Init" & ChrW(0))
            Const maxCodeSize As Integer = &H100

            'Allocate some memory in the target process's address space for the DLL
            'filename and the code we're going to inject.
            Dim remoteMem As IntPtr = VirtualAllocEx(hProcess, Nothing, baFilename.Length + baInitName.Length + maxCodeSize, AllocationFlags.MEM_COMMIT, AllocationFlags.PAGE_EXECUTE_READWRITE)
            If remoteMem = IntPtr.Zero Then
                sErr = My.Resources.FailedToAllocateRemoteMemory
                Return False
            End If
            Dim initAddress As IntPtr = remoteMem + baFilename.Length
            Dim codeAddress As IntPtr = initAddress + baInitName.Length

            'Get the current address of the suspended thread - this is where we will
            'return execution to after our injected code has run...
            'It will remain zero when attaching, as we're creating a new thread from
            'which we just want to return.
            Dim retAddress As IntPtr = IntPtr.Zero
            Dim threadH As IntPtr
            Dim context As CONTEXT = Nothing
            Dim context64 As CONTEXT_x64 = Nothing
            If miMainThread <> 0 Then
                Dim access As Int32 = THREAD_GET_CONTEXT Or THREAD_QUERY_INFORMATION Or THREAD_SET_CONTEXT Or THREAD_SUSPEND_RESUME
                threadH = OpenThread(access, False, miMainThread)
                If threadH.Equals(IntPtr.Zero) Then
                    Dim ex As New Win32Exception(Marshal.GetLastWin32Error())
                    sErr = My.Resources.FailedToOpenThreadD & ex.Message
                End If
                If x64 Then
                    contextMem = Marshal.AllocHGlobal(1232 + 8)
                    contextAligned = New IntPtr(16 * ((contextMem.ToInt64() + 15) \ 16))
                    Marshal.WriteInt32(contextAligned + 6 * 8, CONTEXT_CONTROL Or CONTEXT_AMD64)
                    If Not GetThreadContext64p(threadH, contextAligned) Then
                        Dim ex As New Win32Exception(Marshal.GetLastWin32Error())
                        sErr = My.Resources.FailedToGetThreadContextD & ex.Message
                        Return False
                    End If
                    retAddress = Marshal.ReadIntPtr(contextAligned + 248)
                Else
                    context = New CONTEXT()
                    context.ContextFlags = CONTEXT_CONTROL Or CONTEXT_X86
                    If Not GetThreadContext(threadH, context) Then
                        Dim ex As New Win32Exception(Marshal.GetLastWin32Error())
                        sErr = My.Resources.FailedToGetThreadContextD & ex.Message
                        Return False
                    End If
                    retAddress = context.Eip
                End If
            End If

            'Get the address of LoadLibraryW - because it's in kernel32.dll it will have
            'the same address in the remote process as it does in ours...
            Dim kernel32 As IntPtr = GetModuleHandle("Kernel32")
            Dim loadLibraryW As IntPtr = GetProcAddress(kernel32, "LoadLibraryW")
            'Same for GetProcessAddressW
            Dim getProcAddressA As IntPtr = GetProcAddress(kernel32, "GetProcAddress")
            If getProcAddressA.Equals(IntPtr.Zero) Then
                sErr = My.Resources.CouldntFindGetProcAddress
                Return False
            End If

            'Create the code we want to execute. The IP of the main thread will be
            'set to point to this code, and it will return to the original IP when
            'it has finished.
            Dim asm As New MiniCodeGenerator(x64)
            If Not retAddress.Equals(IntPtr.Zero) Then
                asm.PushAddress(retAddress)
            End If
            asm.Pushall()
            asm.CallWithPtr(loadLibraryW, remoteMem)
            'eax now contains the module handle for our injected DLL...
            asm.CallWithAxAndPtr(getProcAddressA, initAddress)
            'eax/rax now contains the address of our DLL's Init() function...
            asm.CallAx()            '...which we call
            asm.Popall()
            asm.Ret()

            Dim code As Byte() = asm.GetCode()
            If code.Length > maxCodeSize Then
                'This can't actually happen!
                sErr = My.Resources.TooMuchAsmWasGenerated
                Return False
            End If

            'Copy the filename...
            If Not WriteProcessMemory(hProcess, remoteMem, baFilename, baFilename.Length, Nothing) Then
                sErr = My.Resources.FailedToCopyFilename
                Return False
            End If
            'Copy the init name...
            If Not WriteProcessMemory(hProcess, initAddress, baInitName, baFilename.Length, Nothing) Then
                sErr = My.Resources.FailedToCopyInitname
                Return False
            End If
            'Copy the code...
            If Not WriteProcessMemory(hProcess, codeAddress, code, code.Length, Nothing) Then
                sErr = My.Resources.FailedToCopyCode
                Return False
            End If

            If miMainThread <> 0 Then
                'Change the thread's instruction pointer to point to our injected code...
                If x64 Then
                    Marshal.WriteInt32(contextAligned + 6 * 8, CONTEXT_CONTROL Or CONTEXT_AMD64)
                    Marshal.WriteIntPtr(contextAligned + 248, codeAddress)
                    If Not SetThreadContext64p(threadH, contextAligned) Then
                        sErr = My.Resources.FailedToSetThreadContext
                        Return False
                    End If
                Else
                    context.ContextFlags = CONTEXT_CONTROL Or CONTEXT_X86
                    context.Eip = codeAddress
                    If Not SetThreadContext(threadH, context) Then
                        sErr = My.Resources.FailedToSetThreadContext
                        Return False
                    End If
                End If
                'Resume the thread...
                ResumeThread(threadH)
            Else
                threadH = CreateRemoteThread(hProcess, Nothing, 0, codeAddress, IntPtr.Zero, 0, Nothing)
                If threadH = IntPtr.Zero Then
                    sErr = My.Resources.CouldNotStartRemoteThread
                    Return False
                End If
            End If

            'Close things...
            CloseHandle(threadH)
            CloseHandle(hProcess)

            Return True

        Catch ex As Exception
            sErr = ex.Message
            Return False
        Finally
            If contextMem <> IntPtr.Zero Then
                Marshal.FreeHGlobal(contextMem)
            End If
        End Try
    End Function

    ''' <summary>
    ''' Remove our DLL from the target process.
    ''' </summary>
    ''' <param name="sErr">In the event of failure, this contains an error message.
    ''' </param>
    ''' <returns>True if successful, False otherwise.</returns>
    Private Function EndInjection(ByRef sErr As String) As Boolean

        'Find our injected DLL in the target process
        Dim p As Process
        Try
            p = Process.GetProcessById(miTargetPID)
        Catch ex As Exception
            'We couldn't find our target process, which must mean it has terminated
            'already. In that case, we don't need to do anything extra here:
            Return True
        End Try
        Dim ourDll As ProcessModule = Nothing
        Try
            For Each m As ProcessModule In p.Modules
                If m.FileName.ToLower().Contains("bpinjagent") Then
                    ourDll = m
                    Exit For
                End If
            Next
        Catch ex As Exception
            'If this happens, the process has most likely terminated after we got the
            'Process object, while we were enumerating the modules.
            Return True
        End Try
        If ourDll Is Nothing Then
            sErr = "Could not find our injected dll"
            Return False
        End If

        'Open the remote process...
        Dim hProcess As IntPtr = IntPtr.Zero
        Try
            hProcess = OpenProcess(ProcessAccess.UNHOOK_RIGHTS, False, miTargetPID)
            Dim kernel32 As IntPtr = GetModuleHandle("Kernel32")
            Dim freeLibrary As IntPtr = GetProcAddress(kernel32, "FreeLibrary")
            Dim modHandle As IntPtr = GetModuleHandle(ourDll.ModuleName)
            Dim hThread As IntPtr = CreateRemoteThread(hProcess, Nothing, 0, freeLibrary, modHandle, 0, Nothing)
            If hThread = IntPtr.Zero Then
                sErr = "Could not start remote thread"
                Return False
            End If
            WaitForSingleObject(hThread, INFINITE)
            CloseHandle(hThread)
        Catch ex As Exception
            sErr = "Failed while freeing remote library"
            Return False
        Finally
            If hProcess <> IntPtr.Zero Then CloseHandle(hProcess)
        End Try
        Return True

    End Function

End Class
