Imports System.Xml

Imports BluePrism.BPCoreLib.Extensions
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports Component = BluePrism.Images.ImageLists.Keys.Component
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.WebApis
Imports System.Text.RegularExpressions
Imports System.Globalization

''' <summary>
''' The type of a package component - this provides the (image) key, labels and ID
''' type of a specific package component type as well as type-related functions.
''' These should never be created directly by external code - the defined types can
''' be accessed using the individual type constants, eg : <see
''' cref="PackageComponentType.Process"/>, <see cref="PackageComponentType.Queue"/>
''' etc.
''' </summary>
Public Class PackageComponentType : Implements IComparable(Of PackageComponentType), IComparable

#Region " Class Scope Definitions "

    ''' <summary>
    ''' Class to contain the keys used to lookup the types.
    ''' These keys correspond to the 'typekeys' used in the constant type objects
    ''' registered in the PackageComponentType class.
    ''' </summary>
    Protected Class Keys
        Public Const Package As String = Component.Package

        Public Const ReleaseIn As String = Component.ReleaseIn
        Public Const ReleaseOut As String = Component.ReleaseOut
        Public Const Release As String = Component.Release

        Public Const Process As String = Component.Process
        Public Const BusinessObject As String = Component.Object

        Public Const Queue As String = Component.Queue
        Public Const Calendar As String = Component.Calendar
        Public Const Credential As String = Component.Credential
        Public Const Group As String = Component.ClosedGroup
        Public Const Schedule As String = Component.Schedule
        Public Const ScheduleList As String = "schedule-list"
        Public Const WebService As String = Component.WebService
        Public Const WebApi As String = Component.WebApi
        Public Const EnvironmentVariable As String = Component.EnvVar
        Public Const Font As String = Component.Font
        Public Const Tile As String = Component.Tile
        Public Const Dashboard As String = Component.Dashboard
        Public Const DataSource As String = Component.DataSource

        Public Const ProcessGroup As String = "process-group"
        Public Const ObjectGroup As String = "object-group"
        Public Const QueueGroup As String = "work-queue-group"
        Public Const TileGroup As String = "tile-group"

        Public Const Skill As String = Component.Skill

        Public Shared ReadOnly All As ICollection(Of String) = GetReadOnly.IListFrom(
         Package,
         ReleaseOut,
         ReleaseIn,
         Release,
         Process,
         BusinessObject,
         Queue,
         Calendar,
         Credential,
         Group,
         Schedule,
         ScheduleList,
         WebService,
         WebApi,
         EnvironmentVariable,
         Font,
         Tile,
         Dashboard,
         DataSource,
         Skill
        )

    End Class

    ''' <summary>
    ''' All the package component types keyed by their type key
    ''' </summary>
    Public Shared AllTypes As IDictionary(Of String, PackageComponentType) = MakeAllTypes()

#Region " Individual Type Constants "

    ''' <summary>
    ''' Component type defining a package.
    ''' </summary>
    Public Shared ReadOnly Package As PackageComponentType = AllTypes(Keys.Package)

    ''' <summary>
    ''' Component type defining an incoming release
    ''' </summary>
    Public Shared ReadOnly ReleaseIn As PackageComponentType = AllTypes(Keys.ReleaseIn)

    ''' <summary>
    ''' Component type defining an outgoing release
    ''' </summary>
    Public Shared ReadOnly ReleaseOut As PackageComponentType = AllTypes(Keys.ReleaseOut)

    ''' <summary>
    ''' Component type defining a generic release
    ''' </summary>
    Public Shared ReadOnly Release As PackageComponentType = AllTypes(Keys.Release)

    ''' <summary>
    ''' Component type defining a Process.
    ''' </summary>
    Public Shared ReadOnly Process As PackageComponentType = AllTypes(Keys.Process)

    ''' <summary>
    ''' Component type defining a Business Object.
    ''' </summary>
    Public Shared ReadOnly BusinessObject As PackageComponentType = AllTypes(Keys.BusinessObject)

    ''' <summary>
    ''' Component type defining a Queue.
    ''' </summary>
    Public Shared ReadOnly Queue As PackageComponentType = AllTypes(Keys.Queue)

    ''' <summary>
    ''' Component type defining a Calendar.
    ''' </summary>
    Public Shared ReadOnly Calendar As PackageComponentType = AllTypes(Keys.Calendar)

    ''' <summary>
    ''' Component type defining a Credential.
    ''' </summary>
    Public Shared ReadOnly Credential As PackageComponentType = AllTypes(Keys.Credential)

    ''' <summary>
    ''' Component type defining a Group.
    ''' </summary>
    Public Shared ReadOnly Group As PackageComponentType = AllTypes(Keys.Group)

    ''' <summary>
    ''' Component type defining a Schedule.
    ''' </summary>
    Public Shared ReadOnly Schedule As PackageComponentType = AllTypes(Keys.Schedule)

    ''' <summary>
    ''' Component type defining a Schedule List.
    ''' </summary>
    Public Shared ReadOnly ScheduleList As PackageComponentType = AllTypes(Keys.ScheduleList)

    ''' <summary>
    ''' Component type defining a SOAP Web Service.
    ''' </summary>
    Public Shared ReadOnly WebService As PackageComponentType = AllTypes(Keys.WebService)

    ''' <summary>
    ''' Component type defining a Web API.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public Shared ReadOnly WebApi As PackageComponentType = AllTypes(Keys.WebApi)

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Component type defining an Environment Variable.
    ''' </summary>
    Public Shared ReadOnly EnvironmentVariable As PackageComponentType =
     AllTypes(Keys.EnvironmentVariable)

    ''' <summary>
    ''' Component type defining a Font.
    ''' </summary>
    Public Shared ReadOnly Font As PackageComponentType = AllTypes(Keys.Font)

    ''' <summary>
    ''' Component type defining a Tile.
    ''' </summary>
    Public Shared ReadOnly Tile As PackageComponentType = AllTypes(Keys.Tile)

    ''' <summary>
    ''' Component type defining a Dashboard.
    ''' </summary>
    Public Shared ReadOnly Dashboard As PackageComponentType = AllTypes(Keys.Dashboard)

    ''' <summary>
    ''' Component type defining a Tile Data Source.
    ''' </summary>
    Public Shared ReadOnly DataSource As PackageComponentType = AllTypes(Keys.DataSource)

    Public Shared ReadOnly Skill As PackageComponentType = AllTypes(Keys.Skill)

#End Region

    ''' <summary>
    ''' Makes all the types for the AllTypes dictionary
    ''' </summary>
    ''' <returns>A map of component types to their keys</returns>
    Private Shared Function MakeAllTypes() As IDictionary(Of String, PackageComponentType)
        Dim map As New Dictionary(Of String, PackageComponentType)

        map(Keys.Package) = New PackageComponentType(Keys.Package, "Package", GetType(Integer))

        map(Keys.ReleaseIn) = New PackageComponentType(Keys.ReleaseIn, "Release", GetType(Integer))
        map(Keys.ReleaseOut) = New PackageComponentType(Keys.ReleaseOut, "Release", GetType(Integer))
        map(Keys.Release) = New PackageComponentType(Keys.Release, "Release", GetType(Integer))

        map(Keys.Process) =
         New PackageComponentType(Keys.Process, "Process", "Processes", GetType(Guid), GetType(ProcessComponent.ProcessWrapper))

        map(Keys.BusinessObject) =
         New PackageComponentType(Keys.BusinessObject, "Business Object", GetType(Guid), GetType(ProcessComponent.ProcessWrapper))

        map(Keys.Queue) = New PackageComponentType(Keys.Queue, "Work Queue", GetType(Integer), GetType(clsWorkQueue))

        map(Keys.Calendar) = New PackageComponentType(Keys.Calendar, "Calendar", GetType(Integer), GetType(ScheduleCalendar))

        map(Keys.Group) = New PackageComponentType(Keys.Group, "Group", GetType(Guid))
        map(Keys.ProcessGroup) = New PackageComponentType(Keys.ProcessGroup, "Process Group", GetType(Guid))
        map(Keys.ObjectGroup) = New PackageComponentType(Keys.ObjectGroup, "Object Group", GetType(Guid))
        map(Keys.QueueGroup) = New PackageComponentType(Keys.QueueGroup, "Queue Group", GetType(Guid))
        map(Keys.TileGroup) = New PackageComponentType(Keys.TileGroup, "Tile Group", GetType(Guid))

        map(Keys.Schedule) = New PackageComponentType(Keys.Schedule, "Schedule", GetType(Integer), GetType(SessionRunnerSchedule))

        map(Keys.ScheduleList) = New PackageComponentType(Keys.ScheduleList,
         "Schedule Report/Timetable", "Schedule Reports/Timetables", GetType(Integer))

        map(Keys.WebService) =
         New PackageComponentType(Keys.WebService, "SOAP Web Service", GetType(Guid), GetType(clsWebServiceDetails))

        map(Keys.WebApi) =
            New PackageComponentType(Keys.WebApi, "Web API", GetType(Guid), GetType(WebApi))

        map(Keys.EnvironmentVariable) = New PackageComponentType(
         Keys.EnvironmentVariable, "Environment Variable", GetType(String), GetType(clsEnvironmentVariable))

        map(Keys.Credential) =
         New PackageComponentType(Keys.Credential, "Credential", GetType(Guid), GetType(clsCredential))

        map(Keys.Font) = New PackageComponentType(Keys.Font, "Font", GetType(String), GetType(String))

        map(Keys.Tile) = New PackageComponentType(Keys.Tile, "Tile", GetType(Guid), GetType(Tile))
        map(Keys.Dashboard) = New PackageComponentType(Keys.Dashboard, "Dashboard", GetType(Guid), GetType(Dashboard))
        map(Keys.DataSource) = New PackageComponentType(Keys.DataSource, "Data Source", GetType(String))

        map(Keys.Skill) = New PackageComponentType(Keys.Skill, "Skill", GetType(Guid))

        Return GetReadOnly.IDictionary(map)

    End Function

    ''' <summary>
    ''' The order defining the types - any dependencies of a component should come
    ''' before that component so that they get processed first (eg. calendars must
    ''' come before schedules since the calendar assigned to a schedule must exist
    ''' before the schdeule can be saved to the database). Likewise, processes can
    ''' only work if they are processed before process groups.
    ''' </summary>
    Public Shared Orders As IList(Of PackageComponentType) =
    GetReadOnly.IList(New PackageComponentType() {
     AllTypes(Keys.Package),
     AllTypes(Keys.ReleaseIn),
     AllTypes(Keys.ReleaseOut),
     AllTypes(Keys.Release), _
 _
     AllTypes(Keys.Process),
     AllTypes(Keys.BusinessObject), _
 _
     AllTypes(Keys.Queue),
     AllTypes(Keys.Credential), _
 _
     AllTypes(Keys.Calendar),
     AllTypes(Keys.Schedule),
     AllTypes(Keys.ScheduleList), _
 _
     AllTypes(Keys.WebService),
     AllTypes(Keys.WebApi),
     AllTypes(Keys.EnvironmentVariable), _
 _
     AllTypes(Keys.DataSource),
     AllTypes(Keys.Tile),
     AllTypes(Keys.Dashboard), _
 _
     AllTypes(Keys.ProcessGroup),
     AllTypes(Keys.ObjectGroup),
     AllTypes(Keys.QueueGroup),
     AllTypes(Keys.TileGroup), _
 _
    AllTypes(Keys.Skill)
    })

    ''' <summary>
    ''' Compares the two types, testing whether the second one is greater than, equal
    ''' to or less than the first one.
    ''' </summary>
    ''' <param name="one">The first type to test</param>
    ''' <param name="two">The second type to test</param>
    ''' <returns>1, 0 or -1 if the second is larger than, equal to or less than the
    ''' first type, respectively.</returns>
    Public Shared Function CompareTypes(
     ByVal one As PackageComponentType, ByVal two As PackageComponentType) As Integer
        ' Check either / both for null.
        If one Is Nothing AndAlso two Is Nothing Then Return 0
        If one Is Nothing Then Return -1 Else If two Is Nothing Then Return 1

        ' Check their order in the order-defining list
        Dim ix1 As Integer = Orders.IndexOf(one)
        Dim ix2 As Integer = Orders.IndexOf(two)
        If ix1 = ix2 Then Return 0 Else If ix1 < ix2 Then Return -1 Else Return 1

    End Function

    ''' <summary>
    ''' Creates a new component, based on the given type key, and using data from
    ''' the given provider.
    ''' </summary>
    ''' <param name="key">The key defining which type of component to create.</param>
    ''' <param name="prov">The provider with the data for the component.</param>
    ''' <returns>A component initialised with the given data provider.</returns>
    ''' <exception cref="ArgumentException">If the given type did not correspond to
    ''' a component which can be created with a data provider.</exception>
    Public Shared Function NewComponent(ByVal owner As OwnerComponent,
     ByVal key As String, ByVal prov As IDataProvider) As PackageComponent
        Select Case key
            Case Keys.Package : Return New clsPackage(prov)
            Case Keys.Release : Return New clsRelease(prov)
            Case Keys.Process : Return New ProcessComponent(owner, prov)
            Case Keys.BusinessObject : Return New VBOComponent(owner, prov)
            Case Keys.Queue : Return New WorkQueueComponent(owner, prov)
            Case Keys.Calendar : Return New CalendarComponent(owner, prov)
            Case Keys.Credential : Return New CredentialComponent(owner, prov)
            Case Keys.Schedule : Return New ScheduleComponent(owner, prov)
            Case Keys.ScheduleList : Return New ScheduleListComponent(owner, prov)
            Case Keys.WebService : Return New WebServiceComponent(owner, prov)
            Case Keys.WebApi : Return New WebApiComponent(owner, prov)
            Case Keys.EnvironmentVariable : Return New EnvironmentVariableComponent(owner, prov)
            Case Keys.Font : Return New FontComponent(owner, prov)
            Case Keys.Tile : Return New TileComponent(owner, prov)
            Case Keys.Dashboard : Return New DashboardComponent(owner, prov)
            Case Keys.DataSource : Return New DataSourceComponent(owner, prov)
            Case Keys.Skill : Return New SkillComponent(owner, prov)
            Case Else
                If key.EndsWith("-group") Then
                    Dim groupType As String = key.Replace("-group", "")
                    Return New GroupComponent(owner, prov, PackageComponentType.AllTypes.Item(groupType))
                End If
        End Select
        Throw New ArgumentException("Unrecognised typekey: " & key, NameOf(key))
    End Function

    ''' <summary>
    ''' Creates a new component based on the given key, reading its data from the
    ''' supplied xml reader.
    ''' </summary>
    ''' <param name="key">The key for which a component is required.</param>
    ''' <param name="reader">The XML reader to draw the data from.</param>
    ''' <param name="ctx">The context to load the given component with</param>
    ''' <returns>The component loaded from the given reader.</returns>
    ''' <exception cref="InvalidOperationException">If a request is made to create a
    ''' new release using this mechanism - this cannot be done since a release
    ''' controls the loading context itself, and cannot be created using a different
    ''' context. See <see cref="clsRelease.Import"/></exception>
    Public Shared Function NewComponent(ByVal owner As OwnerComponent,
     ByVal key As String, ByVal reader As XmlReader,
     ByVal ctx As IComponentLoadingContext) As PackageComponent
        Select Case key
            Case Keys.Release : Throw New InvalidOperationException(
             "Releases cannot be loaded using this mechanism - See clsRelease.Import()")
            Case Keys.Package : Throw New InvalidOperationException(
             "Packages cannot be loaded using this mechanism")

            Case Keys.Process : Return New ProcessComponent(owner, reader, ctx)
            Case Keys.BusinessObject : Return New VBOComponent(owner, reader, ctx)
            Case Keys.Schedule : Return New ScheduleComponent(owner, reader, ctx)
            Case Keys.Queue : Return New WorkQueueComponent(owner, reader, ctx)
            Case Keys.Credential : Return New CredentialComponent(owner, reader, ctx)
            Case Keys.EnvironmentVariable : Return New EnvironmentVariableComponent(owner, reader, ctx)
            Case Keys.Calendar : Return New CalendarComponent(owner, reader, ctx)
            Case Keys.WebService : Return New WebServiceComponent(owner, reader, ctx)
            Case Keys.WebApi : Return New WebApiComponent(owner, reader, ctx)
            Case Keys.Font : Return New FontComponent(owner, reader, ctx)
            Case Keys.Tile : Return New TileComponent(owner, reader, ctx)
            Case Keys.Dashboard : Return New DashboardComponent(owner, reader, ctx)
            Case Keys.DataSource : Return New DataSourceComponent(owner, reader, ctx)
            Case Keys.Skill : Return New SkillComponent(owner, reader, ctx)
            Case Else
                If key.EndsWith("-group") Then
                    Return New GroupComponent(owner, reader, ctx)
                End If
        End Select
        Throw New NotImplementedException()
    End Function

    ''' <summary>
    ''' Gets the localized friendly name For this component type according To the current culture.
    ''' </summary>
    ''' <param name="type">The component type</param>
    ''' <returns>The name of the data type for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyName(type As PackageComponentType, Optional plural As Boolean = False, Optional capitalize As Boolean = False) As String
        If type Is Nothing Then Throw New ArgumentNullException(NameOf(type))
        Return GetLocalizedFriendlyName(type.Label, plural, capitalize)
    End Function

    Public Shared Function GetLocalizedFriendlyName(type As String, Optional plural As Boolean = False, Optional capitalize As Boolean = False) As String

        If String.IsNullOrEmpty(type) Then Throw New ArgumentNullException(NameOf(type))

        Dim currentCulture = CultureInfo.CurrentCulture.Name.ToUpper()
        Dim resxKey As String = "PackageComponentType_" & Regex.Replace(type, "[^a-zA-Z]*", "")
        Dim result As String = If(plural,
            My.Resources.ResourceManager.GetString($"{resxKey}_PluralTitle"),
            My.Resources.ResourceManager.GetString($"{resxKey}_Title"))

        If result Is Nothing Then result = type
        If currentCulture = "DE-DE" OrElse capitalize Then Return result.Capitalize()
        Return result.ToLower()

    End Function

    ''' <summary>
    ''' Override for equals operator - since no new types can be created outside
    ''' this class, they are equal if they represent the same object, otherwise
    ''' they are inequal
    ''' </summary>
    ''' <param name="tp1">The first type to check</param>
    ''' <param name="tp2">The second type to check</param>
    ''' <returns>True if both references refer to the same object, false otherwise.
    ''' </returns>
    ''' <remarks>
    ''' This operator is here primarily so that package component types can be used
    ''' safely in a Select..Case statement a la:
    ''' Select myType
    '''   Case PackageComponentType.Process : ' Do some process-based stuff
    '''   Case PackageComponentType.BusinessObject : ' And some VBOey stuff
    ''' End Select
    ''' <seealso cref="PackageComponentType.Equals"/>
    ''' </remarks>
    Shared Operator =(ByVal tp1 As PackageComponentType, ByVal tp2 As PackageComponentType) As Boolean
        Return (tp1 Is tp2)
    End Operator

    ''' <summary>
    ''' Override for equals operator - since no new types can be created outside
    ''' this class, they are equal if they represent the same object, otherwise
    ''' they are inequal
    ''' </summary>
    ''' <param name="tp1">The first type to check</param>
    ''' <param name="tp2">The second type to check</param>
    ''' <returns>True if both references refer to the same object, false otherwise.
    ''' </returns>
    ''' <remarks>
    ''' <seealso cref="PackageComponentType.Equals"/>
    ''' </remarks>
    Shared Operator <>(ByVal tp1 As PackageComponentType, ByVal tp2 As PackageComponentType) As Boolean
        Return (tp1 IsNot tp2)
    End Operator
#End Region

#Region " Private Members "

    ' The key identifying this type
    Private mKey As String

    ' The label to display for this type - should start with a capital letter
    Private mLabel As String

    ' The plural label for this type - again, starting with a capital letter
    Private mPlural As String

    ' The system type of the ID object used by this type
    Private mIdType As Type

    ' The type which is used in the associated data for this component type
    ' This is needed for the WCF contract so it can serialize/deserialize correctly
    Private mDataType As Type

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new type package component type with the given properties.
    ''' Note that the plural label in this case will just be label with an "s"
    ''' appended onto it.
    ''' Note that this will not register any data type with this component type.
    ''' </summary>
    ''' <param name="key">The key identifying the type</param>
    ''' <param name="label">The label (with uppercase initial letter) representing
    ''' the type in human readable form.</param>
    ''' <param name="idType">The type of ID that this component type uses.</param>
    Private Sub New(ByVal key As String, ByVal label As String, ByVal idType As Type)
        Me.New(key, label, label & "s", idType, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new type package component type with the given properties.
    ''' Note that the plural label in this case will just be label with an "s"
    ''' appended onto it.
    ''' </summary>
    ''' <param name="key">The key identifying the type</param>
    ''' <param name="label">The label (with uppercase initial letter) representing
    ''' the type in human readable form.</param>
    ''' <param name="idType">The type of ID that this component type uses.</param>
    ''' <param name="dataType">The type of the
    ''' <see cref="PackageComponent.AssociatedData"/> used by this type of component
    ''' or null if it doesn't use any associated data.
    ''' </param>
    Private Sub New(ByVal key As String, ByVal label As String, ByVal idType As Type, dataType As Type)
        Me.New(key, label, label & "s", idType, dataType)
    End Sub

    ''' <summary>
    ''' Creates a new type package component type with the given properties.
    ''' Note that this will not register any data type with this component type.
    ''' </summary>
    ''' <param name="key">The key identifying the type</param>
    ''' <param name="label">The label (with uppercase initial letter) representing
    ''' the type in human readable form.</param>
    ''' <param name="pluralLabel">The label to use for plurals of components of
    ''' this type.</param>
    ''' <param name="idType">The type of ID that this component type uses.</param>
    Private Sub New(ByVal key As String, ByVal label As String, ByVal pluralLabel As String, ByVal idType As Type)
        Me.New(key, label, pluralLabel, idType, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new type package component type with the given properties.
    ''' </summary>
    ''' <param name="key">The key identifying the type</param>
    ''' <param name="label">The label (with uppercase initial letter) representing
    ''' the type in human readable form.</param>
    ''' <param name="pluralLabel">The label to use for plurals of components of
    ''' this type.</param>
    ''' <param name="idType">The type of ID that this component type uses.</param>
    ''' <param name="dataType">The type of the
    ''' <see cref="PackageComponent.AssociatedData"/> used by this type of component
    ''' or null if it doesn't use any associated data.
    ''' </param>
    Private Sub New(ByVal key As String, ByVal label As String, ByVal pluralLabel As String, ByVal idType As Type, dataType As Type)
        mKey = key
        mLabel = label
        mPlural = pluralLabel
        mIdType = idType
        mDataType = dataType
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The key identifying this component type.
    ''' This is usually used as an image key to indicate the icon in an image list
    ''' which represents this type
    ''' </summary>
    Public ReadOnly Property Key() As String
        Get
            Return mKey
        End Get
    End Property

    ''' <summary>
    ''' The label with uppercase initial letter, which can represent this type to
    ''' the user.
    ''' </summary>
    Public ReadOnly Property Label() As String
        Get
            Return mLabel
        End Get
    End Property

    ''' <summary>
    ''' The label which can represent multiple components of this type to the user.
    ''' </summary>
    Public ReadOnly Property Plural() As String
        Get
            Return mPlural
        End Get
    End Property

    ''' <summary>
    ''' The system type of the ID which is used to identify components of this type.
    ''' Typically <see cref="Integer"/>, <see cref="Guid"/> or <see cref="String"/>
    ''' </summary>
    Public ReadOnly Property IdType() As Type
        Get
            Return mIdType
        End Get
    End Property

    ''' <summary>
    ''' Gets the data type associated with this package component type or null if it
    ''' has no associated data type.
    ''' </summary>
    Public ReadOnly Property DataType As Type
        Get
            Return mDataType
        End Get
    End Property

#End Region

#Region " Public methods "

    ''' <summary>
    ''' Converts the given string ID into an ID usable in objects of this type.
    ''' </summary>
    ''' <param name="id">The string ID to convert.</param>
    ''' <returns>The ID that can be used by components of this type.</returns>
    Public Function ConvertId(ByVal id As String) As Object
        ' There is almost certainly a nicer way of handling this using generics,
        ' but I don't really have time to explore it right now - if I get chance
        ' I'll review this later.
        Select Case True
            Case mIdType Is GetType(Guid) : Return New Guid(id)
            Case mIdType Is GetType(Integer) : Return Integer.Parse(id)
            Case mIdType Is GetType(String) : Return id
        End Select
        Throw New NotImplementedException("Cannot convert given ID to type: " & mIdType.ToString())
    End Function

    ''' <summary>
    ''' Compares this type to the given type.
    ''' </summary>
    ''' <param name="other">The type to which this component type should be compared.
    ''' </param>
    ''' <returns>A negative number, zero or a positive number depending on whether
    ''' this type is "less than", "equal to" or "greater than" the given type.
    ''' </returns>
    Public Function CompareTo(ByVal other As PackageComponentType) As Integer _
     Implements IComparable(Of PackageComponentType).CompareTo
        Return CompareTypes(Me, other)
    End Function

    ''' <summary>
    ''' Compares this type to the given type.
    ''' </summary>
    ''' <param name="obj">The type to which this component type should be compared.
    ''' This must be an instance of <c>PackageComponentType</c></param>
    ''' <returns>A negative number, zero or a positive number depending on whether
    ''' this type is "less than", "equal to" or "greater than" the given type.
    ''' </returns>
    ''' <exception cref="InvalidCastException">If the given object is not an instance
    ''' of <c>PackageComponentType</c></exception>
    Private Function CompareToObject(ByVal obj As Object) As Integer _
     Implements IComparable.CompareTo
        Return CompareTo(DirectCast(obj, PackageComponentType))
    End Function

    ''' <summary>
    ''' Checks the given object for equality against this object.
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns>True if the given object represents the same object as this package
    ''' component type.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return (obj Is Me)
    End Function

    ''' <summary>
    ''' Gets an integer hash of this object.
    ''' </summary>
    ''' <returns>An integer hash of this object.</returns>
    Public Overrides Function GetHashCode() As Integer
        Dim accum As Integer = mKey.GetHashCode()
        If mLabel IsNot Nothing Then accum = accum Xor mLabel.GetHashCode()
        If mPlural IsNot Nothing Then accum = accum Xor (mPlural.GetHashCode() << 16)
        Return accum
    End Function

    ''' <summary>
    ''' Gets a string representation of this object.
    ''' </summary>
    ''' <returns>A string representing this object.</returns>
    Public Overrides Function ToString() As String
        Return String.Format("Component Type: {0} ({1})", mLabel, mKey)
    End Function

#End Region

End Class
