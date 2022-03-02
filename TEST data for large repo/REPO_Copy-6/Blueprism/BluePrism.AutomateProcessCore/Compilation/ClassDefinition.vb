Imports System.CodeDom
Imports System.Linq

Namespace Compilation

    ''' <summary>
    ''' Contains the contents of an individual class that will be included in an 
    ''' assembly
    ''' </summary>
    Public Class ClassDefinition
        Inherits DefinitionBase

        ''' <summary>
        ''' Creates a new <see cref="ClassDefinition"/>
        ''' </summary>
        ''' <param name="name">Name used to reference the class</param>
        ''' <param name="methods"></param>
        ''' <param name="sharedCode"></param>
        ''' <param name="sharedCodeFileName"></param>
        Public Sub New(name As String,
                       methods As IEnumerable(Of MethodDefinition),
                       sharedCode As String,
                       sharedCodeFileName As String)
            MyBase.New(name)
            Me.Methods = methods.ToList().AsReadOnly
            Me.SharedCode = sharedCode
            Me.SharedCodeFileName = sharedCodeFileName
        End Sub

        ''' <summary>
        ''' The methods included in this class
        ''' </summary>
        Public ReadOnly Property Methods As IList(Of MethodDefinition)

        ''' <summary>
        ''' Any additional shared code included in this class
        ''' </summary>
        Public ReadOnly Property SharedCode As String

        ''' <summary>
        ''' An identifier used to indicate the source of the shared code in the source code
        ''' - this is used in a <see cref="CodeLinePragma"/> element when generating the code. 
        ''' It is normally used for a file name, but is used to store the id of a code stage
        ''' or similar origin
        ''' </summary>
        Public ReadOnly Property SharedCodeFileName As String

        ''' <summary>
        ''' Indicates whether this class contains any code to compile
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrWhiteSpace(SharedCode) AndAlso
                    Not Methods.Any()
            End Get
        End Property

    End Class
End Namespace