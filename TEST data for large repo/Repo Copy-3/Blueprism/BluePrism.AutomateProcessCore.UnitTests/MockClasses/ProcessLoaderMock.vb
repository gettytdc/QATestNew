Imports BluePrism.AutomateProcessCore.ProcessLoading
Imports BluePrism.CharMatching
Imports BluePrism.ApplicationManager.AMI

''' <summary> 
''' A process loader is optional, but required for some advanced clsProcess
''' functionality, in particular the ability to run sub-processes.
''' 
''' Use this class in your unit tests where a process loader is required e.g. 
''' when environment variables are involved
''' </summary>
Friend Class PrcoessLoaderMock
    Implements ProcessLoading.IProcessLoader

    Dim mEnvVars As Dictionary(Of String, clsArgument)

    Public Sub New()
        mEnvVars = New Dictionary(Of String, clsArgument)
    End Sub

    Public Property CacheBehaviour As CacheRefreshBehaviour Implements IProcessLoader.CacheBehaviour

    Public ReadOnly Property AvailableFontNames As ICollection(Of String) Implements IFontStore.AvailableFontNames
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Sub SaveFont(font As BPFont) Implements IFontStore.SaveFont
        Throw New NotImplementedException()
    End Sub

    Public Function GetProcessXML(gProcessID As Guid, ByRef sXML As String, ByRef lastmod As Date, ByRef sErr As String) As Boolean Implements IProcessLoader.GetProcessXML
        Throw New NotImplementedException()
    End Function

    Public Function GetEffectiveRunMode(processId As Guid) As BusinessObjectRunMode Implements IProcessLoader.GetEffectiveRunMode
        Throw New NotImplementedException()
    End Function

    Public Function GetProcessAtrributes(gProcessID As Guid, ByRef Attributes As ProcessAttributes, ByRef serr As String) As Boolean Implements IProcessLoader.GetProcessAtrributes
        Throw New NotImplementedException()
    End Function

    Public Function GetEnvVars() As Dictionary(Of String, clsArgument) Implements IProcessLoader.GetEnvVars
        Return mEnvVars
    End Function

    Public Function GetEnvVars(freshFromDatabase As Boolean) As Dictionary(Of String, clsArgument) Implements IProcessLoader.GetEnvVars
        Return mEnvVars
    End Function

    Public Function GetEnvVarSingle(name As String, updateCache As Boolean) As clsArgument Implements IProcessLoader.GetEnvVarSingle
        Return New clsArgument("GetEnvVarSingle", New clsProcessValue(DataType.text, "GetEnvVarSingle_txt"))
    End Function

    Public Function GetAMIInfo() As clsGlobalInfo Implements IProcessLoader.GetAMIInfo
        Throw New NotImplementedException()
    End Function

    Public Function GetFont(name As String) As BPFont Implements IFontStore.GetFont
        Throw New NotImplementedException()
    End Function

    Public Function DeleteFont(name As String) As Boolean Implements IFontStore.DeleteFont
        Throw New NotImplementedException()
    End Function

End Class
