Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis
    <Serializable, DataContract([Namespace]:="bp")>
    <KnownType(GetType(JsonPathOutputParameter))>
    <KnownType(GetType(CustomCodeOutputParameter))>
    <KnownType(GetType(ReadOnlyCollection(Of ResponseOutputParameter)))>
    Public Class OutputParameterConfiguration

        <DataMember>
        Private mCode As String = String.Empty

        <DataMember>
        Private mParameters As IReadOnlyCollection(Of ResponseOutputParameter)

        Public ReadOnly Property Code As String
            Get
                Return mCode
            End Get
        End Property

        Public ReadOnly Property Parameters As IReadOnlyCollection(Of ResponseOutputParameter)
            Get
                Return mParameters
            End Get
        End Property



        Public Sub New(parameters As IEnumerable(Of ResponseOutputParameter), code As String)

            If parameters Is Nothing Then _
             Throw New ArgumentNullException(NameOf(parameters))

            If WebApiConfiguration.GetDuplicateNames(parameters.Select(Function(x) x.Name)) <> "" Then _
                Throw New ArgumentException(My.Resources.Resources.OutputParameterConfiguration_TheOutputParametersContainDuplicateNames, NameOf(parameters))

            mParameters = parameters.ToList().AsReadOnly
            mCode = If(code, String.Empty)
        End Sub

        Public Function ToXElement() As XElement
            Return <outputparameters>
                       <parameters>
                           <%= From p In Parameters Select x = p.ToXElement() %>
                       </parameters>
                       <code>
                           <%= From d In New XCData(Code).ToEscapedEnumerable()
                               Select d
                           %>
                       </code>
                   </outputparameters>

        End Function

        Public Shared Function FromXElement(element As XElement) As OutputParameterConfiguration

            If Not element.Name.LocalName.Equals("outputparameters") Then _
                Throw New MissingXmlObjectException("outputparameters")

            Dim outputParameters = element.
                                        Elements.
                                        FirstOrDefault(Function(x) x.Name = "parameters")?.
                                        Elements.
                                        Where(Function(x) x.Name = "customoutputparameter").
                                        Select(Function(x) Deserialize(x))
            If outputParameters Is Nothing Then Throw New MissingXmlObjectException("parameters")

            Dim code = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "code")?.
                                Nodes.
                                OfType(Of XCData).
                                GetConcatenatedValue()
            If code Is Nothing Then Throw New MissingXmlObjectException("code")

            Return New OutputParameterConfiguration(outputParameters, code)

        End Function

    End Class

End Namespace
