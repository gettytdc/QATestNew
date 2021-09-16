Namespace Compilation
    ''' <summary>
    ''' Contains details about a language available for dynamically compiled code used
    ''' by Blue Prism components
    ''' </summary>
    Public Class CodeLanguage

#Region "Shared access members"

        ''' <summary>
        ''' The Visual Basic language definition
        ''' </summary>
        Public Shared ReadOnly VisualBasic As New CodeLanguage("visualbasic", "Visual Basic")

        ''' <summary>
        ''' The C# language definition
        ''' </summary>
        Public Shared ReadOnly CSharp As New CodeLanguage("csharp", "C#")

        ''' <summary>
        ''' The Visual J# language definition
        ''' </summary>
        Public Shared ReadOnly JSharp As New CodeLanguage("vjsharp", "Visual J#")

        Private Shared ReadOnly LanguagesByName As _
            New Dictionary(Of String, CodeLanguage)(StringComparer.OrdinalIgnoreCase) _
            From 
            {
                {VisualBasic.Name, VisualBasic},
                {CSharp.Name, CSharp},
                {JSharp.Name, JSharp}
            }

        ''' <summary>
        ''' Gets the <see cref="CodeLanguage"/> with the specified name
        ''' </summary>
        ''' <param name="name">The name of the language</param>
        ''' <returns>The <see cref="CodeLanguage"/> with the specified name</returns>
        ''' <exception cref="ArgumentException">Exception thrown if an unrecognised
        ''' language name is given</exception>
        Public Shared Function GetByName(name As String) As CodeLanguage
            Dim language As CodeLanguage = Nothing
            If Not LanguagesByName.TryGetValue(name, language) Then
                Throw New ArgumentException(My.Resources.Resources.CodeLanguage_UnrecognisedLanguageName, NameOf(name))
            End If
            Return language
        End Function

#End Region
        
        Private Sub New(name As String, friendlyName As String)
            Me.Name = name
            Me.FriendlyName = friendlyName
        End Sub

        ''' <summary>
        ''' A unique name used to identify the language. This matches the language 
        ''' values recognised by types in System.CodeDom 
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' A title used for the language
        ''' </summary>
        Public ReadOnly Property FriendlyName As String

        Protected Overloads Function Equals(other As CodeLanguage) As Boolean
            Return String.Equals(Name, other.Name) AndAlso String.Equals(FriendlyName, other.FriendlyName)
        End Function

        ''' <inheritdoc />
        Public Overloads Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True
            If obj.GetType IsNot Me.GetType Then Return False
            Return Equals(DirectCast(obj, CodeLanguage))
        End Function

        ''' <inheritdoc />
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode As Integer = Name.GetHashCode
            hashCode = CInt((hashCode*397L) Mod Integer.MaxValue) Xor FriendlyName.GetHashCode
            Return hashCode
        End Function

        ''' <summary>
        ''' Determines whether the specified objects are considered equal
        ''' </summary>
        ''' <param name="left">The first object to compare</param>
        ''' <param name="right">The second object to compare</param>
        ''' <returns>The result of the equality comparison</returns>
        Public Shared Operator =(left as CodeLanguage, right as CodeLanguage) as Boolean
            Return Equals(left, right)
        End Operator

        ''' <summary>
        ''' Determines whether the specified objects are not considered equal
        ''' </summary>
        ''' <param name="left">The first object to compare</param>
        ''' <param name="right">The second object to compare</param>
        ''' <returns>The result of the equality comparison</returns>
        Public Shared Operator <>(left as CodeLanguage, right as CodeLanguage) as Boolean
            Return Not Equals(left, right)
        End Operator
    End Class
End Namespace