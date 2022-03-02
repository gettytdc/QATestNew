Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib.Collections
Imports CompilationLanguage = BluePrism.AutomateProcessCore.Compilation

Public Class CodePropertiesDetails

    ''' <summary>
    ''' Creates an instance of CodePropertiesDetails.
    ''' </summary>
    ''' <param name="code">The code for compilation.</param>
    ''' <param name="namespaces">The namespaces for compilation.</param>
    ''' <param name="references">The references for compilation.</param>
    Sub New(code As String,
            language As CompilationLanguage.CodeLanguage,
            namespaces As IEnumerable(Of String),
            references As IEnumerable(Of String))
        Me.Code = code
        Me.Language = language
        Me.Namespaces.AddAll(namespaces)
        Me.References.AddAll(references)
    End Sub

    Sub New()
        Me.Code = String.Empty
        Me.Language = CompilationLanguage.CodeLanguage.VisualBasic
    End Sub

    ''' <summary>
    ''' The actual code that will be compiled.
    ''' </summary>
    Property Code As String
    ''' <summary>
    ''' The language the code will be compiled using.
    ''' </summary>
    Property Language As CompilationLanguage.CodeLanguage
    ''' <summary>
    ''' The namespaces that are required for the code to be compiled.
    ''' </summary>
    ReadOnly Property Namespaces As New WebApiCollection(Of String) With {
            .ActionSpecific = True
        }
    ''' <summary>
    ''' The references that are required for the code to be compiled.
    ''' </summary>
    ReadOnly Property References As New WebApiCollection(Of String) With {
        .ActionSpecific = True
        }

    ''' <summary>
    ''' Adds a collection of namespaces to this CodePropertyDetails and returns it.
    ''' </summary>
    ''' <param name="namespaces">The parameters to add to this CodePropertyDetails.</param>
    ''' <returns>This CodePropertyDetails with the parameters added.</returns>
    Public Function WithNamespaces(namespaces As IEnumerable(Of String)) As CodePropertiesDetails
        If namespaces IsNot Nothing Then Me.Namespaces.AddAll(namespaces)
        Return Me
    End Function

    ''' <summary>
    ''' Adds a collection of references to this CodePropertyDetails and returns it.
    ''' </summary>
    ''' <param name="references">The parameters to add to this CodePropertyDetails.</param>
    ''' <returns>This CodePropertyDetails with the parameters added.</returns>
    Public Function WithReferences(references As IEnumerable(Of String)) As CodePropertiesDetails
        If references IsNot Nothing Then Me.References.AddAll(references)
        Return Me
    End Function

    ''' <summary>
    ''' Converts an instance of the immutable Codeproperties class to a mutable
    ''' instance of this class.
    ''' </summary>
    ''' <param name="props">The code properties to convert to a mutable CodePropertyDetails
    ''' instance.</param>
    ''' <returns>The CodePropertyDetails instance with the same value as
    ''' <paramref name="props"/>.</returns>
    Public Shared Widening Operator CType(props As CodeProperties) As CodePropertiesDetails
        If props Is Nothing Then Return Nothing

        Return New CodePropertiesDetails() With {
            .Code = props.Code,
            .Language = props.Language
            }.WithNamespaces(props.Namespaces).WithReferences(props.References)

    End Operator

    ''' <summary>
    ''' Converts an instance of this class into a <see cref="CodeProperties"/>
    ''' </summary>
    ''' <param name="codeDetails">The details to convert</param>
    ''' <returns>A CodeProperties with the same value as the given details object.
    ''' </returns>
    Public Shared Widening Operator CType(codeDetails As CodePropertiesDetails) As CodeProperties

        If codeDetails Is Nothing Then Return Nothing

        Return New CodeProperties(codeDetails.Code,
                                  codeDetails.Language,
                                  codeDetails.Namespaces,
                                  codeDetails.References)
    End Operator

End Class



