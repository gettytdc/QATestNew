Namespace Controls.Widgets.SystemManager.WorkQueueAnalysis
    ''' <summary>
    ''' This class exists to fix the strange scroll-jumping behaviour when you click on one
    ''' of the cells within a DataGridView that is placed within a FlowLayoutPanel.
    ''' </summary>
    <ToolboxItem(True)>
    Public Class NoneAutoScrollingFlowLayoutPanel
        Inherits FlowLayoutPanel

        Protected Overrides Function ScrollToControl(activeControl As Control) As Point
            Return AutoScrollPosition
        End Function

    End Class
End NameSpace