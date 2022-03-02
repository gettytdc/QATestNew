Imports BluePrism.Scheduling
Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : ctlRunOnce
''' <summary>
''' Dummy control which is shown when 'Run Once' is chosen from the schedule
''' control.
''' </summary>
Public Class ctlRunOnce
    Inherits Panel
    Implements IIntervalControl

#Region "IScheduleModifier implementation"

    ''' <summary>
    ''' Event fired when the schedule data has changed.
    ''' </summary>
    ''' <param name="sender">The schedule whose data has changed as a result of a
    '''  change on this class.</param>
    ''' <remarks>Note that since there is no configurable data in RunOnce control
    ''' this is never actually fired.</remarks>
    Public Event ScheduleDataChange(ByVal sender As SessionRunnerSchedule) _
     Implements IScheduleModifier.ScheduleDataChange

#End Region

#Region "IInterval Control Implementation"

    ''' <summary>
    ''' The actual control object that this interval control uses to display
    ''' itself. That's me, that is.
    ''' </summary>
    Public ReadOnly Property Control() As Control Implements IIntervalControl.Control
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' Checks if this control supports the given interval or not.
    ''' </summary>
    ''' <param name="interval">The interval to check if the control supports it
    ''' or not.</param>
    ''' <returns>True if the given interval type is <see cref="IntervalType.Once"/>;
    ''' False otherwise.</returns>
    Public Function SupportsInterval(ByVal interval As IntervalType) As Boolean _
     Implements IIntervalControl.SupportsInterval
        Return (interval = IntervalType.Once)
    End Function

    ''' <summary>
    ''' Gets the trigger metadata held specifically by this interval control.
    ''' Running once has no extra metadata, thus this returns an empty object.
    ''' </summary>
    ''' <returns>An empty trigger metadata object</returns>
    Public Function GetData(ByVal err As ICollection(Of clsValidationError)) As TriggerMetaData _
     Implements IIntervalControl.GetData
        Return New TriggerMetaData()
    End Function

    ''' <summary>
    ''' Populates this control with the given data... only this control doesn't
    ''' acutally have any data, so really it doesn't do anything.
    ''' </summary>
    ''' <param name="data">The data to ignore.</param>
    ''' <param name="sched">The owning schedule</param>
    Public Sub Populate(ByVal data As TriggerMetaData, ByVal sched As SessionRunnerSchedule) _
     Implements IIntervalControl.Populate

    End Sub

    ''' <summary>
    ''' Sets the start date for this control
    ''' </summary>
    ''' <param name="d">The start date</param>
    Public Sub SetStartDate(ByVal d As Date) Implements IIntervalControl.SetStartDate
    End Sub

    Public Property [ReadOnly]() As Boolean Implements IIntervalControl.ReadOnly
        Get
            Return mReadonly
        End Get
        Set(ByVal value As Boolean)
            mReadonly = value
        End Set
    End Property
    Private mReadonly As Boolean
#End Region

End Class
