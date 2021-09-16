Imports System.Windows

Namespace Controls.Charts.Resources
    Public NotInheritable Class ThemeResourceDictionary
        Inherits ResourceDictionary

        Public Sub New()
            MergedDictionaries.Add(Theme.ResourceDictionarys)
        End Sub
    End Class
End Namespace
