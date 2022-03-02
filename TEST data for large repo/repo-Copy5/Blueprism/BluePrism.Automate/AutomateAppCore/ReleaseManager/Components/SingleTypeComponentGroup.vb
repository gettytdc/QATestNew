Imports System.Runtime.Serialization
Imports System.Xml

''' <summary>
''' Group component whose members are of a single type.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public MustInherit Class SingleTypeComponentGroup : Inherits ComponentGroup

#Region " Constructors "

    ''' <summary>
    ''' Creates a new group component with the given name and ID.
    ''' </summary>
    ''' <param name="id">The ID of the component</param>
    ''' <param name="name">The name of the component</param>
    Protected Sub New(ByVal owner As OwnerComponent, ByVal id As Object, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub


    ''' <summary>
    ''' Creates a new process component which draws its data from the given XML
    ''' reader.
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the process data.</param>
    ''' <param name="ctx">The loading context for the XML reading</param>
    Protected Sub New(ByVal owner As OwnerComponent, _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The key of the component type supported by this group.
    ''' </summary>
    Public Overridable ReadOnly Property MembersTypeKey() As String
        Get
            Return MembersType.Key
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.AllTypes(MembersTypeKey & "-group")
        End Get
    End Property

    ''' <summary>
    ''' The type of component held in this group.
    ''' </summary>
    Public MustOverride ReadOnly Property MembersType() As PackageComponentType

#End Region

End Class
