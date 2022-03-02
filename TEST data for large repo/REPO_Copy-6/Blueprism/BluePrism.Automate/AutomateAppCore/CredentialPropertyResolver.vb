
Imports BluePrism.Common.Security

''' <summary>
''' Class to resolve property changes against a set of existing properties.
''' </summary>
Public Class CredentialPropertyResolver

    ''' <summary>
    ''' Gets or sets the existing properties currently set against a credential.
    ''' </summary>
    Public Property ExistingProperties As IDictionary(Of String, SafeString)

    ''' <summary>
    ''' Gets or sets the property changes to make when resolving the properties.
    ''' 
    ''' Note that any properties not found in this enumerable will be treated as
    ''' deleted properties.
    ''' 
    ''' If the <see cref="CredentialProperty.Password">Password</see> of a
    ''' CredentialProperty is null, it is treated as 'unchanged', otherwise the
    ''' new value in the password is used in the resolved property.
    ''' </summary>
    Public Property PropertyChanges As IEnumerable(Of CredentialProperty)

    ''' <summary>
    ''' Resolves the property changes against the existing properties, returning
    ''' the dictionary of new properties which should be set against a credential
    ''' </summary>
    ''' <returns>A dictionary containing the new set of properties when the property
    ''' changes were applied to the existing properties. A few rules: <list>
    ''' <item>If a property did not exist in <see cref="PropertyChanges"/>, it will
    ''' not be included in the result of this function. ie. it will be treated the
    ''' same as if it existed with the 'IsDeleted' flag set to True.</item>
    ''' <item>This will never return null - if the resolution ends up with no
    ''' properties, this will return an empty dictionary</item>
    ''' </list></returns>
    Public Function Resolve() As IDictionary(Of String, SafeString)
        Dim existing =
            If(ExistingProperties, New Dictionary(Of String, SafeString))

        If PropertyChanges Is Nothing Then Return existing
        Dim newProperties As New Dictionary(Of String, SafeString)


        For Each prop In PropertyChanges
            Dim oldValue As SafeString = Nothing

            ' Do nothing for deleted properties
            If prop.IsDeleted Then Continue For

            ' If a property exists with the old name...
            If prop.OldName IsNot Nothing AndAlso
             existing.TryGetValue(prop.OldName, oldValue) Then
                ' Ensure it's moved to the new properties with the new name, and
                ' use the new password or the old value if no new one is given
                newProperties(prop.NewName) = If(prop.Password, oldValue)

            Else ' ie. if it's a new property
                newProperties(prop.NewName) = prop.Password

            End If
        Next

        Return newProperties

    End Function
End Class
