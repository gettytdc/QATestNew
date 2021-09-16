Imports System.Linq

Namespace WebApis

    ''' <summary>
    ''' Class that handles the creation of Parameters and output arguments as they
    ''' appear in Process Studio.
    ''' </summary>
    Public Class WebApiParameterHandler

        Private ReadOnly mConfiguration As WebApiConfiguration

        ''' <summary>
        ''' Creates a new instance of WebApiParameterHandler.
        ''' </summary>
        ''' <param name="configuration">The configuration instance containing information
        ''' about the non standard parameters. </param>
        Public Sub New(configuration As WebApiConfiguration)
            mConfiguration = configuration
        End Sub

        ''' <summary>
        ''' Retrieves a readonly collection of parameters applicable to the WebApiAction
        ''' </summary>
        ''' <param name="action">The action with which to get the list of parameters. </param>
        ''' <returns>A readonly collection of clsProcessParameter. </returns>
        Public Function GetParametersForAction(action As WebApiAction) As IReadOnlyCollection(Of clsProcessParameter)
            Dim outputParametres = GetStandardOutputParameters.
                Concat(GetCustomOutputParameters(action)).ToList()

            If action.EnableRequestDataOutputParameter Then _
                outputParametres.Add(CreateRequestContentOutputParameter())

            Dim inputParameters = GetActionParameters(action)
            Return outputParametres.Concat(inputParameters).ToList()
        End Function

        Private Function CreateRequestContentOutputParameter() As clsProcessParameter
            Return New clsProcessParameter(OutputParameters.RequestData,
                                           DataType.text,
                                           ParamDirection.Out)
        End Function

        ''' <summary>
        ''' Gets the action-specific parameters from the WebApiAction instance.
        ''' </summary>
        ''' <param name="action">The WebApiAction instance containing the parameters. </param>
        ''' <returns>A collection of clsProcessParameters. </returns>
        Private Function GetActionParameters(action As WebApiAction) As IEnumerable(Of clsProcessParameter)
            Return action.Parameters.
                               Concat(mConfiguration.
                                        CommonParameters).
                               Concat(mConfiguration.
                                        CommonAuthentication.
                                        GetInputParameters).
                               Concat(action.
                                        Request.
                                        BodyContent.
                                        GetInputParameters()).
                               Where(Function(param) param.ExposeToProcess).
                               Select(Function(param) GetNewParameter(param))
        End Function

        ''' <summary>
        ''' Creates a new clsProcessParameter instance using data from the 
        ''' action parameter.
        ''' </summary>
        ''' <param name="actionParam">The action parameter with properties mapping
        ''' to the clsProcessParameter. </param>
        ''' <returns>A new clsProcessParameter instance. </returns>
        Private Function GetNewParameter(actionParam As ActionParameter) As clsProcessParameter
            Dim parameter = New clsProcessParameter(actionParam.Name, actionParam.DataType, ParamDirection.In, actionParam.Description)

            Dim collectionActionParameter = TryCast(actionParam, ActionParameterWithCollection)
            If collectionActionParameter IsNot Nothing Then _
                parameter.CollectionInfo = collectionActionParameter.CollectionInfo

            Return parameter

        End Function

        ''' <summary>
        ''' Returns the standard output parameters applicable to all WebApiBusinessObjectAction
        ''' instances.
        ''' </summary>
        ''' <returns>An IEnumerable of clsProcessParameter. </returns>
        Private Iterator Function GetStandardOutputParameters() As IEnumerable(Of clsProcessParameter)

            Yield New clsProcessParameter(OutputParameters.ResponseContent, DataType.text, ParamDirection.Out)
            Yield New clsProcessParameter(OutputParameters.StatusCode, DataType.text, ParamDirection.Out)
            Yield New clsProcessParameter(OutputParameters.ResponseHeaders, DataType.collection, ParamDirection.Out)

        End Function

        Private Function GetCustomOutputParameters(action As WebApiAction) As IEnumerable(Of clsProcessParameter)
            Return action.
                        OutputParameterConfiguration.
                        Parameters.
                        Select(Function(p) New clsProcessParameter(p.Name, p.DataType, ParamDirection.Out, p.Description))
        End Function

    End Class
End Namespace

