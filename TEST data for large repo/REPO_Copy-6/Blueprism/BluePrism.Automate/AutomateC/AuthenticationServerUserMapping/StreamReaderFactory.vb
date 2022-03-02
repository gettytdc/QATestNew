Imports System.IO

Public Class StreamReaderFactory
    Implements IStreamReaderFactory

    Public Function Create(path As String) As StreamReader Implements IStreamReaderFactory.Create
        Return New StreamReader(path)
    End Function
End Class
