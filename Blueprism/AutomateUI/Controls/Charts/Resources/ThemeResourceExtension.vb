Imports System.Windows

Namespace Controls.Charts.Resources
    Public NotInheritable Class ThemeResourceExtension
        Inherits DynamicResourceExtension

        Public Overloads Property ResourceKey As ThemeResourceKey
            Get
                Dim resourceKeyed As ThemeResourceKey
                [Enum].TryParse(MyBase.ResourceKey.ToString(), resourceKeyed)
                Return resourceKeyed
            End Get
            Set(ByVal value As ThemeResourceKey)
                MyBase.ResourceKey = value.ToString()
            End Set
        End Property
    End Class
End Namespace
