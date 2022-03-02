Imports System.Xml
Imports System.Runtime.Serialization

''' <summary>
''' Component which can be retired
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public MustInherit Class RetirableComponent : Inherits PackageComponent

    ' Flag indicating if this component is retired or not.
    <DataMember>
    Private mRetired As Boolean

    ''' <summary>
    ''' Creates a new retirable data component with the given ID and name.
    ''' </summary>
    ''' <param name="id">The ID of the component</param>
    ''' <param name="name">The name of the component</param>
    Protected Sub New(ByVal owner As OwnerComponent, ByVal id As Object, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new retirable data component using data from the given XML reader
    ''' in the supplied context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the data for this
    ''' component.</param>
    ''' <param name="ctx">The context in which to load the component.</param>
    Protected Sub New(ByVal owner As OwnerComponent, _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

    ''' <summary>
    ''' Flag indicating if this component is retired or not.
    ''' </summary>
    Public Property Retired() As Boolean
        Get
            Return mRetired
        End Get
        Set(ByVal value As Boolean)
            mRetired = value
        End Set
    End Property

    ''' <summary>
    ''' The key of this component type - used as an image key and for order lookups
    ''' amongst other things.
    ''' <seealso cref="PackageComponentType.Keys"/>
    ''' </summary>
    Public MustOverride Overrides ReadOnly Property Type() As PackageComponentType

    ''' <summary>
    ''' Writes the head of the XML for this component to the given writer. This
    ''' leaves an element open that subclasses can write to if necessary in the
    ''' XML Body writer, though overriding WriteXmlHead() would probably be a more
    ''' logical place to put it if it's head information.
    ''' </summary>
    ''' <param name="writer">The writer to which the head of the XML representing
    ''' this component should be written.</param>
    Protected Overrides Sub WriteXmlHead(ByVal writer As XmlWriter)
        MyBase.WriteXmlHead(writer)
        If Retired Then writer.WriteAttributeString("retired", XmlConvert.ToString(True))
    End Sub


    ''' <summary>
    ''' Reads the head of the XML for this component from the given reader.
    ''' </summary>
    ''' <param name="reader">The reader from where to draw this component's header
    ''' data.</param>
    ''' <param name="ctx">The object providing context for the read operation</param>
    Protected Overrides Sub ReadXmlHead(ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.ReadXmlHead(reader, ctx)
        mRetired = (reader("retired") IsNot Nothing)
    End Sub

End Class
