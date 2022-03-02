Imports System.Xml

Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports System.Runtime.Serialization
Imports System.Reflection
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.Scheduling
Imports BluePrism.Server.Domain.Models


' An idea still in formation
'''' <summary>
'''' Class detailing a package component of a particular type of ID
'''' </summary>
'''' <typeparam name="TId">The type of ID for this component</typeparam>
'''' <typeparam name="TValue">The type of value for this component</typeparam>
'''' <remarks></remarks>
'Public MustInherit Class PackageComponent(Of TId, TValue) : Inherits PackageComponent

'   Public MustOverride Overrides ReadOnly Property TypeKey() As String

'End Class

''' <summary>
''' Basic component class.
''' </summary>
<Serializable, DataContract(IsReference:=True, [Namespace]:="bp"), KnownType("GetAllKnownTypes")>
Public MustInherit Class PackageComponent : Implements IComparable, ICloneable

#Region " Class-scope members "

    ''' <summary>
    ''' Gets all known types required for release manager classes.
    ''' This includes all the concrete subclasses of this class, as well as all the
    ''' registered <see cref="PackageComponentType.DataType">DataType</see>s of
    ''' those components.
    ''' </summary>
    ''' <returns>An enumerable over all known types in relman</returns>
    Protected Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
        ' Get all the valid package components
        Dim types As New HashSet(Of Type)(
         Assembly.GetExecutingAssembly().GetConcreteSubclasses(Of PackageComponent)()
        )
        ' Union with all the data types of associated data used by the components
        types.UnionWith(
            From t In PackageComponentType.AllTypes.Values
            Where t.DataType IsNot Nothing
            Select t.DataType
        )

        Return types
    End Function

    ''' <summary>
    ''' The context to use when cloning a complex set of package components. This
    ''' is here to allow a full set of relationships between disparate components
    ''' (eg. group members / parents / dependents / etc).
    ''' </summary>
    ''' <seealso cref="Clone">See the Clone(CloningContext) method and overrides to
    ''' see the context in use.</seealso>
    Protected Class CloningContext

        ' The original (ie. not a clone) of the component which is the source of the
        ' clone operation
        Private mComp As PackageComponent

        ' The clones, dynamically built up as they are required.
        Private mCloneArmy As _
         New SortedDictionary(Of PackageComponentType, IDictionary(Of Object, PackageComponent))

        ''' <summary>
        ''' Creates a new cloning context based around the given <em>original</em>
        ''' component.
        ''' </summary>
        ''' <param name="original">The component to use as the base for this context.
        ''' Note that this should be the original component object, not a clone
        ''' </param>
        Public Sub New(ByVal original As PackageComponent)
            mComp = original
        End Sub

        ''' <summary>
        ''' Gets the component from this context of the given type and with the given
        ''' ID. If the component has already been cloned in this context, that
        ''' component is returned, otherwise it is cloned and stored into this
        ''' context before being returned.
        ''' </summary>
        ''' <param name="type">The type of component required.</param>
        ''' <param name="id">The ID of the component required.</param>
        ''' <returns>The cloned component from this context, or null if no component
        ''' could be found in this context or in the source component</returns>
        Default Public Property Item( _
         ByVal type As PackageComponentType, ByVal id As Object) As PackageComponent
            Get
                Dim idMap As IDictionary(Of Object, PackageComponent) = Nothing
                If Not mCloneArmy.TryGetValue(type, idMap) Then
                    idMap = New Dictionary(Of Object, PackageComponent)
                    mCloneArmy(type) = idMap
                End If
                Dim comp As PackageComponent = Nothing
                If Not idMap.TryGetValue(id, comp) Then
                    ' Not in the cached cloned components, try the group, if that's what it is
                    Dim gp As ComponentGroup = TryCast(mComp, ComponentGroup)
                    If gp IsNot Nothing Then
                        For Each uncloned As PackageComponent In gp
                            If uncloned.Type = type AndAlso Object.Equals(uncloned.Id, id) Then
                                ' We set the idMap entry to null so that if the uncloned.Clone()
                                ' method looks up its own entry in this context, it doesn't bring
                                ' the process to a screaming StackOverflow-induced halt
                                idMap(id) = Nothing
                                comp = uncloned.Clone(Me)
                                Exit For
                            End If
                        Next
                    End If
                    idMap(id) = comp
                End If

                Return comp

            End Get
            Set(ByVal value As PackageComponent)
                Dim idMap As IDictionary(Of Object, PackageComponent) = Nothing
                If Not mCloneArmy.TryGetValue(type, idMap) Then
                    idMap = New Dictionary(Of Object, PackageComponent)
                    mCloneArmy(type) = idMap
                End If
                idMap(id) = value
            End Set
        End Property

    End Class

    ''' <summary>
    ''' An empty, read only collection containing no components.
    ''' </summary>
    Protected Shared ReadOnly NoComponents As ICollection(Of PackageComponent) = New PackageComponent() {}

    ''' <summary>
    ''' Checks if the given name exists within the given collection of package
    ''' components.
    ''' </summary>
    ''' <param name="name">The name to search for.</param>
    ''' <param name="coll">The collection to search</param>
    ''' <returns>True if the name exists, False otherwise.</returns>
    Public Shared Function NameExists(Of T As PackageComponent)( _
     ByVal name As String, ByVal coll As ICollection(Of T)) As Boolean
        For Each c As PackageComponent In coll
            If c.Name = name Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' The modification types used to indicate to the database handling code any
    ''' modifications which are necessary as part of the import transaction, or ways
    ''' of handling the import determined by the user's choices in conflict
    ''' resolution.
    ''' Note that each component can only have one modification of each type
    ''' registered on it, even if there are multiple conflicts in evidence for that
    ''' component.
    ''' </summary>
    Public Enum ModificationType

        ''' <summary>
        ''' Change incoming object's ID.
        ''' The accompanying data should be the ID of the object, either an integer,
        ''' a GUID or a string
        ''' </summary>
        IncomingId

        ''' <summary>
        ''' Change incoming object's name
        ''' The accompanying data should be the name to give to the incoming object,
        ''' ie. a string
        ''' </summary>
        IncomingName

        ''' <summary>
        ''' Change existing object's name
        ''' The accompanying data should be the name to give to the existing object,
        ''' ie. a string.
        ''' </summary>
        ExistingName

        ''' <summary>
        ''' Publish the object (only applicable to processes).
        ''' No accompanying data is required for this modification.
        ''' </summary>
        Publish


        ''' <summary>
        ''' Retire the object (applicable to processes, objects, schedules?)
        ''' No accompanying data is required for this modification.
        ''' </summary>
        Retire

        ''' <summary>
        ''' Overwrite the existing object. The absence of this implies that a new
        ''' object is being created - ie. if an object exists with the same ID/name
        ''' as the incoming object (after other modifications have been applied)
        ''' and this modification is not present, it is an error condition.
        ''' No accompanying data is required for this modification.
        ''' </summary>
        OverwritingExisting

        ''' <summary>
        ''' Don't import the incoming object - skip it, but continue to import other
        ''' components in the same release.
        ''' </summary>
        Skip

    End Enum

    ''' <summary>
    ''' Enumeration of the sources whence this component's data can be drawn
    ''' </summary>
    Public Enum DataSource

        ''' <summary>
        ''' The source of the data has not been specified
        ''' </summary>
        Unspecified

        ''' <summary>
        ''' The data is retrieved from the database - ie. this component is
        ''' resident in the current environment.
        ''' </summary>
        Database

        ''' <summary>
        ''' The data is retrieved from XML - ie. this component is imported from
        ''' a file / another environment.
        ''' </summary>
        Xml

    End Enum

#End Region

#Region " Private Members "

    ' The source of the data associated with this component.
    <DataMember>
    Private mSource As DataSource

    ' The ID of this component. This can be integer, Guid or string.
    <DataMember>
    Private mId As Object

    ' The name of this component.
    <DataMember>
    Private mName As String

    ' The description of this component.
    <DataMember>
    Private mDescription As String

    ' The nearest owner of this component - this could be a package or a release
    ' It should usually be set - generally package components cannot exist in a
    ' vaccuum - unless they are owner components themselves.
    <DataMember>
    Private mOwner As OwnerComponent

    ' Associated data for this component - not serialized since it could be anything
    <DataMember>
    Private mAssocData As Object

    ' The modifications to apply, either to this object or the existing object.
    ' This is context-driven, eg. an entry of ModificationType.IncomingName means
    ' that the associated value is a string which the incoming object should be
    ' renamed to. An entry of ModificationType.ExistingName also means that the
    ' associated value is a string, but the existing object should be renamed to
    ' it instead.
    <DataMember>
    Private mModifications As New Dictionary(Of ModificationType, Object)

    ' The conflicts that apply to this component
    <NonSerialized()> _
    Private mConflicts As ICollection(Of Conflict)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty component.
    ''' </summary>
    Protected Sub New(ByVal owner As OwnerComponent)
        Me.New(owner, Nothing, DirectCast(Nothing, String))
    End Sub

    ''' <summary>
    ''' Creates a new component with the given ID and name.
    ''' </summary>
    ''' <param name="id">The ID of the component.</param>
    ''' <param name="name">The name of the component</param>
    ''' <remarks>The assumption with this constructor is that the source is from
    ''' the database.</remarks>
    Protected Sub New(ByVal owner As OwnerComponent, ByVal id As Object, ByVal name As String)
        mOwner = owner
        mId = id
        mName = name
        mSource = DataSource.Database
    End Sub

    ''' <summary>
    ''' Creates a new XML-sourced component from data in the given reader, using the
    ''' specified loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Protected Sub New(ByVal owner As OwnerComponent, _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        mOwner = owner
        mSource = DataSource.Xml
        Me.FromXml(reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The nearest owner of this component - usually a package or a release.
    ''' This may be null if this component is a package or release, or if it is not
    ''' currently owned by a component (eg. if it is representing a component in
    ''' the environment which is not yet assigned to a release / package.
    ''' </summary>
    Public Overridable Property Owner() As OwnerComponent
        Get
            Return mOwner
        End Get
        Set(ByVal value As OwnerComponent)
            mOwner = value
        End Set
    End Property

    ''' <summary>
    ''' <para>
    ''' Checks if the ID for this component is global - this affects whether the
    ''' ID is used in a <see cref="Differs"/> check for this component or not - if
    ''' the ID is local, then it is ignored for the purpose of the check. If it is
    ''' global, then it is checked and a different ID will cause the method to report
    ''' that the component is different.
    ''' </para><para>
    ''' Generally speaking, the ID is global if it is a GUID or a String - it is not
    ''' global if it is an integer, since this suggests an IDENTITY field, and it is
    ''' thus local to the database in which it was created.
    ''' Any exceptions to this rule should override this method to make it so.
    ''' </para>
    ''' </summary>
    Protected Overridable ReadOnly Property IsIdGlobal() As Boolean
        Get
            Return (TypeOf Id Is Guid OrElse TypeOf Id Is String)
        End Get
    End Property

    ''' <summary>
    ''' The source for the data for this component.
    ''' </summary>
    Public Property Source() As DataSource
        Get
            Return mSource
        End Get
        Protected Set(ByVal value As DataSource)
            mSource = value
        End Set
    End Property

    ''' <summary>
    ''' The key of this component type - used as an image key and for order lookups
    ''' amongst other things.
    ''' <seealso cref="PackageComponentType.Keys"/>
    ''' </summary>
    Public Overridable ReadOnly Property TypeKey() As String
        Get
            Return Type.Key
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public MustOverride ReadOnly Property Type() As PackageComponentType

    ''' <summary>
    ''' The type key to use for the XML Namespace representing this component.
    ''' Usually the same as the <see cref="TypeKey"/>
    ''' </summary>
    Public Overridable ReadOnly Property NamespaceTypeKey() As String
        Get
            Return TypeKey
        End Get
    End Property

    ''' <summary>
    ''' Gets the XML Namespace for this component.
    ''' </summary>
    Public ReadOnly Property XmlNamespace() As String
        Get
            Return String.Format("http://www.blueprism.co.uk/product/{0}", NamespaceTypeKey)
        End Get
    End Property

    ''' <summary>
    ''' The name of this component.
    ''' </summary>
    Public Overridable Property Name() As String
        Get
            If mName Is Nothing Then Return "" Else Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The ID of this component. This can be any object, but inbuilt support is
    ''' available for integers, GUIDs and strings.
    ''' </summary>
    Public Overridable Property Id() As Object
        Get
            Return mId
        End Get
        Set(ByVal value As Object)
            mId = value
        End Set
    End Property

    ''' <summary>
    ''' The ID of this component as a GUID or Guid.Empty if it has no ID, or if the
    ''' ID is not a GUID.
    ''' </summary>
    Public ReadOnly Property IdAsGuid() As Guid
        Get
            Dim o As Object = Me.Id
            If Not (TypeOf o Is Guid) Then Return Guid.Empty
            Return DirectCast(o, Guid)
        End Get
    End Property

    ''' <summary>
    ''' The ID of this component as an integer, or 0 if it has no ID or if the ID
    ''' is not an integer.
    ''' </summary>
    Public ReadOnly Property IdAsInteger() As Integer
        Get
            Dim o As Object = Me.Id
            If Not (TypeOf o Is Integer) Then Return 0
            Return DirectCast(o, Integer)
        End Get
    End Property

    ''' <summary>
    ''' The ID of this component as a string, or "" if it has no ID or if the ID
    ''' is not a string.
    ''' </summary>
    Public ReadOnly Property IdAsString() As String
        Get
            Dim o As Object = Me.Id
            If Not (TypeOf o Is String) Then Return ""
            Return DirectCast(Me.Id, String)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this component has an ID or not.
    ''' </summary>
    Public ReadOnly Property HasId() As Boolean
        Get
            Return Id IsNot Nothing AndAlso ( _
             IdAsInteger <> 0 OrElse IdAsGuid <> Nothing OrElse IdAsString <> "" _
            )
        End Get
    End Property

    ''' <summary>
    ''' The description of this component, or an empty string if it has no
    ''' description.
    ''' </summary>
    Public Property Description() As String
        Get
            If mDescription Is Nothing Then Return "" Else Return mDescription
        End Get
        Set(ByVal value As String)
            mDescription = value
        End Set
    End Property

    ''' <summary>
    ''' A default group to use for this package component if it is not already in a
    ''' group.
    ''' Components do not have a default group by default, so subclasses which
    ''' require one should override this property and return their appropriate group.
    ''' Note that repeated calls to this will usually create a new default group
    ''' each time - the configuration of the group is down to the component, the
    ''' management of the group's lifetime and contents is down to the caller.
    ''' </summary>
    Public Overridable ReadOnly Property DefaultGroup() As ComponentGroup
        Get
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Flag to indicate whether this component should always be in a group
    ''' </summary>
    Public Overridable ReadOnly Property AlwaysInGroup() As Boolean
        Get
            Return (DefaultGroup IsNot Nothing)
        End Get
    End Property

    ''' <summary>
    ''' The data associated with this component. This will be dependent on the type
    ''' of component that this object represents, eg. for a
    ''' <see cref="WorkQueueComponent"/>, this could be a <see cref="clsWorkQueue"/>
    ''' object whereas for a <see cref="ProcessComponent"/>, this may be a <see
    ''' cref="clsProcess"/> object.
    ''' </summary>
    Public Property AssociatedData() As Object
        Get
            If mAssocData Is Nothing And mSource = DataSource.Database Then
                mAssocData = LoadData()
            End If
            Return mAssocData
        End Get
        Set(ByVal value As Object)
            mAssocData = value
        End Set
    End Property

    ''' <summary>
    ''' The modifications necessary for this process component
    ''' </summary>
    Public ReadOnly Property Modifications() As IDictionary(Of ModificationType, Object)
        Get
            If mModifications Is Nothing Then _
             mModifications = New Dictionary(Of ModificationType, Object)

            Return mModifications
        End Get
    End Property

    ''' <summary>
    ''' Gets the names of any permissions required by a user to import a component of
    ''' this type.
    ''' This implementation returns null, indicating that no permission is required
    ''' in order to import this component. Subclasses for which that assertion does
    ''' not hold should override this property.
    ''' </summary>
    ''' <remarks>If the rules are more complex than a single import permission in
    ''' order to import this component (eg. environment variables require either of
    ''' two permissions), the <see cref="AddMissingImportPermissions"/> method should
    ''' be overridden by the affected class, rather than trying to shoehorn the rules
    ''' into this single string property.</remarks>
    Public Overridable ReadOnly Property ImportPermission() As String
        Get
            Return Nothing
        End Get
    End Property


    ''' <summary>
    ''' Gets the name of the data type updated by this component. This is used to 
    ''' increment the version data which is monitored by other parts of the application
    ''' to check for data changes.
    ''' </summary>
    ''' <returns>The name of the data that should be marked as having changed.</returns>
    ''' <remarks>Possible values are defined in <c cref="DataNames" /> </remarks>
    Public Overridable ReadOnly Property VersionDataName() As String
        Get
            Return Nothing
        End Get
    End Property

#End Region

#Region " Component-specific Methods "

    ''' <summary>
    ''' Loads the data for this component.
    ''' The default implementation assumes no associated data for this component -
    ''' ie. it loads nothing.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overridable Function LoadData() As Object
        Return Nothing
    End Function

    ''' <summary>
    ''' Adds the permissions which are required by this component that the currently
    ''' logged in user is missing.
    ''' Generally, this collection should contain the names of the required
    ''' permissions, but if the rules are more complex (eg. either of 2 permissions
    ''' will satisfy the requirement), the output can be tailored to match the rule,
    ''' but each type of component should produce the same output for the same user.
    ''' </summary>
    ''' <param name="perms">The collection of permissions to add any permission name,
    ''' required to import this component, that the current user is missing.</param>
    Public Overridable Sub AddMissingImportPermissions(ByVal perms As ICollection(Of String))
        Dim perm As String = ImportPermission
        If perm IsNot Nothing AndAlso Not User.Current.HasPermission(perm) Then
            perms.Add(perm)
        End If
    End Sub

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the exportable data
    ''' in the given component differs from the data in this component. The base
    ''' implementation just checks type, ID (<see cref="IsIdGlobal">global</see>
    ''' only), name and description - ie. not the transitory 'state' of the component
    ''' (modification map, conflict collection etc). Subclasses should extend this to
    ''' test their specific data, but should not test any state data that they may
    ''' hold.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overridable Function Differs(ByVal comp As PackageComponent) As Boolean
        Return ( _
         Type <> comp.Type _
         OrElse Name <> comp.Name _
         OrElse (IsIdGlobal AndAlso Not Object.Equals(Id, comp.Id)) _
         OrElse Description <> comp.Description _
        )
    End Function

    ''' <summary>
    ''' Gets the dependents of this component.
    ''' </summary>
    ''' <returns>A collection of components which this component is dependent upon
    ''' (at this moment in time - if the data that this component represents changes
    ''' then repeated calls to this function may generate different results)
    ''' </returns>
    Public Overridable Function GetDependents() As ICollection(Of PackageComponent)
        Return NoComponents
    End Function

    ''' <summary>
    ''' Gets any group assignments for this component.
    ''' </summary>
    ''' <returns>A collection of groups (IDs and full paths) that contain this item.
    ''' </returns>
    Public Overridable Function GetGroupInfo() As IDictionary(Of Guid, String)
        Return New Dictionary(Of Guid, String)()
    End Function

    ''' <summary>
    ''' Gets the conflict definitions which apply between this component and the
    ''' current database.
    ''' </summary>
    ''' <returns>A collection of conflict definitions which </returns>
    Public ReadOnly Property Conflicts() As ICollection(Of Conflict)
        Get
            If mConflicts Is Nothing Then
                mConflicts = New List(Of Conflict)
                For Each defn As ConflictDefinition In FindConflicts()
                    mConflicts.Add(New Conflict(Me, defn))
                Next
            End If
            Return mConflicts
        End Get
    End Property

    ''' <summary>
    ''' Finds the conflicts in this component, returning them in the specified
    ''' collection - this should not cache the conflict definitions - this is
    ''' handled at package component level.
    ''' </summary>
    ''' <returns>The collection of conflict definitions which were found in this
    ''' package component.</returns>
    ''' <remarks>This may well end up being an abstract method - currently it isn't
    ''' while I work on a limited set of components (processes, vbos, groups), but I
    ''' may make it so as work continues.</remarks>
    Protected Overridable Function FindConflicts() As ICollection(Of ConflictDefinition)
        Return GetEmpty.ICollection(Of ConflictDefinition)()
    End Function

    ''' <summary>
    ''' Gets the delegate which is responsible for applying resolutions for this
    ''' component. Subclasses must implement this if they produce conflicts, in order
    ''' to handle those conflicts being resolved.
    ''' </summary>
    ''' <remarks>
    ''' Generally speaking, if the resolution of conflicts requires any
    ''' cross-component validation (eg. 2 processes being renamed needs to check that
    ''' they have not been renamed to the same thing), the delegate should be a
    ''' static method which performs the cross-component validation and then calls
    ''' the instance single-component validation itself. This would ensure that the
    ''' delegate is called once only for all components of the same type.
    ''' 
    ''' If the resolution requires single-component validation only, this should be
    ''' an instance method to ensure that the delegate is called once for each
    ''' instance of the component, rather than once for all components of that type.
    ''' </remarks>
    Public Overridable ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the data handler for the given conflict option. The default
    ''' implementation just calls <see cref="ConflictOption.CreateHandler"/> and
    ''' returns the result - subclasses may need to check some external state before
    ''' deciding on the handler to return.
    ''' </summary>
    ''' <param name="c">The conflict</param>
    ''' <param name="o">The conflict option chosen.</param>
    ''' <returns>The handler to be used for that particular option in the given
    ''' conflict, or null if no further data is required.</returns>
    Public Overridable Function GetHandlerForOption( _
     ByVal c As Conflict, ByVal o As ConflictOption) As ConflictDataHandler
        Return o.CreateHandler()
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
    ''' <remarks>TODO: We need something like this because a small variation on this
    ''' bit of code is popping up in each of the component classes, but currently
    ''' this isn't plugged into anything - ie. it is a work in progress.</remarks>
    Private Sub ApplyDefaultConflictResolutions(ByVal rel As clsRelease, _
     ByVal resolutions As ICollection(Of ConflictResolution), ByVal errors As clsErrorLog)
        Dim mods As IDictionary(Of ModificationType, Object) = Modifications

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            If res.Conflict.Component IsNot Me Then Continue For

            ' Check that the user has selected an option
            Dim opt As ConflictOption = res.ConflictOption
            If opt Is Nothing Then
                errors.Add(Me, My.Resources.PackageComponent_YouMustChooseHowToHandleThe01, PackageComponentType.GetLocalizedFriendlyName(Type), Name)
            Else
                Select Case opt.Choice
                    Case ConflictOption.UserChoice.Skip
                        mods.Add(ModificationType.Skip, True)
                    Case ConflictOption.UserChoice.Overwrite
                        mods.Add(ModificationType.OverwritingExisting, True)
                    Case Else
                        Throw New BluePrismException(My.Resources.PackageComponent_UnrecognisedOption0, opt.Choice)
                End Select
                res.Passed = True
            End If
            ' There's only one conflict resolution for fonts, since there's only one conflict
            ' No point in going through the rest of the loop.
            Return
        Next

    End Sub

    ''' <summary>
    ''' Optional hook that performs additional initialisation on the component prior to import
    ''' </summary>
    ''' <param name="scheduler">The IScheduler instance to use during the import</param>
    Public Overridable Sub InitialiseForImport(scheduler As IScheduler)

    End Sub

#Region " Xml Handling "

    ''' <summary>
    ''' Writes this component out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this component should be written.
    ''' </param>
    ''' <exception cref="BluePrismException">If any errors should occur white
    ''' attempting to write this component to XML. The InnerException will contain
    ''' the source of the error.</exception>
    Public Sub ToXml(ByVal writer As XmlWriter)
        Try

            WriteXmlHead(writer)
            Try
                WriteXmlBody(writer)
            Finally
                WriteXmlTail(writer)
            End Try

        Catch ex As Exception
            ' Chain it into a BP Exception with a (probably) better message
            Throw New BluePrismException(ex,
              My.Resources.PackageComponent_FailedWritingXMLForType0ID1Name2ErrorMessage3,
              TypeKey, Id, Name, ex.Message)

        End Try
    End Sub

    ''' <summary>
    ''' Writes the body of this component to the given writer.
    ''' 
    ''' At the time that this method is called, the common start element exists and
    ''' the writer is in such a state that 'WriteAttributeString()' calls or such
    ''' like will be written to that element.
    ''' 
    ''' Note that the ID and name are automatically written before this method is
    ''' called.
    ''' </summary>
    ''' <param name="writer">The writer to which this component should be written.
    ''' </param>
    Protected Overridable Sub WriteXmlBody(ByVal writer As XmlWriter)
        writer.WriteComment(String.Format(
         "TODO: Override WriteXmlBody() in {0} class", Me.GetType()))
    End Sub

    ''' <summary>
    ''' Writes the head of the XML for this component to the given writer. This
    ''' leaves an element open that subclasses can write to if necessary in the
    ''' XML Body writer, though overriding WriteXmlHead() would probably be a more
    ''' logical place to put it if it's head information.
    ''' 
    ''' The rule of thumb is that if it is part of the component structure itself,
    ''' then it can go in the head. If it's part of the object that the component
    ''' represents, then it should go in the body, so ID, Name, Retired, Published
    ''' are all properties of a process component, ProcessXml is part of the process
    ''' that the component represents.
    ''' </summary>
    ''' <param name="writer">The writer to which the head of the XML representing
    ''' this component should be written.</param>
    Protected Overridable Sub WriteXmlHead(ByVal writer As XmlWriter)
        writer.WriteStartElement(TypeKey, XmlNamespace) ' envelope
        If Me.Id IsNot Nothing Then writer.WriteAttributeString("id", Id.ToString())
        If Me.Name IsNot Nothing Then writer.WriteAttributeString("name", Me.Name)
    End Sub

    ''' <summary>
    ''' Writes the tail of the XML for this component to the given writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this component's XML tail should
    ''' be written.</param>
    Protected Overridable Sub WriteXmlTail(ByVal writer As XmlWriter)
        writer.WriteEndElement() ' envelope
    End Sub

    ''' <summary>
    ''' Reads the XML head for this component from the given reader
    ''' </summary>
    ''' <param name="reader">The XML reader from which the data should be drawn.
    ''' </param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' package component.</param>
    Protected Overridable Sub ReadXmlHead(
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        If Not reader.IsStartElement(Me.TypeKey, XmlNamespace) Then Throw New BluePrismException(
         My.Resources.PackageComponent_InvalidPositionToReadXMLFrom0, reader.Name)

        mId = ConvertId(reader.GetAttribute("id"))
        mName = reader.GetAttribute("name")
    End Sub

    ''' <summary>
    ''' Reads the XML body for this component from the given reader
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the body data for this
    ''' component.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' package component.</param>
    Protected Overridable Sub ReadXmlBody( _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
    End Sub

    ''' <summary>
    ''' Reads the XML tail for this component.
    ''' </summary>
    ''' <param name="reader">The reader providing the XML.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' package component.</param>
    Protected Overridable Sub ReadXmlTail( _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
    End Sub

    ''' <summary>
    ''' Populates this package component from XML provided by the given reader
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the XML.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' package component.</param>
    Protected Overridable Sub FromXml(ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        ReadXmlHead(reader, ctx)
        ReadXmlBody(reader, ctx)
        ReadXmlTail(reader, ctx)
    End Sub

#End Region

    ''' <summary>
    ''' Adds a reference to this process data to the given writer.
    ''' </summary>
    ''' <param name="writer">The XML writer to which this object should be added as
    ''' a reference.</param>
    Public Sub AppendReference(ByVal writer As XmlWriter)
        If Id IsNot Nothing Then
            writer.WriteStartElement(Me.TypeKey)
            writer.WriteAttributeString("id", Id.ToString())
            writer.WriteEndElement() ' process
        End If
    End Sub

    ''' <summary>
    ''' Converts the given string ID (typically read from XML) into an ID object
    ''' valid for this component.
    ''' </summary>
    ''' <param name="id">The ID to check for</param>
    ''' <returns>The ID object for this component</returns>
    Protected Overridable Function ConvertId(ByVal id As String) As Object
        Return Type.ConvertId(id)
    End Function

#End Region

#Region " Object Override Methods "

    ''' <summary>
    ''' Checks if this component is equal to the given object.
    ''' It is equal if it is a component of the same type, and with the same ID and
    ''' name as this component.
    ''' </summary>
    ''' <param name="obj">The object to test to see if it is equal to this component
    ''' </param>
    ''' <returns>True if the given object is the same type as this component, and
    ''' contains the same name and ID as this component; False otherwise.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        If obj Is Nothing Then Return False

        If Object.ReferenceEquals(obj, Me) Then Return True

        Dim data As PackageComponent = TryCast(obj, PackageComponent)

        Return data IsNot Nothing _
            AndAlso Name = data.Name _
            AndAlso data.GetType() Is Me.GetType() _
            AndAlso Object.Equals(Id, data.Id)
    End Function

    ''' <summary>
    ''' Gets a hash code for this component. This is a function of the type, name
    ''' and ID of this component.
    ''' </summary>
    ''' <returns>An integer hash for this component.</returns>
    Public Overrides Function GetHashCode() As Integer

        ' Get a hash from the ID.
        Dim idHash As Integer = 0
        If Id IsNot Nothing Then idHash = Id.GetHashCode()

        Return Me.GetType().GetHashCode() Xor Name.GetHashCode() Xor idHash
    End Function

    ''' <summary>
    ''' Gets a string representation of this component.
    ''' </summary>
    ''' <returns>This component in the form of a string. Currently, this is in the
    ''' form: <c>"{Type}: {Name}({ID})</c>, eg. "QueueData: Big Queue(4)"</returns>
    Public Overrides Function ToString() As String
        Return String.Format("{0}: {1}({2})", _
         Me.GetType().Name, Me.Name, Me.Id)
    End Function

#End Region

#Region " IComparable Implementation "

    ''' <summary>
    ''' Compares this component to the given object.
    ''' The order of a component is determined by the following properties:- <list>
    ''' <item><see cref="Type"/></item>
    ''' <item><see cref="Name"/></item>
    ''' <item><see cref="Id"/> - Note that this will only work if this component and
    ''' the testing object have Id values which are the same type, and that type
    ''' implements the IComparable interface.</item>
    ''' <item><see cref="Description"/></item></list>
    ''' </summary>
    ''' <param name="obj">The object to compare this component to.</param>
    ''' <returns>A positive integer, zero, or a negative integer if this component is
    ''' greater than, equal to or less than the given object, respectively.</returns>
    ''' <exception cref="InvalidCastException">If the given object was not an
    ''' instance of ComponentData.</exception>
    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
        Dim ci As PackageComponent = DirectCast(obj, PackageComponent)

        Dim res As Integer = Type.CompareTo(ci.Type)
        If res = 0 Then res = Name.CompareTo(ci.Name)
        If res = 0 AndAlso Id IsNot Nothing AndAlso ci.Id IsNot Nothing Then
            If Id.GetType() Is ci.Id.GetType() AndAlso TypeOf (Id) Is IComparable Then
                res = DirectCast(Id, IComparable).CompareTo(ci.Id)
            End If
        End If
        If res = 0 Then res = Description.CompareTo(ci.Description)
        Return res
    End Function

#End Region

#Region " Cloning & ICloneable Implementation "

    ''' <summary>
    ''' Clones this component and a disconnected copy of its owner group (with no
    ''' members other than this component)
    ''' </summary>
    ''' <returns>A clone of this component.</returns>
    Private Function CloneObject() As Object Implements System.ICloneable.Clone
        Return Clone()
    End Function

    ''' <summary>
    ''' Clones this component and a <em>disconnected</em> copy of its owner group,
    ''' which will contain the clone of this component only on return from this
    ''' method.
    ''' </summary>
    ''' <returns>A deep clone of this component.</returns>
    ''' <remarks>This is not virtual by design, rather than by default - subclasses
    ''' should override the <see cref="M:PackageComponent.Clone(CloningContext)">
    ''' Clone(CloningContext)</see> method rather than this one to create clones of
    ''' themselves, since they provide a mechanism with which other component
    ''' relations can be accessed.</remarks>
    Public Function Clone() As PackageComponent
        Return Clone(New CloningContext(Me))
    End Function

    ''' <summary>
    ''' Clone this group using the specified cloning context.
    ''' The context provides the point from which components which have already
    ''' been cloned within the current context can be drawn. If they don't already
    ''' exist, the will be cloned on access, so that the full object map can be
    ''' correctly created with all references intact.
    ''' </summary>
    ''' <param name="ctx">The context in which this clone operation is taking place.
    ''' </param>
    ''' <returns>A clone of this component group.</returns>
    Protected Overridable Function Clone(ByVal ctx As CloningContext) As PackageComponent

        ' Member clone first
        Dim copy As PackageComponent = DirectCast(MemberwiseClone(), PackageComponent)

        ' then set into the context in case it's required elsewhere
        ctx(Type, Id) = copy

        Return copy

    End Function


    ''' <summary>
    ''' Clones this package component, disregarding its group membership.
    ''' </summary>
    ''' <returns>A disconnected clone of this component.</returns>
    Public Overridable Function CloneDisconnected() As PackageComponent
        Dim pc As PackageComponent = DirectCast(Me.MemberwiseClone(), PackageComponent)
        pc.mOwner = Nothing
        Return pc
    End Function

#End Region

End Class
