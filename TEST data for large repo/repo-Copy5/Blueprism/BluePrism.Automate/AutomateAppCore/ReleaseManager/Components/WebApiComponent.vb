Imports System.Runtime.Serialization
Imports System.Xml
Imports System.Xml.Linq
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

''' ---------------------------------------------------------------------------------
''' <summary>
''' Component representing a web api 
''' </summary>
''' ---------------------------------------------------------------------------------
<Serializable, DataContract([Namespace]:="bp")>
Public Class WebApiComponent : Inherits PackageComponent

#Region " Class Scope Definitions "

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The conflict definitions which are handled by the process component.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Private Class MyConflicts

        ''' -------------------------------------------------------------------------
        ''' <summary>
        ''' Conflict caused by an api having the same name as an existing web api
        ''' </summary>
        ''' -------------------------------------------------------------------------
        Public Shared ReadOnly Property NameClash As ConflictDefinition
            Get
                If mNameClash Is Nothing Then
                    mNameClash = GetNameClashConflictDefinition()
                Else
                    mNameClash.UpdateConflictDefinitionStrings(GetNameClashConflictDefinition())
                End If
                Return mNameClash
            End Get
        End Property

        Private Shared Function GetNameClashConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("WebApiNameClash",
                                          My.Resources.MyConflicts_AWebApiWithTheSameNameAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingWebApiWithTheIncomingWebApi),
                                          New ConflictOption(ConflictOption.UserChoice.Rename,
                                                             My.Resources.MyConflicts_ChooseANewNameForTheIncomingWebApi,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument("Web API Name",
                                                                                                          "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_WebAPIName))),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.MyConflicts_DonTImportThisWebApi)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mNameClash As ConflictDefinition

        Public Shared ReadOnly Property NameClashForSkill As ConflictDefinition
            Get
                If mNameClashForSkill Is Nothing Then
                    mNameClashForSkill = GetNameClashForSkillConflictDefinition()
                Else
                    mNameClashForSkill.UpdateConflictDefinitionStrings(GetNameClashForSkillConflictDefinition())
                End If
                Return mNameClashForSkill
            End Get
        End Property

        Private Shared Function GetNameClashForSkillConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("WebApiNameClashForSkill",
                                          My.Resources.MyConflicts_AWebApiWithTheSameNameAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_TheExistingWebApiWillNeedToBeRenamedBeforeThisSkillCanBeImported, Nothing
                                          ) With {
                                              .DefaultInteractiveResolution = ConflictOption.UserChoice.Fail,
                                              .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail
                                              }
        End Function

        Private Shared mNameClashForSkill As ConflictDefinition

        Public Shared ReadOnly Property NameClashForSkillWebApi As ConflictDefinition
            Get
                If mNameClashForSkillWebApi Is Nothing Then
                    mNameClashForSkillWebApi = GetNameClashForSkillWebApiConflictDefinition()
                Else
                    mNameClashForSkillWebApi.UpdateConflictDefinitionStrings(GetNameClashForSkillWebApiConflictDefinition())
                End If
                Return mNameClashForSkillWebApi
            End Get
        End Property

        Private Shared Function GetNameClashForSkillWebApiConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("WebApiNameClashForExistingSkill",
                                          My.Resources.MyConflicts_ThereIsAlreadyAWebApiInTheDatabaseWithTheSameNameThatIsUsedByAskill,
                                          "", New ConflictOption(ConflictOption.UserChoice.Rename,
                                                                 My.Resources.MyConflicts_ChooseANewNameForTheIncomingWebApi,
                                                                 New ConflictDataHandler("NewName",
                                                                                         Nothing,
                                                                                         New ConflictArgument("Web API Name", "")))
                                          ) With {
                                              .DefaultInteractiveResolution = ConflictOption.UserChoice.Rename,
                                              .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail
                                              }
        End Function

        Private Shared mNameClashForSkillWebApi As ConflictDefinition
    End Class

#End Region

#Region " Member Variables"

    ' The ID of the existing api with the same name as the incoming one.
    Private mExistingId As Guid

#End Region

#Region " Constructors "
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new web api component using data from the given data provider.
    ''' </summary>
    ''' <param name="prov">The provider of the data to use to initialises this
    ''' component. This expects the following properties to be available :- <list>
    ''' <item>id: Guid</item>
    ''' <item>name: String</item></list></param>
    ''' -----------------------------------------------------------------------------
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", Guid.Empty), prov.GetString("name"))
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new web api component representing the given web api 
    ''' </summary>
    ''' <param name="api">The details of the web api to be represented as a
    ''' component.</param>
    ''' -----------------------------------------------------------------------------
    Public Sub New(ByVal owner As OwnerComponent, ByVal api As WebApi)
        Me.New(owner, api.Id, api.FriendlyName)
        Me.AssociatedData = api
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new web api component using the given properties.
    ''' </summary>
    ''' <param name="id">The ID of the web api on the database.
    ''' </param>
    ''' <param name="name">The name of the web api to use.</param>
    ''' -----------------------------------------------------------------------------
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Guid, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new web api component which draws its data from the given XML
    ''' reader.
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the process data.</param>
    ''' <param name="ctx">The loading context for the XML reading</param>
    ''' -----------------------------------------------------------------------------
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The web api object associated with this component.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public ReadOnly Property AssociatedWebApi() As WebApi
        Get
            Return DirectCast(AssociatedData, WebApi)
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.WebApi
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return Permission.SystemManager.BusinessObjects.WebServicesWebApi
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

#End Region

#Region "Methods"

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Applies the given resolutions to this component.
    ''' </summary>
    ''' <param name="resolutions">The resolutions to check</param>
    ''' <param name="errors">The error log to append any errors for this component
    ''' due to the specified conflict resolution.</param>
    ''' -----------------------------------------------------------------------------
    Private Sub ApplyResolutions(
     ByVal resolutions As ICollection(Of ConflictResolution),
     ByVal errors As clsErrorLog)

        Dim mods As IDictionary(Of ModificationType, Object) = Modifications

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            ' Skip those that aren't me
            If res.Conflict.Component IsNot Me Then Continue For

            ' Deal with the user not choosing an option first - this is an
            ' error for all (current) conflict types.
            If res.ConflictOption Is Nothing Then
                errors.Add(Me,
                 My.Resources.WebApiComponent_YouMustChooseHowToHandleTheConflictingNameForWebApi0, Name)
                ' Move onto the next resolution - without an option there's little we can
                ' do for this one.
                Continue For
            End If

            Dim err As clsError = ApplyResolution(res, mods)
            If err Is Nothing Then res.Passed = True Else errors.Add(err)

        Next

    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Applies the given resolution to this component, adding any modifications to
    ''' the given map and returning an error as appopriate.
    ''' </summary>
    ''' <param name="res">The conflict resolution to apply</param>
    ''' <param name="mods">The modifications to add as a result of applying the
    ''' given resolution.</param>
    ''' <returns>The error which this resolution has evoked, or null if it applied
    ''' without any errors</returns>
    ''' -----------------------------------------------------------------------------
    Private Function ApplyResolution(
     ByVal res As ConflictResolution,
     ByVal mods As IDictionary(Of ModificationType, Object)) _
     As clsError

        ' The resolution option chosen (may be null if no option is chosen)
        Dim opt As ConflictOption = res.ConflictOption

        ' If the option is non-null, that's a programming error, it should have
        ' been checked by now.
        If opt Is Nothing Then Throw New ArgumentNullException("res.ConflictOption")

        ' The proposed name - null if not applicable
        Dim proposedName As String = res.GetArgumentString("Web API Name")

        Select Case opt.Choice
            Case ConflictOption.UserChoice.Overwrite
                mods(ModificationType.OverwritingExisting) = True

            Case ConflictOption.UserChoice.Rename
                If proposedName = "" Then Return New clsError(
                    Me, My.Resources.MyConflicts_YouMustProvideAnAlternativeNameForTheWebApi0, Name)

                If gSv.GetWebApiId(proposedName) <> Guid.Empty Then
                    Return New clsError(Me,
                     My.Resources.WebApiComponent_YouCannotRenameTheIncomingWebApi0To1ThatNameIsAlreadyInUseInThisEnvironment,
                                        Name, proposedName)
                End If

                mods(ModificationType.IncomingName) = proposedName

            Case ConflictOption.UserChoice.RenameExisting
                If gSv.GetWebApiId(proposedName) <> Guid.Empty Then
                    Return New clsError(Me,
                     My.Resources.WebApiComponent_YouCannotRenameTheExistingWebApi0To1ThatNameIsAlreadyInUseInThisEnvironment,
                                        Name, proposedName)
                End If

                mods(ModificationType.ExistingName) = proposedName

            Case ConflictOption.UserChoice.Skip
                mods(ModificationType.Skip) = True

        End Select

        Return Nothing
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Applies the given conflict resolutions to all web apis in the given
    ''' release.
    ''' This cannot be an instance method since it must check across all apis
    ''' rather than just checking the single api data.
    ''' As such, this method is primarily tasked with cross-component checking - any
    ''' single-component validation is handled in the <see cref="ApplyResolutions"/>
    ''' instance method - which this method calls before it performes all the
    ''' necessary cross-component validation.
    ''' </summary>
    ''' <param name="rel">The release to which conflict resolutions are being applied
    ''' </param>
    ''' <param name="resolutions">The resolutions to apply</param>
    ''' <param name="errors">The error log to which any errors are appended. If this
    ''' method does not append any errors, then the processes in the given release
    ''' are considered 'clean' and configured to be submitted to the database</param>
    ''' -----------------------------------------------------------------------------
    Private Shared Sub ApplyConflictResolutions(ByVal rel As clsRelease,
     ByVal resolutions As ICollection(Of ConflictResolution),
     ByVal errors As clsErrorLog)

        ' Let's get all the single-component validation out of the way first...
        For Each comp As PackageComponent In rel
            Dim api As WebApiComponent = TryCast(comp, WebApiComponent)
            If api IsNot Nothing Then api.ApplyResolutions(resolutions, errors)
        Next

        ' Cross component - again, we're checking names so the things to check for:
        ' 1) Renames don't clash with non-conflicting incoming names
        ' 2) Renames don't clash with other renames

        Dim matrix As New clsRenamingMatrix()
        Dim componentsByName As New Dictionary(Of String, WebApiComponent)
        For Each res As ConflictResolution In resolutions
            Dim apiComponent As WebApiComponent = TryCast(res.Conflict.Component, WebApiComponent)
            If apiComponent IsNot Nothing Then
                matrix(apiComponent.Name) = res.GetArgumentString("Web Api Name")
                componentsByName(apiComponent.Name) = apiComponent
            End If
        Next

        For Each c As clsRenamingMatrix.Clash In matrix.Validate()
            Dim apiComponent As WebApiComponent = componentsByName(c.Source)

            ' Each clash is in the form of :
            ' c.Source : The source (original) name of the component which clashed
            ' c.OriginalName : The original name of the component it clashed with
            ' c.NewName : The new name of the component it clashed with - may be null
            If c.NewName Is Nothing Then
                errors.Add(apiComponent,
                 My.Resources.WebApiComponent_TheWebApi0CannotBeRenamedTo1ThereIsAWebApiInThisReleaseWithThatName,
                 c.Source, c.OriginalName)

            Else
                errors.Add(apiComponent,
                 My.Resources.WebApiComponent_TheIncomingWebApis0And1CannotBothBeRenamedTo2,
                 c.Source, c.OriginalName, c.NewName)

            End If
        Next

    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Writes this web API out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which the details of this web
    ''' api should be written.</param>
    ''' -----------------------------------------------------------------------------
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        Dim api As WebApi = AssociatedWebApi

        If api Is Nothing Then Throw New NoSuchElementException(
             My.Resources.WebApiComponent_NoWebApiWithID0Found, IdAsGuid)

        If api.Enabled Then _
            writer.WriteAttributeString("enabled", XmlConvert.ToString(True))

        api.Configuration.ToXElement.WriteTo(writer)

    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Reads the web API component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    ''' -----------------------------------------------------------------------------
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader,
                                        ByVal ctx As IComponentLoadingContext)

        Dim apiId As Guid = IdAsGuid
        Dim friendlyName As String = Me.Name

        Dim enabled As Boolean =
                XmlConvert.ToBoolean(BPUtil.IfNull(r("enabled"), "false"))

        If r.ReadToFollowing("configuration") Then
            Dim configElement = CType(XElement.ReadFrom(r), XElement)
            Dim config As WebApiConfiguration =
                   WebApiConfiguration.FromXElement(configElement)

            AssociatedData = New WebApi(apiId, friendlyName, enabled, config)
        Else
            Throw New MissingXmlObjectException("configuration")
        End If


    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    ''' -----------------------------------------------------------------------------
    Protected Overrides Function LoadData() As Object
        Return gSv.GetWebApi(IdAsGuid)
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the data in the
    ''' given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    ''' -----------------------------------------------------------------------------
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True

        Dim api As WebApi = AssociatedWebApi
        If api Is Nothing Then Throw New NoSuchElementException(
         My.Resources.WebApiComponent_NoWebApiWithTheID0WasFound, Id)

        Dim componentApi As WebApi =
         DirectCast(comp, WebApiComponent).AssociatedWebApi

        Return (
            Normalise(api.Configuration.ToXml) <> Normalise(componentApi.Configuration.ToXml))

    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Replaces any CRLF char pairs in the given XML with a single LF char so that
    ''' the XML ends up 'normalised'. MS XML Writer does this automatically, so that
    ''' a written WSDL may end up different to a stored WSDL, even though it should
    ''' be identical.
    ''' </summary>
    ''' <param name="xml">The XML to normalise</param>
    ''' <returns>The normalised XML, or null if null was given.</returns>
    ''' -----------------------------------------------------------------------------
    Private Function Normalise(ByVal xml As String) As String
        If xml Is Nothing Then Return Nothing
        Return xml.Replace(vbCrLf, vbLf)
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    ''' -----------------------------------------------------------------------------
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        mExistingId = gSv.GetWebApiId(Name)
        If mExistingId <> Nothing Then
            Dim release = TryCast(Owner, clsRelease)
            If release IsNot Nothing AndAlso release.IsSkill Then
                If mExistingId <> IdAsGuid Then Return GetSingleton.ICollection(MyConflicts.NameClashForSkill)
            Else
                If (gSv.GetSkillVersionsWithWebApi(mExistingId).Any()) Then
                    Return GetSingleton.ICollection(MyConflicts.NameClashForSkillWebApi)
                Else
                    Return GetSingleton.ICollection(MyConflicts.NameClash)
                End If
            End If
        End If

        Return GetEmpty.ICollection(Of ConflictDefinition)()
    End Function

#End Region


End Class



