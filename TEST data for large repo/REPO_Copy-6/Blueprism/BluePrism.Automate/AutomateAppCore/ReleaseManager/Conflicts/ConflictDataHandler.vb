Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' An extra data handler which accepts additional data once a conflict option
''' has been chosen.
''' </summary>
Public Class ConflictDataHandler : Implements ICloneable

    ' The ID of this handler
    Private mId As String

    ' The text detailing the data required.
    Private mText As String

    ' The arguments which make up the additional data.
    Private mArgs As ICollection(Of ConflictArgument)

    ''' <summary>
    ''' Creates a new data handler with the given properties.
    ''' </summary>
    ''' <param name="id">The ID for this handler</param>
    ''' <param name="txt">The text to display to the user to request the extra data
    ''' </param>
    ''' <param name="args">The arguments which are required to be handled by this
    ''' object.</param>
    Public Sub New(ByVal id As String, ByVal txt As String, ByVal ParamArray args() As ConflictArgument)
        Me.New(id, txt, DirectCast(args, ICollection(Of ConflictArgument)))
    End Sub

    ''' <summary>
    ''' Creates a new data handler with the given properties.
    ''' </summary>
    ''' <param name="id">The ID for this handler</param>
    ''' <param name="txt">The text to display to the user to request the extra data
    ''' </param>
    ''' <param name="args">The arguments which are required to be handled by this
    ''' object.</param>
    Public Sub New(ByVal id As String, ByVal txt As String, ByVal args As ICollection(Of ConflictArgument))
        mId = id
        mText = txt
        mArgs = New List(Of ConflictArgument)(args)
    End Sub

    ''' <summary>
    ''' The ID for this handler within a conflict option.
    ''' </summary>
    Public ReadOnly Property Id() As String
        Get
            Return mId
        End Get
    End Property

    ''' <summary>
    ''' The text displayed to request the extra data.
    ''' </summary>
    Public ReadOnly Property Text() As String
        Get
            Return mText
        End Get
    End Property

    ''' <summary>
    ''' The readonly collection of arguments that this handler will collect.
    ''' </summary>
    Public ReadOnly Property Arguments() As ICollection(Of ConflictArgument)
        Get
            Return GetReadOnly.ICollection(InternalArguments)
        End Get
    End Property

    ''' <summary>
    ''' A modifiable collection of arguments that this handler will collect.
    ''' </summary>
    Private ReadOnly Property InternalArguments() As ICollection(Of ConflictArgument)
        Get
            If mArgs Is Nothing Then mArgs = New List(Of ConflictArgument)
            Return mArgs
        End Get
    End Property

    ''' <summary>
    ''' Clones this data handler
    ''' </summary>
    ''' <returns>A deep clone of this data handler</returns>
    Private Function CloneObject() As Object Implements ICloneable.Clone
        Return Clone()
    End Function

    ''' <summary>
    ''' Deep-clones this data handler
    ''' </summary>
    ''' <returns>A deep clone of this handler</returns>
    Public Function Clone() As ConflictDataHandler
        Dim copy As ConflictDataHandler = DirectCast(MemberwiseClone(), ConflictDataHandler)
        If mArgs IsNot Nothing Then
            copy.mArgs = Nothing ' reset it so it's auto-created in the property
            ' If we have any args, clone them into the copied data handler
            For Each arg As ConflictArgument In mArgs
                copy.InternalArguments.Add(arg.Clone())
            Next
        End If
        Return copy
    End Function

End Class
