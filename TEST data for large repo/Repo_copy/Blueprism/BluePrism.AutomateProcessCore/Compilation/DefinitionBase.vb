Namespace Compilation

    ''' <summary>
    ''' Base class for elements within the definition of an assembly's code
    ''' </summary>
    Public MustInherit Class DefinitionBase

        ''' <summary>
        ''' Creates a new <see cref="ClassDefinition"/>
        ''' </summary>
        ''' <param name="name">Name used to reference the class</param>
        Protected Sub New(name As String)
            Me.Name = name
            Me.Identifier = CodeCompiler.GetIdentifier(name)
        End Sub

        ''' <summary>
        ''' A name used for reference - normally based on the Blue Prism element on
        ''' which this element is based
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' The exact name used for this element in the code
        ''' </summary>
        Public ReadOnly Property Identifier As String

    End Class
End Namespace