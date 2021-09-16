Imports BluePrism.AutomateProcessCore.ProcessLoading

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsAPC
''' 
''' <summary>
''' The main APC class - currently only used to store configuration information.
''' </summary>

Public Class clsAPC

    ''' <summary>
    ''' A list of add-on functions to be used in addition to the internal ones.
    ''' </summary>
    Private Shared mAddOnFunctions As New List(Of clsFunction)


    ''' <summary>
    ''' The ObjectLoader instance to be used when anything in APC needs to load an
    ''' object, or Nothing if object loading is not possible.
    ''' </summary>
    Public Shared Property ObjectLoader As IObjectLoader

    ''' <summary>
    ''' The clsProcessLoader instance to be used when anything in APC needs to load a
    ''' process, or Nothing if process loading is not possible.
    ''' </summary>
    Public Shared Property ProcessLoader() As IProcessLoader
        Get
            Return mProcessLoader
        End Get
        Set(value As IProcessLoader)
            mProcessLoader = value
            AMI.clsAMI.SetFontLoader(value)
        End Set
    End Property
    Private Shared mProcessLoader As IProcessLoader

    ''' <summary>
    ''' The clsProcessDebugHook instance to be used to provide debug support, or
    ''' Nothing if one has not been set.
    ''' </summary>
    Public Shared ReadOnly Property ProcessDebugHook() As clsProcessDebugHook
        Get
            Return (mProcessDebugHook)
        End Get
    End Property
    Private Shared mProcessDebugHook As clsProcessDebugHook


    ''' <summary>
    ''' Get a list of available add-on functions.
    ''' </summary>
    Friend Shared ReadOnly Property AddOnFunctions() As List(Of clsFunction)
        Get
            Return mAddOnFunctions
        End Get
    End Property


    Shared Sub New()
        'Until such time as the application supplies us with a process loader, we
        'will just assume we can't load one, and any related functionality will be
        'disabled...
        mProcessLoader = Nothing
        'Same goes for the debug hook...
        mProcessDebugHook = Nothing
        'By default, none of the diagnostics stuff is enabled
        mDiagnostics = Diags.DefaultDiags
    End Sub



    ''' <summary>
    ''' Set the clsProcessDebugHook instance to be used to provide debugging
    ''' support
    ''' </summary>
    Public Shared Sub SetProcessDebugHook(ByVal p As clsProcessDebugHook)
        mProcessDebugHook = p
    End Sub

    ''' <summary>
    ''' Add a new add-on function to the list of those available.
    ''' </summary>
    ''' <param name="f">The clsFunction-derived class representing the new function
    ''' </param>
    Public Shared Sub AddFunction(ByVal f As clsFunction)
        Debug.Assert(Not mAddOnFunctions.Contains(f))
        mAddOnFunctions.Add(f)
    End Sub

    ''' <summary>
    ''' Diagnostics flags. (Note - update GetDiagnosticsString if these change)
    ''' </summary>
    <Flags>
    Public Enum Diags
        DefaultDiags = 0
        LogOverrideKey = 1          'Override log inhibit on key stage types
        LogOverrideAll = 2          'Override log inhibit on all stage types
        LogMemory = 4               'Log memory information
        ForceGC = 8                 'Force garbage collection when collection memory info
        LogWebServices = 16         'Log Web Service communication
        LogOverrideErrorsOnly = 32  'Override log inhibit to only show errors
    End Enum
    Private Shared mDiagnostics As Diags
    Public Shared Property Diagnostics() As Diags
        Get
            Return mDiagnostics
        End Get
        Set(ByVal value As Diags)
            mDiagnostics = value
        End Set
    End Property

    ''' <summary>
    ''' Get a String representation of the current diagnostics settings.
    ''' </summary>
    Public Shared Function GetDiagnosticsString() As String
        If mDiagnostics = 0 Then Return My.Resources.Resources.clsAPC_None
        Dim res As String = String.Empty


        If mDiagnostics.HasFlag(Diags.LogOverrideAll) Then
            res &= "LogOverrideAll,"
        ElseIf mDiagnostics.HasFlag(Diags.LogOverrideKey) Then
            res &= "LogOverrideKey,"
        ElseIf mDiagnostics.HasFlag(Diags.LogOverrideErrorsOnly) Then
            res &= "LogOverrideErrorsOnly,"
        End If

        If mDiagnostics.HasFlag(Diags.LogWebServices) Then
            res &= "LogWebServices,"
        End If
        If mDiagnostics.HasFlag(Diags.LogMemory) Then
            res &= "LogMemory,"
        End If
        If mDiagnostics.HasFlag(Diags.ForceGC) Then
            res &= "ForceGC"
        End If
        If res.EndsWith(",") Then
            res = res.Substring(0, res.Length - 1)
        End If
        Return res
    End Function


End Class
