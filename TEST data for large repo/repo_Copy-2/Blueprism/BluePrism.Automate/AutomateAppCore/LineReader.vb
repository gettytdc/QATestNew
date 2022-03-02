Imports System.IO
Imports BluePrism.BPCoreLib

Friend Module LineReader
    ''' <summary>
    ''' Reads a line from the input buffer and removes the line from the buffer
    ''' </summary>
    ''' <param name="inputBuffer">The buffer from which data will be removed</param>
    ''' <returns>The line that was read</returns>
    Public Function ReadLine(ByRef inputBuffer As String) As String
        If inputBuffer Is Nothing Then Return Nothing

        'Discard any leading line feeds from the buffer
        inputBuffer = inputBuffer.TrimStart(CChar(vbLf))

        'There must be a complete line waiting.
        If Not inputBuffer.ContainsAny(vbCr, vbLf) Then Return Nothing

        Using reader As New StringReader(inputBuffer)
            Dim line = reader.ReadLine

            'Now remove the line from the head of the input buffer.
            inputBuffer = reader.ReadToEnd
            Return line
        End Using
    End Function
End Module