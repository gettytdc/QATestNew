Imports System.Xml

Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Triggers

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Imports System.Runtime.Serialization

''' <summary>
''' Component representing a schedule
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class ScheduleComponent : Inherits ScheduleStoredComponent

#Region " Constructors "

    ''' <summary>
    ''' Creates a new schedule component from the given data provider.
    ''' </summary>
    ''' <param name="prov">The data provider for the new component. This expects
    ''' the following properties to be available :-<list>
    ''' <item>id: Integer</item>
    ''' <item>name: String</item></list>
    ''' </param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        MyBase.New(owner, prov.GetValue("id", 0), prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new schedule component representing the given schedule, using the
    ''' given store to retrieve dependant calendar information, if necessary.
    ''' </summary>
    ''' <param name="store">The schedule store to use to access scheduler information
    ''' such as calendar objects.</param>
    ''' <param name="sched">The schedule for which a component is required.</param>
    Public Sub New(ByVal owner As OwnerComponent, _
     ByVal store As DatabaseBackedScheduleStore, ByVal sched As SessionRunnerSchedule)
        MyBase.New(owner, store, sched.Id, sched.Name)
    End Sub

    ''' <summary>
    ''' Creates a new schedule component using the given properties
    ''' </summary>
    ''' <param name="id">The ID of the schedule to be represented.</param>
    ''' <param name="name">The name of the schedule to be represented.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Integer, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent, _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the schedule object associated with this component.
    ''' </summary>
    Public ReadOnly Property AssociatedSchedule() As SessionRunnerSchedule
        Get
            Return DirectCast(AssociatedData, SessionRunnerSchedule)
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Schedule
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return "Edit Schedule"
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Gets the dependents of this component - for schedules, this is any calendars
    ''' referenced by the schedule.
    ''' </summary>
    ''' <returns>The collection of components that this component is dependent upon
    ''' </returns>
    Public Overrides Function GetDependents() As ICollection(Of PackageComponent)
        Dim sched As SessionRunnerSchedule = _
         DirectCast(Store.GetSchedule(Me.IdAsInteger), SessionRunnerSchedule)

        Dim calendarIds As New clsSet(Of Integer)
        For Each trig As TriggerMetaData In sched.Triggers.MetaData
            If trig.CalendarId <> 0 Then calendarIds.Add(trig.CalendarId)
        Next

        If calendarIds.Count = 0 Then Return NoComponents
        Dim cals As New List(Of PackageComponent)
        For Each id As Integer In calendarIds
            cals.Add(New CalendarComponent(Me.Owner, Store, Store.GetCalendar(id)))
        Next
        Return cals
    End Function

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return Store.GetSchedule(IdAsInteger)
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the exportable data
    ''' in the given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True
        Dim sched As SessionRunnerSchedule = AssociatedSchedule
        If sched Is Nothing Then Throw New NoSuchElementException(
         My.Resources.ScheduleComponent_NoScheduleWithTheID0WasFound, Id)

        Dim compsched As SessionRunnerSchedule = _
         DirectCast(comp, ScheduleComponent).AssociatedSchedule

        ' Check the initial task.. by name - we'll be checking task values later,
        ' and since these could be tasks in different data domains, ID is largely irrelevant
        ' Check that both are either null or non-null
        If sched.InitialTask Is Nothing Xor compsched.InitialTask Is Nothing Then Return True

        ' we know compsched.InitialTask is also not null due to above Xor check
        ' So just check if their names differ
        If sched.InitialTask IsNot Nothing AndAlso _
         Not Object.Equals(sched.InitialTask.Name, compsched.InitialTask.Name) Then Return True

        ' OK, we go through the triggers and ensure that each schedule's trigger
        ' group is identical
        Dim matchedTriggers As New clsSet(Of ITrigger)
        For Each t As ITrigger In sched.Triggers.Members
            Dim md As TriggerMetaData = t.PrimaryMetaData
            Dim found As Boolean = False
            ' Search compsched for a trigger which matches this metadata
            For Each compt As ITrigger In compsched.Triggers.Members
                Dim compmd As TriggerMetaData = compt.PrimaryMetaData
                Try
                    ' We check against the given component's store first - purely
                    ' because, at time of writing, the given component is coming from
                    ' an importing release, and thus the calendar will not exist on
                    ' the database - therefore we need to use the context store to
                    ' check the calendar values on the trigger.
                    If md.Equals(compmd, DirectCast(comp, ScheduleComponent).Store) Then
                        matchedTriggers.Add(compt)
                        found = True
                        Exit For
                    End If

                Catch nsee As NoSuchElementException
                    ' If the calendar was not found in that store, try this one - if
                    ' it's a temporary calendar, it may not yet exist on the database
                    If md.Equals(compmd, Store) Then
                        matchedTriggers.Add(compt)
                        found = True
                        Exit For
                    End If

                End Try

            Next
            If Not found Then Return True
        Next

        ' matchedTriggers contains all the triggers on compsched which matched one
        ' on sched. If there exist any others, it means compsched has more than sched
        matchedTriggers.Subtract(compsched.Triggers.Members)
        If matchedTriggers.Count > 0 Then Return True

        ' Now the tasks - we treat this similarly to the triggers, and, again, we're
        ' only testing exportable data, so we're not interested in the sessions.
        ' Note that we can't simply use Equals() here, since it checks ID, which could
        ' differ even if the exportable data is equivalent.
        Dim matchedTasks As New clsSet(Of ScheduledTask)
        For Each t As ScheduledTask In sched
            Dim found As Boolean = False
            For Each compt As ScheduledTask In compsched
                If t.Name = compt.Name AndAlso t.Description = compt.Description _
                 AndAlso GetTaskName(t.OnSuccess) = GetTaskName(compt.OnSuccess) _
                 AndAlso GetTaskName(t.OnFailure) = GetTaskName(compt.OnFailure) Then
                    found = True
                    matchedTasks.Add(compt)
                    Exit For
                End If
            Next
            If Not found Then Return True
        Next

        ' Similarly to triggers, extra tasks indicate compsched having more tasks
        matchedTasks.Subtract(compsched)
        If matchedTasks.Count > 0 Then Return True

        ' righto, schedule, triggers, tasks... that's the lot, I think.
        ' If we've made it this far, they must be the same... ergo:
        Return False

    End Function

    ''' <summary>
    ''' Gets the name of the given task, or null if there was no task there.
    ''' </summary>
    ''' <param name="t">The task to check, or null.</param>
    ''' <returns>The name of the task or null if the given task was null.</returns>
    ''' <remarks>This is primarily a way of simplifying a name comparison on the
    ''' OnSuccess or OnFailure properties of the task, which may be null.
    ''' </remarks>
    Private Function GetTaskName(ByVal t As ScheduledTask) As String
        If t Is Nothing Then Return Nothing
        Return t.Name
    End Function

    ''' <summary>
    ''' Writes this schedule to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The XML writer to which this component should be written
    ''' </param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)

        Dim sched As SessionRunnerSchedule =
         DirectCast(Store.GetSchedule(IdAsInteger), SessionRunnerSchedule)

        writer.WriteElementString("description", sched.Description)

        writer.WriteStartElement("triggers")
        For Each md As TriggerMetaData In sched.Triggers.MetaData
            md.ToXml(writer, Store)
        Next
        writer.WriteEndElement() 'triggers

        writer.WriteStartElement("tasks")
        For Each t As ScheduledTask In sched
            t.ToXml(writer)
        Next
        writer.WriteEndElement() 'tasks

    End Sub

    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        ' Try getting a schedule store from the context - if there's not one there,
        ' then we're the first one in, so create one.
        Dim schedStore As IScheduleStore = DirectCast(ctx("schedule-store"), IScheduleStore)

        If schedStore Is Nothing Then
            schedStore = New DatabaseBackedScheduleStore(gSv)
            ctx("schedule-store") = schedStore
        End If

        Dim sched As SessionRunnerSchedule = New SessionRunnerSchedule(schedStore.Owner)
        sched.Name = Me.Name

        While r.Read()
            ' Skip empty elements of any kind
            If r.NodeType = XmlNodeType.Element AndAlso r.IsEmptyElement Then Continue While

            Select Case r.NodeType

                Case XmlNodeType.Element

                    Select Case r.LocalName

                        Case "description"
                            r.Read() ' read to the text block
                            sched.Description = r.Value ' get the text block

                        Case "triggers"
                            ' Inner loop to read the triggers only
                            While r.Read()
                                If r.NodeType = XmlNodeType.EndElement _
                                 AndAlso r.LocalName = "triggers" Then Exit While ' (inner while)

                                sched.Triggers.Add(TriggerFactory.GetInstance().CreateTrigger( _
                                 TriggerMetaData.FromXml(r.ReadSubtree(), schedStore)))

                            End While

                        Case "tasks"
                            ' Build up the tasks by name so we can resolve the onsuccess
                            ' and onfailure references after parsing is complete
                            Dim tasksByName As New Dictionary(Of String, ScheduledTask)

                            ' Inner loop to read the tasks only
                            While r.Read()
                                If r.NodeType = XmlNodeType.EndElement _
                                 AndAlso r.LocalName = "tasks" Then Exit While ' (inner while)

                                Dim t As New ScheduledTask()
                                ' clsTask.FromXml returns true if it is the initial task, or
                                ' false otherwise.
                                Dim init As Boolean = t.FromXml(r.ReadSubtree())
                                sched.Add(t)
                                If init Then sched.InitialTask = t
                                ' Save each task we've loaded by name so we can resolve the
                                ' onsuccess and onfailure tasks later
                                tasksByName(t.Name) = t

                            End While

                            ' We need to resolve the onsuccess and onfailure tasks
                            ' now that we've parsed them all - clsTask parsing substitutes
                            ' a placeholder OnSuccess/Failure task when it parses it.
                            ' We just need to replace those placeholders with the real
                            ' things.
                            For Each t As ScheduledTask In sched
                                If t.OnSuccess IsNot Nothing Then _
                                 t.OnSuccess = tasksByName(t.OnSuccess.Name)
                                If t.OnFailure IsNot Nothing Then _
                                 t.OnFailure = tasksByName(t.OnFailure.Name)
                            Next


                    End Select

                Case XmlNodeType.EndElement
                    If r.LocalName = "schedule" Then Exit While

            End Select
        End While

        AssociatedData = sched

    End Sub

    ''' <summary>
    ''' Override used during import to initialise owner of associated schedule
    ''' instance
    ''' </summary>
    ''' <param name="scheduler">The scheduler instance created for use during
    ''' import</param>
    Public Overrides Sub InitialiseForImport(scheduler As IScheduler)

        If AssociatedSchedule IsNot Nothing
            AssociatedSchedule.Owner = scheduler
        End If

    End Sub

#End Region

End Class
