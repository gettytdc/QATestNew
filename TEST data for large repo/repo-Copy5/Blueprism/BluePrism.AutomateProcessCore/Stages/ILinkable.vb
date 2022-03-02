Public Interface ILinkable

    ''' <summary>
    ''' Sets the next link to the specified value.
    ''' </summary>
    ''' <param name="dest">The ID of the stage to which a link should be made.
    ''' </param>
    ''' <remarks>Each implementor may have their own policy about what the next link
    ''' is: ordinary process stages will change one link only; decision stages may
    ''' choose which link to change; etc.</remarks>
    Function SetNextLink(ByVal dest As Guid, ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Provides access to the guid of the stage that is being linked to.
    ''' </summary>
    Property OnSuccess As Guid
End Interface
