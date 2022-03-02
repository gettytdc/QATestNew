
Imports System.Collections.ObjectModel
Imports System.Linq
Imports BluePrism.Utilities.Functional
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.Compilation

Namespace WebApis

    ''' <summary>
    ''' Contains the functionality configured for a Web API. This includes common 
    ''' parameters, HTTP headers and the individual actions that can be requested.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    <KnownType(GetType(ReadOnlyCollection(Of ActionParameter)))>
    <KnownType(GetType(ReadOnlyCollection(Of WebApiAction)))>
    <KnownType(GetType(ReadOnlyCollection(Of HttpHeader)))>
    <KnownType(GetType(BasicAuthentication))>
    <KnownType(GetType(BearerTokenAuthentication))>
    <KnownType(GetType(EmptyAuthentication))>
    <KnownType(GetType(OAuth2ClientCredentialsAuthentication))>
    <KnownType(GetType(OAuth2JwtBearerTokenAuthentication))>
    <KnownType(GetType(CustomAuthentication))>
    Public Class WebApiConfiguration

        <DataMember>
        Private mBaseUrl As String

        <DataMember>
        Private mCommonRequestHeaders As IReadOnlyCollection(Of HttpHeader)

        <DataMember>
        Private mCommonParameters As IReadOnlyCollection(Of ActionParameter)

        <DataMember>
        Private mActions As IReadOnlyCollection(Of WebApiAction)

        <DataMember>
        Private mCommonAuthentication As IAuthentication

        <DataMember>
        Private mCommonCode As CodeProperties

        <DataMember>
        Private mConfigurationSettings As WebApiConfigurationSettings

        ''' <summary>
        ''' Creates a new WebApiConfiguration instance
        ''' </summary>
        ''' <param name="baseUrl">The base URL that is used by all child actions
        ''' </param>
        ''' <param name="commonHeaders">Common HTTP headers that apply to all actions
        ''' in this configuration</param>
        ''' <param name="commonParameters">Common parameters that apply to all
        ''' actions in this configuration</param>
        ''' <param name="commonCode">Common code properties that apply to all actions</param>
        ''' <param name="actions">The actions representing individual API calls that
        ''' have been configured</param>
        ''' <param name="commonAuthentication">The authentication used across all
        ''' Web API calls</param>
        Public Sub New(baseUrl As String, commonHeaders As IEnumerable(Of HttpHeader),
                       commonParameters As IEnumerable(Of ActionParameter),
                       commonCode As CodeProperties,
                       actions As IEnumerable(Of WebApiAction),
                       commonAuthentication As IAuthentication,
                       configurationSettings As WebApiConfigurationSettings)

            If baseUrl Is Nothing Then _
                Throw New ArgumentNullException(NameOf(baseUrl))
            If commonHeaders Is Nothing Then _
                Throw New ArgumentNullException(NameOf(commonHeaders))
            If commonParameters Is Nothing Then _
                Throw New ArgumentNullException(NameOf(commonParameters))
            If actions Is Nothing Then _
                Throw New ArgumentNullException(NameOf(actions))
            If commonAuthentication Is Nothing Then _
                Throw New ArgumentNullException(NameOf(commonAuthentication))
            If commonCode Is Nothing Then _
                Throw New ArgumentNullException(NameOf(commonCode))
            If configurationSettings Is Nothing Then _
                Throw New ArgumentNullException(NameOf(configurationSettings))

            mBaseUrl = baseUrl
            mCommonRequestHeaders = commonHeaders.ToList().AsReadOnly()
            mCommonCode = commonCode
            mConfigurationSettings = configurationSettings
            InitCommonParameters(
                commonParameters.ToList().AsReadOnly, NameOf(commonParameters))
            InitActions(actions.ToList().AsReadOnly, NameOf(actions))
            InitAuthentication(commonAuthentication, NameOf(commonAuthentication))
        End Sub

        Private Sub InitCommonParameters(parameters As IReadOnlyCollection(Of ActionParameter),
                                         paramName As String)
            Dim names = parameters.Select(Function(x) x.Name)
            Dim duplicateNames = GetDuplicateNames(names)
            If duplicateNames <> "" Then Throw New ArgumentException(
                String.Format(My.Resources.Resources.WebApiConfiguration_CommonParametersContainNonUniqueNames0, duplicateNames),
                paramName)
            mCommonParameters = parameters
        End Sub

        Private Sub InitActions(actions As IReadOnlyCollection(Of WebApiAction), paramName As String)
            Dim actionNames = actions.Select(Function(x) x.Name)
            Dim duplicateNames = GetDuplicateNames(actionNames)
            If duplicateNames <> "" Then Throw New ArgumentException(
                String.Format(My.Resources.Resources.WebApiConfiguration_ActionsContainNonUniqueNames0, duplicateNames), paramName)

            Dim invalidActions As New List(Of String)
            For Each action In actions
                Dim parameterNames = action.Parameters.Concat(CommonParameters).
                        Select(Function(p) p.Name)
                Dim duplicateParameterNames = GetDuplicateNames(parameterNames)
                If duplicateParameterNames <> "" Then
                    invalidActions.Add($"{action.Name} ({duplicateParameterNames})")
                End If
            Next
            If invalidActions.Any() Then Throw New ArgumentException(
                My.Resources.Resources.WebApiConfiguration_ActionsContainParametersWithNonUniqueNames &
                $"{String.Join(My.Resources.Resources.WebApiConfiguration_Comma, invalidActions)}", paramName)

            mActions = actions

        End Sub

        Private Sub InitAuthentication(auth As IAuthentication, paramName As String)
            ' Get distinct names of parameters used elsewhere (there's a separate 
            ' check for duplicates between action and common parameters). Then check
            ' if authentication parameters result in duplicates
            Dim otherParameterNames = Actions.
                    SelectMany(Function(a) a.Parameters).
                    Concat(CommonParameters).
                    Select(Function(p) p.Name).
                    Distinct()
            Dim authenticationParameterNames = auth.GetInputParameters().Select(Function(p) p.Name)
            Dim allParameterNames = otherParameterNames.Concat(authenticationParameterNames)
            Dim duplicateNames = GetDuplicateNames(allParameterNames)
            If duplicateNames <> "" Then Throw New ArgumentException(
                String.Format(My.Resources.Resources.WebApiConfiguration_AuthenticationParametersContainNonUniqueNames0, duplicateNames),
                paramName)

            mCommonAuthentication = auth
        End Sub

        ''' <summary>
        ''' Gets list of duplicate names from a sequence in a format suitable for use 
        ''' in exception message
        ''' </summary>
        ''' <param name="names">The sequence of names</param>
        ''' <returns>A comma-separated string containing all the duplicate names
        ''' found in the sequence, or an empty string if no duplicates were found.
        ''' </returns>
        ''' <remarks>Names are deemed to be duplicate if they both are converted
        ''' to the same identifier by the code compiler. This restriction will prevent
        ''' code compilation errors when using custom code methods</remarks>
        Public Shared Function GetDuplicateNames(names As IEnumerable(Of String)) As String

            Dim duplicates = names.
                                Select(Function(name) New With {.Identifier = CodeCompiler.GetIdentifier(name), .ActualName = name}).
                                GroupBy(Function(x) x.Identifier).
                                Where(Function(g) g.Count > 1).
                                SelectMany(Function(g) g.Select(Function(x) x.ActualName).Distinct())

            Return String.Join(My.Resources.Resources.WebApiConfiguration_Comma, duplicates)

        End Function

        ''' <summary>
        ''' Authentication configuration shared by all actions in this configuration
        ''' </summary>
        Public ReadOnly Property CommonAuthentication As IAuthentication
            Get
                Return mCommonAuthentication
            End Get
        End Property

        ''' <summary>
        ''' Common code properties shared by all action in this configuration
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CommonCode As CodeProperties
            Get
                Return mCommonCode
            End Get
        End Property

        ''' <summary>
        ''' The base URL that is used by all child actions
        ''' </summary>
        Public ReadOnly Property BaseUrl As String
            Get
                Return mBaseUrl
            End Get
        End Property

        ''' <summary>
        ''' Common HTTP headers that apply to all actions in this configuration
        ''' </summary>
        Public ReadOnly Property CommonRequestHeaders As IReadOnlyCollection(Of HttpHeader)
            Get
                Return mCommonRequestHeaders
            End Get
        End Property

        ''' <summary>
        ''' Common parameters that apply to all actions in this configuration
        ''' </summary>
        Public ReadOnly Property CommonParameters As IReadOnlyCollection(Of ActionParameter)
            Get
                Return mCommonParameters
            End Get
        End Property

        ''' <summary>
        ''' The actions representing individual API calls that have been configured
        ''' </summary>
        Public ReadOnly Property Actions As IReadOnlyCollection(Of WebApiAction)
            Get
                Return mActions
            End Get
        End Property

        ''' <summary>
        ''' The configuration settings for this web api
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ConfigurationSettings As WebApiConfigurationSettings
            Get
                Return mConfigurationSettings
            End Get
        End Property

        ''' <summary>
        ''' Gets the action with the specified name
        ''' </summary>
        ''' <param name="name">The name of the action</param>
        ''' <returns>The action instance or null if the action was not found</returns>
        Public Function GetAction(name As String) As WebApiAction
            Return Actions.FirstOrDefault(Function(a) a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        End Function

        ''' <summary>
        ''' Serializes the current instance to an XML string
        ''' </summary>
        ''' <returns>Returns an XML string</returns>
        Public Function ToXml() As String
            Return ToXElement().ToString()
        End Function

        ''' <summary>
        ''' Deserializes an XML string into a new instance of <see cref="WebApiConfiguration"/>
        ''' </summary>
        ''' <returns>Returns a new instance of <see cref="WebApiConfiguration"/></returns>
        Public Shared Function FromXml(xml As String) As WebApiConfiguration
            Return FromXElement(XElement.Parse(xml))
        End Function


        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="WebApiConfiguration"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Function ToXElement() As XElement

            Return _
                <configuration baseurl=<%= BaseUrl %>>
                    <actions><%= From a In Actions Select x = a.ToXElement() %></actions>
                    <commonparameters><%= From p In CommonParameters Select x = p.ToXElement() %></commonparameters>
                    <commonheaders><%= From h In CommonRequestHeaders Select x = h.ToXElement() %></commonheaders>
                    <commonauthentications><%= CommonAuthentication.ToXElement() %></commonauthentications>
                    <commoncode><%= CommonCode.ToXElement() %></commoncode>
                    <configurationsettings><%= ConfigurationSettings.ToXElement() %></configurationsettings>
                </configuration>


        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="WebApiConfiguration"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="WebApiConfiguration"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As WebApiConfiguration

            If Not element.Name.LocalName.Equals("configuration") Then _
                Throw New MissingXmlObjectException("configuration")

            Dim baseUrl = element.Attribute("baseurl")?.Value() 
            If baseUrl Is Nothing Then Throw New MissingXmlObjectException("baseurl")
            
            Dim headers = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "commonheaders")?.
                            Elements.
                            Where(Function(x) x.Name = "httpheader").
                            Select(Function(x) HttpHeader.FromXElement(x))

            If headers Is Nothing Then Throw New MissingXmlObjectException("commonheaders")


            Dim params = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "commonparameters")?.
                            Elements.
                            Where(Function(x) x.Name = "actionparameter").
                            Select(Function(x) ActionParameter.FromXElement(x))

            If params Is Nothing Then Throw New MissingXmlObjectException("commonparameters")


            Dim actions = element.
                            Elements.
                            FirstOrDefault(Function(x) x.Name = "actions")?.
                            Elements.
                            Where(Function(x) x.Name = "action").
                            Select(Function(x) WebApiAction.FromXElement(x))

            If actions Is Nothing Then Throw New MissingXmlObjectException("actions")


            Dim commonAuthentication = element.
                                    Elements.
                                    FirstOrDefault(Function(x) x.Name = "commonauthentications")?.
                                    Elements().
                                    FirstOrDefault().
                                    Map(Function(x) AuthenticationDeserializer.Deserialize(x))

            If commonAuthentication Is Nothing Then _
                Throw New MissingXmlObjectException("commonauthentications")

            Dim commonCode = element.
                                    Elements.
                                    FirstOrDefault(Function(x) x.Name = "commoncode")?.
                                    Elements().
                                    FirstOrDefault().
                                    Map(Function(x) CodeProperties.FromXElement(x))

            If commonCode Is Nothing Then _
                Throw New MissingXmlObjectException("commonCode")

            Dim configurationSettings = element.
                                    Elements.
                                    FirstOrDefault(Function(x) x.Name = "configurationsettings")?.
                                    Elements().
                                    FirstOrDefault().
                                    Map(Function(x) WebApiConfigurationSettings.FromXElement(x))

            If configurationSettings Is Nothing Then _
                Throw New MissingXmlObjectException("configurationsettings")

            Return New WebApiConfiguration(baseUrl, headers, params, commonCode, actions, commonAuthentication, configurationSettings)

        End Function

    End Class
End Namespace
