Namespace Groups

    ''' <summary>
    ''' Delegate describing a handler which handles group tree events
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The args detailing the event</param>
    Public Delegate Sub GroupTreeEventHandler(
     sender As Object, e As GroupTreeEventArgs)

    ''' <summary>
    ''' Event args for group tree events
    ''' </summary>
    Public Class GroupTreeEventArgs : Inherits EventArgs

        ' The tree which this event represents
        Private mTree As IGroupTree

        ''' <summary>
        ''' Creates a new event args object around a group tree.
        ''' </summary>
        ''' <param name="gpTree">The tree regarding which the event occurred.</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="gpTree"/> is
        ''' null.</exception>
        Public Sub New(gpTree As IGroupTree)
            If gpTree Is Nothing Then Throw New ArgumentNullException(NameOf(gpTree))
            mTree = gpTree
        End Sub

        ''' <summary>
        ''' The tree to which the associated event refers
        ''' </summary>
        Public ReadOnly Property Tree As IGroupTree
            Get
                Return mTree
            End Get
        End Property

    End Class

End Namespace
