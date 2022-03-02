
''' <summary>
''' <para>The mapping type of a parameter.</para>
''' <para>
''' Depending on the type of parameter, only certain map types are valid. The
''' type "None" is always allowed. In addition:
''' </para>
''' <para>
''' The type "Expr" is allowed for the following:
''' <list>
'''   <item>An Action input</item>
'''   <item>A Process (call) input</item>
'''   <item>A SubSheet (call) input</item>
''' </list>
''' </para>
''' <para>
''' The type "Stage" is allowed for the following:
''' <list>
'''   <item>An Action output</item>
'''   <item>A Process (call) output</item>
'''   <item>A SubSheet (call) output</item>
'''   <item>A Start Stage (only has inputs)</item>
'''   <item>An End Stage (only has outputs)</item>
''' </list>
''' As well as being the name of a stage, it can specify a collection field, in the
''' form {Stage}.{FieldName} which refers to the current row of the target collection
''' </para>
''' </summary>
Public Enum MapType

    ''' <summary>
    ''' No map type defined
    ''' </summary>
    None = 0

    ''' <summary>
    ''' The mapping is to a stage - ie. the map value refers to a stage.
    ''' </summary>
    Stage = 1

    ''' <summary>
    ''' The mapping is to an expression - ie. the map value represents an expression
    ''' </summary>
    Expr = 2

    ''' <summary>
    ''' No longer in use - has no effect within a process.
    ''' </summary>
    <Obsolete("No longer has any effect - only exists to support existing XML")> _
    [Const] = 3

End Enum
