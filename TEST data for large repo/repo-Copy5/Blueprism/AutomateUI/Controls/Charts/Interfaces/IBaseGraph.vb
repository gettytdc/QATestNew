
Namespace Controls.Charts
    Public Interface IBaseGraph
        Property XAxis As String
        Property YAxis As String
        ReadOnly Property Labels As List(Of String)
        Property Formatter As Func(Of Double, String)

    End Interface


End Namespace
