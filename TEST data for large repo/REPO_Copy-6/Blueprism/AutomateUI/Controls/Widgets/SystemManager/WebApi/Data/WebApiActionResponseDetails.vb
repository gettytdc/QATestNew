Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib.Collections
''' <summary>
''' Class to represent the response details of a WebApi action
''' </summary>
Friend NotInheritable Class WebApiActionResponseDetails

    ReadOnly Property Action As WebApiActionDetails

    Sub New(actionDetails As WebApiActionDetails)
        Action = actionDetails
    End Sub

    Public Property Code As String

    ''' <summary>
    ''' Gets the custom output parameters associated with this action
    ''' </summary>
    ReadOnly Property CustomOutputParameters As New WebApiCollection(Of ResponseOutputParameter) With {
        .ActionSpecific = True
    }

    ''' <summary>
    ''' Adds a collection of parameters to this action response and returns it.
    ''' </summary>
    ''' <param name="params">The parameters to add to this action response.</param>
    ''' <returns>This action response with the parameters added.</returns>
    Public Function WithParameters(params As IEnumerable(Of ResponseOutputParameter)) As WebApiActionResponseDetails
        If params IsNot Nothing Then CustomOutputParameters.AddAll(params)
        Return Me
    End Function
End Class
