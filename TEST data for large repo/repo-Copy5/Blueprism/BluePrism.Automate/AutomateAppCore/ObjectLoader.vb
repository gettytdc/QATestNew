Imports BluePrism.AutomateProcessCore

''' <summary>
''' Implements an object loader which creates all the internal business objects
''' </summary>
Public Class ObjectLoader : Implements IObjectLoader

    ''' <summary>
    ''' Function to create all the internal business objects
    ''' </summary>
    ''' <param name="p">A reference to the process calling the object</param>
    ''' <param name="s">The session the object is running under</param>
    ''' <returns>A list of internal business objects</returns>
    Public Function CreateAll(p As clsProcess, s As clsSession) _
        As IEnumerable(Of clsInternalBusinessObject) _
        Implements IObjectLoader.CreateAll

        Dim list As New List(Of clsInternalBusinessObject)
        list.Add(New clsCalendarsBusinessObject(p, s))
        list.Add(New clsCredentialsBusinessObject(p, s))
        list.Add(New clsDataGatewaysBusinessObject(p, s))
        list.Add(New clsEncryptionBusinessObject(p, s))
        list.Add(New clsEnvironmentLockingBusinessObject(p, s))
        list.Add(New clsWorkQueuesBusinessObject(p, s))

        Return list

    End Function

End Class
