Namespace Operations
    ''' <summary>
    ''' Provides methods for interacting with windows
    ''' </summary>
    Public Interface IWindowOperationsProvider
        ''' <summary>
        ''' Attempts to force the window with the given handle to the foreground.
        ''' </summary>
        ''' <param name="windowHandle">
        ''' The window handle for the window which should be made into a foreground window.
        ''' If the given window is already the foreground window, according to 
        ''' modWin32.GetForegroundWindow, this method has no effect.
        ''' </param>
        ''' <remarks>
        ''' Throws OperationFailedException if the forcing of the foreground window
        ''' failed for any reason
        ''' </remarks>
        Sub ForceForeground(ByVal windowHandle As IntPtr)
    End Interface
End Namespace