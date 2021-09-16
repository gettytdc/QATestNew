Imports System.Net.Http

Namespace WebApis

    Public Module HttpMethods

        ''' <summary>
        ''' Get the base collection of http verb strings
        ''' </summary>
        Public Function Standard() As HttpMethod()
            Return New HttpMethod() _
            {
                HttpMethod.Get,
                HttpMethod.Put,
                HttpMethod.Post,
                New HttpMethod("PATCH"),
                HttpMethod.Delete,
                HttpMethod.Head,
                HttpMethod.Options,
                HttpMethod.Trace,
                New HttpMethod("CONNECT")
            }
        End Function

    End Module

End Namespace