Imports AutomateUI.Controls.Widgets.SystemManager.WebApi

''' <summary>
''' Panel to display a summary of actions within the managed WebApi
''' </summary>
Friend Class WebApiActionSummary : Implements IGuidanceProvider

    ''' <summary>
    ''' Event fired when an action in this summary panel is 'activated' - eg.
    ''' double clicked.
    ''' </summary>
    Public Event ActionActivated As EventHandler

    ''' <summary>
    ''' Gets or sets the actions associated with this summary control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Actions As ICollection(Of WebApiActionDetails)
        Get
            Return DirectCast(Tag, ICollection(Of WebApiActionDetails))
        End Get
        Set(value As ICollection(Of WebApiActionDetails))
            Tag = value
            gridActions.Rows.Clear()
            If value IsNot Nothing Then
                For Each act In value
                    Dim index = gridActions.Rows.Add(
                        act.Name,
                        act.Description,
                        act.Request.Method?.Method,
                        act.Request.UrlPath
                    )
                    gridActions.Rows(index).Tag = act
                Next
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the guidance text for this panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property GuidanceText As String _
     Implements IGuidanceProvider.GuidanceText
        Get
            Return WebApi_Resources.GuidanceActionSummary
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the selected action in this summary panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedAction As WebApiActionDetails
        Get
            Return DirectCast(gridActions.SelectedRow?.Tag, WebApiActionDetails)
        End Get
        Set(value As WebApiActionDetails)
            For Each r As DataGridViewRow In gridActions.Rows
                If r.Tag Is value Then
                    gridActions.SelectedRow = r
                    gridActions.CurrentCell = r.Cells(0)
                    Return
                End If
            Next
        End Set
    End Property

    ''' <summary>
    ''' Raises the <see cref="ActionActivated"/> event.
    ''' </summary>
    Protected Overridable Sub OnActionActivated(e As EventArgs)
        RaiseEvent ActionActivated(Me, e)
    End Sub

    ''' <summary>
    ''' Handles an action being double-clicked, passing on the event as an
    ''' <see cref="ActionActivated"/> event.
    ''' </summary>
    Private Sub HandleCellDoubleClick(
     sender As Object, e As DataGridViewCellEventArgs) _
     Handles gridActions.CellDoubleClick
        If e.RowIndex < 0 OrElse e.RowIndex >= gridActions.RowCount Then Return
        OnActionActivated(EventArgs.Empty)
    End Sub

End Class
