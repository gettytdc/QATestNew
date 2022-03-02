Imports System.Threading

Public Class HistoryTable : Implements IDisposable
    Private mTable As DataTable
    Private mLock As Object
    Public ReadOnly Property Table As DataTable
        Get
            If mTable Is Nothing Then mTable = New DataTable()
            Return mTable
        End Get
    End Property
    Public Sub New(tab As DataTable, lock As Object)
        mTable = tab
        mLock = lock
        Monitor.Enter(lock)
    End Sub
    Protected Overrides Sub Finalize()
        Monitor.Exit(mLock)
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Monitor.Exit(mLock)
        GC.SuppressFinalize(Me) ' Make sure the monitor is not exited twice
    End Sub
End Class

