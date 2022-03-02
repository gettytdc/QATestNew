
''' <summary>
''' Enumeration which models the state of a data traversal operation
''' </summary>
Public Enum DataTraversalState

    ''' <summary>
    ''' The traversal is before the start of the data
    ''' </summary>
    BeforeStart

    ''' <summary>
    ''' The traversal is somewhere within the data
    ''' </summary>
    InData

    ''' <summary>
    ''' The traversal is after the end of the data
    ''' </summary>
    AfterEnd

End Enum
