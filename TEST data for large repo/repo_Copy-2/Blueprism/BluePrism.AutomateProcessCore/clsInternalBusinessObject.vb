Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsInternalBusinessObject
''' 
''' <summary>
''' This class is the base class for all internal business objects
''' </summary>
Public MustInherit Class clsInternalBusinessObject
    Inherits clsBusinessObject

#Region " Member Variables "

    ''' <summary>
    ''' A reference to the process
    ''' </summary>
    Private mProcess As clsProcess

    ''' <summary>
    ''' The session this object is running under.
    ''' </summary>
    Private mSession As clsSession

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the clsInternalBusinessObject class.
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    ''' <param name="name">The (progid) name of this object.</param>
    ''' <param name="friendlyName">The friendly name of this obejct.</param>
    Public Sub New( _
     ByVal process As clsProcess, ByVal session As clsSession, _
     ByVal name As String, ByVal friendlyName As String)
        Me.New(process, session, name, friendlyName, Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the clsInternalBusinessObject class.
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    ''' <param name="name">The (progid) name of this object.</param>
    ''' <param name="friendlyName">The friendly name of this object.</param>
    ''' <param name="narrative">The narrative describing this object.</param>
    ''' <param name="actions">The actions which make up this object.</param>
    Public Sub New( _
     ByVal process As clsProcess, _
     ByVal session As clsSession, _
     ByVal name As String, _
     ByVal friendlyName As String, _
     ByVal narrative As String, _
     ByVal ParamArray actions() As clsInternalBusinessObjectAction)

        MyBase.New(name, friendlyName, narrative)

        mValid = True
        mConfigurable = False
        mLifecycle = True
        mProcess = process
        mSession = session

        'Derived classes can override if needs be
        mRunMode = BusinessObjectRunMode.Background

        For Each act As clsInternalBusinessObjectAction In actions
            act.Parent = Me
            AddAction(act)
        Next

    End Sub

#End Region

#Region " Internal Business Object Methods "

    ''' <summary>
    ''' Cleans up the business object, with access to the session ID in which it has
    ''' been acting.
    ''' </summary>
    ''' <param name="sessionIdentifier">The identifier representing the session this was instantiated in</param>
    ''' <returns></returns>
    Public Overridable Overloads Function DoCleanUp(ByVal sessionIdentifier As SessionIdentifier) _
     As StageResult
        Return StageResult.OK
    End Function

    ''' <summary>
    ''' Gets the action with the given name as an internal business object action.
    ''' </summary>
    ''' <param name="name">The name of the action required.</param>
    ''' <returns>The internal business object action with the given name or null if
    ''' no such action exists.</returns>
    Public Overloads Function GetAction(ByVal name As String) As clsInternalBusinessObjectAction
        Return DirectCast(MyBase.GetAction(name), clsInternalBusinessObjectAction)
    End Function

#End Region

#Region " Overriding Business Object Methods "

    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoInit() As StageResult
        Return StageResult.OK
    End Function

    ''' <summary>
    ''' Clean up the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoCleanUp() As StageResult
        Return DoCleanUp(mSession.Identifier)
    End Function

    ''' <summary>
    ''' Perform the internal business object action
    ''' </summary>
    ''' <param name="actionName">The name of the action to perform</param>
    ''' <param name="scopeStage">The stage used to resolve scope within the business
    ''' object action. This will be needed if the business object modifies or reads
    ''' any data in the current process. Must not be null.</param>
    ''' <param name="inputs">The xml representing the inputs for the action</param>
    ''' <param name="outputs">The xml representing the outputs for the action</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Protected Overrides Function DoDoAction(
     ByVal actionName As String,
     ByVal scopeStage As clsProcessStage,
     ByVal inputs As clsArgumentList,
     ByRef outputs As clsArgumentList) As StageResult

        Dim act As clsInternalBusinessObjectAction = GetAction(actionName)
        If act Is Nothing Then _
         Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsInternalBusinessObject_MissingAction0, actionName))

        act.Inputs = inputs
        Dim sErr As String = Nothing
        If Not act.Execute(mProcess, mSession, scopeStage, sErr) Then _
         Return New StageResult(False, "Internal", sErr)

        outputs = act.Outputs
        Return StageResult.OK

    End Function

    ''' <summary>
    ''' Overridden to return an error saying the internal business object cannot be
    ''' configured
    ''' </summary>
    ''' <param name="sErr">The error message</param>
    ''' <returns>A blank string</returns>
    Public Overrides Function GetConfig(ByRef sErr As String) As String
        sErr = My.Resources.Resources.clsInternalBusinessObject_NoConfigurationAvailableForAnInternalBusinessObject
        Return ""
    End Function

    ''' <summary>
    ''' Hardcoded not to return a Preamble for the internal business object.
    ''' </summary>
    ''' <param name="xr">This paramter is not used for internal business objects</param>
    Protected Overrides Sub GetHTMLPreamble(ByVal xr As System.Xml.XmlTextWriter)
    End Sub

    ''' <summary>
    ''' Hardcoded to return an error saying that internal business objects cannot
    ''' be configured
    ''' </summary>
    ''' <param name="sErr">The error message</param>
    ''' <returns>False</returns>
    Public Overrides Function ShowConfigUI(ByRef sErr As String) As Boolean
        sErr = My.Resources.Resources.clsInternalBusinessObject_AnInternalBusinessObjectCannotBeConfigured
        Return False
    End Function

    ''' <summary>
    ''' Handles anything that must be done to dispose this object. By default, this
    ''' does nothing. Subclasses should implement if specific actions are required to
    ''' be performed when an object is being disposed of (as opposed to an object
    ''' being <see cref="CleanUp">cleaned up</see>).
    ''' </summary>
    Public Overrides Sub DisposeTasks()
    End Sub

#End Region

#Region " License Checking "
    ''' <summary>
    ''' This method should be overridden and return true if the business object is
    ''' allowed to be used subject to licence restrictions.
    ''' </summary>
    Public MustOverride Function CheckLicense() As Boolean
#End Region

End Class
