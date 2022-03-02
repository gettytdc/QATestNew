Imports BluePrism.BPCoreLib.Collections
Imports System.IO

Namespace InputOutput

    ''' <summary>
    ''' Class which can 'walk' a directory, raising events for each file and
    ''' directory discovered in the walk.
    ''' </summary>
    Public Class DirectoryWalker : Implements IEnumerable(Of FileSystemInfo)

#Region " Published Events "

        ''' <summary>
        ''' Raised when a directory has been walked - ie. has been landed on in the walk
        ''' of the directory structure.
        ''' </summary>
        Public Event DirectoryWalked As DirectoryWalkerDirEventHandler

        ''' <summary>
        ''' Fired when examining a directory's children caused an exception to be thrown
        ''' </summary>
        Public Event DirectoryError As DirectoryWalkerDirErrorEventHandler

        ''' <summary>
        ''' Raised when a file has been walked - ie. has been landed on in the walk of
        ''' the directory structure.
        ''' </summary>
        Public Event FileWalked As DirectoryWalkerFileEventHandler

#End Region

#Region " Member Variables "

        ' The base directory to walk
        Private mDir As DirectoryInfo

        ' The filter to apply to files/directories
        Private mPattern As String

        ' User defined data associated with this walker
        Private mTag As Object

        ' The set of exception types to ei
        Private mExceptionTypesToIgnore As IBPSet(Of Type)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new directory walker
        ''' </summary>
        ''' <param name="fullPath">The full path to the directory whose structure should
        ''' be walked</param>
        ''' <exception cref="ArgumentNullException">If the given directory info is
        ''' null.</exception>
        Public Sub New(ByVal fullPath As String)
            Me.New(New DirectoryInfo(fullPath), Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new directory walker
        ''' </summary>
        ''' <param name="fullPath">The full path to the directory whose structure should
        ''' be walked</param>
        ''' <param name="pattern">The search string to apply to determine which files
        ''' should be walked; null indicates that all files should be walked. Note that
        ''' this doesn't affect the directories walked.</param>
        ''' <exception cref="ArgumentNullException">If the given directory info is
        ''' null.</exception>
        Public Sub New(ByVal fullPath As String, ByVal pattern As String)
            Me.New(New DirectoryInfo(fullPath), pattern)
        End Sub

        ''' <summary>
        ''' Creates a new directory walker
        ''' </summary>
        ''' <param name="dir">The directory whose structure should be walked</param>
        ''' <exception cref="ArgumentNullException">If the given directory info is
        ''' null.</exception>
        Public Sub New(ByVal dir As DirectoryInfo)
            Me.New(dir, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new directory walker
        ''' </summary>
        ''' <param name="dir">The directory whose structure should be walked</param>
        ''' <param name="pattern">The search string to apply to determine which files
        ''' should be walked; null indicates that all files should be walked. Note that
        ''' this doesn't affect the directories walked.</param>
        ''' <exception cref="ArgumentNullException">If the given directory info is
        ''' null.</exception>
        Public Sub New(ByVal dir As DirectoryInfo, ByVal pattern As String)
            If dir Is Nothing Then Throw New ArgumentNullException(NameOf(dir))
            mDir = dir
            mPattern = pattern
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The directory whose structure should be walked
        ''' </summary>
        ''' <exception cref="ArgumentNullException">If an attempt is made to set the
        ''' root directory with a null value</exception>
        Public Property RootDir() As DirectoryInfo
            Get
                Return mDir
            End Get
            Set(ByVal value As DirectoryInfo)
                If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
                mDir = value
            End Set
        End Property

        ''' <summary>
        ''' The search string to apply to determine which files should be walked; null
        ''' indicates that all files should be walked. Note that this doesn't affect the
        ''' directories walked.
        ''' </summary>
        ''' <remarks>Note that changing this pattern will not affect any currently
        ''' executing enumerators created by this walker - ie. they will continue to
        ''' use the pattern set when they were created.</remarks>
        Public Property FilePattern() As String
            Get
                Return mPattern
            End Get
            Set(ByVal value As String)
                mPattern = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a user-defined object associated with this directory walker
        ''' </summary>
        Public Property Tag() As Object
            Get
                Return mTag
            End Get
            Set(ByVal value As Object)
                mTag = value
            End Set
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
        ''' Walks the directory, raising events for all files and directories walked
        ''' over
        ''' </summary>
        Public Sub Walk()
            ' All we need to do is walk - the events take care of the rest
            Using e As IEnumerator(Of FileSystemInfo) = GetEnumerator(True)
                While e.MoveNext() : End While
            End Using
        End Sub

        ''' <summary>
        ''' Gets an enumerator over the base directory set in this walker, firing the
        ''' events defined in this walker as specified.
        ''' </summary>
        ''' <param name="events">True to fire the events defined in this walker for
        ''' all elements enumerated over; False to just enumerate the elements,
        ''' discarding any events before they are bubbled up</param>
        ''' <returns>An enumerator over the directory structure at the base directory
        ''' set in this walker</returns>
        Private Function GetEnumerator(ByVal events As Boolean) _
         As IEnumerator(Of FileSystemInfo)
            Dim de As New DirectoryEnumerator(mDir, mPattern)
            If events Then
                AddHandler de.DirectoryWalked, AddressOf HandleDirWalked
                AddHandler de.DirectoryError, AddressOf HandleDirError
                AddHandler de.FileWalked, AddressOf HandleFileWalked
            End If
            Return de
        End Function

        ''' <summary>
        ''' Gets an enumerator over the base directory set in this walker. The walker
        ''' events will not be fired by traversing this enumerator
        ''' </summary>
        ''' <returns>A non-event-firing enumerator over the specified directory
        ''' structure</returns>
        Private Function GetEnumerator() As IEnumerator(Of FileSystemInfo) _
         Implements IEnumerable(Of FileSystemInfo).GetEnumerator
            Return GetEnumerator(False)
        End Function

        ''' <summary>
        ''' Gets an enumerator over the base directory set in this walker. The walker
        ''' events will not be fired by traversing this enumerator
        ''' </summary>
        ''' <returns>A non-event-firing enumerator over the specified directory
        ''' structure</returns>
        Private Function NonGenericGetEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Handles a directory being walked by an event-registered enumerator
        ''' created by this instance.
        ''' </summary>
        Private Sub HandleDirWalked( _
         ByVal sender As Object, ByVal e As DirectoryWalkerDirEventArgs)
            OnDirectoryWalked(e)
        End Sub

        ''' <summary>
        ''' Handles a directory being walked by an event-registered enumerator
        ''' created by this instance.
        ''' </summary>
        Private Sub HandleDirError( _
         ByVal sender As Object, ByVal e As DirectoryWalkerDirErrorEventArgs)
            OnDirectoryError(e)
        End Sub

        ''' <summary>
        ''' Handles a file being walked by an event-registered enumerator created by
        ''' this instance.
        ''' </summary>
        Private Sub HandleFileWalked( _
         ByVal sender As Object, ByVal e As DirectoryWalkerFileEventArgs)
            OnFileWalked(e)
        End Sub

#End Region

    End Class

End Namespace