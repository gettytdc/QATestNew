namespace BluePrism.Core.Expressions
{
    /// <summary>
    /// The style to apply to individual els within a Blue Prism expression
    /// </summary>
    public enum ExpressionStyle : int
    {
        None = -1,
        Default = 0,
        Whitespace,
        Function,
        BooleanOperator,
        String,
        DataItem,
        Param,
        Literal,
        Operator,
        Error,
        OpenBracket,
        CloseBracket,
        ParamSeparator,
        // Following are standard styles as defined in ScintillaNET
        BraceLight = 34,
        BraceBad = 35
    }
}
