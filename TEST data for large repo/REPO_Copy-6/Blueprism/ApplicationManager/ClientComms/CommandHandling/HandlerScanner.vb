Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection

Namespace CommandHandling

    ''' <summary>
    ''' Locates ICommandHandler implementations in an assembly within a specific namespace
    ''' </summary>
    Friend Class HandlerScanner

        Private ReadOnly mIdConvention As ICommandIdConvention

        ''' <summary>
        ''' Creates a new HandlerScanner
        ''' </summary>
        ''' <param name="idConvention">The convention used to derive a unique id
        ''' that ties a command to a specific handler</param>
        Public Sub New(idConvention As ICommandIdConvention)
            mIDConvention = idConvention
        End Sub

        ''' <summary>
        ''' Finds all ICommandHandler implementations in an assembly within the given 
        ''' namespace
        ''' </summary>
        ''' <param name="assembly">The assembly to scan</param>
        ''' <param name="parentNamespace">The parent namespace to search for handler
        ''' implementations</param>
        ''' <returns>A sequence of HandlerDescriptors for the</returns>
        Public Function FindHandlers(assembly As Assembly, parentNamespace As String) _
            As IEnumerable(Of HandlerDescriptor)

            Dim descriptors = From handlerType In GetHandlerTypes(assembly, parentNamespace) 
                              Let commandName As String = GetCommandId(handlerType)
                              Select New HandlerDescriptor(handlerType, commandName)
            Return descriptors

        End Function

        Private Function GetHandlerTypes(assembly As Assembly, parentNamespace As String) As IEnumerable(Of Type)

            Return From type In assembly.GetTypes Where IsHandler(type, parentNamespace)

        End Function

        Private Function IsHandler(type As Type, parentNamespace As String) As Boolean

            Return type.IsClass AndAlso
                Not type.IsAbstract AndAlso
                GetType(ICommandHandler).IsAssignableFrom(type) AndAlso
                type.Namespace IsNot Nothing AndAlso
                type.Namespace.StartsWith(parentNamespace, StringComparison.OrdinalIgnoreCase)

        End Function

        Private Function GetCommandId(type As Type) As String

            Return mIdConvention.GetId(type)

        End Function

        
        
    End Class
End Namespace