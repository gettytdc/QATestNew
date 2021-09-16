Imports System.Linq
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling

Namespace WebApis.CustomCode

    ''' <summary>
    ''' Generates and invokes Web API <see cref="MethodType.OutputParameter"></see> custom code methods
    ''' </summary>
    Public Class OutputParameterMethod : Inherits CustomCodeMethodType

        Private Const DefaultResponseContentParameterName = "Response_Content"
        Public Overrides ReadOnly Property Type As MethodType = MethodType.OutputParameter
        Public Overrides ReadOnly Property MethodPrefix As String = "GenerateOutputParameters"

        Public Overrides Function GetParametersFromAction(action As WebApiAction, configuration As WebApiConfiguration) As IEnumerable(Of IParameter)
            Return action.
                        OutputParameterConfiguration.
                        Parameters.
                        OfType(Of CustomCodeOutputParameter).
                        OfType(Of IParameter)
        End Function

        Public Overrides Function GetAdditionalParameters(existingNames As IEnumerable(Of IParameter)) As IEnumerable(Of IParameter)
            Dim parameterName = GetResponseContentParameterName(existingNames)
            Return {New ResponseContentInputParameter(parameterName)}
        End Function

        Public Overrides Function GetCode(action As WebApiAction) As String
            Return action.OutputParameterConfiguration.Code
        End Function

        Public Shared Function GetResponseContentParameterName(existingParameters As IEnumerable(Of IParameter)) As String
            Return GetUniqueParameterName(DefaultResponseContentParameterName, existingParameters)
        End Function

        Public Overrides Function HasCode(action As WebApiAction) As Boolean
            Return action.OutputParameterConfiguration.Parameters.Any(Function(p) p.Type = OutputMethodType.CustomCode)
        End Function


        ''' <summary>
        ''' Invoke the custom code method that transforms the response of a Web API
        ''' Action HTTP request into custom output parameters
        ''' </summary>
        ''' <param name="codeBuilder">The builder used to the compile the assembly</param>
        ''' <param name="context">The context of the Web API action being executed</param>
        ''' <param name="responseBody">The body of the HTTP response to transform</param>
        ''' <returns>A collection of the custom code output parameters defined within 
        ''' the Web API configuration and their values calculated from the HTTP 
        ''' response
        ''' </returns>
        Public Function Invoke(codeBuilder As ICustomCodeBuilder,
                                context As ActionContext,
                                responseBody As String) As IEnumerable(Of clsArgument)


            Dim outputParameterConfiguration = context.Action.OutputParameterConfiguration

            Dim customCodeParameters = outputParameterConfiguration.
                    Parameters.
                    OfType(Of CustomCodeOutputParameter).
                    ToList()
            If Not customCodeParameters.Any() Then Return Enumerable.Empty(Of clsArgument)

            Dim assemblyData = codeBuilder.GetAssembly(context)
            Dim classDefinition = assemblyData.AssemblyDefinition.Classes.Single
            Dim methodDefinition = GetMethodDefinition(classDefinition, context.ActionName)

            Dim responseBodyParameter = methodDefinition.Parameters.SingleOrDefault(Function(m) m.IsOutput = False)
            Dim inputArgument = New clsArgument(responseBodyParameter.Name, responseBody)

            Dim outputArguments = methodDefinition.
                                        Parameters.
                                        Where(Function(p) p.IsOutput).
                                        Select(Function(p) New clsArgument(p.Name, New clsProcessValue(p.DataType))).
                                        ToArray()

            Dim returnValues = InvokeMethod(assemblyData, context.ActionName, {inputArgument}, outputArguments)
            Dim customCodeParameterNames = customCodeParameters.Select(Function(p) p.Name)

            Dim outputParameters = returnValues.
                                        Select(Function(p) New clsArgument(FindOutputParameterName(p.Name, customCodeParameterNames),
                                                                           p.Value))
            Return outputParameters
        End Function

        ''' <summary>
        ''' Find the correct output parameter name in a collection, based on the 
        ''' a parameter identifier
        ''' </summary>
        Private Function FindOutputParameterName(codeIdentifier As String, parameterNames As IEnumerable(Of String)) As String
            Return parameterNames.SingleOrDefault(Function(p) CodeCompiler.GetIdentifier(p) = codeIdentifier)
        End Function

    End Class
End Namespace
