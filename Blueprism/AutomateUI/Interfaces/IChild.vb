''' Project  : Automate
''' Interface    : IChild
''' 
''' <summary>
''' This interface is used to define the setparent function that is called on all
''' child controls so that they can make callbacks to frmApplication.
''' </summary>
Friend Interface IChild
    ''' <summary>
    ''' Sets the callback object for the child form.
    ''' </summary>
    Property ParentAppForm As frmApplication
End Interface
