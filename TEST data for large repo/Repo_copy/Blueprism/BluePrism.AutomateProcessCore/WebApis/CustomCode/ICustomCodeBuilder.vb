Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling

Namespace WebApis.CustomCode

    ''' <summary>
    ''' Runs custom code during the execution of a Web API Business Object Action
    ''' </summary>
    Public Interface ICustomCodeBuilder
        Function GetAssembly(context As ActionContext) As AssemblyData

    End Interface
End Namespace