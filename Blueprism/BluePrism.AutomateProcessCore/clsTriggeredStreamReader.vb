''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsTriggeredStreamReader
''' 
''' <summary>
''' Provides an event based stream reader that can be used to update a progress
''' bar while the stream is being read.
''' </summary>
Public Class clsTriggeredStreamReader
    Inherits IO.StreamReader

    ''' <summary>
    ''' The event that is fired on every block/buffered read of the stream
    ''' </summary>
    ''' <param name="index">This index refers to the number of bytes that have
    ''' been read in the stream</param>
    Public Event Trigger(ByVal index As Long)

    ''' <summary>
    ''' Private store used to keep track of how many bytes have been read.
    ''' </summary>
    Private bytesRead As Long

    ''' <summary>
    ''' default constructor, simply to allow access to the underlying streamreaders 
    ''' default constructor.
    ''' </summary>
    ''' <param name="stream">The undelying stream that the streamreader will read.</param>
    Public Sub New(ByVal stream As IO.Stream)
        MyBase.New(stream)
        bytesRead = 0
    End Sub

    ''' <summary>
    ''' Overload the buffered read method of the streamreader, to raise events every
    ''' time a buffered read method is called.
    ''' </summary>
    ''' <param name="buffer">a char array to use as a buffer</param>
    ''' <param name="index">the index for the buffer</param>
    ''' <param name="count">the maximum number of bytes to read</param>
    ''' <returns></returns>
    Public Overloads Overrides Function Read(ByVal buffer() As Char, ByVal index As Integer, ByVal count As Integer) As Integer
        Dim i As Integer = MyBase.Read(buffer, index, count)
        bytesRead += i
        RaiseEvent Trigger(bytesRead)
        Return i
    End Function
End Class
