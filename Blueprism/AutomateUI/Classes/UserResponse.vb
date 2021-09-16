
''' <summary>
''' Response from the user after a user message.
''' Currently only used in a small subset of the UserMessage methods, but far easier
''' to comprehend without searching the code than an integer.
''' </summary>
Public Enum UserResponse As Integer

    ''' <summary>
    ''' No firm response given - eg. the close button was used.
    ''' </summary>
    None = 0

    ''' <summary>
    ''' The user pressed the OK button
    ''' </summary>
    Ok = 1

    ''' <summary>
    ''' The user pressed the Cancel button
    ''' </summary>
    Cancel = 2

    ''' <summary>
    ''' The user pressed the Yes button
    ''' </summary>
    Yes = 6

    ''' <summary>
    ''' The user pressed the No button
    ''' </summary>
    No = 7

    ''' <summary>
    ''' The user pressed the Yes button and ticked the checkbox
    ''' </summary>
    YesChecked = 11

    ''' <summary>
    ''' The user pressed the No buton and ticked the checkbox
    ''' </summary>
    NoChecked = 12

End Enum
