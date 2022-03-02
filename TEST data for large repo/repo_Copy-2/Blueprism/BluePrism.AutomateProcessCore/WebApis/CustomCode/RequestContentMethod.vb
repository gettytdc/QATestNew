Imports System.Linq
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling

Namespace WebApis.CustomCode

    ''' <summary>
    ''' Generates and invokes Web API <see cref="MethodType.RequestContent"></see> custom code methods
    ''' </summary>
    Public Class RequestContentMethod : Inherits CustomCodeMethodType

        Private Const defaultParameterName = "Request_Content"

        Public Overrides ReadOnly Property Type As MethodType = MethodType.RequestContent
        Public Overrides ReadOnly Property MethodPrefix As String = "GenerateRequestContent"

        Public Overrides Function GetParametersFromAction(action As WebApiAction, configuration As WebApiConfiguration) As IEnumerable(Of IParameter)
            Return configuration.
                        CommonParameters.
                        Concat(action.Parameters).
                        Where(Function(p) p.ExposeToProcess).
                        OfType(Of IParameter)
        End Function

        Public Overrides Function GetCode(action As WebApiAction) As String
            Return TryCast(action.Request.BodyContent, BodyContent.CustomCodeBodyContent)?.Code
        End Function

        Public Overrides Function GetAdditionalParameters(existingParameters As IEnumerable(Of IParameter)) As IEnumerable(Of IParameter)
            Dim uniqueParameterName = GetUniqueParameterName(defaultParameterName, existingParameters)
            Return {New RequestContentOutputParameter(uniqueParameterName)}
        End Function

        Public Overrides Function HasCode(action As WebApiAction) As Boolean
            Return action.Request.BodyContent.Type = WebApiRequestBodyType.CustomCode
        End Function

        ''' <summary>
        ''' Invoke the custom code method that uses custom code to generate the 
        ''' body of a Web APi Action Http Request
        ''' </summary>
        ''' <param name="assemblyData">The compiled assembly that contains the method</param>
        ''' <param name="context">The context of the Web API action being executed</param>
        ''' <returns>The calculated request body content</returns>
        Public Function Invoke(assemblyData As AssemblyData,
                               context As ActionContext) As String


            Dim assemblyDefinition = assemblyData.AssemblyDefinition
            Dim assembly = assemblyData.Assembly

            Dim classDefinition = assemblyDefinition.Classes.Single()
            Dim methodDefinition = GetMethodDefinition(classDefinition, context.ActionName)

            Dim parameterValues = context.Parameters

            Dim inputArguments = methodDefinition.
                                        Parameters.
                                        Where(Function(p) Not p.IsOutput).
                                        Select(Function(p) New clsArgument(p.Name, parameterValues.
                                                                                        FirstOrDefault(Function(x) CodeCompiler.GetIdentifier(x.Key) = p.Name).
                                                                                        Value)).
                                        ToArray()

            Dim requestContentOutputParameter = methodDefinition.Parameters.SingleOrDefault(Function(p) p.IsOutput)
            Dim outputArgument = New clsArgument(requestContentOutputParameter.Name, New clsProcessValue(requestContentOutputParameter.DataType))

            Dim methodArguments = InvokeMethod(assemblyData, context.ActionName, inputArguments, {outputArgument})
            Dim requestContent = methodArguments.SingleOrDefault()?.Value?.FormattedValue

            Return requestContent


        End Function

    End Class

End Namespace
