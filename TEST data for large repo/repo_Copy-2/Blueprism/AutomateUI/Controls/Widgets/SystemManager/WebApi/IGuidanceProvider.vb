Namespace Controls.Widgets.SystemManager.WebApi

    ''' <summary>
    ''' Interface which describes a class which can provide instruction in the form of a
    ''' string.
    ''' </summary>
    Friend Interface IGuidanceProvider

        ''' <summary>
        ''' The text which will help to contextualise this class. Typically, some form of
        ''' guidance on how to complete the form that this is part of
        ''' </summary>
        ReadOnly Property GuidanceText As String

    End Interface

End Namespace
