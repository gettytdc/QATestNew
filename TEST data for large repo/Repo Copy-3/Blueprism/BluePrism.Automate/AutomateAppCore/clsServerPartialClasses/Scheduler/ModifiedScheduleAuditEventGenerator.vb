Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServer

Namespace clsServerPartialClasses.Scheduler
    Public Class ModifiedScheduleAuditEventGenerator
        Implements IModifiedScheduleAuditEventGenerator

        Private ReadOnly mOldSchedule As SessionRunnerSchedule
        Private ReadOnly mNewSchedule As SessionRunnerSchedule
        Private ReadOnly mLoggedInUser As IUser

        Public Sub New(oldSchedule As SessionRunnerSchedule, newSchedule As SessionRunnerSchedule, loggedInUser As IUser)
            mOldSchedule = oldSchedule
            mNewSchedule = newSchedule
            mLoggedInUser = loggedInUser
        End Sub

        Public Function Generate() As IEnumerable(Of ScheduleAuditEvent) Implements IModifiedScheduleAuditEventGenerator.Generate
            Return GetAuditEventForModifiedSchedule().Concat(GetAuditEventsForModifiedTasks())
        End Function

        Private Function GetAuditEventForModifiedSchedule() As IEnumerable(Of ScheduleAuditEvent)

            Dim comment As New StringBuilder()

            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Name, mOldSchedule.Name, mNewSchedule.Name)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Description, mOldSchedule.Description, mNewSchedule.Description)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_InitialTask, mOldSchedule.InitialTaskId, mNewSchedule.InitialTaskId)

            Dim oldTrigger = mOldSchedule.Triggers.UserTrigger
            Dim newTrigger = mNewSchedule.Triggers.UserTrigger

            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_StartsOn, oldTrigger?.Start, newTrigger?.Start)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Expires, oldTrigger?.End, newTrigger?.End)

            Dim oldTriggerMetaData = oldTrigger?.PrimaryMetaData
            Dim newTriggerMetaData = newTrigger?.PrimaryMetaData

            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_AllowedHoursStart,
                                              oldTriggerMetaData?.AllowedHours?.StartTime,
                                              newTriggerMetaData?.AllowedHours?.StartTime)

            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_AllowedHoursEnd,
                                              oldTriggerMetaData?.AllowedHours?.EndTime,
                                              newTriggerMetaData?.AllowedHours?.EndTime)

            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Calendar, oldTriggerMetaData?.CalendarId, newTriggerMetaData?.CalendarId)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Days, oldTriggerMetaData?.Days, newTriggerMetaData?.Days)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Interval, oldTriggerMetaData?.Interval, newTriggerMetaData?.Interval)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_MissingDatePolicy, oldTriggerMetaData?.MissingDatePolicy, newTriggerMetaData?.MissingDatePolicy)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_NthOfMonth, oldTriggerMetaData?.Nth, newTriggerMetaData?.Nth)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Period, oldTriggerMetaData?.Period, newTriggerMetaData?.Period)

            Dim auditComment = comment.ToString()

            If String.IsNullOrEmpty(auditComment) Then Return Enumerable.Empty(Of ScheduleAuditEvent)

            Return {New ScheduleAuditEvent(ScheduleEventCode.ScheduleModified, mLoggedInUser,
                                          mNewSchedule.Id, Nothing, Nothing, auditComment)}

        End Function

        Private Function GetAuditEventsForModifiedTasks() As IEnumerable(Of ScheduleAuditEvent)
            Return mOldSchedule.
                        Join(mNewSchedule,
                             Function(oldTask) oldTask.Id,
                             Function(newTask) newTask.Id,
                             Function(oldTask, newTask) New With {oldTask, newTask}).
                        Select(Function(x) GetAuditEventForModifiedTask(x.oldTask, x.newTask)).
                        Where(Function(x) x IsNot Nothing)

        End Function

        Private Function GetAuditEventForModifiedTask(oldTask As ScheduledTask, newTask As ScheduledTask) As ScheduleAuditEvent

            Dim comment As New StringBuilder()

            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_OnCompleted, oldTask.OnSuccess?.Id, newTask.OnSuccess?.Id)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_OnFailure, oldTask.OnFailure?.Id, newTask.OnFailure?.Id)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Name, oldTask.Name, newTask.Name)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_Description, oldTask.Description, newTask.Description)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_FailFastOnError, oldTask.FailFastOnError, newTask.FailFastOnError)
            AppendModifiedValueToAuditComment(comment, My.Resources.ModifiedScheduleAuditEventGenerator_DelayAfterEnd, oldTask.DelayAfterEnd, newTask.DelayAfterEnd)

            Dim auditComment = comment.ToString()

            If String.IsNullOrEmpty(auditComment) Then Return Nothing

            Return New ScheduleAuditEvent(ScheduleEventCode.TaskModified, mLoggedInUser,
                                          newTask.Owner.Id, newTask.Id, Nothing, auditComment)

        End Function

        Private Sub AppendModifiedValueToAuditComment(auditComment As StringBuilder, valueDescription As String,
                                                     oldValue As Object, newValue As Object)

            If (oldValue Is Nothing AndAlso newValue Is Nothing) Then Return
            If oldValue IsNot Nothing AndAlso oldValue.Equals(newValue) Then Return

            auditComment.AppendFormat(My.Resources.ModifiedScheduleAuditEventGenerator_0ValueChangedOldValue1NewValue2, valueDescription,
                                      If(oldValue IsNot Nothing, GetStringFromValue(oldValue), String.Empty),
                                      If(newValue IsNot Nothing, GetStringFromValue(newValue), String.Empty))
        End Sub

        Private Function GetStringFromValue(value As Object) As String

            If value Is Nothing Then Return String.Empty

            If value.GetType() Is GetType(DateTime) Then Return DirectCast(value, DateTime).ToUniversalTime.ToString("O")

            Return value.ToString()

        End Function


    End Class

End Namespace
