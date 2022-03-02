Imports System.Threading

Namespace Classes.UserInterface
    ''' <summary>
    ''' Provides control over when the loading form is closed
    ''' </summary>
    ''' <seealso cref="System.IDisposable" />
    Public Class LoadingFormToken : Implements IDisposable

        ''' <summary>
        ''' The cancellation token source used for the loading form
        ''' </summary>
        Private mCancellationTokenSource As CancellationTokenSource

        ''' <summary>
        ''' Initializes a new instance of the <see cref="LoadingFormToken"/> class.
        ''' </summary>
        ''' <param name="cancellationTokenSource">
        ''' The cancellation token source used for the loading form
        ''' </param>
        Public Sub New(cancellationTokenSource As CancellationTokenSource)
            mCancellationTokenSource = cancellationTokenSource
        End Sub

        ''' <summary>
        ''' Disposes this LoadingFormToken, triggering closing of the loading
        ''' form
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            If mCancellationTokenSource Is Nothing Then
                Throw New ObjectDisposedException("This LoadingFormToken has already been disposed")
            Else
                mCancellationTokenSource.Cancel()
                mCancellationTokenSource.Dispose
                mCancellationTokenSource = Nothing
            End If
        End Sub
    End Class
End Namespace