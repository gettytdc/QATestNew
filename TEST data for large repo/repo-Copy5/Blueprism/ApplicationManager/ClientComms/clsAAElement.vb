Imports Accessibility
Imports System.ComponentModel
Imports System.Collections.Generic
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Runtime.InteropServices

''' <summary>
''' This class serves as a wrapper to the Accessiblity.IAccessible interface. The
''' class is mainly to capture exceptions which can be thrown when accessing
''' properties of the interface, as well as to hide the strange ID,Interface
''' pairing. The class also encapsulates the pinvokes needed to work with
''' accessibility elements. 
''' </summary>
<CLSCompliant(False)> _
Public Class clsAAElement

    ''' <summary>
    ''' The accessibility element interface
    ''' </summary>
    Private mElement As IAccessible

    ''' <summary>
    ''' The accessibility element ID when the element is a simple element
    ''' </summary>
    Private mId As Object

    ''' <summary>
    ''' Used to indicate when this accessibility element is a simple element
    ''' </summary>
    Private mIsSimple As Boolean

    ''' <summary>
    ''' Enumeration that lists all the special ID's that the accessiblity element can
    ''' have. Note that most of the special ID's are negative.
    ''' </summary>
    Private Enum ObjectIdentifiers
        Self = &H0 'Self/Window
        SysMenu = &HFFFFFFFF
        TitleBar = &HFFFFFFFE
        Menu = &HFFFFFFFD
        Client = &HFFFFFFFC
        VScroll = &HFFFFFFFB
        HScroll = &HFFFFFFFA
        SizeGrip = &HFFFFFFF9
        Caret = &HFFFFFFF8
        Cursor = &HFFFFFFF7
        Alert = &HFFFFFFF6
        Sound = &HFFFFFFF5
        QueryClassnameIDX = &HFFFFFFF4
        NativeOM = &HFFFFFFF0
    End Enum

    ''' <summary>
    ''' Constructs a clsAAElement from an accessible interface
    ''' </summary>
    ''' <param name="e"></param>
    Public Sub New(ByVal e As IAccessible)
        Me.New(e, ObjectIdentifiers.Self)
    End Sub

    ''' <summary>
    ''' Constructs a clsAAElement from an accessible interface and an ID
    ''' </summary>
    ''' <param name="e"></param>
    ''' <param name="id"></param>
    Public Sub New(ByVal e As IAccessible, ByVal id As Object)
        mElement = e
        mId = id
        mIsSimple = (CType(mId, ObjectIdentifiers) <> ObjectIdentifiers.Self)
    End Sub

    ''' <summary>
    ''' The Window Handle of the window that contains this element
    ''' </summary>
    Friend ReadOnly Property Window() As IntPtr
        Get
            Dim hWnd As IntPtr
            WindowFromAccessibleObject(mElement, hWnd)
            Return hWnd
        End Get
    End Property

    ''' <summary>
    ''' If this is a complex element then this function returns a list of child
    ''' elements belonging to this element. If this is a simple element an empty list
    ''' will be returned, as simple elements have no children.
    ''' </summary>
    Public ReadOnly Property Elements() As List(Of clsAAElement)
        Get
            Dim elems As New List(Of clsAAElement)

            Dim count As Integer = ElementCount
            If count = 0 Then Return elems

            Dim returnCount As Integer
            Dim children(count - 1) As Object
            AccessibleChildren(mElement, 0, count, children, returnCount)
            For Each child As Object In children
                Dim elem As clsAAElement = ElementFromObject(child)
                If elem IsNot Nothing Then elems.Add(elem)
            Next

            Return elems
        End Get
    End Property

    ''' <summary>
    ''' Checks if this AA element is 'equal enough' to the given AA element to
    ''' strongly suggest that it represents the same element.
    ''' </summary>
    ''' <param name="elem">The element to test against</param>
    ''' <returns>True if the <see cref="Name"/>, <see cref="Role"/>,
    ''' <see cref="ScreenBounds"/>, <see cref="Value"/> and <see cref="Value2"/> of
    ''' the given element match this element's corresponding values; False otherwise.
    ''' </returns>
    Friend Function EqualsIsh(ByVal elem As clsAAElement) As Boolean
        If elem Is Nothing Then Return False
        Return (elem.Name = Me.Name _
         AndAlso elem.Role = Me.Role _
         AndAlso elem.ScreenBoundsRectangle = Me.ScreenBoundsRectangle _
         AndAlso elem.Value = Me.Value _
         AndAlso elem.Value2 = Me.Value2)
    End Function

    ''' <summary>
    ''' Converts a Variant to the correct type of element, this is the strange ID
    ''' Interface duality.
    ''' </summary>
    ''' <param name="o"></param>
    ''' <returns></returns>
    Private Function ElementFromObject(ByVal o As Object) As clsAAElement
        If TypeOf o Is Integer Then
            Return New clsAAElement(mElement, o)
        Else
            Dim ia As IAccessible = TryCast(o, IAccessible)
            If Not ia Is Nothing Then
                Return New clsAAElement(ia, ObjectIdentifiers.Self)
            End If
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets an element from a window handle
    ''' </summary>
    ''' <param name="hWnd"></param>
    ''' <returns></returns>
    Public Shared Function FromWindow(ByVal hWnd As IntPtr) As clsAAElement
        Dim objWindow As IAccessible = Nothing
        'Const OBJID_CLIENT As Integer = &HFFFFFFFC
        Const OBJID_WINDOW As Integer = &H0
        AccessibleObjectFromWindow(hWnd, OBJID_WINDOW, IID_IACCESSIBLE, objWindow)
        Return New clsAAElement(objWindow)
    End Function

    ''' <summary>
    ''' Gets an element from a point on the screen.
    ''' </summary>
    ''' <param name="pt"></param>
    ''' <returns></returns>
    Public Shared Function FromPoint(ByVal pt As POINTAPI) As clsAAElement
        Dim ID As Object = Nothing
        Dim accObj As IAccessible = Nothing
        AccessibleObjectFromPoint(pt, accObj, ID)
        If Not accObj Is Nothing Then
            Return New clsAAElement(accObj, ID)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Returns the number of child elements
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.ElementCount)> _
    Public ReadOnly Property ElementCount() As Integer
        Get
            If Me.mIsSimple Then Return 0

            Try
                Return mElement.accChildCount
            Catch
                Return 0
            End Try
        End Get
    End Property

    ''' <summary>
    ''' The text describing the default action
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.DefaultAction)> _
    Public ReadOnly Property DefaultAction() As String
        Get
            Try
                Dim s As String = mElement.accDefaultAction(mId)
                If s Is Nothing Then Return String.Empty
                Return s
            Catch
                Return String.Empty
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets the description of the accessibility element
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Description)> _
    Public ReadOnly Property Description() As String
        Get
            Try
                Dim s As String = mElement.accDescription(mId)
                If s Is Nothing Then Return String.Empty
                Return s
            Catch
                Return String.Empty
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Performs the Default action
    ''' </summary>
    Public Sub DoDefaultAction()
        mElement.accDoDefaultAction(mId)
    End Sub

    ''' <summary>
    ''' The element that has focus 
    ''' </summary>
    Public ReadOnly Property Focus() As clsAAElement
        Get
            Return ElementFromObject(mElement.accFocus)
        End Get
    End Property

    ''' <summary>
    ''' The help string associated with the accessiblity element
    ''' </summary>
    Public ReadOnly Property Help() As String
        Get
            Try
                Dim s As String = mElement.accHelp
                If s Is Nothing Then Return String.Empty
                Return s
            Catch
                Return String.Empty
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Performs a hit test on the accessibility element given screen co-ordinates
    ''' </summary>
    ''' <param name="pt"></param>
    ''' <returns></returns>
    Public Function HitTest(ByVal pt As POINTAPI) As clsAAElement
        Return ElementFromObject(mElement.accHitTest(pt.x, pt.y))
    End Function

    ''' <summary>
    ''' The keyboard shortcut to invoke the accessibility element
    ''' </summary>
    Public ReadOnly Property KeyboardShortcut() As String
        Get
            Try
                Dim s As String = mElement.accKeyboardShortcut(mId)
                If s Is Nothing Then Return String.Empty
                Return s
            Catch
                Return String.Empty
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the accessibility element relative to the screen
    ''' </summary>
    Public Overridable ReadOnly Property ScreenBounds() As RECT
        Get
            Try
                Dim x As Integer, y As Integer, w As Integer, h As Integer
                mElement.accLocation(x, y, w, h, mId)
                Return New RECT(x, x + w, y, y + h)
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the accessibility element relative to the screen in the
    ''' form of a <see cref="Rectangle"/>
    ''' </summary>
    Public ReadOnly Property ScreenBoundsRectangle() As Rectangle
        Get
            Return CType(ScreenBounds, Rectangle)
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the accessibility element relative
    ''' to its parent rather than the screen
    ''' </summary>
    Public ReadOnly Property ClientBounds() As RECT
        Get
            Try
                Dim bounds As RECT = ScreenBounds
                bounds.Offset(Parent.ScreenBounds.Location, False)
                Return bounds
            Catch
                Return Me.ScreenBounds
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the accessibility element relative to its parent in the
    ''' form of a <see cref="Rectangle"/>
    ''' </summary>
    Public ReadOnly Property ClientBoundsRectangle() As Rectangle
        Get
            Return CType(ClientBounds, Rectangle)
        End Get
    End Property

    ''' <summary>
    ''' The ID string for this AA Element, or an empty string if the ID string was
    ''' unavailable for any reason.
    ''' </summary>
    <Obsolete( _
     "Uses IAccIdentity which I think creates a memory leak - only here so that " & _
     "we have a base to work from in identity capturing in the future", True)> _
    Private ReadOnly Property IdString() As String
        Get
            ' If we don't have an element, we don't have an ID string
            If mElement Is Nothing Then Return ""

            ' If it's not an AccIdentity we don't have an ID string
            Dim ident As IAccIdentity
            Try
                ident = TryCast(mElement, IAccIdentity)
            Catch ' Can happen with COM casting
                ident = Nothing
            End Try
            If ident Is Nothing Then Return ""

            ' Pointer and memory count
            Dim mem As IntPtr = IntPtr.Zero
            Try
                mem = Marshal.AllocCoTaskMem(255)
                Dim memLen As UInteger = 0
                ' Callee assigns the memory using CoTaskMemAlloc when this is called
                ' So we must free it using CoTaskMemFree (ie. Marshal.FreeCoTaskMem)
                ident.GetIdentityString(0, mem, memLen)

                Dim str As String = Marshal.PtrToStringAnsi(mem, CInt(memLen))
                If str Is Nothing Then Return "" Else Return str

            Finally
                ' Free the memory allocated for the string if it's there.
                If mem <> IntPtr.Zero Then Marshal.FreeCoTaskMem(mem)

            End Try

        End Get
    End Property

    ''' <summary>
    ''' The number of ancestors that this element has, ie. the number
    ''' of times that .Parent can be called up through the hierarchy
    ''' until it returns null -or- returns the same object again,
    ''' according to an <see cref="EqualsIsh"/> check.
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.aAncestorCount)> _
    Public ReadOnly Property AncestorCount() As Integer
        Get
            Dim count As Integer = 0
            Dim parent As IAccessible = TryCast(mElement.accParent, IAccessible)
            Dim lastParentElem As New clsAAElement(parent)
            While parent IsNot Nothing
                count += 1
                parent = TryCast(parent.accParent, IAccessible)
                If parent IsNot Nothing Then
                    Dim nextParentElem As New clsAAElement(parent)
                    Dim eqIsh As Boolean = _
                     lastParentElem.EqualsIsh(nextParentElem)
                    If eqIsh Then Exit While
                    lastParentElem = nextParentElem
                End If
                ' Handle silly nesting which is surely an error
                If count > 100 Then Return 999
            End While
            Return count
        End Get
    End Property

    <AAIdentifierAttribute(clsQuery.IdentifierTypes.aX)> _
    Public ReadOnly Property Left() As Integer
        Get
            Return Me.ClientBounds.Left
        End Get
    End Property

    <AAIdentifierAttribute(clsQuery.IdentifierTypes.aY)> _
     Public ReadOnly Property Top() As Integer
        Get
            Return Me.ClientBounds.Top
        End Get
    End Property

    <AAIdentifierAttribute(clsQuery.IdentifierTypes.aWidth)> _
    Public ReadOnly Property Width() As Integer
        Get
            Return Me.ClientBounds.Width
        End Get
    End Property

    <AAIdentifierAttribute(clsQuery.IdentifierTypes.aHeight)> _
     Public ReadOnly Property Height() As Integer
        Get
            Return Me.ClientBounds.Height
        End Get
    End Property

    ''' <summary>
    ''' The name of the accessiblity element
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Name)> _
    Public Property Name() As String
        Get
            Try
                Dim s As String = mElement.accName(mId)
                If s Is Nothing Then Return String.Empty
                Return s
            Catch
                Return String.Empty
            End Try
        End Get
        Set(ByVal value As String)
            mElement.accName(mId) = value
        End Set
    End Property

    ''' <summary>
    ''' Flags used to specify the navigation type in calls
    ''' to clsAAElement.Navigate()
    ''' </summary>
    Public Enum NavigateFlags
        ''' <summary>
        ''' ???
        ''' </summary>
        NAVDIR_MIN = &H0
        ''' <summary>
        ''' Navigate to the sibling object located above
        ''' the starting object.
        ''' </summary>
        NAVDIR_UP = &H1
        ''' <summary>
        ''' Navigate to the sibling object located below
        ''' the starting object.
        ''' </summary>
        NAVDIR_DOWN = &H2
        ''' <summary>
        ''' Navigate to the sibling object located to the
        ''' left of the starting object.
        ''' </summary>
        NAVDIR_LEFT = &H3
        ''' <summary>
        ''' Navigate to the sibling object located to the
        ''' right of the starting object.
        ''' </summary>
        NAVDIR_RIGHT = &H4
        ''' <summary>
        ''' Navigate to the next logical object, generally
        ''' a sibling to the starting object.
        ''' </summary>
        NAVDIR_NEXT = &H5
        ''' <summary>
        ''' Navigate to the previous logical object, generally
        ''' a sibling to the starting object.
        ''' </summary>
        NAVDIR_PREVIOUS = &H6
        ''' <summary>
        ''' Navigate to the first child of this object.
        ''' When using this flag, the lVal member of the
        ''' varStart parameter must be CHILDID_SELF.
        ''' </summary>
        NAVDIR_FIRSTCHILD = &H7
        ''' <summary>
        ''' Navigate to the last child of this object.
        ''' When using this flag, the lVal member of the
        ''' varStart parameter must be CHILDID_SELF
        ''' </summary>
        NAVDIR_LASTCHILD = &H8
        ''' <summary>
        ''' ???
        ''' </summary>
        NAVDIR_MAX = &H9
    End Enum

    ''' <summary>
    ''' Navigates to an adjacent accessibility element 
    ''' </summary>
    ''' <param name="navDir">The direction in which to navigate</param>
    ''' <returns>Returns the element found by following the specified navigation
    ''' direction, if any.</returns>
    Public Function Navigate(ByVal navDir As NavigateFlags) As clsAAElement
        Return ElementFromObject(mElement.accNavigate(navDir, mId))
    End Function

    ''' <summary>
    ''' The parent accessibility element, if any.
    ''' </summary>
    <TypeConverter(GetType(ExpandableObjectConverter))> _
    Public ReadOnly Property Parent() As clsAAElement
        Get
            Return New clsAAElement(TryCast(mElement.accParent, IAccessible))
        End Get
    End Property

    ''' <summary>
    ''' The ID of the accessibility element as a string
    ''' </summary>
    Public ReadOnly Property ID() As String
        Get
            Return CType(mId, ObjectIdentifiers).ToString()
        End Get
    End Property

    ''' <summary>
    ''' Gets the role of the accessibility element as an AccessibleRole number see above.
    ''' </summary>
    Public ReadOnly Property Role() As AccessibleRole
        Get
            Try
                Return CType(mElement.accRole(mId), AccessibleRole)
            Catch
                Return 0
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets the role of the accessibility element as a string
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Role)> _
    Public ReadOnly Property RoleString() As String
        Get
            Return Role.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Gets the role of the accessibility element as a localised string.
    ''' </summary>
    Public ReadOnly Property RoleText() As String
        Get
            Dim sb As New StringBuilder(256)
            GetRoleText(Role, sb, 255)
            Return sb.ToString
        End Get
    End Property

    ''' <summary>
    ''' The types of selection operation available for the
    ''' Active Accessibility <see cref="SelectElement" /> method.
    ''' </summary>
    ''' <remarks>For a quick-reference on selecting child elements,
    ''' see http://msdn2.microsoft.com/en-us/library/ms695741.aspx
    ''' 
    ''' Note that the following flag combinations are not allowed:
    '''    * SELFLAG_ADDSELECTION | SELFLAG_REMOVESELECTION
    '''    * SELFLAG_ADDSELECTION | SELFLAG_TAKESELECTION
    '''    * SELFLAG_REMOVESELECTION | SELFLAG_TAKESELECTION
    '''    * SELFLAG_EXTENDSELECTION | SELFLAG_TAKESELECTION
    '''</remarks>
    Private Enum SelectFlags
        ''' <summary>
        ''' Performs no action. Active Accessibility does not
        ''' change the selection or focus.
        ''' </summary>
        SELFLAG_NONE = 0
        ''' <summary>
        ''' Sets the focus to the object and makes it the selection anchor.
        ''' Used by itself, this flag does not alter the selection.
        ''' The effect is similar to moving the focus manually by pressing
        ''' the arrow keys while holding down the CTRL key in Windows Explorer
        ''' or in any multiple-selection list box.
        '''
        ''' With objects that have the STATE_SYSTEM_MULTISELECTABLE,
        ''' SELFLAG_TAKEFOCUS is combined with the following values:
        '''
        '''    * SELFLAG_TAKESELECTION
        '''    * SELFLAG_EXTENDSELECTION
        '''    * SELFLAG_ADDSELECTION
        '''    * SELFLAG_REMOVESELECTION
        '''    * SELFLAG_ADDSELECTION | SELFLAG_EXTENDSELECTION
        '''    * SELFLAG_REMOVESELECTION | SELFLAG_EXTENDSELECTION
        ''' 
        ''' If you call IAccessible::accSelect with the SELFLAG_TAKEFOCUS
        ''' flag on an object that has an HWND, the flag takes effect only
        ''' if the object's parent already has the focus. 
        ''' </summary>
        SELFLAG_TAKEFOCUS = 1
        ''' <summary>
        ''' Selects the object and removes the selection from all other
        ''' objects in the container.
        '''
        ''' This flag does not change the focus or the selection anchor
        ''' unless it is combined with SELFLAG_TAKEFOCUS.
        ''' The SELFLAG_TAKESELECTION | SELFLAG_TAKEFOCUS combination
        ''' is equivalent to single-clicking an item in Windows Explorer.
        '''
        ''' This flag must not be combined with the following flags:
        '''
        '''    * SELFLAG_ADDSELECTION
        '''    * SELFLAG_REMOVESELECTION
        '''    * SELFLAG_EXTENDSELECTION
        ''' </summary>
        SELFLAG_TAKESELECTION = 2
        ''' <summary>
        ''' Alters the selection so that all objects between the selection
        ''' anchor and this object take on the anchor object's selection state.
        ''' If the anchor object is not selected, the objects are removed from
        ''' the selection. If the anchor object is selected, the selection is
        ''' extended to include this object and all the objects in between.
        ''' Set the selection state by combining this flag with
        ''' SELFLAG_ADDSELECTION or SELFLAG_REMOVESELECTION.
        ''' 
        ''' This flag does not change the focus or the selection anchor unless
        ''' it is combined with SELFLAG_TAKEFOCUS.
        ''' 
        ''' The SELFLAG_EXTENDSELECTION | SELFLAG_TAKEFOCUS combination is
        ''' equivalent to adding an item to a selection manually by holding
        ''' down the SHIFT key and clicking an unselected object in Windows Explorer.
        ''' 
        ''' This flag is not combined with SELFLAG_TAKESELECTION. 
        ''' </summary>
        SELFLAG_EXTENDSELECTION = 4
        ''' <summary>
        ''' Adds the object to the current selection; possible result is a
        ''' noncontiguous selection.
        '''
        ''' This flag does not change the focus or the selection anchor unless
        ''' it is combined with SELFLAG_TAKEFOCUS. The
        ''' SELFLAG_ADDSELECTION | SELFLAG_TAKEFOCUS combination is equivalent
        ''' to adding an item to a selection manually by holding down the CTRL
        ''' key and clicking an unselected object in Windows Explorer.
        '''
        ''' This flag is not combined with SELFLAG_REMOVESELECTION or with
        ''' SELFLAG_TAKESELECTION. 
        ''' </summary>
        SELFLAG_ADDSELECTION = 8
        ''' <summary>
        ''' Removes the object from the current selection; possible result
        ''' is a noncontiguous selection.
        '''
        '''  This flag does not change the focus or the selection anchor unless
        ''' it is combined with SELFLAG_TAKEFOCUS. The
        ''' SELFLAG_REMOVESELECTION | SELFLAG_TAKEFOCUS combination is equivalent
        ''' to removing an item from a selection manually by holding down the CTRL
        ''' key while clicking a selected object in Windows Explorer.
        '''
        ''' This flag is not combined with SELFLAG_ADDSELECTION or SELFLAG_TAKESELECTION. 
        ''' </summary>
        SELFLAG_REMOVESELECTION = 16
    End Enum


    ''' <summary>
    ''' Selects an accessibility element
    ''' </summary>
    ''' <param name="flagsSelect">Indicates the type of selection operation to be
    ''' performed.</param>
    Private Sub SelectElement(ByVal flagsSelect As SelectFlags)
        Try
            mElement.accSelect(flagsSelect, mId)
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Gives focus to the element
    ''' </summary>
    Public Sub TakeFocus()
        SelectElement(SelectFlags.SELFLAG_TAKEFOCUS)
    End Sub

    ''' <summary>
    ''' Makes the current element selected
    ''' </summary>
    Public Sub TakeSelection()
        SelectElement(SelectFlags.SELFLAG_TAKESELECTION)
    End Sub

    ''' <summary>
    ''' Makes the current element selected and focused
    ''' </summary>
    Public Sub TakeFocusSelection()
        SelectElement(SelectFlags.SELFLAG_TAKEFOCUS Or SelectFlags.SELFLAG_TAKESELECTION)
    End Sub

    ''' <summary>
    ''' Returns the selected accessibility element
    ''' </summary>
    Public ReadOnly Property Selection() As clsAAElement
        Get
            Return ElementFromObject(mElement.accSelection)
        End Get
    End Property

    'The following properties just provide neat access to the 32 state flags that are stored in a 32bit number

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Unavailable)> _
    Public ReadOnly Property Unavailable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Unavailable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Selected)> _
     Public ReadOnly Property Selected() As Boolean
        Get
            Return CBool(State And AccessibleStates.Selected)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Focused)> _
     Public ReadOnly Property Focused() As Boolean
        Get
            Return CBool(State And AccessibleStates.Focused)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Pressed)> _
     Public ReadOnly Property Pressed() As Boolean
        Get
            Return CBool(State And AccessibleStates.Pressed)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.aChecked)> _
     Public ReadOnly Property Checked() As Boolean
        Get
            Return CBool(State And AccessibleStates.Checked)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Mixed)> _
     Public ReadOnly Property Mixed() As Boolean
        Get
            Return CBool(State And AccessibleStates.Mixed)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.ReadOnly)> _
    Public ReadOnly Property [ReadOnly]() As Boolean
        Get
            Return CBool(State And AccessibleStates.ReadOnly)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Hottracked)> _
     Public ReadOnly Property Hottracked() As Boolean
        Get
            Return CBool(State And AccessibleStates.Hottracked)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Default)> _
     Public ReadOnly Property [Default]() As Boolean
        Get
            Return CBool(State And AccessibleStates.Default)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Expanded)> _
     Public ReadOnly Property Expanded() As Boolean
        Get
            Return CBool(State And AccessibleStates.Expanded)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Collapsed)> _
     Public ReadOnly Property Collapsed() As Boolean
        Get
            Return CBool(State And AccessibleStates.Collapsed)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Busy)> _
     Public ReadOnly Property Busy() As Boolean
        Get
            Return CBool(State And AccessibleStates.Busy)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Floating)> _
     Public ReadOnly Property Floating() As Boolean
        Get
            Return CBool(State And AccessibleStates.Floating)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Marqueed)> _
     Public ReadOnly Property Marqueed() As Boolean
        Get
            Return CBool(State And AccessibleStates.Marqueed)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Animated)> _
     Public ReadOnly Property Animated() As Boolean
        Get
            Return CBool(State And AccessibleStates.Animated)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Invisible)> _
     Public ReadOnly Property Invisible() As Boolean
        Get
            Return CBool(State And AccessibleStates.Invisible)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Offscreen)> _
     Public ReadOnly Property Offscreen() As Boolean
        Get
            Return CBool(State And AccessibleStates.Offscreen)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Sizeable)> _
     Public ReadOnly Property Sizeable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Sizeable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Moveable)> _
     Public ReadOnly Property Moveable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Moveable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.SelfVoicing)> _
     Public ReadOnly Property SelfVoicing() As Boolean
        Get
            Return CBool(State And AccessibleStates.SelfVoicing)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Focusable)> _
     Public ReadOnly Property Focusable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Focusable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Selectable)> _
    Public ReadOnly Property Selectable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Selectable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Linked)> _
     Public ReadOnly Property Linked() As Boolean
        Get
            Return CBool(State And AccessibleStates.Linked)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Traversed)> _
     Public ReadOnly Property Traversed() As Boolean
        Get
            Return CBool(State And AccessibleStates.Traversed)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Multiselectable)> _
     Public ReadOnly Property Multiselectable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Multiselectable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Extselectable)> _
     Public ReadOnly Property Extselectable() As Boolean
        Get
            Return CBool(State And AccessibleStates.Extselectable)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Alert_low)> _
     Public ReadOnly Property Alert_low() As Boolean
        Get
            Return CBool(State And AccessibleStates.AlertLow)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Alert_medium)> _
     Public ReadOnly Property Alert_medium() As Boolean
        Get
            Return CBool(State And AccessibleStates.AlertMedium)
        End Get
    End Property

    <Category("State")> _
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Alert_high)> _
      Public ReadOnly Property Alert_high() As Boolean
        Get
            Return CBool(State And AccessibleStates.AlertHigh)
        End Get
    End Property

    ''' <summary>
    ''' Gets the state of the accessiblilty element
    ''' </summary>
    Public ReadOnly Property State() As Integer
        <DebuggerHidden()> _
        Get
            Try
                Return CInt(mElement.accState(mId))
            Catch
                Return 0
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets a localised string representing the state of the accessibility element
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.State)> _
    Public ReadOnly Property StateText() As String
        Get
            Dim sb As New StringBuilder(256)
            GetStateText(State, sb, 255)
            Return sb.ToString
        End Get
    End Property

    ''' <summary>
    ''' Gets the value of the accessiblilty element
    ''' </summary>
    Public Property Value() As String
        Get
            Try
                Dim s As String = mElement.accValue(mId)
                If s Is Nothing Then Return String.Empty
                Return s.TrimEnd(Chr(13))
            Catch
                Return String.Empty
            End Try
        End Get
        Set(ByVal value As String)
            mElement.accValue = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the value of the accessiblilty element
    ''' </summary>
    <AAIdentifierAttribute(clsQuery.IdentifierTypes.Value2)> _
    Public Property Value2() As String
        Get
            Return Value
        End Get
        Set(ByVal v As String)
            Me.Value = v
        End Set
    End Property

    ''' <summary>
    ''' Get the helptopic of the accessibility element
    ''' </summary>
    ''' <param name="pszHelpFile"></param>
    Public ReadOnly Property HelpTopic(ByVal pszHelpFile As String) As Integer
        Get
            Try
                Return mElement.accHelpTopic(pszHelpFile)
            Catch
                Return 0
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Converts the clsAAElement to a string (this is useful for debugging)
    ''' </summary>
    Public Overrides Function ToString() As String
        Return String.Format("{0} - {1}", IIf(Name = "", "NAMELESS", Name), Role)
    End Function

    ''' <summary>
    ''' Appends this element's identifiers into a string buffer as a query-encoded
    ''' string and returns it
    ''' </summary>
    ''' <param name="sb">The buffer to which this element should be appended</param>
    ''' <returns>The given buffer with this element appended to it</returns>
    Public Function AppendIdentifiers(ByVal sb As StringBuilder) As StringBuilder
        Dim p As clsAAElement = Parent
        Dim r As RECT = ClientBounds
        Dim pr As RECT = p.ClientBounds
        sb.Append("aX=").Append(r.Left)
        sb.Append(" aY=").Append(r.Top)
        sb.Append(" aWidth=").Append(r.Width)
        sb.Append(" aHeight=").Append(r.Height)
        sb.Append(" +Name=").Append(clsQuery.EncodeValue(Name))
        sb.Append(" +Description=").Append(clsQuery.EncodeValue(Description))
        sb.Append(" +Role=").Append(Role.ToString())
        sb.Append(" +ID=").Append(clsQuery.EncodeValue(ID))
        sb.Append(" State=").Append(clsQuery.EncodeValue(StateText))
        sb.Append(" +Value2=").Append(clsQuery.EncodeValue(Value))
        sb.Append(" +KeyboardShortcut=").Append(clsQuery.EncodeValue(KeyboardShortcut))
        sb.Append(" +DefaultAction=").Append(clsQuery.EncodeValue(DefaultAction))
        sb.Append(" +ElementCount=").Append(ElementCount)
        sb.Append(" aAncestorCount=").Append(AncestorCount)
        sb.Append(" Unavailable=").Append(Unavailable)
        sb.Append(" Selected=").Append(Selected)
        sb.Append(" Focused=").Append(Focused)
        sb.Append(" Pressed=").Append(Pressed)
        sb.Append(" aChecked=").Append(Checked)
        sb.Append(" Mixed=").Append(Mixed)
        sb.Append(" ReadOnly=").Append([ReadOnly])
        sb.Append(" Hottracked=").Append(Hottracked)
        sb.Append(" Default=").Append([Default])
        sb.Append(" Expanded=").Append(Expanded)
        sb.Append(" Collapsed=").Append(Collapsed)
        sb.Append(" Busy=").Append(Busy)
        sb.Append(" Floating=").Append(Floating)
        sb.Append(" Marqueed=").Append(Marqueed)
        sb.Append(" Animated=").Append(Animated)
        sb.Append(" Invisible=").Append(Invisible)
        sb.Append(" Offscreen=").Append(Offscreen)
        sb.Append(" Sizeable=").Append(Sizeable)
        sb.Append(" Moveable=").Append(Moveable)
        sb.Append(" SelfVoicing=").Append(SelfVoicing)
        sb.Append(" Focusable=").Append(Focusable)
        sb.Append(" Selectable=").Append(Selectable)
        sb.Append(" Linked=").Append(Linked)
        sb.Append(" Traversed=").Append(Traversed)
        sb.Append(" Multiselectable=").Append(Multiselectable)
        sb.Append(" Extselectable=").Append(Extselectable)
        sb.Append(" Alert_low=").Append(Alert_low)
        sb.Append(" Alert_medium=").Append(Alert_medium)
        sb.Append(" Alert_high=").Append(Alert_high)
        sb.Append(" ScreenBounds=").Append(ScreenBounds.ToString())
        sb.Append(" paX=").Append(pr.Left)
        sb.Append(" paY=").Append(pr.Top)
        sb.Append(" paWidth=").Append(pr.Width)
        sb.Append(" paHeight=").Append(pr.Height)
        sb.Append(" +pName=").Append(clsQuery.EncodeValue(p.Name))
        sb.Append(" +pDescription=").Append(clsQuery.EncodeValue(p.Description))
        sb.Append(" +pRole=").Append(p.Role.ToString())
        sb.Append(" +pID=").Append(clsQuery.EncodeValue(p.ID))
        sb.Append(" pState=").Append(clsQuery.EncodeValue(p.StateText))
        sb.Append(" +pValue2=").Append(clsQuery.EncodeValue(p.Value))
        sb.Append(" +pKeyboardShortcut=").Append(clsQuery.EncodeValue(p.KeyboardShortcut))
        sb.Append(" +pDefaultAction=").Append(clsQuery.EncodeValue(p.DefaultAction))
        sb.Append(" pElementCount=").Append(p.ElementCount)
        sb.Append(" pUnavailable=").Append(p.Unavailable)
        sb.Append(" pSelected=").Append(p.Selected)
        sb.Append(" pFocused=").Append(p.Focused)
        sb.Append(" pPressed=").Append(p.Pressed)
        sb.Append(" paChecked=").Append(p.Checked)
        sb.Append(" pMixed=").Append(p.Mixed)
        sb.Append(" pReadOnly=").Append(p.ReadOnly)
        sb.Append(" pHottracked=").Append(p.Hottracked)
        sb.Append(" pDefault=").Append(p.Default)
        sb.Append(" pExpanded=").Append(p.Expanded)
        sb.Append(" pCollapsed=").Append(p.Collapsed)
        sb.Append(" pBusy=").Append(p.Busy)
        sb.Append(" pFloating=").Append(p.Floating)
        sb.Append(" pMarqueed=").Append(p.Marqueed)
        sb.Append(" pAnimated=").Append(p.Animated)
        sb.Append(" pInvisible=").Append(p.Invisible)
        sb.Append(" pOffscreen=").Append(p.Offscreen)
        sb.Append(" pSizeable=").Append(p.Sizeable)
        sb.Append(" pMoveable=").Append(p.Moveable)
        sb.Append(" pSelfVoicing=").Append(p.SelfVoicing)
        sb.Append(" pFocusable=").Append(p.Focusable)
        sb.Append(" pSelectable=").Append(p.Selectable)
        sb.Append(" pLinked=").Append(p.Linked)
        sb.Append(" pTraversed=").Append(p.Traversed)
        sb.Append(" pMultiselectable=").Append(p.Multiselectable)
        sb.Append(" pExtselectable=").Append(p.Extselectable)
        sb.Append(" pAlert_low=").Append(p.Alert_low)
        sb.Append(" pAlert_medium=").Append(p.Alert_medium)
        sb.Append(" pAlert_high=").Append(p.Alert_high)
        Return sb

    End Function

    ''' <summary>
    ''' Gets the identifiers for this element into a query-encoded string
    ''' </summary>
    Public Function GetIdentifiers() As String
        Return AppendIdentifiers(New StringBuilder(1024)).ToString()
    End Function

    ''' <summary>
    ''' Holds a list of properties that can be indexed quickly for a given Identifier
    ''' type.
    ''' </summary>
    Private Shared mobjProperties As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)

    ''' <summary>
    ''' Returns a list of properties that can be indexed quickly for a given
    ''' Identifier type.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetProperties() As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)
        If mobjProperties Is Nothing Then
            mobjProperties = New Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)

            'Use reflection to find the appropriate properties to be added to the dictionary
            Dim MatchingProperty As System.Reflection.PropertyInfo = Nothing
            For Each pi As System.Reflection.PropertyInfo In GetType(clsAAElement).GetProperties(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.IgnoreCase)

                Dim Attributes As Object() = pi.GetCustomAttributes(GetType(clsAAElement.AAIdentifierAttribute), False)
                If Attributes IsNot Nothing AndAlso Attributes.Length > 0 Then
                    Dim id As clsQuery.IdentifierTypes = CType(Attributes(0), clsAAElement.AAIdentifierAttribute).Identifier
                    mobjProperties.Add(id, pi)
                End If
            Next
        End If

        Return mobjProperties

    End Function

    ''' <summary>
    ''' Class used in property attributes to match up properties of this class
    ''' against parameters in clsQuery.
    ''' </summary>
    Private Class AAIdentifierAttribute
        Inherits System.Attribute
        Public Identifier As clsQuery.IdentifierTypes
        Public Sub New(ByVal Identifier As clsQuery.IdentifierTypes)
            Me.Identifier = Identifier
        End Sub
    End Class

End Class
