
''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcess.ValidateProcessResult
''' 
''' <summary>
''' This class is used solely for the purpose of returning results from
''' ValidateProcess. A collection of these objects is returned.
''' </summary>
Public Class ValidateProcessResult

    ' The stage this result applies to, or null if it is not stage-specific
    Private mStage As clsProcessStage

    ''' <summary>
    ''' Get a formatted message for this result.
    ''' </summary>
    ''' <param name="text">The unformatted message text. This must be the text
    ''' appropriate to the CheckID of this result.</param>
    ''' <returns>The message text, formatted to include the relevant parameter
    ''' information.</returns>
    Public Function FormatMessage(ByVal text As String) As String
        Return String.Format(text, Parameters)
    End Function

    ''' <summary>
    ''' The check ID
    ''' </summary>
    Public ReadOnly Property CheckID() As Integer
        Get
            Return mCheckId
        End Get
    End Property
    Private mCheckId As Integer

    ''' <summary>
    ''' Parameters to the message
    ''' </summary>
    Public Parameters As Object()

    ''' <summary>
    ''' True if the error can be repaired
    ''' </summary>
    Public Overridable ReadOnly Property Repairable() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The source of the error which is one of normal or code.
    ''' </summary>
    Public ErrorSource As SourceTypes

    ''' <summary>
    ''' The different types of error source.
    ''' </summary>
    Public Enum SourceTypes
        Normal
        Code
    End Enum

    ''' <summary>
    ''' A help reference, either as an integer refering to a help topic (eg
    ''' "1234") or a help page url (eg "frmStagePropertiesData.htm#DataTypes").
    ''' </summary>
    Public HelpReference As String

    ''' <summary>
    ''' Creates a new non-repairable validate result item with no help ref.
    ''' </summary>
    ''' <param name="stg">The (non-null) stage to which it refers.</param>
    ''' <param name="checkId">The ID of the validation check which this object
    ''' represents.</param>
    ''' <param name="parms">The parameters for the validation check</param>
    Public Sub New( _
     ByVal stg As clsProcessStage, ByVal checkId As Integer, ByVal ParamArray parms() As Object)
        Me.New(stg, "", checkId, parms)
    End Sub

    ''' <summary>
    ''' Creates a new non-repairable validate result item with no help ref.
    ''' </summary>
    ''' <param name="stg">The (non-null) stage to which it refers.</param>
    ''' <param name="checkId">The ID of the validation check which this object
    ''' represents.</param>
    ''' <param name="parms">The parameters for the validation check</param>
    Public Sub New(ByVal stg As clsProcessStage, ByVal helpRef As String, _
     ByVal checkId As Integer, ByVal ParamArray parms() As Object)
        mStage = stg
        mCheckId = checkId
        Parameters = parms
        HelpReference = helpRef
    End Sub

    ''' <summary>
    ''' The stage, or Guid.Empty if not relevant
    ''' </summary>
    Public ReadOnly Property StageId() As Guid
        Get
            If mStage Is Nothing Then Return Guid.Empty
            Return mStage.GetStageID()
        End Get
    End Property

    ''' <summary>
    ''' The stage that this result refers to, or null if it is not a
    ''' stage-specific validation result.
    ''' </summary>
    Public ReadOnly Property Stage() As clsProcessStage
        Get
            Return mStage
        End Get
    End Property

    ''' <summary>
    ''' The name of the stage that this validate result refers to or an empty
    ''' string if it is not stage-specific.
    ''' </summary>
    Public ReadOnly Property StageName() As String
        Get
            If mStage Is Nothing Then Return ""
            Return mStage.Name
        End Get
    End Property

    ''' <summary>
    ''' The name of the page that the stage referred to in this result resides,
    ''' or an empty string if it does not refer directly to a specific stage.
    ''' </summary>
    Public ReadOnly Property PageName() As String
        Get
            If mStage Is Nothing Then Return ""
            Return mStage.SubSheet.Name
        End Get
    End Property

End Class

''' <summary>
''' A Validate Process Result which is repairable - adds nothing to the class,
''' but it means that repairable validation results can be found a lot easier
''' using 'Find All References'.
''' </summary>
Public Class RepairableValidateProcessResult : Inherits ValidateProcessResult

    ''' <summary>
    ''' Creates a new repairable validate result item with no help ref.
    ''' </summary>
    ''' <param name="stage">The (non-null) stage to which it refers.</param>
    ''' <param name="checkId">The ID of the validation check which this object
    ''' represents.</param>
    ''' <param name="parms">The parameters for the validation check</param>
    Public Sub New(ByVal stage As clsProcessStage, _
     ByVal checkId As Integer, ByVal ParamArray parms() As Object)
        MyBase.New(stage, "", checkId, parms)
    End Sub

    ''' <summary>
    ''' Creates a new repairable validate result item with the given help ref.
    ''' </summary>
    ''' <param name="stage">The (non-null) stage to which it refers.</param>
    ''' <param name="helpRef">The help reference for this error</param>
    ''' <param name="checkId">The ID of the validation check which this object
    ''' represents.</param>
    ''' <param name="parms">The parameters for the validation check</param>
    Public Sub New(ByVal stage As clsProcessStage, _
     ByVal helpRef As String, ByVal checkId As Integer, ByVal ParamArray parms() As Object)
        MyBase.New(stage, helpRef, checkId, parms)
    End Sub

    ''' <summary>
    ''' True if the error can be repaired. This can be repaired
    ''' </summary>
    Public Overrides ReadOnly Property Repairable() As Boolean
        Get
            Return True
        End Get
    End Property

End Class
