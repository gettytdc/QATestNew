''' <summary>
''' Interface for the group details
''' </summary>
Public Interface IGroupObjectDetails : Inherits IObjectDetails
    ''' <summary>
    ''' The children of the group
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property Children As IList(Of IObjectDetails)
End Interface
