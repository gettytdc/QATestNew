Imports System.Xml

Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.AutomateAppCore.ReleaseManager.Components

''' <summary>
''' Component which draws its data from a schedule store.
''' This utilises a store which can load data from within the loading context rather
''' than necessarily on the database when the component is being loaded from XML.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public MustInherit Class ScheduleStoredComponent : Inherits PackageComponent

#Region " Private Members "

    ' The schedule store to use for the calendar
    <NonSerialized()> _
    Private mStore As DatabaseBackedScheduleStore

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new schedule component representing the given schedule, using the
    ''' given store to retrieve dependant calendar information, if necessary.
    ''' </summary>
    ''' <param name="owner">The owning component of this component. Null to indicate
    ''' no owner.</param>
    ''' <param name="store">The store to use to retrieve scheduled information for
    ''' this schedule (other schedules, calendars). Null will cause the shared
    ''' <see cref="DatabaseBackedScheduleStore.InertStore"/> to be lazily
    ''' referenced if needed.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal store As DatabaseBackedScheduleStore)
        Me.New(owner, store, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new schedule stored component using the given properties
    ''' </summary>
    ''' <param name="owner">The owning component of this component. Null to indicate
    ''' no owner.</param>
    ''' <param name="id">The ID of the component to be represented.</param>
    ''' <param name="name">The name of the component to be represented.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Integer, ByVal name As String)
        Me.New(owner, Nothing, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new schedule stored component using the given properties
    ''' </summary>
    ''' <param name="owner">The owning component of this component. Null to indicate
    ''' no owner.</param>
    ''' <param name="store">The store to use to retrieve scheduled information for
    ''' this schedule (other schedules, calendars). Null will cause the shared
    ''' <see cref="DatabaseBackedScheduleStore.InertStore"/> to be lazily
    ''' referenced if needed.</param>
    ''' <param name="id">The ID of the schedule to be represented.</param>
    ''' <param name="name">The name of the schedule to be represented.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal store As DatabaseBackedScheduleStore, _
     ByVal id As Integer, ByVal name As String)
        MyBase.New(owner, id, name)
        mStore = store
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="owner">The owning component of this component. Null to indicate
    ''' no owner.</param>
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
    ''' The schedule store from which schedule-related information can be
    ''' retrieved.
    ''' </summary>
    Protected ReadOnly Property Store() As DatabaseBackedScheduleStore
        Get
            ' If it's not set, use the inert shared instance
            If mStore Is Nothing Then mStore = DatabaseBackedScheduleStore.InertStore
            Return mStore
        End Get
    End Property

    ''' <summary>
    ''' Override to indicate that this component updates Scheduler data
    ''' </summary>
    Public Overrides ReadOnly Property VersionDataName() As String
        Get
            Return DataNames.Scheduler
        End Get
    End Property


#End Region

#Region " Methods "

    ''' <summary>
    ''' Populates this package component from XML provided by the given reader
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the XML.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' package component.</param>
    Protected Overrides Sub FromXml(ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        ' Set up the store if not already set up - this ensures that all schedule
        ' stored components use the same store - one which prioritises components
        ' in the context over components from the backing store.
        mStore = DirectCast(ctx("schedule-store"), DatabaseBackedScheduleStore)
        If mStore Is Nothing Then
            mStore = New ComponentLoadingContextStore(ctx)
            ctx("schedule-store") = mStore
        End If
        MyBase.FromXml(reader, ctx)
    End Sub

#End Region

End Class
