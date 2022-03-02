
Imports System.IO

''' <summary>
''' Exception thrown when a file already existed and it couldn't be created as a
''' result.
''' </summary>
''' <remarks>Note that this exception is <em>not</em> serializable - a FileInfo loses
''' its meaning on a different machine, so it makes no sense to serialize it.
''' </remarks>
<Serializable>
Public Class FileAlreadyExistsException : Inherits AlreadyExistsException

    ' The file that cannot be created since it already exists.
    Private mFile As FileInfo

    ''' <summary>
    ''' Creates a new exception indicating that the file represented by the given
    ''' FileInfo object already exists and cannot be created.
    ''' </summary>
    ''' <param name="file">The info regarding the pre-existing file.</param>
    Public Sub New(ByVal file As FileInfo)
        MyBase.New(My.Resources.FileAlreadyExistsException_File0AlreadyExistsCannotCreateIt, file.FullName)
        mFile = file
    End Sub

    ''' <summary>
    ''' The FileInfo object representing the file which already exists and cannot
    ''' be created
    ''' </summary>
    Public ReadOnly Property File() As FileInfo
        Get
            Return mFile
        End Get
    End Property

End Class
