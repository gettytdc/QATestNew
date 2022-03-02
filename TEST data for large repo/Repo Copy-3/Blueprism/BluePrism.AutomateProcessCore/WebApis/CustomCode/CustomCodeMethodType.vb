Imports System.Linq
Imports System.Reflection
Imports BluePrism.AutomateProcessCore.Compilation

Namespace WebApis.CustomCode

    Public MustInherit Class CustomCodeMethodType

        Public Shared ReadOnly Property OutputParameters As OutputParameterMethod = New OutputParameterMethod()

        Public Shared ReadOnly Property RequestContent As RequestContentMethod = New RequestContentMethod()

        Public Shared ReadOnly Property All As IEnumerable(Of CustomCodeMethodType)
            Get
                Return {OutputParameters, RequestContent}
            End Get
        End Property

        Public MustOverride ReadOnly Property MethodPrefix As String

        Public MustOverride ReadOnly Property Type As MethodType

        Public MustOverride Function GetParametersFromAction(action As WebApiAction,
                                                             configuration As WebApiConfiguration) _
                                                             As IEnumerable(Of IParameter)
        Public MustOverride Function GetAdditionalParameters(existingParameters As IEnumerable(Of IParameter)) As IEnumerable(Of IParameter)

        Public MustOverride Function HasCode(action As WebApiAction) As Boolean

        Public MustOverride Function GetCode(action As WebApiAction) As String

        Protected Shared Function GetUniqueParameterName(currentParameter As String,
                                                  existingParameters As IEnumerable(Of IParameter)) As String
            Dim nextName = CodeCompiler.GetIdentifier(currentParameter)
            Dim nextNumber = 1
            While existingParameters.Select(Function(p) CodeCompiler.GetIdentifier(p.Name)).Contains(nextName)
                nextName = $"{currentParameter}_{nextNumber}"
                nextNumber = nextNumber + 1
            End While
            Return nextName
        End Function

        Public Iterator Function CreateMethods(configuration As WebApiConfiguration) As IEnumerable(Of MethodDefinition)

            For Each action In configuration.Actions.Where(Function(a) HasCode(a))

                Dim methodName = GetMethodName(action.Name)
                Dim code = GetCode(action)

                Dim actionParameters = GetParametersFromAction(action, configuration)
                Dim additionalParameters = GetAdditionalParameters(actionParameters)

                Dim methodDefinitionParameters = CreateMethodDefinitionParameters(actionParameters, additionalParameters)

                Yield New MethodDefinition(methodName, methodName, code, methodDefinitionParameters)

            Next

        End Function

        Protected Function InvokeMethod(assemblyData As AssemblyData, actionName As String,
                                     inputArguments() As clsArgument, outputArguments() As clsArgument) _
                                     As IEnumerable(Of clsArgument)

            Dim classDefinition = assemblyData.AssemblyDefinition.Classes.Single
            Dim methodDefinition = GetMethodDefinition(classDefinition, actionName)

            Dim type = assemblyData.Assembly.GetType(classDefinition.Identifier)
            Dim target = Activator.CreateInstance(type)

            Try
                MethodInvoker.InvokeMethod(target, methodDefinition.Identifier, inputArguments, outputArguments)
            Catch ex As TargetInvocationException
                Dim template = WebApiResources.CustomCodeExecutionErrorTemplate
                Throw New InvalidCodeException(String.Format(template, actionName, ex.InnerException.Message))
            Catch ex As Exception
                Dim template = WebApiResources.CustomCodeExecutionErrorTemplate
                Throw New InvalidCodeException(String.Format(template, actionName, ex.Message))
            End Try

            Return outputArguments
        End Function

        Private Function CreateMethodDefinitionParameters(actionParameters As IEnumerable(Of IParameter),
                                                       additionalParameters As IEnumerable(Of IParameter)) _
            As List(Of MethodParameterDefinition)

            Return actionParameters.
                        Where(Function(x) x.Direction = ParameterDirection.In).
                   Concat(additionalParameters.
                        Where(Function(x) x.Direction = ParameterDirection.In)).
                   Concat(actionParameters.
                        Where(Function(x) x.Direction = ParameterDirection.Out)).
                   Concat(additionalParameters.
                        Where(Function(x) x.Direction = ParameterDirection.Out)).
                   Select(Function(p) New MethodParameterDefinition(CodeCompiler.GetIdentifier(p.Name),
                                                                    p.DataType,
                                                                    p.Direction = ParameterDirection.Out)).ToList()

        End Function

        Public Function GetMethodDefinition(classDefinition As ClassDefinition, actionName As String) As MethodDefinition
            Return classDefinition.Methods.Single(Function(m) m.Name = GetMethodName(actionName))
        End Function

        Public Function GetMethodName(actionName As String) As String
            Return $"{MethodPrefix} {actionName}"
        End Function

    End Class
End Namespace
