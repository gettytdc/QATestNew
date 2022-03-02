Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar

Namespace ReleaseManager.Components

    ''' <summary>
    ''' Schedule store which can load calendars from the accompanying component
    ''' loading context (via the CalendarComponents in the context).
    ''' If the calendar it is asked for does not exist in the specified context,
    ''' it delegates the call to the super class - ie. it retrieves the calendar
    ''' from the database.
    ''' </summary>
    Friend Class ComponentLoadingContextStore : Inherits DatabaseBackedScheduleStore
        ' The component loading context from which this store is being loaded
        Private mCtx As IComponentLoadingContext

        ''' <summary>
        ''' Creates a new Component loading context store, which is backed by the
        ''' given context.
        ''' </summary>
        ''' <param name="ctx">The context from which the calendars should be drawn
        ''' by default.</param>
        Public Sub New(ByVal ctx As IComponentLoadingContext)
            MyBase.New(New InertScheduler(), gSv)
            mCtx = ctx
        End Sub

        ''' <summary>
        ''' Gets all the calendars from this store.
        ''' This gets the database backed calendars and appends the calendars from
        ''' the context in this store.
        ''' </summary>
        ''' <returns>The calendars represented within this store.</returns>
        Public Overrides Function GetAllCalendars() As ICollection(Of ScheduleCalendar)
            Dim cals As New List(Of ScheduleCalendar)(MyBase.GetAllCalendars())
            For Each comp As CalendarComponent In mCtx.GetAllComponents(PackageComponentType.Calendar)
                cals.Add(comp.AssociatedCalendar)
            Next
            Return cals
        End Function

        ''' <summary>
        ''' Gets the calendar from this store with the given name, or Nothing if
        ''' the given name did not represent a calendar in this store.
        ''' </summary>
        ''' <param name="name">The name of the calendar required</param>
        ''' <returns>The calendar held within this store with the given name or
        ''' Nothing if the name was not found.</returns>
        Public Overrides Function GetCalendar(ByVal name As String) As ScheduleCalendar
            Dim comp As CalendarComponent =
                    DirectCast(mCtx.GetComponent(PackageComponentType.Calendar, name), CalendarComponent)
            If comp Is Nothing Then Return MyBase.GetCalendar(name)
            Return comp.AssociatedCalendar
        End Function

        ''' <summary>
        ''' Gets the calendar from this store with the given ID, or Nothing if
        ''' the given name did not represent a calendar in this store.
        ''' </summary>
        ''' <param name="id">The ID of the calendar required</param>
        ''' <returns>The calendar held within this store with the given ID or
        ''' Nothing if the ID was not found.</returns>
        Public Overrides Function GetCalendar(ByVal id As Integer) As ScheduleCalendar
            Dim comp As CalendarComponent = _
                    DirectCast(mCtx.GetComponent(PackageComponentType.Calendar, id), CalendarComponent)
            If comp IsNot Nothing Then Return comp.AssociatedCalendar
            Return MyBase.GetCalendar(id)
        End Function

    End Class
End NameSpace