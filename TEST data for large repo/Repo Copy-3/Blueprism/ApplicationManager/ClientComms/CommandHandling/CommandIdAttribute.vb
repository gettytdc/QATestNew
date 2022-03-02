Namespace CommandHandling

    ''' <summary>
    ''' Defines the id value used to identify a command handler
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:= False, Inherited:= False)>
    Friend Class CommandIdAttribute : Inherits Attribute

        ''' <summary>
        ''' Creates a new instance of CommandIdAttribute
        ''' </summary>
        ''' <param name="id">The unique identifier for this command</param>
        Public Sub New(id As String)
            Me.Id = id
        End Sub

        ''' <summary>
        ''' The unique identifier for this command
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Id As String

    End Class

End NameSpace