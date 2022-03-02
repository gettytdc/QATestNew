Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.Server.Domain.Models

Namespace WebApis

    ''' <summary>
    ''' Contains information for code compilation. 
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    <KnownType(GetType(ReadOnlyCollection(Of String)))>
    Public Class CodeProperties

        <DataMember>
        Private mCode As String

        <DataMember>
        Private mLanguageName As String

        <NonSerialized>
        Private mLanguage As CodeLanguage

        <DataMember>
        Private mNamespaces As IReadOnlyCollection(Of String)

        <DataMember>
        Private mReferences As IReadOnlyCollection(Of String)

        ''' <summary>
        ''' Creates an instance of CodeProperties.
        ''' </summary>
        ''' <param name="code">The code for compilation.</param>
        ''' <param name="namespaces">The namespaces for compilation.</param>
        ''' <param name="references">The references for compilation.</param>
        Sub New(code As String, language As CodeLanguage, namespaces As IEnumerable(Of String), references As IEnumerable(Of String))
            mCode = code
            mLanguage = language
            mLanguageName = language.Name
            mNamespaces = namespaces.ToList().AsReadOnly()
            mReferences = references.ToList().AsReadOnly()
        End Sub

        ''' <summary>
        ''' The actual code that will be compiled.
        ''' </summary>
        ReadOnly Property Code As String
            Get
                Return mCode
            End Get
        End Property

        ''' <summary>
        ''' The language the code will be compiled using.
        ''' </summary>
        ReadOnly Property Language As CodeLanguage
            Get
                If mLanguage Is Nothing Then
                    mLanguage = CodeLanguage.GetByName(mLanguageName)
                End If
                Return mLanguage
            End Get
        End Property

        ''' <summary>
        ''' The namespaces that are required for the code to be compiled.
        ''' </summary>
        ReadOnly Property Namespaces As IReadOnlyCollection(Of String)
            Get
                Return mNamespaces
            End Get
        End Property

        ''' <summary>
        ''' The references that are required for the code to be compiled.
        ''' </summary>
        ReadOnly Property References As IReadOnlyCollection(Of String)
            Get
                Return mReferences
            End Get
        End Property

        ''' <summary>
        ''' Clones instances
        ''' </summary>
        ''' <returns>A clone</returns>
        Public Function Clone() As CodeProperties
            Return New CodeProperties(Code, Language, Namespaces, References)
        End Function

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="CodeProperties"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Function ToXElement() As XElement

            Return _
                <codeproperties code=<%= Code %> language=<%= Language.Name %>>
                    <namespaces><%= Namespaces.Select(Function(n) <namespace><%= n %></namespace>) %></namespaces>
                    <references><%= References.Select(Function(r) <reference><%= r %></reference>) %></references>
                </codeproperties>
        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="CodeProperties"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="CodeProperties"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As CodeProperties

            If Not element.Name.LocalName.Equals("codeproperties") Then _
                Throw New MissingXmlObjectException("codeproperties")

            Dim code = element.Attribute("code")?.Value

            If code Is Nothing Then _
                Throw New MissingXmlObjectException("code")

            Dim languageName = element.Attribute("language")?.Value

            If languageName Is Nothing Then _
                Throw New MissingXmlObjectException("language")

            Dim language = CodeLanguage.GetByName(languageName)

            Dim namespaces = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "namespaces")?.
                            Elements.
                            Where(Function(x) x.Name = "namespace").
                            Select(Function(x) x.Value)

            If namespaces Is Nothing Then Throw New MissingXmlObjectException("namespaces")

            Dim references = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "references")?.
                            Elements.
                            Where(Function(x) x.Name = "reference").
                            Select(Function(x) x.Value)

            If references Is Nothing Then Throw New MissingXmlObjectException("references")

            Return New CodeProperties(code, language, namespaces, references)

        End Function
    End Class
End Namespace
