Imports System.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Server.Domain.Models

Imports System.Runtime.Serialization

''' <summary>
''' Component representing a work queue.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class WorkQueueComponent : Inherits PackageComponent

#Region " Class Scope Definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the font component.
    ''' </summary>
    Public Class MyConflicts

        ''' <summary>
        ''' 'Conflict' which occurs when a font being imported already exists in
        ''' the target environment.
        ''' </summary>
        Public Shared ReadOnly Property Existing As ConflictDefinition
            Get
                If mExisting Is Nothing Then
                    mExisting = GetExistingWorkQueueConflictDefinition()
                Else
                    mExisting.UpdateConflictDefinitionStrings(GetExistingWorkQueueConflictDefinition())
                End If
                Return mExisting
            End Get
        End Property

        Private Shared Function GetExistingWorkQueueConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("Existing",
                                          My.Resources.MyConflicts_AQueueExistsWithThisNameAndWithADifferentConfiguration,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingQueueDetailsWithTheIncomingDetails),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.MyConflicts_DonTImportThisQueue)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mExisting As ConflictDefinition
        
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new work queue component from the given data provider.
    ''' </summary>
    ''' <param name="prov">The provider of the data representing the work queue. This
    ''' constructor expects the following properties to be available :-<list>
    ''' <item>id: Integer</item>
    ''' <item>name: String</item></list></param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", 0), prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new package component representing the given work queue.
    ''' </summary>
    ''' <param name="q">The queue to be represented by this component</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal q As clsWorkQueue)
        Me.New(owner, q.Ident, q.Name)
        Me.AssociatedData = q
    End Sub

    ''' <summary>
    ''' Creates a new package component representing a work queue defined by the
    ''' given properties.
    ''' </summary>
    ''' <param name="id">The ID of the work queue</param>
    ''' <param name="name">The name of the work queue</param>
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
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Flag to indicate whether this component should always be in a group
    ''' </summary>
    Public Overrides ReadOnly Property AlwaysInGroup As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets the work queue associated with this component, or null if no such
    ''' queue is currently associated with it.
    ''' </summary>
    Public ReadOnly Property AssociatedQueue() As clsWorkQueue
        Get
            Return DirectCast(AssociatedData, clsWorkQueue)
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Queue
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return "Workflow - Work Queue Configuration"
        End Get
    End Property

    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

#End Region

#Region " XML Handling "

    ''' <summary>
    ''' Writes this process out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this process should be written.
    ''' </param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        Dim q As clsWorkQueue = DirectCast(AssociatedData, clsWorkQueue)
        If q Is Nothing Then q = gSv.WorkQueueGetQueue(IdAsInteger)
        If q.KeyField <> "" Then writer.WriteAttributeString("key-field", q.KeyField)
        If q.MaxAttempts <> 1 Then writer.WriteAttributeString("max-attempts", q.MaxAttempts.ToString())
    End Sub

    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)
        AssociatedData = New clsWorkQueue() With {
            .Name = Me.Name,
            .KeyField = r("key-field"),
            .MaxAttempts = XmlConvert.ToInt32(If(r("max-attempts"), "1"))
        }
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The work queue associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.WorkQueueGetQueue(IdAsInteger)
    End Function

    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        ' Get the current queue, if there isn't one there or if the one that's there
        ' has the same config details as the incoming queue: no conflict.
        Dim wq As clsWorkQueue = gSv.WorkQueueGetQueue(Name)
        If wq Is Nothing OrElse wq.HasSameDetails(AssociatedQueue) Then _
         Return GetEmpty.ICollection(Of ConflictDefinition)()

        ' Otherwise there's one there and it doesn't match the incoming queue.
        Return GetSingleton.ICollection(Of ConflictDefinition)(MyConflicts.Existing)

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
        Dim mods As IDictionary(Of ModificationType, Object) = Modifications

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            If res.Conflict.Component IsNot Me Then Continue For

            ' Check that the user has selected an option
            Dim opt As ConflictOption = res.ConflictOption
            If opt Is Nothing Then
                errors.Add(Me, My.Resources.WorkQueueComponent_YouMustChooseHowToHandleTheNewQueue0, Me.Name)
            Else
                Select Case opt.Choice
                    Case ConflictOption.UserChoice.Skip : mods.Add(ModificationType.Skip, True)
                    Case ConflictOption.UserChoice.Overwrite : mods.Add(ModificationType.OverwritingExisting, True)
                    Case Else : Throw New BluePrismException(My.Resources.WorkQueueComponent_UnrecognisedOption0, opt.Choice)
                End Select
                res.Passed = True
            End If
            ' There's only one conflict resolution for fonts, since there's only one conflict
            ' No point in going through the rest of the loop.
            Return
        Next

    End Sub

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the data in the
    ''' given component differs from the data in this component. This implementation
    ''' checks the <see cref="m:PackageComponent.Differs">base implementation</see>
    ''' and the contents of the group to ensure that the components in the group
    ''' match.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True
        Dim wq As clsWorkQueue = AssociatedQueue
        If wq Is Nothing Then Throw New NoSuchElementException(
         My.Resources.WorkQueueComponent_NoQueueWithTheID0WasFound, Id)

        ' If the details don't match... well, that there's a difference.
        Return (Not wq.HasSameDetails(DirectCast(comp, WorkQueueComponent).AssociatedQueue))

    End Function

    ''' <summary>
    ''' Returns any group assignments for the queue represented by this component.
    ''' </summary>
    Public Overrides Function GetGroupInfo() As IDictionary(Of Guid, String)
        Dim mem As New QueueGroupMember()
        mem.Id = Id
        Return gSv.GetPathsToMember(mem)
    End Function

#End Region

End Class

