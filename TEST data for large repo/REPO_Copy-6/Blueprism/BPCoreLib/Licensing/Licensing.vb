Imports System.Security.Cryptography
Imports BluePrism.Server.Domain.Models
Imports Newtonsoft.Json

''' <summary>
''' Module used for global licensing functions and properties.
''' </summary>
Public Module Licensing

    ''' <summary>
    ''' The message to give to users when publishing a process is disallowed,
    ''' because the license does not permit any more.
    ''' </summary>
    Public ReadOnly Property MaxPublishedProcessesLimitReached As String
        Get
            Return My.Resources.Licensing_TheMaximumNumberOfPublishedProcessesPermittedByTheCurrentLicenseWouldBeExceeded
        End Get
    End Property

    ''' <summary>
    ''' The message to give to users when pending a session is disallowed,
    ''' because the license does not permit any more.
    ''' </summary>
    Public ReadOnly Property MaxResourcesLimitReachedMessage As String
        Get
            Return My.Resources.Licensing_TheMaximumNumberOfResourcePCsPermittedByTheCurrentLicenseWouldBeExceeded
        End Get
    End Property

    ''' <summary>
    ''' The message displayed if the limit of the number of pending sessions would be
    ''' exceeded by an operation.
    ''' </summary>
    Public ReadOnly Property SessionLimitReachedMessage As String
        Get
            Return My.Resources.Licensing_TheMaximumNumberOfConcurrentSessionsPermittedByTheCurrentLicenseWouldBeExceeded
        End Get
    End Property

    ''' <summary>
    ''' Gets a generic message to the user, suitable for when
    ''' they have requsted an action from the gui which is disallowed
    ''' by the license.
    ''' </summary>
    ''' <param name="extraInfo">Specific information about why the operation
    ''' is disallowed</param>
    ''' <remarks>See also ShowperationDisallowedMessage, which shows the
    ''' same message to the user. </remarks>
    Public Function GetOperationDisallowedMessage(Optional ByVal extraInfo As String = "") As String
        Dim stem As String =
            My.Resources.Licensing_TheRequestedOperationIsNotPermittedBecauseTheLicenseCurrentlyInUseDoesNotPermit

        Dim msg As String = stem & If(Not String.IsNullOrWhiteSpace(extraInfo), $" {extraInfo}", "")

        Return msg
    End Function

    ''' <summary>
    ''' instance of a license, accessible to all currently running assemblies.
    ''' </summary>
    ''' <returns>Return value is never null; if no license is set, then the default
    ''' license (which is of LicenseType None) is returned.</returns>
    Public ReadOnly Property License() As LicenseInfo
        Get
            Return mLicense
        End Get
    End Property
    Private mLicense As LicenseInfo = LicenseInfo.DefaultLicense

    Public Function CanAddLicense(key As KeyInfo, keys As ICollection(Of KeyInfo)) As Boolean
        key.Verify()
        Return DoesNotOverlap(key, keys)
    End Function

    Public Function DoesNotOverlap(key As KeyInfo, keys As ICollection(Of KeyInfo)) As Boolean

        If keys.Any(Function(k) k.SignatureValue = key.SignatureValue) Then
            Throw New AlreadyExistsException(My.Resources.Licensing_TheGivenKeyIsAlreadyInstalledInThisEnvironment)
        End If

        If keys.Any(Function(k) k.Standalone) OrElse key.Standalone Then
            Dim overlapping = keys.Where(Function(k) (key.StartDate >= k.StartDate AndAlso key.StartDate <= k.ExpiryDate) OrElse (key.ExpiryDate >= k.StartDate AndAlso key.ExpiryDate <= k.ExpiryDate) OrElse (key.StartDate <= k.StartDate AndAlso key.ExpiryDate >= k.ExpiryDate)).ToList()
            If overlapping.Count > 0 Then
                Throw New LicenseOverlapException(String.Format(My.Resources.LicenseOverlapsWithExistingLicense, overlapping.FirstOrDefault().LicenseOwner))
            End If
        Else
            ' We're not allowed to install both NHS and Enterprise licences
            If (key.LicenseType = LicenseTypes.NHS AndAlso keys.Any(Function(k) k.LicenseType = LicenseTypes.Enterprise)) OrElse (key.LicenseType = LicenseTypes.Enterprise AndAlso keys.Any(Function(k) k.LicenseType = LicenseTypes.NHS)) Then
                If keys.Any(Function(k) k.LicenseType <> key.LicenseType) Then
                    Throw New InvalidTypeException(
                        My.Resources.Licensing_YouCannotCombineAn0LicenseWithAnyOtherTypeOfLicense,
                        key.LicenseType)
                End If

            End If
        End If

        Return True
    End Function

    ''' <summary>
    ''' The public key that licenses are signed with.
    ''' </summary>
    Friend ReadOnly Property LicensePublicKey As RSACryptoServiceProvider
        Get
            Dim key = <RSAKeyValue>
                          <Modulus>l7TgyqtC8ZK2VaoSS5EipImBraRi3aDNfgNEpWlr3Kphj6yWI7xb6aPhfW0Vg0IShb2nEuJLN1hWfDvg9QLL4f8ITbFJjGci2zMkbhF+a6RsevbsPwIkn0cVXFdRiROPAgHTFGDxGegRy4Cl/Klxd7d+Ckwb+s9R5W4w81lJKjDlNF2koE+X6WNAjKxD5E/9/zoRL7L/kXgRYG/m5+OGn+RS5Ro0wIN11gUdkiMqY7r88PjdCMDCHoaltZf4Mt/tdrgEHfWI8lPufddzx00EDvoCPI8Ac8GDwKhca7nei7SK694IUkTbq+0lRV9UJV5ha8FhSLx7lKTXBY2qDOti0Q==</Modulus>
                          <Exponent>AQAB</Exponent>
                      </RSAKeyValue>

            Dim cryptoProvider As New RSACryptoServiceProvider()
            cryptoProvider.FromXmlString(key.ToString)
            Return cryptoProvider
        End Get
    End Property

    Private ReadOnly Property ActivationPublicKey As RSACryptoServiceProvider
        Get
            Dim keyXML = <RSAKeyValue>
                             <Modulus>nANgm7x+ZResdbP/gFtBcovLUODpODSHDrjJtB7uZl6tkmGPj7F1VCKkptbWwOVqc8EUSsnTTIwYJSC6DrqlhVvw+9+R145pOsS9pZKM67/Ig1MaKYZE6AWCeWwTApx44SGNipsa4pB68XhsSbFFoUctyHevfVfv3L/m4wTxLnfJW3ZGSBJJRq338UjlOowOe4tBMaNWjIXdUJzRexHXqD6p32QgP9g8WddDafE4FUhywvJsaQdzXfatyoCt5spCTr8R2Fo7uZmdBFhsnUfnXrXFkxBR4CSppzOOZdRS3cf3P+Yj3vl0kQRfMMY2RF1l9owGd0MCqbTzyIhR78TQotUaG41X5rSiu6DXgM730GmxKhTuLx+cw2A9mKDKFBJuPrnIv4hTBCeU6vwzTkgTdxbdQfeznl9sIgpe9wXVPMr+ZMDPVsWYl9bMT5Udy3QaieZ7xC1++BVVcxQkpYsTuTceB8ZReAFvqKfS+llip8sgZVUub9G5pS6ctuCQj8gfRP0NiPC3jq0KJndO4YLz5v5Bg+X0dYkRv2jB3s+QNVZ6QTm6nkmfIX+j2wmTE1MI88WG1WKtYE5Puk0XKq/iVke9wyo/jcRXr4KNHuYRaacZHa2VzOHFd00bvCLjEAzMoiw7gK0RGb4pU06RkNzGfQw5hGFBmoDc3s9E2yVUFWVayOtEIbBBhI4hwr1j5ca9LAOZvVGFoVylNg0BpG/0RvbPKsvv6WBWTcFNfYorSH8zUrQvunuL4Pd3y7aBVKvzOWYF+nXhQlNGR4ktx49sLcSEz8arfbWcpWXjHlA5YZA7LBsXakfduKdtP7nQTX9vnkHqUZZqvYglqEtUOd3zT3hzIju+K1FETQKioToBQcAXHHLHtGGKp4pHtn6NZ9ieojg3AH8l2ONjgEprpUurNl9sHyCUAO7kuFH5428omvlJaBJehszxgpw2djUtlfQw/8ScLpy+h94PCtpJTcaQN3uqoop1bv5PLdc5FI29Bny8to7vPsFqgyzSy7/K/ie9gbV7FcrjS1DMx6riAE+n75XEicsNT6gMg2BlUDviPkhRqtzIQ0fi5KvgWU/y37i9Nk/TQUwvWaw+RkvqgL6U/qrnwMcgeVFyHHWQvOwKT/jX5gw+/pT50saoCzsUETzqmAI4mjDvIsmVGSKMrjy1+AJ5vEjQMbKdPsiFQuGupyZ27U8w+A2MKahS2CKfNtIVsL8TJnz1RrxU3C08YYu7R3sL4Wr3ExLVbgCmXiwuH3ItZQ819Bg+kAq+6pmBepJaIqj8zcoShXWu+qVdWj6bmai+Ihqu/MyrLgyQUPzb3WGAyUPP+oTGdIW+0vAiUV0Mm9oPOangRm4qHcHSHxuHIQ==</Modulus>
                             <Exponent>AQAB</Exponent>
                         </RSAKeyValue>

            Dim pubkey As New RSACryptoServiceProvider()
            pubkey.FromXmlString(keyXML.ToString)
            Return pubkey
        End Get
    End Property

    Public Function GetLicenseActivationJSONContent(base64String As String) As LicenseActivationJSONContent

        If String.IsNullOrWhiteSpace(base64String) Then
            Throw New ArgumentNullException(NameOf(base64String))
        End If

        Try
            Dim decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64String))

            Dim licenseResponse = JsonConvert.DeserializeObject(Of LicenseActivationResponse)(decodedString)

            Dim dataToVerify = Encoding.UTF8.GetBytes(licenseResponse.data)
            Dim signatureArray = Convert.FromBase64String(licenseResponse.sig64)

            Dim valid = ActivationPublicKey.VerifyData(dataToVerify,
                                                       signatureArray,
                                                       HashAlgorithmName.SHA1,
                                                       RSASignaturePadding.Pkcs1)

            If valid Then Return JsonConvert.DeserializeObject(Of LicenseActivationJSONContent)(licenseResponse.data)

            Return Nothing
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Set the license key(s).
    ''' </summary>
    ''' <param name="keys">The keys to set within the current runtime. To restore the
    ''' default licence effectively removing all other license, see
    ''' <see cref="RestoreDefaultLicense"/> although at the moment passing Nothing or
    ''' an empty list will have the same effect..</param>
    ''' <remarks>In the event of failure, the existing licenses remain in force.
    ''' </remarks>
    Public Sub SetLicenseKeys(keys As List(Of KeyInfo))
        mLicense = LicenseInfo.FromLicenseKeys(keys)
    End Sub

    ''' <summary>
    ''' Restores the default license within the current runtime.
    ''' </summary>
    Public Sub RestoreDefaultLicense()
        SetLicenseKeys(Nothing)
    End Sub

    Public Function EncryptLicenseActivationRequest(request As LicenseActivationRequest, lengthErrorMessage As String) As Byte()
        Dim key = ActivationPublicKey
        Dim dataToEncrypt = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request))
        CheckMaxDataLengthForKeysizeUsingOAEP(dataToEncrypt.Length, key.KeySize, lengthErrorMessage)
        Return key.Encrypt(dataToEncrypt, True).ToArray() 'NOTE decrypt needs OAEP True
    End Function

    Public Sub CheckMaxDataLengthForKeysizeUsingOAEP(dataLength As Integer, keySize As Integer, lengthErrorMessage As String)
        Dim maxDataLength As Integer = CType(keySize / 8, Integer) - 42 'max data length for keysize with OAEP padding
        If dataLength > maxDataLength Then _
            Throw New BluePrismException(lengthErrorMessage)
    End Sub

End Module
