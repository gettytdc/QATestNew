
''' <summary>
''' The types of link available between stages.
''' </summary>
Public Enum LinkType
    ''' <summary>
    ''' The unique outbound link from ordinary stages,
    ''' this link is to be followed when a stage completes
    ''' execution successfully.
    ''' </summary>
    OnSuccess
    ''' <summary>
    ''' The unique link to be followed from a decision stage
    ''' when the condition evaluates to true.
    ''' </summary>
    OnTrue
    ''' <summary>
    ''' The unique link to be followed from a decision stage
    ''' when the condition evaluates to false.
    ''' </summary>
    OnFalse
End Enum
