Option Strict On

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Management
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
Imports System.Threading
Imports System.Windows.Forms
Imports System.Xml

Imports AForge.Imaging.Filters
Imports AutomateControls
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ClientCommsI
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.ContainerApp
Imports BluePrism.ApplicationManager.HTML
Imports BluePrism.ApplicationManager.JAB
Imports BluePrism.ApplicationManager.UIAutomation
Imports BluePrism.ApplicationManager.WindowSpy
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.CharMatching
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models
Imports BluePrism.TerminalEmulation
Imports BluePrism.UIAutomation
Imports BluePrism.Utilities.Functional
Imports Microsoft.Win32
Imports NLog
Imports BluePrism.BrowserAutomation.WebMessages

Imports ParameterNames =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.ParameterNames
Imports SpyMode = BluePrism.ApplicationManager.WindowSpy.clsWindowSpy.SpyMode
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.BrowserAutomation
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.BrowserAutomation.Events
Imports BluePrism.BrowserAutomation.Exceptions
Imports BluePrism.Core.Extensions
Imports BluePrism.Core.Utility
Imports BluePrism.BrowserAutomation.NamedPipe
Imports Newtonsoft.Json


<CLSCompliant(False)>
Public Class clsLocalTargetApp
    Inherits clsTargetApp
    Implements ILocalTargetApp

    'These should be injected but the code isn't at that stage yet
    Private ReadOnly mAutomationFactory As IAutomationFactory =
        AutomationTypeProvider.GetType(Of IAutomationFactory)

    ''' <summary>
    ''' The automation ID helper to use for this target app
    ''' </summary>
    Friend ReadOnly Property UIAutomationIdentifierHelper As IUIAutomationIdentifierHelper =
        New UIAutomationIdentifierHelper(mAutomationFactory,
            AutomationTypeProvider.GetType(Of IAutomationHelper)) _
        Implements ILocalTargetApp.UIAutomationIdHelper


    ''' <summary>
    ''' The automation helper to use in this target app
    ''' </summary>
    Private ReadOnly mUIAutomationHelper As IAutomationHelper =
        AutomationTypeProvider.GetType(Of IAutomationHelper)

    ''' <summary>
    ''' The UI automation factory
    ''' </summary>
    Private ReadOnly mUIAutomationFactory As IAutomationFactory =
        AutomationTypeProvider.GetType(Of IAutomationFactory)

    Friend WithEvents MyBrowserAutomationIdentifierHelper As IBrowserAutomationIdentifierHelper

    Friend WithEvents NamedPipeWrapper As INamedPipeWrapper = DependencyResolver.Resolve(Of INamedPipeWrapper)

    ''' <summary>
    ''' The command handler factory to use for this target app
    ''' </summary>
    Private Shared CommandHandlerFactory As IHandlerFactory =
        ApplicationFactoryInitialiser.Initialise()

    ''' <summary>
    ''' The mouse operations provider
    ''' </summary>
    Private ReadOnly mMouseOperationsProvider As IMouseOperationsProvider =
        New MouseOperationsProvider()

    ''' <summary>
    ''' The window operations provider
    ''' </summary>
    Private ReadOnly mWindowOperationsProvider As IWindowOperationsProvider =
        New WindowOperationsProvider()

    ''' <summary>
    ''' The keyboard operations provider
    ''' </summary>
    Private ReadOnly mKeyboardOperationsProvider As IKeyboardOperationsProvider =
        New KeyboardOperationsProvider()

    ''' <summary>
    ''' The UI automation identifier helper
    ''' </summary>
    Private ReadOnly mUIAutomationIdentifierHelper As IUIAutomationIdentifierHelper =
        New UIAutomationIdentifierHelper(mUIAutomationFactory, mUIAutomationHelper)

    ''' <summary>
    ''' The time, in seconds, we wait for a web page to be created
    ''' </summary>
    Private mWebPageCreationTimeout As Integer = 60

    ''' <summary>
    '''  The address for the browser help
    ''' </summary>
    Private Const BrowserHelpAddresss As String = "Guides/chrome-firefox/chrome-firefox.htm"

    ''' <summary>
    ''' Marks if the web page is created
    ''' </summary>
    Private mWebPageCreated As Boolean

    ''' <summary>
    ''' The reported browser extension version
    ''' </summary>
    Private mWebPageExtensionVersion As Version

    Private Shared ReadOnly NlogLogger As Logger = LogManager.GetCurrentClassLogger()

    Private Const ocrPlusExe As String = "OcrPlus\bpocrpp.exe"
    Private Shared ocrProcess As Process = Nothing


    ''' <summary>
    ''' This is actualy a proxy for the ControlProxy found in ManagedSpyLib. We
    ''' have a proxy here so we can dynamically load different versions of that
    ''' assembly.
    ''' </summary>
    Friend Class ControlProxy

        Private Shared mAssembly As Assembly = Nothing
        Private Shared mProxyType As Type

        Private mControlProxy As Object

        Public Shared Function FromHandle(h As IntPtr) As ControlProxy

            'We need to load the ManagedSpyLib assembly if we haven't already done
            'so.
            'TODO: Here is where we should select the correct one for the .NET
            'version of the target application (I'm assuming we can somehow
            'detect that!)
            If mAssembly Is Nothing Then

                'First detect what version of the framework the target application is
                'running, so we can try and select an appropriate ManagedSpyLib.
                Dim usev2 As Boolean = False
                Dim procid As Integer
                modWin32.GetWindowThreadProcessId(h, procid)
                Dim proc As Process = Process.GetProcessById(procid)
                For Each m As ProcessModule In proc.Modules
                    If m.ModuleName.StartsWith("mscor") Then
                        If m.FileVersionInfo.FileMajorPart <= 2 Then
                            usev2 = True
                            Exit For
                        End If
                    End If
                Next

                Dim basePath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                Dim dll As String
                If usev2 Then
                    dll = "ManagedSpyLib.dll"
                Else
                    dll = "ManagedSpyLibNet4.dll"
                End If
                mAssembly = Assembly.LoadFrom(Path.Combine(basePath, dll))
                mProxyType = mAssembly.GetType("Microsoft.ManagedSpy.ControlProxy")
            End If

            Dim cp As New ControlProxy()
            cp.mControlProxy = mProxyType.InvokeMember("FromHandle", BindingFlags.InvokeMethod Or BindingFlags.ExactBinding Or BindingFlags.Static Or BindingFlags.Public, Nothing, Nothing, New Object() {h})
            Return cp
        End Function

        Public Sub FireEvent(ByVal ev As String)
            mProxyType.InvokeMember("FireEvent", BindingFlags.InvokeMethod, Nothing, mControlProxy, New Object() {ev})
        End Sub

        Public Function GetValue(ByVal propname As String) As Object
            Return mProxyType.InvokeMember("GetValue", BindingFlags.InvokeMethod, Nothing, mControlProxy, New Object() {propname})
        End Function

        Public Sub SetValue(ByVal propname As String, ByVal value As Object)
            mProxyType.InvokeMember("SetValue", BindingFlags.InvokeMethod, Nothing, mControlProxy, New Object() {propname, value})
        End Sub

        Public Function GetClassName() As String
            Return CStr(mProxyType.InvokeMember("GetClassName", BindingFlags.InvokeMethod, Nothing, mControlProxy, Nothing))
        End Function

    End Class


    ''' <summary>
    ''' Timer used to keep the model updated.
    ''' </summary>
    Private WithEvents mTimer As System.Timers.Timer


    ''' <summary>
    ''' Get the PID of the target application currently connected.
    ''' </summary>
    Public Overrides ReadOnly Property PID() As Integer Implements ILocalTargetApp.PID
        Get
            Return mPID
        End Get
    End Property
    Private mPID As Integer

    ''' <summary>
    ''' The top-level SAP GUI object for the target application, or Nothing if we
    ''' don't have one. The reference to this object is found on a 'lazy' basis.
    ''' </summary>
    Private mSAPGuiApplication As Object = Nothing

    ''' <summary>
    ''' Determine if SAP Gui API connectivity is available on this application.
    ''' </summary>
    Public ReadOnly Property SAPAvailable() As Boolean Implements ITargetApp.SAPAvailable
        Get
            Try
                GetSapApp()
                Return mSAPGuiApplication IsNot Nothing
            Catch
                Return False
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Get a reference to the SAP GUI application, if possible. This may do nothing
    ''' if the reference has already been found. Used internally by any method that
    ''' references the SAP GUI application, to implement the 'lazy' initialisation
    ''' of that connection.
    ''' </summary>
    ''' <returns>True if there is a usable SAP GUI reference.</returns>
    <DebuggerStepThrough()>
    Private Function GetSapApp() As Boolean
        If mSAPGuiApplication IsNot Nothing Then Return True
        Dim auto As Object = Nothing
        Try
            auto = GetObject("SAPGUI")
        Catch ex As Exception
            'Don't care about exceptions in the above - it is most likely just a case
            'of "Cannot create ActiveX object" which means there is not a usable SAP
            'GUI reference!
        End Try
        If auto Is Nothing Then Return False
        Try
            Dim autotype As Type = auto.GetType()
            mSAPGuiApplication = autotype.InvokeMember("GetScriptingEngine", BindingFlags.InvokeMethod, Nothing, auto, Nothing, Nothing, Nothing, Nothing)
            Return mSAPGuiApplication IsNot Nothing
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionOccurredDuringGetSAPApp & ex.ToString())
        End Try
    End Function

    ''' <summary>
    ''' Find the SAP Gui component at the given screen position.
    ''' </summary>
    ''' <param name="pt">The Point specifying the screen coordinates to look at.</param>
    ''' <returns>The ID of the component found, or Nothing if there is no component
    ''' at the given coordinates, either because there isn't one there or because
    ''' there is no SAP connectivity at all!</returns>
    Function GetSapComponentFromPoint(ByVal pt As Point) As String Implements ITargetApp.GetSapComponentFromPoint
        GetSapApp()
        If mSAPGuiApplication Is Nothing Then Return Nothing

        'Prepare arguments for the call to GuiSession.findByPosition...
        Dim findargs(2) As Object
        findargs(0) = pt.X
        findargs(1) = pt.Y
        findargs(2) = False

        'Get a list of connections...
        Dim connections As Object = mSAPGuiApplication.GetType().InvokeMember("children", BindingFlags.GetProperty, Nothing, mSAPGuiApplication, Nothing)
        Dim concount As Integer = CInt(connections.GetType().InvokeMember("count", BindingFlags.GetProperty, Nothing, connections, Nothing))
        For con As Integer = 0 To concount - 1
            Dim args(0) As Object
            args(0) = con
            Dim thiscon As Object = connections.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, connections, args)
            'Get a list of the sessions within this connection...
            Dim sessions As Object = thiscon.GetType().InvokeMember("children", BindingFlags.GetProperty, Nothing, thiscon, Nothing)
            Dim sescount As Integer = CInt(sessions.GetType().InvokeMember("count", BindingFlags.GetProperty, Nothing, sessions, Nothing))
            For ses As Integer = 0 To sescount - 1
                args(0) = ses
                Dim session As Object = sessions.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, sessions, args)
                Dim found As Object = session.GetType().InvokeMember("findByPosition", BindingFlags.GetProperty, Nothing, session, findargs)
                If found IsNot Nothing Then
                    'Found something in this session. We have a GuiCollection where the first
                    'item is the ID of the found item, and the second is some additional information
                    'about the 'inner' object. This may be useful later for grids, but for now we
                    'are ignoring it.
                    args(0) = 0
                    Dim id As String = CStr(found.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, found, args))
                    Return id
                End If
            Next
        Next

        Return Nothing

    End Function

    ''' <summary>
    ''' Get the screen bounds occupied by the specified SAP Gui component.
    ''' </summary>
    ''' <param name="id">The ID of the component of interest.</param>
    ''' <returns>A Rectangle containing the screen bounds of the component.</returns>
    ''' <remarks>Throws an Exception if anything goes wrong - for example if there
    ''' is no SAP connectivity, or if the requested component does not exist.</remarks>
    Function GetSapComponentScreenRect(ByVal id As String) As Rectangle Implements ITargetApp.GetSapComponentScreenRect
        GetSapApp()
        If mSAPGuiApplication Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPScreenRectUnexpectedFailure)

        Dim args(1) As Object
        args(0) = id
        args(1) = False
        Dim el As Object = mSAPGuiApplication.GetType().InvokeMember("findByID", BindingFlags.InvokeMethod, Nothing, mSAPGuiApplication, args)
        If el Is Nothing Then Throw New InvalidOperationException(String.Format(My.Resources.CanTGetSAPScreenRectFor0ElementNotFound, id))

        Dim x As Integer = CInt(el.GetType().InvokeMember("ScreenLeft", BindingFlags.GetProperty, Nothing, el, Nothing))
        Dim y As Integer = CInt(el.GetType().InvokeMember("ScreenTop", BindingFlags.GetProperty, Nothing, el, Nothing))
        Dim w As Integer = CInt(el.GetType().InvokeMember("Width", BindingFlags.GetProperty, Nothing, el, Nothing))
        Dim h As Integer = CInt(el.GetType().InvokeMember("Height", BindingFlags.GetProperty, Nothing, el, Nothing))

        Dim r As New Rectangle(x, y, w, h)
        Return r

    End Function

    ''' <summary>
    ''' Get the bounds occupied by the specified SAP Gui component.
    ''' </summary>
    ''' <param name="id">The ID of the component of interest.</param>
    ''' <returns>A Rectangle containing the bounds of the component.</returns>
    ''' <remarks>Throws an Exception if anything goes wrong - for example if there
    ''' is no SAP connectivity, or if the requested component does not exist.</remarks>
    Function GetSapComponentRect(ByVal id As String) As Rectangle
        GetSapApp()
        If mSAPGuiApplication Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPRectUnexpectedFailure)

        Dim args(1) As Object
        args(0) = id
        args(1) = False
        Dim el As Object = mSAPGuiApplication.GetType().InvokeMember("findByID", BindingFlags.InvokeMethod, Nothing, mSAPGuiApplication, args)
        If el Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPRectElementNotFound)

        Dim x As Integer = CInt(el.GetType().InvokeMember("Left", BindingFlags.GetProperty, Nothing, el, Nothing))
        Dim y As Integer = CInt(el.GetType().InvokeMember("Top", BindingFlags.GetProperty, Nothing, el, Nothing))
        Dim w As Integer = CInt(el.GetType().InvokeMember("Width", BindingFlags.GetProperty, Nothing, el, Nothing))
        Dim h As Integer = CInt(el.GetType().InvokeMember("Height", BindingFlags.GetProperty, Nothing, el, Nothing))

        Dim r As New Rectangle(x, y, w, h)
        Return r

    End Function

    ''' <summary>
    ''' Get the type of the specified SAP Gui component.
    ''' </summary>
    ''' <param name="id">The ID of the component of interest.</param>
    ''' <returns>The type of the component. This corresponds to the class name of the
    ''' component, as per the documentation - for example "GuiTextField".</returns>
    ''' <remarks>Throws an Exception if anything goes wrong - for example if there
    ''' is no SAP connectivity, or if the requested component does not exist.</remarks>
    Function GetSapComponentType(ByVal id As String) As String
        GetSapApp()
        If mSAPGuiApplication Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPComponentTypeUnexpectedFailure)

        Dim args(1) As Object
        args(0) = id
        args(1) = False
        Dim el As Object = mSAPGuiApplication.GetType().InvokeMember("findByID", BindingFlags.InvokeMethod, Nothing, mSAPGuiApplication, args)
        If el Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPComponentTypeElementNotFound)

        Return CStr(el.GetType().InvokeMember("type", BindingFlags.GetProperty, Nothing, el, Nothing))

    End Function

    ''' <summary>
    ''' Get the SubType property of the specified SAP Gui component.
    ''' </summary>
    ''' <param name="id">The ID of the component of interest.</param>
    ''' <returns>The subtype of the component, or Nothing if it doesn't have one.</returns>
    ''' <remarks>Throws an Exception if anything goes wrong - for example if there
    ''' is no SAP connectivity, or if the requested component does not exist.</remarks>
    Function GetSapSubType(ByVal id As String) As String
        GetSapApp()
        If mSAPGuiApplication Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPComponentTypeUnexpectedFailure)

        Dim args(1) As Object
        args(0) = id
        args(1) = False
        Dim el As Object = mSAPGuiApplication.GetType().InvokeMember("findByID", BindingFlags.InvokeMethod, Nothing, mSAPGuiApplication, args)
        If el Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTGetSAPComponentTypeElementNotFound)

        Try
            Return CStr(el.GetType().InvokeMember("subType", BindingFlags.GetProperty, Nothing, el, Nothing))
        Catch ex As Exception
            Return Nothing
        End Try

    End Function


    ''' <summary>
    ''' Used by GetHTMLDocuments() to cache HTML documents created from IHTMLDocument
    ''' interfaces marshalled from the target process via hooking. The Dictionary is
    ''' keyed on the ID returned by the injection agent.
    ''' </summary>
    Private mCachedHookedHTMLDocuments As New Dictionary(Of String, clsHTMLDocument)

    ''' <summary>
    ''' Calls <see cref="SendKeys.SendWait"/> sending the keys defined in the
    ''' <see cref="ParameterNames.NewText">newtext</see> argument in the
    ''' <paramref name="q"/> to the current foreground application. If an
    ''' <see cref="ParameterNames.Interval">interval</see> is specified in the query
    ''' the keys are split and sent independently, waiting for the interval number of
    ''' seconds. Note that control characters cannot be sent if a non-zero interval
    ''' is provided.
    ''' </summary>
    ''' <param name="q">The query containing the arguments determining how to send
    ''' the keys to the foreground application.</param>
    ''' <exception cref="InvalidValueException">If a non-zero interval value was
    ''' provided, but the text to send contained control characters.</exception>
    Private Sub Win32SendKeys(q As clsQuery)
        KeyHelper.SendKeysFromQuery(q, mKeyboardOperationsProvider)
    End Sub

    ''' <summary>
    ''' Flag values for the lParam value used in WM_SYSKEYUP and WM_SYSKEYDOWN
    ''' messages. See:
    ''' https://msdn.microsoft.com/en-us/library/windows/desktop/ms646286%28v=vs.85%29.aspx
    ''' and
    ''' https://msdn.microsoft.com/en-us/library/windows/desktop/ms646287%28v=vs.85%29.aspx
    ''' for the definitions used here.
    ''' </summary>
    <Flags>
    Private Enum SysKeyFlags As Integer
        ' Indicates whether the key is an extended key, such as the right-hand ALT
        ' and CTRL keys that appear on an enhanced 101- or 102-key keyboard. The
        ' value Is 1 If it Is an extended key; otherwise, it Is 0.
        ExtendedKey = 1 << 24

        ' The context code. The value is 1 if the ALT key is down while the key is
        ' pressed; it is 0 if the WM_SYSKEYDOWN message is posted to the active
        ' window because no window has the keyboard focus.
        ContextCode = 1 << 29

        ' The previous key state. The value is 1 if the key is down before the
        ' message is sent, or it is 0 if the key is up.
        PreviousKeyState = 1 << 30

        ' The transition state. The value is always 0 for a WM_SYSKEYDOWN message.
        TransitionState = 1 << 31

        ' Flags to indicate that Alt is held down
        AltDown = 1 + ContextCode

        ' Flags to indicate that that Alt is released
        AltUp = 1 + ContextCode + PreviousKeyState + TransitionState
    End Enum

    ''' <summary>
    ''' This sends keypresses to a specified Win32 control using the WM_CHAR message
    ''' rather than a SendKeys call. It can be called with or without Alt being held
    ''' down when the chars are sent, and there is an optional interval between
    ''' messages.
    ''' </summary>
    ''' <param name="q">The query identifying the control, the text to be sent and
    ''' the optional interval in between char messages.</param>
    Private Sub Win32SendWMChars(q As clsQuery, withAlt As Boolean)
        Dim interval =
            TimeSpan.FromSeconds(q.GetDecimalParam(ParameterNames.Interval))
        Dim txt = q.GetParameter(ParameterNames.NewText)

        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)

        If Not withAlt Then
            For Each c As Char In txt
                SendMessage(w.Hwnd, WindowMessages.WM_CHAR, AscW(c), 0)
                If interval > Nothing Then Thread.Sleep(interval)
            Next

        Else
            ' Alt (Menu) Down
            PostMessage(w.Hwnd, WindowMessages.WM_SYSKEYDOWN, VirtualKeyCode.VK_MENU,
                SysKeyFlags.AltDown)

            ' Go through each character and send it individually with Alt down
            For Each c As Char In txt
                PostMessage(w.Hwnd, WindowMessages.WM_SYSKEYDOWN, AscW(c),
                 SysKeyFlags.AltDown)
                PostMessage(w.Hwnd, WindowMessages.WM_SYSKEYUP, AscW(c),
                 SysKeyFlags.AltUp)
                If interval > Nothing Then Thread.Sleep(interval)
            Next

            ' Finally, release the Alt (Menu) key
            PostMessage(w.Hwnd, WindowMessages.WM_SYSKEYUP, VirtualKeyCode.VK_MENU,
             SysKeyFlags.AltUp)

        End If

    End Sub

    ''' <summary>
    ''' Get a list of all IHTMLDocument interfaces exposed by the target application.
    ''' </summary>
    ''' <returns>A List containing the 0 or more clsHTMLDocument instances that
    ''' represent the interfaces found.
    ''' </returns>
    Public Function GetHtmlDocuments() As List(Of clsHTMLDocument) _
        Implements ITargetApp.GetHtmlDocuments
        Return GetHtmlDocuments(mActiveTabOnly)
    End Function

    ''' <summary>
    ''' Get a list of all IHTMLDocument interfaces exposed by the target application.
    ''' </summary>
    ''' <param name="ActiveTabOnly">
    ''' Use this to override the application's ActiveTabOnly setting
    ''' </param>
    ''' <returns>A List containing the 0 or more clsHTMLDocument instances that
    ''' represent the interfaces found.
    ''' </returns>
    Private Function GetHtmlDocuments(ActiveTabOnly As Boolean) _
        As List(Of clsHTMLDocument)

        Dim docs As New List(Of clsHTMLDocument)
        Try
            If mHookClient Is Nothing OrElse Not clsConfig.UseMarshaledHTML Then
                'Get the list by the usual means
                clsConfig.LogHTML("Getting HTML documents using PID")
                docs = clsHTMLDocument.GetDocumentsFromPID(mPID, ActiveTabOnly)
            Else
                docs = GetMarshaledHtmlDocuments()
            End If
        Catch ex As Exception
            clsConfig.Log("Exception occured during GetHTMLDocuments - {0} - {1}",
                          ex.Message, ex.StackTrace)
        End Try

        clsConfig.LogHTML("Got " & docs.Count() & " documents")

        Return docs
    End Function

    ''' <summary>
    ''' Get a list of marshaled IHTMLDocument interfaces exposed by the target 
    ''' application.
    ''' </summary>
    Private Function GetMarshaledHtmlDocuments() As List(Of clsHTMLDocument)
        Dim docs As New List(Of clsHTMLDocument)
        Dim list As String = ProcessQuery("gethtmldocuments")
        If list.StartsWith("RESULT:Documents:") Then
            list = list.Substring(17)
            clsConfig.LogHook("GetHTMLDocuments using list of hooked documents:" & list)
            If list.Length > 0 Then
                Dim comdocs() As String = list.Split(","c)
                For Each thisdoc As String In comdocs
                    Try
                        If mCachedHookedHTMLDocuments.ContainsKey(thisdoc) Then
                            'We have this interface in the cache already, so let's
                            'use it!
                            docs.Add(mCachedHookedHTMLDocuments(thisdoc))
                            clsConfig.LogHook("Used clsHTMLDocument from cache")
                        Else
                            Dim res As String = ProcessQuery("marshalhtmldocument newtext=" & thisdoc)
                            If res.StartsWith("RESULT:") Then
                                res = res.Substring(7)

                                Dim iNumBytes As Integer = (res.Length \ 2)
                                Dim hGlobal As IntPtr = Marshal.AllocHGlobal(iNumBytes)
                                Dim stream As System.Runtime.InteropServices.ComTypes.IStream = Nothing
                                CreateStreamOnHGlobal(hGlobal, False, stream)

                                Dim bytes(iNumBytes - 1) As Byte
                                Dim j As Integer = 0
                                For i As Integer = 0 To iNumBytes - 1
                                    Dim num As String = res(j) & res(j + 1)
                                    bytes(i) = CByte(Integer.Parse(num, NumberStyles.HexNumber))
                                    j += 2
                                Next

                                stream.Write(bytes, iNumBytes, Nothing)
                                stream.Commit(0)
                                stream.Seek(0, 0, IntPtr.Zero)

                                Dim iidIhtmlDocument As New Guid("626FC520-A41E-11CF-A731-00A0C9082637")
                                Dim ppv As Object = Nothing
                                CoUnmarshalInterface(stream, iidIhtmlDocument, ppv)

                                Marshal.FreeHGlobal(hGlobal)

                                If Not ppv Is Nothing Then
                                    Dim h As New clsHTMLDocument(ppv)
                                    docs.Add(h)
                                    clsConfig.LogHook("HTML document unmarshaled successfully")
                                    'Add it to the cache...
                                    mCachedHookedHTMLDocuments.Add(thisdoc, h)
                                Else
                                    clsConfig.LogHook("Unmarshaling HTML document returned null object")
                                End If
                            Else
                                clsConfig.LogHook("Marshaling HTML document failed: " & res)
                            End If
                        End If
                    Catch ex As Exception
                        clsConfig.LogHook("Exception occurred while getting HTML document - " & ex.Message)
                    End Try
                Next
                'Now, remove from our cache any documents that were not returned
                'in the list, because if they don't exist in the target application
                'any more we don't want to be holding on to them here...
                For Each thisdoc As String In mCachedHookedHTMLDocuments.Keys
                    Dim found As Boolean = False
                    For Each thisdoc1 As String In comdocs
                        If thisdoc = thisdoc1 Then
                            found = True
                        End If
                    Next
                    If Not found Then mCachedHookedHTMLDocuments.Remove(thisdoc)
                Next
            Else
                'The list returned was empty, so completely clear out our cache.
                mCachedHookedHTMLDocuments.Clear()
            End If

        End If
        Return docs
    End Function


    ''' <summary>
    ''' Set this to True to make JAB actions run on a seperate thread, as for MSAA
    ''' actions, which gets around the modal dialog problem, but currently causes
    ''' crashes within the Java Access Bridge itself.
    ''' </summary>
    Private Const mbJABActionsMultithreaded As Boolean = False

    ''' <summary>
    ''' RunThread is used internally as a helper in order to pass
    ''' information to the real thread handler
    ''' </summary>
    Private Class RunThread

        Delegate Sub Start(ByVal o As Object)

        Private Class Args
            Public o As Object
            Public s As Start
            Public Sub Work()
                s(o)
            End Sub
        End Class

        Public Shared Function CreateThread(ByVal s As Start, ByVal arg As Object) As Thread
            Dim a As New Args
            a.o = arg
            a.s = s
            Dim t As New Thread(AddressOf a.Work)
            t.SetApartmentState(ApartmentState.STA)
            Return t
        End Function
    End Class

    ''' <inheritdoc />
    Public ReadOnly Property Model() As clsUIModel Implements ILocalTargetApp.Model
        Get
            Return mobjModel
        End Get
    End Property

    Private WithEvents mobjModel As clsUIModel

    Private WithEvents mHookClient As clsHookClient
    Private WithEvents mTerminalApp As Terminal

    ''' <summary>
    ''' Boolean indicating whether to exclude traversal of HTC documents within the 
    ''' model. Used for HTML Applcaiton types.
    ''' </summary>
    Public Property ExcludeHTC() As Boolean
        Get
            Return mExcludeHTC
        End Get
        Set(ByVal value As Boolean)
            mExcludeHTC = value
        End Set
    End Property
    Private mExcludeHTC As Boolean = False

    ''' <summary>
    ''' Boolean indicating whether to only get HTML documents on the active 
    ''' tab or to also get HTML documents on tabs that are not currently active. 
    ''' Setting this to false may lead to inconsistent behaviour (and errors), 
    ''' because IE keeps hold of old tabs for a small period of time as invisible
    ''' windows which could be returned if set to false.
    ''' </summary>
    Private mActiveTabOnly As Boolean = False

    ''' <summary>
    ''' When the application has been launched (mLaunched=True) this contains a
    ''' comma-separated list of any options that were requested. It is undefined
    ''' (may be Nothing) when the application is not running.
    ''' </summary>
    Private mOptions As ISet(Of String)

    ''' <summary>
    ''' The application options set in this target application object.
    ''' </summary>
    Public ReadOnly Property Options As ISet(Of String)
        Get
            ' Create a case-insensitive hash set - use ordinal ignore case since
            ' these are effectively 'keywords' and should not be subject to culture
            ' differences
            If mOptions Is Nothing Then _
             mOptions = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            Return mOptions
        End Get
    End Property

    ''' <summary>
    ''' Sets the options in this object to the given value. The options are expected
    ''' to be comma-separated and are case-insensitive. Empty values are stripped
    ''' before being entered into the options set in this object.
    ''' </summary>
    ''' <param name="opts">The comma-separated options to set in this target app
    ''' instance. Empty options are ignored.</param>
    Private Sub SetOptions(opts As String)
        With Options
            .Clear()
            .UnionWith(
                 opts.Split(","c).
                    Select(Function(s) s.Trim()).
                    Where(Function(s) s.Length > 0)
            )
        End With
    End Sub

    ''' <summary>
    ''' Check if the given option is set.
    ''' </summary>
    ''' <param name="opt">The option to check.</param>
    ''' <returns>True if the option is set, False otherwise.</returns>
    Public ReadOnly Property OptionSet(ByVal opt As String) As Boolean
        Get
            Return Options.Contains(opt)
        End Get
    End Property


    'When the target application is running (bLaunched=True) this is the JABWrapper
    'instance in use, or Nothing if JAB support is not available.
    Private mJAB As JABWrapper
    Private WithEvents mCitrix As Citrix.ctlClient

    Private msInjectorFolder As String
    Private WithEvents mProcess As clsProcessWatcher

    ''' <summary>
    ''' Indicates whether any spy operation that is currently underway has since been
    ''' cancelled by a subsequent call.
    ''' </summary>
    Private mSpyCancelled As Boolean

    Private ReadOnly Property ProcessHandle() As IntPtr
        Get
            If mProcess Is Nothing Then Return IntPtr.Zero
            Return mProcess.Handle
        End Get
    End Property


    ''' <summary>
    ''' Determines if the target application is a terminal app.
    ''' </summary>
    ''' <value>True if it is, False otherwise (including if not connected at all!)
    ''' </value>
    Public ReadOnly Property IsTerminalApp() As Boolean
        Get
            Return (mTerminalApp IsNot Nothing)
        End Get
    End Property

    ''' <summary>
    ''' Determines if the target application is a modern (container) app
    ''' </summary>
    ''' <value>True if it is, otherwise False</value>
    Public Property IsModernApp() As Boolean = False

    Public Sub New()
        mobjModel = New clsUIModel(Me)
        Connected = False
        mHookClient = Nothing
        mTerminalApp = Nothing
        mProcess = Nothing
        msInjectorFolder = IO.Path.GetDirectoryName(Me.GetType.Assembly.Location)
        mWindowInformation = New Dictionary(Of IntPtr, WindowInformation)
        'Set up the timer...
        mTimer = New System.Timers.Timer(2000)
    End Sub

    Private Sub TimerHandler(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles mTimer.Elapsed
        UpdateModel()
    End Sub


    ''' <summary>
    ''' Keyed by window handle, gives back window information.
    ''' </summary>
    ''' <remarks>Used to restore windows back to their previous
    ''' known state after hiding them and unhiding for example.
    ''' This process involves altering the window style, which we
    ''' must be careful to restore.</remarks>
    Private mWindowInformation As Dictionary(Of IntPtr, WindowInformation)

    Private Structure WindowInformation
        Public StyleEx As Integer
        Public WindowScreenBounds As RECT
    End Structure

    ''' <summary>
    ''' Dummy object used for locking during thread management.
    ''' </summary>
    ''' <remarks>
    ''' A lock must be placed on this object when performing any operation which
    ''' changes the nature of the connection to the target application
    ''' (eg connect or disconnect).
    ''' 
    ''' Realistically, this is only internal threads which we are worried
    ''' about.
    ''' </remarks>
    Private mApplicationStateLock As New Object

    ''' <summary>
    ''' Disconnects from the target application in a thread-safe manner, if already
    ''' connected.
    ''' </summary>
    ''' <remarks>After a call to disconnect, the target application will not be
    ''' available for interaction unless action is taken to reconnect to the already
    ''' running instance (via attaching for example).
    ''' </remarks>
    Public Sub Disconnect()
        SyncLock mApplicationStateLock
            Dim changed As Boolean = False

            If mHookClient IsNot Nothing Then
                mHookClient.Disconnect()
                mHookClient = Nothing
                changed = True
            End If
            If mTerminalApp IsNot Nothing Then
                mTerminalApp = Nothing
                changed = True
            End If
            If mJAB IsNot Nothing Then
                mJAB.Shutdown()
                mJAB = Nothing
                changed = True
            End If
            If mProcess IsNot Nothing Then
                mProcess.Dispose()
                mProcess = Nothing
                changed = True
            End If

            Connected = False
            If changed Then OnDisconnected()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Do a spying operation.
    ''' </summary>
    ''' <param name="initialmode">The initial spy mode.</param>
    ''' <returns>The response message - see the documentation of the 'spy' query for
    ''' details of the format and possible values.</returns>
    Private Function DoSpy(initialmode As SpyMode) As Reply
        Return DoSpy(initialmode, OptionSet("includewin32"), OptionSet("restrictedwin32spy"))
    End Function

    ''' <summary>
    ''' Do a spying operation.
    ''' </summary>
    ''' <param name="initialmode">The initial spy mode.</param>
    ''' <param name="includeWin32">True to include Win32 attributes in the spy
    ''' operation. This is currently only honoured for UIAutomation spy operations.
    ''' </param>
    ''' <returns>The response message - see the documentation of the 'spy' query for
    ''' details of the format and possible values.</returns>
    Private Function DoSpy(
     initialmode As SpyMode,
     includeWin32 As Boolean,
     restrictedWin32Spying As Boolean) As Reply

        'Special case handler for terminal app...
        If Not mTerminalApp Is Nothing Then
            Dim field As clsTerminalField = Nothing
            If mTerminalApp.DoSpy(field, Me) Then
                Return Reply.TerminalField(
                 "+StartX=" & field.StartColumn.ToString() & " " &
                 "+StartY=" & field.StartRow.ToString() & " " &
                 "+EndX=" & field.EndColumn.ToString() & " " &
                 "+EndY=" & field.EndRow.ToString() & " " &
                 "+FieldType=" & field.FieldType.ToString() & " " &
                 "FieldText=" & clsQuery.EncodeValue(field.FieldText))
            Else
                Return Reply.Cancel
            End If
        End If

        'Determine what spy modes are available...
        Dim allowedmodes As SpyMode
        If initialmode = SpyMode.Bitmap Then
            allowedmodes = initialmode
        ElseIf initialmode = SpyMode.Web Then
            allowedmodes = SpyMode.WebDefaults
        Else
            allowedmodes = SpyMode.BasicDefaults
            allowedmodes = allowedmodes Or SpyMode.Html
            If Not mJAB Is Nothing Then allowedmodes = allowedmodes Or SpyMode.Java
        End If

        Using spy As New clsWindowSpy(initialmode, allowedmodes, restrictedWin32Spying, mJAB) With {
         .IncludeWin32 = includeWin32
        }

            mSpyCancelled = False
            spy.StartSpy(Me)

            Do
                BlockingMessagePump()
                If mSpyCancelled Then spy.CancelSpy()
            Loop Until spy.Ended

            If spy.Failed Then Throw New InvalidOperationException(spy.ExceptionMessage)

            'Build a list of the values we want to return.
            Dim values As New List(Of String)

            If spy.WindowChosen Then
                Dim winfo As clsWindowSpy.WindowInformation = spy.CurrentWindow
                Dim sb As New StringBuilder(1024)

                AppendWindowIdentifiers(winfo.WindowHandle, sb)

                sb.Append(" MatchIndex=1 MatchReverse=True")
                ' In Win32Region mode, both the window info + bitmaps are required.
                If spy.BitmapChosen Then sb.AppendFormat(" Screenshot={0}", spy.CapturedPixRect)

                Return Reply.Window(sb.ToString())

            ElseIf spy.BitmapChosen Then
                Return Reply.Bitmap(spy.CapturedPixRect.ToString())

            ElseIf spy.SAPChosen Then
                Dim rv As String = "+ID=" &
                clsQuery.EncodeValue(spy.SAPComponent) & " " &
                "ComponentType=" & clsQuery.EncodeValue(GetSapComponentType(spy.SAPComponent))
                Dim st As String = GetSapSubType(spy.SAPComponent)
                If st IsNot Nothing Then
                    rv &= " SubType=" & clsQuery.EncodeValue(st)
                End If
                Return Reply.Sap(rv)

            ElseIf spy.AAElementChosen Then
                Dim a As Accessibility.IAccessible = spy.AAElement
                Dim childId As Object = spy.AAElementID
                Dim aaElem As New clsAAElement(a, childId)

                ' 1280 - roughly all static strings to add + a (tiny) bit of space to
                ' kick off dynamic extending
                Dim sb As New StringBuilder(1280)
                GetUpdatedWindowModel(spy.CurrentWindow.WindowHandle).AppendIdentifiers(sb)
                sb.Append(" "c)
                aaElem.AppendIdentifiers(sb)
                sb.Append(" MatchIndex=1 MatchReverse=True")
                Return Reply.AAElement(sb.ToString())

            ElseIf spy.UIAElementChosen Then
                Dim e = spy.UIAElement
                ' 1280 - roughly all static strings to add + a (tiny) bit of space to
                ' kick off dynamic extending
                Dim sb As New StringBuilder(1280)
                If includeWin32 Then
                    GetUpdatedWindowModel(
                     spy.CurrentWindow.WindowHandle).AppendIdentifiers(sb)
                    sb.Append(" "c)
                End If
                UIAutomationIdentifierHelper.AppendIdentifiers(e, sb)
                Return Reply.UIAElement(sb.ToString())

            ElseIf spy.WebElementChosen Then
                Dim element = spy.WebElement

                Dim identifiers = ""
                If MyBrowserAutomationIdentifierHelper IsNot Nothing Then
                    identifiers = String.Join(" ", MyBrowserAutomationIdentifierHelper.GetIdentifiers(element))
                End If

                identifiers &= " MatchIndex=1"

                Return Reply.WebElement(identifiers)

            ElseIf spy.HTMLElementChosen Then
                Dim e As clsHTMLElement = spy.HTMLElement
                Return Reply.Html(e.GetIdentifiers())

            ElseIf spy.JABElementChosen Then
                Using el As JABContext = spy.JABElement
                    Dim sb As New StringBuilder(1024)

                    el.AppendIdentifiers(sb)
                    sb.Append(" MatchIndex=1 MatchReverse=True")
                    Return Reply.Jab(sb.ToString())
                End Using

            Else
                Return Reply.Cancel

            End If
        End Using
    End Function


    ''' <summary>
    ''' Gets the window model object representing the window with a specified handle
    ''' within our application model
    ''' </summary>
    ''' <param name="hwnd">The window handle for which the window model is required.
    ''' </param>
    ''' <returns>The window model within our application model which corresponds to
    ''' the given window handle</returns>
    ''' <exception cref="NoSuchElementException">If no window with the given handle
    ''' could be found in the application model</exception>
    Private Function GetUpdatedWindowModel(ByVal hwnd As IntPtr) As clsUIWindow
        UpdateModel()
        Dim ew As clsUIWindow = mobjModel.FindWindowByHandle(hwnd)
        If ew Is Nothing Then Throw New NoSuchElementException(My.Resources.TheWindowSpiedWasNotFoundInTheModel)
        Return ew
    End Function

    ''' <summary>
    ''' Appends the window identifiers for the given window, in the format returned
    ''' from a spy query.
    ''' </summary>
    ''' <param name="handle">The window handle.</param>
    ''' <param name="sb">The buffer to which the identifiers should be appended
    ''' </param>
    ''' <returns>The given buffer with the formatted identifier information appended
    ''' to it.</returns>
    ''' <exception cref="NoSuchElementException">If the window associated with the
    ''' given handle could not be found in the application model</exception>
    Private Function AppendWindowIdentifiers(ByVal handle As IntPtr, ByVal sb As StringBuilder) As StringBuilder
        Return GetUpdatedWindowModel(handle).AppendIdentifiers(sb)
    End Function

    ''' <summary>
    ''' Get the window identifiers for the given window, in the format returned from
    ''' a spy query.
    ''' </summary>
    ''' <param name="handle">The window handle.</param>
    ''' <returns>The formatted identifier information.</returns>
    Private Function GetWindowIdentifiers(ByVal handle As IntPtr) As String
        Return GetUpdatedWindowModel(handle).GetIdentifiers()
    End Function

    ''' <summary>
    ''' Intitialise JAB access if required. Used internally when launching or
    ''' attaching to an application
    ''' </summary>
    ''' <param name="bUseJAB">True if we want JAB support</param>
    ''' <param name="sErr">On failure, contains an error description.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Private Function InitJab(ByVal bUseJAB As Boolean, ByRef sErr As String) As Boolean
        If bUseJAB Then
            mJAB = New JABWrapper()
            Try
                mJAB.Init()
            Catch ex As Exception
                sErr = My.Resources.CouldNotInitialiseJavaAccessBridgeSupport & ex.Message
                Return False
            End Try
        Else
            mJAB = Nothing
        End If
        Return True
    End Function

    ''' <summary>
    ''' Launch the target application. Overload for Win32-style command-line
    ''' launching.
    ''' </summary>
    ''' <param name="sCommandLine">The command line</param>
    ''' <param name="sWorkingDirectory">The working directory for the process, also known as the current directory</param>
    ''' <param name="bUseHooks">True to use hooks</param>
    ''' <param name="opts">A comma-separated list of options to set, which are
    ''' generally application-type specific. An empty string if there are none.</param>
    ''' <param name="sErr">On failure, contains an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Private Function Launch(ByVal sCommandLine As String, ByVal sWorkingDirectory As String, ByVal bUseHooks As Boolean, ByVal bUseJAB As Boolean, ByVal opts As String, ByRef sErr As String) As Boolean
        SyncLock mApplicationStateLock
            If Connected Then
                sErr = My.Resources.AlreadyLaunched
                Return False
            End If
            NlogLogger.Debug($"Launch: {sCommandLine}")
            'Initialise JAB support...
            If Not InitJab(bUseJAB, sErr) Then Return False

            'Determine if we are dealing with a modern app
            If sCommandLine.StartsWith("<") Then
                IsModernApp = True
                sCommandLine = sCommandLine.Substring(0, sCommandLine.IndexOf(">") + 1)
            End If

            'Launch target application (in a suspended state if hooking)...
            Dim pInfo As PROCESS_INFORMATION
            Dim crflags As Integer = CreationFlags.NORMAL_PRIORITY_CLASS Or CreationFlags.DETACHED_PROCESS
            If bUseHooks Then crflags = crflags Or CreationFlags.CREATE_SUSPENDED
            pInfo = New PROCESS_INFORMATION
            Dim sInfo As STARTUPINFO = New STARTUPINFO
            sInfo.cb = Marshal.SizeOf(sInfo)

            If Not IsModernApp() Then
                NlogLogger.Debug($"Launching as a modern app: {sCommandLine}")
                If Not CreateProcess(Nothing, sCommandLine, Nothing, Nothing, False, crflags, IntPtr.Zero, sWorkingDirectory, sInfo, pInfo) Then
                    sErr = String.Format(My.Resources.FailedToLaunch0, sCommandLine)
                    NlogLogger.Debug(sErr)
                    Return False
                End If
            Else
                If Not clsContainerApp.ContainerAppsAvailable() Then
                    sErr = My.Resources.UnableToLaunchModernApplicationOnThisOperatingSystem
                    Return False
                End If
                pInfo.dwProcessId = clsContainerApp.LaunchApp(sCommandLine, opts)
                pInfo.hProcess = OpenProcess(ProcessAccess.ATTACH_RIGHTS, False, pInfo.dwProcessId)
                If pInfo.hProcess = IntPtr.Zero Then
                    sErr = String.Format(My.Resources.CouldNotOpenTargetProcess0, pInfo.dwProcessId)
                    Return False
                End If
            End If

            NlogLogger.Debug($"Got the PID: {pInfo.dwProcessId}")
            If pInfo.dwProcessId = 0 Then
                sErr = My.Resources.GotAProcessIDOf0ForTheTargetApplication
                Return False
            End If

            'Do the injection... (will resume the suspended target app)
            If bUseHooks Then
                mHookClient = New clsHookClient(msInjectorFolder)
                If Not mHookClient.Connect(pInfo.dwProcessId, pInfo.dwThreadId, sErr) Then
                    mHookClient = Nothing
                    Return False
                End If
            End If

            mProcess = clsProcessWatcher.FromHandle(pInfo.hProcess)
            SetupBrowserHandle()
            mPID = pInfo.dwProcessId
            SetOptions(opts)
            Connected = True

            Return True
        End SyncLock
    End Function

    ''' <summary>
    ''' Launch the target application. Overload for citrix session launching.
    ''' </summary>
    ''' <param name="sPath">Path to the ICA file</param>
    ''' <returns></returns>
    Public Function Launch(ByVal sPath As String, ByRef sErr As String) As Boolean
        Try
            Dim f As New Citrix.frmCitrixClient
            mCitrix = f.Client
            mCitrix.LoadIcaFile(sPath)
            mCitrix.OutputMode = Citrix.OutputMode.OutputModeNormal
            mCitrix.Connect()
            f.Show()
            Return True
        Catch e As Exception
            sErr = String.Format(My.Resources.LaunchFailed0, e.Message)
            mCitrix = Nothing
            Return False
        End Try
    End Function

    Private Sub HookClientFailed(ByVal sErr As String) Handles mHookClient.Failed
        'This is just so we get to see it in HookClientTest. I don't think it will
        'get to the user anywhere else.
        OnLineReceived("HOOK CLIENT FAILURE: " & sErr)
    End Sub

    Private Sub CommsLineRecieved(ByVal sLine As String) Handles mHookClient.LineReceived
        Dim objApiCallDetails As clsAPICallDetails
        objApiCallDetails = clsAPICallDetails.Parse(sLine)
        Try
            mobjModel.ProcessAPICall(objApiCallDetails)
        Catch e As Exception
            Debug.WriteLine("Error processing API call '" & sLine & "' - " & e.Message)
        End Try
    End Sub

    Private Sub CommsLineProcessed(ByVal sLine As String) Handles mobjModel.CommsLineProcessed
        OnLineReceived(sLine)
    End Sub

    ''' <summary>
    ''' Update the model. The owner must call this regularly (or at least before
    ''' attempting to read up-to-date information).
    ''' </summary>
    Public Sub UpdateModel()
        If mHookClient Is Nothing Then
            'We're not hooking, so create a new snapshot model...
            mobjModel = clsUIModel.MakeSnapshot(mPID, Me)
        Else
            'We're hooking, so update our model...
            mobjModel.UpdateModel()
        End If
    End Sub

    ''' <summary>
    ''' Event handler for when the terminal application terminates
    ''' </summary>
    Private Sub mTerminalApp_Terminated() Handles mTerminalApp.Terminated
        Disconnect()
    End Sub

    ''' <summary>
    ''' Event handler for when the process terminates.
    ''' </summary>
    Private Sub mProcess_Disconnected() Handles mProcess.Disconnected
        Disconnect()
    End Sub

    ''' <summary>
    ''' Preserves the stack trace when throwing an inner exception. With .NET 4.5 this
    ''' can be replaced with ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
    ''' </summary>
    Private Shared Sub PreserveStackTrace(e As Exception)
        If e Is Nothing Then Return

        Dim ctx = New StreamingContext(StreamingContextStates.CrossAppDomain)
        Dim mgr = New ObjectManager(Nothing, ctx)
        Dim si = New SerializationInfo(e.GetType(), New FormatterConverter())

        e.GetObjectData(si, ctx)
        mgr.RegisterObject(e, 1, si)
        mgr.DoFixups()
    End Sub

    ''' <summary>
    ''' Execute a query that has been parsed into a clsQuery object. Used internally
    ''' only.
    ''' </summary>
    ''' <param name="objQuery">The query</param>
    ''' <returns>The result</returns>
    Private Function ExecuteQuery(ByVal objQuery As clsQuery) As Reply
        Dim handler = GetCommandHandler(objQuery)
        If handler IsNot Nothing Then
            Dim context As New CommandContext(objQuery)
            Return handler.Execute(context)
        End If

        Dim sHandlerName As String
        sHandlerName = "ProcessCommand" & objQuery.Command
        Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(sHandlerName, Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.IgnoreCase)
        If mi Is Nothing Then
            Throw New InvalidOperationException(String.Format(My.Resources.Command0HasNoImplementation, objQuery.Command))
        End If

        Try
            Dim result As Reply = TryCast(mi.Invoke(Me, New Object() {objQuery}), Reply)
            If result Is Nothing Then
                Throw New InvalidOperationException(String.Format(My.Resources.Command0GaveInvalidResult, objQuery.Command))
            End If
            Return result
        Catch ex As TargetInvocationException
            Dim e = ex.InnerException
            PreserveStackTrace(e)
            Throw e
        End Try
    End Function

    Private Function GetCommandHandler(query As clsQuery) As ICommandHandler

        Return CommandHandlerFactory.CreateHandler(Me, query)

    End Function
    ''' <summary>
    ''' Process a query.
    ''' </summary>
    ''' <param name="sQuery">The query to process.</param>
    ''' <returns>The result of the query.</returns>
    ''' <remarks>Low-level logging is performed at this stage if requested.</remarks>
    Public Overrides Function ProcessQuery(ByVal sQuery As String) As String
        Return ProcessQuery(sQuery, TimeSpan.Zero)
    End Function

    ''' <summary>
    ''' Process a query.
    ''' </summary>
    ''' <param name="sQuery">The query to process.</param>
    ''' <param name="timeout">Length of time before the query times out.</param>
    ''' <returns>The result of the query.</returns>
    ''' <remarks>Low-level logging is performed at this stage if requested.</remarks>
    Public Overrides Function ProcessQuery(ByVal sQuery As String,
                                           timeout As TimeSpan) As String
        Dim objQuery As clsQuery, sResult As String
        Try
            'Always update the model before processing...
            UpdateModel()

            'Log the query if necessary...
            clsConfig.Log("Query:{0}", sQuery)

            'Parse and execute the query...
            objQuery = clsQuery.Parse(sQuery)
            Dim result As Reply = ExecuteQuery(objQuery)
            sResult = result.ToString()

            'Perform cleanup of any remaining
            'JAB Contexts See bug# 6853
            JABContext.CleanUp()

            'Log the response if necessary...
            clsConfig.Log("Response:{0}", sResult)

        Catch ex As Exception
            clsConfig.LogException(ex)
            sResult = "ERROR:" & ex.Message
        End Try
        Return sResult
    End Function

    ''' <summary>
    ''' Search up the Active Accessibility hierarchy for the first element that
    ''' has the given role.
    ''' </summary>
    ''' <param name="element">The element to start the search from. This element is
    ''' included in the search.</param>
    ''' <param name="role">The role to match.</param>
    ''' <returns>The element found, or Nothing if there is no match.</returns>
    Private Function GetAAAncestorWithRole(
     ByVal element As clsAAElement,
     ByVal role As AccessibleRole) As clsAAElement

        If element Is Nothing Then Return Nothing
        If element.Role = role Then Return element
        ' treat window as root element - can't really go much further back
        ' and it gets stuck in an infinite loop (before hitting stack overflow)
        ' looking for 'Nothing'... on XP at least.
        If element.Role = AccessibleRole.Window Then Return Nothing

        Return GetAAAncestorWithRole(element.Parent, role)
    End Function

    Public Shared Function GetWikiDocumentation() As String
        Dim db As New ApplicationManagerUtilities.clsWikiDocumentBuilder
        db.BeginDocument("Command Reference")

        Dim iTotal As Integer = 0
        Dim iDone As Integer = 0
        For Each c As Category In System.Enum.GetValues(GetType(Category))
            Select Case c
                Case Category.General
                    db.CreateHeader("General Commands", 2)
                Case Category.Win32
                    db.CreateHeader("Window-Based App Commands", 2)
                    db.AppendParagraph("Queries are made up of a command, followed by a number of parameters. The command is case-insensitive, and can be one of the following. These definitions match the current prototype, and are subject to much change, but this documentation must always remain current.")
                Case Category.DotNet
                    db.CreateHeader(".NET Framework Commands", 2)
                Case Category.Mouse
                    db.CreateHeader("Mouse Operations", 2)
                    db.AppendParagraph("This section details the commands relating to simulation mouse movement and clicking. Beware of various methods of doing the same thing, suited for different situations and application types.")
                Case Category.ComActiveX
                    db.CreateHeader("COM/ActiveX Commands", 2)
                Case Category.Accessibility
                    db.CreateHeader("Active Accessibility Commands", 2)
                Case Category.Java
                    db.CreateHeader("Java Access Bridge Commands", 2)
                Case Category.Citrix
                    db.CreateHeader("Citrix Application Commands", 2)
                Case Category.Terminal
                    db.CreateHeader("Terminal-Based App Commands", 2)
                Case Category.Html
                    db.CreateHeader("HTML Commands", 2)
                Case Category.Highlighting
                    db.CreateHeader("Element Highlighting Commands", 2)
                Case Category.Diagnostics
                    db.CreateHeader("Diagnostic Commands", 2)
            End Select

            Dim t As Type = GetType(clsLocalTargetApp)
            For Each m As Reflection.MethodInfo In t.GetMethods(Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                If m.Name.StartsWith("ProcessCommand") Then
                    iTotal += 1
                    For Each a As Object In m.GetCustomAttributes(False)
                        Dim ct As CategoryAttribute = TryCast(a, CategoryAttribute)
                        If Not ct Is Nothing Then
                            iDone += 1
                            If ct.Type = c Then
                                'Write Title
                                Dim sName As String = m.Name
                                sName = sName.Replace("ProcessCommand", "")
                                db.CreateHeader(sName, 3)

                                'Write Command documentation
                                For Each a2 As Object In m.GetCustomAttributes(False)
                                    Dim ca As CommandAttribute = TryCast(a2, CommandAttribute)
                                    If Not ca Is Nothing Then
                                        db.AppendParagraph(ca.Narrative)
                                    End If
                                Next

                                'Write Parameter Documentation
                                For Each a2 As Object In m.GetCustomAttributes(False)
                                    Dim pa As ParametersAttribute = TryCast(a2, ParametersAttribute)
                                    If Not pa Is Nothing Then
                                        db.AppendLiteralText(vbCrLf & "'''Parameters:''' " & pa.Narrative & vbCrLf)
                                    End If
                                Next

                                'Write Examples
                                For Each a2 As Object In m.GetCustomAttributes(False)
                                    Dim pa As ExamplesAttribute = TryCast(a2, ExamplesAttribute)
                                    If Not pa Is Nothing Then
                                        db.AppendLiteralText(vbCrLf & "'''Examples:'''" & vbCrLf)
                                        db.AppendLiteralText(vbCrLf & " " & pa.Narrative)
                                    End If
                                Next

                                'Write Examples
                                For Each a2 As Object In m.GetCustomAttributes(False)
                                    Dim pa As ResponseAttribute = TryCast(a2, ResponseAttribute)
                                    If Not pa Is Nothing Then
                                        db.AppendLiteralText(vbCrLf & "'''Response: '''" & pa.Narrative)
                                    End If
                                Next

                            End If
                        End If
                    Next
                End If
            Next
        Next

        db.EndDocument()

        Return db.ToString
    End Function

#Region "Query Command Handlers - HTML"

    <Category(Category.Html)>
    <Command("Invokes a Javascript method in an HTML Document")>
    <Parameters("The methodname specified by 'MethodName' and the JSON arguments specified by 'Arguments' as well as those parameters required to uniquely identify the an element to get to the parent HTML Document. For a method with no arguments, simply specify [] for the Arguments parameter.")>
    Private Function ProcessCommandHtmlInvokeJavascriptMethod(ByVal objQuery As clsQuery) As Reply
        'Get method name
        Dim methodName As String = objQuery.GetParameter(ParameterNames.MethodName)
        If String.IsNullOrEmpty(methodName) Then
            Throw New InvalidOperationException(My.Resources.CanNotInvokeJavascriptMethodSuppliedMethodNameIsBlank)
        End If

        Dim doc As clsHTMLDocument = Me.GetSingleDocument(objQuery)
        Dim sErr As String = Nothing
        Dim result As Object = Nothing

        'Get the arguments we are going to pass to the javascript method. These are
        'passed in the query parameter 
        Dim jsonargs As String = objQuery.GetParameter(ParameterNames.Arguments)
        If jsonargs Is Nothing Then Throw New InvalidOperationException(My.Resources.MissingArguments)

        If Not doc.InvokeJavascriptMethod(methodName, jsonargs, result, sErr) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedWhileInvokingJavascriptMethod0, sErr))
        End If

        Return Reply.Ok
    End Function


    <Category(Category.Html)>
    <Command("Inserts a Javascript method into an HTML Document")>
    <Parameters("The 'FragmentText' parameter containing the JavaScript, as well as those parameters required to uniquely identify the an element to get to the parent HTML Document.")>
    Private Function ProcessCommandHtmlInsertJavascriptFragment(ByVal objQuery As clsQuery) As Reply
        'Get fragment text
        Dim jscriptFragment As String = objQuery.GetParameter(ParameterNames.FragmentText)
        If String.IsNullOrEmpty(jscriptFragment) Then
            Throw New InvalidOperationException(My.Resources.CanNotInsertJavascriptMethodSuppliedMethodDefinitionIsBlank)
        End If

        Dim doc As clsHTMLDocument = Me.GetSingleDocument(objQuery)
        Dim sErr As String = Nothing

        If Not doc.InsertJavascriptFragment(jscriptFragment, sErr) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedWhileInsertingJavascriptMethod0, sErr))
        End If

        Return Reply.Ok
    End Function


    <Category(Category.Html)>
    <Command("Clicks the centre of an HTML element")>
    <Parameters("Those parameters required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlClickCentre(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        Dim t As Thread = RunThread.CreateThread(AddressOf HTMLClickCenterHandler.Exec, el)
        t.Start()

        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Thread handler to do a click on an HTML element.
    ''' </summary>
    Private Class HTMLClickCenterHandler
        Public Shared Sub Exec(ByVal o As Object)
            Try
                Dim el As clsHTMLElement = TryCast(o, clsHTMLElement)
                el.Click()
            Catch ex As Exception
            End Try
        End Sub
    End Class

    ''' <summary>
    ''' Thread handler to do a double click on an HTML element.
    ''' </summary>
    Private Class HTMLDoubleClickCenterHandler
        Public Shared Sub Exec(ByVal o As Object)
            Try
                Dim el As clsHTMLElement = TryCast(o, clsHTMLElement)
                el.DoubleClick()
            Catch ex As Exception
            End Try
        End Sub
    End Class

    <Category(Category.Html)>
    <Command("Double clicks the centre of an HTML element")>
    <Parameters("Those parameters required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlDoubleClickCentre(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        Dim t As Thread = RunThread.CreateThread(AddressOf HTMLDoubleClickCenterHandler.Exec, el)
        t.Start()

        Return Reply.Ok
    End Function

    <Category(Category.Html)>
    <Command("Checks to see if the HTML document is loaded")>
    <Parameters("Those required to uniquely identify an element to get to the parent HTML document.")>
    Private Function ProcessCommandHtmlDocumentLoaded(ByVal objQuery As clsQuery) As Reply
        Dim doc As clsHTMLDocument = Me.GetSingleDocument(objQuery, False)

        Dim browser As clsHTMLDocument.IWebBrowser2 = doc.GetBrowser
        If browser IsNot Nothing Then
            Return Reply.Result(clsHTMLDocument.IsLoaded(browser))
        Else
            Throw New InvalidOperationException(My.Resources.NoBrowserFound)
        End If

        Return Reply.Ok
    End Function

    <Category(Category.Html)>
    <Command("Checks to see if the HTML element exists and its parent document is loaded")>
    <Parameters("Those required to uniquely identify an element to get to the parent HTML document.")>
    Private Function ProcessCommandHtmlCheckExistsAndDocumentLoaded(ByVal objQuery As clsQuery) As Reply
        'Check to see if the element exists.
        Dim objElement As clsHTMLElement
        Try
            Dim objDocuments As List(Of clsHTMLDocument) = GetHtmlDocuments()
            objElement = mobjModel.GetHTMLElement(objQuery, objDocuments, False)
        Catch ex As ApplicationException
            Return Reply.Result(False)
        End Try

        'Ascend up to the root document.
        Dim objRootDocument As clsHTMLDocument = objElement.ParentDocument
        While TypeOf objRootDocument Is clsHTMLDocumentFrame
            objRootDocument = TryCast(objRootDocument, clsHTMLDocumentFrame).FrameElement.ParentDocument
        End While

        'If we could not find the root document then it cannot be loaded
        If objRootDocument Is Nothing Then
            Return Reply.Result(False)
        End If

        Dim bLoaded As Boolean
        If objRootDocument.DocumentLoaded Then
            'If the root document is loaded make sure all frames are loaded.
            Dim objFrames As List(Of clsHTMLDocumentFrame) = objRootDocument.FlatListOfFrames(mExcludeHTC)
            If objFrames.Count = 0 Then
                bLoaded = True
            Else
                bLoaded = True
                For Each objFrame As clsHTMLDocument In objFrames
                    If Not objFrame.DocumentLoaded Then
                        bLoaded = False
                    End If
                Next
            End If
        Else
            bLoaded = False
        End If

        Return Reply.Result(bLoaded)
    End Function

    <Category(Category.Html)>
    <Command("Gets the domain from the url")>
    <Parameters("Those required to uniquely identify an element to get to the parent HTML document.")>
    Private Function ProcessCommandHtmlGetDocumentUrlDomain(ByVal objQuery As clsQuery) As Reply
        Dim doc As clsHTMLDocument = GetSingleDocument(objQuery)
        Dim uri As New Uri(doc.URL)
        Return Reply.Result(uri.Host)
    End Function

    ''' <summary>
    ''' Identify an HTML document based on the identifiers given in a query.
    ''' </summary>
    ''' <param name="objQuery">The query object containing the identifiers</param>
    ''' <param name="bRetry">True to retry the operation 10 times, at intervals of
    ''' 1 second, before giving up!?!?!?</param>
    ''' <returns>The clsHTMLDocument found, or Nothing if no there was no valid
    ''' match (or too many, i.e. more than one)</returns>
    Private Function GetSingleDocument(ByVal objQuery As clsQuery, Optional ByVal bRetry As Boolean = True) As clsHTMLDocument
        Dim docs As List(Of clsHTMLDocument) = Nothing

        Try
            docs = GetHtmlDocuments()
            Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs, bRetry)
            Return el.ParentDocument

        Catch ex As ApplicationException
            ' IE seems to occasionally pop up an empty document which gets in the
            ' way of our document gathering. So if there are any "about:blank" docs
            ' found, ensure we're not including them in our search.
            docs = docs?.Where(Function(d) d.URL <> "about:blank").ToList()
            If docs.Count = 1 AndAlso docs(0) IsNot Nothing Then Return docs(0)
            Throw

        End Try

    End Function

    <Category(Category.Html)>
    <Command("Gets the url for the HTML document")>
    <Parameters("Those required to uniquely identify an element to get to the parent HTML document.")>
    Private Function ProcessCommandHtmlGetDocumentUrl(ByVal objQuery As clsQuery) As Reply
        Dim doc As clsHTMLDocument = Me.GetSingleDocument(objQuery)
        Return Reply.Result(doc.URL)
    End Function

    <Category(Category.Html)>
    <Command("Gets the selected item of a Select element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<value>"" where <value> is the value of the selected item.")>
    Private Function ProcessCommandHtmlGetSelectedValue(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        Dim item As clsHTMLElement = el.SelectedItem
        If Not item Is Nothing Then
            Return Reply.Result(item.Value)
        Else
            Throw New InvalidOperationException(My.Resources.ThereIsNoCurrentlySelectedItem)
        End If
    End Function

    <Category(Category.Html)>
    <Command("Selects an item of a Select element.")>
    <Parameters("The name of the item to select specified by 'NewText' or the position of the item to select as specified by 'Position' or the value of the item to select as specified by 'PropName', as well as those required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlSelectItem(ByVal objQuery As clsQuery) As Reply

        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        Dim sName As String = objQuery.GetParameter(ParameterNames.NewText)
        If sName IsNot Nothing Then
            If el.SelectItemByName(sName) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(My.Resources.NoChildItemExistsWithTheSpecifiedText)
            End If
        End If

        Dim sPos As String = objQuery.GetParameter(ParameterNames.Position)
        If sPos IsNot Nothing Then
            Dim pos As Integer = Integer.Parse(sPos)
            If pos < 1 Then
                Throw New InvalidOperationException(My.Resources.PositionMustBe1OrAbove)
            End If
            Dim index As Integer = pos - 1
            If el.SelectItem(index) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(My.Resources.NoChildItemExistsWithTheSpecifiedPosition)
            End If
        End If

        Dim sValue As String = objQuery.GetParameter(ParameterNames.PropName)
        If sValue IsNot Nothing Then
            If el.SelectItemByValue(sValue) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(My.Resources.NoChildItemExistsWithTheSpecifiedValue)
            End If
        End If

        Throw New InvalidOperationException(My.Resources.YouMustSupplyEitherAnItemTextItemPositionOrItemValueParameter)

    End Function


    <Category(Category.Html)>
    <Command("Checks to see whether an HTML element can be identified.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandHtmlCheckExists(ByVal objQuery As clsQuery) As Reply

        Try
            Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
            Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs, False)
        Catch ex As ApplicationException
            Return Reply.Result(False)
        End Try
        Return Reply.Result(True)

    End Function

    <Category(Category.Html)>
    <Command("Sets the value of an HTML element.")>
    <Parameters("The value to set specified by 'NewText' as well as those required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlSetValue(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String = objQuery.GetParameter(ParameterNames.NewText)


        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        el.Value = sNewText


        Return Reply.Ok
    End Function


    <Category(Category.Diagnostics)>
    <Command("Takes a Snapshot of all the HTML elements.")>
    <Parameters("Usually None, but if you do supply any identifiers then the " &
      "list will only contain the elements that match.")>
    <Response("""RESULT:<snapshot>"" where <snapshot> line seperated, comma " &
      "delimited list of all HTML element attributes. Element attribute values " &
      "that span multiple lines are contained within quotes.")>
    Private Function ProcessCommandHtmlSnapshot(ByVal q As clsQuery) As Reply

        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim sb As New StringBuilder()


        ' We ignore 'includeinvisible' parameters - they are meaningless in a
        ' HTML context (it doesn't even check), but may come in due to their
        ' being used for all other snapshot types.

        ' If there are any params/identifiers (basically anything other than
        ' "htmlsnapshot") other than 'includeinvisible', fall back to the old
        ' flat listing.
        If q.Parameters.Count > 0 OrElse q.Identifiers.Count > 0 Then
            For Each el As clsHTMLElement In mobjModel.GetHTMLElements(q, docs, Nothing)
                sb.Append("HTML:")
                el.AppendIdentifiers(sb).AppendLine()
            Next
        Else ' Otherwise we use a more structured one
            For Each doc As clsHTMLDocument In docs
                Dim root = doc.Root
                If root IsNot Nothing Then
                    root.DumpTo(sb, 0, 2, " "c, "HTML:")
                End If
            Next
        End If
        Return Reply.Result(sb.ToString())



    End Function

    <Category(Category.Diagnostics)>
    <Command("Gets the outer HTML of an element")>
    <Parameters("None.")>
    <Response("""RESULT:<html>"" where <html> is the outer HTML of the element")>
    Private Function ProcessCommandHtmlGetOuterHtml(ByVal objQuery As clsQuery) As Reply

        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        Return Reply.Result(el.OuterHTML)

    End Function

    <Category(Category.Html)>
    <Command("Gets the path of the HTML element")>
    <Parameters("None.")>
    <Response("""RESULT:<path>"" where <path> is the path of the HTML element")>
    Private Function ProcessCommandHtmlGetPath(ByVal objQuery As clsQuery) As Reply

        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        Return Reply.Result(el.Path)

    End Function

    <Category(Category.Diagnostics)>
    <Command("Gets the inner HTML of an element")>
    <Parameters("None.")>
    <Response("""RESULT:<html>"" where <html> is the inner HTML of the element")>
    Private Function ProcessCommandHtmlGetInnerHtml(ByVal objQuery As clsQuery) As Reply

        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        Return Reply.Result(el.InnerHTML)

    End Function

    <Category(Category.Diagnostics)>
    <Command("Takes a Source Capture of all the HTML documents and frames.")>
    <Parameters("None.")>
    <Response("""RESULT:<capture>"" where <capture> is the html of all the documents and frames.")>
    Private Function ProcessCommandHtmlSourceCap(ByVal objQuery As clsQuery) As Reply

        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()

        Dim st As New IO.StringWriter
        Dim xr As New XmlTextWriter(st)

        Dim writtenDocUrls As New List(Of String)

        xr.WriteStartElement("capture")
        For Each doc As clsHTMLDocument In docs
            'Write the main document source
            xr.WriteStartElement("document")
            xr.WriteAttributeString("url", doc.URL)
            xr.WriteAttributeString("browser", doc.GetBrowser().FullName())
            writtenDocUrls.Add(doc.URL)
            Dim s As String = ProcessSource(doc.Source)
            xr.WriteCData(s)
            xr.WriteEndElement()

            'Write the details of frames
            Try
                Dim frames As List(Of clsHTMLDocumentFrame) = doc.FlatListOfFrames(mExcludeHTC)
                For Each f As clsHTMLDocumentFrame In frames
                    If Not writtenDocUrls.Contains(f.URL) Then
                        writtenDocUrls.Add(f.URL)
                        xr.WriteStartElement("frame")
                        xr.WriteAttributeString("url", f.URL)
                        s = ProcessSource(f.Source)
                        xr.WriteCData(s)
                        xr.WriteEndElement()

                        'Including the frame's resources
                        AddDocumentResources(f, xr)
                    End If
                Next
            Catch ex As Exception
                xr.WriteStartElement("frameserror")
                xr.WriteAttributeString("message", ex.Message)
                xr.WriteEndElement()
            End Try

            'Get embedded resources in root document too
            AddDocumentResources(doc, xr)
        Next
        xr.WriteEndElement()

        Return Reply.Result(st.ToString())

    End Function


    ''' <summary>
    ''' Adds any linked resources from within an html document. Such resources include
    ''' linked javascript and css files (hosted in a separate document).
    ''' </summary>
    ''' <param name="doc">The document from which to extract the content</param>
    ''' <param name="xr">The xmltextwriter to which to add the resources
    ''' found. Resources will appear in a 'resources' element.</param>
    Private Sub AddDocumentResources(ByVal doc As clsHTMLDocument, ByVal xr As XmlTextWriter)
        For Each el As clsHTMLElement In doc.All
            Select Case el.TagName.ToLower(CultureInfo.InvariantCulture)
                Case "link", "script"
                    Dim contentUrl As String = Nothing
                    Dim contentType As String = Nothing
                    Dim contentEncoding As String = Nothing
                    Dim content As String
                    Try
                        content = el.GetLinkedContent(contentUrl, contentType, contentEncoding)
                        AvoidNestedCDataProblems(content)
                    Catch ex As Exception
                        'If something went badly wrong while capturing, capture
                        'the details of that instead...
                        content = String.Format(My.Resources.ExceptionWhileCapturing0, ex)
                        contentType = "text"
                        If contentUrl Is Nothing Then
                            contentUrl = el.InnerHTML
                        End If
                    End Try

                    Try
                        If content IsNot Nothing Then
                            xr.WriteStartElement("resource")
                            xr.WriteAttributeString("source", contentUrl)
                            xr.WriteAttributeString("elementid", el.ID)
                            xr.WriteAttributeString("elementparenturl", el.ParentURL)
                            xr.WriteAttributeString("elementpath", el.Path)
                            xr.WriteAttributeString("contenttype", contentType)
                            If Not String.IsNullOrEmpty(contentEncoding) Then
                                xr.WriteAttributeString("contentencoding", contentEncoding)
                            End If
                            xr.WriteCData(content)
                            xr.WriteEndElement()
                        End If
                    Catch ex As Exception
                        'Not much we can do
                    End Try
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Strips cdata entities from the source 
    ''' </summary>
    ''' <param name="sSource"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ProcessSource(ByVal sSource As String) As String
        AvoidNestedCDataProblems(sSource)
        Return "<HTML>" & sSource & "</HTML>"
    End Function

    ''' <summary>
    ''' Joke function which ought to do something better.
    ''' </summary>
    ''' <param name="Content"></param>
    Private Sub AvoidNestedCDataProblems(ByRef Content As String)
        Content = Content.Replace("]]>", String.Empty)
        Content = Content.Replace("<![CDATA[", String.Empty)
    End Sub

    <Category(Category.Html)>
    <Command("Update a cookie within a document, to understand how cookies work you can read this: http://www.w3schools.com/js/js_cookies.asp")>
    <Parameters("The cookie, as well as those parameters required to uniquely identify the an element to get to the parent HTML Document.")>
    Private Function ProcessCommandHtmlUpdateCookie(ByVal objQuery As clsQuery) As Reply
        Dim cookieText As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim doc As clsHTMLDocument = Me.GetSingleDocument(objQuery)

        Try
            doc.UpdateCookie(cookieText)
        Catch
            Throw New InvalidOperationException(My.Resources.FailedToSetCookieDataInTargetDocument)
        End Try

        Return Reply.Ok
    End Function

    <Category(Category.Html)>
    <Command("Gets the value of an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<value>"" where <value> is the value of the html element.")>
    Private Function ProcessCommandHtmlGetValue(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        sNewText = el.Value

        Return Reply.Result(sNewText)
    End Function

    <Category(Category.Html)>
    <Command("Gets all the items in an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml containing all the items of the HTML element.")>
    Private Function ProcessCommandHtmlGetAllItems(ByVal objQuery As clsQuery) As Reply
        Dim xdoc As New XmlDocument
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        Dim rootEl As XmlElement = GetHtmlChildItemsAsCollection(xdoc, el, False)
        xdoc.AppendChild(rootEl)

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Html)>
    <Command("Gets the items within an HTML table")>
    <Parameters("Those required to uniquely identify the required HTML table or an element within the table.")>
    <Response("""RESULT:<xml>"" where <xml> is the collection xml containing the items of the HTML table. The collection contains a single column called 'Item Text'.")>
    Private Function ProcessCommandHtmlGetTable(ByVal objQuery As clsQuery) As Reply
        Dim xdoc As New XmlDocument
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        ' Now we want to get the table in which the element resides:
        el = el.GetAncestorWithTagName("TABLE")
        If el Is Nothing Then Throw New ArgumentException(My.Resources.NoHTMLTableFoundInGetTableCall)
        Dim rootEl As XmlElement = GetHtmlTableAsCollection(xdoc, el)
        xdoc.AppendChild(rootEl)

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the items within an Active Accessibility table")>
    <Parameters("Those required to uniquely identify the table")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml containing the items of the table.")>
    Private Function ProcessCommandAAGetTable(ByVal objQuery As clsQuery) As Reply
        Dim xdoc As New XmlDocument()
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        ' We need the table element - so if this element is a cell (or any element within a cell
        ' we need to navigate back through the tree until we reach a table element)
        Dim table As clsAAElement = GetAAAncestorWithRole(e, AccessibleRole.Table)
        If table Is Nothing Then Throw New InvalidOperationException(My.Resources.NoTableFoundForTheGivenElement)

        ' Now we need to navigate through the table and pick out the cells.
        ' Helpfully, there's no row/column information/structure - just a list of cells, 
        ' so we can only guess at it through the location of the cell.
        ' Since the cells go in definition order, we only really care about when the Y co-ord
        ' changes - this signifies a new row, so we can build a table based on that.
        ' The structure goes : table => cell => editable text/whatever... The name of the editable text
        ' is the value we require.
        Dim collection As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collection)

        Dim currentY As Integer = Integer.MinValue
        Dim rowNum As Integer = 0
        Dim colNum As Integer = 0

        Dim row As XmlElement = Nothing

        For Each cell As clsAAElement In table.Elements

            If cell.Top <> currentY Then
                row = xdoc.CreateElement("row")
                collection.AppendChild(row)
                currentY = cell.Top
                colNum = 0
                rowNum += 1
            End If
            colNum += 1

            ' Generally a cell contains a text element and nothing more, but if there are any
            ' text formatting tags (<i>, <b> etc.) then this will be increased.
            If cell.ElementCount > 1 Then
                ' Collapse the elements and append them into a buffer...
                ' There's no other useful way of doing it.      
                Dim sb As New StringBuilder()
                CollapseChildElements(cell, sb)
                row.AppendChild(CreateCollectionFieldXML(xdoc, sb.ToString, "Text", "Column" & colNum))

            ElseIf cell.ElementCount = 1 Then
                row.AppendChild(CreateCollectionFieldXML(
                 xdoc, cell.Elements(0).Name, "Text", "Column" & colNum))
            Else ' ie. it's an empty cell ("<td></td>") - treat as empty string
                row.AppendChild(CreateCollectionFieldXML(
                 xdoc, "", "Text", "Column" & colNum))
            End If
        Next

        AppendXmlTableRowColumns(xdoc, collection)

        Return Reply.Result(xdoc.OuterXml)

    End Function

    ''' <summary>
    ''' Collapses cell contents child elements into a single string using recursion.
    ''' </summary>
    ''' <param name="cell">The cell to descend.</param>
    ''' <param name="sb">A string builder to hold the string.</param>
    Private Sub CollapseChildElements(ByVal cell As clsAAElement, ByVal sb As StringBuilder)
        For Each element As clsAAElement In cell.Elements
            'If we hit an element which contains more elements, 
            'then we have to do a recursive decent
            If element.ElementCount > 0 Then
                CollapseChildElements(element, sb)
            Else
                sb.Append(element.Name)
            End If
        Next
    End Sub

    <Category(Category.Html)>
    <Command("Gets the selected items in an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml containing the selected items of the HTML element.")>
    Private Function ProcessCommandHtmlGetSelectedItems(ByVal objQuery As clsQuery) As Reply
        Dim xdoc As New XmlDocument
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        Dim rootEl As XmlElement = Me.GetHtmlChildItemsAsCollection(xdoc, el, True)
        xdoc.AppendChild(rootEl)

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Html)>
    <Command("Gets the text of a selected item in an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<value>"" where <value> is the text of the selected item in the HTML element.")>
    Private Function ProcessCommandHtmlGetSelectedItemText(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)

        For Each child As clsHTMLElement In el.Children
            If child.Selected Then
                Return Reply.Result(child.Value)
            End If
        Next

        Throw New InvalidOperationException(My.Resources.ThereAreNoSelectedChildItems)
    End Function

    ''' <summary>
    ''' Gets all child items of an html element as an automate collection.
    ''' </summary>
    ''' <param name="ParentDocument">The document with which to create the collection,
    ''' and its child elements.</param>
    ''' <param name="HTMLElement">The element whose children are sought.</param>
    ''' <param name="SelectedOnly">If true, then only children which are
    ''' selected will be retrieved; otherwise all children will be retrieved.</param>
    ''' <returns>Returns an xml element with the name "collection", representing
    ''' the elements child items, in the format of an Automate collection.</returns>
    Private Function GetHtmlChildItemsAsCollection(ByVal ParentDocument As XmlDocument, ByVal HTMLElement As clsHTMLElement, ByVal SelectedOnly As Boolean) As XmlElement
        Dim rootEl As XmlElement = ParentDocument.CreateElement("collection")
        For Each child As clsHTMLElement In HTMLElement.Children
            If (Not SelectedOnly) OrElse child.Selected Then
                Dim rowEl As XmlElement = ParentDocument.CreateElement("row")
                rowEl.AppendChild(CreateCollectionFieldXML(ParentDocument, child.Value, "text", "Item Text"))
                rootEl.AppendChild(rowEl)
            End If
        Next
        Return rootEl
    End Function

    ''' <summary>
    ''' Gets all child items of an html table as an automate collection.
    ''' </summary>
    ''' <param name="ParentDocument">The document with which to create the collection,
    ''' and its child elements.</param>
    ''' <param name="HTMLElement">The table element whose children are sought.</param>
    ''' <returns>Returns an xml element with the name "collection", representing
    ''' the table element, in the format of an Automate collection.</returns>
    Private Function GetHtmlTableAsCollection(ByVal ParentDocument As XmlDocument, ByVal HTMLElement As clsHTMLElement) As XmlElement
        Dim rootEl As XmlElement = ParentDocument.CreateElement("collection")

        For Each xBody As clsHTMLElement In HTMLElement.Children
            If xBody.TagName = "TBODY" Then
                For Each xRow As clsHTMLElement In xBody.Children
                    If xRow.TagName = "TR" Then
                        Dim rowEl As XmlElement = ParentDocument.CreateElement("row")
                        Dim i As Integer = 0
                        For Each xField As clsHTMLElement In xRow.Children
                            If xField.TagName = "TD" OrElse xField.TagName = "TH" Then
                                i += 1
                                rowEl.AppendChild(CreateCollectionFieldXML(ParentDocument, xField.Value, "text", "Column" & i))
                            End If
                        Next
                        rootEl.AppendChild(rowEl)
                    End If
                Next
            End If
        Next
        Return rootEl
    End Function

    <Category(Category.Html)>
    <Command("Counts the number of items within an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<value>"" where <value> is the number of items within the HTML element.")>
    Private Function ProcessCommandHtmlCountItems(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        If el.Children IsNot Nothing Then
            sNewText = el.Children.Count.ToString
        Else
            sNewText = "0"
        End If
        Return Reply.Result(sNewText)
    End Function

    <Category(Category.Html)>
    <Command("Counts the number of selected items within an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:<value>"" where <value> is the number of selected items within the HTML element.")>
    Private Function ProcessCommandHtmlCountSelectedItems(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        If el.Children IsNot Nothing Then
            Dim count As Integer = 0
            For Each child As clsHTMLElement In el.Children
                If child.Selected Then count += 1
            Next
            sNewText = count.ToString
        Else
            sNewText = "0"
        End If
        Return Reply.Result(sNewText)
    End Function

    <Category(Category.Html)>
    <Command("Sets the check state of an HTML element.")>
    <Parameters("The check state specified by 'NewText' and those required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlSetChecked(ByVal objQuery As clsQuery) As Reply
        Dim bChecked As Boolean = Boolean.Parse(objQuery.GetParameter(ParameterNames.NewText))
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        If el IsNot Nothing Then
            el.Checked = bChecked
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(My.Resources.NoMatchingElementFound)
        End If
    End Function

    <Category(Category.Html)>
    <Command("Sets the check state of an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandHtmlGetChecked(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        If el IsNot Nothing Then
            Return Reply.Result(el.Checked)
        Else
            Throw New InvalidOperationException(My.Resources.NoMatchingElementFound)
        End If
    End Function

    <Category(Category.Html)>
    <Command("Focuses an HTML element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlFocus(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        el.Focus()
        Return Reply.Ok
    End Function

    <Category(Category.Html)>
    <Command("Hovers over an HTML element.")>
    <Parameters("Those parameters required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHtmlHover(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim el As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        HoverAt(New Point(el.AbsoluteBounds.Left, el.AbsoluteBounds.Top))
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Changes the URL of the HTML document to the specified URL. NOTE: This 
    ''' function sometimes throw an unexpected InvalidCastException when trying to 
    ''' access any method/property on the IHTMLDocument2 interface. This has been 
    ''' logged in BG-355.
    ''' </summary>
    <Category(Category.Html)>
    <Command("Navigates to a URL")>
    <Parameters("The URL specified by 'NewText' and those required to uniquely identify the HTML element which can be found within the parent document.")>
    Private Function ProcessCommandHtmlNavigate(ByVal objQuery As clsQuery) As Reply
        Dim sUrl As String = objQuery.GetParameter(ParameterNames.NewText)
        'Get the html document from the current tab
        Dim docs As List(Of clsHTMLDocument) = Me.GetHtmlDocuments(True)
        If docs IsNot Nothing AndAlso docs.Count = 1 Then
            Try
                docs(0).URL = sUrl
            Catch ex As InvalidCastException
                Throw New InvalidCastException(
                    My.Resources.UnableToNavigateDueToAnUnexpectedCOMException)
            End Try
        Else
            Throw New NoSuchHTMLElementException()
        End If
        Return Reply.Ok
    End Function

    <Category(Category.Html)>
    <Command("Gets the bounds of an HTML element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element bounds.")>
    Private Function ProcessCommandHtmlGetElementBounds(ByVal objQuery As clsQuery) As Reply
        Dim el As clsHTMLElement
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        el = mobjModel.GetHTMLElement(objQuery, docs)
        Return Reply.Result(CreateCollectionXMLFromRectangle(el.ClientBounds))
    End Function

    <Category(Category.Html)>
    <Command("Gets the screen relative bounds of an HTML element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element screen bounds.")>
    Private Function ProcessCommandHtmlGetElementScreenBounds(ByVal objQuery As clsQuery) As Reply
        Dim el As clsHTMLElement
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        el = mobjModel.GetHTMLElement(objQuery, docs)
        Return Reply.Result(CreateCollectionXMLFromRectangle(el.AbsoluteBounds))
    End Function

#End Region

#Region "Query Command Handlers - SAP"

    <Category(Category.SAP)>
    <Command("Check for SAP connectivity")>
    <Parameters("None")>
    Private Function ProcessCommandSapCheck(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)
        Return Reply.Ok
    End Function

    <Category(Category.SAP)>
    <Command("Check for existence of a SAP component")>
    <Parameters("ID of the component")>
    Private Function ProcessCommandSapCheckExists(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)
        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Return Reply.Result(False)
        Return Reply.Result(True)
    End Function


    <Category(Category.SAP)>
    <Command("Get all items from a SAP grid view component")>
    <Parameters("ID of the component")>
    Private Function ProcessCommandSapGetAllGridItems(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        Dim rowCount As Integer = CInt(GetProperty(child, "rowCount"))
        Dim columnCount As Integer = CInt(GetProperty(child, "columnCount"))
        'Get a list of the column names...
        Dim columnOrder As Object = GetProperty(child, "columnOrder")
        Dim cols As New List(Of String)
        For i As Integer = 0 To columnCount - 1
            cols.Add(CStr(GetProperty(columnOrder, "item", i)))
        Next

        Dim xdoc As New XmlDocument()
        Dim colRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(colRoot)

        For row As Integer = 0 To rowCount - 1
            Dim rowElement As XmlElement = xdoc.CreateElement("row")
            For Each thiscol As String In cols
                Dim val As String = CStr(InvokeMethod(child, "getCellValue", row, thiscol))
                Dim fieldElement As XmlElement = xdoc.CreateElement("field")
                fieldElement.SetAttribute("type", "text")
                fieldElement.SetAttribute("value", val)
                fieldElement.SetAttribute("name", thiscol)
                rowElement.AppendChild(fieldElement)

            Next
            colRoot.AppendChild(rowElement)
        Next
        Return Reply.Result(xdoc.OuterXml)
    End Function


    <Category(Category.SAP)>
    <Command("Get all items from a SAP table control")>
    <Parameters("ID of the component")>
    Private Function ProcessCommandSapGetAllTableItems(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue

        'Note: With the SAP table control the cell IDs (used by GetCell) are based
        'on the visible rows. Therefore we need to read the visible cells then adjust
        'the vertical scrollbar and repeat. When the scrollbar position is Set the
        'table object needs to be re-loaded.

        Try
            'Identify the SAP table
            Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
            If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

            'Get the visible row count
            Dim visibleRows As Integer = CInt(GetProperty(child, "VisibleRowCount"))

            'Get the vertical scrollbar details
            Dim vScrollBar As Object = GetProperty(child, "VerticalScrollBar")
            Dim initScrollPos As Integer = CInt(GetProperty(vScrollBar, "Position"))
            Dim minScrollPos As Integer = CInt(GetProperty(vScrollBar, "Minimum"))
            Dim maxScrollPos As Integer = CInt(GetProperty(vScrollBar, "Maximum"))

            'Ensure scrollbar starts at the top
            If initScrollPos <> minScrollPos Then
                SetProperty(vScrollBar, "Position", minScrollPos)
                'Re-acquire the table object
                child = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
                vScrollBar = GetProperty(child, "VerticalScrollBar")
            End If

            'Build a list of the column names
            Dim columns As Object = GetProperty(child, "Columns")
            Dim columnCount As Integer = CInt(GetProperty(columns, "Count"))
            Dim cols As New List(Of String)
            For i As Integer = 0 To columnCount - 1
                Dim col As Object = GetProperty(columns, "Item", i)
                cols.Add(CStr(GetProperty(col, "Title")))
            Next

            'Create the collection XML
            Dim xdoc As New XmlDocument()
            Dim colRoot As XmlElement = xdoc.CreateElement("collection")
            xdoc.AppendChild(colRoot)

            Dim moreRows As Boolean = True
            Dim startRow As Integer = 0
            Do
                'Loop round visible rows and add cell contents to collection
                For row As Integer = startRow To visibleRows - 1
                    'Check there are as many cells as columns in this row 
                    Dim rows As Object = GetProperty(child, "Rows")
                    Dim thisRow As Object = InvokeMethod(rows, "ElementAt", row)
                    Dim rowCells As Integer = CInt(GetProperty(thisRow, "Count"))
                    If rowCells <> columnCount Then Continue For

                    Dim rowElement As XmlElement = xdoc.CreateElement("row")
                    For col As Integer = 0 To columnCount - 1
                        Dim cell As Object = InvokeMethod(child, "GetCell", row, col)
                        Dim val As String = CStr(GetProperty(cell, "Text"))

                        Dim fieldElement As XmlElement = xdoc.CreateElement("field")
                        fieldElement.SetAttribute("type", "text")
                        fieldElement.SetAttribute("value", val)
                        fieldElement.SetAttribute("name", cols(col))
                        rowElement.AppendChild(fieldElement)

                    Next
                    colRoot.AppendChild(rowElement)
                Next

                If CInt(GetProperty(vScrollBar, "Position")) = maxScrollPos Then
                    'If we've come to the end of the table then stop
                    moreRows = False
                Else
                    'Otherwise move to next page and repeat, but ensure that we don't read
                    'the same rows twice (e.g. if can't scroll down a full page then some rows
                    'already processed will still be visible)
                    Dim pos As Integer = CInt(GetProperty(vScrollBar, "Position")) + visibleRows
                    If pos > maxScrollPos Then
                        startRow = pos - maxScrollPos
                        pos = maxScrollPos
                    End If
                    SetProperty(vScrollBar, "Position", pos)
                    'Re-acquire the table object
                    child = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
                    vScrollBar = GetProperty(child, "VerticalScrollBar")
                End If
            Loop While moreRows

            'Restore the scrollbar to original position (if changed)
            If CInt(GetProperty(vScrollBar, "Position")) <> initScrollPos Then
                SetProperty(vScrollBar, "Position", initScrollPos)
            End If

            Return Reply.Result(xdoc.OuterXml)

        Catch ex As Exception
            Dim msg As String = My.Resources.FailedWhileReadingSAPTable & ex.Message()
            Throw New InvalidOperationException(msg)
        End Try
    End Function

    <Category(Category.SAP)>
    <Command("Get all items from a SAP combo box")>
    <Parameters("ID of the component")>
    Private Function ProcessCommandSapGetAllListItems(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        Dim entries As Object = GetProperty(child, "Entries")
        Dim entryCount As Integer = CInt(GetProperty(entries, "Count"))

        Dim xdoc As New XmlDocument()
        Dim colRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(colRoot)

        For i As Integer = 0 To entryCount - 1
            Dim rowElement As XmlElement = xdoc.CreateElement("row")
            Dim item As Object = GetProperty(entries, "Item", i)
            Dim val As String = CStr(GetProperty(item, "Value"))
            Dim fieldElement As XmlElement = xdoc.CreateElement("field")
            fieldElement.SetAttribute("type", "text")
            fieldElement.SetAttribute("value", val)
            fieldElement.SetAttribute("name", "Item")
            rowElement.AppendChild(fieldElement)
            colRoot.AppendChild(rowElement)
        Next
        Return Reply.Result(xdoc.OuterXml)
    End Function


    <Category(Category.SAP)>
    <Command("Select a Tab within a SAP tabstrip control")>
    <Parameters("ID of the component and Either the name of the item specified by 'NewText' or the index of the item specified by 'Position'")>
    Private Function ProcessCommandSapSelectTab(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        Dim itemText As String = objQuery.GetParameter(ParameterNames.NewText)
        'The 1-based index of the item to be selected.
        'Only used when itemtext is null, in which case this may not be null
        Dim position As String = objQuery.GetParameter(ParameterNames.Position)
        'Corresponds to Position, but is a zero-based index
        Dim index As Integer = -1

        Dim children As Object = GetProperty(child, "Children")
        Dim childCount As Integer = CInt(GetProperty(children, "Count"))

        If String.IsNullOrEmpty(itemText) Then
            If String.IsNullOrEmpty(position) Then
                Throw New InvalidOperationException(My.Resources.AtLeastOneOfItemTextOrPositionMustBeSpecified)
            Else
                If Integer.TryParse(position, index) Then
                    index -= 1 'Correction for zero-based index
                Else
                    Throw New InvalidOperationException(My.Resources.FailedToIntepretValueForPositionAsAWholeNumber)
                End If
            End If
            'Check that the index is valid
            If (index < 0) OrElse (index > childCount - 1) Then
                Throw New InvalidOperationException(String.Format(My.Resources.IndexIsOutOfRangeHighestZeroBasedValueAllowedIs0, childCount - 1))
            Else
                Dim item As Object = GetProperty(children, "Item", index)
                InvokeMethod(item, "Select")
                Return Reply.Ok
            End If
        End If

        'Whenever ItemText is given Always favor using it to select the tab.
        For i As Integer = 0 To childCount - 1
            Dim item As Object = GetProperty(children, "Item", i)
            Dim text As String = CStr(GetProperty(item, "Text"))
            If text = itemText Then
                InvokeMethod(item, "Select")
                Return Reply.Ok
            End If
        Next

        Throw New InvalidOperationException(String.Format(My.Resources.CouldNotFindTabWithName0, itemText))
    End Function

    <Category(Category.SAP)>
    <Command("Select an item in a SAP control")>
    <Parameters("ID of the component and Either the name of the item specified by 'NewText' or the index of the item specified by 'Position'")>
    Private Function ProcessCommandSAPSelectItem(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        Dim itemText As String = objQuery.GetParameter(ParameterNames.NewText)
        'The 1-based index of the item to be selected.
        'Only used when itemtext is null, in which case this may not be null
        Dim position As String = objQuery.GetParameter(ParameterNames.Position)
        'Corresponds to Position, but is a zero-based index
        Dim index As Integer = -1

        Dim entries As Object = GetProperty(child, "Entries")
        Dim entryCount As Integer = CInt(GetProperty(entries, "Count"))

        If String.IsNullOrEmpty(itemText) Then
            If String.IsNullOrEmpty(position) Then
                Throw New InvalidOperationException(My.Resources.AtLeastOneOfItemTextOrPositionMustBeSpecified)
            Else
                If Integer.TryParse(position, index) Then
                    index -= 1 'Correction for zero-based index
                Else
                    Throw New InvalidOperationException(My.Resources.FailedToIntepretValueForPositionAsAWholeNumber)
                End If
            End If
            'Check that the index is valid
            If (index < 0) OrElse (index > entryCount - 1) Then
                Throw New InvalidOperationException(String.Format(My.Resources.IndexIsOutOfRangeLowestValueZeroOrOneHighestValueAllowedIs0, entryCount - 1))
            Else
                Dim item As Object = GetProperty(entries, "Item", index)
                itemText = CStr(GetProperty(item, "Value"))
            End If
        End If

        Try
            'Use ItemText to select the item.
            SetProperty(child, "value", itemText)
            Return Reply.Ok
        Catch
            Throw New InvalidOperationException(String.Format(My.Resources.CouldNotSelectItemWithName0, itemText))
        End Try
    End Function

    <Category(Category.SAP)>
    <Command("Select a menu item within a SAP menubar control")>
    <Parameters("ID of the component and the menu path of the item specified by 'Value'")>
    Private Function ProcessCommandSapSelectMenuItem(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        'Get menu path string
        Dim menuPath As String = objQuery.GetParameter(ParameterNames.Value)
        If String.IsNullOrEmpty(menuPath) Then
            Throw New InvalidOperationException(My.Resources.CanNotSelectMenuItemNoMenuPathSpecified)
        End If
        Dim items As List(Of String) = ParseMenuPath(menuPath)

        For Each item As String In items
            Dim children As Object = GetProperty(child, "Children")
            Dim childCount As Integer = CInt(GetProperty(children, "Count"))

            Dim found As Boolean = False
            For i As Integer = 0 To childCount - 1
                Dim menuItem As Object = GetProperty(children, "Item", i)
                Dim name As String = CStr(GetProperty(menuItem, "Name"))

                If item = name Then
                    child = menuItem
                    found = True
                    Exit For
                End If
            Next
            If Not found Then
                Throw New InvalidOperationException(String.Format(My.Resources.TheMenuItem0DidnTMatch, item))
            End If
        Next

        InvokeMethod(child, "Select")
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Helper method for invoking methods using reflection.
    ''' </summary>
    ''' <param name="o">The object to invoke the method against</param>
    ''' <param name="name">The name of the method</param>
    ''' <param name="args">The arguments to pass the method (parameter array)</param>
    ''' <returns>The object that the method returns</returns>
    Private Function InvokeMethod(ByVal o As Object, ByVal name As String, ByVal ParamArray args() As Object) As Object
        Return o.GetType().InvokeMember(name, BindingFlags.InvokeMethod, Nothing, o, args)
    End Function

    ''' <summary>
    ''' Helper method to get the value of a property using reflection.
    ''' </summary>
    ''' <param name="o">The object on which to invoke the property</param>
    ''' <param name="name">The name of the property to invoke</param>
    ''' <returns>The object returned by the property</returns>
    Private Function GetProperty(ByVal o As Object, ByVal name As String, ByVal ParamArray args() As Object) As Object
        Return o.GetType().InvokeMember(name, BindingFlags.GetProperty, Nothing, o, args)
    End Function

    ''' <summary>
    ''' Helper method to set the value of a property using reflection.
    ''' </summary>
    ''' <param name="o">The object on which to invoke the property.</param>
    ''' <param name="name">The name of the property to set.</param>
    ''' <param name="value">The value to set the property to.</param>
    Private Sub SetProperty(ByVal o As Object, ByVal name As String, ByVal ParamArray value() As Object)
        o.GetType().InvokeMember(name, BindingFlags.SetProperty, Nothing, o, value)
    End Sub


    <Category(Category.SAP)>
    <Command("Get a property of a SAP Gui component")>
    <Parameters("ID of the component, and PropName is the name of the property. " &
       "Optionally, retproc specifies additional processing on the returned value, " &
       "which is otherwise just returned as a string. If 'colcount' is specified, it " &
       "is assumed to be a GuiCollection, and the count of the items is returned. " &
       "Optionally, targetprop specifies the name of a property of the identified " &
       "component, on which the action will actually be performed. " &
       "Optionally, Arguments is any arguments required to access the " &
       "property, as a comma-separated list." &
       "The arguments are given as a comma-separated list. There must be the " &
       "correct number of arguments. A comma can be escaped with a backslash " &
       "followed by a c, and a backslash by two backslashes. " &
       "Preceding an entry with # means it will be intepreted as an " &
       "integer, and with a ! means a boolean. Anything else is a string")>
    Private Function ProcessCommandSapGetProperty(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim retproc As String = objQuery.GetParameter(ParameterNames.RetProc)
        Dim targetprop As String = objQuery.GetParameter(ParameterNames.TargetProp)
        Dim propname As String = objQuery.GetParameter(ParameterNames.PropName)
        If propname Is Nothing Then Throw New InvalidOperationException(My.Resources.NoPropertyNameSpecified)
        Dim arguments As String = objQuery.GetParameter(ParameterNames.Arguments)
        Dim pargs As Object() = SapProcessArgs(arguments)

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        If targetprop IsNot Nothing Then
            child = GetProperty(child, targetprop, Nothing)
            If child Is Nothing Then Throw New InvalidOperationException(String.Format(My.Resources.MissingTarget0, targetprop))
        End If
        Dim prop As Object = GetProperty(child, propname)
        Return ProcessSapRetVal(prop, child, retproc)
    End Function


    <Category(Category.SAP)>
    <Command("Set a property of a SAP Gui component")>
    <Parameters("ID of the component, PropName is the name of the property. " &
       "Arguments is any arguments required to access the " &
       "property, including the value to set." &
       "Optionally, targetprop specifies the name of a property of the identified " &
       "component, on which the action will actually be performed. " &
       "The arguments are given as a comma-separated list. There must be the " &
       "correct number of arguments. A comma can be escaped with a backslash " &
       "followed by a c, and a backslash by two backslashes. " &
       "Preceding an entry with # means it will be intepreted as an " &
       "integer, with a @ means it will be intepreted as date and turned into a SAP format date, " &
       "and with a ! means a boolean. Anything else is a string")>
    Private Function ProcessCommandSapSetProperty(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim targetprop As String = objQuery.GetParameter(ParameterNames.TargetProp)
        Dim propname As String = objQuery.GetParameter(ParameterNames.PropName)
        If propname Is Nothing Then Throw New InvalidOperationException(My.Resources.NoPropertyNameSpecified)
        Dim arguments As String = objQuery.GetParameter(ParameterNames.Arguments)
        Dim pargs As Object() = SapProcessArgs(arguments)
        If pargs Is Nothing Then Throw New InvalidOperationException(My.Resources.AtLeastOneArgumentRequiredWhenSettingAProperty)

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        If targetprop IsNot Nothing Then
            child = GetProperty(child, targetprop)
            If child Is Nothing Then Throw New InvalidOperationException(String.Format(My.Resources.MissingTarget0, targetprop))
        End If
        SetProperty(child, propname, pargs)
        Return Reply.Ok
    End Function

    <Category(Category.SAP)>
    <Command("Invoke a method of a SAP Gui component")>
    <Parameters("ID of the component, MethodName is the name of the method." &
       "Optionally, retproc specifies additional processing on the returned value, " &
       "which is otherwise just returned as a string. If 'colcount' is specified, it " &
       "is assumed to be a GuiCollection, and the count of the items is returned. " &
       "Optionally, targetprop specifies the name of a property of the identified " &
       "component, on which the action will actually be performed. " &
       "Optionally, Arguments is any arguments required to invoke the " &
       "method, as a comma-separated list." &
       "The arguments are given as a comma-separated list. There must be the " &
       "correct number of arguments. A comma can be escaped with a backslash " &
       "followed by a c, and a backslash by two backslashes. " &
       "Preceding an entry with # means it will be intepreted as an " &
       "integer, and with a ! means a boolean. Anything else is a string")>
    Private Function ProcessCommandSapInvokeMethod(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim retproc As String = objQuery.GetParameter(ParameterNames.RetProc)
        Dim targetprop As String = objQuery.GetParameter(ParameterNames.TargetProp)
        Dim methodname As String = objQuery.GetParameter(ParameterNames.MethodName)
        If methodname Is Nothing Then Throw New InvalidOperationException(My.Resources.NoMethodNameSpecified)
        Dim arguments As String = objQuery.GetParameter(ParameterNames.Arguments)
        Dim pargs As Object() = SapProcessArgs(arguments)

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        If targetprop IsNot Nothing Then
            child = GetProperty(child, targetprop)
            If child Is Nothing Then Throw New InvalidOperationException(String.Format(My.Resources.MissingTarget0, targetprop))
        End If
        Dim prop As Object = InvokeMethod(child, methodname, pargs)
        Return ProcessSapRetVal(prop, child, retproc)
    End Function

    <Category(Category.SAP)>
    <Command("Select a date range in a SAP Calendar control")>
    <Parameters("ID of the component")>
    Private Function ProcessCommandSapSelectDateRange(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim arguments As String = objQuery.GetParameter(ParameterNames.Arguments)
        Dim pargs As Object() = SapProcessArgs(arguments)
        If pargs Is Nothing OrElse pargs.Length <> 2 Then Throw New InvalidOperationException(My.Resources.BothStartAndEndDateAreRequired)

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        Dim dateString As String = String.Format("{0},{1}", pargs(0), pargs(1))
        SetProperty(child, "SelectionInterval", dateString)

        Return Reply.Ok
    End Function

    <Category(Category.SAP)>
    <Command("Select a date range in a SAP Calendar control")>
    <Parameters("ID of the component")>
    Private Function ProcessCommandSapGetDateRange(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        Const SAPDateFormat As String = "yyyyMMdd"
        Const BPDateFormat As String = "yyyy/MM/dd"

        Dim val As String = CStr(GetProperty(child, "SelectionInterval"))
        If val Is Nothing OrElse Not val.Contains(",") Then
            Throw New InvalidOperationException(My.Resources.FailedToParseDateRange)
        End If
        Dim aval() As String = Split(val, ",")
        Dim startDate As Date = Nothing
        If Not Date.TryParseExact(aval(0), SAPDateFormat, Nothing, DateTimeStyles.None, startDate) Then
            Throw New InvalidOperationException(My.Resources.FailedToParseStartDate)
        End If
        Dim endDate As Date = Nothing
        If Not Date.TryParseExact(aval(1), SAPDateFormat, Nothing, DateTimeStyles.None, endDate) Then
            Throw New InvalidOperationException(My.Resources.FailedToParseEndDate)
        End If

        Dim xdoc As New XmlDocument()
        Dim colRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(colRoot)

        Dim rowElement As XmlElement = xdoc.CreateElement("row")

        Dim fieldElement As XmlElement = xdoc.CreateElement("field")
        fieldElement.SetAttribute("type", "date")
        fieldElement.SetAttribute("value", startDate.ToString(BPDateFormat))
        fieldElement.SetAttribute("name", "StartDate")
        rowElement.AppendChild(fieldElement)

        fieldElement = xdoc.CreateElement("field")
        fieldElement.SetAttribute("type", "date")
        fieldElement.SetAttribute("value", endDate.ToString(BPDateFormat))
        fieldElement.SetAttribute("name", "EndDate")
        rowElement.AppendChild(fieldElement)

        colRoot.AppendChild(rowElement)

        Return Reply.Result(xdoc.OuterXml)
    End Function

    ''' <summary>
    ''' Convert a result returned from a SAP method/property read according to the
    ''' specification given.
    ''' </summary>
    ''' <param name="val">The return value, some kind of SAP object.</param>
    ''' <param name="el">The SAP control the call was made against.</param>
    ''' <param name="retproc">The specification.</param>
    ''' <returns>The complete return value for the query.</returns>
    Private Function ProcessSapRetVal(ByVal val As Object, ByVal el As Object, ByVal retproc As String) As Reply
        If retproc Is Nothing Then
            If val Is Nothing Then Return Reply.Ok
            Return Reply.Result(val.ToString())
        End If
        Dim p() As String = retproc.Split("/"c)
        Select Case p(0)
            Case "colcount"
                Return Reply.Result(GetProperty(val, "Count").ToString())
            Case "deepcount"
                If p.Length <> 2 Then Throw New InvalidOperationException(My.Resources.InvalidParametersForDeepcount)
                Dim count As Integer = CInt(GetProperty(val, "Count"))
                Dim childkeys As New List(Of String)
                For i As Integer = 0 To count - 1
                    Dim key As String = GetProperty(val, "Item", i).ToString()
                    childkeys.Add(key)
                Next
                While childkeys.Count > 0
                    Dim val1 As Object = InvokeMethod(el, p(1), childkeys.Item(0))
                    Dim ccount As Integer = CInt(GetProperty(val1, "Count"))
                    count += ccount
                    For i As Integer = 0 To ccount - 1
                        Dim key As String = GetProperty(val1, "Item", i).ToString()
                        childkeys.Add(key)
                    Next
                    childkeys.RemoveAt(0)
                End While
                Return Reply.Result(count)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.InvalidRetproc0Specified, retproc))
        End Select
    End Function

    <Category(Category.SAP)>
    <Command("Get a SAP tree node key, given its text")>
    <Parameters("ID of the component, newtext is the text of the node.")>
    Private Function ProcessCommandSapGetNodeKeyByText(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)

        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim nodeText As String = objQuery.GetParameter("newtext")
        If nodeText Is Nothing Then Throw New InvalidOperationException(My.Resources.NoNodetextSpecified)

        Dim child As Object = InvokeMethod(mSAPGuiApplication, "findByID", id, False)
        If child Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifiedSAPGuiComponentNotFound)

        'The API provides no better way to do this than to iterate through all the
        'nodes in the tree until we find one with matching text.
        Dim childKeys As New List(Of String)
        Dim nodes As Object = InvokeMethod(child, "getNodesCol")
        Dim count = CInt(GetProperty(nodes, "Count"))
        For i = 0 To count - 1
            Dim key As String = GetProperty(nodes, "Item", i).ToString()
            Dim txt As String = InvokeMethod(child, "getNodeTextByKey", key).ToString()
            If txt = nodeText Then
                Return Reply.Result(key)
            End If
            childKeys.Add(key)
        Next

        For Each childKey In childKeys
            Dim result = GetNodeKeyWithText(childKey, nodeText, child)
            If Not String.IsNullOrEmpty(result) Then
                Return Reply.Result(result)
            End If
        Next

        Throw New InvalidOperationException(My.Resources.NoNodeWithSpecifiedText)
    End Function

    Private Function GetNodeKeyWithText(key As String, nodeText As String, child As Object) As String

        Dim nodes = InvokeMethod(child, "getSubNodesCol", key)
        Dim count As Integer

        Try
            count = CInt(GetProperty(nodes, "Count"))
        Catch
            count = 0
        End Try

        For i = 0 To count - 1
            Dim childKey = GetProperty(nodes, "Item", i).ToString()
            Dim result = GetNodeKeyWithText(childKey, nodeText, child)
            If Not String.IsNullOrEmpty(result) Then
                Return result
            End If
        Next

        Dim txt As String = InvokeMethod(child, "getNodeTextByKey", key).ToString()
        If txt = nodeText Then
            Return key
        End If

        Return String.Empty
    End Function


    Private Function SapProcessArgs(ByVal arguments As String) As Object()
        If arguments Is Nothing Then Return Nothing
        Dim a As String() = arguments.Split(","c)
        Dim plist As New List(Of Object)
        For Each aa As String In a
            If aa.StartsWith("#") Then
                plist.Add(Integer.Parse(aa.Substring(1)))
            ElseIf aa.StartsWith("!") Then
                plist.Add(Boolean.Parse(aa.Substring(1)))
            ElseIf aa.StartsWith("@") Then
                Dim d As Date = Date.Parse(aa.Substring(1))
                plist.Add(d.ToString("yyyyMMdd"))
            Else
                aa = aa.Replace("\c", ",")
                aa = aa.Replace("\", "")
                plist.Add(aa)
            End If
        Next
        Return plist.ToArray()
    End Function

    <Category(Category.SAP)>
    <Command("Get all SAP GUI components")>
    <Parameters("None")>
    Private Function ProcessCommandSapListAll(ByVal objQuery As clsQuery) As Reply
        If Not GetSapApp() Then Throw New InvalidOperationException(My.Resources.SAPApplicationNotAvailable)
        Dim lst As New StringBuilder()
        SapListAllAppendChildren(lst, mSAPGuiApplication)
        Return Reply.Result(lst.ToString())
    End Function

    Private Sub SapListAllAppendChildren(ByVal lst As StringBuilder, ByVal parent As Object)
        Dim children As Object = Nothing
        Try
            children = parent.GetType().InvokeMember("children", BindingFlags.GetProperty, Nothing, parent, Nothing, Nothing, Nothing, Nothing)
        Catch ex As Exception
            'The object may not have a 'children' property - only objects that implement
            'the GuiVContainer interface have one.
            Return
        End Try
        Dim count As Integer = CInt(children.GetType().InvokeMember("count", BindingFlags.GetProperty, Nothing, children, Nothing, Nothing, Nothing, Nothing))
        For i As Integer = 0 To count - 1
            Dim args(0) As Object
            args(0) = i
            Dim child As Object = children.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, children, args, Nothing, Nothing, Nothing)
            Dim id As String = CStr(child.GetType().InvokeMember("id", BindingFlags.GetProperty, Nothing, child, Nothing, Nothing, Nothing, Nothing))
            Dim comptype As String = CStr(child.GetType().InvokeMember("type", BindingFlags.GetProperty, Nothing, child, Nothing, Nothing, Nothing, Nothing))
            lst.Append(comptype & " - " & id & vbCrLf)
            SapListAllAppendChildren(lst, child)
        Next
    End Sub

    <Category(Category.SAP)>
    <Command("Gets the screen relative bounds of a SAP element.")>
    <Parameters("Those required to uniquely identify the SAP element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the windows screen bounds.")>
    Private Function ProcessCommandSapGetElementScreenBounds(ByVal objQuery As clsQuery) As Reply
        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Return Reply.Result(CreateCollectionXMLFromRectangle(GetSapComponentScreenRect(id)))
    End Function

    <Category(Category.SAP)>
    <Command("Gets the bounds of a SAP element.")>
    <Parameters("Those required to uniquely identify the SAP element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element bounds.")>
    Private Function ProcessCommandSapGetElementBounds(ByVal objQuery As clsQuery) As Reply
        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Return Reply.Result(CreateCollectionXMLFromRectangle(GetSapComponentRect(id)))
    End Function

    <Category(Category.SAP)>
    <Command("Clicks the mouse in the centre of a SAP element.")>
    <Parameters("Those required to uniquely identify the element, plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandSapMouseClickCentre(ByVal objQuery As clsQuery) As Reply
        Dim buttonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim r As RECT = GetSapComponentScreenRect(id)
        Return DoClickMouse(r.Centre, GetButtonFromString(buttonString))
    End Function

    <Category(Category.SAP)>
    <Command("Clicks the mouse in a SAP element.")>
    <Parameters("Those required to uniquely identify the element, plus 'TargX' and 'TargY', plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandSapMouseClick(ByVal objQuery As clsQuery) As Reply
        Dim buttonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim id As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID).MatchValue
        Dim r As RECT = GetSapComponentScreenRect(id)
        Dim targx As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim targy As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)

        Return DoClickMouse(Point.Add(r.Centre, New Size(targx, targy)), GetButtonFromString(buttonString))
    End Function

#End Region

#Region "Query Command Handlers - Java Access Bridge"

    ''' <summary>
    ''' Thread handler to do an action on an JAB element.
    ''' </summary>
    Private Class JABActionHandler
        Public Shared Sub Exec(ByVal o As Object)
            Dim i As JABActionInfo = CType(o, JABActionInfo)
            Using c As JABContext = i.c
                i.JAB.DoAction(c, i.action)
            End Using
        End Sub
    End Class
    ''' <summary>
    ''' Class used to pass info to the JABActionHandler thread class.
    ''' </summary>
    Private Class JABActionInfo
        ''' <summary>
        ''' The JAB context to act on, which should be disposed by the thread when
        ''' it has finished.
        ''' </summary>
        Public c As JABContext
        ''' <summary>
        ''' The action to perform.
        ''' </summary>
        Public action As String
        ''' <summary>
        ''' The JABWrapper object we're using
        ''' </summary>
        Public JAB As JABWrapper
    End Class

    <Category(Category.Java)>
    <Command("Performs an action against a JAVA element")>
    <Parameters("The action specified by 'Action' and those required to uniquely identify the element.")>
    Private Function ProcessCommandJabAction(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim action As String = objQuery.GetParameter(ParameterNames.Action)
            If String.IsNullOrEmpty(action) Then
                Throw New InvalidOperationException(My.Resources.NoActionSpecified)
            End If
            If mbJABActionsMultithreaded Then
                Dim i As New JABActionInfo
                i.c = c
                i.action = action
                i.JAB = mJAB
                'Create a thread that will do the actual action - we'll return
                'immediately once it has been initiated.
                Dim t As Thread = RunThread.CreateThread(AddressOf JABActionHandler.Exec, i)
                t.Start()
                Return Reply.Ok
            Else
                Try
                    If Not mJAB.DoAction(c, action) Then
                        Throw New InvalidOperationException(My.Resources.JavaActionFailed)
                    End If
                    Return Reply.Ok
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilePerformingJavaAction0, ex.Message))
                End Try
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Selects a tab within a Java Tab control")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabSelectTab(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            c.UpdateCachedInfo()
            Dim ac As Long = WAB.getAccessibleParentFromContext(c.vmID, c.AC)
            If ac > 0 Then
                Using parentTabbedPane As New JABContext(ac, c.vmID)
                    parentTabbedPane.UpdateCachedInfo()
                    If parentTabbedPane.Role = "page tab list" Then
                        Return DoSelectItem(parentTabbedPane, c.Name, -1)
                    End If
                End Using
            End If
            Throw New InvalidOperationException(My.Resources.CouldNotFindParentTabControl)
        End Using
    End Function


    ''' <summary>
    ''' Selects the specified item from the supplied combobox.
    ''' </summary>
    ''' <param name="ComboBox">The combo box whose value is to 
    ''' be changed.</param>
    ''' <param name="ItemText">The text of the item to be selected,
    ''' or an empty string to specify the item by index.</param>
    ''' <param name="ItemIndex">Relevant only when the ItemText
    ''' parameter has an empty argument. Specifies the index to
    ''' be selected.</param>
    ''' <returns>Returns a valid query response, indicating the 
    ''' outcome of the operation.</returns>
    Private Function SelectJavaComboBoxItem(ByVal ComboBox As JABContext, ByVal ItemText As String, ByVal ItemIndex As Integer) As Reply
        Dim innerList As JABContext = Me.GetJavaComboBoxInternalList(ComboBox)
        If innerList IsNot Nothing Then
            If Not String.IsNullOrEmpty(ItemText) Then
                ItemIndex = Me.GetJavaChildIndex(innerList, ItemText)
                If ItemIndex = -1 Then
                    Throw New InvalidOperationException(String.Format(My.Resources.CouldNotFindChildWithText0, ItemText))
                End If
            End If
            If ItemIndex > -1 Then
                WAB.requestFocus(ComboBox.vmID, ComboBox.AC)
                WAB.clearAccessibleSelectionFromContext(ComboBox.vmID, ComboBox.AC)
                WAB.addAccessibleSelectionFromContext(ComboBox.vmID, ComboBox.AC, ItemIndex)
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.InvalidIndexPleaseSpecifyAValueFrom1ToTheNumberOfChildItems0, innerList.ChildCount.ToString))
            End If
        Else
            Throw New InvalidOperationException(My.Resources.FailedToFindInternalListWithinComboBox)
        End If
    End Function

    <Category(Category.Java)>
    <Command("Sets the text of a Java element")>
    <Parameters("The value specified by 'NewText' and those required to uniquely identify the element.")>
    Private Function ProcessCommandJabSetText(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim newText As String = objQuery.GetParameter(ParameterNames.NewText)
            Select Case c.Role
                Case "combo box"
                    Return Me.SelectJavaComboBoxItem(c, newText, -1)
                Case Else
                    Dim sErr As String = Nothing
                    If mJAB.SetText(c, newText, sErr) Then
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(sErr)
                    End If
            End Select
        End Using
    End Function


    <Category(Category.Java)>
    <Command("Sets the text of a Java password element, and ensures the plain text password does not remain in memory.")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' 
         to specify the obfuscated password for the Control.")>
    Private Function ProcessCommandJabSetPasswordText(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim newText As String = objQuery.GetParameter(ParameterNames.NewText)
            Dim ss = SafeString.Decode(newText)
            Dim sErr As String = Nothing
            Dim success = mJAB.SetText(c, ss, sErr)
            If success Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(sErr)
            End If
        End Using
    End Function


    <Category(Category.Java)>
    <Command("Gets the text of a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the value of the Java element.")>
    Private Function ProcessCommandJabGetValue(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim value As String = mJAB.GetValue(c)
            If String.IsNullOrEmpty(value) Then
                Dim sErr As String = Nothing
                If Not JABWrapper.GetText(c, value, sErr) Then
                    Throw New InvalidOperationException(My.Resources.FailedToGetTextFromElement & sErr)
                End If
            End If

            Dim castValueTo As String = objQuery.GetParameter(ParameterNames.CastValueTo)
            If Not String.IsNullOrEmpty(castValueTo) Then
                Select Case castValueTo
                    Case "flag"
                        Try
                            value = CType(value, Boolean).ToString
                        Catch ex As Exception
                            Throw New InvalidOperationException(String.Format(My.Resources.FailedToCastValue0ToType1, value, castValueTo))
                        End Try
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.CastNotAvailableTo0, castValueTo))
                End Select
            End If

            Return Reply.Result(value)
        End Using

    End Function

    <Category(Category.Java)>
    <Command("Gets the numeric value of a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the numeric value of the Java element.")>
    Private Function ProcessCommandJabGetNumericValue(ByVal objquery As clsQuery) As Reply
        Return Me.ProcessCommandJabGetValue(objquery)
    End Function

    <Category(Category.Java)>
    <Command("Gets the minimum numeric value of a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the minimum numeric value of the Java element.")>
    Private Function ProcessCommandJabGetMinNumericValue(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            Dim value As New StringBuilder(WAB.MaxJABStringSize)
            If WAB.getMinimumAccessibleValueFromContext(c.vmID, c.AC, value, CShort(value.Capacity)) Then
                Return Reply.Result(value.ToString)
            Else
                Throw New InvalidOperationException(My.Resources.NativeAPICallFailed)
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Scrolls to the minimim end of a Java ScrollBar element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabScrollToMinimum(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            If c.Role <> "scroll bar" Then
                Throw New InvalidOperationException(My.Resources.ScrollToMinimumIsOnlyAvailableForScrollbars)
            End If

            While mJAB.DoAction(c, "decrementBlock")
                'do nothing
            End While

            Dim valueReply As Reply = Me.ProcessCommandJabGetNumericValue(objQuery)
            Dim minValueReply As Reply = Me.ProcessCommandJabGetMinNumericValue(objQuery)
            Dim value As Integer
            Dim minValue As Integer
            If Integer.TryParse(valueReply.Message, value) AndAlso Integer.TryParse(minValueReply.Message, minValue) Then
                If value = minValue Then
                    Return Reply.Ok
                Else
                    Throw New InvalidOperationException(My.Resources.AttemptedToChangeValueWithoutErrorButNewValueDoesNotMatchIntendedValue)
                End If
            End If

            Throw New InvalidOperationException(My.Resources.FailedToValidateNewValueAgainstAdvertisedMinimum)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets whether the Java element is selected.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandJabIsSelected(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            c.UpdateCachedInfo()
            Return Reply.Result(c.Selected)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets whether the Java element is checked or expanded.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandJabIsExpanded(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            Dim value As Boolean
            c.UpdateCachedInfo()
            Select Case c.Role
                Case "menu"
                    value = c.Checked
                Case Else
                    value = c.Expanded
            End Select
            Return Reply.Result(value)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Expands a treenode Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabExpandTreeNode(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            c.UpdateCachedInfo()
            If c.Role = "label" Then
                If c.AllowsAction("toggleexpand") OrElse c.AllowsAction("toggle expand") Then
                    If c.Expandable AndAlso (Not c.Expanded) Then
                        Return Me.ProcessCommandJabToggleTreeNode(objquery)
                    Else
                        Return Reply.Ok
                    End If
                End If
            End If

            Throw New InvalidOperationException(My.Resources.SpecifiedElementDoesNotAppearToBeExpandable)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Collapses a treenode Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabCollapseTreeNode(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            c.UpdateCachedInfo()
            If c.Role = "label" Then
                If c.AllowsAction("toggleexpand") OrElse c.AllowsAction("toggle expand") Then
                    If c.Expandable AndAlso (Not c.Expanded) Then
                        Return Reply.Ok
                    Else
                        Return Me.ProcessCommandJabToggleTreeNode(objquery)
                    End If
                End If
            End If

            Throw New InvalidOperationException(My.Resources.SpecifiedElementDoesNotAppearToBeCollapsable)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Toggles a treenode Java element i.e if its expanded then collapse it, if its collapsed then expand it.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabToggleTreeNode(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            c.UpdateCachedInfo()
            If c.Role = "label" Then
                If c.AllowsAction("toggleexpand") Then
                    If mJAB.DoAction(c, "toggleexpand") Then
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(My.Resources.APICallFailed_1)
                    End If
                ElseIf c.AllowsAction("toggle expand") Then
                    If mJAB.DoAction(c, "toggle expand") Then
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException("API call failed")
                    End If
                End If
            End If

            Throw New InvalidOperationException(My.Resources.SpecifiedElementDoesNotAppearToBeTogglable)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets the maximum numeric value of a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the maximum numeric value of the Java element.")>
    Private Function ProcessCommandJabGetMaxNumericValue(ByVal objquery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objquery, mJAB)
            Dim value As New StringBuilder(WAB.MaxJABStringSize)
            If WAB.getMaximumAccessibleValueFromContext(c.vmID, c.AC, value, CShort(value.Capacity)) Then
                Return Reply.Result(value.ToString)
            Else
                Throw New InvalidOperationException(My.Resources.NativeAPICallFailed)
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets the selected text from the Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<text>"" where <text> is the selected text of the Java element.")>
    Private Function ProcessCommandJabGetSelectedText(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim text As String = Nothing
            Dim sErr As String = Nothing
            Dim success As Boolean = mJAB.GetSelectedText(c, text, sErr)
            If success Then
                Return Reply.Result(text)
            Else
                Throw New InvalidOperationException(sErr)
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Selects the text in a Java element.")>
    <Parameters("The part of the text to select specified by 'Position' and 'Length' and those required to uniquely identify the element.")>
    Private Function ProcessCommandJabSelectText(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
            Dim index As Integer
            If Not Integer.TryParse(positionString, index) Then
                Throw New InvalidOperationException(String.Format(My.Resources.CouldNotParseValue0AsNumber, positionString))
            Else
                index -= 1 'Correction for zero-based index
            End If

            Dim lengthString As String = objQuery.GetParameter(ParameterNames.Length)
            Dim length As Integer
            If Not Integer.TryParse(lengthString, length) Then
                Throw New InvalidOperationException(String.Format(My.Resources.CouldNotParseValue0AsNumber, positionString))
            End If

            Dim errorMessage As String = String.Empty
            Try
                If WAB.selectTextRange(c.vmID, c.AC, index, index + length) Then
                    'Merely to show up selection:
                    WAB.requestFocus(c.vmID, c.AC)
                    Return Reply.Ok
                Else
                    errorMessage = My.Resources.APICallFailed_1
                End If
            Catch ex As Exception
                errorMessage = ex.Message
            End Try

            Throw New InvalidOperationException(My.Resources.CouldNotSelectText & errorMessage)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Selects all the text in a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabSelectAllText(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim info As WAB.AccessibleTextInfo
            If Not WAB.getAccessibleTextInfo(c.vmID, c.AC, info, 0, 0) Then
                Throw New InvalidOperationException(My.Resources.FailedToDiscoverLengthOfText)
            End If

            If WAB.selectTextRange(c.vmID, c.AC, 0, info.charCount - 1) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(My.Resources.FailedToSelectRequestedText)
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets all the text in a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<text>"" where <text> is the text of the Java element.")>
    Private Function ProcessCommandJabGetText(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim activeElement As JABContext = Nothing
            Select Case c.Role
                Case "combo box"
                    Dim ActiveDescendentAC As Long = WAB.getActiveDescendent(c.vmID, c.AC)
                    If ActiveDescendentAC > 0 Then
                        Dim ActiveDescendent As New JABContext(ActiveDescendentAC, c.vmID)
                        ActiveDescendent.UpdateCachedInfo()
                        activeElement = ActiveDescendent
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToGetComboBoxSActiveDescendent)
                    End If
                Case Else
                    activeElement = c
            End Select

            Dim text As String = Nothing
            Dim sErr As String = Nothing
            Dim success As Boolean = JABWrapper.GetText(activeElement, text, sErr)
            If success Then
                Return Reply.Result(text)
            Else
                Throw New InvalidOperationException(sErr)
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Checks that a Java element can be identified.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandJabCheckExists(ByVal objQuery As clsQuery) As Reply
        Try
            CheckJabReady()
            Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            End Using
        Catch ex As ApplicationException
            Return Reply.Result(False)
        End Try
        Return Reply.Result(True)
    End Function

    <Category(Category.Java)>
    <Command("Focuses a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabFocus(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            If WAB.requestFocus(c.vmID, c.AC) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(My.Resources.RequestForFocusWasRefusedByJavaAccessBridge)
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Determines whether a Java element is focused.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandJabIsFocused(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            c.UpdateCachedInfo()
            Return Reply.Result(c.Focused)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Selects all Java elements within a parent Java element")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabSelectAllItems(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Select Case c.Role
                Case "table"
                    'The ordinary "select all" method doesn't work. The following assumes that the table
                    'accepts full row selection, so it merely selects the first cell in each row, in turn.
                    WAB.clearAccessibleSelectionFromContext(c.vmID, c.AC)
                    Dim tableInfo As New WAB.AccessibleTableMethods.AccessibleTableInfo
                    If WAB.AccessibleTableMethods.getAccessibleTableInfo(c.vmID, c.AC, tableInfo) Then
                        For rowIndex As Integer = 0 To tableInfo.RowCount - 1
                            Dim cellInfo As WAB.AccessibleTableMethods.AccessibleTableCellInfo
                            If WAB.AccessibleTableMethods.getAccessibleTableCellInfo(c.vmID, c.AC, rowIndex, 0, cellInfo) Then
                                If Not cellInfo.isSelected Then
                                    WAB.addAccessibleSelectionFromContext(c.vmID, c.AC, cellInfo.index)
                                End If
                            Else
                                Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetInfoForRow0, (rowIndex + 1).ToString))
                            End If
                        Next
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToGetTableInfo)
                    End If
                Case Else
                    WAB.selectAllAccessibleSelectionFromContext(c.vmID, c.AC)
                    Return Reply.Ok
            End Select
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Deselects all currently selected Java elements.")>
    <Parameters("None.")>
    Private Function ProcessCommandJabClearSelection(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            WAB.clearAccessibleSelectionFromContext(c.vmID, c.AC)
            Return Reply.Ok
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Selects an Item within a Java element")>
    <Parameters("Either the name of the item specified by 'NewText' or the index of the item specified by 'Position' as well as those required to uniquely identify the element.")>
    Private Function ProcessCommandJabSelectItem(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            'The item text to be selected. If this is null then
            'we use the position parameter
            Dim itemText As String = objQuery.GetParameter(ParameterNames.NewText)
            'The 1-based index of the item to be selected.
            'Only used when itemtext is null, in which case this may not be null
            Dim position As String = objQuery.GetParameter(ParameterNames.Position)
            'Corresponds to Position, but is a zero-based index
            Dim index As Integer = -1

            'Do parameter checks
            If String.IsNullOrEmpty(itemText) Then
                If String.IsNullOrEmpty(position) Then
                    Throw New InvalidOperationException(My.Resources.AtLeastOneOfItemTextOrPositionMustBeSpecified)
                Else
                    If Integer.TryParse(position, index) Then
                        index -= 1 'Correction for zero-based index
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToIntepretValueForPositionAsAWholeNumber)
                    End If
                End If
            End If

            'Find the component we really need to work on - eg
            'for a combo box we need the internal list
            c.UpdateCachedInfo()
            Select Case c.Role
                Case "combo box"
                    Return Me.SelectJavaComboBoxItem(c, itemText, index)
                Case "popup menu"
                    'Item text trumps the index parameter
                    If Not String.IsNullOrEmpty(itemText) Then
                        index = Me.GetJavaChildIndex(c, itemText)
                        If index = -1 Then
                            Throw New InvalidOperationException(String.Format(My.Resources.CouldNotFindChildWithText0, itemText))
                        End If
                    End If

                    If c.Children IsNot Nothing AndAlso c.Children.Count > 0 AndAlso index < c.Children.Count Then
                        Dim childToActOn As JABContext = c.Children(index)
                        If childToActOn.AllowsAction("click") Then
                            If mJAB.DoAction(childToActOn, "click") Then
                                Return Reply.Ok
                            End If
                        End If
                    End If

                    'Default in case the prefered method fails
                    Return DoSelectItem(c, itemText, index)

                Case "tool bar"
                    If index < 0 OrElse index > c.Children.Count - 1 Then
                        Throw New InvalidOperationException(String.Format(My.Resources.TheJavaElementIdentifiedHasOnly0ChildrenThereIsNoChildAtPosition1, c.Children.Count, (index + 1).ToString))
                    End If

                    Dim childButton As JABContext = c.Children(index)
                    mJAB.DoAction(childButton, "click")
                    Return Reply.Ok
                Case "table"
                    Dim tableInfo As WAB.AccessibleTableMethods.AccessibleTableInfo
                    If Not WAB.AccessibleTableMethods.getAccessibleTableInfo(c.vmID, c.AC, tableInfo) Then
                        Throw New InvalidOperationException(My.Resources.FailedToRetrieveTableInfo)
                    End If

                    'Decide the index of the item to be selected
                    Dim indexToSelect As Integer = -1
                    If String.IsNullOrEmpty(itemText) Then
                        Dim cellInfo As WAB.AccessibleTableMethods.AccessibleTableCellInfo
                        If Not WAB.AccessibleTableMethods.getAccessibleTableCellInfo(c.vmID, c.AC, index, 0, cellInfo) Then
                            Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveCellInfoForRow0, (index + 1).ToString))
                        End If
                        indexToSelect = cellInfo.index
                    Else
                        'Loop through each row examining the text in the first column
                        For rowIndex As Integer = 0 To tableInfo.RowCount - 1
                            Dim cellInfo As WAB.AccessibleTableMethods.AccessibleTableCellInfo
                            If Not WAB.AccessibleTableMethods.getAccessibleTableCellInfo(c.vmID, c.AC, rowIndex, 0, cellInfo) Then
                                Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveCellInfoForRow0, (rowIndex + 1).ToString))
                            End If
                            Using cellContext As New JABContext(cellInfo.accessibleContext, c.vmID)
                                cellContext.UpdateCachedInfo()


                                Dim cellText As String = String.Empty
                                Dim sErr As String = Nothing
                                If JABWrapper.GetText(cellContext, cellText, sErr) Then
                                    If itemText = cellText Then
                                        indexToSelect = cellInfo.index
                                        Exit For 'We select the first matching item only
                                    End If
                                Else
                                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveTextOfRow01, (cellInfo.index + 1).ToString, sErr))
                                End If
                            End Using
                        Next
                    End If

                    If indexToSelect > -1 Then
                        WAB.addAccessibleSelectionFromContext(c.vmID, c.AC, indexToSelect)
                    End If
                    Return Reply.Ok

                Case "tree"
                    If String.IsNullOrEmpty(itemText) Then
                        If index > -1 Then
                            If index < c.Children.Count Then
                                WAB.addAccessibleSelectionFromContext(c.vmID, c.AC, index)
                                Return Reply.Ok
                            Else
                                Throw New InvalidOperationException(String.Format(My.Resources.IndexOutOfRangeChildCountIs0, c.Children.Count))
                            End If
                        Else
                            Throw New InvalidOperationException(My.Resources.IndexMustBeAtLeastZeroIfNoTextIsSpecified)
                        End If

                    Else
                        'We have to search for it
                        For Each ChildNode As JABContext In c.Children
                            Dim foundNode As JABContext = Me.FindJavaTreenode(ChildNode, itemText)
                            If foundNode IsNot Nothing Then
                                Dim parentAC As Long = WAB.getAccessibleParentFromContext(foundNode.vmID, foundNode.AC)
                                If parentAC > 0 Then
                                    foundNode.UpdateCachedInfo()
                                    WAB.addAccessibleSelectionFromContext(foundNode.vmID, parentAC, foundNode.IndexInParent)
                                    Return Reply.Ok
                                Else
                                    Throw New InvalidOperationException(My.Resources.FoundANodeOfInterestButFailedToIdentifyFoundNodeSParent)
                                End If
                            End If
                        Next

                        Throw New InvalidOperationException(My.Resources.NoNodesWithTheRequestedTextCouldBeFound)
                    End If

                Case Else
                    Return DoSelectItem(c, itemText, index)
            End Select
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Ensures that a item within a scrollable container Java element is visible.")>
    <Parameters("Either the name of the item specified by 'NewText' or the index of the item specified by 'Position' as well as those required to uniquely identify the element.")>
    Private Function ProcessCommandJabEnsureItemVisible(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            'The item text to be selected. If this is null then
            'we use the position parameter
            Dim itemText As String = objQuery.GetParameter(ParameterNames.NewText)
            'The 1-based index of the item to be selected.
            'Only used when itemtext is null, in which case this may not be null
            Dim position As String = objQuery.GetParameter(ParameterNames.Position)
            'Corresponds to Position, but is a zero-based index
            Dim index As Integer = -1

            'Do parameter checks
            If String.IsNullOrEmpty(itemText) Then
                If String.IsNullOrEmpty(position) Then
                    Throw New InvalidOperationException(My.Resources.AtLeastOneOfItemTextOrPositionMustBeSpecified)
                Else
                    If Integer.TryParse(position, index) Then
                        index -= 1 'Correction for zero-based index
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToIntepretValueForPositionAsAWholeNumber)
                    End If
                End If
            End If

            Select Case c.Role
                Case "tree"
                    If String.IsNullOrEmpty(itemText) Then
                        If index > -1 Then
                            If index < c.Children.Count Then
                                Return Reply.Ok
                            Else
                                Throw New InvalidOperationException(String.Format(My.Resources.IndexOutOfRangeChildCountIs0, c.Children.Count))
                            End If
                        Else
                            Throw New InvalidOperationException(My.Resources.IndexMustBeAtLeastZeroIfNoTextIsSpecified)
                        End If

                    Else
                        'We have to search for it
                        For Each ChildNode As JABContext In c.Children
                            Dim foundNode As JABContext = Me.FindJavaTreenode(ChildNode, itemText)
                            If foundNode IsNot Nothing Then

                                'Ascend the tree of ancestors, expanding each node
                                Dim parentAC As Long = WAB.getAccessibleParentFromContext(ChildNode.vmID, ChildNode.AC)
                                Dim parent As New JABContext(parentAC, c.vmID)
                                Dim success As Boolean = True
                                While parentAC > 0 AndAlso success
                                    success = mJAB.DoAction(parent, "toggle expansion")
                                    parentAC = WAB.getAccessibleParentFromContext(ChildNode.vmID, parentAC)
                                    parent = New JABContext(parentAC, c.vmID)
                                End While

                                'Check we reached the top of the tree
                                parent.UpdateCachedInfo()
                                If parent.Role = "tree" Then
                                    Return Reply.Ok
                                Else
                                    Throw New InvalidOperationException(My.Resources.FoundNodeOfInterestButFailedToExpandAllAncestors)
                                End If
                            End If
                        Next

                        Throw New InvalidOperationException(My.Resources.FailedToFindNodeOfInterest)
                    End If
                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.EnsureItemVisibleIsNotImplementedForElementsWithRole0, c.Role))
            End Select
        End Using
    End Function

    ''' <summary>
    ''' Gets the child index of the java element with the specified text.
    ''' </summary>
    ''' <param name="Parent">The element whose children are to be inspected.</param>
    ''' <param name="ItemText">The text sought.</param>
    ''' <returns>Carries back the zero-based index of the first matching child
    ''' found, or -1 if no such child is found.</returns>
    Private Function GetJavaChildIndex(ByVal Parent As JABContext, ByVal ItemText As String) As Integer
        If Not String.IsNullOrEmpty(ItemText) Then
            Parent.UpdateCachedInfo()
            For i As Integer = 0 To Parent.Children.Count - 1
                Dim childText As String = String.Empty
                Dim sErr As String = Nothing
                JABWrapper.GetText(Parent.Children(i), childText, sErr)
                If ItemText.CompareTo(childText) = 0 Then
                    Return i
                End If
            Next
        End If

        Return -1
    End Function

    ''' <summary>
    ''' Selects the item under the specified component, with the supplied text (or
    ''' index), as appropriate.
    ''' </summary>
    ''' <param name="ComponentToActOn">The component whose children are to be
    ''' inspected and selected.</param>
    ''' <param name="ItemText">The text sought. The first child with matching text
    ''' will be selected. If left blank, the index parameter is used instead.</param>
    ''' <param name="ItemIndex">When the ItemText parameter is blank, specifies the
    ''' index of the child to be selected. Also, carries back the index of the child
    ''' ultimately selected.</param>
    ''' <returns>Returns a query response.</returns>
    Private Function DoSelectItem(ByVal ComponentToActOn As JABContext, ByVal ItemText As String, ByRef ItemIndex As Integer) As Reply
        'Get the index, if using text
        If Not String.IsNullOrEmpty(ItemText) Then
            ItemIndex = Me.GetJavaChildIndex(ComponentToActOn, ItemText)
        Else
            'The index variable should already be valid.
        End If

        'Check that the index is valid
        If (ItemIndex < 0) OrElse (ItemIndex > ComponentToActOn.Children.Count - 1) Then
            Throw New InvalidOperationException(String.Format(My.Resources.IndexIsOutOfRangeHighestZeroBasedValueAllowedIs0, ComponentToActOn.Children.Count - 1))
        End If

        WAB.addAccessibleSelectionFromContext(ComponentToActOn.vmID, ComponentToActOn.AC, ItemIndex)
        Return Reply.Ok
    End Function


    <Category(Category.Java)>
    <Command("Gets the number of selected items within a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the number of items selected.")>
    Private Function ProcessCommandJabGetSelectedItemCount(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim result As Integer
            Select Case c.Role
                Case "table"
                    result = WAB.AccessibleTableMethods.getAccessibleTableRowSelectionCount(c.vmID, c.AC)
                Case Else
                    result = WAB.getAccessibleSelectionCountFromContext(c.vmID, c.AC)
            End Select

            Return Reply.Result(result)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Shows the dropdown menu of a Menu or ComboBox Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabShowDropdown(ByVal objQuery As clsQuery) As Reply
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim action As String = Nothing
            Select Case jc.Role
                Case "menu"
                    Dim parentAC As Long = WAB.getAccessibleParentFromContext(jc.vmID, jc.AC)
                    If parentAC > 0 Then
                        WAB.addAccessibleSelectionFromContext(jc.vmID, parentAC, jc.IndexInParent)
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToFindMenuSParent)
                    End If
                Case "combo box"
                    If Not jc.HasState("expanded") Then
                        action = "togglePopup"
                        WAB.requestFocus(jc.vmID, jc.AC)
                    Else
                        Return Reply.Ok
                    End If
                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.ShowDropdownIsNotImplementedForTheJavaElementWithRole0, jc.Role))
            End Select

            If mJAB.DoAction(jc, action) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.DoActionFailedWithParameter0, action))
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Hides the dropdown menu of a Menu or ComboBox Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandJabHideDropdown(ByVal objQuery As clsQuery) As Reply
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim action As String = String.Empty
            Select Case jc.Role
                Case "menu"
                    Dim parentAC As Long = WAB.getAccessibleParentFromContext(jc.vmID, jc.AC)
                    If parentAC > 0 Then
                        WAB.clearAccessibleSelectionFromContext(jc.vmID, parentAC)
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToFindMenuSParent)
                    End If
                Case "combo box"
                    If jc.Expanded Then
                        action = "togglePopup"
                        WAB.requestFocus(jc.vmID, jc.AC)
                    Else
                        Return Reply.Ok
                    End If
                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.HideDropdownIsNotImplementedForTheJavaElementWithRole0, jc.Role))
            End Select

            If mJAB.DoAction(jc, action) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.DoActionFailedWithParameter0, action))
            End If
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets the bounds of a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element bounds.")>
    Private Function ProcessCommandJabGetElementBounds(ByVal objQuery As clsQuery) As Reply
        Me.CheckJabReady()
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)

            Return Reply.Result(CreateCollectionXMLFromRectangle(jc.ClientBounds))
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets the screen relative bounds of a Java element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element screen bounds.")>
    Private Function ProcessCommandJabGetElementScreenBounds(ByVal objQuery As clsQuery) As Reply
        Me.CheckJabReady()
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Return Reply.Result(CreateCollectionXMLFromRectangle(jc.ScreenBounds))
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Gets all items from a Java Table ComboBox ListView or Menu element.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml containing the items of the Java element.")>
    Private Function ProcessCommandJabGetAllItems(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            c.UpdateCachedInfo()
            Dim selectedItemsOnly As Boolean
            Try
                selectedItemsOnly = Boolean.Parse(objQuery.GetParameter(ParameterNames.SelectedItemsOnly))
            Catch ex As Exception
                selectedItemsOnly = False
            End Try

            Select Case c.Role
                Case "table"
                    Dim tableInfo As New WAB.AccessibleTableMethods.AccessibleTableInfo
                    If WAB.AccessibleTableMethods.getAccessibleTableInfo(c.vmID, c.AC, tableInfo) Then

                        'Get field names from the column headers.
                        'The header itself is a table, we get this table and then
                        'extract the cell data as column headers.
                        Dim columnNames As New List(Of String)
                        Dim headerInfo As New WAB.AccessibleTableMethods.AccessibleTableInfo
                        If WAB.AccessibleTableMethods.getAccessibleTableColumnHeader(c.vmID, c.AC, headerInfo) Then

                            'Assume only one row - loop through the columns adding the data as headers
                            For col As Integer = 0 To tableInfo.ColumnCount - 1
                                Dim columnName As String = Nothing

                                Dim cellinfo As WAB.AccessibleTableMethods.AccessibleTableCellInfo
                                If WAB.AccessibleTableMethods.getAccessibleTableCellInfo(c.vmID, headerInfo.AccessibleTableAC, 0, col, cellinfo) Then
                                    Using cell As New JAB.JABContext(cellinfo.accessibleContext, c.vmID)
                                        'Use this column name, unless it's already been used. (See bug #4704)
                                        If Not columnNames.Contains(cell.Name) Then
                                            columnName = cell.Name
                                        End If
                                    End Using   'Important to avoid memory leaks
                                End If

                                If columnName Is Nothing OrElse Not IsValidCollectionFieldName(columnName) Then
                                    columnName = "Column " & (col + 1).ToString
                                End If

                                columnNames.Add(columnName)
                            Next
                        Else
                            For i As Integer = 0 To tableInfo.ColumnCount - 1
                                columnNames.Add("Column " & (i + 1).ToString)
                            Next
                        End If

                        'Prepare xml document for return value
                        Dim xdoc As New XmlDocument()
                        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
                        xdoc.AppendChild(collectionRoot)

                        For RowIndex As Integer = 0 To tableInfo.RowCount - 1
                            If (Not selectedItemsOnly) OrElse WAB.AccessibleTableMethods.isAccessibleTableRowSelected(c.vmID, c.AC, RowIndex) Then
                                Dim rowElement As XmlElement = xdoc.CreateElement("row")

                                For columnIndex As Integer = 0 To tableInfo.ColumnCount - 1

                                    Dim cellReference As String = "(" & RowIndex.ToString & ", " & columnIndex.ToString & ")"
                                    Dim ci As WAB.AccessibleTableMethods.AccessibleTableCellInfo
                                    If WAB.AccessibleTableMethods.getAccessibleTableCellInfo(c.vmID, c.AC, RowIndex, columnIndex, ci) Then

                                        'Decide on the text representing the cell data. Ideally we
                                        'would use WAB.getVirtualAccessibleName for its built-in
                                        'intelligence, but unfortunately it doesn't seem to work
                                        '(marshalling issues?). Instead we just get the name, or
                                        'the accessible value
                                        Dim cellText As String = Nothing

                                        Using CellContext As New JAB.JABContext(ci.accessibleContext, c.vmID)
                                            Try
                                                'First just get name
                                                CellContext.UpdateCachedInfo()
                                                cellText = CellContext.Name

                                                'If name is blank see if the value is any good
                                                If String.IsNullOrEmpty(cellText) Then
                                                    Dim temp As New StringBuilder(WAB.MaxJABStringSize)
                                                    If WAB.getCurrentAccessibleValueFromContext(c.vmID, ci.accessibleContext, temp, CType(temp.Capacity, Short)) Then
                                                        cellText = temp.ToString
                                                    End If
                                                End If
                                            Catch ex As Exception
                                                Throw New InvalidOperationException(String.Format(My.Resources.FailureWhilstRetrievingDataForCell0, cellReference))
                                            End Try
                                        End Using

                                        'Create xml representing the new data cell
                                        Dim fieldelement As XmlElement = xdoc.CreateElement("field")
                                        fieldelement.SetAttribute("type", "text")
                                        fieldelement.SetAttribute("value", cellText)
                                        fieldelement.SetAttribute("name", columnNames(columnIndex))
                                        rowElement.AppendChild(fieldelement)
                                    Else
                                        Throw New InvalidOperationException(String.Format(My.Resources.CouldNotGetCellInformationForCell0, cellReference))
                                    End If
                                Next

                                collectionRoot.AppendChild(rowElement)
                            End If
                        Next

                        Return Reply.Result(xdoc.OuterXml)
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToRetrieveTableInformation)
                    End If

                Case "combo box"

                    Try
                        'Find the internal list, and return its collection of items
                        Using CurrentContext As JABContext = Me.GetJavaComboBoxInternalList(c)
                            If CurrentContext IsNot Nothing Then
                                If CurrentContext.Role = "list" Then
                                    Return GetJabListItems(CurrentContext, selectedItemsOnly)
                                End If
                            End If

                            'Default:
                            Throw New InvalidOperationException(My.Resources.CannotCountChildrenFailedToFindDescendentComponentWithRoleOfList)
                        End Using
                    Catch ex As Exception
                        Throw New InvalidOperationException(My.Resources.CouldNotDescendTreeOfComboBoxChildren)
                    Finally

                    End Try

                Case "list"
                    Return GetJabListItems(c, selectedItemsOnly)
                Case "menu", "popup menu"

                    Dim FieldName As String = "Item Text"

                    'Prepare xml document
                    Dim xdoc As New XmlDocument()
                    Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
                    xdoc.AppendChild(collectionRoot)

                    For Each jcChild As JABContext In c.Children
                        jcChild.UpdateCachedInfo()

                        If (Not selectedItemsOnly) OrElse jcChild.Selected Then
                            Dim NewRow As XmlElement = xdoc.CreateElement("row")

                            'Get the text
                            Dim sErr As String = Nothing
                            Dim itemText As String = String.Empty
                            If Not JABWrapper.GetText(jcChild, itemText, sErr) Then
                                Throw New InvalidOperationException(String.Format(My.Resources.CouldNotRetrieveTextForItem01, (c.Children.IndexOf(jcChild) + 1).ToString, sErr))
                            End If

                            'Failsafe
                            If String.IsNullOrEmpty(itemText) Then
                                Dim temp As New StringBuilder(WAB.MaxJABStringSize)
                                If WAB.getCurrentAccessibleValueFromContext(jcChild.vmID, jcChild.AC, temp, CType(temp.Capacity, Short)) Then
                                    itemText = temp.ToString
                                End If
                            End If

                            'Create xml representing the menu item
                            Dim fieldelement As XmlElement = CreateCollectionFieldXML(xdoc, itemText, "text", FieldName)
                            NewRow.AppendChild(fieldelement)
                            collectionRoot.AppendChild(NewRow)
                        End If

                        jcChild.Dispose()
                    Next

                    Return Reply.Result(xdoc.OuterXml)
                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.GetAllItemsIsNotImplementedForJavaElementsWithRole0, c.Role))
            End Select
        End Using
    End Function

    Private Function GetJabListItems(ByVal c As JABContext, ByVal SelectedItemsOnly As Boolean) As Reply
        Dim fieldName As String = "Item Text"
        'Prepare xml document
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)

        For Each jcChild As JABContext In c.Children
            jcChild.UpdateCachedInfo()

            If (Not SelectedItemsOnly) OrElse jcChild.Selected Then
                Dim NewRow As XmlElement = xdoc.CreateElement("row")

                Dim ItemName As String = jcChild.Name

                If String.IsNullOrEmpty(ItemName) Then
                    Dim Temp As New StringBuilder(WAB.MaxJABStringSize)
                    If WAB.getCurrentAccessibleValueFromContext(jcChild.vmID, jcChild.AC, Temp, CType(Temp.Capacity, Short)) Then
                        ItemName = Temp.ToString
                    End If
                End If

                'Create xml representing the new list item
                Dim fieldelement As XmlElement = xdoc.CreateElement("field")
                fieldelement.SetAttribute("type", "text")
                fieldelement.SetAttribute("value", ItemName)
                fieldelement.SetAttribute("name", fieldName)
                NewRow.AppendChild(fieldelement)

                'Indicate whether it is selected
                If (Not SelectedItemsOnly) Then
                    fieldelement = xdoc.CreateElement("field")
                    fieldelement.SetAttribute("type", "flag")
                    fieldelement.SetAttribute("value", jcChild.Selected.ToString)
                    fieldelement.SetAttribute("name", "Selected")
                    NewRow.AppendChild(fieldelement)
                End If

                collectionRoot.AppendChild(NewRow)
            End If

            jcChild.Dispose()
        Next

        Return Reply.Result(xdoc.OuterXml)
    End Function

    ''' <summary>
    ''' Determine if the given name is valid for a collection column name.
    ''' </summary>
    ''' <param name="name">The name to check.</param>
    ''' <returns>True if the name is valid, False otherwise.</returns>
    Private Function IsValidCollectionFieldName(ByVal name As String) As Boolean
        If name.Length = 0 Then Return False
        If name.Contains("[") OrElse name.Contains("]") Then Return False
        Return True
    End Function


    <Category(Category.Java)>
    <Command("Gets the number of items in the ComboBox ListView or Menu element.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml containing the items of the Java element.")>
    Private Function ProcessCommandJabGetItemCount(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            c.UpdateCachedInfo()
            Select Case c.Role
                Case "table"
                    Dim TableInfo As New WAB.AccessibleTableMethods.AccessibleTableInfo
                    If WAB.AccessibleTableMethods.getAccessibleTableInfo(c.vmID, c.AC, TableInfo) Then
                        Return Reply.Result(TableInfo.RowCount)
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToGetTableInformation)
                    End If
                Case "combo box"
                    Try
                        'This should get us the internal list
                        Using CurrentContext As JABContext = Me.GetJavaComboBoxInternalList(c)

                            'The result is the number of items in that list
                            If CurrentContext IsNot Nothing Then
                                If CurrentContext.Role = "list" Then
                                    CurrentContext.UpdateCachedInfo()
                                    Return Reply.Result(CurrentContext.Children.Count)
                                End If
                            End If

                            'Default:
                            Throw New InvalidOperationException(My.Resources.CannotCountChildrenFailedToFindDescendentComponentWithRoleOfList)
                        End Using
                    Catch ex As Exception
                        Throw New InvalidOperationException(My.Resources.CouldNotDescendTreeOfComboBoxChildren)
                    End Try

                Case Else
                    c.UpdateCachedInfo()
                    Debug.Assert((c.Children Is Nothing) OrElse (c.ChildCount = c.Children.Count))
                    Return Reply.Result(c.ChildCount)
            End Select
        End Using
    End Function

    ''' <summary>
    ''' Gets the internal list component from a java combobox component.
    ''' </summary>
    ''' <param name="Combo">The java combo box of interest. Note that
    ''' this will be released.</param>
    ''' <returns>Returns the list contained in the supplied java
    ''' combo box, or nothing if no such exists (or if an internal
    ''' error occurs).</returns>
    Private Function GetJavaComboBoxInternalList(ByVal Combo As JABContext) As JABContext
        If Combo Is Nothing Then
            Throw New ArgumentException(My.Resources.ParameterCannotBeNullParameterNameCombo)
        Else
            Combo.UpdateCachedInfo()
            If Combo.Role <> "combo box" Then
                Throw New ArgumentException(My.Resources.ParameterWithNameComboMustHaveRoleComboBox)
            End If
        End If

        'Look through descendents for component with role 'list'
        Dim currentContext As JABContext = Combo
        While (currentContext IsNot Nothing) AndAlso (currentContext.Role <> "list")
            currentContext.UpdateCachedInfo()

            Dim childContext As JABContext = Nothing
            If currentContext.Children IsNot Nothing AndAlso currentContext.Children.Count > 0 Then
                childContext = currentContext.Children(0)
            End If

            currentContext = childContext
        End While

        If currentContext IsNot Nothing Then
            If currentContext.Role <> "list" Then
                currentContext = Nothing
            End If
        End If

        Return currentContext
    End Function

    ''' <summary>
    ''' Checks whether java support is enabled.
    ''' </summary>
    Private Sub CheckJabReady()
        If mJAB Is Nothing Then
            Throw New InvalidOperationException(My.Resources.JavaSupportIsNotEnabled)
        End If

    End Sub

    <Category(Category.Java)>
    <Command("Gets whether a Java element is checked.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandJabGetChecked(ByVal objQuery As clsQuery) As Reply
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            jc.UpdateCachedInfo()
            Return Reply.Result(jc.Checked)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Clicks the mouse in the centre of a Java element.")>
    <Parameters("Those required to uniquely identify the element, plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandJabMouseClickCentre(ByVal objQuery As clsQuery) As Reply
        Me.CheckJabReady()
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim buttonString As String = objQuery.GetParameter(ParameterNames.NewText)
            Return DoClickMouse(
             CType(jc.ScreenBounds, RECT).Centre, GetButtonFromString(buttonString))
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Clicks the mouse in a Java element.")>
    <Parameters("Those required to uniquely identify the element, plus 'TargX' and 'TargY', plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandJabMouseClick(ByVal objQuery As clsQuery) As Reply
        Me.CheckJabReady()
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim buttonString As String = objQuery.GetParameter(ParameterNames.NewText)
            Dim button As MouseButton = GetButtonFromString(buttonString)
            Dim targx As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
            Dim targy As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)
            Return DoClickMouse(Point.Add(jc.ScreenBounds.Location, New Size(targx, targy)), button)
        End Using
    End Function

    <Category(Category.Java)>
    <Command("Sets the check state of a Java element.")>
    <Parameters("The check state specified by 'NewText' and those required to uniquely identify the window.")>
    Private Function ProcessCommandJabSetChecked(ByVal objQuery As clsQuery) As Reply
        Dim checked As Boolean = Boolean.Parse(objQuery.GetParameter(ParameterNames.NewText))
        Using jc As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            jc.UpdateCachedInfo()
            Dim success As Boolean = False
            Select Case jc.Role
                Case "check box", "radio button"
                    Dim alreadyChecked As Boolean = jc.HasState("checked")
                    If alreadyChecked <> checked Then
                        success = mJAB.DoAction(jc, "click")

                        If Not (alreadyChecked AndAlso jc.Role = "radio button") Then
                            'Check that we actually changed the value
                            jc.UpdateCachedInfo()
                            Dim checkSucceeded As Boolean = jc.HasState("checked")

                            If checkSucceeded <> checked Then
                                Throw New InvalidOperationException(My.Resources.ValueOfCheckboxDidNotChange)
                            End If
                        End If
                    Else
                        'Nothing to do - desired state is already there
                        'and we have just checked this, so no need to do
                        'a checkSucceeded test
                        success = True
                    End If
                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.SetCheckedNotSupportedForJavaElementWithRole0, jc.Role))
            End Select

            If success Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(My.Resources.DoActionClickWasNotSuccessful)
            End If
        End Using
    End Function

#End Region

#Region "Query Command Handlers - General"

    <Category(Category.Win32)>
    <Command("Gets whether a CheckBox is checked.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandGetChecked(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(w.Checked)
    End Function

    <Category(Category.Win32)>
    <Command("Instructs a Windows Combo Box to show the dropdown, by sending a CB_SHOWDROPDOWN message.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandShowDropdown(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Select Case True
            Case w.ClassName.Contains("COMBOBOX"), w.ClassName.Contains("ComboBox")
                PostMessage(w.Handle, WindowMessages.CB_SHOWDROPDOWN, 1, 0)
                Return Reply.Ok
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.ShowDropDownIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Selects a menu item within a window.")>
    <Parameters("The path to the item of interest specified by 'Value' and those required to uniquely identify the window.")>
    Private Function ProcessCommandSelectMenuItem(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get menu path string
        Dim menuPath As String = objQuery.GetParameter(ParameterNames.Value)
        If String.IsNullOrEmpty(menuPath) Then
            Throw New InvalidOperationException(My.Resources.CanNotSelectMenuItemNoMenuPathSpecified)
        End If

        'Get the menu item specified in the path
        Dim item As MenuItem = GetMenuItemFromMenuPath(w.Handle, menuPath)

        'We send either WM_COMMAND or WM_MENUCOMMAND
        'to simulate the selection of the item of interest.
        'Which notification we send varies according to the menu style
        Dim mi As MENUINFO = Me.GetMenuInfo(item.OwnerMenu)
        Dim command As WindowMessages
        Dim wparam As Integer
        Dim lparam As IntPtr
        If (mi.dwStyle And MNS.MNS_NOTIFYBYPOS) > 0 Then
            command = WindowMessages.WM_MENUCOMMAND
            wparam = item.IndexInParent
            lparam = item.OwnerMenu
        Else
            command = WindowMessages.WM_COMMAND
            wparam = item.MenuItemID
            lparam = IntPtr.Zero
        End If

        'Don't use sendmessage here, because we don't want to
        'wait for a response from modal dialogs!
        PostMessage(w.Handle, command, wparam, lparam)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Determines whether a menu item is checked.")>
    <Parameters("The path to the item of interest specified by 'Value' and those required to uniquely identify the window.")>
    Private Function ProcessCommandIsMenuItemChecked(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        'Get menu path string
        Dim menuPath As String = objQuery.GetParameter(ParameterNames.Value)
        If String.IsNullOrEmpty(menuPath) Then
            Throw New InvalidOperationException(My.Resources.CanNotSelectMenuItemNoMenuPathSpecified)
        End If

        'Get the menu item specified in the path
        Dim item As MenuItem = GetMenuItemFromMenuPath(w.Handle, menuPath)

        'Read its info
        Dim itemInfo As MENUITEMINFO
        itemInfo.cbSize = Marshal.SizeOf(itemInfo)
        itemInfo.fMask = MIIM.MIIM_STATE
        Dim retVal = modWin32.GetMenuItemInfo(item.OwnerMenu, item.IndexInParent, 1, itemInfo)
        If Not retVal Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetMenuItemInfo0, GetLastWin32Error()))
        End If

        Return Reply.Result(((itemInfo.fState And MFS.MFS_CHECKED) > 0))
    End Function

    <Category(Category.Win32)>
    <Command("Determines whether a menu item is enabled.")>
    <Parameters("The path to the item of interest specified by 'Value' and those required to uniquely identify the window.")>
    Private Function ProcessCommandIsMenuItemEnabled(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get menu path string
        Dim menuPath As String = objQuery.GetParameter(ParameterNames.Value)
        If String.IsNullOrEmpty(menuPath) Then
            Throw New InvalidOperationException(My.Resources.CanNotSelectMenuItemNoMenuPathSpecified)
        End If

        'Get the menu item specified in the path
        Dim item As MenuItem = GetMenuItemFromMenuPath(w.Handle, menuPath)

        'Read its info
        Dim itemInfo As MENUITEMINFO
        itemInfo.cbSize = Marshal.SizeOf(itemInfo)
        itemInfo.fMask = MIIM.MIIM_STATE
        Dim retVal = modWin32.GetMenuItemInfo(item.OwnerMenu, item.IndexInParent, 1, itemInfo)
        If Not retVal Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetMenuItemInfo0, GetLastWin32Error()))
        End If

        'Enabled is the default state; we have to check for the 'disabled' flag
        Return Reply.Result((Not (itemInfo.fState And MFS.MFS_DISABLED) > 0))
    End Function

    ''' <summary>
    ''' Information about an application's menu item.
    ''' </summary>
    Private Structure MenuItem
        ''' <summary>
        ''' A handle to the menu owning the menu item of interest. Never null.
        ''' </summary>
        Public OwnerMenu As IntPtr
        ''' <summary>
        ''' The ID of the menu item of interest. This value is defined by the
        ''' target application, and may not be relevant/interesting.
        ''' </summary>
        Public MenuItemID As Integer
        ''' <summary>
        ''' The zero-based index of the menu item in the parent menu.
        ''' </summary>
        Public IndexInParent As Integer

        Public Sub New(ByVal OwnerMenu As IntPtr, ByVal MenuItemID As Integer, ByVal IndexInParent As Integer)
            Me.OwnerMenu = OwnerMenu
            Me.MenuItemID = MenuItemID
            Me.IndexInParent = IndexInParent
        End Sub
    End Structure

    ''' <summary>
    ''' Parses the menu path Used for SAP and Win32 menu selection.
    ''' </summary>
    ''' <param name="MenuPath">The path to the menu item of interest, in the form
    ''' "{File}{Save As}". Use a backslash to escape the spacial &quot;{&quot; and
    ''' &quot;}&quot; characters.</param>
    ''' <returns>Returns a list of menu path items</returns>
    Private Function ParseMenuPath(ByVal MenuPath As String) As List(Of String)
        'Parse the menu path. Example menu path syntax is "{File}{Backup}{Every 2
        'minutes}"
        'Escape char is "\", eg {File}{Options \{A-Z\}}"
        Dim state As Integer = 0 '0=Not in {}, 1=In {}
        Dim items As New List(Of String)
        Dim currentItem As String = ""
        For index As Integer = 0 To MenuPath.Length - 1
            Dim ch As Char = CChar(MenuPath.Substring(index, 1))
            Select Case ch
                Case "{"c
                    If state = 0 Then
                        state = 1
                    Else
                        Throw New InvalidOperationException(My.Resources.DisallowedCharacterWithinMenuPathItem)
                    End If
                Case "}"c
                    If state = 1 Then
                        'We will ignore ampersands in our string comparisons, since they are
                        'used to identify alt key shortcuts
                        currentItem = currentItem.Replace("&", "")
                        items.Add(currentItem)
                        currentItem = ""
                        state = 0
                    Else
                        Throw New InvalidOperationException(My.Resources.DisallowedCharacterOutsideOfMenuPathItem)
                    End If
                Case "\"c
                    If state = 1 Then
                        If index < MenuPath.Length - 1 Then
                            'Don't care what it is - just interpret the next char literally and move on
                            Dim NextChar As Char = CChar(MenuPath.Substring(index + 1))
                            currentItem &= NextChar
                            index += 1
                        Else
                            Throw New InvalidOperationException(My.Resources.UnexpectedEndOfMenuPathCharacterIsAnEscapeCharacterAndMustBeFollowedByAnotherCh)
                        End If
                    Else
                        Throw New InvalidOperationException(My.Resources.UnexpectedEscapeCharacterOutsideOfMenuPathItem)
                    End If
                Case Else
                    If state = 1 Then
                        currentItem &= ch
                    Else
                        Throw New InvalidOperationException(My.Resources.UnexpectedCharacterOutsideOfMenuPathItem)
                    End If
            End Select
        Next
        If state <> 0 Then
            Throw New InvalidOperationException(My.Resources.UnexpectedEndOfMenuPathInMiddleOfMenuPathItem)
        End If

        Return items
    End Function

    ''' <summary>
    ''' Gets the menu item specified by the supplied menu path. The target
    ''' application will be forced to build its menus in the process.
    ''' </summary>
    ''' <param name="TopLevelWindowHandle">Handle to the top-level window
    ''' containing the menu item of interest.</param>
    ''' <param name="MenuPath">The path to the menu item of interest, in the form
    ''' "{File}{Save As}". Use a backslash to escape the spacial &quot;{&quot; and
    ''' &quot;}&quot; characters.</param>
    ''' <returns>Returns information about the matching menu item found.</returns>
    Private Function GetMenuItemFromMenuPath(ByVal TopLevelWindowHandle As IntPtr, ByVal MenuPath As String) As MenuItem

        Dim items As List(Of String) = ParseMenuPath(MenuPath)

        Dim hMenu = modWin32.GetMenu(TopLevelWindowHandle)
        If hMenu = IntPtr.Zero Then
            Throw New InvalidOperationException(My.Resources.WindowHasNoMenu)
        End If
        Dim menuBarItemCount As Integer = modWin32.GetMenuItemCount(hMenu)
        If menuBarItemCount = -1 Then
            Throw New InvalidOperationException(My.Resources.FailedToCountWindowSMenus)
        End If

        'A list of menu handles, one per text item
        Dim menuHandles As New List(Of IntPtr)
        menuHandles.Add(hMenu)

        'Descend the menus according to the strings given, and
        'build up a list of handles on the way
        Dim latestMenu = hMenu 'The latest menu found, in our descent of the menus
        Dim firstTime As Boolean = True
        Dim itemIndex As Integer = 0
        Dim menuItemIndices As New List(Of Integer) 'The index of each menu, in its parent
        menuItemIndices.Add(-1)
        Do
            Dim menuItemCount As Integer = GetMenuItemCount(latestMenu)

            For j As Integer = 0 To menuItemCount - 1
                Dim menuText As String = Me.GetMenuItemText(latestMenu, j).Replace("&", "").Trim
                If menuText = items(itemIndex) Then
                    latestMenu = GetSubMenu(latestMenu, j)
                    If latestMenu <> IntPtr.Zero Then
                        'The top level menu requires this special message
                        If firstTime Then
                            Dim wp As Integer = &HF095
                            Dim menuItemBounds As Rectangle = GetMenuItemRect(TopLevelWindowHandle, hMenu, j)
                            Dim lp As Integer = menuItemBounds.X << 16 + menuItemBounds.Y
                            SendMessage(TopLevelWindowHandle, modWin32.WindowMessages.WM_SYSCOMMAND, wp, lp)
                            firstTime = False
                        End If

                        'This causes submenu to be (re)generated, ensuring the sub-items
                        'that we are looking for are present
                        SendMessage(TopLevelWindowHandle, WindowMessages.WM_INITMENU, latestMenu, IntPtr.Zero)
                        SendMessage(TopLevelWindowHandle, WindowMessages.WM_INITMENUPOPUP, latestMenu, IntPtr.Zero)

                        'move down to next menu level
                        menuHandles.Add(latestMenu)
                        menuItemIndices.Add(j)
                    End If
                    Exit For
                End If
            Next

            itemIndex += 1
        Loop While latestMenu <> IntPtr.Zero AndAlso itemIndex < items.Count

        'Check we have found all menu handles
        If menuHandles.Count <> items.Count Then
            Throw New InvalidOperationException(String.Format(My.Resources.CouldNotFindTheMenuItemSpecified0ItemNumber1, items(menuHandles.Count - 1), (menuHandles.Count).ToString))
        End If

        'Find the menu command of interest
        latestMenu = menuHandles(menuHandles.Count - 1)
        Dim itemCount As Integer = GetMenuItemCount(latestMenu)
        Dim menuItemID As Integer
        Dim menuItemIndex As Integer = -1
        For j As Integer = 0 To itemCount - 1
            Dim menutext As String = Me.GetMenuItemText(latestMenu, j).Replace("&", "").Trim
            If menutext = items(items.Count - 1) Then
                menuItemIndex = j
                menuItemID = GetMenuItemID(latestMenu, j)
                Exit For
            End If
        Next
        If menuItemIndex = -1 Then
            Throw New InvalidOperationException(My.Resources.FailedToFindMenuCommandOfInterest)
        End If

        Return New MenuItem(latestMenu, menuItemID, menuItemIndex)
    End Function

    <Category(Category.Win32)>
    <Command("Instructs a Windows Combo Box to hide the dropdown, by sending a CB_SHOWDROPDOWN message.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandHideDropDown(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Select Case True
            Case w.ClassName.Contains("COMBOBOX"), w.ClassName.Contains("ComboBox")
                PostMessage(w.Handle, WindowMessages.CB_SHOWDROPDOWN, 0, 0)
                Return Reply.Ok
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.ShowDropDownIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Moves a window.")>
    <Parameters("The x and y coordinates specified by 'TargX' and 'TargY' as well as those required to uniquely identify the window.")>
    Private Function ProcessCommandMoveWindow(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        Const SWP_NOSIZE As Integer = &H1
        w = mobjModel.IdentifyWindow(objQuery)
        Dim x As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)
        SetWindowPos(w.Hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Resizes a window.")>
    <Parameters("The width and height specified by 'TargWidth' and 'TargHeight' as well as those required to uniquely identify the window.")>
    Private Function ProcessCommandResizeWindow(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        Const SWP_NOMOVE As Integer = &H2
        wn = mobjModel.IdentifyWindow(objQuery)
        Dim w As Integer = objQuery.GetIntParam(ParameterNames.TargWidth, False)
        Dim h As Integer = objQuery.GetIntParam(ParameterNames.TargHeight, False)
        SetWindowPos(wn.Handle, IntPtr.Zero, 0, 0, w, h, SWP_NOMOVE)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Maximises a window")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandMaximiseWindow(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        wn = mobjModel.IdentifyWindow(objQuery)
        ShowWindow(wn.Handle, ShowWindowCommands.SW_MAXIMIZE)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Minimise a window")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandMinimiseWindow(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        wn = mobjModel.IdentifyWindow(objQuery)
        ShowWindow(wn.Handle, ShowWindowCommands.SW_MINIMIZE)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Restores a window")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandRestoreWindow(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        wn = mobjModel.IdentifyWindow(objQuery)
        ShowWindow(wn.Handle, ShowWindowCommands.SW_RESTORE)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Hides a window by moving it off the screen")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandHideWindow(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        wn = mobjModel.IdentifyWindow(objQuery)
        Dim sErr As String = Nothing
        If Me.HideWindow(wn.Hwnd, sErr) Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Hides all top-level windows belonging to an application, by moving each of them off the screen")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandHideAllWindows(ByVal objQuery As clsQuery) As Reply
        For Each entity As clsUIEntity In mobjModel.GetAllEntities
            Dim window As clsUIWindow = TryCast(entity, clsUIWindow)
            If window IsNot Nothing Then
                If window.IsTopLevel Then
                    Dim sErr As String = Nothing
                    If Not Me.HideWindow(window.Hwnd, sErr) Then
                        Throw New InvalidOperationException(String.Format(My.Resources.CouldNotHideWindowWithText01, window.WindowText, sErr))
                    End If
                End If
            End If
        Next

        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Hides a top-level window by moving it off screen, and turning into a toolbar
    ''' window, so that it does not show up in the taskbar
    ''' </summary>
    ''' <param name="hWnd">Handle to the window of interest.</param>
    ''' <param name="sErr">Carries back a message in the event of an error</param>
    ''' <returns>Returns True on success.</returns>
    Private Function HideWindow(ByVal hWnd As IntPtr, ByRef sErr As String) As Boolean
        'Get geographic information
        Dim screenRect As RECT
        modWin32.GetWindowRect(hWnd, screenRect)
        Dim monitorBounds As Rectangle = System.Windows.Forms.Screen.GetBounds(Point.Empty)

        'check for silliness
        If mWindowInformation.ContainsKey(hWnd) Then
            'give user benefit of doubt - maybe application is offscreen
            If Not CType(screenRect, Rectangle).IntersectsWith(monitorBounds) Then
                'application is already hidden
                Return True
            Else
                'application is no longer hidden, so prepare to go ahead
                mWindowInformation.Remove(hWnd)
            End If
        End If

        'Save window's current style
        Dim wi As New WindowInformation
        wi.WindowScreenBounds = screenRect
        Dim currentStyle As Integer = GetWindowLong(hWnd, GWL.GWL_EXSTYLE)
        wi.StyleEx = currentStyle
        Me.mWindowInformation.Add(hWnd, wi)

        'Hide window
        ShowWindow(hWnd, ShowWindowCommands.SW_HIDE)

        'Move offscreen, and make into toolwindow so that does not show up in taskbar
        Dim retval = SetWindowPos(hWnd, IntPtr.Zero, Math.Min(monitorBounds.Right + 1, screenRect.Left + monitorBounds.Width), screenRect.Top, 0, 0, SWP.SWP_NOSIZE Or SWP.SWP_NOZORDER Or SWP.SWP_NOOWNERZORDER Or SWP.SWP_NOACTIVATE)
        If Not retval Then
            sErr = String.Format(My.Resources.CouldNotSetPositionOfWindowLastWin32ErrorWas0, Marshal.GetLastWin32Error)
            Return False
        End If
        SetWindowLong(hWnd, GWL.GWL_EXSTYLE, (currentStyle And (Not WindowStyles.WS_EX_APPWINDOW)) Or WindowStyles.WS_EX_TOOLWINDOW)

        'Reshow window
        ShowWindow(hWnd, ShowWindowCommands.SW_SHOWNOACTIVATE)

        'Now verify that the application is offscreen
        modWin32.GetWindowRect(hWnd, screenRect)
        If CType(screenRect, Rectangle).IntersectsWith(monitorBounds) Then
            sErr = My.Resources.AttemptToMoveApplicationOffscreenUnsuccessfulTheApplicationStillAppearsToBeInte
            Return False
        End If

        Return True
    End Function

    <Category(Category.Win32)>
    <Command("Unhides a window which has previously been hidden")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandUnHideWindow(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        wn = mobjModel.IdentifyWindow(objQuery)

        'Get geographic information
        Dim screenRect As RECT
        modWin32.GetWindowRect(wn.Handle, screenRect)
        Dim monitorBounds As Rectangle = System.Windows.Forms.Screen.GetBounds(Point.Empty)

        'Hide window
        ShowWindow(wn.Handle, ShowWindowCommands.SW_HIDE)

        'Restore old state
        Dim wi As WindowInformation
        If Me.mWindowInformation.ContainsKey(wn.Handle) Then
            wi = Me.mWindowInformation(wn.Handle)
            Me.mWindowInformation.Remove(wn.Handle)
        Else
            Throw New InvalidOperationException(My.Resources.CannotRestoreWindowNoRecordOfWindowInformationFoundForThisWindow)
        End If
        SetWindowLong(wn.Hwnd, GWL.GWL_EXSTYLE, wi.StyleEx)
        Dim retval = SetWindowPos(wn.Handle, IntPtr.Zero, wi.WindowScreenBounds.Left, wi.WindowScreenBounds.Top, 0, 0, SWP.SWP_NOSIZE Or SWP.SWP_NOZORDER Or SWP.SWP_NOOWNERZORDER Or SWP.SWP_NOACTIVATE)
        If Not retval Then
            Throw New InvalidOperationException(String.Format(My.Resources.CouldNotSetPositionOfWindowLastWin32ErrorWas0, Marshal.GetLastWin32Error))
        End If

        'Reshow window
        ShowWindow(wn.Handle, ShowWindowCommands.SW_SHOWNOACTIVATE)

        Return Reply.Ok
    End Function


    <Category(Category.Win32)>
    <Command("Determines whether a window has previously been hidden, and has not yet been restored")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandIsWindowHidden(ByVal objQuery As clsQuery) As Reply
        Dim wn As clsUIWindow
        wn = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(Me.mWindowInformation.ContainsKey(wn.Handle))
    End Function

    ''' <summary>
    ''' Gets the handle of the windows taskbar.
    ''' </summary>
    ''' <returns>Returns the handle of the taskbar, or zero if it cannot be found</returns>
    Private Function GetTaskbarHandle() As IntPtr
        Dim desktopHandle As IntPtr = GetDesktopWindow()
        If desktopHandle <> IntPtr.Zero Then
            Dim trayHandle As IntPtr = FindWindowEx(desktopHandle, Nothing, "Shell_TrayWnd", Nothing)
            If (trayHandle <> IntPtr.Zero) Then
                Dim RebarHandle As IntPtr = FindWindowEx(trayHandle, Nothing, "ReBarWindow32", Nothing)
                If (RebarHandle <> IntPtr.Zero) Then
                    Dim MSTaskHandle As IntPtr = FindWindowEx(RebarHandle, Nothing, "MSTaskSwWClass", Nothing)
                    If (MSTaskHandle <> IntPtr.Zero) Then
                        Dim TaskbarHandle As IntPtr = FindWindowEx(MSTaskHandle, Nothing, "ToolbarWindow32", Nothing)
                        Return TaskbarHandle
                    End If
                End If
            End If
        End If

        Return IntPtr.Zero
    End Function

    ''' <summary>
    ''' Time for which highlighting of application elements should be performed,
    ''' in milliseconds.
    ''' </summary>
    Private Const HighlightingPeriod As Integer = 2000

    <Category(Category.Highlighting)>
    <Command("Highlights a window.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandHighlightWindow(ByVal objQuery As clsQuery) As Reply
        Dim windows As ICollection(Of clsUIWindow) = mobjModel.IdentifyWindows(objQuery, 5)

        'if we have more than 3 windows/elements to highlight, just show the error
        If windows.Count < 4 Then
            HighlightWindows(windows)
        End If

        Select Case windows.Count
            Case 0
                Throw New InvalidOperationException(My.Resources.NoMatchingWindowsFound)
            Case 1
                Return Reply.Ok
            Case Else
                Return Reply.Warning(My.Resources.MoreThanOneMatchingWindowFound)
        End Select
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights a SAP Gui component.")>
    <Parameters("Those required to uniquely identify the component - i.e. the ID.")>
    Private Function ProcessCommandHighlightSap(ByVal objQuery As clsQuery) As Reply
        Dim idi As clsIdentifierMatchTarget = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ID)
        If idi Is Nothing Then Throw New InvalidOperationException(My.Resources.SAPComponentMustBeIdentifiedByID)
        Dim id As String = idi.MatchValue
        Dim rects As New List(Of Rectangle)
        rects.Add(GetSapComponentScreenRect(id))
        HighlightRectangles(rects)
        Return Reply.Ok
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights an Active Accessibility element")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandHighlightElement(ByVal objQuery As clsQuery) As Reply
        Dim matches As ICollection(Of clsAAElement) = Nothing

        matches = mobjModel.IdentifyAccessibleObjects(objQuery, 5)
        ' This shouldn't occur, but just in case...
        If matches Is Nothing Then _
         Throw New InvalidOperationException(My.Resources.FailedWhileTryingToMatchAnyAAElements)
        If matches.Count < 4 Then
            HighlightElements(matches)
        End If
        Select Case matches.Count
            Case 0 : Throw New InvalidOperationException(My.Resources.NoMatchingWindowsFound)
            Case 1 : Return Reply.Ok
            Case Else : Return Reply.Warning(My.Resources.MoreThanOneMatchingWindowFound)
        End Select
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights an UI Automation element")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandHighlightUiaElement(ByVal q As clsQuery) As Reply
        Dim matches = UIAutomationIdentifierHelper.FindAllUIAutomationElements(q, PID)

        ' This shouldn't occur, but just in case...
        If matches Is Nothing Then _
         Throw New InvalidOperationException(My.Resources.FailedWhileTryingToMatchAnyAAElements)

        HighlightElements(matches)
        Select Case matches.Count
            Case 0 : Throw New InvalidOperationException(My.Resources.NoMatchingWindowsFound)
            Case 1 : Return Reply.Ok
            Case Else : Return Reply.Warning(My.Resources.MoreThanOneMatchingWindowFound)
        End Select
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights a Web element")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandHighlightWebElement(query As clsQuery) As Reply
        Dim matches = MyBrowserAutomationIdentifierHelper?.FindElements(query)
        HighlightWebElements(matches)

        If matches Is Nothing OrElse matches.Count = 0 Then
            Throw New InvalidOperationException(My.Resources.NoMatchingWebElementsFound)
        ElseIf matches.Count = 1 Then
            Return Reply.Ok
        Else
            Return Reply.Warning(My.Resources.MoreThanOneMatchingWebElementFound)
        End If
    End Function

    ''' <summary>
    ''' Highlights all of the supplied elements at once.
    ''' </summary>
    Private Sub HighlightElements(ByVal elements As IEnumerable(Of IAutomationElement))
        Dim boundingRectangles =
            elements.
                Select(Function(x) x.CurrentBoundingRectangle).
                Select(Function(x) New Rectangle(CInt(x.X), CInt(x.Y), CInt(x.Width), CInt(x.Height))).
                ToList()

        If boundingRectangles.Any() Then HighlightRectangles(boundingRectangles)
    End Sub

    <Category(Category.Highlighting)>
    <Command("Highlights an HTML element")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHighlightHtmlElement(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()

        Const maxRectangles As Integer = 5
        Dim matches As ICollection(Of clsHTMLElement) = mobjModel.GetHTMLElements(objQuery, docs, maxRectangles)
        If matches.Count = 0 Then Throw New NoSuchHTMLElementException()

        HighlightRectangles(matches.Take(maxRectangles).Select(Function(match) match.AbsoluteBounds).ToList())

        Select Case matches.Count
            Case 0
                Throw New InvalidOperationException(My.Resources.NoMatchingWindowsFound)
            Case 1
                Return Reply.Ok
            Case Else
                Return Reply.Warning(My.Resources.MoreThanOneMatchingWindowFound)
        End Select
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights a Java element")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandHighlightJabElement(ByVal objQuery As clsQuery) As Reply
        Dim matches As ICollection(Of JABContext) =
         mobjModel.GetJABObjects(objQuery, mJAB, 5)

        Dim rects As New List(Of Rectangle)
        For Each m As JABContext In matches
            rects.Add(m.ScreenRect)
        Next
        HighlightRectangles(rects)

        Select Case matches.Count
            Case 0
                Throw New InvalidOperationException(My.Resources.NoMatchingJavaElementsFound)
            Case 1
                Return Reply.Ok
            Case Else
                Return Reply.Warning(My.Resources.MoreThanOneMatchingJavaElementFound)
        End Select
    End Function

    ''' <summary>
    ''' Highlights all of the supplied windows at once.
    ''' </summary>
    ''' <param name="windows">The windows to highlight. Must not be null, but may be
    ''' empty.</param>
    Private Sub HighlightWindows(ByVal windows As IEnumerable(Of clsUIWindow))
        Dim rects As New List(Of Rectangle)
        For Each w As clsUIWindow In windows
            rects.Add(w.ScreenBounds)
        Next
        HighlightRectangles(rects)
    End Sub

    ''' <summary>
    ''' Highlights all of the supplied elements at once.
    ''' </summary>
    ''' <param name="elements">A List of clsAAElements to highlight.</param>
    Private Sub HighlightElements(ByVal elements As IEnumerable(Of clsAAElement))
        Dim rects As New List(Of Rectangle)
        For Each w As clsAAElement In elements
            rects.Add(w.ScreenBounds)
        Next
        HighlightRectangles(rects)
    End Sub

    ''' <summary>
    ''' Highlights all of the supplied rectangles at once.
    ''' </summary>
    ''' <param name="rects">A List of Rectangles to highlight, in screen
    ''' coordinates.</param>
    Private Sub HighlightRectangles(ByVal rects As ICollection(Of Rectangle))

        Dim span As TimeSpan = TimeSpan.FromMilliseconds(HighlightingPeriod)
        Dim highlighters As New List(Of clsWindowHighlighter)
        For Each r As Rectangle In rects
            clsWindowHighlighter.ShowNewFrameFor(r, span)
        Next
        While span > TimeSpan.Zero
            Dim startLoop As Date = Date.UtcNow
            BlockingMessagePump()
            Dim endLoop As Date = Date.UtcNow
            span -= endLoop - startLoop
        End While

    End Sub

    Private Sub HighlightWebElements(elements As IReadOnlyCollection(Of IWebElement))
        If elements Is Nothing Then Return
        elements.ForEach(Sub(x) x.Highlight(Color.Red)).Evaluate()
        Dim span = TimeSpan.FromMilliseconds(HighlightingPeriod)
        While span > TimeSpan.Zero
            Dim startLoop As Date = Date.UtcNow
            BlockingMessagePump()
            Dim endLoop As Date = Date.UtcNow
            span -= endLoop - startLoop
        End While
        elements.ForEach(Sub(x) x.Page.RemoveHighlighting()).Evaluate()
    End Sub


    ''' <summary>
    ''' Highlights the given object (which must be a Rectangle or RECT structure)
    ''' </summary>
    ''' <param name="o">The object to highlight - must be a Rectangle or RECT
    ''' structure.</param>
    ''' <exception cref="InvalidCastException">If the given object was neither a
    ''' Rectangle or a RECT</exception>
    Private Sub HighlightObject(ByVal o As Object)
        If TypeOf o Is Rectangle _
         Then HighlightRectangle(DirectCast(o, Rectangle)) _
         Else HighlightRectangle(DirectCast(o, RECT))
    End Sub

    ''' <summary>
    ''' Highlights the supplied rectangle by drawing
    ''' a window frame around it.
    ''' </summary>
    ''' <param name="rect">The rectangle to highlight, in screen coords.</param>
    Private Sub HighlightRectangle(ByVal rect As Rectangle)
        HighlightRectangles(GetSingleton.ICollection(rect))
    End Sub

    <Category(Category.General)>
    <Command("Get the PID of the connected target application")>
    Private Function ProcessCommandGetPid(ByVal objQuery As clsQuery) As Reply
        Return Reply.Result(mPID)
    End Function

    <Category(Category.General)>
    <Command("Get all the current window identifiers for the given window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("Identifiers in the same format as from the spy command, or error message")>
    Private Function ProcessCommandGetWindowIdentifiers(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Window(GetWindowIdentifiers(w.Handle))
    End Function

    <Category(Category.General)>
    <Command("Get all the current AA identifiers for the given element.")>
    <Parameters("Those required to uniquely identify the Active Accessibility element.")>
    <Response("Identifiers in the same format as from the spy command, or error message")>
    Private Function ProcessCommandGetActiveAccessibilityIdentifiers(ByVal objQuery As clsQuery) As Reply
        Dim el As clsAAElement
        el = mobjModel.IdentifyAccessibleObject(objQuery)
        Return Reply.AAElement(el.GetIdentifiers())
    End Function

    <Category(Category.General)>
    <Command("Get all the current JAB identifiers for the given element.")>
    <Parameters("Those required to uniquely identify the JAB element.")>
    <Response("Identifiers in the same format as from the spy command, or error message")>
    Private Function ProcessCommandGetJabIdentifiers(ByVal objQuery As clsQuery) As Reply
        CheckJabReady()
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            c.UpdateCachedInfo()
            Return Reply.Jab(c.GetIdentifiers())
        End Using
    End Function

    <Category(Category.General)>
    <Command("Get all the current HTML identifiers for the given element.")>
    <Parameters("Those required to uniquely identify the HTML element.")>
    <Response("Identifiers in the same format as from the spy command, or error message")>
    Private Function ProcessCommandGetHtmlIdentifiers(ByVal objQuery As clsQuery) As Reply
        Dim el As clsHTMLElement
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        el = mobjModel.GetHTMLElement(objQuery, docs)
        If el Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoMatchingHtmlElementFound)
        End If

        Return Reply.Html(el.GetIdentifiers())
    End Function

    <Category(Category.General)>
    <Command("Get the current value of an identifier for the given element.")>
    <Parameters("'idname' is the name of the identifier, 'idtype' is the type (aa, html, window, jab), plus those required to uniquely identify the element.")>
    <Response("The following responses are possible:" & vbCrLf &
    "* RESULT:The idenfitier value" & vbCrLf &
    "* Error - error message")>
    Private Function ProcessCommandGetIdentifier(ByVal objQuery As clsQuery) As Reply
        Dim idname As String = objQuery.GetParameter(ParameterNames.IDName)
        If idname Is Nothing Then Throw New InvalidOperationException(My.Resources.IdentifierNameMustBeSpecified)
        idname = idname.ToLower(CultureInfo.InvariantCulture)

        Dim idtype As String = objQuery.GetParameter(ParameterNames.IDType)
        If idtype Is Nothing Then Throw New InvalidOperationException(My.Resources.IdentifierTypeMustBeSpecified)
        idtype = idtype.ToLower(CultureInfo.InvariantCulture)

        Dim ids As String
        Select Case idtype
            Case "window"
                Dim w As clsUIWindow

                w = mobjModel.IdentifyWindow(objQuery)

                ids = GetWindowIdentifiers(w.Handle)
            Case "aa"
                Dim el As clsAAElement

                el = mobjModel.IdentifyAccessibleObject(objQuery)

                ids = el.GetIdentifiers()

                ' See bug 7951 for reasons behind this little uglifix.
                ' Special case - Value2 and pValue2 are presented to the user as
                ' "Value" and "Parent Value"... which unfortunately match the
                ' identifiers 'Value' and 'pValue'. AA doesn't pull back Value and
                ' pValue, so we need to slightly change what the call is looking for
                If idname = "value" _
                 Then idname = "value2" _
                Else If idname = "pvalue" _
                 Then idname = "pvalue2"

            Case "jab"

                CheckJabReady()
                Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
                    c.UpdateCachedInfo()
                    ids = c.GetIdentifiers()
                End Using

            Case "html"
                Dim el As clsHTMLElement

                Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
                el = mobjModel.GetHTMLElement(objQuery, docs)

                If el Is Nothing Then
                    Throw New InvalidOperationException(My.Resources.NoMatchingHtmlElementFound)
                End If
                ids = el.GetIdentifiers()

                ' See bug 7951 / 8575
                If idname = "inputidentifier" _
                 Then idname = "inputidentifier2"

            Case "uia"
                Dim element = mUIAutomationIdentifierHelper.FindUIAutomationElement(objQuery, mPID)

                If element Is Nothing Then
                    Throw New InvalidOperationException(My.Resources.NoMatchingUIAutomationElementFound)
                End If

                Dim stringBuilder = New StringBuilder()
                mUIAutomationIdentifierHelper.AppendIdentifiers(element, stringBuilder)
                ids = stringBuilder.ToString()

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.UnrecognisedIdentifierType0, idtype))
        End Select

        While ids.Length() > 0
            Dim index As Integer = ids.IndexOf("=")
            If index = -1 Then Throw New InvalidOperationException(My.Resources.InvalidIdentifierData)
            Dim thisname As String = ids.Substring(0, index)
            ids = ids.Substring(index + 1)
            Dim idvalue As String = clsQuery.ParseValue(ids)
            Dim def As Boolean = False
            If thisname.StartsWith("+"c) Then
                def = True
                thisname = thisname.Substring(1)
            End If
            If thisname.ToLower(CultureInfo.InvariantCulture) = idname Then
                Return Reply.Result(idvalue)
            End If
        End While
        Throw New InvalidOperationException(String.Format(My.Resources.Identifier0OfType1NotFound, idname, idtype))
    End Function

    <Category(Category.General)>
    <Command("Starts a bitmap spying operation. Works like an interactive version of 'readbitmap'")>
    <Response("The following responses are possible:" & vbCrLf &
    "* BITMAP:data - a bitmap selection, with data being the base64-encoded bitmap data" & vbCrLf &
    "* Cancellation - CANCEL" & vbCrLf &
    "* Error - error message")>
    Private Function ProcessCommandSpyBitmap(ByVal objQuery As clsQuery) As Reply
        Return DoSpy(SpyMode.Bitmap)
    End Function

    <Category(Category.General)>
    <Command("Starts the relevant spying user interface, allowing the user to select an element from the target application. ")>
    <Response("Depending on what type of selection is made by the user, and also the type of application in use, various responses are possible." & vbCrLf & vbCrLf &
    "A successful selection is usually (see below for exceptions) of the form TYPE:idenfiers, where TYPE is one of:" & vbCrLf &
    "* WINDOW" & vbCrLf &
    "* WINDOWRECT" & vbCrLf &
    "* TERMINALFIELD" & vbCrLf &
    "* AAELEMENT" & vbCrLf &
    "* HTMLELEMENT" & vbCrLf &
    "* JAB" & vbCrLf &
    "* SAP" & vbCrLf &
    "In all the above cases, the idenfitiers are supplied in the same form that they would be used when " &
    "identifying the element. Identifiers can be preceded by a '+' to suggest that they should be used by " &
    "default." & vbCrLf & vbCrLf &
    "The following responses are also possible:" & vbCrLf &
    "* BITMAP:data - a bitmap selection, with data being the base64-encoded bitmap data" & vbCrLf &
    "* Cancellation - CANCEL" & vbCrLf &
    "* Error - ERROR:message")>
    Private Function ProcessCommandSpy(ByVal q As clsQuery) As Reply
        Dim mode As SpyMode = SpyMode.Win32
        Dim initMode As String = q.GetParameter(ParameterNames.InitialSpyMode)
        If initMode IsNot Nothing AndAlso Not clsEnum.TryParse(initMode, mode) Then
            Throw New InvalidArgumentException(
             My.Resources.InvalidSpyModeValidCaseSensitiveModesAre0,
             String.Join(",", [Enum].GetValues(GetType(SpyMode)))
            )
        End If

        Return DoSpy(mode)
    End Function

    <Category(Category.General)>
    <Command("Cancels the spying operation")>
    Private Function ProcessCommandCancelSpy(ByVal objquery As clsQuery) As Reply
        If (Not mTerminalApp Is Nothing) Then
            mTerminalApp.CancelSpy()
        Else
            mSpyCancelled = True
        End If
        Return Reply.Ok
    End Function

    <Category(Category.General)>
    <Command("Get information about a particular element.")>
    <Parameters("The mode to use, the identifiers necessary to identify the parent (or root=<anything> to start at the root), plus 'elementindex' containing the index of the child element required, 0-n.")>
    <Response("The response is in the same format as for the Spy command. Additionally MAX means the child index is too high, and NONE means that child is not addressable.")>
    Private Function ProcessCommandGetElement(ByVal objQuery As clsQuery) As Reply
        Dim root As Boolean = objQuery.GetParameter(ParameterNames.Root) IsNot Nothing
        Dim mode As SpyMode = SpyMode.Win32
        Dim sMode As String = objQuery.GetParameter(ParameterNames.Mode)
        If sMode IsNot Nothing AndAlso Not clsEnum.TryParse(sMode, mode) Then
            Throw New InvalidOperationException(String.Format(My.Resources.ERRORInvalidSpyModeValidCaseSensitiveModesAre0, CollectionUtil.Join([Enum].GetValues(GetType(SpyMode)), ",")))
        End If

        Dim index As Integer = objQuery.GetIntParam(ParameterNames.ElementIndex, False)
        Select Case mode
            Case SpyMode.Win32
                If root Then
                    UpdateModel()
                    Dim entities As List(Of clsUIEntity) = mobjModel.GetAllEntities()
                    For Each e As clsUIEntity In entities
                        If TypeOf e Is clsUIWindow AndAlso e.Parent Is Nothing Then
                            If index = 0 Then
                                Return Reply.Window(GetWindowIdentifiers(CType(e, clsUIWindow).Handle))
                            End If
                            index -= 1
                        End If
                    Next
                    Return Reply.Max
                Else
                    Dim w As clsUIWindow = mobjModel.IdentifyWindow(objQuery)
                    If index >= w.ChildCount Then Return Reply.Max
                    Dim e As clsUIEntity = CType(w.Children(index), clsUIEntity)
                    If Not TypeOf e Is clsUIWindow Then Return Reply.None
                    Return Reply.Window(GetWindowIdentifiers(CType(e, clsUIWindow).Handle))
                End If
            Case Else
                Throw New InvalidOperationException(My.Resources.ThatModeIsNotCurrentlySupported)
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Selects a particular tab in a Windows Tab Control, using the TCM_SETCURSEL message." & vbCrLf & "NOTE: Not very useful currently. TCM_SETCURSEL will cause the tab to be selected, but it does not notify the parent, so the action of changing tab may not be completed in full. The parent needs a WM_NOTIFY message, but we can't send it cross-application. Suggest clicking if possible for now.")>
    <Parameters("Those required to uniquely identify the window, plus 'index' containing the index of the tab to select, 0-n.")>
    Private Function ProcessCommandSelectTab(ByVal objQuery As clsQuery) As Reply
        Return ProcessCommandSelectItem(objQuery)
    End Function

    <Category(Category.Win32)>
    <Command("Selects an item in a Windows ComboBox, ListBox or List View that " &
        "matches the given text. This uses the CB_SELECTSTRING, LB_SELECTSTRING " &
        "or LVM_FINDSTRING messages respectively. The window must be a standard " &
        "window class, with the classname identified as either ""ComboBox"", " &
        """Listbox"" or ""SysListView32.""")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' containing the text to match.")>
    Private Function ProcessCommandSelectItem(ByVal q As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)

        'Get the fallback index
        Dim posnStr As String = q.GetParameter(ParameterNames.Position)
        Dim posn As Integer = -1
        If posnStr IsNot Nothing Then
            If Not Integer.TryParse(posnStr, posn) Then Throw New InvalidArgumentException(
                My.Resources.FailedToInterpretValue0AsANumber, posnStr)

            posn -= 1 'Adjustment for zero-based index from one-based index
        End If

        'Get the text we are searching for...
        Dim txt As String = q.GetParameter(ParameterNames.NewText)
        If txt Is Nothing AndAlso posn < 0 Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        If w.ClassContains("combobox") Then
            ' Get the index of the item to select
            Dim selInd As Integer = posn

            ' Only "get the item index" if we don't have text to search for
            If txt IsNot Nothing Then selInd = GetItemIndex(w, txt, posn)

            ' If we couldn't find it
            If selInd = -1 Then
                ' ... and we're using a positional match
                If posn <> -1 Then Throw New NoSuchElementException(
                 My.Resources.NoItemWasFoundAtPosition0, posn)

                ' Otherwise, we're using the text...
                Throw New NoSuchElementException(
                 My.Resources.NoItemWasFoundUsingTheSearchText & txt & "'")
            End If

            ' Send notification to fire associated events
            Dim pHwnd = GetParent(w.Hwnd)
            Dim cid As Integer = modWin32.GetDlgCtrlID(pHwnd)
            clsConfig.LogTiming("Win32: Send CBN_EDITCHANGE from control ID {0:X8} (handle:{1:X8}) to {2:X8}", cid, w.Handle, pHwnd)

            SendMessage(pHwnd, WindowMessages.WM_COMMAND, MakeWParam(cid, CBN.CBN_EDITCHANGE), w.Handle)

            Dim resp = SendMessage(w.Hwnd, WindowMessages.CB_SETCURSEL, selInd, 0)
            If resp.ToInt64() = CB.CB_ERR Then Throw New OperationFailedException(My.Resources.SpecifiedItemFoundButSelectionFailed0_1, GetLastWin32Error())

            clsConfig.LogTiming("Win32: Send CBN_SELCHANGE from control ID {0:X8} (handle:{1:X8}) to {2:X8}", cid, w.Handle, pHwnd)

            SendMessage(pHwnd, WindowMessages.WM_COMMAND, MakeWParam(cid, CBN.CBN_SELCHANGE), w.Handle)

        ElseIf w.ClassContains("listbox", "combolbox") Then

            'Note that what follows is more appropriate than using the LB_SELECTSTRING
            'message, because SELECTSTRING is not appropriate for multi-selection listboxes
            '(read the official documentation). What follows is intended to be appropriate for both
            'single selection and multiselection listboxes.

            'Get the index of the item to select
            Dim IndexToSelect As Integer = Me.GetItemIndex(w, txt, posn)
            If IndexToSelect = -1 Then
                Throw New InvalidOperationException(String.Format(My.Resources.NoItemWasFoundUsingTheSearchText0, txt))
            End If

            'Select the item, using a  different message according to whether
            'this is a multiselection box or not
            Dim result As IntPtr
            If IsListboxMultiSelection(w.Handle) Then
                result = SendMessage(w.Handle, WindowMessages.LB_SETSEL, 1, IndexToSelect)
            Else
                result = SendMessage(w.Handle, WindowMessages.LB_SETCURSEL, IndexToSelect, 0)
            End If

            If result.ToInt64() = LB.LB_ERR Then
                Throw New InvalidOperationException(String.Format(My.Resources.SpecifiedItemFoundButSelectionFailed0, GetLastWin32Error()))
            End If

            'CG:
            'When you change the ListBox selection like this, it doesn't send the
            'normal notification message it would send if the user had changed the
            'selection, so assuming we know its parent, we'll send it ourselves...
            '(Note: this works for standard Win32 apps, but for some reason not
            'for .NET-based apps)
            '
            'PW:
            'The comment above about .NET apps does not seem to be true: 
            'the VS2005 test object responds to the selection notification sent below:
            'Neil's selectedindexchanged event is fired
            If Not w.Parent Is Nothing And TypeOf w.Parent Is clsUIWindow Then
                Dim ew As clsUIWindow
                ew = CType(w.Parent, clsUIWindow)
                Dim cid As Integer
                cid = GetWindowLong(w.Hwnd, GWL.GWL_ID) And &HFFFF
                clsConfig.LogTiming("Win32: Sent LBN_SELCHANGE from control ID " & cid.ToString("X8") & " (handle:" & w.Handle.ToString("X8") & ") to " & ew.Handle.ToString("X8"))
                SendMessage(ew.Handle, WindowMessages.WM_COMMAND, LBN_SELCHANGE << 16 Or cid, w.Handle)
            End If

        ElseIf w.ClassContains("SysTreeView32", "PBTreeView32") Then
            ' Get handle to the tree node of interest - the existence of 'txt'
            ' determines whether we use text or position to find the node.
            Dim usingText As Boolean = (txt <> "")
            Dim nodeHandle = If(usingText, GetTreenodeHandle(w.Handle, txt), GetTreenodeHandle(w.Hwnd, posn))

            If nodeHandle = IntPtr.Zero Then Throw New NoSuchElementException(
             My.Resources.NoNodeWith0ValueOf1Found,
             If(usingText, "text", "position"),
             If(usingText, txt, CStr(posn + 1)))

            Dim resp = SendMessage(w.Hwnd, WindowMessages.TVM_SELECTITEM, TreeviewNextItemFlags.TVGN_CARET, nodeHandle)

            If resp = IntPtr.Zero Then Throw New OperationFailedException(
             My.Resources.FailedToSelectItem01,
             If(usingText, txt, CStr(posn + 1)),
             GetLastWin32Error())

            Return Reply.Ok

        ElseIf w.ClassContains("SysListView32", "PBListView32") Then

            If ProcessCommandClearSelection(q).IsOk Then
                Return Me.ProcessCommandMultiSelectItem(q)
            Else
                Throw New InvalidOperationException(My.Resources.FailedToClearSelection)
            End If

        ElseIf w.ClassContains("SSTabCtlWndClass") Then
            If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.MustBeHookedToInteractWithSSTabControls)
            Dim index As Integer = GetTabControlItemIndex(txt, index, w)
            If index = -1 Then
                Throw New InvalidOperationException(My.Resources.CanNotSelectFailedToFindMatchingTab)
            End If
            Dim cmd As String = "property_set " & w.Handle.ToString("X") & ",Tab,~S" & index.ToString()
            Dim sResult As String = Nothing
            If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(String.Format(My.Resources.CouldnTSelectTab01, index, sResult))
            Return Reply.Ok

        ElseIf w.ClassContains("SysTabControl32", "TTabControl", "PBTabControl32") Then
            Dim indexToSelect As Integer = GetTabControlItemIndex(txt, posn, w)
            If indexToSelect = -1 Then
                Throw New InvalidOperationException(My.Resources.CanNotSelectFailedToFindMatchingTab)
            End If
            'Select the new tab
            SendMessage(w.Handle, WindowMessages.TCM_SETCURSEL, indexToSelect, 0)

            'Send the parent window a notification to tell it of the new
            'tab selection.
            Dim parentWindow As clsUIWindow = CType(w.Parent, clsUIWindow)
            Dim childID As Int32 = GetWindowLong(w.Hwnd, GWL.GWL_ID) And &HFFFF
            Dim rpms As New RemoteProcessMessageSender(Of NMHDR, NMHDR)
            rpms.Handle = parentWindow.Handle
            rpms.MessageToSend = WindowMessages.WM_NOTIFY
            rpms.wParam = childID
            rpms.InputValues = New NMHDR() {New NMHDR(TCN.TCN_SELCHANGE, childID, w.Handle)}
            rpms.OutputValues = Nothing
            clsConfig.LogTiming("Win32: Sent TCN_SELCHANGE from control ID " & childID.ToString("X8") & " (handle:" & w.Handle.ToString("X8") & ") to " & parentWindow.Handle.ToString("X8"))
            rpms.SendMessage(ProcessHandle)

            'For .NET apps, simulate the reflected message from the parent.
            'See TabControl.WndProc() and TabControl.WmSelChange in reflector
            rpms.Handle = w.Handle
            rpms.MessageToSend = WindowMessages.WM_NOTIFY Or WindowMessages.WM_REFLECT
            clsConfig.LogTiming("Win32: Sent TCN_SELCHANGE from control ID " & childID.ToString("X8") & " (handle:" & w.Handle.ToString("X8") & ") to " & w.Handle.ToString("X8"))
            rpms.SendMessage(ProcessHandle)

            Return Reply.Ok

        Else
            Throw New InvalidOperationException(My.Resources.SelectItemIsNotImplementedForControlsWithClassname & w.ClassName & "'")
        End If

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Clicks an item in a Windows List View that matches the given text.")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' containing the text to match.")>
    Private Function ProcessCommandClickItem(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing AndAlso fallBackIndex < 0 Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")

                Dim result As Reply = ProcessCommandGetItemBounds(objQuery)
                If result.IsRect Then
                    Dim r As RECT = RECT.Parse(result.Message)

                    Dim buttonString As String = objQuery.GetParameter(ParameterNames.MouseButton)
                    Dim button As MouseButton

                    button = Me.GetButtonFromString(buttonString)


                    Const MK_LBUTTON As Integer = &H1
                    Const MK_RBUTTON As Integer = &H2

                    Dim downMessage As WindowMessages = WindowMessages.WM_LBUTTONDOWN
                    Dim upMessage As WindowMessages = WindowMessages.WM_LBUTTONUP
                    Dim wparam As Integer = MK_LBUTTON
                    If button = MouseButton.Right Then
                        downMessage = WindowMessages.WM_RBUTTONDOWN
                        upMessage = WindowMessages.WM_RBUTTONUP
                        wparam = MK_RBUTTON
                    End If

                    'Send a click message to the centre of the item's label
                    Dim x As Integer = r.Centre.X
                    Dim y As Integer = r.Centre.Y
                    Dim lParam As Integer = (y << 16) Or x 'Use bitshift to get the y value into the high order byte of the dword
                    PostMessage(w.Handle, downMessage, 0, lParam)
                    PostMessage(w.Handle, upMessage, wparam, lParam)

                    'If a right-click then send context menu message
                    If button = MouseButton.Right Then
                        Dim screenx As Integer = x + w.ScreenBounds.Left
                        Dim screeny As Integer = y + w.ScreenBounds.Top
                        Dim screenLParam As Integer = (screeny << 16) Or screenx
                        PostMessage(w.Handle, WindowMessages.WM_CONTEXTMENU, w.Handle, New IntPtr(screenLParam))
                    End If

                    Return Reply.Ok
                Else
                    Return result
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SelectItemIsNotImplementedForControlsWithClassname0, w.ClassName))
        End Select

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Checks that an item within a ListBox TreeView ListView or TabControl exists")>
    <Parameters("The text we are searching for specified by 'NewText' and those required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandCheckItemExists(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(searchText) Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Dim className = w.ClassName.ToLower(CultureInfo.InvariantCulture)
        Dim indexFound As Integer
        Select Case True
            Case className.Contains("listbox")
                indexFound = Me.GetItemIndex(w, searchText, -1)
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                indexFound = If(Me.GetTreenodeHandle(w.Handle, searchText) <> IntPtr.Zero, 0, -1)
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Me.GetListviewItemIndex(searchText, -1, w.Handle)
            Case w.ClassName.Contains("SysTabControl32"), w.ClassName.Contains("TTabControl"), w.ClassName.Contains("PBTabControl32")
                indexFound = Me.GetTabControlItemIndex(searchText, -1, w)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CheckItemExistsIsNotAvailableForControlsWithClassname0, w.ClassName))
        End Select

        If indexFound > -1 Then
            Return Reply.Result(True)
        Else
            Return Reply.Result(False)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Clicks a tab within an TabControl Window")>
    <Parameters("The index of the tab specified by 'Position' and those required to uniquely identify the window.")>
    Private Function ProcessCommandClickTab(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(searchText) Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("SysTabControl32"), w.ClassName.Contains("TTabControl"), w.ClassName.Contains("PBTabControl32")
                Dim indexToSelect As Integer = GetTabControlItemIndex(searchText, fallBackIndex, w)
                If indexToSelect = -1 Then
                    Throw New InvalidOperationException(My.Resources.CanNotSendClickMessageFailedToFindMatchingTab)
                Else
                    'Send a mouse down/up combination at the centre of the tab
                    Dim tabRect As RECT = GetTabBounds(w.Handle, indexToSelect)
                    Dim clickTarget As Point = tabRect.Centre
                    Dim lParam As Integer = (clickTarget.Y << 16) + clickTarget.X
                    SendMessage(w.Handle, WindowMessages.WM_LBUTTONDOWN, 0, lParam)
                    SendMessage(w.Handle, WindowMessages.WM_LBUTTONUP, 0, lParam)

                    Return Reply.Ok
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CanNotPerformClickTabOnAControlWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Clicks a tab within an TabControl Window using the mouse")>
    <Parameters("The index of the tab specified by 'Position' and those required to uniquely identify the window.")>
    Private Function ProcessCommandMouseClickTab(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(searchText) Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("SysTabControl32"), w.ClassName.Contains("TTabControl"), w.ClassName.Contains("PBTabControl32")
                Dim indexToSelect As Integer = GetTabControlItemIndex(searchText, fallBackIndex, w)
                If indexToSelect = -1 Then
                    Throw New InvalidOperationException(My.Resources.CanNotSendClickMessageFailedToFindMatchingTab)
                Else
                    'Send a genuine mouse click at the centre of the tab
                    Dim relativeClickTarget As Point = GetTabBounds(w.Handle, indexToSelect).Centre
                    Dim screenTarget As Point = Point.Add(w.ScreenBounds.Location, New Size(relativeClickTarget))
                    Me.DoClickMouse(screenTarget)

                    Return Reply.Ok
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CanNotPerformMouseClickTabOnAControlWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Clicks a button within a Toolbar Window")>
    <Parameters("The 1-based index (specified by 'Position') of the button of interest and those required to uniquely identify the window.")>
    Private Function ProcessCommandClickToolbarButton(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the button index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("msvb_lib_toolbar"), w.ClassName.Contains("ToolbarWindow32"), w.ClassName.Contains("PBTabControl32")
                Dim indexToSelect As Integer = fallBackIndex
                If indexToSelect = -1 Then
                    Throw New InvalidOperationException(My.Resources.CanNotSendClickMessageFailedToFindMatchingButton)
                Else
                    'Send a mouse down/up combination at the centre of the tab
                    Dim buttonRect As RECT = Me.GetToolbarButtonBounds(w.Handle, indexToSelect)
                    Dim clickTarget As Point = buttonRect.Centre
                    Dim lParam As Integer = (clickTarget.Y << 16) + clickTarget.X
                    PostMessage(w.Handle, WindowMessages.WM_LBUTTONDOWN, 0, lParam)
                    PostMessage(w.Handle, WindowMessages.WM_LBUTTONUP, 0, lParam)
                    Return Reply.Ok
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CanNotPerformClickToolbarButtonOnAControlWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Reads the 'enabled' state of a toolbar button")>
    <Parameters("The 1-based index (specified by 'Position') of the button of interest and those required to uniquely identify the window.")>
    Private Function ProcessCommandIsToolbarButtonEnabled(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the button index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("msvb_lib_toolbar"), w.ClassName.Contains("ToolbarWindow32"), w.ClassName.Contains("PBTabControl32")
                Dim itemIndex As Integer = fallBackIndex
                If itemIndex = -1 Then
                    Throw New InvalidOperationException(My.Resources.FailedToFindMatchingButton)
                Else
                    Dim buttonState As TBSTATE = Me.GetToolbarButtonState(w.Handle, itemIndex)
                    Dim isEnabled As Boolean = (buttonState And TBSTATE.TBSTATE_ENABLED) > 0
                    Return Reply.Result(isEnabled)
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CanNotPerformIsToolbarButtonEnabledOnAControlWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Reads the 'checked' state of a toolbar button")>
    <Parameters("The 1-based index (specified by 'Position') of the button of interest and those required to uniquely identify the window.")>
    Private Function ProcessCommandIsToolbarButtonChecked(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the button index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("msvb_lib_toolbar"), w.ClassName.Contains("ToolbarWindow32"), w.ClassName.Contains("PBTabControl32")
                Dim itemIndex As Integer = fallBackIndex
                If itemIndex = -1 Then
                    Throw New InvalidOperationException(My.Resources.FailedToFindMatchingButton)
                Else
                    Dim buttonState As TBSTATE = Me.GetToolbarButtonState(w.Handle, itemIndex)
                    Dim isChecked As Boolean = (buttonState And TBSTATE.TBSTATE_CHECKED) > 0
                    Return Reply.Result(isChecked)
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CanNotPerformIsToolbarButtonCheckedOnAControlWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Reads the 'pressed' state of a toolbar button")>
    <Parameters("The 1-based index (specified by 'Position') of the button of interest and those required to uniquely identify the window.")>
    Private Function ProcessCommandIsToolbarButtonPressed(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the button index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("msvb_lib_toolbar"), w.ClassName.Contains("ToolbarWindow32"), w.ClassName.Contains("PBTabControl32")
                Dim itemIndex As Integer = fallBackIndex
                If itemIndex = -1 Then
                    Throw New InvalidOperationException(My.Resources.FailedToFindMatchingButton)
                Else
                    Dim buttonState As TBSTATE = Me.GetToolbarButtonState(w.Handle, itemIndex)
                    Dim isPressed As Boolean = (buttonState And TBSTATE.TBSTATE_PRESSED) > 0
                    Return Reply.Result(isPressed)
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.CanNotPerformIsToolbarButtonPressedOnAControlWithClassName0, w.ClassName))
        End Select
    End Function

    ''' <summary>
    ''' Determines whether a win32 listbox accepts multiple selections.
    ''' </summary>
    ''' <param name="Handle">The handle to the listbox of interest.</param>
    ''' <returns>Returns True if the listbox accepts multiple selections, False
    ''' otherwise.</returns>
    Private Function IsListboxMultiSelection(ByVal Handle As IntPtr) As Boolean
        Dim style As Integer = GetWindowLong(Handle, GWL.GWL_STYLE)
        Return ((style And (LBS.LBS_MULTIPLESEL Or LBS.LBS_EXTENDEDSEL)) > 0)
    End Function

    <Category(Category.Win32)>
    <Command("Clears a selected items within a ListView Window.")>
    Private Function ProcessCommandClearSelection(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim item As New LV_ITEM
                item.mask = LVIF.LVIF_STATE
                item.stateMask = modWin32.LVIS.LVIS_SELECTED
                item.state = 0
                item.iItem = -1  'Index of -1 means apply to all items
                Dim inputs As LV_ITEM()
                ReDim inputs(0)
                inputs(0) = item
                Dim rpms As New RemoteProcessMessageSender(Of LV_ITEM, LV_ITEM)
                rpms.Handle = w.Handle
                rpms.InputValues = inputs
                rpms.OutputValues = Nothing
                rpms.MessageToSend = WindowMessages.LVM_SETITEMSTATE
                rpms.wParam = -1 'Index of -1 means apply to all items
                rpms.SendMessage(ProcessHandle)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.ClearSelectionIsNotImplementedForControlsWithClassname0, w.ClassName))
        End Select
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Gets the handle of the treeview item with the specified text, in the
    ''' specified treeview.
    ''' </summary>
    ''' <param name="hwnd">The handle of the parent treeview.</param>
    ''' <param name="txt">The text to search for.</param>
    ''' <returns>The handle of the first matching node, or -1 (!?) if not found.
    ''' </returns>
    Private Function GetTreenodeHandle(hwnd As IntPtr, txt As String) As IntPtr

        Dim rootNode = SendMessage(hwnd, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_ROOT, 0)
        Dim node = FindTreenode(hwnd, rootNode, txt)
        If node <> IntPtr.Zero Then Return node

        'If not yet found, loop through each subsequent sibling, descending into each
        Dim nextItem = rootNode
        Do
            nextItem = SendMessage(hwnd, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_NEXT, nextItem)

            ' No nextItem? That means that there are no more siblings left
            If nextItem = IntPtr.Zero Then Return IntPtr.Zero

            node = FindTreenode(hwnd, nextItem, txt)
            If node <> IntPtr.Zero Then Return node
        Loop

        'No matching item found
        Return IntPtr.Zero
    End Function

    ''' <summary>
    ''' Gets the handle to the treenode at the specified (0-based) position in the
    ''' top level of the treeview with the specified handle.
    ''' </summary>
    ''' <param name="hwnd">The window handle of the treeview</param>
    ''' <param name="posn">The 0-based position of the node required from the root
    ''' nodes in the treeview - ie. 0 represents the first node at the top level of
    ''' the tree; 1 represents its immediate sibling, etc.</param>
    ''' <returns>The node handle of the required treenode, or -1 if no such node was
    ''' found in the top level nodes of the treeview.</returns>
    ''' <exception cref="InvalidArgumentException">If <paramref name="posn"/> is less
    ''' than 0.</exception>
    Private Function GetTreenodeHandle(hwnd As IntPtr, posn As Integer) As IntPtr
        If posn < 0 Then Throw New InvalidArgumentException(
            My.Resources.ThePositionOfTheTreenodeMustBe0OrMorePosnFound0, posn)

        Dim count As Integer = 0
        ' Start at the root node (TVGN_ROOT)
        Dim nextItem = SendMessage(hwnd, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_ROOT, 0)

        ' Loop through the siblings until we either reach the number that we're after
        ' or we run out of siblings to search
        While nextItem <> IntPtr.Zero AndAlso count < posn
            count += 1
            nextItem = SendMessage(hwnd, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_NEXT, nextItem)
        End While

        ' A zero node handle means no node; translate to -1 which means the same to
        ' our callers
        Return If(nextItem = IntPtr.Zero, IntPtr.Zero, nextItem)

    End Function

    ''' <summary>
    ''' Finds the first treenode with the specified text, via a recursive search.
    ''' </summary>
    ''' <param name="hwnd">The handle of the treeview of interest.</param>
    ''' <param name="n">The handle of the node at which to start the
    ''' search. The node and all of its descendants will be searched.</param>
    ''' <param name="txt">The text to match on the node.</param>
    ''' <returns>Returns the handle of the first matching node, or -1 (!?) if not
    ''' found.</returns>
    Private Function FindTreenode(ByVal hwnd As IntPtr, ByVal n As IntPtr, ByVal txt As String) As IntPtr
        Dim node As TreeviewItem = GetTreenodeItem(hwnd, n)

        ' Null node means that the starting node is not a valid treenode
        If node Is Nothing Then Return IntPtr.Zero

        ' If we've found our target, might as well exit now
        If node.Text = txt Then Return n

        Dim childNode = SendMessage(hwnd, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_CHILD, n)

        ' If there's no children, report as much
        If childNode.ToInt64() <= 0 Then Return IntPtr.Zero

        'Descend into the first child
        Dim resp = FindTreenode(hwnd, childNode, txt)
        If resp.ToInt64() > 0 Then Return resp

        'If not yet found, loop through each subsequent sibling, descending into each
        Dim nextItem = childNode
        Do
            nextItem = SendMessage(hwnd, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_NEXT, nextItem)
            ' No nextItem? We've run out of things to check
            If nextItem.ToInt64() <= 0 Then Return IntPtr.Zero

            resp = FindTreenode(hwnd, nextItem, txt)
            If resp.ToInt64() > 0 Then Return resp
        Loop

        ' No matching item found
        Return IntPtr.Zero

    End Function

    Private Function GetTreenodeItem(ByVal TreeviewHandle As IntPtr, ByVal TreenodeHandle As IntPtr) As TreeviewItem
        Dim pStrBufferMemory As IntPtr
        Dim pRemoteItem As IntPtr

        Const MAX_LVMSTRING As Integer = 255

        'Allocate the string buffer
        pStrBufferMemory = VirtualAllocEx(ProcessHandle, IntPtr.Zero, MAX_LVMSTRING, MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)

        'initialize the local structure
        Dim myItem As New TVITEM()
        myItem.mask = TreeviewGetItemFlags.TVIF_TEXT Or TreeviewGetItemFlags.TVIF_STATE Or TreeviewGetItemFlags.TVIF_HANDLE Or TreeviewGetItemFlags.TVIF_CHILDREN
        myItem.stateMask = 255 'This indicates that we wish to retrieve bits 0 to 7 of the state bitfield
        myItem.pszText = pStrBufferMemory
        myItem.cchTextMax = MAX_LVMSTRING
        myItem.hItem = TreenodeHandle 'Indicates which treenode we would like info about

        'write the structure into the remote process's memory space
        Dim structLength As Integer = Marshal.SizeOf(GetType(TVITEM))
        Dim pLocalItem As IntPtr = Marshal.AllocHGlobal(structLength)
        Marshal.StructureToPtr(myItem, pLocalItem, False)
        pRemoteItem = VirtualAllocEx(ProcessHandle, IntPtr.Zero, structLength, MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)
        WriteProcessMemory(ProcessHandle, pRemoteItem, pLocalItem, structLength, IntPtr.Zero)
        Marshal.FreeHGlobal(pLocalItem)

        'send the GetItem message 
        SendMessage(TreeviewHandle, WindowMessages.TVM_GETITEM, 0, pRemoteItem)

        'Get the item text
        Dim pTempstring As IntPtr = Marshal.AllocHGlobal(MAX_LVMSTRING)
        ReadProcessMemory(ProcessHandle, pStrBufferMemory, pTempstring, MAX_LVMSTRING, IntPtr.Zero)
        Dim localString(MAX_LVMSTRING - 1) As Byte
        Marshal.Copy(pTempstring, localString, 0, MAX_LVMSTRING)
        Marshal.FreeHGlobal(pTempstring)

        'Get the item structure
        Dim pNewItem As IntPtr = Marshal.AllocHGlobal(structLength)
        ReadProcessMemory(ProcessHandle, pRemoteItem, pNewItem, structLength, IntPtr.Zero)
        Dim newTVItem As TVITEM = CType(Marshal.PtrToStructure(pNewItem, GetType(TVITEM)), TVITEM)
        Marshal.FreeHGlobal(pNewItem)

        ' Instantiate here before de-allocating memory - trim any null chars from the
        ' end of the string (ie. null terminator and padding).
        Dim retVal As New TreeviewItem(newTVItem,
         Encoding.ASCII.GetString(localString).Trim(Char.MinValue))

        'Deallocate memory
        VirtualFreeEx(ProcessHandle, pStrBufferMemory, 0, MEM_RELEASE)
        VirtualFreeEx(ProcessHandle, pRemoteItem, 0, MEM_RELEASE)

        Return retVal
    End Function


    ''' <summary>
    ''' Finds the first java tree node with the specified text.
    ''' </summary>
    ''' <param name="StartingNode">The node at which to begin the search.
    ''' The node itself and all of its descendents will be searched.</param>
    ''' <param name="NodeText">The text to match against treenodes.</param>
    ''' <returns>Returns the first node found whilst descending the tree
    ''' in preorder.</returns>
    ''' <remarks>The tree traversal is done in pre-order rather than
    ''' level-order.</remarks>
    Private Function FindJavaTreenode(ByVal StartingNode As JABContext, ByVal NodeText As String) As JABContext
        If StartingNode IsNot Nothing Then
            StartingNode.UpdateCachedInfo()

            Dim text As String = String.Empty
            Dim sErr As String = Nothing

            'The node must be expanded if its children are to be
            'read (see bug 2895). Thus we expand it and then take
            'care to restore it to its previous state afterwards
            Dim wasExpanded As Boolean
            If Not StartingNode.Expanded Then
                mJAB.DoAction(StartingNode, "toggle expand")
            Else
                wasExpanded = True
            End If

            Try
                If JABWrapper.GetText(StartingNode, text, sErr) Then
                    If text = NodeText Then Return StartingNode
                End If

                For Each ChildNode As JABContext In StartingNode.Children
                    Dim foundNode As JABContext = FindJavaTreenode(ChildNode, NodeText)
                    If foundNode IsNot Nothing Then Return foundNode
                Next
            Finally
                'Restore 'expanded' state of tree node to its previous state
                StartingNode.UpdateCachedInfo()
                If wasExpanded <> StartingNode.Expanded Then
                    mJAB.DoAction(StartingNode, "toggle expand")
                End If
            End Try
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the bounds of the specified tab from a win32 tab control.
    ''' </summary>
    ''' <param name="TabControlHandle">The handle of the tabcontrol of interest</param>
    ''' <param name="TabIndex">The index of the tab of interest.</param>
    ''' <returns>Returns the bounds of the clickable tab (note not the bounds 
    ''' of the page it corresponds to, if any).</returns>
    Private Function GetTabBounds(ByVal TabControlHandle As IntPtr, ByVal TabIndex As Integer) As RECT
        Dim tabRect As RECT
        Dim rectRpms As New RemoteProcessMessageSender(Of RECT, RECT)
        rectRpms.Handle = TabControlHandle
        rectRpms.MessageToSend = WindowMessages.TCM_GETITEMRECT
        rectRpms.wParam = TabIndex
        rectRpms.InputValues = New RECT() {tabRect}
        rectRpms.OutputValues = New RECT() {}
        ReDim rectRpms.OutputValues(0)
        rectRpms.SendMessage(ProcessHandle)

        Return rectRpms.OutputValues(0)
    End Function

    ''' <summary>
    ''' Gets the index of the named combobox item.
    ''' </summary>
    ''' <param name="w">The clsUIWindow object of the combobox of interest.</param>
    ''' <param name="searchtext">The text of the combobox item of interest</param>
    ''' <param name="FallbackIndex">An index to return if no item with the
    ''' search text is found.</param>
    ''' <returns>Returns the index of the item of interest
    ''' or the FallBack index if no such exists.</returns>
    Private Function GetItemIndex(ByVal w As clsUIWindow, ByVal SearchText As String, ByVal FallbackIndex As Integer) As Integer
        Dim itemCount As Integer = Me.GetItemCount(w)
        Dim textLenMessage As WindowMessages
        Dim getTextMessage As WindowMessages

        Dim className = w.ClassName.ToLower(CultureInfo.InvariantCulture)
        Select Case True
            Case className.Contains("listbox"), className.Contains("combolbox")
                textLenMessage = WindowMessages.LB_GETTEXTLEN
                getTextMessage = WindowMessages.LB_GETTEXT
            Case className.Contains("combobox")
                textLenMessage = WindowMessages.CB_GETLBTEXTLEN
                getTextMessage = WindowMessages.CB_GETLBTEXT
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetItemIndexIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select
        Try
            For itemIndex As Integer = 0 To itemCount
                Dim result As String = GetItemText(w.Handle, textLenMessage, getTextMessage, itemIndex)
                If result = SearchText Then
                    Return itemIndex
                End If
            Next

            Return FallbackIndex
        Catch
            Return FallbackIndex
        End Try
    End Function

    ''' <summary>
    ''' Get the number of items (i.e. tabs) in a tab control.
    ''' </summary>
    ''' <param name="w">The clsUIWindow representing the tab control.</param>
    ''' <returns>The tab count. Throws an Exception if the count can't be retrieved.
    ''' </returns>
    Private Function GetTabControlItemCount(ByVal w As clsUIWindow) As Integer
        If w.ClassName = "SSTabCtlWndClass" Then
            Dim cmd As String = "property_get " & w.Handle.ToString("X") & ",Tabs"
            Dim sResult As String = Nothing
            If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
            Return Integer.Parse(sResult)
        Else
            Return SendMessage(w.Handle, WindowMessages.TCM_GETITEMCOUNT, 0, 0).ToInt32()
        End If
    End Function

    ''' <summary>
    ''' Gets the index of the named tab control item.
    ''' </summary>
    ''' <param name="searchtext">The text of the tab control item of interest</param>
    ''' <param name="FallbackIndex">An index to return if no item with the
    ''' search text is found.</param>
    ''' <param name="w">The clsUIWindow representing the tab control.</param>
    ''' <returns>Returns the index of the item of interest or the fallback index if
    ''' no such exists.</returns>
    Private Function GetTabControlItemIndex(ByVal SearchText As String, ByVal FallbackIndex As Integer, ByVal w As clsUIWindow) As Integer
        If SearchText IsNot Nothing Then
            Dim numitems As Integer = GetTabControlItemCount(w)
            For i As Integer = 0 To numitems - 1
                Dim itemText As String = GetTabControlItemText(w, i)
                If String.Compare(SearchText, itemText) = 0 Then
                    Return i
                End If
            Next
        End If

        Return FallbackIndex
    End Function

    ''' <summary>
    ''' Gets the index of the named listview item.
    ''' </summary>
    ''' <param name="SearchText">The text of the listview item
    ''' of interest. May be null, in which case, the fallback index
    ''' will be returned.</param>
    ''' <param name="ListViewHandle">Handle to the listview
    ''' of interest.</param>
    ''' <param name="FallbackIndex">An index to return if no item with
    ''' the search text is found.</param>
    ''' <returns>Returns the index of the item of interest
    ''' or the FallBack index if no such exists.</returns>
    Private Function GetListviewItemIndex(ByVal searchtext As String, ByVal FallbackIndex As Integer, ByVal ListViewHandle As IntPtr) As Integer
        If searchtext Is Nothing Then Return FallbackIndex

        Dim pRemoteBuffer As IntPtr
        Dim Result As Integer
        Try
            'Allocate a buffer with the search string inside
            pRemoteBuffer = VirtualAllocEx(ProcessHandle, IntPtr.Zero, searchtext.Length + 1, MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)
            Dim pLocalBuffer As IntPtr = Marshal.StringToHGlobalAnsi(searchtext)
            WriteProcessMemory(ProcessHandle, pRemoteBuffer, pLocalBuffer, searchtext.Length + 1, IntPtr.Zero)
            Marshal.FreeHGlobal(pLocalBuffer)

            'Do the search
            Dim rpms As New RemoteProcessMessageSender(Of LVFINDINFO, LVFINDINFO)
            rpms.Handle = ListViewHandle
            rpms.MessageToSend = WindowMessages.LVM_FINDITEM
            rpms.wParam = -1
            Dim input As LVFINDINFO
            input.flags = LVFI.LVFI_STRING
            input.psz = pRemoteBuffer
            rpms.InputValues = New LVFINDINFO() {input}
            Result = rpms.SendMessage(ProcessHandle).ToInt32()

            If Not rpms.Success Then
                Throw New ApiException("API call failed. " & GetLastWin32Error())
            End If
        Catch ex As InvalidOperationException
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstSearchingForMatchingListviewItem, ex)
        Finally
            'Free the allocated string buffer
            If pRemoteBuffer <> IntPtr.Zero Then
                VirtualFreeEx(ProcessHandle, pRemoteBuffer, 0, MEM_RELEASE)
            End If
        End Try

        If Result = -1 Then
            Return FallbackIndex
        Else
            Return Result
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Gets the bounds of an item within a ListView Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("The bounding rectangle specified by ""RECT:<left>,<top>,<bottom>,<right>""")>
    Private Function ProcessCommandGetItemBounds(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(searchText) Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                'Get the listview item index
                Dim listviewHandle = w.Handle
                Dim index As Integer
                index = Me.GetListviewItemIndex(searchText, -1, listviewHandle)
                If index = -1 Then
                    Throw New InvalidOperationException(String.Format(My.Resources.NoItemWithTheText0Found, searchText))
                End If

                'Prepare the input RECT structure
                Const LVIR_BOUNDS As Integer = 0
                Dim r As RECT
                r.Left = LVIR_BOUNDS
                Dim inputValues As RECT() = {r}

                'Prepare remote memory message sender (RPMS)
                Dim rpms As New RemoteProcessMessageSender(Of RECT, RECT)
                rpms.Handle = w.Handle
                rpms.MessageToSend = WindowMessages.LVM_GETITEMRECT
                rpms.wParam = index
                rpms.InputValues = inputValues
                Dim OutputValues(0) As RECT
                rpms.OutputValues = OutputValues

                Dim retVal = rpms.SendMessage(ProcessHandle).ToInt64()
                If retVal > 0 Then
                    Return Reply.Rect(rpms.OutputValues(0).ToString())
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveBoundsOfListviewItem0, searchText))
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetItemBoundsIsNotSupportedForClassType0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the bounds of an item within a ListView Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("The bounding rectangle specified by ""RECT:<left>,<top>,<bottom>,<right>""")>
    Private Function ProcessCommandGetItemBoundsAsCollection(ByVal objQuery As clsQuery) As Reply
        Dim result As Reply = ProcessCommandGetItemBounds(objQuery)
        If result.IsRect Then
            Return Reply.Result(CreateCollectionXMLFromRectangle(RECT.Parse(result.Message)))
        Else
            Return result
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Gets the bounds of an item within a ListView Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("The bounding rectangle specified by ""RECT:<left>,<top>,<bottom>,<right>""")>
    Private Function ProcessCommandGetItemScreenBoundsAsCollection(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim result As Reply = ProcessCommandGetItemBounds(objQuery)
                If result.IsRect Then
                    Return Reply.Result(CreateCollectionXMLFromRectangle(RECT.Parse(result.Message)))
                Else
                    Return result
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetItemScreenBoundsAsCollectionIsNotSupportedForClassType0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Starts the Drag and Drop of a listview item")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandDragListviewItem(ByVal objQuery As clsQuery) As Reply
        'The index of the item of interest
        Dim foundItemIndex As Integer
        Dim listviewHandle As IntPtr
        Try
            foundItemIndex = IdentifyListviewItemIndex(objQuery, listviewHandle)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnableToIdentifyListviewItemOfInterest0, ex.Message))
        End Try
        If foundItemIndex = -1 Then
            Throw New InvalidOperationException(My.Resources.CouldNotFindSpecifiedListviewItem)
        End If

        'Get bounds of icon on listview item
        Dim labelRect As RECT
        Try
            labelRect = GetListviewItemBounds(listviewHandle, foundItemIndex, LVIR.LVIR_LABEL)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnableToGetItemSLABELBounds0, ex.Message))
        End Try

        Dim query As String = objQuery.QueryString
        query &= " targx=" & (labelRect.Left + 1).ToString & " targy=" & (labelRect.Top + 1).ToString()
        Return ProcessCommandDrag(clsQuery.Parse(query))
    End Function

    ''' <summary>
    ''' Determines the zero-based index of a listview item described by the supplied
    ''' query.
    ''' </summary>
    ''' <param name="objQuery">The query describing the listview item of interest.
    ''' The query must contain 'Position' and 'newtext' arguments, describing
    ''' the 1-based index of a listview item, or alternatively some searchtext.
    ''' The query must also identify the listview of interest.</param>
    ''' <param name="listviewHandle">Carries back the handle of the listview
    ''' identified by the query.</param>
    ''' <returns>Returns the zero-based index of the first matching listview
    ''' item. If no match is found, then -1 is returned.</returns>
    ''' <remarks>An exception may be thrown if an error occurs.</remarks>
    Private Function IdentifyListviewItemIndex(ByVal objQuery As clsQuery, ByRef listviewHandle As IntPtr) As Integer
        Dim w As clsUIWindow
        Try
            w = mobjModel.IdentifyWindow(objQuery)
        Catch e As ApplicationException
            Throw e
        End Try
        listviewHandle = w.Handle

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        'The index of the item of interest
        Return GetListviewItemIndex(searchText, fallBackIndex, w.Handle)
    End Function

    <Category(Category.Win32)>
    <Command("Ends the Drag and Drop of a listview item")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandDropOntoListviewItem(ByVal objQuery As clsQuery) As Reply
        'The index of the item of interest
        Dim foundItemIndex As Integer
        Dim listviewHandle As IntPtr
        Try
            foundItemIndex = IdentifyListviewItemIndex(objQuery, listviewHandle)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnableToIdentifyListviewItemOfInterest0, ex.Message))
        End Try
        If foundItemIndex = -1 Then
            Throw New InvalidOperationException(My.Resources.CouldNotFindSpecifiedListviewItem)
        End If

        'Get bounds of icon on listview item
        Dim labelRect As RECT
        Try
            labelRect = GetListviewItemBounds(listviewHandle, foundItemIndex, LVIR.LVIR_LABEL)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnableToGetItemSLABELBounds0, ex.Message))
        End Try

        Dim query As String = objQuery.QueryString
        query &= " targx=" & (labelRect.Left + 1).ToString & " targy=" & (labelRect.Top + 1).ToString()
        Return Me.ProcessCommandDrop(clsQuery.Parse(query))
    End Function

    <Category(Category.Win32)>
    <Command("Scrolls a listview to the end, by using the EnsureItemVisible command on the highest available index.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandScrollListviewToBottom(ByVal objQuery As clsQuery) As Reply
        Dim result As Reply = Me.ProcessCommandGetItemCount(objQuery)
        Dim lastIndex As Integer
        Try
            lastIndex = CInt(result.Message)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToCountItemsInListviewCouldNotParseValue0AsAnInteger, result.Message))
        End Try

        Dim query As String = objQuery.QueryString & " position=" & lastIndex.ToString
        Return Me.ProcessCommandEnsureItemVisible(clsQuery.Parse(query))
    End Function

    <Category(Category.Win32)>
    <Command("Scrolls a listview to the top, by using the EnsureListviewItemVisible command on the lowest available index." & vbCrLf &
    "This is safe to use even if there are no items in the listview")>
    Private Function ProcessCommandScrollListviewToTop(ByVal objQuery As clsQuery) As Reply
        Dim result As Reply = Me.ProcessCommandGetItemCount(objQuery)
        Dim firstIndex As Integer
        Try
            firstIndex = CInt(result.Message)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToCountItemsInListviewCouldNotParseValue0AsAnInteger, result.Message))
        End Try

        If firstIndex > 0 Then
            Dim query As String = objQuery.QueryString & " position=1"
            Return Me.ProcessCommandEnsureItemVisible(clsQuery.Parse(query))
        Else
            'No need to do anything if there are no items
            Return Reply.Ok
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Ensures that the specified listview item is visible.")>
    <Parameters("ordinal and newtext, used to specify the item by 1-based index or by name (respectively). Only one parameter need be specified; index is used in preference to newtext.")>
    Private Function ProcessCommandEnsureItemVisible(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim listviewHandle = w.Handle

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim searchText As String = Nothing
                Dim ordinal As Integer

                'Get the Index of the item, or the item text
                Dim ordinalValue As String = objQuery.GetParameter(ParameterNames.Position)
                If Not String.IsNullOrEmpty(ordinalValue) Then
                    Try
                        ordinal = CInt(ordinalValue)
                        If ordinal <= 0 Then
                            Throw New InvalidOperationException(My.Resources.ValueSuppliedToPositionMustBeAtLeast1)
                        End If
                    Catch ex As Exception
                        Throw New InvalidOperationException(String.Format(My.Resources.TheValue0IsInvalidForParameterPosition, ordinalValue))
                    End Try
                Else
                    'Get the text of the item to be made visible
                    searchText = objQuery.GetParameter(ParameterNames.NewText)
                    If String.IsNullOrEmpty(searchText) Then
                        Throw New InvalidOperationException(My.Resources.NoItemSpecifiedUseOneOfItemTextParameterOrPositionParameter)
                    End If
                End If

                Dim index As Integer
                If ordinal <= 0 Then
                    'Get the listview item index

                    index = Me.GetListviewItemIndex(searchText, ordinal, listviewHandle)

                    If index = -1 Then
                        Throw New InvalidOperationException(String.Format(My.Resources.NoItemWithTheText0Found, searchText))
                    End If
                Else
                    index = ordinal - 1 'Correction for 1-based index
                End If

                'Send the message, requiring the whole of the item to become visible
                SendMessage(listviewHandle, WindowMessages.LVM_ENSUREVISIBLE, index, 0)
                Return Reply.Ok
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)

                Dim treeNodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
                If treeNodeHandle = IntPtr.Zero Then
                    Throw New InvalidOperationException(String.Format(My.Resources.NoTreeNodeWithTheText0Exists, searchText))
                End If

                SendMessage(w.Handle, WindowMessages.TVM_ENSUREVISIBLE, 0, treeNodeHandle)
                Return Reply.Ok
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.EnsureVisibleIsNotAvailableForClassType0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Expands the node of a treenode Window.")>
    <Parameters("The name of the node to expand specified by 'NewText' and those required to uniquely identify the window.")>
    Private Function ProcessCommandExpandTreeNode(ByVal objquery As clsQuery) As Reply
        Return DoTreeviewNodeExpansion(objquery, TreeviewExpandFlags.TVE_EXPAND)
    End Function

    <Category(Category.Win32)>
    <Command("Collapses the node of a treenode Window.")>
    <Parameters("The name of the node to collapse specified by 'NewText' and those required to uniquely identify the window.")>
    Private Function ProcessCommandCollapseTreeNode(ByVal objquery As clsQuery) As Reply
        Return DoTreeviewNodeExpansion(objquery, TreeviewExpandFlags.TVE_COLLAPSE)
    End Function

    <Category(Category.Win32)>
    <Command("Toggles the node of a treenode Window.")>
    <Parameters("The name of the node to Toggle specified by 'NewText' and those required to uniquely identify the window.")>
    Private Function ProcessCommandToggleTreeNode(ByVal objquery As clsQuery) As Reply
        Return DoTreeviewNodeExpansion(objquery, TreeviewExpandFlags.TVE_TOGGLE)
    End Function

    Private Function DoTreeviewNodeExpansion(ByVal objquery As clsQuery, ByVal Operation As TreeviewExpandFlags) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Dim searchText As String = objquery.GetParameter(ParameterNames.NewText)
        Dim treenodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
        If treenodeHandle = IntPtr.Zero Then
            Throw New InvalidOperationException(String.Format(My.Resources.NoTreenodeWithTheText0WasFound, searchText))
        End If

        SendMessage(w.Handle, WindowMessages.TVM_EXPAND, Operation, treenodeHandle)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Gets the Numeric value in a Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<value>"" where <value> is the numeric value in the window")>
    Private Function ProcessCommandGetNumericValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Select Case True
            Case w.ClassName.Contains("msctls_trackbar32"), w.ClassName.Contains("TTrackBar")
                Dim result = SendMessage(w.Handle, WindowMessages.TBM_GETPOS, 0, 0)
                Return Reply.Result(result.ToInt32())
            Case w.ClassName.Contains("msctls_updown32"), w.ClassName.Contains("TUpDown")
                Dim result = SendMessage(w.Handle, WindowMessages.UDM_GETPOS, 0, 0)
                Dim success As Boolean = GetHighWord(result.ToInt32()) = 0
                If success Then
                    Return Reply.Result(GetLowWord(result.ToInt32()))
                Else
                    Throw New InvalidOperationException(My.Resources.APICallFailedValueWasNotRetrieved)
                End If
            Case w.ClassName.Contains("SCROLLBAR")
                Dim retVal As Integer = GetScrollPos(w.Hwnd, ScrollBarDirection.SB_CTL)
                Return Reply.Result(retVal)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetNumericValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    ''' <summary>
    ''' Get the low word from an integer.
    ''' </summary>
    ''' <param name="value">The integer to get the low word from</param>
    Private Function GetLowWord(ByVal value As Integer) As Integer
        Return value And &HFFFF
    End Function

    ''' <summary>
    ''' Get the high word from an integer.
    ''' </summary>
    ''' <param name="value">The integer to get the high word from</param>
    Private Function GetHighWord(ByVal value As Integer) As Integer
        Return (value >> 16) And &HFFFF
    End Function

    ''' <summary>
    ''' Set the low word of an integer.
    ''' </summary>
    ''' <param name="number">The integer to set</param>
    ''' <param name="value">The low word value</param>
    Public Function SetLowWord(number As Integer, value As Integer) As Integer
        Return (number And &HFFFF0000) + (value And &HFFFF)
    End Function

    ''' <summary>
    ''' Set the high word of an integer.
    ''' </summary>
    ''' <param name="number">The integer to set</param>
    ''' <param name="value">The high word value</param>
    Public Function SetHighWord(number As Integer, value As Integer) As Integer
        Return (number And &HFFFF) + (value << 16)
    End Function

    ''' <summary>
    ''' Set the low and high words of an integer word of an integer.
    ''' </summary>
    ''' <param name="low">The low word value</param>
    ''' <param name="high">The high word value</param>
    Public Shared Function MakeWParam(low As Integer, high As Integer) As Integer
        Return (high << 16) Or (low And &HFFFF)
    End Function

    <Category(Category.Win32)>
    <Command("Sets the Numeric value in a Window.")>
    <Parameters("The numeric value specified by 'NumericValue' and those required to uniquely identify the window.")>
    Private Function ProcessCommandSetNumericValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Dim temp As String = objquery.GetParameter(ParameterNames.NumericValue)
        Dim value As Double
        If Not Double.TryParse(temp, value) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToParseValue0AsFloatingPointNumber, temp))
        End If

        Select Case True

            Case w.ClassName.Contains("msctls_trackbar32"), w.ClassName.Contains("TTrackBar")
                'Set new value
                Dim newValue As Integer = CInt(value)
                SendMessage(w.Handle, WindowMessages.TBM_SETPOS, 1, newValue)

                'Notify parent of the change
                Dim parentWindow As clsUIWindow = TryCast(w.Parent, clsUIWindow)
                If parentWindow IsNot Nothing Then
                    Dim Style As Integer = modWin32.GetWindowLong(w.Hwnd, modWin32.GWL.GWL_STYLE)
                    Dim Message As WindowMessages
                    If (Style And TBS.TBS_VERT) > 0 Then
                        Message = WindowMessages.WM_VSCROLL
                    Else
                        Message = WindowMessages.WM_HSCROLL
                    End If
                    SendMessage(parentWindow.Handle, Message, (newValue << 16) + TB.TB_THUMBPOSITION, w.Handle)
                    SendMessage(parentWindow.Handle, Message, TB.TB_ENDTRACK, w.Handle)
                End If

                Return Reply.Ok

            Case w.ClassName.Contains("msctls_updown32"), w.ClassName.Contains("TUpDown")
                Dim newValue As Integer = CInt(value)

                'Notify parent of the change
                Dim parentWindow As clsUIWindow = TryCast(w.Parent, clsUIWindow)
                If parentWindow IsNot Nothing Then

                    'Get current position, so that we can calculate the 'delta'
                    'change
                    Dim currentValue = SendMessage(w.Handle, WindowMessages.UDM_GETPOS32, 0, 0)
                    Dim delta As Integer = newValue - currentValue.ToInt32()

                    'We first send WM_NOTIFY with UDN_DELTAPOS. If the application
                    'rejects this proposed change then we must respect it
                    Dim hdr As New NMHDR(UDN.UDN_DELTAPOS, modWin32.GetDlgCtrlID(w.Hwnd), w.Handle)
                    Dim updown As New NMUPDOWN(hdr, currentValue.ToInt32(), delta)
                    Dim retVal = SendMessage(parentWindow.Handle, WindowMessages.WM_NOTIFY, updown.hdr.idFrom, updown)
                    If retVal.ToInt64() > 0 Then
                        Throw New InvalidOperationException(My.Resources.CanNotAmendTargetValueBecauseTheOwnerApplicationRefusedTheRequest)
                    End If

                    'Otherwise we go ahead and send the new value
                    Dim style As Integer = modWin32.GetWindowLong(w.Hwnd, modWin32.GWL.GWL_STYLE)
                    Dim message As WindowMessages
                    If (style And UDS.UDS_HORZ) > 0 Then
                        message = WindowMessages.WM_HSCROLL
                    Else
                        message = WindowMessages.WM_VSCROLL
                    End If
                    SendMessage(parentWindow.Handle, message, (newValue << 16) + SB.SB_THUMBPOSITION, w.Handle)
                    SendMessage(parentWindow.Handle, message, SB.SB_ENDSCROLL, w.Handle)
                End If

                Return Reply.Ok

            Case w.ClassName.Contains("SCROLLBAR")
                Dim newValue As Integer = CInt(value)
                Dim min, max As Integer
                If GetScrollRange(w.Hwnd, ScrollBarDirection.SB_CTL, min, max) Then
                    ConfineValueToRange(newValue, min, max)
                    SetScrollPos(w.Hwnd, ScrollBarDirection.SB_CTL, newValue, True)
                    Return Reply.Ok
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.GetScrollRangeFailed0, GetLastWin32Error()))
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SetNumericValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the minimum numeric value in a Window.")>
    <Response("""RESULT:<value>"" where <value> is the numeric value in the window")>
    Private Function ProcessCommandGetMinNumericValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Select Case True
            Case w.ClassName.Contains("msctls_trackbar32"), w.ClassName.Contains("TTrackBar")
                Dim result = SendMessage(w.Handle, WindowMessages.TBM_GETRANGEMIN, 0, 0)
                Return Reply.Result(result.ToInt32())
            Case w.ClassName.Contains("msctls_updown32"), w.ClassName.Contains("TUpDown")
                Dim result = SendMessage(w.Handle, WindowMessages.UDM_GETRANGE, 0, 0).ToInt32()
                GetLowWord(result)
                Dim minValue As Integer = GetHighWord(result)
                Return Reply.Result(minValue)
            Case w.ClassName.Contains("SCROLLBAR")
                Dim min, max As Integer
                If GetScrollRange(w.Hwnd, ScrollBarDirection.SB_CTL, min, max) Then
                    Return Reply.Result(min)
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.GetScrollRangeFailed0, GetLastWin32Error()))
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetMinimumNumericValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the maximum numeric value in a Window.")>
    <Response("""RESULT:<value>"" where <value> is the numeric value in the window")>
    Private Function ProcessCommandGetMaxNumericValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Select Case True
            Case w.ClassName.Contains("msctls_trackbar32"), w.ClassName.Contains("TTrackBar")
                Dim result As Integer = SendMessage(w.Handle, WindowMessages.TBM_GETRANGEMAX, 0, 0).ToInt32()
                Return Reply.Result(result)
            Case w.ClassName.Contains("msctls_updown32"), w.ClassName.Contains("TUpDown")
                Dim result As Integer = SendMessage(w.Handle, WindowMessages.UDM_GETRANGE, 0, 0).ToInt32()
                Dim maxValue As Integer = GetLowWord(result)
                GetHighWord(result)
                Return Reply.Result(maxValue)
            Case w.ClassName.Contains("SCROLLBAR")
                Dim Min, Max As Integer
                If GetScrollRange(w.Hwnd, ScrollBarDirection.SB_CTL, Min, Max) Then
                    Return Reply.Result(Max)
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.GetScrollRangeFailed0, GetLastWin32Error()))
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetMaximumNumericValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Scrolls a Window by a specified amount.")>
    <Parameters("The numeric value specified by 'NumericValue' and those required to uniquely identify the window.")>
    Private Function ProcessCommandScrollByAmount(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        'This is a double value to allow values such as 0.5. It can be positive or negative
        Dim numPagesToScroll As Double
        Dim temp As String = objquery.GetParameter(ParameterNames.NumericValue)
        If Not Double.TryParse(temp, numPagesToScroll) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToParseValue0AsNumericValueDoublePrecision, temp))
        End If

        Select Case True
            Case w.ClassName.Contains("SCROLLBAR")
                Dim sErr As String = Nothing
                Dim si As SCROLLINFO
                si.cbSize = Marshal.SizeOf(GetType(SCROLLINFO))
                si.fMask = ScrollInfoMask.SIF_ALL
                If GetScrollInfo(w.Handle, si, sErr) Then
                    Dim amountToScroll As Integer = CInt(numPagesToScroll * si.nPage)
                    Dim currentScrollValue As Integer = GetScrollPos(w.Hwnd, ScrollBarDirection.SB_CTL)
                    Dim newValue As Integer = (currentScrollValue + amountToScroll)
                    Dim queryString As String = objquery.QueryString & " numericvalue=" & newValue.ToString
                    Return ProcessCommandSetNumericValue(clsQuery.Parse(queryString))
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetPageSize0, sErr))
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetMaximumNumericValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    Private Function GetScrollInfo(ByVal ScrollBarHandle As IntPtr, ByRef SI As SCROLLINFO, ByRef sErr As String) As Boolean
        Dim pRemoteStruct As IntPtr
        Try
            'Copy structure to remote memory
            Dim structSize As Integer = Marshal.SizeOf(GetType(SCROLLINFO))
            SI.cbSize = structSize
            Dim pLocalStruct As IntPtr = Marshal.AllocHGlobal(structSize)
            Marshal.StructureToPtr(SI, pLocalStruct, False)
            pRemoteStruct = VirtualAllocEx(ProcessHandle, IntPtr.Zero, structSize, MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)
            WriteProcessMemory(ProcessHandle, pRemoteStruct, pLocalStruct, structSize, IntPtr.Zero)
            Marshal.FreeHGlobal(pLocalStruct)

            'send the message 
            Dim retVal As Integer = modWin32.GetScrollInfo(ScrollBarHandle, ScrollBarDirection.SB_CTL, pRemoteStruct)
            If retVal = 0 Then
                sErr = "Call to send message failed. " & GetLastWin32Error()
                Return False
            End If

            'Get the output structure from remote memory
            Dim pNewLocalStruct As IntPtr = Marshal.AllocHGlobal(structSize)
            ReadProcessMemory(ProcessHandle, pRemoteStruct, pNewLocalStruct, structSize, IntPtr.Zero)
            Dim newLocalStruct As SCROLLINFO = CType(Marshal.PtrToStructure(pNewLocalStruct, GetType(SCROLLINFO)), SCROLLINFO)
            Marshal.FreeHGlobal(pNewLocalStruct)

            SI = newLocalStruct
            Return True
        Finally
            'Cleanup
            VirtualFreeEx(ProcessHandle, pRemoteStruct, 0, MEM_RELEASE)
        End Try
    End Function

    <Category(Category.Win32)>
    <Command("Gets a date and time value from a DateTimePicker or Calendar Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<datetime>"" where <datetime> is the date/time as displayed in the control, formatted to ISO standard.")>
    Private Function ProcessCommandGetDateTimeValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        'DECIDE WHAT MESSAGE TO SEND - THEY USUALLY HAVE THE SAME FORMAT
        Dim messageToSend As WindowMessages
        Dim expectedValue As Integer
        Select Case True
            Case w.ClassName.Contains("SysDateTimePick32")
                messageToSend = WindowMessages.DTM_GETSYSTEMTIME
                expectedValue = GDT_VALID
            Case w.ClassName.Contains("SysMonthCal32")
                'Which message we send depends on whether the MCS_MULTISELECT style is set.
                'Note that (from looking in reflector) all .NET apps set this style
                Dim windowStyle As Integer = GetWindowLong(w.Hwnd, GWL.GWL_STYLE)
                If (windowStyle And MCS.MCS_MULTISELECT) > 0 Then
                    Return Me.ProcessCommandGetMinSelectedDateTimeValue(objquery)
                Else
                    messageToSend = WindowMessages.MCM_GETCURSEL
                    expectedValue = 1
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetDateTimeValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select

        'Now do the sending ...
        Dim rpms As New RemoteProcessMessageSender(Of SYSTEMTIME, SYSTEMTIME)
        rpms.Handle = w.Handle
        rpms.MessageToSend = messageToSend
        rpms.wParam = 0
        rpms.InputValues = Nothing
        Dim Outputs(0) As SYSTEMTIME
        rpms.OutputValues = Outputs
        Dim retVal As Integer = rpms.SendMessage(ProcessHandle).ToInt32()
        If rpms.Success Then
            If retVal = expectedValue Then
                Return Reply.Result(rpms.OutputValues(0).ToDateTime().ToString("yyyy-MM-dd HH:mm:ss"))
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.SendmessageIndicatesAnError0, GetLastWin32Error()))
            End If
        Else
            Throw New InvalidOperationException(rpms.ErrorMessage)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Sets a date and time value in a DTPicker COM object.")>
    <Parameters("A parseable date/time specified by 'newtext' and those required to uniquely identify the window.")>
    Private Function ProcessCommandSetDTPickerDateTime(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.MustBeHookedToInteractWithDTPickerActiveXControl)

        Dim tmp As String = objquery.GetParameter(ParameterNames.NewText)
        Dim val As DateTime
        If Not DateTime.TryParse(tmp, val) Then
            Throw New InvalidOperationException(String.Format(My.Resources.UnableToParseString0AsAValidDateTime, tmp))
        End If

        Dim cmd As String = "property_set " & w.Handle.ToString("X") & ",Value,~D" & val.ToString("yyyy-MM-dd HH:mm:ss")
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Get a date and time value from a DTPicker COM object.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandGetDTPickerDateTime(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.MustBeHookedToInteractWithDTPickerActiveXControl)

        Dim cmd As String = "property_get " & w.Handle.ToString("X") & ",Value"
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Return Reply.Result(sResult)
    End Function

    <Category(Category.Win32)>
    <Command("Sets a date and time value from a DateTimePicker or Calendar Window.")>
    <Parameters("The ISO formatted date/time specified by 'newtext' and those required to uniquely identify the window.")>
    Private Function ProcessCommandSetDateTimeValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Dim s As String = objquery.GetParameter(ParameterNames.NewText)
        Dim value As DateTime
        If Not DateTime.TryParse(s, value) Then
            Throw New InvalidOperationException(String.Format(My.Resources.UnableToParseString0AsAValidDateTime, s))
        End If

        Dim systime As SYSTEMTIME = SYSTEMTIME.FromDateTime(value)

        'DECIDE WHAT MESSAGE TO SEND - THEY USUALLY HAVE THE SAME FORMAT
        Dim msg As WindowMessages
        Dim flags As Integer
        Select Case True

            Case w.ClassName.Contains("SysDateTimePick32")
                msg = WindowMessages.DTM_SETSYSTEMTIME
                Const GDT_VALID As Integer = 0
                flags = GDT_VALID

            Case w.ClassName.Contains("SysMonthCal32")
                'Which message we send depends on whether the MCS_MULTISELECT style is set.
                'Note that (from looking in reflector) all .NET apps set this style
                Dim style As Integer = GetWindowLong(w.Hwnd, GWL.GWL_STYLE)
                If (style And MCS.MCS_MULTISELECT) > 0 Then
                    Dim retval As Integer = SetRange(w.Handle, WindowMessages.MCM_SETSELRANGE, New SYSTEMTIME() {systime, systime}, GDTR.GDTR_MIN Or GDTR.GDTR_MAX)
                    If retval > 0 Then
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(String.Format(My.Resources.SendmessageCallWasNotSuccessful0, GetLastWin32Error()))
                    End If
                Else
                    msg = WindowMessages.MCM_SETCURSEL
                    flags = 0
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SetDateTimeValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select

        Dim sender As New RemoteProcessMessageSender(Of SYSTEMTIME, SYSTEMTIME)
        sender.Handle = w.Handle
        sender.MessageToSend = msg
        sender.wParam = flags
        sender.InputValues = New SYSTEMTIME() {systime}
        sender.OutputValues = Nothing

        Dim result = sender.SendMessage(ProcessHandle)
        If result.ToInt32() = 0 Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToSetDatetimeValue0, GetLastWin32Error()))
        End If

        Dim parentWindow As clsUIWindow = CType(w.Parent, clsUIWindow)
        Dim childID As Int32 = GetWindowLong(w.Hwnd, GWL.GWL_ID) And &HFFFF
        Dim rpms As New RemoteProcessMessageSender(Of NMDATETIMECHANGE, NMDATETIMECHANGE)
        rpms.Handle = parentWindow.Handle
        rpms.MessageToSend = WindowMessages.WM_NOTIFY
        rpms.wParam = childID
        rpms.InputValues = New NMDATETIMECHANGE() {New NMDATETIMECHANGE(DTN.DTN_DATETIMECHANGE, childID, w.Handle, flags, systime)}
        rpms.OutputValues = Nothing
        rpms.SendMessage(ProcessHandle)

        'For .NET apps, simulate the reflected message from the parent.
        'See TabControl.WndProc() and TabControl.WmSelChange in reflector
        rpms.Handle = w.Handle
        rpms.MessageToSend = WindowMessages.WM_NOTIFY Or WindowMessages.WM_REFLECT
        rpms.SendMessage(ProcessHandle)

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Gets the  minimum selected datetime value from a DateTimePicker or Calendar Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<datetime>"" where <datetime> is the UTC time formatted to ISO standard.")>
    Private Function ProcessCommandGetMinSelectedDateTimeValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Select Case True
            Case w.ClassName.Contains("SysMonthCal32")
                'Which message we send depends on whether the MCS_MULTISELECT style is set.
                'Note that (from looking in reflector) all .NET apps set this style
                Dim windowStyle As Integer = GetWindowLong(w.Hwnd, GWL.GWL_STYLE)
                If (windowStyle And MCS.MCS_MULTISELECT) > 0 Then
                    Return GetDateFromRange(w.Handle, WindowMessages.MCM_GETSELRANGE, False)
                Else
                    Return Me.ProcessCommandGetDateTimeValue(objquery)
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetDateTimeValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the  maximum selected datetime value from a DateTimePicker or Calendar Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<datetime>"" where <datetime> is the UTC time formatted to ISO standard.")>
    Private Function ProcessCommandGetMaxSelectedDateTimeValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Select Case True
            Case w.ClassName.Contains("SysMonthCal32")
                'Which message we send depends on whether the MCS_MULTISELECT style is set.
                'Note that (from looking in reflector) all .NET apps set this style
                Dim windowStyle As Integer = GetWindowLong(w.Hwnd, GWL.GWL_STYLE)
                If (windowStyle And MCS.MCS_MULTISELECT) > 0 Then
                    Return GetDateFromRange(w.Handle, WindowMessages.MCM_GETSELRANGE, True)
                Else
                    Return Me.ProcessCommandGetDateTimeValue(objquery)
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetDateTimeValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the maximum datetime value from a DateTimePicker or Calendar Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<datetime>"" where <datetime> is the date/time from the control, formatted to ISO standard.")>
    Private Function ProcessCommandGetMaxDateTimeValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Dim messageToSend As WindowMessages
        Select Case True
            Case w.ClassName.Contains("SysDateTimePick32")
                messageToSend = WindowMessages.DTM_GETRANGE
            Case w.ClassName.Contains("SysMonthCal32")
                messageToSend = WindowMessages.MCM_GETRANGE
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetMaxDateTimeValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select

        Dim range As SYSTEMTIME()
        Dim validElements As Integer
        Try
            range = Me.GetDateRange(w.Handle, messageToSend, validElements)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnexpectedErrorWhilstRetrievingDatetimeValueFromRemoteProcess0, ex.Message))
        End Try
        If range IsNot Nothing Then
            If range.Length = 2 Then
                If (validElements And GDTR.GDTR_MAX) > 0 Then
                    Return Reply.Result(range(1).ToDateTime.ToString("yyyy-MM-dd HH:mm:ss"))
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotReadInformationFromDatePicker)
                End If
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.CouldNotReadInformationFromDatePickerUnexpectedLengthOfArrayFromGetDatePickerRa, range.Length.ToString))
            End If
        Else
            Throw New InvalidOperationException(My.Resources.CouldNotReadInformationFromDatePickerFunctionGetDatePickerRangeDidNotReturnAnyD)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Gets the  minimum datetime value from a DateTimePicker or Calendar Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<datetime>"" where <datetime> is the date/time as displayed in the control, formatted to ISO standard.")>
    Private Function ProcessCommandGetMinDateTimeValue(ByVal objquery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objquery)

        Dim messageToSend As WindowMessages
        Select Case True
            Case w.ClassName.Contains("SysDateTimePick32")
                messageToSend = WindowMessages.DTM_GETRANGE
            Case w.ClassName.Contains("SysMonthCal32")
                messageToSend = WindowMessages.MCM_GETRANGE
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetMinDateTimeValueIsNotImplementedForControlsWithClassName0, w.ClassName))
        End Select

        Dim range As SYSTEMTIME()
        Dim validElements As Integer
        Try
            range = Me.GetDateRange(w.Handle, messageToSend, validElements)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnexpectedErrorWhilstRetrievingDatetimeValueFromRemoteProcess0, ex.Message))
        End Try
        If range IsNot Nothing Then
            If range.Length = 2 Then
                If (validElements And GDTR.GDTR_MIN) > 0 Then
                    Return Reply.Result(range(0).ToDateTime().ToString("yyyy-MM-dd HH:mm:ss"))
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotReadInformationFromDatePicker)
                End If
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.CouldNotReadInformationFromDatePickerUnexpectedLengthOfArrayFromGetDatePickerRa, range.Length.ToString))
            End If
        Else
            Throw New InvalidOperationException(My.Resources.CouldNotReadInformationFromDatePickerFunctionGetDatePickerRangeDidNotReturnAnyD)
        End If
    End Function

    ''' <summary>
    ''' Sets a date range by sending the supplied message to the
    ''' target window.
    ''' </summary>
    ''' <param name="Values">A 2-element array containing the
    ''' minimum value followed by the maxmimum value.</param>
    ''' <param name="Flags">Flags indicating which of min,
    ''' max (or both) are to be set from the supplied array.</param>
    ''' <returns>Returns the return value from the SendMessage
    ''' call.</returns>
    ''' <remarks>This method is safe for cross-process message
    ''' calls.</remarks>
    Private Function SetRange(ByVal TargetWindowHandle As IntPtr, ByVal MessageToSend As WindowMessages, ByVal Values As SYSTEMTIME(), ByVal Flags As GDTR) As Integer
        Dim rpms As New RemoteProcessMessageSender(Of SYSTEMTIME, SYSTEMTIME)
        rpms.Handle = TargetWindowHandle
        rpms.MessageToSend = MessageToSend
        rpms.wParam = Flags
        rpms.InputValues = Values
        rpms.OutputValues = Nothing
        Return rpms.SendMessage(ProcessHandle).ToInt32()
    End Function

    ''' <summary>
    ''' Gets the range of allowable values in a datetime picker
    ''' control.
    ''' </summary>
    ''' <param name="DatePickerHandle">Handle to the date time
    ''' picker.</param>
    ''' <param name="ValidElements">Carries back the return
    ''' value of SendMessage in the format as explained at
    ''' http://msdn2.microsoft.com/en-us/library/ms672156.aspx
    ''' The caller should check this value against GDTR_MIN and GDTR_MAX
    ''' to discover which elements of the array are valid.</param>
    ''' <param name="MessageToSend">The message to send. This will
    ''' vary slightly according to the window class (eg SysDatePicker32
    ''' or SysMonthCal32), but the format of the messages and
    ''' return values always seems to be the same.</param>
    ''' <returns>Returns a 2-element array of SYSTEMTIME
    ''' structures. The first is the minimum allowable value;
    ''' the second is the maximum.</returns>
    Private Function GetDateRange(ByVal DatePickerHandle As IntPtr, ByVal MessageToSend As WindowMessages, ByRef ValidElements As Integer) As SYSTEMTIME()
        Dim rpms As New RemoteProcessMessageSender(Of SYSTEMTIME, SYSTEMTIME)
        rpms.Handle = DatePickerHandle
        rpms.MessageToSend = MessageToSend
        rpms.InputValues = Nothing
        Dim outputs(1) As SYSTEMTIME
        rpms.OutputValues = outputs
        rpms.wParam = 0
        ValidElements = rpms.SendMessage(ProcessHandle).ToInt32()
        If rpms.Success Then
            Return rpms.OutputValues
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Gets one of the maximum or minimum in a date range.
    ''' </summary>
    ''' <param name="DatePickerHandle">The handle of the compatible window.</param>
    ''' <param name="MessageToSend">The message to be sent to the window.</param>
    ''' <param name="GetMax">True if the maximum in the range is
    ''' desired, false if the minimum is desired.</param>
    ''' <returns>Returns a string of the form RESULT: followed
    ''' by the date as a string.</returns>
    Private Function GetDateFromRange(ByVal DatePickerHandle As IntPtr, ByVal MessageToSend As WindowMessages, ByVal GetMax As Boolean) As Reply
        Dim validElements As Integer
        Dim range As SYSTEMTIME()
        Try
            range = Me.GetDateRange(DatePickerHandle, MessageToSend, validElements)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.UnexpectedErrorWhilstRetrievingDatetimeValueFromRemoteProcess0, ex.Message))
        End Try
        If range IsNot Nothing Then
            Dim index As Integer
            Dim expectedFlag As Integer
            If GetMax Then
                index = 1
                expectedFlag = GDTR.GDTR_MAX
            Else
                index = 0
                expectedFlag = GDTR.GDTR_MIN
            End If
            If range.Length = 2 Then
                If (validElements And expectedFlag) > 0 Then
                    Return Reply.Result(range(index).ToDateTime().ToString("yyyy-MM-dd HH:mm:ss"))
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotReadInformationFromDatePicker)
                End If
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.CouldNotReadInformationFromDatePickerUnexpectedLengthOfArrayFromGetDatePickerRa, range.Length.ToString))
            End If
        Else
            Throw New InvalidOperationException(My.Resources.CouldNotReadInformationFromDatePickerFunctionGetDatePickerRangeDidNotReturnAnyD)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Gets the number of items in a listbox, listview, tab control, toolbar etc.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("The number of items.")>
    Private Function ProcessCommandGetItemCount(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(GetItemCount(w))
    End Function

    ''' <summary>
    ''' Counts the number of items contained in the specified windows control.
    ''' Relevant for listboxes, comboboxes, tab controls, toolbars, etc.
    ''' </summary>
    ''' <param name="w">The clsUIWindow representing the control of interest</param>
    ''' <returns>Returns the number of items. Throws an exception in the event of an
    ''' error.</returns>
    Private Function GetItemCount(ByVal w As clsUIWindow) As Integer
        Dim windowMessage As Integer
        Dim itemCount As Integer = -1

        Dim className = w.ClassName.ToLower(CultureInfo.InvariantCulture)
        Select Case True
            Case w.ClassName = "ListViewWndClass" Or w.ClassName = "ListView20WndClass"
                If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractWithAnActiveXListViewUnlessHooked)
                Dim cmd As String = "listview_count " & w.Handle.ToString("X")
                Dim sResult As String = Nothing
                If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New ApiException(sResult)
                Return Integer.Parse(sResult)
            Case w.ClassName = "TreeView20WndClass"
                If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractWithAnActiveXTreeViewUnlessHooked)
                Dim cmd As String = "treeview_count " & w.Handle.ToString("X")
                Dim sResult As String = Nothing
                If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New ApiException(sResult)
                Return Integer.Parse(sResult)
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                windowMessage = WindowMessages.LVM_GETITEMCOUNT
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                windowMessage = WindowMessages.TVM_GETCOUNT
            Case className.Contains("listbox"), className.Contains("combolbox")
                windowMessage = WindowMessages.LB_GETCOUNT
            Case w.ClassName.Contains("COMBOBOX"), w.ClassName.Contains("ComboBox")
                windowMessage = WindowMessages.CB_GETCOUNT
            Case w.ClassName.Contains("SysTabControl32"), w.ClassName.Contains("TTabControl"), w.ClassName.Contains("PBTabControl32"), w.ClassName = "SSTabCtlWndClass"
                itemCount = GetTabControlItemCount(w)
            Case w.ClassName.Contains("ToolbarWindow32") OrElse w.ClassName.Contains("msvb_lib_toolbar")
                windowMessage = WindowMessages.TB_BUTTONCOUNT
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetItemCountIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select

        If itemCount = -1 Then itemCount = SendMessage(w.Handle, windowMessage, 0, 0).ToInt32()
        If itemCount = LB.LB_ERR Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetItemCount0, GetLastWin32Error()))
        Else
            Return itemCount
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Gets the number of selected items in the Window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<count>"" where <count> is the number of selected items.")>
    Private Function ProcessCommandGetSelectedItemCount(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim count As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETSELECTEDCOUNT, 0, 0).ToInt32()
                Return Reply.Result(count)
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("TTreeView"), w.ClassName.Contains("PBTreeView32")
                Dim nodeHandle = SendMessage(w.Handle, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_CARET, 0)
                If nodeHandle <> IntPtr.Zero Then
                    Return Reply.Result(1)
                Else
                    Return Reply.Result(0)
                End If
            Case w.ClassName.Contains("ListBox") OrElse w.ClassName.Contains("LISTBOX")
                Dim Result As Integer
                If IsListboxMultiSelection(w.Handle) Then
                    Result = SendMessage(w.Handle, WindowMessages.LB_GETSELCOUNT, 0, 0).ToInt32()
                    If Result <> LB.LB_ERR Then
                        Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveSelectedItemCountFromListBox0, GetLastWin32Error()))
                    End If
                Else
                    Result = SendMessage(w.Handle, WindowMessages.LB_GETCURSEL, 0, 0).ToInt32()
                    If Result = LB.LB_ERR Then
                        Result = 0
                    Else
                        Result = 1
                    End If
                End If
                Return Reply.Result(Result)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetSelectedItemCountIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the text from the selected item.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<value>"" where <value> is the text of the selected item.")>
    Private Function ProcessCommandGetSelectedItemText(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Select Case True
            Case w.ClassName = "ListViewWndClass" Or w.ClassName = "ListView20WndClass"
                If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractiveWithAnActiveXListViewUnlessHooked)
                Dim cmd As String = "listview_getselecteditemtext " & w.Handle.ToString("X")
                Dim sResult As String = Nothing
                If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New ApiException(sResult)
                Return Reply.Result(sResult)

            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim itemHandle = SendMessage(w.Handle, WindowMessages.LVM_GETNEXTITEM, -1, ListviewNextItemFlags.LVNI_SELECTED)
                Dim selectedCount As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETSELECTEDCOUNT, 0, 0).ToInt32()
                If itemHandle.ToInt64() >= 0 And selectedCount <> 0 Then
                    'Iterate through the rows looking for the first selected item
                    Dim itemCount As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETITEMCOUNT, 0, 0).ToInt32()
                    For rowIndex As Integer = 0 To itemCount - 1
                        Dim itemState As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETITEMSTATE, rowIndex, LVIS.LVIS_SELECTED).ToInt32()
                        Dim itemSelected As Boolean = (itemState And LVIS.LVIS_SELECTED) > 0

                        If itemSelected Then
                            Return Reply.Result(Me.GetListviewItemText(w.Handle, rowIndex, 0))
                        End If
                    Next
                    Throw New InvalidOperationException(My.Resources.CouldNotFindAnySelectedItems)
                End If

            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                Dim nodeHandle = SendMessage(w.Handle, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_CARET, 0)
                If nodeHandle.ToInt64() >= 0 Then
                    Dim node As TreeviewItem = Me.GetTreenodeItem(w.Handle, nodeHandle)
                    If node IsNot Nothing Then
                        Return Reply.Result(node.Text)
                    End If
                    Throw New InvalidOperationException(My.Resources.FailedToResolveHandleAsTreenodeItem)
                End If

            Case w.ClassName.Contains("COMBOBOX"), w.ClassName.Contains("ComboBox")
                Dim selectedItemIndex = SendMessage(w.Handle, WindowMessages.CB_GETCURSEL, 0, 0).ToInt32()
                If selectedItemIndex = modWin32.CB.CB_ERR Then
                    If Not String.IsNullOrEmpty(w.WindowText) Then
                        Return Reply.Result(w.WindowText)
                    Else
                        Throw New InvalidOperationException(My.Resources.NoItemIsSelectedInTheComboBox)
                    End If
                End If

                Dim itemText As String = String.Empty
                Try
                    itemText = GetItemText(w.Handle, WindowMessages.CB_GETLBTEXTLEN, WindowMessages.CB_GETLBTEXT, selectedItemIndex)
                    Return Reply.Result(itemText)
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstRetrievingText0, ex.Message))
                End Try

            Case w.ClassName.Contains("LISTBOX") OrElse w.ClassName.Contains("ListBox")
                'Get the index of the first selected item. How we obtain this depends on 
                'whether the listbox allows multiple selections or not
                Dim currentSelection As Integer
                If Me.IsListboxMultiSelection(w.Handle) Then
                    Dim rpms As New RemoteProcessMessageSender(Of Integer, Integer)
                    rpms.Handle = w.Handle
                    rpms.MessageToSend = WindowMessages.LB_GETSELITEMS
                    rpms.InputValues = Nothing
                    Dim outputs As Integer()
                    ReDim outputs(0)
                    rpms.OutputValues = outputs
                    rpms.wParam = 1

                    If rpms.SendMessage(ProcessHandle).ToInt32() <> LB.LB_ERR Then
                        currentSelection = outputs(0)
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToGetSelectionInfoOfListbox)
                    End If

                Else
                    currentSelection = SendMessage(w.Handle, WindowMessages.LB_GETCURSEL, 0, 0).ToInt32()
                End If

                If currentSelection = -1 Then
                    Return Reply.Result("")
                Else
                    Dim itemText As String

                    itemText = GetItemText(w.Handle, WindowMessages.LB_GETTEXTLEN, WindowMessages.LB_GETTEXT, currentSelection)
                    Return Reply.Result(itemText)

                End If

            Case w.ClassName.Contains("TabControl")
                Dim tabIndex = SendMessage(w.Handle, WindowMessages.TCM_GETCURSEL, 0, 0).ToInt32()
                If tabIndex > -1 Then
                    Return Reply.Result(Me.GetTabControlItemText(w, tabIndex))
                Else
                    Throw New InvalidOperationException(My.Resources.NoTabIsSelected)
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetSelectedItemTextIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select

        Throw New InvalidOperationException(My.Resources.CannotReturnTextOfSelectedItemBecauseNoItemIsSelected)
    End Function


    Private Function GetItemText(ByVal Handle As IntPtr, ByVal GetLengthMessage As WindowMessages, ByVal GetTextMessage As WindowMessages, ByVal ItemIndex As Integer) As String
        Dim itemText As String = String.Empty
        Dim pRemoteBuffer As IntPtr
        Try
            Dim textLength As Integer = SendMessage(Handle, GetLengthMessage, ItemIndex, 0).ToInt32()
            If textLength = 0 Then
                Return String.Empty
            Else
                Select Case GetTextMessage
                    'As described at http://www.microsoft.com/msj/0997/win320997.aspx
                    'some of the older controls have messages marshaled automatically.
                    'Indeed, doing it the 'proper' way seems to fail for some reason.
                    Case WindowMessages.LB_GETTEXT, WindowMessages.CB_GETLBTEXT
                        Dim pLocalBuffer As IntPtr = Marshal.AllocHGlobal(textLength + 1)
                        Dim result As Integer = modWin32.SendMessage(Handle, GetTextMessage, ItemIndex, pLocalBuffer).ToInt32()
                        If result = CB.CB_ERR Then
                            Throw New InvalidOperationException(My.Resources.APICallFailed_2 & GetLastWin32Error())
                        End If

                        itemText = Marshal.PtrToStringAnsi(pLocalBuffer)
                        Marshal.FreeHGlobal(pLocalBuffer)

                        Return itemText
                    Case Else
                        'Allocate some remote memory. This method also blanks the requested memory
                        pRemoteBuffer = VirtualAllocEx(ProcessHandle, IntPtr.Zero, textLength + 1, MEM_COMMIT, PAGE_READWRITE)

                        'Call for the text to be written to the remote buffer
                        Dim result As Integer = modWin32.SendMessage(Handle, GetTextMessage, ItemIndex, pRemoteBuffer).ToInt32()
                        If result = CB.CB_ERR Then
                            Throw New InvalidOperationException(String.Format(My.Resources.APICallFailed0, GetLastWin32Error()))
                        End If

                        'Get text, storing in remote buffer
                        result = modWin32.SendMessage(Handle, GetTextMessage, ItemIndex, pRemoteBuffer).ToInt32()
                        If result = CB.CB_ERR Then
                            Throw New InvalidOperationException(String.Format(My.Resources.APICallFailed0, GetLastWin32Error()))
                        End If

                        'Copy back the remote text 
                        Dim pLocalBuffer As IntPtr = Marshal.AllocHGlobal(textLength + 1)
                        ReadProcessMemory(ProcessHandle, pRemoteBuffer, pLocalBuffer, textLength + 1, IntPtr.Zero)
                        itemText = Marshal.PtrToStringAnsi(pLocalBuffer)
                        Marshal.FreeHGlobal(pLocalBuffer)

                        Return itemText
                End Select
            End If
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(
             My.Resources.ExceptionWhilstRetrievingTextForItemWithIndex01, ItemIndex, ex), ex)
        Finally
            'deallocate the memory and close the process handle
            If pRemoteBuffer <> IntPtr.Zero Then
                VirtualFreeEx(ProcessHandle, pRemoteBuffer, 0, MEM_RELEASE)
            End If
        End Try
    End Function

    <Category(Category.Win32)>
    <Command("Gets the ImageIndex from the specified item of a Windows Listview.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<value>"" where <value> is the zero-based ImageIndex of the specified item.")>
    Private Function ProcessCommandGetItemImageIndex(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the (optional) fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing AndAlso fallBackIndex < 0 Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim itemIndex As Integer
                itemIndex = Me.GetListviewItemIndex(searchText, fallBackIndex, w.Handle)

                If itemIndex = -1 Then
                    Throw New InvalidOperationException(String.Format(My.Resources.NoItemWithTheText0Found, searchText))
                End If

                Dim lvItem As New LV_ITEM
                lvItem.iItem = itemIndex
                lvItem.mask = LVIF.LVIF_IMAGE
                Dim rpms As New RemoteProcessMessageSender(Of LV_ITEM, LV_ITEM)
                rpms.MessageToSend = WindowMessages.LVM_GETITEMA
                rpms.Handle = w.Handle
                rpms.wParam = 0
                Dim Outputs As LV_ITEM()
                ReDim Outputs(0)
                rpms.OutputValues = Outputs
                rpms.InputValues = New LV_ITEM() {lvItem}

                If rpms.SendMessage(ProcessHandle).ToInt32() > 0 Then
                    Return Reply.Result(rpms.OutputValues(0).iImage)
                Else
                    Throw New InvalidOperationException(String.Format("Unexpected result from {0}. {1}", rpms.MessageToSend.ToString, GetLastWin32Error()))
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetSelectedItemTextIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets the number of listview items visible per 'page' when a listview is in details mode, and requires a scrollbar because there are too many items to fit on one page.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<pagesize>"" where <pagesize> is the number of listview items visible per page.")>
    Private Function ProcessCommandGetPageCapacity(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim windowMessage As Integer
        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                windowMessage = WindowMessages.LVM_GETCOUNTPERPAGE
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                windowMessage = WindowMessages.TVM_GETVISIBLECOUNT
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetPageCapacityIsNotAvailableForWindowsWithClassname0, w.ClassName))
        End Select

        Dim pageSize As Integer = SendMessage(w.Handle, windowMessage, 0, 0).ToInt32()
        Return Reply.Result(pageSize)
    End Function

    <Category(Category.Win32)>
    <Command("Sets an item in a listview or treeview to checked.")>
    <Parameters("The index of the item specified by 'Position' or the item text to search for specified by 'NewText' as well as the value of 'True' or 'False' specified by 'Value' additonally the parameters required to uniquely identify the window.")>
    Private Function ProcessCommandSetItemChecked(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        'Get value specifying whether we should set to checked or unchecked
        Dim newCheckedString As String = objQuery.GetParameter(ParameterNames.Value)
        Dim newCheckedValue As Boolean
        If Not Boolean.TryParse(newCheckedString, newCheckedValue) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsAFlag, newCheckedString))
        End If

        Select Case True
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")

                Dim foundNodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
                If foundNodeHandle.ToInt64() >= 0 Then

                    Const bitShift As Integer = 12
                    Const checkedState As Integer = 2

                    Dim currentState = GetTreeviewItemState(w.Handle, foundNodeHandle, TVITEMStates.TVIS_STATEIMAGEMASK, bitShift) = checkedState
                    If currentState = newCheckedValue Then Return Reply.Ok

                    Dim item As New TVITEM With
                        {
                            .hItem = foundNodeHandle,
                            .mask = TreeviewGetItemFlags.TVIF_STATE Or TreeviewGetItemFlags.TVIF_HANDLE,
                            .stateMask = TVITEMStates.TVIS_STATEIMAGEMASK
                        }
                    ' We deliberately set the checkstate to the opposite of what
                    ' we want so that the keydown message will check it again.
                    item.state = If(Not newCheckedValue, 2, 1) << 12

                    Dim rpms As New RemoteProcessMessageSender(Of TVITEM, TVITEM) With
                        {
                            .MessageToSend = WindowMessages.TVM_SETITEMA,
                            .Handle = w.Handle,
                            .wParam = 0,
                            .OutputValues = Nothing,
                            .InputValues = New TVITEM() {item}
                        }

                    rpms.SendMessage(ProcessHandle)

                    If rpms.ReturnValue.ToInt32() > 0 Then
                        ' Send a keydown message to check the checkbox and raise the checked event.
                        SendMessage(w.Handle, WindowMessages.TVM_SELECTITEM, TreeviewSelectItemFlags.TVGN_CARET, foundNodeHandle)
                        SendMessage(w.Handle, WindowMessages.WM_KEYDOWN, VirtualKeyCode.VK_SPACE, 0)
                        SendMessage(w.Handle, WindowMessages.WM_KEYUP, VirtualKeyCode.VK_SPACE, 0)

                        Dim newState = GetTreeviewItemState(w.Handle, foundNodeHandle, TVITEMStates.TVIS_STATEIMAGEMASK, bitShift) = checkedState
                        If newState = newCheckedValue Then Return Reply.Ok
                    End If

                    Throw New InvalidOperationException(My.Resources.FailedToSetTreeviewItemState)
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindTreeNodeMatchingTheSpecifiedCriteria)
                End If

            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                'The index of the item to have its checkstate changed
                Dim foundItemIndex As Integer = Me.GetListviewItemIndex(searchText, fallBackIndex, w.Handle)
                If foundItemIndex = -1 Then
                    Throw New InvalidOperationException(My.Resources.CouldNotFindSpecifiedListviewItem)
                End If

                'Reject listviews which do not have the correct style
                Dim exStyle As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0).ToInt32()
                If (exStyle And LVS_EX.LVS_EX_CHECKBOXES) = 0 Then
                    Throw New InvalidOperationException(My.Resources.CanNotSetItemCheckedOnAListviewThatDoesNotHaveTheLVS_EX_CHECKBOXESStyleSet)
                End If

                'Find out whether the item of interest is already checked
                Dim state As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETITEMSTATE, foundItemIndex, LVIS.LVIS_STATEIMAGEMASK).ToInt32()
                Dim alreadyChecked As Boolean = (state And 8192) > 0

                'Quit early if no work to do - ie if desired state is already in place
                If alreadyChecked = newCheckedValue Then
                    Return Reply.Ok
                End If

                'In order for a click to be taken in context, we must ensure
                'that the item of interest is visible
                Dim result As Integer = SendMessage(w.Handle, WindowMessages.LVM_ENSUREVISIBLE, foundItemIndex, 0).ToInt32()
                If result = 0 Then
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToEnsureThatTheTargetItemIsVisible0, GetLastWin32Error()))
                End If

                'Set the checked state
                Dim stateImageIndex = If(newCheckedValue, 1, 0)
                Dim stateIndex As Integer = ((stateImageIndex + 1) << 12) 'state index is 1-based

                Dim item As New LV_ITEM
                item.mask = LVIF.LVIF_STATE
                item.stateMask = LVIS.LVIS_STATEIMAGEMASK
                item.state = stateIndex
                Dim inputs() As LV_ITEM = {item}

                Dim msgSender As New RemoteProcessMessageSender(Of LV_ITEM, LV_ITEM)
                msgSender.Handle = w.Handle
                msgSender.InputValues = inputs
                msgSender.OutputValues = Nothing
                msgSender.MessageToSend = WindowMessages.LVM_SETITEMSTATE
                msgSender.wParam = foundItemIndex
                msgSender.SendMessage(ProcessHandle)

                Return Reply.Ok

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SetItemCheckedIsNotAvailableForControlsWithClassname0, w.ClassName))
        End Select

        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Gets the bounds of a listview item. The icon bounds, the label bounds and the whole
    ''' item bounds are some of the options available by modifying value supplied to the
    ''' <paramref name="boundsModifier">BoundsModifier</paramref> parameter.
    ''' </summary>
    ''' <param name="handle">The handle of the listview of interest.</param>
    ''' <param name="itemIndex">The zero-based index of the listview item
    ''' of interest, within the listview.</param>
    ''' <param name="boundsModifier">Specifies which part of the listview item is of
    ''' interest. The bounds of this part of the item will be retrieved.</param>
    ''' <returns>Returns a RECT structure, detailing the bounds requested in 
    ''' coordinates relative to the listview's top left.</returns>
    Private Function GetListviewItemBounds(ByVal handle As IntPtr, ByVal itemIndex As Integer, ByVal boundsModifier As LVIR) As RECT
        Dim rpmsBounds As New RemoteProcessMessageSender(Of RECT, RECT)()
        rpmsBounds.Handle = handle

        Dim labelRect As RECT
        labelRect.Left = boundsModifier
        rpmsBounds.MessageToSend = WindowMessages.LVM_GETITEMRECT
        rpmsBounds.wParam = itemIndex
        rpmsBounds.InputValues = New RECT() {labelRect}
        rpmsBounds.OutputValues = New RECT() {}
        ReDim rpmsBounds.OutputValues(0)

        Dim result As Integer = rpmsBounds.SendMessage(ProcessHandle).ToInt32()
        If result = 0 OrElse Not rpmsBounds.Success Then
            Throw New InvalidOperationException(My.Resources.FailedToRetrieveBoundsOfTargetItemSLabel)
        Else
            Return rpmsBounds.OutputValues(0)
        End If
    End Function


    <Category(Category.Win32)>
    <Command("Indicates whether a window is the active window - the window which receives user input.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandIsWindowActive(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow

        w = mobjModel.IdentifyWindow(objQuery)

        Dim hActiveWindow As IntPtr = GetForegroundWindow()
        If hActiveWindow = w.Handle Then
            Return Reply.Result(True)
        Else
            Return Reply.Result(False)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Indicates whether a window is the active window - the window which receives user input.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandCheckWindowActive(ByVal objQuery As clsQuery) As Reply
        Return ProcessCommandIsWindowActive(objQuery)
    End Function

    <Category(Category.Win32)>
    <Command("Gets whether an item in a listview or treeview is checked.")>
    <Parameters("The index of the item specified by 'Position' or the item text to search for specified by 'NewText' as well as those parameters required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandIsItemChecked(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                Dim foundNodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
                If foundNodeHandle.ToInt64() > 0 Then
                    Dim itemState As Integer = Me.GetTreeviewItemState(w.Handle, foundNodeHandle, TVITEMStates.TVIS_STATEIMAGEMASK, 12)
                    Dim itemChecked As Boolean = (itemState = 2)
                    Return Reply.Result(itemChecked)
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindTreeNodeMatchingTheSpecifiedCriteria)
                End If
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim foundItemIndex As Integer = Me.GetListviewItemIndex(searchText, fallBackIndex, w.Handle)
                Dim state As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETITEMSTATE, foundItemIndex, LVIS.LVIS_STATEIMAGEMASK).ToInt32()
                Dim alreadyChecked As Boolean = (state And 8192) > 0
                Return Reply.Result(alreadyChecked)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SelectItemIsNotAvailableForControlsWithClassname0, w.ClassName))
        End Select

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Gets whether an item in a ListBox TreeView or ListView is selected.")>
    <Parameters("The index of the item specified by 'Position' or the item text to search for specified by 'NewText' as well as those parameters required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandIsItemSelected(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("ListBox") OrElse w.ClassName.Contains("LISTBOX")
                Dim index As Integer = Me.GetItemIndex(w, searchText, fallBackIndex)
                Dim result = SendMessage(w.Handle, WindowMessages.LB_GETSEL, index, 0).ToInt64()
                Return Reply.Result((result > 0))

            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                Dim foundNodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
                If foundNodeHandle.ToInt64() > 0 Then

                    Dim itemState As Integer = Me.GetTreeviewItemState(w.Handle, foundNodeHandle, TVITEMStates.TVIS_SELECTED)
                    Dim itemSelected As Boolean = (itemState And TVITEMStates.TVIS_SELECTED) > 0
                    Return Reply.Result(itemSelected)

                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindTreeNodeMatchingTheSpecifiedCriteria)
                End If

            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                'Get the listview item index
                Dim listviewHandle = w.Handle
                Dim index As Integer

                index = Me.GetListviewItemIndex(searchText, fallBackIndex, listviewHandle)

                If index = -1 Then
                    Throw New InvalidOperationException(String.Format(My.Resources.NoItemWithTheText0Found, searchText))
                End If

                'Get the 'selected' state
                Dim result As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETITEMSTATE, index, LVIS.LVIS_SELECTED).ToInt32()
                Dim isSelected As Boolean = (result And LVIS.LVIS_SELECTED) > 0
                Return Reply.Result(isSelected)

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.IsItemSelectedIsNotAvailableOnControlsWithClassname0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets whether an item in a TreeView is expanded.")>
    <Parameters("The index of the item specified by 'Position' or the item text to search for specified by 'NewText' as well as those parameters required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandIsItemExpanded(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                Dim foundNodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
                If foundNodeHandle.ToInt64() > 0 Then
                    Dim itemState As Integer = Me.GetTreeviewItemState(w.Handle, foundNodeHandle, TVITEMStates.TVIS_EXPANDED)
                    Dim itemSelected As Boolean = (itemState And TVITEMStates.TVIS_EXPANDED) > 0
                    Return Reply.Result(itemSelected)
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindTreeNodeMatchingTheSpecifiedCriteria)
                End If
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.IsItemExpandedIsNotAvailableOnControlsWithClassname0, w.ClassName))
        End Select
    End Function

    <Category(Category.Win32)>
    <Command("Gets whether an item in a TreeView or ListView is focused.")>
    <Parameters("The index of the item specified by 'Position' or the item text to search for specified by 'NewText' as well as those parameters required to uniquely identify the window.")>
    <Response("""RESULT:True"" or ""RESULT:False""")>
    Private Function ProcessCommandIsItemFocused(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("SysTreeView32"), w.ClassName.Contains("PBTreeView32")
                Dim foundNodeHandle = Me.GetTreenodeHandle(w.Handle, searchText)
                If foundNodeHandle.ToInt64() > 0 Then
                    Dim itemState As Integer = Me.GetTreeviewItemState(w.Handle, foundNodeHandle, TVITEMStates.TVIS_FOCUSED)
                    Dim itemSelected As Boolean = (itemState And TVITEMStates.TVIS_FOCUSED) > 0
                    Return Reply.Result(itemSelected)
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindTreeNodeMatchingTheSpecifiedCriteria)
                End If
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                Dim foundItemIndex As Integer = Me.GetListviewItemIndex(searchText, fallBackIndex, w.Handle)
                Dim state As Integer = SendMessage(w.Handle, WindowMessages.LVM_GETITEMSTATE, foundItemIndex, LVIS.LVIS_FOCUSED).ToInt32()
                Return Reply.Result((state > 0))
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.IsItemFocusedIsNotAvailableOnControlsWithClassname0, w.ClassName))
        End Select
    End Function

    ''' <summary>
    ''' Gets the requested state information from a treeview item
    ''' </summary>
    ''' <param name="TreeviewHandle">The handle to the treeview of interest.</param>
    ''' <param name="TreenodeHandle">The handle of the treeview item of interest</param>
    ''' <param name="StateMask">The states of interest, ORed together if desired.</param>
    ''' <param name="BitShift">Optional rightwards bitshift to apply to the retrieved value
    ''' after retrieval. Eg to shift the image index down to a zero-based index from its
    ''' location in the high-order word.</param>
    ''' <returns>Returns the state retrieved via a TVM_GETITEM request, after the
    ''' supplied mask has been applied and the requested rightward bitshift performed.</returns>
    Private Function GetTreeviewItemState(ByVal TreeviewHandle As IntPtr, ByVal TreenodeHandle As IntPtr, ByVal StateMask As TVITEMStates, Optional ByVal BitShift As Integer = 0) As Integer
        Dim state = SendMessage(TreeviewHandle, WindowMessages.TVM_GETITEMSTATE, TreenodeHandle, StateMask).ToInt32()
        Return (state And StateMask) >> BitShift
    End Function

    ''' <summary>
    ''' Performs a hit-test against the specified listview.
    ''' </summary>
    ''' <param name="Handle">The handle of the listview of interest.</param>
    ''' <param name="HitTestInfo">The appropriate hit test structure, for
    ''' the chosen message.</param>
    ''' <returns>Returns true if the hit-test result indicates a success.</returns>
    Private Function GetListviewHitTestResult(ByVal Handle As IntPtr, ByRef HitTestInfo As LVHITTESTINFO) As Boolean
        Dim rpms As New RemoteProcessMessageSender(Of LVHITTESTINFO, LVHITTESTINFO)
        rpms.MessageToSend = WindowMessages.LVM_HITTEST
        rpms.Handle = Handle
        rpms.InputValues = New LVHITTESTINFO() {HitTestInfo}
        Dim outputs As LVHITTESTINFO()
        ReDim outputs(0)
        rpms.OutputValues = outputs
        rpms.wParam = 0

        Dim result As Integer = rpms.SendMessage(ProcessHandle).ToInt32()
        If result > -1 Then
            HitTestInfo = outputs(0)
            Return True
        Else
            Return False
        End If
    End Function

    <Category(Category.Win32)>
    <Command("As for SelectItem, but for use with multiple selection controls. Only currently works with a ListBox.")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' containing the text to match.")>
    Private Function ProcessCommandMultiSelectItem(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the (optional) fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing AndAlso fallBackIndex < 0 Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Select Case True
            Case w.ClassName.Contains("ListBox")
                Dim index As Integer = SendMessageString(w.Handle, WindowMessages.LB_FINDSTRINGEXACT, 0, searchText)
                SendMessage(w.Hwnd, WindowMessages.LB_SETSEL, 1, index)

            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                'Get the listview item index
                Dim listviewHandle = w.Handle
                Dim index As Integer
                index = Me.GetListviewItemIndex(searchText, fallBackIndex, listviewHandle)
                If index = -1 Then
                    Throw New InvalidOperationException(String.Format(My.Resources.NoItemWithTheText0Found, searchText))
                End If

                'Prepare the item to be sent                
                Dim lvItem As LV_ITEM
                lvItem.stateMask = LVIS.LVIS_SELECTED
                lvItem.state = LVIS.LVIS_SELECTED

                Dim rpms As New RemoteProcessMessageSender(Of LV_ITEM, LV_ITEM)
                rpms.Handle = w.Handle
                rpms.MessageToSend = WindowMessages.LVM_SETITEMSTATE
                rpms.wParam = index
                rpms.InputValues = New LV_ITEM() {lvItem}
                rpms.OutputValues = Nothing
                rpms.SendMessage(ProcessHandle)

                If rpms.Success Then
                    If rpms.ReturnValue.ToInt32() > 0 Then
                        Return Reply.Ok
                    Else
                        Throw New InvalidOperationException(String.Format(My.Resources.SendmessageDidNotIndicateSuccess0, GetLastWin32Error()))
                    End If
                Else
                    Throw New InvalidOperationException(rpms.ErrorMessage)
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SelectItemIsNotAvailableForControlsWithClassname0, w.ClassName))
        End Select

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Gets the conflicting character groups in a font.")>
    <Parameters("The name of the font of interest, using the 'Font' parameter.")>
    Private Function ProcessCommandGetConflictingFontCharacters(ByVal objQuery As clsQuery) As Reply
        Dim fontName As String = objQuery.GetParameter(ParameterNames.Font)
        If String.IsNullOrEmpty(fontName) Then
            Throw New InvalidArgumentException(My.Resources.NoFontSpecified)
        End If

        Dim doc As New XmlDocument()
        Dim collEl As XmlElement = doc.CreateElement("collection")
        doc.AppendChild(collEl)

        Dim conflicts As ICollection(Of ICollection(Of CharData))
        Try
            conflicts =
             FontReader.GetFontData(fontName).GetConflictingCharacterGroups()
        Catch nsfe As NoSuchFontException ' This can be thrown as is
            Throw
        Catch efe As EmptyFontException
            Return Reply.Result(doc.OuterXml)
        Catch ex As Exception
            Throw New OperationFailedException(ex,
             My.Resources.ErrorWhileLoadingFont01, fontName, ex)
        End Try

        For Each conflict As ICollection(Of CharData) In conflicts
            Dim rowEl As XmlElement = doc.CreateElement("row")
            Dim conflictsBuffer As New StringBuilder()
            For Each ch As CharData In conflict
                conflictsBuffer.Append(ch.Value)
            Next
            Dim fldEl As XmlElement = CreateCollectionFieldXML(
             doc, conflictsBuffer.ToString(), "text", "Character Group")
            rowEl.AppendChild(fldEl)
            collEl.AppendChild(rowEl)
        Next

        Return Reply.Result(doc.OuterXml)
    End Function

    ''' <summary>
    ''' Thread handler to do a default action on an Active Accessibility element.
    ''' </summary>
    Private Class AADoDefaultActionHandler
        Public Shared Sub Exec(ByVal o As Object)
            Try
                Dim e As clsAAElement = TryCast(o, clsAAElement)
                e.DoDefaultAction()
            Catch ex As Exception
            End Try
        End Sub
    End Class

    <Category(Category.Accessibility)>
    <Command("Performs the default action on the Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandAADefAction(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        'Create a thread that will do the actual action - we'll return
        'immediately once it has been initiated.
        Dim t As Thread = RunThread.CreateThread(AddressOf AADoDefaultActionHandler.Exec, e)
        t.Start()
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Sets an Active Accessibility elements check state ")>
    <Parameters("NewText specifies a value of true or false. Those required to uniquely identify the element.")>
    Private Function ProcessCommandAASetChecked(ByVal objQuery As clsQuery) As Reply
        Dim checked As Boolean = Boolean.Parse(objQuery.GetParameter(ParameterNames.NewText))
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        If e.Checked <> checked Then
            e.DoDefaultAction()
        End If
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the checked state of the Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESLUT:True"" if checked ""RESULT:False"" if not")>
    Private Function ProcessCommandAAGetChecked(ByVal objQuery As clsQuery) As Reply
        Dim sChecked As String
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        sChecked = e.Checked.ToString
        Return Reply.Result(sChecked)
    End Function

    <Category(Category.Accessibility)>
    <Command("Sets the value of an Active Accessibility element.")>
    <Parameters("The value to be set specified by NewText and those required to uniquely identify the element.")>
    Private Function ProcessCommandAASetValue(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        e.Value = sNewText
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the value of the Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the value of the element.")>
    Private Function ProcessCommandAAGetValue(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        sNewText = e.Value
        Return Reply.Result(sNewText)
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the name of the Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<name>"" where <name> is the name of the element.")>
    Private Function ProcessCommandAAGetName(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        Return Reply.Result(e.Name)
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the description of the Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<description>"" where <description> is the description of the element.")>
    Private Function ProcessCommandAAGetDescription(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        Return Reply.Result(e.Description)
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the item count of a ComboBox Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<count>"" where <count> is the number of items in the ComboBox element.")>
    Private Function ProcessCommandAAGetItemCount(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement
        e = mobjModel.IdentifyAccessibleObject(objQuery)

        Select Case e.Role
            Case AccessibleRole.ComboBox
                'Get reference to the inner list, which is nested in
                'a window with scrollbars, etc
                Dim ComboInnerList As clsAAElement = Me.GetAAComboBoxInnerList(e)
                If ComboInnerList IsNot Nothing Then
                    Return Reply.Result(ComboInnerList.Elements.Count)
                Else
                    Throw New InvalidOperationException(My.Resources.FailedToIdentifyTheComboBoxSInnerList)
                End If
            Case AccessibleRole.List
                Return Reply.Result(e.Elements.Count)
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetItemCountIsNotImplementedForActiveAccessibilityElementsWithRole0, e.Role.ToString))
        End Select
    End Function

    <Category(Category.Accessibility)>
    <Command("Shows the drop down of a drop down Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandAAShowDropDown(ByVal objQuery As clsQuery) As Reply
        Dim e = mobjModel.IdentifyAccessibleObject(objQuery)
        AAShowHideDropDown(e, True)
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Hides the drop down of a drop down Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandAAHideDropDown(ByVal objQuery As clsQuery) As Reply
        Dim e = mobjModel.IdentifyAccessibleObject(objQuery)
        AAShowHideDropDown(e, False)
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Gives focus to the Active Accessibility element.")>
    <Parameters("Identifiers to uniquely identify the element.")>
    Private Function ProcessCommandAAFocus(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        ForceForeground(e.Window)
        e.TakeFocus()
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Activates the parent window of the AA element, gives focus to the AA element, and performs global send keys.")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' (keys to send) and, optionally, 'interval' (seconds to wait between each key).")>
    Private Function ProcessCommandAASendKeys(ByVal q As clsQuery) As Reply
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(q)
        ForceForeground(e.Window)
        e.TakeFocus()
        Win32SendKeys(q)
        Return Reply.Ok
    End Function

    <Category(Category.Accessibility)>
    <Command("Selects a item in a ComboBox Active Accessibility element.")>
    <Parameters("The index of the item to be selected specified by NewText and those required to uniquely identify the element.")>
    Private Function ProcessCommandAASelectItem(ByVal objQuery As clsQuery) As Reply
        Dim e = mobjModel.IdentifyAccessibleObject(objQuery)

        'Get the position of the item to select.
        'This param is secondary to the searchtext
        'param - if the searchtext param is specified
        'then this one is ignored
        Dim itemIndexString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim itemIndex As Integer = -1
        If Not String.IsNullOrEmpty(itemIndexString) Then
            If Not Integer.TryParse(itemIndexString, itemIndex) Then
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, itemIndexString))
            Else
                'Correction for 1-based index
                itemIndex -= 1
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(searchText) Then
            If itemIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.ItemTextNotSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case e.Role
            Case AccessibleRole.ComboBox

                'First click the combobox dropdown button
                AAShowHideDropDown(e, True)

                'Get reference to the inner list, which is nested in a window
                'with scrollbars, etc
                Dim comboInnerList As clsAAElement = Me.GetAAComboBoxInnerList(e)
                If comboInnerList Is Nothing Then _
                    Throw New InvalidOperationException(My.Resources.FailedToIdentifyTheComboBoxSInnerList)

                'If Search Text is specified then we need to search for index to select
                If Not String.IsNullOrEmpty(searchText) Then
                    For i As Integer = 0 To comboInnerList.Elements.Count - 1
                        Dim listItem As clsAAElement = comboInnerList.Elements(i)
                        If String.Compare(listItem.Name, searchText, False) = 0 Then
                            itemIndex = i
                            Exit For
                        End If
                    Next
                End If

                If itemIndex < 0 Then _
                    Throw New InvalidOperationException(My.Resources.FailedToIndentifyTheItemToBeSelected)

                'Now just double click the correct item in the combo box.
                Dim element = comboInnerList.Elements(itemIndex)
                element.TakeFocus()
                element.DoDefaultAction()
                Return Reply.Ok

            Case AccessibleRole.List
                If Not String.IsNullOrEmpty(searchText) Then
                    For i As Integer = 0 To e.Elements.Count - 1
                        Dim ListItem As clsAAElement = e.Elements(i)
                        If String.Compare(ListItem.Name, searchText, False) = 0 Then
                            itemIndex = i
                            Exit For
                        End If
                    Next
                End If

                If itemIndex > -1 Then
                    e.Elements(itemIndex).TakeFocusSelection()
                    Return Reply.Ok
                Else
                    Throw New InvalidOperationException(My.Resources.FailedToIndentifyTheItemToBeSelected)
                End If

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.SelectItemIsNotImplementedForActiveAccessibilityElementsWithRole0, e.Role.ToString))
        End Select
    End Function

    ''' <summary>
    ''' Get the button for an AA ComboBox Element
    ''' </summary>
    ''' <param name="comboBoxElement"></param>
    Private Function GetAAComboBoxButton(comboBoxElement As clsAAElement) As clsAAElement
        For Each child As clsAAElement In comboBoxElement.Elements
            If child.Role = AccessibleRole.PushButton Then
                Return child
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Show or hide Active Accessability combo box dropdown menu.
    ''' </summary>
    ''' <param name="show">True to show, False to hide.</param>
    ''' <param name="comboBoxElement">The combobox element</param>
    ''' <remarks></remarks>
    Private Sub AAShowHideDropDown(comboBoxElement As clsAAElement, show As Boolean)
        Dim button = GetAAComboBoxButton(comboBoxElement)
        If button Is Nothing Then _
            Throw New InvalidOperationException(My.Resources.FailedToIdentifyTheComboBoxSDropdownButton)

        Select Case button.DefaultAction
            Case "Open"
                If show Then button.DoDefaultAction()
            Case "Close"
                If Not show Then button.DoDefaultAction()
            Case Else
                Throw New InvalidOperationException(String.Format("Unexpected default action of '{0}' on combo box's button", button.DefaultAction))
        End Select
    End Sub

    ''' <summary>
    ''' Gets the inner list contained in a combobox.
    ''' </summary>
    ''' <param name="ComboBoxElement">The combobox element whose inner list
    ''' is sought.</param>
    ''' <returns>Returns the inner list, if it can be found; returns
    ''' nothing if not.</returns>
    Private Function GetAAComboBoxInnerList(ByVal ComboBoxElement As clsAAElement) As clsAAElement

        'Check for bad calls
        If ComboBoxElement IsNot Nothing Then
            If ComboBoxElement.Role <> AccessibleRole.ComboBox Then
                Throw New System.ArgumentException(My.Resources.ElementMustHaveRoleOfComboBox, NameOf(ComboBoxElement))
            End If
        Else
            Throw New System.ArgumentException(My.Resources.ValueCanNotBeNull, NameOf(ComboBoxElement))
        End If

        'Loop through the first two level of descendents, where we
        'expect to find the inner list as a grandchild.
        For Each child As clsAAElement In ComboBoxElement.Elements
            If child.Role = AccessibleRole.Window Then
                For Each grandchild As clsAAElement In child.Elements
                    If grandchild.Role = AccessibleRole.List Then
                        Return grandchild
                    End If
                Next
            ElseIf child.Role = AccessibleRole.ListItem Then
                'In some unusual applications such as FoxPro the combobox IS the list.
                'In this case the children will be list items, and the 'InnerList' as
                'we call it is therefore the parent.
                Return child.Parent
            End If
        Next

        Return Nothing
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets a list of all the items in a List or ComboBox Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collection xml representing the list. The collection contains a single column called 'Item Text'.")>
    Private Function ProcessCommandAAGetAllItems(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement
        Try
            e = mobjModel.IdentifyAccessibleObject(objQuery)
        Catch ex As ApplicationException
            Throw New InvalidOperationException(My.Resources.FailedToIdentifyElementUsingTheSpecifiedQueryTerms)
        End Try

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)


        Select Case e.Role
            Case AccessibleRole.ComboBox
                'Get reference to the inner list, which is nested in
                'a window with scrollbars, etc
                Dim comboInnerList As clsAAElement = Me.GetAAComboBoxInnerList(e)
                If comboInnerList IsNot Nothing Then
                    For Each childItem As clsAAElement In comboInnerList.Elements
                        Dim rowElement As XmlElement = xdoc.CreateElement("row")
                        rowElement.AppendChild(CreateCollectionFieldXML(xdoc, childItem.Name, "text", "Item Text"))
                        collectionRoot.AppendChild(rowElement)
                    Next
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindInternalListForComboBox)
                End If

            Case AccessibleRole.List
                For Each childItem As clsAAElement In e.Elements
                    Dim RowElement As XmlElement = xdoc.CreateElement("row")
                    RowElement.AppendChild(CreateCollectionFieldXML(xdoc, childItem.Name, "text", "Item Text"))
                    collectionRoot.AppendChild(RowElement)
                Next

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetAllItemsNotImplementedForElementsWithRole0, e.Role.ToString))
        End Select

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets a list of selected items in a List or ComboBox Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collection xml representing the list.")>
    Private Function ProcessCommandAAGetSelectedItems(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement
        Try
            e = mobjModel.IdentifyAccessibleObject(objQuery)
        Catch ex As ApplicationException
            Throw New InvalidOperationException(My.Resources.FailedToIdentifyElementUsingTheSpecifiedQueryTerms)
        End Try

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)

        Select Case e.Role
            Case AccessibleRole.ComboBox
                'Get reference to the inner list, which is nested in
                'a window with scrollbars, etc
                Dim comboInnerList As clsAAElement = Me.GetAAComboBoxInnerList(e)
                If comboInnerList IsNot Nothing Then
                    For Each childItem As clsAAElement In comboInnerList.Elements
                        If childItem.Selected Then
                            Dim RowElement As XmlElement = xdoc.CreateElement("row")
                            RowElement.AppendChild(CreateCollectionFieldXML(xdoc, childItem.Name, "text", "Item Text"))
                            collectionRoot.AppendChild(RowElement)
                        End If
                    Next
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindInternalListForComboBox)
                End If

            Case AccessibleRole.List
                For Each childItem As clsAAElement In e.Elements
                    If childItem.Selected Then
                        Dim RowElement As XmlElement = xdoc.CreateElement("row")
                        RowElement.AppendChild(CreateCollectionFieldXML(xdoc, childItem.Name, "text", "Item Text"))
                        collectionRoot.AppendChild(RowElement)
                    End If
                Next

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetAllItemsNotImplementedForElementsWithRole0, e.Role.ToString))
        End Select

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the value of the selected item in a List or ComboBox Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<value>"" where <value> is the value of the selected item.")>
    Private Function ProcessCommandAAGetSelectedItemText(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement
        Try
            e = mobjModel.IdentifyAccessibleObject(objQuery)
        Catch ex As ApplicationException
            Throw New InvalidOperationException(My.Resources.FailedToIdentifyElementUsingTheSpecifiedQueryTerms)
        End Try

        Select Case e.Role
            Case AccessibleRole.ComboBox
                'Get reference to the inner list, which is nested in
                'a window with scrollbars, etc
                Dim comboInnerList As clsAAElement = Me.GetAAComboBoxInnerList(e)
                If comboInnerList IsNot Nothing Then
                    For Each ChildItem As clsAAElement In comboInnerList.Elements
                        If ChildItem.Selected Then
                            Return Reply.Result(ChildItem.Name)
                        End If
                    Next
                Else
                    Throw New InvalidOperationException(My.Resources.CouldNotFindInternalListForComboBox)
                End If
            Case AccessibleRole.List
                For Each ChildItem As clsAAElement In e.Elements
                    If ChildItem.Selected Then
                        Return Reply.Result(ChildItem.Name)
                    End If
                Next
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetAllItemsNotImplementedForElementsWithRole0, e.Role.ToString))
        End Select

        Return Reply.Result("")
    End Function

    <Category(Category.Accessibility)>
    <Command("Checks to see if the Active Accessibility element can be identified.")>
    <Parameters("Those required to uniquely identify the element.")>
    Private Function ProcessCommandAACheckExists(ByVal objQuery As clsQuery) As Reply
        Try
            Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        Catch ex As ApplicationException
            Return Reply.Result(False)
        End Try
        Return Reply.Result(True)
    End Function

    ''' <summary>
    ''' Ensures that the supplied value is within the
    ''' specified range, adjusting it if necessary.
    ''' </summary>
    ''' <param name="Value">The value to be checked/adjusted.</param>
    ''' <param name="Minimum">The minimum acceptable value for Value.
    ''' Must not exceed Maximum.</param>
    ''' <param name="Maximum">The maximum acceptable value for Value.</param>
    Private Sub ConfineValueToRange(ByRef Value As Integer, ByVal Minimum As Integer, ByVal Maximum As Integer)
        Value = Math.Max(Math.Min(Value, Maximum), Minimum)
    End Sub

    <Category(Category.Win32)>
    <Command("Scrolls to the minimum value of a scrollable window.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandScrollToMinimum(ByVal objQuery As clsQuery) As Reply
        objQuery = clsQuery.Parse(objQuery.QueryString & " numericvalue=" & Integer.MinValue.ToString)
        Return ProcessCommandSetNumericValue(objQuery)
    End Function

    <Category(Category.Win32)>
    <Command("Scrolls to the maximum value of a scrollable window.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandScrollToMaximum(ByVal objQuery As clsQuery) As Reply
        objQuery = clsQuery.Parse(objQuery.QueryString & " numericvalue=" & Integer.MaxValue.ToString)
        Return ProcessCommandSetNumericValue(objQuery)
    End Function

    <Category(Category.Win32)>
    <Command("Gets the WindowText of the given window.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandGetWindowText(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(w.WindowText)
    End Function

    <Category(Category.Win32)>
    <Command("OBSOLETE: Gets the bounds of a window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the window bounds.")>
    <Obsolete>
    Private Function ProcessCommandGetElementBounds(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(CreateCollectionXMLFromRectangle(w.ClientBounds))
    End Function

    <Category(Category.Win32)>
    <Command("Gets the bounds of a window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the window bounds.")>
    Private Function ProcessCommandGetRelativeElementBounds(objQuery As clsQuery) As Reply
        Dim w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(CreateCollectionXMLFromRectangle(w.RelativeClientBounds))
    End Function

    <Category(Category.Win32)>
    <Command("Gets the screen relative bounds of a window.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the windows screen bounds.")>
    Private Function ProcessCommandGetElementScreenBounds(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Return Reply.Result(CreateCollectionXMLFromRectangle(w.ScreenBounds))
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the bounds of an Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element bounds.")>
    Private Function ProcessCommandAAGetElementBounds(ByVal objQuery As clsQuery) As Reply
        Dim aa As clsAAElement
        aa = mobjModel.IdentifyAccessibleObject(objQuery)
        Return Reply.Result(CreateCollectionXMLFromRectangle(aa.ClientBounds))
    End Function

    <Category(Category.Accessibility)>
    <Command("Gets the screen relative bounds of an Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing the element screen bounds.")>
    Private Function ProcessCommandAAGetElementScreenBounds(ByVal objQuery As clsQuery) As Reply
        Dim aa As clsAAElement
        aa = mobjModel.IdentifyAccessibleObject(objQuery)
        Return Reply.Result(CreateCollectionXMLFromRectangle(aa.ScreenBounds))
    End Function

    <Category(Category.Win32)>
    <Command("Gets all items from a control, as a collection.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:"" followed by some Automate collection xml, or error message. For simple controls, the collection contains a single column called 'Item Text'. For more controls with multiple columns themselves, the columns are named as in the control itself where possible.")>
    Private Function ProcessCommandGetAllItems(ByVal objQuery As clsQuery) As Reply
        'Decide what message is needed for getting the items
        Dim ew As clsUIWindow = mobjModel.IdentifyWindow(objQuery)

        'Count the number of items in the control
        Dim itemCount As Integer = GetItemCount(ew)

        Dim getTextMessage As WindowMessages
        Dim getLengthMessage As WindowMessages
        Select Case True
            Case ew.ClassName.Contains("ListBox") OrElse ew.ClassName.Contains("LISTBOX")
                getTextMessage = WindowMessages.LB_GETTEXT
                getLengthMessage = WindowMessages.LB_GETTEXTLEN
            Case ew.ClassName.Contains("COMBOBOX"), ew.ClassName.Contains("ComboBox")
                getTextMessage = WindowMessages.CB_GETLBTEXT
                getLengthMessage = WindowMessages.CB_GETLBTEXTLEN
            Case ew.ClassName.Contains("SysListView32"), ew.ClassName.Contains("PBListView32")
                Return Reply.Result(GetListviewItems(ew.Handle, False))
            Case ew.ClassName.Contains("TabControl")
                Return Reply.Result(GetTabControlItems(ew))
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetAllItemsIsNotImplementedForControlsWithClassname0, ew.ClassName))
        End Select

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)

        'Get each item in turn and add it to the collection xml
        For itemIndex As Integer = 0 To itemCount - 1
            Dim itemText As String = Nothing
            Try
                itemText = GetItemText(ew.Handle, getLengthMessage, getTextMessage, itemIndex)
            Catch ex As Exception
                Dim message As String = ex.Message
                If ex.InnerException IsNot Nothing Then message &= " - " & ex.InnerException.Message.ToString
                Throw New InvalidOperationException(message)
            End Try

            'Add data to xml collection
            Dim rowElement As XmlElement = xdoc.CreateElement("row")
            rowElement.AppendChild(CreateCollectionFieldXML(xdoc, itemText, "text", "Item Text"))
            collectionRoot.AppendChild(rowElement)
        Next

        Return Reply.Result(xdoc.OuterXml)
    End Function

    ''' <summary>
    ''' Gets the items in a listview, as an Automate collection.
    ''' </summary>
    ''' <param name="ListviewHandle">The handle of the listview of interest.</param>
    ''' <param name="SelectedOnly">If true, then only items which are selected in the
    ''' listview are returned.</param>
    ''' <returns>A string representing an Automate collection, in xml.</returns>
    Private Function GetListviewItems(ByVal ListviewHandle As IntPtr, ByVal SelectedOnly As Boolean) As String
        'Get array representing the order of columns
        Dim columnIndices As Int32()
        Try
            columnIndices = Me.GetListviewColumnIndices(ListviewHandle)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstDeterminingTheOrderOfTheListviewColumns0, ex.Message))
        End Try

        'Get an array containing the names of all of the columns, ordered
        'as they appear on screen
        Dim columnNames As String()
        ReDim columnNames(columnIndices.Length - 1)
        For columnIndex As Integer = 0 To columnIndices.Length - 1
            Dim abstractColumnIndex As Integer = columnIndices(columnIndex)
            Try
                columnNames(columnIndex) = GetListviewColumnText(ListviewHandle, abstractColumnIndex)
            Catch ex As Exception
                Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstRetrievingTextForColumn01, (columnIndex + 1).ToString, ex.Message))
            End Try
        Next

        'Prepare xml document for return value
        Dim xListviewDoc As New XmlDocument()
        Dim listviewCollectionRoot As XmlElement = xListviewDoc.CreateElement("collection")
        xListviewDoc.AppendChild(listviewCollectionRoot)

        'Add each row in turn
        Dim itemCount As Integer = SendMessage(ListviewHandle, WindowMessages.LVM_GETITEMCOUNT, 0, 0).ToInt32()
        For rowIndex As Integer = 0 To itemCount - 1
            Dim itemState As Integer = SendMessage(ListviewHandle, WindowMessages.LVM_GETITEMSTATE, rowIndex, LVIS.LVIS_SELECTED).ToInt32()
            Dim itemSelected As Boolean = (itemState And LVIS.LVIS_SELECTED) > 0

            If (Not SelectedOnly) OrElse itemSelected Then
                Dim rowElement As XmlElement = Me.GetListviewItemAsCollectionRow(xListviewDoc, ListviewHandle, rowIndex, columnIndices, columnNames)
                listviewCollectionRoot.AppendChild(rowElement)
            End If
        Next

        Return xListviewDoc.OuterXml
    End Function

    ''' <summary>
    ''' Gets all items of a tab control as an automate collection xml string.
    ''' </summary>
    ''' <param name="w">The clsUIWindow representing the tab control.</param>
    ''' <returns>Returns collection xml for the tab items.</returns>
    Private Function GetTabControlItems(ByVal w As clsUIWindow) As String
        Dim itemCount As Integer = GetTabControlItemCount(w)

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)

        For i As Integer = 0 To itemCount - 1
            'Add the row
            Dim rowElement As XmlElement = xdoc.CreateElement("row")
            rowElement.AppendChild(CreateCollectionFieldXML(xdoc, GetTabControlItemText(w, i), "text", "Item Text"))
            collectionRoot.AppendChild(rowElement)
        Next

        Return xdoc.OuterXml
    End Function

    <Category(Category.Win32)>
    <Command("Gets an item within a listview control.")>
    <Parameters("The index of the item specified by 'Position' or the text of the item specified by 'NewText' and those required to uniquely identify the window.")>
    Private Function ProcessCommandGetItem(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the fallback index
        Dim positionString As String = objQuery.GetParameter(ParameterNames.Position)
        Dim fallBackIndex As Integer = -1
        If positionString IsNot Nothing Then
            If Integer.TryParse(positionString, fallBackIndex) Then
                fallBackIndex -= 1 'Adjustment for zero-based index from one-based index
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValue0AsANumber, positionString))
            End If
        End If

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(searchText) Then
            If fallBackIndex < 0 Then
                Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
            End If
        End If

        Select Case True
            Case w.ClassName.Contains("SysListView32"), w.ClassName.Contains("PBListView32")
                'Find the item of interest
                Dim rowIndex As Integer = Me.GetListviewItemIndex(searchText, fallBackIndex, w.Handle)
                If rowIndex = -1 Then
                    Throw New InvalidOperationException(My.Resources.CouldNotFindListviewItemMatchingTheSuppliedCriteria)
                End If

                'Get array representing the order of columns
                Dim columnIndices As Int32()
                Try
                    columnIndices = Me.GetListviewColumnIndices(w.Handle)
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstDeterminingTheOrderOfTheListviewColumns0, ex.Message))
                End Try

                'Get an array containing the names of all of the columns, ordered
                'as they appear on screen
                Dim columnNames As String()
                ReDim columnNames(columnIndices.Length - 1)
                For columnIndex As Integer = 0 To columnIndices.Length - 1
                    Dim abstractColumnIndex As Integer = columnIndices(columnIndex)
                    Try
                        columnNames(columnIndex) = GetListviewColumnText(w.Handle, abstractColumnIndex)
                    Catch ex As Exception
                        Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstRetrievingTextForColumn01, (columnIndex + 1).ToString, ex.Message))
                    End Try
                Next

                'Prepare xml document 
                Dim xListviewDoc As New XmlDocument()
                Dim listviewCollectionRoot As XmlElement = xListviewDoc.CreateElement("collection")
                xListviewDoc.AppendChild(listviewCollectionRoot)

                'Add the row
                Dim rowElement As XmlElement = Me.GetListviewItemAsCollectionRow(xListviewDoc, w.Handle, rowIndex, columnIndices, columnNames)
                listviewCollectionRoot.AppendChild(rowElement)

                Return Reply.Result(xListviewDoc.OuterXml)

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetItemIsNotAvailableForControlsWithClassname0, w.ClassName))

        End Select
    End Function

    ''' <summary>
    ''' Gets the data from a listview item as a collection.
    ''' </summary>
    ''' <param name="ParentDocument">The xml document which is to own the new row.</param>
    ''' <param name="ListviewHandle">The handle to the listview of interest.</param>
    ''' <param name="ItemIndex">The zero-based index of the row of interest.</param>
    ''' <param name="ColumnIndices">An array of integers indicating the order in which
    ''' columns are presented on screen, indexed from zero. Eg if the 3rd column (in the data model is
    ''' presented at the fifth position on screen, then the value at index 2 will be 4).</param>
    ''' <param name="ColumnNames">The names of the columns, in the order in which they 
    ''' appear on screen - eg the item at index 2 is the text of the column displayed
    ''' in the third position on screen).</param>
    ''' <returns>Returns an xml element representing an Automate collection row.</returns>
    Private Function GetListviewItemAsCollectionRow(ByVal ParentDocument As XmlDocument, ByVal ListviewHandle As IntPtr, ByVal ItemIndex As Integer, ByVal ColumnIndices As Integer(), ByVal ColumnNames As String()) As XmlElement
        Dim rowElement As XmlElement = ParentDocument.CreateElement("row")

        'Loop through columns adding data
        For ScreenColumnIndex As Integer = 0 To ColumnIndices.Length - 1
            'The index of the column in the data model which corresponds
            'to the column displayed at this position on screen
            Dim abstractColumnIndex As Integer = ColumnIndices(ScreenColumnIndex)
            Dim itemText As String
            Try
                itemText = Me.GetListviewItemText(ListviewHandle, ItemIndex, abstractColumnIndex)
            Catch ex As Exception
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetTextForItem0Column1, (ItemIndex + 1).ToString, (ScreenColumnIndex + 1).ToString), ex)
            End Try

            'Add data to current xml collection row
            Dim fieldelement As XmlElement = CreateCollectionFieldXML(ParentDocument, itemText, "text", ColumnNames(ScreenColumnIndex))
            rowElement.AppendChild(fieldelement)
        Next

        Return rowElement
    End Function

    ''' <summary>
    ''' Counts the number of columns in a listview.
    ''' </summary>
    ''' <param name="ListviewHandle">The handle to the listview of interest.</param>
    ''' <returns>Returns the number of columns contained in the listview.</returns>
    ''' <remarks>Throws an exception on error.</remarks>
    Private Function GetListviewColumnCount(ByVal ListviewHandle As IntPtr) As Integer
        Dim header = SendMessage(ListviewHandle, WindowMessages.LVM_GETHEADER, 0, 0)
        If header.ToInt64() = 0 Then
            Throw New ApiException(String.Format(My.Resources.FailedToGetListviewColumnHeader0, GetLastWin32Error()))
        End If

        Dim columnCount As Integer = SendMessage(header, WindowMessages.HDM_GETITEMCOUNT, 0, 0).ToInt32()
        If columnCount = -1 Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToCountTheNumberOfListviewColumns0, GetLastWin32Error()))
        End If

        Return columnCount
    End Function

    ''' <summary>
    ''' Gets the order of a listview's columns as they appear on screen.
    ''' </summary>
    ''' <param name="ListviewHandle">The handle of the listview of interest.</param>
    ''' <returns>Returns an array of integers, containing the indices of the columns
    ''' in the order in which they appear on screen.</returns>
    Private Function GetListviewColumnIndices(ByVal ListviewHandle As IntPtr) As Int32()
        Dim columnCount As Integer = GetListviewColumnCount(ListviewHandle)
        Dim intArray As Int32()
        ReDim intArray(columnCount - 1)

        Dim rpms As New RemoteProcessMessageSender(Of Int32, Int32)
        rpms.Handle = ListviewHandle
        rpms.InputValues = Nothing
        rpms.OutputValues = intArray
        rpms.MessageToSend = WindowMessages.LVM_GETCOLUMNORDERARRAY
        rpms.wParam = columnCount
        rpms.SendMessage(ProcessHandle)

        Dim columnIndices As Int32() = Nothing
        If rpms.Success AndAlso rpms.ReturnValue.ToInt32() > 0 Then
            columnIndices = rpms.OutputValues
        End If

        If (columnIndices Is Nothing) OrElse (columnIndices.Length = 0) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetColumnDetails0, GetLastWin32Error()))
        End If

        Return columnIndices
    End Function

    ''' <summary>
    ''' Gets the text of a listview column.
    ''' </summary>
    ''' <param name="ListviewHandle">The listview of interest.</param>
    ''' <param name="ColumnIndex">The zero-based index of the column
    ''' whose text is desired. This is the index of the column in the remote
    ''' application's data model, which may not necessarily correspond to the
    ''' index of the column as it appears on screen. Use GetListviewColumnIndices to
    ''' map between the two.</param>
    ''' <returns>Returns a string containing the text of the listview column.</returns>
    ''' <remarks>Throws an exception on error.
    ''' 
    ''' The text returned will not necessarily match that displayed on screen,
    ''' since the text displayed is truncated beyond 260 characters.</remarks>
    Private Function GetListviewColumnText(ByVal ListviewHandle As IntPtr, ByVal ColumnIndex As Integer) As String
        'Get handle to the listview header
        Dim headerHandle = SendMessage(ListviewHandle, WindowMessages.LVM_GETHEADER, 0, 0)
        If headerHandle.ToInt64() = 0 Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetListviewColumnHeader0, GetLastWin32Error()))
        End If

        Dim item As New HDITEM
        item.mask = HDI.HDI_TEXT
        Dim structSize As Integer = Marshal.SizeOf(item)

        'The following faff ensues because we don't know how
        'long the text is. We repeatedly retrieve the text
        'using buffers of increasing size until there is spare capacity
        'in the buffer allocated; there doesn't seem to more direct
        'way of finding out how large a buffer is necessary.
        Dim pRemoteItem As IntPtr
        Dim itemText As String
        Try
            pRemoteItem = VirtualAllocEx(ProcessHandle, IntPtr.Zero, structSize, MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)

            Dim textLength As Integer = 128 'Reasonable initial length
            Dim returnedTextLength As Integer
            Dim returnedString As String
            Do
                textLength *= 2 'Allocate twice as many bytes as last time
                item.cchTextMax = textLength - 1 'Minus one to account for the terminating null byte

                Dim textBuffer As IntPtr
                Try
                    'Allocate a new text buffer to receive column text
                    textBuffer = VirtualAllocEx(ProcessHandle, IntPtr.Zero, textLength, MEM_COMMIT Or MEM_RESERVE, PAGE_READWRITE)
                    item.pszText = textBuffer

                    'Copy structure into remote memory
                    Marshal.DestroyStructure(pRemoteItem, GetType(HDITEM))
                    Dim pLocalItem As IntPtr = Marshal.AllocHGlobal(structSize)
                    Marshal.StructureToPtr(item, pLocalItem, False)
                    WriteProcessMemory(ProcessHandle, pRemoteItem, pLocalItem, structSize, IntPtr.Zero)
                    Marshal.FreeHGlobal(pLocalItem)

                    'Gets the text up to the max length specified;
                    Dim success = SendMessage(headerHandle, WindowMessages.HDM_GETITEMA, ColumnIndex, pRemoteItem).ToInt32()
                    If success = 0 Then
                        Throw New ApiException(String.Format(My.Resources.APICallFailedWhilstRetrievingTextForColumn0, (ColumnIndex + 1).ToString))
                    End If

                    'Read the string out of the remote text buffer
                    Dim pLocalText As IntPtr = Marshal.AllocHGlobal(textLength)
                    ReadProcessMemory(ProcessHandle, textBuffer, pLocalText, textLength, IntPtr.Zero)
                    returnedString = Marshal.PtrToStringAnsi(pLocalText)
                    Marshal.FreeHGlobal(pLocalText)
                Finally
                    If textBuffer <> IntPtr.Zero Then
                        VirtualFreeEx(ProcessHandle, textBuffer, 0, MEM_RELEASE)
                    End If
                End Try

                If Not String.IsNullOrEmpty(returnedString) Then
                    returnedTextLength = returnedString.Length
                Else
                    returnedTextLength = 0
                End If

                'We only quit the loop once we are sure that the buffer
                'allocated is sufficiently large to contain the whole text
            Loop While (returnedTextLength = item.cchTextMax)

            itemText = returnedString
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstRetrievingTextForColumn01, (ColumnIndex + 1).ToString, ex.Message))
        Finally
            If pRemoteItem <> IntPtr.Zero Then
                VirtualFreeEx(ProcessHandle, pRemoteItem, 0, MEM_RELEASE)
            End If
        End Try

        Return itemText
    End Function

    ''' <summary>
    ''' Gets the text from a listview item.
    ''' </summary>
    ''' <param name="ListviewHandle">Handle to the listview of interest.</param>
    ''' <param name="ItemIndex">The zero-based index of the row in which the item resides.</param>
    ''' <param name="SubItemIndex">The one-based index of the subitem to fetch, or set
    ''' to zero to fetch the text of the item itself. This is the index of the subitem in the remote
    ''' application's data model, which may not necessarily correspond to the
    ''' index of the subitem as it appears on screen. Use GetListviewColumnIndices to
    ''' map between the two.</param>
    ''' <returns>Returns the text contained in the requested item/subitem.</returns>
    Private Function GetListviewItemText(ByVal ListviewHandle As IntPtr, ByVal ItemIndex As Integer, ByVal SubItemIndex As Integer) As String
        Dim lvItem As New LV_ITEM
        lvItem.iItem = ItemIndex
        lvItem.iSubItem = SubItemIndex 'Subitems are indexed from 1; the item at index 0 is the parent item itself
        lvItem.mask = LVIF.LVIF_TEXT
        Dim structSize As Integer = Marshal.SizeOf(lvItem)

        'The following faff ensues because we don't know how
        'long the text is. We repeatedly retrieve the text
        'using buffers of increasing size until there is spare capacity
        'in the buffer allocated; there doesn't seem to more direct
        'way of finding out how large a buffer is necessary.
        Dim pRemoteItem As IntPtr
        Dim itemText As String
        Try
            pRemoteItem = VirtualAllocEx(ProcessHandle, IntPtr.Zero, Marshal.SizeOf(lvItem), MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)

            Dim textLength As Integer = 128 'Reasonable initial length
            Dim returnedString As String
            Dim returnedTextLength As Integer
            Do
                textLength *= 2 'Allocate twice as many bytes as last time
                lvItem.cchTextMax = textLength - 1 'Minus one to account for the terminating null byte

                Dim textBuffer As IntPtr
                Try
                    'Allocate a new text buffer to receive column text
                    textBuffer = VirtualAllocEx(ProcessHandle, IntPtr.Zero, textLength, MEM_COMMIT Or MEM_RESERVE, PAGE_READWRITE)
                    lvItem.pszText = textBuffer

                    'Copy structure into remote memory
                    Marshal.DestroyStructure(pRemoteItem, GetType(LV_ITEM))
                    Dim pLocalItem As IntPtr = Marshal.AllocHGlobal(structSize)
                    Marshal.StructureToPtr(lvItem, pLocalItem, False)
                    WriteProcessMemory(ProcessHandle, pRemoteItem, pLocalItem, structSize, IntPtr.Zero)
                    Marshal.FreeHGlobal(pLocalItem)

                    'Gets the text up to the max length specified
                    returnedTextLength = SendMessage(ListviewHandle, WindowMessages.LVM_GETITEMTEXTA, ItemIndex, pRemoteItem).ToInt32()
                    If returnedTextLength = -1 Then
                        Throw New InvalidOperationException(String.Format(My.Resources.APICallFailedWhilstRetrievingTextForRow0Column1, (ItemIndex + 1).ToString, (SubItemIndex + 1).ToString))
                    End If

                    'Read the string out of the remote text buffer
                    Dim pLocalText As IntPtr = Marshal.AllocHGlobal(textLength)
                    ReadProcessMemory(ProcessHandle, textBuffer, pLocalText, textLength, IntPtr.Zero)
                    returnedString = Marshal.PtrToStringAnsi(pLocalText)
                    Marshal.FreeHGlobal(pLocalText)
                Finally
                    If textBuffer <> IntPtr.Zero Then
                        VirtualFreeEx(ProcessHandle, textBuffer, 0, MEM_RELEASE)
                    End If
                End Try

                'We only quit the loop once we are sure that the buffer
                'allocated is sufficiently large to contain the whole text
            Loop While (returnedTextLength = lvItem.cchTextMax)

            itemText = returnedString

        Catch ex As InvalidOperationException
            Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstRetrievingTextForItem0AndColumn12, (ItemIndex + 1).ToString, (SubItemIndex + 1).ToString, ex.Message))
        Finally
            If pRemoteItem <> IntPtr.Zero Then
                VirtualFreeEx(ProcessHandle, pRemoteItem, 0, MEM_RELEASE)
            End If
        End Try

        Return itemText
    End Function

    ''' <summary>
    ''' Gets the text of a menu item.
    ''' </summary>
    ''' <param name="hMenu">The parent menu of interest.</param>
    ''' <param name="ItemIndex">The zero-based index of the menu item of interest.</param>
    ''' <returns>Gets the string contained in the specified
    ''' menu item.</returns>
    ''' <remarks>Throws an exception on error.</remarks>
    Private Function GetMenuItemText(ByVal hMenu As IntPtr, ByVal ItemIndex As Integer) As String
        Dim si As New StringBuilder(100)
        Dim r As Integer = GetMenuString(hMenu, ItemIndex, si, 100, MF.MF_BYPOSITION)
        Return si.ToString
    End Function

    ''' <summary>
    ''' Gets the bounds of the specified menu item, in screen coordinates.
    ''' </summary>
    ''' <param name="OwningWindowHandle">The handle of the window owning the
    ''' menu containing the menu item of interest.</param>
    ''' <param name="MenuHandle">A handle to the menu of interest.</param>
    ''' <param name="ItemIndex">The index of the menu item of interest.</param>
    ''' <returns>Returns the screen bounds of the specified menu item.</returns>
    ''' <remarks>Throws an exception on error.</remarks>
    Private Function GetMenuItemRect(ByVal OwningWindowHandle As IntPtr, ByVal MenuHandle As IntPtr, ByVal ItemIndex As Integer) As Rectangle
        Dim pRemoteItem As IntPtr
        Try
            'Allocate buffer in remote memory for RECT structure
            Dim rectMi As RECT
            pRemoteItem = VirtualAllocEx(ProcessHandle,
          IntPtr.Zero, Marshal.SizeOf(rectMi), MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)

            If Not modWin32.GetMenuItemRect(OwningWindowHandle, MenuHandle, ItemIndex, pRemoteItem) Then
                Throw New InvalidOperationException(My.Resources.FailedToGetBoundsOfMenuItem)
            End If

            'Read out the new value of that structure
            Marshal.PtrToStructure(pRemoteItem, rectMi)
            Return rectMi
        Catch ex As Exception
            If pRemoteItem <> IntPtr.Zero Then
                VirtualFreeEx(ProcessHandle, pRemoteItem, 0, MEM_RELEASE)
            End If
        End Try
    End Function

    ''' <summary>
    ''' Gets the information about a menu item
    ''' </summary>
    ''' <param name="hMenu">A handle to the menu of interest.</param>
    Private Function GetMenuInfo(ByVal hMenu As IntPtr) As MENUINFO
        Dim mi As New MENUINFO
        Dim structSize As Integer = Marshal.SizeOf(mi)
        mi.cbSize = structSize
        mi.fMask = CInt(MIM.MIM_BACKGROUND Or MIM.MIM_HELPID Or MIM.MIM_MAXHEIGHT Or MIM.MIM_MENUDATA Or MIM.MIM_STYLE)

        Dim retVal As Integer = modWin32.GetMenuInfo(hMenu, mi)
        If retVal = 0 Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetMenuInfo0, GetLastWin32Error()))
        End If

        Return mi
    End Function

    ''' <summary>
    ''' Gets the text from a TabControl item.
    ''' </summary>
    ''' <param name="w">The clsUIWindow representing the tab control.</param>
    ''' <param name="ItemIndex">The zero-based index of the row in which the item
    ''' resides.</param>
    ''' <returns>Returns the text contained in the requested item. Throws an
    ''' Exception if the text could not be retrieved.</returns>
    Private Function GetTabControlItemText(ByVal w As clsUIWindow, ByVal ItemIndex As Integer) As String
        If w.ClassName = "SSTabCtlWndClass" Then
            If mHookClient Is Nothing Then Throw New ArgumentException(My.Resources.MustBeHookedToInteractWithSSTabControls)
            Dim sResult As String = Nothing
            Dim cmd As String = "property_get " & w.Handle.ToString("X") & ",TabCaption,~S" & ItemIndex.ToString()
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(String.Format(My.Resources.CouldnTGetTabText0, sResult))
            End If
            Return sResult
        Else
            Dim item As New TCITEM
            item.mask = TCIF.TCIF_TEXT
            Dim structSize As Integer = Marshal.SizeOf(item)

            'The following faff ensues because we don't know how
            'long the text is. We repeatedly retrieve the text
            'using buffers of increasing size until there is spare capacity
            'in the buffer allocated; there doesn't seem to more direct
            'way of finding out how large a buffer is necessary.
            Dim pRemoteItem As IntPtr
            Dim itemText As String
            Try
                pRemoteItem = VirtualAllocEx(ProcessHandle, IntPtr.Zero, Marshal.SizeOf(item), MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)

                Dim textLength As Integer = 128 'Reasonable initial length
                Dim returnedString As String
                Dim returnedTextLength As Integer
                Do
                    textLength *= 2 'Allocate twice as many bytes as last time
                    item.cchTextMax = textLength - 1 'Minus one to account for the terminating null byte

                    Dim textBuffer As IntPtr
                    Try
                        'Allocate a new text buffer to receive column text
                        textBuffer = VirtualAllocEx(ProcessHandle, IntPtr.Zero, textLength, MEM_COMMIT Or MEM_RESERVE, PAGE_READWRITE)
                        item.pszText = textBuffer

                        'Copy structure into remote memory
                        Marshal.DestroyStructure(pRemoteItem, GetType(TCITEM))
                        Dim pLocalItem As IntPtr = Marshal.AllocHGlobal(structSize)
                        Marshal.StructureToPtr(item, pLocalItem, False)
                        WriteProcessMemory(ProcessHandle, pRemoteItem, pLocalItem, structSize, IntPtr.Zero)
                        Marshal.FreeHGlobal(pLocalItem)

                        'Gets the text up to the max length specified
                        Dim result As Integer = SendMessage(w.Handle, WindowMessages.TCM_GETITEMA, ItemIndex, pRemoteItem).ToInt32()
                        If result = 0 Then
                            Throw New InvalidOperationException(String.Format(My.Resources.APICallFailedWhilstRetrievingTextForTab01, (ItemIndex + 1).ToString, GetLastWin32Error()))
                        End If

                        'Read the string out of the remote text buffer
                        If item.pszText <> IntPtr.Zero Then
                            Dim pLocalText As IntPtr = Marshal.AllocHGlobal(textLength)
                            ReadProcessMemory(ProcessHandle, item.pszText, pLocalText, textLength, IntPtr.Zero)
                            returnedString = Marshal.PtrToStringAnsi(pLocalText)
                            Marshal.FreeHGlobal(pLocalText)
                        Else
                            returnedString = String.Empty
                            Exit Do
                        End If

                        returnedTextLength = returnedString.Length
                    Finally
                        If textBuffer <> IntPtr.Zero Then
                            VirtualFreeEx(ProcessHandle, textBuffer, 0, MEM_RELEASE)
                        End If
                    End Try

                    'We only quit the loop once we are sure that the buffer
                    'allocated is sufficiently large to contain the whole text
                Loop While (returnedTextLength = item.cchTextMax)

                itemText = returnedString

            Catch ex As InvalidOperationException
                Throw New InvalidOperationException(String.Format(My.Resources.ExceptionWhilstRetrievingTextForItem0, (ItemIndex + 1).ToString))
            Finally
                If pRemoteItem <> IntPtr.Zero Then
                    VirtualFreeEx(ProcessHandle, pRemoteItem, 0, MEM_RELEASE)
                End If
            End Try

            Return itemText
        End If
    End Function

    ''' <summary>
    ''' Gets the command ID of a toolbar button, by its index
    ''' </summary>
    ''' <param name="ToolbarHandle">A handle to the toolbar window of interest</param>
    ''' <param name="ItemIndex">The index of the button of interest within the toolbar</param>
    ''' <returns>Returns the command id of the button of interest.</returns>
    Private Function GetToolbarItemCommandID(ByVal ToolbarHandle As IntPtr, ByVal ItemIndex As Integer) As Integer
        'Get the command id for the button
        Dim info As New TBBUTTONINFO
        info.cbSize = Marshal.SizeOf(info)
        info.dwMask = TBIFlags.TBIF_BYINDEX Or TBIFlags.TBIF_COMMAND
        Dim rpms As New RemoteProcessMessageSender(Of TBBUTTONINFO, TBBUTTONINFO)
        rpms.MessageToSend = WindowMessages.TB_GETBUTTONINFOW
        rpms.Handle = ToolbarHandle
        rpms.wParam = ItemIndex
        rpms.InputValues = New TBBUTTONINFO() {info}
        rpms.OutputValues = New TBBUTTONINFO() {New TBBUTTONINFO}
        Dim retval = rpms.SendMessage(ProcessHandle)

        If retval.ToInt32() = -1 Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveCommandIDFromToolbarButtonItemWithZeroBasedIndex0LastWin32Error, ItemIndex, Marshal.GetLastWin32Error.ToString))
        Else
            Return rpms.OutputValues(0).idCommand
        End If
    End Function

    ''' <summary>
    ''' Gets the state of a toolbar button on a standard windows toolbar control.
    ''' </summary>
    ''' <param name="ToolbarHandle">Handle to the toolbar of interest</param>
    ''' <param name="ItemIndex">Index of the button of interest</param>
    ''' <returns>Returns the state of the requested button.</returns>
    ''' <remarks>Throws an exception in the event of an error</remarks>
    Private Function GetToolbarButtonState(ByVal ToolbarHandle As IntPtr, ByVal ItemIndex As Integer) As TBSTATE
        Dim bt As TBBUTTON
        Dim rpms As New RemoteProcessMessageSender(Of TBBUTTON, TBBUTTON)
        rpms.Handle = ToolbarHandle
        rpms.MessageToSend = WindowMessages.TB_GETBUTTON
        rpms.wParam = ItemIndex
        rpms.InputValues = New TBBUTTON() {bt}
        rpms.OutputValues = New TBBUTTON() {New TBBUTTON}
        Dim retval = rpms.SendMessage(ProcessHandle)
        If retval.ToInt32() = 0 Then
            Throw New InvalidOperationException(My.Resources.FailedToRetrieveButtonInformationLastWin32ErrorMessageWas & Marshal.GetLastWin32Error)
        Else
            Return CType(rpms.OutputValues(0).fsState, TBSTATE)
        End If
    End Function

    ''' <summary>
    ''' Gets the bounds of the specified item from a win32 toolbar control.
    ''' </summary>
    ''' <param name="ToolbarHandle">The handle of the toolbar of interest</param>
    ''' <param name="ItemIndex">The index of the toolbar button of interest.</param>
    ''' <returns>Returns the bounds of the clickable button, relative to its
    ''' parent toolbar control.</returns>
    Private Function GetToolbarButtonBounds(ByVal ToolbarHandle As IntPtr, ByVal ItemIndex As Integer) As RECT
        Dim buttonRect As RECT
        Dim rectRpms As New RemoteProcessMessageSender(Of RECT, RECT)
        rectRpms.Handle = ToolbarHandle
        rectRpms.MessageToSend = WindowMessages.TB_GETITEMRECT
        rectRpms.wParam = ItemIndex
        rectRpms.InputValues = New RECT() {buttonRect}
        rectRpms.OutputValues = New RECT() {}
        ReDim rectRpms.OutputValues(0)
        rectRpms.SendMessage(ProcessHandle)

        Return rectRpms.OutputValues(0)
    End Function

    ''' <summary>
    ''' Sets the visibility flag of a toolbar button
    ''' </summary>
    ''' <param name="ToolbarHandle">The handle of the toolbar of interest</param>
    ''' <param name="ButtonIndex">Thezero-based index of the button of interest</param>
    ''' <param name="Visible">When true, the button of interest will be made visible, otherwise
    ''' it will be hidden</param>
    ''' <param name="sErr">Carries back a message in the event of an error</param>
    ''' <returns>Returns true on success, false otherwise.</returns>
    Private Function SetToolbarButtonVisible(ByVal ToolbarHandle As IntPtr, ByVal ButtonIndex As Integer, ByVal Visible As Boolean, ByRef sErr As String) As Boolean
        Dim buttonCount As Integer = SendMessage(ToolbarHandle, WindowMessages.TB_BUTTONCOUNT, 0, 0).ToInt32()
        If ButtonIndex < 0 OrElse ButtonIndex >= buttonCount Then
            sErr = String.Format(My.Resources.InvalidToolbarButtonIndex0ButtonCountIs1, ButtonIndex.ToString, buttonCount.ToString)
            Return False
        End If

        Dim cmdID As Integer
        Try
            cmdID = Me.GetToolbarItemCommandID(ToolbarHandle, ButtonIndex)
        Catch ex As Exception
            sErr = My.Resources.CouldNotResolveButtonCommandId & ex.Message
        End Try
        Dim retval As Integer = SendMessage(ToolbarHandle, WindowMessages.TB_HIDEBUTTON, cmdID, CInt(Visible)).ToInt32()
        If retval = 0 Then
            sErr = My.Resources.FailedToSetToolbarButtonVisibilityLastWin32ErrorWas & Marshal.GetLastWin32Error
        End If

        Return True
    End Function

    ''' <summary>
    ''' Gets the text from a toolbar button
    ''' </summary>
    ''' <param name="ToolbarHandle">Handle to the toolbar</param>
    ''' <param name="ButtonIndex">The zero-based index of the button of interest</param>
    ''' <returns>Returns the text of the button of interest, or throws
    ''' an exception in the event of an error</returns>
    ''' <remarks>This works well for the windows taskbar (when you supply an appropriate
    ''' handle to the process owning the toolbar). Does not seem to work for every single
    ''' toolbar, but this may be because the toolbar buttons do not have any text set.</remarks>
    Private Function GetToolbarButtonText(ByVal ToolbarHandle As IntPtr, ByVal ButtonIndex As Integer) As String
        Dim item As TBBUTTON
        Const MaxTextLength As Integer = 256 'Lazy fixed value because docs are not clear how to get length, or what max value is. Seems to be 80 however.

        'Ask for button info
        Dim rpms As New RemoteProcessMessageSender(Of TBBUTTON, TBBUTTON)
        rpms.MessageToSend = WindowMessages.TB_GETBUTTON
        rpms.Handle = ToolbarHandle
        rpms.wParam = ButtonIndex
        rpms.InputValues = New TBBUTTON() {item}
        rpms.OutputValues = New TBBUTTON() {New TBBUTTON}
        Dim retval = rpms.SendMessage(ProcessHandle)
        If retval.ToInt32() = 0 Then
            Throw New InvalidOperationException(My.Resources.CouldNotGetToolbarButtonInfoLastWin32ErrorWas & Marshal.GetLastWin32Error)
        End If

        'Inspect button info for string value
        Dim remoteTextPtr As IntPtr
        Dim selfAllocated As Boolean = False
        Try
            Dim returnItem As TBBUTTON = rpms.OutputValues(0)
            If modWin32.IS_INTRESOURCE(returnItem.iString) Then
                remoteTextPtr = VirtualAllocEx(ProcessHandle, IntPtr.Zero, Marshal.SizeOf(item), MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)
                selfAllocated = True
                retval = SendMessage(ToolbarHandle, WindowMessages.TB_GETSTRINGA, MakeLong(MaxTextLength, CShort(returnItem.iString)), remoteTextPtr)
                If retval.ToInt32() = -1 Then
                    Throw New InvalidOperationException(My.Resources.FailedToResolveIntegerStringResourceLastWin32ErrorMessageWas & Marshal.GetLastWin32Error)
                End If
            Else
                remoteTextPtr = New IntPtr(returnItem.iString)
            End If


            Return Me.GetRemoteNullTerminatedUnicodeString(remoteTextPtr, ProcessHandle, MaxTextLength)
        Finally
            If selfAllocated AndAlso remoteTextPtr <> IntPtr.Zero Then
                VirtualFreeEx(ProcessHandle, remoteTextPtr, 0, MEM_RELEASE)
            End If
        End Try
    End Function

    ''' <summary>
    ''' Reads a null-terminated unicode string from remote memory.
    ''' </summary>
    ''' <param name="pRemoteText">A pointer to the string located in another
    ''' process' memory space.</param>
    ''' <param name="ProcessHandle">An open handle to the process owning the
    ''' memory location. This handle must have the relevant permissions (eg PAGE_READ etc).
    ''' It is the caller's responsibility to release this handle.</param>
    ''' <param name="MaxTextLength">The maximum anticipated length of the text.</param>
    ''' <returns>Returns the string read if successful. Throws an exception, or returns an
    ''' empty string in the event of an error.</returns>
    Private Function GetRemoteNullTerminatedUnicodeString(ByVal pRemoteText As IntPtr, ByVal ProcessHandle As IntPtr, Optional ByVal MaxTextLength As Integer = Integer.MaxValue) As String
        Dim stringToReturn As New StringBuilder
        Dim pLocalText As IntPtr
        Try
            pLocalText = Marshal.AllocHGlobal(2)
            Dim offset As Integer = 0
            Dim bytesRead(1) As Byte
            Dim fragmentRead As String
            Do
                Dim retval As Integer = ReadProcessMemory(ProcessHandle, New IntPtr(pRemoteText.ToInt64 + offset), pLocalText, 2, IntPtr.Zero)
                If retval > 0 Then
                    bytesRead(0) = Marshal.ReadByte(pLocalText, 0)
                    bytesRead(1) = Marshal.ReadByte(pLocalText, 1)
                    fragmentRead = Encoding.Unicode.GetString(bytesRead)
                    offset += 2
                Else
                    fragmentRead = ""
                End If
                stringToReturn.Append(fragmentRead)
            Loop While offset < MaxTextLength AndAlso Not String.IsNullOrEmpty(fragmentRead)
        Finally
            If pLocalText <> IntPtr.Zero Then
                Marshal.FreeHGlobal(pLocalText)
            End If
        End Try

        Return stringToReturn.ToString
    End Function

    <Category(Category.Win32)>
    <Command("Gets all selected items from a control, as a collection.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:"" followed by some Automate collection xml, or error message.")>
    Private Function ProcessCommandGetSelectedItems(ByVal objQuery As clsQuery) As Reply
        'Count the number of selected items in the control
        Dim cntReply As Reply = Me.ProcessCommandGetSelectedItemCount(objQuery)
        Dim itemCount As Integer
        If cntReply.IsResult Then
            If Not Integer.TryParse(cntReply.Message, itemCount) Then
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretResultOf0AsTheNumberOfRowsInTheListview, cntReply.Message))
            End If
        Else
            Throw New InvalidOperationException(My.Resources.FailedToCountTheNumberOfItemsInTheControl & cntReply.Message)
        End If

        'An array of integers indicating which items are selected
        Dim selectedIndices As Integer()

        'Decide what message is needed for getting the items
        Dim ew As clsUIWindow = mobjModel.IdentifyWindow(objQuery)
        Dim getTextMessage As WindowMessages
        Dim getLengthMessage As WindowMessages
        Select Case True
            Case ew.ClassName.Contains("ListBox") OrElse ew.ClassName.Contains("LISTBOX")
                If IsListboxMultiSelection(ew.Handle) Then
                    ReDim selectedIndices(itemCount - 1)
                    Dim rpms As New RemoteProcessMessageSender(Of Integer, Integer)
                    rpms.InputValues = Nothing
                    rpms.OutputValues = selectedIndices
                    rpms.MessageToSend = WindowMessages.LB_GETSELITEMS
                    rpms.wParam = itemCount
                    rpms.Handle = ew.Handle
                    rpms.SendMessage(ProcessHandle)
                    If rpms.Success Then
                        selectedIndices = rpms.OutputValues
                    Else
                        Throw New InvalidOperationException(My.Resources.FailedToRetrieveIndicesOfSelectedItems)
                    End If
                Else
                    ReDim selectedIndices(0)
                    Dim result As Integer = SendMessage(ew.Handle, WindowMessages.LB_GETCURSEL, 0, 0).ToInt32()
                    If result = LB.LB_ERR Then
                        selectedIndices = Nothing
                    Else
                        selectedIndices(0) = result
                    End If
                End If
                getTextMessage = WindowMessages.LB_GETTEXT
                getLengthMessage = WindowMessages.LB_GETTEXTLEN

            Case ew.ClassName.Contains("SysListView32")
                Return Reply.Result(Me.GetListviewItems(ew.Handle, True))

            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.GetSelectedItemsIsNotImplementedForControlsWithClassname0, ew.ClassName))
        End Select

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)

        'Get each item in turn and add it to the collection xml
        If selectedIndices IsNot Nothing Then
            For Each itemIndex As Integer In selectedIndices
                Dim ItemText As String = Nothing
                ItemText = GetItemText(ew.Handle, getLengthMessage, getTextMessage, itemIndex)

                'Add data to xml collection
                Dim rowElement As XmlElement = xdoc.CreateElement("row")
                rowElement.AppendChild(CreateCollectionFieldXML(xdoc, ItemText, "text", "Item Text"))
                collectionRoot.AppendChild(rowElement)
            Next
        End If

        Return Reply.Result(xdoc.OuterXml)
    End Function

    ''' <summary>
    ''' Convert some one-dimensional tab-separated data into an Automate collection.
    ''' </summary>
    ''' <param name="data">The input data, tab-separated. The first entry is the item
    ''' count. This must be followed by that many entries of actual data.</param>
    ''' <param name="columnName">The name of the single collection column into which
    ''' all the data will be inserted, one row per item.</param>
    ''' <returns>The collection data in XML format.</returns>
    Private Function TabData1DToCollectionXML(ByVal data As String, ByVal columnName As String) As String
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)
        Dim dat As String() = data.Split(Chr(9))
        Dim numrows As Integer = Integer.Parse(dat(0))
        For row As Integer = 1 To numrows
            Dim rowel As XmlElement = xdoc.CreateElement("row")
            rowel.AppendChild(CreateCollectionFieldXML(xdoc, dat(row), "text", columnName))
            root.AppendChild(rowel)
        Next
        Return xdoc.OuterXml

    End Function

    <Category(Category.Win32)>
    <Command("Ensures that a named node in an ActiveX TreeView is visible, scrolling and expanding if necessary.")>
    <Parameters("NewText specifies the text of the node, plus those required to uniquely identify the window.")>
    <Response("""RESULT:OK"" , or error message.")>
    Private Function ProcessCommandAxTreeViewEnsureVisible(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim nodeText As String = objQuery.GetParameter(ParameterNames.NewText)
        If nodeText Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoNodeTextUseNewtextParameter)
        End If

        If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractWithAnActiveXTreeViewUnlessHooked)
        Dim cmd As String = "treeview_ensurevisible " & w.Handle.ToString("X") & "," & nodeText
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Select a named node in an ActiveX TreeView.")>
    <Parameters("NewText specifies the text of the node, plus those required to uniquely identify the window.")>
    <Response("""RESULT:OK"" , or  error message.")>
    Private Function ProcessCommandAxTreeViewSelectNode(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim nodeText As String = objQuery.GetParameter(ParameterNames.NewText)
        If nodeText Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoNodeTextUseNewtextParameter)
        End If

        If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractWithAnActiveXTreeViewUnlessHooked)
        Dim cmd As String = "treeview_select " & w.Handle.ToString("X") & "," & nodeText
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Gets all the children of a node from a TreeView control, as a collection.")>
    <Parameters("NewText specifies the text of the node from which the children are retrieved, plus those required to uniquely identify the window.")>
    <Response("""RESULT:"" followed by some Automate collection xml, or  error message.")>
    Private Function ProcessCommandGetTreenodeChildItems(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Try
            Select Case True
                Case w.ClassName.Contains("SysTreeView32")
                    Dim nodeHandle = GetTreenodeHandle(w.Handle, searchText)
                    If nodeHandle <> IntPtr.Zero Then
                        Dim firstChildNode = SendMessage(w.Handle, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_CHILD, nodeHandle)
                        If firstChildNode <> IntPtr.Zero Then
                            Return DoGetTreenodeSiblingItems(w.Handle, firstChildNode)
                        Else
                            'return empty collection
                            Return Reply.Result("<collection />")
                        End If
                    Else
                        Throw New InvalidOperationException(String.Format(My.Resources.FailedToFindTreeNodeWithText0, searchText))
                    End If

                Case w.ClassName = "TreeView20WndClass"
                    If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractWithAnActiveXListViewUnlessHooked)
                    Dim cmd As String = "treeview_children " & w.Handle.ToString("X") & "," & searchText
                    Dim sResult As String = Nothing
                    If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
                    Dim col As String = TabData1DToCollectionXML(sResult, "Node Text")
                    Return Reply.Result(col)

                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.GetAllChildItemsIsNotImplementedForControlsWithClassname0, w.ClassName))
            End Select

        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstAnalysingElement & ex.Message)
        End Try
    End Function

    <Category(Category.Win32)>
    <Command("Gets all treeview sibling items from a control, as a collection.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""RESULT:"" followed by some Automate collection xml, or  error message.")>
    Private Function ProcessCommandGetTreenodeSiblingItems(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Get the text we are searching for...
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoSearchTextSpecifiedUseNewtextParameter)
        End If

        Try
            Select Case True

                Case w.ClassName.Contains("SysTreeView32")
                    Dim nodeHandle = GetTreenodeHandle(w.Handle, searchText)
                    If nodeHandle <> IntPtr.Zero Then
                        Return DoGetTreenodeSiblingItems(w.Handle, nodeHandle)
                    Else
                        Throw New InvalidOperationException(String.Format(My.Resources.FailedToFindTreeNodeWithText0, searchText))
                    End If

                Case w.ClassName = "TreeView20WndClass"
                    If mHookClient Is Nothing Then Throw New InvalidOperationException(My.Resources.CanTInteractWithAnActiveXListViewUnlessHooked)
                    Dim cmd As String = "treeview_siblings " & w.Handle.ToString("X") & "," & searchText
                    Dim sResult As String = Nothing
                    If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
                    Dim col As String = TabData1DToCollectionXML(sResult, "Node Text")
                    Return Reply.Result(col)

                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.GetAllChildItemsIsNotImplementedForControlsWithClassname0, w.ClassName))
            End Select

        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstAnalysingElement & ex.Message)
        End Try
    End Function

    Private Function DoGetTreenodeSiblingItems(ByVal treeviewHandle As IntPtr, ByVal siblingNodeHandle As IntPtr) As Reply
        'Get the handle of the first sibling
        Dim currentSearchHandle = siblingNodeHandle
        Do
            Dim nextNodeHandle = SendMessage(treeviewHandle, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_PREVIOUS, currentSearchHandle)
            If nextNodeHandle.ToInt64() = 0 Then
                Exit Do
            Else
                currentSearchHandle = nextNodeHandle
            End If
        Loop
        Dim firstNodeHandle = currentSearchHandle

        'Prepare the xml document
        Dim xdoc As New XmlDocument()
        Dim collectionRoot As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(collectionRoot)

        'Iterate through the items adding to collection
        Dim currentNode = firstNodeHandle
        While currentNode <> IntPtr.Zero
            Dim rowElement As XmlElement = xdoc.CreateElement("row")
            Dim nodeText As String = Me.GetTreenodeItem(treeviewHandle, currentNode).Text
            rowElement.AppendChild(CreateCollectionFieldXML(xdoc, nodeText, "text", "Node Text"))
            collectionRoot.AppendChild(rowElement)

            currentNode = SendMessage(treeviewHandle, WindowMessages.TVM_GETNEXTITEM, TreeviewNextItemFlags.TVGN_NEXT, currentNode)
        End While

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Win32)>
    <Command("Sets the check state of a Window.")>
    <Parameters("The check state specified by 'NewText' and those required to uniquely identify the window.")>
    Private Function ProcessCommandSetChecked(ByVal objQuery As clsQuery) As Reply
        Dim checked As Boolean = Boolean.Parse(objQuery.GetParameter(ParameterNames.NewText))
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        If checked <> w.Checked Then
            Dim wparam As Integer = CInt(IIf(checked, ButtonCheckStates.BST_CHECKED, ButtonCheckStates.BST_UNCHECKED))
            SendMessage(w.Handle, WindowMessages.BM_SETCHECK, wparam, 0)
        End If
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Sets the text for a given Control. This operates on Windows Controls, e.g. Edit Controls. The text is set by sending a WM_SETTEXT message.")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' to specify the new text for the Control.")>
    Private Function ProcessCommandSetText(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim sNewText As String = objQuery.GetParameter(ParameterNames.NewText)
        SendMessageString(w.Handle, WindowMessages.WM_SETTEXT, 0, sNewText)
        ' Multiline textboxes don't send an EN_CHANGE message themselves when set with a
        ' WM_SETTEXT message, meaning that some events (eg. TextChanged) are never fired
        ' on a text change - so we force an EN_CHANGE message in ourselves.
        SendMessage(w.Hwnd, WindowMessages.WM_COMMAND,
         EditControlNotifCode.EN_CHANGE << 16, w.Handle)
        Return Reply.Ok
    End Function

    <Category(Category.Win32),
     Command(
         "Sets the password in a given Control. " &
         "This works on Win32 TextBoxes and it ensures that the password is " &
         "not loaded as plaintext into managed memory."),
     Parameters(
         "Those required to uniquely identify the window, plus 'newtext' " &
         "to specify the obfuscated password for the Control.")>
    Private Function ProcessCommandSetPasswordText(q As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        SendMessage(w.Hwnd, WindowMessages.WM_SETTEXT, 0,
            SafeString.Decode(q.GetParameter(ParameterNames.NewText)))

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Asks a window to close, using the WM_CLOSE message.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandCloseWindow(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        PostMessage(w.Handle, WindowMessages.WM_CLOSE, 0, 0)

        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Window enumeration callback used when looking for windows to
    ''' attach to.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window under inspection.</param>
    ''' <param name="info">Contains matching targets and stores result data</param>
    ''' <returns>True - always continuing the enumeration</returns>
    Private Function EnumWindowsMatchCallback(hWnd As IntPtr, info As clsEnumWindowsInfo) As Boolean
        'Get name of process owning this window
        Dim processID As Integer
        GetWindowThreadProcessId(hWnd, processID)

        If Not info.AllProcesses.ContainsKey(processID) Then
            GetProcessInfoByProcessId(info.UseWMI, processID,
            Sub(id, name, userFactory)
                info.AllProcesses.Add(id, New Tuple(Of String, Func(Of String))(name, userFactory))
            End Sub)
        End If

        Dim tuple = info.AllProcesses(processID)
        Dim processName As String = tuple.Item1
        Dim userNameFactory As Func(Of String) = tuple.Item2

        If processName = "ApplicationFrameHost" Then
            'If this process is a container app frame host then enumerate its child windows
            EnumChildWindows(hWnd, AddressOf EnumWindowsMatchCallback, info)
        Else
            Dim processNameMatcher = TryCast(info.ProcessNameMatcher, clsMatchTarget)
            Dim userNameMatcher = TryCast(info.UsernameMatcher, clsMatchTarget)
            If processNameMatcher Is Nothing OrElse processNameMatcher.IsMatch(processName) Then

                Dim windowTitleMatcher = TryCast(info.TitleMatcher, clsMatchTarget)
                If windowTitleMatcher Is Nothing AndAlso
                    (userNameMatcher Is Nothing OrElse userNameMatcher.IsMatch(userNameFactory.Invoke())) Then
                    info.MatchResults.Add(processID)
                Else
                    'Get Window Text
                    Dim len As Integer = 2 * GetWindowTextLength(hWnd)
                    Dim sb As New StringBuilder(len)
                    GetWindowText(hWnd, sb, len)
                    Dim windowTitle = sb.ToString

                    'Test windowtext for match against our criteria
                    If windowTitleMatcher.IsMatch(windowTitle) AndAlso
                        (userNameMatcher Is Nothing OrElse userNameMatcher.IsMatch(userNameFactory.Invoke())) Then
                        info.MatchResults.Add(processID)
                    End If
                End If
            End If
        End If

        Return True
    End Function

    <Category(Category.Win32)>
    <Command("Similar to StartApplication, but 'attaches' to an already running application. The application can be identified by process name, a top-level window title, or both." & vbCrLf & "One or both of the processname and windowtitle parameters must be specified to identify the application. ")>
    <Parameters("processname - the process name, windowtitle - the window title, Optionally, hook - True or False(default) to determine whether to hook the target, optionally jab - True or False(default) to determine whether to use Java Access Bridge or not, and hook - True or False(default) to determine whether to hook the target application or not, and 'options' as a comma-separated list of additional (application type specific options) to set.")>
    <Examples("attachapplication windowtitle=""Account Management""" & vbCrLf & "attachapplication processname=iexplore.exe windowtitle=""Hello - Microsoft Internet Explorer""")>
    Private Function ProcessCommandAttachApplication(ByVal objQuery As clsQuery) As Reply
        'Determine if we will be using JAB and hooks or not...
        Dim useJab = False
        Dim useHooks = False
        Dim browserAttach = False
        Dim tabTitleSearch = String.Empty
        Dim trackingId = String.Empty
        Dim s = objQuery.GetParameter(ParameterNames.Hook)
        If s IsNot Nothing AndAlso Boolean.Parse(s) Then useHooks = True
        s = objQuery.GetParameter(ParameterNames.JAB)
        If s IsNot Nothing AndAlso Boolean.Parse(s) Then useJab = True

        Dim options = ""
        s = objQuery.GetParameter(ParameterNames.Options)
        If s IsNot Nothing Then options = s

        s = objQuery.GetParameter(ParameterNames.ExcludeHTC)
        If Not Boolean.TryParse(s, mExcludeHTC) Then
            mExcludeHTC = False
        End If

        s = objQuery.GetParameter(ParameterNames.ActiveTabOnly)
        If Not Boolean.TryParse(s, mActiveTabOnly) Then
            mActiveTabOnly = True
        End If

        s = objQuery.GetParameter(ParameterNames.BrowserAttach)
        If s IsNot Nothing AndAlso Boolean.Parse(s) Then
            browserAttach = True

            s = objQuery.GetParameter(ParameterNames.TrackingId)
            If s IsNot Nothing Then trackingId = s
        End If

        'If we are attaching by pid then don't bother using any of the
        'other match criteria since pid is unique
        Dim uniquePID = objQuery.GetIntParam(ParameterNames.ProcessID, True)
        If uniquePID > 0 Then
            DoAttach(uniquePID, useJab, useHooks, options)
            Return Reply.Ok
        End If

        'Process matcher for process user names
        Dim useWmi = False
        Dim userNameMatcher = objQuery.GetIdentifier(clsQuery.IdentifierTypes.Username)
        If userNameMatcher IsNot Nothing Then
            useWmi = True
            userNameMatcher.ComparisonType = clsMatchTarget.ComparisonTypes.Wildcard
        End If

        'Process matcher for process names
        Dim processNameMatcher = objQuery.GetIdentifier(clsQuery.IdentifierTypes.ProcessName)
        If processNameMatcher IsNot Nothing Then
            processNameMatcher.ComparisonType = clsMatchTarget.ComparisonTypes.Wildcard
        End If

        'Process matcher for window titles
        Dim windowTitleMatcher = objQuery.GetIdentifier(clsQuery.IdentifierTypes.WindowTitle)

        Dim matchResults As New HashSet(Of Integer)
        If windowTitleMatcher Is Nothing Then
            ForEachProcess(useWmi,
                Sub(id, name, userNameFactory)
                    If (userNameMatcher Is Nothing OrElse userNameMatcher.IsMatch(userNameFactory())) AndAlso
                        (processNameMatcher Is Nothing OrElse processNameMatcher.IsMatch(name)) Then
                        matchResults.Add(id)
                    End If
                End Sub)
        Else
            'Search for processes which have window names which match our criteria
            windowTitleMatcher.ComparisonType = clsMatchTarget.ComparisonTypes.Wildcard

            Dim allProcesses As New Dictionary(Of Integer, Tuple(Of String, Func(Of String)))
            ForEachProcess(useWmi,
                Sub(id, name, userNameFactory)
                    allProcesses.Add(id, Tuple.Create(name, userNameFactory))
                End Sub)

            EnumWindows(AddressOf EnumWindowsMatchCallback,
                        New clsEnumWindowsInfo With {
                            .TitleMatcher = windowTitleMatcher,
                            .ProcessNameMatcher = processNameMatcher,
                            .UsernameMatcher = userNameMatcher,
                            .AllProcesses = allProcesses,
                            .MatchResults = matchResults,
                            .UseWMI = useWmi
                        })
            If browserAttach Then
                tabTitleSearch = windowTitleMatcher.MatchValue
            End If
        End If

        'Choose a process given a childindex parameter
        s = objQuery.GetParameter(ParameterNames.ChildIndex)
        If s IsNot Nothing Then
            Dim childIndex = Integer.Parse(s)

            Dim chosenProcess As Process = Nothing
            For Each p In matchResults
                Dim children = Me.GetChildProcesses(p)
                If childIndex > 0 AndAlso children.Count >= childIndex Then
                    chosenProcess = children(childIndex - 1)
                    Exit For
                ElseIf childIndex = 0 AndAlso children.Count > 0 Then
                    chosenProcess = Process.GetProcessById(p)
                    Exit For
                End If
            Next
            If Not chosenProcess Is Nothing Then
                DoAttach(chosenProcess.Id, useJab, useHooks, options, tabTitleSearch, trackingId)
                Return Reply.Ok
            End If
        End If

        'Check we have a unique match
        Select Case matchResults.Count
            Case 0
                Throw GetNoMatchedApplicationsException(windowTitleMatcher, processNameMatcher, userNameMatcher)
            Case 1
                uniquePID = matchResults.First
            Case Else
                Throw New InvalidOperationException(My.Resources.MoreThanOneApplicationMatchedTheCriteria)
        End Select

        If uniquePID = 0 Then
            Throw New InvalidOperationException(My.Resources.TargetApplicationCouldNotBeIdentified)
        End If

        DoAttach(uniquePID, useJab, useHooks, options, tabTitleSearch, trackingId)

        Return Reply.Ok
    End Function

    Private Function GetNoMatchedApplicationsException(windowTitleMatcher As clsIdentifierMatchTarget, processNameMatcher As clsIdentifierMatchTarget,
                                                       userNameMatcher As clsIdentifierMatchTarget) As ApplicationException
        Dim applicationMatcherValidationMessage = GetApplicationMatcherValidationMessage(windowTitleMatcher, processNameMatcher, userNameMatcher)

        Return New ApplicationException(String.Format(My.Resources.TargetApplicationCouldNotBeIdentifiedWithMatchers, applicationMatcherValidationMessage))
    End Function

    Private Shared Function GetApplicationMatcherValidationMessage(windowTitleMatcher As clsIdentifierMatchTarget, processNameMatcher As clsIdentifierMatchTarget,
                                                                   userNameMatcher As clsIdentifierMatchTarget) As String
        Dim windowTitleMatcherSupplied = windowTitleMatcher IsNot Nothing
        Dim processNameMatcherSupplied = processNameMatcher IsNot Nothing
        Dim userNameMatcherSupplied = userNameMatcher IsNot Nothing

        Dim windowTitleMatcherMessage = If(windowTitleMatcherSupplied, windowTitleMatcher.Identifier.ToString(), String.Empty)
        Dim processNameMatcherMessage = If(processNameMatcherSupplied, processNameMatcher.Identifier.ToString(), String.Empty)
        Dim userNameMatcherMessage = If(userNameMatcherSupplied, userNameMatcher.Identifier.ToString(), String.Empty)

        Dim anyMatchersPopulated = windowTitleMatcherSupplied OrElse processNameMatcherSupplied OrElse userNameMatcherSupplied

        Return If(anyMatchersPopulated,
                   String.Format(My.Resources.ReviewMatchers,
                                 String.Join(", ", {windowTitleMatcherMessage, processNameMatcherMessage, userNameMatcherMessage}.
                                 Where(Function(matcher) Not String.IsNullOrEmpty(matcher)))), String.Empty)
    End Function

    ''' <summary>
    ''' Applies and action for each process in the entire list of processes
    ''' username is given in the format Domain\User
    ''' </summary>
    Private Shared Sub ForEachProcess(useWmi As Boolean, action As Action(Of Integer, String, Func(Of String)))
        Try
            If useWmi Then
                Dim searcher As New ManagementObjectSearcher("Select * From Win32_Process")

                For Each obj As ManagementObject In searcher.Get

                    Dim managementObject = obj
                    Dim processName = CStr(managementObject.GetPropertyValue("Name"))
                    Dim id = CInt(managementObject.GetPropertyValue("ProcessID"))

                    action.Invoke(id, processName, Function() GetManagementObjectUsername(managementObject))
                Next
            Else
                For Each p In Process.GetProcesses
                    action.Invoke(p.Id, p.ProcessName, Function() String.Empty)
                Next
            End If
        Catch
            'If this goes wrong do nothing
        End Try
    End Sub

    Private Shared Function GetManagementObjectUsername(obj As ManagementObject) As String
        Dim userName = String.Empty
        Dim args = {String.Empty, String.Empty}

        Dim getOwnerResultCount = CInt(obj.InvokeMethod("GetOwner", args))

        If getOwnerResultCount = 0 Then userName = String.Format("{1}\{0}", args)

        Return userName
    End Function

    Private Shared Sub GetProcessInfoByProcessId(useWmi As Boolean, processId As Integer, action As Action(Of Integer, String, Func(Of String)))
        Try
            If useWmi Then
                Dim searcher As New ManagementObjectSearcher($"Select * From Win32_Process Where ProcessId = {processId} ")
                Dim processResult = searcher.Get.OfType(Of ManagementObject).SingleOrDefault()

                'make sure single process returned'
                If processResult IsNot Nothing Then
                    Dim processName = CStr(processResult.GetPropertyValue("Name"))
                    Dim id = CInt(processResult.GetPropertyValue("ProcessID"))

                    action.Invoke(id, processName, Function() GetManagementObjectUsername(processResult))
                End If
            Else
                Dim process As Process = Process.GetProcessById(processId)
                action.Invoke(process.Id, process.ProcessName, Function() String.Empty)
            End If
        Catch
            'If this goes wrong do nothing
        End Try
    End Sub
    Private Function GetChildProcesses(ByVal pid As Integer) As IList(Of Process)
        Dim children As New SortedList(Of DateTime, Process)
        For Each p As Process In Process.GetProcesses
            Try
                Dim id As Integer = modWin32.GetParentProcessID(p.Handle)
                If id = pid Then
                    children.Add(p.StartTime, p)
                End If
            Catch
            End Try
        Next

        Return children.Values
    End Function

    ''' <summary>
    ''' Attaches to the process with the supplied Process ID.
    ''' </summary>
    ''' <param name="pid">The ID of the process to be attached to.</param>
    ''' <param name="useJab">True if Java Access Bridge support is required.</param>
    ''' <param name="useHooks">True if the application is to be hooked.</param>
    ''' <param name="opts">A comma-separated list of options to set, which are
    ''' generally application-type specific. An empty string if there are none.</param>
    Private Sub DoAttach(ByVal pid As Integer, ByVal useJab As Boolean, ByVal useHooks As Boolean, ByVal opts As String, Optional tabTitleSearch As String = "", Optional trackingId As String = "")
        SyncLock mApplicationStateLock
            If Connected Then
                Throw New InvalidOperationException(My.Resources.AlreadyLaunched)
            End If

            Dim browserType = CheckBrowserAndSetupAutomation(pid)

            'Set up the 'connection' to the target application, leaving things in a
            'similar state to if we had called Launch()

            'Initialise JAB support...
            Dim sErr As String = Nothing
            If Not InitJab(useJab, sErr) Then Throw New InvalidOperationException(sErr)
            Dim processHandle As IntPtr = OpenProcess(ProcessAccess.ATTACH_RIGHTS, False, pid)
            If processHandle = IntPtr.Zero Then
                Throw New InvalidOperationException(My.Resources.CouldNotOpenTargetProcess)
            End If
            mPID = pid

            'Determine if we've attached to a container app
            If Process.GetProcessById(mPID).ProcessName = "ApplicationFrameHost" Then
                IsModernApp = True
            Else
                For Each p As Process In Process.GetProcessesByName("ApplicationFrameHost")
                    Dim info As New clsEnumWindowsInfo()
                    info.TargetPID = p.Id
                    info.Tag = mPID
                    foundPID = False
                    EnumWindows(AddressOf EnumHostedWindows, info)
                    If foundPID Then
                        IsModernApp = True
                        Exit For
                    End If
                Next
            End If

            'If hooking was requested, we'll do that now...
            If useHooks Then

                'Create a snapshot of the target application as it is now. Once it resumes,
                'the model will be updated from the hook messages only.
                mobjModel = clsUIModel.MakeSnapshot(mPID, Me)

                'Hook it...
                mHookClient = New clsHookClient(msInjectorFolder)
                If Not mHookClient.Connect(mPID, 0, sErr) Then
                    mHookClient = Nothing
                    Throw New InvalidOperationException(My.Resources.FailedToHookTargetApplication & sErr)
                End If

            End If

            SetOptions(opts)
            Connected = True

            'We want to know if/when this application is closed
            mProcess = clsProcessWatcher.FromHandle(processHandle)
            SetupBrowserHandle()

            Try
                If Not String.IsNullOrEmpty(tabTitleSearch) Then
                    SendBrowserAttach(browserType, tabTitleSearch, trackingId)
                    Connected = WaitForBrowser()
                End If
            Catch ex As Exception
                Connected = False
                Throw
            End Try

        End SyncLock
    End Sub

    Private Function CheckBrowserAndSetupAutomation(pid As Integer) As BrowserType
        Dim processName = Process.GetProcessById(pid).ProcessName
        Dim browsers As String() = {BrowserProcessNames.Chrome, BrowserProcessNames.Firefox, BrowserProcessNames.Edge}
        Dim browserProcess = browsers.Contains(processName)
        Dim browserType As BrowserType
        If browserProcess Then
            Select Case processName
                Case BrowserProcessNames.Chrome
                    browserType = BrowserType.Chrome
                Case BrowserProcessNames.Firefox
                    browserType = BrowserType.Firefox
                Case BrowserProcessNames.Edge
                    browserType = BrowserType.Edge
            End Select
            SetupBrowserAutomation()
        End If

        Return browserType
    End Function


    Private Sub SetupBrowserAutomation()
        If MyBrowserAutomationIdentifierHelper Is Nothing Then
            MyBrowserAutomationIdentifierHelper = DependencyResolver.Resolve(Of IBrowserAutomationIdentifierHelper)
        End If
    End Sub

    Private Sub SetupBrowserHandle()
        If mProcess IsNot Nothing AndAlso MyBrowserAutomationIdentifierHelper IsNot Nothing Then
            MyBrowserAutomationIdentifierHelper.SetProcHandle(mProcess.GetBrowserHandle)
        End If
    End Sub

    Private Sub DetachBrowserPages(trackingId As String)
        If MyBrowserAutomationIdentifierHelper IsNot Nothing Then
            MyBrowserAutomationIdentifierHelper.ActiveMessagingHost.Detach(trackingId)
            MyBrowserAutomationIdentifierHelper.CloseActivePages()
        End If
    End Sub

    Private Shared foundPID As Boolean = False

    ''' <summary>
    ''' Callback for EnumWindows. Identifies top-level hosted windows and searches
    ''' their child windows for windows owned by the application pid.
    ''' </summary>
    ''' <param name="hWnd">The window handle</param>
    ''' <param name="lParam">The information being search for (i.e. app pid)</param>
    Private Shared Function EnumHostedWindows(hWnd As System.IntPtr, lParam As clsEnumWindowsInfo) As Boolean
        Dim pid As Int32
        GetWindowThreadProcessId(hWnd, pid)
        If pid = lParam.TargetPID Then EnumChildWindows(hWnd, AddressOf EnumHostedChildWindows, lParam)
        Return Not foundPID
    End Function

    ''' <summary>
    ''' Callback for EnumChildWindows. Attempts to locate a hosted child window owned
    ''' by the application PID passed in lParam.Tag
    ''' </summary>
    ''' <param name="hWnd">The child window handle</param>
    ''' <param name="lParam">The information being search for (i.e. app pid)</param>
    Private Shared Function EnumHostedChildWindows(hWnd As System.IntPtr, lParam As clsEnumWindowsInfo) As Boolean
        Dim pid As Int32
        GetWindowThreadProcessId(hWnd, pid)
        If pid = CInt(lParam.Tag) Then
            foundPID = True
            Return False
        End If
        Return True
    End Function

    <Category(Category.Win32)>
    <Command("Detaches an already running application.")>
    <Parameters("None.")>
    Private Function ProcessCommandDetachApplication(ByVal objQuery As clsQuery) As Reply
        Try
            Dim trackingId As String = objQuery.GetParameter(ParameterNames.TrackingId)
            OnExpectDisconnect()
            DetachBrowserPages(trackingId)
            mProcess?.Detach()
        Finally
            Disconnect()
        End Try
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Launches the target application.")>
    <Parameters("path - the path to the executable, and any command-line parameters. Optionally, hook - True(default) or False to determine whether to hook the target, and optionally jab - True or False(default) to determine whether to use Java Access Bridge or not. " & vbCrLf & "Starting a java application is normally done by giving ""java -jar <path to JAR file>"" as the path, and obviously the jab parameter should be set to true. Also optionally, 'options' as a comma-separated list of additional (application type specific options) to set. ")>
    <Examples("startapplication path=""c:\program files\internet explorer\iexplore.exe http://www.google.com"" hook=false")>
    Private Function ProcessCommandStartApplication(ByVal objQuery As clsQuery) As Reply
        Dim sPath As String = objQuery.GetParameter(ParameterNames.Path)
        If sPath Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoCommandLineSpecified)
        End If
        Dim useHooks As Boolean = True
        Dim useJAB As Boolean = False
        Dim s As String = objQuery.GetParameter(ParameterNames.Hook)
        If s IsNot Nothing AndAlso Not Boolean.Parse(s) Then useHooks = False
        s = objQuery.GetParameter(ParameterNames.JAB)
        If s IsNot Nothing AndAlso Boolean.Parse(s) Then useJAB = True

        Dim options As String = ""
        s = objQuery.GetParameter(ParameterNames.Options)
        If s IsNot Nothing Then options = s

        s = objQuery.GetParameter(ParameterNames.ExcludeHTC)
        If Not Boolean.TryParse(s, mExcludeHTC) Then
            mExcludeHTC = False
        End If

        s = objQuery.GetParameter(ParameterNames.ActiveTabOnly)
        If Not Boolean.TryParse(s, mActiveTabOnly) Then
            mActiveTabOnly = True
        End If

        Dim sWorkingDirectory As String = Nothing
        s = objQuery.GetParameter(ParameterNames.WorkingDir)
        If s IsNot Nothing AndAlso s <> "" Then sWorkingDirectory = s

        Dim sErr As String = Nothing
        If Not Launch(sPath, sWorkingDirectory, useHooks, useJAB, options, sErr) Then
            Throw New InvalidOperationException(sErr)
        End If
        Debug.Assert(mPID <> 0)
        Return Reply.Ok
    End Function

    Private Function CreateBrowserProcess(path As String, workingDirectory As String) As PROCESS_INFORMATION
        'Launch target application
        Dim processInfo As PROCESS_INFORMATION
        Dim creationFlags As Integer = modWin32.CreationFlags.NORMAL_PRIORITY_CLASS Or modWin32.CreationFlags.DETACHED_PROCESS
        processInfo = New PROCESS_INFORMATION
        Dim startupInfo As STARTUPINFO = New STARTUPINFO
        startupInfo.cb = Marshal.SizeOf(startupInfo)
        If Not CreateProcess(Nothing, path, Nothing, Nothing, False, creationFlags, IntPtr.Zero, workingDirectory, startupInfo, processInfo) Then
            Throw New BluePrismException(My.Resources.UnableToCreateProcess & path)
        End If
        Return processInfo
    End Function
    Private Function GetRunningBrowserProcessID(browser As BrowserType, wait As Boolean) As Integer
        Dim browserName = BrowserProcessNames.Chrome
        Select Case browser
            Case BrowserType.Chrome
                browserName = BrowserProcessNames.Chrome
            Case BrowserType.Edge
                browserName = BrowserProcessNames.Edge
            Case BrowserType.Firefox
                browserName = BrowserProcessNames.Firefox
        End Select

        Dim sw As New Stopwatch
        sw.Start()
        Do
            Dim browserProcesses = Process.GetProcessesByName(browserName)
            Dim mainProcess = browserProcesses.FirstOrDefault(Function(x) Not String.IsNullOrEmpty(x.MainWindowTitle))
            If mainProcess IsNot Nothing Then Return mainProcess.Id
            Thread.Sleep(500)
        Loop While wait AndAlso sw.Elapsed.Seconds < 15
        sw.Stop()
        sw = Nothing

        Return -1
    End Function

    Private Function LaunchChromiumBrowser(path As String, workingDirectory As String, options As String, browser As BrowserType, trackingId As String) As Boolean
        Dim processId = GetRunningBrowserProcessID(browser, False)
        Dim hasExistingChromiumBrowserProcess = processId <> -1

        If Not hasExistingChromiumBrowserProcess Then
            CreateBrowserProcess(path, workingDirectory)
            ' Since 68 Firefox and 89 Edge, the browser launches a "starter process" which dies after spawning the true main process. 
            ' Wait for it to die before attaching to the main process
            processId = GetRunningBrowserProcessID(browser, True)
        End If

        If Not HasLaunchArgs(path) AndAlso hasExistingChromiumBrowserProcess Then
            MyBrowserAutomationIdentifierHelper.ActiveMessagingHost.Launch(browser, GetURLsFromPath(path), trackingId)
            Dim processHandle As IntPtr = OpenProcess(ProcessAccess.ATTACH_RIGHTS, False, processId)
            mProcess = clsProcessWatcher.FromHandle(processHandle)
            SetupBrowserHandle()
        Else
            DoAttach(processId, False, False, options)
            SendBrowserAttach(browser, String.Empty, trackingId)
            WaitForBrowser()
            ' In the event that the attach message was sent before the pages had fully loaded, resend the attach
            If Not MyBrowserAutomationIdentifierHelper.IsTracking(trackingId) Then
                SendBrowserAttach(browser, String.Empty, trackingId)
            End If
            Return True
        End If

        Return WaitForBrowser()
    End Function

    Private Sub SendBrowserAttach(browser As BrowserType, windowTitle As String, trackingId As String)
        If Not IsMessagingHostRunning(browser) Then Throw New BluePrismException(String.Format(My.Resources.TheBrowserExtensionIsNotInstalledOrEnabled, HelpLauncher.GetHelpUrl(BrowserHelpAddresss)))
        MyBrowserAutomationIdentifierHelper.ActiveMessagingHost.Attach(browser, windowTitle, trackingId)
    End Sub

    Private Function IsMessagingHostRunning(browser As BrowserType) As Boolean
        Dim browserProcessName = BrowserProcessNames.GetBrowserProcessName(browser)
        Dim stopwatch = New Stopwatch()
        stopwatch.Start()
        While stopwatch.Elapsed.TotalSeconds < 10
            Dim processes = Process.GetProcessesByName("BluePrism.MessagingHost")
            If IsProcessRunning(browserProcessName, processes) Then Return True
            Thread.Sleep(100)
        End While

        Return False
    End Function

    Private Function IsProcessRunning(browserProcessName As String, processes As Process()) As Boolean
        For Each process In processes
            Try
                Dim parent = process.Parent()
                While parent IsNot Nothing
                    If String.Equals(parent.ProcessName, browserProcessName, StringComparison.InvariantCultureIgnoreCase) Then
                        Return True
                    End If
                    parent = parent.Parent()
                End While
            Catch ex As ArgumentException
                ' We've reached the root process without finding the matching process name. Move to the next process
            End Try
        Next

        Return False
    End Function

    Private Shared Function GetURLsFromPath(path As String) As String
        Dim urlList = path.Split(" "c).ToList().Where(Function(x) x.IsValidUrl).ToList()
        Dim urlsString = String.Empty
        For Each url In urlList
            Dim tempUri As Uri = Nothing
            If Not Uri.TryCreate(url, UriKind.Absolute, tempUri) Then
                Uri.TryCreate($"{Uri.UriSchemeHttp}{Uri.SchemeDelimiter}{url}", UriKind.Absolute, tempUri)
            End If

            urlsString = $"{urlsString} {tempUri.ToString()}"
        Next

        Return urlsString.Trim
    End Function

    Private Function WaitForBrowser() As Boolean
        mWebPageCreated = False
        'lets wait a while (determined by WebPageCreationTimeout) to create the browser, if a webPage is created move on.

        If NativeMessagingHostNotFound Then
            Throw New BluePrismException(String.Format(My.Resources.TheBrowserExtensionIsNotInstalledOrEnabled, HelpLauncher.GetHelpUrl(BrowserHelpAddresss)))
        End If

        Dim stopWatch = New Stopwatch
        stopWatch.Start()
        While Not mWebPageCreated AndAlso stopWatch.Elapsed.TotalSeconds < mWebPageCreationTimeout
            'Waiting ...
            Thread.Sleep(100)
        End While

        If Not mWebPageCreated Then
            Throw New BluePrismException(String.Format(My.Resources.TheBrowserExtensionIsNotInstalledOrEnabled, HelpLauncher.GetHelpUrl(BrowserHelpAddresss)))
        End If

        If Not ValidExtensionVersion() Then
            Throw New BluePrismException(String.Format(My.Resources.BrowserExtensionIsIncompatibleWithBluePrismVersion, GetBluePrismVersion(3)))
        End If

        Return True
    End Function

    Private Function ValidExtensionVersion() As Boolean
        Dim bpVersion = New Version(0, 0, 0, 0)
        VersionExtensions.TryParseVersionString(GetBluePrismVersion(), bpVersion)
        Return bpVersion.Major = mWebPageExtensionVersion.Major AndAlso bpVersion.MajorRevision = mWebPageExtensionVersion.MajorRevision _
                AndAlso bpVersion.Minor = mWebPageExtensionVersion.Minor
    End Function

    Private Sub WebPageCreatedHandler(sender As Object, e As WebPageCreatedEventArgs) Handles MyBrowserAutomationIdentifierHelper.OnWebPageCreated
        mWebPageExtensionVersion = e.ExtensionVersion
        mWebPageCreated = True
    End Sub

    Private Property NativeMessagingHostNotFound As Boolean

    Private Sub NativeMessagingHostNotFoundHandler(sender As Object, e As NativeMessagingHostNotFoundEventArgs) Handles NamedPipeWrapper.HostNotFound
        NativeMessagingHostNotFound = e.HostNotFound
    End Sub

    Private Function HasLaunchArgs(path As String) As Boolean

        Dim cmdArgs = GetFilePath(path).ToArray

        If cmdArgs.Where(Function(x) Not x.IsValidUrl()).Count() > 1 Then : Return True : End If
        Return False
    End Function

    Private Shared Function GetFilePath(path As String) As List(Of String)
        Dim cmdArgs As String() = path.Split(" "c).ToArray()
        Dim results As List(Of String) = New List(Of String)
        Dim found = False
        Dim constructedPath = String.Empty
        For Each arg In cmdArgs
            If Not found Then
                If File.Exists($"{constructedPath} {arg}".Trim) Then
                    found = True
                End If
                constructedPath = $"{constructedPath} {arg}"
                If found Then
                    results.Add(constructedPath.Trim)
                End If
            Else
                results.Add(arg)
            End If
        Next

        Return results
    End Function

    Private Function LaunchBrowser(path As String, workingDirectory As String, options As String) As Boolean
        Dim processInfo = CreateBrowserProcess(path, workingDirectory)
        mProcess = clsProcessWatcher.FromHandle(processInfo.hProcess)
        SetupBrowserHandle()
        mPID = processInfo.dwProcessId
        SetOptions(options)
        Connected = True
        Return True
    End Function

    Private Function LaunchBrowserApplication(path As String, workingDirectory As String, options As String, ByRef [error] As String, trackingId As String) As Boolean
        SetupBrowserAutomation()

        Try
            If Connected Then
                [error] = My.Resources.AlreadyLaunched
                Return False
            End If

            Select Case True
                Case path.Contains("chrome.exe")
                    Return LaunchChromiumBrowser(path, workingDirectory, options, BrowserType.Chrome, trackingId)
                Case path.Contains("msedge.exe")
                    Return LaunchChromiumBrowser(path, workingDirectory, options, BrowserType.Edge, trackingId)
                Case path.Contains("firefox.exe")
                    Return LaunchChromiumBrowser(path, workingDirectory, options, BrowserType.Firefox, trackingId)
                Case Else
                    ' Launch generic browser process.
                    Return LaunchBrowser(path, workingDirectory, options)
            End Select

        Catch ex As BluePrismException
            [error] = ex.Message
            Return False
        End Try
    End Function

    <Category(Category.Web)>
    Private Function ProcessCommandLaunchBrowserApplication(ByVal query As clsQuery) As Reply
        Dim path As String = query.GetParameter(ParameterNames.Path)
        If path Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoCommandLineSpecified)
        End If

        Dim options = If(query.GetParameter(ParameterNames.Options), "")
        Dim trackingId = If(query.GetParameter(ParameterNames.TrackingId), "")

        Dim browserLaunchTimeoutString = If(query.GetParameter(ParameterNames.BrowserLaunchTimeout), "")
        Dim browserLaunchTimeout = 0
        Integer.TryParse(browserLaunchTimeoutString, browserLaunchTimeout)
        mWebPageCreationTimeout = If(browserLaunchTimeout > 0, browserLaunchTimeout, mWebPageCreationTimeout)

        Dim workingDirectory As String = Nothing
        Dim workingDirectoryParameterValue = query.GetParameter(ParameterNames.WorkingDir)
        If workingDirectoryParameterValue IsNot Nothing AndAlso workingDirectoryParameterValue <> "" Then workingDirectory = workingDirectoryParameterValue

        Dim [error] As String = Nothing
        If Not LaunchBrowserApplication(path, workingDirectory, options, [error], trackingId) Then
            Throw New InvalidOperationException([error])
        End If

        Return Reply.Ok
    End Function

    <Category(Category.Web)>
    <Command("Takes a Snapshot of all the Web elements to create the Application Navigator tree
              for Chromium browsers")>
    <Parameters("None")>
    <Response("The entire raw Html so that it can be parsed in ChromiumHTMLDocument")>
    Private Function ProcessCommandWebSnapshot(ByVal objQuery As clsQuery) As Reply
        ' "GetHtmlSource" just needs any active browser page. The plugin does the work in determining which tab is active and returns the html for that
        Dim htmlDocument = New ChromiumHtmlDocument(MyBrowserAutomationIdentifierHelper.GetActiveWebPages.FirstOrDefault()?.GetHTMLSource)
        Return Reply.Result(htmlDocument.GenerateNavigatorString())
    End Function

    Private Function LaunchInternetExplorer(url As String) As Integer
        Dim pi As New PROCESS_INFORMATION()
        Try
            Dim li As New IELAUNCHURLINFO()
            li.cbSize = Marshal.SizeOf(li)
            Dim hr = IELaunchURL(url, pi, li)
            Marshal.ThrowExceptionForHR(hr)

            Return pi.dwProcessId
        Finally
            CloseHandle(pi.hProcess)
            CloseHandle(pi.hThread)
        End Try
    End Function

    <Category(Category.Win32)>
    <Command("Launches the target application.")>
    <Parameters("path - the path to the executable, and any command-line parameters. Optionally, hook - True(default) or False to determine whether to hook the target, and optionally jab - True or False(default) to determine whether to use Java Access Bridge or not. " & vbCrLf & "Starting a java application is normally done by giving ""java -jar <path to JAR file>"" as the path, and obviously the jab parameter should be set to true. ")>
    <Examples("startapplication path=""c:\program files\internet explorer\iexplore.exe http://www.google.com"" hook=false")>
    Private Function ProcessCommandStartIExploreOnVista(ByVal objQuery As clsQuery) As Reply
        'Windows Vista Only
        If Environment.OSVersion.Version.Major < 6 Then
            Throw New InvalidOperationException(String.Format(My.Resources.StartIExploreOnVistaIsOnlyAvailableOnWindowsVistaYourCurrentNTVersionIs0,
                                                              Environment.OSVersion.Version.Major.ToString))
        End If

        Dim bUseHooks As Boolean = True
        Dim bUseJAB As Boolean = False
        Dim s As String = objQuery.GetParameter(ParameterNames.Hook)
        If Not s Is Nothing AndAlso Not Boolean.Parse(s) Then bUseHooks = False
        s = objQuery.GetParameter(ParameterNames.JAB)
        If Not s Is Nothing AndAlso Boolean.Parse(s) Then bUseJAB = True
        Dim sErr As String = Nothing

        Dim options As String = ""
        s = objQuery.GetParameter(ParameterNames.Options)
        If s IsNot Nothing Then options = s

        s = objQuery.GetParameter(ParameterNames.ExcludeHTC)
        If Not Boolean.TryParse(s, mExcludeHTC) Then
            mExcludeHTC = False
        End If

        s = objQuery.GetParameter(ParameterNames.ActiveTabOnly)
        If Not Boolean.TryParse(s, mActiveTabOnly) Then
            mActiveTabOnly = True
        End If

        'Special launching for IE on Vista, because it behaves differently
        Dim attachPID As Integer

        Dim urlParam As String = objQuery.GetParameter(ParameterNames.HTMLCommandline)
        If String.IsNullOrEmpty(urlParam) Then
            'We allow blank urls which would fail the validation
            'check below.
            attachPID = LaunchInternetExplorer(String.Empty)
        Else
            'Ensure the url is valid.
            Dim url As New Uri(urlParam)
            attachPID = LaunchInternetExplorer(url.OriginalString)
        End If

        'For IE8 and above we need the parent pid of the pid given by IELaunchURL
        Dim v As Version = GetIEVersion()
        If v IsNot Nothing AndAlso v.Major >= 8 Then
            Dim parentPID As Integer = GetParentProcessID(attachPID)
            'However if TabProcGrowth =0 is used then IE will be launched with only
            'one process and the parent pid will be the pid of automate.exe.
            If parentPID <> GetCurrentProcessId() Then
                attachPID = parentPID
            End If
        End If

        'Attach to IE process
        DoAttach(attachPID, bUseJAB, bUseHooks, options)

        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Gets the Parent ProcessID for the given ProcessID
    ''' </summary>
    ''' <param name="pid">The ProcessID for which to find the parent</param>
    ''' <returns>The Parent ProcessID</returns>
    Private Function GetParentProcessID(ByVal pid As Integer) As Integer
        Dim hSnapShot As IntPtr
        Try
            hSnapShot = CreateToolhelp32Snapshot(SnapshotFlags.Process, pid)
            Dim bContinue As Boolean
            Dim procEntry As New PROCESSENTRY32
            procEntry.dwSize = Marshal.SizeOf(procEntry)

            bContinue = Process32First(hSnapShot, procEntry)
            While bContinue
                If procEntry.th32ProcessID = pid Then

                    Return procEntry.th32ParentProcessID
                End If
                bContinue = Process32Next(hSnapShot, procEntry)
            End While
        Finally
            CloseHandle(hSnapShot)
        End Try
    End Function

    ''' <summary>
    ''' Gets the version of Internet Explorer installed on the machine from
    ''' the registry
    ''' </summary>
    ''' <returns>The version of Internet Explorer, Or Nothing on Exception</returns>
    Private Function GetIEVersion() As Version
        Try
            Using key As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\Microsoft\Internet Explorer")
                Return New Version(key.GetValue("Version").ToString())
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    <Category(Category.General)>
    <Command("Closes the target application.")>
    Private Function ProcessCommandCloseApplication(ByVal objQuery As clsQuery) As Reply
        Try
            OnExpectDisconnect()
            mProcess.Terminate()
        Finally
            Disconnect()
        End Try
        Return Reply.Ok

    End Function

    <Category(Category.Win32)>
    <Command("Checks an event.")>
    Private Function ProcessCommandCheckEvent(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(objQuery)
        Dim eventtype As String = objQuery.GetParameter(ParameterNames.EventType)
        If eventtype Is Nothing Then
            Throw New InvalidOperationException(My.Resources.EventTypeNotSpecified)
        End If
        If mobjModel.GetEvent(w.Handle, eventtype) Then Return Reply.Result(True)
        Return Reply.Result(False)

    End Function

    <Category(Category.Win32)>
    <Command("Checks if a given Window can be identified. Used to check if a Window is present, e.g. when waiting for a dialog to appear.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Response("""YES"" if the Window was identified, or ""NO"" otherwise. ")>
    Private Function ProcessCommandCheckWindow(ByVal objQuery As clsQuery) As Reply
        Try
            Dim w As clsUIWindow = mobjModel.IdentifyWindow(objQuery)
        Catch ex As ApplicationException
            Return Reply.Result(False)
        End Try
        Return Reply.Result(True)

    End Function

    <Category(Category.General)>
    <Command("Sends key 'events' to the active application, which is not necessarily the target application. Note that unlike TypeText, this is not targetted to a particular window, so the keys are processed by whatever has focus." & vbCrLf &
    "This differs from SendKeys in that it uses a lower level method of sending the keys, and therefore is more likely to work with some applications. A specific example of this is a Citrix client, which will not respond to SendKeys but will work with SendKeyEvents.")>
    <Parameters("'newtext' to specify the key events to send. 'interval' to specify the pause between characters. " &
        "The syntax for newtext is as follows:" & vbCrLf & vbCrLf &
        "* All normal characters, e.g. ""A"", ""9"", just get sent as that key. A key down followed by a key up is sent." & vbCrLf &
        "* Special keys are enclosed in {brackets}. This allows things such as {HOME} and {ALT} to be sent." & vbCrLf &
        "* The < and > characters are used to modify the NEXT key to be just a key down or key up respectively." & vbCrLf &
        "* Since ""{"", ""}"", ""<"" and "">"" are special characters, they must be sent as follows: {{} {}, {<} and {>} " & vbCrLf &
        "* The following special keys are available: CTRL, LCTRL, RCTRL, SHIFT, ALT, LALT, RALT, TAB, RETURN/ENTER, ESC/ESCAPE, HOME, UP, DOWN, LEFT, RIGHT, DEL, INS, END, PGUP, PGDN, F1-F12" & vbCrLf &
        "* Any key name listed at https://msdn.microsoft.com/en-us/library/system.windows.forms.keys%28v=vs.110%29.aspx can also be used.")>
    <Examples("""hello{RETURN}"" - sends HELLO and presses return" & vbCrLf &
       """<{CTRL}A>{CTRL}"" - presses the ctrl key down, then presses A, then releases the ctrl key" & vbCrLf &
       """<{ALT}AB>{ALT}"" - presses the alt key down, then presses A, then B, then releases the alt key - i.e. does Alt-A, Alt-B" & vbCrLf &
       """<{ALT}A>{ALT}B"" - presses the alt key down, then presses A, then releases the alt key and pressed B - i.e. does Alt-A, B" & vbCrLf &
       """<{CTRL}<{SHIFT}{ESCAPE}>{SHIFT}>{CTRL}"" - presses ctrl and shift, then escape, then lets go of ctrl and shift")>
    Private Function ProcessCommandSendKeyEvents(ByVal q As clsQuery) As Reply
        Dim keys As String = q.GetParameter(ParameterNames.NewText)
        If keys Is Nothing Then Throw New InvalidOperationException(
            My.Resources.NoKeyEventsSpecified)

        ' Get the interval (default:50ms) to wait between sending of keyup/keydown.
        ' Note that we halve the specified interval because at user-level it's the
        ' interval between *characters* - we put half after a keyup, and the other
        ' half after a keydown.
        Dim interval = TimeSpan.FromSeconds(
            q.GetDecimalParam(ParameterNames.Interval) / 2
        )
        If interval <= Nothing Then interval = TimeSpan.FromMilliseconds(50)

        For Each i As KeyInstruction In GetKeyCodes(keys)

            Dim scanCode As Integer = MapVirtualKey(i.KeyCode, 0)
            Dim flags = 0

            Select Case DirectCast(i.KeyCode, VirtualKeyCode)
                ' arrow keys
                ' page up and page down
                ' numpad slash
                Case VirtualKeyCode.VK_LEFT, VirtualKeyCode.VK_UP,
                    VirtualKeyCode.VK_RIGHT, VirtualKeyCode.VK_DOWN,
                    VirtualKeyCode.VK_PRIOR, VirtualKeyCode.VK_NEXT,
                    VirtualKeyCode.VK_END, VirtualKeyCode.VK_HOME,
                    VirtualKeyCode.VK_INSERT, VirtualKeyCode.VK_DELETE,
                    VirtualKeyCode.VK_DIVIDE, VirtualKeyCode.VK_NUMLOCK,
                    VirtualKeyCode.VK_RCONTROL

                    ' set extended bit
                    flags = flags Or KEYEVENTF_EXTENDEDKEY

            End Select

            If (i.Mode And SendKeyMode.Down) <> 0 Then
                keybd_event(i.KeyCode, scanCode, flags, 0)
                If interval <> Nothing Then Thread.Sleep(interval)
            End If

            If (i.Mode And SendKeyMode.Up) <> 0 Then
                keybd_event(i.KeyCode, scanCode, flags Or KEYEVENTF_KEYUP, 0)
                If interval <> Nothing Then Thread.Sleep(interval)
            End If

        Next

        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Enum to instruct key up down or both.
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum SendKeyMode As Integer
        Down = 1
        Up = 2
        Both = 3
    End Enum
    ''' <summary>
    ''' Structure to hold a keycode and mode pair
    ''' used by GetKeyCodes.
    ''' </summary>
    ''' <remarks></remarks>
    Private Structure KeyInstruction
        Public KeyCode As Integer
        Public Mode As SendKeyMode
    End Structure

    ''' <summary>
    ''' Gets the Virtual Key codes for a formatted string.
    ''' The formatted string can contain instructions for producing combinations of special characters 
    ''' and key up/down combinations e.g.
    ''' "hello{RETURN}" - sends HELLO and presses return 
    ''' <![CDATA[ "<{CTRL}A>{CTRL}" ]]> - presses the ctrl key down, then presses A, then releases the ctrl key 
    ''' <![CDATA[ "<{ALT}AB>{ALT}" ]]> - presses the alt key down, then presses A, then B, then releases the alt key - i.e. does Alt-A, Alt-B 
    ''' <![CDATA[ "<{ALT}A>{ALT}B" ]]> - presses the alt key down, then presses A, then releases the alt key and pressed B - i.e. does Alt-A, B 
    ''' <![CDATA[ "<{CTRL}<{SHIFT}{ESCAPE}>{SHIFT}>{CTRL}" ]]> - presses ctrl and shift, then escape, then lets go of ctrl and shift 
    ''' </summary>
    ''' <param name="keys">The formatted string</param>
    ''' <returns>A list of key instructions</returns>
    Private Function GetKeyCodes(ByVal keys As String) As List(Of KeyInstruction)
        Dim keyslist As New List(Of KeyInstruction)
        Dim sThisKey As String
        Dim iIndex As Integer
        Dim iVKK As Integer

        'Loop through all events
        Do While keys.Length > 0
            Dim i As KeyInstruction
            'Allow > or < to modify default mode of pressing both key down and up.
            i.Mode = SendKeyMode.Both
            If keys(0) = ">"c Then
                i.Mode = SendKeyMode.Up
                keys = keys.Substring(1)
            ElseIf keys(0) = "<"c Then
                i.Mode = SendKeyMode.Down
                keys = keys.Substring(1)
            End If
            If keys.Length = 0 Then Throw New InvalidOperationException(My.Resources.InvalidEventSyntaxMissingKeyAfterOr)

            If keys(0) = "{"c Then
                iIndex = keys.IndexOf("}")
                If iIndex = -1 Then Throw New InvalidOperationException(My.Resources.InvalidEventSyntaxMissing)
                sThisKey = keys.Substring(1, iIndex - 1)
                Select Case sThisKey
                    Case ">", "<", "{"
                        iVKK = VkKeyScan(sThisKey(0))
                    Case "}"
                        iVKK = VkKeyScan(sThisKey(0))
                        iIndex += 1
                    Case "CTRL"
                        iVKK = VirtualKeyCode.VK_CONTROL
                    Case "LCTRL"
                        iVKK = VirtualKeyCode.VK_LCONTROL
                    Case "RCTRL"
                        iVKK = VirtualKeyCode.VK_RCONTROL
                    Case "SHIFT"
                        iVKK = VirtualKeyCode.VK_SHIFT
                    Case "ALT"
                        iVKK = VirtualKeyCode.VK_MENU
                    Case "LALT"
                        iVKK = VirtualKeyCode.VK_LMENU
                    Case "RALT"
                        iVKK = VirtualKeyCode.VK_RMENU
                    Case "TAB"
                        iVKK = VirtualKeyCode.VK_TAB
                    Case "RETURN", "ENTER"
                        iVKK = VirtualKeyCode.VK_RETURN
                    Case "ESC", "ESCAPE"
                        iVKK = VirtualKeyCode.VK_ESCAPE
                    Case "HOME"
                        iVKK = VirtualKeyCode.VK_HOME
                    Case "UP"
                        iVKK = VirtualKeyCode.VK_UP
                    Case "DOWN"
                        iVKK = VirtualKeyCode.VK_DOWN
                    Case "LEFT"
                        iVKK = VirtualKeyCode.VK_LEFT
                    Case "RIGHT"
                        iVKK = VirtualKeyCode.VK_RIGHT
                    Case "DEL"
                        iVKK = VirtualKeyCode.VK_DELETE
                    Case "INS"
                        iVKK = VirtualKeyCode.VK_INSERT
                    Case "END"
                        iVKK = VirtualKeyCode.VK_END
                    Case "PGUP"
                        iVKK = VirtualKeyCode.VK_PRIOR
                    Case "PGDN"
                        iVKK = VirtualKeyCode.VK_NEXT
                    Case "F1"
                        iVKK = VirtualKeyCode.VK_F1
                    Case "F2"
                        iVKK = VirtualKeyCode.VK_F2
                    Case "F3"
                        iVKK = VirtualKeyCode.VK_F3
                    Case "F4"
                        iVKK = VirtualKeyCode.VK_F4
                    Case "F5"
                        iVKK = VirtualKeyCode.VK_F5
                    Case "F6"
                        iVKK = VirtualKeyCode.VK_F6
                    Case "F7"
                        iVKK = VirtualKeyCode.VK_F7
                    Case "F8"
                        iVKK = VirtualKeyCode.VK_F8
                    Case "F9"
                        iVKK = VirtualKeyCode.VK_F9
                    Case "F10"
                        iVKK = VirtualKeyCode.VK_F10
                    Case "F11"
                        iVKK = VirtualKeyCode.VK_F11
                    Case "F12"
                        iVKK = VirtualKeyCode.VK_F12
                    Case Else
                        Try
                            iVKK = CInt([Enum].Parse(GetType(Windows.Forms.Keys), sThisKey)) And (Not Windows.Forms.Keys.Modifiers)
                        Catch
                            Throw New InvalidOperationException(String.Format(My.Resources.UnsupportedKey0, sThisKey))
                        End Try
                End Select
                keys = keys.Substring(iIndex + 1)
            Else
                iVKK = VkKeyScan(keys(0))
                keys = keys.Substring(1)
            End If
            i.KeyCode = iVKK

            keyslist.Add(i)
        Loop

        Return keyslist
    End Function

    <Category(Category.Win32), Command(
        "Types characters to the given window. Characters are posted to the " &
        "Window via the WM_CHAR Windows Message.")>
    Private Function ProcessCommandTypeText(ByVal q As clsQuery) As Reply
        Win32SendWMChars(q, False)
        Return Reply.Ok
    End Function

    <Category(Category.Win32), Command(
        "Types characters to the given window, with the Alt key held down. " &
        "Characters are posted to the Window via the WM_CHAR Windows Message.")>
    Private Function ProcessCommandTypeTextAlt(ByVal q As clsQuery) As Reply
        Win32SendWMChars(q, True)
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Attempts the force the window with the given handle to the front of the
    ''' desktop.
    ''' </summary>
    ''' <param name="hwnd">The window handle for the window which should be made into
    ''' a foreground window. If the given window is already the foreground window,
    ''' according to <see cref="GetForegroundWindow"/>, this method has no effect.
    ''' </param>
    ''' <exception cref="OperationFailedException">If the forcing of the foreground
    ''' window failed for any reason</exception>
    Private Sub ForceForeground(ByVal hwnd As IntPtr)
        mWindowOperationsProvider.ForceForeground(hwnd)
    End Sub

    <Category(Category.General)>
    <Command("Activates the application, i.e. brings it to the foreground. " &
     "This needs to be targetted to the application's main window.")>
    <Parameters("Those required to uniquely identify the main window.")>
    Private Function ProcessCommandActivateApp(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(objQuery)
        ForceForeground(w.Hwnd)
        Return Reply.Ok
    End Function

    <Category(Category.General)>
    <Command("The wait command allows a number of queries and expected results to be specified and will block until one of the conditions is true, or a timeout occurs, whichever is the sooner." & vbCrLf &
      "The syntax is as follows:" & vbCrLf & vbCrLf &
      "wait timeout=5000 {query 1}=<result 1> ...[{query n}=<result n>]" & vbCrLf & vbCrLf &
      "The timeout is a value in milliseconds.")>
    <Parameters("timeout is used to specify the timeout time in milliseconds.")>
    <Response("The expected response will be RESULT:X where X is a value from 0-n (0 representing timeout, or 1-n represeting one of the possible wait conditions being met). An error message response is also possible.")>
    Private Function ProcessCommandWait(ByVal objQuery As clsQuery) As Reply
        'Get the timeout time, and barf if it hasn't been specified...
        Dim sTimeout As String = objQuery.GetParameter(ParameterNames.Timeout)
        If sTimeout Is Nothing Then
            Throw New InvalidOperationException(My.Resources.SpecifyTimeout)
        End If
        Dim iTimeout As Integer = CInt(sTimeout)

        'Optimisation - no need to do any messing around if there are no conditions,
        'since we're just being asked to wait a fixed amount of time.
        If objQuery.ConditionCount = 0 Then
            Thread.Sleep(iTimeout)
            Return Reply.Result(0)
        End If

        ''Record the start time, so we can determine when to time out. The Ticks() value
        ''is in units of 100 nanoseconds.
        Dim lStartTime As Long
        lStartTime = System.DateTime.Now.Ticks()

        Dim iCondNum As Integer
        Dim result As Reply
        Do
            'Check each condition to see if it has been met
            iCondNum = 1
            For Each objCond As clsConditionMatchTarget In objQuery.Conditions
                Try
                    result = ExecuteQuery(objCond.ConditionQuery)
                    clsConfig.LogWait(objCond.ConditionQuery.QueryString & " " & result.ToString())
                    If result.IsResult Then
                        Dim ReplyValue As String = result.Message
                        If objCond.IsMatch(ReplyValue) Then
                            Return Reply.Result(iCondNum)
                        End If
                    End If
                Catch be As BrowserAutomationException
                    Throw New ApplicationException(be.Message, be)
                Catch ex As Exception
                    clsConfig.LogException(ex)
                    clsConfig.LogWait(objCond.ConditionQuery.QueryString & " ERROR:" & ex.Message)
                End Try
                iCondNum += 1
            Next

            'Check for timeout...
            Dim lTimeElapsed As Long
            lTimeElapsed = System.DateTime.Now.Ticks() - lStartTime
            If lTimeElapsed \ 10000 >= iTimeout Then
                Return Reply.Result(0)
            End If

            'Update the model, otherwise nothing is going to change!
            UpdateModel()

            'And we don't need to completely hog the CPU while we're here...
            'TODO: There should be no DoEvents here. Remove this when Process/Object
            'studio run things on a seperate thread. See script 6-463 and bug #3161
            Dim SleepStartTime As DateTime = DateTime.Now
            While DateTime.Now.Subtract(SleepStartTime).TotalMilliseconds < 500
                System.Windows.Forms.Application.DoEvents()
                System.Threading.Thread.Sleep(25)
            End While
        Loop
    End Function

    <Category(Category.Win32)>
    <Command("Waits until the application is in an 'input idle' state - i.e. its message queue is empty.")>
    <Parameters("Just 'Timeout' to set the timeout time in milliseconds.")>
    Private Function ProcessCommandWaitForInputIdle(ByVal objQuery As clsQuery) As Reply
        Dim sTimeout As String = objQuery.GetParameter(ParameterNames.Timeout)
        If sTimeout Is Nothing Then
            Throw New InvalidOperationException(My.Resources.SpecifyTimeout)
        End If
        Dim iTimeout As Integer = CInt(sTimeout)
        If WaitForInputIdle(ProcessHandle, iTimeout) = 0 Then
            Return Reply.Ok
        Else
            Return Reply.Timeout
        End If
    End Function

    <Category(Category.General)>
    <Command("Get the text contents of the clipboard.")>
    <Parameters("None.")>
    Private Function ProcessCommandGetClipboard(ByVal objQuery As clsQuery) As Reply
        Dim objData As Windows.Forms.IDataObject = Windows.Forms.Clipboard.GetDataObject
        Dim sTxt As String = CStr(objData.GetData(Windows.Forms.DataFormats.StringFormat, True))
        If sTxt Is Nothing Then
            Throw New InvalidOperationException(My.Resources.CannotReadFromClipboard)
        Else
            Return Reply.Result(sTxt)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Sends keypresses to the active application, which is not necessarily the target application. Note that unlike TypeText, this is not targetted to a particular window, so the keys are processed by whatever has focus. Also, this query does not return until the target application has actually processed the keypresses.")>
    <Parameters("'newtext' to specify the characters to type; 'interval' to define a pause in between each character. Depending on the type of target application, 'special' characters can be specified using control codes." & vbCrLf & vbCrLf &
    "* For Windows applications - the text is passed directly to the .NET Framework SendKeys class, using the SendWait method. Please see [http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfsystemwindowsformssendkeysclasstopic.asp here] for details of the available codes.")>
    Private Function ProcessCommandSendKeys(ByVal q As clsQuery) As Reply
        If Not IsTerminalApp Then
            Win32SendKeys(q)
            Return Reply.Ok
        End If

        ' For backwards compatibility we allow SendKeys to be performed on terminal
        ' applications using the now obsolete SendKeystroke function. See the new
        ' MainframeSendKeys for the right way to send keys to the terminal.
#Disable Warning BC40008 ' disable obsolete method warning
        If mTerminalApp IsNot Nothing Then
            Dim sErr As String = Nothing
            If Not mTerminalApp.SendKeystroke(q.GetParameter(ParameterNames.NewText), sErr) Then
                Throw New InvalidOperationException(sErr)
            End If
        End If
        Return Reply.Ok
#Enable Warning BC40008 ' re-enable obsolete method warning
    End Function

    <Category(Category.Mouse)>
    <Command("Clicks the mouse at a given position in a window, by sending WM_LBUTTONDOWN and WM_LBUTTONUP. The mouse pointer is not affected.")>
    <Parameters("Those required to uniquely identify the window, plus 'targx' and 'targy' to specify the position, 'newtext' (optional) which specifies the button 'left' (default) or 'right'.")>
    Private Function ProcessCommandClickWindow(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Dim x As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)

        'Use bitshift to get the y value into the high order byte of the dword
        Dim lParam As Integer = (y << 16) + x
        If Button = MouseButton.Right Then
            PostMessage(w.Handle, WindowMessages.WM_RBUTTONDOWN, 0, lParam)
            PostMessage(w.Handle, WindowMessages.WM_RBUTTONUP, 0, lParam)
        Else
            PostMessage(w.Handle, WindowMessages.WM_LBUTTONDOWN, 0, lParam)
            PostMessage(w.Handle, WindowMessages.WM_LBUTTONUP, 0, lParam)
        End If
        Return Reply.Ok

    End Function

    <Category(Category.Mouse)>
    <Command("Clicks the mouse at the centre of a window, by sending WM_LBUTTONDOWN and WM_LBUTTONUP. The mouse pointer is not affected.")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' (optional) which specifies the button 'left' (default) or 'right'.")>
    Private Function ProcessCommandClickWindowCentre(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Dim RelativeCentre As System.Drawing.Point = New System.Drawing.Point(w.GetBounds.Width \ 2, w.GetBounds.Height \ 2)
        'Use bitshift to get the y value into the high order byte of the dword
        Dim lParam As Integer = (RelativeCentre.Y << 16) + RelativeCentre.X
        If Button = MouseButton.Right Then
            PostMessage(w.Handle, WindowMessages.WM_RBUTTONDOWN, 0, lParam)
            PostMessage(w.Handle, WindowMessages.WM_RBUTTONUP, 0, lParam)
        Else
            PostMessage(w.Handle, WindowMessages.WM_LBUTTONDOWN, 0, lParam)
            PostMessage(w.Handle, WindowMessages.WM_LBUTTONUP, 0, lParam)
        End If
        Return Reply.Ok

    End Function

    <Category(Category.DotNet)>
    <Command("Clicks a .NET LinkLabel Control by firing LinkClicked Event.")>
    <Parameters("Those required to uniquely identify the window")>
    Private Function ProcessCommandNetClickLinkLabel(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim RelativeCentre = New Point(w.GetBounds.Width \ 2, w.GetBounds.Height \ 2)
        Dim lParam = (RelativeCentre.Y << 16) + RelativeCentre.X
        PostMessage(w.Handle, WindowMessages.WM_MOUSEMOVE, 1, lParam)
        PostMessage(w.Handle, WindowMessages.WM_LBUTTONDOWN, 0, lParam)
        PostMessage(w.Handle, WindowMessages.WM_LBUTTONUP, 0, lParam)

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Pushes a Windows Button by sending the WM_COMMAND message to the parent window")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandPushButton(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)
        Dim hp = TryCast(w.Parent, clsUIWindow).Handle
        PostMessage(hp, WindowMessages.WM_COMMAND, w.CtrlID And &HFFFF, w.Handle)
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Handles a mouse click or double-click query.
    ''' </summary>
    ''' <param name="q">The query detailing the window and location of the click.
    ''' </param>
    ''' <param name="dblClick">True to effect a double click, false for a single
    ''' click.</param>
    ''' <returns>The text response from the query</returns>
    Private Function HandleMouseClickQuery(ByVal q As clsQuery, ByVal dblClick As Boolean) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        Dim x As Integer = q.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = q.GetIntParam(ParameterNames.TargY, False)

        Dim btn As MouseButton =
         GetButtonFromString(q.GetParameter(ParameterNames.NewText))

        Return DoClickMouse(w.ScreenBounds.Location + New Size(x, y), dblClick, btn)
    End Function

    <Category(Category.Mouse)>
    <Command("Moves the mouse pointer to a position in the given Window, and clicks using mouse_event (user32.dll). ")>
    <Parameters("Those required to uniquely identify the window, plus 'targx' and 'targy' to specify the position, plus 'newtext' which accepts a value from [left, right] (a null value defaults to left).")>
    Private Function ProcessCommandMouseClick(ByVal q As clsQuery) As Reply
        Return HandleMouseClickQuery(q, False)
    End Function

    <Category(Category.Mouse)>
    <Command("Moves the mouse pointer to a position in the given Window, and double clicks using mouse_event (user32.dll). ")>
    <Parameters("Those required to uniquely identify the window, plus 'targx' and 'targy' to specify the position, plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandMouseDoubleClick(ByVal q As clsQuery) As Reply
        Return HandleMouseClickQuery(q, True)
    End Function

    <Category(Category.Mouse)>
    <Command("As for MouseClick, but the coordinate parameters are ignored; instead the centre of the window's bounds. The 'newtext' parameter detailing the mouse button name is still valid.")>
    Private Function ProcessCommandMouseClickCentre(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Dim p As Point = CType(w.ScreenBounds, RECT).Centre
        Return DoClickMouse(p, Button)
    End Function

    <Category(Category.Mouse)>
    <Command("As for MouseDoubleClick, but the coordinate parameters are ignored; instead the centre of the window's bounds. The 'newtext' parameter detailing the mouse button name is still valid.")>
    Private Function ProcessCommandMouseDoubleClickCentre(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Dim p As Point = CType(w.ScreenBounds, RECT).Centre
        Return DoClickMouse(p, True, Button)
    End Function

    <Category(Category.Html)>
    <Command("Clicks the mouse in an HTML element.")>
    <Parameters("Those required to uniquely identify the element, plus 'TargX' and 'TargY', plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'")>
    Private Function ProcessCommandHtmlMouseClick(ByVal objQuery As clsQuery) As Reply
        Dim el As clsHTMLElement
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        el = mobjModel.GetHTMLElement(objQuery, docs)

        If el Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoMatchingHtmlElementFound)
        End If

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Dim targx As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim targy As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)

        Return DoClickMouse(Point.Add(el.AbsoluteBounds.Location, New Size(targx, targy)), Button)
    End Function

    <Category(Category.Html)>
    <Command("Clicks the mouse in the centre of an HTML element.")>
    <Parameters("Those required to uniquely identify the element, plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandHtmlMouseClickCentre(ByVal objQuery As clsQuery) As Reply
        Dim el As clsHTMLElement
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        el = mobjModel.GetHTMLElement(objQuery, docs)

        If el Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NoMatchingHtmlElementFound)
        End If

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Return DoClickMouse(CType(el.AbsoluteBounds, RECT).Centre, Button)
    End Function

    <Category(Category.Accessibility)>
    <Command("Clicks the mouse in an Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element, plus 'TargX' and 'TargY', plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandAAMouseClick(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement
        e = mobjModel.IdentifyAccessibleObject(objQuery)

        Dim ButtonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Dim targx As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim targy As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)

        Return DoClickMouse(Point.Add(e.ScreenBounds.Location, New Size(targx, targy)), Button)
    End Function

    <Category(Category.Accessibility)>
    <Command("Clicks the mouse in the centre of an Active Accessibility element.")>
    <Parameters("Those required to uniquely identify the element, plus " &
     "optionally (defaulting to left) 'newtext' which specifies the " &
     "button, 'left' or 'right'.")>
    Private Function ProcessCommandAAClickCentre(ByVal q As clsQuery) As Reply
        Dim e As clsAAElement
        e = mobjModel.IdentifyAccessibleObject(q)

        Dim ButtonString As String = q.GetParameter(ParameterNames.NewText)
        Dim Button As MouseButton
        Button = Me.GetButtonFromString(ButtonString)

        Return DoClickMouse(e.ScreenBounds.Centre, Button)
    End Function

    ''' <summary>
    ''' Sends a mouse click to the specified point on screen.
    ''' </summary>
    ''' <param name="position">The point at which to click, in screen
    ''' coordinates.</param>
    ''' <returns>Returns a string indicating the outcome
    ''' of the operation.</returns>
    Friend Function DoClickMouse(ByVal position As System.Drawing.Point,
                                 ByVal doubleClick As Boolean,
                                 ByVal button As MouseButton) As Reply

        mMouseOperationsProvider.ClickAt(
            position.X,
            position.Y,
            doubleClick,
            button)

        Return Reply.Ok

    End Function

    Private Function DoClickMouse(ByVal position As System.Drawing.Point, ByVal button As MouseButton) As Reply
        Return DoClickMouse(position, False, button)
    End Function

    Private Function DoClickMouse(ByVal position As System.Drawing.Point) As Reply
        Return DoClickMouse(position, False, MouseButton.Left)
    End Function

    <Category(Category.Mouse)>
    <Command("Clicks the mouse at a given position in a window, by sending WM_LBTNDBLCLICK. The mouse pointer is not affected.")>
    <Parameters("Those required to uniquely identify the window, plus 'targx' and 'targy' to specify the position.")>
    Private Function ProcessCommandDblClickWindow(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim x As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)
        'Use bitshift to get the y value into the high order byte of the dword
        Dim lParam As Integer = (y << 16) + x
        SendMessage(w.Hwnd, WindowMessages.WM_LBUTTONDBLCLK, 0, lParam)
        Return Reply.Ok
    End Function

    <Category(Category.Mouse)>
    <Command("Moves the mouse pointer to a position in the given Window.")>
    <Parameters("Those required to uniquely identify the window, plus 'targx' and 'targy' to specify the position.")>
    Private Function ProcessCommandMoveMouse(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim x As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)
        'Use bitshift to get the y value into the high order byte of the dword
        Dim lParam As Integer = (y << 16) + x
        SendMessage(w.Hwnd, WindowMessages.WM_MOUSEMOVE, 0, lParam)
        SendMessage(w.Hwnd, WindowMessages.WM_SETCURSOR, w.Handle, lParam)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Starts a drag and drop by dragging from a point.")>
    <Parameters("Those required to uniquely identify the window, plus 'TargX' and 'TargY' to define point at which to drop.")>
    Private Function ProcessCommandDrag(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim x As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)
        Dim AbsoluteX As Integer = w.GetBounds().Left + x
        Dim AbsoluteY As Integer = w.GetBounds().Top + y

        StartDrag(New Point(AbsoluteX, AbsoluteY))
        Return Reply.Ok
    End Function

    Private Sub StartDrag(ByVal ScreenPoint As Point)
        SetCursorPos(ScreenPoint.X, ScreenPoint.Y)
        mouse_event(MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0)
        mouse_event(MOUSEEVENTF.MOVE, 15, 15, 0, 0) 'The extra nudge is to convince applications that we really are dragging
    End Sub

    Private Sub DropAt(ByVal Screenpoint As Point)
        SetCursorPos(Screenpoint.X, Screenpoint.Y)
        mouse_event(MOUSEEVENTF.MOVE, 1, 1, 0, 0)   'pseudo mousemove to ensure
        mouse_event(MOUSEEVENTF.MOVE, -1, -1, 0, 0) 'tracked destination is known
        mouse_event(MOUSEEVENTF.LEFTUP, 0, 0, 0, 0)
    End Sub

    Private Sub HoverAt(screenPoint As Point)
        SetCursorPos(screenPoint.X, screenPoint.Y)
        mouse_event(MOUSEEVENTF.MOVE, 5, 5, 0, 0)
    End Sub

    <Category(Category.Win32)>
    <Command("Finishes a drag and drop by dropping at point.")>
    <Parameters("Those required to uniquely identify the window, plus 'TargX' and 'TargY' to define point at which to drop.")>
    Private Function ProcessCommandDrop(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim x As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim y As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)
        Dim AbsoluteX As Integer = w.GetBounds().Left + x
        Dim AbsoluteY As Integer = w.GetBounds().Top + y

        DropAt(New Point(AbsoluteX, AbsoluteY))
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Presses a Windows Button using the BM_CLICK message.")>
    <Parameters("Those required to uniquely identify the window.")>
    Private Function ProcessCommandPressButton(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim Result As Integer = PostMessage(w.Handle, WindowMessages.BM_CLICK, 0, 0)
        If Result > 0 Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(My.Resources.ButtonPressOperationFailed & GetLastWin32Error())
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Verifies that the element can be found, and optionally highlights the element")>
    <Parameters("Those required to uniquely identify the window, and highlight which if set to ""True"" will highlight the window")>
    Private Function ProcessCommandVerify(ByVal q As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        Dim hp As String = q.GetParameter(ParameterNames.Highlight)
        If hp IsNot Nothing AndAlso Boolean.Parse(hp) Then
            HighlightRectangle(w.ScreenBounds)
        End If
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Verifies a region, highlighting it if requested to do so.
    ''' </summary>
    ''' <param name="q">The query defining the region and requesting a highlight
    ''' </param>
    ''' <returns>"OK" if the region was found and optionally highlighted;
    ''' or error message if it failed to locate the region.
    ''' </returns>
    <Category(Category.Win32),
     Command("Verifies that the element can be found, and optionally highlights the element"),
     Parameters("Those required to uniquely identify the window plus " &
      "'startx' 'starty', 'endx', 'endy' to specify the region and " &
      "'highlight' set to true to highlight the region")>
    Private Function ProcessCommandRegionVerify(ByVal q As clsQuery) As Reply
        ' Get the screen bounds (feathered to ensure something is displayed even
        ' if the region is empty) and highlight it
        Dim r As Rectangle = GetRegionScreenBounds(q)
        If q.GetBoolParam(ParameterNames.Highlight) Then HighlightRectangle(r)
        Return Reply.Ok
        Return ProcessCommandHighlightRegion(q)
    End Function

    ''' <summary>
    ''' Recursively dumps the JAB data found in a context and its descendants into
    ''' a buffer.
    ''' </summary>
    ''' <param name="jc">The JAB context whose data should be dumped</param>
    ''' <param name="includeInvisible">True to include all 'non-showing' elements and
    ''' their descendants; False otherwise. Note that if the application model option
    ''' "ignorenotshowing" is set to 'True', this parameter is effectively ignored
    ''' and any non-showing elements are <em>not</em> included in the dump.</param>
    ''' <param name="sb">The buffer to which the JAB data should be dumped</param>
    ''' <param name="level">The indent level to use for this dump.</param>
    Private Sub DumpJab(ByVal jc As JABContext, ByVal includeInvisible As Boolean,
     ByVal sb As StringBuilder, ByVal level As Integer)
        sb.Append(" "c, level * 2)
        sb.Append("JAB:")
        jc.AppendIdentifiers(sb)
        sb.Append(" MatchIndex=1 MatchReverse=True")
        sb.AppendLine()

        If (OptionSet("ignorenotshowing") OrElse Not includeInvisible) _
         AndAlso Not jc.Showing Then Return

        For Each child As JABContext In jc.Children
            DumpJab(child, includeInvisible, sb, level + 1)
        Next
    End Sub

    ''' <summary>
    ''' Dumps an AA element and its descendants to a buffer
    ''' </summary>
    ''' <param name="aa">The AA element whose data should be dumped</param>
    ''' <param name="includeInvisible">True to dump data for invisible AA elements
    ''' and their descendants; False to only process visible (to be exact, elements
    ''' which do not have 'invisible' state) elements.</param>
    ''' <param name="sb">The buffer to which the element data should be dumped
    ''' </param>
    ''' <param name="whandle">The Win32 window which contains the AA element.</param>
    ''' <param name="level">The indent level to use for this element in the dumping
    ''' to the buffer</param>
    Private Sub DumpAA(ByVal aa As clsAAElement, ByVal includeInvisible As Boolean,
     ByVal sb As StringBuilder, ByVal whandle As IntPtr, ByVal level As Integer)
        ' Don't include or descend below invisible elements...
        If Not includeInvisible AndAlso aa.Invisible Then Return

        sb.Append(" "c, level * 2)
        sb.Append("AAELEMENT:")
        GetUpdatedWindowModel(whandle).AppendIdentifiers(sb).Append(" ")
        aa.AppendIdentifiers(sb).Append(" ")
        sb.Append("MatchIndex=1 MatchReverse=True").AppendLine()

        For Each caa As clsAAElement In aa.Elements
            DumpAA(caa, includeInvisible, sb, whandle, level + 1)
        Next
    End Sub

    ''' <summary>
    ''' Dumps a UI entity to a string buffer, indenting the text dump to a specified
    ''' level.
    ''' </summary>
    ''' <param name="e">The UI entity whose data should be dumped</param>
    ''' <param name="includeInvisible">True to include invisible elements and their
    ''' descendants in the dump; False to ignore them. This affects the contained
    ''' AA and JAB elements more than the Win32 ones.</param>
    ''' <param name="sb">The buffer to which the entity should be dumped</param>
    ''' <param name="level">The indent level to set the entity data dump at</param>
    Private Sub DumpEntity(ByVal e As clsUIEntity, ByVal includeInvisible As Boolean,
     ByVal sb As StringBuilder, ByVal level As Integer)
        ' Device Contexts and Raw Text are not supoprted in the snapshot
        If TypeOf e Is clsUIDC OrElse TypeOf e Is clsUIText Then Return

        ' Add 2 spaces for each level
        sb.Append(" "c, level * 2)
        sb.Append(e.EntityTypeID).Append(":"c)

        Dim w As clsUIWindow = TryCast(e, clsUIWindow)
        If w Is Nothing Then sb.AppendLine() : Return

        GetUpdatedWindowModel(w.Handle).AppendIdentifiers(sb).AppendLine()

        If w.Handle <> IntPtr.Zero Then
            DumpAA(clsAAElement.FromWindow(w.Handle),
             includeInvisible, sb, w.Handle, level + 1)

            ' If the JAB is currently enabled in this app
            If mJAB IsNot Nothing Then
                ' Get a JAB context from the window. If one exists and the
                ' window is the highest level window with such a context,
                ' dump the java contents of the window too
                Using jc As JABContext = mJAB.GetContextFromWindow(w.Handle)
                    If jc IsNot Nothing AndAlso
                     (w.IsTopLevel OrElse Not mJAB.HasJavaAncestor(w.Hwnd)) Then
                        DumpJab(jc, includeInvisible, sb, level + 1)
                    End If
                End Using
            End If
        End If

        For Each ce As clsUIEntity In e.Children
            DumpEntity(ce, includeInvisible, sb, level + 1)
        Next
    End Sub

    Private Sub DumpSapChildren(ByVal obj As Object, ByVal sb As StringBuilder, ByVal level As String)
        Dim children As Object
        Try
            children = obj.GetType().InvokeMember("children", BindingFlags.GetProperty, Nothing, obj, Nothing)
        Catch ex As Exception
            'No children, apparently!
            Return
        End Try
        Dim childcount As Integer = CInt(children.GetType().InvokeMember("count", BindingFlags.GetProperty, Nothing, children, Nothing))
        For cn As Integer = 0 To childcount - 1
            Dim args(0) As Object
            args(0) = cn
            Dim el As Object = children.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, children, args)

            sb.Append(level & "SAP:")

            Dim props As String() = {"*type", "@id", "name", "text", "tooltip",
             "containerType", "subType", "ScreenLeft", "ScreenTop", "Width", "Height", "selectedNode"}
            Dim first As Boolean = True
            For Each tprop As String In props
                Try
                    Dim prop As String
                    If tprop.StartsWith("*") OrElse tprop.StartsWith("@") Then
                        prop = tprop.Substring(1)
                    Else
                        prop = tprop
                    End If
                    Dim val As String = el.GetType().InvokeMember(prop, BindingFlags.GetProperty, Nothing, el, Nothing).ToString()
                    If Not first Then
                        sb.Append(" ")
                    Else
                        first = False
                    End If
                    If tprop.StartsWith("*") Then
                        sb.Append("ComponentType=" & clsQuery.EncodeValue(val))
                    ElseIf tprop.StartsWith("@") Then
                        sb.Append("+ID=" & clsQuery.EncodeValue(val))
                    Else
                        sb.Append("Note=" & clsQuery.EncodeValue(prop & ":" & val))
                    End If
                Catch ex As Exception
                    'Don't care, just ignoring properties that don't exist
                End Try
            Next
            sb.Append(vbCrLf)

            DumpSapChildren(el, sb, level & "  ")
        Next
    End Sub

    Private Sub DumpSap(ByVal sb As StringBuilder)
        If GetSapApp() Then
            Dim connections As Object = mSAPGuiApplication.GetType().InvokeMember("children", BindingFlags.GetProperty, Nothing, mSAPGuiApplication, Nothing)
            Dim concount As Integer = CInt(connections.GetType().InvokeMember("count", BindingFlags.GetProperty, Nothing, connections, Nothing))
            For con As Integer = 0 To concount - 1
                Dim args(0) As Object
                args(0) = con
                Dim thiscon As Object = connections.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, connections, args)
                sb.Append("SAPCONNECTION:Note=" & con.ToString() & vbCrLf)
                'Get a list of the sessions within this connection...
                Dim sessions As Object = thiscon.GetType().InvokeMember("children", BindingFlags.GetProperty, Nothing, thiscon, Nothing)
                Dim sescount As Integer = CInt(sessions.GetType().InvokeMember("count", BindingFlags.GetProperty, Nothing, sessions, Nothing))
                For ses As Integer = 0 To sescount - 1
                    args(0) = ses
                    Dim session As Object = sessions.GetType().InvokeMember("item", BindingFlags.GetProperty, Nothing, sessions, args)
                    sb.Append("  " & "SAPSESSION:Note=" & clsQuery.EncodeValue(session.ToString()) & vbCrLf)
                    DumpSapChildren(session, sb, "      ")
                Next
            Next
        End If
    End Sub

    ''' <summary>
    ''' Takes a full snapshot of all Windows elements, as well as contained AA
    ''' elements and, if JAB is enabled for this app model, the contained JAB
    ''' elements.
    ''' </summary>
    <Category(Category.Diagnostics)>
    <Command("Takes a snapshot of all the Windows elements and their contained " &
      "AA elements and, if JAB is enabled, JAB elements.")>
    <Parameters("includeinvisible: True to include invisible AA/JAB elements; " &
      "False to ignore them and their descendants. Default is False.")>
    <Response("""RESULT:<snapshot>"" where <snapshot> is a multi-line description" &
      " of all the elements found.")>
    Private Function ProcessCommandWindowsSnapshot(ByVal q As clsQuery) As Reply
        Dim includeInvisible As Boolean =
         q.GetBoolParam(ParameterNames.IncludeInvisible)
        Dim sb As New StringBuilder()
        For Each e As clsUIEntity In mobjModel.GetAllEntities()
            ' Only dump top level entities
            If e.Parent Is Nothing Then DumpEntity(e, includeInvisible, sb, 0)
        Next

        'Add in SAP stuff if there is any...
        DumpSap(sb)

        Return Reply.Result(sb.ToString())
    End Function

    <Category(Category.Accessibility)>
    <Command("Verifies that the element can be found, and optionally highlights the element")>
    <Parameters("Those required to uniquely identify the element, and highlight which if set to ""True"" will highlight the element")>
    Private Function ProcessCommandAAVerify(ByVal objQuery As clsQuery) As Reply
        Dim e As clsAAElement = mobjModel.IdentifyAccessibleObject(objQuery)
        Dim hp As String = objQuery.GetParameter(ParameterNames.Highlight)
        If hp IsNot Nothing AndAlso Boolean.Parse(hp) Then
            HighlightRectangle(e.ScreenBounds)
        End If
        Return Reply.Ok
    End Function

    ''' <summary>
    ''' Verifies the existence of a UIA element, optionally highlighting it when
    ''' found.
    ''' </summary>
    ''' <param name="q">The query containing the search constraints for the element.
    ''' </param>
    ''' <returns><see cref="Reply.Ok"/> if the element was found</returns>
    ''' <exception cref="NoSuchElementException">If no element matching the provided
    ''' search constraints could be found.</exception>
    ''' <exception cref="TooManyElementsException">If more than one element matching
    ''' the provided search constraints was found</exception>
    <Category(Category.UIAutomation),
     Command(
      "Verifies that the element can be found and optionally highlights the element"),
     Parameters(
      "Those required to uniquely identify the window, and highlight which if " &
      "set to ""True"" will highlight the window")>
    Private Function ProcessCommandUiaVerify(q As clsQuery) As Reply
        Dim elem = UIAutomationIdentifierHelper.FindUIAutomationElement(q, mPID)
        If q.GetBoolParam(ParameterNames.Highlight) Then HighlightRectangle(
            elem.CurrentBoundingRectangle
        )
        Return Reply.Ok
    End Function

    <Category(Category.Web)>
    <Command("Verifies that the element can be found, and optionally highlights the element")>
    <Parameters("Those required to uniquely identify the element, and highlight which if set to ""True"" will highlight the element")>
    Private Function ProcessCommandWebVerify(query As clsQuery) As Reply
        If MyBrowserAutomationIdentifierHelper Is Nothing Then
            Throw New NoSuchElementException(My.Resources.NoElementMatchedTheQueryTerms)
        End If
        Dim element = MyBrowserAutomationIdentifierHelper.FindSingleElement(query)

        If query.GetBoolParam(ParameterNames.Highlight) Then
            element.Highlight(Color.Red)
        End If
        Return Reply.Ok
    End Function

    <Category(Category.Html)>
    <Command("Verifies that the element can be found, and optionally highlights the element")>
    <Parameters("Those required to uniquely identify the element, and highlight which if set to ""True"" will highlight the element")>
    Private Function ProcessCommandHtmlVerify(ByVal objQuery As clsQuery) As Reply
        Dim docs As List(Of clsHTMLDocument) = GetHtmlDocuments()
        Dim e As clsHTMLElement = mobjModel.GetHTMLElement(objQuery, docs)
        Dim hp As String = objQuery.GetParameter(ParameterNames.Highlight)
        If hp IsNot Nothing AndAlso Boolean.Parse(hp) Then
            HighlightRectangle(e.AbsoluteBounds)
        End If
        Return Reply.Ok
    End Function

    <Category(Category.Terminal)>
    <Command("Verifies that we are connected to the terminal, and optionally highlights the element")>
    <Parameters("Those required to uniquely identify the element, and highlight which if set to ""True"" will highlight the element")>
    Private Function ProcessCommandTerminalVerify(ByVal objQuery As clsQuery) As Reply
        If Not mTerminalApp Is Nothing Then
            Dim hp As String = objQuery.GetParameter(ParameterNames.Highlight)
            If hp IsNot Nothing AndAlso Boolean.Parse(hp) Then
                Return ProcessCommandHighlightTerminalField(objQuery)
            End If
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(My.Resources.NotConnected)
        End If
    End Function

    <Category(Category.Java)>
    <Command("Verifies that the element can be found, and optionally highlights the element")>
    <Parameters("Those required to uniquely identify the element, and highlight which if set to ""True"" will highlight the element")>
    Private Function ProcessCommandJabVerify(ByVal objQuery As clsQuery) As Reply
        Using c As JABContext = mobjModel.GetJABObject(objQuery, mJAB)
            Dim hp As String = objQuery.GetParameter(ParameterNames.Highlight)
            If hp IsNot Nothing AndAlso Boolean.Parse(hp) Then
                HighlightRectangle(c.ScreenRect)
            End If
            Return Reply.Ok
        End Using
    End Function

#End Region

    ''' <summary>
    ''' This method loops through the rows of an XmlElement and appends the rows where there
    ''' were more columns in the subsequent rows than in the previous.
    ''' </summary>
    ''' <param name="xDocument">An xml document.</param>
    ''' <param name="tableCollection">An xml formated table.</param>
    Private Shared Sub AppendXmlTableRowColumns(ByRef xDocument As XmlDocument, ByRef tableCollection As XmlElement)
        ' We need to check for variable columns in the rows - 
        ' if the first row has 1 column and the subsequent rows have 5, the collection will 
        ' only return 1 column for each row - it uses the count in the first row to calculate
        ' how many columns are required across the board.
        Dim maxCols As Integer = 0
        Dim firstRowCols As Integer = 0
        Dim first As Boolean = True

        For Each xrow As XmlElement In tableCollection.ChildNodes
            If (first) Then
                first = False
                firstRowCols = xrow.ChildNodes.Count
            End If
            maxCols = Math.Max(maxCols, xrow.ChildNodes.Count)
        Next

        While firstRowCols < maxCols
            firstRowCols += 1
            tableCollection.ChildNodes(0).AppendChild(CreateCollectionFieldXML(
              xDocument, "", "Text", "Column" & firstRowCols))
        End While
    End Sub

#Region "Query Command Handlers - Region"

    ''' <summary>
    ''' Gets the unfeathered <em>screen</em> bounds defined by the region in the
    ''' given query.
    ''' </summary>
    ''' <param name="q">The query containing the parameters defining the region. This
    ''' should contain the parameters required to identify the Win32 window on which
    ''' the region is defined as well as the parameters "startx", "starty", "endx"
    ''' and "endy" which define the region itself.
    ''' </param>
    ''' <returns>A RECT structure containing the screen bounds of the region
    ''' defined in the query.</returns>
    ''' <exception cref="ApplicationException">If the Win32 window specified in the
    ''' query could not be uniquely identified.</exception>
    ''' <exception cref="FormatException">If the required parameters defining the
    ''' region were either not present or could not be parsed into integers
    ''' </exception>
    Private Function GetRegionScreenBounds(ByVal q As clsQuery) As RECT
        Return GetRegionScreenBounds(q, False)
    End Function

    ''' <summary>
    ''' Gets the <em>screen</em> bounds defined by the region in the given query.
    ''' </summary>
    ''' <param name="q">The query containing the parameters defining the region. This
    ''' should contain the parameters required to identify the Win32 window on which
    ''' the region is defined as well as the parameters "startx", "starty", "endx"
    ''' and "endy" which define the region itself.
    ''' </param>
    ''' <param name="feather">True to feather the region by 1 - ie. to expand the
    ''' rectangle's right and bottom edge by 1. This is primarily for highlighting,
    ''' in order to ensure that the region is fully encapsulated by the rectangle.
    ''' </param>
    ''' <returns>A RECT structure containing the screen bounds of the region
    ''' defined in the query.</returns>
    ''' <exception cref="ApplicationException">If the Win32 window specified in the
    ''' query could not be uniquely identified.</exception>
    ''' <exception cref="FormatException">If the required parameters defining the
    ''' region were either not present or could not be parsed into integers
    ''' </exception>
    Private Function GetRegionScreenBounds(ByVal q As clsQuery, ByVal feather As Boolean) As RECT
        Dim w = mobjModel.IdentifyWindow(q)
        ' Get the rectangle defining the region relative to the window
        Dim r As RECT = GetRegionWindowBounds(q, w)

        ' Offset it with the window location to get the bounds relative to the screen
        r.Offset(w.ScreenBounds.Location)

        ' Feather the region if necessary
        If feather Then r.Expand(1, 1)
        Return r
    End Function

    ''' <summary>
    ''' Gets the bounds defined by the region in the given query relative to the
    ''' window on which the region is described - ie. that specified in the query.
    ''' </summary>
    ''' <param name="q">The query containing the parameters defining the region. This
    ''' should contain the parameters "startx", "starty", "endx" and "endy" which
    ''' define the region itself.
    ''' Optionally, they can contain parameters which expose the region as a list or
    ''' grid region. Namely, for a list region :- <list>
    ''' <item><see cref="ParameterNames.ElementNumber"/> (presence identifies region
    ''' as a list item)</item>
    ''' <item><see cref="ParameterNames.Padding"/> (optional)</item>
    ''' <item><see cref="ParameterNames.ListDirection"/> (optional - one of
    ''' 'TopDown', 'BottomUp', 'LeftToRight', 'RightToLeft'. Default is 'TopDown')
    ''' </item>
    ''' </list>
    ''' For a grid region (all params mandatory - any missing and the region is not
    ''' treated as a grid region):- <list>
    ''' <item><see cref="ParameterNames.ColumnNumber"/></item>
    ''' <item><see cref="ParameterNames.RowNumber"/></item>
    ''' <item><see cref="ParameterNames.GridSchema"/></item>
    ''' </list>
    ''' </param>
    ''' <returns>A RECT structure containing the client bounds of the region
    ''' defined in the query.</returns>
    ''' <exception cref="NoSuchImageRegionException">If the region or any of its 
    ''' anchoring regions could not be located as an image.</exception>
    ''' <exception cref="InvalidFormatException">If the required parameters defining
    ''' the region were either not present or could not be parsed into integers, -or-
    ''' if row, column or element number params were found but could not be parsed
    ''' into integers, -or- if a grid schema param was present which could not be
    ''' parsed into a grid schema.
    ''' </exception>
    ''' <exception cref="ArgumentOutOfRangeException">If row / column number params
    ''' were found which exceeded the range of the accompanying grid schema param.
    ''' </exception>
    ''' <exception cref="BluePrismException">If throwIfRegionImageNotFound is true, the 
    ''' region is configured to be located using an image and the image cannot be found.
    ''' </exception>
    Private Function GetRegionWindowBounds(ByVal q As clsQuery) As RECT
        Dim w = mobjModel.IdentifyWindow(q)
        Return GetRegionWindowBounds(q, w)
    End Function

    ''' <summary>
    ''' Gets the bounds defined by the region in the given query relative to the
    ''' given window
    ''' </summary>
    ''' <param name="q">The query containing the parameters defining the region.</param>
    ''' <param name="w">The window in which the region is relative to</param>
    ''' <returns></returns>
    Private Function GetRegionWindowBounds(ByVal q As clsQuery, w As clsUIWindow) As RECT
        Dim regionCoords = GetRegionCoords(q, w)
        ' Should always be a value here
        Debug.Assert(Not regionCoords.Equals(RECT.Empty))

        ' Check if we're dealing with list or grid regions here - the existence of
        ' certain parameters will decide this for us.

        ' Row? Column? GridSchema? Must be a grid region
        If q.HasAllParameters(
         ParameterNames.RowNumber, ParameterNames.ColumnNumber, ParameterNames.GridSchema) Then

            Dim schema As New GridSchema(q.Parameters(ParameterNames.GridSchema))
            ' Row and column numbers are 1-based within AMI,
            ' They are zero-based in the grid schema so convert
            Dim cellRect As Rectangle
            Try
                cellRect = schema.GetCell(
                 CInt(q.Parameters(ParameterNames.ColumnNumber)) - 1,
                 CInt(q.Parameters(ParameterNames.RowNumber)) - 1,
                 regionCoords.Size
                )
            Catch aoore As ArgumentOutOfRangeException ' col/row number too high/low
                Throw
            Catch ice As InvalidCastException
                Throw New InvalidFormatException(
                 My.Resources.ColumnRowValuesCouldNotBeParsedIntoNumbersColumn0Row1,
                 q.Parameters(ParameterNames.ColumnNumber), q.Parameters(ParameterNames.RowNumber))
            End Try

            ' This cell rect is relative to the region, so we need to
            ' offset it by the region's location to make it relative to
            ' the host control
            cellRect.Offset(regionCoords.Location)

            ' And we return this rect rather than the region rect since
            ' it is the subregion specified by the query that we need
            ' the bounds of.
            Return cellRect

            ' Element number? I say list region
        ElseIf q.HasParameter(ParameterNames.ElementNumber) Then

            ' Get the padding if it's set (default of 0 - ie. no padding)
            Dim padding As Integer = 0
            If q.HasParameter(ParameterNames.Padding) Then _
             padding = CInt(q.Parameters(ParameterNames.Padding))

            ' And the list direction - the default is TopDown
            Dim dirn As ListDirection
            clsEnum.TryParse(q.Parameters(ParameterNames.ListDirection), dirn)

            ' The element number is 1-based, so reduce it to an offset (ie. make it
            ' 0-based) by subtracting 1 from it.
            Dim elNo As Integer = CInt(q.Parameters(ParameterNames.ElementNumber)) - 1

            If elNo > 0 Then
                Dim offset As New Point()
                Select Case dirn
                    Case ListDirection.TopDown
                        offset.Y = elNo * (regionCoords.Height + padding)
                    Case ListDirection.BottomUp
                        offset.Y = -elNo * (regionCoords.Height + padding)
                    Case ListDirection.LeftToRight
                        offset.X = elNo * (regionCoords.Width + padding)
                    Case ListDirection.RightToLeft
                        offset.X = -elNo * (regionCoords.Width + padding)
                End Select
                regionCoords.Offset(offset)

            End If
        End If

        Return regionCoords
    End Function

    Private Function GetRegionCoords(q As clsQuery, w As clsUIWindow) As RECT
        Dim locationParams = RegionLocationParamsMapper.FromQuery(q)
        Dim finder As New RegionFinder(Function() _
            CaptureApplicationWindow(RECT.Empty, w))
        Dim result = finder.FindRegion(locationParams)

        If result = Rectangle.Empty Then
            Return RECT.Empty
        Else
            Return New RECT(result.Left, result.Right, result.Top, result.Bottom)
        End If
    End Function

    ''' <summary>
    ''' Gets ImageValue or ElementSnapshot parameter converted to an image, throwing an
    ''' exception if the parameter is not found
    ''' </summary>
    Private Shared Function EnsureRegionImage(q As clsQuery) As clsPixRect
        Dim img = If(q.GetImageParam(clsQuery.ParameterNames.ImageValue, True),
                     q.GetImageParam(clsQuery.ParameterNames.ElementSnapshot, True))
        If img Is Nothing Then Throw New BluePrismException(
            My.Resources.AnElementsnapshotOrImagevalueMustBeProvidedToMatchAgainst)
        Return img
    End Function

    ''' <summary>
    ''' Capture application window as a Bitmap, normalised to 24bit RGB format
    ''' </summary>
    ''' <param name="searchRect">The area within the window to capture</param>
    ''' <param name="w">The window to capture</param>
    ''' <returns>A Bitmap image based on the area captured</returns>
    ''' <remarks></remarks>
    Private Function CaptureApplicationWindow(searchRect As RECT, w As clsUIWindow) As Bitmap
        Dim captured = WindowCapturer.CaptureBitmap(w.Hwnd, searchRect)
        Dim normalised = BitmapFormatConverter.NormaliseBitmap(captured)
        Return normalised
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights a region.")>
    <Parameters("The bounds of the region specified by 'StartX' 'StartY' 'EndX' and 'EndY' " &
     "as well as those required to uniquely identify the window.")>
    Private Function ProcessCommandHighlightRegion(ByVal objQuery As clsQuery) As Reply
        ' Get the screen bounds (feathered to ensure something is displayed even
        ' if the region is empty) and highlight it
        HighlightRectangle(GetRegionScreenBounds(objQuery, True))
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Checks if the specified region exists.")>
    <Parameters("Those required to uniquely identify the host window, " &
                "plus 'startx','starty','endx' and 'endy' to define the rectangle, " &
                "'locationmethod' to determine how the region is identified and the image " &
                "search parameters ('imagesearcharea', 'imagesearchpadding' and 'imagevalue') " &
                "that will be used if the region is located using its image.")>
    Private Function ProcessCommandRegionCheckExists(ByVal q As clsQuery) As Reply
        Try
            Dim bounds = GetRegionWindowBounds(q)
            Dim exists As Boolean = Not bounds.Equals(RECT.Empty)
            Return Reply.Result(exists)

        Catch nsre As NoSuchImageRegionException
            Return Reply.Result(False)
        End Try
    End Function

    <Category(Category.Mouse)>
    <Command("Clicks the mouse in a region.")>
    <Parameters("Those required to uniquely identify the window, " &
     "plus 'startx' 'starty', 'endx', 'endy' to specify the region " &
     "and 'targx', 'targy' to specify the relative location of the mouse click, plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandRegionMouseClick(ByVal q As clsQuery) As Reply
        ' Parse the target of the mouse click
        Dim target As Point = New Point(
         CInt(q.Parameters(ParameterNames.TargX)), CInt(q.Parameters(ParameterNames.TargY)))

        ' The target is currently relative to the region, so offset it
        ' by the region's screen location to make it relative to the screen
        target.Offset(GetRegionScreenBounds(q).Location)

        ' Click the mouse at that target location using the specified button
        Return DoClickMouse(target, GetButtonFromString(q.Parameters(ParameterNames.NewText)))
    End Function

    <Category(Category.Mouse)>
    <Command("Clicks the mouse in the centre of a region.")>
    <Parameters("Those required to uniquely identify the window, " &
     "plus 'startx' 'starty', 'endx', 'endy' to specify the region, " &
     "plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'")>
    Private Function ProcessCommandRegionMouseClickCentre(ByVal q As clsQuery) As Reply
        ' Click the mouse into the centre of the screen bounds using the specified button
        Return DoClickMouse(GetRegionScreenBounds(q).Centre,
         GetButtonFromString(q.Parameters(ParameterNames.NewText)))
    End Function

    <Category(Category.Win32)>
    <Command("Clicks the parent window of the region in the centre of the region.")>
    <Parameters("Those required to uniquely identify the window, plus 'startx' 'starty', 'endx', 'endy' to specify the region.")>
    Private Function ProcessCommandRegionParentClickCentre(ByVal q As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)

        ' Find the centre of the region relative to the window
        Dim target As Point = GetRegionWindowBounds(q).Centre

        ' convert the target to be relative to the client area of the window
        Dim clientOffset As Point = w.GetClientOffset()
        target.Offset(-clientOffset.X, -clientOffset.Y)

        'Use bitshift to get the y value into the high order byte of the dword
        Dim lParam As Integer = (target.Y << 16) + target.X
        PostMessage(w.Handle, WindowMessages.WM_LBUTTONDOWN, 0, lParam)
        PostMessage(w.Handle, WindowMessages.WM_LBUTTONUP, 0, lParam)

        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Starts a drag and drop by dragging from the centre of a region.")>
    <Parameters("Those required to uniquely identify the window, plus 'StartX', 'StartY', 'EndX' and 'EndY' to define the region bounds.")>
    Private Function ProcessCommandRegionStartDrag(ByVal objQuery As clsQuery) As Reply
        StartDrag(GetRegionScreenBounds(objQuery).Centre)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Finishes a drag and drop by dropping at the centre of a region.")>
    <Parameters("Those required to uniquely identify the window, plus 'StartX', 'StartY', 'EndX' and 'EndY' to define the region bounds.")>
    Private Function ProcessCommandRegionDropOnto(ByVal objQuery As clsQuery) As Reply
        DropAt(GetRegionScreenBounds(objQuery).Centre)
        Return Reply.Ok
    End Function

    <Category(Category.Win32)>
    <Command("Gets the bounds of a region relative to its window.")>
    <Parameters("Those required to uniquely identify the window plus region bounds: " &
     "startx, starty, endx, endy. Optionally 'listdirection', 'padding' and " &
     "'elementnumber' for list regions and 'gridschema', 'columnnumber' and " &
     "'rownumber' for grid regions.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing " &
     "the element bounds of the region")>
    Private Function ProcessCommandRegionGetElementBounds(ByVal q As clsQuery) As Reply
        Return Reply.Result(CreateCollectionXMLFromRectangle(GetRegionWindowBounds(q)))
    End Function

    <Category(Category.Win32)>
    <Command("Gets the bounds of a region relative to the screen.")>
    <Parameters("Those required to uniquely identify the window plus region bounds: " &
     "startx, starty, endx, endy. Optionally 'listdirection', 'padding' and " &
     "'elementnumber' for list regions and 'gridschema', 'columnnumber' and " &
     "'rownumber' for grid regions.")>
    <Response("""RESULT:<xml>"" where <xml> is the collections xml representing " &
     "the screen bounds of the region")>
    Private Function ProcessCommandRegionGetElementScreenBounds(ByVal q As clsQuery) As Reply
        Return Reply.Result(CreateCollectionXMLFromRectangle(GetRegionScreenBounds(q)))
    End Function

    <Category(Category.Win32)>
    <Parameters("Those required to uniquely identify the window, plus optionally 'startx','starty','endx' and 'endy' to define the rectangle. " &
     "If no 'startx' parameter is given, a bitmap covering the whole window is returned.")>
    Private Function ProcessCommandReadBitmap(ByVal query As clsQuery) As Reply
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(query)
        Dim r As Rectangle
        ' startx not being there is valid - it captures the entire window, 
        ' rather than a region within it
        If query.HasParameter(ParameterNames.StartX) Then r = GetRegionWindowBounds(query)
        Return Reply.Result(clsPixRect.Capture(w.Hwnd, r).ToString())
    End Function

    <Category(Category.Win32)>
    <Command("Reads text from within a rectangular area of a given window using OCR.")>
    <Parameters("Either a) Those required to uniquely identify the window, plus 'startx','starty','endx' and 'endy' to define a rectangle; or b) ImageFilePath specifying the full path of an image stored on disk. Also Scale (default 4) is the amount to scale the image by before passing to OCR. Also DiagsPath, if specified, is a directoy to which diagnostics files (e.g. intermediate images) will be written.")>
    Private Function ProcessCommandReadTextOcr(ByVal q As clsQuery) As Reply
        Try
            Dim p As clsPixRect = Nothing
            If q.HasParameter(ParameterNames.ImageFile) Then
                Dim imgFile As String = q.Parameters(ParameterNames.ImageFile)
                If Not File.Exists(imgFile) Then Throw New InvalidOperationException(My.Resources.FileNotFound & imgFile)
                p = New clsPixRect(CType(System.Drawing.Image.FromFile(imgFile), Bitmap))

            Else
                Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
                p = clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(q))
            End If

            Dim scale As Decimal = 4
            If q.HasParameter(ParameterNames.Scale) Then
                If Not Decimal.TryParse(q.GetParameter(ParameterNames.Scale), scale) Then
                    Throw New InvalidOperationException(My.Resources.ScaleIsNotValid)
                End If
            End If

            Dim diagspath As String = q.GetParameter(ParameterNames.DiagsPath)
            Dim charWhitelist As String = q.GetParameter(ParameterNames.CharWhitelist)
            Dim language As String = q.GetParameter(ParameterNames.Language)
            If language Is Nothing Then language = "eng"

            Dim engineMode As Integer = 3
            If q.HasParameter(ParameterNames.EngineMode) Then
                engineMode = Integer.Parse(q.GetParameter(ParameterNames.EngineMode))
            End If

            Dim pageSegMode As Integer = 3
            If q.HasParameter(ParameterNames.PageSegMode) Then
                pageSegMode = GetPageSegmentationModeFromString(q.GetParameter(ParameterNames.PageSegMode))
            End If

            Dim text As String = DoOCR(p, scale, charWhitelist, diagspath, language, engineMode, pageSegMode)
            If text Is Nothing Then
                Throw New InvalidOperationException(My.Resources.NoTextCouldBeRead)
            End If
            Return Reply.Result(text)

        Catch ex As Exception
            Throw New InvalidOperationException(ex.Message)
        End Try

    End Function

    <Category(Category.Win32)>
    <Command("Reads text from within a rectangular area of a given window using OCR++.")>
    <Parameters("Either a) Those required to uniquely identify the window, plus 'startx','starty','endx' and 'endy' to define a rectangle; or b) ImageFilePath specifying the full path of an image stored on disk. Also Scale (default 4) is the amount to scale the image by before passing to OCR. Also DiagsPath, if specified, is a directoy to which diagnostics files (e.g. intermediate images) will be written.")>
    Private Function ProcessCommandReadTextOCRPlus(query As clsQuery) As Reply

        Try
            Dim p As clsPixRect = Nothing
            If query.HasParameter(ParameterNames.ImageFile) Then
                Dim imgFile As String = query.Parameters(ParameterNames.ImageFile)
                If Not File.Exists(imgFile) Then Throw New InvalidOperationException(My.Resources.FileNotFound & imgFile)
                p = New clsPixRect(CType(System.Drawing.Image.FromFile(imgFile), Bitmap))
            Else
                Dim w As clsUIWindow = mobjModel.IdentifyWindow(query)
                p = clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(query))
            End If

            If query.HasParameter(ParameterNames.FontName) Then
                Dim text As String = DoOCRPlus(p, query.GetParameter(ParameterNames.FontName))
                If text Is Nothing Then
                    Throw New InvalidOperationException(My.Resources.NoTextCouldBeRead)
                End If
                Return Reply.Result(text)
            Else
                Throw New InvalidOperationException(My.Resources.NoFontSpecified)
            End If
        Catch ex As Exception
            Throw New InvalidOperationException(ex.Message)
        End Try

    End Function

    ''' <summary>
    ''' Parses the string input to retrieve the integer value of Page segmentation 
    ''' mode as defined in the Enum PageSegMode
    ''' </summary>
    ''' <param name="psmInputString">String parameter value from the read stage, 
    ''' should be one of the values defined in the enum</param>
    ''' <returns>Integer value from 0 to 10</returns>
    Public Function GetPageSegmentationModeFromString(ByVal psmInputString As String) As Integer
        Dim psmString As String = psmInputString.ToLowerInvariant
        psmString.Trim()

        Return CInt([Enum].Parse(GetType(PageSegMode), psmString, True))
    End Function

    ''' <summary>
    ''' Enumeration to hold the values defined by Tesseract for page segmenttion mode
    ''' </summary>
    ''' <remarks>Integer values are passed in to the command when performing
    ''' OCR, string values are provided purely for usability</remarks>
    Enum PageSegMode
        OSD = 0
        AutoWithOSD = 1
        AutoNoOCR = 2
        Auto = 3
        Column = 4
        VerticalBlock = 5
        Block = 6
        Line = 7
        Word = 8
        CircledWord = 9
        Character = 10
        SparseText = 11
        SparseTextWithOSD = 12
        RawLine = 13
    End Enum

    Private Function TesseractVersionFromFileName(dir As String, ByRef fileName As String) As Version
        Const searchPattern As String = "tesseract*.exe"

        fileName = Directory.GetFiles(dir, searchPattern).FirstOrDefault
        If fileName Is Nothing Then
            Throw New BluePrismException(My.Resources.CouldNotFindTesseractExecutable0, Path.Combine(dir, searchPattern))
        End If
        Dim name = Path.GetFileNameWithoutExtension(fileName)
        Dim parts = name.Split("-"c)

        Dim version As Version = Nothing
        If parts.Length > 1 AndAlso Version.TryParse(parts.Last(), version) Then
            Return version
        End If
        Return mVersion3_05_01
    End Function

    Private ReadOnly mVersion3_05_01 As New Version(3, 5, 1)

    ''' <summary>
    ''' Performs OCR on the given bitmap.
    ''' </summary>
    ''' <param name="pr">The pixrect against which OCR should be performed.</param>
    ''' <param name="scale">The scale factor to use on the image. Something like 4
    ''' is usually a reasonable choice.</param>
    ''' <param name="charWhitelist">Optional whitelist of characters.</param>
    ''' <param name="diagspath">Path to write diagnostics images to, or Nothing to
    ''' not do that.</param>
    ''' <param name="language">Language code (as defined by Tesseract) to use.
    ''' e.g. "eng".</param>
    ''' <param name="pageSegmentationMode">Page Segmentation Mode (as defined by 
    ''' Tesseract) as integer from 0 to 10</param>
    ''' <returns>Returns the text read.</returns>
    Private Function DoOCR(ByVal pr As clsPixRect, ByVal scale As Decimal,
      ByVal charWhitelist As String, ByVal diagspath As String,
      ByVal language As String, engineMode As Integer, ByVal pageSegmentationMode As Integer) As String

        Dim appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location())
        Dim basedir = Path.Combine(appDir, "Tesseract")

        Dim exePath As String = Nothing
        Dim version = TesseractVersionFromFileName(basedir, exePath)

        'Process the image such that it is first monochromed, and then scaled up.
        Dim b As Bitmap = pr.ToBitmap()
        If diagspath IsNot Nothing Then
            b.Save(Path.Combine(diagspath, "ocr_input.png"))
        End If

        Dim grey_b As Bitmap = Grayscale.CommonAlgorithms.BT709.Apply(b)
        If diagspath IsNot Nothing Then
            grey_b.Save(Path.Combine(diagspath, "ocr_grey.png"))
        End If

        '       Dim f2 As New ResizeBicubic(b.Width * 4, b.Height * 4)
        Dim f2 As New ResizeBilinear(CInt(scale * b.Width), CInt(scale * b.Height))
        Dim large_b As Bitmap = f2.Apply(grey_b)
        If diagspath IsNot Nothing Then
            large_b.Save(Path.Combine(diagspath, "ocr_scaled.png"))
        End If

        Dim filename As String = BPUtil.GetRandomFilePath()
        Dim confname As String = BPUtil.GetRandomFilePath()
        Dim output As String = Nothing
        Try

            'Save the processed image to a temporary file, and then get rid of the
            'intermediate stuff.
            large_b.Save(filename, Imaging.ImageFormat.Tiff)
            If diagspath IsNot Nothing Then
                large_b.Save(Path.Combine(diagspath, "ocr_in.tiff"), Imaging.ImageFormat.Tiff)
            End If

            'Create a config file, for any extra control parameters
            'Note that in 3.03 (not yet released) you can specify these
            'directly on the command line.
            Dim config As String = ""
            If charWhitelist IsNot Nothing Then
                config &= "tessedit_char_whitelist " & charWhitelist & "\n"
            End If
            File.WriteAllText(confname, config)

            Dim args As New List(Of Object) From {
                    filename,
                    filename,
                    "-l", language
            }

            If (version > mVersion3_05_01) Then
                args.Add("--oem")
                args.Add(engineMode)
                args.Add("--psm")
                args.Add(pageSegmentationMode)
            Else
                args.Add("-psm")
                args.Add(pageSegmentationMode)
            End If

            args.Add(confname)

            'Run tesseract
            Dim si As New ProcessStartInfo() With {
                .CreateNoWindow = True,
                .UseShellExecute = False,
                .RedirectStandardError = True,
                .FileName = exePath,
                .Arguments = BPUtil.BuildCommandLineArgString(args.ToArray)
            }
            If (version > mVersion3_05_01) Then
                si.EnvironmentVariables.Add("TESSDATA_PREFIX", Path.Combine(basedir, "tessdata"))
            Else
                si.EnvironmentVariables.Add("TESSDATA_PREFIX", basedir)
            End If
            Using proc As Process = Process.Start(si)
                proc.WaitForExit()
                If proc.ExitCode <> 0 Then
                    Throw New BluePrismException(My.Resources.ErrorRunningTesseract0, proc.StandardError.ReadToEnd)
                End If
            End Using

            'Get the output
            output = File.ReadAllText(filename & ".txt").Trim()
            If diagspath IsNot Nothing Then
                File.WriteAllText(Path.Combine(diagspath, "output.txt"), output)
            End If

        Finally

            'Remove temporary files
            File.Delete(filename)
            File.Delete(filename & ".txt")
            'Dispose bitmaps
            large_b.Dispose()
            b.Dispose()
            grey_b.Dispose()

        End Try
        Return output

    End Function

    Private Function DoOCRPlus(p As clsPixRect, fontName As String) As String
        Dim response As String = Nothing
        Dim imageFolder As String = Path.GetTempPath() & "bpocrplus"
        Dim imageFilename = Path.GetRandomFileName()
        imageFilename = imageFolder & "\" & Path.ChangeExtension(imageFilename, "bmp")
        Dim bmp As Bitmap = Nothing
        Try
            If Not Directory.Exists(imageFolder) Then
                Directory.CreateDirectory(imageFolder)
            End If
            bmp = p.ToBitmap()
            bmp.Save(imageFilename)

            fontName = NormaliseOCRPlusFontName(fontName)
            Dim fontCreated = CreateOCRPlusFontFile(fontName)
            EnsureOcrPlusPlusRunning(fontName, fontCreated)

            Dim reader = ocrProcess.StandardOutput
            Dim writer = ocrProcess.StandardInput
            Dim command = $" --image ""{imageFilename}"" --region 0,0,{bmp.Width},{bmp.Height} --fonts ""{fontName}"""
            writer.WriteLine(command)

            response = reader.ReadLine()

            If response IsNot Nothing AndAlso response.Contains("""success"": true") Then
                Dim ocrResponse = JsonConvert.DeserializeObject(Of OcrPlusJsonResponse)(response)
                response = ocrResponse.text
            Else
                response = Nothing
            End If
        Catch
            response = Nothing
        End Try

        Try
            If bmp IsNot Nothing Then bmp.Dispose()
            If File.Exists(imageFilename) Then File.Delete(imageFilename)
        Catch
            'something went wrong
        End Try

        Return response
    End Function

    Private Sub EnsureOcrPlusPlusRunning(fontName As String, fontCreated As Boolean)
        Dim startProcess = False
        Try
            If ocrProcess Is Nothing Then
                startProcess = True
            Else
                If ocrProcess.HasExited Then
                    ocrProcess.Close()
                    startProcess = True
                End If
            End If

            If startProcess Then
                Dim fontFolder = GetOCRPlusFontFolderPath()

                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ocrPlusExe)

                startInfo.Arguments = $" -d ""{fontFolder}"" --parent-pid {Process.GetCurrentProcess().Id}"
                startInfo.UseShellExecute = False
                startInfo.RedirectStandardOutput = True
                startInfo.RedirectStandardInput = True
                startInfo.RedirectStandardError = True
                startInfo.CreateNoWindow = True
                ocrProcess = System.Diagnostics.Process.Start(startInfo)

                RemoveHandler AppDomain.CurrentDomain.ProcessExit, AddressOf OcrPlusPlus_Kill
                AddHandler AppDomain.CurrentDomain.ProcessExit, AddressOf OcrPlusPlus_Kill
            Else
                If fontCreated Then
                    Dim reader = ocrProcess.StandardOutput
                    Dim writer = ocrProcess.StandardInput
                    Dim jsonFilename = GetOCRPlusFontFilePath(fontName)
                    Dim command = $" -json ""{jsonFilename}"""
                    writer.WriteLine(command)

                    reader.ReadLine() 'keep reads in step
                End If
            End If
        Catch
            'something went wrong
        End Try
    End Sub

    Private Shared Sub OcrPlusPlus_Kill(sender As Object, e As EventArgs)
        If ocrProcess IsNot Nothing Then
            Try
                Dim writer = ocrProcess.StandardInput
                Dim command = "exit"
                writer.WriteLine(command)
            Catch
                'something went wrong
            End Try

            ocrProcess.Close()
            ocrProcess = Nothing
        End If
    End Sub


    Private Function CreateOCRPlusFontFile(fontName As String) As Boolean
        If File.Exists(GetOCRPlusFontFilePath(fontName)) Then
            Return False
        End If

        Dim json = FontReader.GetFontDataOcrPlus(fontName)
        Dim fontFamily = GetOCRPlusFontFamily(fontName)

        Dim temp = GetOCRPlusFontFolderPath() & "\" & fontFamily
        Directory.CreateDirectory(temp)

        Dim jsonFilename As String = GetOCRPlusFontFilePath(fontName)
        File.WriteAllText(jsonFilename, json)

        Return True
    End Function

    Private Function GetOCRPlusFontFilePath(fontName As String) As String
        Dim fontFamily = GetOCRPlusFontFamily(fontName)

        Dim temp = Path.Combine(GetOCRPlusFontFolderPath(), fontFamily)

        Dim jsonFilename As String = Path.Combine(temp, fontName & ".bpfont.json")

        Return jsonFilename
    End Function

    Private Function GetOCRPlusFontFolderPath() As String
        Dim folder = Path.GetTempPath() & "bpocrplusfonts"
        Return folder
    End Function

    Private Function GetOCRPlusFontFamily(fontName As String) As String
        Dim words As String() = fontName.Split(" "c)
        Dim fontFamily As String = String.Empty

        For Each word As String In words
            If Char.IsNumber(word(0)) Then
                fontFamily = fontFamily.Trim()
                Return fontFamily
            End If
            fontFamily = $"{fontFamily}{word} "
        Next
        Return fontFamily
    End Function

    Private Function NormaliseOCRPlusFontName(fontName As String) As String
        fontName = fontName.Replace("(", "")
        fontName = fontName.Replace(")", "")
        Return fontName
    End Function

    <Category(Category.Win32)>
    <Command("Reads text from within a rectangular area Of a given window Using character matching.")>
    <Parameters("Either a) Those required To uniquely identify the window, plus 'startx','starty','endx' and 'endy' to define a rectangle; or b) ImageFilePath specifying the full path of an image stored on disk. The font information is specified by the 'font', 'colour' and 'backgroundcolour' parameters, where font must identify a pre-defined font (or blank to use the default ""System"" font). The 'colour' is the colour of text to read, specified as hex RGB, e.g. 000000 for black (the default) or FF0000 for red. Instead of (but not as well as!) 'colour', 'backgroundcolour' can be specified - again, this can be a colour, and any colour not matching will be considered to be text. It can also be 'auto' in which case the background colour will be automatically detected. Optionally one of multiline=true to invoke the multiline method, or singleline=true to invoke the single-line method may be used.")>
    Private Function ProcessCommandReadText(ByVal q As clsQuery) As Reply
        Try
            Dim p As clsPixRect = Nothing
            If q.HasParameter(ParameterNames.ImageFile) Then
                Dim imgFile As String = q.Parameters(ParameterNames.ImageFile)
                If Not File.Exists(imgFile) Then Throw New InvalidOperationException(My.Resources.FileNotFound & imgFile)
                p = New clsPixRect(CType(Image.FromFile(imgFile), Bitmap))

            Else
                Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
                p = clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(q))

            End If
            Return Reply.Result(DoCharMatching(p, q, ""))

        Catch ex As Exception
            Throw New InvalidOperationException(ex.Message)
        End Try
    End Function

    <Category(Category.Win32)>
    <Command("Reads text from multiple elements defined in a list using character matching")>
    <Parameters("Those required to uniquely identify the host window, plus 'startx'," &
     "'starty', 'endx' and 'endy' to define rectangle. Optionally, 'listdirection' " &
     "to specify a direction for the list ('TopDown', 'BottomUp', 'LeftToRight' or " &
     "'RightToLeft'), 'padding' to define the padding between elements, and " &
     "'firstelement' and 'lastelement' to define the list range")>
    Private Function ProcessCommandReadTextMultiElement(ByVal q As clsQuery) As Reply
        ' Get the first and last element numbers - default is 1 for both of them
        Dim first As Integer
        If q.HasParameter(ParameterNames.FirstElement) _
         Then first = CInt(q.GetParameter(ParameterNames.FirstElement)) _
         Else first = 1

        Dim last As Integer
        If q.HasParameter(ParameterNames.LastElement) _
         Then last = CInt(q.GetParameter(ParameterNames.LastElement)) _
         Else last = 1

        ' Silliness check
        If first > last Then Throw New BluePrismException(
         My.Resources.FirstelementMustBeLessThanOrEqualToLastelement01,
         first, last)

        ' Get the window to use
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)

        ' Read the texts into a collection
        Dim xdoc As New XmlDocument()
        Dim root As XmlNode = xdoc.AppendChild(xdoc.CreateElement("collection"))

        For i As Integer = first To last
            ' For each element, capture a screenshot of the region and read the
            ' text within it.
            Dim subQuery As clsQuery = q.WithParams(
             New clsParamSetting(ParameterNames.ElementNumber, CStr(i)))

            Dim p As clsPixRect =
             clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(subQuery))

            Dim txt As String = DoCharMatching(p, subQuery, "")

            ' Add it to the collection as we go along
            Dim row As XmlElement = xdoc.CreateElement("row")
            row.AppendChild(CreateCollectionFieldXML(xdoc, txt, "text", "Text"))
            root.AppendChild(row)
        Next

        ' and return it
        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.Win32)>
    <Command("Reads text from multiple elements defined in a grid using character matching")>
    <Parameters("Those required to uniquely identify the host window, plus 'startx'," &
     "'starty', 'endx' and 'endy' to define rectangle. Also, 'gridschema' to " &
     "provide the layout of the grid from which to read the text")>
    Private Function ProcessCommandReadTextGrid(ByVal q As clsQuery) As Reply
        ' Pick out the schema from the parameters
        Dim schema As GridSchema =
         New GridSchema(q.GetParameter(ParameterNames.GridSchema))

        ' Get the window and capture an up to date screenshot of it
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)

        ' Build up the collection
        Dim xdoc As New XmlDocument()
        Dim root As XmlNode = xdoc.AppendChild(xdoc.CreateElement("collection"))

        For colNo As Integer = 1 To schema.ColumnCount
            For rowNo As Integer = 1 To schema.RowCount
                ' Do each col/row combo in turn - do so by setting the column/row
                ' params in the query, screenshotting the region reading the text
                Dim subQuery As clsQuery = q.WithParams(
                 New clsParamSetting(ParameterNames.ColumnNumber, CStr(colNo)),
                 New clsParamSetting(ParameterNames.RowNumber, CStr(rowNo))
                )

                Dim p As clsPixRect =
                 clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(subQuery))
                Dim txt As String = DoCharMatching(p, subQuery, "")

                ' Add it to the collection as we go along
                Dim row As XmlElement = xdoc.CreateElement("row")
                row.AppendChild(
                 CreateCollectionFieldXML(xdoc, txt, "text", "Column " & colNo))
                root.AppendChild(row)
            Next
        Next

        Return Reply.Result(xdoc.OuterXml)
    End Function

    ''' <summary>
    ''' Performs character matching using the supplied query, against the specified
    ''' bitmap.
    ''' </summary>
    ''' <param name="pr">The pixrect against which the char matching should be
    ''' performed.</param>
    ''' <param name="query">The query specifying the char matching query
    ''' parameters.</param>
    ''' <param name="fontname">Carries back the name of the font used during char
    ''' matching.</param>
    ''' <returns>Returns the text read.</returns>
    ''' <exception cref="InvalidValueException">If any of the parameters in the query
    ''' were in an invalid format</exception>
    ''' <exception cref="CharMatchingException">If any errors occur while attempting
    ''' to match the characters in the given image using the specified query.
    ''' </exception>
    Private Function DoCharMatching(ByVal pr As clsPixRect,
     ByVal query As clsQuery, ByRef fontname As String) As String
        ' Determine font to use - top billing is given to the fontname specified in
        ' the stage. If that's not present, use the font name specified in the
        ' element. Failing both of those, fall back to 'System'.
        fontname = query.Parameters(ParameterNames.Font)
        If fontname = "" Then fontname = query.Parameters(ParameterNames.FontName)
        If fontname = "" Then fontname = "System"

        'And colours...
        Dim fg As Integer = -1, bg As Integer = -1

        ' Foreground colour
        Dim fgStr As String = query.Parameters(ParameterNames.Colour)
        If Not FontReader.TryParseColourString(fgStr, fg) Then Throw New _
         InvalidValueException(My.Resources.InvalidForegroundColourProvided0, fgStr)

        ' Background colour - "auto" uses the modal colour from the image
        Dim bgStr As String = query.Parameters(ParameterNames.BackgroundColour)
        If "auto".Equals(bgStr, StringComparison.CurrentCultureIgnoreCase) Then
            bg = pr.GetModalColour()

        ElseIf bgStr IsNot Nothing Then
            If Not FontReader.TryParseColourString(bgStr, bg) Then Throw New _
             InvalidValueException(My.Resources.InvalidBackgroundColourProvided0, bgStr)

        End If

        'Validate colours and decide which to use
        If fg <> -1 AndAlso bg <> -1 Then Throw New CharMatchingException(
         My.Resources.OnlyOneOfColourAndBackgroundColourMayBeSpecifiedFoundColour0BackgroundColour1, fgStr, bgStr)

        If fg = -1 AndAlso bg = -1 Then fg = 0 ' default behaviour - ie. fg=black

        Dim useBg As Boolean = (bg <> -1)
        Dim activeCol As Integer = CInt(IIf(useBg, bg, fg))

        'Read text...
        Dim multiline As Boolean = query.GetBoolParam(ParameterNames.Multiline)
        Dim origAlg As Boolean = query.GetBoolParam(ParameterNames.OrigAlgorithm)

        Dim outText As String
        If origAlg Then
            outText = FontReader.ReadText(useBg, pr, fontname, activeCol)

        ElseIf multiline Then
            Dim eraseBlocks As Boolean =
             query.GetBoolParam(ParameterNames.EraseBlocks)

            outText = FontReader.ReadTextMultiline(
             useBg, pr, fontname, activeCol, eraseBlocks)

        Else
            outText = FontReader.ReadTextSingleLine(
             useBg, pr, fontname, activeCol)

        End If

        'Generate diagnostics output if required...
        If clsConfig.LoggingCharMatching Then
            Using bitmap As Bitmap = pr.ToBitmap()
                clsConfig.LogCharMatching(outText, bitmap)
            End Using
        End If

        Return outText
    End Function

    <Category(Category.Win32)>
    <Command("Verifies a character matching sample stored in an image file by " &
     "reading the image and comparing the output of the character matching engine " &
     "with a value stored in a text file.")>
    <Parameters("ImageFilePath, specifying the full path of an image stored on " &
     "disk; ReferenceFile, specifying the full path of a text file stored on " &
     "disk; Font, specifying the name of the font; Colour, specifying the " &
     "colour of text to read as hex RGB, e.g. 000000 for black (the default) " &
     "or FF0000 for red. Optionally one of Multiline=true to invoke the " &
     "multiline method, or SingleLine=true to invoke the single-line method may " &
     "be used.")>
    Private Function ProcessCommandVerifyFontRecSample(ByVal objQuery As clsQuery) As Reply
        'Get sample reference text
        Dim referenceText As String = ""
        Dim referenceFilePath As String = objQuery.GetParameter(ParameterNames.ReferenceFile)
        If referenceFilePath Is Nothing Then
            Throw New InvalidOperationException(My.Resources.MissingArgumentForParameter & ParameterNames.ReferenceFile.ToString())
        End If
        If Not File.Exists(referenceFilePath) Then
            Throw New InvalidOperationException(My.Resources.ReferenceFileNotFound & referenceFilePath)
        End If
        referenceText = File.ReadAllText(referenceFilePath)

        'Get sample image file
        Dim p As clsPixRect = Nothing
        Dim imageFilePath As String = objQuery.GetParameter(ParameterNames.ImageFile)
        If imageFilePath Is Nothing Then
            Throw New InvalidOperationException(My.Resources.MissingArgumentForParameter & ParameterNames.ImageFile.ToString())
        End If
        If Not File.Exists(imageFilePath) Then
            Throw New InvalidOperationException(My.Resources.FileNotFound & imageFilePath)
        End If
        p = New clsPixRect(CType(Image.FromFile(imageFilePath), Bitmap))

        Dim sOutText As String = Nothing
        Dim fontName As String = Nothing

        sOutText = DoCharMatching(p, objQuery, fontName)

        'Remove character conflicts
        Dim cleanTextRead As String = sOutText
        Dim cleanReferenceText As String = referenceText
        Dim conflictLists As ICollection(Of ICollection(Of CharData)) =
         FontReader.GetFontData(fontName).GetConflictingCharacterGroups()

        For Each conflictList As List(Of CharData) In conflictLists
            For Each conflictChar As CharData In conflictList
                cleanTextRead = cleanTextRead.Replace(conflictChar.Value, "")
                cleanReferenceText = cleanReferenceText.Replace(conflictChar.Value, "")
            Next
        Next

        If cleanReferenceText = cleanTextRead Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(My.Resources.ValuesDoNotMatch & sOutText & vbCrLf & vbCrLf & referenceText)
        End If
    End Function

    <Category(Category.Win32)>
    <Command("Matches an image in the specified region.")>
    <Parameters("Those required to uniquely identify the host window, " &
     "plus 'startx','starty','endx' and 'endy' to define the rectangle, " &
     "'imagevalue' with the image to match against and " &
     "potentially those parameters which define a list element or grid cell.")>
    Private Function ProcessCommandMatchImage(ByVal q As clsQuery) As Reply
        Dim imgStr As String = q.GetParameter(ParameterNames.ImageValue)
        If imgStr = "" Then imgStr = q.GetParameter(ParameterNames.ElementSnapshot)
        If imgStr = "" Then Throw New BluePrismException(
         My.Resources.AnElementsnapshotOrImagevalueMustBeProvidedToMatchAgainst)

        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        Dim pStr As String =
         clsPixRect.CapturePixRectString(w.Hwnd, GetRegionWindowBounds(q))

        If clsPixRect.Matches(pStr, imgStr) _
         Then Return Reply.Result(True) _
         Else Return Reply.Result(False)
    End Function

    <Category(Category.Win32)>
    <Command("Checks if the specified region contains a given image.")>
    <Parameters("Those required to uniquely identify the host window, " &
     "plus 'startx','starty','endx' and 'endy' to define the rectangle, " &
     "'imagevalue' with the image to match against and " &
     "potentially those parameters which define a list element or grid cell.")>
    Private Function ProcessCommandContainsImage(ByVal q As clsQuery) As Reply
        ' Get the image to test for from the query
        ' ImageValue overrides ElementSnapshot, but at least one must be present
        Dim imgStr As String = q.GetParameter(ParameterNames.ImageValue)
        If imgStr = "" Then imgStr = q.GetParameter(ParameterNames.ElementSnapshot)
        If imgStr = "" Then Throw New BluePrismException(
         My.Resources.AnElementsnapshotOrImagevalueMustBeProvidedToMatchAgainst)
        Dim img As clsPixRect = New clsPixRect(imgStr)

        ' Get the window and snapshot the region from it
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        Dim pr As clsPixRect = clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(q))

        ' Check if the image to test for is contained within the region snapshot.
        Return Reply.Result(pr.Contains(img))
    End Function

    <Category(Category.Win32)>
    <Command("Checks if the specified region contains a given colour.")>
    <Parameters("Those required to uniquely identify the host window, " &
     "plus 'startx','starty','endx' and 'endy' to define the rectangle, " &
     "'colour' with the colour to match against and " &
     "potentially those parameters which define a list element or grid cell.")>
    Private Function ProcessCommandContainsColour(ByVal q As clsQuery) As Reply
        ' Get the colour to match against (mandatory)
        Dim colInt As Integer
        Dim colStrg As String = q.GetParameter(ParameterNames.Colour)
        If colStrg = "" Then Throw New BluePrismException(
         My.Resources.AColourMustBeProvidedToMatchAgainst)

        ' Check it is a valid colour
        If Not FontReader.TryParseColourString(colStrg, colInt) Then Throw New _
         InvalidValueException(My.Resources.InvalidColourProvided0, colStrg)

        'Get the window and snapshot the region from it
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        Dim pr As clsPixRect = clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(q))

        ' Get map of colours in region - condition is met if passed colour is in list
        Return Reply.Result(pr.GetColourDistribution().ContainsKey(colInt))
    End Function

    <Category(Category.Win32)>
    <Command("Checks if the specified region contains a single colour.")>
    <Parameters("Those required to uniquely identify the host window, " &
       "plus 'startx','starty','endx' and 'endy' to define the rectangle, " &
       "optionally 'colour' with the colour to match against and " &
       "potentially those parameters which define a list element or grid cell.")>
    Private Function ProcessCommandUniformColour(ByVal q As clsQuery) As Reply
        ' Get the colour (optional)
        Dim colInt As Integer = -1
        Dim colStrg As String = q.GetParameter(ParameterNames.Colour)
        If Not FontReader.TryParseColourString(colStrg, colInt) Then Throw New _
         InvalidValueException(My.Resources.InvalidColourProvided0, colStrg)

        'Get the window and snapshot the region from it
        Dim w As clsUIWindow = mobjModel.IdentifyWindow(q)
        Dim pr As clsPixRect = clsPixRect.Capture(w.Hwnd, GetRegionWindowBounds(q))

        ' Get list of colours in region
        Dim cols As IDictionary(Of Integer, Integer) = pr.GetColourDistribution()

        ' Condition is not met if region contains more than 1 colour
        ' Condition is met if no colour was specified, or the specified colour
        ' matches that found in the region
        Return Reply.Result(
            cols.Count = 1 AndAlso (colInt = -1 OrElse cols.ContainsKey(colInt)))
    End Function

    <Category(Category.Win32)>
    <Command("Gets any text found within a rectangular area of a given window. " &
     "This retrieves text drawn to the window by the application.")>
    <Parameters("Those required to uniquely identify the window, " &
     "plus 'startx','starty','endx' and 'endy' to define the rectangle. ")>
    Private Function ProcessCommandGetText(ByVal q As clsQuery) As Reply
        Try
            Dim w = mobjModel.IdentifyWindow(q)
            Dim b = GetRegionWindowBounds(q, w)
            Dim text = mobjModel.FindTextByRect(w, b)
            If text.Count = 0 Then
                Throw New InvalidOperationException(NearByTextExceptionMessage(w, b, False))
            End If

            Return Reply.Result(CollectionUtil.Join(text.Select(Function(x) x.Text), " "))

        Catch ex As Exception
            Throw New InvalidOperationException(ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Creates the exception message string for the near by text.
    ''' </summary>
    Private Function NearByTextExceptionMessage(w As clsUIWindow, b As Rectangle, center As Boolean) As String
        Dim r = Rectangle.Inflate(b, 50, 50)
        Dim nearbyText = If(center, mobjModel.FindTextByRectCenter(w, r), mobjModel.FindTextByRect(w, r))
        If nearbyText.Count = 0 Then Return My.Resources.NoTextAtThatLocation
        Dim sb As New StringBuilder
        For Each t In nearbyText
            sb.AppendLine()
            sb.AppendFormat("Found '{0}' at X: {1} Y: {2} Width: {3} Height {4}", t.Text, t.X, t.Y, t.Width, t.Height)
        Next
        Return String.Format("No text at that location, but: {0}", sb.ToString)
    End Function

    <Category(Category.Win32)>
    <Command("Gets any text where the centre of the text can be found within a reigon. " &
     "This retrieves text drawn to the window or any of its descendants by the application.")>
    <Parameters("Those required to uniquely identify the window, " &
     "plus 'startx','starty','endx' and 'endy' to define the rectangle. ")>
    Private Function ProcessCommandGetTextCenter(ByVal q As clsQuery) As Reply
        Try
            Dim window = mobjModel.IdentifyWindow(q)
            Dim bounds = GetRegionWindowBounds(q, window)
            Dim text = mobjModel.FindTextByRectCenter(window, bounds)
            If text.Count = 0 Then
                Throw New InvalidOperationException(NearByTextExceptionMessage(window, bounds, True))
            End If

            Dim sortedText = text.OrderBy(Function(a) a.X).ThenBy(Function(b) b.Y)
            Dim sortedStrings = sortedText.Select(Function(e) e.Text)

            Return Reply.Result(String.Join(" ", sortedStrings))

        Catch ex As Exception
            Throw New InvalidOperationException(ex.Message)
        End Try
    End Function

#End Region

#Region "Query Command Handlers - Citrix"

    <Category(Category.Citrix)>
    <Command("Starts the citrix session")>
    <Parameters("Path the path to the session file (.ica)")>
    Private Function ProcessCommandStartCitrixSession(ByVal objQuery As clsQuery) As Reply
        Dim sErr As String = Nothing
        Dim sPath As String = objQuery.GetParameter(ParameterNames.Path)
        If Launch(sPath, sErr) Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Citrix)>
    <Command("Sends a mouse click to the Citrix session")>
    <Parameters("TargX the x co-ordinate, TargY the y co-ordinate, plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.")>
    Private Function ProcessCommandCtxMouseClick(ByVal objQuery As clsQuery) As Reply
        Dim targX As Integer = objQuery.GetIntParam(ParameterNames.TargX, False)
        Dim targY As Integer = objQuery.GetIntParam(ParameterNames.TargY, False)

        Dim buttonString As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim button As MouseButton = GetButtonFromString(buttonString)

        Dim objSession As Citrix.ISession = mCitrix.Session
        objSession.ReplayMode = True
        Dim m As Citrix.IMouse = objSession.Mouse
        m.SendMouseMove(0, 0, targX, targY)
        m.SendMouseDown(button, 0, targX, targY)
        m.SendMouseUp(button, 0, targX, targY)
        objSession.ReplayMode = False

        Return Reply.Ok
    End Function

    <Category(Category.Citrix)>
    <Command("Sends keys to the Citrix session")>
    <Parameters("NewText the formatted string specifying the keys to send")>
    Private Function ProcessCommandCtxSendKeys(ByVal objQuery As clsQuery) As Reply
        Dim sNewText As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim objSession As Citrix.ISession = mCitrix.Session
        objSession.ReplayMode = True
        Dim k As Citrix.IKeyboard = objSession.Keyboard

        For Each i As KeyInstruction In Me.GetKeyCodes(sNewText)
            If (i.Mode And SendKeyMode.Down) > 0 Then
                k.SendKeyDown(i.KeyCode)
            End If
            If (i.Mode And SendKeyMode.Up) > 0 Then
                k.SendKeyUp(i.KeyCode)
            End If
        Next
        objSession.ReplayMode = False
        Return Reply.Ok
    End Function

#End Region

#Region "Query Command Handlers - .NET"

    <Category(Category.DotNet)>
    <Command("Set the value of a property on a .NET control")>
    <Parameters("Those required to uniquely identify the window, plus 'propname' identifying the property name and 'newtext' containing the value")>
    <Examples("setdotnetcontrolproperty x=12 y=32 propname=Checked newtext=True")>
    Private Function ProcessCommandSetDotNetControlProperty(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim c As ControlProxy = ControlProxy.FromHandle(w.Hwnd)
        If c Is Nothing Then
            Throw New InvalidOperationException(My.Resources.CouldNotGetControlProxy)
        End If

        Dim propname As String = objQuery.GetParameter(ParameterNames.PropName)
        If propname Is Nothing Then Throw New InvalidOperationException(My.Resources.PropertyNameMustBeSpecifiedUsingThePropnameParameter)

        Dim sNewText As String = objQuery.GetParameter(ParameterNames.NewText)
        If sNewText Is Nothing Then Throw New InvalidOperationException(My.Resources.PropertyValueMustBeSpecifiedUsingTheNewtextParameter)

        'Get the property's current value, which we only need so we can figure out
        'what Type it is...
        Dim propval As Object = c.GetValue(propname)
        If propval Is Nothing Then
            Throw New InvalidOperationException(String.Format(My.Resources.CannotAccessProperty0, propname))
        End If

        Dim args() As Object = {sNewText}
        Dim newval As Object = propval.GetType.InvokeMember("Parse", BindingFlags.InvokeMethod, Nothing, propval, args)
        c.SetValue(propname, newval)
        Return Reply.Ok
    End Function

    <Category(Category.DotNet)>
    <Command("Get the value of a property from a .NET control")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' identifying the property name")>
    <Examples("getdotnetcontrolproperty x=12 y=32 newtext=CurrentRowIndex")>
    Private Function ProcessCommandGetDotNetControlProperty(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim c As ControlProxy = ControlProxy.FromHandle(w.Hwnd)
        If c Is Nothing Then
            Throw New InvalidOperationException(My.Resources.CouldNotGetControlProxy)
        End If

        Dim propname As String = objQuery.GetParameter(ParameterNames.NewText)
        If propname Is Nothing Then Throw New InvalidOperationException(My.Resources.PropertyNameMustBeSpecifiedUsingTheNewtextParameter)

        Dim o As Object = c.GetValue(propname)
        If o Is Nothing Then Throw New InvalidOperationException(My.Resources.CouldNotGetProperty)

        Return Reply.Result(o.ToString())
    End Function

    ''' <summary>
    ''' Turn a DataSource and DataMember pair into an IEnumerable.
    ''' </summary>
    ''' <param name="datasource"></param>
    ''' <param name="datamember"></param>
    ''' <returns>An IEnumerable interface, or Nothing</returns>
    ''' <remarks>I am not entirely sure why or how this works, since the documentation
    ''' for these interfaces leaves a lot to be desired.</remarks>
    Private Function GetResolvedDataSource(ByVal datasource As Object, ByVal datamember As String) As IEnumerable
        If datasource IsNot Nothing Then
            Dim source1 As IListSource = TryCast(datasource, IListSource)
            If source1 IsNot Nothing Then
                Dim list1 As IList = source1.GetList()
                If Not source1.ContainsListCollection Then
                    Return list1
                End If
                If (list1 IsNot Nothing) AndAlso (TypeOf (list1) Is ITypedList) Then
                    Dim list2 As ITypedList = TryCast(list1, ITypedList)
                    Dim collection1 As PropertyDescriptorCollection = list2.GetItemProperties(New PropertyDescriptor() {})
                    If (collection1 Is Nothing) OrElse (collection1.Count = 0) Then
                        Return Nothing
                    End If
                    Dim desc1 As PropertyDescriptor = Nothing
                    If (datamember Is Nothing) OrElse (datamember.Length = 0) Then
                        desc1 = collection1(0)
                    Else
                        desc1 = collection1.Find(datamember, True)
                    End If
                    If desc1 IsNot Nothing Then

                        Dim obj1 As Object = list1(0)
                        Dim obj2 As Object = desc1.GetValue(obj1)
                        If (obj2 IsNot Nothing) AndAlso (TypeOf (obj2) Is IEnumerable) Then
                            Return TryCast(obj2, IEnumerable)
                        End If
                    End If
                    Return Nothing
                End If
            End If
            If TypeOf (datasource) Is IEnumerable Then
                Return TryCast(datasource, IEnumerable)
            End If
        End If
        Return Nothing
    End Function

    <Category(Category.DotNet)>
    <Command("Get the data from a .NET control")>
    <Parameters("Those required to uniquely identify the window, plus 'newtext' identifying the property name")>
    <Examples("getdatagriddata x=12 y=32")>
    Private Function ProcessCommandGetDataGridData(ByVal objQuery As clsQuery) As Reply
        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim c As ControlProxy = ControlProxy.FromHandle(w.Hwnd)
        If c Is Nothing Then
            Throw New InvalidOperationException(My.Resources.CouldNotGetControlProxy)
        End If

        'Try and get the underlying data from the grid...
        Dim o As Object = c.GetValue("DataSource")
        Dim s As String = CStr(c.GetValue("DataMember"))
        Dim en As IEnumerable = GetResolvedDataSource(o, s)
        If en Is Nothing Then
            Throw New InvalidOperationException(My.Resources.CouldNotResolveDataSource)
        End If

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)

        For Each item As Object In en
            Dim ct As ICustomTypeDescriptor = TryCast(item, ICustomTypeDescriptor)
            If ct Is Nothing Then
                Throw New InvalidOperationException(My.Resources.ExpectedDataItemsToImplementICustomTypeDescriptor)
            End If
            Dim rowel As XmlElement = xdoc.CreateElement("row")
            root.AppendChild(rowel)
            For Each prop As PropertyDescriptor In ct.GetProperties()
                Dim value As Object = prop.GetValue(item)
                rowel.AppendChild(CreateCollectionFieldXML(xdoc, value.ToString(), "text", prop.Name))
            Next
        Next

        Return Reply.Result(xdoc.OuterXml)
    End Function

#End Region

#Region "Query Command Handlers - COM"

    <Category(Category.ComActiveX)>
    <Command("Get all IHtmlDocument interfaces from an Internet Explorer ActiveX control that have been hooked in the target application.")>
    <Parameters("None.")>
    <Examples("gethtmldocuments")>
    Private Function ProcessCommandGetHtmlDocuments(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim sResult As String = Nothing
        Dim cmd As String = "get_htmldocuments"
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)

        Return Reply.Result(sResult)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Marshal the interface to an IHTMLDocument returned by the gethtmldocuments query.")>
    <Parameters("newtext should contain an ID from a list returned by gethtmldocuments")>
    <Examples("marshalhtmldocument newtext=0C432330")>
    Private Function ProcessCommandMarshalHtmlDocument(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim id As String = objQuery.GetParameter(ParameterNames.NewText)
        If id Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NODOCUMENTSPECIFIED)
        End If

        Dim sResult As String = Nothing
        Dim cmd As String = "marshal_htmldocument " & id
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)

        Return Reply.Result(sResult)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the HTML source from an IHTMLDocument returned by the gethtmldocuments query. The source for the document and all nested frames is returned.")>
    <Parameters("newtext should contain an ID from a list returned by gethtmldocuments")>
    <Examples("gethtmlsource newtext=0C432330")>
    Private Function ProcessCommandGetHtmlSource(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim id As String = objQuery.GetParameter(ParameterNames.NewText)
        If id Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NODOCUMENTSPECIFIED)
        End If

        Dim sResult As String = Nothing
        Dim cmd As String = "gethtmlsource " & id
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)

        Return Reply.Result(sResult)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the entire contents of an MSFlexGrid in collection form.")>
    <Parameters("Those required to uniquely identify the window. Optionally, method=old can be specified to use the old (slower) method of reading.")>
    <Examples("getmsflexgridcontents classname=MSFlexGridWndClass")>
    Private Function ProcessCommandGetMsFlexGridContents(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim sResult As String = Nothing
        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)

        'Decide whether to use the old method (see below) or not. Unless explicitly
        'specified, we'll always use the new one.
        Dim oldmethod As Boolean = False
        Dim smethod As String = objQuery.GetParameter(ParameterNames.Method)
        If smethod IsNot Nothing AndAlso smethod.ToLower(CultureInfo.InvariantCulture) = "old" Then
            oldmethod = True
        End If

        If oldmethod Then

            'This is the old method, which uses individual property gets to read
            'the number of rows and columns and then loop over the grid cells
            'reading each with a further property get.

            'Get the number of rows and columns...
            Dim cmd As String = "property_get " & w.Handle.ToString("X") & ",Rows"
            If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
            Dim iRows As Integer = Integer.Parse(sResult)
            cmd = "property_get " & w.Handle.ToString("X") & ",Cols"
            If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
            Dim iCols As Integer = Integer.Parse(sResult)

            'Read each row in turn...
            For row As Integer = 0 To iRows - 1

                Dim rowel As XmlElement = xdoc.CreateElement("row")
                For col As Integer = 0 To iCols - 1
                    cmd = "property_get " & w.Handle.ToString("X") & ",TextMatrix," & row.ToString() & "," & col.ToString()
                    If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
                    rowel.AppendChild(CreateCollectionFieldXML(xdoc, sResult, "text", "Column" & col.ToString()))
                Next
                root.AppendChild(rowel)
            Next
        Else
            'This is the new method that uses a custom BPInjAgent command to read
            'the whole grid at once...

            'Get the number of rows and columns...
            Dim cmd As String = "flex_readall " & w.Handle.ToString("X")
            If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
            Dim aData() As String = sResult.Split(Chr(9))
            Dim iRows As Integer = Integer.Parse(aData(0))
            Dim iCols As Integer = Integer.Parse(aData(1))
            If aData.Length <> 2 + iRows * iCols Then Throw New InvalidOperationException(String.Format(My.Resources.MismatchedDataReturned0RowsBy1ColsShouldHaveBeen22ElementsButWas3, iRows, iCols.ToString, (iRows * iCols), aData.Length))

            'Read each row in turn...
            Dim pos As Integer = 2
            For row As Integer = 0 To iRows - 1

                Dim rowel As XmlElement = xdoc.CreateElement("row")
                For col As Integer = 0 To iCols - 1
                    rowel.AppendChild(CreateCollectionFieldXML(xdoc, aData(pos), "text", "Column" & col.ToString()))
                    pos += 1
                Next
                root.AppendChild(rowel)
            Next
        End If

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the entire contents of a status bar")>
    <Parameters("Those required to uniquely identify the window.")>
    <Examples("getlistviewcontents classname=ListView20WndClass")>
    Private Function ProcessCommandGetStatusBarContents(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)

        Dim sResult As String = Nothing

        'Get the number of rows and columns...
        Dim cmd As String = "statusbar_read " & w.Handle.ToString("X")
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Dim aData() As String = sResult.Split(Chr(9))
        Dim iPanels As Integer = Integer.Parse(aData(0))
        If aData.Length <> 1 + iPanels Then Throw New InvalidOperationException(String.Format(My.Resources.MismatchedDataReturned0PanelsShouldHaveBeen01ElementsButWas1, iPanels, aData.Length))

        'Read each panel in turn...
        Dim pos As Integer = 1
        Dim panelel As XmlElement = xdoc.CreateElement("row")
        For panel As Integer = 0 To iPanels - 1
            panelel.AppendChild(CreateCollectionFieldXML(xdoc, aData(pos), "text", "Column" & panel.ToString()))
            pos += 1
        Next
        root.AppendChild(panelel)

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the entire contents of an Listview in collection form.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Examples("getlistviewcontents classname=ListView20WndClass")>
    Private Function ProcessCommandGetListViewContents(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)

        Dim sResult As String = Nothing

        'Get the number of rows and columns...
        Dim cmd As String = "listview_readall " & w.Handle.ToString("X")
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Dim aData() As String = sResult.Split(Chr(9))
        Dim iRows As Integer = Integer.Parse(aData(0))
        Dim iCols As Integer = Integer.Parse(aData(1))
        If aData.Length <> 2 + iRows * iCols Then Throw New InvalidOperationException(String.Format(My.Resources.MismatchedDataReturned0RowsBy1ColsShouldHaveBeen22ElementsButWas3, iRows, iCols.ToString, (iRows * iCols), aData.Length))

        'Read each row in turn...
        Dim pos As Integer = 2
        For row As Integer = 0 To iRows - 1

            Dim rowel As XmlElement = xdoc.CreateElement("row")
            For col As Integer = 0 To iCols - 1
                rowel.AppendChild(CreateCollectionFieldXML(xdoc, aData(pos), "text", "Column" & col.ToString()))
                pos += 1
            Next
            root.AppendChild(rowel)
        Next

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the entire contents of an ApexGrid in collection form.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Examples("getapexgridcontents classname=ApexGrid.19")>
    Private Function ProcessCommandGetApexGridContents(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim sResult As String = Nothing

        'Get the number of rows and columns...
        Dim cmd As String = "apex_cols " & w.Handle.ToString("X")
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Dim iCols As Integer = Integer.Parse(sResult)

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)

        Dim iCurRow As Integer = 0
        'Read all rows...
        While True

            'Set the current row to the one we're looking at. If we get an error
            'while doing this, we assume we've reached the end of the grid...
            cmd = "property_set " & w.Handle.ToString("X") & ",Row," & iCurRow
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Exit While
            End If

            'Read the data from the current row...
            Dim rowel As XmlElement = xdoc.CreateElement("row")
            cmd = "apex_readcurrow " & w.Handle.ToString("X")
            If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
            Dim vals() As String = sResult.Split(Chr(9))
            Dim col As Integer = 0
            For Each val As String In vals
                rowel.AppendChild(CreateCollectionFieldXML(xdoc, val, "text", "Column" & col.ToString()))
                col += 1
            Next
            root.AppendChild(rowel)

            iCurRow += 1
        End While

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the number of rows in an ApexGrid.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Examples("apexgridrows classname=ApexGridWndClass")>
    Private Function ProcessCommandGetApexGridRow(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim sResult As String = Nothing
        Dim cmd As String

        'Prepare xml document for return value
        Dim xdoc As New XmlDocument()
        Dim root As XmlElement = xdoc.CreateElement("collection")
        xdoc.AppendChild(root)

        Dim rowel As XmlElement = xdoc.CreateElement("row")
        cmd = "apex_readcurrow " & w.Handle.ToString("X")
        If Not mHookClient.SendCommand(cmd, sResult, True) Then Throw New InvalidOperationException(sResult)
        Dim vals() As String = sResult.Split(Chr(9))
        Dim col As Integer = 0
        For Each val As String In vals
            rowel.AppendChild(CreateCollectionFieldXML(xdoc, val, "text", "Column" & col.ToString()))
            col += 1
        Next
        root.AppendChild(rowel)

        Return Reply.Result(xdoc.OuterXml)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the number of rows in an MSFlexGrid.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Examples("getmsflexgridrows classname=MSFlexGridWndClass")>
    Private Function ProcessCommandGetMsFlexGridRows(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim cmd As String = "property_get " & w.Handle.ToString("X") & ",Rows"
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        Else
            Return Reply.Result(sResult)
        End If
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the number of cols in an ApexGrid.")>
    <Parameters("Those required to uniquely identify the window.")>
    <Examples("apexgridcols classname=ApexGridWndClass")>
    Private Function ProcessCommandGetApexGridCols(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim cmd As String = "apex_cols " & w.Handle.ToString("X")
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        Else
            Return Reply.Result(sResult)
        End If
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the contents of a cell in an MSFlexGrid.")>
    <Parameters("Those required to uniquely identify the window, plus 'startx' and 'starty' to specify the position.")>
    <Examples("getmsflexgridcell classname=MSFlexGridWndClass startx=2 starty=3")>
    Private Function ProcessCommandGetMsFlexGridCell(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim row As String = objQuery.GetParameter(ParameterNames.StartY)
        Dim col As String = objQuery.GetParameter(ParameterNames.StartX)
        Dim cmd As String = "property_get " & w.Handle.ToString("X") & ",TextMatrix," & row & "," & col
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        Else
            Return Reply.Result(sResult)
        End If
    End Function

    <Category(Category.ComActiveX)>
    <Command("Move the current position to the specified location in an MSFlexGrid.")>
    <Parameters("Those required to uniquely identify the window, plus 'startx' and 'starty' to specify the position." &
     "Set the row/col parameters to -1 to avoid setting them - in particular, for a grid in 'Full Row Select' mode you may need to avoid setting the x value")>
    <Examples("msflexgridgoto classname=MSFlexGridWndClass startx=2 starty=3")>
    Private Function ProcessCommandMsFlexGridGoto(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim row As String = objQuery.GetParameter(ParameterNames.StartY)
        Dim col As String = objQuery.GetParameter(ParameterNames.StartX)
        Dim sResult As String = Nothing
        Dim cmd As String
        If row <> "-1" Then
            cmd = "property_set " & w.Handle.ToString("X") & ",Row," & row
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(sResult)
            End If
        End If
        If col <> "-1" Then
            cmd = "property_set " & w.Handle.ToString("X") & ",Col," & col
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(sResult)
            End If
        End If
        Return Reply.Ok
    End Function

    <Category(Category.ComActiveX)>
    <Command("Set the top row in an MSFlexGrid.")>
    <Parameters("Those required to uniquely identify the window, plus 'starty' to specify the row.")>
    <Examples("msflexgridsettoprow classname=MSFlexGridWndClass starty=3")>
    Private Function ProcessCommandMsFlexGridSetTopRow(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim row As String = objQuery.GetParameter(ParameterNames.StartY)
        Dim sResult As String = Nothing
        Dim cmd As String
        cmd = "property_set " & w.Handle.ToString("X") & ",TopRow," & row
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        End If
        Return Reply.Ok
    End Function

    <Category(Category.ComActiveX)>
    <Command("Get the offset, in pixels, from the top of the grid to the top of a particular row in an MSFlexGrid.")>
    <Parameters("Those required to uniquely identify the window, plus 'starty' to specify the row.")>
    <Examples("msflexgridgetrowoffset classname=MSFlexGridWndClass starty=3")>
    Private Function ProcessCommandMsFlexGridGetRowOffset(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim row As String = objQuery.GetParameter(ParameterNames.StartY)
        Dim sResult As String = Nothing
        Dim cmd As String
        cmd = "property_get " & w.Handle.ToString("X") & ",RowPos," & row
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        End If
        Dim iTwips As Integer = Integer.Parse(sResult)
        Dim desktop As IntPtr = GetDesktopWindow()
        Dim deskdc As IntPtr = GetWindowDC(desktop)
        Dim iPixels As Integer
        Try
            iPixels = CInt(iTwips / 1440) * GetDeviceCaps(deskdc, WU_LOGPIXELSY)
        Finally
            ReleaseDC(desktop, deskdc)
        End Try
        Return Reply.Result(iPixels)
    End Function

    <Category(Category.ComActiveX)>
    <Command("Select a region in an MSFlexGrid.")>
    <Parameters("Those required to uniquely identify the window, plus 'startx' and 'starty' to specify the position, and 'endx' and 'endy' to specify the non-inclusive other boundary of the region." &
       "Any of the row/column parameters may be set to -1 to skip setting that value - in particular, for a grid in 'Full Row Select' mode you may need to avoid setting the two x values.")>
    <Examples("msflexgridselect classname=MSFlexGridWndClass startx=2 starty=3 endx=3 endy=4")>
    Private Function ProcessCommandMsFlexGridSelect(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim row As String = objQuery.GetParameter(ParameterNames.StartY)
        Dim col As String = objQuery.GetParameter(ParameterNames.StartX)
        Dim endrow As String = objQuery.GetParameter(ParameterNames.EndY)
        Dim endcol As String = objQuery.GetParameter(ParameterNames.EndX)
        Dim sResult As String = Nothing
        Dim cmd As String
        If row <> "-1" Then
            cmd = "property_set " & w.Handle.ToString("X") & ",Row," & row
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(sResult)
            End If
        End If
        If col <> "-1" Then
            cmd = "property_set " & w.Handle.ToString("X") & ",Col," & col
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(sResult)
            End If
        End If
        If endrow <> "-1" Then
            cmd = "property_set " & w.Handle.ToString("X") & ",RowSel," & endrow
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(sResult)
            End If
        End If
        If endcol <> "-1" Then
            cmd = "property_set " & w.Handle.ToString("X") & ",ColSel," & endcol
            If Not mHookClient.SendCommand(cmd, sResult, True) Then
                Throw New InvalidOperationException(sResult)
            End If
        End If
        Return Reply.Ok
    End Function

    <Category(Category.ComActiveX)>
    <Command("Move the current position to the specified location in an ApexGrid.")>
    <Parameters("Those required to uniquely identify the window, plus 'startx' and 'starty' to specify the position.")>
    <Examples("apexgridgoto classname=APEXGridWndClass startx=2 starty=3")>
    Private Function ProcessCommandApexGridGoto(ByVal objQuery As clsQuery) As Reply
        If mHookClient Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NOINJECTOR)
        End If

        Dim w As clsUIWindow
        w = mobjModel.IdentifyWindow(objQuery)

        Dim row As String = objQuery.GetParameter(ParameterNames.StartY)
        Dim col As String = objQuery.GetParameter(ParameterNames.StartX)
        Dim cmd As String = "property_set " & w.Handle.ToString("X") & ",Row," & row
        Dim sResult As String = Nothing
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        End If
        cmd = "property_set " & w.Handle.ToString("X") & ",Col," & col
        If Not mHookClient.SendCommand(cmd, sResult, True) Then
            Throw New InvalidOperationException(sResult)
        Else
            Return Reply.Ok
        End If
    End Function

#End Region

#Region "Query Command Handlers - DDE"

    <Category(Category.DDE)>
    <Command("Gets a list of servers, topics and fields available over DDE.")>
    <Parameters("None.")>
    <Examples("DDESnapshot")>
    Private Function ProcessCommandDdeSnapshot(ByVal objQuery As clsQuery) As Reply
        Try
            Dim ddec As DDE.clsDDEClient = Nothing
            Try
                ddec = New DDE.clsDDEClient("don't care")
                Dim s As String = ddec.BrowseServers()
                Return Reply.Result(s)
            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message)
            Finally
                If ddec IsNot Nothing Then
                    ddec.Dispose()
                End If
            End Try
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstCommunicatingOverDDE & ex.ToString)
        End Try
    End Function

    <Category(Category.DDE)>
    <Command("Reads the text value from the specified DDE field.")>
    <Parameters("The 'DDEServerName', 'DDETopicName', 'DDEItemName' identifying the item whose value is to be read.")>
    <Examples("DDEGetText DDESeverName=MyServer DDETopicName=MyTopic DDEItemName=MyItem")>
    Private Function ProcessCommandDdeGetText(ByVal objQuery As clsQuery) As Reply
        Try
            Dim serverName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEServerName).MatchValue
            Dim topicName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDETopicName).MatchValue
            Dim itemName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEItemName).MatchValue

            If String.IsNullOrEmpty(serverName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForServerNameMustNotBeBlank)
            End If

            If String.IsNullOrEmpty(topicName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForTopicNameMustNotBeBlank)
            End If

            Dim ddec As DDE.clsDDEClient = Nothing
            Dim el As DDE.clsDDEElement = Nothing
            Dim res As String
            Try
                ddec = New DDE.clsDDEClient(serverName)
                el = New DDE.clsDDEElement(ddec, topicName, itemName)
                res = el.getTextValue()
                Return Reply.Result(res)
            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message)
            Finally
                If el IsNot Nothing Then
                    el.Dispose()
                End If
                If ddec IsNot Nothing Then
                    ddec.Dispose()
                End If
            End Try
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstCommunicatingOverDDE & ex.ToString)
        End Try
    End Function

    <Category(Category.DDE)>
    <Command("Sets the text value of the specified DDE field.")>
    <Parameters("The 'newtext' parameter specifies the value, plus 'DDEServerName', 'DDETopicName', 'DDEItemName' identifying the item whose value is to be set.")>
    <Examples("DDESetText newtext=hello DDESeverName=MyServer DDETopicName=MyTopic DDEItemName=MyItem")>
    Private Function ProcessCommandDdeSetText(ByVal objQuery As clsQuery) As Reply
        Try
            Dim serverName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEServerName).MatchValue
            Dim topicName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDETopicName).MatchValue
            Dim itemName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEItemName).MatchValue
            Dim value As String = objQuery.GetParameter(ParameterNames.NewText)

            If String.IsNullOrEmpty(serverName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForServerNameMustNotBeBlank)
            End If

            If String.IsNullOrEmpty(topicName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForTopicNameMustNotBeBlank)
            End If

            Dim ddec As DDE.clsDDEClient = Nothing
            Dim el As DDE.clsDDEElement = Nothing
            Try
                ddec = New DDE.clsDDEClient(serverName)
                el = New DDE.clsDDEElement(ddec, topicName, itemName)
                el.setTextValue(value)
                Return Reply.Ok
            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message)
            Finally
                If el IsNot Nothing Then
                    el.Dispose()
                End If
                If ddec IsNot Nothing Then
                    ddec.Dispose()
                End If
            End Try
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstCommunicatingOverDDE & ex.ToString)
        End Try
    End Function

    <Category(Category.DDE)>
    <Command("Executes a command over DDE.")>
    <Parameters("newtext - the data to send, as part of the command. nocheck (optional, default false) - when true, the API function call's return value will not be checked (used to work around badly behaving apps).")>
    <Examples("ExecuteDDECommand DDEServerName=""excel"" DDETopicName=""Book1.xls"" newtext=""[App.Minimize()]""")>
    Private Function ProcessCommandExecuteDdeCommand(ByVal objQuery As clsQuery) As Reply
        Try
            Dim serverName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEServerName).MatchValue
            Dim topicName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDETopicName).MatchValue
            Dim itemName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEItemName).MatchValue
            Dim data As String = objQuery.GetParameter(ParameterNames.NewText)

            Dim sNoCheck As String = objQuery.GetParameter(ParameterNames.NoCheck)
            Dim noCheck As Boolean
            If Not String.IsNullOrEmpty(sNoCheck) Then
                If Not Boolean.TryParse(sNoCheck, noCheck) Then
                    Throw New InvalidOperationException(My.Resources.BadValueForFlagParameterNocheck & sNoCheck)
                End If
            Else
                noCheck = False
            End If

            If String.IsNullOrEmpty(serverName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForServerNameMustNotBeBlank)
            End If

            If String.IsNullOrEmpty(topicName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForTopicNameMustNotBeBlank)
            End If

            Dim ddec As DDE.clsDDEClient = Nothing
            Dim el As DDE.clsDDEElement = Nothing
            Try
                ddec = New DDE.clsDDEClient(serverName)
                el = New DDE.clsDDEElement(ddec, topicName, itemName)
                el.excecuteCommand(data, noCheck)
                Return Reply.Ok
            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message)
            Finally
                If el IsNot Nothing Then
                    el.Dispose()
                End If
                If ddec IsNot Nothing Then
                    ddec.Dispose()
                End If
            End Try
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstCommunicatingOverDDE & ex.ToString)
        End Try
    End Function

    <Category(Category.DDE)>
    <Command("CheckDDEServerAndTopicAvailable.")>
    <Parameters("None.")>
    <Examples("CheckDDEServerAndTopicAvailable DDEServerName=""excel"" DDETopicName=""Book1.xls""")>
    Private Function ProcessCommandCheckDdeServerAndTopicAvailable(ByVal objQuery As clsQuery) As Reply
        Try
            Dim serverName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEServerName).MatchValue
            Dim topicName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDETopicName).MatchValue

            If String.IsNullOrEmpty(serverName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForServerNameMustNotBeBlank)
            End If

            If String.IsNullOrEmpty(topicName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForTopicNameMustNotBeBlank)
            End If

            Dim ddec As DDE.clsDDEClient = Nothing
            Dim el As DDE.clsDDEElement = Nothing
            Try
                ddec = New DDE.clsDDEClient(serverName)
                el = New DDE.clsDDEElement(ddec, topicName, "don't care")
                Return Reply.Result(el.checkTopicAvailable())
            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message)
            Finally
                If el IsNot Nothing Then
                    el.Dispose()
                End If
                If ddec IsNot Nothing Then
                    ddec.Dispose()
                End If
            End Try
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstCommunicatingOverDDE & ex.ToString)
        End Try
    End Function

    <Category(Category.DDE)>
    <Command("CheckDDEElementReadable.")>
    <Parameters("None.")>
    <Examples("CheckDDEElementReadable DDEServerName=""excel"" DDETopicName=""Book1.xls""  DDEItemName=""r1c1""")>
    Private Function ProcessCommandCheckDdeElementReadable(ByVal objQuery As clsQuery) As Reply
        Try
            Dim serverName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEServerName).MatchValue
            Dim topicName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDETopicName).MatchValue
            Dim itemName As String = objQuery.GetIdentifier(clsQuery.IdentifierTypes.DDEItemName).MatchValue

            If String.IsNullOrEmpty(serverName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForServerNameMustNotBeBlank)
            End If

            If String.IsNullOrEmpty(topicName) Then
                Throw New InvalidOperationException(My.Resources.InvalidValueForTopicNameMustNotBeBlank)
            End If

            Dim ddec As DDE.clsDDEClient = Nothing
            Dim el As DDE.clsDDEElement = Nothing
            Try
                ddec = New DDE.clsDDEClient(serverName)
                el = New DDE.clsDDEElement(ddec, topicName, itemName)
                Try
                    el.getTextValue()
                    Return Reply.Result(True)
                Catch ex As Exception
                    Return Reply.Result(False)
                End Try
            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message)
            Finally
                If el IsNot Nothing Then
                    el.Dispose()
                End If
                If ddec IsNot Nothing Then
                    ddec.Dispose()
                End If
            End Try
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.ExceptionWhilstCommunicatingOverDDE & ex.ToString)
        End Try
    End Function

#End Region

    Private Function GetButtonFromString(button As String) As MouseButton
        Return mMouseOperationsProvider.ParseMouseButton(button)
    End Function

    ''' <summary>
    ''' Used for executing SendMessage calls which require passing pointers to
    ''' structures in memory. Such structures must reside in the target process'
    ''' memory space, so careful preparation and cleanup is needed.
    ''' </summary>
    Private Class RemoteProcessMessageSender(Of InputStructType, OutputStructType)

        ''' <summary>
        ''' The handle of the control to which a message is
        ''' to be sent.
        ''' </summary>
        ''' <remarks>Corresponds to the hWnd parameter of the
        ''' win32 SendMessage function.</remarks>
        Public Handle As IntPtr

        ''' <summary>
        ''' The Message to be sent.
        ''' </summary>
        Public MessageToSend As WindowMessages

        ''' <summary>
        ''' Information to be passed in the wParam parameter of the win32 SendMessage
        ''' call.</summary>
        ''' <remarks>Defaults to zero.</remarks>
        Public wParam As Integer

        ''' <summary>
        ''' Cached value of result of SendMessage function.
        ''' </summary>
        ''' <remarks>Only meaningful after a call to SendMessage()</remarks>
        Public ReturnValue As IntPtr

        ''' <summary>
        ''' Array of values to be passed in the SendMessage call.
        ''' </summary>
        ''' <remarks>This may be left null or empty depending on the type of message
        ''' sent, and the return value of sendmessage.</remarks>
        Public InputValues As InputStructType()

        ''' <summary>
        ''' Array of values retrieved from the SendMessage call. If outputs are
        ''' expected, this array must be initialised, and its length must correspond
        ''' to the expected number of structures to be retrieved.</summary>
        ''' <remarks>This may be null or empty depending on the type of message sent,
        ''' and the return value of sendmessage.</remarks>
        Public OutputValues As OutputStructType()

        ''' <summary>
        ''' Details about an error following a call to SendMessage.
        ''' </summary>
        ''' <remarks>Relevant only when Success is false following a call to
        ''' SendMessage.</remarks>
        Public ErrorMessage As String

        ''' <summary>
        ''' Indicates whether a call to SendMessage was successful. However, the
        ''' caller should compare the value of ReturnValue against the expected value
        ''' according to the message sent - this may indicate further errors
        ''' undetected here.
        ''' </summary>
        ''' <remarks>Valid only after a call to SendMessage; before such time this
        ''' value will always be false. When false, ErrorMessage may provide further
        ''' information.</remarks>
        Public Success As Boolean

        Public Function SendMessage(ByVal hProcess As IntPtr) As IntPtr
            Dim pRemoteStructArray As IntPtr
            Try
                'Assume success, but revert it later if an error occurs
                Success = True

                'Some useful values
                Dim inputStructSize As Integer = Marshal.SizeOf(GetType(InputStructType))
                Dim outputStructSize As Integer = Marshal.SizeOf(GetType(OutputStructType))
                Dim inputsLength As Integer
                If (InputValues IsNot Nothing) Then
                    inputsLength = InputValues.Length
                End If
                Dim outputsLength As Integer
                If (OutputValues IsNot Nothing) Then
                    outputsLength = OutputValues.Length
                End If

                'We need to allocate enough space to hold both the
                'input and output arrays, but not simultaneously
                Dim maxArrayLength As Integer = Math.Max(inputsLength, outputsLength)
                Dim maxStructSize As Integer = Math.Max(inputStructSize, outputStructSize)
                Dim memoryBlockSize As Integer = maxStructSize * maxArrayLength
                pRemoteStructArray = VirtualAllocEx(hProcess, IntPtr.Zero, memoryBlockSize, MEM_RESERVE Or MEM_COMMIT, PAGE_READWRITE)

                'Initialise Inputs structure(s) and write them to remote memory.
                If (InputValues IsNot Nothing) AndAlso (InputValues.Length > 0) Then
                    For i As Integer = 0 To InputValues.Length - 1
                        Dim pItem As IntPtr = Marshal.AllocHGlobal(inputStructSize)
                        Marshal.StructureToPtr(InputValues(i), pItem, False)
                        Dim pDestination As IntPtr = IntPtr.op_Explicit(pRemoteStructArray.ToInt64 + i * inputStructSize)
                        WriteProcessMemory(hProcess, pDestination, pItem, inputStructSize, IntPtr.Zero)
                        Marshal.FreeHGlobal(pItem)
                    Next
                End If

                'send the message 
                ReturnValue = modWin32.SendMessage(Handle, MessageToSend, wParam, pRemoteStructArray)

                'Get the output structure(s) from remote memory
                If (Me.OutputValues IsNot Nothing) AndAlso (OutputValues.Length > 0) Then
                    Dim outputArraySize As Integer = outputsLength * outputStructSize
                    Dim pLocalStructArray As IntPtr = Marshal.AllocHGlobal(outputArraySize)
                    ReadProcessMemory(hProcess, pRemoteStructArray, pLocalStructArray, outputArraySize, IntPtr.Zero)

                    For i As Integer = 0 To outputsLength - 1
                        OutputValues(i) = CType(Marshal.PtrToStructure(IntPtr.op_Explicit(pLocalStructArray.ToInt64 + i * outputStructSize), GetType(OutputStructType)), OutputStructType)
                    Next
                    Marshal.FreeHGlobal(pLocalStructArray)
                End If

                Return ReturnValue
            Catch ex As Exception
                ErrorMessage = My.Resources.UnexpectedError & ex.Message
                Success = False
                Return IntPtr.Zero
            Finally
                'deallocate the memory and close the process handle
                If pRemoteStructArray <> IntPtr.Zero Then
                    VirtualFreeEx(hProcess, pRemoteStructArray, 0, MEM_RELEASE)
                End If
            End Try
        End Function
    End Class

    Protected Overrides Sub Dispose(ByVal disposingExplicitly As Boolean)
        If IsDisposed Then Return
        Disconnect()
        If mTimer IsNot Nothing Then
            mTimer.Stop()
            mTimer.Dispose()
        End If
        If mTerminalApp IsNot Nothing Then
            mTerminalApp.Dispose()
            mTerminalApp = Nothing
        End If
        MyBase.Dispose(disposingExplicitly)
    End Sub

    Private Sub mCitrix_OnDisconnect() Handles mCitrix.OnDisconnect
        OnDisconnected()
    End Sub

    ''' <summary>
    ''' Documentation attribute for the command description
    ''' </summary>
    Friend Class CommandAttribute
        Inherits System.Attribute
        Public Narrative As String
        Public Sub New(ByVal Narrative As String)
            Me.Narrative = Narrative
        End Sub
    End Class

    ''' <summary>
    ''' Documentation attribute for the command parameters
    ''' </summary>
    Friend Class ParametersAttribute
        Inherits System.Attribute
        Public Narrative As String
        Public Sub New(ByVal Narrative As String)
            Me.Narrative = Narrative
        End Sub
    End Class

    ''' <summary>
    ''' Documentation attribute for the commands response
    ''' </summary>
    Friend Class ResponseAttribute
        Inherits System.Attribute
        Public Narrative As String
        Public Sub New(ByVal Narrative As String)
            Me.Narrative = Narrative
        End Sub
    End Class

    ''' <summary>
    ''' Documentation attribute for the commands examples
    ''' </summary>
    Private Class ExamplesAttribute
        Inherits System.Attribute
        Public Narrative As String
        Public Sub New(ByVal Narrative As String)
            Narrative = Narrative.Replace(vbCrLf, vbCrLf & " ")
            Me.Narrative = Narrative
        End Sub
    End Class

    ''' <summary>
    ''' Documentation attribute for the commands category
    ''' </summary>
    Friend Class CategoryAttribute
        Inherits System.Attribute
        Public Type As Category
        Public Sub New(ByVal Type As Category)
            Me.Type = Type
        End Sub
    End Class

    ''' <summary>
    ''' The category for the documentation.
    ''' </summary>
    Public Enum Category
        General = 0
        Win32 = 1
        Mouse = 2
        ComActiveX = 3
        Accessibility = 4
        Java = 5
        Citrix = 6
        Terminal = 7
        Html = 8
        Highlighting = 9
        Diagnostics = 10
        DotNet = 11
        DDE = 12
        SAP = 13
        UIAutomation = 14
        Web = 15
    End Enum

    Private Class OcrPlusJsonResponse
        <JsonProperty("success")>
        Public Property success As Boolean

        <JsonProperty("text")>
        Public Property text As String

        <JsonProperty("fontname")>
        Public Property fontname As String

        <JsonProperty("fontstyle")>
        Public Property fontstyle As String

        <JsonProperty("fontsize")>
        Public Property fontsize As Single
    End Class

End Class
