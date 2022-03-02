Option Strict On
Imports AutomateUI


Module main

    <STAThread()> Public Function main(ByVal args() As String) As Integer
        Return BPApplication.Start(args)
    End Function

End Module
