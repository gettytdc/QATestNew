Imports System.IO

Namespace InputOutput

    ''' <summary>
    ''' Delegate describing a method which can handle a DirectoryWalkerDirError event
    ''' </summary>
    ''' <param name="sender">The source of the event, typically a DirectoryWalker or
    ''' DirectoryEnumerator</param>
    ''' <param name="e">The args detailing the event</param>
    Public Delegate Sub DirectoryWalkerDirErrorEventHandler( _
      ByVal sender As Object, ByVal e As DirectoryWalkerDirErrorEventArgs)

    ''' <summary>
    ''' Event arguments detailing an error while walking a directory and allowing a
    ''' listener to indicate that the error should be ignored. Note that this will
    ''' only occur after a 'current' directory has been chosen - ie. while looking at
    ''' its child directories or files. If a current directory cannot be chosen, the
    ''' enumerator has no option but to terminate since it cannot return the
    ''' 'Current' element.
    ''' </summary>
    Public Class DirectoryWalkerDirErrorEventArgs
        Inherits DirectoryWalkerEventArgs(Of DirectoryInfo)

        ' The exception thrown when walking the directory
        Private mException As Exception

        ' True to ignore the error, False to continue with the enumeration
        Private mIgnore As Boolean

        ''' <summary>
        ''' Creates a new event args object
        ''' </summary>
        ''' <param name="dir">The directory on which the error occurred</param>
        ''' <param name="ex">The exception thrown while walking the directory</param>
        Public Sub New(ByVal dir As DirectoryInfo, ByVal ex As Exception)
            MyBase.New(dir)
            mException = ex
        End Sub

        ''' <summary>
        ''' Gets or sets a flag indicating that the error should be ignored.
        ''' </summary>
        Public Property Ignore() As Boolean
            Get
                Return mIgnore
            End Get
            Set(ByVal value As Boolean)
                mIgnore = value
            End Set
        End Property

        ''' <summary>
        ''' The exception which caused the event to be fired
        ''' </summary>
        Public ReadOnly Property Exception() As Exception
            Get
                Return mException
            End Get
        End Property

    End Class

End Namespace