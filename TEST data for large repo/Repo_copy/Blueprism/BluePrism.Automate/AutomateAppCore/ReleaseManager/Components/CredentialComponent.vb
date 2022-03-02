Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.Common.Security
Imports System.Runtime.Serialization

''' <summary>
''' Component representing a credential.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class CredentialComponent : Inherits SingleTypeComponentGroup

#Region " Class Scope Definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the credential component.
    ''' </summary>
    Public Class MyConflicts

        ''' <summary>
        ''' A space character to be used in strings
        ''' </summary>
        Private Const mSpace As String = " "

        ''' <summary>
        ''' 'Conflict' which occurs when a credential doesn't exist in the target
        ''' environment - odd for a conflict, but it means that the user name and
        ''' password for the credential must be created. Note that this conflict
        ''' doesn't occur when a release is imported non-interactively via the
        ''' AutomateC command
        ''' </summary>
        ''' <param name="credentialType">The credential type we are creating a conflict
        ''' definition for. </param>
        ''' <returns>A conflict definition instance. </returns>
        Public Shared Function NewCredential(credentialType As CredentialType) As ConflictDefinition

            If credentialType Is Nothing Then Throw New ArgumentNullException(NameOf(credentialType))

            Dim conflictId = "NewCredential"

            Dim textBuilder = New StringBuilder()
            textBuilder.Append(ComponentResources.New_Credential_Conflict_Text)
            textBuilder.Append(mSpace)
            textBuilder.Append(String.Format(ComponentResources.New_Credential_Credential_Type_Template, credentialType.LocalisedTitle))
            textBuilder.AppendLine()
            textBuilder.Append(credentialType.LocalisedDescription)

            Dim conflictArgs = GetConflictHandlerArguments(credentialType).ToArray()
            Dim dataHandler = New ConflictDataHandler("CredentialDetails", Nothing, conflictArgs)

            Dim conflictOptions = {New ConflictOption(ConflictOption.UserChoice.ExtraInfo,
                                             ComponentResources.New_Credential_Conflict_Option_Import,
                                             dataHandler),
                                   New ConflictOption(ConflictOption.UserChoice.Skip,
                                             ComponentResources.New_Credential_conflict_Option_Dont_Import)}

            Dim result = New ConflictDefinition(conflictId,
                                                textBuilder.ToString(),
                                                ComponentResources.New_Credential_Conflict_Hint,
                                                conflictOptions)

            result.DefaultInteractiveResolution = ConflictOption.UserChoice.ExtraInfo

            Return result

        End Function

        ''' <summary>
        ''' Gets an array of ConflictArgument which defines what UI components to use on 
        ''' the wizard form for a given credential type.
        ''' </summary>
        ''' <param name="credentialType">The credential type we are defining arguments 
        ''' for. </param>
        ''' <returns>An Enumerable of ConflictArgument. </returns>
        Private Shared Iterator Function GetConflictHandlerArguments(credentialType As CredentialType) As IEnumerable(Of ConflictArgument)

            If credentialType.IsUsernameVisible Then
                Yield New ConflictArgument("Username", "",
                                           credentialType.LocalisedUsernamePropertyTitle)
            End If

            Yield New ConflictArgument("Password", New clsProcessValue(DataType.password),
                                       credentialType.LocalisedPasswordPropertyTitle)
        End Function


        ''' <summary>
        ''' Conflict which occurs when no master key is available for the encryption
        ''' of credentials in this environment - in this case, credentials cannot
        ''' be imported
        ''' </summary>
        Public Shared ReadOnly Property NoMasterKey As ConflictDefinition
            Get
                If nNoMasterKey Is Nothing Then
                    nNoMasterKey = GetNoMasterKeyConflictDefinition()
                Else
                    nNoMasterKey.UpdateConflictDefinitionStrings(GetNoMasterKeyConflictDefinition())
                End If
                Return nNoMasterKey
            End Get
        End Property

        Private Shared Function GetNoMasterKeyConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("NoMaster",
                                          My.Resources.
                                             MyConflicts_ThereIsNoCredentialEncryptionSchemeConfiguredInThisEnvironment,
                                          My.Resources.
                                             MyConflicts_CredentialsCannotBeImportedUntilOneIsSetUpInBluePrismServerOrSystemManager,
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.MyConflicts_DonTImportThisCredential)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Skip,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail}
        End Function

        Private Shared nNoMasterKey As ConflictDefinition

    End Class

   

    ''' <summary>
    ''' Extracts the collection of process components from the given credential
    ''' object.
    ''' </summary>
    ''' <param name="cred">The credential from which the process components that it
    ''' relates to should be extracted.</param>
    ''' <returns>The collection of package components representing the processes that
    ''' this credential applies to.</returns>
    Private Shared Function ExtractComponents(
     ByVal owner As OwnerComponent, ByVal cred As clsCredential) _
     As ICollection(Of PackageComponent)
        If cred Is Nothing OrElse cred.ProcessIDs.Count = 0 Then Return Nothing
        Dim comps As New List(Of PackageComponent)
        For Each id As Guid In cred.ProcessIDs
            comps.Add(New ProcessComponent(owner, id))
        Next
        Return comps
    End Function

    ''' <summary>
    ''' Merges the process group data into the given collection, using processes
    ''' from the given map and group data from the specified provider.
    ''' This will check to see if a process group with the given ID already exists
    ''' in the component collection. If it does, it will add the process referred to
    ''' by the provider to that group. Otherwise, it will create and add a new group
    ''' and add the process to that.
    ''' </summary>
    ''' <param name="comps">The collection into which the resultant process group
    ''' should be merged.</param>
    ''' <param name="processes">The map of processes to their IDs already loaded
    ''' (and presumably part of the 'comps' collection)</param>
    ''' <param name="prov">The data provider which provides the config of the
    ''' process group. This expects the data attributes : <list>
    ''' <item>id: Guid (the credential id)</item>
    ''' <item>name: String (the credential name)</item>
    ''' <item>processid: Guid (the process id)</item></list></param>
    Public Shared Sub MergeInto(
     ByVal owner As OwnerComponent,
     ByVal comps As ICollection(Of PackageComponent),
     ByVal processes As IDictionary(Of Guid, ProcessComponent),
     ByVal prov As IDataProvider)
        Dim credId As Guid = prov.GetValue("id", Guid.Empty)
        Dim credName As String = prov.GetString("name")
        Dim procId As Guid = prov.GetValue("processid", Guid.Empty)
        ' If there is no processid, then the credential is for 'all processes'
        If procId = Nothing Then
            comps.Add(New CredentialComponent(owner, credId, credName,
             New clsCovariantCollection(Of PackageComponent, ProcessComponent)(processes.Values)))

        Else
            ' See if the given processes include this process (it may be that a
            ' credential is assigned to a process not handled by this collection)
            ' If it does, then find the credential component and add the process
            ' to it. If the credential component doesn't exist, create a new
            ' one with the process and add it to the collection.
            Dim proc As ProcessComponent = Nothing
            ' If the process exists in the collection, we add it to the credential.
            ' If it doesn't, we still need to create the credential - even if it
            ' ends up with absolutely no processes assigned to it.
            processes.TryGetValue(procId, proc)
            For Each comp As PackageComponent In comps
                If TypeOf comp Is CredentialComponent AndAlso comp.IdAsGuid = credId Then
                    ' Note that Add(Nothing) just no-ops so this is fine if proc wasn't
                    ' found in the processes dictionary.
                    DirectCast(comp, CredentialComponent).Members.Add(proc)
                    Return
                End If
            Next
            ' Credential component wasn't found - add a new one with that process (or lack thereof)
            comps.Add(New CredentialComponent(owner, credId, credName, proc))
        End If

    End Sub

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new credential component from the given data provider.
    ''' </summary>
    ''' <param name="prov">The provider containing the data for this component. This
    ''' constructor expects the following properties from the provider :-<list>
    ''' <item>id: Guid</item>
    ''' <item>name: String</item></list></param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner,
         prov.GetValue("id", Guid.Empty), prov.GetString("name"), New PackageComponent() {})
    End Sub

    ''' <summary>
    ''' Creates a new credential component from the given values
    ''' </summary>
    ''' <param name="id">The ID of the credential that this component represents.
    ''' </param>
    ''' <param name="name">The name of the credential</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Guid, ByVal name As String)
        Me.New(owner, id, name, New PackageComponent() {})
    End Sub

    ''' <summary>
    ''' Creates a new credential component from the credential object.
    ''' </summary>
    ''' <param name="cred">The credential object from which to create the component.
    ''' </param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal cred As clsCredential)
        Me.New(owner, cred.ID, cred.Name, ExtractComponents(owner, cred))
    End Sub

    ''' <summary>
    ''' Creates a new credential component from the given values
    ''' </summary>
    ''' <param name="id">The ID of the credential that this component represents.
    ''' </param>
    ''' <param name="name">The name of the credential</param>
    ''' <param name="proc">The single process which this credential should be
    ''' associated with.</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal id As Guid, ByVal name As String, ByVal proc As PackageComponent)
        Me.New(owner, id, name, DirectCast(
         IIf(proc Is Nothing, New PackageComponent() {}, New PackageComponent() {proc}),
         ICollection(Of PackageComponent)))
    End Sub

    ''' <summary>
    ''' Creates a new credential component from the given values
    ''' </summary>
    ''' <param name="id">The ID of the credential that this component represents.
    ''' </param>
    ''' <param name="name">The name of the credential</param>
    ''' <param name="procs">The collection of processes that this credential should
    ''' be associated with.</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal id As Guid, ByVal name As String, ByVal procs As ICollection(Of PackageComponent))
        MyBase.New(owner, id, name)
        AddAll(procs)
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of the members of this component.
    ''' </summary>
    Public Overrides ReadOnly Property MembersType() As PackageComponentType
        Get
            Return PackageComponentType.Process
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Credential
        End Get
    End Property

    ''' <summary>
    ''' Gets the credential associated with this component, if one exists.
    ''' Note that, if retrieved from the database, this credential will <em>not</em>
    ''' contain the username and password data from the current environment.
    ''' </summary>
    Public ReadOnly Property AssociatedCredential() As clsCredential
        Get
            Return DirectCast(AssociatedData, clsCredential)
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return Auth.Permission.SystemManager.Security.Credentials
        End Get
    End Property

#End Region

#Region " XML Handling "

    ''' <summary>
    ''' Writes this component out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this component should be written.
    ''' </param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        Dim c As clsCredential = AssociatedCredential
        If c.Description <> "" Then writer.WriteElementString("description", c.Description)
        If c.ExpiryDate <> DateTime.MinValue Then writer.WriteElementString("expirydate", c.ExpiryDate.ToString("o"))
        If c.IsInvalid Then writer.WriteElementString("invalid", c.IsInvalid.ToString())
        If c.Properties.Count > 0 Then
            writer.WriteStartElement("properties")
            For Each prop As KeyValuePair(Of String, SafeString) In c.Properties
                writer.WriteElementString("name", prop.Key)
            Next
            writer.WriteEndElement()
        End If
        writer.WriteElementString("credentialType", c.Type.Name)
        MyBase.WriteXmlBody(writer)
    End Sub

    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        Dim c As New clsCredential()
        c.Name = Me.Name

        ' If it's an empty element, that just means there's no description or members ..
        If r.IsEmptyElement Then Return

        ' We have content inside the credential element, read the description
        While r.Read()
            If r.NodeType = XmlNodeType.Element Then
                Select Case r.Name
                    Case "description"
                        If r.IsEmptyElement Then Continue While ' No description
                        r.Read() ' Get to the text element
                        c.Description = r.Value

                    Case "expirydate"
                        If r.IsEmptyElement Then Continue While ' No expirydate
                        r.Read()
                        DateTime.TryParse(r.Value, c.ExpiryDate)

                    Case "invalid"
                        If r.IsEmptyElement Then Continue While ' No invalid flag
                        r.Read()
                        c.IsInvalid = Convert.ToBoolean(r.Value)

                    Case "properties"
                        If r.IsEmptyElement Then Continue While ' No properties
                        Dim props As New Dictionary(Of String, SafeString)
                        While r.Read()
                            ' Not an element, skip to the next node
                            If r.NodeType <> XmlNodeType.Element Then Continue While
                            ' Not the name element, skip to the next node
                            If r.Name <> "name" Then Continue While
                            ' Name element is empty (?), skip to the next node
                            If r.IsEmptyElement Then Continue While
                            ' Read the value and save the property with a blank value
                            r.Read()
                            props.Add(r.Value, New SafeString())
                        End While
                        c.Properties = props

                    Case "credentialType"
                        If r.IsEmptyElement Then Continue While ' No credentialType
                        r.Read()
                        c.Type = CredentialType.GetByName(r.Value)

                    Case ComponentGroup.MembersXmlElementName
                        ReadMemberReferences(r.ReadSubtree(), ctx)

                End Select
            End If
        End While

        AssociatedData = c

    End Sub

#End Region

#Region " Conflict Handling "

    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)

        ' If we don't have a credential master key, we can't import any credentials.
        If Not gSv.HasCredentialKey() Then _
         Return GetSingleton.ICollection(MyConflicts.NoMasterKey)

        Dim captureCredentials = True
        Dim rel = TryCast(Owner, clsRelease)
        If rel IsNot Nothing AndAlso rel.UnattendedImport Then captureCredentials = False

        If captureCredentials Then
            Dim credID As Guid = gSv.GetCredentialID(Name)
            ' It's a new credential - we need to capture the user name and password
            If credID = Guid.Empty Then _
             Return GetSingleton.ICollection(
                MyConflicts.NewCredential(AssociatedCredential.Type))
        End If

        ' Otherwise, we can just amend the existing credential with any
        ' process assignments
        Return GetEmpty.ICollection(Of ConflictDefinition)()
    End Function

    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

    ''' <summary>
    ''' Applies all the conflict resolutions to this component - in reality, it only
    ''' needs one and there is no cross-component validation required for a
    ''' credential, so we can use an instance method rather than a static one.
    ''' </summary>
    ''' <param name="rel">The release in which the conflict occurred.</param>
    ''' <param name="resolutions">All the conflict resolutions for the release.
    ''' </param>
    ''' <param name="errors">The error log to which any validation errors should be
    ''' reported.</param>
    Private Sub ApplyConflictResolutions(ByVal rel As clsRelease,
     ByVal resolutions As ICollection(Of ConflictResolution), ByVal errors As clsErrorLog)

        Dim mods As IDictionary(Of ModificationType, Object) = Modifications

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            If res.Conflict.Component IsNot Me Then Continue For

            ' Check that the user has selected an option
            Dim opt As ConflictOption = res.ConflictOption
            If opt Is Nothing Then
                errors.Add(Me, My.Resources.CredentialComponent_YouMustChooseHowToHandleTheNewCredential0, Me.Name)
                Return
            End If

            ' If we're skipping import, well... that's easy.
            If opt.Choice = ConflictOption.UserChoice.Skip Then
                mods.Add(ModificationType.Skip, True)
                res.Passed = True
                Return
            End If

            ' Otherwise, one must assume that the user is to supply the details
            Debug.Assert(opt.Choice = ConflictOption.UserChoice.ExtraInfo)

            ' Set the data into the associated credential (any username and password
            ' in there (hint: there shouldn't be any) are invalid anyway).
            ' AssociatedCredential contains the credential Type.

            Dim cred As clsCredential = AssociatedCredential
            cred.Username = if(cred.Type.IsUsernameVisible, res.GetArgumentString("Username").Trim(), String.Empty)
            cred.Password = CType(res.GetArgumentValue("Password"), SafeString)
            res.Passed = True


            ' We can safely return, here - this component can have, at most, one
            ' conflict set on it, which we've dealt with, so there's no need to
            ' continue processing the collection.
            Return

        Next

    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.GetCredentialExcludingLogon(IdAsGuid)
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the exportable data
    ''' in the given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean

        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True

        Dim credComp As CredentialComponent = DirectCast(comp, CredentialComponent)
        Return (AssociatedCredential.Description <> credComp.AssociatedCredential.Description)

    End Function

#End Region

End Class
