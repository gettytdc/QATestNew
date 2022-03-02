Imports System.IO

Namespace InputOutput

    Public Delegate Sub DirectoryWalkerFileEventHandler( _
      ByVal sender As Object, ByVal e As DirectoryWalkerFileEventArgs)

    Public Class DirectoryWalkerFileEventArgs
        Inherits DirectoryWalkerEventArgs(Of FileInfo)

        Public Sub New(ByVal f As FileInfo)
            MyBase.New(f)
        End Sub

    End Class

End Namespace