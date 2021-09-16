Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks
Imports BluePrism.Core.Utility

Namespace BackgroundJobs.Monitoring

    ''' <summary>
    ''' Extensions for BackgroundJob monitoring
    ''' </summary>
    Public Module BackgroundJobMonitoringExtensions

        ''' <summary>
        ''' Waits for a background job. This function runs asynchronously, returning a result when the 
        ''' job has completed or monitoring stops due to an error. This is a convenience extension 
        ''' designed to minimise boiler plate code needed to setup job monitoring.
        ''' </summary>
        ''' <param name="job">The background job</param>
        ''' <param name="notifier">Signals when updates are available for the job (when running in-process 
        ''' or server callbacks are supported)</param>
        ''' <param name="update">Action to execute when job is running and updated data is available 
        ''' about progress of the job</param>
        ''' <param name="updateInterval">Interval at which to check for updates (optional - defaults to 
        ''' 1 second)</param>
        ''' <param name="timeout">The maximum duration from when the job was last updated that we will 
        ''' wait for a new update before timing out (optional - defaults to 10 minutes)</param>
        ''' <returns>A task representing ongoing monitoring for completion of the job. When complete the task
        ''' result contains data about the outcome of the job.</returns>
        <Extension>
        Public Async Function Wait(job As BackgroundJob,
                             notifier As BackgroundJobNotifier,
                             update As Action(Of BackgroundJobData),
                             Optional updateInterval As TimeSpan? = Nothing,
                             Optional timeout As TimeSpan? = Nothing) _
            As Task(Of BackgroundJobResult)

            Dim completionSource As New TaskCompletionSource(Of BackgroundJobResult)

            If updateInterval Is Nothing Then updateInterval = TimeSpan.FromSeconds(1)
            If timeout Is Nothing Then timeout = TimeSpan.FromMinutes(10)

            Using trigger As New UpdateTrigger(updateInterval.Value, notifier)
                Using jobMonitor As New BackgroundJobMonitor(job, gSv, trigger,
                                                             timeout.Value, New SystemClockWrapper())

                    AddHandler jobMonitor.Running,
                        Sub(sender, args)
                            update(args.Data)
                        End Sub
                    AddHandler jobMonitor.Done,
                        Sub(sender, args)
                            completionSource.SetResult(args.Result)
                        End Sub
                    jobMonitor.Start()
                    trigger.Start()

                    Return Await completionSource.Task

                End Using
            End Using

        End Function

    End Module
End Namespace