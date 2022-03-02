Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class CustomCodeOutputParameter : Inherits ResponseOutputParameter

        <DataMember>
        Private ReadOnly mCustomOutputParameterType As OutputMethodType = OutputMethodType.CustomCode

        Public Overrides ReadOnly Property Type As OutputMethodType
            Get
                Return mCustomOutputParameterType
            End Get
        End Property

        Public Overrides ReadOnly Property Path As String
            Get
                Return String.Empty
            End Get
        End Property

        Sub New(id As Integer, name As String, description As String, type As DataType)
            MyBase.New(id, name, description, type)
        End Sub

        Sub New(name As String, description As String, type As DataType)
            MyBase.New(0, name, description, type)
        End Sub

        Public Overrides Function ToXElement() As XElement
            Return _
                <customoutputparameter
                    type=<%= Type %>
                    name=<%= Name %>
                    description=<%= Description %>
                    datatype=<%= DataType %>>
                </customoutputparameter>

        End Function

        Public Shared Function FromXElement(element As XElement) As CustomCodeOutputParameter

            If Not element.Name.LocalName.Equals("customoutputparameter") Then _
                Throw New MissingXmlObjectException("customoutputparameter")

            Dim name = element.Attribute("name")?.Value
            If name Is Nothing Then Throw New MissingXmlObjectException("name")

            Dim description = element.Attribute("description")?.Value
            If description Is Nothing Then description = ""

            Dim dataTypeName = element.Attribute("datatype")?.Value
            If dataTypeName Is Nothing Then Throw New MissingXmlObjectException("datatype")

            Dim dataTypeObject As DataType
            If Not [Enum].TryParse(dataTypeName, dataTypeObject) Then _
                Throw New InvalidArgumentException(My.Resources.Resources.CustomCodeOutputParameter_InvalidDataType0, dataTypeName)

            Return New CustomCodeOutputParameter(name, description, dataTypeObject)

        End Function

    End Class

End Namespace
