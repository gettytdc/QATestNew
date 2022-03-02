Imports System.Net.Http
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Class to represent a Web API which provides mutable properties.
''' </summary>
Friend Class WebApiDetails

    ''' <summary>
    ''' Create a new instance of the mutable Web API class. In this constructor the
    ''' Id is set to an empty guid, which indicates that it is a new Web API.
    ''' </summary>
    Public Sub New()
        Me.New(Guid.Empty)
    End Sub

    ''' <summary>
    ''' Create a new instance of the mutable Web API class.
    ''' </summary>
    ''' <param name="serviceId">The id of the Web API</param>
    Public Sub New(serviceId As Guid)
        Id = serviceId
    End Sub
    ''' <summary>
    ''' Gets or sets the ID of the Web API as held on the database.
    ''' </summary>
    ReadOnly Property Id As Guid

    ''' <summary>
    ''' Gets or sets the name of the Web Api within Blue Prism.
    ''' </summary>
    Property Name As String

    ''' <summary>
    ''' Gets or sets the Base URL of the Web API in string form
    ''' </summary>
    Property BaseUrl As String

    ''' <summary>
    ''' Gets or sets whether this Web API is enabled or not.
    ''' </summary>
    Property Enabled As Boolean = True

    ''' <summary>
    ''' The Authentication details used across the Web Api Actions.
    ''' </summary>
    Property CommonAuthentication As AuthenticationWrapper

    ''' <summary>
    ''' CommonCode shared across actions
    ''' </summary>
    Property CommonCode As CodePropertiesDetails

    ''' <summary>
    ''' Gets the collection of actions associated with this Web API
    ''' </summary>
    ReadOnly Property Actions As ICollection(Of WebApiActionDetails) =
        New List(Of WebApiActionDetails)

    ''' <summary>
    ''' Gets the collection of headers common to all actions within this Web API
    ''' </summary>
    ReadOnly Property CommonHeaders As New WebApiCollection(Of HttpHeader)

    ''' <summary>
    ''' Gets the collection of parameters common to all actions within this Web API
    ''' </summary>
    ReadOnly Property CommonParameters As New WebApiCollection(Of ActionParameter)

    ''' <summary>
    ''' Settings shared by actions
    ''' </summary>
    ''' <returns></returns>
    Property ConfigurationSettings As WebApiConfigurationSettingsDetails = WebApiConfigurationSettingsDetails.InitialiseWithDefaults()

    ''' <summary>
    ''' Gets or sets the Base URL of the Web API in string form.
    ''' </summary>
    ''' <exception cref="InvalidArgumentException">When setting the API via this
    ''' property with an invalid URI string, as specified in the
    ''' <see cref="Uri"/> class.</exception>
    Property BaseUrlString As String
        Get
            Return BaseUrl
        End Get
        Set(value As String)
            BaseUrl = If(value = "", Nothing, value)
        End Set
    End Property

    ''' <summary>
    ''' Adds the given action to this details object and returns it
    ''' </summary>
    ''' <param name="act">The action to add to this Web API</param>
    ''' <returns>This details object after the action is added</returns>
    ''' <exception cref="DuplicateException">If an action with the same name as
    ''' <paramref name="act"/> exists within this web api.</exception>
    Public Function WithAction(
     act As WebApiActionDetails) As WebApiDetails
        If Actions.Any(Function(a) a.Name.Equals(
         act.Name, StringComparison.CurrentCultureIgnoreCase)) Then
            Throw New DuplicateException(
                WebApi_Resources.ErrorDuplicateActionName_Template, act.Name)
        End If
        Actions.Add(act)
        Return Me
    End Function

    ''' <summary>
    ''' Converts a mutable WebAPI instance into an immutable <see cref="WebApi"/>
    ''' which is used for the rest of the product.
    ''' </summary>
    ''' <param name="details">The details object to convert</param>
    ''' <returns>A <see cref="WebApi"/> object with the same value as 
    ''' <paramref name="details"/>. If the argument is null, this will return null.
    ''' </returns>
    Public Shared Widening Operator CType(details As WebApiDetails) As WebApi
        If details Is Nothing Then Return Nothing
        With details
            Return New WebApi(
                .Id,
                .Name,
                .Enabled,
                New WebApiConfiguration(
                    .BaseUrl,
                    .CommonHeaders.
                        Where(Function(h) h.Name <> ""),
                    .CommonParameters.
                        Where(Function(param) param.Name <> ""),
                    .CommonCode,
                    .Actions.Select(Function(a) WebApiActionDetails.MapTo(a)),
                    .CommonAuthentication.Authentication,
                    WebApiConfigurationSettingsDetails.MapTo(details.ConfigurationSettings))
                )
        End With
    End Operator

    ''' <summary>
    ''' Converts an immutable WebApi instance to a mutable <see cref="WebApiDetails"/>
    ''' instance for use within the UI.
    ''' </summary>
    ''' <param name="api">The API object to convert</param>
    ''' <returns>The <see cref="WebApiDetails"/> object with the same value as
    ''' <paramref name="api"/>. If <paramref name="api"/> is null, this will return
    ''' null.</returns>
    Public Shared Widening Operator CType(api As WebApi) As WebApiDetails
        If api Is Nothing Then Return Nothing
        Dim webApiDetails = New WebApiDetails(api.Id) With {
            .Name = api.Name,
            .BaseUrl = api.Configuration.BaseUrl,
            .Enabled = api.Enabled,
            .CommonCode = api.Configuration.CommonCode
        }
        webApiDetails.Actions.AddAll(
         api.Configuration.Actions.Select(Function(a) WebApiActionDetails.CreateFrom(a, webApiDetails)))
        webApiDetails.CommonHeaders.AddAll(api.Configuration.CommonRequestHeaders)
        webApiDetails.CommonParameters.AddAll(api.Configuration.CommonParameters)
        webApiDetails.CommonAuthentication = New AuthenticationWrapper(api.Configuration.CommonAuthentication)
        webApiDetails.ConfigurationSettings = WebApiConfigurationSettingsDetails.CreateFrom(api.Configuration.ConfigurationSettings)

        Return webApiDetails

    End Operator
    Private Shared ReadOnly Property DefaultNameSpaces As IEnumerable(Of String) = {"System", "System.Drawing", "System.Data"}
    Private Shared ReadOnly Property DefaultImports As IEnumerable(Of String) = {"System.dll", "System.Data.dll", "System.Xml.dll", "System.Drawing.dll"}
    Public Shared Function CreateNewInstanceWithAction(name As String) As WebApiDetails

        Dim language = BluePrism.AutomateProcessCore.Compilation.CodeLanguage.VisualBasic
        Dim commonCode = New CodePropertiesDetails("", language, DefaultNameSpaces, DefaultImports)

        Dim result = New WebApiDetails() With {
                .Name = name,
                .BaseUrlString = "http://www.example.com/",
                .Enabled = True,
                .CommonAuthentication = New AuthenticationWrapper(New EmptyAuthentication()),
                .CommonCode = commonCode
        }

        Dim action = New WebApiActionDetails(result) With {
                    .Name = WebApi_Resources.NewActionName_Template.Replace("{0}", "").Trim(),
                    .Enabled = True
                    }

        action.Request.UrlPath = "/"
        action.Request.BodyContent = New NoBodyContent()
        action.Request.Method = HttpMethod.Get

        Return result.WithAction(action)
    End Function

End Class
