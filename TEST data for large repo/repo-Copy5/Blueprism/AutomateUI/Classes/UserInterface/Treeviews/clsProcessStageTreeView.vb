
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : clsProcessStageTreeView
'''
''' <summary>
''' A tree view to display process stages arranged by page and stage type.
''' </summary>
Public Class clsProcessStageTreeView
    Inherits clsAutoCheckingTreeView


    Private mProcess As clsProcess
    Private mStagesInSubSheet As Dictionary(Of Guid, List(Of clsProcessStage))


    Public Sub SetProcess(ByVal process As clsProcess)

        mProcess = process

        mStagesInSubSheet = New Dictionary(Of Guid, List(Of clsProcessStage))

        'Make lists to hold the stages in each page and collect them in a hashtable.
        For Each subsheet As clsProcessSubSheet In process.SubSheets
            mStagesInSubSheet.Add(subsheet.ID, New List(Of clsProcessStage))
        Next

        'Collect stage objects, keyed on subsheet ID.
        For Each stage As clsProcessStage In process.GetStages()
            mStagesInSubSheet(stage.GetSubSheetID()).Add(stage)
        Next

        TreeViewNodeSorter = New NodeSorter()

    End Sub

    Public Sub Populate(ByVal iStageTypes As StageTypes)

        Dim sSubSheetName As String
        Dim shtNode, typeNode, stgNode As TreeNode
        Dim iIndex As Integer
        Dim iStageType As StageTypes
        Dim sStageType, sObject, sAction As String

        Nodes.Clear()

        For Each ssId As Guid In mStagesInSubSheet.Keys

            sSubSheetName = mProcess.GetSubSheetName(ssId)
            shtNode = New TreeNode(sSubSheetName)
            'oSheetNode.NodeFont = New Font(Me.Font, FontStyle.Bold)
            shtNode.Tag = ssId
            Me.Nodes.Add(shtNode)

            iIndex = 0
            While 2 ^ iIndex <= CInt(iStageTypes)
                If (CLng(2 ^ iIndex) And iStageTypes) > 0 Then
                    iStageType = CType(2 ^ iIndex, StageTypes)
                    sStageType = clsStageTypeName.GetLocalizedFriendlyName(iStageType.ToString(), True)
                    typeNode = New TreeNode(sStageType)
                    typeNode.Tag = iStageType

                    shtNode.Nodes.Add(typeNode)
                End If
                iIndex += 1
            End While

            For Each stg As clsProcessStage In mStagesInSubSheet(ssId)

                For Each typeNode In shtNode.Nodes
                    If stg.StageType.Equals(typeNode.Tag) Then
                        stgNode = New TreeNode(stg.GetName)
                        stgNode.Tag = stg
                        typeNode.Nodes.Add(stgNode)
                        Select Case stg.StageType

                            Case StageTypes.Calculation, StageTypes.Decision
                                stgNode.ToolTipText =
                                 CType(stg, IExpressionHolder).Expression.LocalForm

                            Case StageTypes.Action
                                sObject = ""
                                sAction = ""
                                CType(stg, Stages.clsActionStage).GetResource(sObject, sAction)
                                If sObject <> "" And sAction <> "" Then
                                    stgNode.ToolTipText = sObject & "." & sAction
                                End If

                            Case StageTypes.Skill
                                Dim skill = CType(stg, Stages.clsSkillStage)
                                stgNode.ToolTipText = skill.ActionName

                            Case StageTypes.Data
                                stgNode.ToolTipText = CType(stg, Stages.clsDataStage).GetShortText

                            Case Else
                                If stg.GetNarrative <> "" Then
                                    stgNode.ToolTipText = stg.GetNarrative
                                End If
                        End Select

                        Exit For
                    End If
                Next

            Next

        Next

    End Sub

    Public Sub EnsureVisible(ByVal gSubSheetID As Guid)

        For Each oSheetNode As TreeNode In Me.Nodes
            If gSubSheetID.Equals(oSheetNode.Tag) Then
                oSheetNode.EnsureVisible()
                Exit For
            End If
        Next

    End Sub

    Public Sub CheckSubSheet(ByVal gSubSheetID As Guid)

        For Each oSheetNode As TreeNode In Me.Nodes
            If gSubSheetID.Equals(oSheetNode.Tag) Then
                oSheetNode.Checked = True
                Exit For
            End If
        Next

    End Sub

    Public Sub Expand(ByVal gSubSheetID As Guid, Optional ByVal bExpandChildren As Boolean = True)

        For Each oSheetNode As TreeNode In Me.Nodes
            If gSubSheetID.Equals(oSheetNode.Tag) Then
                oSheetNode.Expand()
                If bExpandChildren Then
                    For Each oTypeNode As TreeNode In oSheetNode.Nodes
                        oTypeNode.Expand()
                    Next
                End If
                Exit For
            End If
        Next

    End Sub

    Public Function SomeStagesAreChecked() As Boolean

        For Each oSheetNode As TreeNode In Me.Nodes
            For Each oTypeNode As TreeNode In oSheetNode.Nodes
                For Each oStageNode As TreeNode In oTypeNode.Nodes
                    If oStageNode.Checked Then
                        Return True
                    End If
                Next
            Next
        Next
        Return False

    End Function

    Public Function AllStagesAreChecked() As Boolean

        Dim iUnChecked As Integer

        For Each oSheetNode As TreeNode In Me.Nodes
            For Each oTypeNode As TreeNode In oSheetNode.Nodes
                For Each oStageNode As TreeNode In oTypeNode.Nodes
                    If Not oStageNode.Checked Then
                        iUnChecked += 1
                    End If
                Next
            Next
        Next
        Return iUnChecked = 0

    End Function

    Public Function SubSheetIsChecked(ByVal gSubSheetID As Guid) As Boolean

        For Each oSheetNode As TreeNode In Me.Nodes
            If gSubSheetID.Equals(oSheetNode.Tag) Then
                Return oSheetNode.Checked
            End If
        Next
        Return False

    End Function

    Public Function GetStageTypesOfCheckedStages() As StageTypes

        Dim iStageType As StageTypes

        For Each oSheetNode As TreeNode In Me.Nodes
            For Each oTypeNode As TreeNode In oSheetNode.Nodes
                If (iStageType And CType(oTypeNode.Tag, StageTypes)) > 0 Then
                    'Got this stage type already
                Else
                    For Each oStageNode As TreeNode In oTypeNode.Nodes
                        If oStageNode.Checked Then
                            iStageType = iStageType Or CType(oTypeNode.Tag, StageTypes)
                            Exit For
                        End If
                    Next
                End If
            Next
        Next
        Return iStageType

    End Function

    Public Function GetCheckedStageIDs() As Guid()

        Dim aCheckedStages As New List(Of Guid)

        For Each oSheetNode As TreeNode In Me.Nodes
            For Each oTypeNode As TreeNode In oSheetNode.Nodes
                For Each oStageNode As TreeNode In oTypeNode.Nodes
                    If oStageNode.Checked Then
                        aCheckedStages.Add(CType(oStageNode.Tag, clsProcessStage).GetStageID)
                    End If
                Next
            Next
        Next
        Return aCheckedStages.ToArray()

    End Function

    Private Class NodeSorter
        Implements IComparer

        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare

            Return String.Compare(CType(x, TreeNode).Text, CType(y, TreeNode).Text)

        End Function

    End Class

End Class
