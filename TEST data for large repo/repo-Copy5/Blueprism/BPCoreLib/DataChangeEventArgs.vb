''' Project  : BPCoreLib
''' Class    : DataChangeEventArgs
''' <summary>
''' Event arguments class which holds the data regarding a DataChanged event.
''' </summary>
Public Class DataChangeEventArgs
    Inherits EventArgs

    ' The name of the specific datum being changed, or the name of the data change
    ' type (eg. "ItemAdded")
    Private mName As String

    ' The old value
    Private mOld As Object

    ' The new value.
    Private mNew As Object

    ''' <summary>
    ''' Creates a new empty event arguments object.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new event arguments object with the provided values.
    ''' </summary>
    ''' <param name="name">The name of the data change.</param>
    ''' <param name="oldValue">The old value of the data item.</param>
    ''' <param name="newValue">The new value of the data item.</param>
    ''' <remarks>It's common to fire a DataChanged event when an item is being
    ''' added or removed from an underlying collection. Generally, an 'added'
    ''' data change will have no old value and the added value in the
    ''' 'new value' property, whereas a 'removed' data change will contain the
    ''' value being removed in the 'old value' property, and have no 'new value'.
    ''' </remarks>
    Public Sub New(ByVal name As String, ByVal oldValue As Object, ByVal newValue As Object)
        mName = name
        mOld = oldValue
        mNew = newValue
    End Sub

    ''' <summary>
    ''' The name of the data change which is taking place
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    ''' <summary>
    ''' The old value of the data which is being changed.
    ''' </summary>
    Public ReadOnly Property OldValue() As Object
        Get
            Return mOld
        End Get
    End Property

    ''' <summary>
    ''' The new value of the data which is being changed.
    ''' </summary>
    Public ReadOnly Property NewValue() As Object
        Get
            Return mNew
        End Get
    End Property

    ''' <summary>
    ''' Checks if this data change is as a result of a call to the 'Mark()' method
    ''' in the DataMonitor class.
    ''' If it is the result of a 'Mark()' then no further data will be available
    ''' (data name / old / new values)
    ''' </summary>
    Public ReadOnly Property IsFromMark() As Boolean
        Get
            Return (mName Is Nothing AndAlso mOld Is Nothing AndAlso mNew Is Nothing)
        End Get
    End Property

End Class