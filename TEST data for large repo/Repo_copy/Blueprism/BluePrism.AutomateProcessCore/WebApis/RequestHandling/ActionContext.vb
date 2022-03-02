Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Contains data used during creation and execution of a web request based on a 
    ''' Web API action
    ''' </summary>
    Public Class ActionContext

        ''' <summary>
        ''' Creates a new ActionContext
        ''' </summary>
        ''' <param name="configuration">The configuration of the Web API</param>
        ''' <param name="actionName">The name of the action</param>
        ''' <param name="parameters">Parameter values supplied for the action</param>
        Public Sub New(webApiId As Guid,
                       configuration As WebApiConfiguration,
                       actionName As String,
                       parameters As Dictionary(Of String, clsProcessValue),
                       session As clsSession)

            If configuration Is Nothing Then
                Throw New ArgumentNullException(NameOf(configuration))
            End If
            If String.IsNullOrWhiteSpace(actionName) Then
                Throw New ArgumentException(My.Resources.Resources.ActionContext_InvalidName, NameOf(actionName))
            End If
            If parameters Is Nothing Then
                Throw New ArgumentNullException(NameOf(parameters))
            End If

            Me.WebApiId = webApiId
            Me.Configuration = configuration
            Me.ActionName = actionName
            Me.Parameters = parameters
            Me.SessionId = session.ID
            Me.SessionWebConnectionSettings = session.WebConnectionSettings

            Action = configuration.GetAction(actionName)
            If Action Is Nothing Then _
                Throw New ArgumentException(String.Format(My.Resources.Resources.ActionContext_UnknownAction0, actionName), NameOf(actionName))

        End Sub

        ''' <summary>
        ''' The id of the Web API to which the action belongs
        ''' </summary>
        Public ReadOnly Property WebApiId As Guid

        ''' <summary>
        ''' The configuration for the Web API to which the action belongs
        ''' </summary>
        Public ReadOnly Property Configuration As WebApiConfiguration

        ''' <summary>
        ''' The action for the Web API to which the action belongs
        ''' </summary>
        Public ReadOnly Property Action As WebApiAction

        ''' <summary>
        ''' The name of the action
        ''' </summary>
        Public ReadOnly Property ActionName As String

        ''' <summary>
        ''' Parameter values supplied for the action
        ''' </summary>
        Public Property Parameters As Dictionary(Of String, clsProcessValue)

        ''' <summary>
        ''' The Id of the session being used to execute the action, required to check
        ''' credential access for authentication.
        ''' </summary>
        ''' <returns></returns>
        Public Property SessionId As Guid


        ''' <summary>
        ''' The WebConnectionSettings retrieved for the session being used to execute 
        ''' the action.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SessionWebConnectionSettings As WebConnectionSettings


    End Class


End Namespace