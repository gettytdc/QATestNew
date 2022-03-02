
Imports System.Linq
Imports Autofac
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.BPCoreLib.DependencyInjection

Namespace WebApis

    ''' <summary>
    ''' Class used in performing an action and requesting data through the HttpClient
    ''' which the user interacts with in Process Studio.
    ''' </summary>
    Public Class WebApiBusinessObjectAction
        Inherits clsBusinessObjectAction
        Implements IDiagnosticEmitter

        Private ReadOnly mConfiguration As WebApiConfiguration
        Private ReadOnly mParamHandler As WebApiParameterHandler
        Private ReadOnly mWebApiId As Guid
        Private ReadOnly mAction As WebApiAction

        ''' <summary>
        ''' Event raised with diagnostics messages
        ''' </summary>
        Public Event Diags(ByVal msg As String) Implements IDiagnosticEmitter.Diags

        Private Sub RaiseDiags(ByVal msg As String)
            RaiseEvent Diags(msg)
        End Sub

        ''' <summary>
        ''' Create a new instance of a Web API business object action
        ''' </summary>
        ''' <param name="action">The Web API action to create the business object 
        ''' action for</param>
        ''' <param name="webApiId">The ID of the web api the action belongs to</param>
        ''' <param name="configuration">The overall configuration of the Web API that 
        ''' the action belongs to</param>
        Public Sub New(action As WebApiAction,
                       webApiId As Guid,
                       configuration As WebApiConfiguration)

            mAction = action
            mWebApiId = webApiId
            mConfiguration = configuration
            mParamHandler = New WebApiParameterHandler(mConfiguration)
            SetName(action.Name)
            InitialiseParameters()
        End Sub

        ''' <summary>
        ''' Sets up the parameters for the action as they appear in Process Studio.
        ''' </summary>
        Private Sub InitialiseParameters()

            For Each parameter In mParamHandler.GetParametersForAction(mAction)
                AddParameter(parameter)
            Next

        End Sub

        ''' <summary>
        ''' Performs the web API service operation and sets the output parameters for
        ''' Process Studio, if the response was unsuccessful then the error is processed.
        ''' </summary>
        ''' <param name="inputs">The input values supplied to the action</param>
        ''' <param name="scopeStage">The session from the process containing the action 
        ''' stage</param>
        ''' <returns>A list of arguments containing the output values</returns>
        Public Function DoAction(inputs As clsArgumentList, scopeStage As clsProcessStage) As clsArgumentList
            ' Cache based on current session is registered in container lifetime scope
            Dim store = New SessionCacheStore(Function() scopeStage.Process?.Session)
            Dim cache As New ObjectCache(store)
            
            Dim parameters = GetActionParameterValues(inputs)
            
            Dim session = scopeStage.Process.Session
          
            Dim context = New ActionContext(mWebApiId, mConfiguration, GetName, parameters, session)

            Dim outputs = DependencyResolver.FromScope(
                Sub(b) b.RegisterInstance(cache).As(Of IObjectCache)(),
                Function(scope As ILifetimeScope)

                    Dim requester = scope.Resolve(Of IHttpRequester)
                    If TypeOf requester Is IDiagnosticEmitter Then _
                        AddHandler DirectCast(requester, IDiagnosticEmitter).Diags, AddressOf RaiseDiags

                    Using response = requester.GetResponse(context)
                        Try
                            Dim outputMapper = scope.Resolve(Of OutputParameterMapper)
                            Return outputMapper.CreateParameters(response, mAction.EnableRequestDataOutputParameter, context)
                        Catch ex As Exception
                            RaiseDiags($"Error creating output parameters: {ex.Message}")
                            Throw
                        End Try
                    End Using
                End Function
            )

            Return outputs

        End Function

        ''' <summary>
        ''' Takes the input arguments from Process Studio and returns a
        ''' dictionary for use in creating a web API context.
        ''' </summary>
        ''' <returns>A dictionary with the argument name as a key and
        ''' argument value as the value. </returns>
        Private Function GetActionParameterValues(inputArguments As clsArgumentList) _
            As Dictionary(Of String, clsProcessValue)

            Dim result = New Dictionary(Of String, clsProcessValue)

            Dim allParameters = mConfiguration.CommonParameters.
                                Concat(mAction.Parameters).
                                Concat(mConfiguration.
                                            CommonAuthentication.
                                            GetInputParameters()).
                                Concat(mAction.
                                        Request.
                                        BodyContent.
                                        GetInputParameters())

            For Each parameter In allParameters
                Dim value = parameter.InitialValue
                If parameter.ExposeToProcess Then
                    Dim input = inputArguments.FirstOrDefault(Function(i) i.Name = parameter.Name)
                    If input IsNot Nothing Then
                        value = input.Value
                    End If
                End If

                result(parameter.Name) = value
            Next

            Return result

        End Function

        ''' <inheritdoc/>       
        Public Overrides Function GetEndpoint() As String
            'We don't support endpoints for web API, so this is
            'blank.
            Return String.Empty
        End Function

        ''' <inheritdoc/>
        Public Overrides Function GetPreConditions() As Collection
            'We don't support preconditions for a web service, so this is
            'an empty collection.
            Return New Collection
        End Function

    End Class
End Namespace

