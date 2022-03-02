Namespace Groups

    ''' <summary>
    ''' Delegate describing a handler which handles group tree events
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The args detailing the event</param>
    Public Delegate Sub GroupPathChangedEventHandler(
     sender As Object, e As GroupPathChangedEventArgs)

    ''' <summary>
    ''' Event args for effected changes to a group path.
    ''' </summary>
    Public Class GroupPathChangedEventArgs : Inherits EventArgs

        ' The old name of the group
        Private mOldPath As String

        ' The group whose name has changed
        Private mNewPath As String

        ''' <summary>
        ''' Creates a new event args object around a group tree.
        ''' </summary>
        ''' <param name="prevPath">The previous path where the change occurred. This
        ''' must be present for this type of event object.</param>
        ''' <param name="currPath">The path after the change, or null if the path has
        ''' been deleted.</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="prevPath"/>
        ''' is null.</exception>
        Public Sub New(prevPath As String, currPath As String)
            If prevPath Is Nothing Then Throw New ArgumentNullException(NameOf(prevPath))
            mOldPath = prevPath
            mNewPath = currPath
        End Sub

        ''' <summary>
        ''' The previous path, or an empty string if there is no previous path.
        ''' </summary>
        Public ReadOnly Property PreviousPath As String
            Get
                Return mOldPath
            End Get
        End Property

        ''' <summary>
        ''' The new path value after the change or null if the path has been deleted
        ''' </summary>
        Public ReadOnly Property NewPath As String
            Get
                Return mNewPath
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this event args object is being raised because a path
        ''' has been removed from the model - ie. it has been deleted
        ''' </summary>
        Public ReadOnly Property IsPathDeleted As Boolean
            Get
                Return (mNewPath Is Nothing)
            End Get
        End Property

    End Class

End Namespace
