Imports System.Linq
Imports BluePrism.Core.Utility

Namespace Compilation

    ''' <summary>
    ''' Invokes a method on an object, bridging between Blue Prism input and output
    ''' arguments and the method parameters
    ''' </summary>
    Public Module MethodInvoker

        ''' <summary>
        ''' Invokes the specified method on an object, mapping between Blue Prism 
        ''' values and method parameters and updating any output arguments based
        ''' on the values of output parameters after the method has executed.
        ''' </summary>
        ''' <param name="target">The target object</param>
        ''' <param name="methodName">The name of the method</param>
        ''' <param name="inputArguments">The input arguments containing values that 
        ''' will be supplied as parameters to the method</param>
        ''' <param name="outputArguments">The output arguments that will be supplied
        ''' as parameters to the method. The corresponding parameters will be defined 
        ''' as output parameters in the generated code, allowing values to set when 
        ''' the method executes. Each argument within this collection will be updated 
        ''' in-place with the parameter value that is set by the method.</param>
        Public Sub InvokeMethod(target As Object, methodName As String,
                                inputArguments() As clsArgument,
                                outputArguments() As clsArgument)
            If target Is Nothing Then
                Throw New ArgumentNullException(NameOf(target))
            End If
            If methodName Is Nothing Then
                Throw New ArgumentNullException(NameOf(methodName))
            End If

            Dim method = target.GetType().GetMethod(methodName)
            If method Is Nothing Then
                Dim template = CompilationResources.MethodInvoker_MethodNotFoundErrorTemplate
                Throw New ArgumentException(String.Format(template, methodName), NameOf(methodName))
            End If

            Dim inputParameters = inputArguments.
                    Select(Function(a) ProcessValueConvertor.ConvertValueToNetType(a.Value, False, a.Name)).
                    ToArray()
            Dim outputParameters = outputArguments.
                    Select(Function(a) ProcessValueConvertor.ConvertValueToNetType(a.Value, True, a.Name)).
                    ToArray()
            Dim parameters = inputParameters.Concat(outputParameters).ToArray()
            method.Invoke(target, parameters)
            Dim updatedOutputParameters = parameters.Skip(inputParameters.Length)
            outputArguments.ZipEach(updatedOutputParameters,
                                    Sub(argument, parameter) UpdateOutputParameterValue(argument, parameter))

        End Sub

        Private Sub UpdateOutputParameterValue(argument As clsArgument, value As Object)
            Dim processValue = ProcessValueConvertor.ConvertNetTypeToValue(value, argument.Value.DataType)
            argument.Value = processValue
            ' Dispose output parameter if needed see bug #4562
            Try
                TryCast(value, IDisposable)?.Dispose()
            Catch 'Do nothing if object is already disposed, or any other failure to dispose
            End Try
        End Sub
    End Module


End NameSpace