Imports System.Linq
Imports BluePrism.AutomateProcessCore.Compilation

Namespace WebApis.CustomCode

    ''' <summary>
    ''' Creates <see cref="AssemblyDefinition"/> objects from a Web API
    ''' configuration containing custom code methods
    ''' </summary>
    Public Module AssemblyDefinitionMapper

        ''' <summary>
        ''' The file name used within the pragma directive that is generated for 
        ''' common code. CompilerError instances contain the filename and line number of
        ''' any errors which can be used to link errors back to the code from which they 
        ''' originate.
        ''' </summary>
        Public Const SharedCodeFileName = "commoncode"

        Public Function Create(className As String, configuration As WebApiConfiguration) As AssemblyDefinition

            Dim classes = {Create_Class(className, configuration)}
            Dim assemblyName = className + "Assembly"
            Dim commonCode = configuration.CommonCode

            Return New AssemblyDefinition(assemblyName, commonCode.Language, commonCode.Namespaces,
                                          commonCode.References, classes)
        End Function

        Private Function Create_Class(className As String, configuration As WebApiConfiguration) As ClassDefinition

            Dim methodDefinitions = CustomCodeMethodType.All.SelectMany(Function(x) x.CreateMethods(configuration))
            Return New ClassDefinition(className, methodDefinitions, configuration.CommonCode.Code, SharedCodeFileName)

        End Function

    End Module

End Namespace