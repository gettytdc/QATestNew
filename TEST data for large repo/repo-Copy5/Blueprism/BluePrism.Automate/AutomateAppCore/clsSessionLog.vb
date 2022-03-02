
Imports System.IO
Imports System.Xml
Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.IO.Compression

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Class to represent a session log from the database.
''' 
''' This is designed primarily as a serializable way to retrieve or send a log
''' to or from the database for archiving purposes. In this, it serves quite a
''' different purpose to clsProcessSession, which acts primarily as an active
''' session for a running process.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsSessionLog : Implements IEnumerable(Of clsSessionLogEntry)

#Region " Member variables and constants "

    ''' <summary>
    ''' The format string to use for any dates written out to XML
    ''' </summary>
    Friend Const DateFormat As String = "yyyy-MM-dd HH:mm:ss.fff"

    ''' <summary>
    ''' The number of log entries to retrieve in each call to the server.
    ''' </summary>
    Protected Const BatchSize As Integer = 1000000

    Public Const AttributeXmlMaxLength As Integer = 2000000000

    ''' <summary>
    ''' The regular expression which contains all invalid file name
    ''' characters inside a character set block (ie. [...])
    ''' </summary>
    Private Shared ReadOnly SanitizeFileNameRegex As Regex = New Regex( _
        String.Format("[{0}]", Regex.Escape(New String(Path.GetInvalidFileNameChars()))))

    ' The log's session number
    <DataMember>
    Protected mSessionNumber As Integer

    ' The session ID of this log
    <DataMember>
    Private mSessionId As Guid

    ' When this log started
    <DataMember>
    Private mStartDateTime As Date

    ' When this log ended. Date.MaxValue if it is still ongoing.
    <DataMember>
    Private mEndDateTime As Date

    ' The ID of the process which ran for this session
    <DataMember>
    Private mProcessId As Guid

    ' The name of the process which ran for this session
    <DataMember>
    Private mProcessName As String

    ' The ID of the resource that started this session
    <DataMember>
    Private mStarterResourceId As Guid

    ' The ID of the user that started this session
    <DataMember>
    Private mStarterUserId As Guid

    ' The ID of the resource which ran/is running this session
    <DataMember>
    Private mRunningResourceId As Guid

    ' The name of the resource which ran/is running this session
    <DataMember>
    Private mRunningResourceName As String

    ' The (O/S) username that this session ran under
    <DataMember>
    Private mRunningOSUserName As String

    ' The status of the session
    <DataMember>
    Private mStatusId As SessionStatus

    ' The collection of imported entries. Usually null.
    <DataMember>
    Private mImportedEntries As ICollection(Of clsSessionLogEntry)

    <DataMember>
    Private mSessionLogMaxAttributeXmlLength As Integer

    <DataMember>
    Private mStartTimezoneOffset As Integer

    <DataMember>
    Private mEndTimezoneOffset As Integer

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new session log using data from the given provider.
    ''' This constructor expects the following data :-
    ''' <list>
    ''' <item>SessionId : Guid</item>
    ''' <item>SessionNumber : Integer</item>
    ''' <item>StartDateTime : DateTime</item>
    ''' <item>EndDateTime : DateTime</item>
    ''' <item>ProcessId : Guid</item>
    ''' <item>ProcessName : String</item>
    ''' <item>StarterResourceId : Guid</item>
    ''' <item>StarterUserId : Guid</item>
    ''' <item>RunningResourceId : Guid</item>
    ''' <item>RunningResourceName : String</item>
    ''' <item>RunningOSUserName : String</item>
    ''' <item>StatusId : SessionStatus (Integer)</item>
    ''' </list>
    ''' </summary>
    ''' <param name="prov">The provider containing the data for this session log.
    ''' </param>
    Public Sub New(ByVal prov As IDataProvider)
        mSessionId = prov.GetValue("SessionId", Guid.Empty)
        mSessionNumber = prov.GetValue("SessionNumber", 0)
        mStartDateTime = prov.GetValue("StartDateTime", Date.MinValue)
        mEndDateTime = prov.GetValue("EndDateTime", Date.MaxValue)
        mProcessId = prov.GetValue("ProcessId", Guid.Empty)
        mProcessName = prov.GetValue("ProcessName", "")
        mStarterResourceId = prov.GetValue("StarterResourceId", Guid.Empty)
        mStarterUserId = prov.GetValue("StarterUserId", Guid.Empty)
        mRunningResourceId = prov.GetValue("RunningResourceId", Guid.Empty)
        mRunningResourceName = prov.GetValue("RunningResourceName", "")
        mRunningOSUserName = prov.GetValue("RunningOSUserName", "")
        mStatusId = prov.GetValue("StatusID", SessionStatus.All)
        mSessionLogMaxAttributeXmlLength = AttributeXmlMaxLength
        mStartTimezoneOffset = prov.GetValue("starttimezoneoffset", 0)
        mEndTimezoneOffset = prov.GetValue("endtimezoneoffset", 0)
    End Sub

    ''' <summary>
    ''' Empty constructor - only used when loading internally
    ''' </summary>
    Private Sub New()
        ' This is the only point at which this collection is created - ie, if 
        ' creating a log from a data provider, no collection exists, and any entry
        ' data is loaded in batches from the database. The existence of this
        ' collection overrides that behaviour (See GetEnumerator())
        mImportedEntries = New List(Of clsSessionLogEntry)
        mEndDateTime = Date.MaxValue
        mStatusId = SessionStatus.All
    End Sub

    ''' <summary>
    ''' Constructor for testing purposes - allows a session log and log entries to be
    ''' created and used for serialization/de-serialization tests.
    ''' </summary>
    ''' <param name="prov">The provider containing the data for this log</param>
    ''' <param name="entries">The associated list of log entries</param>
    Friend Sub New(prov As IDataProvider, entries As IList(Of clsSessionLogEntry))
        Me.New(prov)
        mImportedEntries = entries
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The session number of this log
    ''' </summary>
    Public ReadOnly Property SessionNumber() As Integer
        Get
            Return mSessionNumber
        End Get
    End Property

    ''' <summary>
    ''' The session ID of this log
    ''' </summary>
    Public ReadOnly Property SessionId() As Guid
        Get
            Return mSessionId
        End Get
    End Property

    ''' <summary>
    ''' The start date/time of this log
    ''' </summary>
    Public ReadOnly Property StartDateTime() As Date
        Get
            Return mStartDateTime
        End Get
    End Property

    ''' <summary>
    ''' Gets the time of day, to minute granularity, of this session
    ''' </summary>
    Public ReadOnly Property StartTimeOfDay() As TimeSpan
        Get
            Return New TimeSpan(
                mStartDateTime.TimeOfDay.Ticks \ TimeSpan.TicksPerMinute)
        End Get
    End Property

    ''' <summary>
    ''' The end date/time of this log
    ''' </summary>
    Public ReadOnly Property EndDateTime() As Date
        Get
            Return mEndDateTime
        End Get
    End Property

    ''' <summary>
    ''' The process ID of this primary process that this session represents
    ''' </summary>
    Public ReadOnly Property ProcessId() As Guid
        Get
            Return mProcessId
        End Get
    End Property

    ''' <summary>
    ''' The name of the process that was executed in this session
    ''' </summary>
    Public ReadOnly Property ProcessName() As String
        Get
            Return mProcessName
        End Get
    End Property

    ''' <summary>
    ''' The status of the session
    ''' </summary>
    Public ReadOnly Property Status() As SessionStatus
        Get
            Return mStatusId
        End Get
    End Property

    ''' <summary>
    ''' Checks if this session log represents a finished session, ie. whether it has
    ''' <see cref="SessionStatus.Completed">completed</see>,
    ''' <see cref="SessionStatus.Failed">failed</see>,
    ''' <see cref="SessionStatus.Stopped">stopped</see>,
    ''' <see cref="SessionStatus.Terminated">terminated</see> or
    ''' <see cref="SessionStatus.Debugging">ended debugging</see>
    ''' </summary>
    Public ReadOnly Property IsFinished() As Boolean
        Get
            Return (Status = SessionStatus.Completed OrElse
                    Status = SessionStatus.Failed OrElse
                    Status = SessionStatus.Stopped OrElse
                    Status = SessionStatus.Terminated OrElse
                    (Status = SessionStatus.Debugging AndAlso EndDateTime <> Date.MinValue))
        End Get
    End Property

    ''' <summary>
    ''' The ID of the resource which started this session
    ''' </summary>
    Public ReadOnly Property StarterResourceId() As Guid
        Get
            Return mStarterResourceId
        End Get
    End Property

    ''' <summary>
    ''' The ID of the user which started this session
    ''' </summary>
    Public ReadOnly Property StarterUserId() As Guid
        Get
            Return mStarterUserId
        End Get
    End Property

    ''' <summary>
    ''' The ID of the resource on which this session ran
    ''' </summary>
    Public ReadOnly Property RunningResourceId() As Guid
        Get
            Return mRunningResourceId
        End Get
    End Property

    ''' <summary>
    ''' The name of the resource on which this session ran
    ''' </summary>
    Public ReadOnly Property RunningResourceName() As String
        Get
            Return mRunningResourceName
        End Get
    End Property

    ''' <summary>
    ''' The operating system user name that this session ran under
    ''' </summary>
    Public ReadOnly Property RunningOSUserName() As String
        Get
            Return mRunningOSUserName
        End Get
    End Property

    Public Property SessionLogMaxAttributeXmlLength() As Integer
        Get
            Return mSessionLogMaxAttributeXmlLength
        End Get
        Set(value As Integer)
            mSessionLogMaxAttributeXmlLength = value
        End Set
    End Property

#End Region

#Region " Export file / Search directories for logs "

    ''' <summary>
    ''' Sanitizes the given string, ensuring that it contains a valid filename.
    ''' </summary>
    ''' <param name="name">The name to sanitize</param>
    ''' <returns>The given name with any invalid filename chars replaced with an 
    ''' underscore.</returns>
    Private Shared Function Sanitize(ByVal name As String) As String

        If name.StartsWith(" ") Then
            name = "_" & name
        End If
        If name.EndsWith(" ") Then
            name = name & "_"
        End If

        Return SanitizeFileNameRegex.Replace(name, "_")

    End Function

    ''' <summary>
    ''' Combines the given path elements into a single path.
    ''' </summary>
    ''' <param name="pathElements">The path elements which should be combined into
    ''' a single path.</param>
    ''' <returns>A path containing all the elements given.</returns>
    Private Function CombinePaths(ByVal ParamArray pathElements() As String) As String
        Dim accum As String = ""
        For Each el As String In pathElements
            accum = Path.Combine(accum, el)
        Next
        Return accum
    End Function

    ''' <summary>
    ''' Gets the directory structure and filename to use to export this session log.
    ''' Note that this does not include the archive output directory, just the path
    ''' below the output directory and the name of the generated file.
    ''' Also note that this may not be the final filename - if the exported log is
    ''' compressed, it will be suffixed with a ".gz" extension
    ''' </summary>
    Public ReadOnly Property ExportFilePath() As String
        Get
            Dim d As Date = mStartDateTime
            Return CombinePaths( _
             d.Year.ToString(), _
             d.ToString("MM MMMM"), _
             d.ToString("dd dddd"), _
             Sanitize(mProcessName), _
             mRunningResourceName.Replace(":"c, ";"c), _
 _
             d.ToString("yyyyMMdd HHmmss ") & mSessionId.ToString() & ".bpl" _
            )
        End Get
    End Property

    ''' <summary>
    ''' Class to compare two file infos - this is largely here to ensure a consistent
    ''' ordering for file paths, used when finding the log files in a directory.
    ''' </summary>
    Private Class FileInfoComparer : Implements IComparer(Of FileInfo)

        ''' <summary>
        ''' Compares the two fileinfo objects, returning a negative number, zero or
        ''' a positive number depending on whether <paramref name="x"/> is less than,
        ''' equal to or greater than <paramref name="y"/>, respectively.
        ''' This comparer just sorts on the full path and filename.
        ''' </summary>
        ''' <param name="x">The first FileInfo to check.</param>
        ''' <param name="y">The second FileInfo to check</param>
        ''' <returns>a negative number, zero or a positive number depending on
        ''' whether <paramref name="x"/> is less than, equal to or greater than
        ''' <paramref name="y"/>, respectively.</returns>
        Public Function Compare(ByVal x As FileInfo, ByVal y As FileInfo) As Integer _
         Implements IComparer(Of FileInfo).Compare
            Return x.FullName.CompareTo(y.FullName)
        End Function

    End Class

    ''' <summary>
    ''' Finds all log files below the given directory whose path names indicate
    ''' that the log falls within the specified date range
    ''' </summary>
    ''' <param name="root">The directory from which the files should be searched.
    ''' </param>
    ''' <param name="fromDate">The first date/time for which the log files are
    ''' required.</param>
    ''' <param name="toDate">The last date/time for which the log files are required.
    ''' </param>
    ''' <returns>A collection of FileInfo objects containing FileInfo's for all
    ''' files whose paths indicated their date fell within the desired range.
    ''' </returns>
    ''' <remarks>Two things to note :-<list>
    ''' <item>This only uses the dates embedded in the descendent directory names
    ''' to determine the 'date' of the log. The log data itself is not examined,
    ''' and thus may be different... though it shouldn't be in the usual running
    ''' of things.</item>
    ''' <item>The dates formed from the file paths have no time component, ie.
    ''' they are set to midnight on the date they represent. This should be
    ''' considered when creating the dates for the date range required.</item>
    ''' </list>
    ''' </remarks>
    Public Shared Function FindLogFiles(ByVal root As DirectoryInfo, _
     ByVal fromDate As Date, ByVal toDate As Date) As ICollection(Of FileInfo)

        Dim allFiles As New List(Of FileInfo)
        For Each yearDir As DirectoryInfo In root.GetDirectories()
            ' The year dir name is a simple number - just parse it and exclude
            ' any that fall outside the date range required.
            Dim year As Integer
            If Integer.TryParse(yearDir.Name, year) _
             AndAlso year >= fromDate.Year AndAlso year <= toDate.Year Then
                ' From here, we might as well build up the complete set of
                ' files and then exclude any which don't fit in the range.
                allFiles.AddRange(yearDir.GetFiles("*.*", SearchOption.AllDirectories))
            End If
        Next

        ' Okay, now 'files' contains all the files underneath any of the year folders
        ' which represent years which form part of the search range.

        ' Now we need to check each path to see if it falls inside the range or not.
        ' The following regex captures the year, month and day from the file path
        Dim rx As New Regex("^" & Regex.Escape(root.FullName) & _
         "\\([0-9]{4})\\([0-9]{2}) \p{L}+\\([0-9]{2}) \p{L}+\\.*\.bpl(?:\.gz)?$")

        Dim files As New clsSortedSet(Of FileInfo)(New FileInfoComparer())

        For Each file As FileInfo In allFiles
            Dim m As Match = rx.Match(file.FullName)
            If m.Success Then

                Dim dt As New Date( _
                 Integer.Parse(m.Groups(1).Value), _
                 Integer.Parse(m.Groups(2).Value), _
                 Integer.Parse(m.Groups(3).Value) _
                )

                If dt >= fromDate AndAlso dt <= toDate Then files.Add(file)

            End If
        Next
        Return files

    End Function

#End Region

#Region " Inserting a log "

    ''' <summary>
    ''' Sets this log's data into the given parameter collection.
    ''' </summary>
    ''' <param name="params">The parameter collection into which the data regarding
    ''' this log should be set.</param>
    Friend Sub SetInto(ByVal params As SqlParameterCollection)

        params.AddWithValue("@SessionId", mSessionId)
        params.AddWithValue("@StartDateTime", mStartDateTime)
        params.AddWithValue("@EndDateTime", _
         IIf(mEndDateTime = Date.MaxValue, DBNull.Value, mEndDateTime))
        params.AddWithValue("@ProcessId", mProcessId)
        params.AddWithValue("@StarterResourceId", mStarterResourceId)
        params.AddWithValue("@StarterUserId", mStarterUserId)
        params.AddWithValue("@RunningResourceId", mRunningResourceId)
        params.AddWithValue("@RunningOSUserName", mRunningOSUserName)
        params.AddWithValue("@StatusID",
         IIf(mStatusId = SessionStatus.All, DBNull.Value, mStatusId))
        params.AddWithValue("@starttimezoneoffset", mStartTimezoneOffset)
        params.AddWithValue("@endtimezoneoffset", mEndTimezoneOffset)
    End Sub

#End Region

#Region " Importing "

    ''' <summary>
    ''' Flushes the current set of imported log entries to the databases, and clears
    ''' those entries.
    ''' </summary>
    ''' <exception cref="SqlException">Any database errors which occur while
    ''' attempting to flush the data to the database will be thrown by this method.
    ''' </exception>
    Private Sub FlushToDatabase()
        gSv.RestoreSessionLogData(mImportedEntries.ToList())
        mImportedEntries.Clear()
    End Sub

    ''' <summary>
    ''' Imports the data from the given file into the database.
    ''' Note that if the filename ends with the extension ".gz", it is assumed to
    ''' be a compressed file and will be treated as such.
    ''' </summary>
    ''' <param name="file">The abstract representation of the file from which the
    ''' session log data should be loaded.</param>
    ''' <exception cref="Exception">Any exceptions thrown by the underlying code
    ''' (either loading the file or writing to the database) are unhandled by this
    ''' method.</exception>
    Public Shared Sub ImportFrom(ByVal file As FileInfo)
        Dim s As Stream = New FileStream(file.FullName, FileMode.Open)
        If file.Name.EndsWith(".gz") Then s = New GZipStream(s, CompressionMode.Decompress)
        Using s
            ImportFrom(s)
        End Using
    End Sub

    ''' <summary>
    ''' Imports the data from the given stream into the database.
    ''' </summary>
    ''' <param name="inStream">The stream from which the data should be imported.
    ''' </param>
    ''' <exception cref="Exception">Any exceptions thrown by the underlying code
    ''' (either parsing the XML or writing to the database) are unhandled by this
    ''' method.</exception>
    Public Shared Sub ImportFrom(ByVal inStream As Stream)

        Dim log As New clsSessionLog()
        Dim proc As String = Nothing, page As String = Nothing
        Dim sessNo As Integer = 0 ' The new session number for the session after restoration
        Dim versionNo As Integer = -1

        Dim settings As New XmlReaderSettings()
        settings.CloseInput = False
        settings.IgnoreWhitespace = True
        ' We cheerfully write out invalid characters in our session logs (binary
        ' results stored in text data items); they should really be base64-encoded
        ' or such like, but until they are, we need to be able to read them back in
        settings.CheckCharacters = False

        ' Create an XmlReader and step through all the elements.
        Using r As XmlReader = XmlReader.Create(inStream, settings)

            While r.Read()

                Select Case r.NodeType

                    Case XmlNodeType.Element ' Effectively "StartElement"
                        Select Case r.Name

                            Case "DataSet"
                                ' This means it's an old version of the exported log, we need
                                ' to get this in a different way.
                                ' Set the version number to zero and come out of the read loop
                                versionNo = 0
                                Exit While

                            Case "session-log"
                                versionNo = Integer.Parse(r.GetAttribute("version"))
                                log.ReadFrom(r)
                                ' Restore the log on the database, and set the session number
                                sessNo = gSv.RestoreSessionLog(log)

                            Case "process" : proc = r.GetAttribute("name")
                            Case "page" : page = r.GetAttribute("name")

                            Case "entry"
                                log.mImportedEntries.Add( _
                                 clsSessionLogEntry.ReadFrom(r, sessNo, proc, page))
                                If log.mImportedEntries.Sum(Function(s) s.AttributeSize) > BatchSize Then
                                    log.FlushToDatabase()
                                End If

                        End Select

                    Case XmlNodeType.EndElement
                        Select Case r.Name
                            Case "page" : page = Nothing
                            Case "process" : proc = Nothing
                            Case "session-log-entries" : log.FlushToDatabase()
                        End Select


                End Select

            End While

        End Using

        If versionNo = 0 Then
            ' The file was a serialized DataSet - ie. a file from the old method of 
            ' exporting the session logs.
            ' Return to the beginning of the file, and open a new reader - create a
            ' DataSet from the serialized XML, and import the log from that.
            inStream.Seek(0, SeekOrigin.Begin)
            Using r As XmlReader = XmlReader.Create(inStream, settings)
                Dim ds As New DataSet()
                ds.ReadXml(r)
                gSv.ArchiveRestoreFromDataSet(ds)
            End Using
        End If

    End Sub

    ''' <summary>
    ''' Reads this logs data from the attributes in the current location of the given
    ''' XmlReader. This assumes that the reader is before the first attribute, ie.
    ''' the element has just been moved to.
    ''' </summary>
    ''' <param name="r">The XML reader from which this log's data should be read in
    ''' the form of its attributes.</param>
    ''' <remarks></remarks>
    Private Sub ReadFrom(ByVal r As XmlReader)

        ' First, read this entry's data from the attributes of the current XML element.
        ' Get to the first attribute, and go from there...
        If Not r.MoveToFirstAttribute() Then
            Throw New InvalidOperationException( _
             "No attributes to read log data from: Current name: " & r.Name)
        End If

        ' Go through all the attributes in the reader's current element, and set
        ' this log's data from them.
        Do
            Select Case r.Name
                Case "id" : mSessionId = New Guid(r.Value)
                Case "no" : mSessionNumber = Integer.Parse(r.Value)
                Case "start" : mStartDateTime = Date.ParseExact(r.Value, DateFormat, Nothing)
                Case "end" : mEndDateTime = Date.ParseExact(r.Value, DateFormat, Nothing)
                Case "process" : mProcessId = New Guid(r.Value)
                Case "starter-resource" : mStarterResourceId = New Guid(r.Value)
                Case "starter-user" : mStarterUserId = New Guid(r.Value)
                Case "running-resource" : mRunningResourceId = New Guid(r.Value)
                Case "running-os-user" : mRunningOSUserName = r.Value
                Case "status" : mStatusId = clsEnum(Of SessionStatus).Parse(r.Value, False)
                Case "starttimezoneoffset" : mStartTimezoneOffset = Integer.Parse(r.Value)
                Case "endtimezoneoffset" : mendTimezoneOffset = Integer.Parse(r.Value)
            End Select
        Loop While r.MoveToNextAttribute()

    End Sub

#End Region

#Region " Exporting "

    ''' <summary>
    ''' Exports this log into the given directory, compressing it as it goes.
    ''' Note that the full directory path defined in <see cref="ExportFilePath"/>
    ''' is created below the given directory in order to export the log.
    ''' </summary>
    ''' <param name="dir">The base directory into which this log should be exported.
    ''' </param>
    ''' <returns>The file into which the log was exported.</returns>
    Public Function ExportTo(ByVal dir As DirectoryInfo) As FileInfo
        Return ExportTo(New FileInfo(Path.Combine(dir.FullName, Me.ExportFilePath)), True)
    End Function

    ''' <summary>
    ''' Exports this log into the given file.
    ''' </summary>
    ''' 
    ''' <param name="file">The file to which this log should be saved.</param>
    ''' <param name="compress">True to compress the log output on the fly, false
    ''' to write the file uncompressed. Note that if this parameter is true, the
    ''' file written to will end with the extension ".gz" to indicate that it is
    ''' a gzip-compressed file. If the given filename does not already have that
    ''' extension, it will be added by this method.</param>
    ''' 
    ''' <returns>The info regarding the file that was written to by this method,
    ''' which may be different to the file passed in.</returns>
    ''' 
    ''' <exception cref="ArgumentNullException">If the given file was null.
    ''' </exception>
    ''' <exception cref="FileAlreadyExistsException">If the output file already
    ''' exists and therefore cannot be written to.</exception>
    ''' <exception cref="IOException">If any other IO errors occur while attempting
    ''' to write the log file.</exception>
    Public Function ExportTo(ByVal file As FileInfo, ByVal compress As Boolean) As FileInfo

        If file Is Nothing Then Throw New ArgumentNullException(NameOf(file))

        If compress AndAlso Not file.Name.ToLower().EndsWith(".gz") Then
            file = New FileInfo(file.FullName & ".gz")
        End If

        If file.Exists Then Throw New FileAlreadyExistsException(file)

        If Not file.Directory.Exists Then file.Directory.Create()

        Dim os As Stream = Nothing
        Try
            os = New FileStream(file.FullName, FileMode.CreateNew)
            If compress Then os = New GZipStream(os, CompressionMode.Compress)
            ExportTo(os)

        Finally
            If os IsNot Nothing Then os.Close()

        End Try

        Return file

    End Function

    ''' <summary>
    ''' Exports this session log to the given stream, leaving the stream open and
    ''' intact while doing so.
    ''' </summary>
    ''' <param name="out">The (output) stream to which this log should be exported.
    ''' This should be initialised and ready to be written to before calling this
    ''' method, and it will remain open when this method returns.</param>
    Public Sub ExportTo(ByVal out As Stream)

        Dim settings As New XmlWriterSettings()

        ' Go go UTF8
        settings.Encoding = Encoding.UTF8

        ' Don't close the stream when the writer closes
        settings.CloseOutput = False

        ' Indentation settings. Can be safely removed - only here for ease of dev / testing
        ' FIXME: Remove indentation from clsSessionLog.ExportTo()
        settings.Indent = True
        settings.IndentChars = vbTab

        Using writer = XmlWriter.Create(out, settings)

            writer.WriteStartDocument()

            ' session-log element
            writer.WriteStartElement("session-log")
            writer.WriteAttributeString("version", "1") ' version number of this exported file
            writer.WriteAttributeString("id", mSessionId.ToString())
            writer.WriteAttributeString("no", mSessionNumber.ToString())
            If mStartDateTime <> Date.MinValue Then
                writer.WriteAttributeString("start", mStartDateTime.ToString(DateFormat))
            End If
            If mEndDateTime <> Date.MaxValue Then
                writer.WriteAttributeString("end", mEndDateTime.ToString(DateFormat))
            End If
            writer.WriteAttributeString("process", mProcessId.ToString())
            writer.WriteAttributeString("starter-resource", mStarterResourceId.ToString())
            writer.WriteAttributeString("starter-user", mStarterUserId.ToString())
            writer.WriteAttributeString("running-resource", mRunningResourceId.ToString())
            writer.WriteAttributeString("running-os-user", mRunningOSUserName)
            writer.WriteAttributeString("status", mStatusId.ToString())
            writer.WriteAttributeString("starttimezoneoffset", mStartTimezoneOffset.ToString())
            writer.WriteAttributeString("endtimezoneoffset", mEndTimezoneOffset.ToString())

            ' log entries
            Dim procName As String = Nothing, pageName As String = Nothing

            writer.WriteStartElement("session-log-entries")

            Dim builder = New System.Text.StringBuilder()

            For Each entry As clsSessionLogEntry In Me

                builder.Clear()

                If entry.AttributeXmlGzip IsNot Nothing Then

                    DecompressAndStringBuild(entry.AttributeXmlGzip, builder)

                    If entry.AttributeSize > clsSessionLog.BatchSize Then

                        Dim length = entry.AttributeSize
                        Dim bytesLeft = entry.AttributeSize
                        Dim batchSize As Integer = clsSessionLog.BatchSize

                        Dim index As Long = batchSize + 1    'deliberate, starts at 1 !
                        While index <= length

                            bytesLeft = bytesLeft - batchSize
                            If bytesLeft < batchSize Then
                                batchSize = CType(bytesLeft, Integer)
                            End If

                            Dim attxmlList = gSv.GetSessionLogAttributeXml(Me.SessionNumber, entry.LogId, index, batchSize)

                            If attxmlList Is Nothing Or attxmlList.FirstOrDefault() Is Nothing Or attxmlList.FirstOrDefault().AttributeXmlGzip Is Nothing Then Exit While

                            DecompressAndStringBuild(attxmlList.FirstOrDefault().AttributeXmlGzip, builder)

                            index = index + batchSize
                        End While

                    End If

                    entry.AttributeXml = builder.ToString()
                End If

                builder.Clear()
                entry.ExportTo(writer, procName, pageName)
            Next
            ' Close the page and process elements, if they are open.
            If pageName IsNot Nothing Then writer.WriteEndElement()
            If procName IsNot Nothing Then writer.WriteEndElement()

            writer.WriteEndElement()

            writer.WriteEndDocument()

            writer.Flush()

        End Using

    End Sub

    Private Sub DecompressAndStringBuild(ByRef byteArray As Byte(), ByRef builder As StringBuilder)
        Using memory = New System.IO.MemoryStream(byteArray)
            Using gzip = New System.IO.Compression.GZipStream(memory, System.IO.Compression.CompressionMode.Decompress)
                Using reader = New StreamReader(gzip, Encoding.UTF8)
                    builder.Append(reader.ReadToEnd())
                End Using
            End Using
        End Using
    End Sub

#End Region

#Region " Enumerator class and IEnumerable implementation "

    ''' <summary>
    ''' Enumerates over all the session log entries for a particular session log.
    ''' </summary>
    Private Class EntryEnumerator : Implements IEnumerator(Of clsSessionLogEntry)

        ' The session number of the log whose entries are being iterated over
        Private mSessNo As Integer

        ' The last logid retrieved, -1 if the enumerator is unstarted,
        Private mLastLogId As Long

        ' The iterator over the returned batch of log entries from the server.
        Private mIter As IEnumerator(Of clsSessionLogEntry)

        Private mSessionLogMaxAttributeXmlLength As Integer

        ''' <summary>
        ''' Creates a new entry enumerator over the session log with the given
        ''' session number.
        ''' </summary>
        ''' <param name="sessionNumber">The session number of the log for which
        ''' the entries should be enumerated.</param>
        Public Sub New(ByVal sessionNumber As Integer, SessionLogMaxAttributeXmlLength As Integer)
            mSessNo = sessionNumber
            mSessionLogMaxAttributeXmlLength = SessionLogMaxAttributeXmlLength
            ' Set our 'unstarted' values...
            Reset()
        End Sub

        ''' <summary>
        ''' Gets the current session log entry.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">If the enumerator is 
        ''' before the first element or after the last element.</exception>
        Public ReadOnly Property Current() As clsSessionLogEntry _
         Implements IEnumerator(Of clsSessionLogEntry).Current
            Get
                If mIter Is Nothing Then _
                 Throw New InvalidOperationException("Enumerator has no current element")

                Return mIter.Current
            End Get
        End Property

        ''' <summary>
        ''' Gets the current session log entry.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">If the enumerator is 
        ''' before the first element or after the last element.</exception>
        Private ReadOnly Property Current1() As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        ''' <summary>
        ''' Attempts to move to the next log entry returning an indicator of 
        ''' its success or otherwise.
        ''' </summary>
        ''' <returns>True if the enumerator successfully moved to the next
        ''' element that it is iterating over; False if there are no more
        ''' elements to iterate over.</returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext

            ' iter is dead and last-seq is at end... nothing left to see here.
            If mIter Is Nothing AndAlso mLastLogId = Long.MaxValue Then Return False

            ' iter is available and has more to offer...
            If mIter IsNot Nothing AndAlso mIter.MoveNext() Then
                mLastLogId = mIter.Current.LogId
                Return True
            End If

            ' Otherwise either :-
            ' - iter is null and lastLogId < Long.MaxValue - ie. it's unstarted... or
            ' - iter is non-null but MoveNext() returned false - ie. end of current batch.
            ' Either way, get the next batch of log entries and store the iterator for them.
            Dim coll = gSv.GetSessionLogData(mSessNo, mLastLogId, clsSessionLog.BatchSize, mSessionLogMaxAttributeXmlLength)

            ' If no entries returned, that's our EOF marker...
            If coll.Count = 0 Then
                mIter = Nothing
                mLastLogId = Long.MaxValue
                Return False
            End If

            ' Otherwise, set up the iterator and get it to move to the first value.
            mIter = coll.GetEnumerator()

            ' We already know the collection is non-empty, so this shouldn't need an If..Then
            mIter.MoveNext()
            mLastLogId = mIter.Current.LogId
            Return True

        End Function

        ''' <summary>
        ''' Resets this enumerator such that it will again iterate over
        ''' the session log's entries from the beginning.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            mIter = Nothing
            mLastLogId = -1
        End Sub

        ''' <summary>
        ''' Disposes of this object.
        ''' Since this has no managed or unmanaged resources per se, it doesn't do
        ''' a whole lot.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Er... well, it removes one reference, I guess...
            mIter = Nothing
        End Sub

    End Class

    ''' <summary>
    ''' Gets an enumerator over all the log entries in this log.
    ''' This will enumerate over any imported entries, if there are any. Otherwise,
    ''' it will retrieve entries for this log from the database, retrieving them
    ''' in batches as necessary.
    ''' </summary>
    ''' <returns>An enumerator which iterates over all the session log entries
    ''' for this session log.</returns>
    ''' <remarks>This is overridable primarily so that test classes can write an
    ''' override for this method which draws its entries from predefined test data
    ''' rather than from the database.</remarks>
    Public Overridable Function GetEnumerator() As IEnumerator(Of clsSessionLogEntry) _
     Implements IEnumerable(Of clsSessionLogEntry).GetEnumerator
        If mImportedEntries IsNot Nothing Then
            Return mImportedEntries.GetEnumerator()
        Else
            Return New EntryEnumerator(Me.mSessionNumber, Me.mSessionLogMaxAttributeXmlLength)
        End If
    End Function

    ''' <summary>
    ''' Gets an enumerator over all the log entries in this log
    ''' </summary>
    ''' <returns>An enumerator which iterates over all the session log entries
    ''' for this session log.</returns>
    Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

#End Region

End Class
