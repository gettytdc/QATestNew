Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Event handlers for group events.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="e">The args detailing the event.</param>
Public Delegate Sub GroupEventHandler(sender As Object, e As GroupEventArgs)

''' <summary>
''' Event Args detailing an event targeting a specific group member
''' </summary>
Public Class GroupEventArgs : Inherits GroupMemberEventArgs

    ''' <summary>
    ''' Creates a new event args object referring to the given group
    ''' </summary>
    ''' <param name="targ">The group to which the event refers</param>
    Public Sub New(targ As IGroup)
        MyBase.New(targ)
    End Sub

    ''' <summary>
    ''' The target group to which this event refers.
    ''' </summary>
    Public Shadows ReadOnly Property Target As IGroup
        Get
            Return DirectCast(MyBase.Target, IGroup)
        End Get
    End Property

End Class
