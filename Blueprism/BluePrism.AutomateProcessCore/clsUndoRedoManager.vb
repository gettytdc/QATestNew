Imports System.Text
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports BluePrism.Common.Security

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.UndoRedoManager
''' 
''' <summary>
''' Provides a general mechanism for undo/redo operations.
''' Allows user to "undo the undo" if they undo some changes and then start
''' adding new states - successive use of the undo operation will redo the changes
''' they removed using the undo operation.
''' </summary>
Public Class clsUndoRedoManager : Implements IDisposable

#Region "Fields"
    ''' <summary>
    ''' The path we will use to store the und information
    ''' </summary>
    Private ReadOnly mTempFolder As String = Path.GetTempPath()

    ''' <summary>
    ''' Points to the state we are at now in the buffer.
    ''' </summary>
    Private mCurrentBufferIndex As Integer

    ''' <summary>
    ''' The maximum number of undo levels that will be kept
    ''' </summary>
    Private mMaxUndoLevels As Integer = 20

    ''' <summary>
    ''' The states added to this undo/redo buffer.
    ''' </summary>
    Private mStatesBuffer As New List(Of clsUndo)

    ''' <summary>
    ''' Indicates if GZIP is being used.
    ''' </summary>
    Private mUseGzip As Boolean = False
    
    ''' <summary>
    ''' The obfuscated key used in the encrypt decrypt operations
    ''' </summary>
    Private shared mObfuscatedKey As Byte()
    ''' <summary>
    ''' Initialisation vector for the encrypt decrypt operations
    ''' </summary>
    Private Const IvSize As Integer = 16
#End Region

#Region "Events"
    ''' <summary>
    ''' Notifies clients of significant changes. The following is a comprehensive
    ''' list:
    ''' 
    ''' a) New state added.
    ''' b) All states cleared.
    ''' c) A call to undo/redo is made, meaning that the value of a call to 
    '''    CanUndo() or CanRedo() may now be different.
    ''' </summary>
    ''' <param name="Status">The new status of the manager.</param>
    Public Event StatusChanged(ByVal status As ManagerStates)

#End Region

#Region "Enums"
    ''' <summary>
    ''' The possible states of an UndoRedoManager. Can be combined with logical or
    ''' operator. Eg state could be CanUndo and CanRedo at the same time by having
    ''' value CanRedo or CanUndo = 3 = 2 + 1 = CanRedo + CanUndo.
    ''' </summary>
    Public Enum ManagerStates
        ''' <summary>
        ''' No state defined.
        ''' </summary>
        Undefined = 0
        ''' <summary>
        ''' The buffer can supply a state by a call to Undo(). This status is true if and
        ''' only if CanUndo returns true.
        ''' </summary>
        CanUndo = 1
        ''' <summary>
        ''' The buffer can supply a state by a call to Redo(). This status is true if and
        ''' only if CanRedo returns true.
        ''' </summary>
        CanRedo = 2

        ' *** Note to maintainers: ***
        'this enumeration should be continued with 4,8,16 etc
    End Enum

#End Region

#Region "Properties"
    ''' <summary>
    ''' Checks if this manager will actually hold undoable stages or not.
    ''' True if it can hold any undo levels, false otherwise.
    ''' </summary>
    Public ReadOnly Property Enabled() As Boolean
        Get
            Return mMaxUndoLevels > 0
        End Get
    End Property

    ''' <summary>
    ''' The maximum number of undo levels to keep in this manager.
    ''' </summary>
    Public ReadOnly Property MaxUndoLevels() As Integer
        Get
            Return mMaxUndoLevels
        End Get
    End Property

    Public Shared ReadOnly Property ObfuscatedKey As Byte()
        Get
            If (mObfuscatedKey Is Nothing) Then
                mObfuscatedKey = CreateKey(Guid.NewGuid().ToString())
            End If
            Return mObfuscatedKey
        End Get
    End Property
#End Region

#Region "Methods"

    ''' <summary>
    ''' Adds a state to the buffer, making it available for future undo/redo
    ''' operations. Performing an undo operation will not return the last state
    ''' added, but the penultimate state. Thus the latest state added here should
    ''' mirror the latest state of your object.
    ''' </summary>
    ''' <param name="state">The state to be added.</param>
    Public Sub AddState(ByVal state As clsUndo)

        ' If we can't store anything, might as well not bother...
        If Not Enabled Then Return

        'if we are not at the front of the buffer then we copy the
        'forward states to the front of the buffer in reverse order
        'so that we can "undo the undo".
        Dim i As Integer = mStatesBuffer.Count - 2
        While i >= mCurrentBufferIndex
            ' We want a copy of the state at this point but with the details of the
            ' subsequent action (the one that has been undone), so we can flag it as
            ' reversed.
            Dim copyState As clsUndo = mStatesBuffer.Item(i + 1).Clone()
            copyState.State = mStatesBuffer.Item(i).State
            copyState.StateBytes = mStatesBuffer.Item(i).StateBytes
            copyState.Reverse()

            mStatesBuffer.Add(copyState)
            i -= 1
        End While

        'Limit the length of the undo buffer by removing the oldest item(s) before
        'adding a new one...
        While mStatesBuffer.Count > mMaxUndoLevels
            DeleteTempFile(mStatesBuffer.Item(0))
            mStatesBuffer.RemoveAt(0)
        End While

        WriteState(state)

        mStatesBuffer.Add(state)
        mCurrentBufferIndex = mStatesBuffer.Count - 1

        'raise event telling observers of the change of state
        RaiseEvent StatusChanged(GetCurrentState)
    End Sub

    ''' <summary>
    ''' Determines if a meaningful state will be returned if the Redo() method
    ''' is called.
    ''' </summary>
    ''' <returns>Returns true a meaningful forward state is available; false
    ''' otherwise.</returns>
    Public Function CanRedo() As Boolean
        Return Enabled AndAlso mCurrentBufferIndex < mStatesBuffer.Count - 1
    End Function

    ''' <summary>
    ''' Determines if a meaningful state will be returned if the Undo() method
    ''' is called.
    ''' </summary>
    ''' <returns>Returns true a meaningful backward state is available; false
    ''' otherwise.</returns>
    Public Function CanUndo() As Boolean
        Return Enabled AndAlso mCurrentBufferIndex > 0
    End Function

    ''' <summary>
    ''' Clears all states. Essentially, this is equivalent to discarding this manager
    ''' and instantiating a new one, except this way observers of the StatusChanged
    ''' event will be notified.
    ''' </summary>
    Public Sub ClearStates()

        DeleteTempFiles()

        mCurrentBufferIndex = 0
        mStatesBuffer = New List(Of clsUndo)

        'Raise event telling observers of the change of state
        RaiseEvent StatusChanged(GetCurrentState)
    End Sub

    ''' <summary>
    ''' Get a list of`the states available to be redone in the current buffer.
    ''' </summary>
    ''' <returns>A list of states which can be redone, with the next first.</returns>
    Public Function GetRedoStates() As List(Of clsUndo)
        Dim l As New List(Of clsUndo)
        For i As Integer = mCurrentBufferIndex + 1 To mStatesBuffer.Count - 1
            l.Add(mStatesBuffer.Item(i))
        Next
        Return l
    End Function

    ''' <summary>
    ''' Get a list of the states available to be undone in the current buffer.
    ''' </summary>
    ''' <returns>A list of states which can be undone, with the next first.</returns>
    Public Function GetUndoStates() As List(Of clsUndo)
        Dim l As New List(Of clsUndo)
        For i As Integer = mCurrentBufferIndex To 1 Step -1
            l.Add(mStatesBuffer.Item(i))
        Next
        Return l
    End Function

    ''' <summary>
    ''' Performs the opposite of the undo operation.
    ''' 
    ''' </summary>
    ''' <param name="NewState">Carries back the new state contained in the redo
    ''' point.</param>
    ''' <returns>Returns true if a redo point was found and applied;
    ''' returns false otherwise (ie if no change was made to the parameter 
    ''' NewXML).</returns>
    Public Function Redo(ByRef newState As clsUndo) As Boolean
        If CanRedo() Then
            'Get the new state and put it in byref argument
            mCurrentBufferIndex += 1

            'NewState = Me.mStatesBuffer.Item(Me.mCurrentBufferIndex)
            If Not ReadState(newState) Then Return False

            'raise event telling observers of the change of state
            RaiseEvent StatusChanged(GetCurrentState)

            'return success
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Returns the penultimate state added. Alternatively if a new state was
    ''' added after an undo operation, calling this method will "undo the undo
    ''' operation".
    ''' 
    ''' </summary>
    ''' <param name="NewState">Carries back the new state contained in the undo
    ''' point.</param>
    ''' <returns>Returns true if an undo point was found and applied;
    ''' returns false otherwise (ie if no change was made to the parameter 
    ''' NewState).</returns>
    Public Function Undo(ByRef newState As clsUndo) As Boolean
        If CanUndo() Then
            'Get the new state and put it in byref argument
            mCurrentBufferIndex -= 1

            'NewState = Me.mStatesBuffer.Item(Me.mCurrentBufferIndex)
            If Not ReadState(newState) Then Return False

            'Raise event telling observers of the change of state
            RaiseEvent StatusChanged(GetCurrentState)

            'return success
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Sets the maximum number of undo levels that this manager should support.
    ''' </summary>
    ''' <param name="value">The maximum number of undo levels. Negative numbers
    ''' are treated as 0, ie. don't hold any undo levels. The currently held
    ''' set of undoable states will be reduced until there are no more states
    ''' held than the newly specified maximum.</param>
    ''' <remarks>This is in a method rather than the setter of the <see
    ''' cref="MaxUndoLevels"/> property because it has side-effects (ie. it reduces
    ''' the states buffer so that it holds no more than the new maximum and raises
    ''' an event indicating that the status has changed).</remarks>
    Friend Sub SetMaxUndoLevels(ByVal value As Integer)

        ' Just a quick sanity check
        If value < 0 Then value = 0

        mMaxUndoLevels = value

        ' Reduce the buffer to the new value (if there are currently more states
        ' held in it than the new maximum supports)
        If mStatesBuffer.Count > value Then

            ' The count / value check is done once more than necessary in order
            ' to ensure that the event is raised only if the status has changed
            While mStatesBuffer.Count > value
                DeleteTempFile(mStatesBuffer.Item(0))
                mStatesBuffer.RemoveAt(0)
            End While

            mCurrentBufferIndex = mStatesBuffer.Count - 1

            'raise event telling observers of the change of state
            RaiseEvent StatusChanged(GetCurrentState)

        End If
    End Sub

    Private Sub Compress(ByRef inString As String, ByRef byteArray As Byte())
        byteArray = Encoding.UTF8.GetBytes(inString)
        Using memory = New MemoryStream()
            Using gzip = New Compression.GZipStream(memory, Compression.CompressionLevel.Optimal)
                gzip.Write(byteArray, 0, byteArray.Length)
            End Using
            byteArray = memory.ToArray()
        End Using
    End Sub

    Private Shared Function ComputeSHA256(inString As String) As String
        Using sha256Hash = SHA256.Create()
            Dim bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inString))
            Dim builder = New StringBuilder()
            Dim penultimate = bytes.Length - 1
            For index As Integer = 0 To penultimate
                builder.Append(bytes(index).ToString("X2"))
            Next
            Return builder.ToString()
        End Using
    End Function
    Private Sub Decompress(ByRef byteArray As Byte(), ByRef outString As String)
        Using memory = New MemoryStream(byteArray)
            Using gzip = New Compression.GZipStream(memory, Compression.CompressionMode.Decompress)
                Using reader = New StreamReader(gzip, Encoding.UTF8)
                    outString = reader.ReadToEnd()
                End Using
            End Using
        End Using
    End Sub

    Private Sub DeleteTempFile(ByRef item As clsUndo)
        Try
            Dim path = mTempFolder + "\" + item.State + ".txt"
            If item.StateBytes Is Nothing AndAlso File.Exists(path) Then
                File.Delete(path)
            End If
        Catch
            'Catch suppressed intentionally
        End Try
    End Sub

    Private Sub DeleteTempFiles()
        If mStatesBuffer.Count > 0 Then
            For Each item In mStatesBuffer
                Try
                    DeleteTempFile(item)
                Catch
                    'Catch suppressed intentionally
                End Try
            Next
        End If
    End Sub

    ''' <summary>
    ''' Gets the current state of this manager.
    ''' </summary>
    ''' <returns>As summary.</returns>
    Private Function GetCurrentState() As ManagerStates
        Dim s = ManagerStates.Undefined
        If CanRedo() Then s = s Or ManagerStates.CanRedo
        If CanUndo() Then s = s Or ManagerStates.CanUndo

        Return s
    End Function

    Private Function ReadState(ByRef newState As clsUndo) As Boolean
        Dim copy As clsUndo = mStatesBuffer.Item(mCurrentBufferIndex).Clone()

        If mStatesBuffer.Item(mCurrentBufferIndex).StateBytes IsNot Nothing Then
            Decompress(mStatesBuffer.Item(mCurrentBufferIndex).StateBytes, copy.State)
        Else
            Try
                Dim path = mTempFolder + "\" + copy.State + ".txt"
                copy.State = Decrypt(File.ReadAllText(path, Encoding.UTF8))
            Catch ex As Exception
                Return False
            End Try
        End If

        newState = copy
        Return True
    End Function

    Private Sub WriteState(ByRef state As clsUndo)
        If Not mUseGzip Then
            Dim hash = ComputeSHA256(state.State)
            Dim dateTime = Date.UtcNow
            Dim name = dateTime.Year _
                    & dateTime.Month.ToString("00") _
                    & dateTime.Day.ToString("00") _
                    & dateTime.Second.ToString("00") _
                    & dateTime.Millisecond.ToString("000") _
                    & "_" & hash.Substring(0, 16)
            Dim path = mTempFolder + "\" + name + ".txt"
            Try
                File.WriteAllText(path, Encrypt(state.State), Encoding.UTF8)
                state.State = name
            Catch e As Exception
                mUseGzip = True
            End Try
        End If

        If mUseGzip Then
            Compress(state.State, state.StateBytes)
            state.State = Nothing
        End If
    End Sub

    Private Function Encrypt(textToEncrypt As String) As String
        Dim plainTextBytes = Encoding.UTF8.GetBytes(textToEncrypt)

        Using algorithm = New AesCryptoServiceProvider With {
            .Mode = CipherMode.CBC,
            .Padding = PaddingMode.PKCS7}

            Using encryptor = algorithm.CreateEncryptor(ObfuscatedKey, algorithm.IV)
                Using memoryStream = New MemoryStream()
                    Using cryptoStream = New CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length)
                        cryptoStream.FlushFinalBlock()
                        Return Convert.ToBase64String(algorithm.IV.Concat(memoryStream.ToArray()).ToArray())
                    End Using
                End Using
            End Using
        End Using
    End Function

    Private Function Decrypt(encrypted As String) As String

        Dim encryptedBytes = Convert.FromBase64String(encrypted)

        Dim ivStringBytes = encryptedBytes.Take(IvSize).ToArray()
        Dim cipherTextBytes = encryptedBytes.Skip(IvSize).ToArray()

        Using symmetricKey = New AesCryptoServiceProvider() With {
            .Mode = CipherMode.CBC,
            .Padding = PaddingMode.PKCS7}

            Using decryptor = symmetricKey.CreateDecryptor(ObfuscatedKey, ivStringBytes)
                Using memoryStream = New MemoryStream(cipherTextBytes)
                    Using cryptoStream = New CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)

                        Dim plainTextBytes = New Byte(cipherTextBytes.Length - 1) {}
                        Dim decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length)
                        Return Encoding.UTF8.GetString(plainTextBytes.Take(decryptedByteCount).ToArray())
                    End Using
                End Using
            End Using
        End Using
    End Function

    Private Shared Function CreateKey(ByVal password As String, ByVal Optional keyBytes As Integer = 32) As Byte()
        Const iterations As Integer = 300
        Const saltLength As Integer = 10
        Dim rng As New RNGCryptoServiceProvider
        Dim salt(saltLength - 1) As Byte
        rng.GetBytes(salt)
        Dim keyGenerator = New Rfc2898DeriveBytes(password, salt, iterations)
        Return keyGenerator.GetBytes(keyBytes)
    End Function
#End Region

#Region "IDisposable Support"
    Private mDisposing As Boolean ' To detect redundant calls

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not mDisposing Then
            DeleteTempFiles()
        End If
        mDisposing = True
    End Sub
#End Region

End Class
