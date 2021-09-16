Imports BluePrism.Core.Extensions
Imports LocaleTools

''' <summary>
''' A class to help populate undo and redo menu items.
''' </summary>
''' <remarks></remarks>
Public Class clsUndo

#Region "Members"

    ''' <summary>
    ''' The XML state of the undo position.
    ''' </summary>
    ''' <remarks></remarks>
    Public State As String

    ''' <summary>
    ''' The XML state bytes of the undo position.
    ''' </summary>
    ''' <remarks></remarks>
    Public StateBytes As Byte() 

    ''' <summary>
    ''' A summary of the undo position.
    ''' </summary>
    ''' <remarks></remarks>
    Public Summary As String

    ''' <summary>
    ''' A description of the undo position.
    ''' </summary>
    ''' <remarks></remarks>
    Public Description As String

    ''' <summary>
    ''' The type of action that created the undo position.
    ''' </summary>
    ''' <remarks></remarks>
    Public TypeOfAction As ActionType

    ''' <summary>
    ''' The type of action that will reverse the undo.
    ''' </summary>
    ''' <remarks></remarks>
    Private TypeOfUndoAction As UndoActionType

    ''' <summary>
    ''' A flag to indicate that a redo has become an undo. Rather than 
    ''' display 'undo redo change properties', display 'undo reset properties'.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbReversed As Boolean

    ''' <summary>
    ''' The types of action that can propmt an undo point.
    ''' </summary>
    ''' <remarks>Used to set text on undo menu items</remarks>
    Public Enum ActionType
        [Default]
        Create
        Delete
        ChangePositionOf
        ChangeSizeOf
        ChangePropertiesOf
        Publish
        ChangeNameOf
        ChangePageOrder
        LinkTo
        SwitchLink
        Cut
        Paste
        ChangeFontOf
        ChangeFontColourOf
        ChangeFontSizeOf
        ChangeFontStyleOf
        ChangeLoggingOf
        Breakpoint

    End Enum

    ''' <summary>
    ''' The reverse of actions that can propmt an undo point.
    ''' </summary>
    ''' <remarks>Used to set text on undo menu items</remarks>
    Private Enum UndoActionType
        [Default]
        Remove
        Restore
        ResetPositionOf
        ResetSizeOf
        ResetPropertiesOf
        Unpublish
        ResetNameOf
        ResetPageOrder
        RemoveLinkTo
        ReswitchLink
        Uncut
        Unpaste
        ResetFontOf
        ResetFontColourOf
        ResetFontSizeOf
        ResetFontStyleOf
        ResetLoggingOf

    End Enum

#End Region

#Region "New"

    Public Sub New()
        Me.New("", "", "")
    End Sub

    Public Sub New(ByVal sState As String)

        Me.New(sState, "", "")

    End Sub

    Public Sub New(ByVal sState As String, ByVal sSummary As String, ByVal sDescription As String)

        State = sState
        Summary = sSummary
        Description = sDescription
        TypeOfAction = ActionType.Default
        TypeOfUndoAction = UndoActionType.Default

    End Sub

    Public Sub New(ByVal sState As String, ByVal iActionType As ActionType, ByVal sSummary As String, ByVal sDescription As String)

        State = sState
        Summary = sSummary
        Description = sDescription
        TypeOfAction = iActionType
        TypeOfUndoAction = CType(iActionType, UndoActionType)

    End Sub

    Public Sub New(ByVal sState As String, ByVal iActionType As ActionType)

        Me.New(sState, iActionType, "", "")

    End Sub


    Public Sub New(ByVal sState As String, ByVal iActionType As ActionType, ByVal oStage As clsProcessStage)

        Me.New(sState, iActionType, clsStageTypeName.GetLocalizedFriendlyName(oStage.StageType.ToString()), "'" & oStage.GetName & "'")

    End Sub

    Public Sub New(ByVal sState As String, ByVal iActionType As ActionType, ByVal aStages As clsProcessStage())

        If aStages.Length = 1 Then
            State = sState
            Summary = clsStageTypeName.GetLocalizedFriendlyName(aStages(0).StageType.ToString())
            Description = "'" & aStages(0).GetName & "'"
            TypeOfAction = iActionType
            TypeOfUndoAction = CType(iActionType, UndoActionType)
        Else
            State = sState
            Summary = My.Resources.Resources.clsUndo_Selection
            Description = String.Format(My.Resources.Resources.clsUndo_Selected0Items, CStr(aStages.Length))
            TypeOfAction = iActionType
            TypeOfUndoAction = CType(iActionType, UndoActionType)
        End If

    End Sub

    Public Sub New(ByVal sState As String, ByVal iActionType As ActionType, ByVal oProcessSubSheet As clsProcessSubSheet)

        Me.New(sState, iActionType, My.Resources.Resources.clsUndo_Page, "'" & oProcessSubSheet.Name & "'")

    End Sub

#End Region

#Region "Clone"

    Public Function Clone() As clsUndo

        Dim oClone As New clsUndo
        oClone.State = Me.State
        oClone.Summary = Me.Summary
        oClone.Description = Me.Description
        oClone.TypeOfAction = Me.TypeOfAction
        oClone.TypeOfUndoAction = Me.TypeOfUndoAction
        oClone.mbReversed = Me.mbReversed
        Return oClone

    End Function

#End Region

#Region "Reverse"

    Public Sub Reverse()

        mbReversed = Not mbReversed

    End Sub

#End Region

#Region "ToString"

    Public Overrides Function ToString() As String

        Dim sAction As String

        If mbReversed Then
            sAction = GetLocalizedFriendlyName(TypeOfUndoAction.ToString)
        Else
            sAction = GetLocalizedFriendlyName(TypeOfAction.ToString)
        End If

        Select Case TypeOfAction
            Case ActionType.Default
                Return ""
            Case ActionType.ChangePageOrder
                Return sAction
            Case Else
                'For JA and other langs, linguists will need to switch the order of the Action and Summary/Description
                'Eg:
                'sAction = "Change position of processinfo"
                'Summary = ProcessInfo
                Return LTools.Format(My.Resources.Resources.clsUndo_ActionSummary, "Action", sAction, "Summary",  Summary.ToConditionalLowerNoun())
        End Select

    End Function

#End Region

#Region "Tooltip"

    Public Function ToolTip() As String

        Dim sAction As String

        If mbReversed Then
            sAction = GetLocalizedFriendlyName(TypeOfUndoAction.ToString)
        Else
            sAction = GetLocalizedFriendlyName(TypeOfAction.ToString)
        End If

        Select Case TypeOfAction
            Case ActionType.Default
                Return ""
            Case ActionType.ChangePageOrder
                Return GetSentenceCase(Description)
            Case Else
               Return LTools.Format(My.Resources.Resources.clsUndo_ActionDescription, "Action", sAction, "Description", Description)
        End Select

    End Function


#End Region

#Region "String utilities"

    Private Function GetSentenceCase(ByVal sentence As String) As String
        If sentence = "" Then
            Return sentence
        Else
            Return sentence.Substring(0, 1).ToUpper & sentence.Substring(1).ToLower
        End If
    End Function

    Private Function GetUnCamelled(ByVal camel As String) As String
        Return System.Text.RegularExpressions.Regex.Replace(camel, "[a-z][A-Z]", AddressOf InsertSpace)
    End Function

    Private Function InsertSpace(ByVal match As System.Text.RegularExpressions.Match) As String
        Return match.ToString().Chars(0) & " " & match.ToString().Chars(1)
    End Function

    ''' <summary>
    ''' Gets the localized friendly name For this undo type according To the current culture.
    ''' </summary>
    ''' <param name="type">The Undo type</param>
    ''' <returns>The name of the data type for the current culture</returns>
    Private Function GetLocalizedFriendlyName(type As String) As String
        Return My.Resources.Resources.ResourceManager.GetString($"clsUndo_{type}")
    End Function

#End Region

End Class
