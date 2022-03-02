Imports AutomateControls

Public Class clsWindowHighlighter : Implements IDisposable

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Timer class to handle asymmetric flashes - being 'on' for a longer period
    ''' than it is 'off'
    ''' </summary>
    Private Class FlashTimer : Inherits Timer
        Private Const mOnInterval As Integer = 400
        Private Const mOffInterval As Integer = 150
        Private mOn As Boolean

        Public Sub New()
            mOn = False
        End Sub

        Protected Overrides Sub OnTick(ByVal e As EventArgs)
            mOn = Not mOn
            Interval = CInt(IIf(mOn, mOnInterval, mOffInterval))
            MyBase.OnTick(e)
        End Sub

        Public Property IsOn() As Boolean
            Get
                Return mOn
            End Get
            Set(ByVal value As Boolean)
                mOn = value
                Interval = CInt(IIf(mOn, mOnInterval, mOffInterval))
            End Set
        End Property
    End Class

#End Region

#Region " Member Variables "

    ' The persistently maintained highlighter window
    Private WithEvents mFrame As New HighlighterWindow()

    ' Timer for handling deferred hiding of the highlight
    Private WithEvents mShowTimer As New Timer()

    ' Timer for handling the flashing of the highlight
    Private WithEvents mFlashTimer As New FlashTimer()

#End Region

#Region " Properties "

    ''' <summary>
    ''' The colour to use to highlight a window
    ''' </summary>
    Public Property HighlightColour() As Color
        Get
            Return mFrame.HighlightColor
        End Get
        Set(ByVal value As Color)
            mFrame.HighlightColor = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Hides any highlighting that is currently visible.
    ''' </summary>
    ''' <remarks>The hiding will be excuted on the same thread
    ''' on which this object was created.</remarks>
    Public Sub HideFrame()
        If mFrame.InvokeRequired Then
            mFrame.Invoke(New MethodInvoker(AddressOf HideFrame))
            Return
        End If
        mShowTimer.Enabled = False
        mFlashTimer.Enabled = False
        mFrame.Visible = False
    End Sub

    ''' <summary>
    ''' Hides any highlighting that is currently visible.
    ''' </summary>
    ''' <param name="Delay">Specifies a delay before the highlighting is actually
    ''' hidden.</param>
    ''' <remarks>The hiding will be excuted on the same thread on which this object
    ''' was created.</remarks>
    Public Sub HideFrame(ByVal Delay As TimeSpan)
        mShowTimer.Interval = CInt(Delay.TotalMilliseconds)
        mShowTimer.Enabled = True
    End Sub

    ''' <summary>
    ''' Shows a new frame highlighting a given rectangle for a specific amount of
    ''' time. This is done asynchronously - there must be some message pumping
    ''' active on this thread for the asynchronous highlighting to work (either
    ''' through a .net-managed message pump or more directly with
    ''' modWin32.MessagePump() or Application.DoEvents())
    ''' </summary>
    ''' <param name="r">The rectangle to highlight</param>
    ''' <param name="time">The time to show the frame for</param>
    ''' <remarks>This returns immediately, it does not wait for the window to stop
    ''' displaying first.</remarks>
    Public Shared Sub ShowNewFrameFor( _
     ByVal r As Rectangle, ByVal time As TimeSpan)
        HighlighterWindow.ShowForAsync(Nothing, r, time)
    End Sub

    ''' <summary>
    ''' Shows a frame at the specified rectangle.
    ''' </summary>
    ''' <param name="r">The rectangle to show.</param>
    Public Sub ShowFrame(ByVal r As Rectangle)
        ShowFrame(r, TimeSpan.Zero, False)
    End Sub

    ''' <summary>
    ''' Shows a frame at the given rectangle using the given colour, setting the
    ''' highlight colour from the given value first.
    ''' </summary>
    ''' <param name="r">The rectangle defining the area which should be highlighted.
    ''' </param>
    ''' <param name="c">The colour to use for the highlight colour</param>
    Public Sub ShowFrame(ByVal r As Rectangle, ByVal c As Color)
        mFrame.HighlightColor = c
        ShowFrame(r)
    End Sub

    ''' <summary>
    ''' Shows a frame at the specified rectangle.
    ''' </summary>
    ''' <param name="r">The rectangle at which a frame should be shown. The frame
    ''' will be shown around the outside of the supplied rectangle.</param>
    ''' <param name="durn">The time for which the frame should be made visible.
    ''' If TimeSpan.Zero then stays until HideFrame is next called.</param>
    ''' <param name="flashFrame">Determines whether the frame should flash/flicker to
    ''' attract the user's attention.</param>
    Public Sub ShowFrame( _
     ByVal r As Rectangle, ByVal durn As TimeSpan, ByVal flashFrame As Boolean)
        ' We need to ensure we do this on the appropriate thread
        If mFrame.InvokeRequired Then
            mFrame.Invoke(New Action(Of Rectangle, TimeSpan, Boolean)( _
             AddressOf ShowFrame), r, durn, flashFrame)
            Return
        End If

        mShowTimer.Enabled = False
        mFlashTimer.Enabled = False
        If r <> mFrame.HighlightScreenRect _
         OrElse Not mFrame.Visible Then
            mFrame.HighlightScreenRect = r
            mFrame.Visible = True
        End If
        If durn > TimeSpan.Zero Then
            mShowTimer.Interval = CInt(durn.TotalMilliseconds)
            mShowTimer.Enabled = True
        End If
        If flashFrame Then mFlashTimer.Enabled = True

    End Sub

    ''' <summary>
    ''' Handles the 'Show' timer ticking, meaning that the frame should stop showing
    ''' the highlight rectangle
    ''' </summary>
    Private Sub HandleShowTimerTick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mShowTimer.Tick
        mShowTimer.Enabled = False
        mFrame.Visible = False
    End Sub

    ''' <summary>
    ''' Handles the 'flash' timer ticking, meaning that the frame visibility should
    ''' be toggled.
    ''' </summary>
    Private Sub HandleFlashTimerTick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mFlashTimer.Tick
        mFrame.Visible = (mFlashTimer.IsOn)
    End Sub

    ''' <summary>
    ''' Ends any highlighting and disposes of the UI objects used to display it
    ''' </summary>
    Private Sub DisposeUI()
        If mFrame.InvokeRequired Then _
         mFrame.Invoke(New Action(AddressOf DisposeUI)) : Return

        If mFrame IsNot Nothing Then _
        mFlashTimer.Dispose()

        mShowTimer.Dispose()
        mFrame.Dispose()

    End Sub

#End Region

#Region " IDisposable Implementation "

    Private mIsDisposed As Boolean = False

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not mIsDisposed Then
            If disposing Then DisposeUI()
        End If
        mIsDisposed = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
