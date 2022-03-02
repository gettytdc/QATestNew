Imports System.Linq

Namespace Compilation

    ''' <summary>
    ''' Contains information used to compile an assembly at runtime. A separate 
    ''' assembly is usually based on each individual component within Blue Prism 
    ''' that uses custom code. 
    ''' </summary>
    ''' <remarks>
    ''' This is currently used for business objects and Web APIs</remarks>
    Public Class AssemblyDefinition
        Inherits DefinitionBase
        ''' <summary>
        ''' Creates a new <see cref="AssemblyDefinition"/>
        ''' </summary>
        ''' <param name="name">The name of the assembly</param>
        ''' <param name="language">The code language</param>
        ''' <param name="namespaces">The namespaces imported by the generated source 
        ''' code</param>
        ''' <param name="referencedAssemblies">The assemblies referenced by the 
        ''' assembly</param>
        ''' <param name="classes">The definitions for classes that will be included 
        ''' in the assembly</param>
        Public Sub New(name As String, language As CodeLanguage, 
                       namespaces As IEnumerable(Of String), 
                       referencedAssemblies As IEnumerable(Of String), 
                       classes As IEnumerable(Of ClassDefinition))
            MyBase.New(name)
            Me.Language = language
            Me.Namespaces = namespaces.ToList().AsReadOnly
            Me.AssemblyReferences = referencedAssemblies.ToList().AsReadOnly
            Me.Classes = classes.ToList().AsReadOnly
        End Sub
        
        ''' <summary>
        ''' The code language in which source code is written
        ''' </summary>
        Public ReadOnly Property Language As CodeLanguage

        ''' <summary>
        ''' The namespaces imported by the generated source 
        ''' code
        ''' </summary>
        Public ReadOnly Property Namespaces As IList(Of String)

        ''' <summary>
        ''' The assemblies referenced by the assembly
        ''' </summary>
        Public ReadOnly Property AssemblyReferences As IList(Of String)

        ''' <summary>
        ''' The definitions for classes that will be included in the assembly
        ''' </summary>
        Public ReadOnly Property Classes As IList(Of ClassDefinition)

        ''' <summary>
        ''' Indicates whether this definition contains any code to compile
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Classes.All(Function(c)c.IsEmpty)
            End Get
        End Property

    End Class
End NameSpace