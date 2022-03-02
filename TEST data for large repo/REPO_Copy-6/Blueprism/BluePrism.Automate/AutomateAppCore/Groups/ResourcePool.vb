

Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Images

Namespace Groups
    ''' <summary>
    ''' Class to encompass a resource pool on the tree view
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", IsReference:=True)>
    <KnownType("GetAllKnownTypes")>
    Public Class ResourcePool
        Inherits Group

        ''' <summary>
        ''' Gets the known types which are in use in this class.
        ''' </summary>
        ''' <returns>An enumerable of known types used in this group and its contents
        ''' </returns>
        Public Overloads Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
            Return GroupTree.GetAllKnownTypes()
        End Function

        ''' <summary>
        ''' Return the image for this tree node type
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                Return ImageLists.Keys.Component.ResourcePool
            End Get
        End Property

        ''' <summary>
        ''' Returns the expanded image for this tree node type
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property ImageKeyExpanded As String
            Get
                Return ImageLists.Keys.Component.ResourcePool
            End Get
        End Property

        ''' <summary>
        ''' Name of the table used to store the item->group releationship
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupResource"
            End Get
        End Property

        ''' <summary>
        ''' Creates a new empty group with no defined tree type
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

        ''' <summary>
        ''' Creates a new group with provided data.
        ''' </summary>
        ''' <param name="prov">The provider of the data which should be used to
        ''' populate this group</param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
        End Sub

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Pool
            End Get
        End Property

        Public Overrides Function GetFilteredView(filtree As IFilteringGroupTree) _
         As IGroupMember
            Return New FilteringPool(filtree, Me)
        End Function

    End Class
End Namespace
