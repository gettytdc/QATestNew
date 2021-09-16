Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

Namespace Operations

    Public Module KeyHelper
    
        ''' <summary>
        ''' Invokes the IKeyOperationsProvider.SendKeys method using arguments 
        ''' specified by the <see cref="clsQuery.ParameterNames.NewText">newtext</see> and 
        ''' <see cref="clsQuery.ParameterNames.Interval">interval</see> query parameters.
        ''' </summary>
        ''' <param name="query">The query containing the arguments determining how to send
        ''' the keys to the foreground application.</param>
        ''' <param name="keyboardOperationsProvider">The IKeyOperationsProvider instance</param>
        Public Sub SendKeysFromQuery(query As clsQuery, keyboardOperationsProvider As IKeyboardOperationsProvider)

            Dim text = query.GetParameter(clsQuery.ParameterNames.NewText)
            Dim interval = TimeSpan.FromSeconds(query.GetDecimalParam(clsQuery.ParameterNames.Interval))
            keyboardOperationsProvider.SendKeys(text, interval)

        End Sub

    End Module

End NameSpace