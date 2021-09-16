Option Strict On

''' Project  : Automate
''' Class    : clsAutomateListView
''' 
''' <summary>
''' This class extends the listview to add new features such as ctrl+a to select
''' all.
''' </summary>
Public Class clsAutomateListView
    Inherits AutomateControls.ScrollFiringListView

    ''' <summary>
    ''' The column resize event.
    ''' </summary>
    ''' <param name="sender">The event sender</param>
    Public Event ColumnsResized(ByVal sender As Object)

    ''' <summary>
    ''' Performs actions and raises events when a ListView column is resized.
    ''' </summary>
    Protected Overrides Sub OnColumnWidthChanged(ByVal e As ColumnWidthChangedEventArgs)
        MyBase.OnColumnWidthChanged(e)
        RaiseEvent ColumnsResized(Me)
    End Sub

    ''' <summary>
    ''' Handle a keyup event to process Ctrl-A presses (ie. select all)
    ''' </summary>
    Protected Overrides Sub OnKeyUp(ByVal e As KeyEventArgs)
        If e.Control AndAlso e.KeyCode = Keys.A Then
            For Each item As ListViewItem In Me.Items
                item.Selected = True
            Next
        End If
        MyBase.OnKeyUp(e)
    End Sub

End Class
