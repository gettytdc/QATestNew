Imports System.Xml
Imports System.Data.SqlClient

Imports BluePrism.AutomateProcessCore

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization

''' <summary>
''' Class to represent a session log entry.
''' This is used primarily as a way of transporting the data to / from the
''' database and is therefore very database-heavy in terms of its function.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsSessionLogEntry

#Region " Member Variables "

    ' The session number for this entry's log
    <DataMember>
    Private mSessionNumber As Integer

    ' The log number of this particular entry
    <DataMember>
    Private mLogNumber As Integer

    <DataMember>
    Private mLogId As Long

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

    ' The size of the AttributeXml in the database
    <DataMember>
    Private mAttributeSize As Long

    ' The size of the AttributeXml in the database
    <DataMember>
    Private mAttributeXmlGzip As Byte()

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new session log entry with data from the given provider.
    ''' The provider is expected to contain the following data :-
    ''' <list>
    ''' <item>SessionNumber : Integer</item>
    ''' <item>LogId : Long</item>
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
        mLogId = prov.GetValue("logid", -1)
        mSessionNumber = prov.GetValue("sessionnumber", 0)
        mLogNumber = prov.GetValue("lognumber", 0)
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
        mAttributeSize = prov.GetValue("attributesize", 0L)
    End Sub

    Public Sub New(prov As clsSessionLogEntry_pre65)
        mLogId = prov.SequenceNo
        mSessionNumber = prov.SessionNumber
        mLogNumber = 0
        mStageId = prov.StageId
        mStageName = prov.StageName
        mStageType = prov.StageType
        mProcessName = prov.ProcessName
        mPageName = prov.PageName
        mObjectName = prov.ObjectName
        mActionName = prov.ActionName
        mResult = prov.Result
        mResultType = prov.ResultType
        mStartDate = prov.StartDate
        mEndDate = prov.EndDate
        mAttributeXml = prov.AttributeXml
        mWorkingSet = prov.WorkingSet
        mTargetApp = prov.TargetApp
        mTargetAppWorkingSet = prov.TargetAppWorkingSet
        mAttributeSize = If(prov.AttributeXml Is Nothing, 0, CType(prov.AttributeXml.Length, Long))
    End Sub

    ''' <summary>
    ''' Creates a new empty session log entry.
    ''' </summary>
    Private Sub New()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The log number of this log entry within its containing log
    ''' </summary>
    Public ReadOnly Property LogNumber() As Integer
        Get
            Return mLogNumber
        End Get
    End Property

    Public ReadOnly Property StageId() As String
        Get
            Return mStageId.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The log id of this log entry within its containing log
    ''' </summary>
    Public ReadOnly Property LogId() As Long
        Get
            Return mLogId
        End Get
    End Property

    ''' <summary>
    ''' The start date/time of this entry
    ''' </summary>
    Public ReadOnly Property StartDate() As DateTimeOffset
        Get
            Return mStartDate
        End Get
    End Property

    ''' <summary>
    ''' A display representation of the start datetime/offset of this entry.
    ''' </summary>
    Public ReadOnly Property StartDateDisplay() As String
        Get
            If mStartDate = DateTimeOffset.MinValue Then Return String.Empty
            Return mStartDate.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The end date/time recorded on this entry
    ''' </summary>
    Public ReadOnly Property EndDate() As DateTimeOffset
        Get
            Return mEndDate
        End Get
    End Property

    ''' <summary>
    ''' A display representation of the end datetime/offset of this entry
    ''' </summary>
    Public ReadOnly Property EndDateDisplay() As String
        Get
            If mEndDate = DateTimeOffset.MinValue OrElse
                mEndDate = DateTimeOffset.MaxValue Then Return String.Empty
            Return mEndDate.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The stage type for this log entry
    ''' </summary>
    Public ReadOnly Property StageType() As StageTypes
        Get
            Return mStageType
        End Get
    End Property

    ''' <summary>
    ''' The name of the stage recorded on this entry, or an empty string if no
    ''' stage name is recorded.
    ''' </summary>
    Public ReadOnly Property StageName() As String
        Get
            If mStageName Is Nothing Then Return ""
            Return mStageName
        End Get
    End Property

    ''' <summary>
    ''' The name of the process recorded on this entry, or an empty string if no
    ''' process name is recorded
    ''' </summary>
    Public ReadOnly Property ProcessName() As String
        Get
            If mProcessName Is Nothing Then Return ""
            Return mProcessName
        End Get
    End Property

    ''' <summary>
    ''' The name of the business object recorded on this entry, or an empty string
    ''' if no business object is recorded
    ''' </summary>
    Public ReadOnly Property ObjectName() As String
        Get
            If mObjectName Is Nothing Then Return ""
            Return mObjectName
        End Get
    End Property

    ''' <summary>
    ''' Gets a text representation of this log entry
    ''' </summary>
    Public ReadOnly Property Text() As String
        Get
            Dim sb As New StringBuilder(StartDateDisplay, 255)

            sb.AppendFormat(" {0}:", clsStageTypeName.GetLocalizedFriendlyName(mStageType.ToString()).ToUpper())

            Select Case mStageType

                Case StageTypes.Action
                    sb.AppendFormat(My.Resources.clsSessionLogEntry_Stage0, mStageName)
                    If mObjectName <> "" Then sb.AppendFormat(
                     My.Resources.clsSessionLogEntry_BusinessObject0Action1,
                     mObjectName, clsBusinessObjectAction.GetLocalizedFriendlyName(mActionName, mObjectName, "Action"))

                Case StageTypes.Start, StageTypes.End
                    If mProcessName <> "" Then sb.AppendFormat(
                     My.Resources.clsSessionLogEntry_Process0Page1,
                     mProcessName, mPageName)

                Case Else
                    sb.AppendFormat(My.Resources.clsSessionLogEntry_Stage0, mStageName)

            End Select

            If mResult <> "" Then
                If mResult.StartsWith("ERROR: ") Then
                    mResult = mResult.Replace("ERROR: ", My.Resources.clsSessionLogEntry_LogError)
                End If
                sb.AppendFormat(My.Resources.clsSessionLogEntry_RESULT01, mResult, clsDataTypeInfo.GetLocalizedFriendlyName(mResultType))
            End If

            Return sb.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The result set in this entry or an empty string if no result was set.
    ''' </summary>
    Public ReadOnly Property Result() As String
        Get
            If mResult Is Nothing Then Return ""
            Return mResult
        End Get
    End Property

    ''' <summary>
    ''' The result type set in this log entry or <see cref="DataType.unknown"/> if
    ''' no result or type is set.
    ''' </summary>
    Public ReadOnly Property ResultType() As DataType
        Get
            Return mResultType
        End Get
    End Property

    ''' <summary>
    ''' The result type, formatted for display - returns a string representation of
    ''' the datatype of the result, or an empty string if no result or type is set.
    ''' </summary>
    Public ReadOnly Property ResultTypeDisplay() As String
        Get
            If mResultType = DataType.unknown Then Return ""
            Return mResultType.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The attribute XML set in this entry, or null if none is set.
    ''' </summary>

    Public Property AttributeXml() As String
        Get
            Return mAttributeXml
        End Get
        Set(value As String)
            mAttributeXml = value
        End Set
    End Property

    Public Property AttributeSize() As Long
        Get
            Return mAttributeSize
        End Get
        Set(value As Long)
            mAttributeSize = value
        End Set
    End Property

    Public Property AttributeXmlGzip() As Byte()
        Get
            Return mAttributeXmlGzip
        End Get
        Set(value As Byte())
            mAttributeXmlGzip = value
        End Set
    End Property


#End Region

#Region " Import / Export Business "

    ''' <summary>
    ''' Reads a log entry from the given reader, which is expected to be at the
    ''' start of the entry element from which this object's data should be drawn.
    ''' </summary>
    ''' <param name="reader">The reader from which the XML data should be read.
    ''' </param>
    ''' <param name="sessNo">The session number of the log that this entry is a
    ''' part of.</param>
    ''' <param name="currProcess">The current process name as read from a preceding
    ''' process element. Null if there is no current process.</param>
    ''' <param name="currPage">The current page name as read from a preceding
    ''' page element. Null if there is no current page.</param>
    ''' <returns>A fully initialised session log entry object, with its data
    ''' populated from the given XML reader. The reader will be pointing at the
    ''' end element of the entry on exit of this method.</returns>
    Friend Shared Function ReadFrom(ByVal reader As XmlReader, ByVal sessNo As Integer,
     ByVal currProcess As String, ByVal currPage As String) As clsSessionLogEntry

        Dim entry As New clsSessionLogEntry()
        entry.mSessionNumber = sessNo
        entry.mProcessName = currProcess
        entry.mPageName = currPage
        entry.ReadFrom(reader)

        Return entry

    End Function

    ''' <summary>
    ''' Reads this entry's data from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader from which this entry should read its data.
    ''' </param>
    Private Sub ReadFrom(ByVal r As XmlReader)

        ' If the entry element has no attributexml, it is likely to be an empty element -
        ' which doesn't raise an 'EndElement' node in the parsing (somewhat annoyingly).
        ' So do a quick check here before attempting to read the attributes (since
        ' moving into the attributes 'loses' this value from the reader)
        Dim empty As Boolean = r.IsEmptyElement

        ' First, read this entry's data from the attributes of the current XML element.
        ' Get to the first attribute, and go from there...
        If Not r.MoveToFirstAttribute() Then
            Throw New InvalidOperationException(
             "No attributes to read log entry data from: Current name: " & r.Name)
        End If

        Dim startDateTime As DateTime?
        Dim startDateOffset As Integer?
        Dim endDateTime As DateTime?
        Dim endDateOffset As Integer?
        ' Loop through all the available attributes to load the data for this entry.
        Do
            Select Case r.Name
                Case "start" : startDateTime = Date.ParseExact(r.Value, clsSessionLog.DateFormat, Nothing)
                Case "startoffset" : startDateOffset = Integer.Parse(r.Value)
                Case "end" : endDateTime = Date.ParseExact(r.Value, clsSessionLog.DateFormat, Nothing)
                Case "endoffset" : endDateOffset = Integer.Parse(r.Value)
                Case "stageid" : mStageId = New Guid(r.Value)
                Case "stagename" : mStageName = r.Value
                Case "stagetype" : mStageType = clsEnum(Of StageTypes).Parse(r.Value)
                Case "object" : mObjectName = r.Value
                Case "action" : mActionName = r.Value
                Case "result" : mResult = r.Value
                Case "result-base64" : mResult = Encoding.UTF8.GetString(Convert.FromBase64String(r.Value))
                Case "resulttype" : mResultType = clsEnum(Of DataType).Parse(r.Value)
                Case "workingset" : mWorkingSet = Long.Parse(r.Value)
                Case "targetapp" : mTargetApp = r.Value
                Case "targetapp-workingset" : mTargetAppWorkingSet = Long.Parse(r.Value)
            End Select
        Loop While r.MoveToNextAttribute()

        mStartDate = BPUtil.ConvertDateTimeOffset(startDateTime, startDateOffset)
        mEndDate = BPUtil.ConvertDateTimeOffset(endDateTime, endDateOffset)

        ' Attributes done... check if this was empty before and return if it was.
        If empty Then Return

        ' Having got the XML attributes, we want to read to the end of the entry, to
        ' ensure we get the, er, 'attributexml' - ie. input/output parameters of the
        ' log entry. In hindsight, probably not the clearest of names to choose.
        While r.Read()

            ' Otherwise, if we hit the <attributexml> entry, just
            If r.NodeType = XmlNodeType.Element AndAlso r.Name = "attributexml" Then
                mAttributeXml = r.ReadInnerXml()
            End If
            ' Note that r.ReadInnerXml() reads the inner XML of the element and then
            ' *moves to the next element*. Therefore, the order of these tests is
            ' important.

            ' If we've hit the </entry> end-element, return to the clsSessionLog
            If r.NodeType = XmlNodeType.EndElement AndAlso r.Name = "entry" Then Return

        End While

    End Sub

    ''' <summary>
    ''' Exports this log entry to the given XML writer, writing new process / page
    ''' elements if this entry's values differ to the given process and page names,
    ''' and returning this elements process and page name in the ref parameters.
    ''' </summary>
    ''' <param name="writer">The XML writer to use to write this element.</param>
    ''' <param name="currProcess">The name of current process element which is open
    ''' in the XML writer. On return, this will contain the process name for this
    ''' element.</param>
    ''' <param name="currPage">The name of the current page element which is open
    ''' in the XML writer. On return this will contain the page name for this
    ''' element.</param>
    Friend Sub ExportTo(
     ByVal writer As XmlWriter, ByRef currProcess As String, ByRef currPage As String)

        ' Is this entry in a different process to the currently open element?
        If currProcess <> mProcessName Then
            ' We're in a different process, so we need to close both the page and
            ' the process elements if they are open.

            ' If page element is open, end it.
            If currPage IsNot Nothing Then writer.WriteEndElement()

            ' If process element is open, end it.
            If currProcess IsNot Nothing Then writer.WriteEndElement()

            ' open new process & page elements, set the current names into the byref params.
            ' If we're not in a process (stepping through an action), don't write out any
            ' process / page elements to indicate that this entry has none.
            If mProcessName IsNot Nothing Then
                writer.WriteStartElement("process")
                writer.WriteAttributeString("name", mProcessName)
            End If
            currProcess = mProcessName

            If mPageName IsNot Nothing Then
                writer.WriteStartElement("page")
                writer.WriteAttributeString("name", mPageName)
            End If
            currPage = mPageName

        ElseIf currPage <> mPageName Then
            ' We're in the same process, but a different page, so close the page element.
            If currPage IsNot Nothing Then writer.WriteEndElement()

            ' I can't imagine how the page name would be null, when the process name
            ' isn't, but just in case...
            If mPageName IsNot Nothing Then
                writer.WriteStartElement("page")
                writer.WriteAttributeString("name", mPageName)
            End If
            currPage = mPageName

        Else
            ' Otherwise, we're in the same process and page... just write it out.

        End If

        writer.WriteStartElement("entry")

        ' Dates - start is always there, end is optional.
        writer.WriteAttributeString("start", mStartDate.DateTime.ToString(clsSessionLog.DateFormat))
        writer.WriteAttributeString("startoffset", mStartDate.Offset.TotalSeconds.ToString())

        If mEndDate <> DateTimeOffset.MinValue Then
            writer.WriteAttributeString("end", mEndDate.DateTime.ToString(clsSessionLog.DateFormat))
            writer.WriteAttributeString("endoffset", mStartDate.Offset.TotalSeconds.ToString())
        End If

        ' Stage details
        writer.WriteAttributeString("stageid", mStageId.ToString())
        writer.WriteAttributeString("stagename", mStageName)
        writer.WriteAttributeString("stagetype", mStageType.ToString())

        ' Object/action details - may be null.
        If mObjectName IsNot Nothing Then
            writer.WriteAttributeString("object", mObjectName)
            writer.WriteAttributeString("action", mActionName)
        End If

        ' Result - may be null
        If mResult IsNot Nothing Then
            ' Handle the result being binary data : See bug #5044
            ' Try writing out the result as plaintext (damn sight easier to read if anyone
            ' wants to look at the logs in notepad or such like)

            ' First check that it's character data - if the XML Writer recognises it,
            ' it will write the appropriate XML entity out. If it doesn't, it writes
            ' it out unencoded, which breaks the XML writer and kills the archive
            ' operation. From the xml (1.0) spec, valid chars are:
            ' #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
            ' any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            If BPUtil.IsValidXmlString(mResult) Then
                writer.WriteAttributeString("result", mResult)
            Else
                writer.WriteAttributeString("result-base64",
                 Convert.ToBase64String(Encoding.UTF8.GetBytes(mResult)))
            End If

            writer.WriteAttributeString("resulttype", mResultType.ToString())
        End If

        ' Memory stats...
        If mWorkingSet > 0L Then _
         writer.WriteAttributeString("workingset", mWorkingSet.ToString())
        If mTargetApp <> "" Then _
         writer.WriteAttributeString("targetapp", mTargetApp)
        If mTargetAppWorkingSet > 0L Then _
         writer.WriteAttributeString("targetapp-workingset", mTargetAppWorkingSet.ToString())

        If mAttributeXml IsNot Nothing Then
            writer.WriteStartElement("attributexml")
            writer.WriteRaw(mAttributeXml)
            writer.WriteEndElement()
        End If

        writer.WriteEndElement()

    End Sub

#End Region

#Region " Inserting a log entry "

    ''' <summary>
    ''' Sets placeholders for all the parameters used by this class in the
    ''' <see cref="SetInto"/> method in preperation for that call.
    ''' </summary>
    Friend Shared Sub CreateParameters(ByVal params As SqlParameterCollection)
        params.AddRange(New SqlParameter() {
         New SqlParameter("@sessionnumber", SqlDbType.Int),
         New SqlParameter("@stageid", SqlDbType.UniqueIdentifier),
         New SqlParameter("@stagename", SqlDbType.VarChar),
         New SqlParameter("@stagetype", SqlDbType.Int),
         New SqlParameter("@processname", SqlDbType.VarChar),
         New SqlParameter("@pagename", SqlDbType.VarChar),
         New SqlParameter("@objectname", SqlDbType.VarChar),
         New SqlParameter("@actionname", SqlDbType.VarChar),
         New SqlParameter("@result", SqlDbType.NText),
         New SqlParameter("@resulttype", SqlDbType.Int),
         New SqlParameter("@startdatetime", SqlDbType.DateTime),
         New SqlParameter("@starttimezoneoffset", SqlDbType.Int),
         New SqlParameter("@enddatetime", SqlDbType.DateTime),
         New SqlParameter("@endtimezoneoffset", SqlDbType.Int),
         New SqlParameter("@attributexml", SqlDbType.NVarChar),
         New SqlParameter("@automateworkingset", SqlDbType.BigInt),
         New SqlParameter("@targetappname", SqlDbType.Text),
         New SqlParameter("@targetappworkingset", SqlDbType.BigInt)
        })
    End Sub

    ''' <summary>
    ''' Sets this entry's values into the given parameter collection.
    ''' </summary>
    ''' <param name="params">The pre-configured parameter collection into which this
    ''' log entry's values should be set.</param>
    Friend Sub SetInto(ByVal params As SqlParameterCollection)
        params("@sessionnumber").Value = mSessionNumber
        params("@stageid").Value = mStageId
        params("@stagename").Value = mStageName
        params("@stagetype").Value = mStageType
        params("@processname").Value = IIf(mProcessName Is Nothing, DBNull.Value, mProcessName)
        params("@pagename").Value = IIf(mPageName Is Nothing, DBNull.Value, mPageName)
        params("@objectname").Value = IIf(mObjectName Is Nothing, DBNull.Value, mObjectName)
        params("@actionname").Value = IIf(mActionName Is Nothing, DBNull.Value, mActionName)
        params("@result").Value = IIf(mResult Is Nothing, DBNull.Value, mResult)
        params("@resulttype").Value = IIf(mResultType = DataType.unknown, DBNull.Value, mResultType)
        params("@startdatetime").Value = mStartDate.DateTime
        params("@starttimezoneoffset").Value = mStartDate.Offset.TotalSeconds
        params("@enddatetime").Value = IIf(mEndDate <> DateTimeOffset.MinValue, mEndDate.DateTime, DBNull.Value)
        params("@endtimezoneoffset").Value = IIf(mEndDate <> DateTimeOffset.MinValue, mEndDate.Offset.TotalSeconds, DBNull.Value)
        params("@attributexml").Value = IIf(mAttributeXml Is Nothing, DBNull.Value, mAttributeXml)
        params("@automateworkingset").Value = mWorkingSet
        params("@targetappname").Value = IIf(mTargetApp Is Nothing, DBNull.Value, mTargetApp)
        params("@targetappworkingset").Value = mTargetAppWorkingSet
    End Sub

#End Region

End Class

