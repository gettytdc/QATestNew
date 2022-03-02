Imports System.Runtime.InteropServices
Imports BluePrism.BPCoreLib

Public Class WAB

    ''' <summary>
    ''' The maximum string size (in characters) allowed in the Windows Access Bridge.
    ''' This constant was taken directly from:
    '''   C:\Program Files\Java Access Bridge\src\bridge\src\AccessBridgePackages.h
    ''' (see #define MAX_STRING_SIZE)
    ''' </summary>
    ''' <remarks>Note that in many cases this length will need to be doubled in order
    ''' to obtain the length in bytes, since the encoding of choice seems to be
    ''' UTF16.</remarks>
    Public Const MaxJABStringSize As Integer = 1024

    ''' <summary>
    ''' The length in characters of the JAB short string size. This constant was
    ''' taken directly from:
    '''    C:\Program Files\Java Access Bridge\src\bridge\src\AccessBridgePackages.h
    ''' (see #define SHORT_STRING_SIZE)
    ''' </summary>
    ''' <remarks>Note that in many cases this length will need to be doubled in order
    ''' to obtain the length in bytes, since the encoding of choice seems to be
    ''' UTF16.</remarks>
    Public Const ShortJABStringSize As Integer = 256

    ''' <summary>
    ''' The maximum number of jab action info structures that can be grouped at once.
    ''' This constant was taken directly from:
    '''    C:\Program Files\Java Access Bridge\src\bridge\src\AccessBridgePackages.h
    ''' (see #define MAX_ACTION_INFO)
    ''' </summary>
    Public Const MaxJABActionInfo As Integer = 256

    ''' <summary>
    ''' The maximum number of jab actions that can be performed at once. This
    ''' constant was taken directly from:
    '''    C:\Program Files\Java Access Bridge\src\bridge\src\AccessBridgePackages.h
    ''' (see #define MAX_ACTIONS_TODO)
    ''' </summary>
    Public Const MaxJABActionsToDo As Integer = 32

    Public Const MaxNumChildren As Integer = 256

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Public Structure AccessibleContextInfo
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MaxJABStringSize)> _
        Public name As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MaxJABStringSize)> _
        Public description As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public role As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public role_EN_US As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public states As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public states_EN_US As String
        Public indexInParent As Integer
        Public childrenCount As Integer
        ''' <summary>
        ''' The x-coordinate of the element, in screen coordinates
        ''' </summary>
        Public x As Integer
        ''' <summary>
        ''' The y-coordinate of the element, in screen coordinates
        ''' </summary>
        Public y As Integer
        ''' <summary>
        ''' The width of the element, in pixels
        ''' </summary>
        Public width As Integer
        ''' <summary>
        ''' The height of the element, in pixels
        ''' </summary>
        Public height As Integer
        Public accessibleComponent As Boolean
        Public accessibleAction As Boolean
        Public accessibleSelection As Boolean
        Public accessibleText As Boolean
        Public accessibleInterfaces As Boolean
    End Structure


    ''' <summary>
    ''' Groups several accessible actions. Used for getting info about
    ''' all accessible actions of a single context.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)> _
    Public Structure AccessibleActions
        Public ActionsCount As Integer
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=MaxJABActionInfo)> _
        Public ActionInfo() As AccessibleActionInfo
    End Structure

    ''' <summary>
    ''' Information about an accessible action.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Public Structure AccessibleActionInfo
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public Name As String
    End Structure

    ''' <summary>
    ''' Groups several accessible actions, to be performed at once
    ''' using doAccessibleActions.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)> _
    Public Structure AccessibleActionsToDo
        Public ActionsCount As Integer
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=MaxJABActionsToDo)> _
        Public Actions() As AccessibleActionInfo
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Public Structure AccessibleTextSelectionInfo
        Public selectionStartIndex As Integer
        Public selectionEndIndex As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MaxJABStringSize)> _
        Public selectedText As String
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure AccessibleTextInfo
        ''' <summary>
        ''' The number of characters in the text.
        ''' </summary>
        Public charCount As Integer
        ''' <summary>
        ''' The index of the caret position.
        ''' </summary>
        Public CaretIndex As Integer
        ''' <summary>
        ''' The index of the character under the specified
        ''' coordinate location.
        ''' </summary>
        Public IndexAtPoint As Integer
    End Structure

    ''' <summary>
    ''' The type of architecture for a process, ie. 64-bit, 32-bit running on 64-bit
    ''' OS, or 32-bit running on 32-bit OS
    ''' </summary>
    Public Enum Architecture

        ''' <summary>
        ''' An unknown architecture
        ''' </summary>
        Unknown = 0

        ''' <summary>
        ''' Native 32bit Java i.e. Legacy
        ''' </summary>
        Native32

        ''' <summary>
        ''' (Wow64), meaning a 32bit Java running on 64bit platform
        ''' </summary>
        Wow64_32

        ''' <summary>
        ''' Native 64bit Java.
        ''' </summary>
        Native64

    End Enum

    ''' <summary>
    ''' Gets the architecture of the current process. Note that this is entirely
    ''' uncached.
    ''' </summary>
    Private Shared ReadOnly Property BaseArchitecture() As Architecture
        Get
            If IntPtr.Size = 8 Then Return Architecture.Native64
            If BPUtil.Is64BitOperatingSystem Then Return Architecture.Wow64_32
            Return Architecture.Native32
        End Get
    End Property

    ''' <summary>
    ''' Gets the active architecture in use in this class. Note that this may be
    ''' set directly by the initialisation of the windows access bridge if the 'base'
    ''' architecture is unavailable.
    ''' </summary>
    ''' <seealso cref="Windows_run"/>
    Private Shared Property ActiveArchitecture() As Architecture
        Get
            If mArch = Architecture.Unknown Then mArch = BaseArchitecture
            Return mArch
        End Get
        Set(ByVal value As Architecture)
            mArch = value
        End Set
    End Property
    Private Shared mArch As Architecture

    ''' <summary>
    ''' Gets info about the text in the specified Accessible Context, at the
    ''' specified location.
    ''' </summary>
    ''' <param name="vmID">The virtual machine.</param>
    ''' <param name="ac"></param>
    ''' <param name="Info">Carries back the info.</param>
    ''' <param name="LocationX">The X location of interest, in pixels relative to the
    ''' context's top left corner.</param>
    ''' <param name="LocationY">The Y location of interest, in pixels relative to the
    ''' context's top left corner.</param>
    ''' <returns>True on success.</returns>
    Public Shared Function getAccessibleTextInfo(ByVal vmID As Integer, ByVal ac As Long, ByRef Info As AccessibleTextInfo, ByVal LocationX As Integer, ByVal LocationY As Integer) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleTextInfo(vmID, CType(ac, Int32), Info, LocationX, LocationY)
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleTextInfo(vmID, ac, Info, LocationX, LocationY)
            Case Architecture.Native64
                Return WAB64.getAccessibleTextInfo(vmID, ac, Info, LocationX, LocationY)
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Gets a substring of a body of text by specifying line numbers
    ''' of interest.
    ''' </summary>
    ''' <param name="vmID">The virtual machine ID.</param>
    ''' <param name="ac">The accessibility context.</param>
    ''' <param name="StartIndex">The zero-based index of the first
    ''' character to be retrieved (inclusive).</param>
    ''' <param name="EndIndex">The zero-based index of the last character
    ''' to be retrieved (inclusive).</param>
    ''' <param name="Text">Carries back the text, in unicode.</param>
    ''' <param name="Length">The maximum number of characters to retrieve.
    ''' This length must not exceed the number of characters available.
    ''' Use the getAccessibleTextInfo method to retrieve this information.</param>
    ''' <returns>True on success.</returns>
    Public Shared Function getAccessibleTextRange(ByVal vmID As Integer, ByVal ac As Long, ByVal StartIndex As Integer, ByVal EndIndex As Integer, ByVal Text As StringBuilder, ByVal Length As Short) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleTextRange(vmID, CType(ac, Int32), StartIndex, EndIndex, Text, Length)
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleTextRange(vmID, ac, StartIndex, EndIndex, Text, Length)
            Case Architecture.Native64
                Return WAB64.getAccessibleTextRange(vmID, ac, StartIndex, EndIndex, Text, Length)
        End Select
        Return False
    End Function

    Public Shared Function getAccessibleTextSelectionInfo(ByVal vmID As Integer, ByVal ac As Long, ByRef info As AccessibleTextSelectionInfo) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleTextSelectionInfo(vmID, CType(ac, Int32), info)
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleTextSelectionInfo(vmID, ac, info)
            Case Architecture.Native64
                Return WAB64.getAccessibleTextSelectionInfo(vmID, ac, info)
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Selects text in the specified range.
    ''' </summary>
    ''' <param name="vmID">The virtual machine ID.</param>
    ''' <param name="ac">The accessibility context.</param>
    ''' <param name="StartIndex">The zero-based index of the first character
    ''' to be selected (inclusive).</param>
    ''' <param name="EndIndex">The zero-based index of the last character to
    ''' be selected (inclusive).</param>
    ''' <returns>Returns true on success.</returns>
    ''' <remarks>Any existing selection will be replaced.</remarks>
    Public Shared Function selectTextRange(ByVal vmID As Integer, ByVal ac As Long, ByVal StartIndex As Integer, ByVal EndIndex As Integer) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.selectTextRange(vmID, CType(ac, Int32), StartIndex, EndIndex)
            Case Architecture.Wow64_32
                Return WAB32.selectTextRange(vmID, ac, StartIndex, EndIndex)
            Case Architecture.Native64
                Return WAB64.selectTextRange(vmID, ac, StartIndex, EndIndex)
        End Select
        Return False
    End Function

    Public Shared Sub Windows_run()
        Select Case ActiveArchitecture
            Case Architecture.Native32
                WABLegacy.Windows_run()

            Case Architecture.Wow64_32
                ' This is an awkward one - previously, under this architecture, we
                ' supported the legacy, ie. JAB 2.0.0, DLL so if the WAB32 DLL is
                ' not there, we need to check to see if the legacy DLL *is* there
                ' and fall back to that if it's available.
                Try
                    WAB32.Windows_run()
                Catch dnfe As DllNotFoundException
                    Try
                        ' See if the old one is there, if it is, update our active
                        ' architecture so that the following function calls are all
                        ' directed to the correct DLL.
                        WABLegacy.Windows_run()
                        ActiveArchitecture = Architecture.Native32
                        ' And return (ie. successfully)
                        Return
                    Catch
                    End Try
                    ' Ignore the error thrown when testing for WAB-Legacy, and
                    ' rethrow the one raised by the lack of WAB32 - that's the one
                    ' that we want to promote, not the old version.
                    Throw
                End Try

            Case Architecture.Native64
                WAB64.Windows_run()
        End Select
    End Sub

    Public Shared Function isJavaWindow(ByVal hWnd As IntPtr) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.isJavaWindow(hWnd)
            Case Architecture.Wow64_32
                Return WAB32.isJavaWindow(hWnd)
            Case Architecture.Native64
                Return WAB64.isJavaWindow(hWnd)
        End Select
        Return False
    End Function

    Public Shared Function getAccessibleContextFromHWND(ByVal target As IntPtr, ByRef vmID As Integer, ByRef ac As Long) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Dim _ac As Int32
                If WABLegacy.getAccessibleContextFromHWND(target, vmID, _ac) Then
                    ac = _ac
                    Return True
                End If
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleContextFromHWND(target, vmID, ac)
            Case Architecture.Native64
                Return WAB64.getAccessibleContextFromHWND(target, vmID, ac)
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Determines whether two contexts refer to the same thing.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="ac1">The first accessible context of interest.</param>
    ''' <param name="ac2">The second accessible context of interest.</param>
    ''' <returns>Returns true if the two contexts refer to the same
    ''' component.</returns>
    ''' <remarks>Some elements in different parts of the tree of contexts
    ''' can refer to the same underlying UI component. Eg in the BP SwingSet2
    ''' application some of the radio group menu items belong to both the menu
    ''' and to a separate button group, yet they are the same object. See
    ''' bug 2998.</remarks>
    Public Shared Function isSameObject(ByVal vmID As Integer, ByVal ac1 As Long, ByVal ac2 As Long) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.isSameObject(vmID, CType(ac1, Int32), CType(ac2, Int32))
            Case Architecture.Wow64_32
                Return WAB32.isSameObject(vmID, ac1, ac2)
            Case Architecture.Native64
                Return WAB64.isSameObject(vmID, ac1, ac2)
        End Select
        Return False
    End Function

    Public Shared Function getAccessibleParentFromContext(ByVal vmID As Integer, ByVal ac As Long) As Long
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleParentFromContext(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleParentFromContext(vmID, ac)
            Case Architecture.Native64
                Return WAB64.getAccessibleParentFromContext(vmID, ac)
        End Select
        Return 0
    End Function

    Public Shared Function getAccessibleContextInfo(ByVal vmID As Integer, ByVal ac As Long, ByRef info As AccessibleContextInfo) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleContextInfo(vmID, CType(ac, Int32), info)
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleContextInfo(vmID, ac, info)
            Case Architecture.Native64
                Return WAB64.getAccessibleContextInfo(vmID, ac, info)
        End Select
        Return False
    End Function

    Public Shared Sub releaseJavaObject(ByVal vmID As Integer, ByVal ac As Long)
        Select Case ActiveArchitecture
            Case Architecture.Native32
                WABLegacy.releaseJavaObject(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                WAB32.releaseJavaObject(vmID, ac)
            Case Architecture.Native64
                WAB64.releaseJavaObject(vmID, ac)
        End Select
    End Sub

    Public Shared Function doAccessibleActions(ByVal vmID As Integer, ByVal ac As Long, ByRef actionsToDo As AccessibleActionsToDo, ByRef failure As Integer) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.doAccessibleActions(vmID, CType(ac, Int32), actionsToDo, failure)
            Case Architecture.Wow64_32
                Return WAB32.doAccessibleActions(vmID, ac, actionsToDo, failure)
            Case Architecture.Native64
                Return WAB64.doAccessibleActions(vmID, ac, actionsToDo, failure)
        End Select
        Return False
    End Function

    Public Shared Function getMinimumAccessibleValueFromContext(ByVal vmID As Integer, ByVal ac As Long, ByVal value As StringBuilder, ByVal len As Short) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getMinimumAccessibleValueFromContext(vmID, CType(ac, Int32), value, len)
            Case Architecture.Wow64_32
                Return WAB32.getMinimumAccessibleValueFromContext(vmID, ac, value, len)
            Case Architecture.Native64
                Return WAB64.getMinimumAccessibleValueFromContext(vmID, ac, value, len)
        End Select
        Return False
    End Function

    Public Shared Function getCurrentAccessibleValueFromContext(ByVal vmID As Integer, ByVal ac As Long, ByVal value As StringBuilder, ByVal len As Short) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getCurrentAccessibleValueFromContext(vmID, CType(ac, Int32), value, len)
            Case Architecture.Wow64_32
                Return WAB32.getCurrentAccessibleValueFromContext(vmID, ac, value, len)
            Case Architecture.Native64
                Return WAB64.getCurrentAccessibleValueFromContext(vmID, ac, value, len)
        End Select
        Return False
    End Function

    Public Shared Function getMaximumAccessibleValueFromContext(ByVal vmID As Integer, ByVal ac As Long, ByVal value As StringBuilder, ByVal len As Short) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getMaximumAccessibleValueFromContext(vmID, CType(ac, Int32), value, len)
            Case Architecture.Wow64_32
                Return WAB32.getMaximumAccessibleValueFromContext(vmID, ac, value, len)
            Case Architecture.Native64
                Return WAB64.getMaximumAccessibleValueFromContext(vmID, ac, value, len)
        End Select
        Return False
    End Function

    Public Shared Function setTextContents(ByVal vmID As Integer, ByVal ac As Long, ByVal text As String) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.setTextContents(vmID, CType(ac, Int32), text)
            Case Architecture.Wow64_32
                Return WAB32.setTextContents(vmID, ac, text)
            Case Architecture.Native64
                Return WAB64.setTextContents(vmID, ac, text)
        End Select
        Return False
    End Function

    Public Shared Function setTextContents(ByVal vmID As Integer, ByVal ac As Long, ByVal text As IntPtr) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.setTextContents(vmID, CType(ac, Int32), text)
            Case Architecture.Wow64_32
                Return WAB32.setTextContents(vmID, ac, text)
            Case Architecture.Native64
                Return WAB64.setTextContents(vmID, ac, text)
        End Select
        Return False
    End Function


    Public Shared Function getAccessibleActions(ByVal vmID As Integer, ByVal ac As Long, ByVal actions As IntPtr) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleActions(vmID, CType(ac, Int32), actions)
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleActions(vmID, ac, actions)
            Case Architecture.Native64
                Return WAB64.getAccessibleActions(vmID, ac, actions)
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Gets the visible children of the specified JABContext.
    ''' </summary>
    ''' <param name="vmID">The ID of the virtual machine.</param>
    ''' <param name="ac">The Acessible Context</param>
    ''' <param name="startIndex">The index at which to start the search.</param>
    ''' <returns>A list of accessible contexts for the children</returns>
    Public Shared Function getVisibleChildren(ByVal vmID As Integer, ByVal ac As Long, ByVal startIndex As Integer) As List(Of Long)
        Dim children As New List(Of Long)
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Dim info As WABLegacy.VisibleChildrenInfo = Nothing
                If WABLegacy.getVisibleChildren(vmID, CType(ac, Int32), startIndex, info) Then
                    For i As Integer = 0 To Math.Min(info.count, MaxNumChildren) - 1
                        children.Add(info.children(i))
                    Next
                    Return children
                End If
            Case Architecture.Wow64_32, Architecture.Native64
                getVisibleChildren(vmID, ac, children)
                Return children
        End Select
        Return Nothing
    End Function

    ''' <summary>
    ''' Get visible children in a recursive way. This is somewhat similar to what descendtree
    ''' does with the exception that it will only descend visible components, so its faster
    ''' and also not likely to cause a crash. This should in effect do what the JAB api function
    ''' call getVisibleChildren would do, and works around the fact that the api call doesn't
    ''' appear to work on wow64 and native64 modes.
    ''' </summary>
    ''' <param name="vmID">The virtual machine ID.</param>
    ''' <param name="ac">The accessibility context.</param>
    ''' <param name="children">A list of child contexts</param>
    Private Shared Sub getVisibleChildren(ByVal vmID As Integer, ByVal ac As Long, ByVal children As List(Of Long))
        Dim info As WAB.AccessibleContextInfo = Nothing
        If getAccessibleContextInfo(vmID, ac, info) Then
            For i As Integer = 0 To info.childrenCount
                Dim childAC As Long = getAccessibleChildFromContext(vmID, ac, i)
                If childAC <> 0 Then
                    If getAccessibleContextInfo(vmID, childAC, info) Then
                        If info.states_EN_US.Contains("visible") Then
                            If Not alreadyHaveSameObject(vmID, childAC, children) Then
                                children.Add(childAC)
                                getVisibleChildren(vmID, childAC, children)
                            End If
                        Else
                            releaseJavaObject(vmID, childAC)
                        End If
                    End If
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' Returns true if the given context is already in the given list of contexts.
    ''' </summary>
    ''' <param name="vmID">The virtual machine ID.</param>
    ''' <param name="ac">The accessibility context.</param>
    ''' <param name="container">The list of contexts to check</param>
    Private Shared Function alreadyHaveSameObject(ByVal vmid As Integer, ByVal ac As Long, ByVal container As List(Of Long)) As Boolean
        For Each thisAC As Long In container
            If isSameObject(vmid, ac, thisAC) Then
                Return True
            End If
        Next
        Return False
    End Function


    ''' <summary>
    ''' Gets the accessible context of the specified accessible child.
    ''' </summary>
    ''' <param name="vmID">The ID of the virtual machine.</param>
    ''' <param name="ac">The Acessible Context whose child is sought.</param>
    ''' <param name="ChildIndex">The zero-based index of the child
    ''' sought.</param>
    ''' <returns>Returns the accessible context of the specified child.</returns>
    ''' <remarks></remarks>
    Public Shared Function getAccessibleChildFromContext(ByVal vmID As Integer, ByVal ac As Long, ByVal ChildIndex As Integer) As Long
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleChildFromContext(vmID, CType(ac, Int32), ChildIndex)
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleChildFromContext(vmID, ac, ChildIndex)
            Case Architecture.Native64
                Return WAB64.getAccessibleChildFromContext(vmID, ac, ChildIndex)
        End Select
        Return 0
    End Function


    ''' <summary>
    ''' Gets the accessible name of a component, based on the JAWS algorithm.
    ''' </summary>
    ''' <param name="vmID">The ID of the virtual machine.</param>
    ''' <param name="ac">The Accessible Context of the component of interest.</param>
    ''' <param name="VirtualName">Carries back the virtual name of the component.</param>
    ''' <param name="Length">The (maximum) length of the text, expected. This should
    ''' be no greater than  <see cref="MaxJABStringSize">MaxJABStringSize</see>.</param>
    ''' <returns>Returns True on success, False otherwise.</returns>
    ''' <remarks>The advantage of this method over the ordinary name is that (for
    ''' example) if a checkbox is nested in a table without a label then the value
    ''' (true/false) is returned instead.
    ''' 
    ''' However please be aware that this method does canny things such as copying
    ''' the name of a nearby label, if the component of interest has no meaningful
    ''' name of its own. Thus results may not always be as expected</remarks>
    Public Shared Function getVirtualAccessibleName(ByVal vmID As Integer, ByVal ac As Long, ByVal VirtualName As StringBuilder, ByVal Length As Integer) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getVirtualAccessibleName(vmID, CType(ac, Int32), VirtualName, Length)
            Case Architecture.Wow64_32
                Return WAB32.getVirtualAccessibleName(vmID, ac, VirtualName, Length)
            Case Architecture.Native64
                Return WAB64.getVirtualAccessibleName(vmID, ac, VirtualName, Length)
        End Select
        Return False
    End Function


    ''' <summary>
    ''' Attempts to bring the specified java component into focus.
    ''' </summary>
    ''' <param name="vmID">The ID of the virtual machine.</param>
    ''' <param name="ac">The Accessible Context of the component to be focused.</param>
    ''' <returns>Returns True on success.</returns>
    ''' <remarks>The focused component is the one which receives keyboard input.</remarks>
    Public Shared Function requestFocus(ByVal vmID As Integer, ByVal ac As Long) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.requestFocus(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                Return WAB32.requestFocus(vmID, ac)
            Case Architecture.Native64
                Return WAB64.requestFocus(vmID, ac)
        End Select
        Return False
    End Function

    ''' <summary>
    ''' Version Info structure.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Public Structure AccessBridgeVersionInfo
        ''' <summary>
        ''' The virtual machine version
        ''' </summary>
        ''' <remarks>Equivalent to the output of "java -version"</remarks>
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public VMversion As String
        ''' <summary>
        ''' Version of the AccessBridge.class
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public bridgeJavaClassVersion As String
        ''' <summary>
        ''' Version of Java Access Bridge
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public bridgeJavaDLLVersion As String
        ''' <summary>
        ''' Version of Windows Access Bridge
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=ShortJABStringSize)> _
        Public bridgeWinDLLVersion As String
    End Structure

    ''' <summary>
    ''' Gets version information for the windows access bridge.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="info">Carries back version information.</param>
    ''' <returns>Returns True on success.</returns>
    Public Shared Function getVersionInfo(ByVal vmID As Integer, ByRef info As AccessBridgeVersionInfo) As Boolean
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getVersionInfo(vmID, info)
            Case Architecture.Wow64_32
                Return WAB32.getVersionInfo(vmID, info)
            Case Architecture.Native64
                Return WAB64.getVersionInfo(vmID, info)
        End Select
        Return False
    End Function


    ''' <summary>
    ''' Adds a component's child to its selection.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="ac">The accessible context of the component whose selection of
    ''' child-components is to be modified.</param>
    ''' <param name="Index">The zero-based index of the child item to be added.</param>
    ''' <remarks>E.g. used to add a new row to the selection in a listbox.</remarks>
    Public Shared Sub addAccessibleSelectionFromContext(ByVal vmID As Integer, ByVal ac As Long, ByVal Index As Integer)
        Select Case ActiveArchitecture
            Case Architecture.Native32
                WABLegacy.addAccessibleSelectionFromContext(vmID, CType(ac, Int32), Index)
            Case Architecture.Wow64_32
                WAB32.addAccessibleSelectionFromContext(vmID, ac, Index)
            Case Architecture.Native64
                WAB64.addAccessibleSelectionFromContext(vmID, ac, Index)
        End Select
    End Sub

    ''' <summary>
    ''' Clears the selection of child objects from a component.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="ac">The accessible context of the component whose selection of
    ''' child-components is to be modified.</param>
    ''' <remarks>E.g. used to clear the selection in a listbox.</remarks>
    Public Shared Sub clearAccessibleSelectionFromContext(ByVal vmID As Integer, ByVal ac As Long)
        Select Case ActiveArchitecture
            Case Architecture.Native32
                WABLegacy.clearAccessibleSelectionFromContext(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                WAB32.clearAccessibleSelectionFromContext(vmID, ac)
            Case Architecture.Native64
                WAB64.clearAccessibleSelectionFromContext(vmID, ac)
        End Select
    End Sub

    ''' <summary>
    ''' Selects all child-components of the supplied component.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="ac">The accessible context of the component whose selection of
    ''' child-components is to be modified.</param>
    ''' <remarks>E.g. used to select all items in a listbox.</remarks>
    Public Shared Sub selectAllAccessibleSelectionFromContext(ByVal vmID As Integer, ByVal ac As Long)
        Select Case ActiveArchitecture
            Case Architecture.Native32
                WABLegacy.selectAllAccessibleSelectionFromContext(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                WAB32.selectAllAccessibleSelectionFromContext(vmID, ac)
            Case Architecture.Native64
                WAB64.selectAllAccessibleSelectionFromContext(vmID, ac)
        End Select
    End Sub

    ''' <summary>
    ''' Counts the number of child items which are selected.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="ac">The accessible context of the component whose selection of
    ''' child-components is to be counted.</param>
    ''' <returns>Returns the number of child items which are selected.</returns>
    ''' <remarks>E.g. counts the number of items selected in a listbox.</remarks>
    Public Shared Function getAccessibleSelectionCountFromContext(ByVal vmID As Integer, ByVal ac As Long) As Integer
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getAccessibleSelectionCountFromContext(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                Return WAB32.getAccessibleSelectionCountFromContext(vmID, ac)
            Case Architecture.Native64
                Return WAB64.getAccessibleSelectionCountFromContext(vmID, ac)
        End Select
        Return 0
    End Function


    ''' <summary>
    ''' Gets a java component's active descendent.
    ''' </summary>
    ''' <param name="vmID">The virtual machine in use.</param>
    ''' <param name="ac">The accessible context of the component whose descendent is
    ''' sought.</param>
    ''' <returns>Returns the accessible context of the component's active descendent,
    ''' if there is one. Returns zero in the event of an error, or if no such
    ''' descendent exists.</returns>
    ''' <remarks>The active descendent of a component is the descendent which has
    ''' focus, or which is active in the component (e.g. the unique selected treenode
    ''' in treeview is the active descendent of the treeview).</remarks>
    Public Shared Function getActiveDescendent(ByVal vmID As Integer, ByVal ac As Long) As Long
        Select Case ActiveArchitecture
            Case Architecture.Native32
                Return WABLegacy.getActiveDescendent(vmID, CType(ac, Int32))
            Case Architecture.Wow64_32
                Return WAB32.getActiveDescendent(vmID, ac)
            Case Architecture.Native64
                Return WAB64.getActiveDescendent(vmID, ac)
        End Select
        Return 0
    End Function




    ''' <summary>
    ''' Dummy place-holder class for grouping keybinding methods together.
    ''' </summary>
    Public Class AccessibleKeysMethods

        ''' <summary>
        ''' The maximum number of key bindings that can be associated
        ''' with a component.
        ''' </summary>
        Private Const MAX_KEY_BINDINGS As Integer = 10

        ''' <summary>
        ''' A key binding associated with a component
        ''' </summary>
        ''' <remarks></remarks>
        <StructLayout(LayoutKind.Sequential)> _
        Public Structure AccessibleKeyBindingInfo
            ''' <summary>
            ''' The key character
            ''' </summary>
            Public character As Char
            ''' <summary>
            ''' The key modifiers
            ''' </summary>
            Public modifiers As Integer
        End Structure

        ''' <summary>
        ''' All of the key bindings associated with a component
        ''' </summary>
        ''' <remarks></remarks>
        <StructLayout(LayoutKind.Sequential)> _
        Public Structure AccessibleKeyBindings
            ''' <summary>
            ''' Number of key bindings
            ''' </summary>
            Public keyBindingsCount As Integer

            ''' <summary>
            ''' An array of the key bindings associated with the component.
            ''' </summary>
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=MAX_KEY_BINDINGS)> _
            Public keyBindingInfo As AccessibleKeyBindingInfo()
        End Structure

        ''' <summary>
        ''' Gets the key binding associated with a component.
        ''' </summary>
        ''' <param name="vmID">The virtual machine ID.</param>
        ''' <param name="ac">The accessible context of the text container.</param>
        ''' <param name="KeyBindings">Carries back the keybindings.</param>
        ''' <returns>Returns true on success.</returns>
        Public Shared Function getAccessibleKeyBindings(ByVal vmID As Integer, ByVal ac As Long, ByRef KeyBindings As AccessibleKeyBindings) As Boolean
            Select Case ActiveArchitecture
                Case Architecture.Native32
                    Return WABLegacy.getAccessibleKeyBindings(vmID, CType(ac, Int32), KeyBindings)
                Case Architecture.Wow64_32
                    Return WAB32.getAccessibleKeyBindings(vmID, ac, KeyBindings)
                Case Architecture.Native64
                    Return WAB64.getAccessibleKeyBindings(vmID, ac, KeyBindings)
            End Select
            Return False
        End Function
    End Class


    ''' <summary>
    ''' Dummy place-holder class for grouping table methods together.
    ''' </summary>
    Public Class AccessibleTableMethods

        ''' <summary>
        ''' Information about a java table.
        ''' </summary>
        <StructLayout(LayoutKind.Sequential)> _
        Public Structure AccessibleTableInfo
            ''' <summary>
            ''' The accessible context of the table's caption
            ''' </summary>
            Public CaptionAC As Long
            ''' <summary>
            ''' The accessible context of the table's description
            ''' </summary>
            Public SummaryAC As Long
            ''' <summary>
            ''' The number of rows in the table.
            ''' </summary>
            Public RowCount As Integer
            ''' <summary>
            ''' The number of columns in the table.
            ''' </summary>
            Public ColumnCount As Integer
            ''' <summary>
            ''' The AC of the owner of this info.
            ''' </summary>
            ''' <remarks> Such an owner might be a table
            ''' row, a table column header, etc.</remarks>
            Public AccessibleContext As Long
            ''' <summary>
            ''' The accessible context of the table to which
            ''' this info relates.
            ''' </summary>
            Public AccessibleTableAC As Long
        End Structure

        ''' <summary>
        ''' Information about a java table cell.
        ''' </summary>
        <StructLayout(LayoutKind.Sequential)> _
        Public Structure AccessibleTableCellInfo
            ''' <summary>
            ''' The AC of the table cell to which this
            ''' information pertains.
            ''' </summary>
            Public accessibleContext As Long
            ''' <summary>
            ''' The zero-based index of the cell in the
            ''' table's collection of cells.
            ''' </summary>
            Public index As Integer
            ''' <summary>
            ''' The zero-based index of the row in which the
            ''' cell resides.
            ''' </summary>
            Public row As Integer
            ''' <summary>
            ''' The zero-based index of the column in which the
            ''' cell resides.
            ''' </summary>
            Public column As Integer
            ''' <summary>
            ''' The row span of this table cell.
            ''' </summary>
            Public rowExtent As Integer
            ''' <summary>
            ''' The column span of this table cell.
            ''' </summary>
            Public columnExtent As Integer
            ''' <summary>
            ''' Indicates whether the cell is selected.
            ''' </summary>
            Public isSelected As Boolean
        End Structure

        ''' <summary>
        ''' Gets info about an accessible table.
        ''' </summary>
        ''' <param name="vmID">The virtual machine ID.</param>
        ''' <param name="ac">The accessible context of the table</param>
        ''' <param name="TableInfo">Carries back information about the table.</param>
        ''' <returns>Returns true on success.</returns>
        Public Shared Function getAccessibleTableInfo(ByVal vmID As Integer, ByVal ac As Long, ByRef TableInfo As AccessibleTableInfo) As Boolean
            Select Case ActiveArchitecture
                Case Architecture.Native32
                    Dim _tableInfo As WABLegacy.AccessibleTableInfo
                    If WABLegacy.getAccessibleTableInfo(vmID, CType(ac, Int32), _tableInfo) Then
                        TableInfo.AccessibleContext = _tableInfo.AccessibleContext
                        TableInfo.AccessibleTableAC = _tableInfo.AccessibleTableAC
                        TableInfo.CaptionAC = _tableInfo.CaptionAC
                        TableInfo.ColumnCount = _tableInfo.ColumnCount
                        TableInfo.RowCount = _tableInfo.RowCount
                        TableInfo.SummaryAC = _tableInfo.SummaryAC
                        Return True
                    End If
                Case Architecture.Wow64_32
                    Dim _tableInfo As WAB32.AccessibleTableInfo
                    If WAB32.getAccessibleTableInfo(vmID, CType(ac, Int32), _tableInfo) Then
                        TableInfo.AccessibleContext = _tableInfo.AccessibleContext
                        TableInfo.AccessibleTableAC = _tableInfo.AccessibleTableAC
                        TableInfo.CaptionAC = _tableInfo.CaptionAC
                        TableInfo.ColumnCount = _tableInfo.ColumnCount
                        TableInfo.RowCount = _tableInfo.RowCount
                        TableInfo.SummaryAC = _tableInfo.SummaryAC
                        Return True
                    End If
                Case Architecture.Native64
                    Dim _tableInfo As WAB64.AccessibleTableInfo
                    If WAB64.getAccessibleTableInfo(vmID, CType(ac, Int32), _tableInfo) Then
                        TableInfo.AccessibleContext = _tableInfo.AccessibleContext
                        TableInfo.AccessibleTableAC = _tableInfo.AccessibleTableAC
                        TableInfo.CaptionAC = _tableInfo.CaptionAC
                        TableInfo.ColumnCount = _tableInfo.ColumnCount
                        TableInfo.RowCount = _tableInfo.RowCount
                        TableInfo.SummaryAC = _tableInfo.SummaryAC
                        Return True
                    End If
            End Select
            Return False
        End Function

        ''' <summary>
        ''' Gets info about an accessible table cell.
        ''' </summary>
        ''' <param name="vmID">The virtual machine ID.</param>
        ''' <param name="ac">The accessible context of the table</param>
        ''' <param name="ColumnIndex">The zero-based index of the column
        ''' in which the cell of interest resides.</param>
        ''' <param name="RowIndex">The zero-based index of the row
        ''' in which the cell of interest resides.</param>
        ''' <param name="CellInfo">Carries back information about the table cell.</param>
        ''' <returns>Returns true on success.</returns>
        Public Shared Function getAccessibleTableCellInfo(ByVal vmID As Integer, ByVal ac As Long, ByVal RowIndex As Integer, ByVal ColumnIndex As Integer, ByRef CellInfo As AccessibleTableCellInfo) As Boolean
            Select Case ActiveArchitecture
                Case Architecture.Native32
                    Dim _cellinfo As WABLegacy.AccessibleTableCellInfo
                    If WABLegacy.getAccessibleTableCellInfo(vmID, CType(ac, Int32), RowIndex, ColumnIndex, _cellinfo) Then
                        CellInfo.accessibleContext = _cellinfo.accessibleContext
                        CellInfo.column = _cellinfo.column
                        CellInfo.columnExtent = _cellinfo.columnExtent
                        CellInfo.index = _cellinfo.index
                        CellInfo.isSelected = _cellinfo.isSelected
                        CellInfo.row = _cellinfo.row
                        CellInfo.rowExtent = _cellinfo.rowExtent
                        Return True
                    End If
                Case Architecture.Wow64_32
                    Dim _cellinfo As WAB32.AccessibleTableCellInfo
                    If WAB32.getAccessibleTableCellInfo(vmID, ac, RowIndex, ColumnIndex, _cellinfo) Then
                        CellInfo.accessibleContext = _cellinfo.accessibleContext
                        CellInfo.column = _cellinfo.column
                        CellInfo.columnExtent = _cellinfo.columnExtent
                        CellInfo.index = _cellinfo.index
                        CellInfo.isSelected = _cellinfo.isSelected
                        CellInfo.row = _cellinfo.row
                        CellInfo.rowExtent = _cellinfo.rowExtent
                        Return True
                    End If
                Case Architecture.Native64
                    Dim _cellinfo As WAB64.AccessibleTableCellInfo
                    If WAB64.getAccessibleTableCellInfo(vmID, ac, RowIndex, ColumnIndex, _cellinfo) Then
                        CellInfo.accessibleContext = _cellinfo.accessibleContext
                        CellInfo.column = _cellinfo.column
                        CellInfo.columnExtent = _cellinfo.columnExtent
                        CellInfo.index = _cellinfo.index
                        CellInfo.isSelected = _cellinfo.isSelected
                        CellInfo.row = _cellinfo.row
                        CellInfo.rowExtent = _cellinfo.rowExtent
                        Return True
                    End If
            End Select
            Return False
        End Function

        ''' <summary>
        ''' Gets the column header of an accessible table.
        ''' </summary>
        ''' <param name="vmID">The virtual machine ID.</param>
        ''' <param name="ac">The accessible context of the table column header</param>
        ''' <param name="TableInfo">Carries back the table info of the table
        ''' which constitutes the table header. This is usually a table of one
        ''' row, and has the same number of columns as the original table.</param>
        ''' <returns>Returns true on success.</returns>
        Public Shared Function getAccessibleTableColumnHeader(ByVal vmID As Integer, ByVal ac As Long, ByRef TableInfo As AccessibleTableInfo) As Boolean
            Select Case ActiveArchitecture
                Case Architecture.Native32
                    Dim _tableInfo As WABLegacy.AccessibleTableInfo
                    If WABLegacy.getAccessibleTableColumnHeader(vmID, CType(ac, Int32), _tableInfo) Then
                        TableInfo.AccessibleContext = _tableInfo.AccessibleContext
                        TableInfo.AccessibleTableAC = _tableInfo.AccessibleTableAC
                        TableInfo.CaptionAC = _tableInfo.CaptionAC
                        TableInfo.ColumnCount = _tableInfo.ColumnCount
                        TableInfo.RowCount = _tableInfo.RowCount
                        TableInfo.SummaryAC = _tableInfo.SummaryAC
                        Return True
                    End If
                Case Architecture.Wow64_32
                    Dim _tableInfo As WAB32.AccessibleTableInfo
                    If WAB32.getAccessibleTableColumnHeader(vmID, CType(ac, Int32), _tableInfo) Then
                        TableInfo.AccessibleContext = _tableInfo.AccessibleContext
                        TableInfo.AccessibleTableAC = _tableInfo.AccessibleTableAC
                        TableInfo.CaptionAC = _tableInfo.CaptionAC
                        TableInfo.ColumnCount = _tableInfo.ColumnCount
                        TableInfo.RowCount = _tableInfo.RowCount
                        TableInfo.SummaryAC = _tableInfo.SummaryAC
                        Return True
                    End If
                Case Architecture.Native64
                    Dim _tableInfo As WAB64.AccessibleTableInfo
                    If WAB64.getAccessibleTableColumnHeader(vmID, CType(ac, Int32), _tableInfo) Then
                        TableInfo.AccessibleContext = _tableInfo.AccessibleContext
                        TableInfo.AccessibleTableAC = _tableInfo.AccessibleTableAC
                        TableInfo.CaptionAC = _tableInfo.CaptionAC
                        TableInfo.ColumnCount = _tableInfo.ColumnCount
                        TableInfo.RowCount = _tableInfo.RowCount
                        TableInfo.SummaryAC = _tableInfo.SummaryAC
                        Return True
                    End If
            End Select
            Return False
        End Function

        ''' <summary>
        ''' Counts the number of selected rows in a table.
        ''' </summary>
        ''' <param name="vmID">The virtual machine ID.</param>
        ''' <param name="ac">The accessible context of the table</param>
        ''' <returns>Gets the number of selected rows.</returns>
        Public Shared Function getAccessibleTableRowSelectionCount(ByVal vmID As Integer, ByVal ac As Long) As Integer
            Select Case ActiveArchitecture
                Case Architecture.Native32
                    Return WABLegacy.getAccessibleTableRowSelectionCount(vmID, CType(ac, Int32))
                Case Architecture.Wow64_32
                    Return WAB32.getAccessibleTableRowSelectionCount(vmID, ac)
                Case Architecture.Native64
                    Return WAB64.getAccessibleTableRowSelectionCount(vmID, ac)
            End Select
            Return 0
        End Function


        ''' <summary>
        ''' Determines whether a table row is selected.
        ''' </summary>
        ''' <param name="vmID">The virtual machine ID.</param>
        ''' <param name="ac">The accessible context of the table</param>
        ''' <param name="RowIndex">The zero-based index of the row of interest</param>
        ''' <returns>Returns true if the specified row is selected,
        ''' false otherwise.</returns>
        Public Shared Function isAccessibleTableRowSelected(ByVal vmID As Integer, ByVal ac As Long, ByVal RowIndex As Integer) As Boolean
            Select Case ActiveArchitecture
                Case Architecture.Native32
                    Return WABLegacy.isAccessibleTableRowSelected(vmID, CType(ac, Int32), RowIndex)
                Case Architecture.Wow64_32
                    Return WAB32.isAccessibleTableRowSelected(vmID, ac, RowIndex)
                Case Architecture.Native64
                    Return WAB64.isAccessibleTableRowSelected(vmID, ac, RowIndex)
            End Select
            Return False
        End Function
    End Class

    Public Interface IJavaMouseHook
        Inherits IDisposable
        ReadOnly Property CurrentContext() As JABContext
    End Interface

    Public Shared Function SetupMouseHook() As IJavaMouseHook
        Dim hook As IJavaMouseHook = Nothing
        Select Case ActiveArchitecture
            Case Architecture.Native32
                hook = New WABLegacy.JavaMouseHook
            Case Architecture.Wow64_32
                hook = New WAB32.JavaMouseHook
            Case Architecture.Native64
                hook = New WAB64.JavaMouseHook
        End Select
        Return hook
    End Function
End Class
