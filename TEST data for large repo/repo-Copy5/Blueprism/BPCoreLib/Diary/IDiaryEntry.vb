
Namespace Diary

    ''' <summary>
    ''' Interface representing an entry in a diary.
    ''' Primarily used for interacting with the AutomateControls Diary controls.
    ''' </summary>
    Public Interface IDiaryEntry

        ''' <summary>
        ''' The date/time at which the entry is set for.
        ''' </summary>
        ReadOnly Property Time() As Date

        ''' <summary>
        ''' The title of the entry.
        ''' </summary>
        ReadOnly Property Title() As String

    End Interface

End Namespace
