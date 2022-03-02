''' Project   : Automate
''' Interface : IRefreshable
''' 
''' <summary>
''' This interface is used to define the refreshview function that is called on all
''' child controls so that they can refresh the view from the data model.
''' </summary>
Public Interface IRefreshable
    Sub RefreshView()
End Interface
