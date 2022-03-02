Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

Friend Class frmStagePropertiesReadWriteBase
    Implements IDataItemTreeRefresher

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        objDataItemTreeView.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        objDataItemTreeView.RemoveDataItemTreeNode(stage)
    End Sub

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        Dim objProcess As clsProcess = mProcessStage.Process
        If Not objProcess Is Nothing Then
            Dim objDefinition As clsApplicationDefinition = objProcess.ApplicationDefinition
            If Not objDefinition Is Nothing Then
                Me.ctlListPair.ApplicationDefinition = objDefinition
                Me.CtlApplicationExplorer1.LoadApplicationDefinition(objDefinition)
            End If
        End If

        Me.ctlListPair.Stage = mProcessStage
        Me.ctlListPair.ProcessViewer = Me.ProcessViewer
        objDataItemTreeView.Populate(mProcessStage)
        Me.objDataItemTreeView.ProcessViewer = Me.ProcessViewer

        ctlListPair.TabControl1.Height = ctlListPair.btnAdd.Top - ctlListPair.TabControl1.Top - 8
    End Sub


    Protected Overrides Function ApplyChanges() As Boolean
        If MyBase.ApplyChanges() Then

            ctlListPair.ctlActions.EndEditing()
            ctlListPair.ctlArguments.EndEditing()

            Dim objAppStage As clsAppStage = TryCast(mProcessStage, clsAppStage)
            objAppStage.Steps.Clear()

            For Each objRow As clsListRow In Me.ctlListPair.ctlActions.Rows
                Dim objReadWriteRow As clsReadWriteListRow = TryCast(objRow, clsReadWriteListRow)
                If Not objReadWriteRow Is Nothing Then
                    Dim objStep As clsStep = objReadWriteRow.ReadWriteStep
                    If Not objStep Is Nothing Then
                        objAppStage.Steps.Add(objStep)
                    End If
                End If
            Next
            Return True
        End If
        Return False
    End Function

    Protected Overridable Function GetCurrentAction() As String
        Return String.Empty
    End Function
End Class
