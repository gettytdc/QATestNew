Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class to represent the request details of a WebApi action
''' </summary>
Friend NotInheritable Class WebApiActionRequestDetails

    ReadOnly Property Action As WebApiActionDetails

    Sub New(actionDetails As WebApiActionDetails)
        Action = actionDetails
    End Sub

    ''' <summary>
    ''' Gets or sets the HTTP method to be used in this action
    ''' </summary>
    Property Method As HttpMethod = HttpMethod.Get

    ''' <summary>
    ''' Gets or sets the URL path associated with this action
    ''' </summary>
    Property UrlPath As String = "/"

    ''' <summary>
    ''' Gets the headers associated with this action
    ''' </summary>
    ReadOnly Property Headers As New WebApiCollection(Of HttpHeader) With {
        .ActionSpecific = True
    }

    ''' <summary>
    ''' Gets or sets the body content for this action
    ''' </summary>
    Property BodyContent As IBodyContent = New NoBodyContent()

    ''' <summary>
    ''' Adds a collection of headers to this action request and returns it.
    ''' </summary>
    ''' <param name="headers">The headers to add to this action request</param>
    ''' <returns>This action request with the headers added</returns>
    Public Function WithHeaders(headers As IEnumerable(Of HttpHeader)) As WebApiActionRequestDetails
        If headers IsNot Nothing Then Me.Headers.AddAll(headers)
        Return Me
    End Function
End Class

