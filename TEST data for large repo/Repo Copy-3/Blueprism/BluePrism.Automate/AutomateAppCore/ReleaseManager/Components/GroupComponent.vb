Imports System.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Component representing a group of members in a tree (e.g. processes, objects,
''' work queues and tiles)
''' </summary>
<Serializable, DataContract([Namespace]:="bp", IsReference:=True)>
Public Class GroupComponent : Inherits SingleTypeComponentGroup

#Region " Static methods "

    ''' <summary>
    ''' Merges the group information (for the passed referencing item) into group
    ''' compoenents. If a group component does not exist it is created. The item
    ''' referencing the group is added as a group member.
    ''' </summary>
    ''' <param name="owner">The owning component</param>
    ''' <param name="comps">The collection of components to merge into</param>
    ''' <param name="refComp">The component referencing the groups</param>
    ''' <param name="groupPaths">The list of referenced groups</param>
    Public Shared Sub MergeInto(owner As OwnerComponent, comps As ICollection(Of PackageComponent),
                        refComp As PackageComponent, groupPaths As IDictionary(Of Guid, String))

        For Each c As PackageComponent In comps
            'If the group component already exists then add this referencing component to it
            If TypeOf c Is GroupComponent AndAlso groupPaths.TryGetValue(c.IdAsGuid, Nothing) Then
                CType(c, GroupComponent).Members.Add(refComp)
                groupPaths.Remove(c.IdAsGuid)
            End If
        Next

        'Create group components for any remaining groups
        For Each grp As KeyValuePair(Of Guid, String) In groupPaths
            Dim group = gSv.GetGroup(grp.Key)
            Dim gComp As New GroupComponent(owner, grp.Key, grp.Value, refComp.Type, group.IsDefault)
            gComp.Members.Add(refComp)
            comps.Add(gComp)
        Next
    End Sub

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new group component from the given properties.
    ''' </summary>
    ''' <param name="owner">The owning component</param>
    ''' <param name="id">The ID of the group on the database.</param>
    ''' <param name="name">The name of the group.</param>
    ''' <param name="type">The member type for this group</param>
    Public Sub New(owner As OwnerComponent, id As Object, name As String, type As PackageComponentType, isDefaultGroup As Boolean)
        MyBase.New(owner, id, name)
        memberTypeKey = type.Key
        mIsDefaultGroup = isDefaultGroup
    End Sub

    ''' <summary>
    ''' Creates a new group component from the given data provider.
    ''' </summary>
    ''' <param name="owner">The owning component</param>
    ''' <param name="prov">The data provider</param>
    ''' <param name="type">The member type for this group</param>
    Public Sub New(owner As OwnerComponent, prov As IDataProvider, type As PackageComponentType)
        MyBase.New(owner, prov.GetGuid("id"), prov.GetString("name"))
        memberTypeKey = type.Key
    End Sub

    ''' <summary>
    ''' Creates a new group component drawing its data from the given XML reader.
    ''' </summary>
    ''' <param name="owner">The owning component</param>
    ''' <param name="reader">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Public Sub New(owner As OwnerComponent, reader As XmlReader, ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Member variables "

    'The type of members in this group (note this is the type key rather than the
    'type itself, as PackageComponentType is not Serializable)
    <DataMember>
    Private memberTypeKey As String

    <DataMember>
    Private mIsDefaultGroup As Boolean
#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of this component. This component reveals itself as a group of the
    ''' members type key (i.e. the member type key appended with -group
    ''' </summary>
    Public Overrides ReadOnly Property Type As PackageComponentType
        Get
            Dim groupType As String = String.Format("{0}-group", memberTypeKey)
            Return PackageComponentType.AllTypes.Item(groupType)
        End Get
    End Property

    ''' <summary>
    ''' Flag to indicate that, when rendered, the group should have its members shown
    ''' in the component tree.
    ''' </summary>
    Public Overrides ReadOnly Property ShowMembersInComponentTree As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' The type key for members of this group.
    ''' </summary>
    Public Overrides ReadOnly Property MembersType As PackageComponentType
        Get
            Return PackageComponentType.AllTypes.Item(memberTypeKey)
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            'ToDo: Do we need to add some import permissions?
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Is this Group Component referencing a default group
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsDefaultGroup() As Boolean
        Get
            Return mIsDefaultGroup
        End Get
    End Property
#End Region

#Region " XML Handling "


    ''' <summary>
    ''' Overrides the standard reading of the XML header, so that the member type
    ''' for this group (and hence the type of this component) can be derived.
    ''' </summary>
    ''' <param name="reader">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Protected Overrides Sub ReadXmlHead(reader As XmlReader, ctx As IComponentLoadingContext)
        If Not reader.IsStartElement() Then Throw New BluePrismException(
         My.Resources.GroupComponent_InvalidPositionToReadXMLFrom0, reader.Name)

        'Set member type
        Dim key As String = reader.LocalName().Replace("-group", "")
        If Not PackageComponentType.AllTypes.ContainsKey(key) Then
            Throw New BluePrismException(My.Resources.GroupComponent_UnknownGroupComponent0, reader.Name)
        End If
        memberTypeKey = PackageComponentType.AllTypes.Item(key).Key

        Id = ConvertId(reader.GetAttribute("id"))
        Name = reader.GetAttribute("name")
        mIsDefaultGroup = CType(If(reader.GetAttribute("isDefaultGroup"), "False"), Boolean)
    End Sub

    Protected Overrides Sub WriteXmlHead(writer As XmlWriter)
        MyBase.WriteXmlHead(writer)
        writer.WriteAttributeString("isDefaultGroup", CStr(Me.mIsDefaultGroup))
    End Sub

#End Region

End Class
