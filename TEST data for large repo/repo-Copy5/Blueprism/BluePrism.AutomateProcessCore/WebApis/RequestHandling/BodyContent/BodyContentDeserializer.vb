Imports System.IO
Imports System.Xml
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis.RequestHandling.BodyContent

    ''' <summary>
    ''' Module used to deserialize an XElement serialization of an implementation of 
    ''' the <see cref="IBodyContent"/> interface.
    ''' </summary>
    Public Module BodyContentDeserializer

        ''' <summary>
        ''' Deserialize the given XML String to the
        ''' <see cref="IBodyContent"/> object it represents.
        ''' </summary>
        ''' <param name="xml">The xml string to deserialize
        ''' is being deserialized</param>
        ''' <returns>A deserialized <see cref="IBodyContent"/> object</returns>
        Public Function Deserialize(xml As String) As IBodyContent
            Using xmlTextReader = New XmlTextReader(New StringReader(xml))
                Return Deserialize(XElement.Load(xmlTextReader))
            End Using
        End Function

        Public Function Deserialize(element As XElement) As IBodyContent

            If Not element.Name.LocalName.Equals("bodycontent") Then _
                Throw New MissingXmlObjectException("bodycontent")

            Dim typeAttribute = element.Attribute("type")
            If typeAttribute Is Nothing Then Throw New MissingXmlObjectException("type")

            Dim bodyType = typeAttribute.Value.ParseEnum(Of WebApiRequestBodyType)

            Select Case bodyType
                Case WebApiRequestBodyType.None
                    Return NoBodyContent.FromXElement(element)
                Case WebApiRequestBodyType.Template
                    Return TemplateBodyContent.FromXElement(element)
                Case WebApiRequestBodyType.SingleFile
                    Return SingleFileBodyContent.FromXElement(element)
                Case WebApiRequestBodyType.MultiFile
                    Return FileCollectionBodyContent.FromXElement(element)
                Case WebApiRequestBodyType.CustomCode
                    Return CustomCodeBodyContent.FromXElement(element)
                Case Else
                    Throw New NotImplementedException(
                        String.Format(My.Resources.Resources.BodyContentDeserializer_DeserializeNotImplementedFor0, bodyType.ToString()))
            End Select

        End Function

    End Module

End Namespace
