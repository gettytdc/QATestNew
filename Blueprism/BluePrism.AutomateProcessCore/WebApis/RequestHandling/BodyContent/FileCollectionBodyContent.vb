Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis.RequestHandling.BodyContent

    <DataContract([Namespace]:="bp"), Serializable>
    Public Class FileCollectionBodyContent : Implements IBodyContent

        Private Const mBodyType As WebApiRequestBodyType = WebApiRequestBodyType.MultiFile

        <DataMember>
        Private mFileInputParameterName As String

        <NonSerialized>
        Private mExpectedSchema As ExpectedSchema

        Public ReadOnly Property Type As WebApiRequestBodyType Implements IBodyContent.Type
            Get
                Return mBodyType
            End Get
        End Property

        ''' <summary>
        ''' The expected schema of the collection used to pass the multiple files
        ''' as an input parameter to the Web API action
        ''' </summary>
        Public ReadOnly Property InputSchema As clsCollectionInfo
            Get
                ' This is necessary because the mExpectedSchema is nonserialized and is 
                ' being set back to Nothing on deserialization
                If mExpectedSchema Is Nothing Then
                    mExpectedSchema = New ExpectedSchema()
                End If
                Return mExpectedSchema.InputSchema
            End Get
        End Property

        Public ReadOnly Property InputSchemaFields As ExpectedSchema
            Get
                Return mExpectedSchema
            End Get
        End Property

        Public ReadOnly Property FileInputParameterName As String
            Get
                Return mFileInputParameterName
            End Get
        End Property

        Public Sub New()
            Me.New(WebApiResources.RequestBodyType_MultiFile_ParameterName)
        End Sub

        Public Sub New(fileParameterName As String)

            mFileInputParameterName = fileParameterName
            mExpectedSchema = New ExpectedSchema()

        End Sub

        Public Shared Function FromXElement(element As XElement) As IBodyContent

            If Not element.Name.LocalName.Equals("bodycontent") Then _
                Throw New MissingXmlObjectException("bodycontent")

            Dim type = element.Attribute("type")?.Value
            If type Is Nothing Then Throw New MissingXmlObjectException("type")

            Dim fileInputParameterName = element.Attribute("fileinputparametername")?.Value
            If fileInputParameterName Is Nothing Then Throw New MissingXmlObjectException("fileinputparametername")

            Return New FileCollectionBodyContent(fileInputParameterName)
        End Function

        Public Function ToXElement() As XElement Implements IBodyContent.ToXElement
            Return <bodycontent type=<%= CInt(Type) %> fileinputparametername=<%= FileInputParameterName %>></bodycontent>
        End Function

        Public Iterator Function GetInputParameters() As IEnumerable(Of ActionParameter) _
            Implements IBodyContent.GetInputParameters

            Yield New ActionParameterWithCollection(
                mFileInputParameterName,
                WebApiResources.FileCollectionInputParameterDescription,
                InputSchema, True, New clsProcessValue(New Byte()))
        End Function

        Public Function CollectionRowContainsRequiredField(collectionRow As clsCollectionRow) As Boolean
            Return InputSchemaFields.
                RequiredFields.
                      All(Function(x)
                              Dim fileField = collectionRow.GetField(x.Name)
                              Return fileField IsNot Nothing AndAlso fileField.DataType = x.DataType
                          End Function)

        End Function


        Public Class ExpectedSchema
            Public ReadOnly Property File As clsCollectionFieldInfo
            Public ReadOnly Property FileName As clsCollectionFieldInfo
            Public ReadOnly Property FieldName As clsCollectionFieldInfo
            Public ReadOnly Property ContentType As clsCollectionFieldInfo
            Public ReadOnly Property InputSchema As clsCollectionInfo

            Public Sub New()
                File = New clsCollectionFieldInfo("File", DataType.binary,
                                                      WebApiResources.FileCollectionBodyContent_InputCollection_Fields_File_Description)
                FileName = New clsCollectionFieldInfo("File Name", DataType.text,
                                                          WebApiResources.FileCollectionBodyContent_InputCollection_Fields_FileName_Description)
                FieldName = New clsCollectionFieldInfo("Field Name", DataType.text,
                                                           WebApiResources.FileCollectionBodyContent_InputCollection_Fields_FieldName_Description)
                ContentType = New clsCollectionFieldInfo("Content Type", DataType.text,
                                                             WebApiResources.FileCollectionBodyContent_InputCollection_Fields_ContentType_Description)

                InputSchema = New clsCollectionInfo
                InputSchema.AddField(File)
                InputSchema.AddField(FileName)
                InputSchema.AddField(FieldName)
                InputSchema.AddField(ContentType)
            End Sub

            Public Function RequiredFields() As IEnumerable(Of clsCollectionFieldInfo)
                Return New HashSet(Of clsCollectionFieldInfo) From {File}
            End Function

        End Class

    End Class

End Namespace

