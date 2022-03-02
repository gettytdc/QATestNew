Imports System.Threading
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : clsAutoSaver
''' 
''' <summary>
''' A class for creating, timing and handling process back up records.
''' </summary>
Public Class clsAutoSaver
    Implements IDisposable

#Region "Private variables"

    ''' <summary>
    ''' The timer object.
    ''' </summary>
    Private mobjTimer As Timer

    ''' <summary>
    ''' The process id.
    ''' </summary>
    Private mgProcessId As Guid

    ''' <summary>
    ''' The process object.
    ''' </summary>
    Private mProcess As clsProcess

    ''' <summary>
    ''' The back up interval.
    ''' </summary>
    Private miMinutes As Long

#End Region

#Region "Event Classes etc"

    ''' Project  : Automate
    ''' Class    : clsAutoSaver.BackupEventArgs
    ''' 
    ''' <summary>
    ''' The information we pass to those observing events from this class.
    ''' </summary>
    Public Class BackupEventArgs
        Public Sub New(ByVal BackupInterval As Long)
            Me.BackupInterval = BackupInterval
        End Sub
        ''' <summary>
        ''' The current backup interval in minutes.
        ''' </summary>
        Public BackupInterval As Long
    End Class

    ''' <summary>
    ''' Handler signature for event AutosavePerformed
    ''' </summary>
    Delegate Sub OnAutosavePerformed(ByVal e As BackupEventArgs)

    ''' <summary>
    ''' Event raised when process is written to database
    ''' </summary>
    Public Event AutosavePerformed As OnAutosavePerformed

    ''' <summary>
    ''' Handler signature for event AutosaveError
    ''' </summary>
    Delegate Sub OnAutosaveError(ByVal e As Exception)

    ''' <summary>
    ''' Event raised when fails to write process to database
    ''' </summary>
    Public Event AutosaveError As OnAutosaveError

#End Region


    ''' <summary>
    ''' The auto save interval.
    ''' </summary>
    ''' <value>The interval in minutes</value>
    Public Property Interval() As Long
        Get
            Return miMinutes
        End Get
        Set(ByVal Value As Long)
            Dim millis As Long
            millis = Value * 60 * 1000
            If Not miMinutes = Value Then
                mobjTimer.Change(millis, millis)
                miMinutes = Value
            End If
        End Set
    End Property

#Region "Constructors"

    ''' <summary>
    ''' Creates an object using the default timer interval
    ''' </summary>
    ''' <param name="process">The process object</param>
    ''' <param name="processId">The process id</param>
    Public Sub New(ByVal process As clsProcess, ByVal processId As Guid)
        Dim minutes As Integer = gSv.AutoSaveReadInterval()
        CreateTimer(minutes)
        mgProcessId = processId
        mProcess = process
        miMinutes = minutes
    End Sub

#End Region



#Region "Disposal"

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub

    Protected Sub Dispose(ByVal disposing As Boolean)
        mobjTimer.Dispose()
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

#End Region



#Region "CreateTimer"

    ''' <summary>
    ''' Creates the timer object.
    ''' </summary>
    ''' <param name="minutes">The timer interval</param>
    Private Sub CreateTimer(ByVal minutes As Integer)

        Dim timerDelegate As New TimerCallback(AddressOf DoBackUp)
        Dim millis As Long

        'Set the timer to never run (ie OFF) if the interval is zero
        If minutes > 0 Then
            millis = minutes * 60 * 1000
            mobjTimer = New Timer(timerDelegate, Nothing, millis, millis)
        Else
            mobjTimer = New Timer(timerDelegate, Nothing, Timeout.Infinite, Timeout.Infinite)
        End If
    End Sub

#End Region


    ''' <summary>
    ''' The method associated with the timer tick event.
    ''' </summary>
    Private Sub DoBackUp(ByVal state As Object)

        If Not mProcess.IsDisposed AndAlso mProcess.HasChanged() Then
            Try
                gSv.CreateProcessAutoSave(mProcess.GenerateXML(), mgProcessId)
                RaiseEvent AutosavePerformed(New BackupEventArgs(Me.Interval))
            Catch lu As LockUnavailableException
                RaiseEvent AutosaveError(lu)
            Catch ex As Exception
                RaiseEvent AutosaveError(ex)
            End Try
        End If

    End Sub

End Class

