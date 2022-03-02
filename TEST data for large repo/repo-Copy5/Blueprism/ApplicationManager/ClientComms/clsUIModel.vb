Option Strict On

Imports System.Drawing
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Reflection
Imports System.Threading
Imports BluePrism.ApplicationManager.HTML
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.JAB
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Imports ComparisonTypes =
 BluePrism.ApplicationManager.ApplicationManagerUtilities.clsMatchTarget.ComparisonTypes
Imports IdentifierTypes =
 BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.IdentifierTypes
Imports ParameterNames =
 BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.ParameterNames
Imports System.Linq
Imports BluePrism.ApplicationManager.UIAutomation
Imports BluePrism.UIAutomation
Imports BluePrism.Core.Conversion

Public Enum TriggerEvents
    'The button was clicked - detected via WM_COMMAND, which means it catches all
    'possible ways of the button being pressed, but only works in environments where
    'that message is raised - this covers most Win32 buttons, but NOT .NET applications.
    ButtonPressed

    'The left mouse button was pressed down.
    MouseLeftDown

End Enum

Public Class clsTriggerEvent
    Public mEventType As TriggerEvents
    Public miHandle As IntPtr

    Public Sub New(ByVal type As TriggerEvents, ByVal handle As IntPtr)
        mEventType = type
        miHandle = handle
    End Sub
End Class



<CLSCompliant(False)>
Public Class clsUIModel

    Public Event CommsLineProcessed(ByVal sLine As String)

    'A List of all entities in the model. The List is also used as a
    'synchronisation object to ensure that only one thread reads/writes the model at
    'a time.
    Private mAllEntities As New List(Of clsUIEntity)

    'A queue of trigger events that have been detected
    Private mTriggerQueue As New List(Of clsTriggerEvent)

    Private mqAPICallsToProcess As New Queue

    Private mTargetApp As clsLocalTargetApp

    Private Shared mHostPids As IList(Of Integer)

    'These should be injected but the code isn't at that stage yet
    Private ReadOnly mUIAutomationFactory As IAutomationFactory = AutomationTypeProvider.GetType(Of IAutomationFactory)
    Private ReadOnly mUIAutomationHelper As IAutomationHelper = AutomationTypeProvider.GetType(Of IAutomationHelper)
    Private ReadOnly mUIAutomationIdentifierHelper As IUIAutomationIdentifierHelper =
        New UIAutomationIdentifierHelper(mUIAutomationFactory, mUIAutomationHelper)

    ''' <summary>
    ''' Create a new model.
    ''' </summary>
    ''' <param name="target">The target application that owns the model.</param>
    Public Sub New(ByVal target As clsLocalTargetApp)
        mTargetApp = target
        EmptyModel()
    End Sub

    ''' <summary>
    ''' Get a list of all entities in the model.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetAllEntities() As List(Of clsUIEntity)
        SyncLock mAllEntities
            Dim c As New List(Of clsUIEntity)
            For Each e As clsUIEntity In mAllEntities
                c.Add(e)
            Next
            Return c
        End SyncLock
    End Function

    ''' <summary>
    ''' Reset to an empty model with just a desktop window
    ''' </summary>
    Private Sub EmptyModel()
        SyncLock mTriggerQueue
            mTriggerQueue.Clear()
        End SyncLock
        mAllEntities.Clear()
        Dim win As New clsUIWindow()
        win.Handle = IntPtr.Zero
        win.Name = "Desktop"
        mAllEntities.Add(win)
    End Sub

    ''' <summary>
    ''' Process an API call that has occurred in the target application. This updates
    ''' the model according to what has happened. The details are queued to be processed
    ''' on the main thread, since any hint of blocking here can stall the target
    ''' application and lead to a deadlock.
    ''' </summary>
    ''' <param name="objAPICallDetails">The details of the API call.</param>
    Public Sub ProcessAPICall(ByVal objAPICallDetails As clsAPICallDetails)
        SyncLock mqAPICallsToProcess
            mqAPICallsToProcess.Enqueue(objAPICallDetails)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Must be called at regular intervals when hooking to allow the model to be
    ''' updated.
    ''' </summary>
    Friend Sub UpdateModel()
        SyncLock mqAPICallsToProcess
            Dim objAPI As clsAPICallDetails
            Do While mqAPICallsToProcess.Count > 0
                objAPI = CType(mqAPICallsToProcess.Dequeue(), clsAPICallDetails)
                Try
                    clsConfig.LogHook("HOOK: Processing: " & objAPI.SourceLine)
                    ProcessAPICallNow(objAPI)
                Catch e As Exception
                    clsConfig.LogHook("HOOK: Exception processing API call: " & e.Message)
                End Try
            Loop
        End SyncLock
    End Sub

    ''' <summary>
    ''' Process an API call that has occurred in the target application. This updates
    ''' the model according to what has happened. This should never be called from a
    ''' thread that could be triggered by an interaction with the target application.
    ''' From the main thread only, ideally.
    ''' Throws an ApplicationException if the API call could not be processed.
    ''' </summary>
    ''' <param name="details">The details of the API call.</param>
    Private Sub ProcessAPICallNow(ByVal details As clsAPICallDetails)

        If details.Valid Then
            SyncLock mAllEntities
                Select Case details.APIFunction
                    Case "HOOK.MSG"

                        Select Case ParseHex(details.Parameters("msg"))
                            Case WindowMessages.WM_LBUTTONDOWN
                                Dim handle = ParseHandle(details.Parameters("hWnd"))
                                SyncLock mTriggerQueue
                                    mTriggerQueue.Add(New clsTriggerEvent(TriggerEvents.MouseLeftDown, handle))
                                End SyncLock
                        End Select

                    Case "HOOK.CWP.COMMAND"

                        'Look for button clicks...
                        If details.Parameters("CODE") = "0000" Then
                            Dim handle = ParseHandle(details.Parameters("hWnd"))
                            SyncLock mTriggerQueue
                                mTriggerQueue.Add(New clsTriggerEvent(TriggerEvents.ButtonPressed, handle))
                            End SyncLock
                        End If

                    Case "HOOK.HCBT_CREATEWND"

                        Dim handle = ParseHandle(details.Parameters("wParam"))
                        Dim window = FindWindowByHandleP(handle)
                        If window Is Nothing Then
                            Dim newWindow = New clsUIWindow
                            newWindow.Handle = handle
                            newWindow.Name = "Window " & newWindow.Handle.ToString("X8") & " (Hooked Window)"
                            newWindow.ClassName = details.Parameters("classname")
                            newWindow.Parent = FindWindowByHandleP(ParseHandle(details.Parameters("hWndParent")))
                            If Not newWindow.Parent Is Nothing Then
                                newWindow.Ordinal = newWindow.Parent.AddChild(newWindow)
                            End If
                            mAllEntities.Add(newWindow)
                        End If

                    Case "HOOK.HCBT_DESTROYWND"

                        Dim window = FindWindowByHandleP(ParseHandle(details.Parameters("hWnd")))
                        If window Is Nothing Then
                            Throw New InvalidOperationException("Window not found for HOOK.HCBT_DESTROYWND")
                        End If
                        RemoveEntity(window)

                    Case "SetWindowPos", "HOOK.HCBT_MOVESIZE"

                        Dim window = FindWindowByHandleP(ParseHandle(details.Parameters("hWnd")))
                        If window Is Nothing Then
                            Throw New InvalidOperationException("Window not found for SetWindowPos")
                        End If

                    Case "CreateCompatibleDC"

                        'Where a compatible DC is created, we don't set it as being a child of the
                        'related window, because that would make text on it discoverable within
                        'that window, although it wouldn't be visible. Text drawn to the compatible
                        'DC will be tracked, and copied to the real DC when it's written to it by
                        'a BitBlt.
                        Dim newDC As New clsUIDC With {
                                .Handle = ParseHandle(details.Parameters("retval")),
                                .Name = String.Format("Compatible DC {0:X8}", .Handle)
                            }
                        mAllEntities.Add(newDC)

                    Case "BitBlt"

                        Dim srcDC = FindDCByHandle(ParseHandle(details.Parameters("hdcSrc")))
                        If srcDC Is Nothing Then
                            Throw New InvalidOperationException("BitBlt from unknown source DC")
                        End If

                        Dim destDC = FindDCByHandle(ParseHandle(details.Parameters("hdcDest")))
                        If destDC Is Nothing Then
                            Throw New InvalidOperationException("BitBlt to unknown destination DC")
                        End If

                        Dim destX = Integer.Parse(details.Parameters("nXDest"))
                        Dim destY = Integer.Parse(details.Parameters("nYDest"))
                        Dim srcX = Integer.Parse(details.Parameters("nXSrc"))
                        Dim srcY = Integer.Parse(details.Parameters("nYSrc"))
                        Dim cx = Integer.Parse(details.Parameters("cx"))
                        Dim cy = Integer.Parse(details.Parameters("cy"))

                        Dim offset = New Point(0, 0)
                        If destDC.ClientDC Then
                            offset = destDC.Parent.GetClientOffset()
                        End If

                        For Each obscuredText In FindTextObscuredByRect(destDC, destX + offset.X,
                                                                        destY + offset.Y,
                                                                        destX + offset.X + cx - 1,
                                                                        destY + offset.Y + cy - 1).ToList()
                            clsConfig.LogHook("HOOK: BitBlt obscured " & obscuredText.Name)
                            RemoveEntity(obscuredText)
                        Next

                        clsConfig.LogHook(String.Format("HOOK: Copying {0} text items from {1:X8} to {2:X8} with client offset ({3}, {4})",
                                          srcDC.ChildText.Count, srcDC.Handle, destDC.Handle, offset.X, offset.Y))
                        For Each childText In srcDC.ChildText

                            Dim text = New clsUIText With {
                                .Text = childText.Text,
                                .X = childText.X + (destX - srcX) + offset.X,
                                .Y = childText.Y + (destY - srcY) + offset.Y,
                                .Width = childText.Width,
                                .Height = childText.Height,
                                .Name = childText.Name,
                                .Parent = destDC.Parent,
                                .DC = destDC
                            }
                            destDC.ChildText.Add(text)
                            If Not text.Parent Is Nothing Then
                                text.Parent.AddChild(text)
                            End If
                            mAllEntities.Add(text)
                        Next

                    Case "PatBlt"

                        Dim destDC = FindDCByHandle(ParseHandle(details.Parameters("dc")))
                        If destDC Is Nothing Then
                            Throw New InvalidOperationException("PatBlt to unknown destination DC")
                        End If

                        Dim destX = Integer.Parse(details.Parameters("x"))
                        Dim destY = Integer.Parse(details.Parameters("y"))
                        Dim cx = Integer.Parse(details.Parameters("w"))
                        Dim cy = Integer.Parse(details.Parameters("h"))

                        Dim offset = New Point(0, 0)
                        If destDC.ClientDC Then
                            offset = destDC.Parent.GetClientOffset()
                        End If

                        For Each obscuredText In FindTextObscuredByRect(destDC, destX + offset.X,
                                                                        destY + offset.Y,
                                                                        destX + offset.X + cx - 1,
                                                                        destY + offset.Y + cy - 1).ToList()
                            clsConfig.LogHook("HOOK: PatBlt obscured " & obscuredText.Name)
                            RemoveEntity(obscuredText)
                        Next

                    Case "GetDC", "GetDCEx", "BeginPaint", "GetWindowDC", "CreateDC"

                        Dim newDC = New clsUIDC With {
                            .Handle = ParseHandle(details.Parameters("retval")),
                            .Parent = FindWindowByHandleP(ParseHandle(details.Parameters("hWnd"))),
                            .Name = String.Format("DC {0:X8}", .Handle)
                        }
                        'Figure out if this DC is referring to the whole window or just
                        'the client area. There may be more logic required here!
                        If details.APIFunction = "GetWindowDC" Then
                            newDC.ClientDC = False
                        Else
                            newDC.ClientDC = True
                        End If
                        If newDC.Parent IsNot Nothing Then
                            newDC.Parent.AddChild(newDC)
                        End If
                        mAllEntities.Add(newDC)

                    Case "ReleaseDC", "DeleteDC", "EndPaint"

                        Dim DC = FindDCByHandle(ParseHandle(details.Parameters("hDC")))
                        If DC Is Nothing Then
                            Throw New InvalidOperationException("ReleaseDC with no matching GetDC")
                        End If

                        RemoveEntity(DC)

                    Case "SetTextAlign"

                        Dim DC = FindDCByHandle(ParseHandle(details.Parameters("hDC")))
                        If DC Is Nothing Then
                            Throw New InvalidOperationException("SetTextAlign to unknown DC")
                        End If
                        DC.Alignment = Integer.Parse(details.Parameters("align"))

                    Case "FillRect"

                        Dim DC = FindDCByHandle(ParseHandle(details.Parameters("hDC")))
                        If DC Is Nothing Then
                            Throw New InvalidOperationException("FillRect to unknown DC")
                        End If
                        Dim sr = details.Parameters("lprc")
                        If sr <> "NULL" Then
                            Dim srs() = sr.Split(";"c)
                            If srs.Length <> 4 Then
                                Throw New InvalidOperationException("Invalid FillRect rectangle format")
                            End If
                            Dim x1 = Integer.Parse(srs(0))
                            Dim y1 = Integer.Parse(srs(1))
                            Dim x2 = Integer.Parse(srs(2))
                            Dim y2 = Integer.Parse(srs(3))
                            For Each obscuredText In FindTextObscuredByRect(DC, x1, y1, x2, y2).ToList()
                                clsConfig.LogHook("HOOK: FillRect obscured " & obscuredText.Name)
                                RemoveEntity(obscuredText)
                            Next
                        End If

                    Case "DrawTextA", "DrawTextW"

                        Dim uFormat = Integer.Parse(details.Parameters("uFormat"))
                        Const DT_CALCRECT As Integer = 1024
                        If (uFormat And DT_CALCRECT) <> 0 Then
                            Exit Select
                        End If

                        Dim DC = FindDCByHandle(ParseHandle(details.Parameters("hDC")))
                        If DC Is Nothing Then
                            Throw New InvalidOperationException("Text written to unknown DC")
                        End If

                        Dim text = New clsUIText With {
                            .Text = details.Parameters("lpString"),
                            .Name = "Text: " & .Text,
                            .Parent = DC.Parent,
                            .DC = DC
                        }
                        DC.ChildText.Add(text)

                        Dim sr = details.Parameters("lpRect")
                        Dim srs() = sr.Split(";"c)
                        If srs.Length <> 4 Then
                            Throw New InvalidOperationException("Invalid lpRect rectangle format")
                        End If
                        Dim x1 = Integer.Parse(srs(0))
                        Dim y1 = Integer.Parse(srs(1))
                        Dim x2 = Integer.Parse(srs(2))
                        Dim y2 = Integer.Parse(srs(3))
                        text.X = x1
                        text.Y = y1
                        text.Width = x2 - x1
                        text.Height = y2 - y1

                        'If the DC refers to the client area of the window, then we need
                        'to adjust the text coordinates accordingly...
                        If DC.ClientDC Then
                            Dim offset = DC.Parent.GetClientOffset()
                            text.X += offset.X
                            text.Y += offset.Y
                        End If

                        For Each obscuredText In FindTextObscuredByText(DC, text).ToList()
                            clsConfig.LogHook("HOOK: DrawText obscured " & obscuredText.Name)
                            RemoveEntity(obscuredText)
                        Next

                        If Not text.Parent Is Nothing Then
                            text.Parent.AddChild(text)
                        End If
                        mAllEntities.Add(text)

                    Case "TextOutA", "ExtTextOutW"

                        If details.APIFunction = "ExtTextOutW" Then
                            'According to some documentation, the 'IGNORELANGUAGE' flag is reserved
                            'for system usage. From observation, the text output by it is garbage, and
                            'even does things like obscuring real output with empty strings, or spaces,
                            'for some reason!?
                            'For GLYPH_INDEX, there's no way to get useful text.
                            'So with either of these flags, we just ignore the API call.
                            Const ETO_GLYPH_INDEX = 16
                            Const ETO_IGNORELANGUAGE = 4096
                            Dim fuOptions = Integer.Parse(details.Parameters("fuOptions"))
                            If (fuOptions And (ETO_IGNORELANGUAGE Or ETO_GLYPH_INDEX)) <> 0 Then
                                Throw New InvalidOperationException("Skipping ExtTextOutW with unwanted options")
                            End If
                        End If

                        Dim DC = FindDCByHandle(ParseHandle(details.Parameters("hDC")))
                        If DC Is Nothing Then
                            Throw New InvalidOperationException("Text written to unknown DC")
                        End If

                        Dim text = New clsUIText With {
                            .Text = details.Parameters("lpString"),
                            .X = CInt(details.Parameters("nXStart")),
                            .Y = CInt(details.Parameters("nYStart")),
                            .Width = CInt(details.Parameters("nWidth")),
                            .Height = CInt(details.Parameters("nHeight")),
                            .Name = String.Format("Text: {0}", .Text),
                            .Parent = DC.Parent,
                            .DC = DC
                        }
                        DC.ChildText.Add(text)

                        'Alignment is only adjusted for ExtTextOutW and TextOutA
                        DC.AdjustAlignment(text, CInt(details.Parameters("tmDescent")))

                        'If the DC refers to the client area of the window, then we need
                        'to adjust the text coordinates accordingly...
                        If DC.ClientDC Then
                            Dim offset = DC.Parent.GetClientOffset()
                            text.X += offset.X
                            text.Y += offset.Y
                        End If

                        For Each obscuredText In FindTextObscuredByText(DC, text).ToList()
                            clsConfig.LogHook("HOOK: TextOut obscured " & obscuredText.Name)
                            RemoveEntity(obscuredText)
                        Next

                        If Not text.Parent Is Nothing Then
                            text.Parent.AddChild(text)
                        End If
                        mAllEntities.Add(text)

                End Select
            End SyncLock
        End If

        RaiseEvent CommsLineProcessed(details.SourceLine)

    End Sub

    ''' <summary>
    ''' Retrieve an event from the trigger queue. The event, if it exists, is removed
    ''' from the queue.
    ''' </summary>
    ''' <param name="hwnd">The handle of the source window.</param>
    ''' <param name="eventtype">The event type</param>
    ''' <returns>True if the event was found and removed from the queue. False if
    ''' there was no matching event in the queue.</returns>
    Friend Function GetEvent(ByVal hwnd As IntPtr, ByVal eventtype As String) As Boolean
        SyncLock mTriggerQueue
            For Each e As clsTriggerEvent In mTriggerQueue
                If e.miHandle = hwnd AndAlso e.mEventType.ToString() = eventtype Then
                    mTriggerQueue.Remove(e)
                    Return True
                End If
            Next
            Return False
        End SyncLock
    End Function


    Private Function ParseHandle(ByVal sHandle As String) As IntPtr
        Return IntPtrConvertor.PtrConvert(Int64.Parse(sHandle, Globalization.NumberStyles.HexNumber))
    End Function
    Private Function ParseHex(ByVal sHandle As String) As Long
        Return Int64.Parse(sHandle, Globalization.NumberStyles.HexNumber)
    End Function



    ''' <summary>
    ''' Find all text items (drawn text) within the given rectangle on a given window
    ''' </summary>
    ''' <param name="w">The clsUIWindow to look at</param>
    ''' <param name="x1">The X co-ordinate of the top left of the required region.
    ''' </param>
    ''' <param name="y1">The Y co-ordinate of the top left of the required region.
    ''' </param>
    ''' <param name="x2">The X co-ordinate of the bottom right of the required region
    ''' </param>
    ''' <param name="y2">The Y co-ordinate of the bottom right of the required region
    ''' </param>
    ''' <returns>An collection of strings found at the given region</returns>
    Public Function FindTextByRect(ByVal w As clsUIWindow, ByVal x1 As Integer, ByVal y1 As Integer, ByVal x2 As Integer, ByVal y2 As Integer) As IEnumerable(Of clsUIText)
        Return FindTextByRect(w, New Rectangle(x1, y1, x2 - x1, y2 - y1))
    End Function

    ''' <summary>
    ''' Find all text items (drawn text) within the given rectangle on a given window
    ''' </summary>
    ''' <param name="w">The clsUIWindow to look at</param>
    ''' <param name="r">The rectangle within which to look on the specified window</param>
    ''' <returns>An collection of clsUIText elements</returns>
    Public Function FindTextByRect(ByVal w As clsUIWindow, ByVal r As Rectangle) As IEnumerable(Of clsUIText)
        SyncLock mAllEntities
            Return mAllEntities _
                .OfType(Of clsUIText) _
                .Where(Function(x) x.Parent Is w AndAlso r.Contains(x.Location))
        End SyncLock
    End Function

    ''' <summary>
    '''Find all text items (drawn text) within the given rectangle on a given window or any of the windows
    '''descendants based on the center co-ordinates of the text.
    ''' </summary>
    ''' <param name="window">The clsUIWindow to look at</param>
    ''' <param name="rect">The rectangle within which to look on the specified window</param>
    ''' <returns>An collection of clsUIText elements</returns>
    Public Function FindTextByRectCenter(window As clsUIWindow, rect As Rectangle) As IEnumerable(Of clsUIText)
        SyncLock mAllEntities
            Return mAllEntities _
                .OfType(Of clsUIText) _
                .Where(Function(x) x.Parent Is window AndAlso rect.Contains(x.Center))
        End SyncLock
    End Function

    ''' <summary>
    ''' Returns true if the given entity is a descendant of the given window.
    ''' </summary>
    ''' <param name="entity">The entity to check</param>
    ''' <param name="window">The window to confirm the entity is a descendant of</param>
    ''' <returns>True if the entity is a descendant</returns>
    Private Function IsDescendantOf(entity As clsUIEntity, window As clsUIWindow) As Boolean
        Return entity.Parent IsNot Nothing AndAlso (entity.Parent Is window OrElse IsDescendantOf(entity.Parent, window))
    End Function

    ''' <summary>
    ''' Get the handle of the application's main window. (In fact, this just gets the
    ''' first window it can find thats not the desktop and whos parent is the
    ''' desktop, and is visible)
    ''' </summary>
    ''' <returns>A window handle, or 0 if none could be found</returns>
    Public Function GetMainWindowHandle() As IntPtr
        Return Me.GetMainWindow.Handle
    End Function

    Public Function GetMainWindow() As clsUIWindow
        For Each e As clsUIEntity In mAllEntities
            If TypeOf e Is clsUIWindow Then
                Dim w As clsUIWindow = CType(e, clsUIWindow)
                Dim p As clsUIWindow = CType(w.Parent, clsUIWindow)
                If w.Handle <> IntPtr.Zero AndAlso p IsNot Nothing AndAlso p.Handle = IntPtr.Zero AndAlso w.Visible = "True" Then
                    Return CType(e, clsUIWindow)
                End If
            End If
        Next
        Throw New InvalidOperationException(My.Resources.FailedToGetMainWindowHandle)
    End Function

    Private Function FindWindowByHandleP(ByVal handle As Long) As clsUIWindow
        Return FindWindowByHandleP(IntPtrConvertor.PtrConvert(handle))
    End Function

    ''' <summary>
    ''' Finds the first window in the model with the specified handle.
    ''' </summary>
    ''' <param name="iHandle">The handle of interest.</param>
    ''' <returns>Returns the first matching window found in the model,
    ''' or a null reference if no such window is found.</returns>
    Private Function FindWindowByHandleP(ByVal iHandle As IntPtr) As clsUIWindow
        For Each e As clsUIEntity In mAllEntities
            If TypeOf e Is clsUIWindow Then
                If CType(e, clsUIWindow).Handle = iHandle Then
                    Return CType(e, clsUIWindow)
                End If
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the first window in the model with the specified handle.
    ''' </summary>
    ''' <param name="iHandle">The handle of interest.</param>
    ''' <returns>Returns the first matching window found in the model,
    ''' or a null reference if no such window is found.</returns>
    Public Function FindWindowByHandle(ByVal iHandle As IntPtr) As clsUIWindow
        SyncLock mAllEntities
            Return FindWindowByHandleP(iHandle)
        End SyncLock
    End Function

    Public Function FindWindowByHandle(ByVal handle As Long) As clsUIWindow
        Return FindWindowByHandleP(IntPtrConvertor.PtrConvert(handle))
    End Function


    ''' <summary>
    ''' Determines whether the target application contains a window having the
    ''' supplied window handle.
    ''' </summary>
    ''' <param name="iHandle">The handle of the window of interest.</param>
    ''' <returns>Returns True if the window is present in the application model,
    ''' False otherwise.</returns>
    ''' <remarks>Relevant only for win32 applications. Returns False for terminal
    ''' applications.</remarks>
    Public Function ContainsWindow(ByVal iHandle As Int32) As Boolean

        'If iHandle = 0 Then Return False

        Dim ew As clsUIWindow = Nothing
        ew = FindWindowByHandle(iHandle)
        Return Not (ew Is Nothing)
    End Function


    Private Function FindWindowByWindowText(ByVal sText As String) As clsUIWindow
        For Each e As clsUIEntity In mAllEntities
            If TypeOf e Is clsUIWindow Then
                If CType(e, clsUIWindow).WindowText = sText Then
                    Return CType(e, clsUIWindow)
                End If
            End If
        Next
        Return Nothing
    End Function

    Private Function FindDCByHandle(ByVal iHandle As IntPtr) As clsUIDC
        For Each e As clsUIEntity In mAllEntities
            If TypeOf e Is clsUIDC Then
                If CType(e, clsUIDC).Handle = iHandle Then
                    Return CType(e, clsUIDC)
                End If
            End If
        Next
        Return Nothing
    End Function


    ''' <summary>
    ''' Find all text items (drawn text) that are obscured by the given rectangle.
    ''' </summary>
    ''' <param name="DC">The device context</param>
    ''' <param name="x1"></param>
    ''' <param name="y1"></param>
    ''' <param name="x2"></param>
    ''' <param name="y2"></param>
    ''' <returns>An IEnumerable(Of clsUIText) containing the elements that are obscured.</returns>
    Public Function FindTextObscuredByRect(DC As clsUIDC, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As IEnumerable(Of clsUIText)

        Return mAllEntities _
            .OfType(Of clsUIText) _
            .Where(Function(t) t.DC Is DC AndAlso t.X >= x1 AndAlso t.X <= x2 AndAlso t.Y >= y1 AndAlso t.Y <= y2)

    End Function

    ''' <summary>
    ''' Find all text items (drawn text) that are obscured by the given new text item.
    ''' </summary>
    ''' <param name="DC">The device context</param>
    ''' <param name="t">The text item</param>
    ''' <returns>An IEnumerable(Of clsUIText) containing the elements that are obscured.</returns>
    Private Function FindTextObscuredByText(DC As clsUIDC, t As clsUIText) As IEnumerable(Of clsUIText)

        Return FindTextObscuredByRect(DC, t.X, t.Y, t.X + t.Width - 1, t.Y + t.Height - 1)

    End Function

    Private Sub RemoveEntity(ByVal e As clsUIEntity)

        'Remove the entity from the 'set of all entities'...
        mAllEntities.Remove(e)

        'Remove any links in the model, depending on what type of entity
        'this is...
        If Not e.Parent Is Nothing Then
            e.Parent.RemoveChild(e)
            e.Parent = Nothing
        End If
        Dim ew2 As clsUIWindow
        Dim et As clsUIText
        Dim ed As clsUIDC
        If TypeOf e Is clsUIText Then
            et = CType(e, clsUIText)
            If Not et.DC Is Nothing Then
                et.DC.RemoveChild(et)
                et.DC.ChildText.Remove(et)
            End If
        ElseIf TypeOf e Is clsUIDC Then
            ed = CType(e, clsUIDC)
            If Not ed.Parent Is Nothing Then
                ew2 = CType(ed.Parent, clsUIWindow)
                ew2.RemoveChild(e)
            End If
            For Each ex As clsUIEntity In ed.Children
                If TypeOf e Is clsUIText Then
                    CType(e, clsUIText).DC = Nothing
                Else
                    Throw New InvalidOperationException("Bad child for DC")
                End If
            Next

        End If

    End Sub


#Region "Active Accessibility Support"

    Private Const OBJID_SELF As Integer = 0

    ''' <summary>
    ''' Identify an Accessible Object in the application based on the Identifiers in the
    ''' given query. One and only one window must match the criteria. In the event of any
    ''' failure, an ApplicationException is thrown.
    ''' </summary>
    ''' <param name="objQuery">The query</param>
    ''' <returns>The unique clsAAException matching the query.</returns>
    ''' <exception cref="ApplicationException">Exception thrown if no matching elemenets
    ''' are found, or if a non-unique match exists (ie more than one
    ''' element matches the given query).</exception>
    Friend Function IdentifyAccessibleObject(ByVal objQuery As clsQuery) As clsAAElement

        'Get a list of matching elements. A maximum of 2 are required to determine that
        'our selection is not unique...
        Dim m As ICollection(Of clsAAElement) = IdentifyAccessibleObjects(objQuery, 2)

        If m.Count > 1 Then Throw New InvalidOperationException(
         My.Resources.MoreThanOneElementMatchedTheQueryTerms)

        'We only matched one AAelement - here it is...
        Return CollectionUtil.First(m)
    End Function

    ''' <summary>
    ''' Gets the accessible objects matching a supplied query.
    ''' </summary>
    ''' <param name="query">The query used to match elements.</param>
    ''' <param name="MaxResults">The maximum number of results desired.</param>
    ''' <returns>Returns a list of matching elements.</returns>
    ''' <exception cref="NoSuchAAElementException">If no AA elements were found which
    ''' matched the specified query.</exception>
    Friend Function IdentifyAccessibleObjects(ByVal query As clsQuery,
     Optional ByVal maxResults As Integer = 100) As ICollection(Of clsAAElement)

        ' Extract the match index from the query
        Dim matchIndex As Integer =
         BPUtil.IfNull(query.GetParameter(ParameterNames.MatchIndex), 0)

        ' If we need to match more than our max results to meet the matchIndex
        ' constraint, ensure that we're looking at least that far
        If matchIndex > maxResults Then maxResults = matchIndex

        ' Extract the match reverse flag from the query
        Dim matchReverse As Boolean =
         BPUtil.IfNull(query.GetParameter(ParameterNames.MatchReverse), False)

        Dim depthId As clsIdentifierMatchTarget =
         query.GetIdentifier(IdentifierTypes.aAncestorCount)

        'we set a default of 100 here and then get from the appSettings, the try catch just protects us from someone setting the value to a non integer.
        Dim maxDepth As Integer = 100
        Try
            Dim maxDepthSetting = CType(ConfigurationManager.AppSettings("AA.Mode.MaximumRecursion"), Integer)
            If maxDepthSetting > 0 Then
                maxDepth = maxDepthSetting
            End If
        Catch
        End Try

        If depthId IsNot Nothing Then
            Select Case depthId.ComparisonType
                Case ComparisonTypes.Equal, ComparisonTypes.LessThanOrEqual
                    Integer.TryParse(depthId.MatchValue, maxDepth)
                Case ComparisonTypes.LessThan
                    If Integer.TryParse(depthId.MatchValue, maxDepth) Then _
                     maxDepth -= 1
            End Select
        End If

        'Some identifiers for AA match the parent Window, and we then descend those using
        'the other identifiers to match the AA elements within. So, first, we get a list
        'of all the windows that match...

        ' Note that if we didn't just identify the window (doesn't exist - not unique is
        ' valid in this instance since we have AA attributes to constrain on) we
        ' will have already aborted by throwing an exception.
        Dim matches As New clsOrderedDictionary(Of RECT, clsAAElement)
        For Each w As clsUIWindow In IdentifyWindows(query, False, Integer.MaxValue)
            If w.Handle = IntPtr.Zero Then Continue For ' Do not descend the desktop window.

            Dim e As clsAAElement = clsAAElement.FromWindow(w.Handle)
            ' First check the window itself
            If MatchAccessibleObject(query, e) Then matches(e.ScreenBounds) = e

            Dim finished As Boolean = False
            If Not DescendAccessibleObjects(query, e, matches,
             matchIndex, matchReverse, 0, maxDepth) Then finished = True

            'Add the newly found elements, if they are not added already

            ' If we've hit our maximum, stop there
            If matches.Count >= maxResults Then Exit For

            ' If a matchindex is in place, stop if we have reached it
            If matchIndex > 0 AndAlso matches.Count >= matchIndex Then Exit For

            ' If we've stopped searching for any reason, exit the search loop
            If finished Then Exit For

        Next

        If matches.Count = 0 Then Throw New NoSuchAAElementException()

        Dim elemList As New List(Of clsAAElement)(matches.Values)

        If matchIndex > 0 Then
            ' If we didn't reach match index - that's a failure, right there
            If elemList.Count < matchIndex Then Throw New NoSuchAAElementException()

            Return GetSingleton.ICollection(Of clsAAElement)(elemList(matchIndex - 1))
        End If

        Return elemList
    End Function

    Private Function DescendAccessibleObjects(
     ByVal query As clsQuery, ByVal element As clsAAElement,
     ByVal matches As IDictionary(Of RECT, clsAAElement),
     ByVal matchIndex As Integer, ByVal matchReverse As Boolean,
     ByVal currentDepth As Integer, ByVal maxDepth As Integer) As Boolean

        Dim nextDepth As Integer = currentDepth + 1
        If nextDepth > maxDepth Then Return True

        Dim children As List(Of clsAAElement) = element.Elements
        If Not children.Count = 0 Then

            Dim start As Integer = CInt(IIf(matchReverse, children.Count - 1, 0))
            Dim finish As Integer = CInt(IIf(matchReverse, 0, children.Count - 1))
            Dim dir As Integer = CInt(IIf(matchReverse, -1, 1))

            For i As Integer = start To finish Step dir
                Dim child As clsAAElement = children(i)
                If Not child Is Nothing Then
                    If MatchAccessibleObject(query, child) Then
                        matches(child.ScreenBounds) = child
                        If matchIndex > 0 AndAlso matches.Count >= matchIndex Then
                            Return False
                        End If
                    End If

                    If Not DescendAccessibleObjects(query, child, matches,
                     matchIndex, matchReverse, nextDepth, maxDepth) Then
                        Return False
                    End If
                End If
            Next
        End If
        Return True
    End Function


    ''' <summary>
    ''' Determine if the given Active Accessibility element matches the identifiers in
    ''' the given query.
    ''' </summary>
    ''' <param name="q">The query to match against</param>
    ''' <param name="child">The element to check</param>
    ''' <returns>True if the element matches.</returns>
    Private Function MatchAccessibleObject(ByVal q As clsQuery, ByVal child As clsAAElement) As Boolean

        For Each constraint As clsIdentifierMatchTarget In q.Identifiers
            'Decide whether to match against the child or the parent
            'by inspecting the first letter of the parameter.
            'Here we are testing for the possibility of identifiers
            'such as pWidth
            Dim aaElem As clsAAElement = child

            Dim idType As IdentifierTypes = constraint.Identifier
            Dim id As String = idType.ToString()

            Dim pIdType As IdentifierTypes

            ' If the id is prefixed 'p', it could be a parent identifier
            ' The derived ID is attained by stripping the leading 'p' out.
            ' If that doesn't produce a valid ID, just use the raw ID
            If id.StartsWith("p") AndAlso
             clsEnum.TryParse(id.Right(id.Length - 1), True, pIdType) Then
                aaElem = child.Parent
                idType = pIdType
            End If

            If aaElem Is Nothing Then Return False

            Dim prop As PropertyInfo = Nothing

            ' Find the appropriate property on the comparison element
            ' by looking it up in the dictionary
            If clsAAElement.GetProperties().TryGetValue(idType, prop) AndAlso
             prop IsNot Nothing Then
                Dim val As String = CStr(prop.GetValue(aaElem, Nothing))
                If Not constraint.IsMatch(val) Then Return False
            Else
                'Ignore the identifer. This is a valid thing to do, because
                'AA identifiers are mixed together with win32 identifiers
            End If
        Next

        Return True

    End Function

#End Region


#Region "HTML DOM Support"

    ''' <summary>
    ''' Gets a unique HTML element matching the supplied query.
    ''' </summary>
    ''' <param name="query">The query specifying the HTML element of interest.</param>
    ''' <param name="docs">A list of documents in which to search for the element.</param>
    ''' <param name="allowRetry">If True, failure to match any elements at all will
    ''' result in repeated attempts at one second intervals, up to 10 times.</param>
    ''' <returns>Returns a unique HTML element match, or else throws an Exception.</returns>
    Friend Function GetHTMLElement(ByVal query As clsQuery,
     ByVal docs As List(Of clsHTMLDocument),
     Optional ByVal allowRetry As Boolean = True) As clsHTMLElement

        Dim elements As ICollection(Of clsHTMLElement) = GetHTMLElements(query, docs, 2)

        Dim attempts As Integer = 1

        ' Try another 10 times before giving up
        While elements.Count = 0 AndAlso allowRetry AndAlso attempts < 10

            ' Output explanatory message on first iteration
            If attempts = 1 Then clsConfig.LogRetry("Failed to find element " &
             "matching the supplied query terms - Retrying")

            ' Then wait a second, try again, bump the attempts number and log success
            Thread.Sleep(1000)
            elements = GetHTMLElements(query, docs, 2)
            attempts += 1

            If elements.Count = 1 Then _
             clsConfig.LogRetry("Retrying succeeded after " & attempts & " attempts")

        End While

        Select Case elements.Count
            Case 0
                Throw New NoSuchHTMLElementException()
            Case 1 : Return CollectionUtil.First(elements)
            Case Else
                Throw New LimitReachedException(
                 My.Resources.MoreThanOneElementMatchedTheQueryTerms)
        End Select

    End Function

    Friend mExplorerApplicationWindow As clsUIWindow

    ''' <summary>
    ''' Gets the window handle of any Internet Explorer window in the target
    ''' application.
    ''' </summary>
    ''' <returns>Returns the handle to the first such window found, or IntPtr.Zero if
    ''' none found.</returns>
    Public Function GetBrowserHandle() As IntPtr

        'Find the first Internet Explorer server in our application (we expect there to only be one)
        For Each e As clsUIEntity In mAllEntities
            If TypeOf (e) Is clsUIWindow Then
                Dim ew As clsUIWindow = CType(e, clsUIWindow)
                If ew.ClassName = "Internet Explorer_Server" Then
                    mExplorerApplicationWindow = ew
                    Return ew.Handle
                End If
            End If
        Next

        Return IntPtr.Zero
    End Function

    ''' <summary>
    ''' Matches HTML Document elements against a given query.
    ''' </summary>
    ''' <param name="query">The query to use when matching</param>
    ''' <param name="doc">The document containing the elements to
    ''' match</param>
    ''' <param name="elements">The list to store matches in</param>
    ''' <param name="matchIndex">The index at which to stop matching</param>
    ''' <param name="ignorePath">Boolean indicating whether to ignore the path
    ''' during the match.</param>
    ''' <param name="matchReverse">Boolean indicating whether to itterate through
    ''' document in reverse</param>
    ''' <param name="elementIDMatcher">If Set used to match elements against
    ''' ID</param>
    Private Sub MatchHTMLDocumentElements(ByVal query As clsQuery,
     ByVal doc As clsHTMLDocument,
     ByVal elements As List(Of clsHTMLElement),
     ByVal matchIndex As Integer,
     ByVal ignorePath As Boolean,
     ByVal matchReverse As Boolean,
     ByVal elementIDMatcher As clsIdentifierMatchTarget,
     ByVal maxMatchingItems As Integer?)

        Dim all As List(Of clsHTMLElement)

        If elementIDMatcher Is Nothing _
           OrElse String.IsNullOrEmpty(elementIDMatcher.MatchValue) _
           OrElse elementIDMatcher.ComparisonType <> ComparisonTypes.Equal Then
            all = doc.All
        Else
            all = doc.GetElementsByID(elementIDMatcher.MatchValue)
        End If

        Dim start As Integer, finish As Integer, dir As Integer
        GetLoopDirectionValues(matchReverse, all.Count, start, finish, dir)

        For index As Integer = start To finish Step dir
            Dim el As clsHTMLElement = all(index - 1)
            If VerifyHTMLElementMatch(query, el, ignorePath) Then
                elements.Add(el)
                If MaximumElementsReached(elements.Count, matchIndex, maxMatchingItems) Then Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' Matches HTML Frame elements against a given query.
    ''' </summary>
    ''' <param name="query">The query to use when matching</param>
    ''' <param name="objDocument">The document containing the elements to
    ''' match</param>
    ''' <param name="elements">The list to store matches in</param>
    ''' <param name="matchIndex">The index at which to stop matching</param>
    ''' <param name="ignorePath">Boolean indicating whether to ignore the path
    ''' during the match.</param>
    ''' <param name="matchReverse">Boolean indicating whether to itterate through
    ''' document in reverse</param>
    ''' <param name="elementIDMatcher">If Set used to match elements against
    ''' ID</param>
    Private Sub MatchHTMLFrameElements(ByVal query As clsQuery,
     ByVal objDocument As clsHTMLDocument,
     ByVal elements As List(Of clsHTMLElement),
     ByVal matchIndex As Integer,
     ByVal ignorePath As Boolean,
     ByVal matchReverse As Boolean,
     ByVal elementIDMatcher As clsIdentifierMatchTarget,
     ByVal excludeHTC As Boolean,
     ByVal maxMatchingItems As Integer?)

        'If we are not using paths we have no option but to
        'search evey frame and every element
        Dim all As List(Of clsHTMLDocumentFrame) = objDocument.FlatListOfFrames(excludeHTC)

        Dim start As Integer, finish As Integer, dir As Integer
        GetLoopDirectionValues(matchReverse, all.Count, start, finish, dir)

        For index As Integer = start To finish Step dir
            Dim frm As clsHTMLDocument = all(index - 1)
            MatchHTMLDocumentElements(query, frm, elements, matchIndex, ignorePath, matchReverse, elementIDMatcher, maxMatchingItems)
            If MaximumElementsReached(elements.Count, matchIndex, maxMatchingItems) Then Exit For
        Next
    End Sub

    ''' <summary>
    ''' Gets values for start finish and dir based on match direction
    ''' </summary>
    ''' <param name="matchReverse">Set true to match in reverse</param>
    ''' <param name="count">The total number of elements to itterate</param>
    ''' <param name="start">The start value</param>
    ''' <param name="finish">The finish value</param>
    ''' <param name="dir">The direction (step)</param>
    Private Sub GetLoopDirectionValues(ByVal matchReverse As Boolean, ByVal count As Integer, ByRef start As Integer, ByRef finish As Integer, ByRef dir As Integer)
        start = CInt(IIf(matchReverse, count, 1))
        finish = CInt(IIf(matchReverse, 1, count))
        dir = CInt(IIf(matchReverse, -1, 1))
    End Sub

    ''' <summary>
    ''' Gets all the HTML elements matching the supplied query.
    ''' </summary>
    ''' <param name="query">The query specifying the HTML element of interest.</param>
    ''' <param name="docs">A list of documents in which to search for the elements.</param>
    ''' <returns>A List of all the clsHTMLElement instances representing the elements
    ''' found.</returns>
    Friend Function GetHTMLElements(ByVal query As clsQuery, ByVal docs As List(Of clsHTMLDocument), ByVal maxMatchingItems As Integer?) As List(Of clsHTMLElement)

        If docs Is Nothing Then Throw New ArgumentNullException(NameOf(docs))

        Dim elements As New List(Of clsHTMLElement)
        Dim pathString As String = Nothing
        Dim pathMatch As clsIdentifierMatchTarget =
         query.GetIdentifier(IdentifierTypes.Path)

        If pathMatch IsNot Nothing Then pathString = pathMatch.MatchValue

        Dim useExactPath As Boolean =
         (pathString <> "" AndAlso pathMatch.ComparisonType = ComparisonTypes.Equal)

        'ignore path whenever none specified
        Dim ignorePath As Boolean = String.IsNullOrEmpty(pathString)

        Dim matchIndex As Integer =
         BPUtil.IfNull(query.GetParameter(ParameterNames.MatchIndex), 0)

        ' Extract the match reverse flag from the query
        Dim matchReverse As Boolean =
         BPUtil.IfNull(query.GetParameter(ParameterNames.MatchReverse), False)

        Dim start As Integer, finish As Integer, dir As Integer
        GetLoopDirectionValues(matchReverse, docs.Count, start, finish, dir)

        Dim elementIDMatcher As clsIdentifierMatchTarget =
         query.GetIdentifier(IdentifierTypes.ID)

        Dim excludeHTC As Boolean = mTargetApp.ExcludeHTC

        For index As Integer = start To finish Step dir
            Dim doc As clsHTMLDocument = docs(index - 1)
            If useExactPath Then
                If pathString.StartsWith("/") Then
                    GetHTMLElement(doc, elements, pathString, query, True, excludeHTC)
                Else
                    'Old style paths don't start with /. We have to search every frame
                    'recursively. These paths are no longer generated (and are much
                    'slower than the new ones) but we still support them for backwards
                    'compatiblility. (Except in 3.0.60, see bug #3817)
                    For Each f As clsHTMLDocumentFrame In doc.FlatListOfFrames(excludeHTC)
                        GetHTMLElement(f, elements, pathString, query, False, excludeHTC)
                        If MaximumElementsReached(elements.Count, matchIndex, maxMatchingItems) Then Exit For
                    Next

                    If Not MaximumElementsReached(elements.Count, matchIndex, maxMatchingItems) Then
                        GetHTMLElement(doc, elements, pathString, query, False, excludeHTC)
                    End If
                End If
            Else
                If matchReverse Then MatchHTMLDocumentElements(query, doc, elements, matchIndex, ignorePath, matchReverse, elementIDMatcher, maxMatchingItems)

                MatchHTMLFrameElements(query, doc, elements, matchIndex, ignorePath, matchReverse, elementIDMatcher, excludeHTC, maxMatchingItems)

                If Not matchReverse Then MatchHTMLDocumentElements(query, doc, elements, matchIndex, ignorePath, matchReverse, elementIDMatcher, maxMatchingItems)
            End If

            If MaximumElementsReached(elements.Count, matchIndex, maxMatchingItems) Then Exit For
        Next

        If matchIndex > 0 Then
            If elements.Count >= matchIndex Then
                Dim matches As New List(Of clsHTMLElement)
                matches.Add(elements(matchIndex - 1))
                Return matches
            End If
            ' Otherwise, we didn't reach 'matchIndex' elements, effectively we
            ' have not matched the query - clear any gathered elements.
            elements.Clear()
        End If

        Return elements
    End Function

    Private Function MaximumElementsReached(elementsCount As Integer, matchIndex As Integer, maxMatchingItems As Integer?) As Boolean
        Return (matchIndex > 0 AndAlso elementsCount >= matchIndex) OrElse (matchIndex = 0 AndAlso maxMatchingItems.HasValue AndAlso elementsCount >= maxMatchingItems.Value)
    End Function

    ''' <summary>
    ''' Gets HTML Elements by using the full path string.
    ''' </summary>
    ''' <param name="doc">The document in which to find the element</param>
    ''' <param name="elements">A list of elements found</param>
    ''' <param name="path">The path used to find the element.</param>
    ''' <param name="q">The query to check the element against once found</param>
    ''' <param name="isFullPath">Whether to use a fullpath or relative path</param>
    Private Sub GetHTMLElement(ByVal doc As clsHTMLDocument,
     ByRef elements As List(Of clsHTMLElement), ByVal path As String,
     ByVal q As clsQuery, ByVal isFullPath As Boolean, ByVal excludeHTC As Boolean)

        Dim el As clsHTMLElement = Nothing

        ' If we don't have a document, I don't see how we could find anything
        If doc Is Nothing Then Return

        For Each p As clsPathPart In ParsePath(path)
            Select Case p.tagname
                Case "HTML"
                    If isFullPath AndAlso el IsNot Nothing Then
                        For Each f As clsHTMLDocumentFrame In doc.ChildFrames(excludeHTC)
                            If f.FrameElement.ElementEquals(el) Then
                                doc = f
                                Exit For
                            End If
                        Next
                    End If
                    el = doc.Root

                Case "BODY"
                    el = doc.Body

                Case "BASE"
                    Try
                        Dim base As clsHTMLElement = el.GetChild(p.index, p.tagname)
                        If base IsNot Nothing AndAlso base.Children.Count > 0 Then
                            el = base
                        End If
                    Catch
                        'Do Nothing
                    End Try

                Case Else
                    'Where the "root" element is a frameset (in the place of an html BODY element), there
                    'seems to be a disjoin between the frameset element and the element corresponding to
                    'the parent HTML tag. This disjoin does not exist where the FRAMESET element is nested
                    'inside something other than HTML (eg DIV instead). The following is a workaround for
                    'the specific case described. Tested in IE6 and IE7. See bug 4014
                    If p.tagname = "FRAMESET" AndAlso el IsNot Nothing AndAlso el.HasNoParent Then
                        el = doc.Body
                    Else
                        el = el.GetChild(p.index, p.tagname)
                    End If
                    If el Is Nothing Then Exit Sub

            End Select
        Next

        ' we can ignore path since we have just matched against the path already
        If VerifyHTMLElementMatch(q, el, True) Then
            elements.Add(el)
        End If

    End Sub

    ''' <summary>
    ''' Simple helper class for paths.
    ''' </summary>
    ''' <remarks></remarks>
    Private Class clsPathPart
        Public tagname As String
        Public index As Integer
    End Class

    ''' <summary>
    ''' Parse a HTML path and return a list of path parts.
    ''' </summary>
    ''' <param name="sPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ParsePath(ByVal sPath As String) As List(Of clsPathPart)
        Try
            Dim pathparts As New List(Of clsPathPart)

            Dim parts() As String = sPath.Split("/"c)
            For Each s As String In parts
                If Not s = String.Empty Then
                    Dim start As Integer = s.IndexOf("("c)
                    Dim finish As Integer = s.IndexOf(")"c)

                    Dim p As New clsPathPart
                    If start = -1 OrElse finish = -1 Then
                        p.tagname = s
                        p.index = 0
                    Else
                        p.tagname = s.Substring(0, start)
                        start += 1
                        Dim indexval As String = s.Substring(start, finish - start)
                        p.index = Integer.Parse(indexval) - 1
                    End If

                    pathparts.Add(p)
                End If
            Next

            Return pathparts

        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.FailedToParseThePath)
        End Try
    End Function

    ''' <summary>
    ''' Determines whether a candidate html element matches the identifiers set
    ''' out in the supplied query.
    ''' </summary>
    ''' <param name="objQuery">The query containing the attributes to match</param>
    ''' <param name="e">The element to be checked.</param>
    ''' <param name="IgnorePath">An ugly method of optimisation (because path matching
    ''' is slow). When true, the path attribute will be ignored even if the query object
    ''' specifies a 'path' identifier.</param>
    ''' <returns>True if it does match</returns>
    Public Function VerifyHTMLElementMatch(ByVal objQuery As clsQuery, ByVal e As clsHTMLElement, Optional ByVal IgnorePath As Boolean = False) As Boolean
        Dim bMatch As Boolean = True

        For Each IdentifierMatcher As clsIdentifierMatchTarget In objQuery.Identifiers
            Dim ComparisonElement As clsHTMLElement = e

            'OPTIMISTATION - PATHS ARE SLOW, THEREFORE AVOID WHEN PERMITTED BY CALLEE
            If Not (IgnorePath AndAlso IdentifierMatcher.Identifier = clsQuery.IdentifierTypes.Path) Then

                'Find the appropriate property on the comparison element by looking it up in the dictionary
                Dim IdentifierProperties As Dictionary(Of clsQuery.IdentifierTypes, PropertyInfo) = clsHTMLElement.GetProperties

                Dim MatchingProperty As PropertyInfo = Nothing
                IdentifierProperties.TryGetValue(IdentifierMatcher.Identifier, MatchingProperty)
                If MatchingProperty IsNot Nothing Then
                    If Not IdentifierMatcher.IsMatch(CStr(MatchingProperty.GetValue(ComparisonElement, Nothing))) Then
                        Return False
                    End If
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.x0IsNotAValidHTMLIdentifier, IdentifierMatcher.Identifier.ToString))
                End If

            End If
        Next


        Return True
    End Function
#End Region


    ''' <summary>
    ''' Determines whether the supplied context matches the supplied query terms.
    ''' </summary>
    ''' <param name="jc">The context to be tested.</param>
    ''' <param name="objQuery">The query against which the supplied context is to be
    ''' tested.</param>
    ''' <returns>Returns True if the context matches the supplied query, False
    ''' otherwise.</returns>
    Private Function CheckJABContextIsMatch(ByVal jc As JABContext, ByVal objQuery As clsQuery) As Boolean

        clsConfig.LogJAB("Checking for match on JAB context " & jc.UniqueID.ToString("X8") & " name:" & jc.Name)

        Dim identifierProperties As Dictionary(Of clsQuery.IdentifierTypes, PropertyInfo) = JABContext.GetProperties
        For Each identifierMatcher As clsIdentifierMatchTarget In objQuery.Identifiers
            Dim matchingProperty As PropertyInfo = Nothing
            identifierProperties.TryGetValue(identifierMatcher.Identifier, matchingProperty)
            If matchingProperty IsNot Nothing Then
                clsConfig.LogJAB(" - checking property " & matchingProperty.Name)
                If Not identifierMatcher.IsMatch(CStr(matchingProperty.GetValue(jc, Nothing))) Then
                    clsConfig.LogJAB(" - did not match")
                    Return False
                End If
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.x0IsNotAValidJavaIdentifier, identifierMatcher.Identifier.ToString))
            End If
        Next

        clsConfig.LogJAB("Matched " & jc.Name)
        Return True

    End Function

    ''' <summary>
    ''' Performs recursive search against the supplied context and all its children,
    ''' for a match against the supplied query terms.
    ''' </summary>
    ''' <param name="jc">The context to be tested. All descendents will also be
    ''' tested.</param>
    ''' <param name="query">The query against which the contexts are to be tested.
    ''' </param>
    ''' <param name="maxItems">The maximum number of items to be matched; once this
    ''' number of matches has been found, the method will exit even if it has not
    ''' yet finished inspecting the entire tree.</param>
    ''' <param name="matchReverse">When True, the tree will be traversed in reverse
    ''' order.</param>
    ''' <param name="foundItems">Carries back a list of matching items. Must never be
    ''' Nothing.</param>
    Private Sub SearchJABContext(
     ByVal jc As JABContext,
     ByVal query As clsQuery,
     ByVal maxItems As Integer,
     ByVal matchReverse As Boolean,
     ByVal foundItems As Dictionary(Of Long, JABContext))

        ' If the query includes the recursive depth in the tree to look for, ensure we don't perform
        ' any more work than is necessary when matching it by skipping any elements
        ' which are further away from the root node than the (maximum) desired depth.
        Dim depthId As clsIdentifierMatchTarget =
         query.GetIdentifier(IdentifierTypes.AncestorCount)

        Dim maxDepth As Integer = Integer.MaxValue

        If depthId IsNot Nothing Then
            Select Case depthId.ComparisonType

                Case ComparisonTypes.Equal, ComparisonTypes.LessThanOrEqual
                    If Not Integer.TryParse(depthId.MatchValue, maxDepth) Then _
                     maxDepth = Integer.MaxValue

                Case ComparisonTypes.LessThan
                    If Integer.TryParse(depthId.MatchValue, maxDepth) _
                     Then maxDepth -= 1 _
                     Else maxDepth = Integer.MaxValue

            End Select
        End If

        SearchJABContext(jc, 0, query, maxItems, maxDepth, matchReverse, foundItems)

    End Sub


    ''' <summary>
    ''' Performs recursive search against the supplied context and all its children,
    ''' for a match against the supplied query terms.
    ''' </summary>
    ''' <param name="jc">The context to be tested. All descendents will also be
    ''' tested.</param>
    ''' <param name="currentDepth">The current depth that the given context is at
    ''' from the root node. 0 indicates that <paramref name="jc"/> is the root node.
    ''' </param>
    ''' <param name="query">The query against which the contexts are to be tested.
    ''' </param>
    ''' <param name="maxDepth">The maximum depth to search. If the maximum depth
    ''' to search has been reached, then this will not search any deeper, but will
    ''' pop up through the stack to allow wider searches to continue.</param>
    ''' <param name="maxItems">The maximum number of items to be matched; once this
    ''' number of matches has been found, the method will exit even if it has not
    ''' yet finished inspecting the entire tree.</param>
    ''' <param name="matchReverse">When True, the tree will be traversed in reverse
    ''' order.</param>
    ''' <param name="foundItems">Carries back a list of matching items. Must never be
    ''' Nothing.</param>
    Private Sub SearchJABContext(
     ByVal jc As JABContext,
     ByVal currentDepth As Integer,
     ByVal query As clsQuery,
     ByVal maxItems As Integer,
     ByVal maxDepth As Integer,
     ByVal matchReverse As Boolean,
     ByVal foundItems As Dictionary(Of Long, JABContext))

        If foundItems.Count >= maxItems Then Exit Sub

        If CheckJABContextIsMatch(jc, query) Then
            If Not foundItems.ContainsKey(jc.UniqueID) Then
                foundItems.Add(jc.UniqueID, jc)
                If foundItems.Count = maxItems Then Exit Sub
            End If
        End If

        If mTargetApp.OptionSet("ignorenotshowing") Then
            If Not jc.Showing Then Return
        End If

        ' Calculate the next depth - if it's beyond the maximum, don't bother
        ' searching the children
        Dim nextDepth As Integer = currentDepth + 1
        If nextDepth > maxDepth Then Exit Sub

        Dim cc As List(Of JABContext) = jc.Children
        Dim childnum As Integer = 1

        Dim start As Integer = CInt(IIf(matchReverse, cc.Count - 1, 0))
        Dim finish As Integer = CInt(IIf(matchReverse, 0, cc.Count - 1))
        Dim dir As Integer = CInt(IIf(matchReverse, -1, 1))

        For i As Integer = start To finish Step dir
            Dim jcChild As JABContext = cc.Item(i)
            clsConfig.LogJAB("Searching child #" & childnum.ToString())
            childnum += 1
            SearchJABContext(jcChild, nextDepth, query, maxItems, maxDepth, matchReverse, foundItems)
            If foundItems.Count >= maxItems Then Exit Sub
        Next

    End Sub


    ''' <summary>
    ''' Identify a JAB element in the application based on the Parameters in the
    ''' givenquery. One and only one element must match the criteria. In the event of
    ''' any failure, an ApplicationException is thrown.
    ''' </summary>
    ''' <param name="q">The query for which a JAB object is required</param>
    ''' <param name="jab">The JAB wrapper on which to retrieve the objects</param>
    ''' <returns>The unique JABContext matching the query</returns>
    ''' <exception cref="NoSuchJavaElementException">If no matching java elements are
    ''' found</exception>
    ''' <exception cref="LimitReachedException">If a non-unique match exists (i.e.
    ''' more than one window matches the given query).</exception>
    Friend Function GetJABObject(ByVal q As clsQuery, ByVal jab As JABWrapper) _
     As JABContext
        Dim matches As ICollection(Of JABContext) = GetJABObjects(q, jab, 2)

        If matches.Count = 1 Then
            Return CollectionUtil.First(matches)

        ElseIf matches.Count = 0 Then
            Throw New NoSuchJavaElementException()

        Else
            For Each child As JABContext In matches
                child.Dispose()
            Next
            Throw New LimitReachedException(
             My.Resources.MoreThanOneJavaElementMatchesTheQuery)

        End If
    End Function


    ''' <summary>
    ''' Gets all JAB objects matching the supplied query, subject to the specified
    ''' maximum number of matches.
    ''' </summary>
    ''' <param name="q">The query determining the objects of interest.</param>
    ''' <param name="jab">The JABWrapper in use.</param>
    ''' <param name="max">The maximum permissible number of matches. Once this
    ''' figure is reached, the search will stop and no further matches will be
    ''' returned.</param>
    ''' <returns>A collection of matching objects.</returns>
    Friend Function GetJABObjects(ByVal q As clsQuery, ByVal jab As JABWrapper,
     Optional ByVal max As Integer = 100) As ICollection(Of JABContext)

        Dim items As New Dictionary(Of Long, JABContext)

        clsConfig.LogJAB("Looking for up to " & max & " JAB objects matching query")

        Dim matchIndex As Integer =
         BPUtil.IfNull(q.GetParameter(ParameterNames.MatchIndex), 0)

        ' Extract the match reverse flag from the query
        Dim reverse As Boolean =
         BPUtil.IfNull(q.GetParameter(ParameterNames.MatchReverse), False)


        SyncLock mAllEntities

            Dim w32Start As Integer = CInt(IIf(reverse, mAllEntities.Count - 1, 0))
            Dim w32Finish As Integer = CInt(IIf(reverse, 0, mAllEntities.Count - 1))
            Dim w32Dir As Integer = CInt(IIf(reverse, -1, 1))

            ' gatherLimit ensures that we can reach the match index, even if we have to
            ' go beyond the maximum number of windows to do so.
            Dim gatherLimit As Integer = Math.Max(max, matchIndex)

            For i As Integer = w32Start To w32Finish Step w32Dir

                Dim ew As clsUIWindow = TryCast(mAllEntities(i), clsUIWindow)
                If ew Is Nothing Then Continue For

                Dim jc As JABContext = jab.GetContextFromWindow(ew.Handle)
                If jc Is Nothing Then Continue For

                clsConfig.LogJAB(
                 "Searching window " & ew.Handle.ToString("X8") & " - " & ew.ClassName)

                ' Only search from top-level windows - otherwise we end up counting
                ' the descendants twice
                If Not ew.IsTopLevel AndAlso jab.HasJavaAncestor(ew.Hwnd) Then _
                 Continue For

                If mTargetApp.OptionSet("descendtree") Then
                    SearchJABContext(jc, q, gatherLimit, reverse, items)

                Else
                    Dim allContexts As List(Of JABContext) = Nothing
                    Try
                        allContexts = GetAllVisibleJABContexts(jc)
                        Dim visStart As Integer =
                         CInt(IIf(reverse, allContexts.Count - 1, 0))
                        Dim visFinish As Integer =
                         CInt(IIf(reverse, 0, allContexts.Count - 1))
                        Dim visDir As Integer = CInt(IIf(reverse, -1, 1))

                        For j As Integer = visStart To visFinish Step visDir
                            Dim ctx As JABContext = allContexts(j)

                            If CheckJABContextIsMatch(ctx, q) Then
                                items.Add(ctx.UniqueID, ctx)
                                If items.Values.Count >= gatherLimit Then Exit For
                            End If
                        Next

                    Finally
                        If allContexts IsNot Nothing Then
                            For Each jcChild As JABContext In allContexts
                                If Not items.ContainsKey(jcChild.UniqueID) Then _
                                 jcChild.Dispose()
                            Next
                        End If
                    End Try
                End If

                If items.Values.Count >= gatherLimit Then Exit For
            Next
        End SyncLock

        Dim matches As New List(Of JABContext)
        For Each ctx As JABContext In items.Values
            'Loop through to see if we've already added it
            'Note that two ACs can refer to the same component - see bug 2998
            Dim alreadyContained As Boolean = False
            For Each containedContext As JABContext In matches
                If WAB.isSameObject(ctx.vmID, ctx.AC, containedContext.AC) Then
                    alreadyContained = True
                    Exit For
                End If
            Next

            If alreadyContained Then
                ctx.Dispose()
            Else
                matches.Add(ctx)
            End If
        Next

        If matchIndex > 0 Then
            If matches.Count >= matchIndex Then _
             Return GetSingleton.ICollection(Of JABContext)(matches(matchIndex - 1))
            ' Otherwise our matches didn't reach the match index - no such element
            Return GetEmpty.ICollection(Of JABContext)()

        End If

        ' If we have more matches than the 'max' constraint, reduce our collection
        ' down to the max value
        If matches.Count > max Then matches.RemoveRange(max, matches.Count - max)

        Return matches

    End Function


    ''' <summary>
    ''' Gets all visible JAB contexts, which are descended from the supplied
    ''' context.
    ''' </summary>
    ''' <param name="rootContext">The root context beneath which the search is to
    ''' take place.</param>
    ''' <returns>Returns a list of all JAB contexts found. This always includes
    ''' the root context supplied.</returns>
    Private Function GetAllVisibleJABContexts(ByVal rootContext As JABContext) As List(Of JABContext)

        Dim retval As New List(Of JABContext)
        retval.Add(rootContext)

        Dim children As List(Of Long) = WAB.getVisibleChildren(rootContext.vmID, rootContext.AC, 0)
        If children IsNot Nothing Then
            For Each ac As Long In children
                If ac > 0 Then
                    retval.Add(New JABContext(ac, rootContext.vmID))
                End If
            Next
        End If

        clsConfig.LogJAB("GetAllVisibleJABContexts found " & retval.Count.ToString())
        Return retval

    End Function

    ''' <summary>
    ''' As for IdentifyWindow, except that all matching windows are returned (up to
    ''' a maximum of 10).
    ''' </summary>
    ''' <param name="query">The query to use.</param>
    ''' <returns>Returns a list of matching windows.</returns>
    ''' <exception cref="ApplicationException">If no matching windows are found.
    ''' </exception>
    Friend Function IdentifyWindows(ByVal query As clsQuery) As ICollection(Of clsUIWindow)
        Return IdentifyWindows(query, True, 10)
    End Function

    ''' <summary>
    ''' As for IdentifyWindow, except that all matching windows are returned.
    ''' </summary>
    ''' <param name="query">The query to use.</param>
    ''' <param name="max">The maximum number of results to return</param>
    ''' <returns>Returns a list of matching windows.</returns>
    ''' <exception cref="ApplicationException">If no matching windows are found.
    ''' </exception>
    Friend Function IdentifyWindows(ByVal query As clsQuery, ByVal max As Integer) As ICollection(Of clsUIWindow)
        Return IdentifyWindows(query, True, max)
    End Function

    ''' <summary>
    ''' As for IdentifyWindow, except that all matching windows are returned.
    ''' </summary>
    ''' <param name="query">The query to use.</param>
    ''' <param name="limitToMatchIndex">True to return no more results than the
    ''' match index configured in the query - False to return all results, regardless
    ''' of any match index value.</param>
    ''' <param name="max">The maximum number of results to return</param>
    ''' <returns>Returns a list of matching windows.</returns>
    ''' <exception cref="MissingIdentifierException">If no identifiers were given to
    ''' match windows against</exception>
    ''' <exception cref="NoSuchWindowException">If no matching windows are found.
    ''' </exception>
    Friend Function IdentifyWindows(ByVal query As clsQuery, ByVal limitToMatchIndex As Boolean,
     ByVal max As Integer) As ICollection(Of clsUIWindow)
        Dim matches As New List(Of clsUIWindow)

        ' Extract the match index from the query
        Dim matchIndex As Integer =
         BPUtil.IfNull(query.GetParameter(ParameterNames.MatchIndex), 0)
        ' If we're not limiting to match index value, effectively we don't recognise
        ' that there is a match index value (for the purpose of this method at least).
        If Not limitToMatchIndex Then matchIndex = 0

        ' Extract the match reverse flag from the query
        Dim matchReverse As Boolean =
         BPUtil.IfNull(query.GetParameter(ParameterNames.MatchReverse), False)

        ' gatherLimit ensures that we can reach the match index, even if we have to
        ' go beyond the maximum number of windows to do so.
        Dim gatherLimit As Integer = Math.Max(max, matchIndex)

        SyncLock mAllEntities

            ' Generate the start, end and step values to use, depending on the direction
            ' we are matching in
            Dim start As Integer = CInt(IIf(matchReverse, mAllEntities.Count - 1, 0))
            Dim finish As Integer = CInt(IIf(matchReverse, 0, mAllEntities.Count - 1))
            Dim dir As Integer = CInt(IIf(matchReverse, -1, 1))

            For i As Integer = start To finish Step dir
                Dim ew As clsUIWindow = TryCast(mAllEntities(i), clsUIWindow)
                ' If it's not a window, skip it
                If ew Is Nothing Then Continue For

                'We assume a match for this window, and then try and find a window
                'identifier parameter that doesn't match...
                Dim isMatch As Boolean = True
                Dim checkedSomething As Boolean = False

                For Each match As clsIdentifierMatchTarget In query.Identifiers
                    'Decide whether to match against the child or the parent
                    'by inspecting the first letter of the parameter
                    Dim elem As clsUIWindow = ew
                    Dim idType As IdentifierTypes = match.Identifier
                    Dim id As String = match.Identifier.ToString()

                    Dim pIdType As IdentifierTypes

                    ' If the id is prefixed 'p', it could be a parent identifier
                    ' The derived ID is attained by stripping the leading 'p' out.
                    ' If that doesn't produce a valid ID, just use the raw ID
                    If id.StartsWith("p") AndAlso
                     clsEnum.TryParse(id.Right(id.Length - 1), True, pIdType) Then
                        elem = ew.Parent
                        idType = pIdType
                    End If

                    If elem Is Nothing Then
                        isMatch = False
                        Exit For
                    End If

                    Dim prop As PropertyInfo = Nothing
                    If clsUIWindow.GetProperties().TryGetValue(idType, prop) _
                     AndAlso prop IsNot Nothing Then
                        checkedSomething = True
                        If Not match.IsMatch(CStr(prop.GetValue(elem, Nothing))) Then
                            isMatch = False
                            Exit For
                        End If
                    End If

                Next

                If Not checkedSomething Then Throw New MissingIdentifierException()

                If isMatch Then
                    matches.Add(ew)
                    If matches.Count >= gatherLimit Then Exit For
                End If

                If matchIndex > 0 AndAlso matches.Count >= matchIndex Then Exit For
            Next
        End SyncLock

        If matches.Count = 0 Then Throw New NoSuchWindowException()

        If matchIndex > 0 Then
            If matches.Count >= matchIndex Then _
             Return GetSingleton.ICollection(Of clsUIWindow)(matches(matchIndex - 1))
            ' Otherwise we didn't get enough matches to reach the match index
            Throw New NoSuchWindowException()
        End If

        ' If we have more matches than the 'max' constraint, reduce our collection
        ' down to the max value
        If matches.Count > max Then matches.RemoveRange(max, matches.Count - max)

        Return matches

    End Function

    ''' <summary>
    ''' Identify a window in the application based on the Identifiers in the given
    ''' query. One and only one window must match the criteria. In the event of any
    ''' failure, an ApplicationException is thrown.
    ''' </summary>
    ''' <param name="objQuery">The query</param>
    ''' <returns>The unique clsUIWindow matching the query</returns>
    ''' <exception cref="ApplicationException">Exception thrown if no matching windows
    ''' are found, or if a non-unique match exists (ie more than one
    ''' window matches the given query).</exception>
    Public Function IdentifyWindow(ByVal objQuery As clsQuery) As clsUIWindow

        'Get a list of matching windows. A maximum of 2 are required to determine that
        'our selection is not unique...
        Dim m As ICollection(Of clsUIWindow) = IdentifyWindows(objQuery, 2)

        If m.Count > 1 Then
            Throw New InvalidOperationException(My.Resources.MoreThanOneWindowMatchedTheQueryTerms)
        End If

        'We only matched one window - here it is...
        Dim ew As clsUIWindow = CollectionUtil.First(m)

        'See if we should now modify the selection...
        Dim relationship As String = objQuery.GetParameter(ParameterNames.Relationship)
        If Not relationship Is Nothing Then
            Select Case relationship
                Case "parent"
                    ew = CType(ew.Parent, clsUIWindow)
                    If ew Is Nothing Then
                        Throw New InvalidOperationException(My.Resources.ParentRelationshipSpecifiedButWindowHasNoParent)
                    End If
                Case "topchild"
                    Dim h = ew.Handle
                    h = GetTopWindow(h)
                    ew = FindWindowByHandleP(h)
                    If ew Is Nothing Then
                        Throw New InvalidOperationException(My.Resources.TopchildRelationshipCouldNotBeDetermined)
                    End If
                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.InvalidRelationshipType0, relationship))
            End Select
        End If

        'Return the final result...
        Return ew
    End Function

    Private Function RectContainsPoint(ByVal rect As RECT, ByVal pt As POINTAPI) As Boolean
        If rect.Left <= pt.x AndAlso pt.x < rect.Right AndAlso rect.Top <= pt.y Then
            Return pt.y < rect.Bottom
        End If
        Return False
    End Function

    Private Function Rect1ContainsRect2(ByVal rect1 As RECT, ByVal rect2 As RECT) As Boolean
        If rect1.Left <= rect2.Left AndAlso rect2.Right <= rect1.Right AndAlso rect1.Top <= rect2.Top Then
            Return rect2.Bottom <= rect1.Bottom
        End If
        Return False
    End Function

    Friend Function FindContainedEntities(ByVal r As RECT) As RECT
        Dim minx As Integer = Integer.MaxValue
        Dim miny As Integer = Integer.MaxValue
        Dim maxx As Integer = Integer.MinValue
        Dim maxy As Integer = Integer.MinValue

        For Each en As clsUIEntity In mAllEntities
            If TypeOf en Is clsUIText Then
                Dim tx As clsUIText = CType(en, clsUIText)
                Dim tr As RECT = tx.GetBounds()

                If Rect1ContainsRect2(r, tr) Then
                    With tr
                        If .Left < minx Then minx = .Left
                        If .Top < miny Then miny = .Top

                        If .Right > maxx Then maxx = .Right
                        If .Bottom > maxy Then maxy = .Bottom
                    End With

                End If

            End If
        Next

        If maxx = Integer.MinValue Then
            maxx = 0
        End If
        If maxy = Integer.MinValue Then
            maxy = 0
        End If

        If minx = Integer.MaxValue Then
            minx = 0
        End If

        If miny = Integer.MaxValue Then
            miny = 0
        End If

        Dim r3 As RECT
        r3.Left = minx
        r3.Top = miny
        r3.Right = maxx
        r3.Bottom = maxy
        Return r3
    End Function

    Friend Function GetWindowBounds(ByVal wn As clsUIWindow) As RECT
        Dim bounds As RECT
        GetWindowRect(wn.Handle, bounds)
        Return bounds
    End Function

    Friend Function FindWindowFromPoint(ByVal pt As POINTAPI) As clsUIWindow

        Dim iHandle = WindowFromPoint(pt)
        If iHandle <> IntPtr.Zero Then
            For Each en As clsUIEntity In mAllEntities
                If TypeOf en Is clsUIWindow Then
                    Dim wn As clsUIWindow = CType(en, clsUIWindow)
                    If wn.Handle = iHandle Then
                        Return wn
                    End If
                End If
            Next
        End If
        Return Nothing

    End Function

    Friend Function FindTextAtPoint(ByVal pt As POINTAPI) As clsUIEntity
        For Each en As clsUIEntity In mAllEntities
            If TypeOf en Is clsUIText Then
                Dim tx As clsUIText = CType(en, clsUIText)

                Dim par As clsUIWindow = CType(tx.Parent, clsUIWindow)
                If par.Active = "True" Then             'this should be Parent.Visible
                    If tx.GetBounds().Contains(pt) Then '  RectContainsPoint(tx.GetBounds, pt) Then
                        Return tx
                    End If
                End If
            End If
        Next
        Return Nothing
    End Function

    'We need to keep a single instance of this so we can synchronise against it...
    Private Shared mBuildSync As New Object()
    Private Shared mobjBuildingModel As clsUIModel

    ''' <summary>
    ''' Child window enumeration callback used when building a snapshot model.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window found</param>
    ''' <param name="lParam">Not used</param>
    ''' <returns>True - always continuing the enumeration</returns>
    Private Shared Function EnumChildProc(ByVal hWnd As System.IntPtr, ByVal lParam As clsEnumWindowsInfo) As Boolean

        'For container apps check the child window belongs to our process (this might
        'not be the case if we're dealing with a parent window belonging to its host)
        If CBool(lParam.Tag) Then
            Dim pid As Int32
            GetWindowThreadProcessId(hWnd, pid)
            If pid <> lParam.TargetPID Then Return True
        End If

        Dim hParent As IntPtr
        hParent = GetParent(hWnd)
        Dim parent As clsUIWindow = mobjBuildingModel.FindWindowByHandleP(hParent.ToInt32())
        If parent Is Nothing Then
            'If this child window's parent is not already in the model, don't add the
            'child to the model either. See bug #4426 for the reasoning behind this
            'and evidence of the seemingly impossible scenario.
            Return True
        End If

        Dim w As New clsUIWindow()
        w.Handle = hWnd
        w.Name = "Window " & hWnd.ToString("X8") & " (Snapshot Window)"
        Dim sbClassName As New StringBuilder(128)
        RealGetWindowClass(hWnd, sbClassName, 128)
        w.ClassName = sbClassName.ToString()
        w.Parent = parent
        w.Ordinal = w.Parent.AddChild(w)
        w.Style = GetWindowLong(hWnd, GWL.GWL_STYLE)

        mobjBuildingModel.mAllEntities.Add(w)
        Return True
    End Function

    ''' <summary>
    ''' Window enumeration callback used when building a snapshot model.
    ''' </summary>
    ''' <param name="hWnd">The handle of the window found</param>
    ''' <param name="lParam">The PID of the process we're building a model of</param>
    ''' <returns>True - always continuing the enumeration</returns>
    Private Shared Function EnumProc(ByVal hWnd As System.IntPtr, ByVal lParam As clsEnumWindowsInfo) As Boolean

        'See if the window belongs to our target process, (or a host process for
        'container apps).
        Dim procid As Int32
        GetWindowThreadProcessId(hWnd, procid)
        If procid = lParam.TargetPID OrElse (CBool(lParam.Tag) = True AndAlso mHostPids.Contains(procid)) Then
            'Yes, so we've found a top level window belonging to our process - we will
            'add it to the model, and then add all its descendants:
            Dim w As New clsUIWindow()
            w.Handle = hWnd
            w.CtrlID = GetDlgCtrlID(hWnd)
            w.Name = "Window " & hWnd.ToString("X8") & " (Snapshot Window)"
            Dim sbClassName As New StringBuilder(128)
            RealGetWindowClass(hWnd, sbClassName, 128)
            w.ClassName = sbClassName.ToString()
            w.Parent = mobjBuildingModel.FindWindowByHandleP(0)
            w.Ordinal = w.Parent.AddChild(w)
            w.Style = GetWindowLong(hWnd, GWL.GWL_STYLE)
            mobjBuildingModel.mAllEntities.Add(w)

            'Enumerate all the children of this top level window. Note that this covers
            'all descendents, not just the next level down, as can be confirmed from the
            'Remarks section here:
            'http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowfunctions/enumchildwindows.asp
            EnumChildWindows(hWnd, AddressOf EnumChildProc, lParam)

        End If

        Return True
    End Function

    ''' <summary>
    ''' Create a model which is a snapshot of the current state of the process with
    ''' the given process ID.
    ''' </summary>
    ''' <param name="iPID">The PID of the process to be modeled.</param>
    ''' <param name="target">The target application that owns the model.</param>
    ''' <returns>Returns the model observed.</returns>
    Public Shared Function MakeSnapshot(ByVal iPID As Int32, ByVal target As clsLocalTargetApp) As clsUIModel

        SyncLock mBuildSync
            mobjBuildingModel = New clsUIModel(target)

            'Don't bother enumerating if the target process ID is 0, that must mean
            'it hasn't been launched yet.
            If iPID <> 0 Then
                mHostPids = New List(Of Integer)
                ' If container apps are available make note of any host processes
                If target.IsModernApp Then
                    For Each p In Process.GetProcessesByName("ApplicationFrameHost")
                        mHostPids.Add(p.Id)
                    Next
                End If

                Dim info As New clsEnumWindowsInfo
                info.TargetPID = iPID
                info.Tag = target.IsModernApp
                EnumWindows(AddressOf EnumProc, info)
            End If

            Return mobjBuildingModel
        End SyncLock
    End Function

#Region " UIAutomation "


#End Region

End Class
