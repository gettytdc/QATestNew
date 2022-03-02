Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Module ExtensionMethods

    ''' <summary>
    ''' Rethrows an exception, retaining the stack trace currently set inside it,
    ''' rather than replacing it with the trace of the current stack frames.
    ''' </summary>
    ''' <param name="this">The exception to rethrow</param>
    ''' <returns>The exception with stack trace intact - in reality, this will never
    ''' actually get returned, since this method throws the exception within itself,
    ''' but it means that you can indicate to the compiler that the calling context
    ''' will terminate at this method call by doing something like the following:
    ''' <pre>
    ''' Catch ite As InvocationTargetException
    '''     If ite.InnerException Is Nothing Then Throw
    '''     Throw ite.InnerException.RethrowWithStackTrace()
    ''' End Catch
    ''' </pre>
    ''' </returns>
    <Extension>
    Public Function RethrowWithStackTrace(this As Exception) As Exception

        ' In .NET 4.5, this can be replaced with:
        ' ExceptionDispatchInfo.Capture(this).Throw()
        ' but for .NET 4, we have to use reflection to preserve the stack trace

        Dim mi As MethodInfo = GetType(Exception).GetMethod(
         "InternalPreserveStackTrace",
         BindingFlags.Instance Or BindingFlags.NonPublic)
        mi.Invoke(this, Nothing)
        Throw this

    End Function

End Module
