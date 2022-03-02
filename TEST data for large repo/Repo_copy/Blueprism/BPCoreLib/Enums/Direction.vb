''' <summary>
''' Enumeration of a direction - a generic 'top / right / bottom / left' enumeration.
''' It has the flags attribute indicating that these values can be combined together
''' in a single value using the OR operator
''' </summary>
<Flags()> _
Public Enum Direction
    None = 0
    Top = 1
    Right = 2
    Bottom = 4
    Left = 8
End Enum
