Imports System.IO

Namespace InputOutput

    Public Class DirectoryWalkerEventArgs(Of T As {FileSystemInfo}) : Inherits EventArgs

        Private mInfo As T

        Protected Sub New(ByVal fsInfo As T)
            mInfo = fsInfo
        End Sub

        Public ReadOnly Property Info() As T
            Get
                Return mInfo
            End Get
        End Property

    End Class

End Namespace