Option Strict On

Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Reflection
Imports System.Text.RegularExpressions

Imports Accessibility

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.JAB
Imports BluePrism.ApplicationManager.HTML
Imports BluePrism.ApplicationManager.ClientCommsI
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.BrowserAutomation
Imports BluePrism.UIAutomation

''' <summary>
''' Class responsible for spying elements within the operating system - it handles
''' all supported modes and the transition between them and interfaces directly with
''' the elements in order to build up identification strings for the chosen elements
''' </summary>
<CLSCompliant(False)>
Public Class clsWindowSpy : Implements IDisposable

#Region " Class scope declarations "

    ''' <summary>
    ''' The different types of spy mode available
    ''' </summary>
    ''' <remarks>Note to developers: if you update this enumeration then please
    ''' update the NextMode() method, and any other similar methods.</remarks>
    <Flags>
    Public Enum SpyMode

        ''' <summary>
        ''' Captures win32 windows using the Microsoft
        ''' Windows win32 API.
        ''' </summary>
        <SpyModeInfoAttribute("Win32", KnownColor.LightPink, KnownColor.Red)>
        Win32 = &H1

        ''' <summary>
        ''' Captures application elements exposed by the UI Automation API
        ''' </summary>
        <SpyModeInfoAttribute("UI Automation", KnownColor.LightSteelBlue, KnownColor.Purple)>
        UIAutomation = &H2

        ''' <summary>
        ''' Captures elements of applications exposed by the
        ''' Microsoft Active Accessibility API.
        ''' </summary>
        <SpyModeInfoAttribute("Accessibility", KnownColor.LightBlue, KnownColor.Blue)>
        Accessibility = &H4

        ''' <summary>
        ''' Captures win32 windows using the Microsoft Windows win32 API - includes
        ''' a screenshot of the window at the time of the capture.
        ''' </summary>
        <SpyModeInfoAttribute("Region",
         KnownColor.MistyRose, KnownColor.IndianRed, "Region")>
        Win32Region = &H8

        ''' <summary>
        ''' Captures elements of an html document, by examining the
        ''' DOM exposed by an MSIE-compatible application.
        ''' </summary>
        <SpyModeInfoAttribute("IE HTML", KnownColor.LightSeaGreen, KnownColor.Green)>
        Html = &H10

        ''' <summary>
        ''' Captures an element from a web page using a browser plugin
        ''' </summary>
        <SpyModeInfoAttribute("Browser", KnownColor.PaleVioletRed, KnownColor.MediumPurple)>
        Web = &H20

        ''' <summary>
        ''' Captures features of the target application, exposed
        ''' by the Java Access Bridge API.
        ''' </summary>
        <SpyModeInfoAttribute("Java", KnownColor.LightYellow, KnownColor.Yellow)>
        Java = &H40

        ''' <summary>
        ''' SAP GUI API
        ''' </summary>
        SAP = &H80

        ''' <summary>
        ''' Captures a bitmap selected by dragging (like a region) and returns
        ''' the data. If spying is started in this mode, then it's the only mode
        ''' available.
        ''' </summary>
        Bitmap = &H100

        ''' <summary>
        ''' The basic default spy modes which require no further checks (current
        ''' application modes, licence checks etc.) to be enabled
        ''' </summary>
        BasicDefaults = Win32 Or Accessibility Or SAP Or Win32Region Or UIAutomation

        ''' <summary>
        ''' The default spy modes for web-based applications
        ''' </summary>
        WebDefaults = Web Or Win32 Or UIAutomation Or Win32Region

    End Enum

    ''' <summary>
    ''' A structure to hold all the gathered window information
    ''' </summary>
    Public Structure WindowInformation
        Public WindowHandle As IntPtr  'The handle of the selected window
        Public ScreenRect As Rectangle  'The extent of the window in screen coords
    End Structure

    ''' <summary>
    ''' Invalidate the InfoCache in case of locale change
    ''' </summary>
    Public Shared Sub InvalidateInfoCache()
        SpyModeInfoAttribute.InvalidateInfoCache()
    End Sub

    ''' <summary>
    ''' Attribute describing various metadata around a spy mode.
    ''' </summary>
    Private Class SpyModeInfoAttribute : Inherits Attribute

        ' Caches the attributes against the spy mode to which they are assigned -
        ' on the presumption that this is quicker than enumerating the attributes
        ' each time.
        Private Shared sInfoCache As IDictionary(Of SpyMode, SpyModeInfoAttribute) =
         GetSynced.IDictionary(New Dictionary(Of SpyMode, SpyModeInfoAttribute))

        ' The user-friendly name of the spy mode
        Private mName As String
        ' The narrative to display explaining the spy mode
        Private mNarrative As String
        ' The background colour to use for the spy mode.
        Private mBackColor As Color
        ' The foreground colour to use for the spy mode
        Private mForeColor As Color

        ''' <summary>
        ''' Creates a new empty spy mode info which will adopt the name of the
        ''' spymode it is created for and the default narrative and colour attributes
        ''' </summary>
        Public Sub New()
            Me.New(Nothing, My.Resources.UseCtrlAndLeftClickToSelectTheHighlightedItemOrCtrlAndRightClickToCancel, KnownColor.LightPink, KnownColor.Red)
        End Sub

        ''' <summary>
        ''' Creates a new spy mode info attribute with the given properties and a
        ''' default narrative
        ''' </summary>
        ''' <param name="name">The user-friendly name</param>
        ''' <param name="backColor">The background colour to use for the mode</param>
        ''' <param name="foreColor">The foreground colour to use for the mode</param>
        Public Sub New(ByVal name As String,
         ByVal backColor As KnownColor, ByVal foreColor As KnownColor)
            Me.New(name, My.Resources.UseCtrlAndLeftClickToSelectTheHighlightedItemOrCtrlAndRightClickToCancel, backColor, foreColor)
        End Sub

        ''' <summary>
        ''' Creates a new spy mode info attribute with the given properties
        ''' </summary>
        ''' <param name="name">The user-friendly name</param>
        ''' <param name="narrative">The narrative description of the spy mode</param>
        ''' <param name="backColor">The background colour to use for the mode</param>
        ''' <param name="foreColor">The foreground colour to use for the mode</param>
        Public Sub New(ByVal name As String, ByVal narrative As String,
         ByVal backColor As KnownColor, ByVal foreColor As KnownColor)
            mName = name
            mNarrative = narrative
            mBackColor = Color.FromKnownColor(backColor)
            mForeColor = Color.FromKnownColor(foreColor)
        End Sub

        ''' <summary>
        ''' Creates a new spy mode info attribute with the given properties
        ''' </summary>
        ''' <param name="name">The user-friendly name</param>
        ''' <param name="backColor">The background colour to use for the mode</param>
        ''' <param name="foreColor">The foreground colour to use for the mode</param>
        '''  <param name="extraNarrativeFromRes">The narrative description of the spy mode</param>

        Public Sub New(ByVal name As String,
                       ByVal backColor As KnownColor, ByVal foreColor As KnownColor, ByVal extraNarrativeFromRes As String)
            mName = name
            mNarrative = My.Resources.UseCtrlAndLeftClickToSelectTheHighlightedItemOrCtrlAndRightClickToCancel & vbCrLf & vbCrLf & My.Resources.ResourceManager.GetString("extranarrative_" & extraNarrativeFromRes)
            mBackColor = Color.FromKnownColor(backColor)
            mForeColor = Color.FromKnownColor(foreColor)
        End Sub

        ''' <summary>
        ''' Gets the info attribute assigned to the given spy mode.
        ''' </summary>
        ''' <param name="m">The spy mode for which the info attribute is required.
        ''' </param>
        ''' <returns>The info attribute assigned to the given spy mode or a newly
        ''' minted default attribute if none was assigned at design time.</returns>
        Public Shared Function GetInfo(m As SpyMode) As SpyModeInfoAttribute
            ' Check the cache first - if there isn't an info object there, retrieve
            ' it from the spy mode via reflection - if it hasn't got one, create a
            ' default one for it and use that.
            Dim attr As SpyModeInfoAttribute = Nothing
            If Not sInfoCache.TryGetValue(m, attr) Then

                Dim info As FieldInfo = m.GetType().GetField(m.ToString())
                If info IsNot Nothing Then
                    Dim customAttribs = info.GetCustomAttributes(GetType(SpyModeInfoAttribute), False)
                    If customAttribs IsNot Nothing Then
                        attr = customAttribs.OfType(Of SpyModeInfoAttribute).FirstOrDefault()
                    End If
                End If

                ' If there's none there, create a default one for this spy mode
                If attr Is Nothing Then attr = New SpyModeInfoAttribute()

                ' Check the name - if it's null, it needs to be set from the
                ' spy mode's name - ie. that is the default value to use (and it
                ' can't be retrieved on attribute creation)
                If attr.mName Is Nothing Then attr.mName = m.ToString()

                ' Check if there is a localized version of the name
                Dim resxKey As String = Regex.Replace(attr.mName, "\b(\w)+\b",
                                                      Function(lambda) _
                                                         lambda.Value(0).ToString().ToUpper() &
                                                         lambda.Value.Substring(1))
                resxKey = "SpyModeInfoAttributeName_" & Regex.Replace(resxKey, "[^a-zA-Z0-9]*", "")
                Dim res As String = My.Resources.ResourceManager.GetString($"{resxKey}")
                If res IsNot Nothing Then
                    attr.mName = res
                End If

                ' Cache the attribute
                sInfoCache(m) = attr

            End If

            Return attr
        End Function

        ''' <summary>
        ''' Invalidate the InfoCache in case of locale change
        ''' </summary>
        Public Shared Sub InvalidateInfoCache()
            sInfoCache.Clear()
        End Sub

        ''' <summary>
        ''' The user-friendly name of the spy mode
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        ''' The narrative describing the spy mode
        ''' </summary>
        Public ReadOnly Property Narrative() As String
            Get
                Return mNarrative
            End Get
        End Property

        ''' <summary>
        ''' The background colour to use for the spy mode
        ''' </summary>
        Public ReadOnly Property BackColor() As Color
            Get
                Return mBackColor
            End Get
        End Property

        ''' <summary>
        ''' The foreground color to use for the spymode
        ''' </summary>
        Public ReadOnly Property ForeColor() As Color
            Get
                Return mForeColor
            End Get
        End Property

    End Class

#End Region

#Region " Member Variables "

    ' Flag indicating if a control key is currently held down
    Private Ctrl As Boolean

    ' The state of the left ctrl button when the mousedown event was handled
    Private MouseDownLeftCtrl As Boolean

    ' The state of the right ctrl button when the mousedown event was handled
    Private MouseDownRightCtrl As Boolean

    ' A list of child process IDs of the target process
    Private mTargetChildPIDs As IList(Of Integer)

    ' Flag indicating if spy mode is currently active
    Private mSpyActive As Boolean = False

    ' A list of HTML documents in which to find HTML elements
    Private mDocuments As List(Of clsHTMLDocument)

    ' Thread used to handle spying - active when the spy has been started, and until
    ' it completes.
    Private mSpyingThread As Thread

    ' The PID of our target process, or 0 for any.
    Private mTargetPID As Int32

    ' The target application we are spying, or Nothing for any.
    Private mTargetApp As ITargetApp

    ' Message describing an exception which occurs when spying. Null indicates no err
    Private msExceptionMessage As String

    ' Flag indicating if the last spy operation failed.
    Private mbFailed As Boolean

    ' Flag indicating if the spy mode has ended correctly
    Private mEnded As Boolean

    ' A global hook to capture mouse movement within the operating system
    Private mGlobalMouseHook As clsGlobalMouseHook

    ' A global hook to capture key events within the operating system
    Private mGlobalKeyboardHook As clsGlobalKeyboardHook

    'NB - this is actually storing the currently highlighted Win32 window.
    Private mPreviousWindow As IntPtr

    ' Flag indicating whether the last call to WindowFromAccessibleObject using
    ' the current AA element and the current (er 'previous') hwnd failed
    Private mGetWindowFromObjectFailed As Boolean

    ' The tooltip to display indicating the spy mode.
    Private mToolTip As frmToolTip

    ' The chosen HTML element
    Private mHTMLElement As clsHTMLElement

    ' The spy mode currently in use.
    Private mSpyMode As SpyMode

    ' Spy modes that are enabled for this spy operation
    Private mEnabledSpyModes As SpyMode

    ' The factory used to generate UIAutomation elements
    Private ReadOnly mAutomation As IAutomationFactory

    Private ReadOnly mWebPageProvider As IWebPageProvider

    ' The last recorded location of a mouse move event
    Private mLocation As Point

    ' The Active Accessibility element currently highlighted
    Private mAccObject As IAccessible

    ' The highlighted UIA element
    Private mUIAElement As IAutomationElement

    ' The highlighted Web element
    Private mWebElement As IWebElement

    ' The Child ID of the highlighted element at the last known location of the mouse
    Private mChildID As Object

    ' Flag indicating if an element is highlighted
    Private mGotSelection As Boolean

    ' The JABContext currently highlighted
    Private mHighlightedJABContext As JABContext

    ' The ID (path) of the SAP Gui component currently highlighted
    Private mHighlightedSAPComponent As String

    ' The highlighter responsible for highlighting windows / elements
    Private mHighlighter As clsWindowHighlighter

    ' The bitmap data captured and converted into a PixRect object
    Private mCapturedPixRect As clsPixRect

    ' Contains all the gathered window information
    Private mCurrentWindowInformation As WindowInformation

    ' State flag for the spying thread - set to true to end the thread cleanly
    Private mTerminate As Boolean

    ' Flag indicating if a window has been selected
    Private mWindowChosen As Boolean = False

    ' Flag indicating if a SAP component has been selected
    Private mSAPChosen As Boolean = False

    ' Flag indicating if an AA element has been selected
    Private mAAElementChosen As Boolean = False

    ' Flag indicating if a UIA element has been selected
    Private mUIAElementChosen As Boolean

    ' Flag indicating if a Web element has been selected
    Private mWebElementChosen As Boolean

    ' The current web page with highlighting
    Private mHighlightedWebPage As IWebPage = Nothing

    ' Flag indicating if an HTML element has been selected
    Private mHTMLElementChosen As Boolean

    ' Flag indicating if a JAB element has been selected
    Private mJABElementChosen As Boolean

    ' Flag indicating if a bitmap has been captured
    Private mBitmapChosen As Boolean = False

    ' The active JABWrapper object to use for spying Java applications via the Java
    ' Access Bridge.
    Private mJAB As JABWrapper = Nothing

    ''' <summary>
    ''' Restricts win32 spying to windows that are the smallest child of
    ''' the greatest ancestor of the window at the given point?!
    ''' </summary>
    Private mRestrictedWin32Spying As Boolean = True
#End Region

#Region " Auto-Properties "

    ''' <summary>
    ''' True to retrieve Win32 data on completed spy operation; False otherwise.
    ''' </summary>
    ''' <remarks>This is only honoured for UIA spy operations; otherwise Win32 data
    ''' is retrieved for Win32 and AA spy operations and not for other operations.
    ''' </remarks>
    Public Property IncludeWin32 As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="initialMode">The initial spy mode.</param>
    ''' <param name="allowedModes">All allowed spy modes for this instance.</param>
    Public Sub New(initialMode As SpyMode, allowedModes As SpyMode, restrictedWin32Spying As Boolean)
        mEnabledSpyModes = allowedModes

        mAutomation = AutomationTypeProvider.GetType(Of IAutomationFactory)
        mWebPageProvider = DependencyResolver.Resolve(Of IWebPageProvider)
        mSpyMode = initialMode
        mRestrictedWin32Spying = restrictedWin32Spying
    End Sub

    Public Sub New(initialMode As SpyMode, allowedModes As SpyMode, restrictedWin32Spying As Boolean, jab As JABWrapper)
        Me.New(initialMode, allowedModes, restrictedWin32Spying)
        mJAB = jab
    End Sub

#End Region

#Region " Destructor / Dispose Support "

    Private isDisposed As Boolean = False
    ' To detect redundant calls

    ''' <summary>
    ''' Destructor to clean up unmanaged references in this class
    ''' </summary>
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ''' <summary>
    ''' Disposes of this window spy class.
    ''' </summary>
    ''' <param name="disposing">True to clean up both managed and unmanaged
    ''' resources; False to only clean up unmanaged resources</param>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not isDisposed Then
            Try
                If mSpyActive Then
                    Debug.Assert(mSpyActive, "Dispose called when spy not active")

                    'Tell the main spying thread loop to terminate
                    mTerminate = True

                    'Remove hooks...
                    If mGlobalMouseHook IsNot Nothing Then
                        RemoveHandler mGlobalMouseHook.MouseMoved, AddressOf MouseMove
                        RemoveHandler mGlobalMouseHook.MouseUp, AddressOf MouseUp
                        RemoveHandler mGlobalMouseHook.MouseDown, AddressOf MouseDown
                        RemoveHandler mGlobalKeyboardHook.KeyDown, AddressOf KeyDown
                        RemoveHandler mGlobalKeyboardHook.KeyUp, AddressOf KeyUp

                        mGlobalMouseHook.Dispose()
                        mGlobalKeyboardHook.Dispose()
                    End If

                    mJAB?.CleanupMouseHook()

                    mHighlightedWebPage?.RemoveHighlighting()

                    'Finish cleaning up...
                    If mHighlighter IsNot Nothing Then
                        mHighlighter.Dispose()
                        mHighlighter = Nothing
                    End If

                    DisposeTooltip()

                    mPreviousWindow = IntPtr.Zero

                    mSpyActive = False
                End If
            Catch ' Ignore any failures
            End Try
        End If
        isDisposed = True
    End Sub

    ''' <summary>
    ''' Explicitly disposes of managed and unmanaged resources in use by this object
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the metadata surrounding the current spy mode.
    ''' </summary>
    Private ReadOnly Property ModeInfo() As SpyModeInfoAttribute
        Get
            Return SpyModeInfoAttribute.GetInfo(mSpyMode)
        End Get
    End Property

    ''' <summary>
    ''' The information about the currently selected window
    ''' </summary>
    Public ReadOnly Property CurrentWindow() As WindowInformation
        Get
            Return mCurrentWindowInformation
        End Get
    End Property

    ''' <summary>
    ''' The captured bitmap data in a PixRect object or null if there is no captured
    ''' bitmap data.
    ''' </summary>
    Public ReadOnly Property CapturedPixRect() As clsPixRect
        Get
            Return mCapturedPixRect
        End Get
    End Property

    ''' <summary>
    ''' Contains the exception message when spying fails.
    ''' </summary>
    Public ReadOnly Property ExceptionMessage() As String
        Get
            Return msExceptionMessage
        End Get
    End Property

    ''' <summary>
    ''' True when spying has failed.
    ''' </summary>
    Public ReadOnly Property Failed() As Boolean
        Get
            Return mbFailed
        End Get
    End Property

    ''' <summary>
    ''' True when spying has completed.
    ''' </summary>
    Public ReadOnly Property Ended() As Boolean
        Get
            Return mEnded
        End Get
    End Property

    ''' <summary>
    ''' Determines whether the user has chosen a window. When True, clients may
    ''' retrieve information about the window chosen from the public member
    ''' CurrentWindowInformation.
    ''' </summary>
    Public ReadOnly Property WindowChosen() As Boolean
        Get
            Return mWindowChosen
        End Get
    End Property

    Public ReadOnly Property SAPChosen() As Boolean
        Get
            Return mSAPChosen
        End Get
    End Property

    Public ReadOnly Property AAElementChosen() As Boolean
        Get
            Return mAAElementChosen
        End Get
    End Property

    ''' <summary>
    ''' Gets a flag indicating if a UIA aelement has been chosen
    ''' </summary>
    Public ReadOnly Property UIAElementChosen() As Boolean
        Get
            Return mUIAElementChosen
        End Get
    End Property

    Public ReadOnly Property WebElementChosen As Boolean
        Get
            Return mWebElementChosen
        End Get
    End Property

    Public ReadOnly Property JABElement() As JABContext
        Get
            Return mHighlightedJABContext
        End Get
    End Property

    Public ReadOnly Property HTMLElementChosen() As Boolean
        Get
            Return mHTMLElementChosen
        End Get
    End Property

    Public ReadOnly Property JABElementChosen() As Boolean
        Get
            Return mJABElementChosen
        End Get
    End Property

    ''' <summary>
    ''' Determines whether the user has chosen a bitmap. When True, clients may
    ''' retrieve the bitmap from the public CapturedPixRect member.
    ''' </summary>
    Public ReadOnly Property BitmapChosen() As Boolean
        Get
            Return mBitmapChosen
        End Get
    End Property

    Public ReadOnly Property SAPComponent() As String
        Get
            Return mHighlightedSAPComponent
        End Get
    End Property

    Public ReadOnly Property AAElement() As Accessibility.IAccessible
        Get
            Return mAccObject
        End Get
    End Property

    Public ReadOnly Property AAElementID() As Object
        Get
            Return mChildID
        End Get
    End Property

    ''' <summary>
    ''' Gets the selected UIA element, or null if none has been selected
    ''' </summary>
    ReadOnly Property UIAElement() As IAutomationElement
        Get
            Return mUIAElement
        End Get
    End Property

    ReadOnly Property WebElement As IWebElement
        Get
            Return mWebElement
        End Get
    End Property

    Public ReadOnly Property HTMLElement() As clsHTMLElement
        Get
            Return mHTMLElement
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Spy directly at a specific location on the screen, with no user interface.
    ''' </summary>
    ''' <param name="targetapp">An implementation of ClientCommsI.ITargetApp that
    ''' specifies the target application to be spied. Can be Nothing to allow any
    ''' application to be spied.</param>
    ''' <param name="at">The screen position to spy at.</param>
    Public Sub SpyAt(ByVal targetapp As ITargetApp, ByVal at As Point)

        mTargetApp = targetapp
        If mTargetApp Is Nothing Then
            mTargetPID = 0
        Else
            mTargetPID = mTargetApp.PID
            GetChildPIDs(mTargetPID)
        End If

        mHighlighter = New clsWindowHighlighter()
        Try
            mPreviousWindow = IntPtr.Zero
            ProcessLocation(at)
            If mGotSelection Then ProcessCompletion()
        Finally
            If mHighlighter IsNot Nothing Then mHighlighter.Dispose()
            mHighlighter = Nothing
        End Try

    End Sub


    ''' <summary>
    ''' Start the hooks
    ''' </summary>
    ''' <param name="targetapp">An implementation of ClientCommsI.ITargetApp that
    ''' specifies the target application to be spied. Can be Nothing to allow any
    ''' application to be spied.</param>
    Public Sub StartSpy(ByVal targetapp As ITargetApp)

        Debug.Assert(Not mSpyActive, "StartSpy called when spying already active")

        mSpyActive = True
        mTerminate = False
        mEnded = False
        mbFailed = False

        mTargetApp = targetapp
        If mTargetApp Is Nothing Then
            mTargetPID = 0
        Else
            mTargetPID = mTargetApp.PID
            GetChildPIDs(mTargetPID)
        End If

        mPreviousWindow = IntPtr.Zero

        mHighlighter = New clsWindowHighlighter()

        'ControlKeys = Keys
        mToolTip = New frmToolTip()
        UpdateTooltip()

        mSpyingThread = New Thread(AddressOf SpyThread)
        mSpyingThread.Start()
        mGlobalMouseHook = New clsGlobalMouseHook()
        AddHandler mGlobalMouseHook.MouseMoved, AddressOf MouseMove
        AddHandler mGlobalMouseHook.MouseUp, AddressOf MouseUp
        AddHandler mGlobalMouseHook.MouseDown, AddressOf MouseDown
        mGlobalKeyboardHook = New clsGlobalKeyboardHook()
        AddHandler mGlobalKeyboardHook.KeyDown, AddressOf KeyDown
        AddHandler mGlobalKeyboardHook.KeyUp, AddressOf KeyUp

    End Sub


    ''' <summary>
    ''' Thread that handles updates while spying is in progress. Too much work is
    ''' done to allow us to be able to do it in the mouse/keyboard event handlers, so
    ''' those handlers just save information to be processed here.
    ''' </summary>
    Private Sub SpyThread()
        Try
            'Keep track of the last location we processed so we don't keep on
            'doing the same one over and over again - the code (e.g. HTML) can
            'be extremely slow!
            Dim lastloc As New Point(-1, -1)

            While Not mTerminate
                If lastloc <> mLocation Then
                    lastloc = mLocation
                    ProcessLocation(lastloc)
                End If
                Thread.Sleep(50)
            End While
        Catch ex As Exception
            clsConfig.LogException(ex)
            msExceptionMessage = ex.Message
            mbFailed = True
            If mSpyActive Then Dispose()
        End Try
        mEnded = True
    End Sub

    Private Sub ProcessLocation(ByVal loc As Point)
        Select Case mSpyMode
            Case SpyMode.Java
                HandleJABMouseMovements(loc)
            Case SpyMode.Accessibility
                HandleAccessibilityMouseMovements(loc)
            Case SpyMode.UIAutomation
                HandleUIAMouseMovements(loc)
            Case SpyMode.Web
                HandleWebMouseMovements(loc)
            Case SpyMode.Html
                HandleHTMLMouseMovements(loc)
            Case SpyMode.Win32, SpyMode.Bitmap, SpyMode.Win32Region
                HandleMouseMovements(loc)
            Case SpyMode.SAP
                HandleSAPMouseMovements(loc)
        End Select
    End Sub

    Private Function ProcessCompletion() As Boolean
        Select Case mSpyMode
            Case SpyMode.Accessibility
                GetWindowInformation()
                mAAElementChosen = True
                Return True
            Case SpyMode.UIAutomation
                If IncludeWin32 Then GetWindowInformation()
                mUIAElementChosen = True
                Return True
            Case SpyMode.SAP
                mSAPChosen = True
                Return True
            Case SpyMode.Web
                mWebElementChosen = True
                Return True
            Case SpyMode.Html
                mHTMLElementChosen = True
                Return True
            Case SpyMode.Java
                mJABElementChosen = True
                Return True
            Case SpyMode.Win32
                GetWindowInformation()
                mWindowChosen = True
                Return True
            Case SpyMode.Bitmap
                mCapturedPixRect = clsPixRect.Capture(mPreviousWindow)
                mBitmapChosen = True
                Return True
            Case SpyMode.Win32Region
                GetWindowInformation()
                mCapturedPixRect = clsPixRect.Capture(mPreviousWindow)
                mWindowChosen = True
                mBitmapChosen = True
                Return True
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Cancel spying.
    ''' </summary>
    Public Sub CancelSpy()
        If mSpyActive Then Dispose()
    End Sub

    Private Sub MouseMove(ByVal p As Point)
        mLocation = p
    End Sub

    Private Sub KeyDown(ByVal sender As Object, ByVal e As GlobalKeyEventArgs)
        'Alt key goes to next mode...
        Select Case e.Key
            Case Keys.Menu, Keys.LMenu, Keys.RMenu : NextMode()
            Case Keys.Control, Keys.LControlKey, Keys.RControlKey : Ctrl = True
        End Select
    End Sub

    Private Sub KeyUp(ByVal sender As Object, ByVal e As GlobalKeyEventArgs)
        Select Case e.Key
            Case Keys.Control, Keys.LControlKey, Keys.RControlKey : Ctrl = False
        End Select
    End Sub

    ''' <summary>
    ''' Handle the MouseDown global hook
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub MouseDown(ByVal e As clsGlobalMouseEventArgs)
        If Ctrl Then e.Cancel = True
        Select Case e.Button
            Case MouseButtons.Left : MouseDownLeftCtrl = Ctrl
            Case MouseButtons.Right : MouseDownRightCtrl = Ctrl
        End Select
    End Sub


    ''' <summary>
    ''' Handle the MouseUp global hook
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub MouseUp(ByVal e As clsGlobalMouseEventArgs)
        Select Case e.Button
            Case MouseButtons.Left
                If MouseDownLeftCtrl Then
                    'Ctrl and left click selects what's under the cursor...
                    If mGotSelection AndAlso ProcessCompletion() Then Dispose()
                    e.Cancel = True
                End If

            Case MouseButtons.Right
                If MouseDownRightCtrl Then
                    'Ctrl and Right Click cancels spying
                    Dispose()
                    e.Cancel = True
                End If
        End Select

    End Sub


    ''' <summary>
    ''' Select the next available spy mode and update the tooltip.
    ''' </summary>
    Private Sub NextMode()

        If mSpyMode = SpyMode.Bitmap Then Return

        If mSpyMode = SpyMode.Web Then
            mHighlightedWebPage?.RemoveHighlighting()
        End If

        Dim newValue As Integer = mSpyMode
        Do
            newValue = newValue << 1
            If mTargetApp IsNot Nothing AndAlso mTargetApp.SAPAvailable Then
                If newValue > SpyMode.SAP Then newValue = 1
            Else
                If newValue > SpyMode.Java Then newValue = 1
            End If
        Loop While (mEnabledSpyModes And newValue) = 0

        mSpyMode = CType(newValue, SpyMode)

        ' Change the colour of the highlighter
        If mHighlighter IsNot Nothing Then _
         mHighlighter.HighlightColour = ModeInfo.ForeColor

        ' And update the mouse location - we need to ensure that we have a valid
        ' element being highlighted for the new mode (or nothing if there's nothing
        ' there)
        ProcessLocation(Cursor.Position)

        UpdateTooltip()
    End Sub

    ''' <summary>
    ''' Draws a red rectangle around the supplied window
    ''' </summary>
    ''' <param name="hWnd">The handle of the window to highlight.</param>
    Private Sub Highlight(ByVal hWnd As IntPtr)
        Dim rc As RECT
        GetWindowRect(hWnd, rc)
        mHighlighter.ShowFrame(rc, ModeInfo.ForeColor)
    End Sub

    ''' <summary>
    ''' Handles the html mouse movements 
    ''' </summary>
    ''' <param name="p"></param>
    Private Sub HandleHTMLMouseMovements(ByVal p As Point)

        ' Assume we have *not* got a selection
        mGotSelection = False

        'Lazy initialise the documents.
        If mDocuments Is Nothing Then
            If Not mTargetApp Is Nothing Then
                mDocuments = mTargetApp.GetHtmlDocuments()
            End If
            If mDocuments.Count = 0 Then
                mDocuments = Nothing
                'We couldn't find an internet explorer instance so just move to the next mode.
                NextMode()
                Exit Sub
            End If
            For Each mDocument As clsHTMLDocument In mDocuments
                ' If the highlighter is not set, that effectively means that the
                ' spy operation has been cancelled
                If Not mSpyActive Then Return
                mDocument.Initialise()
            Next
        End If

        For Each mDocument As clsHTMLDocument In mDocuments
            ' If the spy has been cancelled, leave now
            If Not mSpyActive Then Return

            ' Try and get the element at the current location
            mHTMLElement = mDocument.ElementFromPoint(p.X, p.Y)

            ' If we didn't get an element, continue looking
            If mHTMLElement Is Nothing Then Continue For

            Try
                ' Double checks for race conditions - the highlighter window can be
                ' disposed and nullified in a different thread, so create a local for
                ' it and double check that before using it.
                Dim hlWin As clsWindowHighlighter = mHighlighter
                If hlWin Is Nothing Then Return

                hlWin.ShowFrame(
                 mHTMLElement.AbsoluteBounds, ModeInfo.ForeColor)

            Catch ode As ObjectDisposedException
                ' Ignore - this just means that the spy operation was cancelled
                ' and we hit a race condition while the reference to the window
                ' was still non-null but the window itself had been disposed of
                Return
            End Try

            mGotSelection = True
            Exit For
        Next

        If Not mGotSelection AndAlso mHighlighter IsNot Nothing Then _
         mHighlighter.HideFrame()

    End Sub



    ''' <summary>
    ''' Handles the active accessibility Mouse movements.
    ''' </summary>
    ''' <param name="p"></param>
    Private Sub HandleAccessibilityMouseMovements(ByVal p As Point)

        AccessibleObjectFromPoint(p, mAccObject, mChildID)

        If Not mAccObject Is Nothing Then
            ShowFrameIfNeeded(p, mAccObject, mChildID)
            mGotSelection = True
        Else
            mHighlighter.HideFrame()
            mGotSelection = False
        End If
    End Sub

    ''' <summary>
    ''' Attempts to pick out an automation element representing the menu item from
    ''' under the given cursor location, rather than the menu element which it
    ''' returns by default for submenus.
    ''' </summary>
    ''' <param name="p">The cursor location at which a dropdown menu element has been
    ''' found</param>
    ''' <param name="elem">The current 'best fit' automation element which represents
    ''' the menu at <paramref name="p"/></param>
    ''' <returns>An automation element representing the menuitem under the cursor, or
    ''' <paramref name="elem"/> if no such element could be found.</returns>
    Private Function AttemptToSpyMenuItem(
     p As Point, elem As IAutomationElement) As IAutomationElement

        ' Get the accessible object from this point
        Dim iacc As IAccessible = Nothing
        Dim accChildId As Object = Nothing
        AccessibleObjectFromPoint(p, iacc, accChildId)

        ' If we didn't get an object, just return what we have already
        If iacc Is Nothing Then Return elem

        ' If we have a menuitem accessible object, get a menuitem UIAutomation
        ' element directly from that.
        Dim role = DirectCast(iacc.accRole, AccessibleRole)
        If role = AccessibleRole.MenuItem Then
            ' If we can get a UIA menuitem element from the iacc element use that
            Dim menuItemElem = mAutomation.FromIAccessible(iacc, CInt(accChildId))
            If menuItemElem IsNot Nothing Then Return menuItemElem
        End If

        ' Otherwise, we can't get the menu item, so just return what we were given
        Return elem

    End Function

    ''' <summary>
    ''' Handles the UI Automation Mouse movements.
    ''' </summary>
    ''' <param name="p">The point that should be used to identify and highlight
    ''' application elements.</param>
    Private Sub HandleUIAMouseMovements(ByVal p As Point)

        Try
            mUIAElement = mAutomation.FromPoint(p)
        Catch ex As UnauthorizedAccessException
            ' If the mouse moves over a process we do not have access to
            ' then an UnauthorizedAccessException will be thrown.
            ' We just need to ignore the exception and continue
        End Try

        If mUIAElement IsNot Nothing Then
            If IncludeWin32 Then mPreviousWindow = mUIAElement.CurrentNativeWindowHandle

            ' If this is a dropdown menu attempt to get the menu item element instead
            ' See us-1457
            Dim curr = mUIAElement.Current
            If curr.ControlType = ControlType.Menu AndAlso
             curr.Name.Contains("DropDown") Then
                mUIAElement = AttemptToSpyMenuItem(p, mUIAElement)
            End If

            HighlightUIAElement(mUIAElement)
            mGotSelection = True

        Else
            mHighlighter.HideFrame()
            mGotSelection = False

        End If
    End Sub

    ''' <summary>
    ''' Handles mouse movements for spying web elements.
    ''' </summary>
    ''' <param name="cursorPosition">The cursor position.</param>
    Private Sub HandleWebMouseMovements(cursorPosition As Point)

        Dim failFindingElement =
                Sub()
                    mHighlightedWebPage?.RemoveHighlighting()
                    mGotSelection = False
                End Sub

        mHighlighter.HideFrame()
        mWebElement = mWebPageProvider.ElementUnderCursor

        If mWebElement Is Nothing Then
            failFindingElement()
            Return
        End If

        If mHighlightedWebPage IsNot Nothing AndAlso mWebElement.Page.Id <> mHighlightedWebPage.Id Then
            mHighlightedWebPage.RemoveHighlighting()
        End If

        Try
            Dim pageCursorPosition = mWebElement.Page.GetCursorPosition()

            If pageCursorPosition.X = -1 Then
                failFindingElement()
                Return
            End If

            mWebElement.Highlight(ModeInfo.ForeColor)
            mHighlightedWebPage = mWebElement.Page
            mGotSelection = True
        Catch ex As TimeoutException
            failFindingElement()
        End Try

    End Sub


    ''' <summary>
    ''' Get the window from the accessible object.
    ''' </summary>
    Private Sub GetWindowFromObject(ByVal resetevent As Object)
        Try
            mGetWindowFromObjectFailed =
             (WindowFromAccessibleObject(mAccObject, mPreviousWindow) <> 0)
        Catch
            mGetWindowFromObjectFailed = True
        Finally
            DirectCast(resetevent, AutoResetEvent).Set()
        End Try
    End Sub

    ''' <summary>
    ''' Handles the JAB Mouse movements.
    ''' </summary>
    ''' <param name="p"></param>
    Private Sub HandleJABMouseMovements(ByVal p As Point)

        If mJAB Is Nothing Then Exit Sub

        'Safe to call as many times as we like
        'since its lazy initialised
        mJAB.SetupMouseHook()

        mHighlightedJABContext = mJAB.CurrentContext

        If Not mHighlightedJABContext Is Nothing Then
            'If we have hold of something like a panel, then
            'find something else more interesting from inside it, instead
            Dim UnwantedRoles As New clsSet(Of String)(New String() {
             "panel", "scroll pane", "layered pane", "root pane", "viewport",
             "frame", "internal frame"}
            )
            mHighlightedJABContext.UpdateCachedInfo()
            While UnwantedRoles.Contains(mHighlightedJABContext.Role) _
             AndAlso mHighlightedJABContext.Children.Count > 0
                mHighlightedJABContext = mHighlightedJABContext.Children(0)
                mHighlightedJABContext.UpdateCachedInfo()
            End While

            HighlightJABContext(mHighlightedJABContext)
            mGotSelection = True
        Else
            mHighlighter.HideFrame()
            mGotSelection = False
        End If
    End Sub

    ''' <summary>
    ''' Highlights the given JAB Context
    ''' </summary>
    Private Sub HighlightJABContext(ByVal jc As JABContext)
        mHighlighter.ShowFrame(jc.ScreenRect, ModeInfo.ForeColor)
    End Sub

    ''' <summary>
    ''' Shows the frame if needed during Active Accessibility Spying mode
    ''' </summary>
    ''' <param name="accobj"></param>
    ''' <param name="ChildID"></param>
    Private Sub ShowFrameIfNeeded(ByVal p As Point, ByVal accobj As IAccessible, ByVal ChildID As Object)
        'GetWindowFromObject has to be called in a seperate STA thread otherwise it doesn't work.
        '(WindowFromAccessibleObject always returns a hwnd of zero)
        Using r As New AutoResetEvent(False)
            Dim t As New Thread(AddressOf GetWindowFromObject)
            t.SetApartmentState(ApartmentState.STA)
            t.Start(r)
            r.WaitOne()
        End Using

        If mGetWindowFromObjectFailed Then
            mPreviousWindow = GetWindowFromPoint(p)
        End If

        'hPreviousWindow was set by the GetWindowFromObject call above.
        If WindowOwnedByTarget(mPreviousWindow) Then
            Dim x As Integer, y As Integer, w As Integer, h As Integer
            Try
                accobj.accLocation(x, y, w, h, ChildID)
            Catch
            End Try

            mHighlighter.ShowFrame(New Rectangle(x, y, w, h), ModeInfo.ForeColor)
        Else
            mHighlighter.HideFrame()
        End If
    End Sub

    ''' <summary>
    ''' Converts the given <see cref="System.Windows.Rect"/> value to a
    ''' <see cref="System.Drawing.Rectangle"/>, as used in a lot of our existing
    ''' functions.
    ''' </summary>
    ''' <param name="r">The Rect to convert</param>
    ''' <returns>The Rectangle equivalent of <paramref name="r"/></returns>
    Private Function ToRectangle(r As Windows.Rect) As Rectangle
        Return New Rectangle(CInt(r.X), CInt(r.Y), CInt(r.Width), CInt(r.Height))
    End Function

    ''' <summary>
    ''' Checks if the given process ID is the target process's ID or the ID of a
    ''' child process.
    ''' </summary>
    ''' <param name="pid">The process ID to check</param>
    ''' <returns>True if <paramref name="pid"/> is the ID of the target process or
    ''' the ID of one of the target process's child processes; False otherwise.
    ''' </returns>
    Private Function IsTargetPid(pid As Integer) As Boolean
        Return (mTargetPID = pid OrElse mTargetChildPIDs.Contains(pid))
    End Function

    ''' <summary>
    ''' Highlights the given UIA element, if that element belongs to the target process
    ''' </summary>
    Private Sub HighlightUIAElement(ByVal element As IAutomationElement)
        Dim bounds = element.CurrentBoundingRectangle
        If IsTargetPid(element.CurrentProcessId) AndAlso Not bounds.IsEmpty Then
            Try
                mHighlighter.ShowFrame(bounds, ModeInfo.ForeColor)
            Catch
                mHighlighter.HideFrame()
            End Try
        Else
            mHighlighter.HideFrame()
        End If
    End Sub

    ''' <summary>
    ''' Updates the tooltip in a thread-safe manner.
    ''' </summary>
    ''' <param name="title">The title to be displayed on the
    ''' tooltip.</param>
    ''' <param name="text">The text to be displayed on the tooltip.</param>
    ''' <remarks>To simply change the visibility of the tooltip
    ''' without altering the text or title, use SetToolTipEnabled.</remarks>
    Private Sub UpdateTooltip(ByVal title As String, ByVal text As String)
        ' If there's no tooltip do nothing
        If mToolTip Is Nothing OrElse Not mToolTip.IsHandleCreated Then Return

        ' If we're not running on the UI thread, re-invoke this method on the
        ' correct thread. Then set the tooltip values and show it
        If mToolTip.InvokeRequired Then
            mToolTip.Invoke(
             New Action(Of String, String)(AddressOf UpdateTooltip), title, text)
        Else
            mToolTip.ChangeTooltipBackColor(ModeInfo.BackColor)
            mToolTip.SetToolTip(title, text)
            mToolTip.Visible = True
        End If
    End Sub

    ''' <summary>
    ''' Updates the tooltip according to the current mode
    ''' </summary>
    Private Sub UpdateTooltip()
        ' The tooltip text contains the narrative for the mode
        Dim text As String = ModeInfo.Narrative

        ' If we have multiple modes enabled, append a pointer to explain how to
        ' change modes
        text &= vbCrLf & vbCrLf & My.Resources.PressTheAltKeyToSwitchSpyModes

        ' The title is largely just the name of the current spy mode.
        UpdateTooltip(String.Format(
         My.Resources.UsingTheIdentificationTool0Mode, ModeInfo.Name), text)

    End Sub

    ''' <summary>
    ''' Blah blah blah these functions are ridiculous
    ''' </summary>
    Private Sub HandleSAPMouseMovements(ByVal pt As Point)
        If mTargetApp Is Nothing Then Return
        Try
            Dim sapid As String = mTargetApp.GetSapComponentFromPoint(pt)
            If sapid Is Nothing Then
                mHighlighter.HideFrame()
                mGotSelection = False
                Return
            End If

            Dim r As Rectangle = mTargetApp.GetSapComponentScreenRect(sapid)
            mHighlighter.ShowFrame(r)
            mGotSelection = True

            mHighlightedSAPComponent = sapid

        Catch
            mHighlighter.HideFrame()
            mGotSelection = False
            NextMode()
        End Try
    End Sub

    ''' <summary>
    ''' Updates the tooltip form, draws the red window outline or dragged selection
    ''' </summary>
    Private Sub HandleMouseMovements(ByVal pt As Point)
        Try
            Dim hWnd As IntPtr = GetWindowFromPoint(pt)
            ' If we had a window highlighted and it was different to the current one
            ' Hide the highlighter
            If mPreviousWindow <> IntPtr.Zero AndAlso mPreviousWindow <> hWnd Then
                mHighlighter.HideFrame()
                mGotSelection = False
            Else
                ' If this window is owned by our target window, highlight it,
                ' otherwise reset our window handle to NULL
                If WindowOwnedByTarget(hWnd) Then
                    Highlight(hWnd)
                    mGotSelection = True
                Else
                    hWnd = IntPtr.Zero
                End If
            End If
            mPreviousWindow = hWnd

        Catch ex As Exception
            Debug.Fail(ex.ToString())

        End Try
    End Sub

    ''' <summary>
    ''' Retrieves the handle to the window containing the specified point.
    ''' </summary>
    ''' <param name="mouseLocation">The point to be contained by a window.</param>
    ''' <returns>Returns the handle to a window containing the specified point, or
    ''' IntPtr.Zero if no window is found.</returns>
    Private Function GetWindowFromPoint(mouseLocation As Point) As IntPtr
        Dim windowFromPoint = modWin32.WindowFromPoint(mouseLocation)
        If mRestrictedWin32Spying Then
            Dim windowHandle = GetUltimateParentContainingPoint(windowFromPoint, mouseLocation)
            Return GetSmallestSubWindowContainingPoint(windowHandle, mouseLocation)
        End If
        Return windowFromPoint
    End Function

    ''' <summary>
    ''' Get the smallest child window of a given window that contains the given
    ''' point.
    ''' </summary>
    ''' <param name="hWnd">The parent window</param>
    ''' <param name="point">The point to check against.</param>
    ''' <returns>The handle of the window found.</returns>
    Private Function GetSmallestSubWindowContainingPoint(ByVal hWnd As IntPtr, ByVal point As POINTAPI) As IntPtr
        'By default we assume that this is already the smallest window
        Dim bestWindowFound As IntPtr = hWnd
        Dim rectOfBestFound As RECT
        GetWindowRect(hWnd, rectOfBestFound)

        'Go through each child in turn measuring window sizes
        Dim trialRect As RECT
        Dim hChild As IntPtr = GetWindow(hWnd, modWin32.GetWndConsts.GW_CHILD)
        While hChild <> IntPtr.Zero

            If IsVisibleWindow(hChild) Then
                GetWindowRect(hChild, trialRect)
                If trialRect.Contains(point) Then
                    Dim w As IntPtr = GetSmallestSubWindowContainingPoint(hChild, point)
                    Dim correspondingRect As RECT
                    If GetWindowRect(w, correspondingRect) <> 0 Then
                        If RectIsSmallerThan(correspondingRect, rectOfBestFound) Then
                            rectOfBestFound = correspondingRect
                            bestWindowFound = w
                        End If
                    End If
                End If
            End If

            'Move on to next child
            hChild = GetWindow(hChild, GW_HWNDNEXT)

        End While

        Return bestWindowFound

    End Function

    ''' <summary>
    ''' Determines if the first rectangle supplied is smaller in each dimension than
    ''' the second.
    ''' </summary>
    ''' <param name="firstRect">The first of the two rectangles to compare.</param>
    ''' <param name="secondRect">The second of the two rectangles to compare.</param>
    ''' <returns>Returns True if the first rectangle is at least as small as the
    ''' second in both width and height, False otherwise.</returns>
    Private Function RectIsSmallerThan(ByVal firstRect As RECT, ByVal secondRect As RECT) As Boolean
        Return firstRect.Width <= secondRect.Width AndAlso firstRect.Height <= secondRect.Height
    End Function


    ''' <summary>
    ''' Gets the greatest ancestor of the supplied window which contains the supplied
    ''' point.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window whose ancestor is sought.</param>
    ''' <param name="point">The point to be tested. This should be contained in the
    ''' windowrect of the supplied window.</param>
    ''' <returns>Returns the handle to the greatest ancestor, containing the supplied
    ''' point.</returns>
    Private Function GetUltimateParentContainingPoint(
     ByVal hWnd As IntPtr, ByVal point As POINTAPI) As IntPtr
        Dim hParent As IntPtr = GetParent(hWnd)
        If hParent = IntPtr.Zero Then Return hWnd

        'We shouldn't rise above top-level windows which are
        'owned by other top-level windows.
        If IsDialogWindow(hWnd) AndAlso IsDialogWindow(hParent) Then
            Return hWnd
        End If

        Dim parentRect As RECT
        GetWindowRect(hParent, parentRect)
        If parentRect.Contains(point) Then
            Return GetUltimateParentContainingPoint(hParent, point)
        End If

        Return hWnd
    End Function

    ''' <summary>
    ''' Determine if the given window is a dialog window, i.e. if it has the
    ''' WS_DLGFRAME style set.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window to check.</param>
    ''' <returns>True if the window is a dialog window.</returns>
    Private Function IsDialogWindow(ByVal hWnd As IntPtr) As Boolean
        Return (GetWindowLong(hWnd, GWL.GWL_STYLE) And WindowStyles.WS_DLGFRAME) <> 0
    End Function

    ''' <summary>
    ''' Determine if the given window is visible, i.e. if it has the WS_VISIBLE
    ''' style set.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window to check.</param>
    ''' <returns>True if the window is visible.</returns>
    Private Function IsVisibleWindow(ByVal hWnd As IntPtr) As Boolean
        Return (GetWindowLong(hWnd, GWL.GWL_STYLE) And WindowStyles.WS_VISIBLE) <> 0
    End Function

    ''' <summary>
    ''' Gathers information about the current window or control and its parent.
    ''' </summary>
    Private Sub GetWindowInformation()
        'We choose the last window not the current mouse position to respect callback
        Dim hWnd As IntPtr = mPreviousWindow

        'Save window handle...
        mCurrentWindowInformation.WindowHandle = hWnd
        If hWnd = IntPtr.Zero Then Return

        'Save window's screen coordinates...
        Dim rc As RECT
        GetWindowRect(hWnd, rc)
        mCurrentWindowInformation.ScreenRect = rc

    End Sub

    ''' <summary>
    ''' Ends any highlighting. This must be called before
    ''' the object is disposed of.
    ''' </summary>
    Public Sub DisposeTooltip()
        If mToolTip Is Nothing OrElse Not mToolTip.IsHandleCreated Then Return
        If mToolTip.InvokeRequired _
         Then mToolTip.Invoke(New Action(AddressOf mToolTip.Dispose)) _
         Else mToolTip.Dispose()
    End Sub

    ''' <summary>
    ''' Refreshes the magnifying glass
    ''' </summary>
    Public Sub RefreshMagnifyingGlass(ByVal globalPos As Point, ByVal localPos As Point)
        If mToolTip Is Nothing OrElse Not mToolTip.IsHandleCreated Then Return
        If mToolTip.InvokeRequired Then
            mToolTip.Invoke(
             New Action(Of Point, Point)(AddressOf mToolTip.RefreshMagnifyingGlass),
             globalPos, localPos)
        Else
            mToolTip.RefreshMagnifyingGlass(globalPos, localPos)
        End If
    End Sub

    ''' <summary>
    ''' Determine if the given window is owned by our target process. If we don't
    ''' know what our target process is, this will always return True.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window to check.</param>
    ''' <returns>True if owned, False otherwise</returns>
    Public Function WindowOwnedByTarget(ByVal hWnd As IntPtr) As Boolean
        If mTargetPID = 0 Then Return True
        Dim pid As Int32
        modWin32.GetWindowThreadProcessId(hWnd, pid)

        If pid <> mTargetPID AndAlso mTargetChildPIDs IsNot Nothing Then
            'If the pid doesn't match try the child pids see Bug# 4527
            'Some applications such as IE8 launch child processes for each tab.
            For Each childPID As Integer In mTargetChildPIDs
                If childPID = pid Then
                    Return True
                End If
            Next
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Get the Process ID's of all the child processes
    ''' </summary>
    ''' <param name="pid">The Process ID for which to get the child processes of</param>
    Private Sub GetChildPIDs(ByVal pid As Integer)
        mTargetChildPIDs = New List(Of Integer)

        Dim hSnapshot As IntPtr = CreateToolhelp32Snapshot(SnapshotFlags.Process, pid)
        Dim lppe As PROCESSENTRY32 = Nothing
        lppe.dwSize = Marshal.SizeOf(GetType(PROCESSENTRY32))
        If Process32First(hSnapshot, lppe) Then
            Do
                If lppe.th32ParentProcessID = pid Then
                    mTargetChildPIDs.Add(lppe.th32ProcessID)
                End If
            Loop While Process32Next(hSnapshot, lppe)
        End If

    End Sub

#End Region

End Class
