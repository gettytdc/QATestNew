Imports System.Xml
Imports System.Text
Imports System.Drawing
Imports System.Globalization

Imports BluePrism.Core
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.Core.Expressions
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models
Imports System.Linq
Imports System.Runtime.Serialization

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessStage
''' 
''' <summary>
''' A class representing a stage within a process.
''' </summary>
<DebuggerDisplay("Name='{Name}'"),
    Serializable,
    DataContract([Namespace]:="bp"),
    KnownType(GetType(FontStyle)),
    KnownType(GetType(GraphicsUnit))>
Public MustInherit Class clsProcessStage

    ''' <summary>
    ''' The Default font. Generate the XML if the font is different from the default
    ''' </summary>
    <NonSerialized>
    Private Shared ReadOnly mDefaultFont As New Font(BPUtil.AvailableFont(), 10, GraphicsUnit.Pixel)

    ''' <summary>
    ''' The Default font colour. Generate the XML if the font colour is different from the default
    ''' </summary>
    <NonSerialized>
    Private Shared ReadOnly mDefaultFontColor As Color = Color.Black

    ''' <summary>
    ''' The Default stage height for most stage types. Generate the XML if the stage height is different from the default 
    ''' </summary>
    <NonSerialized>
    Private mDefaultStageHeight As Single

    ''' <summary>
    ''' The Default stage width for most stage types. Generate the XML if the font colour is different from the default  
    ''' </summary>
    <NonSerialized>
    Private mDefaultStageWidth As Single

    ''' <summary>
    ''' The parent clsProcess of this stage, always valid.
    ''' </summary>
    <NonSerialized>
    Protected mParent As clsProcess 'TODO: Remove this? Or can it be serialized eventually?

    ''' <summary>
    ''' The unique ID for this stage
    ''' </summary>
    <DataMember>
    Private mgStageID As Guid

    ''' <summary>
    ''' The name of the stage. Note that this identifier need not be unique
    ''' within a process; the only way to uniquely identify a stage is
    ''' using its StageID. See the GetStageID() method.
    ''' </summary>
    <DataMember>
    Private msName As String

    <DataMember>
    Private msInitialName As String = String.Empty

    ''' <summary>
    ''' The narrative. This is a user-defined description of the stage
    ''' and any other comments.
    ''' </summary>
    <DataMember>
    Private msNarrative As String

    ''' <summary>
    ''' Display x position
    ''' </summary>
    <DataMember>
    Private msngDisplayX As Single

    ''' <summary>
    ''' Display y position
    ''' </summary>
    <DataMember>
    Private msngDisplayY As Single

    ''' <summary>
    ''' Display Width
    ''' </summary>
    <DataMember>
    Private msngDisplayWidth As Single

    ''' <summary>
    ''' Display Height
    ''' </summary>
    <DataMember>
    Private msngDisplayHeight As Single

    ''' <summary>
    ''' Destination stage on success, or Guid.Empty to indicate the link is not
    ''' active.
    ''' </summary>
    <DataMember>
    Protected mOnSuccess As Guid

    ''' <summary>
    ''' A collection of strings to be displayed to the user as preconditions for
    ''' the stage. Currently only envisaged for the start stage of processes and
    ''' subpages.
    ''' </summary>
    Public Property Preconditions() As ICollection(Of String)
        Get
            If mPreconditions Is Nothing Then mPreconditions = New List(Of String)
            Return mPreconditions
        End Get
        Set(ByVal value As ICollection(Of String))
            mPreconditions = value
        End Set
    End Property
    <DataMember>
    Private mPreconditions As ICollection(Of String)


    ''' <summary>
    ''' Postconditions for the stage.
    ''' </summary>
    Public Property PostConditions() As ICollection(Of String)
        Get
            If mPostConditions Is Nothing Then mPostConditions = New List(Of String)
            Return mPostConditions
        End Get
        Set(ByVal value As ICollection(Of String))
            mPostConditions = value
        End Set
    End Property
    <DataMember>
    Private mPostConditions As ICollection(Of String)

    ''' <summary>
    ''' Gets or sets the subsheet which contains this stage.
    ''' </summary>
    ''' <remarks>Will return null if this stage is not associated with a parent
    ''' process.</remarks>
    Public Property SubSheet As clsProcessSubSheet
        Get
            If mParent Is Nothing Then Return Nothing
            Return mParent.GetSubSheetByID(mgSubSheetID)
        End Get
        Set(value As clsProcessSubSheet)
            mgSubSheetID = If(value Is Nothing, Guid.Empty, value.ID)
        End Set
    End Property

    ''' <summary>
    ''' The initial name of the stage when it is first created.
    ''' </summary>
    Public Property InitialName As String
        Get
            Return msInitialName
        End Get
        Set(value As String)
            msInitialName = value
            Name = msInitialName
        End Set
    End Property

    ''' <summary>
    ''' The name of this stage.
    ''' </summary>
    Public Property Name() As String
        Get
            Return msName
        End Get
        Set(ByVal value As String)
            If msName <> value Then
                msName = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the narrative for this stage.
    ''' </summary>
    Public Property Narrative As String
        Get
            Return GetNarrative()
        End Get
        Set(value As String)
            SetNarrative(value)
        End Set
    End Property

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Property Id() As Guid
        Get
            Return GetStageID()
        End Get
        Set(ByVal value As Guid)
            SetStageID(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the location of the stage. Note that this is actually the
    ''' <em>centre</em> of the stage, not any corner of it. As such, the standard
    ''' windows way of getting the bounds, combining Location and Size into a
    ''' Rectangle, will not work with stages. See <see cref="Bounds"/> to get the
    ''' actual bounds of a stage which takes this location oddness into account.
    ''' </summary>
    Public Property Location As PointF
        Get
            Return New PointF(msngDisplayX, msngDisplayY)
        End Get
        Set(value As PointF)
            msngDisplayX = value.X
            msngDisplayY = value.Y
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the size of this process stage.
    ''' </summary>
    Public Property Size As SizeF
        Get
            Return New SizeF(msngDisplayWidth, msngDisplayHeight)
        End Get
        Set(value As SizeF)
            msngDisplayWidth = value.Width
            msngDisplayHeight = value.Height
        End Set
    End Property

    ''' <summary>
    ''' The process that this stage is a part of, if the stage is currently set
    ''' into a process.
    ''' </summary>
    Public Overridable Property Process() As clsProcess
        Get
            Return mParent
        End Get
        Set(ByVal value As clsProcess)
            mParent = value
            For Each p As clsProcessParameter In mParameters
                p.Process = value
            Next
        End Set
    End Property

    ''' <summary>
    ''' An identifier for this stage for the end user, to enable them to find
    ''' the stage with relative ease.
    ''' </summary>
    ''' <remarks>Note that this relies on the fact that the subclasses of this
    ''' stage have <see cref="StageTypes"/> whose names are suitable for display.
    ''' If a subclass is implemented where this isn't the case (eg. something
    ''' like <see cref="clsWaitStartStage"/> then this property must be
    ''' overridden in the subclass).</remarks>
    Public Overridable ReadOnly Property DisplayIdentifer() As String
        Get
            Dim ss As clsProcessSubSheet = SubSheet

            If ss Is Nothing Then _
             Return String.Format(My.Resources.Resources.clsProcessStage_0Stage1NotOnAPage,
                                  clsStageTypeName.GetLocalizedFriendlyName(StageType.ToString), Name)

            Return String.Format(My.Resources.Resources.clsProcessStage_0Stage1OnPage2,
             clsStageTypeName.GetLocalizedFriendlyName(StageType.ToString), Name, ss.Name)

        End Get
    End Property

    ''' <summary>
    ''' Either Guid.Empty if on the main sheet, or the ID of the subsheet
    ''' this stage lives on.
    ''' </summary>
    <DataMember>
    Private mgSubSheetID As Guid


    ''' <summary>
    ''' List of parameters for this stage. Arguments to these parameters are
    ''' supplied as Inputs. See msInputXML for info.
    ''' </summary>
    <DataMember>
    Private mParameters As List(Of clsProcessParameter)


    ''' <summary>
    ''' The following is relevant to pretty much any stage type. The
    ''' user interface determines which it can actually be set on.
    ''' When set logging is inhibited for this stage.
    ''' </summary>
    Public Property LogInhibit() As LogInfo.InhibitModes
        Get
            Return mbLogInhibit
        End Get
        Set(ByVal Value As LogInfo.InhibitModes)
            mbLogInhibit = Value
        End Set
    End Property
    <DataMember>
    Private mbLogInhibit As LogInfo.InhibitModes


    ''' <summary>
    ''' The following is relevant to pretty much any stage type. The
    ''' user interface determines which it can actually be set on.
    ''' When set (always set by default) logging of parameters is enabled
    ''' for this stage.
    ''' </summary>
    Public Property LogParameters() As Boolean
        Get
            Return mbLogParameters
        End Get
        Set(ByVal Value As Boolean)
            mbLogParameters = Value
        End Set
    End Property
    <DataMember>
    Private mbLogParameters As Boolean

    ''' <summary>
    ''' The display mode of the stage.
    ''' </summary>
    ''' <value>A member of ShowMode, e.g. ShowMode.Normal</value>
    Public Property DisplayMode() As StageShowMode
        Get
            Return mDisplayMode
        End Get
        Set(ByVal Value As StageShowMode)
            mDisplayMode = Value
        End Set
    End Property
    <DataMember>
    Private mDisplayMode As StageShowMode

    ''' <summary>
    ''' This property is used to set the colour of the link(s)
    ''' from this stage.
    ''' </summary>
    Public Property LinkColour() As StageLinkMode
        Get
            Return mLinkMode
        End Get
        Set(ByVal Value As StageLinkMode)
            mLinkMode = Value
        End Set
    End Property
    <DataMember>
    Private mLinkMode As StageLinkMode


    ''' <summary>
    ''' Sets a summary of the changes made to an individual
    ''' stage.
    ''' </summary>
    Public Property EditSummary() As String
        Get
            Return msEditSummary
        End Get
        Set(ByVal Value As String)
            msEditSummary = Value
        End Set
    End Property
    <DataMember>
    Private msEditSummary As String


    ''' <summary>
    ''' Used as temporary storage by the process comparison form in Automate to store
    ''' list of the changes made to a the stage.
    ''' </summary>
    Public Property FullChangesList() As String
        Get
            Return msFullChangesList
        End Get
        Set(ByVal Value As String)
            msFullChangesList = Value
        End Set
    End Property
    <DataMember>
    Private msFullChangesList As String

    ''' <summary>
    ''' Only relevant when comparing processes in the Process Comparison form.
    ''' Indicates whether there are more changes to the stage over and above those
    ''' already listed in the EditSummary property.
    ''' </summary>
    Public ReadOnly Property MoreChangesAvailable() As Boolean
        Get
            If Not String.IsNullOrEmpty(msFullChangesList) AndAlso
                msFullChangesList <> msEditSummary Then Return True
            Return False
        End Get
    End Property

    Public Property FontStyle As FontStyle
    <DataMember>
    Public Property FontFamily As String
    <DataMember>
    Public Property FontSize As Single
    <DataMember>
    Public Property FontSizeUnit As GraphicsUnit

    ''' <summary>
    ''' Provides access to the font value of the stage. This font is used to render the
    ''' stage on the screen.
    ''' </summary>
    Public ReadOnly Property Font() As Font
        Get
            Return If(mParent IsNot Nothing,
                        mParent.Fonts.GetFont(FontFamily, FontSize, FontStyle, FontSizeUnit),
                        New Font(FontFamily, FontSize, FontStyle, FontSizeUnit))
        End Get
    End Property

    ''' <summary>
    '''  Provides access to the font colour of the font in the stage
    ''' </summary>
    <DataMember>
    Public Property Color As Color

    ''' <summary>
    ''' BreakPoint object for this stage. Can be null.
    ''' </summary>
    ''' <value>.</value>
    Public Property BreakPoint() As clsProcessBreakpoint
        Get
            Return mobjBreakPoint
        End Get
        Set(ByVal value As clsProcessBreakpoint)
            mobjBreakPoint = value
        End Set
    End Property
    <DataMember>
    Private mobjBreakPoint As clsProcessBreakpoint

    ''' <summary>
    ''' Gets the application information associated with the parent process of this
    ''' stage or null if the parent process is not set, or it has no application
    ''' information associated with it.
    ''' </summary>
    Public ReadOnly Property ApplicationInfo As clsApplicationTypeInfo
        Get
            Dim proc = mParent
            If proc Is Nothing Then Return Nothing

            Dim appDef = proc.ApplicationDefinition
            If appDef Is Nothing Then Return Nothing

            Return appDef.ApplicationInfo
        End Get
    End Property

    ''' <summary>
    ''' The default stage warning threshold (i.e. use system setting)
    ''' </summary>
    Public ReadOnly Property DefaultWarningThreshold As Integer
        Get
            Return -1
        End Get
    End Property

    ''' <summary>
    ''' Gets or Sets the warning threshold override for this particular stage in
    ''' minutes. Possible values are:
    ''' -1 = Use the system wide setting (default)
    '''  0 = Disabled
    ''' >0 = The number of minutes to wait before warning
    ''' </summary>
    Public Property WarningThreshold As Integer

    ''' <summary>
    ''' Indicates whether or not the warning threshold has been overridden for this
    ''' stage.
    ''' </summary>
    Public ReadOnly Property OverrideDefaultWarningThreshold As Boolean
        Get
            Return WarningThreshold <> DefaultWarningThreshold
        End Get
    End Property

    Protected Overridable Function GetDefaultStageWidth() As Single
        Return 60
    End Function

    Protected Overridable Function GetDefaultStageHeight() As Single
        Return 30
    End Function
    Protected Sub New(ByVal parent As clsProcess)
        mParent = parent
        mgStageID = Guid.NewGuid()
        mgSubSheetID = Guid.Empty
        msName = Nothing
        msNarrative = ""
        msngDisplayX = 0
        msngDisplayY = 0
        mDefaultStageWidth = GetDefaultStageWidth()
        mDefaultStageHeight = GetDefaultStageHeight()
        msngDisplayWidth = mDefaultStageWidth
        msngDisplayHeight = mDefaultStageHeight
        mOnSuccess = Guid.Empty

        mbLogInhibit = LogInfo.InhibitModes.Never
        mbLogParameters = True
        WarningThreshold = DefaultWarningThreshold
        mParameters = New List(Of clsProcessParameter)

        SetFontFromDefault()
    End Sub

    Private Sub SetFontFromDefault()
        FontFamily = mDefaultFont.Name
        FontSize = mDefaultFont.Size
        FontSizeUnit = mDefaultFont.Unit
        Color = mDefaultFontColor
    End Sub

    ''' <summary>
    ''' Get the type of this stage.
    ''' </summary>
    Public MustOverride ReadOnly Property StageType() As StageTypes

    ''' <summary>
    ''' Returns any references to to other dependency items.
    ''' Sub-classes should override and add stage-specific references (if required).
    ''' </summary>
    ''' <param name="inclInternal">Include internal references</param>
    ''' <returns>List of dependency objects</returns>
    Public Overridable Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
        Dim deps As New List(Of clsProcessDependency)()

        'Default external references - none

        'Default internal references - data items referenced within stage input/output parameters
        If inclInternal Then
            For Each p As clsProcessParameter In Parameters
                If p.GetMapType() = MapType.Expr Then
                    For Each dataItem As String In p.Expression.GetDataItems()
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            deps.Add(New clsProcessDataItemDependency(stage))
                    Next
                ElseIf p.GetMapType() = MapType.Stage Then
                    If p.GetMap() <> String.Empty Then
                        Dim outOfScope As Boolean
                        Dim stage = mParent.GetDataStage(p.GetMap, Me, outOfScope)
                        If Not outOfScope AndAlso stage IsNot Nothing Then _
                            deps.Add(New clsProcessDataItemDependency(stage))
                    End If
                End If
            Next
        End If

        Return deps
    End Function

    Public Shared ReadOnly Property KeyStages As IEnumerable(Of StageTypes)
        Get
            Return {StageTypes.Action,
                 StageTypes.Skill,
                 StageTypes.Code,
                 StageTypes.Navigate,
                 StageTypes.Process,
                 StageTypes.Read,
                 StageTypes.Write,
                 StageTypes.WaitStart
                }
        End Get
    End Property

    Private Function ShouldInhibitLogging(failure As Boolean) As Boolean
        If Me.LogInhibit = LogInfo.InhibitModes.Always Then _
            Return True
        If Me.LogInhibit = LogInfo.InhibitModes.OnSuccess AndAlso Not failure Then _
            Return True

        Return False
    End Function

    Private Function DoDiagnosticsRequireLogging(failure As Boolean) As Boolean
        If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.LogOverrideAll) Then _
            Return True
        If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.LogOverrideKey) _
                AndAlso KeyStages.Contains(Me.StageType) Then _
            Return True
        If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.LogOverrideErrorsOnly) _
                    AndAlso failure Then _
            Return True

        Return False
    End Function

    Private Function DoDiagnosticsInhibitLogging(failure As Boolean) As Boolean
        If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.LogOverrideKey) _
                AndAlso Not KeyStages.Contains(Me.StageType) Then _
            Return True
        If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.LogOverrideErrorsOnly) _
                AndAlso Not failure Then _
            Return True

        Return False
    End Function

    ''' <summary>
    ''' Get the extra logging information required.
    ''' </summary>
    Protected Function GetLogInfo(Optional ByVal failure As Boolean = False) As LogInfo
        Dim info As New LogInfo() With {
            .Inhibit = ShouldInhibitLogging(failure),
            .InhibitParams = Not Me.LogParameters
        }

        If DoDiagnosticsRequireLogging(failure) Then
            info.Inhibit = False
        End If

        If DoDiagnosticsInhibitLogging(failure) Then
            info.Inhibit = True
        End If

        If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.LogMemory) Then

            If clsAPC.Diagnostics.HasFlag(clsAPC.Diags.ForceGC) Then
                GC.Collect()
            End If

            Dim p = Diagnostics.Process.GetCurrentProcess()
            info.AutomateWorkingSet = p.WorkingSet64

            Select Case Me.StageType
                Case StageTypes.Navigate, StageTypes.Read, StageTypes.Write, StageTypes.WaitStart
                    Dim pid As Integer = mParent.AMI.TargetPID
                    If pid <> 0 Then
                        p = Diagnostics.Process.GetProcessById(pid)
                        If p IsNot Nothing Then
                            info.TargetAppName = p.ProcessName
                            info.TargetAppWorkingSet = p.WorkingSet64()
                        End If
                    End If
            End Select

        End If

        Return info
    End Function

    Friend Sub LogError(ByVal sErrorMessage As String, logger As CompoundLoggingEngine)
        Dim info = GetLogInfo(True)
        logger.LogError(info, Me, sErrorMessage)
    End Sub

    Public MustOverride Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

    ''' <summary>
    ''' Validates the process stage.
    ''' </summary>
    ''' <param name="bAttemptRepair">Set to true to have errors automatically
    ''' repaired, where possible.</param>
    ''' <param name="SkipObjects">If True, checks for installed Business Objects
    ''' and similar are skipped.</param>
    ''' <exception cref="ArgumentNullException">If a null exceptionTypes List was 
    ''' passed.</exception>
    ''' <returns>Returns a list of errors found.</returns>
    Public Overridable Overloads Function CheckForErrors(ByVal bAttemptRepair As Boolean,
                                               ByVal SkipObjects As Boolean) As ValidationErrorList
        Return CheckForErrors(bAttemptRepair, SkipObjects, New List(Of String))
    End Function

    ''' <summary>
    ''' Validates the process stage.
    ''' </summary>
    ''' <param name="bAttemptRepair">Set to true to have errors automatically
    ''' repaired, where possible.</param>
    ''' <param name="SkipObjects">If True, checks for installed Business Objects
    ''' and similar are skipped.</param>
    ''' <param name="exceptionTypes">A list of exception types stored in the database,
    ''' to be used with exception type validation in sub class clsExceptionStage.</param>
    ''' <exception cref="ArgumentNullException">If a null exceptionTypes List was 
    ''' passed.</exception>
    ''' <returns>Returns a list of errors found.</returns>
    Friend Overridable Overloads Function CheckForErrors(ByVal bAttemptRepair As Boolean,
                                               ByVal SkipObjects As Boolean,
                                               ByVal exceptionTypes As ICollection(Of String)) As ValidationErrorList

        If exceptionTypes Is Nothing Then _
            Throw New ArgumentNullException(NameOf(exceptionTypes))

        Dim errors As New ValidationErrorList

        'Check for blank name (doesn't apply to process or subsheet info stages
        'because they show the name of the process or page)
        If StageType <> StageTypes.ProcessInfo AndAlso StageType <> StageTypes.SubSheetInfo Then
            If String.IsNullOrEmpty(msName) Then
                If bAttemptRepair Then
                    msName = mParent.GetUniqueStageID(StageType)
                Else
                    errors.Add(New RepairableValidateProcessResult(Me, 58))
                End If
            End If
        End If

        'Check parameters
        For Each p In GetParameters()
            errors.AddRange(p.CheckForErrors(Me, bAttemptRepair))
        Next

        Return errors

    End Function

    ''' <summary>
    ''' Evaluates the given expression, returning the process value that it resolved
    ''' as.
    ''' </summary>
    ''' <param name="expr">The expression to evaluate.</param>
    ''' <returns>The resultant process value.</returns>
    ''' <exception cref="InvalidFormatException">If the given expression was not in a
    ''' valid format and thus was unable to be evaluated.</exception>
    Protected Function Evaluate(expr As BPExpression) As clsProcessValue
        Dim err As String = Nothing
        Dim val As clsProcessValue = Nothing
        If clsExpression.EvaluateExpression(expr, val, Me, False, Nothing, err) Then _
         Return val

        Throw New InvalidFormatException(My.Resources.Resources.clsProcessStage_InvalidExpression0, expr)

    End Function

    ''' <summary>
    ''' Validates the given expression within the context of this stage
    ''' </summary>
    ''' <param name="expr">The expression to validate</param>
    ''' <param name="locn">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1".</param>
    ''' <param name="targetDataTypes"></param>
    ''' <returns>A list containing any validation errors found while validating the
    ''' given expression.</returns>
    Protected Function ValidateExpression(
     expr As BPExpression, locn As String, ParamArray targetDataTypes() As DataType) _
     As ValidationErrorList
        Return clsExpression.CheckExpressionForErrors(
            expr, Me, targetDataTypes, locn, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Gets whether this stage is in a recovery section within the context of its
    ''' owning process.
    ''' </summary>
    ''' <returns>True if this stage has an owning process and is within a recovery
    ''' section on that process.</returns>
    ''' <seealso cref="clsProcess.IsStageInRecoverySection"/>
    Public Function IsInRecoverySection() As Boolean
        Return (mParent IsNot Nothing AndAlso mParent.IsStageInRecoverySection(Me))
    End Function

    ''' <summary>
    ''' Finds name conflicts with this stage in the owning process.
    ''' This checks the process for stages which have the same name as this stage
    ''' and a type which would cause a conflict.
    ''' By default, process stages can have the same name - subclasses (such as
    ''' data stages or code stages) may have uniqueness requirements.   ''' 
    ''' </summary>
    ''' <returns>A collection of process stages which have a naming conflict with
    ''' this stage - an empty collection means no conflicts.</returns>
    Public Function FindNamingConflicts() As ICollection(Of clsProcessStage)
        Return FindNamingConflicts(Me.Name)
    End Function

    ''' <summary>
    ''' Finds name conflicts with this stage in the owning process.
    ''' This checks the process for stages which have the same name as this stage
    ''' and a type which would cause a conflict.
    ''' Code stages cannot have the same name as other code stages within the process
    ''' </summary>
    ''' <param name="proposedName">The proposed name for this code stage.</param>
    ''' <returns>A collection of process stages which have a naming conflict with
    ''' this stage - an empty collection means no conflicts.</returns>
    Public Overridable Function FindNamingConflicts(ByVal proposedName As String) _
     As ICollection(Of clsProcessStage)
        Return GetEmpty.ICollection(Of clsProcessStage)()
    End Function


    ''' <summary>
    ''' Analyse the usage of actions in a stage. In the counts that are returned,
    ''' it is the AMI-based actions that are counted, so for example, a Navigate
    ''' stage with three rows will count as three.
    ''' </summary>
    ''' <param name="total">The current count of the total number of actions, which
    ''' is updated on return to reflect the contents of this stage.</param>
    ''' <param name="globalcount">The current count of the number of actions that
    ''' use a 'global' method, i.e. a global mouse click or keypress. Updated on
    ''' return to reflect the contents of this stage.</param>
    ''' <param name="globaldetails">A StringBuilder to which details of any global
    ''' actions can be appended.</param>
    Public Overridable Sub AnalyseAMIActions(ByRef total As Integer, ByRef globalcount As Integer, ByVal globaldetails As StringBuilder)
        'For most stages, we do nothing. Stages that actually use AMI actions
        'should override this.
    End Sub

    ''' <summary>
    ''' Checks if the property BreakPoint is null.
    ''' </summary>
    ''' <returns>Returns true if property Breakpoint is not nothing; false otherwise.
    ''' </returns>
    Public Function HasBreakPoint() As Boolean
        Return Not mobjBreakPoint Is Nothing
    End Function

    ''' <summary>
    ''' Sets the ID of the subsheet. This determines which subsheet of the process
    ''' that the stage belongs to.
    ''' </summary>
    ''' <param name="gID">The ID of the subsheet.</param>
    Public Sub SetSubSheetID(ByVal gID As Guid)
        mgSubSheetID = gID
    End Sub

    ''' <summary>
    ''' Gets the ID of the subsheet that this stage resides on.
    ''' </summary>
    ''' <returns>The unique identifier for this stage's subsheet.</returns>
    Public Function GetSubSheetID() As Guid
        Return mgSubSheetID
    End Function

    Friend Function IsOnSubSheet(subsheetId As Guid) As Boolean
        Return mgSubSheetID.CompareTo(subsheetId) = 0
    End Function

    ''' <summary>
    ''' Gets the name of the subsheet on which this stage resides.
    ''' </summary>
    ''' <returns>The name of the subsheet that this stage is on within the process.
    ''' </returns>
    ''' <exception cref="NullReferenceException">If this stage is not currently
    ''' assigned to a process.</exception>
    Public Function GetSubSheetName() As String
        Return mParent?.GetSubSheetName(mgSubSheetID)
    End Function

    ''' <summary>
    ''' Determines if the stage is visible in its parent process, based on the
    ''' supplied viewport size, and the current camera location of the process.
    ''' </summary>
    ''' <param name="ViewPortSize">The size of the viewport, in non-world
    ''' coordinates.</param>
    ''' <returns>Returns True if the stage is wholly visible in the world viewport;
    ''' False otherwise.</returns>
    Public Function IsVisible(ByVal ViewPortSize As Size) As Boolean
        Dim Bounds As RectangleF = New RectangleF(msngDisplayX - msngDisplayWidth, msngDisplayY - msngDisplayHeight, msngDisplayWidth, msngDisplayHeight)
        Return (Me.GetSubSheetID.Equals(mParent.GetActiveSubSheet)) AndAlso (mParent.GetWorldViewPort(ViewPortSize).Contains(Bounds))
    End Function

    Public Sub SetDisplayX(ByVal d As Single)
        msngDisplayX = d
    End Sub

    Public Sub SetDisplayY(ByVal d As Single)
        msngDisplayY = d
    End Sub

    Public Sub SetDisplayWidth(ByVal d As Single)
        msngDisplayWidth = d
    End Sub

    Public Sub SetDisplayHeight(ByVal d As Single)
        msngDisplayHeight = d
    End Sub

    Public Function GetDisplayX() As Single
        GetDisplayX = msngDisplayX
    End Function

    Public Function GetDisplayY() As Single
        GetDisplayY = msngDisplayY
    End Function

    Public Function GetDisplayWidth() As Single
        GetDisplayWidth = msngDisplayWidth
    End Function

    Public Function GetDisplayHeight() As Single
        GetDisplayHeight = msngDisplayHeight
    End Function

    ''' <summary>
    ''' Gets the display bounds of this stage.
    ''' </summary>
    ''' <remarks>This is equivalent to a call to <see cref="GetDisplayBounds"/>
    ''' </remarks>
    Public ReadOnly Property Bounds() As RectangleF
        Get
            Return GetDisplayBounds()
        End Get
    End Property

    ''' <summary>
    ''' Gets the display bounds of the stage. Note that though most stage types are
    ''' positioned with their DisplayX and DisplayY as the centre, some have this at
    ''' the top left corner. Therefore, using this method is the simplest way to
    ''' determine the actual bounds in a way that covers both cases.
    ''' </summary>
    ''' <returns>A RectangleF containing the bounds.</returns>
    Public Overridable Function GetDisplayBounds() As RectangleF
        Return New RectangleF(msngDisplayX - msngDisplayWidth / 2, msngDisplayY - msngDisplayHeight / 2, msngDisplayWidth, msngDisplayHeight)
    End Function

    ''' <summary>
    ''' Determines whether this stage's bounds contain the specified point,
    ''' on the specified page.
    ''' </summary>
    ''' <param name="sngX">The x coordinate of interest, in world coordinates.</param>
    ''' <param name="sngY">The x coordinate of interest, in world coordinates.</param>
    ''' <param name="gSubSheet">The subsheet of interest.</param>
    ''' <returns>Returns true if the stage's bounds contain the specified point,
    ''' on the specified page.</returns>
    Public Overridable Function IsAtPosition(ByVal sngX As Single, ByVal sngY As Single, ByVal gSubSheet As Guid) As Boolean
        If gSubSheet = Me.GetSubSheetID Then
            Return Me.GetDisplayBounds.Contains(sngX, sngY)
        Else
            Return False
        End If
    End Function


    ''' <summary>
    ''' Determine if this stage links (in a forwards direction) to another stage.
    ''' </summary>
    ''' <param name="st">The stage to check against</param>
    ''' <returns>True if there is a link, False otherwise.</returns>
    Public Overridable Function LinksTo(ByVal st As clsProcessStage) As Boolean
        Return False
    End Function


    ''' <summary>
    ''' Determines if there is a link of any kind out of this stage with 
    ''' its arrow head at the specified coordinates on the specified subpage.
    ''' </summary>
    ''' <param name="gSubSheetID">The ID of the subsheet we are testing on.</param>
    ''' <param name="dblX">The X coord in world coordinates, to test for the arrow
    ''' head.</param>
    ''' <param name="dblY">The Y coord in world coordinates, to test for the arrow
    ''' head.</param>
    ''' <param name="sLinkType">The type of the link that is found, if any.
    ''' This is set to a blank string if no link is found. Valid types are
    ''' "True", "False", "Error", "Success".</param>
    ''' <returns>True if a link is found, false otherwise. The type of any 
    ''' link found will be stored in sLinkType.</returns>
    Public Function IsLinkAtPosition(ByVal gSubSheetID As Guid, ByVal dblX As Single, ByVal dblY As Single, ByRef sLinkType As String) As Boolean

        sLinkType = "True"
        If IsLinkTypeAtPosition(gSubSheetID, dblX, dblY, sLinkType) Then
            Return True
        End If
        sLinkType = "False"
        If IsLinkTypeAtPosition(gSubSheetID, dblX, dblY, sLinkType) Then
            Return True
        End If
        sLinkType = "Success"
        If IsLinkTypeAtPosition(gSubSheetID, dblX, dblY, sLinkType) Then
            Return True
        End If
        sLinkType = "Error"
        If IsLinkTypeAtPosition(gSubSheetID, dblX, dblY, sLinkType) Then
            Return True
        End If

        sLinkType = ""
        Return False
    End Function

    ''' <summary>
    ''' Checks whether the rectangle described by this stage intersects with the
    ''' given rectangle on the same page.
    ''' </summary>
    ''' <param name="r">The rectangle defining the area to check for an intersect
    ''' with this stage</param>
    ''' <returns>True if the given rectangle intersects this stage; False otherwise.
    ''' </returns>
    Public Function Intersects(ByVal r As RectangleF) As Boolean
        Return IsInRegion(r.Left, r.Top, r.Right, r.Bottom, SubSheet.ID)
    End Function

    ''' <summary>
    ''' Checks whether the area occupied by the given stage intersects with this
    ''' stage.
    ''' </summary>
    ''' <param name="stg">The stage to check for an intersect with this stage</param>
    ''' <returns>True if the given stage intersects this stage; False otherwise.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">If a null stage was passed.
    ''' </exception>
    Public Function Intersects(ByVal stg As clsProcessStage) As Boolean
        If stg Is Nothing Then Throw New ArgumentNullException(NameOf(stg))
        Return (stg.SubSheet Is Me.SubSheet AndAlso Intersects(stg.Bounds))
    End Function

    ''' <summary>
    ''' Checks whether the rectangle described by this stage falls fully within the
    ''' given rectangle on the same page.
    ''' </summary>
    ''' <param name="r">The rectangle defining the area to check if it fully contains
    ''' this stage</param>
    ''' <returns>True if the given rectangle fully contains this stage; False
    ''' otherwise.</returns>
    Public Function IsFullyWithin(ByVal r As RectangleF) As Boolean
        Return IsInRegion(r.Left, r.Top, r.Right, r.Bottom, SubSheet.ID, True)
    End Function

    ''' <summary>
    ''' Checks whether the given block stage fully contains this stage.
    ''' </summary>
    ''' <param name="blk">The block stage to check if it fully contains this stage
    ''' </param>
    ''' <returns>True if the given block fully contains this stage; False otherwise
    ''' </returns>
    Public Function IsFullyWithin(ByVal blk As clsBlockStage) As Boolean
        Return (blk.SubSheet Is Me.SubSheet AndAlso IsFullyWithin(blk.Bounds))
    End Function

    ''' <summary>
    ''' Checks whether this stage falls partly within the given block stage and
    ''' partly outside it, assuming the rectangle defines a region on the same page
    ''' as this stage.
    ''' </summary>
    ''' <param name="r">The rectangle to check to see if this stage falls partly
    ''' within it or not</param>
    ''' <returns>True if this stage falls partly within the given block; False
    ''' otherwise.</returns>
    Public Function IsPartlyWithin(ByVal r As RectangleF) As Boolean
        Return (Intersects(r) AndAlso Not IsFullyWithin(r))
    End Function

    ''' <summary>
    ''' Checks whether this stage falls partly within the given block stage and
    ''' partly outside it.
    ''' </summary>
    ''' <param name="blk">The block stage to check to see if this stage falls partly
    ''' within it or not</param>
    ''' <returns>True if this stage falls partly within the given block; False
    ''' otherwise.</returns>
    Public Function IsPartlyWithin(ByVal blk As clsBlockStage) As Boolean
        Return (Intersects(blk) AndAlso Not IsFullyWithin(blk))
    End Function

    ''' <summary>
    ''' Determine is the stage is displayed within the given rectangular region. The
    ''' given region can be defined in any order, e.g. finish Y can be less than start
    ''' Y.
    ''' </summary>
    ''' <param name="sx">The start X</param>
    ''' <param name="sy">The start Y</param>
    ''' <param name="fx">The finish X</param>
    ''' <param name="fy">The finish Y</param>
    ''' <param name="gSubSheetID">The subsheet ID of the region</param>
    ''' <param name="entirely">If True, the stage must be entirely within the given
    ''' region to qualify. If False, the default, it need only be partially in it.
    ''' </param>
    ''' <returns>True if the stage is in the region.</returns>
    Public Function IsInRegion(ByVal sx As Single, ByVal sy As Single, ByVal fx As Single, ByVal fy As Single, ByVal gSubSheetID As Guid, Optional ByVal entirely As Boolean = False) As Boolean
        Dim b As RectangleF = GetDisplayBounds()
        Dim t As Single
        If fy < sy Then
            t = fy
            fy = sy
            sy = t
        End If
        If fx < sx Then
            t = fx
            fx = sx
            sx = t
        End If
        If gSubSheetID.CompareTo(mgSubSheetID) <> 0 Then Return False
        If fx < b.Left Then Return False
        If sx >= b.Right Then Return False
        If fy < b.Top Then Return False
        If sy >= b.Bottom Then Return False
        If entirely Then
            If b.Left < sx Then Return False
            If b.Right > fx Then Return False
            If b.Top < sy Then Return False
            If b.Bottom > fy Then Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Deletes the link of the specified kind from this stage, should one exist.
    ''' </summary>
    ''' <param name="sType">The type of outbound link from this stage to delete.</param>
    Public Sub DeleteLink(ByVal sType As String)
        Select Case StageType
            Case StageTypes.Decision
                Dim objDecision As clsDecisionStage = CType(Me, clsDecisionStage)
                Select Case sType
                    Case "True"
                        objDecision.OnTrue = Guid.Empty
                    Case "False"
                        objDecision.OnFalse = Guid.Empty
                End Select
            Case Else
                Select Case sType
                    Case "Success"
                        mOnSuccess = Guid.Empty
                End Select
        End Select
    End Sub

    ''' <summary>
    ''' Determines if this stage owns an outward link of the specified type
    ''' with the link arrow at the specified coordinates.
    ''' </summary>
    ''' <param name="gSubSheetID">The subsheet of interest.</param>
    ''' <param name="dblX">The X coord in world coordinates, to test for the arrow
    ''' head.</param>
    ''' <param name="dblY">The Y coord in world coordinates, to test for the arrow
    ''' head.</param>
    ''' <param name="sLinkType">The type of link to test for. This can be 
    ''' one of "True", "False", "Success", "Error".</param>
    ''' <returns>Returns True if link is found; False otherwise.</returns>
    Public Function IsLinkTypeAtPosition(ByVal gSubSheetID As Guid, ByVal dblX As Single, ByVal dblY As Single, ByRef sLinkType As String) As Boolean
        IsLinkTypeAtPosition = False

        'no point checking any further if page IDs do not match
        If Not gSubSheetID.Equals(Me.mgSubSheetID) Then Return False

        Dim gDest As Guid
        Select Case StageType
            Case StageTypes.Decision
                Dim objDecision As clsDecisionStage = CType(Me, clsDecisionStage)
                Select Case sLinkType
                    Case "True"
                        gDest = objDecision.OnTrue
                    Case "False"
                        gDest = objDecision.OnFalse
                End Select
            Case Else
                Select Case sLinkType
                    Case "Success"
                        gDest = mOnSuccess
                    Case Else
                        Return False
                End Select
        End Select

        If gDest.Equals(Guid.Empty) Then Return False
        Dim objDestStage As clsProcessStage = mParent.GetStage(gDest)
        If objDestStage IsNot Nothing Then
            Return IsLinkAtPosition(New PointF(dblX, dblY), Me.GetDisplayBounds, objDestStage.GetDisplayBounds)
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Works out if a link is at the position based entirely on co-ordinates.
    ''' </summary>
    ''' <returns>True if the link exists at this point.</returns>
    Public Function IsLinkAtPosition(ByVal pHitPoint As PointF, ByVal rLinkSrc As RectangleF, ByVal rLinkDest As RectangleF) As Boolean
        Dim LinkPosition As PointF = GetLinkArrowPosition(rLinkSrc, rLinkDest)

        'say yes if hit point is within a small distance
        Dim Distance As Double = Math.Sqrt(Math.Pow(LinkPosition.X - pHitPoint.X, 2) + Math.Pow(LinkPosition.Y - pHitPoint.Y, 2))
        Return Distance < 5
    End Function

    ''' <summary>
    ''' Gets the location of the arrow displayed on the link between
    ''' the two supplied rectangles.
    ''' </summary>
    ''' <param name="rLinkSrc">The bounding rectangle of the link source object,
    ''' in world coordinates.</param>
    ''' <param name="rLinkDest">The bounding rectangle of the link target object,
    ''' in world coordinates.</param>
    ''' <returns>Returns the point at which the link arrow should be drawn
    ''' between the two objects, in world coordinates.</returns>
    Public Shared Function GetLinkArrowPosition(ByVal rLinkSrc As RectangleF, ByVal rLinkDest As RectangleF) As PointF
        'Get source/target rectangle centres
        Dim pSourceCentre As PointF = clsGeometry.GetRectangleCentre(rLinkSrc)
        Dim pDestCentre As PointF = clsGeometry.GetRectangleCentre(rLinkDest)
        Dim UnitDirectionVect As clsGeometry.Vector = New clsGeometry.Vector(pDestCentre.X - pSourceCentre.X, pDestCentre.Y - pSourceCentre.Y).ToUnitVector

        'Find intersection of line with the bounds of the two rectangles
        Dim IntersectionOfLineWithSourceRect As PointF = clsGeometry.GetIntersectionOfLineFromRectCentreWithRectEdge(UnitDirectionVect, rLinkSrc)
        Dim IntersectionOfLineWithDestRect As PointF = clsGeometry.GetIntersectionOfLineFromRectCentreWithRectEdge(-UnitDirectionVect, rLinkDest)

        'Calculate the midpoint of the line, in world coords,
        'taking the edge of the stages as the endpoints of the line
        Dim linemid As PointF
        linemid.X = (IntersectionOfLineWithSourceRect.X + IntersectionOfLineWithDestRect.X) / 2
        linemid.Y = (IntersectionOfLineWithSourceRect.Y + IntersectionOfLineWithDestRect.Y) / 2

        Return linemid
    End Function

    ''' <summary>
    ''' Gets a readonly collection of stages that this stage links to.
    ''' </summary>
    ''' <returns>A collection of stages that this stage links to</returns>
    Friend Overridable Function GetLinks() As ICollection(Of clsProcessStage)
        Return GetEmpty.ICollection(Of clsProcessStage)()
    End Function

    ''' <summary>
    ''' Gets the ID used to identify this stage internally. This should be unique
    ''' among different stage objects.
    ''' </summary>
    ''' <returns>Returns the guid identifying this stage.</returns>
    Public Function GetStageID() As Guid
        Return mgStageID
    End Function


    ''' <summary>
    ''' Sets the ID of the stage. This ID is a unique identifier for the stage.
    ''' </summary>
    ''' <param name="gID">The ID.</param>
    Public Sub SetStageID(ByVal gID As Guid)
        mgStageID = gID
    End Sub

    ''' <summary>
    ''' Gets the name of this stage. Need not be unique among stages.
    ''' </summary>
    ''' <returns>String holding the name of this stage. No limit on length 
    ''' of string.</returns>
    Public Function GetName() As String
        Return Name
    End Function

    ''' <summary>
    ''' Gets the description of this stage. 
    ''' </summary>
    ''' <returns>String holding the description of this stage. No limit on length 
    ''' of string.</returns>
    Public Function GetNarrative() As String
        GetNarrative = msNarrative
    End Function

    Public Sub SetNarrative(ByVal sNarrative As String)
        msNarrative = sNarrative
    End Sub


    ''' <summary>
    ''' Gets the text that should be displayed on the stage in the user interface.
    ''' In most cases, this will simply be the stage name. In special cases (eg such as
    ''' the case of data items), the output will be different (eg for data items,
    ''' the current value will also be displayed).
    ''' </summary>
    ''' <returns>See summary.</returns>
    Public Overridable Function GetShortText() As String
        Return msName
    End Function

    ''' <summary>
    ''' Gets the short value (&lt;1000 characters) of the stage when this stage
    ''' is a data item and when the value is of datatype text, otherwise just
    ''' returns the value unmodified, or if this stage is not even a data item
    ''' just returns an empty string.
    ''' </summary>
    ''' <param name="initValue">True to retrieve the 'short' version of the initial
    ''' value; False to retrieve the current value.
    ''' the current value</param>
    ''' <returns>A string representing the short version of the required value of
    ''' this data stage.</returns>
    ''' <remarks>Data stages are the only stages which make sense to have a short
    ''' value, so by default, this will return an empty string.</remarks>
    ''' <seealso cref="clsDataStage.GetShortValue"/>
    Public Overridable Function GetShortValue(ByVal initValue As Boolean) As String
        ' This only makes sense for data stages - other stages have no 'short value'
        Return ""
    End Function

    ''' <summary>
    ''' Get the outputs of the stage.
    ''' </summary>
    ''' <returns>A List of clsProcessParameter objects, each
    ''' one representing an output from the stage.</returns>
    Public Function GetOutputs() As List(Of clsProcessParameter)
        Dim arrl As New List(Of clsProcessParameter)
        For Each p As clsProcessParameter In mParameters
            If p.Direction = ParamDirection.Out Then arrl.Add(p)
        Next
        Return arrl
    End Function

    ''' <summary>
    ''' Get the inputs of the stage.
    ''' </summary>
    ''' <returns>An List of clsProcessParameter objects, each
    ''' one representing an output from the stage.</returns>
    Public Function GetInputs() As List(Of clsProcessParameter)
        Dim arrl As New List(Of clsProcessParameter)
        For Each p As clsProcessParameter In mParameters
            If p.Direction = ParamDirection.In Then arrl.Add(p)
        Next
        Return arrl
    End Function


    ''' <summary>
    ''' Sets the inputs for this stage. See sInputXML parameter for details of how
    ''' this should be formatted.bThe supplied inputs will be appended to the
    ''' existing ones.
    ''' </summary>
    ''' <param name="sInputXML">An XML string representing the input. Root level
    ''' nodes must have name "input". Attributes may be "name", "type", "stage",
    ''' "const", "expression". Any inner text will be ignored.</param>
    Private Sub SetInputXML(ByVal sInputXML As String)
        Try
            If sInputXML <> "" Then
                Dim x As New ReadableXmlDocument(sInputXML)
                For Each child As XmlElement In x.ChildNodes
                    Select Case child.Name
                        Case "input"
                            Dim p As clsProcessParameter = clsProcessParameter.FromXML(child)
                            Debug.Assert(p.Direction = ParamDirection.In)
                            Me.AddParameter(p)
                    End Select
                Next
            End If
        Catch ex As Exception
            'do nothing
        End Try
    End Sub

    ''' <summary>
    ''' Sets the outputs for this stage. See sOutputXML parameter for details of how
    ''' this should be formatted. The supplied inputs will be appended to the
    ''' existing ones.
    ''' </summary>
    ''' <param name="sOutputXML">An XML string representing the input. Root level
    ''' nodes must have name "input". Attributes may be "name", "type", "stage",
    ''' "const", "expression". Any inner text will be ignored.</param>
    Private Sub SetOutputXML(ByVal sOutputXML As String)
        Try
            If sOutputXML <> "" Then
                Dim x As New ReadableXmlDocument(sOutputXML)
                For Each child As XmlElement In x.ChildNodes
                    Select Case child.Name
                        Case "output"
                            Dim p As clsProcessParameter = clsProcessParameter.FromXML(child)
                            Debug.Assert(p.Direction = ParamDirection.Out)
                            Me.AddParameter(p)
                    End Select
                Next
            End If
        Catch
            'do nothing
        End Try
    End Sub

    ''' <summary>
    ''' Get the number of parameters (both input and output) for this stage.
    ''' </summary>
    ''' <returns>The number of parameters, 0-n.</returns>
    Public Function GetNumParameters() As Integer
        Return mParameters.Count
    End Function

    ''' <summary>
    ''' Get a reference to the given parameter.
    ''' </summary>
    ''' <param name="iParmIndex">The index, 0-n, of the parameter
    ''' required, where 'n' is GetNumParameters()-1</param>
    ''' <returns>A reference to a clsProcessParameter</returns>
    Public Function GetParameter(ByVal iParmIndex As Integer) As clsProcessParameter
        Return mParameters(iParmIndex)
    End Function

    ''' <summary>
    ''' Get all the parameters of this stage.
    ''' </summary>
    ''' <returns>An List of clsProcessParameter objects, with
    ''' each describing a parameter for the stage.</returns>
    Public Function GetParameters() As List(Of clsProcessParameter)
        Return mParameters
    End Function

    ''' <summary>
    ''' Gets or sets the parameters in this stage. Note that setting the parameters
    ''' will copy the parameters from the given collection into this stage, it does
    ''' not use the actual given collection in this stage.
    ''' </summary>
    Public Property Parameters() As ICollection(Of clsProcessParameter)
        Get
            Return mParameters
        End Get
        Set(ByVal value As ICollection(Of clsProcessParameter))
            ClearParameters()
            AddParameters(value)
        End Set
    End Property

    ''' <summary>
    ''' Get a reference to the given parameter.
    ''' </summary>
    ''' <param name="sName">The parameter name</param>
    ''' <param name="dir">The direction of the parameter</param>
    ''' <returns>A reference to a clsProcessParameter</returns>
    Public Function GetParameter(ByVal sName As String,
     ByVal dir As ParamDirection) As clsProcessParameter
        For Each param As clsProcessParameter In mParameters
            If param.Name = sName And param.Direction = dir Then Return param
        Next
        Return Nothing
    End Function

    ''' <summary>
    '''  Add a parameter to this stage.
    ''' </summary>
    ''' <param name="dtDir">The parameter direction</param>
    ''' <param name="dtDataType">The data type of the parameter</param>
    ''' <param name="sName">The name of the parameter</param>
    ''' <param name="sNarrative"></param>
    ''' <param name="sMapType">The mapping type, which can be one of:  "Stage", "Const", "Expr", "None".</param>
    ''' <param name="sMap">Mapping information, dependent on the specified mapping type.</param>
    ''' <param name="validator">value validator, can be null</param>
    ''' <param name="collectionInfo"></param>
    ''' <param name="sFriendlyName"></param>
    ''' <returns></returns>
    Public Function AddParameter(ByVal dtDir As ParamDirection, ByVal dtDataType As DataType, ByVal sName As String, ByVal sNarrative As String, ByVal sMapType As MapType, ByVal sMap As String, validator As IParameterValidation, Optional ByVal collectionInfo As clsCollectionInfo = Nothing, Optional ByVal sFriendlyName As String = Nothing) As clsProcessParameter

        Dim p As New clsProcessParameter(sName, dtDataType, dtDir, sNarrative, sFriendlyName) With {
            .Process = mParent
        }
        p.SetMapType(sMapType)
        p.SetMap(sMap)
        p.Narrative = sNarrative
        p.SetValidator(validator)


        If collectionInfo IsNot Nothing Then
            If dtDataType <> DataType.collection Then
                Throw New InvalidArgumentException(My.Resources.Resources.clsProcessStage_UnexpectedCollectionInfoPassedForNonCollectionParameter)
            Else
                p.CollectionInfo = collectionInfo
            End If
        End If

        mParameters.Add(p)
        Return p
    End Function

    ''' <summary>
    ''' Add a parameter to this stage.
    ''' </summary>
    ''' <param name="dtDir">The parameter direction</param>
    ''' <param name="dtDataType">The data type of the parameter</param>
    ''' <param name="sName">The name of the parameter</param>
    ''' <param name="sMapType">The mapping type, which can be
    ''' one of:  "Stage", "Const", "Expr", "None".</param>
    ''' <param name="sMap">Mapping information, dependent on the
    ''' specified mapping type. 
    ''' For "Stage", it is a stage ID, which must be a stage of
    ''' type "Data". For "Const" it is a constant value.</param>
    ''' <returns>A reference to the new clsProcessParameter</returns>

    Public Function AddParameter(ByVal dtDir As ParamDirection, ByVal dtDataType As DataType, ByVal sName As String, ByVal sNarrative As String, ByVal sMapType As MapType, ByVal sMap As String, Optional ByVal CollectionInfo As clsCollectionInfo = Nothing, Optional ByVal sFriendlyName As String = Nothing) As clsProcessParameter
        Dim p As New clsProcessParameter(sName, dtDataType, dtDir, sNarrative, sFriendlyName)
        p.Process = mParent
        p.SetMapType(sMapType)
        p.SetMap(sMap)
        p.Narrative = sNarrative

        If CollectionInfo IsNot Nothing Then
            If dtDataType <> DataType.collection Then
                Throw New InvalidArgumentException(My.Resources.Resources.clsProcessStage_UnexpectedCollectionInfoPassedForNonCollectionParameter)
            Else
                p.CollectionInfo = CollectionInfo
            End If
        End If

        mParameters.Add(p)
        Return p
    End Function


    ''' <summary>
    ''' Adds the supplied parameter to this stage's parameters, setting the
    ''' parameter's process to be the process of this stage.
    ''' </summary>
    ''' <param name="objParm">The parameter to add.</param>
    Public Sub AddParameter(ByVal objParm As clsProcessParameter)
        objParm.Process = mParent
        mParameters.Add(objParm)
    End Sub

    ''' <summary>
    ''' Adds the supplied list of parameters to the existing parameters.
    ''' </summary>
    ''' <param name="Params">The list of parameters to be added.</param>
    Public Sub AddParameters(ByVal params As IEnumerable(Of clsProcessParameter))
        ' FIXME: Should this be setting all the parameters Process property as per
        ' AddParameter(clsProcessParameter)?
        mParameters.AddRange(params)
    End Sub

    ''' <summary>
    ''' Empties the current list of parameters.
    ''' </summary>
    Public Sub ClearParameters()
        mParameters.Clear()
    End Sub

    ''' <summary>
    ''' Remove the specified parameter from this stage
    ''' </summary>
    ''' <param name="sName">The name of the parameter to remove</param>
    ''' <param name="dtDir">The data type of the parameter to be removed.</param>
    Public Sub RemoveParameter(ByVal sName As String, ByVal dtDir As ParamDirection)
        Dim objParm As clsProcessParameter = GetParameter(sName, dtDir)
        If Not objParm Is Nothing Then RemoveParameter(objParm)
    End Sub

    ''' <summary>
    ''' Remove the specified parameter from this stage
    ''' </summary>
    ''' <param name="objP">The parameter to remove</param>
    Public Sub RemoveParameter(ByVal objP As clsProcessParameter)
        mParameters.Remove(objP)
    End Sub


    ''' <summary>
    ''' Attempt to move the corner of an object. If the new setting is invalid, no
    ''' action is taken.
    ''' </summary>
    ''' <param name="dblX">The new X location, in world coordinates, of the corner
    ''' being resized.</param>
    ''' <param name="dblY">The new Y location, in world coordinates, of the corner
    ''' being resized.</param>
    Public Overridable Sub SetCorner(ByVal dblX As Single, ByVal dblY As Single, ByVal iCorner As Integer)
        'Make the new corner position relative to the centre
        'of the object...
        dblX = dblX - msngDisplayX
        dblY = dblY - msngDisplayY

        'Figure out the new width and height based on that...
        Dim dblWidth As Single, dblHeight As Single
        dblWidth = Math.Abs(dblX) * 2
        dblHeight = Math.Abs(dblY) * 2

        'Make sure the new size is something approaching
        'reasonable before storing...
        If dblWidth > 5 Then msngDisplayWidth = dblWidth
        If dblHeight > 5 Then msngDisplayHeight = dblHeight
    End Sub

    ''' <summary>
    ''' Cloning must be split into two operations, creating an instance of the class
    ''' which is different depending on the subclass, and copying all the classes 
    ''' members, for which each sub class appends some additional calls.
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function CloneCreate() As clsProcessStage

    ''' <summary>
    ''' Clone this stage object.
    ''' </summary>
    ''' <returns>The clone</returns>
    Public Overridable Function Clone() As clsProcessStage
        Dim copy As clsProcessStage = CloneCreate()
        copy.SetSubSheetID(Me.GetSubSheetID)
        copy.SetStageID(Me.GetStageID)
        copy.SetNarrative(Me.GetNarrative)
        copy.Name = Me.Name
        copy.msInitialName = Me.InitialName
        copy.SetDisplayY(Me.GetDisplayY)
        copy.SetDisplayX(Me.GetDisplayX)
        copy.SetDisplayWidth(Me.GetDisplayWidth)
        copy.SetDisplayHeight(Me.GetDisplayHeight)
        If Me.HasBreakPoint Then
            copy.BreakPoint = New clsProcessBreakpoint(copy)
            copy.BreakPoint.Condition = Me.BreakPoint.Condition          'behaves as value type, ok to copy
            copy.BreakPoint.BreakPointType = Me.BreakPoint.BreakPointType              'value type, ok to copy
        End If
        copy.LogInhibit = Me.LogInhibit
        copy.LogParameters = Me.LogParameters
        copy.WarningThreshold = Me.WarningThreshold
        CollectionUtil.CloneInto(Me.Preconditions, copy.Preconditions)
        CollectionUtil.CloneInto(Me.PostConditions, copy.PostConditions)
        For Each p As clsProcessParameter In Me.GetParameters
            copy.AddParameter((CType(p.Clone(), clsProcessParameter)))
        Next
        copy.Color = Me.Color
        copy.FontFamily = Me.FontFamily
        copy.FontSize = Me.FontSize
        copy.FontSizeUnit = Me.FontSizeUnit
        copy.FontStyle = Me.FontStyle

        Return copy
    End Function


    ''' <summary>
    ''' Populate the instance with values read from XML. This can ONLY be used on a
    ''' clean newly constructed clsProcessStage.
    ''' </summary>
    ''' <param name="stgEl">The base element of the 'stage' structure</param>
    Public Overridable Sub FromXML(ByVal stgEl As XmlElement)

        Me.SetStageID(New Guid(stgEl.GetAttribute("stageid")))

        Me.Name = stgEl.GetAttribute("name")
        SetFontFromDefault()
        Me.Narrative = ""

        'Now read all the child nodes of the <stage> element
        'and pass the information to the stage object...

        'Default to system wide warning threshold for backwards compatibility
        Me.WarningThreshold = DefaultWarningThreshold

        For Each propEl As XmlElement In stgEl.ChildNodes
            Select Case propEl.Name
                Case "subsheetid"
                    SetSubSheetID(New Guid(propEl.InnerText))
                Case "displayx"
                    SetDisplayX(InternalCulture.Sng(propEl.InnerText))
                Case "displayy"
                    SetDisplayY(InternalCulture.Sng(propEl.InnerText))
                Case "displaywidth"
                    SetDisplayWidth(InternalCulture.Sng(propEl.InnerText))
                Case "displayheight"
                    SetDisplayHeight(InternalCulture.Sng(propEl.InnerText))
                'Using Cases displayx, displayy, displaywidth and displayheight for backwards compatibility
                Case "display"
                    SetDisplayX(InternalCulture.Sng(propEl.GetAttribute("x")))
                    SetDisplayY(InternalCulture.Sng(propEl.GetAttribute("y")))
                    if propEl.HasAttribute("w") AndAlso propEl.HasAttribute("h")
                        SetDisplayWidth(InternalCulture.Sng(propEl.GetAttribute("w")))
                        SetDisplayHeight(InternalCulture.Sng(propEl.GetAttribute("h")))
                    End If
                Case "font"
                    FontStyle = clsEnum(Of FontStyle).Parse(propEl.GetAttribute("style"))
                    FontFamily = propEl.GetAttribute("family")
                    FontSize = InternalCulture.Sng(propEl.GetAttribute("size"))
                    FontSizeUnit = GraphicsUnit.Pixel

                    Me.Color = Color.FromArgb( _
                     Integer.Parse("ff" & propEl.GetAttribute("color"), _
                     NumberStyles.HexNumber))

                Case "inputs"
                    For Each inp As XmlElement In propEl.ChildNodes
                        If inp.Name = "input" Then SetInputXML(inp.OuterXml)
                    Next
                Case "outputs"
                    For Each outp As XmlElement In propEl.ChildNodes
                        If outp.Name = "output" Then SetOutputXML(outp.OuterXml)
                    Next
                Case "preconditions"
                    For Each precon As XmlElement In propEl.ChildNodes
                        Preconditions.Add(precon.GetAttribute("narrative"))
                    Next
                Case "postconditions"
                    For Each postcon As XmlElement In propEl.ChildNodes
                        PostConditions.Add(postcon.GetAttribute("narrative"))
                    Next
                Case "narrative"
                    Me.SetNarrative(propEl.InnerText)
                Case "loginhibit"
                    Me.LogInhibit = LogInfo.InhibitModes.Always
                    If propEl.GetAttribute("onsuccess") = "true" Then
                        Me.LogInhibit = LogInfo.InhibitModes.OnSuccess
                    End If
                Case "warningthreshold"
                    Me.WarningThreshold = CInt(propEl.InnerText)
                Case "loginhibitparameters"
                    Me.LogParameters = False
                Case "Breakpoint", "breakpoint"
                    Me.BreakPoint = clsProcessBreakpoint.FromXML(propEl, Me)
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Generate the XML for all the properties associated with this stage.
    ''' </summary>
    ''' <param name="ParentDocument">The XmlDocument being added to</param>
    ''' <param name="StageElement">The stage node for this stage,
    ''' to which all elements generated here will be added.</param>
    Public Overridable Sub ToXml(ByVal ParentDocument As XmlDocument, ByVal StageElement As XmlElement, ByVal bSelectionOnly As Boolean)
        Dim e2, e3 As XmlElement
        Dim bIncludeLink As Boolean

        StageElement.SetAttribute("stageid", GetStageID().ToString())
        StageElement.SetAttribute("name", GetName())
        'Subsheet info on main page acts as process info, so don't worry here about converting stage types 

        StageElement.SetAttribute("type", StageType.ToString())

        If Me.OverrideDefaultWarningThreshold Then
            e2 = ParentDocument.CreateElement("warningthreshold")
            e2.AppendChild(ParentDocument.CreateTextNode(Me.WarningThreshold.ToString()))
            StageElement.AppendChild(e2)
        End If
        If Me.LogInhibit <> LogInfo.InhibitModes.Never Then
            e2 = ParentDocument.CreateElement("loginhibit")
            If Me.LogInhibit = LogInfo.InhibitModes.OnSuccess Then
                e2.SetAttribute("onsuccess", "true")
            End If
            StageElement.AppendChild(e2)
        End If
        If Not Me.LogParameters Then
            e2 = ParentDocument.CreateElement("loginhibitparameters")
            StageElement.AppendChild(e2)
        End If
        'add preconditions
        e2 = ParentDocument.CreateElement("preconditions")
        If mPreconditions IsNot Nothing AndAlso mPreconditions.Count > 0 Then
            Dim sCond As String
            For Each sCond In Me.Preconditions
                e3 = ParentDocument.CreateElement("condition")
                e3.SetAttribute("narrative", sCond)
                e2.AppendChild(e3)
            Next
            If e2.ChildNodes.Count > 0 Then StageElement.AppendChild(e2)
        End If
        'and postconditions
        e2 = ParentDocument.CreateElement("postconditions")
        If mPreconditions IsNot Nothing AndAlso mPreconditions.Count > 0 Then
            Dim sCond As String
            For Each sCond In Me.PostConditions
                e3 = ParentDocument.CreateElement("condition")
                e3.SetAttribute("narrative", sCond)
                e2.AppendChild(e3)
            Next
            If e2.ChildNodes.Count > 0 Then StageElement.AppendChild(e2)
        End If

        Dim narrative = Me.GetNarrative()
        If Not String.IsNullOrEmpty(narrative) Then
            e2 = ParentDocument.CreateElement("narrative")
            e2.AppendChild(ParentDocument.CreateTextNode(narrative))
            StageElement.AppendChild(e2)
        End If

        e2 = ParentDocument.CreateElement("display")
        e2.SetAttribute("x", Me.GetDisplayX().ToString)
        e2.SetAttribute("y", Me.GetDisplayY().ToString)

        If Not (msngDisplayWidth.Equals(mDefaultStageWidth) AndAlso msngDisplayHeight = mDefaultStageHeight) Then
            e2.SetAttribute("w", Me.GetDisplayWidth().ToString)
            e2.SetAttribute("h", Me.GetDisplayHeight().ToString)
        End If

        StageElement.AppendChild(e2)

        If Not (Font.Equals(mDefaultFont) AndAlso Color.ToArgb = mDefaultFontColor.ToArgb) Then
            'Formatting options
            e2 = ParentDocument.CreateElement("font")
            e2.SetAttribute("family", Me.Font.FontFamily.Name)
            e2.SetAttribute("size", Me.Font.Size.ToString)
            e2.SetAttribute("style", Me.Font.Style.ToString)
            e2.SetAttribute("color", Hex(Me.Color.R).PadLeft(2, "0"c) & Hex(Me.Color.G).PadLeft(2, "0"c) & Hex(Me.Color.B).PadLeft(2, "0"c))
            StageElement.AppendChild(e2)
        End If

        Select Case StageType
            Case StageTypes.Action, StageTypes.Skill, StageTypes.Start, StageTypes.End, StageTypes.Process, StageTypes.SubSheet, StageTypes.Code

                'Inputs
                Dim inputs As List(Of clsProcessParameter) = GetInputs()
                If inputs.Count > 0 Then
                    e2 = ParentDocument.CreateElement("inputs")
                    For Each p As clsProcessParameter In inputs
                        e3 = p.ToXML(ParentDocument)
                        e2.AppendChild(e3)
                    Next
                    StageElement.AppendChild(e2)
                End If

                'Outputs
                Dim outputs As List(Of clsProcessParameter) = GetOutputs()
                If outputs.Count > 0 Then
                    e2 = ParentDocument.CreateElement("outputs")
                    For Each p As clsProcessParameter In outputs
                        e3 = p.ToXML(ParentDocument)
                        e2.AppendChild(e3)
                    Next
                    StageElement.AppendChild(e2)
                End If

        End Select

        If TypeOf Me Is clsLinkableStage Then
            Dim Linkable As clsLinkableStage = CType(Me, clsLinkableStage)
            If Not Linkable.OnSuccess.Equals(Guid.Empty) Then
                bIncludeLink = True
                If bSelectionOnly Then
                    If Not mParent.IsStageSelected(Linkable.OnSuccess) Then
                        bIncludeLink = False
                    End If
                End If
                If bIncludeLink Then
                    e2 = ParentDocument.CreateElement("onsuccess")
                    e2.AppendChild(ParentDocument.CreateTextNode(Linkable.OnSuccess.ToString()))
                    StageElement.AppendChild(e2)
                End If
            End If
        End If

        'record breakpoint info
        If Me.BreakPoint IsNot Nothing Then
            e2 = Me.BreakPoint.ToXML(ParentDocument)
            If e2 IsNot Nothing Then StageElement.AppendChild(e2)
        End If
    End Sub

    ''' <summary>
    ''' Determines if this stage is included in its parent process' selection.
    ''' </summary>
    ''' <returns>Returns True if it is part of the selection of the stage's parent
    ''' process; False otherwise.</returns>
    Public Function IsSelected() As Boolean
        Return (mParent IsNot Nothing) AndAlso mParent.IsStageSelected(Me.mgStageID)
    End Function

    Public Overridable Sub SetContext(context As Object)
        Exit Sub
    End Sub

        ''' <summary>
    ''' Get the date time that the stage was started
    ''' </summary>
    Public Property DateTimeStarted() As DateTimeOffset
        Get
            Return mDateTimeStarted
        End Get
        Set(ByVal Value As DateTimeOffset)
            mDateTimeStarted = Value
        End Set
    End Property    
    <DataMember>
    Private mDateTimeStarted As DateTimeOffset

    Friend Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
        Return {
            GetType(clsActionStage),
            GetType(clsAlertStage),
            GetType(clsAnchorStage),
            GetType(clsBlockStage),
            GetType(clsCalculationStage),
            GetType(clsChoiceEndStage),
            GetType(clsChoiceStartStage),
            GetType(clsCodeStage),
            GetType(clsCollectionStage),
            GetType(clsDecisionStage),
            GetType(clsEndStage),
            GetType(clsExceptionStage),
            GetType(clsLoopEndStage),
            GetType(clsLoopStartStage),
            GetType(clsMultipleCalculationStage),
            GetType(clsNavigateStage),
            GetType(clsNoteStage),
            GetType(clsReadStage),
            GetType(clsRecoverStage),
            GetType(clsResumeStage),
            GetType(clsSkillStage),
            GetType(clsStartStage),
            GetType(clsSubProcessRefStage),
            GetType(clsSubsheetInfoStage),
            GetType(clsSubSheetRefStage),
            GetType(clsWaitEndStage),
            GetType(clsWaitStartStage),
            GetType(clsWriteStage)
        }

    End Function
End Class

