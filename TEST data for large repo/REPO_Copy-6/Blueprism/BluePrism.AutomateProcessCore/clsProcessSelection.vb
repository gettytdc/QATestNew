''' <summary>
''' This class represents a single selected item in a process. The
''' total selection is defined by a collection of these objects.
''' </summary>
Public Class clsProcessSelection

    ''' <summary>
    ''' The type of object selected
    ''' </summary>
    Public mtType As SelectionType

    ''' <summary>
    ''' The selection type
    ''' </summary>
    Public Enum SelectionType As Integer
        None
        Multiple
        Stage
        Link
        ChoiceNode
        ChoiceLink
    End Enum

    ''' <summary>
    ''' The id of the stage selected - also relevant for a link
    ''' </summary>
    Public mgStageID As Guid

    ''' <summary>
    ''' Where the object selected is a "Link", this determines the
    ''' link type, e.g. "True", "Success", etc.
    ''' </summary>
    Public msLinkType As String

    ''' <summary>
    ''' Where the object selected is a ChoiceNode this determines
    ''' which of the nodes is selected.
    ''' </summary>
    Public miChoiceIndex As Integer

End Class

