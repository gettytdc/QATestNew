Imports System.IO

Public Class StreamWriterFactory
    Implements IStreamWriterFactory

    Public Function Create(path As String) As StreamWriter Implements IStreamWriterFactory.Create
        Return New StreamWriter(path)
    End Function
End Class
