Public Interface IDataGatewaysOutputOptions

    Event OptionsValidChanged(isCorrect As Boolean)
    Function AreOptionsValid() As Boolean
    Sub Closing()

End Interface