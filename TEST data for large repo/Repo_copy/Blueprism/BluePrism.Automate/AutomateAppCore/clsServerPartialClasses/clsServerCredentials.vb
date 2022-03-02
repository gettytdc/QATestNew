

Imports System.Data.SqlClient

Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Common.Security
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

#Region " Secured Methods "

    ''' <summary>
    ''' Returns a collection of all credential objects, populated with their ID, name
    ''' description, expiry date and invalid flag.
    ''' </summary>
    ''' <returns>The collection of credential objects</returns>
    <SecuredMethod()>
    Public Function GetAllCredentialsInfo() As ICollection(Of clsCredential) Implements IServer.GetAllCredentialsInfo
        CheckPermissions()
        Using con As IDatabaseConnection = GetConnection()
            Return GetAllCredentialsInfo(con, clsCredential.Status.All, False, False)
        End Using
    End Function

    ''' <summary>
    ''' Returns the credential for a given credential ID, including the sensitive data
    ''' logon details, property values etc. (user/process/resource restrictions are 
    ''' not taken into account).
    ''' </summary>
    ''' <param name="id">The credential ID</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    <SecuredMethod(Permission.SystemManager.Security.Credentials)>
    Public Function GetCredentialIncludingLogon(ByVal id As Guid) _
        As clsCredential Implements IServer.GetCredentialIncludingLogon
        CheckPermissions()
        Return GetCredential(id, True, True, True)
    End Function

    ''' <summary>
    ''' Returns the credential for a given credential ID, optionally including the
    ''' logon details, property values etc. (user/process/resource restrictions are 
    ''' not taken into account).
    ''' </summary>
    ''' <param name="id">The credential ID</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    <SecuredMethod()>
    Public Function GetCredentialExcludingLogon(ByVal id As Guid) _
    As clsCredential Implements IServer.GetCredentialExcludingLogon
        CheckPermissions()
        Return GetCredential(id, includePassword:=False, includeUsername:=False, includePropertyValues:=False)
    End Function

    ''' <summary>
    ''' Returns the credential for a given ID for display in the UI, including the 
    ''' username and property names, but excluding the sensitive password and 
    ''' property values.
    ''' </summary>
    ''' <param name="id">The credential ID</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    <SecuredMethod(Permission.SystemManager.Security.Credentials)>
    Public Function GetCredentialForUI(ByVal id As Guid) As clsCredential _
        Implements IServer.GetCredentialForUI

        CheckPermissions()
        Return GetCredential(id, True, False, False)
    End Function

    ''' <summary>
    ''' Returns the ID of the credential for a given name (user/process/resource 
    ''' restrictions are not taken into account).
    ''' </summary>
    ''' <param name="name">The credential name</param>
    ''' <returns>The credential ID, or <see cref="Guid.Empty"/> if no credential with
    ''' the given name was found</returns>
    <SecuredMethod()>
    Public Function GetCredentialID(ByVal name As String) As Guid Implements IServer.GetCredentialID
        CheckPermissions()
        Using con = GetConnection()
            Try
                Return GetCredentialInfo(con, name, False, False).ID
            Catch ex As NoSuchCredentialException
                Return Guid.Empty
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Updates a credential, without any constraint checking.
    ''' </summary>
    ''' <param name="credential">The credential object</param>
    ''' <param name="oldName">The previous name (or the current name it has not been
    ''' changed)</param>
    ''' <exception cref="NameAlreadyExistsException">If the chosen new name is
    ''' already in use on the database by a different credential</exception>
    <SecuredMethod(Permission.SystemManager.Security.Credentials)>
    Public Sub UpdateCredential(
     credential As clsCredential,
     oldName As String,
     properties As ICollection(Of CredentialProperty),
     passwordChanged As Boolean) Implements IServer.UpdateCredential

        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            ' If we're changing the credential name, check it is not already in use
            If credential.Name <> oldName Then
                Try
                    If GetCredentialInfo(con, credential.Name, False, False).ID <> credential.ID Then
                        Throw New NameAlreadyExistsException(
                         My.Resources.clsServer_ACredentialWithName0AlreadyExists, credential.Name)
                    End If
                Catch ex As NoSuchElementException
                    ' Name not in use so can update
                End Try
            End If

            UpdateCredentialInfo(con, credential)
            SaveCredentialAssociatedData(con, credential, properties)
            AuditRecordCredentialsEvent(con, CredentialsEventCode.Modify, oldName, credential, passwordChanged)

            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Creates a new credential
    ''' </summary>
    ''' <param name="credential">The credential object</param>
    ''' <returns>The new credential ID</returns>
    <SecuredMethod(Permission.SystemManager.Security.Credentials)>
    Public Function CreateCredential(ByVal credential As clsCredential) As Guid Implements IServer.CreateCredential
        CheckPermissions()
        Dim credentialID As Guid = Guid.Empty

        Using con = GetConnection()
            con.BeginTransaction()

            Try
                GetCredentialInfo(con, credential.Name, False, False)
                Throw New NameAlreadyExistsException(
                 My.Resources.clsServer_ACredentialWithName0AlreadyExists, credential.Name)
            Catch ex As NoSuchElementException
                ' Name not in use so can create
            End Try

            credentialID = CreateCredential(con, credential)
            Dim passwordChanged As Boolean = credential.Password.Length <> 0
            AuditRecordCredentialsEvent(con, CredentialsEventCode.Create, credential, passwordChanged)

            con.CommitTransaction()
        End Using

        Return credentialID
    End Function

    ''' <summary>
    ''' Deletes a list of credentials
    ''' </summary>
    ''' <param name="credentials">The list of credentials</param>
    <SecuredMethod(Permission.SystemManager.Security.Credentials)>
    Public Sub DeleteCredentials(ByVal credentials As IEnumerable(Of clsCredential)) Implements IServer.DeleteCredentials
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            For Each cred As clsCredential In credentials
                DeleteCredential(con, cred.ID)
                AuditRecordCredentialsEvent(con, CredentialsEventCode.Delete, cred, False)
            Next
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Requests a single credential given the credential name and session ID, and
    ''' using the role of the current logged in user.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The name of the credential to get</param>
    ''' <returns>The credential associated with the given name</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' name is found on the database</exception>
    ''' <exception cref="PermissionException">If the current caller context does not
    ''' have access rights to the specified credential</exception>
    <SecuredMethod()>
    Public Function RequestCredential(
     ByVal sessId As Guid, ByVal name As String) As clsCredential Implements IServer.RequestCredential
        CheckPermissions()
        Using con = GetConnection()
            Return RequestCredential(con, sessId, name)
        End Using
    End Function


    <SecuredMethod()>
    Public Function RequestCredentialForDataGatewayProcess(name As String) As clsCredential Implements IServer.RequestCredentialForDataGatewayProcess
        CheckPermissions()
        Using con = GetConnection()
            Return RequestCredentialForDataGatewayProcess(con, name)
        End Using

    End Function


    Private Function RequestCredentialForDataGatewayProcess(con As IDatabaseConnection, name As String) As clsCredential

        If Not mLoggedInUser.AuthType = AuthMode.System Then
            Throw New BluePrismException(My.Resources.UserNotPermittedToAccessCredential)
        End If

        Dim credential = GetCredential(con, name)
        If credential.Type.Name <> "DataGatewayCredentials" Then
            Throw New BluePrismException(String.Format(My.Resources.NotADataGatewayCredential, name))
        End If

        Return credential

    End Function


    <SecuredMethod()>
    Public Function GetDataGatewayCredentials() As List(Of string) Implements IServer.GetDataGatewayCredentials
        CheckPermissions()
        Using con = GetConnection()
            Return GetDataGatewayCredentials(con)
        End Using
    End Function

    Private Function GetDataGatewayCredentials(con As IDatabaseConnection) As List(Of String)
        Dim command As SqlCommand = New SqlCommand("select name from BPACredentials where credentialType = 'DataGatewayCredentials'")
        Dim reader = con.ExecuteReturnDataReader(command)

        Dim results = New List(Of String)
        While reader.Read()
            results.Add(reader.GetString(0))
        End While

        Return results

    End Function

    ''' <summary>
    ''' Requests that the named credential is reset. The credential be accessible to
    ''' the user/process/resource.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="username">The username</param>
    ''' <param name="password">The password</param>
    ''' <param name="expirydate">The expiry date</param>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    <SecuredMethod()>
    Public Sub RequestCredentialSet(ByVal sessId As Guid, ByVal name As String,
     ByVal username As String, ByVal password As SafeString, ByVal expirydate As Date) Implements IServer.RequestCredentialSet
        CheckPermissions()
        Using con = GetConnection()
            Dim credential As clsCredential = RequestCredential(con, sessId, name)
            credential.Username = username
            credential.Password = password
            credential.ExpiryDate = expirydate
            credential.IsInvalid = False
            UpdateCredentialInfo(con, credential)
        End Using
    End Sub

    ''' <summary>
    ''' Request specified credential is invalidated. The credential be accessible to
    ''' the user/process/resource.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    <SecuredMethod()>
    Public Sub RequestCredentialInvalidated(ByVal sessId As Guid, ByVal name As String) Implements IServer.RequestCredentialInvalidated
        CheckPermissions()
        Using con = GetConnection()
            Dim credential As clsCredential = RequestCredential(con, sessId, name)
            credential.IsInvalid = True
            UpdateCredentialInfo(con, credential)
        End Using
    End Sub

    ''' <summary>
    ''' Request the value of a property for a specific credential. The credential
    ''' be accessible to the user/process/resource.
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="propName">The property name</param>
    ''' <returns>The property value</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    ''' <exception cref="NoSuchElementException">If the credential did not have a
    ''' property with the name specified by <paramref name="propName"/></exception>
    <SecuredMethod()>
    Public Function RequestCredentialProperty(ByVal sessId As Guid,
     ByVal name As String, ByVal propName As String) As SafeString Implements IServer.RequestCredentialProperty
        CheckPermissions()
        Using con = GetConnection()
            Dim credential As clsCredential = RequestCredential(con, sessId, name)
            Dim propValue As SafeString = Nothing

            LoadCredentialProperties(con, propName, credential, True)
            If credential.Properties.TryGetValue(propName, propValue) Then
                Return propValue
            End If

            Throw New NoSuchElementException(
             My.Resources.clsServer_TheCredentialPropertyWithName0CouldNotBeFound, propName)
        End Using
    End Function

    ''' <summary>
    ''' Request that the value of a specified credential property is set to the given 
    ''' value. The credential must be accessible to the user/process/resource. 
    ''' If the specified credential does not have such a property, the property will 
    ''' be created before being set at the required value. 
    ''' </summary>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="propName">The property name</param>
    ''' <param name="propValue">The property value</param>
    ''' ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the current calling context does not
    ''' have access rights to the specified credential</exception>
    <SecuredMethod()>
    Public Sub RequestSetCredentialProperty(
        ByVal sessId As Guid,
        ByVal name As String, ByVal propName As String,
        ByVal propValue As SafeString) Implements IServer.RequestSetCredentialProperty

        CheckPermissions()
        Using con = GetConnection()
            Dim cred As clsCredential = RequestCredential(con, sessId, name)
            SetCredentialProperty(con, cred, propName, propValue)
            AuditRecordCredentialsEvent(con, CredentialsEventCode.Modify, cred, False)
        End Using
    End Sub


    ''' <summary>
    ''' Request that the value of a specified credential property is set to the given 
    ''' value. If the specified credential does not have such a property, the property will 
    ''' be created before being set at the required value. 
    ''' </summary>
    ''' <param name="name">The credential name</param>
    ''' <param name="propName">The property name</param>
    ''' <param name="propValue">The property value</param>
    ''' ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' <paramref name="name">name</paramref> was found in the database.</exception>
    ''' <exception cref="PermissionException">If the logged in user doesn't have the
    ''' correct permissions to call this method</exception>
    <SecuredMethod(Permission.SystemManager.Security.Credentials)>
    Public Sub RequestSetCredentialProperty(
        ByVal name As String, ByVal propName As String,
        ByVal propValue As SafeString) Implements IServer.RequestSetCredentialProperty
        CheckPermissions()
        Using con = GetConnection()
            Dim cred As clsCredential = GetCredential(con, name)
            SetCredentialProperty(con, cred, propName, propValue)
            AuditRecordCredentialsEvent(con, CredentialsEventCode.Modify, cred, False)
        End Using
    End Sub

    ''' <summary>
    ''' Request a list of credentials matching the passed status. Note that the
    ''' actual login details (username/password) are not returned and that user/
    ''' process/resource restrictions are not taken into account.
    ''' </summary>
    ''' <param name="sessId">The ID of the session which is requesting access to the
    ''' credential.</param>
    ''' <param name="status">The status criteria</param>
    ''' <returns>The collection of credentials matching the passed
    ''' status criteria</returns>
    ''' <remarks>This method is used by clsCredentialsBusinessObject</remarks>
    <SecuredMethod()>
    Public Function RequestCredentialsList(ByVal sessId As Guid,
     ByVal status As clsCredential.Status) As ICollection(Of clsCredential) Implements IServer.RequestCredentialsList
        CheckPermissions()
        Using con As IDatabaseConnection = GetConnection()
            Return GetAllCredentialsInfo(con, status, False, False)
        End Using
    End Function

    ''' <summary>
    ''' Updates the default encryption scheme (used for credentials, screen captures)
    ''' </summary>
    ''' <param name="encryptid"></param>
    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Sub SetDefaultEncrypter(encryptId As Integer, name As String) Implements IServer.SetDefaultEncrypter
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("update BPASysConfig set defaultencryptid=@encryptid")
            cmd.Parameters.AddWithValue("@encryptid", encryptId)
            con.BeginTransaction()
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.DefaultEncrypter,
             String.Format(My.Resources.clsServerCredentials_SetDefaultEncrypterComment, encryptId, name))
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Re-encrypts all credentials using the currently configured encryption scheme.
    ''' </summary>
    ''' <returns>Count of credentials updated</returns>
    <SecuredMethod(Permission.SystemManager.Security.ManageEncryptionSchemes)>
    Public Function ReEncryptCredentials() As Integer Implements IServer.ReEncryptCredentials
        CheckPermissions()
        Dim count As Integer = 0
        Using con = GetConnection()
            'Setup command to select credentials (they cannot be un-encrypted, so 
            'encryptid can never be null here)
            Dim cmd As New SqlCommand(
             "select id from BPACredentials" &
             " where encryptid <> (select defaultencryptid from BPASysConfig)")

            'Get list of credential IDs
            Dim ids As New List(Of Guid)
            Using reader = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
                Dim prov As New ReaderDataProvider(reader)
                If Not reader.HasRows() Then Return -1

                While reader.Read()
                    ids.Add(prov.GetValue("id", Guid.Empty))
                End While
            End Using

            'Update credential data in single transaction
            con.BeginTransaction()
            For Each id As Guid In ids
                Dim cred As clsCredential = GetCredentialInfo(con, id, True, True)
                LoadCredentialRights(con, cred)
                LoadCredentialProperties(con, String.Empty, cred, True)
                UpdateCredentialInfo(con, cred)
                SaveCredentialAssociatedData(con, cred)
                count += 1
            Next
            con.CommitTransaction()
        End Using
        Return count
    End Function

    ''' <summary>
    ''' Returns the ID of the current default encryption scheme.
    ''' </summary>
    ''' <returns>The encrypter ID</returns>
    <SecuredMethod()>
    Public Function GetDefaultEncrypter() As Integer Implements IServer.GetDefaultEncrypter
        CheckPermissions()
        Using con = GetConnection()
            Return GetDefaultEncrypter(con)
        End Using
    End Function

#End Region

#Region " Unsecured Methods "

    ''' <summary>
    ''' Indicates whether or not a valid credential key exists
    ''' </summary>
    ''' <returns>True if credential key is valid</returns>
    <UnsecuredMethod()>
    Public Function HasCredentialKey() As Boolean Implements IServer.HasCredentialKey
        Using con = GetConnection()
            Dim scheme As clsEncryptionScheme = GetEncryptionSchemeByID(GetDefaultEncrypter(con))
            If scheme.HasValidKey Then Return True
            Return False
        End Using
    End Function

#End Region

#Region " Private Methods "
    ''' <summary>
    ''' Returns a collection of all credential objects, populated with their ID, name
    ''' description, expiry date and invalid flag.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="status">The status criteria</param>
    ''' <param name="includeUsername">True to include username</param>
    ''' <param name="includePassword">True to include password</param>
    ''' <returns>The collection of credential objects</returns>
    Private Function GetAllCredentialsInfo(ByVal con As IDatabaseConnection,
     ByVal status As clsCredential.Status, ByVal includeUsername As Boolean,
                                           ByVal includePassword As Boolean) As ICollection(Of clsCredential)
        Dim ids As New List(Of Guid)
        Dim credentials As New List(Of clsCredential)

        Dim cmd As New SqlCommand("select id from BPACredentials")
        Select Case status
            Case clsCredential.Status.Invalid
                cmd.CommandText &= " where invalid=1"
            Case clsCredential.Status.Expired
                cmd.CommandText &= " where invalid=0 and expirydate is not null and expirydate<@today"
                cmd.Parameters.AddWithValue("@today", Date.Today)
            Case clsCredential.Status.Valid
                cmd.CommandText &= " where invalid=0 and (expirydate is null or expirydate>=@today)"
                cmd.Parameters.AddWithValue("@today", Date.Today)
        End Select

        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                ids.Add(CType(reader("id"), Guid))
            End While
        End Using

        For Each id As Guid In ids
            credentials.Add(GetCredentialInfo(con, id, includeUsername, includePassword))
        Next

        Return credentials
    End Function

    ''' <summary>
    ''' Returns the credential for a given name (user/process/resource restrictions 
    ''' are not taken into account).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="name">The credential name</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given name</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    Private Function GetCredential(ByVal con As IDatabaseConnection,
                                   ByVal name As String) As clsCredential
        Dim credential As clsCredential

        credential = GetCredentialInfo(con, name, True, True)
        LoadCredentialRights(con, credential)
        LoadCredentialProperties(con, String.Empty, credential, True)

        Return credential
    End Function

    ''' <summary>
    ''' Returns summary credential information for a given credential ID (user/
    ''' process/resource restrictions are not taken into account).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="id">The credential ID</param>
    ''' <param name="name">The name to look up the credential with</param>
    ''' <param name="includeUsername">True to include username</param>
    ''' <param name="includePassword">True to include password</param>
    ''' <returns>The partial credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the
    ''' specified identifying value (id or name) was found.</exception>
    ''' <remarks>Only one of <paramref name="id"/> or <paramref name="name"/>
    ''' should be provided; however, if both are populated, the name is used in
    ''' preference over the id - ie. if the name is provided, the id will be ignored.
    ''' </remarks>
    Private Function GetCredentialInfo(ByVal con As IDatabaseConnection,
                                       ByVal id As Guid,
                                       ByVal name As String,
                                       ByVal includeUsername As Boolean,
                                       ByVal includePassword As Boolean
                                       ) As clsCredential
        Dim cmd As New SqlCommand("select * from BPACredentials where ")

        ' If name is provided use that, otherwise use the ID
        If name <> "" Then
            cmd.CommandText &= "name = @name"
            cmd.Parameters.AddWithValue("@name", name)
        Else
            cmd.CommandText &= "id = @id"
            cmd.Parameters.AddWithValue("@id", id)
        End If

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then
                If name <> "" Then
                    Throw New NoSuchCredentialException(name)
                Else
                    Throw New NoSuchCredentialException(id)
                End If
            End If

            Dim cred As New clsCredential()
            With New ReaderDataProvider(reader)
                cred.EncryptionKeyID = .GetValue("encryptid", 0)
                cred.ID = .GetValue("id", Guid.Empty)
                cred.Name = .GetString("name")
                cred.Description = .GetString("description")
                cred.Type = CredentialType.GetByName(.GetString("credentialType"))

                If includeUsername Then cred.Username = .GetString("login")

                If includePassword Then _
                    cred.Password = DecryptToSafeString(cred.EncryptionKeyID, .GetString("password"))

                cred.ExpiryDate = .GetValue("expirydate", DateTime.MinValue)
                '#9428 Trap credentials stored with Date.MaxValue and assume no expiry
                If cred.ExpiryDate.Date = Date.MaxValue.Date Then cred.ExpiryDate = Date.MinValue
                cred.IsInvalid = .GetValue("invalid", False)
            End With
            Return cred

        End Using

    End Function

    ''' <summary>
    ''' Returns summary credential information for a given credential ID (user/
    ''' process/resource restrictions are not taken into account).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="id">The credential ID</param>
    ''' <param name="includeUsername">True to include username</param>
    ''' <param name="includePassword">True to include password</param>
    ''' <returns>The partial credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' ID was found</exception>
    Private Function GetCredentialInfo(ByVal con As IDatabaseConnection,
                                       ByVal id As Guid,
                                       ByVal includeUsername As Boolean,
                                       ByVal includePassword As Boolean
                                       ) As clsCredential
        Return GetCredentialInfo(con, id, Nothing, includeUsername, includePassword)
    End Function

    ''' <summary>
    ''' Returns summary credential information for a given credential name (user/
    ''' process/resource restrictions are not taken into account).
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="name">The credential name</param>
    ''' <param name="includeUsername">True to include username</param>
    ''' <param name="includePassword">True to include password</param>
    ''' <returns>The partial credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' name was found</exception>
    Private Function GetCredentialInfo(ByVal con As IDatabaseConnection,
                                       ByVal name As String,
                                       ByVal includeUsername As Boolean,
                                       ByVal includePassword As Boolean
                                       ) As clsCredential
        Return GetCredentialInfo(con, Nothing, name, includeUsername, includePassword)
    End Function

    ''' <summary>
    ''' Load the credential access rights information into the passed credential
    ''' object.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="cred">The credential object into which the data should be
    ''' loaded</param>
    ''' <exception cref="OperationFailedException">If any of the resource or role
    ''' data was not returned as expected.</exception>
    Private Sub LoadCredentialRights(
     ByVal con As IDatabaseConnection, ByRef cred As clsCredential)

        Dim cmd As New SqlCommand(
         "select processid from BPACredentialsProcesses where credentialid=@id; " &
         "select resourceid from BPACredentialsResources where credentialid=@id;" &
         "select userroleid from BPACredentialRole where credentialid=@id"
         )
        cmd.Parameters.AddWithValue("@id", cred.ID)

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)

            cred.ProcessIDs.Clear()
            While reader.Read()
                cred.ProcessIDs.Add(prov.GetValue("processid", Guid.Empty))
            End While

            If Not reader.NextResult() Then
                Throw New OperationFailedException(
                      My.Resources.clsServer_FailedToRetrieveResourceRightsForCredential)
            End If

            cred.ResourceIDs.Clear()
            While reader.Read()
                cred.ResourceIDs.Add(prov.GetValue("resourceid", Guid.Empty))
            End While

            If Not reader.NextResult() Then
                Throw New OperationFailedException(
                      My.Resources.clsServer_FailedToRetrieveUserRoleRightsForCredential)
            End If

            cred.Roles.Clear()
            Dim sysRoles As RoleSet = SystemRoleSet.Current
            While reader.Read()
                cred.Roles.Add(sysRoles(prov.GetValue("userroleid", 0)))
            End While

        End Using
    End Sub

    ''' <summary>
    ''' Load the credential property information (and optionally property values) 
    ''' into the passed credential object.
    ''' If no property name is specified all are loaded.
    ''' </summary>
    ''' <param name="connection">The database connection</param>
    ''' <param name="propertyNameToInclude">The optional property name</param>
    ''' <param name="credential">The credential object</param>
    ''' <param name="includeValues">True to include the sensitive property values</param>
    Private Sub LoadCredentialProperties(
     connection As IDatabaseConnection,
     propertyNameToInclude As String,
     credential As clsCredential,
     includeValues As Boolean)
        credential.Properties.Clear()

        Dim query = New StringBuilder("SELECT name")
        If includeValues Then
            query.Append(", value ")
        Else
            query.Append(" ")
        End If
        query.Append("FROM BPACredentialsProperties 
                      WHERE credentialid = @id")

        Using command = New SqlCommand(query.ToString())
            command.Parameters.AddWithValue("@id", credential.ID)
            If propertyNameToInclude <> String.Empty Then
                command.CommandText += " AND name = @name"
                command.Parameters.AddWithValue("@name", propertyNameToInclude)
            End If
            Using reader = connection.ExecuteReturnDataReader(command)
                Dim provider As New ReaderDataProvider(reader)
                Dim properties As New Dictionary(Of String, SafeString)
                While reader.Read()
                    Dim propertyName = provider.GetString("name")
                    Dim propertyValue = If(includeValues, DecryptToSafeString(credential.EncryptionKeyID, provider.GetString("value")), New SafeString())

                    properties.Add(propertyName, propertyValue)
                End While
                credential.Properties = properties
            End Using
        End Using
        
    End Sub

    ''' <summary>
    ''' Set the value of a credential property to the given value. If the specified 
    ''' credential does not have such a property, then the property will be created
    ''' before being set at the required value. 
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="credential">The credential object</param>
    ''' <param name="propertyName">The name of the credential property</param>
    ''' <param name="propertyValue">The value to set the property with</param>
    Private Sub SetCredentialProperty(ByVal con As IDatabaseConnection,
                                      ByRef credential As clsCredential,
                                      ByVal propertyName As String,
                                      ByVal propertyValue As SafeString)

        LoadCredentialProperties(con, propertyName, credential, True)

        Dim cmd As SqlCommand

        If credential.Properties.ContainsKey(propertyName) Then
            cmd = New SqlCommand(
                    "update BPACredentialsProperties " &
                    "set value = @value " &
                    "where credentialid = @credid and name = @propName")
        Else
            cmd = New SqlCommand(
             " insert into BPACredentialsProperties (id,name,credentialid,value)" &
             "  values (@id,@propName,@credid,@value)")
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid())
        End If
        With cmd.Parameters
            .AddWithValue("@credid", credential.ID)
            .AddWithValue("@propName", propertyName)
            If propertyValue.IsEmpty Then
                .AddWithValue("@value", DBNull.Value)
            Else
                .AddWithValue("@value", Encrypt(credential.EncryptionKeyID, propertyValue))
            End If
        End With
        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Saves the credential properties set in the given credential
    ''' </summary>
    ''' <param name="con">The connection to the database to save the properties over.
    ''' </param>
    ''' <param name="cred">The credential containing the properties to save to
    ''' the database. Note that this credential should already exist on the database.
    ''' </param>
    Private Sub SaveCredentialProperties(
     con As IDatabaseConnection, cred As clsCredential)

        Dim cmd As New SqlCommand(
            "delete from BPACredentialsProperties where credentialid = @credid;"
        )
        cmd.Parameters.AddWithValue("@credid", cred.ID)
        con.Execute(cmd)

        cmd.CommandText =
         " insert into BPACredentialsProperties (id, name, credentialid, value)" &
         "  values (@id, @name, @credid, @value);"
        Dim idParam As SqlParameter =
            cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier)
        Dim propNameParam As SqlParameter =
            cmd.Parameters.Add("@name", SqlDbType.NVarChar, 255)
        Dim propValueParam As SqlParameter =
            cmd.Parameters.Add("@value", SqlDbType.NVarChar, -1)

        For Each pair In cred.Properties
            idParam.Value = Guid.NewGuid()
            propNameParam.Value = pair.Key
            propValueParam.Value = IIf(pair.Value Is Nothing, DBNull.Value,
             Encrypt(cred.EncryptionKeyID, pair.Value))
            con.Execute(cmd)
        Next

    End Sub


    ''' <summary>
    ''' Updates the credential with changes made to it's properties from the
    ''' collection of Credential Property.
    ''' </summary>
    ''' <param name="con">The connection. </param>
    ''' <param name="cred">The credential which has properties we will be changing.
    ''' </param>
    ''' <param name="properties">A collection of properties which are associated with
    ''' the Credential. If this is null, the properties that are set in
    ''' <paramref name="cred"/> will be treated as current; otherwise, the values in
    ''' <paramref name="properties"/> will be treated as current.</param>
    Private Sub UpdatePropertyChanges(
     con As IDatabaseConnection,
     cred As clsCredential,
     properties As ICollection(Of CredentialProperty))

        ' "properties" overrides "cred.Properties" if it is given.
        If properties IsNot Nothing Then
            LoadCredentialProperties(con, Nothing, cred, True)
            Dim resolver As New CredentialPropertyResolver() With {
                .ExistingProperties = cred.Properties,
                .PropertyChanges = properties
            }
            cred.Properties = resolver.Resolve()
        End If
        SaveCredentialProperties(con, cred)

    End Sub

    ''' <summary>
    ''' Saves the associated credential data to the database - this includes
    ''' the processes/resources from which this credential can be successfully
    ''' requested, and any property name/value pairs.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="cred">The credential object</param>
    ''' <remarks>Used by the unconstrained Update() and Create() methods.</remarks>
    Private Sub SaveCredentialAssociatedData(
     ByVal con As IDatabaseConnection, ByVal cred As clsCredential)
        SaveCredentialAssociatedData(con, cred, Nothing)
    End Sub

    ''' <summary>
    ''' Saves the associated credential data to the database - this includes
    ''' the processes/resources from which this credential can be successfully
    ''' requested, and any property name/value pairs.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="cred">The credential object</param>
    ''' <param name="properties">The list of credential properties to update. </param>
    ''' <remarks>Used by the unconstrained Update() and Create() methods.</remarks>
    Private Sub SaveCredentialAssociatedData(
     con As IDatabaseConnection,
     cred As clsCredential,
     properties As ICollection(Of CredentialProperty))

        ' Remove existing associated data

        Dim deleteCommandBuilder = New StringBuilder(
            " delete from BPACredentialRole where credentialid = @credid;" &
            " delete from BPACredentialsProcesses where credentialid = @credid;" &
            " delete from BPACredentialsResources where credentialid = @credid;")

        If properties Is Nothing Then
            deleteCommandBuilder.Append(" delete from BPACredentialsProperties where credentialid = @credid;")
        End If

        Dim cmd As New SqlCommand(deleteCommandBuilder.ToString())

        cmd.Parameters.AddWithValue("@credid", cred.ID)
        con.Execute(cmd)
        Dim sb As New StringBuilder()

        If cred.Roles.Count > 0 Then
            sb.Append("insert into BPACredentialRole (credentialid, userroleid) ")
            Dim paramNo As Integer = 0
            For Each r As Role In cred.Roles
                If paramNo > 0 Then sb.Append("union all ")
                paramNo += 1
                sb.AppendFormat("select @credid, @id{0} ", paramNo)
                Dim idVal As Object
                If r Is Nothing Then idVal = DBNull.Value Else idVal = r.Id
                cmd.Parameters.AddWithValue("@id" & paramNo, idVal)
            Next
            cmd.CommandText = sb.ToString()
            con.Execute(cmd)
        End If

        ' Insert the processes we need to use.
        cmd.CommandText =
         " insert into BPACredentialsProcesses (credentialid,processid)" &
         "  values (@credid,@processid)"
        Dim procParam As SqlParameter =
         cmd.Parameters.Add("@processid", SqlDbType.UniqueIdentifier)
        For Each procId As Guid In cred.ProcessIDs
            procParam.Value = IIf(procId = Guid.Empty, DBNull.Value, procId)
            con.Execute(cmd)
        Next
        cmd.Parameters.Remove(procParam)

        ' Add the resources we want to allow this credential to be used by
        cmd.CommandText =
         " insert into BPACredentialsResources (credentialid,resourceid)" &
         "  values (@credid,@resourceid)"

        Dim resParam As SqlParameter =
         cmd.Parameters.Add("@resourceid", SqlDbType.UniqueIdentifier)
        For Each resId As Guid In cred.ResourceIDs
            resParam.Value = IIf(resId = Guid.Empty, DBNull.Value, resId)
            con.Execute(cmd)
        Next
        cmd.Parameters.Remove(resParam)

        ' Add the properties associated with this credential
        UpdatePropertyChanges(con, cred, properties)

    End Sub

    ''' <summary>
    ''' Updates a credential, without any constraint checking.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="credential">The credential object</param>
    Private Sub UpdateCredentialInfo(
     ByVal con As IDatabaseConnection, ByVal credential As clsCredential)

        Dim updatePassword As Boolean = credential.Password.Length > 0
        Dim passwordCommand As String = If(updatePassword, "  password=@password,", String.Empty)

        Dim cmd As New SqlCommand(
         " update BPACredentials set " &
         "  name=@name," &
         "  description=@description," &
         "  login=@login," &
         "  encryptid=@encryptid," &
            passwordCommand &
         "  expirydate=@expirydate, " &
         "  invalid=@invalid, " &
         "  credentialType=@credentialType " &
         " where id=@id")

        With cmd.Parameters
            .AddWithValue("@id", credential.ID)
            .AddWithValue("@name", credential.Name)
            .AddWithValue("@description", credential.Description)
            .AddWithValue("@login", credential.Username)
            credential.EncryptionKeyID = GetDefaultEncrypter(con)
            .AddWithValue("@encryptid", credential.EncryptionKeyID)
            .AddWithValue("@expirydate", IIf(credential.ExpiryDate = DateTime.MinValue, DBNull.Value, credential.ExpiryDate))
            .AddWithValue("@invalid", credential.IsInvalid)
            .AddWithValue("@credentialType", credential.Type.Name)
            If updatePassword Then .AddWithValue("@password", Encrypt(credential.EncryptionKeyID, credential.Password))
        End With

        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Creates a new credential on the database, <em>including</em> its
    ''' <see cref="SaveCredentialAssociatedData">associated data</see>.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="credential">The credential to create</param>
    Private Function CreateCredential(
     ByVal con As IDatabaseConnection, ByVal credential As clsCredential) As Guid

        Try
            credential.ID = Guid.NewGuid()
            Dim cmd As New SqlCommand(
             " insert into BPACredentials (id,name,description,login,encryptid,password,expirydate,invalid,credentialType)" &
             "  values (@id,@name,@description,@login,@encryptid,@password,@expirydate,@invalid,@credentialType)")
            With cmd.Parameters
                .AddWithValue("@id", credential.ID)
                .AddWithValue("@name", credential.Name)
                .AddWithValue("@description", credential.Description)
                .AddWithValue("@login", credential.Username)
                credential.EncryptionKeyID = GetDefaultEncrypter(con)
                .AddWithValue("@encryptid", credential.EncryptionKeyID)
                .AddWithValue("@password", Encrypt(credential.EncryptionKeyID, credential.Password))
                .AddWithValue("@expirydate", IIf(credential.ExpiryDate = DateTime.MinValue, DBNull.Value, credential.ExpiryDate))
                .AddWithValue("@invalid", credential.IsInvalid)
                .AddWithValue("@credentialType", credential.Type.Name)
            End With
            con.Execute(cmd)

            SaveCredentialAssociatedData(con, credential)

            Return credential.ID

        Catch
            ' Ensure we don't leave a now-invalid ID inside the credential
            credential.ID = Nothing
            Throw

        End Try
    End Function

    ''' <summary>
    ''' Deletes a credential
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="id">The credential ID</param>
    Private Sub DeleteCredential(ByVal con As IDatabaseConnection, ByVal id As Guid)
        Dim cmd As New SqlCommand(
         " delete from BPACredentialRole where credentialid=@credid;" &
         " delete from BPACredentialsProcesses where credentialid=@credid;" &
         " delete from BPACredentialsResources where credentialid=@credid;" &
         " delete from BPACredentialsProperties where credentialid=@credid;" &
         " delete from BPACredentials where id=@credid;")
        cmd.Parameters.AddWithValue("@credid", id)

        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Requests a single credential given the credential name and session ID, and
    ''' using the role of the current logged in user.
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the credential.
    ''' </param>
    ''' <param name="sessId">The current session ID</param>
    ''' <param name="name">The name of the credential to get</param>
    ''' <returns>The credential associated with the given name</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential with the given
    ''' name is found on the database</exception>
    ''' <exception cref="PermissionException">If the current caller context does not
    ''' have access rights to the specified credential</exception>
    Private Function RequestCredential(ByVal con As IDatabaseConnection,
     ByVal sessId As Guid, ByVal name As String) As clsCredential

        Dim cred As clsCredential = GetCredential(con, name)
        Dim exReason As String = ""
        If CanAccessCredential(con, sessId, cred, exReason) Then Return cred
        ' Else...
        Throw New PermissionException(exReason)

    End Function

    ''' <summary>
    ''' Checks that the credential is accessible to current role, resource and
    ''' process. The passed credential object must have at least ID and role(s) set
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="sessId">The ID of the session which is requesting access to the
    ''' credential.</param>
    ''' <param name="credential">The credential object</param>
    ''' <param name="exReason">The message to be returned in the exception detailing
    ''' the reason the credential cannot be accessed</param>
    ''' <returns>True if the credential is accessible, otherwise false</returns>
    Private Function CanAccessCredential(ByVal con As IDatabaseConnection,
     ByVal sessId As Guid, ByVal credential As clsCredential,
     ByRef exReason As String) As Boolean

        ' First check that the passed sessionID actually represents a valid,
        ' active session running on this resource
        Dim session As clsProcessSession = Nothing
        If sessId <> Guid.Empty Then _
            session = GetActualSessions(con, sessId, {mLoggedInMachine}).FirstOrDefault
        If session Is Nothing OrElse Not session.IsRunningOrDebugging Then
            exReason = String.Format(
                My.Resources.clsServer_TheSession0IsNotCurrentlyRunningOnThisResource1,
                sessId, mLoggedInMachine)
            Return False
        End If

        ' Check if such a row exists, if not, then fail permission check
        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@credid", credential.ID)

        cmd.CommandText =
         " select 1 from BPACredentialsProcesses" &
         "  where credentialid=@credid and (processid is null or processid=@procid)"
        cmd.Parameters.AddWithValue("@procid", session.ProcessID)
        If con.ExecuteReturnScalar(cmd) Is Nothing Then
            exReason = String.Format(
                My.Resources.clsServer_TheProcess0DoesNotHavePermissionToAccessTheCredential1,
                session.ProcessName, credential.Name)
            Return False
        End If

        cmd.Parameters.RemoveAt("@procid")

        ' Again, check if such a row exists, if not, then fail permission check
        cmd.CommandText =
         " select 1 from BPACredentialsResources" &
         "  where credentialid=@credid and (resourceid is null or resourceid=@resid" &
         " or resourceid = (select pool from BPAResource WHERE resourceid = @resid))"
        cmd.Parameters.AddWithValue("@resid", session.ResourceID)

        If con.ExecuteReturnScalar(cmd) Is Nothing Then
            exReason = String.Format(
                My.Resources.clsServer_TheResource0DoesNotHavePermissionToAccessTheCredential1,
                session.ResourceName, credential.Name)
            Return False
        End If

        cmd.Parameters.RemoveAt("@resid")

        ' Finally do the same with the roles - a bit more awkward since it's a
        ' many-many check rather than a one-many.
        Dim roles As RoleSet = GetLoggedInUserRoles()
        If roles.Count = 0 Then
            ' If there are no roles (either not logged in or user has no roles
            ' assigned to them), then the only type of credential allowed is one
            ' which is accessible to 'all roles'.
            cmd.CommandText =
             " select 1 from BPACredentialRole" &
             "  where credentialid=@credid and userroleid is null"
        Else
            Dim sb As New StringBuilder(
             " select 1 from BPACredentialRole" &
             "  where credentialid=@credid " &
             "    and (userroleid is null or userroleid in ("
            )
            Dim ind As Integer = 0
            For Each r As Role In roles
                If ind > 0 Then sb.Append(","c)
                ind += 1
                sb.Append("@role").Append(ind)
                cmd.Parameters.AddWithValue("@role" & ind, r.Id)
            Next
            sb.Append("))")
            cmd.CommandText = sb.ToString()
        End If
        If con.ExecuteReturnScalar(cmd) Is Nothing Then
            'Either: 
            If roles.Count = 0 AndAlso Not GetLoggedIn() Then
                'the resource being used is not logged in
                exReason = String.Format(My.Resources.clsServer_TheResource0IsNotLoggedInSoDoesNotHavePermissionToAccessCredential1WhichIsRestr,
                                         session.ResourceName, credential.Name)
            Else
                'or the user who starts the process doesn't have the required role
                exReason = String.Format(
                                My.Resources.clsServer_User0DoesNotHavePermissionToAccessTheCredential1AsItIsRestrictedByUserRole,
                                GetLoggedInUserName, credential.Name)
            End If

            Return False
        End If
        ' If we found all we needed, the user / process / resource can access it
        Return True
    End Function

    ''' <summary>
    ''' Returns the ID of the current default encryption scheme.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>The encrypter ID</returns>
    Private Function GetDefaultEncrypter(con As IDatabaseConnection) As Integer
        Dim cmd As New SqlCommand("select defaultencryptid from BPASysConfig")
        Return CInt(con.ExecuteReturnScalar(cmd))
    End Function

    ''' <summary>
    ''' Returns the credential for a given credential ID, optionally including the
    ''' logon details (user/process/resource restrictions are not taken into account).
    ''' </summary>
    ''' <param name="id">The credential ID</param>
    ''' <param name="includeUsername">True to include username</param>
    ''' <param name="includePassword">True to include password</param>
    ''' <param name="includePropertyValues">True to include the sensitive values of 
    ''' the credential's properties</param>
    ''' <returns>The credential object</returns>
    ''' <exception cref="NoSuchCredentialException">If no credential was found on the
    ''' database with the given ID</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to get
    ''' the credential</exception>
    Private Function GetCredential(ByVal id As Guid,
                                   ByVal includeUsername As Boolean,
                                   ByVal includePassword As Boolean,
                                   ByVal includePropertyValues As Boolean) As clsCredential
        Using con As IDatabaseConnection = GetConnection()
            Dim credential As clsCredential

            credential = GetCredentialInfo(con, id, includeUsername, includePassword)
            LoadCredentialRights(con, credential)
            LoadCredentialProperties(con, String.Empty, credential, includePropertyValues)

            Return credential
        End Using
    End Function


#End Region


End Class
