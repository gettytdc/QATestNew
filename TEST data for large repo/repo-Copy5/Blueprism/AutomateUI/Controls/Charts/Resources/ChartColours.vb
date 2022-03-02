Imports System.Windows.Media

Public NotInheritable Class ChartColour
    Private Shared Function ColorFromHex(ByVal colorHex As String) As Color
        Return If(CType(ColorConverter.ConvertFromString(colorHex), Color?), Colors.Transparent)
    End Function

    Public Shared ReadOnly Lavender_light As Color = ColorFromHex("#FF575FCD")
    Public Shared ReadOnly Petunia_light  As Color = ColorFromHex("#FFCE3779")
    Public Shared ReadOnly Navy_light     As Color = ColorFromHex("#FF005285")
    Public Shared ReadOnly Poppy_light    As Color = ColorFromHex("#FF5D0029")
    Public Shared ReadOnly Larkspur_light As Color = ColorFromHex("#FF002489")
    Public Shared ReadOnly Daffodil_light As Color = ColorFromHex("#FF8C9500")

    Public Shared ReadOnly Lavender_dark  As Color = ColorFromHex("#FF9DA1E2")
    Public Shared ReadOnly Petunia_dark   As Color = ColorFromHex("#FFEA96C0")
    Public Shared ReadOnly Navy_dark      As Color = ColorFromHex("#FF81D0EC")
    Public Shared ReadOnly Poppy_dark     As Color = ColorFromHex("#FFFFA38C")
    Public Shared ReadOnly Larkspur_dark  As Color = ColorFromHex("#FF5574FF")
    Public Shared ReadOnly Daffodil_dark  As Color = ColorFromHex("#FFF9FA93")
End Class
