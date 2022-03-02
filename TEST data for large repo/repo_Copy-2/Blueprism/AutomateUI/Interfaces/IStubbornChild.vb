
''' <summary>
''' Interface to describe a child of a Blue Prism application which may impede the
''' application from closing or the child page from being navigated away from.
''' </summary>
Friend Interface IStubbornChild
    Inherits IChild

    ''' <summary>
    ''' Checks if the child page is currently leaveable or not.
    ''' </summary>
    ''' <returns>True if the child will currently allow the application to be closed;
    ''' False if more work is required.</returns>
    Function CanLeave() As Boolean

End Interface
