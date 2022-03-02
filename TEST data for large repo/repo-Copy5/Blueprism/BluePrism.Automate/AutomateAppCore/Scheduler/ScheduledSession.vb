Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Scheduling.ScheduleData

''' Project  : AutomateAppCore
''' Class    : clsScheduledSession
''' <summary>
''' Class defining a session which is scheduled to run at some point
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
<KnownType(GetType(clsOrderedDictionary(Of Guid, RunnerStatus)))>
Public Class ScheduledSession
    Implements ISession

#Region "SessionIdentifier Structure"

    ''' <summary>
    ''' Structure to combine both types of identifier for a session, so they can
    ''' be used interchangeably with minimal fuss
    ''' </summary>
    <Serializable()> _
    Private Structure SessionIdentifier

        ' The GUID session ID for the identified session.
        Private sessionId As Guid

        ' The numeric session number for the identified session.
        Private sessionNo As Integer

        ''' <summary>
        ''' Creates a new session identifier for the given GUID.
        ''' This will retrieve the corresponding session number from the database.
        ''' </summary>
        ''' <param name="id">The session ID to create.</param>
        Public Sub New(ByVal id As Guid)
            Me.New(id, gSv.GetSessionScheduleNumber(id))
        End Sub

        ''' <summary>
        ''' Creates a new session identifier for the given number.
        ''' This will retrieve the corresponding session ID from the database.
        ''' </summary>
        ''' <param name="num">The session number to create an identifier for.</param>
        Public Sub New(ByVal num As Integer)
            Me.New(gSv.GetSessionID(num), num)
        End Sub

        ''' <summary>
        ''' Creates a new session identifier for the given ID and number.
        ''' </summary>
        ''' <param name="id">The GUID representing the session ID that this
        ''' identifier contains</param>
        ''' <param name="num">The integer representing the session number that this
        ''' identifier contains.</param>
        Public Sub New(ByVal id As Guid, ByVal num As Integer)
            sessionId = id
            sessionNo = num
        End Sub

        ''' <summary>
        ''' The GUID-based session ID for the session identified by this object.
        ''' </summary>
        Public ReadOnly Property ID() As Guid
            Get
                Return sessionId
            End Get
        End Property

        ''' <summary>
        ''' The int-based session number for the session identifier by this object.
        ''' </summary>
        Public ReadOnly Property Number() As Integer
            Get
                Return sessionNo
            End Get
        End Property

    End Structure

#End Region

#Region "Member Variables"

    ' The unique ID for this session definition
    <DataMember>
    Private mId As Integer

    ' The process to be run in this session
    <DataMember>
    Private mProcessId As Guid
    ' A cached version of the process name - weak rather than solid (like resource
    ' name is) because the process name can change, so this should be allowed to
    ' expire so we can pick up the new name.
    Private mProcessNameRef As New WeakReference(Nothing)

    ' The resource on which this session will occur
    <DataMember>
    Private mResourceId As Guid
    <DataMember>
    Private mResourceName As String ' lazily populated from the database when accessed...

    ' The parameters to send to the process
    <DataMember>
    Private mArguments As clsArgumentList

    ' session ID and session number of the session log created by this session -
    ' held together to ensure that they are kept in step with each other.
    <DataMember>
    Private mSession As SessionIdentifier

    ' Due to the asynchronous nature of the connections to resource PCs, the
    ' session needs to store if a session creation has failed. This may be
    ' rendered unnecessary if a 'synchronous' mode is added to the resource
    ' connection manager class
    <DataMember>
    Private mErrorMessage As String

    ' Supplementary data regarding the failure to create a session.
    ' Currently only used for resources which are 'too busy' to provide a
    ' report of what they are busy with.
    <DataMember>
    Private mData As Object

    ' A reference to the process so that once it has been loaded, it need
    ' not be loaded again 
    <NonSerialized()> _
    Private mProcess As clsProcess

#End Region

#Region "Properties"

    ''' <summary>
    ''' The unique ID of this scheduled session
    ''' </summary>
    Public ReadOnly Property Id() As Integer
        Get
            Return mId
        End Get
    End Property

    ''' <summary>
    ''' The ID of the process which is to run in this session
    ''' </summary>
    Public Property ProcessId() As Guid Implements ISession.ProcessID
        Get
            Return mProcessId
        End Get
        Set(ByVal value As Guid)
            mProcessId = value
        End Set
    End Property

    ''' <summary>
    ''' The name of the process that this session refers to.
    ''' </summary>
    Public ReadOnly Property ProcessName() As String
        Get
            If mProcessId = Nothing Then Return ""
            Dim procName As String = Nothing
            If mProcessNameRef IsNot Nothing Then
                procName = DirectCast(mProcessNameRef.Target, String)
            End If
            If procName Is Nothing Then
                procName = gSv.GetProcessNameByID(mProcessId)
                mProcessNameRef = New WeakReference(procName)
            End If
            Return procName
        End Get
    End Property

    ''' <summary>
    ''' The ID of the resource which this session runs on
    ''' </summary>
    Public ReadOnly Property ResourceId() As Guid
        Get
            Return mResourceId
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the resource by name which this session runs on
    ''' </summary>
    Public ReadOnly Property ResourceName() As String Implements ISession.ResourceName
        Get
            Return mResourceName
        End Get
    End Property

    ''' <summary>
    ''' The XML defining the arguments which are passed to the process
    ''' when this scheduled session is activated.
    ''' </summary>
    Public Property ArgumentsXML() As String
        Get
            Return Arguments.ArgumentsToXML(False)
        End Get
        Set(ByVal value As String)
            Arguments = clsArgumentList.XMLToArguments(value, False)
        End Set
    End Property

    ''' <summary>
    ''' The arguments which are passed into the process as an ArgumentList.
    ''' This cannot be set directly, but it can be modified through this
    ''' property, and the resultant XML property <see cref="ArgumentsXML"/>
    ''' will be updated appropriately
    ''' </summary>
    Public Property Arguments() As clsArgumentList Implements ISession.Arguments
        Get
            If mArguments Is Nothing Then mArguments = New clsArgumentList()
            Return mArguments
        End Get
        Set(ByVal value As clsArgumentList)
            mArguments = value
        End Set
    End Property

    ''' <summary>
    ''' The sequence number representing the identity of the session log which
    ''' was created as a result of this session
    ''' </summary>
    Public Property SessionNumber() As Integer
        Get
            Return mSession.Number
        End Get
        Set(ByVal value As Integer)
            If value <= 0 Then
                mSession = Nothing ' Reset to default values
            Else
                mSession = New SessionIdentifier(value)
            End If
        End Set
    End Property

    ''' <summary>
    ''' The GUID representing the ID of the session log which was created as a
    ''' result of this session.
    ''' </summary>
    Public Property SessionId() As Guid
        Get
            Return mSession.ID
        End Get
        Set(ByVal value As Guid)
            If value = Nothing Then
                mSession = Nothing ' Reset to default values
            Else
                mSession = New SessionIdentifier(value)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Error message which occurred when the session described by this object
    ''' was attempted to be created. Null if creation of the session has not been
    ''' attempted or resulted in no errors from the specified resource.
    ''' </summary>
    Public Property ErrorMessage() As String
        Get
            Return mErrorMessage
        End Get
        Set(ByVal value As String)
            If value = "" Then mErrorMessage = Nothing Else mErrorMessage = value
        End Set
    End Property

    ''' <summary>
    ''' The diagnostic data around this session. Null if this session has not
    ''' errored in creation, or if no diag data exists
    ''' </summary>
    Public Property Data() As Object
        Get
            Return mData
        End Get
        Set(ByVal value As Object)
            If Object.Equals(value, "") Then mData = Nothing Else mData = value
        End Set
    End Property

    ''' <summary>
    ''' Signifys if the user has permission to see the process
    ''' </summary>
    ''' <returns></returns>
    <DataMember>
    Public Property CanCurrentUserSeeProcess As Boolean = True

    ''' <summary>
    ''' Signifys if the user has permission to see the resource
    ''' </summary>
    ''' <returns></returns>
    <DataMember>
    Public Property CanCurrentUserSeeResource As Boolean = True

#End Region

#Region "Constructor(s)"

    Public Sub New(sessionData As ScheduleTaskSessionDatabaseData)
        Me.New(
         sessionData.Id,
         sessionData.ProcessId,
         sessionData.ResourceName,
         sessionData.ResourceId,
         sessionData.CanCurrentUserSeeProcess,
         sessionData.CanCurrentUserSeeResource,
         clsArgumentList.XMLToArguments(sessionData.ProcessParams, False)
        )
    End Sub


    Public Sub New(
     id As Integer,
     procId As Guid,
     resName As String,
     resId As Guid,
     canSeeProcess As Boolean,
     canSeeResource As Boolean,
     params As clsArgumentList)

        mId = id
        mProcessId = procId
        mResourceName = resName
        mResourceId = resId
        CanCurrentUserSeeProcess = canSeeProcess
        CanCurrentUserSeeResource = canSeeResource

        ' If null, create a new one for use in this object... otherwise, use what was passed
        If params Is Nothing Then mArguments = New clsArgumentList() Else mArguments = params

    End Sub

#End Region

#Region "Object Overrides"

    ''' <summary>
    ''' Gets a string representation of this scheduled session
    ''' </summary>
    ''' <returns>A string detailing this session</returns>
    Public Overrides Function ToString() As String
        Return String.Format(My.Resources.ScheduledSession_Process0OnResource1, ProcessId, ResourceName)
    End Function

    ''' <summary>
    ''' Gets a hash for this scheduled session - this is just a function
    ''' of the process Id and resource Id.
    ''' </summary>
    ''' <returns>An integer hash for this value</returns>
    Public Overrides Function GetHashCode() As Integer

        ' Just a bit of xoring on the side
        Return ProcessId.GetHashCode() Xor ResourceId.GetHashCode()

    End Function

    ''' <summary>
    ''' Checks if this object is equal to the given object.
    ''' See Equals(ISession) for more details as to what makes an object equal
    ''' to a clsScheduledSession instance.
    ''' </summary>
    ''' <param name="obj">The object to check for equality against.</param>
    ''' <returns>True if the given object is equal to this one. False otherwise.
    ''' </returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return Equals(TryCast(obj, ScheduledSession))
    End Function

#End Region

#Region "Public Methods"

    ''' <summary>
    ''' Checks if this scheduled session has a created session log number set
    ''' into it.
    ''' </summary>
    ''' <returns>True if this scheduled session has a non-zero Session Log
    ''' Number, indicating that the session log has been created for this
    ''' scheduled session; False otherwise.</returns>
    Public Function HasSessionNumber() As Boolean
        Return SessionNumber > 0
    End Function

    ''' <summary>
    ''' Resets the session log number held in this scheduled session object.
    ''' This has the effect of also clearing any error message which may have
    ''' been set on creation of the session.
    ''' </summary>
    Public Sub ClearSessionData()
        SessionNumber = 0
        mErrorMessage = Nothing
        mData = Nothing
    End Sub

    ''' <summary>
    ''' Checks if an error message has been recorded on this session.
    ''' </summary>
    ''' <returns>True if there is an error message on this session, false
    ''' otherwise.</returns>
    Public Function HasErrorMessage() As Boolean
        Return (mErrorMessage IsNot Nothing)
    End Function

    ''' <summary>
    ''' Checks if diagnostic data has been recorded on this session.
    ''' </summary>
    ''' <returns>True if there is an error message on this session, false
    ''' otherwise.</returns>
    Public Function HasData() As Boolean
        Return (mData IsNot Nothing)
    End Function

#End Region

    ''' <summary>
    ''' Creates a deep clone of the session
    ''' </summary>
    ''' <returns>A clone of the session</returns>
    Public Function Clone() As Object Implements ICloneable.Clone
        Dim session As ISession = TryCast(MemberwiseClone(), ISession)

        If Me.Arguments Is Nothing Then
            session.Arguments = Nothing
        Else
            session.Arguments = New clsArgumentList
            For Each p As clsArgument In Me.Arguments
                session.Arguments.Add(p.Clone())
            Next
        End If

        Return session
    End Function

    ''' <summary>
    ''' Gets the start parameters from the process xml
    ''' </summary>
    ''' <returns>A list of startup parameters for the process in the session</returns>
    Public Function GetStartParameters() As List(Of clsProcessParameter) Implements ISession.GetStartParameters
        If mProcess Is Nothing Then

            Dim processXML As String = gSv.GetProcessXML(mProcessId)
            Dim sErr As String = Nothing
            mProcess = clsProcess.FromXML(Options.Instance.GetExternalObjectsInfo, processXML, False, sErr)
            If mProcess Is Nothing Then
                Throw New InvalidOperationException(sErr)
            End If
        End If

        Dim startStageID As Guid = mProcess.GetStartStage()
        If startStageID = Guid.Empty Then Return Nothing

        Dim startStage As clsProcessStage = mProcess.GetStage(startStageID)
        If startStage Is Nothing Then Return Nothing

        Return startStage.GetParameters()
    End Function

    ''' <summary>
    ''' Checks if the given object is equal to this scheduled session
    ''' </summary>
    ''' <param name="other">The object to check to see if it is equal to
    ''' this session or not.</param>
    ''' <returns>True if the given object is a ScheduledSession with the
    ''' same values set in it as this session; False otherwise</returns>
    Public Overloads Function Equals(ByVal other As ISession) As Boolean Implements IEquatable(Of ISession).Equals
        Dim sess As ScheduledSession = TryCast(other, ScheduledSession)
        Return (sess IsNot Nothing AndAlso _
         sess.Id = Me.Id AndAlso _
         sess.ProcessId = Me.ProcessId AndAlso _
         sess.ResourceName = Me.ResourceName AndAlso _
         sess.ArgumentsXML = Me.ArgumentsXML)
    End Function
End Class

