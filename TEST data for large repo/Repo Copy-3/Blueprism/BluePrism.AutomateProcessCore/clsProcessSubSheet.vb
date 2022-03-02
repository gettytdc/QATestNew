Imports System.Drawing
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessSubSheet
''' 
''' <summary>
''' This class is used internally to represent a subsheet. There must
''' always be a corresponding 'SubSheetInfo' stage for each of these.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsProcessSubSheet

    ''' <summary>
    ''' Holds a reference to the parent of this subsheet.
    ''' </summary>
    <NonSerialized> 'TODO: Change in the future once clsProcess is fully serializable.
    Private mParent As clsProcess

    <DataMember>
    Private mId As Guid

    <DataMember>
    Private msName As String

    <DataMember>
    Private msngZoom As Single

    <DataMember>
    Private msngCameraX As Single

    <DataMember>
    Private msngCameraY As Single

    ''' <summary>
    ''' Holds a value indicating what type of sheet this is. This is currently only 
    ''' applicable to documents in object studio
    ''' </summary>
    <DataMember>
    Private mtSheetType As SubsheetType

    ''' <summary>
    ''' Holds a reference to a value indicating whether the subsheet has been published
    ''' or not. This is currently only applicable to documents in object studio
    ''' </summary>
    <DataMember>
    Private mbPublished As Boolean

    ''' <summary>
    ''' Provides access to the name of the subsheet
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Name() As String
        Get
            Return msName
        End Get
        Set(ByVal value As String)
            msName = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the ID of the subsheet
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ID() As Guid
        Get
            Return mId
        End Get
        Set(ByVal value As Guid)
            mId = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the zoom level of the subsheet
    ''' </summary>
    Public Property Zoom() As Single
        Get
            Return msngZoom
        End Get
        Set(ByVal value As Single)
            msngZoom = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the zoom for this subsheet as a percentage.
    ''' </summary>
    Public Property ZoomPercent As Integer
        Get
            Return CInt(100.0! * (Zoom / clsProcess.ScaleFactor))
        End Get
        Set(value As Integer)
            Zoom = (CSng(value) * clsProcess.ScaleFactor) / 100.0!
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the X coord of the camera for this sheet
    ''' </summary>
    Public Property CameraX() As Single
        Get
            Return msngCameraX
        End Get
        Set(ByVal value As Single)
            msngCameraX = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the Y coord of the camera for this sheet
    ''' </summary>
    Public Property CameraY() As Single
        Get
            Return msngCameraY
        End Get
        Set(ByVal value As Single)
            msngCameraY = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the location of the camera for this sheet
    ''' </summary>
    Public ReadOnly Property Camera() As PointF
        Get
            Return New PointF(CameraX, CameraY)
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the type of the subsheet. This is currently only
    ''' applicable to documents in object studio
    ''' </summary>
    ''' <value></value>
    Public Property SheetType() As SubsheetType
        Get
            Return mtSheetType
        End Get
        Set(ByVal Value As SubsheetType)
            mtSheetType = Value
        End Set
    End Property

    ''' <summary>
    ''' Indicates that this sheet is 'normal', ie. not a Main Page or Clean Up sheet.
    ''' A 'Capability' sheet (which appears to be entirely unused in the code as far
    ''' as I can tell) is treated as normal, largely because that's what the UI which
    ''' was testing for normal sheets was doing.
    ''' </summary>
    Public ReadOnly Property IsNormal() As Boolean
        Get
            Return (mtSheetType = SubsheetType.Normal _
             OrElse mtSheetType = SubsheetType.Capability)
        End Get
    End Property

    ''' <summary>
    ''' Gets the index within the parent process's subsheets where this subsheet
    ''' resides, or -1 if it has no parent process or if the parent process has
    ''' no record of it. Which should never happen. That would be bad.
    ''' </summary>
    Public ReadOnly Property Index() As Integer
        Get
            If mParent Is Nothing Then Return -1
            Return mParent.GetSubSheetIndex(ID)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this subsheet exists in a VBO.
    ''' </summary>
    Private ReadOnly Property IsObject As Boolean
        Get
            Return (mParent IsNot Nothing AndAlso
                    mParent.ProcessType = DiagramType.Object)
        End Get
    End Property

    ''' <summary>
    ''' Provides access to a value indicating whether the subsheet has been published 
    ''' or not. This is currently only applicable to documents in object studio
    ''' </summary>
    ''' <remarks>Note that Published will always return false for processes (ie. not
    ''' VBOs), and will always return true for an object's "Initialise" and
    ''' "Clean Up" sub-pages.</remarks>
    Public Property Published() As Boolean
        Get
            ' If it's not a VBO, then it cannot be published
            If Not IsObject Then Return False

            ' "CleanUp" pages are always published
            If mtSheetType = SubsheetType.CleanUp Then Return True

            ' As are "Initialise" pages
            If mtSheetType = SubsheetType.MainPage Then Return True

            ' Otherwise, we delegate to the user-set flag
            Return mbPublished
        End Get
        Set(ByVal Value As Boolean)
            mbPublished = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets the start stage for this subsheet
    ''' </summary>
    Public ReadOnly Property StartStage() As clsStartStage
        Get
            If mParent Is Nothing Then Return Nothing
            Return TryCast(mParent.GetStage(mParent.GetSubSheetStartStage(mId)), clsStartStage)
        End Get
    End Property

    ''' <summary>
    ''' Gets the first end stage found on this subsheet or null if no end stage was
    ''' found.
    ''' </summary>
    Public ReadOnly Property EndStage() As clsEndStage
        Get
            If mParent Is Nothing Then Return Nothing
            Return TryCast(mParent.GetStage(mParent.GetSubSheetEndStage(mId)), clsEndStage)
        End Get
    End Property

    ''' <summary>
    ''' Search for a given stage on the subsheet
    ''' </summary>
    ''' <param name="stageId">stage id</param>
    ''' <returns>Found stage or null</returns>
    Public Function GetStage(stageId As Guid) As clsProcessStage
        If mParent Is Nothing Then Return Nothing
        Return mParent.GetStageByIdAndSubSheet(ID, stageId)
    End Function

    Public Sub New(ByVal parent As clsProcess)
        'After creating one of these objects, all the values should
        'be filled in, but for safety's sake we will put some sensible
        'defaults in here:
        mParent = parent
        msngCameraX = 0
        msngCameraY = 0
        msngZoom = 1
        msName = String.Empty
        mId = Guid.Empty
        mbPublished = False
        mtSheetType = SubsheetType.Normal
    End Sub
End Class

