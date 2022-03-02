
Imports System.Runtime.InteropServices

''' <summary>
''' GDI32.dll Helper class.
''' </summary>
''' <remarks></remarks>
Friend Class clsGDI32

    Public Shared SRCCOPY As Integer = &HCC0020

    <DllImport("gdi32", SetLastError:=True)> _
    Public Shared Function BitBlt(ByVal hObject As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hObjectSource As IntPtr, ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As Integer) As Boolean
    End Function

    <DllImport("gdi32", SetLastError:=True)> _
    Public Shared Function CreateCompatibleBitmap(ByVal hDC As IntPtr, ByVal nWidth As Integer, ByVal nHeight As Integer) As IntPtr
    End Function

    <DllImport("gdi32", SetLastError:=True)> _
    Public Shared Function CreateCompatibleDC(ByVal hDC As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32", SetLastError:=True)> _
    Public Shared Function DeleteDC(ByVal hDC As IntPtr) As Boolean
    End Function

    <DllImport("gdi32", SetLastError:=True)> _
    Public Shared Function DeleteObject(ByVal hObject As IntPtr) As Boolean
    End Function

    <DllImport("gdi32", SetLastError:=True)> _
    Public Shared Function SelectObject(ByVal hDC As IntPtr, ByVal hObject As IntPtr) As IntPtr
    End Function

End Class
