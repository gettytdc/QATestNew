Imports System.Runtime.InteropServices

Class WABLegacy

    <StructLayout(LayoutKind.Sequential)> _
     Public Structure AccessibleTableInfo
        Public CaptionAC As Int32
        Public SummaryAC As Int32
        Public RowCount As Integer
        Public ColumnCount As Integer
        Public AccessibleContext As Int32
        Public AccessibleTableAC As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
     Public Structure AccessibleTableCellInfo
        Public accessibleContext As Int32
        Public index As Int32
        Public row As Int32
        Public column As Int32
        Public rowExtent As Int32
        Public columnExtent As Int32
        Public isSelected As Boolean
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VisibleChildrenInfo
        Public count As Int32
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=WAB.MaxNumChildren)> _
        Public children() As Int32
    End Structure

    Public Class JavaMouseHook
        Implements WAB.IJavaMouseHook

        Public Sub New()
            mouseEnteredFP = New MouseDelegate(AddressOf HandleMouse)
            setMouseEnteredFP(mouseEnteredFP)

            mouseExitedFP = New MouseDelegate(AddressOf HandleMouse)
            setMouseExitedFP(mouseExitedFP)
        End Sub

        Private mouseEnteredFP As MouseDelegate
        Private mouseExitedFP As MouseDelegate

        Private Sub HandleMouse(ByVal vmID As Int32, ByVal jevent As Int32, ByVal ac As Int32)
            mContext = New JABContext(ac, vmID)
        End Sub

        Public ReadOnly Property CurrentContext() As JABContext Implements WAB.IJavaMouseHook.CurrentContext
            Get
                Return mContext
            End Get
        End Property
        Public mContext As JABContext

        Private disposedValue As Boolean = False
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                setMouseEnteredFP(Nothing)
                setMouseExitedFP(Nothing)
            End If
            Me.disposedValue = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    <UnmanagedFunctionPointer(CallingConvention.Cdecl)> _
    Private Delegate Sub MouseDelegate(ByVal vmID As Int32, ByVal jevent As Int32, ByVal ac As Int32)

    <DllImport("WindowsAccessBridge.dll", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Sub setMouseEnteredFP(ByVal fp As MouseDelegate)
    End Sub

    <DllImport("WindowsAccessBridge.dll", CallingConvention:=CallingConvention.Cdecl)> _
    Private Shared Sub setMouseExitedFP(ByVal fp As MouseDelegate)
    End Sub

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleTextInfo(ByVal vmID As Int32, ByVal ac As Int32, ByRef Info As WAB.AccessibleTextInfo, ByVal LocationX As Int32, ByVal LocationY As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleTextRange(ByVal vmID As Int32, ByVal ac As Int32, ByVal StartIndex As Int32, ByVal EndIndex As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal Text As StringBuilder, ByVal Length As Int16) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleTextSelectionInfo(ByVal vmID As Int32, ByVal ac As Int32, ByRef info As WAB.AccessibleTextSelectionInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function selectTextRange(ByVal vmID As Int32, ByVal ac As Int32, ByVal StartIndex As Int32, ByVal EndIndex As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Sub Windows_run()
    End Sub

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function isJavaWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function getAccessibleContextFromHWND(ByVal target As IntPtr, ByRef vmID As Int32, ByRef ac As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function isSameObject(ByVal vmID As Int32, ByVal ac1 As Int32, ByVal ac2 As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleParentFromContext(ByVal vmID As Int32, ByVal ac As Int32) As Int32
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleContextInfo(ByVal vmID As Int32, ByVal ac As Int32, ByRef info As WAB.AccessibleContextInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Sub releaseJavaObject(ByVal vmID As Int32, ByVal ac As Int32)
    End Sub

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function doAccessibleActions(ByVal vmID As Int32, ByVal ac As Int32, ByRef actionsToDo As WAB.AccessibleActionsToDo, ByRef failure As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getMinimumAccessibleValueFromContext(ByVal vmID As Int32, ByVal ac As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal value As StringBuilder, ByVal len As Int16) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
     Public Shared Function getCurrentAccessibleValueFromContext(ByVal vmID As Int32, ByVal ac As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal value As StringBuilder, ByVal len As Int16) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getMaximumAccessibleValueFromContext(ByVal vmID As Int32, ByVal ac As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal value As StringBuilder, ByVal len As Int16) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function setTextContents(ByVal vmID As Int32, ByVal ac As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal text As String) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function setTextContents(ByVal vmID As Int32, ByVal ac As Int32, ByVal text As IntPtr) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleActions(ByVal vmID As Int32, ByVal ac As Int32, ByVal actions As IntPtr) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getVisibleChildren(ByVal vmID As Int32, ByVal ac As Int32, ByVal startIndex As Int32, ByRef info As VisibleChildrenInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleChildFromContext(ByVal vmID As Int32, ByVal ac As Int32, ByVal ChildIndex As Int32) As Int32
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getVirtualAccessibleName(ByVal vmID As Int32, ByVal ac As Int32, <MarshalAs(UnmanagedType.LPWStr)> ByVal VirtualName As StringBuilder, ByVal Length As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function requestFocus(ByVal vmID As Int32, ByVal ac As Int32) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getVersionInfo(ByVal vmID As Int32, ByRef info As WAB.AccessBridgeVersionInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Sub addAccessibleSelectionFromContext(ByVal vmID As Int32, ByVal ac As Int32, ByVal Index As Int32)
    End Sub

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Sub clearAccessibleSelectionFromContext(ByVal vmID As Int32, ByVal ac As Int32)
    End Sub

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Sub selectAllAccessibleSelectionFromContext(ByVal vmID As Int32, ByVal ac As Int32)
    End Sub

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleSelectionCountFromContext(ByVal vmID As Int32, ByVal ac As Int32) As Int32
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getActiveDescendent(ByVal vmID As Int32, ByVal ac As Int32) As Int32
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
    Public Shared Function getAccessibleKeyBindings(ByVal vmID As Int32, ByVal ac As Int32, ByRef KeyBindings As WAB.AccessibleKeysMethods.AccessibleKeyBindings) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
     Public Shared Function getAccessibleTableInfo(ByVal vmID As Int32, ByVal ac As Int32, ByRef TableInfo As AccessibleTableInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
     Public Shared Function getAccessibleTableCellInfo(ByVal vmID As Int32, ByVal ac As Int32, ByVal RowIndex As Int32, ByVal ColumnIndex As Int32, ByRef CellInfo As AccessibleTableCellInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
     Public Shared Function getAccessibleTableColumnHeader(ByVal vmID As Int32, ByVal ac As Int32, ByRef TableInfo As AccessibleTableInfo) As Boolean
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
     Public Shared Function getAccessibleTableRowSelectionCount(ByVal vmID As Int32, ByVal ac As Int32) As Int32
    End Function

    <DllImport("WindowsAccessBridge.dll", CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.Cdecl)> _
      Public Shared Function isAccessibleTableRowSelected(ByVal vmID As Int32, ByVal ac As Int32, ByVal RowIndex As Int32) As Boolean
    End Function
End Class
