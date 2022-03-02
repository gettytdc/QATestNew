Imports BluePrism.Scheduling
Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : ctlRunYearly
''' 
''' <summary>
''' Allows the user to configure yearly running
''' </summary>
Public Class ctlRunYearly
    Inherits UserControl
    Implements IIntervalControl

#Region "IScheduleModifier implementation"

    ''' <summary>
    ''' Event fired when the schedule data has changed.
    ''' </summary>
    ''' <param name="sender">The schedule whose data has changed as a result of a
    '''  change on this class.</param>
    ''' <remarks>Note that since there is no configurable data in the RunYearly
    ''' control this is never actually fired.</remarks>
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
    ''' <returns>True if the given interval type is <see cref="IntervalType.Year"/>;
    ''' False otherwise.</returns>
    Public Function SupportsInterval(ByVal interval As IntervalType) As Boolean _
     Implements IIntervalControl.SupportsInterval
        Return (interval = IntervalType.Year)
    End Function

    ''' <summary>
    ''' Gets the TriggetMetaData generated by the fields on the control
    ''' </summary>
    Public Function GetData(ByVal err As ICollection(Of clsValidationError)) As TriggerMetaData _
     Implements IIntervalControl.GetData


        Dim d As New TriggerMetaData
        d.Interval = IntervalType.Year
        d.Period = CInt(updnPeriod.Value)
        Return d
    End Function

    ''' <summary>
    ''' Populates the control from the given TriggerMetaData
    ''' </summary>
    ''' <param name="data">The TriggerMetaData to populate the control with</param>
    ''' <param name="sched">The schedule also needed to populate some controls</param>
    Public Sub Populate(ByVal data As TriggerMetaData, ByVal sched As SessionRunnerSchedule) _
     Implements IIntervalControl.Populate
        updnPeriod.Value = data.Period
    End Sub

    ''' <summary>
    ''' Sets the start date needed by some controls
    ''' </summary>
    ''' <param name="d">The Date</param>
    Public Sub SetStartDate(ByVal d As Date) Implements IIntervalControl.SetStartDate
    End Sub

    ''' <summary>
    ''' Sets the control into readonly mode if true
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IIntervalControl.ReadOnly
        Get
            Return mReadonly
        End Get
        Set(ByVal value As Boolean)
            mReadonly = value
            updnPeriod.ReadOnly = value
            updnPeriod.Increment = CInt(IIf(value, 0, 1))
        End Set
    End Property
    Private mReadonly As Boolean
#End Region

End Class
