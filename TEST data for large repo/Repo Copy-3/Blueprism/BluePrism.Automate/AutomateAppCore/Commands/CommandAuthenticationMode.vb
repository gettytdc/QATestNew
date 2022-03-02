Namespace Commands
    ''' <summary>
    ''' An enumeration of the valid authentication requirements of a command
    ''' </summary>
    Public Enum CommandAuthenticationMode
        ''' <summary>
        ''' Anyone can use it
        ''' </summary>
        Any

        ''' <summary>
        ''' Only authenticated users
        ''' </summary>
        Authed

        ''' <summary>
        ''' Only authenticated users, or connections from the local machine
        ''' </summary>
        AuthedOrLocal

    End Enum
End NameSpace