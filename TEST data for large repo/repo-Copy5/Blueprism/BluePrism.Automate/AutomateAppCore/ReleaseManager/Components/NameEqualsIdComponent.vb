Imports System.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization

''' <summary>
''' Class representing a component for which the name and the ID are identical - ie.
''' the name <em>is</em> the ID.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public MustInherit Class NameEqualsIdComponent : Inherits PackageComponent

#Region " Constructors "

    ''' <summary>
    ''' Creates a new environment variable component from the given provider.
    ''' </summary>
    ''' <param name="prov">The provider of data for this component. This expects a
    ''' single property :- name : String</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner, prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new component representing an environment variable with the given
    ''' name.
    ''' </summary>
    ''' <param name="name">The name of the environment variable that this component
    ''' should represent.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal name As String)
        MyBase.New(owner, name, name)
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Name/ID Property Overrides "

    ''' <summary>
    ''' An environment variables name is its ID too. When the name is changed, the ID
    ''' is changed to match it.
    ''' </summary>
    Public Overrides Property Name() As String
        Get
            Return MyBase.Name
        End Get
        Set(ByVal value As String)
            MyBase.Name = value
            MyBase.Id = value
        End Set
    End Property

    ''' <summary>
    ''' An environment variables name is its ID too. When the ID is changed, the name
    ''' is changed to match it.
    ''' </summary>
    Public Overrides Property Id() As Object
        Get
            Return MyBase.Id
        End Get
        Set(ByVal value As Object)
            If value IsNot Nothing AndAlso TryCast(value, String) Is Nothing Then
                Throw New BluePrismException( _
                 "An environment variable's ID must be its name - Given: {0} of type: {1}", _
                 value, value.GetType())
            End If
            MyBase.Id = value
            MyBase.Name = DirectCast(value, String)
        End Set
    End Property

#End Region


End Class
