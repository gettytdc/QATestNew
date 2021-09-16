Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Resources

Public Class ActiveRobot

    Private mMachine As ResourceMachine
    Private mSessions As ICollection(Of clsProcessSession)

    ''' <summary>
    ''' Creates a new active robot
    ''' </summary>
    ''' <param name="mach">The resource machine representing the active robot.
    ''' </param>
    Public Sub New(mach As ResourceMachine)
        If mach Is Nothing Then Throw New ArgumentNullException(NameOf(mach))
        mMachine = mach
    End Sub

    ''' <summary>
    ''' The machine representing a connection to the resource which hosts the robot
    ''' </summary>
    Public ReadOnly Property Machine As ResourceMachine
        Get
            Return mMachine
        End Get
    End Property

    ''' <summary>
    ''' Gets a readonly collection of the sessions running on this robot
    ''' </summary>
    Public ReadOnly Property Sessions As ICollection(Of clsProcessSession)
        Get
            If mSessions IsNot Nothing Then Return GetReadOnly.ICollection(mSessions)
            Return GetEmpty.ICollection(Of clsProcessSession)()
        End Get
    End Property

    ''' <summary>
    ''' Updates the sessions for this robot from the database
    ''' </summary>
    ''' <param name="sv">The clsServer instance to use to update the sessions</param>
    Public Sub UpdateSessions(sv As clsServer)
        mSessions = sv.GetActualSessions(Machine.Name)
    End Sub

End Class
