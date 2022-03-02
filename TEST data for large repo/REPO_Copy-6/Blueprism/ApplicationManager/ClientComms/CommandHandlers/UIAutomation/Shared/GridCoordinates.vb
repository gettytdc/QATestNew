Namespace CommandHandlers.UIAutomation.Shared
    
    ''' <summary>
    ''' Represents coordinates within a grid control
    ''' </summary>
    Public Class GridCoordinates
        Implements IEquatable(Of GridCoordinates)
    
        ''' <summary>
        ''' Creates a new instance of GridCoordinates
        ''' </summary>
        ''' <param name="row"></param>
        ''' <param name="column"></param>
        Public Sub New (column As Integer, row As Integer)
            Me.Column = column
            Me.Row = row
        End Sub

        ''' <summary>
        ''' The zero-based index of the row
        ''' </summary>
        Public ReadOnly Property Row As Integer

        ''' <summary>
        ''' The zero-based index of the column
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Column As Integer

        Public Overloads Function Equals(other As GridCoordinates) As Boolean Implements IEquatable(Of GridCoordinates).Equals
            If ReferenceEquals(Nothing, other) Then Return False
            If ReferenceEquals(Me, other) Then Return True
            Return Row = other.Row AndAlso Column = other.Column
        End Function

        Public Overloads Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True
            If obj.GetType IsNot Me.GetType Then Return False
            Return Equals(DirectCast(obj, GridCoordinates))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim hashCode As Integer = Row
            hashCode = CInt((hashCode*397L) Mod Integer.MaxValue) Xor Column
            Return hashCode
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(Row)}: {Row}, {NameOf(Column)}: {Column}"
        End Function
    End Class
End NameSpace