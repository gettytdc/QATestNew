Imports System.Runtime.Serialization
''' <summary>
''' Additional information included with logging calls, usually for diagnostics
''' purposes.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class LogInfo

    ''' <summary>
    ''' Stage log inhibiting modes
    ''' </summary>
    Public Enum InhibitModes
        Never
        Always
        OnSuccess
    End Enum

    <DataMember>
    Private mInhibitParams As Boolean

    ''' <summary>
    ''' Creates a new empty LogInfo instance
    ''' </summary>
    Public Sub New()
        Inhibit = False
        mInhibitParams = False
        AutomateWorkingSet = 0
        TargetAppName = Nothing
        TargetAppWorkingSet = 0
    End Sub

    ''' <summary>
    ''' Whether or not this log entry should be inhibited, according to the
    ''' combination of process designer and diagnostics settings. The logging
    ''' engine may take no notice of this, however, and log it anyway - e.g.
    ''' the interactive logs in Process Studio do not inhibit.
    ''' </summary>
    <DataMember>
    Public Inhibit As Boolean

    ''' <summary>
    ''' Whether or not this log entry should inhibit the publishing of parameters
    ''' to the log. Like <see cref="Inhibit"/> the logging engine may choose to
    ''' ignore this setting, depending on the context within which it is logging.
    ''' </summary>
    Public Property InhibitParams() As Boolean
        Get
            Return mInhibitParams
        End Get
        Set(ByVal value As Boolean)
            mInhibitParams = value
        End Set
    End Property

    ''' <summary>
    ''' The size of the Working Set, in bytes, of Automate.exe, or 0 if this
    ''' information is not gathered/recorded.
    ''' </summary>
    <DataMember>
    Public AutomateWorkingSet As Long

    ''' <summary>
    ''' The name of the target application. Nothing if this information is not
    ''' gathered/recorded.
    ''' </summary>
    <DataMember>
    Public TargetAppName As String

    ''' <summary>
    ''' The size of the Working Set, in bytes, of the target application, or 0 if
    ''' this information is not gathered/recorded.
    ''' </summary>
    <DataMember>
    Public TargetAppWorkingSet As Long

End Class

