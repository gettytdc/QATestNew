Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data

Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models

Imports System.Runtime.Serialization

''' <summary>
''' Component representing a web service
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class WebServiceComponent : Inherits PackageComponent

#Region " Conflict Definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the process component.
    ''' </summary>
    Public Class MyConflicts

        ''' <summary>
        ''' Conflict caused by a web service having the same name as an existing
        ''' web service
        ''' </summary>
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

            Return New ConflictDefinition("WebServiceNameClash",
                                          My.Resources.MyConflicts_AWebServiceWithTheSameNameAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingWebServiceWithTheIncomingWebService),
                                          New ConflictOption(ConflictOption.UserChoice.Rename,
                                                             My.Resources.
                                                                MyConflicts_ChooseANewNameForTheIncomingWebService,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument(
                                                                                         "Web Service Name", "",
                                                                                         My.Resources.
                                                                                                             MyConflicts_WebServiceName))),
                                          New ConflictOption(ConflictOption.UserChoice.RenameExisting,
                                                             My.Resources.
                                                                MyConflicts_ChooseANewNameForTheExistingWebService,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument(
                                                                                         "Web Service Name", "",
                                                                                         My.Resources.
                                                                                                             MyConflicts_WebServiceName)))) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mNameClash As ConflictDefinition

    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new web service component using data from the given data provider.
    ''' </summary>
    ''' <param name="prov">The provider of the data to use to initialises this
    ''' component. This expects the following properties to be available :- <list>
    ''' <item>id: Guid</item>
    ''' <item>name: String</item></list></param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", Guid.Empty), prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new web service component representing the given web service
    ''' </summary>
    ''' <param name="ws">The details of the web service to be represented as a
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal ws As clsWebServiceDetails)
        Me.New(owner, ws.Id, ws.FriendlyName)
        Me.AssociatedData = ws
    End Sub

    ''' <summary>
    ''' Creates a new web service component using the given properties.
    ''' </summary>
    ''' <param name="id">The ID of the web service definition on the database.
    ''' </param>
    ''' <param name="name">The name of the web service to use.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Guid, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new process group component which draws its data from the given XML
    ''' reader.
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the process data.</param>
    ''' <param name="ctx">The loading context for the XML reading</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The web service details object associated with this component.
    ''' </summary>
    Public ReadOnly Property AssociatedWebService() As clsWebServiceDetails
        Get
            Return DirectCast(AssociatedData, clsWebServiceDetails)
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.WebService
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return "Business Objects - SOAP Web Services"
        End Get
    End Property

#End Region

#Region " XML Handling "

    ''' <summary>
    ''' Writes this web service out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which the details of this web
    ''' service should be written.</param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        Dim ws As clsWebServiceDetails = AssociatedWebService
        If ws Is Nothing Then Throw New NoSuchElementException(
         My.Resources.WebServiceComponent_NoWebServiceWithID0Found, IdAsGuid)

        ' First we add our attributes to the existing XML head
        writer.WriteAttributeString("timeout", ws.Timeout.ToString())
        writer.WriteAttributeString("url", ws.URL)
        If ws.Enabled Then writer.WriteAttributeString("enabled", XmlConvert.ToString(True))

        ' Then our inner elements.
        writer.WriteElementString("cached-wsdl", ws.WSDL)

        writer.WriteStartElement("assets")
        For Each s As String In ws.ExtraWSDL
            writer.WriteElementString("extra-wsdl", s)
        Next
        For Each s As String In ws.Schemas
            writer.WriteElementString("schema-xml", s)
        Next
        writer.WriteEndElement()

        writer.WriteElementString("settings-xml", ws.GetSettings())

    End Sub

    ''' <summary>
    ''' Reads the web service component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)

        Dim ws As clsWebServiceDetails = New clsWebServiceDetails()
        ws.FriendlyName = Me.Name
        ws.Id = IdAsGuid

        ' Pick up our extra attributes from the opening element
        ws.Timeout = XmlConvert.ToInt32(BPUtil.IfNull(r("timeout"), "30"))
        ws.Enabled = XmlConvert.ToBoolean(BPUtil.IfNull(r("enabled"), "false"))
        ws.URL = r("url")

        ' The name of the element we're reading the text from.
        Dim elName As String = Nothing
        While r.Read()
            Select Case r.NodeType
                Case XmlNodeType.Element
                    ' Set the name of the element we're reading.
                    elName = r.LocalName

                Case XmlNodeType.Text
                    ' read the text for the element we've set.
                    Select Case elName
                        Case "cached-wsdl" : ws.WSDL = r.Value
                        Case "extra-wsdl" : ws.ExtraWSDL.Add(r.Value)
                        Case "schema-xml" : ws.Schemas.Add(r.Value)
                        Case "settings-xml" : ws.SetSettings(r.Value)
                    End Select

            End Select
        End While
        AssociatedData = ws

    End Sub

#End Region

#Region " Conflict/Resolution Handling "

    ' The ID of the existing web service with the same name as the incoming one.
    Private mExistingId As Guid

    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        mExistingId = gSv.GetWebServiceId(Name)
        If mExistingId <> Nothing Then Return GetSingleton.ICollection(MyConflicts.NameClash)
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
    ''' Applies the given conflict resolutions to all web services in the given
    ''' release.
    ''' This cannot be an instance method since it must check across all services
    ''' rather than just checking the single service data.
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
    Private Shared Sub ApplyConflictResolutions(ByVal rel As clsRelease,
     ByVal resolutions As ICollection(Of ConflictResolution),
     ByVal errors As clsErrorLog)

        ' Let's get all the single-component validation out of the way first...
        For Each comp As PackageComponent In rel
            Dim ws As WebServiceComponent = TryCast(comp, WebServiceComponent)
            If ws IsNot Nothing Then ws.ApplyResolutions(resolutions, errors)
        Next

        ' Cross component - again, we're checking names so the things to check for:
        ' 1) Renames don't clash with non-conflicting incoming names
        ' 2) Renames don't clash with other renames

        Dim matrix As New clsRenamingMatrix()
        Dim componentsByName As New Dictionary(Of String, WebServiceComponent)
        For Each res As ConflictResolution In resolutions
            Dim ws As WebServiceComponent = TryCast(res.Conflict.Component, WebServiceComponent)
            If ws IsNot Nothing Then
                matrix(ws.Name) = res.GetArgumentString("Web Service Name")
                componentsByName(ws.Name) = ws
            End If
        Next

        For Each c As clsRenamingMatrix.Clash In matrix.Validate()
            Dim comp As WebServiceComponent = componentsByName(c.Source)

            ' Each clash is in the form of :
            ' c.Source : The source (original) name of the component which clashed
            ' c.OriginalName : The original name of the component it clashed with
            ' c.NewName : The new name of the component it clashed with - may be null
            If c.NewName Is Nothing Then
                errors.Add(comp,
                 My.Resources.WebServiceComponent_TheWebService0CannotBeRenamedTo1ThereIsAWebServiceInThisReleaseWithThatName,
                 c.Source, c.OriginalName)

            Else
                errors.Add(comp,
                 My.Resources.WebServiceComponent_TheIncomingWebServices0And1CannotBothBeRenamedTo2,
                 c.Source, c.OriginalName, c.NewName)

            End If
        Next

    End Sub

    ''' <summary>
    ''' Applies the given resolutions to this component.
    ''' </summary>
    ''' <param name="resolutions">The resolutions to check</param>
    ''' <param name="errors">The error log to append any errors for this component
    ''' due to the specified conflict resolution.</param>
    Private Sub ApplyResolutions(
     ByVal resolutions As ICollection(Of ConflictResolution), ByVal errors As clsErrorLog)

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
                 My.Resources.WebServiceComponent_YouMustChooseHowToHandleTheConflictingNameForWebService0, Name)
                ' Move onto the next resolution - without an option there's little we can
                ' do for this one.
                Continue For
            End If

            Dim err As clsError = ApplyResolution(res, mods)
            If err Is Nothing Then res.Passed = True Else errors.Add(err)

        Next

    End Sub

    ''' <summary>
    ''' Applies the given resolution to this component, adding any modifications to
    ''' the given map and returning an error as appopriate.
    ''' </summary>
    ''' <param name="res">The conflict resolution to apply</param>
    ''' <param name="mods">The modifications to add as a result of applying the
    ''' given resolution.</param>
    ''' <returns>The error which this resolution has evoked, or null if it applied
    ''' without any errors</returns>
    Private Function ApplyResolution(
     ByVal res As ConflictResolution, ByVal mods As IDictionary(Of ModificationType, Object)) _
     As clsError

        ' The definition of that conflict
        Dim defn As ConflictDefinition = res.Conflict.Definition

        ' The resolution option chosen (may be null if no option is chosen)
        Dim opt As ConflictOption = res.ConflictOption

        ' If the option is non-null, that's a programming error, it should have
        ' been checked by now.
        If opt Is Nothing Then Throw New ArgumentNullException("res.ConflictOption")

        ' The proposed name - null if not applicable
        Dim proposedName As String = res.GetArgumentString("Web Service Name")

        Select Case opt.Choice
            Case ConflictOption.UserChoice.Overwrite
                mods(ModificationType.OverwritingExisting) = True

            Case ConflictOption.UserChoice.Rename
                If gSv.GetWebServiceId(proposedName) <> Guid.Empty Then
                    Return New clsError(Me,
                     My.Resources.WebServiceComponent_YouCannotRenameTheIncomingWebService0To1ThatNameIsAlreadyInUseInThisEnvironment, Name, proposedName)
                End If

                mods(ModificationType.IncomingName) = proposedName

            Case ConflictOption.UserChoice.RenameExisting
                If gSv.GetWebServiceId(proposedName) <> Guid.Empty Then
                    Return New clsError(Me,
                     My.Resources.WebServiceComponent_YouCannotRenameTheExistingWebService0To1ThatNameIsAlreadyInUseInThisEnvironment, Name, proposedName)
                End If

                mods(ModificationType.ExistingName) = proposedName

        End Select

        Return Nothing
    End Function

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.GetWebServiceDefinition(IdAsGuid)
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the data in the
    ''' given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True
        Dim ws As clsWebServiceDetails = AssociatedWebService
        If ws Is Nothing Then Throw New NoSuchElementException(
         My.Resources.WebServiceComponent_NoWebServiceWithTheID0WasFound, Id)

        Dim compws As clsWebServiceDetails = _
         DirectCast(comp, WebServiceComponent).AssociatedWebService

        Return (ws.URL <> compws.URL OrElse ws.Timeout <> compws.Timeout _
         OrElse ws.GetSettings() <> compws.GetSettings() _
         OrElse Normalise(ws.WSDL) <> Normalise(compws.WSDL))

    End Function

    ''' <summary>
    ''' Replaces any CRLF char pairs in the given XML with a single LF char so that
    ''' the XML ends up 'normalised'. MS XML Writer does this automatically, so that
    ''' a written WSDL may end up different to a stored WSDL, even though it should
    ''' be identical.
    ''' </summary>
    ''' <param name="xml">The XML to normalise</param>
    ''' <returns>The normalised XML, or null if null was given.</returns>
    Private Function Normalise(ByVal xml As String) As String
        If xml Is Nothing Then Return Nothing
        Return xml.Replace(vbCrLf, vbLf)
    End Function

#End Region

End Class
