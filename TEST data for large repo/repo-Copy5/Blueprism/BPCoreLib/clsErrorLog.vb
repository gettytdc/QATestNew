
Public Class clsError
    Private mMessage As String
    Private mTag As Object

    Public Sub New(ByVal msg As String)
        Me.New(DirectCast(Nothing, Object), msg)
    End Sub

    Public Sub New(ByVal msg As String, ByVal ParamArray args() As Object)
        Me.New(DirectCast(Nothing, Object), msg, args)
    End Sub

    Public Sub New(ByVal tag As Object, ByVal msg As String, ByVal ParamArray args() As Object)
        mTag = tag
        mMessage = String.Format(msg, args)
    End Sub

    Public ReadOnly Property Message() As String
        Get
            If mMessage Is Nothing Then Return ""
            Return mMessage
        End Get
    End Property

    Public Property Tag() As Object
        Get
            Return mTag
        End Get
        Set(ByVal value As Object)
            mTag = value
        End Set
    End Property

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim err As clsError = TryCast(obj, clsError)
        If err Is Nothing Then Return False
        Return Object.Equals(Me.Message, err.Message) AndAlso Object.Equals(mTag, err.mTag)
    End Function

    Public Overrides Function ToString() As String
        Return My.Resources.ErrorLog_Error_Prefix & Message
    End Function

    Public Overrides Function GetHashCode() As Integer
        Dim hash As Integer = Message.GetHashCode()
        If mTag IsNot Nothing Then hash = hash Xor mTag.GetHashCode()
        Return hash
    End Function
End Class

Public Class clsErrorLog : Implements ICollection(Of clsError)

    Private mErrors As ICollection(Of clsError)

    Public Sub New()
        mErrors = New List(Of clsError)
    End Sub

    Public Function Add(ByVal tag As Object, ByVal msg As String, ByVal ParamArray args() As Object) _
     As clsError
        Dim err As New clsError(tag, msg, args)
        Add(err)
        Return err
    End Function

    Public Function Add(ByVal msg As String, ByVal ParamArray args() As Object) As clsError
        Return Add(DirectCast(Nothing, Object), msg, args)
    End Function

    Public ReadOnly Property IsEmpty() As Boolean
        Get
            Return (mErrors.Count = 0)
        End Get
    End Property

#Region " ICollection implementation "

    Public Sub Add(ByVal item As clsError) Implements ICollection(Of clsError).Add
        mErrors.Add(item)
    End Sub

    Public Sub Clear() Implements ICollection(Of clsError).Clear
        mErrors.Clear()
    End Sub

    Public Function Contains(ByVal item As clsError) As Boolean Implements ICollection(Of clsError).Contains
        Return mErrors.Contains(item)
    End Function

    Public Sub CopyTo(ByVal array() As clsError, ByVal arrayIndex As Integer) Implements ICollection(Of clsError).CopyTo
        mErrors.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count() As Integer Implements ICollection(Of clsError).Count
        Get
            Return mErrors.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of clsError).IsReadOnly
        Get
            Return mErrors.IsReadOnly
        End Get
    End Property

    Public Function Remove(ByVal item As clsError) As Boolean Implements ICollection(Of clsError).Remove
        mErrors.Remove(item)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of clsError) Implements IEnumerable(Of clsError).GetEnumerator
        Return mErrors.GetEnumerator()
    End Function

    Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

#End Region

End Class
