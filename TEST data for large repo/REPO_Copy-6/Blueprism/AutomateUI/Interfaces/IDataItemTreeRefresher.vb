''' <summary>
''' Interface to mark a frmProperties as an object that contains a ctlDataItemTreeView
''' and may need to be updated from an external source.
''' </summary>
Public Interface IDataItemTreeRefresher
    ''' <summary>
    ''' Get the stage that this form references
    ''' </summary>
    ReadOnly Property Stage As BluePrism.AutomateProcessCore.clsProcessStage
    ''' <summary>
    ''' Hook to this classes ctlDataItemTreeview Repopulate function to refresh the data item tree view.
    ''' </summary>
    ''' <param name="displayStage">A stage whose corresponding node is to be made</param>
    Sub Repopulate(displayStage As BluePrism.AutomateProcessCore.Stages.clsDataStage)
    ''' <summary>
    ''' Hook to this classes ctlDataItemTreeview to remove a node holding the given stage
    ''' </summary>
    ''' <param name="stage">A stage whose corresponding node is to be removed</param>
    Sub Remove(stage as BluePrism.AutomateProcessCore.Stages.clsDataStage)
End Interface
