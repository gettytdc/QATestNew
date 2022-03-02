Imports System.IO

Namespace InputOutput

    Public Delegate Sub DirectoryWalkerDirEventHandler( _
      ByVal sender As Object, ByVal e As DirectoryWalkerDirEventArgs)

    Public Class DirectoryWalkerDirEventArgs
        Inherits DirectoryWalkerEventArgs(Of DirectoryInfo)

        Private mDescend As Boolean

        Public Sub New(ByVal dir As DirectoryInfo)
            MyBase.New(dir)
            mDescend = True
        End Sub

        Public Property Descend() As Boolean
            Get
                Return mDescend
            End Get
            Set(ByVal value As Boolean)
                mDescend = value
            End Set
        End Property

    End Class

End Namespace
