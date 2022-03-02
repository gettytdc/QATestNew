''' -----------------------------------------------------------------------------
''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.UndoRedoManager
''' 
''' -----------------------------------------------------------------------------
''' <summary>
''' Provides a general mechanism for undo/redo operations. This class can easily
''' be extended to use the .NET 2.0 generics feature so that it is no longer
''' specific to strings.
''' 
''' Allows user to "undo the undo" if they undo some changes and then start
''' adding new states - successive use of the undo operation will redo the changes
''' they removed using the undo operation.
''' </summary>
''' -----------------------------------------------------------------------------
Public Class UndoRedoManager

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Determines if a meaningful state will be returned if the Undo() method
    ''' is called.
    ''' </summary>
    ''' <returns>Returns true a meaningful backward state is available; false
    ''' otherwise.</returns>
    ''' -----------------------------------------------------------------------------
    Public Function CanUndo() As Boolean
        Return Me.miCurrentBufferIndex > 1
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Determines if a meaningful state will be returned if the Redo() method
    ''' is called.
    ''' </summary>
    ''' <returns>Returns true a meaningful forward state is available; false
    ''' otherwise.</returns>
    ''' -----------------------------------------------------------------------------
    Public Function CanRedo() As Boolean
        Return Me.miCurrentBufferIndex < Me.mStatesBuffer.Count
    End Function


    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Returns the penultimate state added. Alternatively if a new state was
    ''' added after an undo operation, calling this method will "undo the undo
    ''' operation".
    ''' 
    ''' </summary>
    ''' <param name="NewXML">Carries back the new XML contained in the undo
    ''' point.</param>
    ''' <returns>Returns true if an undo point was found and applied;
    ''' returns false otherwise (ie if no change was made to the parameter 
    ''' NewXML).</returns>
    ''' -----------------------------------------------------------------------------
    Public Function Undo(ByRef NewXML As String) As Boolean
        If Me.CanUndo Then
            Me.miCurrentBufferIndex -= 1
            NewXML = CType(Me.mStatesBuffer.Item(Me.miCurrentBufferIndex), String)
            Return True
        Else
            Return False
        End If
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Performs the opposite of the undo operation.
    ''' 
    ''' </summary>
    ''' <param name="NewXML">Carries back the new XML contained in the redo
    ''' point.</param>
    ''' <returns>Returns true if a redo point was found and applied;
    ''' returns false otherwise (ie if no change was made to the parameter 
    ''' NewXML).</returns>
    ''' -----------------------------------------------------------------------------
    Public Function Redo(ByRef NewXML As String) As Boolean
        If Me.CanRedo Then
            Me.miCurrentBufferIndex += 1
            NewXML = CType(Me.mStatesBuffer.Item(Me.miCurrentBufferIndex), String)
            Return True
        Else
            Return False
        End If
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The states added to this undo/redo buffer.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Private mStatesBuffer As New Collection

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Points to the state we are at now in the buffer.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Private miCurrentBufferIndex As Integer

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Adds a state to the buffer, making it available for future undo/redo
    ''' operations. Performing an undo operation will not return the last state
    ''' added, but the penultimate state. Thus the latest state added here should
    ''' mirror the latest state of your object.
    ''' </summary>
    ''' <param name="sXML">The state to be added.</param>
    ''' -----------------------------------------------------------------------------
    Public Sub AddState(ByVal sXML As String)
        'if we are not at the front of the buffer then we copy the
        'forward states to the front of the buffer in reverse order
        'so that we can "undo the undo".
        Dim i As Integer = Me.mStatesBuffer.Count - 1
        While i >= Me.miCurrentBufferIndex
            Me.mStatesBuffer.Add(Me.mStatesBuffer.Item(i))
            i -= 1
        End While
        Me.mStatesBuffer.Add(sXML)
        Me.miCurrentBufferIndex = Me.mStatesBuffer.Count
    End Sub

End Class
