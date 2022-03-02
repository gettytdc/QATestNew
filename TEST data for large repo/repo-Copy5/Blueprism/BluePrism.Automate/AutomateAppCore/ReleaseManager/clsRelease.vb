Imports System.Xml
Imports System.IO
Imports System.IO.Compression
Imports System.Text.RegularExpressions

Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.ReleaseManager.Components
Imports BluePrism.Scheduling
Imports BluePrism.Skills
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.ProcessComponent
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Class to represent a release
''' </summary>
<Serializable, DataContract([Namespace]:="bp", IsReference:=True)>
Public Class clsRelease : Inherits OwnerComponent

#Region " Class-scope definitions "

    ''' <summary>
    ''' The extension appended to a filename for a release.
    ''' </summary>
    Public Const FileExtension As String = "bprelease"

    ''' <summary>
    ''' Regex to find the bare name of the process file without the unnecessary
    ''' prefix noise.
    ''' </summary>
    Private Shared ReadOnly LegacyFileNameRegex As _
     New Regex("^BPA (?:Object|Process) - (.*)$")

    ''' <summary>
    ''' Class to provide a component loading context based around a release.
    ''' </summary>
    Private Class ReleaseContext : Inherits BaseComponentLoadingContext

        ' The release providing the context
        Private mRelease As clsRelease

        ''' <summary>
        ''' Creates a new release-based loading context.
        ''' </summary>
        ''' <param name="rel"></param>
        Public Sub New(ByVal rel As clsRelease)
            mRelease = rel
        End Sub

        ''' <summary>
        ''' Finds all components of the specified type within this context.
        ''' this group.
        ''' </summary>
        ''' <param name="tp">The type of component required.</param>
        ''' <returns>A non-null collection of all components which are of the
        ''' required type.</returns>
        Public Overrides Function GetAllComponents(
         ByVal tp As PackageComponentType) As ICollection(Of PackageComponent)

            Dim comps As New List(Of PackageComponent)
            For Each comp As PackageComponent In mRelease.Members
                If comp.Type = tp Then comps.Add(comp)
            Next
            Return comps

        End Function

    End Class

#End Region

#Region " Private Members "

    ' Flag indicating if this release is local (ie. created from a package in this
    ' environment) or forein (ie. imported from another environment)
    <DataMember>
    Private mLocal As Boolean

    ' The package (version) that this release was built from
    <DataMember>
    Private mPackage As clsPackage

    ' The monitor to which progress should be reported when importing a release
    Private mProgressMonitor As clsProgressMonitor

    ' The error log - null indicates not validated
    ' We don't pass the error log to the server since it makes no sense to send it there
    <NonSerialized()>
    Private mErrorLog As clsErrorLog

    ' The conflict set representing the conflicts in this release.
    <NonSerialized()>
    Private mConflicts As ConflictSet

    ' The filename that this release came from
    <DataMember>
    Private mFilename As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty release with no package assigned to it, using data from
    ''' the specified provider.
    ''' </summary>
    ''' <param name="prov">The provider giving the data.</param>
    Public Sub New(ByVal prov As IDataProvider)
        Me.New(Nothing, prov)
    End Sub

    ''' <summary>
    ''' Creates a new (empty) release from the given package, using data from the
    ''' specified provider.
    ''' </summary>
    ''' <param name="pkg">The package that this release was drawn from.</param>
    ''' <param name="prov">The data for this release.</param>
    Public Sub New(ByVal pkg As clsPackage, ByVal prov As IDataProvider)
        Me.New(pkg, prov.GetString("name"), prov.GetValue("created", Date.UtcNow),
         prov.GetString("username"), False)
        Me.Id = prov.GetValue("id", 0)
    End Sub

    ''' <summary>
    ''' Creates a new release with the given name and draws its contents from the
    ''' given package as specified.
    ''' </summary>
    ''' <param name="pkg">The package which this release was formed from.</param>
    ''' <param name="name">The name of the release.</param>
    ''' <param name="generateContentsNow">Flag to indicate if the contents of the
    ''' release should be generated immediately.</param>
    Public Sub New(ByVal pkg As clsPackage, ByVal name As String, ByVal generateContentsNow As Boolean)
        Me.New(pkg, name, Date.UtcNow, User.CurrentName, generateContentsNow)
    End Sub

    ''' <summary>
    ''' Creates a new local release, based on the given package with the specified
    ''' properties
    ''' </summary>
    ''' <param name="pkg">The package which this release was formed from.</param>
    ''' <param name="nm">The name of the release</param>
    ''' <param name="dt">The creation date of the release</param>
    ''' <param name="user">The user who created the release.</param>
    ''' <param name="generateContentsNow">Flag to indicate if the contents of the
    ''' release should be generated immediately.</param>
    Public Sub New(ByVal pkg As clsPackage,
     ByVal nm As String, ByVal dt As Date, ByVal user As String, ByVal generateContentsNow As Boolean)
        MyBase.New(Nothing, nm, dt, user)
        mPackage = pkg
        ' Assume generated from a local package, rather than imported from another
        ' environment.
        mLocal = True

        ' We need to build up the release from this package.
        If generateContentsNow Then GenerateContents(Auth.User.Current)
    End Sub

    ''' <summary>
    ''' Creates a new release, loading the data from the given XML Reader.
    ''' </summary>
    ''' <param name="reader">The reader from which the data in this release should
    ''' be loaded.</param>
    Public Sub New(ByVal reader As XmlReader)
        Me.New(reader, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new release, loading the data from the given XML Reader.
    ''' </summary>
    ''' <param name="reader">The reader from which the data in this release should
    ''' be loaded.</param>
    ''' <param name="mon">The monitor with which progress can be monitored for this
    ''' release loading</param>
    Public Sub New(ByVal reader As XmlReader, ByVal mon As clsProgressMonitor)
        MyBase.New(Nothing, Nothing, Nothing, Nothing)
        ' This wasn't generated from a package, so assume remote-import
        mLocal = False
        mProgressMonitor = mon
        Try
            ' Load the XML using this release as its own context - this means that,
            ' for example, when groups are loading their members, they can look up
            ' the actual members in this release and use those objects.
            FromXml(reader, New ReleaseContext(Me))
        Finally
            ' Ensure that the progress monitor is removed from this release
            ' once it has been used for the xml loading.
            mProgressMonitor = Nothing
        End Try
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Flag indicating if this release is locally sourced or not, ie. if it was
    ''' created from a package on this system, or imported from another environment
    ''' </summary>
    Public Property Local() As Boolean
        Get
            Return mLocal
        End Get
        Set(ByVal value As Boolean)
            mLocal = value
        End Set
    End Property

    ''' <summary>
    ''' Synonym for the <see cref="Description"/>
    ''' </summary>
    Public Property ReleaseNotes() As String
        Get
            Return Description
        End Get
        Set(ByVal value As String)
            Description = value
        End Set
    End Property

    ''' <summary>
    ''' The package (version) that this release was formed from.
    ''' </summary>
    Public Property Package() As clsPackage
        Get
            Return mPackage
        End Get
        Set(ByVal value As clsPackage)
            mPackage = value
        End Set
    End Property

    ''' <summary>
    ''' The only component which can own a release is a package. Specifically, the
    ''' currently assigned package for this release, so return that.
    ''' </summary>
    ''' <exception cref="InvalidCastException">If an attempt is made to set the
    ''' owner of this release to anything other than a package.</exception>
    Public Overrides Property Owner() As OwnerComponent
        Get
            Return Me.Package
        End Get
        Set(ByVal value As OwnerComponent)
            If value IsNot Nothing AndAlso Not TypeOf (value) Is clsPackage Then
                Throw New InvalidCastException(String.Format(
                 My.Resources.clsRelease_ReleasesCanOnlyBeOwnedByPackagesCanTSetOwnerTo0, value))
            End If
            Me.Package = DirectCast(value, clsPackage)
        End Set
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            If mLocal Then Return PackageComponentType.ReleaseOut
            Return PackageComponentType.ReleaseIn
        End Get
    End Property

    ''' <summary>
    ''' The type key to use for the XML Namespace representing this component.
    ''' For releases, this makes no distinction between 'incoming' and 'outgoing'
    ''' releases.
    ''' </summary>
    Public Overrides ReadOnly Property NamespaceTypeKey() As String
        Get
            Return PackageComponentType.Release.Key
        End Get
    End Property

    ''' <summary>
    ''' Checks if this is a legacy release or not - a legacy release is a temp
    ''' release object which represents a single process or VBO, loaded from an XML
    ''' file.
    ''' </summary>
    Public ReadOnly Property IsLegacy() As Boolean
        Get
            ' Currently, the only way to have a null package is to import a legacy
            ' process / VBO, hence...
            Return (mPackage Is Nothing)
        End Get
    End Property

    ''' <summary>
    ''' The filename from which this release was imported - only valid for releases
    ''' being imported.
    ''' </summary>
    Public Property FileName() As String
        Get
            Return mFilename
        End Get
        Set(ByVal value As String)
            mFilename = value
        End Set
    End Property

    ''' <summary>
    ''' Only valid for releases being imported. If True then this release is being
    ''' imported non-interactively via the AutomateC command line, and therefore
    ''' default conflict resolutions will be applied where possible.
    ''' </summary>
    Public Property UnattendedImport() As Boolean

    Public ReadOnly Property IsSkill() As Boolean
        Get
            Return Members.Any(Function(c) c.Type = PackageComponentType.Skill)
        End Get
    End Property

#End Region

#Region " Release-specific Methods "

    ''' <summary>
    ''' Finds the conflicts with this release.
    ''' </summary>
    ''' <returns>A conflict set containing all the conflicts with the database which
    ''' are present in this release.</returns>
    Public Overloads ReadOnly Property Conflicts() As ConflictSet
        Get
            If mConflicts Is Nothing OrElse mConflicts.Conflicts.Count = 0 Then
                mConflicts = New ConflictSet(Me)
                For Each comp As PackageComponent In Me
                    mConflicts.AddConflicts(comp)
                Next
            End If
            Return mConflicts
        End Get
    End Property

    ''' <summary>
    ''' Gets the conflict resolvers for this release - ie. all the delegates which
    ''' apply conflict resolutions to this release's components
    ''' </summary>
    Public ReadOnly Property ConflictResolvers() As ICollection(Of Conflict.Resolver)
        Get
            Dim vals As New clsSet(Of Conflict.Resolver)
            For Each comp As PackageComponent In Me
                Dim ccc As Conflict.Resolver = comp.ResolutionApplier
                If ccc IsNot Nothing Then vals.Add(ccc)
            Next
            Return vals
        End Get
    End Property

    ''' <summary>
    ''' Generates the contents for this release from the currently set package.
    ''' </summary>
    ''' <param name="user">The user whose permissions to check when generating the release contents</param>
    ''' <exception cref="InvalidOperationException">If the package is currently not
    ''' set in this release.</exception>
    Public Sub GenerateContents(user As IUser)
        If mPackage Is Nothing Then Throw New InvalidOperationException(
         My.Resources.clsRelease_CouldnTGenerateContentsForThisReleaseNoPackageIsSet)

        Me.Members.Clear()
        Dim components As New clsOrderedSet(Of PackageComponent)

        Dim packageClone As clsPackage
        ' If this exists on the database, get it from there, which will ensure
        ' that the components are updated with the most up to date information
        If mPackage.IdAsInteger > 0 Then
            packageClone = gSv.GetPackage(mPackage.IdAsInteger)
        Else ' Otherwise it's adhoc, just clone the package itself
            packageClone = DirectCast(mPackage.Clone(), clsPackage)
        End If

        ' We need to add the dependents first
        For Each cd As PackageComponent In packageClone
            Dim deps As ICollection(Of PackageComponent) = cd.GetDependents()
            If deps IsNot Nothing AndAlso deps.Count > 0 Then components.Union(deps)
        Next

        ' Then we add the rest of the package
        components.Union(packageClone)



        ' Check the user has permission to export each component.
        For Each component In components
            If Not HasExportPermission(component, user) Then
                Throw New BluePrismException(String.Format(My.Resources.clsRelease_UserDoesNotHavePermissionToExportComponent0, component.Name))
            End If
        Next

        ' Finally add any group assignments for the whole set of components
        For Each cd As PackageComponent In components
            Dim groupInfo As IDictionary(Of Guid, String) = cd.GetGroupInfo()
            If groupInfo IsNot Nothing Then
                GroupComponent.MergeInto(Nothing, components, cd, groupInfo)
            End If
        Next

        AddAll(components)
    End Sub

    ''' <summary>
    ''' Where the package component references a group member, checks the group level permissions of that member to 
    ''' determine if the user has permission to export it.
    ''' </summary>
    ''' <param name="component"></param>
    ''' <param name="user"></param>
    ''' <returns></returns>
    ''' <remarks>Currently we have only implemented group permissions on Processes on Objects. I've tried to make this generic enough so that
    ''' once we implement group perms for Queues and Tiles, this method won't require any changes.</remarks>
    Private Function HasExportPermission(component As PackageComponent, user As IUser) As Boolean

        Dim groupMember As IGroupMember
        Dim requiredPermission As Permission

        Select Case component.Type
            Case PackageComponentType.Process
                groupMember = New ProcessGroupMember() With {.Id = CType(component.AssociatedData, ProcessWrapper).Process.Id}
                requiredPermission = GroupTreeType.Processes.GetTreeDefinition().ExportItemPermission

            Case PackageComponentType.BusinessObject
                groupMember = New ObjectGroupMember() With {.Id = CType(component.AssociatedData, ProcessWrapper).Process.Id}
                requiredPermission = GroupTreeType.Objects.GetTreeDefinition().ExportItemPermission

            Case PackageComponentType.Queue
                groupMember = New QueueGroupMember() With {.Id = CType(component.AssociatedData, clsWorkQueue).Id}
                requiredPermission = GroupTreeType.Queues.GetTreeDefinition().ExportItemPermission

            Case PackageComponentType.Tile
                groupMember = New TileGroupMember() With {.Id = CType(component.AssociatedData, Tile).ID}
                requiredPermission = GroupTreeType.Tiles.GetTreeDefinition().ExportItemPermission

            Case Else
                Return True

        End Select

        If requiredPermission Is Nothing Then Return True

        Dim perms = gSv.GetEffectiveMemberPermissions(groupMember)

        ' Haven't implemented group perms for this group member type yet.
        If perms.State = PermissionState.Unknown Then Return True

        Return perms.HasPermission(user, requiredPermission)

    End Function

    ''' <summary>
    ''' Gets a map of differences from this release to the given release
    ''' </summary>
    ''' <param name="rel">The release to test for differences against.</param>
    ''' <param name="mon">The progress monitor to inform of the testing</param>
    ''' <returns>A map of differences keyed against the component within this release
    ''' that the difference occurred on.</returns>
    Public Function Diff(ByVal rel As clsRelease, ByVal mon As clsProgressMonitor) _
     As IDictionary(Of PackageComponent, String)

        Dim diffs As New clsOrderedDictionary(Of PackageComponent, String)

        ' Keep track of the components that rel contains - as we match each
        ' one, we'll remove it from this set.
        Dim remaining As New clsSet(Of PackageComponent)(rel.Members)
        For Each myComp As PackageComponent In Me
            Dim comp As PackageComponent = rel.FindComponent(myComp.Type, myComp.Name)
            Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(myComp.Type)
            If comp Is Nothing Then
                diffs.Add(myComp, String.Format(
                 My.Resources.clsRelease_The0Component1IsNotPresentIn2,
                 typeLabel, myComp.Name, rel.Name))

                Continue For
            End If
            remaining.Remove(comp)
            If myComp.Differs(comp) Then
                diffs.Add(myComp, String.Format(
                 My.Resources.clsRelease_The0Component1IsDifferentIn2,
                 typeLabel, myComp.Name, rel.Name))
            End If
        Next
        ' So now matchedComponents matches all components in rel which were found
        ' to be matched with components in this release.
        ' If rel contains any other components, then, by definition, they are not
        ' included in this release
        For Each comp As PackageComponent In remaining
            Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(comp.Type)
            diffs.Add(comp, String.Format(
             My.Resources.clsRelease_The0Component1IsNotPresentIn2,
             typeLabel, comp.Name, Me.Name))
        Next
        Return diffs
    End Function

    ''' <summary>
    ''' Fires a progress change to the currently set progress monitor, if one
    ''' currently exists
    ''' </summary>
    ''' <param name="val">The percentage of progress to report.</param>
    ''' <param name="status">The status message to send along with the report</param>
    Private Sub FireProgress(ByVal val As Integer, ByVal status As String)
        If mProgressMonitor IsNot Nothing Then mProgressMonitor.FireProgressChange(val, status)
    End Sub

#End Region

#Region " Xml Handling "

    ''' <summary>
    ''' Writes the head of the XML for this component to the given writer. This
    ''' leaves an element open that subclasses can write to if necessary in the
    ''' XML Body writer.
    ''' </summary>
    ''' <param name="writer">The writer to which the head of the XML representing
    ''' this component should be written.</param>
    Protected Overrides Sub WriteXmlHead(ByVal writer As XmlWriter)
        If Me.Count = 0 Then Throw New BluePrismException(
         My.Resources.clsRelease_NoContentsFoundIn0CanTExportAnEmptyRelease, Me.Name)

        FireProgress(10, My.Resources.clsRelease_SavingContents)
        Dim ns As String = XmlNamespace
        writer.WriteStartElement("bpr", "release", ns)  ' envelope
        writer.WriteAttributeString("xmlns", "bpr", Nothing, ns)
        writer.WriteElementString("bpr", "name", ns, Me.Name)
        writer.WriteElementString("bpr", "release-notes", ns, Me.ReleaseNotes)
        writer.WriteElementString("bpr", "created", ns, Me.Created.ToString("u"))
        writer.WriteElementString("bpr", "package-id", ns, Me.Package.Id.ToString())
        writer.WriteElementString("bpr", "package-name", ns, Me.Package.Name)
        writer.WriteElementString("bpr", "user-created-by", ns, Me.UserName)
        writer.WriteStartElement("bpr", "contents", ns) ' contents
        writer.WriteAttributeString("count", Me.Count.ToString())
    End Sub

    ''' <summary>
    ''' Appends this release to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this release should be written.
    ''' </param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        Dim progress As Integer = 10
        Dim count As Integer = 0
        Dim componentStep As Integer = (75 - 10) \ Me.Count

        For Each comp As PackageComponent In Me.Members
            progress += componentStep
            If progress > 75 Then progress = 75
            FireProgress(progress, String.Format(My.Resources.clsRelease_Saving01, comp.TypeKey, comp.Name))
            comp.ToXml(writer)
        Next
        FireProgress(90, My.Resources.clsRelease_SavedContents)
    End Sub

    ''' <summary>
    ''' Writes the tail of the XML for this component to the given writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this component's XML tail should
    ''' be written.</param>
    Protected Overrides Sub WriteXmlTail(ByVal writer As XmlWriter)
        writer.WriteEndElement() ' contents
        writer.WriteEndElement() ' envelope
        FireProgress(100, My.Resources.clsRelease_SavedRelease)
    End Sub

    ''' <summary>
    ''' Reads the head from the given XML reader
    ''' </summary>
    ''' <param name="r">The XML reader from where to draw the XML head for this
    ''' release.</param>
    ''' <param name="ctx">The context in which this release is being read.</param>
    Protected Overrides Sub ReadXmlHead(
     ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        FireProgress(0, My.Resources.clsRelease_LoadingHeader)
        Dim ns As String = XmlNamespace
        r.MoveToContent()
        r.ReadStartElement("release", ns)   ' Envelope
        Me.Name = r.ReadElementContentAsString("name", ns)
        Me.ReleaseNotes = r.ReadElementContentAsString("release-notes", ns)
        Me.Created = DateTime.ParseExact(r.ReadElementContentAsString("created", ns), "u", Nothing)
        Dim pkgId As Integer = Integer.Parse(r.ReadElementContentAsString("package-id", ns))
        mPackage = New clsPackage(r.ReadElementContentAsString("package-name", ns))
        mPackage.Id = pkgId
        mPackage.Releases.Add(Me)

        Me.UserName = r.ReadElementContentAsString("user-created-by", ns)

        ' We want to ensure that we are positioned at the <contents> start element node,
        ' ready for the first component to be handled
        Debug.Assert(r.LocalName = "contents" AndAlso r.NamespaceURI = ns)

        ' Get the number of members in the importing release.
        ctx("clsRelease.MemberCount") = Integer.Parse(BPUtil.IfNull(r("count"), "0"))

        ' Leave it there, ready for the body processing

    End Sub

    ''' <summary>
    ''' Reads the XML body from the given reader.
    ''' </summary>
    ''' <param name="r">The reader from which to draw the XML body representing
    ''' this release.</param>
    ''' <param name="ctx">The context in which this component is being loaded</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        ' We want to ensure that we are positioned at the <contents> start element node,
        ' ready for the first component to be handled
        Debug.Assert(r.LocalName = "contents" AndAlso r.NamespaceURI = XmlNamespace)

        FireProgress(10, My.Resources.clsRelease_LoadingContents)
        Dim progress As Integer = 10
        ' If we're on an element, don't read past it, if we're on an end element, go
        ' for a single read and see if there's anything left.
        Dim count As Integer = 0
        Dim componentStep As Integer = 10
        Dim elementCount As Integer = DirectCast(ctx("clsRelease.MemberCount"), Integer)
        If elementCount > 0 Then componentStep = (75 - 10) \ elementCount

        While r.Read() ' r.NodeType = XmlNodeType.Element OrElse r.Read()
            Select Case r.NodeType
                Case XmlNodeType.Element
                    progress += componentStep
                    If progress > 75 Then progress = 75
                    FireProgress(progress, String.Format(My.Resources.clsRelease_Loading01, PackageComponentType.GetLocalizedFriendlyName(r.LocalName.Capitalize), r("name")))
                    Dim comp As PackageComponent =
                     PackageComponentType.NewComponent(Me, r.LocalName, r.ReadSubtree(), ctx)
                    Add(comp)
                    mPackage.Add(comp)

                Case XmlNodeType.EndElement
                    If r.LocalName = "contents" AndAlso r.NamespaceURI = XmlNamespace Then
                        FireProgress(90, My.Resources.clsRelease_LoadedContents)
                        r.Read() ' Go "one past" the end element
                        Return
                    End If

            End Select
            'check the mProgressMonitor for the cancel flag, if found then exit.
            If mProgressMonitor?.IsCancelRequested Then
                Throw New OperationCancelledException()
            End If

        End While

    End Sub

    ''' <summary>
    ''' Reads the XML tail for this release from the given reader.
    ''' Actually, there is no XML tail to read - this just updates the progress
    ''' with a 100% value.
    ''' </summary>
    ''' <param name="r">The XML reader where the tail would be, if there was one.
    ''' </param>
    ''' <param name="ctx">The context in which to load this release</param>
    Protected Overrides Sub ReadXmlTail(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)
        FireProgress(100, My.Resources.clsRelease_LoadedRelease)
    End Sub

#End Region

#Region " Exporting "

    ''' <summary>
    ''' Exports this release to the given file, compressing it on the fly, as
    ''' specified by the "release.compress" pref, without a progress monitor.
    ''' </summary>
    ''' <param name="file">The file to which this release should be exported.</param>
    Public Sub Export(ByVal file As FileInfo)
        Export(file, DirectCast(Nothing, clsProgressMonitor))
    End Sub

    ''' <summary>
    ''' Exports this release to the given file, compressing it on the fly, as
    ''' specified by the "release.compress" pref with the given monitor.
    ''' </summary>
    ''' <param name="file">The file to which this release should be exported.</param>
    ''' <param name="mon">The progress monitor to keep updated with the export
    ''' progress.</param>
    Public Sub Export(ByVal file As FileInfo, ByVal mon As clsProgressMonitor)
        Export(file, gSv.GetPref(PreferenceNames.Release.ReleaseCompressed, False), mon)
    End Sub

    ''' <summary>
    ''' Exports this release to the given file, compressing it as specified, without
    ''' a progress monitor.
    ''' </summary>
    ''' <param name="file">The file to which this release should be exported.</param>
    ''' <param name="compress">True to gzip-compress the file, false to leave it
    ''' uncompressed.</param>
    Public Sub Export(ByVal file As FileInfo, ByVal compress As Boolean)
        Export(file, compress, Nothing)
    End Sub

    ''' <summary>
    ''' Exports this release to the given file, compressing it as specified,
    ''' reporting progress to the given monitor.
    ''' </summary>
    ''' <param name="file">The file to which this release should be exported.</param>
    ''' <param name="compress">True to gzip-compress the file, false to leave it
    ''' uncompressed. Note that this does not change the output filename - ie. it
    ''' will not have .gz appended to the end of it.</param>
    ''' <param name="mon">The progress monitor to whom progress should be reported
    ''' </param>
    Public Sub Export(ByVal file As FileInfo, ByVal compress As Boolean, ByVal mon As clsProgressMonitor)
        ' First we want to write to a temp file, then we can move it to the given file
        ' once the export has been successful.
        Dim tempFile As FileInfo = New FileInfo(BPUtil.GetRandomFilePath())

        ' We can't put stream into a Using block since it might change, and it
        ' must remain the same if used in a Using...End Using
        Dim stream As Stream = New FileStream(tempFile.FullName, FileMode.Create, FileAccess.Write)
        Try
            If compress Then stream = New GZipStream(stream, CompressionMode.Compress)
            Export(stream, mon)
            stream.Flush()
            stream.Close()
            ' Exporting has succeeded, delete the existing file if one exists
            If file.Exists Then file.Delete()
            ' And move the tempfile to where that file was.
            tempFile.MoveTo(file.FullName)

        Catch
            ' Assume that the tempFile still represents the tempFile (which it
            ' doesn't after MoveTo() is called.
            If stream IsNot Nothing Then stream.Dispose()
            If tempFile.Exists Then tempFile.Delete()
            ' Rethrow whatever exception we got.
            Throw

        Finally
            If stream IsNot Nothing Then stream.Dispose()

        End Try
    End Sub

    ''' <summary>
    ''' Exports this release to the given stream
    ''' </summary>
    ''' <param name="stream">The stream to which this release should be exported.</param>
    Public Sub Export(ByVal stream As Stream, ByVal mon As clsProgressMonitor)
        Dim settings As New XmlWriterSettings()
        With settings
            .Indent = True
            .IndentChars = New String(" "c, 4)
            .Encoding = New UTF8Encoding()
        End With
        mProgressMonitor = mon
        Try
            Using xw As XmlWriter = XmlWriter.Create(New StreamWriter(stream, Encoding.UTF8), settings)
                ToXml(xw)
            End Using
            stream.Flush()
        Finally
            ' Ensure that the progress monitor is removed from this release
            ' once it has been used for the xml loading.
            mProgressMonitor = Nothing
        End Try
    End Sub

#End Region

#Region " Importing "

    ''' <summary>
    ''' Imports a release object from the given file.
    ''' </summary>
    ''' <param name="file">The file from which to draw the release.</param>
    ''' <returns>The release object, loaded from the given file.</returns>
    Public Shared Function Import(ByVal file As FileInfo) As clsRelease
        Return Import(file, Nothing, False)
    End Function

    ''' <summary>
    ''' Imports a release object from the given file, reporting progress as required
    ''' </summary>
    ''' <param name="file">The file from which to draw the release</param>
    ''' <param name="mon">The monitor to which progress should be sent</param>
    ''' <returns>The release object loaded from the specified file</returns>
    Public Shared Function Import(file As FileInfo, mon As clsProgressMonitor, unattended As Boolean) As clsRelease
        ' Open the stream
        Dim stream As Stream = New FileStream(file.FullName, FileMode.Open, FileAccess.Read)

        Try
            ' Check the first two bytes - if they are 0x1f (31) then 0x8b (139), then this file is
            ' gzip-compressed. See http://www.fileformat.info/format/gzip/gzip.magic
            ' Since the alternative (in order to be valid) is XML, and 0x1f is an ASCII control
            ' char, and thus invalid in XML, this is pretty foolproof (assuming a valid file)
            Dim compressed As Boolean = (stream.ReadByte() = &H1F AndAlso stream.ReadByte() = &H8B)
            Dim isSkill = False

            ' Get back to the beginning of the file in order to process the whole file
            stream.Seek(0, SeekOrigin.Begin)

            ' If compressed, wrap the stream inside a GZipStream to uncompress it on the fly
            ' If it's compressed, we know it's not legacy (since that never compresses a file)
            If compressed Then
                stream = New GZipStream(stream, CompressionMode.Decompress)
            Else
                ' Secondary check - is this an old-style process / business object.
                Using r As XmlReader = CreateXmlReader(stream)
                    Try
                        r.MoveToContent()
                    Catch ex As XmlException
                        isSkill = True
                    End Try
                    If Not isSkill AndAlso r.LocalName = "process" Then
                        ' It's a legacy process file so we create a dummy release
                        ' into which the process can be added.
                        ' Try and get a sensible name to call it. Legacy exports used
                        ' to be prefixed 'BPA Object - ' or 'BPA Process - ', so
                        ' strip that out if it's there and use the remainder.
                        Dim name As String = file.Name
                        Dim m As Match = LegacyFileNameRegex.Match(name)
                        If m.Success Then
                            name = m.Groups(1).Value
                            ' If the group had no text, just revert to the filename
                            If name = "" Then name = file.Name
                        End If
                        Dim rel As New clsRelease(Nothing,
                            name, Date.UtcNow, "[clsRelease]", False)
                        rel.Add(ProcessComponent.ImportLegacy(rel, r))
                        rel.FileName = file.FullName
                        rel.UnattendedImport = unattended
                        Return rel
                    End If
                End Using

                ' So this isn't a legacy process file - back to the beginning again,
                ' ready to be read by the release proper

                ' We actually have to reopen the stream now - the Dispose() of the
                ' XmlReader will close the underlying stream; we can't inhibit that
                ' *and* have Normalization set to false through public methods, and
                ' it's too risky to use reflection in case the internal structure of
                ' the xml readers change.
                stream.Dispose() ' Just make sure in case future .NET doesn't do this
                stream = New FileStream(file.FullName, FileMode.Open, FileAccess.Read)
                If isSkill Then stream = Skill.DecryptAndVerify(stream)

            End If

            If stream IsNot Nothing Then ' kinda redundant, but VB doesn't have scoping blocks
                Dim rel As clsRelease = Import(stream, mon)
                rel.FileName = file.FullName
                rel.UnattendedImport = unattended
                Return rel
            End If

        Finally
            stream.Close()
            stream.Dispose()
        End Try
        Return Nothing

    End Function

    Public Shared Function ImportProcessesAsRelease(ByRef importFiles As List(Of ImportFile), mon As clsProgressMonitor, unattended As Boolean) As clsRelease
        If Not importFiles.Any Then Return Nothing

        Dim name As String = GetFileName(importFiles(0).File)
        Dim release As New clsRelease(Nothing,
                                 name, Date.UtcNow, "[clsRelease]", False) With {
            .FileName = importFiles(0).File.FullName,
            .UnattendedImport = unattended
                                 }
        ImportFilesIntoRelease(importFiles, release)

        Return release
    End Function

    Private Shared Sub ImportFilesIntoRelease(importFiles As List(Of ImportFile), release As clsRelease)

        For Each importFile In importFiles
            If String.IsNullOrEmpty(importFile.FileName) Then
                importFile.CanImport = False
                importFile.Errors.Add(My.Resources.clsReleaseFileImportBlankName)
            End If
            If Not File.Exists(importFile.FileName) Then
                importFile.CanImport = False
                importFile.Errors.Add(My.Resources.clsReleaseImportFileFileDoesNotExist)
            End If
            If clsProcess.CheckValidExtensionForType(importFile.FileName) <> clsProcess.IsValidForType.Valid Then
                importFile.CanImport = False
                importFile.BluePrismName = Path.GetFileName(importFile.FileName)
                importFile.Errors.Add(String.Format(My.Resources.TheSelectedFileIsNotAValidBluePrism0,
                                                    If(importFile.FileName.EndsWith(clsProcess.ObjectFileExtension,
                                                                                    StringComparison.OrdinalIgnoreCase),
                                                       My.Resources.BusinessObject,
                                                       My.Resources.Process)))
            End If
            If importFile.CanImport Then
                ImportProcessIntoRelease(release, importFile)
            End If
        Next
    End Sub

    Private Shared Sub ImportProcessIntoRelease(release As clsRelease, importFile As ImportFile)

        Using stream As Stream = New FileStream(importFile.File.FullName, FileMode.Open, FileAccess.Read)
            Using r As XmlReader = CreateXmlReader(stream)
                Try
                    r.MoveToContent()
                    Dim process = ImportLegacy(release, r)
                    With importFile
                        .ProcessType = process.Type
                        .BluePrismName = process.Name
                        .BluePrismId = process.OriginalId
                    End With
                    If (process.Type = PackageComponentType.BusinessObject AndAlso User.Current.HasPermission(Permission.ObjectStudio.ImportBusinessObject)) OrElse
                       (process.Type = PackageComponentType.Process AndAlso User.Current.HasPermission(Permission.ProcessStudio.ImportProcess)) Then
                        AddToRelease(importFile, release, process)
                    Else
                        importFile.CanImport = False
                        importFile.UserHasPermission = False
                        importFile.Errors.Add(If(process.Type = PackageComponentType.BusinessObject, My.Resources.clsRelease_YouDoNotHavePermissionToImportBusinessObjects, My.Resources.clsRelease_YouDoNotHavePermissionToImportProcess))
                    End If
                Catch ex As Exception
                    importFile.Errors.Add(ex.Message)
                    importFile.BluePrismName = importFile.File.Name
                    importFile.CanImport = False
                End Try
            End Using
        End Using
    End Sub

    Private Shared Sub AddToRelease(importFile As ImportFile, release As clsRelease, process As ProcessComponent)
        If Not release.InnerMembers.Contains(process) Then
            release.Add(process)
        Else
            importFile.CanImport = False
            importFile.Errors.Add(String.Format(My.Resources.clsRelease_DuplicateOf0Object, release.InnerMembers.FirstOrDefault(Function(x) x.IdAsGuid = process.IdAsGuid).Name))
        End If
    End Sub

    Private Shared Function GetFileName(file As FileInfo) As String

        Dim name As String = file.Name
        Dim legacyFileNameMatch As Match = LegacyFileNameRegex.Match(name)
        If legacyFileNameMatch.Success Then
            name = legacyFileNameMatch.Groups(1).Value
            ' If the group had no text, just revert to the filename
            If name = "" Then name = file.Name
        End If
        Return name
    End Function

    ''' <summary>
    ''' Creates an XML Reader over the given stream which reads UTF-encoded text
    ''' and parses it from XML.
    ''' </summary>
    ''' <param name="stream">The stream to read</param>
    ''' <returns>An initialised XML reader over the given stream.</returns>
    Private Shared Function CreateXmlReader(ByVal stream As Stream) As XmlReader
        ' We apparently supported outputting invalid XML chars - Denis sent in a
        ' VBO for Kana which had an attribute with a byte of value 0x12 inside it,
        ' which was breaking the XML parsing here (though not, strangely, in an
        ' XmlDocument.LoadXml() call) - "Normalization = False" disables that
        ' checking within the XmlReader, ensuring backwards compatibility. It also
        ' ensures that whitespace chars within attribute values are retained rather
        ' than normalized to space characters. "XmlResolver = Nothing" ensures
        ' that we're not attempting to resolve external entities. (See bg-1050)
        Return New XmlTextReader(New StreamReader(stream, Encoding.UTF8)) With {
            .Normalization = False,
            .WhitespaceHandling = WhitespaceHandling.Significant,
            .XmlResolver = Nothing
        }
    End Function

    ''' <summary>
    ''' Imports a release object from the given stream, reporting progress as
    ''' required
    ''' </summary>
    ''' <param name="str">The stream from which to draw the release data.</param>
    ''' <param name="mon">The monitor to which progress reports should be sent
    ''' </param>
    ''' <returns>A Release object, loaded from the given stream</returns>
    Public Shared Function Import(ByVal str As Stream, ByVal mon As clsProgressMonitor) As clsRelease
        Using reader As XmlReader = CreateXmlReader(str)
            Return New clsRelease(reader, mon)
        End Using
    End Function

    ''' <summary>
    ''' Checks the currently logged in user's import permissions against those
    ''' required to import this release, returning a collection of all permissions
    ''' which the current user <em>does not have</em>, which are required before
    ''' importing can continue.
    ''' </summary>
    ''' <returns>A collection of the permission names which are required in order to
    ''' import this release, which the current user does not have.</returns>
    Public Function CheckImportPermissions() As ICollection(Of String)
        Dim missingPerms As New clsOrderedSet(Of String)
        AddMissingImportPermissions(missingPerms)
        Return missingPerms
    End Function

    ''' <summary>
    ''' Adds the permissions which are required by this component that the currently
    ''' logged in user is missing.
    ''' Generally, this collection should contain the names of the required
    ''' permissions, but if the rules are more complex (eg. either of 2 permissions
    ''' will satisfy the requirement), the output can be tailored to match the rule,
    ''' but each type of component should produce the same output for the same user.
    ''' </summary>
    ''' <param name="perms">The collection of permissions to add to</param>
    Public Overrides Sub AddMissingImportPermissions(ByVal perms As ICollection(Of String))
        For Each comp As PackageComponent In Me
            comp.AddMissingImportPermissions(perms)
        Next
    End Sub

    ''' <summary>
    ''' Sets the 'created' user to the given user name, also setting the 'created'
    ''' date to the current (UTC) time.
    ''' </summary>
    ''' <param name="userName">The user name to set as the 'created' user on this
    ''' release. This becomes the equivalent of the 'importing' user for a nonlocal
    ''' release.</param>
    Friend Sub SetUserToImportingUser(ByVal userName As String)
        Me.UserName = userName
        Me.Created = Date.UtcNow
    End Sub

    ''' <summary>
    ''' Initialises components and their associated data with dependencies
    ''' </summary>
    Public Sub InitialiseComponentsForImport()
        Dim context As New ReleaseContext(Me)
        Dim store As New ComponentLoadingContextStore(context)
        Dim scheduler As New InertScheduler(store)
        For Each component In Me
            component.InitialiseForImport(scheduler)
        Next
    End Sub

#End Region

    Public Sub UpdateFrom(release As clsRelease)

        ' Otherwise, just copy all the properties across.
        Id = release.Id
        Name = release.Name
        ReleaseNotes = release.ReleaseNotes
        mLocal = release.mLocal
        Created = release.Created
        UserName = release.UserName
        mPackage = release.mPackage
        InnerMembers.Clear()
        InnerMembers.Union(release.InnerMembers)

        ' This release is up to date - ensure that the package knows who its release is.
        If mPackage IsNot Nothing Then ' If it's a legacy import, it will have no package
            mPackage.Releases.Remove(release)
            mPackage.Releases.Add(Me)
        End If

    End Sub



End Class

