Imports System.Linq

Namespace CommandHandling
    
    ''' <summary>
    ''' Creates command id based on CommandIdAttribute defined on the type
    ''' </summary>
    Friend Class DefaultCommandIdConvention :
        Implements ICommandIdConvention

        ''' <inheritdoc/>
        Public Function GetId(handlerType As Type) _
            As String Implements ICommandIdConvention.GetId

            Dim id As String = handlerType.GetCustomAttributes(False) _
                .OfType(Of CommandIdAttribute) _
                .Select(Function(attribute)attribute.Id) _
                .FirstOrDefault()
            Return If(id IsNot Nothing, id, handlerType.FullName)

        End Function

    End Class
End NameSpace