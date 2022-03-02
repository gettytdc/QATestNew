Imports System.IO

Public Interface IStreamWriterFactory
    Function Create(path As String) As StreamWriter
End Interface
