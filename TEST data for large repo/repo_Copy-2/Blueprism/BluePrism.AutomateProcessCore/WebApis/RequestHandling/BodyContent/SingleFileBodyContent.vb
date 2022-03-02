Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis.RequestHandling.BodyContent

    <DataContract([Namespace]:="bp"), Serializable>
    Public Class SingleFileBodyContent : Implements IBodyContent

        Private Const mBodyType As WebApiRequestBodyType = WebApiRequestBodyType.SingleFile

        <DataMember>
        Private mFileInputParameterName As String

        Public Sub New()
            Me.New(WebApiResources.RequestBodyType_SingleFile_ParameterName)
        End Sub

        Public Sub New(fileInputParameterName As String)
            mFileInputParameterName = fileInputParameterName
        End Sub

        Public ReadOnly Property FileInputParameterName As String
            Get
                Return mFileInputParameterName
            End Get
        End Property

        Public ReadOnly Property Type As WebApiRequestBodyType Implements IBodyContent.Type
            Get
                Return mBodyType
            End Get
        End Property

        Public Function ToXElement() As XElement Implements IBodyContent.ToXElement
            Return <bodycontent
                       type=<%= CInt(Type) %>
                       fileinputparametername=<%= FileInputParameterName %>
                   />
        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="IBodyContent"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="TemplateBodyContent"/> is 
        ''' expected</param>
        ''' <returns>
        ''' A new instance of <see cref="IBodyContent"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As IBodyContent

            If Not element.Name.LocalName.Equals("bodycontent") Then _
                Throw New MissingXmlObjectException("bodycontent")

            Dim type = element.Attribute("type")?.Value
            If type Is Nothing Then Throw New MissingXmlObjectException("type")

            Dim fileInputParameterName = element.Attribute("fileinputparametername")?.Value
            If fileInputParameterName Is Nothing Then Throw New MissingXmlObjectException("fileinputparametername")

            Return New SingleFileBodyContent(fileInputParameterName)
        End Function

        ''' <inheritdoc />
        Public Iterator Function GetInputParameters() As IEnumerable(Of ActionParameter) _
            Implements IBodyContent.GetInputParameters

            Yield New ActionParameter(mFileInputParameterName,
                WebApiResources.FileCollectionInputParameterDescription,
                DataType.binary, True, New clsProcessValue(New Byte()))

        End Function

    End Class

End Namespace

