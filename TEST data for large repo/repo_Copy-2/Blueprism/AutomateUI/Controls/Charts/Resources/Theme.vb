Imports System.Windows
Imports System.Windows.Media
Imports WindowsSystemParams = System.Windows.SystemParameters

Namespace Controls.Charts.Resources
    Public NotInheritable Class Theme
        <ThreadStatic>
        Private Shared resourceDictionary As ResourceDictionary

        Friend Shared ReadOnly Property ResourceDictionarys As ResourceDictionary
            Get

                If resourceDictionary IsNot Nothing Then
                    Return resourceDictionary
                End If

                resourceDictionary = New ResourceDictionary()

                Dim isHighContrast = WindowsSystemParams.HighContrast
                Dim contrastTheme = If(isHighContrast, ThemeType.HighContrast， ThemeType.Light)
                LoadThemeType(contrastTheme)

                Return resourceDictionary
            End Get
        End Property

        Public Shared Property ThemeType As ThemeType = ThemeType.Light
        Public Shared Sub LoadThemeType(ByVal type As ThemeType)
            ThemeType = type
            Select Case type
                Case ThemeType.Light
                    SetResource(ThemeResourceKey.ContentBackground.ToString(), New SolidColorBrush(ColorFromHex("#FFFFFFFF")))
                    SetResource(ThemeResourceKey.ContentForeground.ToString(), New SolidColorBrush(ColorFromHex("#FF3F3F3F")))
                    SetResource(ThemeResourceKey.ContentLabel.ToString(), New SolidColorBrush(ColorFromHex("#FF434A4F")))
                    SetResource(ThemeResourceKey.TooltipBackground.ToString(), New SolidColorBrush(ColorFromHex("#FFFFFFFF")))
                    SetResource(ThemeResourceKey.TooltipBorder.ToString(), New SolidColorBrush(ColorFromHex("#FFD4D4D4")))
                    SetResource(ThemeResourceKey.ChartFlatBlue.ToString(), New SolidColorBrush(ColorFromHex("#005186")))
                    SetResource(ThemeResourceKey.SeriesColors.ToString(), New LiveCharts.Wpf.ColorsCollection() From {
                        ChartColour.Lavender_light,
                        ChartColour.Petunia_light,
                        ChartColour.Navy_light,
                        ChartColour.Poppy_light,
                        ChartColour.Larkspur_light,
                        ChartColour.Daffodil_light
                    })
                    Exit Select
                Case ThemeType.HighContrast
                    SetResource(ThemeResourceKey.ContentBackground.ToString(), SystemColors.ControlBrush)
                    SetResource(ThemeResourceKey.ContentForeground.ToString(), SystemColors.ControlTextBrush)
                    SetResource(ThemeResourceKey.ContentLabel.ToString(), SystemColors.ControlTextBrush)
                    SetResource(ThemeResourceKey.ChartFlatBlue.ToString(), New SolidColorBrush(ColorFromHex("#005186")))
                    SetResource(ThemeResourceKey.TooltipBackground.ToString(), New SolidColorBrush(ColorFromHex("#FFFFFFFF")))
                    SetResource(ThemeResourceKey.TooltipBorder.ToString(), New SolidColorBrush(ColorFromHex("#FFD4D4D4")))
                    SetResource(ThemeResourceKey.SeriesColors.ToString(), New LiveCharts.Wpf.ColorsCollection() From {
                        ChartColour.Lavender_dark,
                        ChartColour.Petunia_dark,
                        ChartColour.Navy_dark,
                        ChartColour.Poppy_dark,
                        ChartColour.Larkspur_dark,
                        ChartColour.Daffodil_dark
                    })
                    Exit Select
            End Select
        End Sub

        Public Shared Function GetResource(ByVal resourceKey As ThemeResourceKey) As Object
            Return If(resourceDictionary.Contains(resourceKey.ToString()), resourceDictionary(resourceKey.ToString()), Nothing)
        End Function

        Friend Shared Sub SetResource(ByVal key As Object, ByVal resource As Object)
            resourceDictionary(key) = resource
        End Sub

        Friend Shared Function ColorFromHex(ByVal colorHex As String) As Color
            Return If(CType(ColorConverter.ConvertFromString(colorHex), Color?), Colors.Transparent)
        End Function

    End Class
End Namespace
