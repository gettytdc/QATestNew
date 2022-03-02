Option Strict On
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Reflection
Imports System.Drawing
Imports System.Linq
Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports mshtml

''' Project  : ApplicationManagerUtilities
''' Class    : clsHTMLDocument
''' 
''' <summary>
''' This class represents an html document it simply wraps an IHTMLDocument2 
''' and provides some extra functionality.
''' </summary>
Public Class clsHTMLDocument

    ''' <summary>
    ''' Used to hold the underlying IHTMLDocument2 interface
    ''' </summary>
    Private mDocument As mshtml.IHTMLDocument2

    ''' <summary>
    ''' Callback delegate used to find the explorer server
    ''' </summary>
    Private callback As EnumWindowsProc

    ''' <summary>
    ''' Holds the windowhandle for the explorer server when it has been found
    ''' </summary>
    Private mServerHandle As IntPtr

    ''' <summary>
    ''' Holds a list of frames inside the document
    ''' </summary>
    Private mFrames As New List(Of clsHTMLDocumentFrame)

    Protected Sub New()
    End Sub

    ''' <summary>
    ''' Constructor that takes an IHTMLDocument. Used only when the interface has
    ''' been marshaled from a remote hooked process.
    ''' </summary>
    ''' <param name="o">An object that should be an IHTMLDocument.</param>
    Public Sub New(ByVal o As Object)
        Dim ih1 As mshtml.IHTMLDocument = TryCast(o, mshtml.IHTMLDocument)
        mDocument = TryCast(ih1, mshtml.IHTMLDocument2)
        mServerHandle = IntPtr.Zero
    End Sub

    Protected Sub New(ByVal doc As mshtml.IHTMLDocument2, ByVal ServerHandle As IntPtr)
        mDocument = doc
        mServerHandle = ServerHandle
    End Sub

    ''' <summary>
    ''' Gets a clsHTMLDocument given the hwnd of the web browser server window.
    ''' </summary>
    ''' <param name="hwnd">The handle of the web browser server window.</param>
    ''' <returns>A clsHTMLDocument representing the underlying document.</returns>
    Public Shared Function GetDocumentFromHWnd(ByVal hwnd As IntPtr) As clsHTMLDocument
        Dim d As mshtml.IHTMLDocument2 = GetIHTMLDocumentFromHwnd(hwnd)
        Return New clsHTMLDocument(d, hwnd)
    End Function

    ''' <summary>
    ''' Gets a IHTMLDocument2 interface given the hwnd of the web browser server window.
    ''' </summary>
    ''' <param name="hwnd">The handle of the web browser server window.</param>
    ''' <returns>Returns the document contained in the supplied window, if
    ''' any. May return Nothing if none found, or in the event of an error.</returns>
    ''' <remarks>This technique is based on http://support.microsoft.com/kb/249232.
    ''' See also GetIHTMLDocumentFromHwnd that uses an alternative method.</remarks>
    Private Shared Function GetIHTMLDocumentFromHwnd( _
     ByVal hWnd As IntPtr) As mshtml.IHTMLDocument2
        Dim message As Integer = RegisterWindowMessage("WM_HTML_GETOBJECT")
        Dim lResult As Int32
        Dim hResult = SendMessageTimeout(hWnd, message, 0, 0, SendMessageTimeoutFlags.SMTO_BLOCK, 10000, lResult)

        If hResult = IntPtr.Zero OrElse lResult = 0 Then
            Throw New InvalidOperationException(My.Resources.ErrorAttachingToHTMLDocument)
        Else
            Dim doc As Object = Nothing
            ObjectFromLresult(New IntPtr(lResult), _
             GetType(mshtml.IHTMLDocument2).GUID, IntPtr.Zero, doc)
            If doc Is Nothing Then
                Throw New InvalidOperationException(My.Resources.ErrorHTMLDocumentIsNull)
            End If
            Return TryCast(doc, mshtml.IHTMLDocument2)
        End If
    End Function

    ''' <summary>
    ''' Gets a IHTMLDocument2 interface given the hwnd of the web browser server window.
    ''' </summary>
    ''' <param name="hwnd">The handle of the web browser server window.</param>
    ''' <returns>Returns the document contained in the supplied window, if
    ''' any. May return Nothing if none found, or in the event of an error. May also
    ''' throw various exceptions.</returns>
    ''' <remarks>This is a variation on the 'normal' GetIHTMLDocumentFromHwnd that
    ''' uses Active Accessibility to get hold of the interface.</remarks>
    Private Shared Function GetIHTMLDocumentFromHwnd2(ByVal hWnd As IntPtr) As mshtml.IHTMLDocument2
        Dim accobj As Accessibility.IAccessible = Nothing
        Const OBJID_WINDOW As Integer = &H0
        AccessibleObjectFromWindow(hWnd.ToInt32(), _
         OBJID_WINDOW, IID_IACCESSIBLE, accobj)

        Dim fetchCount As Integer
        Dim count As Integer = accobj.accChildCount
        Dim children(count - 1) As Object
        Dim hr As Integer = AccessibleChildren(accobj, 0, count, children, fetchCount)
        Marshal.ThrowExceptionForHR(hr)

        If children.Length = 0 Then
            clsConfig.LogWin32("HTML VIA AA:No accessible children for window " & hWnd.ToString("X8"))
            Return Nothing
        End If
        Dim firstchild As Accessibility.IAccessible = TryCast(children(0), Accessibility.IAccessible)
        If firstchild Is Nothing Then
            clsConfig.LogWin32("HTML VIA AA:Could not cast child for window " & hWnd.ToString("X8"))
            Return Nothing
        End If

        Dim el As mshtml.IHTMLElement = AccessibleToHTMLElement(firstchild)
        If el Is Nothing Then
            clsConfig.LogWin32("HTML VIA AA:Could not convert to HTML element for window " & hWnd.ToString("X8"))
            Return Nothing
        End If
        Dim doc As mshtml.IHTMLDocument2 = TryCast(el.document, mshtml.IHTMLDocument2)
        If doc Is Nothing Then Debug.WriteLine("Doc is Nothing")
        Return doc
    End Function

    Public Function GetBrowser() As clsHTMLDocument.IWebBrowser2
        'Read http://www.com.it-berater.org/COM/webbrowser/Interfaces/IWebBrowser2.htm
        'and http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=301976&SiteID=1
        Dim iWebBrowserAppID As New Guid("0002DF05-0000-0000-C000-000000000046")
        Dim iWebBrowser2ID As Guid = GetType(clsHTMLDocument.IWebBrowser2).GUID

        Dim ppvObject As Object = Nothing

        If mDocument IsNot Nothing Then
            Dim sErr As String = Nothing
            Dim cast As clsHTMLDocument.IServiceProvider = TryCast(GetParentWindow(sErr), clsHTMLDocument.IServiceProvider)
            If cast IsNot Nothing Then
                cast.QueryService(iWebBrowserAppID, iWebBrowser2ID, ppvObject)
            End If
        End If

        Return TryCast(ppvObject, clsHTMLDocument.IWebBrowser2)
    End Function


    ''' <summary>
    ''' This interface is required for GetBrowserFromHwnd
    ''' </summary>
    <ComImport(), Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Private Interface IServiceProvider
        Sub QueryService(ByRef guidService As Guid, ByRef riid As Guid, <MarshalAs(UnmanagedType.Interface)> ByRef ppvObject As Object)
    End Interface

    ''' <summary>
    ''' This interface is required by GetFrames.
    ''' </summary>
    <ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000011B-0000-0000-C000-000000000046")> _
    Private Interface IOleContainer
        <PreserveSig()> _
        Function ParseDisplayName(<[In](), MarshalAs(UnmanagedType.Interface)> ByVal pbc As Object, <[In](), MarshalAs(UnmanagedType.BStr)> ByVal pszDisplayName As String, <Out(), MarshalAs(UnmanagedType.LPArray)> ByVal pchEaten As Integer(), <Out(), MarshalAs(UnmanagedType.LPArray)> ByVal ppmkOut As Object()) As Integer
        <PreserveSig()> _
        Function EnumObjects(<[In](), MarshalAs(UnmanagedType.U4)> ByVal grfFlags As Integer, <Out()> ByRef ppenum As IEnumUnknown) As Integer
        <PreserveSig()> _
        Function LockContainer(ByVal fLock As Boolean) As Integer
    End Interface

    ''' <summary>
    ''' This interface is required by GetFrames
    ''' </summary>
    <ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000100-0000-0000-C000-000000000046")> _
    Private Interface IEnumUnknown
        <PreserveSig()> _
        Function [Next](<[In](), MarshalAs(UnmanagedType.U4)> ByVal celt As Integer, <MarshalAs(UnmanagedType.IUnknown)> ByRef rgelt As Object, ByVal pceltFetched As Integer) As Integer
        <PreserveSig()> _
        Function Skip(<[In](), MarshalAs(UnmanagedType.U4)> ByVal celt As Integer) As Integer
        Sub Reset()
        Sub Clone(<Out()> ByRef ppenum As IEnumUnknown)
    End Interface


    ''' <summary>
    ''' Interface definition for the IWebBrowser2 COM interface.
    ''' </summary>
    <ComImport(), Guid("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), DefaultMember("Name"), SuppressUnmanagedCodeSecurity()> _
    Public Interface IWebBrowser2
        Sub GoBack()
        Sub GoForward()
        Sub GoHome()
        Sub GoSearch()
        Sub Navigate(<[In]()> ByVal URL As String, <[In]()> ByRef Flags As Object, <[In]()> ByRef TargetFrameName As Object, <[In]()> ByRef PostData As Object, <[In]()> ByRef Headers As Object)
        Sub Refresh()
        Sub Refresh2(<[In]()> ByRef Level As Object)
        Sub [Stop]()
        ReadOnly Property Application() As <MarshalAs(UnmanagedType.IDispatch)> Object
        ReadOnly Property Parent() As <MarshalAs(UnmanagedType.IDispatch)> Object
        ReadOnly Property Container() As <MarshalAs(UnmanagedType.IDispatch)> Object
        ReadOnly Property Document() As <MarshalAs(UnmanagedType.IDispatch)> Object
        ReadOnly Property TopLevelContainer() As Boolean
        ReadOnly Property Type() As String
        Property Left() As Integer
        Property Top() As Integer
        Property Width() As Integer
        Property Height() As Integer
        ReadOnly Property LocationName() As String
        ReadOnly Property LocationURL() As String
        ReadOnly Property Busy() As Boolean
        Sub Quit()
        Sub ClientToWindow(ByRef pcx As Integer, ByRef pcy As Integer)
        Sub PutProperty(<[In]()> ByVal [Property] As String, <[In]()> ByVal vtValue As Object)
        Function GetProperty(ByVal [Property] As String) As Object
        ReadOnly Property Name() As String
        ReadOnly Property HWND() As Integer
        ReadOnly Property FullName() As String
        ReadOnly Property Path() As String
        Property Visible() As Boolean
        Property StatusBar() As Boolean
        Property StatusText() As String
        Property ToolBar() As Integer
        Property MenuBar() As Boolean
        Property FullScreen() As Boolean
        Sub Navigate2(<[In]()> ByRef URL As Object, <[In]()> ByRef Flags As Object, <[In]()> ByRef TargetFrameName As Object, <[In]()> ByRef PostData As Object, <[In]()> ByRef Headers As Object)
        Sub ShowBrowserBar(<[In]()> ByRef pvaClsid As Object, <[In]()> ByRef pvarShow As Object, <[In]()> ByRef pvarSize As Object)
        Property Offline() As Boolean
        Property Silent() As Boolean
        Property RegisterAsBrowser() As Boolean
        Property RegisterAsDropTarget() As Boolean
        Property TheaterMode() As Boolean
        Property AddressBar() As Boolean
        Property Resizable() As Boolean
    End Interface

    ''' <summary>
    ''' Get the HTML documents present in the process with the given PID.
    ''' </summary>
    ''' <param name="targetpid">The PID of the process of interest.</param>
    ''' <returns>A List of clsHTMLDocument objects found in that process.</returns>
    Public Shared Function GetDocumentsFromPID(ByVal targetpid As Integer, _
                                               Optional visibleOnly As Boolean = False) As List(Of clsHTMLDocument)
        Dim info As New clsEnumWindowsInfo()
        info.TargetPID = targetpid
        info.Tag = New List(Of clsHTMLDocument)
        info.VisibleOnly = visibleOnly
        EnumWindows(AddressOf FindProcessWindows, info)
        Return CType(info.Tag, List(Of clsHTMLDocument))

    End Function

    ''' <summary>
    ''' Find all the windows belonging to this process and enumerate its child
    ''' windows to find explorer servers.
    ''' </summary>
    ''' <param name="hwnd">The handle of the window</param>
    ''' <param name="info">A clsEnumWindowsInfo instance containing the information
    ''' we are interested in - specifically the target PID.</param>
    Private Shared Function FindProcessWindows(hwnd As IntPtr,
                                               info As clsEnumWindowsInfo) As Boolean

        'If we are only searching for visible windows, we need to check whether the 
        'window is visible
        If Not info.VisibleOnly OrElse _
            (info.VisibleOnly AndAlso IsWindowVisible(hwnd)) Then

            'Get the process ID of the current window
            Dim pid As Integer
            GetWindowThreadProcessId(hwnd, pid)

            'Check whether the process ID matches the process we are searching for
            If info.TargetPID = pid Then
                'Enumerate the window's children to try and find the explorer server
                If EnumChildWindows(hwnd, _
                                    AddressOf FindExplorerServer, info) = Nothing Then
                    'Do nothing on EnumChildWindows returning an error  i.e. nothing,
                    'otherwise this can exit the FindProcessWindows callback and not 
                    'attempt to enumerate other child windows for other process windows.
                End If
            End If
        End If

        Return True
    End Function

    ''' <summary>
    ''' Gets an element matching the given ID. If there are more than one matches 
    ''' the first one is returned
    ''' </summary>
    ''' <param name="ID">The Element ID to match against</param>
    ''' <returns>The matching element</returns>
    Public Function GetElementByID(ByVal ID As String) As clsHTMLElement
        Try
            Dim doc = TryCast(mDocument, mshtml.IHTMLDocument3)
            If doc IsNot Nothing Then
                Dim e = doc.getElementById(ID)
                If e IsNot Nothing Then
                    Return New clsHTMLElement(e, Me)
                End If
            End If
        Catch ex As Exception
            clsConfig.LogException(ex)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Sets a cookie on the browser.
    ''' </summary>
    ''' <param name="cookie">The cookie</param>
    Public Sub UpdateCookie(ByVal cookie As String)
        mDocument.cookie = cookie
    End Sub

    ''' <summary>
    ''' Returns a list containing the one matching element for a given ID
    ''' </summary>
    ''' <param name="ID">The Element ID to match against</param>
    ''' <returns>A list containing the matching element</returns>
    Public Function GetElementsByID(ByVal ID As String) As List(Of clsHTMLElement)
        Dim all As New List(Of clsHTMLElement)
        Dim e As clsHTMLElement = GetElementByID(ID)
        If e IsNot Nothing Then
            all.Add(e)
        End If
        Return all
    End Function

    ''' <summary>
    ''' EnumWindows callback function used by FindExplorerServers.
    ''' </summary>
    ''' <param name="hwnd">The handle of the window found</param>
    ''' <param name="lparam">The lparam, which we know is a clsEnumWindowsInfo</param>
    ''' <returns> True to continue the enumeration, and false to stop it.
    ''' </returns>
    Private Shared Function FindExplorerServer(hwnd As IntPtr,
                                               lparam As clsEnumWindowsInfo) _
                                           As Boolean

        'Check whether the window's class name is the explorer server
        Dim className As New StringBuilder(128)
        RealGetWindowClass(hwnd, className, 128)
        If className.ToString <> "Internet Explorer_Server" Then Return True

        'If we are only searching for visible windows, we need to check whether the 
        'window is visible
        If lparam.VisibleOnly AndAlso Not IsWindowVisible(hwnd) Then Return True

        clsConfig.LogHTML("Window " & hwnd.ToString("X8") &
                          " is of class Internet Explorer_Server")

        'Get the html document from the window and add it to the list of documents
        'held in the the param
        Dim docs As List(Of clsHTMLDocument) = _
            CType(lparam.Tag, List(Of clsHTMLDocument))

        Dim doc As clsHTMLDocument
        If clsConfig.GetHTMLViaAA Then
            doc = New clsHTMLDocument(GetIHTMLDocumentFromHwnd2(hwnd), hwnd)
        Else
            doc = New clsHTMLDocument(GetIHTMLDocumentFromHwnd(hwnd), hwnd)
        End If
        docs.Add(doc)

        'If we are only finding visible windows then we can stop enumerating
        'at this point
        Return Not lparam.VisibleOnly


    End Function

    ''' <summary>
    ''' Provides access to a flat list of all the frames within the document
    ''' including nested frames
    ''' </summary>
    Public ReadOnly Property FlatListOfFrames(ByVal excludeHTC As Boolean) As List(Of clsHTMLDocumentFrame)
        Get
            Dim objFlatListOfFrames As New List(Of clsHTMLDocumentFrame)
            mFrames = New List(Of clsHTMLDocumentFrame)
            GetFrames(mDocument, excludeHTC, True)
            Dim lst As List(Of clsHTMLDocumentFrame) = mFrames
            While lst.Count > 0
                Dim f As clsHTMLDocumentFrame = lst(0)
                lst.RemoveAt(0)
                objFlatListOfFrames.Add(f)
                f.mFrames = New List(Of clsHTMLDocumentFrame)
                GetFrames(f.mDocument, excludeHTC, True)
                lst.AddRange(f.mFrames)
            End While

            Return objFlatListOfFrames
        End Get
    End Property

    ''' <summary>
    ''' Initialises the frames in the document
    ''' </summary>
    Public Sub Initialise()
        mFrames = New List(Of clsHTMLDocumentFrame)
        GetFrames(mDocument, False, True)
    End Sub

    ''' <summary>
    ''' Gets the frames in the document
    ''' </summary>
    ''' <returns>A List of clsHTMLDocumentFrames</returns>
    Private Function GetFrames() As List(Of clsHTMLDocumentFrame)
        If mFrames Is Nothing Then
            Throw New InvalidOperationException($"{My.Resources.FramesNotLoaded}" )
        End If

        Return mFrames
    End Function

    ''' <summary>
    ''' Provides access to a list of the frames within the document
    ''' </summary>
    Public ReadOnly Property ChildFrames(ByVal excludeHTC As Boolean) As List(Of clsHTMLDocumentFrame)
        Get
            mFrames = New List(Of clsHTMLDocumentFrame)
            GetFrames(mDocument, excludeHTC, False)
            Return mFrames
        End Get
    End Property


    ''' <summary>
    ''' Gets all the frames nested in the document, optionally including frames
    ''' nested within frames. The technique is documented here:
    ''' http://www.codeguru.com/cpp/i-n/internet/browsercontrol/article.php/c13065/
    ''' </summary>
    ''' <param name="docMain">The document on which the frames are required</param>
    ''' <param name="bDescend">Whether to descend all nested frames</param>
    Private Sub GetFrames(ByVal docMain As IHTMLDocument2, ByVal excludeHTC As Boolean, ByVal bDescend As Boolean)

        'Get the HTC documents, we expect that there will only be one level of
        'nesting for these
        If Not excludeHTC Then
            For Each obj As Object In docMain.all
                Try
                    Dim htmlEl As IHTMLGenericElement = TryCast(obj, IHTMLGenericElement)
                    If htmlEl IsNot Nothing Then _
                     mFrames.Add(GetHTCDocumentFromElement(htmlEl, Me))
                Catch 'If we failed to get the HTC document we will just ignore it
                End Try
            Next
        End If

        'Now get the Frames
        Dim cont As IOleContainer = TryCast(docMain, IOleContainer)
        ' Nothing we can do if it's not a container - it has no children to enumerate
        If cont Is Nothing Then Return

        Const OLECONTF_EMBEDDINGS As Integer = 1
        Const S_OK As Integer = 0

        Dim enu As IEnumUnknown = Nothing
        Dim unk As Object = Nothing
        Dim fetchCount As Integer = 0

        If cont.EnumObjects(OLECONTF_EMBEDDINGS, enu) <> S_OK Then Return
        enu.Reset()

        While enu.Next(1, unk, fetchCount) = S_OK
            ' We're only interested in elements which implement both IWebBrowser2
            ' and IHTMLElement

            Dim browser As IWebBrowser2 = TryCast(unk, IWebBrowser2)
            ' Not a browser, move onto the next element
            If browser Is Nothing Then Continue While

            Dim frmEl As IHTMLElement = TryCast(unk, IHTMLElement)
            ' Not a HTML element, move onto the next element
            If frmEl Is Nothing Then Continue While

            ' Get the document
            Dim doc As IHTMLDocument2 = Nothing
            Try 'IE11 can throw with a MEMBER_NOT_FOUND exception
                doc = TryCast(browser.Document, IHTMLDocument2)
            Catch ex As COMException
                Try
                    ' See if this is an iframe in a different domain
                    doc = GetDocumentFromElement(frmEl)
                Catch
                    ' We are deliberately ignoring an error here.
                End Try
            Catch
                'Ignore all exceptions except COMException.
            End Try

            ' If no doc is there (?), move onto the next element
            If doc Is Nothing Then Continue While

            Dim frame As New clsHTMLDocumentFrame(doc, New clsHTMLElement(frmEl, Me))
            mFrames.Add(frame)
            If bDescend Then frame.GetFrames(doc, excludeHTC, bDescend)

        End While

    End Sub

    ''' <summary>
    ''' Looks in side the element to see if the iframe contains child items.
    ''' </summary>
    ''' <param name="element"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function GetDocumentFromElement(element As IHTMLElement) As IHTMLDocument2
        Dim objAccessibleElement As Accessibility.IAccessible =
            HTMLElementToAccessible(element)

        Dim aChildren(objAccessibleElement.accChildCount) As Object

        AccessibleChildren(objAccessibleElement,
                           0,
                           objAccessibleElement.accChildCount,
                           aChildren,
                           0)

        Dim objFirstChild As Accessibility.IAccessible =
            TryCast(aChildren.FirstOrDefault(), Accessibility.IAccessible)

        If objFirstChild Is Nothing Then
            Throw New InvalidOperationException(My.Resources.FailedToGetTheFirstChildOfAnIAccessibleElement)
        End If

        Dim objHTMLBodyElement As mshtml.IHTMLElement =
            AccessibleToHTMLElement(objFirstChild)

        Return TryCast(objHTMLBodyElement.document, IHTMLDocument2)

    End Function

    ''' <summary>
    ''' Gets the HTC document given an HTML element in the parent document.
    ''' </summary>
    ''' <param name="el">The HTML element in the parent document.</param>
    ''' <param name="objParent"></param>
    ''' <returns>A clsHTMLDocument object representing the HTML of the HTC component.
    ''' </returns>
    Friend Shared Function GetHTCDocumentFromElement( _
     ByVal el As IHTMLGenericElement, ByVal objParent As clsHTMLDocument) _
     As clsHTMLDocumentFrame
        Return GetHTCDocumentFromElement(DirectCast(el, IHTMLElement), objParent)
    End Function

    ''' <summary>
    ''' Gets a HTC document given an HTML element in the parent document.
    ''' </summary>
    ''' <param name="el">The HTML element in the parent document.</param>
    ''' <returns>A clsHTMLDocument object representing the HTML of the HTC component.</returns>
    Friend Shared Function GetHTCDocumentFromElement(ByVal el As mshtml.IHTMLElement, ByVal objParent As clsHTMLDocument) As clsHTMLDocumentFrame

        Dim objDocument2 As mshtml.IHTMLDocument2 =
            GetDocumentFromElement(el)

        Dim objFrameElement As New clsHTMLElement(el, objParent)

        Return New clsHTMLDocumentFrame(objDocument2, objFrameElement)

    End Function

    ''' <summary>
    ''' Gets an IHTMLElement object from a given IAccessible object.
    ''' </summary>
    ''' <param name="objAccessibleElement">The IAccessible object to get the
    ''' IHTMLElement object for.</param>
    ''' <returns>An IHTMLElement object.</returns>
    Private Shared Function AccessibleToHTMLElement(ByVal objAccessibleElement As Accessibility.IAccessible) As mshtml.IHTMLElement
        Dim gIHTMLElement As Guid = GetType(mshtml.IHTMLElement).GUID

        Dim objServiceProvider As IServiceProvider = TryCast(objAccessibleElement, IServiceProvider)
        If objServiceProvider IsNot Nothing Then
            Dim objHTMLElement As Object = Nothing
            objServiceProvider.QueryService(gIHTMLElement, gIHTMLElement, objHTMLElement)
            Return TryCast(objHTMLElement, mshtml.IHTMLElement)
        End If

        Throw New InvalidOperationException(My.Resources.FailedToGetIHTMLElementFromIAccessibleElement)
    End Function

    ''' <summary>
    ''' Gets an IAccessible object from a given IHTMLElement object.
    ''' </summary>
    ''' <param name="objHTMLelement">The IHTMLElement object to get the IAccessible
    ''' object for.</param>
    ''' <returns>An IAccessible object.</returns>
    Private Shared Function HTMLElementToAccessible(ByVal objHTMLelement As mshtml.IHTMLElement) As Accessibility.IAccessible
        Dim gAccessible As Guid = GetType(Accessibility.IAccessible).GUID

        Dim objServiceProvider As IServiceProvider = TryCast(objHTMLelement, IServiceProvider)
        If objServiceProvider IsNot Nothing Then
            Dim objAccessibleElement As Object = Nothing
            objServiceProvider.QueryService(gAccessible, gAccessible, objAccessibleElement)
            Return TryCast(objAccessibleElement, Accessibility.IAccessible)
        End If

        Throw New InvalidOperationException(My.Resources.FailedToGetIAccessibleElementFromIHTMLElement)
    End Function

    ''' <summary>
    ''' Boolean indicating whether the document is loaded.
    ''' </summary>
    ''' <param name="browser">The browser object to use to check is loaded.</param>
    ''' <returns>True if loaded, otherwise False.</returns>
    Public Shared Function IsLoaded(ByVal browser As IWebBrowser2) As Boolean
        Return (browser.Document IsNot Nothing) AndAlso (Not browser.Busy)
    End Function

    ''' <summary>
    ''' Check whether the document is loaded, using the readystate method.
    ''' </summary>
    ''' <returns>True if loaded, otherwise False.</returns>
    Public Function DocumentLoaded() As Boolean
        Return mDocument IsNot Nothing AndAlso mDocument.readyState = "complete"
    End Function

    ''' <summary>
    ''' Provides access to all the elements in the document.
    ''' </summary>
    Public ReadOnly Property All() As List(Of clsHTMLElement)
        Get
            Dim els As New List(Of clsHTMLElement)
            Try
                For Each el As IHTMLElement In mDocument.all
                    els.Add(New clsHTMLElement(el, Me))
                Next
            Catch ex As Exception
                clsConfig.LogException(ex)
            End Try
            Return els
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the root of the document
    ''' </summary>
    Public ReadOnly Property Root() As clsHTMLElement
        Get
            Try
                Dim doc = TryCast(mDocument, mshtml.IHTMLDocument3)
                If doc IsNot Nothing Then
                    Return New clsHTMLElement(doc.documentElement, Me)
                End If
            Catch ex As Exception
                clsConfig.LogException(ex)
            End Try
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the source of the HTML document.
    ''' </summary>
    Public ReadOnly Property Source() As String
        Get
            Try
                Dim doc = TryCast(mDocument, mshtml.IHTMLDocument3)
                If doc IsNot Nothing Then
                    Return doc.documentElement.innerHTML
                End If
            Catch ex As Exception
                clsConfig.LogException(ex)
            End Try
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the URL of the currently loaded document Returns "about:blank"
    ''' if no document is loaded.
    ''' </summary>
    Public Property URL() As String
        Get
            Return mDocument.url
        End Get
        Set(ByVal newurl As String)
            mDocument.url = newurl
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the body of the document
    ''' </summary>
    Public ReadOnly Property Body() As clsHTMLElement
        Get
            Return New clsHTMLElement(mDocument.body, Me)
        End Get
    End Property

    ''' <summary>
    ''' Finds an element at a point on the screen, this takes into account any sub
    ''' documents, e.g frames and iframes
    ''' </summary>
    ''' <param name="ScreenX">The x location to be queried, in screen coordinates.</param>
    ''' <param name="ScreenY">The y location to be queried, in screen coordinates.</param>
    ''' <returns>Returns the element at the specified screen location, or Nothing
    ''' if no such element exists.</returns>
    Public Function ElementFromPoint(ByVal ScreenX As Integer, ByVal ScreenY As Integer) As clsHTMLElement
        'First check our nested child frames
        Dim FrameResult As clsHTMLElement = Me.FindNestedElementFromPoint(ScreenX, ScreenY)
        If FrameResult IsNot Nothing Then Return FrameResult

        'Search current document if nothing exists in any of the frames
        Dim found As mshtml.IHTMLElement = TryCast(mDocument.elementFromPoint(ScreenX - RootDocumentBounds.X, ScreenY - RootDocumentBounds.Y), mshtml.IHTMLElement)
        If found IsNot Nothing Then
            Return New clsHTMLElement(found, Me)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Finds an element nested within child frames of this document, according to a
    ''' screen-based coordinate.
    ''' </summary>
    ''' <param name="ScreenX">The x location to be queried, in screen coordinates.</param>
    ''' <param name="ScreenY">The y location to be queried, in screen coordinates.</param>
    ''' <returns>Gets the element at the specified screen point, nested in
    ''' any child frames of this document. Returns nothing if no match found.</returns>
    Private Function FindNestedElementFromPoint(ByVal ScreenX As Integer, ByVal ScreenY As Integer) As clsHTMLElement

        'See if there is a match in any of the (possibly nested) frames
        For Each df As clsHTMLDocumentFrame In GetFrames()

            Dim FrameBounds As Rectangle = df.GetAbsoluteBounds(df.FrameElement)
            FrameBounds.Offset(-RootDocumentBounds.X, -RootDocumentBounds.Y)

            'Do recursive search within frame
            Dim NativeResult As clsHTMLElement
            NativeResult = df.FindNestedElementFromPoint(ScreenX, ScreenY)
            If NativeResult IsNot Nothing Then
                Return NativeResult
            Else
                'If nothing found deeper down tree (ie we are in leaf)
                'then do search within this frame
                FrameBounds.Offset(RootDocumentBounds.Location)
                Dim Result As mshtml.IHTMLElement = df.mDocument.elementFromPoint(ScreenX - FrameBounds.X, ScreenY - FrameBounds.Y)
                If Not Result Is Nothing Then
                    Return New clsHTMLElement(Result, df)
                End If
            End If
        Next

        Return Nothing
    End Function


    ''' <summary>
    ''' Gets the absolute bounds of a given clsHTMLElement
    ''' </summary>
    ''' <param name="e">The clsHTMLElement</param>
    ''' <returns>The bounds as a Rectangle</returns>
    Friend Overridable Function GetAbsoluteBounds(ByVal e As clsHTMLElement) As Rectangle
        clsConfig.LogTiming("HTML: Start GetAbsoluteBounds")

        'These bounds are relative to the containing document/frame
        Dim rBoundingRectangle As Rectangle = e.BoundingRectangle

        'Give the bounds relative to this frame/top-level element
        clsConfig.LogTiming("HTML: Finish GetAbsoluteBounds")

        Dim objDocumentFrame As clsHTMLDocumentFrame = TryCast(e.ParentDocument, clsHTMLDocumentFrame)
        If Not objDocumentFrame Is Nothing Then
            Dim rFrameBounds As Rectangle = objDocumentFrame.FrameBounds
            Return New Rectangle(rFrameBounds.Left + rBoundingRectangle.Left, rFrameBounds.Top + rBoundingRectangle.Top, rBoundingRectangle.Width, rBoundingRectangle.Height)
        Else
            Return New Rectangle(RootDocumentBounds.Left + rBoundingRectangle.Left, RootDocumentBounds.Top + rBoundingRectangle.Top, rBoundingRectangle.Width, rBoundingRectangle.Height)
        End If
    End Function

    ''' <summary>
    ''' Gets the bounds of the document window 
    ''' </summary>
    Public ReadOnly Property RootDocumentBounds() As Rectangle
        Get
            If TypeOf Me Is clsHTMLDocumentFrame Then
                Dim F As clsHTMLDocumentFrame = CType(Me, clsHTMLDocumentFrame)
                mServerHandle = F.FrameElement.ParentDocument.mServerHandle
            End If

            Dim r As RECT
            GetWindowRect(mServerHandle, r)
            Return r
        End Get
    End Property

    ''' <summary>
    ''' Navigate to a given URL
    ''' </summary>
    ''' <param name="url">The URL to navigate to</param>
    ''' <remarks>If the user does not have the rights to enter addresses locally (i.e.
    ''' in Windows and Internet Explorer, this will fail. This is a common situation
    ''' on corporate networks.</remarks>
    Public Sub Navigate(ByVal url As String)
        Dim sErr As String = Nothing
        Dim parent As mshtml.IHTMLWindow2 = GetParentWindow(sErr)
        If parent IsNot Nothing Then
            parent.navigate(url)
        End If
    End Sub

    ''' <summary>
    ''' Gets the parent window of the document. NOTE: The API call can fail with an 
    ''' Invalid Cast Exception. If this happens it is likely because the thread on
    ''' which the method is being called in not from an STA Thread appartment. See
    ''' bug 8079, and 3710 for details.
    ''' </summary>
    ''' <param name="sErr">Detailed exception message on Failure</param>
    ''' <returns>The parent window.</returns>
    Private Function GetParentWindow(ByRef sErr As String) As mshtml.IHTMLWindow2
        Try
            Return mDocument.parentWindow
        Catch ex As Exception
            sErr = String.Format(My.Resources.FailedToGetParentWindow01, ex.Message, ex.StackTrace)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Invoke a Javascript method
    ''' </summary>
    ''' <param name="methodname">The name of the method</param>
    ''' <param name="jsonargs">The arguments in JSON format - i.e. an array of objects
    ''' with each object being one of the parameters.</param>
    ''' <param name="retval">If successful, the function's return value</param>
    ''' <param name="sErr">On failure, contains an error description.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function InvokeJavascriptMethod(ByVal methodname As String, ByVal jsonargs As String, ByRef retval As Object, ByRef sErr As String) As Boolean
        Dim parent As mshtml.IHTMLWindow2 = GetParentWindow(sErr)
        If parent Is Nothing Then Return False
        Try
            retval = parent.execScript("function appman_exec(name,json) { return eval(name).apply(null,eval(json)); } appman_exec('" & methodname & "', " & jsonargs & ")")
            Return True
        Catch ex As Exception
            sErr = ex.Message & "->" & ex.StackTrace
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Inserts a javascript method into the target page.
    ''' </summary>
    ''' <param name="MethodDefinition">A full definition of the method to be
    ''' inserted, e.g.:
    ''' function doSomething() {
    '''     alert('Done something');
    ''' }
    ''' </param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns True on success; False otherwise.</returns>
    Public Function InsertJavascriptFragment(ByVal MethodDefinition As String, ByRef sErr As String) As Boolean
        Dim parent As mshtml.IHTMLWindow2 = GetParentWindow(sErr)
        If parent Is Nothing Then Return False
        Try
            parent.execScript(MethodDefinition)
            Return True
        Catch ex As Exception
            sErr = ex.Message & "->" & ex.StackTrace
            Return False
        End Try
    End Function

End Class
