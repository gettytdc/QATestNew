Imports System.Reflection
Imports System.Runtime.InteropServices
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports System.Drawing
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Public Class JABContext
    Implements IDisposable

    ''' <summary>
    ''' Holds references to JABContexts
    ''' </summary>
    Private Shared mInstances As New List(Of JABContext)

    ''' <summary>
    ''' Cleanup JABContexts still present in the shared instance list.
    ''' </summary>
    Public Shared Sub CleanUp()
        Try
            Debug.Print("Found {0} undisposed JABContext instances", mInstances.Count)
            'Make a copy of the list of instances so that when
            'we dispose them this local list is not modified
            Dim instances As New List(Of JABContext)(mInstances)
            For Each instance As JABContext In instances
                Try
                    instance.Dispose()
                Catch
                End Try
            Next
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Constructs a new JABContext
    ''' </summary>
    ''' <param name="ac">Identifier for this AccessibleContext</param>
    ''' <param name="vmID">The Java virtual machine (VM) to which this context belongs.</param>
    Public Sub New(ByVal ac As Long, ByVal vmID As Integer)
        Me.mAC = ac
        Me.mvmID = vmID
        mInstances.Add(Me)
    End Sub

    ''' <summary>
    ''' The Java virtual machine (VM) to which this context belongs.
    ''' </summary>
    Public ReadOnly Property vmID() As Integer
        Get
            Return mvmID
        End Get
    End Property
    Private mvmID As Int32

    ''' <summary>
    ''' Identifier for this AccessibilityContext
    ''' </summary>
    ''' <remarks>When this context is finished with, the AC should be released to
    ''' avoid memory leaks.</remarks>
    Public ReadOnly Property AC() As Long
        Get
            Return mAC
        End Get
    End Property
    Private mAC As Long

    ''' <summary>
    ''' Unique identifier for this AccessibilityContext - this is a combination of
    ''' the virtual machine ID and the AC's own ID.
    ''' </summary>
    Public ReadOnly Property UniqueID() As Long
        Get
            Return (CLng(vmID) << 32) Or AC
        End Get
    End Property

    ''' <summary>
    ''' True if mInfo is valid - i.e. if we have already retrieved the information.
    ''' </summary>
    Public mInfoValid As Boolean = False

    ''' <summary>
    ''' If mInfoValid is True, this contains the information retrieved via the
    ''' getAccessibleContextInfo call. It is retrieved internally on demand using
    ''' UpdateCachedInfo()
    ''' </summary>
    Private mInfo As WAB.AccessibleContextInfo

    ''' <summary>
    ''' The allowed actions for this element, or Nothing if we haven't retrieved
    ''' that information yet.
    ''' </summary>
    Public mAllowedActions As List(Of String) = Nothing

    ''' <summary>
    ''' The children of this element, or Nothing if we haven't retrieved that
    ''' information yet.
    ''' </summary>
    Private mChildren As List(Of JABContext) = Nothing

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.ChildCount, True)> _
    Public ReadOnly Property ChildCount() As Integer
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.childrenCount
        End Get
    End Property

    Public ReadOnly Property Children() As List(Of JABContext)
        Get
            If mChildren Is Nothing Then
                mChildren = GetChildren()
            End If
            Return mChildren
        End Get
    End Property

    ''' <summary>
    ''' The distance in pixels from the left of the screen (x-coordinate) of the
    ''' element, in screen coordinates
    ''' </summary>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.X, False)> _
    Public ReadOnly Property Left() As Integer
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.x
        End Get
    End Property

    ''' <summary>
    ''' The distance in pixels from the top of the screen (y-coordinate) of the
    ''' element, in screen coordinates
    ''' </summary>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Y, False)> _
    Public ReadOnly Property Top() As Integer
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.y
        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Width, True)> _
     Public ReadOnly Property Width() As Integer
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.width
        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Height, True)> _
    Public ReadOnly Property Height() As Integer
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.height
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the jabcontext, in screen coordinates
    ''' </summary>
    Public ReadOnly Property ScreenBounds() As System.Drawing.Rectangle
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return New Rectangle(mInfo.x, mInfo.y, mInfo.width, mInfo.height)
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the jabcontext, relative to its parent.
    ''' </summary>
    ''' <remarks>If no parent exists, then this value coincides with
    ''' the screen coordinates.</remarks>
    Public ReadOnly Property ClientBounds() As System.Drawing.Rectangle
        Get
            'Try to get parent location
            Dim ParentLocation As Point = Point.Empty
            Dim ParentAC As Long = WAB.getAccessibleParentFromContext(Me.vmID, Me.AC)
            If ParentAC <> 0 Then
                Using parentCtx As New JABContext(ParentAC, Me.vmID)
                    ParentLocation = New Point(parentCtx.Left, parentCtx.Top)
                End Using
            End If

            'When no parent is exists, then the screenbounds happen to be the
            'correct "local" bounds, so this is ok
            Return New Rectangle(mInfo.x - ParentLocation.X, mInfo.y - ParentLocation.Y, mInfo.width, mInfo.height)
        End Get
    End Property

    ''' <summary>
    ''' Gets the depth of recursion for this jabcontext from the root node (which
    ''' has an implied depth of zero).
    ''' This will return a maximum value of 9999, ie. elements of a recursive depth
    ''' of 10000 or more will not be precise
    ''' </summary>
    ''' <remarks>The 9999 limit is just a sanity check to ensure that we don't get
    ''' into an infinite loop looking for the root context</remarks>
    <JABContextIdentifier(clsQuery.IdentifierTypes.AncestorCount, False)> _
    Public ReadOnly Property RecursiveDepth() As Integer
        Get
            ' Recurse through the parents, pushing each one onto a stack until we
            ' reach a node without parents (ie. the root node).
            ' This craps out at 9999 ancestors, which is frankly silly anyway
            Dim s As New Stack(Of JABContext)
            Try
                Dim currentCtx As JABContext = Me
                While s.Count < 9999
                    Dim parentAc As Long = WAB.getAccessibleParentFromContext(vmID, currentCtx.AC)
                    If parentAc <= 0 Then Return s.Count ' Found the root
                    currentCtx = New JABContext(parentAc, vmID)
                    s.Push(currentCtx)
                End While
                ' If we get here then we've failed to find the root node in 9999
                ' ancestors. Assume that we cannot get the depth for this element.
                Return Integer.MaxValue

            Finally
                For Each jc As JABContext In s
                    Try
                        jc.Dispose()
                    Catch ' Ignore any errors - we want to ensure they are all disposed of.
                    End Try
                Next
            End Try

        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Ordinal, True)> _
     Public ReadOnly Property IndexInParent() As Integer
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.indexInParent
        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Role, True)> _
    Public ReadOnly Property Role() As String
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.role
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the AccessibleContext is an AccessibleText
    ''' </summary>
    Public ReadOnly Property hasAccessibleText() As Boolean
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.accessibleText
        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Description, True)> _
    Public ReadOnly Property Description() As String
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.description
        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Name, True)> _
    Public ReadOnly Property Name() As String
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return mInfo.name
        End Get
    End Property

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.VirtualName, False)> _
    Public ReadOnly Property VirtualName() As String
        Get
            If mVirtualName Is Nothing Then
                Dim sb As New StringBuilder(WAB.MaxJABStringSize)
                WAB.getVirtualAccessibleName(vmID, AC, sb, WAB.MaxJABStringSize)
                mVirtualName = sb.ToString()
            End If
            Return mVirtualName
        End Get
    End Property
    Private mVirtualName As String = Nothing

    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.KeyBindings, True)> _
    Public ReadOnly Property KeyBindings() As String
        Get
            Dim Bindings As New WAB.AccessibleKeysMethods.AccessibleKeyBindings
            If WAB.AccessibleKeysMethods.getAccessibleKeyBindings(Me.vmID, Me.AC, Bindings) Then
                Dim First As Boolean = True
                Dim RetVal As String = String.Empty
                If Bindings.keyBindingsCount > 0 Then
                    For i As Integer = 0 To Bindings.keyBindingsCount - 1
                        If First Then
                            First = False
                        Else
                            RetVal &= ", "
                        End If
                        RetVal &= GetKeyBindingString(Bindings.keyBindingInfo(i))
                    Next
                End If
                Return RetVal
            Else
                Return String.Empty
            End If
        End Get
    End Property

    Private Function GetKeyBindingString(ByVal KeyBinding As WAB.AccessibleKeysMethods.AccessibleKeyBindingInfo) As String
        Dim ModifierString As String = String.Empty
        If KeyBinding.modifiers <> 0 Then
            ModifierString &= KeyBinding.modifiers.ToString & " + "
        End If
        Return ModifierString & KeyBinding.character.ToString
    End Function


    Public Sub UpdateCachedInfo()
        mInfoValid = False

        'Skip this whole block if we're not logging, to avoid unnecessarily retrieving
        'the 'VirtualAccessibleName'.
        If clsConfig.JABLogging Then
            Try
                clsConfig.LogJAB("About to update cached info for " & UniqueID.ToString("X8") & " - vtext:" & VirtualName)
            Catch ex As Exception
                clsConfig.LogJAB("Exception getting accessible name for " & UniqueID.ToString("X8") & " - " & ex.Message)
            End Try
        End If

        If Not WAB.getAccessibleContextInfo(vmID, AC, mInfo) Then
            clsConfig.LogJAB("Failed to update cached info for " & UniqueID.ToString("X8"))
            Exit Sub
        End If
        mInfoValid = True
        clsConfig.LogJAB("Updated cached info for " & UniqueID.ToString("X8") & " name:" & Name & " role:" & Role & " description:" & Description)
    End Sub

    Public ReadOnly Property ScreenRect() As Rectangle
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Return New Rectangle(mInfo.x, mInfo.y, mInfo.width, mInfo.height)
        End Get
    End Property


    ''' <summary>
    ''' Gets the states.
    ''' </summary>
    ''' <value></value>
    ''' <returns>An array of states, or nothing if no such exist.</returns>
    Private ReadOnly Property States() As String()
        Get
            If Not mInfoValid Then
                UpdateCachedInfo()
            End If
            Dim LocalStates As String = mInfo.states_EN_US
            If Not String.IsNullOrEmpty(LocalStates) Then
                Return LocalStates.Split(","c)
            Else
                Return Nothing
            End If
        End Get
    End Property


    ''' <summary>
    ''' Determines whether the states held by this
    ''' context contains the specified string.
    ''' </summary>
    ''' <param name="State">The state string to check
    ''' for.</param>
    ''' <returns>True if the state is held, false otherwise.</returns>
    Public Function HasState(ByVal State As String) As Boolean
        Dim LocalStates As String() = Me.States

        If LocalStates IsNot Nothing Then
            For Each s As String In LocalStates
                If s.CompareTo(State) = 0 Then Return True
            Next
        End If

        Return False
    End Function

    ''' <summary>
    ''' Indicates a window is currently the active window. 
    ''' </summary>
    ''' <returns>Returns true if this context is Active.</returns>
    ''' <remarks> This includes windows, dialogs, frames, etc.
    ''' In addition, this state is used to indicate the currently
    ''' active child of a component such as a list, table, or tree.
    ''' For example, the active child of a list is the child that
    ''' is drawn with a rectangle around it.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Active, False)> _
     Public ReadOnly Property Active() As Boolean
        Get
            Return Me.HasState("active")
        End Get
    End Property

    ''' <summary>
    ''' Indicates that the object is armed.
    ''' </summary>
    ''' <remarks>This is usually used on buttons that have
    ''' been pressed but not yet released, and the mouse
    ''' pointer is still over the button.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Armed, False)> _
    Public ReadOnly Property Armed() As Boolean
        Get
            Return Me.HasState("armed")
        End Get
    End Property


    ''' <summary>
    ''' Indicates the current object is busy.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks> This is usually used on objects such as
    ''' progress bars, sliders, or scroll bars to indicate
    ''' they are in a state of transition.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Busy, False)> _
    Public ReadOnly Property Busy() As Boolean
        Get
            Return Me.HasState("busy")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is currently checked. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually used on objects such as
    ''' toggle buttons, radio buttons, and check boxes.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Checked, False)> _
    Public ReadOnly Property Checked() As Boolean
        Get
            Return Me.HasState("checked")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is collapsed.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually paired with the EXPANDABLE state
    ''' and is used on objects that provide progressive disclosure
    ''' such as trees.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Collapsed, False)> _
    Public ReadOnly Property Collapsed() As Boolean
        Get
            Return Me.HasState("collapsed")
        End Get
    End Property

    ''' <summary>
    ''' Indicates the user can change the contents of this object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks> This is usually used primarily for objects that
    ''' allow the user to enter text. Other objects, such as
    ''' scroll bars and sliders, are automatically editable if
    ''' they are enabled.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Editable, False)> _
    Public ReadOnly Property Editable() As Boolean
        Get
            Return Me.HasState("editable")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is enabled.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>The absence of this state from an object's state
    ''' set indicates this object is not enabled. An object that is
    ''' not enabled cannot be manipulated by the user.
    ''' 
    ''' In a graphical display, it is usually grayed out.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Enabled, False)> _
    Public ReadOnly Property Enabled() As Boolean
        Get
            Return Me.HasState("enabled")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object allows progressive disclosure of its children.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually used with hierarchical objects such
    ''' as trees and is often paired with the EXPANDED or
    ''' COLLAPSED states.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Expandable, False)> _
    Public ReadOnly Property Expandable() As Boolean
        Get
            Return Me.HasState("expandable")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is expanded.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually paired with the EXPANDABLE state and
    ''' is used on objects that provide progressive disclosure such
    ''' as trees.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Expanded, False)> _
    Public ReadOnly Property Expanded() As Boolean
        Get
            Return Me.HasState("expanded")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object can accept keyboard focus.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Being focusable means all events resulting
    ''' from typing on the keyboard will normally be passed
    ''' to it when it has focus.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Focusable, False)> _
    Public ReadOnly Property Focusable() As Boolean
        Get
            Return Me.HasState("focusable")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object currently has the keyboard focus.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Focused, False)> _
    Public ReadOnly Property Focused() As Boolean
        Get
            Return Me.HasState("focused")
        End Get
    End Property

    ''' <summary>
    ''' Indicates the orientation of this object is horizontal.
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually associated with objects such as
    ''' scrollbars, sliders, and progress bars.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Horizontal, True)> _
    Public ReadOnly Property Horizontal() As Boolean
        Get
            Return Me.HasState("horizontal")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is minimized and is represented only by an icon.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually only associated with frames and
    ''' internal frames.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Iconified, False)> _
    Public ReadOnly Property Iconified() As Boolean
        Get
            Return Me.HasState("iconified")
        End Get
    End Property

    ''' <summary>
    ''' Indicates something must be done with this object before the
    ''' user can interact with an object in a different window.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks> This is usually associated only with dialogs.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Modal, True)> _
    Public ReadOnly Property Modal() As Boolean
        Get
            Return Me.HasState("modal")
        End Get
    End Property


    ''' <summary>
    ''' Indicates this (text) object can contain multiple lines of text
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.MultipleLine, True)> _
    Public ReadOnly Property Multiple_Line() As Boolean
        Get
            Return Me.HasState("multiple line")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object allows more than one of its
    ''' children to be selected at the same time.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Multiselectable, False)> _
    Public ReadOnly Property Multiselectable() As Boolean
        Get
            Return Me.HasState("multiselectable")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object paints every pixel within its rectangular region.
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks> A non-opaque component paints only some of its pixels,
    ''' allowing the pixels underneath it to "show through". A component
    ''' that does not fully paint its pixels therefore provides a degree
    ''' of transparency.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Opaque, False)> _
    Public ReadOnly Property Opaque() As Boolean
        Get
            Return Me.HasState("opaque")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is currently pressed.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This is usually
    ''' associated with buttons and indicates the user has pressed
    ''' a mouse button while the pointer was over the button and
    ''' has not yet released the mouse button.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Pressed, False)> _
    Public ReadOnly Property Pressed() As Boolean
        Get
            Return Me.HasState("pressed")
        End Get
    End Property

    ''' <summary>
    ''' Indicates the size of this object is not fixed.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Resizable, False)> _
    Public ReadOnly Property Resizable() As Boolean
        Get
            Return Me.HasState("resizable")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is the child of an object that
    ''' allows its children to be selected, and that this
    ''' child is one of those children that can be selected.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Selectable, False)> _
    Public ReadOnly Property Selectable() As Boolean
        Get
            Return Me.HasState("selectable")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is the child of an object that allows
    ''' its children to be selected, and that this child is one of
    ''' those children that has been selected.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Selected, False)> _
    Public ReadOnly Property Selected() As Boolean
        Get
            Return Me.HasState("selected")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object, the object's parent, the object's parent's parent,
    ''' and so on, are all visible.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks> Note that this does not necessarily mean the
    ''' object is painted on the screen. It might be occluded by
    ''' some other showing object.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Showing, False)> _
    Public ReadOnly Property Showing() As Boolean
        Get
            Return Me.HasState("showing")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this (text) object can contain only a single line of text
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.SingleLine, True)> _
    Public ReadOnly Property Single_Line() As Boolean
        Get
            Return Me.HasState("single line")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this (text) object can contain only a single line of text
    ''' </summary>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.JavaText, True)> _
    Public ReadOnly Property Text() As String
        Get
            Dim sErr As String = Nothing
            Dim Value As String = Nothing
            Dim Success As Boolean = JABWrapper.GetText(Me, Value, sErr)

            If Success Then
                Return Value
            Else
                Return ""
            End If
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is transient. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>An assistive technology should not add a PropertyChange
    ''' listener to an object with transient state, as that object will
    ''' never generate any events. Transient objects are typically created
    ''' to answer Java Accessibility method queries, but otherwise do not
    ''' remain linked to the underlying object (for example, those objects
    ''' underneath lists, tables, and trees in Swing, where only one
    ''' actual UI Component does shared rendering duty for all of the
    ''' data objects underneath the actual list/table/tree elements).</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Transient, False)> _
    Public ReadOnly Property Transient() As Boolean
        Get
            Return Me.HasState("transient")
        End Get
    End Property

    ''' <summary>
    ''' Indicates the orientation of this object is vertical.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks> This is usually associated with objects such as
    ''' scrollbars, sliders, and progress bars.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Vertical, True)> _
    Public ReadOnly Property Vertical() As Boolean
        Get
            Return Me.HasState("vertical")
        End Get
    End Property

    ''' <summary>
    ''' Indicates this object is visible. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Note: this means that the object intends to be visible;
    ''' however, it may not in fact be showing on the screen because one
    ''' of the objects that this object is contained by is not visible.</remarks>
    <JABContextIdentifierAttribute(clsQuery.IdentifierTypes.Visible, False)> _
    Public ReadOnly Property Visible() As Boolean
        Get
            Return Me.HasState("visible")
        End Get
    End Property

    ''' <summary>
    ''' Gets the allowed actions as a comma-separated string of values.
    ''' </summary>
    <JABContextIdentifier(clsQuery.IdentifierTypes.AllowedActions, True)> _
    Public ReadOnly Property AllowedActions() As String
        Get
            If mAllowedActions Is Nothing Then
                mAllowedActions = GetActions()
            End If
            Return Collections.CollectionUtil.Join(mAllowedActions, ", ")
        End Get
    End Property

    ''' <summary>
    ''' Get a list of the possible actions that can be performed on this element.
    ''' </summary>
    ''' <returns>A List of Strings, each being the name of an action.</returns>
    Private Function GetActions() As List(Of String)

        'This call fails if AC is null.
        If AC = 0 Then
            Throw New InvalidOperationException(My.Resources.CanNotGetActionsForAnUndefinedJavaContext)
        End If

        Dim a As IntPtr
        Try
            Dim list As New List(Of String)

            Dim s As Integer = Marshal.SizeOf(GetType(WAB.AccessibleActions))
            a = Marshal.AllocHGlobal(s)
            Try
                If Not WAB.getAccessibleActions(vmID, AC, a) Then Return list 'ie return empty list
            Catch ex As Exception
                'Trapping marshalling errors here, for example
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetAccessibleActions0, ex.Message))
            End Try
            Dim actions As WAB.AccessibleActions = CType(Marshal.PtrToStructure(a, GetType(WAB.AccessibleActions)), WAB.AccessibleActions)
            For i As Integer = 0 To actions.ActionsCount - 1
                Dim action As WAB.AccessibleActionInfo = actions.ActionInfo(i)
                Dim name As String = action.Name
                If Not String.IsNullOrEmpty(name) Then
                    list.Add(name)
                End If
            Next
            Return list
        Finally
            If a <> IntPtr.Zero Then Marshal.FreeHGlobal(a)
        End Try
    End Function

    ''' <summary>
    ''' Determines whether a particular action is permitted by this
    ''' element, as published in the AllowedActions property.
    ''' </summary>
    ''' <param name="ActionName">The name of the action to be tested</param>
    ''' <returns>Returns true if the supplied value is contained in the 
    ''' list of allowed actions.</returns>
    Public Function AllowsAction(ByVal ActionName As String) As Boolean
        If mAllowedActions Is Nothing Then
            mAllowedActions = GetActions()
        End If
        Return mAllowedActions.Contains(ActionName)
    End Function

    ''' <summary>
    ''' Get all the children of this JABContext, both 
    ''' visible and non-visible.
    ''' </summary>
    ''' <returns>A list of JABContext objects representing the children.</returns>
    ''' <remarks>The member <see cref="mInfo">info</see> must have been populated first;
    ''' the <see cref="WAB.AccessibleContextInfo.childrenCount">childrencount</see>
    ''' property is required.</remarks>
    Private Function GetChildren() As List(Of JABContext)
        Dim list As New List(Of JABContext)

        For i As Integer = 0 To mInfo.childrenCount - 1
            Dim ChildAC As Long = WAB.getAccessibleChildFromContext(Me.vmID, Me.AC, i)
            If ChildAC <> 0 Then
                list.Add(New JABContext(ChildAC, Me.vmID))
            End If
        Next

        Return list
    End Function


    Protected Overrides Sub Finalize()
        Me.Dispose(False)
    End Sub

    ''' <summary>
    ''' Releases the AC
    ''' </summary>
    Private Sub ReleaseAC()
        If mAC <> 0 Then
            WAB.releaseJavaObject(vmID, mAC)
            mAC = 0
        End If
    End Sub

#Region " IDisposable Support "
    Private disposedValue As Boolean = False
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        mInstances.Remove(Me)

        If Not Me.disposedValue Then
            If disposing Then
                'Free managed resources when explicitly called
                If mChildren IsNot Nothing Then
                    For Each jcChild As JABContext In mChildren
                        jcChild.Dispose()
                    Next
                End If
            End If

            'Free shared unmanaged resources
            Me.ReleaseAC()
        End If
        Me.disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    ''' <summary>
    ''' Holds a list of properties that can be indexed quickly for a given Identifier type.
    ''' </summary>
    Private Shared mobjProperties As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)

    ''' <summary>
    ''' Returns a list of properties that can be indexed quickly for a given Identifier type.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetProperties() As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)

        If mobjProperties Is Nothing Then
            mobjProperties = New Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)
            For Each pi As System.Reflection.PropertyInfo In GetType(JABContext).GetProperties(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.IgnoreCase)
                Dim Attributes As Object() = pi.GetCustomAttributes(GetType(JABContextIdentifierAttribute), False)
                If Attributes IsNot Nothing AndAlso Attributes.Length > 0 Then
                    mobjProperties.Add(CType(Attributes(0), JABContextIdentifierAttribute).Identifier, pi)
                End If
            Next
        End If

        Return mobjProperties
    End Function

    ''' <summary>
    ''' Appends the identifiers for this element in spy query format into a buffer
    ''' </summary>
    ''' <param name="sb">The buffer to which this context should append its
    ''' identifiers</param>
    ''' <returns>The buffer into which this element has been appended</returns>
    Public Function AppendIdentifiers(ByVal sb As StringBuilder) As StringBuilder

        For Each pi As PropertyInfo In GetType(JABContext) _
         .GetProperties(BindingFlags.Instance Or BindingFlags.Public)

            Dim attrs As Object() = pi.GetCustomAttributes(
             GetType(JABContextIdentifierAttribute), False)

            If attrs Is Nothing OrElse attrs.Length = 0 Then Continue For

            Dim jci As JABContextIdentifierAttribute =
             CType(attrs(0), JABContextIdentifierAttribute)
            Dim val As String
            Try
                val = CStr(pi.GetValue(Me, Nothing))
            Catch ex As TargetInvocationException
                Throw New BluePrismException(My.Resources.ExceptionGettingJavaIdentfier0Exception1, pi.Name, ex.InnerException.Message)
            End Try
            If jci.DefaultEnabled Then sb.Append("+")
            sb.Append(CObj(jci.Identifier)).Append("=") _
             .Append(clsQuery.EncodeValue(val)).Append(" "c)

        Next
        ' Add the screen bounds separately so we can convert it to its correct format
        sb.Append("ScreenBounds=").Append(CType(ScreenBounds, RECT))

        Return sb

    End Function

    ''' <summary>
    ''' Gets the identifiers for this element in spy query format
    ''' </summary>
    ''' <returns>The identifiers which identify the element represented by this
    ''' context</returns>
    Public Function GetIdentifiers() As String
        Return AppendIdentifiers(New StringBuilder(1024)).ToString()
    End Function

End Class

