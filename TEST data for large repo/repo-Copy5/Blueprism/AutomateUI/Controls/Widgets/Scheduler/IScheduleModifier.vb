
Imports BluePrism.AutomateAppCore

''' <summary>
''' Interface which defines a class which modifies schedule data.
''' This ensures that it is possible to keep track of a schedule's
''' data being changed.
''' </summary>
Public Interface IScheduleModifier

    ''' <summary>
    ''' Event fired when the schedule data has changed.
    ''' </summary>
    ''' <param name="sender">The schedule whose data has changed
    ''' as a result of a change on this class.</param>
    Event ScheduleDataChange(ByVal sender As SessionRunnerSchedule)

End Interface
