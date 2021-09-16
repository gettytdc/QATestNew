Imports System.Windows.Forms

''' <summary>
''' A window which dispatches events on receipt of windows messages.
''' </summary>
Public Class clsEventDispatchingWindow
    Inherits NativeWindow

    Public Sub New()
        MyBase.CreateHandle(New CreateParams)
    End Sub

    ''' <summary>
    ''' Event handler definition.
    ''' </summary>
    ''' <param name="m">The message received.</param>
    ''' <param name="handled">If set to true by event handler, then message
    ''' is ignored; otherwise native wndproc will be used to process message.</param>
    Public Delegate Sub WndProcEventHandler(ByRef m As Message, ByRef handled As Boolean)

    ''' <summary>
    ''' Event raised when message is received.
    ''' </summary>
    Public Event WindowsMessageReceived As WndProcEventHandler

    ''' <summary>
    ''' Processes windows messages.
    ''' </summary>
    ''' <param name="m">The message to be processed.</param>
    Protected Overrides Sub WndProc(ByRef m As Message)
        Dim handledByObserver As Boolean = False
        RaiseEvent WindowsMessageReceived(m, handledByObserver)
        If Not handledByObserver Then
            MyBase.WndProc(m)
        End If
    End Sub

End Class
