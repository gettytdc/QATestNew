Imports System.Runtime.Serialization
Imports BluePrism.Common.Security

''' <summary>
''' Class used in logging property changes on frmCredential.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class CredentialProperty

    ''' <summary>
    ''' Gets or sets a flag indicating if this property is marked for deletion
    ''' </summary>
    <DataMember>
    Public Property IsDeleted As Boolean = False

    ''' <summary>
    ''' Gets or sets the old name associated with this property.
    ''' </summary>
    <DataMember>
    Public Property OldName As String = String.Empty

    ''' <summary>
    ''' Gets or sets the new name associated with this property
    ''' </summary>
    <DataMember>
    Public Property NewName As String = String.Empty

    ''' <summary>
    ''' Gets or sets the password associated with this property. A null value
    ''' indicates that the password is not being set using this property instance.
    ''' </summary>
    <DataMember>
    Public Property Password As SafeString = Nothing

    ''' <summary>
    ''' Gets whether this credential property is new to its owning credential.
    ''' </summary>
    Public ReadOnly Property IsNew As Boolean
        Get
            Return OldName = String.Empty
        End Get
    End Property

    ''' <summary>
    ''' Checks if this property is equal to the given object.
    ''' </summary>
    ''' <param name="obj">The object to test against this credential property for
    ''' equality.</param>
    ''' <returns>True if <paramref name="obj"/> is a non-null credential property
    ''' with the same values as this object.</returns>
    Public Overrides Function Equals(obj As Object) As Boolean
        Dim prop = TryCast(obj, CredentialProperty)
        If prop Is Nothing Then Return False

        Return (
            IsDeleted = prop.IsDeleted AndAlso
            OldName = prop.OldName AndAlso
            NewName = prop.NewName AndAlso
            Object.Equals(Password, prop.Password)
        )
    End Function

    ''' <summary>
    ''' Gets an integer hash based on the value in this property.
    ''' </summary>
    ''' <returns>An integer hash based on the values set in this property. This is
    ''' guaranteed to be the same value as different CredentialProperty instance with
    ''' the same values set in it as this object.</returns>
    Public Overrides Function GetHashCode() As Integer
        Return (
            If(IsDeleted, 0, 1) << 24 Xor
            If(OldName, "").GetHashCode() << 16 Xor
            If(NewName, "").GetHashCode() << 8
        )
    End Function

End Class
