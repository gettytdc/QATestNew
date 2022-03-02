Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Delegate used to handle multi-session create events
''' </summary>
Public Delegate Sub MultiSessionCreateEventHandler(
    sender As Object, e As MultiSessionCreateEventArgs)

''' <summary>
''' Event Args used to define a number of processes being assigned to a
''' number of resources.
''' </summary>
Public Class MultiSessionCreateEventArgs : Inherits EventArgs

    ' The resource names on which the sessions were created
    Private mResourceNames As ICollection(Of String)

    ' The IDs of the processes to be run in the created sessions
    Private mProcessIds As ICollection(Of Guid)

    ''' <summary>
    ''' Creates new event args describing a number of processes being run on a single
    ''' resource.
    ''' </summary>
    ''' <param name="processIds">The IDs of the processes to be created.</param>
    ''' <param name="resourceName">The name of the resource on which the processes
    ''' should be run</param>
    Public Sub New(ByVal processIds As ICollection(Of Guid), ByVal resourceName As String)
        Me.New(processIds, GetSingleton.ICollection(resourceName))
    End Sub

    ''' <summary>
    ''' Creates new event args describing a process being run on a number of resources.
    ''' </summary>
    ''' <param name="processId">The ID of the process to be created.</param>
    ''' <param name="resourceNames">The names of the resources on which the process
    ''' should be run</param>
    Public Sub New(ByVal processId As Guid, ByVal resourceNames As ICollection(Of String))
        Me.New(GetSingleton.ICollection(processId), resourceNames)
    End Sub

    ''' <summary>
    ''' Creates new event args describing a number of processes being run on a number of
    ''' resources.
    ''' </summary>
    ''' <param name="processIds">The IDs of the processes to be created.</param>
    ''' <param name="resourceNames">The names of the resources on which the process
    ''' should be run</param>
    ''' <remarks>Note that this effectively requests a sessions for all of the given
    ''' processes on all of the given resources.</remarks>
    Public Sub New(ByVal processIds As ICollection(Of Guid), ByVal resourceNames As ICollection(Of String))
        mProcessIds = processIds
        mResourceNames = resourceNames
    End Sub

    ''' <summary>
    ''' The resource names on which the sessions have been requested
    ''' </summary>
    Public ReadOnly Property ResourceNames() As ICollection(Of String)
        Get
            Return mResourceNames
        End Get
    End Property

    ''' <summary>
    ''' The IDs of the processes for which sessions have been requested.
    ''' </summary>
    Public ReadOnly Property ProcessIds() As ICollection(Of Guid)
        Get
            Return mProcessIds
        End Get
    End Property

End Class
