Imports System.Data.Linq
Imports System.Drawing
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Common.Security
Imports Newtonsoft.Json.Linq

Namespace WebApis

    ''' <summary>
    ''' Contains the information to transform Web API response JSON to an output parameter
    ''' via it's defined JSON path
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class JsonPathOutputParameter : Inherits ResponseOutputParameter

        <DataMember>
        Private ReadOnly mCustomOutputParameterType As OutputMethodType = OutputMethodType.JsonPath

        <DataMember>
        Private mPath As String

        ''' <summary>
        ''' The path which will be used to parse the value of this
        ''' parameter from the response Json. Should be in the format
        ''' '$.path'
        ''' </summary>
        ''' <returns>A string</returns>
        Public Overrides ReadOnly Property Path As String
            Get
                Return mPath
            End Get
        End Property


        ''' <summary>
        ''' The Blue Prism data type that the result should be parsed as.
        ''' </summary>
        ''' <returns>The relevant data type.</returns>
        Public Overrides ReadOnly Property Type As OutputMethodType
            Get
                Return mCustomOutputParameterType
            End Get
        End Property

        ''' <summary>
        ''' Creates a new ouput parameter
        ''' </summary>
        ''' <param name="id">The Id of the parameter></param>
        ''' <param name="name">The name of the parameter</param>
        ''' <param name="path">The path of the parameter</param>
        ''' <param name="dataType">The data type of the parameter</param>
        ''' <exception cref="ArgumentNullException">If either <paramref name="name"/>
        ''' or <paramref name="path"/> is null.</exception>
        Sub New(id As Integer, name As String, description As String, path As String, dataType As DataType)
            MyBase.New(id, name, description, dataType)
            If path Is Nothing Then Throw New ArgumentNullException(NameOf(path))
            mPath = path
        End Sub

        ''' <summary>
        ''' Creates a new ouput parameter
        ''' </summary>
        ''' <param name="name">The name of the parameter</param>
        ''' <param name="path">The path of the parameter</param>
        ''' <param name="dataType">The data type of the parameter</param>
        Sub New(name As String, description As String, path As String, dataType As DataType)
            MyBase.New(name, description, dataType)
            If path Is Nothing Then Throw New ArgumentNullException(NameOf(path))
            mPath = path
        End Sub

        ''' <summary>
        ''' Gets the value of the parameter from the supplied response Json,
        ''' using the configured path. Parses according to the configured data
        ''' type, and creates a clsProcessValue with the value of the result.
        ''' </summary>
        ''' <param name="json">The reponse data from which the result should be found</param>
        ''' <returns>A clsProcessValue containing the result.</returns>
        Public Function GetFromResponse(json As JToken) As clsProcessValue
            If json Is Nothing Then
                Return New clsProcessValue(DataType, String.Empty)
            End If
            Select Case DataType
                Case DataType.binary
                    Dim value = Deserialize(Of Binary)(json)
                    Return New clsProcessValue(
                        If(value Is Nothing,  Nothing, Deserialize(Of Binary)(json).ToArray()))
                Case DataType.collection
                    Dim value = JsonHelper.DeserializeCollection(json, Path)
                    Return New clsProcessValue(
                        If(value Is Nothing, Nothing, New clsCollection(value)))
                Case DataType.date
                    Return New clsProcessValue(DataType.date, Deserialize(Of DateTime)(json).Date)
                Case DataType.datetime
                    Return New clsProcessValue(DataType.datetime, Deserialize(Of DateTime)(json))
                Case DataType.flag
                    Return If(JsonHelper.CanDeserializeNonNullableType(json, Path),
                              New clsProcessValue(Deserialize(Of Boolean)(json)),
                              New clsProcessValue(DataType.flag))
                Case DataType.image
                    Return If(JsonHelper.CanDeserializeNonNullableType(json, Path),
                              New clsProcessValue(New Bitmap(New MemoryStream(Deserialize(Of Byte())(json)))),
                              New clsProcessValue(DataType.image))
                Case DataType.number
                    Return If(JsonHelper.CanDeserializeNonNullableType(json, Path),
                              New clsProcessValue(Deserialize(Of Decimal)(json)),
                              New clsProcessValue(DataType.number))
                Case DataType.password
                    Return New clsProcessValue(New SafeString(Deserialize(Of String)(json)))
                Case DataType.text
                    Return New clsProcessValue(Deserialize(Of String)(json))
                Case DataType.time
                    Dim datetime = Deserialize(Of DateTime)(json)
                    Return If(JsonHelper.CanDeserializeNonNullableType(json, Path),
                              New clsProcessValue(DataType.time, DateTime.MinValue.Add(datetime.TimeOfDay)),
                              New clsProcessValue(DataType.time))
                Case DataType.timespan
                    Return If(JsonHelper.CanDeserializeNonNullableType(json, Path),
                              New clsProcessValue(Deserialize(Of Timespan)(json)),
                              New clsProcessValue(DataType.timespan))
            End Select

            Throw New NotImplementedException(String.Format(My.Resources.Resources.JsonPathOutputParameter_NotImplementedForTheDataType0, DataType))

        End Function

        Private Function Deserialize(Of T)(content As JToken) As T
            Return JsonHelper.Deserialize(Of T)(content, Path)
        End Function

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="JsonPathOutputParameter"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Overrides Function ToXElement() As XElement
            Return _
                <customoutputparameter
                    type=<%= Type %>
                    name=<%= Name %>
                    description=<%= Description %>
                    path=<%= Path %>
                    datatype=<%= DataType %>>
                </customoutputparameter>

        End Function


        ''' <summary>
        ''' Create a new instance of <see cref="JsonPathOutputParameter"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="JsonPathOutputParameter"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As JsonPathOutputParameter

            If Not element.Name.LocalName.Equals("customoutputparameter") Then _
                Throw New MissingXmlObjectException("customoutputparameter")

            Dim name = element.Attribute("name")?.Value
            If name Is Nothing Then Throw New MissingXmlObjectException("name")

            Dim description = element.Attribute("description")?.Value
            If description Is Nothing Then description = ""

            Dim path = element.Attribute("path")?.Value
            If path Is Nothing Then Throw New MissingXmlObjectException("path")

            Dim dataTypeName = element.Attribute("datatype")?.Value
            If dataTypeName Is Nothing Then Throw New MissingXmlObjectException("datatype")

            Dim dataTypeObject As DataType
            If Not [Enum].TryParse(dataTypeName, dataTypeObject) Then _
                Throw New InvalidArgumentException(String.Format(My.Resources.Resources.JsonPathOutputParameter_InvalidDataType0, dataTypeName))

            Return New JsonPathOutputParameter(name, description, path, dataTypeObject)
        End Function
    End Class
End Namespace
