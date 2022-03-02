
''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsVBODetails
''' <summary>
''' This class is used to store and transfer details about a
''' particular Visual Business Object registered with Automate and available
''' to processes.
''' </summary>
Public Class clsVBODetails : Implements IObjectDetails

    ''' <summary>
    ''' The ID (as found in the process table, for example) of the object
    ''' </summary>
    Public Property ID() As Guid
        Get
            Return mgID
        End Get
        Set(ByVal Value As Guid)
            mgID = Value
        End Set
    End Property
    Private mgID As Guid

    ''' <summary>
    ''' The 'friendly name' for the object - this is the name
    ''' that is presented to users of Automate when editing a process.
    ''' </summary>
    Public Property FriendlyName() As String Implements IObjectDetails.FriendlyName
        Get
            Return msFriendlyName
        End Get
        Set(ByVal Value As String)
            msFriendlyName = Value
        End Set
    End Property
    Private msFriendlyName As String

End Class
