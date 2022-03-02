Imports System.Reflection
Imports BluePrism.AutomateProcessCore.Compilation

Namespace WebApis.CustomCode

    ''' <summary>
    ''' Contains an Assembly generated from custom code together within the
    ''' AssemblyDefinition on which it is based
    ''' </summary>
    Public Class AssemblyData

        Public Sub New(assemblyDefinition As AssemblyDefinition, assembly As Assembly)
            Me.Assembly = assembly
            Me.AssemblyDefinition = assemblyDefinition
        End Sub

        Public ReadOnly Property Assembly As Assembly

        Public ReadOnly Property AssemblyDefinition As AssemblyDefinition

    End Class

    
End NameSpace