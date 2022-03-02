Namespace Compilation

    ''' <summary>
    ''' Contains the details of an individual parameter within a 
    ''' <see cref="MethodDefinition"/>
    ''' </summary>
    Public Class MethodParameterDefinition
        Inherits DefinitionBase

        ''' <summary>
        ''' Creates a new <see cref="MethodParameterDefinition"/>
        ''' </summary>
        ''' <param name="name">A name used for reference - normally based on the Blue 
        ''' Prism element on
        ''' which this parameter is based</param>
        ''' <param name="dataType">The Blue Prism data type for the parameter</param>
        ''' <param name="isOutput">Defines whether this is an output parameter</param>
        Sub New(name As String, dataType As DataType, isOutput As Boolean)
            MyBase.New(name)
            Me.DataType = dataType
            Me.IsOutput = isOutput
        End Sub
        
        ''' <summary>
        ''' The Blue Prism data type for the parameter
        ''' </summary>
        Public ReadOnly Property DataType As DataType

        ''' <summary>
        ''' Defines whether this is an output parameter
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsOutput As Boolean

    End Class
End NameSpace