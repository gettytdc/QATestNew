Imports System.Data.SqlClient
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security
Imports BluePrism.Common.Security.Exceptions
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports System.Security.Cryptography

Partial Public Class clsServer

    'The cached key store
    Private mKeyStore As New List(Of clsEncryptionScheme)
    'The secret keys residing on the App Server
    Private Shared serverKeys As New Dictionary(Of String, clsEncryptionScheme)

#Region " Secured  methods "
    ''' <summary>
    ''' Encrypts plain text data using the specified encryption scheme name.
    ''' </summary>
    ''' <param name="schemeName">The encryption scheme name</param>
    ''' <param name="plainText">The data to encrypt</param>
    ''' <returns>The encrypted data</returns>
    <SecuredMethod()>
    Public Function Encrypt(ByVal schemeName As String, ByVal plainText As String) As String Implements IServer.Encrypt
        CheckPermissions()
        If schemeName Is Nothing Then Return plainText
        Return GetEncryptionSchemeByName(schemeName).Encrypt(plainText)
    End Function

    ''' <summary>
    ''' Decrypts cipher text data using the specified encryption scheme name.
    ''' </summary>
    ''' <param name="schemeName">The encryption scheme name</param>
    ''' <param name="cipherText">The data to encrypt</param>
    ''' <returns>The encrypted data</returns>
    <SecuredMethod()>
    Public Function Decrypt(ByVal schemeName As String, ByVal cipherText As String) As String Implements IServer.Decrypt
        CheckPermissions()
        If schemeName Is Nothing Then Return cipherText
        Return GetEncryptionSchemeByName(schemeName).Decrypt(cipherText)
    End Function

    ''' <summary>
    ''' Returns the encryption scheme with the specified name from the KeyStore cache
    ''' loading it from the database if it doesn't already exist there.
    ''' </summary>
    ''' <param name="name">The scheme name</param>
    ''' <returns>The encryption scheme</returns>
    <SecuredMethod()>
    Public Function GetEncryptionSchemeByName(ByVal name As String) As clsEncryptionScheme Implements IServer.GetEncryptionSchemeByName
        CheckPermissions()
        'Attempt to find scheme in KeyStore cache
        For Each scheme As clsEncryptionScheme In mKeyStore
            If scheme.Name = name Then Return scheme
        Next
        'Otherwise load it from the database
        Return GetEncryptionSchemeFromDB(Nothing, name, True)
    End Function

    ''' <summary>
    ''' Returns the encryption scheme with the specified name, without the encryption key.
    ''' </summary>
    ''' <param name="name">Name of encryption scheme</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <SecuredMethod(Auth.Permission.SystemManager.Security.ViewEncryptionSchemes)>
    Public Function GetEncryptionSchemeExcludingKey(name As String) As clsEncryptionScheme _
    Implements IServer.GetEncryptionSchemeExcludingKey
        CheckPermissions()
        Return GetEncryptionSchemeFromDB(Nothing, name, False)
    End Function


    ''' <summary>
    ''' Returns true if the encryption scheme exists in the database.
    ''' </summary>
    ''' <param name="name">Name of the encryption scheme</param>
    ''' <returns>True if exists</returns>
    ''' <remarks></remarks>
    <SecuredMethod()>
    Public Function HasEncryptionScheme(name As String) As Boolean _
        Implements IServer.HasEncryptionScheme
        CheckPermissions()

        Using con = GetConnection()

            Dim cmd As New SqlCommand("select COUNT(*) from BPAKeyStore where name=@name")
            cmd.Parameters.AddWithValue("@name", name)

            Dim numRows = CInt(con.ExecuteReturnScalar(cmd))
            Return numRows >= 1
        End Using
    End Function

    ''' <summary>
    ''' Returns true if the encryption scheme passes fips compliance policy
    ''' </summary>
    ''' <param name="name">Name of the encryption scheme</param>
    ''' <returns>True if fips policy is off or scheme is compliant</returns>
    ''' <remarks></remarks>
    <SecuredMethod()>
    Public Function CheckSchemeForFIPSCompliance(name As String) As Boolean _
        Implements IServer.CheckSchemeForFIPSCompliance
        CheckPermissions()
        Dim scheme = GetEncryptionSchemeFromDB(Nothing, name, True)
        Return clsFIPSCompliance.CheckForFIPSCompliance(scheme.Algorithm)
    End Function

    ''' <summary>
    ''' Returns a collection of encryption schemes excluding the key, for use in 
    ''' populating the UI for read-only views
    ''' </summary>
    ''' <returns>Collection of encryption schemes</returns>
    ''' <remarks>Care should be taken if using the schemes returned by this function as they will 
    ''' appear as Invalid and the property AlgorithmName will be set to UnresolvedKey. 
    ''' The method GetAlgorithmName() in this class can be used on a scheme with no key</remarks>
    <SecuredMethod()>
    Public Function GetEncryptionSchemesExcludingKey() As ICollection(Of clsEncryptionScheme) _
    Implements IServer.GetEncryptionSchemesExcludingKey
        CheckPermissions()
        Using con = GetConnection()
            Return GetEncryptionSchemes(con, False)
        End Using
    End Function

    ''' <summary>
    ''' Returns a collection of encryption schemes
    ''' </summary>
    ''' <returns>Collection of encryption schemes</returns>
    Private Function GetEncryptionSchemes() As ICollection(Of clsEncryptionScheme) _
    Implements IServer.GetEncryptionSchemes
        Using con = GetConnection()
            Return GetEncryptionSchemes(con, True)
        End Using
    End Function

    ''' <summary>
    ''' Saves the passed encryption scheme definition to the database.
    ''' </summary>
    ''' <param name="scheme">The scheme to store</param>
    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Function StoreEncryptionScheme(ByVal scheme As clsEncryptionScheme) As Boolean Implements IServer.StoreEncryptionScheme
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As SqlCommand
            If Not scheme.InDatabase Then
                cmd = New SqlCommand("insert into BPAKeyStore" &
                 " (name, location, isavailable, method, encryptkey)" &
                 " values (@name, @location, @available, @method, @key)")
            Else
                cmd = New SqlCommand("update BPAKeyStore set" &
                 " name=@name, location=@location, isavailable=@available, method=@method, encryptkey=@key" &
                 " where id=@id")
                cmd.Parameters.AddWithValue("@id", scheme.ID)
            End If
            With cmd.Parameters
                .AddWithValue("@name", scheme.Name)
                .AddWithValue("@location", scheme.KeyLocation)
                .AddWithValue("@available", scheme.IsAvailable)
                If scheme.KeyLocation = EncryptionKeyLocation.Database Then
                    .AddWithValue("@method", scheme.Algorithm)
                    Using pstr = scheme.Key.Pin()
                        .AddWithValue("@key", New String(pstr.Chars))
                    End Using
                Else
                    .AddWithValue("@method", DBNull.Value)
                    .AddWithValue("@key", DBNull.Value)
                End If
            End With
            Try
                con.BeginTransaction()
                con.Execute(cmd)
                If Not scheme.InDatabase Then _
                 AuditRecordKeyStoreEvent(con, KeyStoreEventCode.Create, scheme) Else _
                 AuditRecordKeyStoreEvent(con, KeyStoreEventCode.Modify, scheme)
                con.CommitTransaction()

                ' Remove the scheme we've just saved from the cache. We don't store the updated
                ' scheme object in the cache as it will not be fully populated if the key is stored
                ' on the server and the scheme was loaded without the key data (e.g. via 
                ' GetEncryptionSchemesExcludingKey). The cache will be refreshed (in 
                ' GetEncryptionSchemeByID) next time the full schema data is loaded.
                mKeyStore.RemoveAll(Function(sch) sch.ID = scheme.ID)
            Catch sqlex As SqlException
                If sqlex.Number = DatabaseErrorCode.UniqueConstraintError Then
                    Throw New NameAlreadyExistsException()
                End If
                Throw
            End Try
        End Using
        Return True
    End Function

    ''' <summary>
    ''' Deletes the passed encryption scheme from the database.
    ''' </summary>
    ''' <param name="scheme">The scheme to delete</param>
    <SecuredMethod(Auth.Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Sub DeleteEncryptionScheme(ByVal scheme As clsEncryptionScheme) Implements IServer.DeleteEncryptionScheme
        CheckPermissions()
        Using con = GetConnection()
            'Check if scheme is referenced in configuration
            If EncryptionSchemeInUse(con, scheme.ID) Then
                Throw New InvalidOperationException(String.Format(
                 My.Resources.clsServer_TheEncryptionScheme0CannotBeDeletedBecauseItIsInUse, scheme.Name))
            End If

            'Try to delete the scheme and trap any FK violations
            Try
                Dim cmd As New SqlCommand("delete from BPAKeyStore where id=@id")
                cmd.Parameters.AddWithValue("@id", scheme.ID)
                con.BeginTransaction()
                con.Execute(cmd)
                AuditRecordKeyStoreEvent(con, KeyStoreEventCode.Delete, scheme)
                con.CommitTransaction()
                'remove the scheme from the cache 
                mKeyStore.RemoveAll(Function(sch) sch.ID = scheme.ID)
            Catch sqlex As SqlException
                If sqlex.Number = DatabaseErrorCode.ForeignKeyError Then
                    Throw New InvalidOperationException(String.Format(
                     My.Resources.clsServer_TheEncryptionScheme0CannotBeDeletedBecauseThereIsStillDataEncryptedUsingThisKey, scheme.Name))
                End If
                Throw
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' Some clsEncryptionScheme Objects are returned to the UI deliberately without 
    ''' their key, for security. The Algorithm Name property is set using the encryption 
    ''' key so this method allows this property to be returned from the server to the 
    ''' UI without exposing the key in a public call.
    ''' (The AlgorithmNameProperty for a keyless scheme will always be UnresolvedKey 
    ''' so we must ensure that we check it against the full object)
    ''' </summary>
    ''' <param name="schemeNoKey">The clsEncryptionScheme object which is known to have
    '''  no key info (it may or may not have a valid key)</param>
    ''' <returns>The AlgorithmName Property for the key specified</returns>
    <SecuredMethod()>
    Public Function GetAlgorithmName(ByVal schemeNoKey As clsEncryptionScheme) As String Implements IServer.GetAlgorithmName
        CheckPermissions()
        Dim scheme As clsEncryptionScheme = GetEncryptionSchemeByID(schemeNoKey.ID)
        Return scheme.AlgorithmName
    End Function

    ''' <summary>
    ''' Indicates whether the encryption scheme identified by the passed ID is used
    ''' by either Credential or Work Queue configuration. Note that this is not the
    ''' same as there being data still encrypted with it.
    ''' </summary>
    ''' <param name="id">The ID of the encryption scheme</param>
    ''' <returns>True if the scheme is in use, otherwise False</returns>
    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Function EncryptionSchemeInUse(ByVal id As Integer) As Boolean Implements IServer.EncryptionSchemeInUse
        CheckPermissions()
        Using con = GetConnection()
            Return EncryptionSchemeInUse(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Checks all encryption schemes and returns a list of the names of any that are
    ''' not valid without exposing sensitive data
    ''' </summary>
    ''' <returns>a list of the names of any invalid encryption schemes</returns>
    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Function GetInvalidEncryptionSchemeNames() As List(Of String) Implements IServer.GetInvalidEncryptionSchemeNames
        CheckPermissions()
        Dim schemes As ICollection(Of clsEncryptionScheme) = GetEncryptionSchemes()
        Dim invalidSchemeNames As New List(Of String)
        For Each sc As clsEncryptionScheme In schemes
            If Not sc.HasValidKey Then
                invalidSchemeNames.Add(sc.Name)
            End If
        Next
        Return invalidSchemeNames
    End Function

    ''' <summary>
    ''' Checks all encryption schemes to see if they are FIPS compliant
    ''' when the FIPS GPO is enabled
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If there are any non FIPS compliant encryption schemes</exception>
    <SecuredMethod>
    Public Function DBEncryptionSchemesAreFipsCompliant() As List(Of String) Implements IServer.DBEncryptionSchemesAreFipsCompliant
        CheckPermissions()
        Dim nonFIPSEncryptMessage As New List(Of String)

        If Not CryptoConfig.AllowOnlyFipsAlgorithms Then Return nonFIPSEncryptMessage

        For Each encryptionScheme As clsTableEncryption In GetEncryptionSchemesInDb(GetConnection())
            Dim alg = CType(encryptionScheme.EncryptionId, EncryptionAlgorithm)
            If Not clsFIPSCompliance.CheckForFIPSCompliance(alg) Then
                Dim encryptName = CType(encryptionScheme.EncryptionId, EncryptionAlgorithm)
                Dim errorMessage = String.Format(My.Resources.InvalidEncryptionScheme0InUseBy1,
                                                 encryptName, encryptionScheme.Function)
                nonFIPSEncryptMessage.Add(errorMessage)
            End If
        Next

        return nonFIPSEncryptMessage
    End Function

    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Function GetConfigEncryptMethod() As String Implements IServer.GetConfigEncryptMethod
        CheckPermissions()
        If Options.Instance.SelectedConfigEncryptionMethod = MachineConfig.ConfigEncryptionMethod.OwnCertificate Then _
                Return Options.Instance.Thumbprint
    End Function

    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Sub SetConfigEncryptMethod(ByVal thumbprint As String, ByVal forceConfigEncryptParam As Boolean) Implements IServer.SetConfigEncryptMethod
        CheckPermissions()

        Dim canSave = False
        Dim allowError = False

        Try
            If String.IsNullOrWhiteSpace(thumbprint) Then
                Options.Instance.Thumbprint = String.Empty
                Options.Instance.SelectedConfigEncryptionMethod = MachineConfig.ConfigEncryptionMethod.BuiltIn
                canSave = True
            Else
                Dim certStore = New CertificateStoreService()
                certStore.ValidateThumbprint(thumbprint)

                Dim certificate = certStore.GetCertificate(thumbprint)
                certStore.ValidateCert(certificate, CertificateStoreCheckFlags.PrivateKey)

                Options.Instance.Thumbprint = thumbprint
                Options.Instance.SelectedConfigEncryptionMethod = MachineConfig.ConfigEncryptionMethod.OwnCertificate
                canSave = True
            End If
        Catch cex As CertificateException
            Dim certificateErrorCode = cex.CertificateErrorCode
            Dim exceptionMessage As String

            Select Case certificateErrorCode
                Case CertificateErrorCode.NotFound
                    exceptionMessage = My.Resources.NoCertificateExistsForThisThumbprint
                Case CertificateErrorCode.NotActiveYet
                    exceptionMessage = My.Resources.TheCertificateHasNotBeenActivated
                Case CertificateErrorCode.Expired
                    exceptionMessage = My.Resources.TheCertificateHasExpired
                Case CertificateErrorCode.NotUnique
                    exceptionMessage = My.Resources.MultipleCertificatesExistForThisThumbprint
                Case CertificateErrorCode.Unauthorised
                    exceptionMessage = My.Resources.ThisUserDoesNotHaveAccessToThisCertificate & " : " & cex.InnerException.Message
                Case CertificateErrorCode.PrivateKey
                    exceptionMessage = My.Resources.ThisCertificateDoesNotHaveAPrivateKey
                Case CertificateErrorCode.PrivateKeyNotAccessible
                    exceptionMessage = My.Resources.YouDoNotHaveAccessToThePrivateKeyAssociatedWithThisCertificate
                    If forceConfigEncryptParam Then
                        allowError = True
                        exceptionMessage = My.Resources.WarningYouDoNotHaveAccessToThePrivateKeyAssociatedWithThisCertificate
                    End If
                Case CertificateErrorCode.PartialCertificateChain
                    exceptionMessage = My.Resources.CertificateVerificationChainFailed
                Case CertificateErrorCode.InvalidThumbprintRegex
                    exceptionMessage = My.Resources.TheThumbprintIsNotInTheCorrectFormat
                Case CertificateErrorCode.EncryptDecryptFail
                    exceptionMessage = My.Resources.ThisCertificatePrivateKeyCannotDecrypt
                Case Else
                    exceptionMessage = My.Resources.ConfigEncryptThereIsAnErrorWithTheCertificate
            End Select

            If allowError Then
                Options.Instance.Thumbprint = thumbprint
                Options.Instance.SelectedConfigEncryptionMethod = MachineConfig.ConfigEncryptionMethod.OwnCertificate
                canSave = True
            End If

            Throw New CertificateException(exceptionMessage, certificateErrorCode)
        Catch ex As Exception
            Throw ex
        End Try
        Try
            If canSave Then
                Options.Instance.Save()
            End If
        Catch
            Throw
        End Try
    End Sub   

#End Region

#Region " Private  methods "
    ''' <summary>
    ''' Encrypts plain text data using the specified encryption scheme ID.
    ''' </summary>
    ''' <param name="encryptid">The encryption scheme id</param>
    ''' <param name="plainText">The data to encrypt</param>
    ''' <returns>The encrypted data</returns>
    Private Function Encrypt(ByVal encryptID As Integer, ByVal plainText As String) As String
        If encryptID = 0 Then Return plainText
        Return GetEncryptionSchemeByID(encryptID).Encrypt(plainText)
    End Function

    ''' <summary>
    ''' Encrypts plain text data of a SafeString using the specified encryption scheme ID.
    ''' </summary>
    ''' <param name="encryptid">The encryption scheme id</param>
    ''' <param name="safeString">The data to encrypt</param>
    ''' <returns>The encrypted data</returns>
    Private Function Encrypt(ByVal encryptID As Integer, ByVal safeString As SafeString) As String
        Return GetEncryptionSchemeByID(encryptID).Encrypt(safeString)
    End Function

    ''' <summary>
    ''' Decrypts cipher text data using the specified encryption scheme ID.
    ''' </summary>
    ''' <param name="encryptID">The encryption scheme name</param>
    ''' <param name="cipherText">The data to decrypt</param>
    ''' <returns>The decrypted data</returns>
    Friend Function Decrypt(ByVal encryptID As Integer, ByVal cipherText As String) As String
        If encryptID = 0 Then Return cipherText
        Return GetEncryptionSchemeByID(encryptID).Decrypt(cipherText)
    End Function

    ''' <summary>
    ''' Decrypts cipher text data using the specified encryption scheme ID,
    ''' returning a SafeString.
    ''' </summary>
    ''' <param name="encryptID">The encryption scheme name</param>
    ''' <param name="cipherText">The data to decrypt</param>
    ''' <returns>The decrypted data</returns>
    Private Function DecryptToSafeString(ByVal encryptID As Integer, ByVal cipherText As String) As SafeString
        Return GetEncryptionSchemeByID(encryptID).DecryptToSafeString(cipherText)
    End Function


    ''' <summary>
    ''' Loads the passed encryption scheme (identified either by ID or Name) from the
    ''' database and, if required, attaches it to the relevant secret key held by the
    ''' App Server.
    ''' </summary>
    ''' <param name="id">The scheme ID (if Name not passed)</param>
    ''' <param name="name">The scheme Name (if ID not passed)</param>
    ''' <param name="includeKey">True to include the encryption key and algorithm with 
    ''' the returned scheme</param>
    ''' <returns>The encryption scheme</returns>
    Private Function GetEncryptionSchemeFromDB(ByVal id As Integer, ByVal name As String,
                                               ByVal includeKey As Boolean) As clsEncryptionScheme
        Using con = GetConnection()
            Return GetEncryptionSchemeFromDB(con, id, name, includeKey)
        End Using
    End Function


    Private Function GetEncryptionSchemeFromDB(con As IDatabaseConnection, ByVal id As Integer, ByVal name As String, _
                                                   ByVal includeKey As Boolean) As clsEncryptionScheme
        Dim sb As New StringBuilder("select id, name, location, isavailable, method, encryptkey")
        sb.Append(String.Format(" from BPAKeyStore where {0}=@what", IIf(id <> Nothing, "id", "name")))

        Dim cmd As New SqlCommand(sb.ToString())
        cmd.Parameters.AddWithValue("@what", IIf(id <> Nothing, id, name))

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then
                Throw New NoSuchEncrypterException(name)
            End If

            Dim prov As New ReaderDataProvider(reader)
            id = prov.GetValue("id", 0)
            name = prov.GetString("name")
            Dim scheme As New clsEncryptionScheme(id, name)
            scheme.IsAvailable = prov.GetValue("isavailable", False)
            scheme.KeyLocation = DirectCast(prov.GetValue("location", 0), EncryptionKeyLocation)

            If includeKey Then
                If scheme.KeyLocation = EncryptionKeyLocation.Database Then
                    scheme.Algorithm = DirectCast(prov.GetValue("method", 0), EncryptionAlgorithm)
                    scheme.Key = New SafeString(prov.GetString("encryptkey"))
                ElseIf serverKeys.ContainsKey(name) Then
                    ' Note that serverKeys will only be available when running on BPServer
                    scheme.Algorithm = serverKeys(name).Algorithm
                    scheme.Key = serverKeys(name).Key
                End If
                'Add to key store cache and return (only if we are looking at the full object with key)
                mKeyStore.Add(scheme)
            Else
                ' Need to set the FIPS compliance, but not embed the key in the object.
                If scheme.KeyLocation = EncryptionKeyLocation.Database Then
                    scheme.SetFIPSCompatibility(DirectCast(prov.GetValue("method", 0), EncryptionAlgorithm))
                ElseIf serverKeys.ContainsKey(name) Then
                    scheme.SetFIPSCompatibility(serverKeys(name).Algorithm)
                Else
                    scheme.SetFIPSCompatibility(EncryptionAlgorithm.None)
                End If
            End If

            Return scheme
        End Using
    End Function

    

    ''' <summary>
    ''' Returns a collection of encryption schemes
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="includeKey">Option to include the Encryption Keys in the 
    ''' returned scheme data</param>
    ''' <returns>Collection of encryption schemes</returns>
    Private Function GetEncryptionSchemes(ByVal con As IDatabaseConnection, ByVal includeKey As Boolean) _
        As ICollection(Of clsEncryptionScheme)

        Dim schemes As New List(Of clsEncryptionScheme)
        Dim cmd As New SqlCommand("select id from BPAKeyStore")
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                schemes.Add(GetEncryptionSchemeFromDB(prov.GetValue("id", 0), Nothing, includeKey))
            End While
        End Using
        Return schemes
    End Function

    ''' <summary>
    ''' Returns a collection of encryption schemes
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>Collection of encryption schemes</returns>
    Private Function GetEncryptionSchemesInDb(ByVal con As IDatabaseConnection) _
        As ICollection(Of clsTableEncryption)

        Dim schemes As New List(Of clsTableEncryption)
        Dim cmd As New SqlCommand("select BPAKeyStore.method, TableName from (
                                   select encryptid as EId, 'BPACredentials' as [TableName] from BPACredentials group by encryptid union
                                   select encryptid as EId, 'BPADataPipelineProcessConfig' as [TableName] from BPADataPipelineProcessConfig group by encryptid union
                                   select encryptid as EId, 'BPAScreenshot' as [TableName]  from BPAScreenshot group by encryptid union
                                   select defaultencryptid as EId, 'BPASysConfig' as [TableName] from BPASysConfig group by defaultencryptid    union
                                   select encryptid as EId, 'BPAWorkQueue' as [TableName] from BPAWorkQueue group by encryptid union
                                   select encryptid as EId, 'BPAWorkQueueItem' as [TableName] from BPAWorkQueueItem group by encryptid union
                                   select id as EId, 'BPAKeyStore' as [TableName] from BPAKeyStore group by id 
                                    ) as T join BPAKeyStore on BPAKeyStore.id = t.EId where T.EId is not null")

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                Dim tableName = prov.GetValue("TableName", "")
                schemes.Add(New clsTableEncryption(prov.GetValue("method", 0), tableName))
            End While
        End Using
        Return schemes
    End Function

    ''' <summary>
    ''' Returns the encryption scheme with the specified ID from the KeyStore cache
    ''' loading it from the database if it doesn't already exist there.
    ''' </summary>
    ''' <param name="id">The scheme ID</param>
    ''' <returns>The encryption scheme</returns>
    Private Function GetEncryptionSchemeByID(ByVal id As Integer) As clsEncryptionScheme
        'Attempt to find scheme in KeyStore cache
        For Each scheme As clsEncryptionScheme In mKeyStore
            If scheme.ID = id Then Return scheme
        Next
        'Otherwise load it from the database
        Return GetEncryptionSchemeFromDB(id, Nothing, True)
    End Function


    ''' <summary>
    ''' Indicates whether the encryption scheme identified by the passed ID is used
    ''' by either Credential or Work Queue configuration.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="id">The ID of the encryption scheme</param>
    ''' <returns>True if the scheme is in use, otherwise False</returns>
    Private Function EncryptionSchemeInUse(ByVal con As IDatabaseConnection, ByVal id As Integer) As Boolean
        If GetDefaultEncrypter(con) = id Then Return True
        For Each wq As clsWorkQueue In GetQueuesWithoutStats(con)
            If wq.EncryptionKeyID = id Then Return True
        Next
    End Function
#End Region


End Class
