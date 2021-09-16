Imports System.Runtime.Serialization

Namespace Auth
    <DataContract([Namespace]:="bp")>
    <Serializable>
    Public Class ExternalIdentityMapping
        Implements IEquatable(Of ExternalIdentityMapping)

        <DataMember>
        Private mExternalId As String
        <DataMember>
        Private mIdentityProviderName As String
        <DataMember>
        Private mIdentityProviderType As String

        Public Sub New(externalIdentity As String, idProviderName As String, idProviderType As String)
            ExternalId = externalIdentity
            IdentityProviderName = idProviderName
            IdentityProviderType = idProviderType
        End Sub

        Public Overloads Function Equals(other As ExternalIdentityMapping) As Boolean Implements IEquatable(Of ExternalIdentityMapping).Equals
            If ReferenceEquals(Nothing, other) Then Return False
            If ReferenceEquals(Me, other) Then Return True
            Return String.Equals(mExternalId, other.mExternalId, StringComparison.Ordinal) AndAlso
                String.Equals(mIdentityProviderName, other.mIdentityProviderName, StringComparison.OrdinalIgnoreCase) AndAlso
                String.Equals(mIdentityProviderType, other.mIdentityProviderType, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Overloads Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True
            If obj.GetType IsNot Me.GetType Then Return False
            Return Equals(DirectCast(obj, ExternalIdentityMapping))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim hashCode As Integer = 0
            If mExternalId IsNot Nothing Then
                hashCode = CInt((hashCode * 397L) Mod Integer.MaxValue) Xor mExternalId.GetHashCode
            End If
            If mIdentityProviderName IsNot Nothing Then
                hashCode = CInt((hashCode * 397L) Mod Integer.MaxValue) Xor mIdentityProviderName.GetHashCode
            End If
            If mIdentityProviderType IsNot Nothing Then
                hashCode = CInt((hashCode * 397L) Mod Integer.MaxValue) Xor mIdentityProviderType.GetHashCode
            End If
            Return hashCode
        End Function

        Public Shared Operator =(left As ExternalIdentityMapping, right As ExternalIdentityMapping) As Boolean
            Return Equals(left, right)
        End Operator

        Public Shared Operator <>(left As ExternalIdentityMapping, right As ExternalIdentityMapping) As Boolean
            Return Not Equals(left, right)
        End Operator

        Public Property ExternalId As String
            Get
                Return mExternalId
            End Get
            Private Set(value As String)
                mExternalId = value
            End Set
        End Property
        Public Property IdentityProviderName As String
            Get
                Return mIdentityProviderName
            End Get
            Private Set(value As String)
                mIdentityProviderName = value
            End Set
        End Property

        Public Property IdentityProviderType As String
            Get
                Return mIdentityProviderType
            End Get
            Private Set(value As String)
                mIdentityProviderType = value
            End Set
        End Property

    End Class

End Namespace