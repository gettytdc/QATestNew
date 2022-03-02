Imports System.IO
Imports System.Runtime.Serialization
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

<Serializable, DataContract([Namespace]:="bp")>
Public Class clsEncryptionScheme

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The regex that defines the encoded format of the default encryption format.
    ''' Group 1 represents the base64-encoded initialisation vector; Group 2
    ''' represents the base-64 encoded encrypted data.
    ''' </summary>
    Friend Shared ReadOnly DefaultDecrypterRegex As New Regex("(.+):(.+)")

    ' The name of the default encryption scheme created during install
    Public Const DefaultEncryptionSchemeName As String = "Default Encryption Scheme"

    Public Const TempEncryptionSchemeName As String = "Temp Encryption Scheme"

    Public Const UnresolvedKeyName As String = "<Unresolved Key>"

    ' The algorithm of the default encryption scheme created during install
    Public Const DefaultEncryptionAlgorithm As EncryptionAlgorithm = EncryptionAlgorithm.AES256

#End Region

#Region " Member Variables "

    ' The integer ID of the encryption scheme
    <DataMember>
    Private mID As Integer

    ' The name of the scheme
    <DataMember>
    Private mName As String

    ' The algorithm to use for the scheme
    <DataMember>
    Private mAlgorithm As EncryptionAlgorithm

    ' The location of the key
    <DataMember>
    Private mLocation As EncryptionKeyLocation

    ' The key itself
    <DataMember>
    Private mKey As SafeString

    ' Whether this scheme is retired or not
    <DataMember>
    Private mRetired As Boolean

    <DataMember>
    Private mFIPSCompliant As Boolean

#End Region

#Region " Properties "

    ''' <summary>
    ''' Internal database identifier of this encryption scheme (only valid for
    ''' schemes retrieved from the database).
    ''' </summary>
    Public ReadOnly Property ID() As Integer
        Get
            Return mID
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether or not this encryption scheme exists in the database.
    ''' </summary>
    Public ReadOnly Property InDatabase() As Boolean
        Get
            Return mID <> Nothing
        End Get
    End Property

    ''' <summary>
    ''' Friendly name for this encryption scheme (for displaying in drop-down
    ''' lists and audits etc.)
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The algorithm used to perform the encryption.
    ''' </summary>
    Public Property Algorithm() As EncryptionAlgorithm
        Get
            Return mAlgorithm
        End Get
        Set(ByVal value As EncryptionAlgorithm)
            mAlgorithm = value
        End Set
    End Property

    Public ReadOnly Property FIPSCompliant() As Boolean
        Get
            Return mFIPSCompliant
        End Get
    End Property

    ''' <summary>
    ''' The friendly name of the algorithm
    ''' </summary>
    Public ReadOnly Property AlgorithmName() As String
        Get
            Return GetLocalizedFriendlyName(CStr(IIf(HasValidKey, mAlgorithm.GetFriendlyName(), UnresolvedKeyName)))
        End Get
    End Property

    ''' <summary>
    ''' The filename for this key if it is stored externally on the server.
    ''' </summary>
    Public ReadOnly Property ExternalFileName() As String
        Get
            Return String.Format("{0}.bpk", mName)
        End Get
    End Property

    ''' <summary>
    ''' The location of the secret key
    ''' </summary>
    Public Property KeyLocation() As EncryptionKeyLocation
        Get
            Return mLocation
        End Get
        Set(ByVal value As EncryptionKeyLocation)
            mLocation = value
        End Set
    End Property

    ''' <summary>
    ''' The key (represented as a base64 encoded string) used by the algorithm to
    ''' encrypt the data.
    ''' </summary>
    Public Property Key() As SafeString
        Get
            Return mKey
        End Get
        Set(ByVal value As SafeString)
            mKey = value
        End Set
    End Property

    ''' <summary>
    ''' The key represented as a byte array.
    ''' </summary>
    Private ReadOnly Property KeyBytes() As Byte()
        Get
            Try
                Using pinned = mKey.Pin()
                    Return Convert.FromBase64CharArray(pinned.Chars, 0, pinned.Chars.Length)
                End Using
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the scheme is available for selection.
    ''' </summary>
    Public Property IsAvailable() As Boolean
        Get
            Return mRetired
        End Get
        Set(ByVal value As Boolean)
            mRetired = value
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether an encryption key is actually available for this
    ''' scheme and is valid (i.e. could be false if the key has not been defined
    ''' on the app server).
    ''' </summary>
    Public ReadOnly Property HasValidKey() As Boolean
        Get
            If Key Is Nothing OrElse Key.IsEmpty OrElse
             KeyBytes Is Nothing OrElse
             KeyBytes.Length <> EncryptionKeyLengthAttribute.GetKeyLengthFor(mAlgorithm) Then _
              Return False
            Return True
        End Get
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' General purpose constructor
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Constructor for use by the App Server when loading its keys without the
    ''' context of the schemes (which are held in the database).
    ''' </summary>
    ''' <param name="name">Scheme name</param>
    Public Sub New(ByVal name As String)
        Me.New(Nothing, name)
    End Sub

    ''' <summary>
    ''' Constructor for use when retrieving encryption schemes from the database.
    ''' </summary>
    ''' <param name="id">Scheme ID</param>
    ''' <param name="name">Scheme name</param>
    Public Sub New(ByVal id As Integer, ByVal name As String)
        mID = id
        mName = name
    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Gets the localized friendly name for attribute according To the current culture.
    ''' The resources are created from EncryptionAlgorithm.vb, , EncryptionKeyLocation.vb
    ''' plus "Unresolved Key"
    ''' </summary>
    ''' <param name="type">The encryption scheme string</param>
    ''' <returns>The localised encryption scheme string for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyName(type As String) As String
        Dim resxKey As String = Regex.Replace(type, "\b(\w)+\b", Function(m) m.Value(0).ToString().ToUpper() & m.Value.Substring(1))
        resxKey = Regex.Replace(type, "[^a-zA-Z0-9]*", "")
        resxKey = "clsEncryptionScheme_" & resxKey
        Dim res As String = My.Resources.ResourceManager.GetString($"{resxKey}")
        Return CStr(IIf(res Is Nothing, type, res))
    End Function

    ''' <summary>
    ''' Returns a default encryption scheme, hardcoded to "Credentials
    ''' Key" as in previous versions.
    ''' </summary>
    ''' <returns>A default encrypter</returns>
    Public Shared Function DefaultEncrypter() As clsEncryptionScheme
        Dim scheme As New clsEncryptionScheme() With {
            .Name = DefaultEncryptionSchemeName,
            .Algorithm = DefaultEncryptionAlgorithm}
        scheme.GenerateKey()
        Return scheme
    End Function

    ''' <summary>
    ''' Returns a list of algorithms, in order of which should be used. The first
    ''' algorithm in the list is the default and any algorithms that are retired
    ''' will appear at the end of the list
    ''' </summary>
    ''' <returns>Returns an ordered list of all encryption scheme algorithms</returns>
    Public Shared Function GetOrderedAlgorithms() As IEnumerable(Of EncryptionAlgorithm)
        Return New EncryptionAlgorithm() {
                                    EncryptionAlgorithm.AES256,
                                    EncryptionAlgorithm.Rijndael256,
                                    EncryptionAlgorithm.TripleDES
                                }


    End Function

    Public Sub SetFIPSCompatibility(alg As EncryptionAlgorithm)
       mFIPSCompliant = clsFIPSCompliance.CheckForFIPSCompliance(alg)
    End Sub

    ''' <summary>
    ''' Generate a secret key using the random number generator provided by the
    ''' cryptographic service provider.
    ''' </summary>
    Public Sub GenerateKey()
        Using rng As New RNGCryptoServiceProvider()
            Dim size = EncryptionKeyLengthAttribute.GetKeyLengthFor(Algorithm)
            Dim buff As Byte() = New Byte(size - 1) {}
            rng.GetBytes(buff)
            mKey = New SafeString(Convert.ToBase64String(buff))
        End Using
    End Sub

    ''' <summary>
    ''' Encrypts the passed plain text data according to the algorithm associated
    ''' with this scheme, using it's secret key.
    ''' </summary>
    ''' <param name="plainText">The data to encrypt</param>
    ''' <returns>The encrypted data</returns>
    Public Function Encrypt(ByVal plainText As String) As String
        'If plain text is null then return what was passed
        If plainText Is Nothing Then Return plainText

        If Not HasValidKey Then Throw New InvalidValueException(
         My.Resources.clsEncryptionScheme_CouldNotEncryptDataBecauseTheScheme0IsInvalid, Name)

        If Not clsFIPSCompliance.CheckForFIPSCompliance(Me.Algorithm) Then
            Throw New BluePrismException(My.Resources.clsEncryptionScheme_AlgorithmIsNotFIPSCompliant_Encrypt)
        End If

        Dim symAlg As SymmetricAlgorithm = Me.Algorithm.GetProvider()

        Try
            Using ms As New MemoryStream()
                Using encStream As New CryptoStream(ms, symAlg.CreateEncryptor(KeyBytes, symAlg.IV), CryptoStreamMode.Write)
                    Using sw As New StreamWriter(encStream)
                        sw.Write(plainText)
                        sw.Flush()
                        encStream.FlushFinalBlock()

                        ms.Seek(0, SeekOrigin.Begin)

                        Return String.Format("{0}:{1}",
                     Convert.ToBase64String(symAlg.IV),
                     Convert.ToBase64String(ms.ToArray()))
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw New OperationFailedException(
             My.Resources.clsEncryptionScheme_CouldNotEncryptData0, ex.Message)
        Finally
            'Overwrite any sensitive data
            symAlg.Clear()
            symAlg.Dispose()
        End Try

    End Function


    ''' <summary>
    ''' Encrypts the passed SafeString according to the algorithm associated
    ''' with this scheme, using it's secret key.
    ''' </summary>
    ''' <param name="plainText">The data to encrypt</param>
    ''' <returns>The encrypted data</returns>
    Public Function Encrypt(ByVal plainText As SafeString) As String
        'If plain text is null then return what was passed
        If plainText Is Nothing Then Return Nothing

        If Not HasValidKey Then Throw New InvalidValueException(
         My.Resources.clsEncryptionScheme_CouldNotEncryptDataBecauseTheScheme0IsInvalid, Name)

        If Not clsFIPSCompliance.CheckForFIPSCompliance(Me.Algorithm) Then
            Throw New BluePrismException(My.Resources.clsEncryptionScheme_AlgorithmIsNotFIPSCompliant_Encrypt)
        End If

        Dim symAlg As SymmetricAlgorithm = Me.Algorithm.GetProvider()

        Try
            Using pinnedByteArray = plainText.Pin()

                Using ms As New MemoryStream()
                    Using encStream As New CryptoStream(ms, symAlg.CreateEncryptor(KeyBytes, symAlg.IV), CryptoStreamMode.Write)

                        Dim plainTextBytes = pinnedByteArray.GetBytes()
                        encStream.Write(plainTextBytes, 0, plainTextBytes.Length)
                        encStream.Flush()
                        encStream.FlushFinalBlock()
                        ms.Seek(0, SeekOrigin.Begin)

                        Return String.Format("{0}:{1}",
                         Convert.ToBase64String(symAlg.IV),
                         Convert.ToBase64String(ms.ToArray()))

                    End Using
                End Using
            End Using

        Catch ex As Exception
            Throw New OperationFailedException(
             My.Resources.clsEncryptionScheme_CouldNotEncryptData0, ex.Message)
        Finally
            'Overwrite any sensitive data
            symAlg.Clear()
            symAlg.Dispose()
        End Try
    End Function

    ''' <summary>
    ''' Decrypts the passed cipher text data according to the algorithm
    ''' associated with this scheme, using it's secret key.
    ''' </summary>
    ''' <param name="cipherText">The data to decrypt</param>
    ''' <returns>The decrypted data</returns>
    Public Function Decrypt(ByVal cipherText As String) As String

        If cipherText Is Nothing Then Return cipherText

        If Not HasValidKey Then Throw New InvalidValueException(
         My.Resources.clsEncryptionScheme_CouldNotDecryptDataBecauseTheScheme0IsInvalid, Name)

        Dim m As Match = DefaultDecrypterRegex.Match(cipherText)
        If Not m.Success Then Throw New InvalidEncryptedDataException(
         My.Resources.clsEncryptionScheme_DataToDecryptIsNotInTheRequiredFormat)

        Dim iv As Byte() = Convert.FromBase64String(m.Groups(1).Value)
        Dim data As Byte() = Convert.FromBase64String(m.Groups(2).Value)

        If Not clsFIPSCompliance.CheckForFIPSCompliance(Me.Algorithm) Then
            Throw New BluePrismException(My.Resources.clsEncryptionScheme_AlgorithmIsNotFIPSCompliant_Decrypt)
        End If

        Dim symAlg As SymmetricAlgorithm = Me.Algorithm.GetProvider()

        Try
            Using encStream As New CryptoStream(New MemoryStream(data),
             symAlg.CreateDecryptor(KeyBytes, iv), CryptoStreamMode.Read)
                Using sr As New StreamReader(encStream)
                    Return sr.ReadToEnd()
                End Using
            End Using
        Catch ex As Exception
            Throw New OperationFailedException(
             My.Resources.clsEncryptionScheme_CouldNotDecryptData0, ex.Message)
        Finally
            'Overwrite any sensitive data
            symAlg.Clear()
            symAlg.Dispose()
        End Try
    End Function

    ''' <summary>
    ''' Decrypts the passed cipher text data according to the algorithm
    ''' associated with this scheme, using it's secret key.
    ''' </summary>
    ''' <param name="cipherText">The data to decrypt</param>
    ''' <returns>The decrypted data in a SafeString</returns>
    Public Function DecryptToSafeString(ByVal cipherText As String) As SafeString

        If cipherText Is Nothing Then Return New SafeString()

        If Not HasValidKey Then Throw New InvalidValueException(
         My.Resources.clsEncryptionScheme_CouldNotDecryptDataBecauseTheScheme0IsInvalid, Name)

        Dim m As Match = DefaultDecrypterRegex.Match(cipherText)
        If Not m.Success Then Throw New InvalidEncryptedDataException(
         My.Resources.clsEncryptionScheme_DataToDecryptIsNotInTheRequiredFormat)

        Dim iv As Byte() = Convert.FromBase64String(m.Groups(1).Value)
        Dim data As Byte() = Convert.FromBase64String(m.Groups(2).Value)

        If Not clsFIPSCompliance.CheckForFIPSCompliance(Me.Algorithm) Then
            Throw New BluePrismException(My.Resources.clsEncryptionScheme_AlgorithmIsNotFIPSCompliant_Decrypt)
        End If

        Dim symAlg As SymmetricAlgorithm = Me.Algorithm.GetProvider()

        Try
            Using encStream As New CryptoStream(New MemoryStream(data),
             symAlg.CreateDecryptor(KeyBytes, iv), CryptoStreamMode.Read)

                Dim safeString = New SafeString()
                Dim decoder = Encoding.UTF8.GetDecoder()
                Dim chars(1) As Char
                Dim bytes = New Byte(0) {}
                Try

                    While True
                        Dim val = encStream.ReadByte()
                        If val = -1 Then Exit While

                        bytes(0) = CByte(val)
                        Dim charCount = decoder.GetChars(bytes, 0, 1, chars, 0)

                        For i As Integer = 0 To charCount - 1
                            Dim nextChar = chars(i)
                            safeString.AppendChar(nextChar)
                        Next
                    End While

                Finally
                    decoder.Reset()
                    Array.Clear(bytes, 0, bytes.Length)
                    Array.Clear(chars, 0, chars.Length)
                End Try

                Return safeString
            End Using
        Catch ex As Exception
            Throw New OperationFailedException(
             My.Resources.clsEncryptionScheme_CouldNotDecryptData0, ex.Message)
        Finally
            'Overwrite any sensitive data
            symAlg.Clear()
            symAlg.Dispose()
        End Try
    End Function

    ''' <summary>
    ''' Returns the key formatted for storing in an external file on the server.
    ''' </summary>
    Public Function ToExternalFileFormat() As String
        Dim obfuscatedKey As New SafeString(Key, New EncryptingObfuscator)
        'The format has a forth extra 'column' (see FromExternalFileFormat) which
        'distinguishes this output from the legacy formats we no longer write.
        Return String.Format("{0}:{1}", CStr(Algorithm), obfuscatedKey.Encoded)
    End Function

    ''' <summary>
    ''' Extracts the Algorithm and Key from the external file formatted string.
    ''' This function detects whether the key is stored in one of the legacy
    ''' formats and uses the appropriate decoding method.
    ''' </summary>
    Public Sub FromExternalFileFormat(contents As String)
        Dim parts As String() = contents.Split(":"c)
        Dim skey As New SafeString()
        If parts.Count = 2 Then
            'Legacy format {Name}:{Key}
            skey = LegacyCipherDecrypter.Instance.Decrypt(parts(1))
        ElseIf parts.Count = 3 Then
            'Legacy format {Name}:{ObfuscatorType}:{Key}
            skey = SafeString.Decode(String.Format("{0}:{1}", parts(1), parts(2)))
        Else
            'Current format {Name}:{ObfuscatorType}:{EncryptionIV}:{Key}
            skey = SafeString.Decode(String.Format("{0}:{1}:{2}", parts(1), parts(2), parts(3)))
        End If
        Key = skey
        Algorithm = CType(parts(0), EncryptionAlgorithm)
    End Sub

#End Region

End Class
