''' <summary>
''' Interface describing a control which can handle MenuButton events.
''' </summary>
Public Interface IMenuButtonHandler

    ''' <summary>
    ''' Gets the ContextMenuStrip to associate with the MenuButton hosted by the
    ''' container of the control. Null will be treated as a statement to not show
    ''' the menu button for this control
    ''' </summary>
    ReadOnly Property MenuStrip As ContextMenuStrip

End Interface
