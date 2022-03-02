Imports BluePrism.AutomateAppCore

Public Class ctlScheduleTimetable : Inherits ctlScheduleListPanel

    ''' <summary>
    ''' The text used for the link label for viewing a schedule.
    ''' </summary>
    Private VIEW_SCHEDULE As String = My.Resources.ViewSchedule

    ''' <summary>
    ''' The text used for the link label for viewing a timetable.
    ''' </summary>
    Private VIEW_TIMETABLE As String = My.Resources.ViewTimetable

    ''' <summary>
    ''' The list of relative date items which are used for a timetable panel.
    ''' </summary>
    Private Shared ReadOnly RelativeDateList As IList(Of RelativeDateItem) =
     New List(Of RelativeDateItem)(New RelativeDateItem() _
      {RelativeToday, RelativeTomorrow}).AsReadOnly()

    ''' <summary>
    ''' Creates a new schedule timetable panel.
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        With gridScheduleList.Columns
            .Remove(colViewLog)
            .Remove(colEnd)
            .Remove(colScheduler)
        End With

    End Sub

    Private mInstances As ICollection(Of IScheduleInstance)

    ''' <summary>
    ''' Gets the list of relative date items which is held in this list
    ''' control. For a timetable panel, this is: None; Today; Tomorrow.
    ''' </summary>
    ''' <returns>A non-null list of relative date items to display in this
    ''' schedule list panel.</returns>
    Protected Overrides Function GetRelativeDateItems() As IList(Of RelativeDateItem)
        Return RelativeDateList
    End Function


    ''' <summary>
    ''' The verb for the label which sits between the 'days' and the date to
    ''' which those days apply. ie.
    ''' In the statement "Show 5 days [label] Today", this should return the
    ''' text which goes into [label].
    ''' For a timetable this is "starting", ie. "Show 5 days starting Today"
    ''' </summary>
    Protected Overrides ReadOnly Property DaysVerbing() As String
        Get
            Return My.Resources.Starting
        End Get
    End Property

    ''' <summary>
    ''' The verb for the label which sits between "Show the" and the days distance
    ''' updown textbox.
    ''' </summary>
    Protected Overrides ReadOnly Property ShowVerbing() As String
        Get
            Return My.Resources.xNext
        End Get
    End Property

    ''' <summary>
    ''' Populates this panel and its child controls with the data from the
    ''' given list.
    ''' </summary>
    ''' <param name="list">The list to populate this list panel with.</param>
    Protected Overrides Sub InnerPopulate(ByVal list As ScheduleList)

        MyBase.InnerPopulate(list)

        mInstances = list.Store.GetListEntries(list)

        gridScheduleList.Rows.Clear()
        For Each instance As IScheduleInstance In mInstances

            Dim i As Integer = gridScheduleList.Rows.Add(instance.Status, _
             instance.Schedule.Name, instance.InstanceTime, VIEW_SCHEDULE)
            gridScheduleList.Rows(i).Tag = instance
        Next
    End Sub

    ''' <summary>
    ''' Handles a link being clicked on a schedule instance.
    ''' </summary>
    ''' <param name="inst">The instance that was clicked.</param>
    ''' <param name="linkText">The text of the link that was clicked.</param>
    Private Sub ScheduleLinkClicked(ByVal inst As IScheduleInstance, ByVal linkText As String) _
     Handles MyBase.LinkClick

        If linkText = VIEW_SCHEDULE Then

            Dim manager As ctlControlRoom = _
             clsUserInterfaceUtils.GetAncestor(Of ctlControlRoom)(Me)

            If manager IsNot Nothing Then
                manager.SelectSchedule(inst.Schedule)
            End If

        End If

    End Sub

End Class
