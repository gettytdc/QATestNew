Imports System.Text.RegularExpressions

Namespace Resources 
    Public Class DigitalWorkerName
        Private Const DigitalWorkerNamePattern As String = "^[\p{L}|\p{N}|_-]*$"

        Public Sub New(fullName As String)
            If Not IsValid(fullName) Then
                Throw New ArgumentException("Digital worker name is invalid", NameOf(fullName))
            End If

            Me.FullName = fullName
        End Sub

        Public ReadOnly Property FullName As String

        'Validation includes the design for RabbitMQ queue name limitation to 255 characters, allowing padding for digital worker control queue names
        Public Shared Function IsValid(fullName As String) As Boolean
            Return Not String.IsNullOrEmpty(fullName) AndAlso Regex.IsMatch(fullName, DigitalWorkerNamePattern) AndAlso fullName.Length <= 245
        End Function

        Protected Overloads Function Equals(other As DigitalWorkerName) As Boolean
            Return Equals(FullName, other.FullName)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If obj Is Nothing Then
                Return False
            End If

            If ReferenceEquals(Me, obj) Then
                Return True
            End If

            Return obj.GetType() Is [GetType]() AndAlso Equals(CType(obj, DigitalWorkerName))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return If(Not Equals(FullName, Nothing), FullName.GetHashCode(), 0)
        End Function
    End Class
End Namespace

