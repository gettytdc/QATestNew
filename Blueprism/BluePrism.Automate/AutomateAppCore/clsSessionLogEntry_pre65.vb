
Imports BluePrism.AutomateProcessCore

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization


<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsSessionLogEntry_pre65

#Region " Member Variables "

    ' The session number for this entry's log
    <DataMember>
    Private mSessionNumber As Integer

    ' The sequence number of this particular entry
    <DataMember>
    Private mSequenceNo As Integer

    ' The ID of the stage which this entry was called from.
    <DataMember>
    Private mStageId As Guid

    ' The name of the stage that this entry was called from. This should always
    ' have a value.
    <DataMember>
    Private mStageName As String

    ' The type of stage called from.
    <DataMember>
    Private mStageType As StageTypes

    ' The name of the process that this log entry was written from.
    ' This will be null if this log entry has no such value.
    <DataMember>
    Private mProcessName As String

    ' The name of the page, within a process, that this log entry was written from.
    ' This will be null if this log entry has no such value.
    <DataMember>
    Private mPageName As String

    ' The name of the business object that this log entry was written from.
    ' This will be null if this log entry has no such value.
    <DataMember>
    Private mObjectName As String

    ' The name of the action, within an object, that this entry was written from.
    ' This will be null if this log entry has no such value.
    <DataMember>
    Private mActionName As String

    ' The result in this log entry in its string-encoded form.
    ' This will be null if this log entry has no such value.
    <DataMember>
    Private mResult As String

    ' The data type of the result
    <DataMember>
    Private mResultType As DataType

    ' The start date/time for this log entry - MinValue if this is unset
    <DataMember>
    Private mStartDate As DateTimeOffset

    ' The end date/time for this log entry - MinValue if this is unset
    <DataMember>
    Private mEndDate As DateTimeOffset

    ' The attributes for this log entry in an XML string
    ' This will be null if this log entry has no such value.
    <DataMember>
    Private mAttributeXml As String

    ' The working set for automate at the time of this log entry
    <DataMember>
    Private mWorkingSet As Long

    ' The target application related to this log entry
    <DataMember>
    Private mTargetApp As String

    ' The working set of the target app at the time of this log entry
    <DataMember>
    Private mTargetAppWorkingSet As Long

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new session log entry with data from the given provider.
    ''' The provider is expected to contain the following data :-
    ''' <list>
    ''' <item>SessionNumber : Integer</item>
    ''' <item>SeqNum : Integer</item>
    ''' <item>StageId : Guid</item>
    ''' <item>StageName : String</item>
    ''' <item>StageType : StageTypes (Integer)</item>
    ''' <item>ProcessName : String</item>
    ''' <item>PageName : String</item>
    ''' <item>ObjectName : String</item>
    ''' <item>ActionName : String</item>
    ''' <item>Result : String</item>
    ''' <item>ResultType : DataType (Integer)</item>
    ''' <item>StartDateTime : DateTime</item>
    ''' <item>EndDateTime : DateTime</item>
    ''' <item>AttributeXML : String</item>
    ''' </list>
    ''' </summary>
    ''' <param name="prov">The provider of the data for this entry.</param>
    Public Sub New(prov As IDataProvider)
        mSessionNumber = prov.GetValue("sessionnumber", 0)
        mSequenceNo = prov.GetValue("seqnum", 0)
        mStageId = prov.GetValue("stageid", Guid.Empty)
        mStageName = prov.GetValue("stagename", "")
        mStageType = prov.GetValue("stagetype", StageTypes.Undefined)
        mProcessName = prov.GetString("processname")
        mPageName = prov.GetString("pagename")
        mObjectName = prov.GetString("objectname")
        mActionName = prov.GetString("actionname")
        mResult = prov.GetString("result")
        mResultType = prov.GetValue("resulttype", DataType.unknown)
        mStartDate = BPUtil.ConvertDateTimeOffset(prov, "startdatetime", "starttimezoneoffset")
        mEndDate = BPUtil.ConvertDateTimeOffset(prov, "enddatetime", "endtimezoneoffset")
        mAttributeXml = prov.GetString("attributexml")
        mWorkingSet = prov.GetValue("automateworkingset", 0L)
        mTargetApp = prov.GetString("targetappname")
        mTargetAppWorkingSet = prov.GetValue("targetappworkingset", 0L)
    End Sub

    ''' <summary>
    ''' Creates a new empty session log entry.
    ''' </summary>
    Private Sub New()
    End Sub

#End Region

#Region " Properties "

    Public ReadOnly Property SessionNumber() As Integer
        Get
            Return mSessionNumber
        End Get
    End Property

    Public ReadOnly Property SequenceNo() As Integer
        Get
            Return mSequenceNo
        End Get
    End Property
    Public ReadOnly Property StageId() As Guid
        Get
            Return mStageId
        End Get
    End Property

    Public ReadOnly Property StageName() As String
        Get
            Return mStageName
        End Get
    End Property

    Public ReadOnly Property StageType() As StageTypes
        Get
            Return mStageType
        End Get
    End Property

    Public ReadOnly Property ProcessName() As String
        Get
            Return mProcessName
        End Get
    End Property

    Public ReadOnly Property PageName() As String
        Get
            Return mPageName
        End Get
    End Property

    Public ReadOnly Property ObjectName() As String
        Get
            Return mObjectName
        End Get
    End Property

    Public ReadOnly Property ActionName() As String
        Get
            Return mActionName
        End Get
    End Property

    Public ReadOnly Property Result() As String
        Get
            Return mResult
        End Get
    End Property

    Public ReadOnly Property ResultType() As DataType
        Get
            Return mResultType
        End Get
    End Property

    Public ReadOnly Property StartDate() As DateTimeOffset
        Get
            Return mStartDate
        End Get
    End Property

    Public ReadOnly Property EndDate() As DateTimeOffset
        Get
            Return mEndDate
        End Get
    End Property

    Public ReadOnly Property AttributeXml() As String
        Get
            Return mAttributeXml
        End Get
    End Property

    Public ReadOnly Property WorkingSet() As Long
        Get
            Return mWorkingSet
        End Get
    End Property

    Public ReadOnly Property TargetApp() As String
        Get
            Return mTargetApp
        End Get
    End Property

    Public ReadOnly Property TargetAppWorkingSet() As Long
        Get
            Return mTargetAppWorkingSet
        End Get
    End Property

#End Region

End Class
