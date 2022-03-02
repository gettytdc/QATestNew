Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Scheduling
Imports BluePrism.AutomateAppCore.Resources

''' <summary>
''' A background thread scheduler which contains a resource connection manager,
''' logged in as the <c>Scheduler</c> system user.
''' The manager is available whenever the scheduler is alive - ie. running or paused.
''' It is closed and removed when the scheduler is stopped.
''' </summary>
Public Class AutomateScheduler : Inherits BackgroundThreadScheduler

    ' The resource manager owned by this scheduler. It is started and stopped when
    ' the scheduler is started and stopped. While the scheduler is idle (ie. stopped,
    ' not paused) this will be null.
    Private mResourceManager As IResourceConnectionManager

    ''' <summary>
    ''' The user that the scheduler will be running as
    ''' </summary>
    Private ReadOnly mUser As IUser

    Public Sub New(user As IUser, resourceManager As IResourceConnectionManager, name As String)
        Me.New(user, resourceManager, name, New DatabaseBackedScheduleStore(gSv))
    End Sub

    Private Sub New(user As IUser, resourceManager As IResourceConnectionManager,
                    name As String, ByVal store As DatabaseBackedScheduleStore)
        MyBase.New(name, store)
        mResourceManager = resourceManager
        mUser = user
    End Sub


    ''' <summary>
    ''' Gets or sets the DB backed schedule store in this scheduler.
    ''' </summary>
    Public Overloads ReadOnly Property Store() As DatabaseBackedScheduleStore
        Get
            Return DirectCast(MyBase.Store, DatabaseBackedScheduleStore)
        End Get
    End Property

    ''' <summary>
    ''' The resource connection manager which this scheduler owns.
    ''' This uses the system user named "Scheduler", and thus cannot connect to any
    ''' logged in resources.
    ''' </summary>
    Friend ReadOnly Property ResourceConnectionManager() As IResourceConnectionManager
        Get
            Return mResourceManager
        End Get
    End Property

    ''' <summary>
    ''' Starts the scheduler thread, and listens for trigger events to be fired,
    ''' checking the registered number of milliseconds into the past for any missed
    ''' schedules.
    ''' </summary>
    ''' <param name="millisToReview">The number of milliseconds in the past to
    ''' review for un-executed schedules.</param>
    Public Overrides Sub Start(ByVal millisToReview As Integer)
        MyBase.Start(millisToReview)
    End Sub

    ''' <summary>
    ''' Stops the scheduler, and closes its resource connection manager 
    ''' </summary>
    Public Overrides Sub [Stop]()
        Try
            MyBase.Stop()
        Finally
            mResourceManager.Dispose()
            mResourceManager = Nothing
        End Try
    End Sub

End Class
