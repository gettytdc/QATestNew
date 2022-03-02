Imports System.Security.Cryptography
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports Mono.Security.Cryptography
Imports System.IdentityModel.Tokens.Jwt
Imports System.Runtime.InteropServices
Imports System.Security.Claims
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.TemplateProcessing
Imports BluePrism.Common.Security
Imports BluePrism.Core.Utility
Imports Microsoft.IdentityModel.Tokens

Namespace WebApis.AccessTokens
    ''' <summary>
    ''' Creates a signed jwt from the specified jwt configuration and credential, 
    ''' which can be used to request an access token to authenticate a webapi
    ''' </summary>
    Public Class JwtBuilder : Implements IJwtBuilder

        Private mJwtConfiguration As JwtConfiguration

        Private mPrivateKey As SafeString

        Private mCredential As ICredential

        Private mClock As ISystemClock

        Private mActionParams As Dictionary(Of String, clsProcessValue)


        ''' <summary>
        ''' Creates a new instance of the <see cref="JwtBuilder"/> class
        ''' </summary>
        ''' <param name="clock">Injected instance of <see cref="ISystemClock"/></param>
        Sub New(clock As ISystemClock)
            mClock = clock
        End Sub

        ''' <inheritdoc />
        Public Function BuildJwt(config As JwtConfiguration, credential As ICredential,
                                 actionParameters As Dictionary(Of String, clsProcessValue)) As String _
            Implements IJwtBuilder.BuildJwt

            mJwtConfiguration = config
            mCredential = credential
            mPrivateKey = credential.Password
            mActionParams = actionParameters
            Return CreateSignedJwt()
        End Function

        Private Function CreateSignedJwt() As String
            Dim rsa As RSA = DecodePrivateKey(mPrivateKey)

            Dim signingCredentials =
                New SigningCredentials(New RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)

            Dim header = New JwtHeader(signingCredentials)

            Dim payload = New JwtPayload(mCredential.Username,
                                         mJwtConfiguration.Audience.ToString,
                                         GetClaims(mJwtConfiguration, mActionParams),
                                         Nothing,
                                         GetExpiryDate(mJwtConfiguration.JwtExpiry),
                                         mClock.UtcNow.UtcDateTime)

            Dim token = New JwtSecurityToken(header, payload)
            Dim handler = New JwtSecurityTokenHandler()

            Return handler.WriteToken(token)
        End Function

        Private Shared Function DecodePrivateKey(privateKey As SafeString) As RSA
            Dim privateKeyTrimmed = TrimPrivateKey(privateKey)
            Using p As New PinnedSecureString(privateKeyTrimmed)
                Dim bytes = Convert.FromBase64CharArray(p.Chars, 0, p.Chars.Length)
                Dim privateKeyInfo = New PKCS8.PrivateKeyInfo(bytes)
                Return PKCS8.PrivateKeyInfo.DecodeRSA(privateKeyInfo.PrivateKey)
            End Using
        End Function

        ''' <summary>
        ''' Prepares a private key for use by removing the prefix, suffix 
        ''' and incompatible whitespace characters which are present in its raw format.
        ''' </summary>
        ''' <param name="privateKey">The private key to be trimmed as a Safestring</param>
        ''' <returns>A safestring representing the base 64 encoded private key with 
        ''' the prefix, suffix and incompatible whitespace characters removed.</returns>
        Public Shared Function TrimPrivateKey(privateKey As SafeString) As SafeString
            Const keyPrefix = "-----BEGIN PRIVATE KEY-----"
            Const keySuffix = "-----END PRIVATE KEY-----"
            Dim prefixChars As Char() = keyPrefix.ToCharArray
            Dim suffixChars As Char() = keySuffix.ToCharArray
            Dim prefixLength = prefixChars.Length
            Dim suffixLength = suffixChars.Length

            ' The array that will contain the key once the prefix, suffix and 
            ' whitespace have been removed
            Dim keyCharsPrepared As Char() = Nothing
            Dim _charsPreparedPin As GCHandle

            Try
                Using pinnedKey As New PinnedSecureString(privateKey)
                    Dim keyChars = pinnedKey.Chars

                    ' Prepare the chars for checking and trimming by removing any instances 
                    ' of the "\n" newline character as these are not recognised as whitespace in vb.

                    ' We are providing the stripping of this specific char because 
                    ' Google supply downloadable JSON files containing private keys 
                    ' as part of their Service Accounts.
                    ' Therefore this \n new line character is likely to be added when  
                    ' users copy and paste the private key, including line breaks, 
                    ' from the JSON file.
                    ' All other whitespace characters are deemed less likely to occur 
                    ' here so instead we throw a detailed exception allowing the user 
                    ' to strip out any incompatible chars themselves.
                    keyCharsPrepared = New Char(keyChars.Length) {}
                    _charsPreparedPin = GCHandle.Alloc(keyCharsPrepared, GCHandleType.Pinned)

                    Dim countNonNull = 0
                    For i = 0 To keyChars.Length - 1
                        If keyChars(i) = "\" Then
                            If i < keyChars.Length - 1 AndAlso keyChars(i + 1) = "n" Then
                                i = i + 1 'ignore it and skip one due to the extra char
                            Else
                                Dim ch = If(i = keyChars.Length - 1, Nothing, keyChars(i + 1))
                                Throw New ArgumentException(
                                    $"Unexpected escape character sequence \{ch} found at position {i} in private key. Only \n is supported.",
                                    NameOf(privateKey))
                            End If
                        Else
                            keyCharsPrepared.SetValue(keyChars(i), countNonNull)
                            countNonNull = countNonNull + 1
                        End If
                    Next
                End Using

                Dim firstCharIndex As Integer = 0
                Dim lastCharIndex As Integer = 0

                ' Check prefix is present and find index of the 1st char of the key itself
                For i = 0 To keyCharsPrepared.Length - 1

                    Dim c = keyCharsPrepared(i)
                    If c = vbNullChar OrElse Char.IsWhiteSpace(c) Then Continue For

                    ' check each character of the prefix is present
                    For j = i To i + prefixLength - 1
                        If Not keyCharsPrepared(j) = prefixChars(j - i) Then
                            Throw New ArgumentException(
                                    $"PKCS8 data must be contained within '{keyPrefix}' and '{keySuffix}'.",
                                                               NameOf(privateKey))
                        End If
                    Next

                    'i is the location of the first char of the prefix
                    firstCharIndex = i + prefixLength
                    Exit For
                Next

                ' Check suffix is present and find index of the last char of the key itself
                For i = keyCharsPrepared.Length - 1 To 0 Step -1
                    Dim c = keyCharsPrepared(i)
                    If c = vbNullChar OrElse Char.IsWhiteSpace(c) Then Continue For

                    ' check each character of the suffix is present
                    For j = i To i - (suffixLength - 1) Step -1
                        If Not keyCharsPrepared(j) = suffixChars(suffixLength - (i - j) - 1) Then
                            Throw New ArgumentException(
                                        $"PKCS8 data must be contained within '{keyPrefix}' and '{keySuffix}'.",
                                        NameOf(privateKey))
                        End If
                    Next
                    'i is the location of the last char of the suffix
                    lastCharIndex = i - suffixLength
                    Exit For
                Next

                ' Now collect the actual key chars
                Dim result = New SafeString()
                For i = firstCharIndex To lastCharIndex
                    result.AppendChar(keyCharsPrepared(i))
                Next

                Return result

            Finally
                If keyCharsPrepared IsNot Nothing Then _
                    Array.Clear(keyCharsPrepared, 0, keyCharsPrepared.Length)

                If _charsPreparedPin.IsAllocated Then _charsPreparedPin.Free()
            End Try

        End Function

        Public Function GetClaims(config As JwtConfiguration,
                                  params As Dictionary(Of String, clsProcessValue)) As IEnumerable(Of Claim)
            Dim claims = New List(Of Claim)
            If Not String.IsNullOrWhiteSpace(config.Scope) Then claims.Add(New Claim("scope", config.Scope))

            If Not String.IsNullOrWhiteSpace(config.Subject) Then
                Dim p = ParameterInterpolator.ProcessTemplate(config.Subject, params)
                claims.Add(New Claim("sub", p))
            End If
            Return claims
        End Function

        Public Function GetExpiryDate(expiry As Integer) As Date?
            Return mClock.UtcNow.UtcDateTime.AddSeconds(expiry)
        End Function
    End Class
End Namespace
