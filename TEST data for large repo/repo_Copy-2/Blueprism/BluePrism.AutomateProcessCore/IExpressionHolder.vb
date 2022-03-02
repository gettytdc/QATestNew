Imports BluePrism.Core.Expressions

''' <summary>
''' Interface which defines an object capable of holding an expression
''' </summary>
Public Interface IExpressionHolder

    ''' <summary>
    ''' Gets or sets the expression held within this object.
    ''' Implementors of this interface should guarantee that the expression returned
    ''' is never null
    ''' </summary>
    Property Expression() As BPExpression

End Interface
