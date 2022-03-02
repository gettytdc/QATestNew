Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class describing a resolution to a conflict (which may or may not be a valid
''' resolution - it is up to the component to determine whether the data entered
''' is valid or not)
''' </summary>
Public Class ConflictResolution

    ' The conflict that this (maybe) resolves
    Private mConflict As Conflict

    ' The option chosen for this resolution - null if no option has been chosen
    Private mOption As ConflictOption

    ' The handler for additional data handled in this resolution
    Private mHandler As ConflictDataHandler

    ''' <summary>
    ''' Creates a new resolution for the given conflict.
    ''' </summary>
    ''' <param name="conflict">The conflict that this object 'resolves'</param>
    Public Sub New(ByVal conflict As Conflict)
        Me.New(conflict, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new resolution for the given conflict, indicating the given option
    ''' </summary>
    ''' <param name="conflict">The conflict that this object 'resolves'</param>
    ''' <param name="opt">The option chosen as part of the resolution of the conflict
    ''' or null if no option has been chosen.</param>
    Public Sub New(ByVal conflict As Conflict, ByVal opt As ConflictOption)
        Me.New(conflict, opt, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new resolution for the given conflict, indicating the given option
    ''' </summary>
    ''' <param name="conflict">The conflict that this object 'resolves'</param>
    ''' <param name="opt">The option chosen as part of the resolution of the conflict
    ''' or null if no option has been chosen.</param>
    ''' <param name="handler">The handler for the extra data held in this resolution,
    ''' or null if no extra data is to be held.</param>
    Public Sub New(ByVal conflict As Conflict, ByVal opt As ConflictOption, ByVal handler As ConflictDataHandler)
        mConflict = conflict
        mOption = opt
        mHandler = handler
    End Sub

    ''' <summary>
    ''' The conflict which this resolution pertains to.
    ''' </summary>
    Public ReadOnly Property Conflict() As Conflict
        Get
            Return mConflict
        End Get
    End Property

    ''' <summary>
    ''' The conflict option chosen in this resolution.
    ''' </summary>
    Public ReadOnly Property ConflictOption() As ConflictOption
        Get
            Return mOption
        End Get
    End Property

    ''' <summary>
    ''' The extra data handler holding the additional data required by this
    ''' resolution. Null if this resolution is maintaining no extra data.
    ''' </summary>
    Public ReadOnly Property ConflictDataHandler() As ConflictDataHandler
        Get
            Return mHandler
        End Get
    End Property

    ''' <summary>
    ''' The first argument in the extra data held by this resolution, or null if
    ''' no such argument (or extra data) exists
    ''' </summary>
    Public ReadOnly Property FirstArgument() As ConflictArgument
        Get
            If mHandler Is Nothing Then Return Nothing
            If mHandler.Arguments.Count = 0 Then Return Nothing
            Return CollectionUtil.First(mHandler.Arguments)
        End Get
    End Property

    ''' <summary>
    ''' Gets the argument value from the argument with the given name, held in the
    ''' extra data within this resolution.
    ''' </summary>
    ''' <param name="name">The name of the argument required from this resolution.
    ''' </param>
    ''' <returns>The process value held by this resolution under the given name.
    ''' </returns>
    Public Function GetArgumentValue(ByVal name As String) As clsProcessValue
        If mHandler Is Nothing Then Return Nothing
        For Each arg As ConflictArgument In mHandler.Arguments
            If arg.Name = name Then Return arg.Value
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the encoded argument value from the argument with the given name, held
    ''' in the extra data within this resolution.
    ''' </summary>
    ''' <param name="name">The name of the argument required from this resolution.
    ''' </param>
    ''' <returns>The encoded value of the process value held by this resolution under
    ''' the given name.
    ''' </returns>
    Public Function GetArgumentString(ByVal name As String) As String
        Return CStr(GetArgumentValue(name))
    End Function

    ''' <summary>
    ''' Gets or sets whether this resolution has been passed - ie. accepted as
    ''' resolving the associated conflict.
    ''' If set to true, this will set itself as the resolution in the conflict;
    ''' if set to false, it will clear itself as the resolution (but not clear any
    ''' other resolution which may have been set in the conflict).
    ''' </summary>
    Public Property Passed() As Boolean
        Get
            Return (mConflict.Resolution Is Me)
        End Get
        Set(ByVal value As Boolean)
            If value Then mConflict.Resolution = Me : Return
            ' Otherwise, we're setting Passed to false.
            ' Remove this resolution from the associated conflict
            If mConflict.Resolution Is Me Then mConflict.Resolution = Nothing
        End Set
    End Property

End Class
