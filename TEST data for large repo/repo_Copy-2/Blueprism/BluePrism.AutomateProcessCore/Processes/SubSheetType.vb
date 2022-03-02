Namespace Processes

    ''' <summary>
    ''' The different types of subsheet are Capability and CleanUp. Process studio 
    ''' Processes have subsheets of type normal, however note that they also have a
    ''' Main page for which there is no subsheet. Object studio documents have 
    ''' sheets of type capability or Cleanup, however not that there is also an 
    ''' Init page which is the the same as the main page, in that it also has no
    ''' subsheet.
    ''' </summary>
    Public Enum SubsheetType
        Normal
        Capability
        CleanUp
        MainPage
    End Enum

End Namespace