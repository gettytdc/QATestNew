Imports System.Xml

Imports BluePrism.Scheduling.Calendar

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Imports System.Runtime.Serialization

''' <summary>
''' Component representing a calendar. This isn't a first class citizen at the moment
''' ie. calendars are only included in a package as dependents of a schedule.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class CalendarComponent : Inherits ScheduleStoredComponent

#Region " Class Scope Definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the calendar component.
    ''' </summary>
    Public Class MyConflicts

        ''' <summary>
        ''' 'Conflict' which occurs when a font being imported already exists in
        ''' the target environment.
        ''' </summary>
        Public Shared ReadOnly Property ExistingCalendar As ConflictDefinition
            Get
                If mExistingCalendar Is Nothing Then
                    mExistingCalendar = GetExistingCalendarConflictDefinition()
                Else
                    mExistingCalendar.UpdateConflictDefinitionStrings(GetExistingCalendarConflictDefinition())
                End If
                Return mExistingCalendar
            End Get
        End Property

        Private Shared Function GetExistingCalendarConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("ExistingCalendar", My.Resources.MyConflicts_ACalendarAlreadyExistsWithThisName,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingCalendarWithTheIncomingCalendar),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.MyConflicts_DonTImportThisCalendar)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mExistingCalendar As ConflictDefinition
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new calendar component using the given calendar.
    ''' </summary>
    ''' <param name="cal">The calendar to use for this component.</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal store As DatabaseBackedScheduleStore, ByVal cal As ScheduleCalendar)
        MyBase.New(owner, cal.Id, cal.Name)
        AssociatedData = cal
    End Sub

    ''' <summary>
    ''' Creates a new calendar component using the data in the given provider.
    ''' </summary>
    ''' <param name="prov">The provider with the data. This expects the following
    ''' attributes to be in the provider :- <list>
    ''' <item>id: Integer</item>
    ''' <item>name: String</item></list></param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        MyBase.New(owner, prov.GetValue("id", 0), prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the schedule calendar associated with this component.
    ''' </summary>
    Public ReadOnly Property AssociatedCalendar() As ScheduleCalendar
        Get
            Return DirectCast(AssociatedData, ScheduleCalendar)
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Calendar
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return "System - Calendars"
        End Get
    End Property

#End Region

#Region " Xml Handling "

    ''' <summary>
    ''' Writes this component out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this component should be written.
    ''' </param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        AssociatedCalendar.ToXml(writer)
    End Sub

    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        ' We should be on the component's calendar element, we need to read to the
        ' schedule calendar's element and subtree from that
        r.Read()
        Debug.Assert(r.NodeType = XmlNodeType.Element AndAlso r.LocalName = "schedule-calendar")

        Dim cal As ScheduleCalendar = ScheduleCalendar.FromXml(r.ReadSubtree(), Store)

        ' OK, at this point, ScheduleCalendar has assigned the new calendar a
        ' temporary ID, a proper one being assigned on saving to the database.
        ' We need to get that ID back into this component so that it can be
        ' picked up by the component loading context store.
        ' Calendars' IDs are IDENTITY fields anyway, so there's no global
        ' context to the incoming ID - names are what matter at this stage.
        Me.Id = cal.Id
        AssociatedData = cal
    End Sub

#End Region

#Region " Conflict Handling "


    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        ' We can't just check using the base class's store property - that is
        ' configured to expose calendars within the owner as well as on the database.
        If DatabaseBackedScheduleStore.InertStore.GetCalendar(Me.Name) Is Nothing Then
            Return GetEmpty.ICollection(Of ConflictDefinition)()
        End If
        Return GetSingleton.ICollection(MyConflicts.ExistingCalendar)
    End Function

    ''' <summary>
    ''' Applies all the conflict resolutions to this component - in reality, it only
    ''' needs one and there is no cross-component validation required for a
    ''' credential, so we can use an instance method rather than a static one.
    ''' </summary>
    ''' <param name="rel">The release in which the conflict occurred.</param>
    ''' <param name="resolutions">All the conflict resolutions for the release.
    ''' </param>
    ''' <param name="errors">The error log to which any validation errors should be
    ''' reported.</param>
    Private Sub ApplyConflictResolutions(ByVal rel As clsRelease,
     ByVal resolutions As ICollection(Of ConflictResolution), ByVal errors As clsErrorLog)

        ' FIXME: This is near-identical to the applier in fonts (and probably others)
        ' It could do with being wrapped into a common method somewhere - maybe have a
        ' protected 'ApplyDefaultResolutions' method in PackageComponent or some such.
        Dim mods As IDictionary(Of ModificationType, Object) = Modifications

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            If res.Conflict.Component IsNot Me Then Continue For

            ' Check that the user has selected an option
            Dim opt As ConflictOption = res.ConflictOption
            If opt Is Nothing Then
                errors.Add(Me, My.Resources.CalendarComponent_YouMustChooseHowToHandleTheNewCalendar0, Me.Name)
            Else
                Select Case opt.Choice
                    Case ConflictOption.UserChoice.Skip
                        mods.Add(ModificationType.Skip, True)
                    Case ConflictOption.UserChoice.Overwrite
                        mods.Add(ModificationType.OverwritingExisting, True)
                    Case Else
                        Throw New BluePrismException(My.Resources.CalendarComponent_UnrecognisedOption0, opt.Choice)
                End Select
                res.Passed = True
            End If
            ' There's only one conflict resolution for fonts, since there's only one conflict
            ' No point in going through the rest of the loop.
            Return
        Next

    End Sub

#End Region

#Region " Other methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return Store.GetCalendar(IdAsInteger)
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
        Return MyBase.Differs(comp) OrElse _
         Not AssociatedCalendar.Equals(DirectCast(comp, CalendarComponent).AssociatedCalendar)
    End Function

#End Region

End Class
