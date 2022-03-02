Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis
    Public Module OutputParameterDeserializer

        Public Function Deserialize(element As XElement) As ResponseOutputParameter

            If Not element.Name.LocalName.Equals("customoutputparameter") Then _
                Throw New MissingXmlObjectException("customoutputparameter")

            Dim typeAttribute = element.Attribute("type")
            If typeAttribute Is Nothing Then Throw New MissingXmlObjectException("type")

            Dim authType = typeAttribute.Value.ParseEnum(Of OutputMethodType)

            Select Case authType
                Case OutputMethodType.JsonPath
                    Return JsonPathOutputParameter.FromXElement(element)
                Case OutputMethodType.CustomCode
                    Return CustomCodeOutputParameter.FromXElement(element)
                Case Else
                    Throw New NotImplementedException(
                        String.Format(My.Resources.Resources.OutputParameterDeserializer_DeserializeNotImplementedFor0, authType.ToString()))
            End Select

        End Function
    End Module
End Namespace
