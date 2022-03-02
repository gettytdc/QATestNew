
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

Public Class JABWrapper

    ''' <summary>
    ''' Boolean used to indicate whether the JAB wrapper has been initialised.
    ''' </summary>
    Private mbInited As Boolean = False

    ''' <summary>
    ''' Initialise Java Access Bridge connectivity.
    ''' </summary>
    ''' <remarks>The only likely failure is if Java Access Bridge is not installed
    ''' correctly on the machine.</remarks>
    Public Sub Init()
        If mbInited Then Return
        Try
            WAB.Windows_run()
            mbInited = True
        Catch ex As DllNotFoundException
            Throw New JavaAccessBridgeDLLNotFoundException(ex)
        Catch
            Throw
        End Try
    End Sub

    <Serializable>
    Public Class JavaAccessBridgeDLLNotFoundException : Inherits BluePrismException
        Public Sub New(ByVal InnerException As DllNotFoundException)
            MyBase.New(GetInfo(InnerException), InnerException)
        End Sub

        <DllImport("kernel32.dll")>
        Private Shared Function GetWindowsDirectory(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpBuffer As StringBuilder, ByVal length As Integer) As Integer
        End Function
        Private Shared ReadOnly Property WindowsDirectory() As String
            Get
                Dim stringBuilder As StringBuilder = New StringBuilder(260)
                If GetWindowsDirectory(stringBuilder, 260) = 0 Then
                    Return stringBuilder.ToString()
                End If
                Return Environment.GetEnvironmentVariable("windir")
            End Get
        End Property
        <DllImport("shell32.dll")>
        Public Shared Function SHGetSpecialFolderPath(ByVal hwndOwner As IntPtr, <[Out]()> ByVal lpszPath As StringBuilder, ByVal nFolder As Integer, ByVal fCreate As Boolean) As Boolean
        End Function
        Private Shared ReadOnly Property SystemDirectory() As String
            Get
                Dim path As New StringBuilder(260)
                SHGetSpecialFolderPath(IntPtr.Zero, path, &H29, False)
                Return path.ToString()
            End Get
        End Property

        Public Shared Function GetInfo(ByVal ex As DllNotFoundException) As String
            Try
                Dim msg As String = ex.Message
                Dim start As Integer = msg.IndexOf("'") + 1
                Dim finish As Integer = msg.LastIndexOf("'")
                Dim what As String = msg.Substring(start, finish - start)

                Dim result As New StringBuilder()
                result.AppendLine(
                 My.Resources.FailedToLoadJavaAccessBridgeDLLTheFileCouldNotBeFoundInAnyOfTheFollowingExpecte)
                Dim paths As New Dictionary(Of String, String)
                paths.Add(My.Resources.ApplicationDirectory, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                paths.Add(My.Resources.SystemDirectory, SystemDirectory)
                paths.Add(My.Resources.WindowsDirectory, WindowsDirectory)
                paths.Add(My.Resources.CurrentDirectory, Environment.CurrentDirectory)
                For Each key As String In paths.Keys
                    Dim testPath As String = Path.Combine(paths(key), what)
                    If Not File.Exists(testPath) Then
                        result.Append(key)
                        result.AppendLine(testPath)
                    End If
                Next
                Return result.ToString()
            Catch
                'If we fail we just get our original exception message so we are no
                'worse off
                Return ex.Message
            End Try
        End Function
    End Class

    ''' <summary>
    ''' Shut down Java Access Bridge connectivity.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Shutdown()
        CleanupMouseHook()
        mbInited = False
    End Sub

    Protected Overrides Sub Finalize()
        Shutdown()
    End Sub

    ''' <summary>
    ''' Determine if a given window is a Java window that can be accessed via JAB.
    ''' </summary>
    ''' <param name="hwnd">The window's handle</param>
    ''' <returns>True if it is a Java window, False otherwise.</returns>
    Public Function IsJavaWindow(ByVal hwnd As IntPtr) As Boolean
        Return mbInited AndAlso WAB.isJavaWindow(hwnd)
    End Function

    ''' <summary>
    ''' Determines whether the specified window has a java ancestor.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window of interest.</param>
    ''' <returns>Returns true if the window has a win32 ancestor
    ''' which is a java window, (as determined by the IsJavaWindow method).
    ''' The window itself does not count as an ancestor.</returns>
    Public Function HasJavaAncestor(ByVal hWnd As IntPtr) As Boolean
        Do
            hWnd = GetWindowLongPtr(hWnd, GWL.GWL_HWNDPARENT)
            If hWnd <> IntPtr.Zero AndAlso IsJavaWindow(hWnd) Then Return True
        Loop While (hWnd <> IntPtr.Zero)
        Return False
    End Function

    ''' <summary>
    ''' Get a JABContext object from the given window handle.
    ''' </summary>
    ''' <param name="iHandle">The target window handle.</param>
    ''' <returns>A JABContext which can be used for further queries, or Nothing
    ''' if no context was available.</returns>
    Public Function GetContextFromWindow(ByVal iHandle As IntPtr) As JABContext
        If iHandle <> IntPtr.Zero Then
            Dim vmID As Integer, AC As Long
            If WAB.getAccessibleContextFromHWND(iHandle, vmID, AC) Then
                Return New JABContext(AC, vmID)
            End If
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' The java mouse hook
    ''' </summary>
    Private mMouseHook As WAB.IJavaMouseHook = Nothing

    ''' <summary>
    ''' Sets up the java mouse hook used for spying.
    ''' </summary>
    Public Sub SetupMouseHook()
        If mMouseHook Is Nothing Then
            mMouseHook = WAB.SetupMouseHook()
        End If
    End Sub

    ''' <summary>
    ''' Provides access to the current context.
    ''' </summary>
    Public ReadOnly Property CurrentContext() As JABContext
        Get
            Return mMouseHook.CurrentContext
        End Get
    End Property

    ''' <summary>
    ''' Cleans up the java mouse hook
    ''' </summary>
    Public Sub CleanupMouseHook()
        If mMouseHook IsNot Nothing Then
            mMouseHook.Dispose()
            mMouseHook = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Perform the specified action on the given element.
    ''' </summary>
    ''' <param name="c">The JABContext identifying the element.</param>
    ''' <param name="actionname">The name of the action</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Function DoAction(ByVal c As JABContext, ByVal actionname As String) As Boolean
        Dim failure As Integer = 0

        'Create the action to be done
        Dim actionInfo As New WAB.AccessibleActionInfo
        actionInfo.Name = actionname

        'Stuff it into the containing structure
        Dim actionsToDo As New WAB.AccessibleActionsToDo
        ReDim actionsToDo.Actions(WAB.MaxJABActionsToDo - 1)
        Dim actions As New List(Of WAB.AccessibleActionInfo)
        actions.Add(actionInfo)
        actionsToDo.ActionsCount = actions.Count
        actions.CopyTo(actionsToDo.Actions)

        If WAB.doAccessibleActions(c.vmID, c.AC, actionsToDo, failure) Then
            Return (failure = -1)
        End If
        Return False
    End Function

    ''' <summary>
    ''' Set the text contents of the given element.
    ''' </summary>
    ''' <param name="c">The JABContext identifying the element.</param>
    ''' <param name="value">The new value</param>
    ''' <param name="sErr">Carries back an error message
    ''' in the event of an error.</param>
    ''' <returns>True on success; false on failure.</returns>
    Public Function SetText(ByVal c As JABContext, ByVal value As String, ByRef sErr As String) As Boolean
        If Not WAB.setTextContents(c.vmID, c.AC, value) Then
            sErr = GetSetTextErrorMessage(c)
            Return False
        End If
        Return True
    End Function


    ''' <summary>
    ''' Set the text contents of the given element.
    ''' </summary>
    ''' <param name="c">The JABContext identifying the element.</param>
    ''' <param name="value">The new value</param>
    ''' <param name="sErr">Carries back an error message
    ''' in the event of an error.</param>
    ''' <returns>True on success; false on failure.</returns>
    Public Function SetText(ByVal c As JABContext, ByVal value As SafeString, ByRef sErr As String) As Boolean


        Dim ptr = IntPtr.Zero
        Try
            ptr = Marshal.SecureStringToGlobalAllocUnicode(value)
            If Not WAB.setTextContents(c.vmID, c.AC, ptr) Then
                sErr = GetSetTextErrorMessage(c)
                Return False
            End If
        Finally
            Marshal.ZeroFreeGlobalAllocUnicode(ptr)
        End Try
        Return True
    End Function

    ''' <summary>
    ''' Returns an error message in the event WAB.setTextContents fails.
    ''' </summary>
    ''' <param name="c">he JABContext identifying the element.</param>
    ''' <returns></returns>
    Private Function GetSetTextErrorMessage(ByVal c As JABContext) As String
        Dim errorMsg As String = My.Resources.CallToSetTextContentsFailed

        'If this failure is down to a JRE known to be faulty then explain this fact ...
        Dim VersionInfo As New WAB.AccessBridgeVersionInfo
        If WAB.getVersionInfo(c.vmID, VersionInfo) Then
            Dim VMVersionText As String = VersionInfo.VMversion
            If Not String.IsNullOrEmpty(VMVersionText) Then
                If VMVersionText.StartsWith("1.3.1") Then
                    errorMsg &= My.Resources.ThisMethodIsKnownNotToWorkAgainstVersion13XOfTheJavaRuntimeEnvironment & "|||HELPTOPIC:32808"
                End If
            End If
        End If
        Return errorMsg
    End Function


    ''' <summary>
    ''' Get the text content of given element.
    ''' </summary>
    ''' <param name="c">The JABContext identifying the element.</param>
    ''' <param name="ResultingText">Carries back the text read.</param>
    ''' <param name="sErr">Carries back an error message in the event
    ''' of a failure.</param>
    ''' <returns>Returns true on success; false on failure. When
    ''' false, sErr will contain an error message.</returns>
    Public Shared Function GetText(ByVal c As JABContext, ByRef ResultingText As String, ByRef sErr As String) As Boolean
        'In some special cases we can just read off the name
        Select Case c.Role
            Case "label", "push button", "toggle button", "page tab",
             "menu", "menu item", "radio button", "check box", "panel"
                ResultingText = c.Name
                Return True
        End Select

        'Only get the text if the AccessibleContext is an AccessibleText
        If Not c.hasAccessibleText Then
            ResultingText = ""
            Return True
        End If

        'Find out how many characters there are available
        Dim info As New WAB.AccessibleTextInfo
        If Not WAB.getAccessibleTextInfo(c.vmID, c.AC, info, 0, 0) Then
            sErr = My.Resources.CouldNotRetrieveInfoAboutTheAvailableText
            Return False
        End If

        'Ask for that many characters
        Dim Length As Short = CShort(Math.Min(info.charCount, Short.MaxValue))
        If Length = 0 Then
            ResultingText = ""
        Else
            Dim Result As New System.Text.StringBuilder(Length - 1) 'For some reason if we use Length or more here we get extra chars on the end, but Length -1 seems to work.
            If Not WAB.getAccessibleTextRange(c.vmID, c.AC, 0, Length - 1, Result, Length) Then
                sErr = My.Resources.CallToGetAccessibleTextRangeWithFailed
                Return False
            End If
            ResultingText = Result.ToString
        End If

        Return True
    End Function

    ''' <summary>
    ''' Gets the selected text for the supplied jabcontext.
    ''' </summary>
    ''' <param name="c">The context whose text is to be retrieved.</param>
    ''' <param name="ResultingText">Carries back the selected text.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.
    ''' Relevant only when the return value is false.</param>
    ''' <returns>Returns true on success.</returns>
    Public Function GetSelectedText(ByVal c As JABContext, ByRef ResultingText As String, ByRef sErr As String) As Boolean
        Try

            Dim t As New WAB.AccessibleTextSelectionInfo
            If Not WAB.getAccessibleTextSelectionInfo(c.vmID, c.AC, t) Then
                sErr = My.Resources.RetrievalError
                Return False
            End If

            ResultingText = t.selectedText
            Return True
        Catch ex As Exception
            sErr = String.Format(My.Resources.UnexpectedError0, ex.Message)
            Return False
        End Try
    End Function



    ''' <summary>
    ''' Get the value of the given element. Note that this refers specifically to
    ''' a numeric or similar value type - the contents of text fields cannot be
    ''' retreived this way - use GetText instead.
    ''' </summary>
    ''' <param name="c">The JABContext identifying the element.</param>
    ''' <returns>The value</returns>
    Public Function GetValue(ByVal c As JABContext) As String
        Dim s As New StringBuilder(8192)
        WAB.getCurrentAccessibleValueFromContext(c.vmID, c.AC, s, CShort(s.Capacity + 1))
        Return s.ToString()
    End Function

End Class

