Imports System.Threading
Imports NLog

Namespace Classes.UserInterface

    ''' <summary>
    ''' Coordinates display of a form to indicate that the application is unavailable.
    ''' This can be used to prevent the application from appearing to freeze when data 
    ''' is being loaded or the UI thread is blocked during display of a large volume
    ''' of data.
    ''' </summary>
    Public Class LoadingFormHelper

        Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

        ''' <summary>
        ''' The time we will wait before displaying the loading form
        ''' </summary>
        Private Shared ReadOnly InitialDelay As TimeSpan = TimeSpan.FromSeconds(2)

        ''' <summary>
        ''' The minimum time to display the loading form
        ''' </summary>
        Private Shared ReadOnly MinimumDisplayTime As TimeSpan = TimeSpan.FromSeconds(1)

        ''' <summary>
        ''' Shows the loading form. The form will be closed when the caller indicates 
        ''' that the action it is waiting for has completed via the LoadingFormToken 
        ''' object that is returned from this function. The form is shown after a 
        ''' short delay and will not be shown if the action completes within this
        ''' period. 
        ''' </summary>
        ''' <returns>An object use to control closing of the form</returns>
        Public Shared Function ShowForm() As LoadingFormToken

            Dim cancellationTokenSource = New CancellationTokenSource()
            Dim token = cancellationTokenSource.Token
            ShowWithDelay(token)
            Return New LoadingFormToken(cancellationTokenSource)

        End Function

        ''' <summary>
        ''' Shows the loading form whilst the given action is executing. The form is shown
        ''' after a short delay and will not be shown if the action completes within this
        ''' period.
        ''' </summary>
        ''' <param name="action">The action to execute</param>
        Public Shared Sub ShowForm(action As Action)
            Using ShowForm()
                action()
            End Using
        End Sub


        Private Shared Sub ShowWithDelay(token As CancellationToken)

            Task.Run(
                Async Function()

                    Await Task.Delay(InitialDelay)

                    If Not token.IsCancellationRequested Then

                        Dim loadingForm As frmLoading = Await OpenForm()

                        Dim watch = Stopwatch.StartNew
                        Await WaitForCancel(token)
                        watch.Stop()

                        ' Ensure form displayed for minimum time
                        Dim millisecondsRemaining =
                            CType(MinimumDisplayTime.TotalMilliseconds - watch.ElapsedMilliseconds, Integer)
                        If millisecondsRemaining > 0 Then
                            Await Task.Delay(millisecondsRemaining)
                        End If

                        ' As form display is run on a separate thread, a race 
                        ' condition might result in the form being in a state 
                        ' where an error occurs when we try to close it.
                        Try
                            loadingForm.Invoke(Sub() loadingForm.Close())
                        Catch ex As Exception
                            Log.Error(ex, "Error closing loading form")
                        Finally
                            loadingForm.Invoke(Sub() loadingForm.Dispose())
                        End Try

                    End If
                End Function)

        End Sub

        ''' <summary>
        ''' Creates a new instance of the loading form and runs it on a new thread
        ''' </summary>
        ''' <returns></returns>
        Private Shared Async Function OpenForm() As Task(Of frmLoading)

            Dim completionSource As New TaskCompletionSource(Of frmLoading)

            ' Create and run the form on a new thread. Note that Application.Run 
            ' blocks this new thread until the form is closed so we do not await 
            ' completion (that's why warning BC42358 is disabled).

#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
            Dim taskFactory = New TaskFactory()
            taskFactory.StartNew(
                Sub()
                    Dim loadingForm As New frmLoading
                    completionSource.SetResult(loadingForm)
                    Application.Run(loadingForm)
                End Sub,
                TaskCreationOptions.LongRunning)
#Enable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
            Return Await completionSource.Task

        End Function

        ''' <summary>
        ''' Creates a task that completes when a CancellationToken has been cancelled
        ''' </summary>
        Private Shared Async Function WaitForCancel(token As CancellationToken) As Task(Of Boolean)

            Dim completionSource As New TaskCompletionSource(Of Boolean)
            Using token.Register(Sub()
                                     completionSource.SetResult(True)
                                 End Sub)
                Return Await completionSource.Task
            End Using

        End Function


    End Class
End Namespace