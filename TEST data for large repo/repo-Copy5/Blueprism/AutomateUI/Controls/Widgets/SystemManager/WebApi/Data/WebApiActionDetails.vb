Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class to represent a mutable WebApi action
''' </summary>
Friend Class WebApiActionDetails

    ''' <summary>
    ''' Create a new instance of the mutable Web API action class. In this
    ''' constructor the Id is set to 0, which indicates that it is a new action.
    ''' </summary>
    Public Sub New(api As WebApiDetails)
        Me.New(api, 0)
    End Sub

    ''' <summary>
    ''' Create a new instance of the mutable Web API action class.
    ''' </summary>
    ''' <param name="actionId">The id of Action</param>
    Public Sub New(api As WebApiDetails, actionId As Integer)
        Me.Api = If(api, New WebApiDetails())
        Id = actionId
        Request = New WebApiActionRequestDetails(Me)
        Response = New WebApiActionResponseDetails(Me)
    End Sub

    ''' <summary>
    ''' Gets the id of the action. Should be 0 for a record not yet saved to
    ''' the db
    ''' </summary>
    ReadOnly Property Id As Integer

    ''' <summary>
    ''' Gets the Api details for the Api this action belongs to
    ''' </summary>
    ReadOnly Property Api As WebApiDetails

    ''' <summary>
    ''' Gets or sets the name of the action
    ''' </summary>
    Property Name As String

    ''' <summary>
    ''' Gets or sets the description of the action
    ''' </summary>
    Property Description As String

    ''' <summary>
    ''' Gets or sets whether this action is enabled or not.
    ''' </summary>
    Property Enabled As Boolean = True

    ''' <summary>
    ''' Gets or sets whether the additional output parameter 
    ''' containing the request information will be generated or not
    ''' </summary>
    Property EnableRequestDataOutputParameter As Boolean

    ''' <summary>
    ''' Gets or sets whether the request should 
    ''' actually be sent or not
    ''' </summary>
    ''' <returns></returns>
    Property DisableSendingOfRequest As Boolean

    ''' <summary>
    ''' Gets or sets the Request details for the action.
    ''' </summary>
    Public ReadOnly Property Request As WebApiActionRequestDetails

    ''' <summary>
    ''' Gets or sets the Response details for the action.
    ''' </summary>
    Public ReadOnly Property Response As WebApiActionResponseDetails

    ''' <summary>
    ''' Gets the parameters associated with this action
    ''' </summary>
    ReadOnly Property Parameters As New WebApiCollection(Of ActionParameter) With {
    .ActionSpecific = True
}

    ''' <summary>
    ''' Adds a collection of parameters to this action and returns it.
    ''' </summary>
    ''' <param name="params">The parameters to add to this action.</param>
    ''' <returns>This action with the parameters added.</returns>
    Public Function WithParameters(params As IEnumerable(Of ActionParameter)) As WebApiActionDetails
        If params IsNot Nothing Then Parameters.AddAll(params)
        Return Me
    End Function

    ''' <summary>
    ''' Converts an instance of this class into a <see cref="WebApiAction"/>
    ''' </summary>
    ''' <param name="det">The details to convert</param>
    ''' <returns>A WebApiAction with the same value as the given details object.
    ''' </returns>
    Public Shared Function MapTo(det As WebApiActionDetails) As WebApiAction
        If det Is Nothing Then Return Nothing
        With det
            Return New WebApiAction(
            .Id,
            If(.Name, String.Empty),
            If(.Description, String.Empty),
            .Enabled,
            .EnableRequestDataOutputParameter,
            .DisableSendingOfRequest,
            New WebApiRequest(
                If(.Request.Method, HttpMethod.Get),
                If(.Request.UrlPath, String.Empty),
                .Request.Headers,
                .Request.BodyContent
            ),
        .Parameters,
        New OutputParameterConfiguration(.Response.CustomOutputParameters, .Response.Code)
    )
        End With
    End Function

    ''' <summary>
    ''' Converts an instance of the immutable WebApiAction class to a mutable
    ''' instance of this class.
    ''' </summary>
    ''' <param name="act">The action to convert to a mutable WebApiActionDetails
    ''' instance.</param>
    ''' <returns>The WebApiActionDetails instance with the same value as
    ''' <paramref name="act"/>.</returns>
    Public Shared Function CreateFrom(act As WebApiAction, api As WebApiDetails) As WebApiActionDetails
        If act Is Nothing Or api Is Nothing Then Return Nothing

        Dim actionDetails = New WebApiActionDetails(api, act.Id) With {
        .Name = act.Name,
        .Description = act.Description,
        .Enabled = act.Enabled,
        .EnableRequestDataOutputParameter = act.EnableRequestDataOutputParameter,
        .DisableSendingOfRequest = act.DisableSendingOfRequest
    }.WithParameters(act.Parameters)

        actionDetails.Request.UrlPath = act.Request.UrlPath
        actionDetails.Request.Method = act.Request.HttpMethod
        actionDetails.Request.BodyContent = act.Request.BodyContent
        actionDetails.Request.WithHeaders(act.Request.Headers)

        actionDetails.Response.WithParameters(act.OutputParameterConfiguration.Parameters)
        actionDetails.Response.Code = act.OutputParameterConfiguration.Code

        Return actionDetails
    End Function

End Class
