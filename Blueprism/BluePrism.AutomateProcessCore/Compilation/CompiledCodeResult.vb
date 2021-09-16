
Imports System.CodeDom.Compiler
Imports System.Reflection

Namespace Compilation

    ''' <summary>
    ''' Contains the result of compiling an assembly from source code in an 
    ''' <see cref="AssemblyDefinition"/>. 
    ''' </summary>
    Public Class CompiledCodeResult

        ''' <summary>
        ''' Gets an empty result
        ''' </summary>
        Public Shared ReadOnly Empty As CompiledCodeResult = 
                                   New CompiledCodeResult(Nothing, "", New CompilerErrorCollection())

        ''' <summary>
        ''' Creates a new <see cref="CompiledCodeResult"/>
        ''' </summary>
        ''' <param name="assembly">The compiled assembly (or nothing if compiling failed)</param>
        ''' <param name="sourceCode">The source code generated to compile the assembly</param>
        ''' <param name="errors">The errors found during compilation</param>
        Sub New(assembly As Assembly, sourceCode As String, errors As CompilerErrorCollection)
            Me.Assembly = assembly
            Me.SourceCode = sourceCode
            Me.Errors = errors
        End Sub

        ''' <summary>
        ''' The compiled assembly (or nothing if compiling failed)
        ''' </summary>
        Public ReadOnly Property Assembly As Assembly

        ''' <summary>
        ''' Indicates whether an assembly has been generated
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Assembly Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' The source code on which the compiled assembly is based
        ''' </summary>
        Public ReadOnly Property SourceCode As String

        ''' <summary>
        ''' A collection of errors raised when compiling the source code
        ''' </summary>
        Public ReadOnly Property Errors As CompilerErrorCollection

        Public ReadOnly Property FormattedErrors As String
            Get
                Return String.Join(", ", GetErrors())
            End Get
        End Property

        Private Iterator Function GetErrors() As IEnumerable(Of String)
            For Each _error As CompilerError In Errors
                Yield _error.ErrorText
            Next
        End Function
    End Class
End Namespace