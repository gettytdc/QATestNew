Imports System.CodeDom
Imports System.Linq

Namespace Compilation

    ''' <summary>
    ''' Contains the details specified for an individual method
    ''' </summary>
    Public Class MethodDefinition
        Inherits DefinitionBase

        ''' <summary>
        ''' Creates a new <see cref="MethodDefinition"/>
        ''' </summary>
        ''' <param name="name">A name used for reference - normally based on the Blue
        ''' Prism element on which the method is based</param>
        ''' <param name="fileName">An identifier used to indicate the source of the method 
        ''' in the source code - this is used in a <see cref="CodeLinePragma"/> 
        ''' element when generating the code. It is normally used for a file name, 
        ''' but is used to store the id of a code stage or other component from
        ''' which it originated</param>
        ''' <param name="body">The textual body of the method that this code stage 
        ''' represents. The code for the opening method declaration and closing of 
        ''' the method is not included here.</param>
        ''' <param name="parameters">The parameters for the method</param>
        Sub New(name As String, fileName As String, body As String, parameters As IEnumerable(Of MethodParameterDefinition))
            MyBase.New(name)
            Me.FileName = fileName
            Me.Body = body
            Me.Parameters = parameters.ToList().AsReadOnly
        End Sub
        
        ''' <summary>
        ''' An identifier used to indicate the source of the method in the source code
        ''' - this is used in a <see cref="CodeLinePragma"/> element when generating the code. 
        ''' It is normally used for a file name, but is used to store the id of a code stage
        ''' or other component from which it originated.
        ''' </summary>
        Public ReadOnly Property FileName As String

        ''' <summary>
        ''' The textual body of the method that this code stage represents. The code 
        ''' for the opening method declaration and closing of the method is not 
        ''' included here.
        ''' </summary>
        Public Property Body As String

        ''' <summary>
        ''' The parameters for the method
        ''' </summary>
        Public ReadOnly Property Parameters As IList(Of MethodParameterDefinition)

    End Class
End Namespace