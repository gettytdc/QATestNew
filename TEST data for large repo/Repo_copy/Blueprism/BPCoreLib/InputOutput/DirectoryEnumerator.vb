Imports System.IO

Namespace InputOutput
    ''' <summary>
    ''' Class which can enumerate a directory
    ''' </summary>
    Public Class DirectoryEnumerator : Implements IEnumerator(Of FileSystemInfo)

#Region " Class-scope Declarations "

        ''' <summary>
        ''' Enum to hold the current state of the enumerator
        ''' </summary>
        Private Enum State : Before : During : After : End Enum

#End Region

#Region " Published Events "

        ''' <summary>
        ''' Fired when a directory has been walked - ie. landed on by the enumerator
        ''' </summary>
        Public Event DirectoryWalked As DirectoryWalkerDirEventHandler

        ''' <summary>
        ''' Fired when examining a directory's children caused an exception to be
        ''' thrown
        ''' </summary>
        Public Event DirectoryError As DirectoryWalkerDirErrorEventHandler

        ''' <summary>
        ''' Fired when a file has been walked - ie. landed on by the enumerator
        ''' </summary>
        Public Event FileWalked As DirectoryWalkerFileEventHandler

#End Region

#Region " Member Variables "

        ' The current state of the enumerator
        Private mState As State

        ' The search string to use to determine the files to walk
        Private mPattern As String

        ' The first directory to enumerate; null indicates the enumerator is disposed
        Private mRoot As DirectoryInfo

        ' The current file/dir in the enumerator
        Private mCurrent As FileSystemInfo

        ' The stack of items to work
        Private mStack As Stack(Of FileSystemInfo)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new directory enumerator
        ''' </summary>
        ''' <param name="dir">The directory to enumerate</param>
        Public Sub New(ByVal dir As DirectoryInfo)
            Me.New(dir, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new directory enumerator
        ''' </summary>
        ''' <param name="dir">The directory to enumerate</param>
        ''' <param name="pattern">The filename pattern to apply to determine which
        ''' files should be enumerated; null indicates that all files should be
        ''' enumerated. Note that this doesn't affect the directories enumerated.
        ''' </param>
        Public Sub New(ByVal dir As DirectoryInfo, ByVal pattern As String)
            If dir Is Nothing Then Throw New ArgumentNullException(NameOf(dir))
            mStack = New Stack(Of FileSystemInfo)
            mState = State.Before
            mRoot = dir
            mPattern = pattern
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The current file system object enumerated over
        ''' </summary>
        ''' <exception cref="ObjectDisposedException">If this enumerator has been
        ''' disposed</exception>
        ''' <exception cref="InvalidOperationException">If the enumerator is before
        ''' the first element or after the last element.</exception>
        Public ReadOnly Property Current() As FileSystemInfo _
         Implements IEnumerator(Of FileSystemInfo).Current
            Get
                If mRoot Is Nothing Then _
                 Throw New ObjectDisposedException("DirectoryWalkEnumerator")
                If mState = State.After Then Throw New InvalidOperationException(
                 My.Resources.DirectoryEnumerator_EnumeratorIsAfterLastFileSystemElement)
                If mState = State.Before Then Throw New InvalidOperationException( _
                 "Enumerator is before first file system element")
                Return mCurrent
            End Get
        End Property

        ''' <summary>
        ''' The current file system object enumerated overi
        ''' </summary>
        ''' <exception cref="ObjectDisposedException">If this enumerator has been
        ''' disposed</exception>
        ''' <exception cref="InvalidOperationException">If the enumerator is before
        ''' the first element or after the last element.</exception>
        Private ReadOnly Property NonGenericCurrent() As Object _
         Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Raises the <see cref="FileWalked"/> event
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        Protected Overridable Sub OnFileWalked( _
         ByVal e As DirectoryWalkerFileEventArgs)
            RaiseEvent FileWalked(Me, e)
        End Sub

        ''' <summary>
        ''' Raises the <see cref="DirectoryWalked"/> event
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        Protected Overridable Sub OnDirectoryWalked( _
         ByVal e As DirectoryWalkerDirEventArgs)
            RaiseEvent DirectoryWalked(Me, e)
        End Sub

        ''' <summary>
        ''' Raises the <see cref="DirectoryError"/> event
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        Protected Overridable Sub OnDirectoryError( _
         ByVal e As DirectoryWalkerDirErrorEventArgs)
            RaiseEvent DirectoryError(Me, e)
        End Sub

        ''' <summary>
        ''' Moves to the next file system object in the enumerator.
        ''' </summary>
        ''' <returns>True if the enumerator was safely moved to the next file system
        ''' object; False if there are no more objects to enumerate</returns>
        ''' <exception cref="ObjectDisposedException">If this enumerator has been
        ''' disposed</exception>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext

            If mRoot Is Nothing Then _
             Throw New ObjectDisposedException("DirectoryWalkEnumerator")

            If mState = State.After Then Return False

            ' Start with the root - note that this means that we walk the root
            ' directory itself, not just its contents
            If mState = State.Before Then
                mState = State.During
                mStack.Push(mRoot)
            End If

            If mStack.Count = 0 Then mState = State.After : Return False

            mCurrent = mStack.Pop()

            Dim currDir As DirectoryInfo = TryCast(mCurrent, DirectoryInfo)
            Dim currFile As FileInfo = TryCast(mCurrent, FileInfo)

            If currDir IsNot Nothing Then
                Dim e As New DirectoryWalkerDirEventArgs(currDir)
                OnDirectoryWalked(e)
                If Not e.Descend Then Return True
                ' We push the directories from this directory first, then the files,
                ' so we process files first; effectively walking breadth-first.
                ' Push to the stack in reverse so we handle it in "forward" order
                Dim dirs(0) As DirectoryInfo
                Try
                    dirs = currDir.GetDirectories()
                Catch ex As Exception
                    Dim errEvt As New DirectoryWalkerDirErrorEventArgs(currDir, ex)
                    OnDirectoryError(errEvt)
                    If Not errEvt.Ignore Then Throw
                End Try

                For i As Integer = dirs.Length - 1 To 0 Step -1
                    mStack.Push(dirs(i))
                Next
                Dim files(0) As FileInfo
                Try
                    If mPattern Is Nothing _
                     Then files = currDir.GetFiles() _
                     Else files = currDir.GetFiles(mPattern)
                Catch ex As Exception
                    Dim errEvt As New DirectoryWalkerDirErrorEventArgs(currDir, ex)
                    OnDirectoryError(errEvt)
                    If Not errEvt.Ignore Then Throw
                End Try

                For i As Integer = files.Length - 1 To 0 Step -1
                    mStack.Push(files(i))
                Next


            ElseIf currFile IsNot Nothing Then
                OnFileWalked(New DirectoryWalkerFileEventArgs(currFile))
            End If

            Return True

        End Function

        ''' <summary>
        ''' Resets this enumerator to the beginning.
        ''' </summary>
        ''' <exception cref="ObjectDisposedException">If this enumerator has been
        ''' disposed</exception>
        Public Sub Reset() Implements IEnumerator.Reset
            If mRoot Is Nothing Then _
             Throw New ObjectDisposedException("DirectoryWalkEnumerator")
            mState = State.Before
            mStack.Clear()
        End Sub

        ''' <summary>
        ''' Disposes of this enumerator
        ''' </summary>

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Private mDisposed As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mDisposed Then
                If disposing Then
                    mRoot = Nothing
                End If
            End If
            mDisposed = True
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub

#End Region

    End Class

End Namespace