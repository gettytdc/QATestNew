Imports System.Drawing

Public Class FontCache
    Implements IDisposable

    Private Class FontDefinition
        Public ReadOnly Property Name As String
        Public ReadOnly Property Size As Single
        Public ReadOnly Property Style As FontStyle
        Public ReadOnly Property GraphicsUnit As GraphicsUnit

        Public Sub New(ByVal name As String, ByVal size As Single, ByVal style As FontStyle, graphicsUnit As GraphicsUnit)
            Me.Name = name
            Me.Size = size
            Me.Style = style
            Me.GraphicsUnit = graphicsUnit
        End Sub

        Public Overrides Function Equals(ByVal o As Object) As Boolean
            If Not (TypeOf o Is FontDefinition) Then Return False
            Dim fd = CType(o, FontDefinition)
            Return (Name = fd.Name AndAlso Size = fd.Size AndAlso Style = fd.Style AndAlso GraphicsUnit = fd.GraphicsUnit)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return If(Name, String.Empty).Length << 24 Xor (CInt(Style) << 16) Xor Size.GetHashCode() Xor GraphicsUnit.GetHashCode()
        End Function

        Public Overrides Function ToString() As String
            Return "FontDefinition:{" & Name & ":" & Size & "-" & Style & "}"
        End Function
    End Class

    Private mFontMap As IDictionary(Of FontDefinition, Font)

    Public Sub New()
        mFontMap = New Dictionary(Of FontDefinition, Font)()
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

    Public Function GetFont(ByVal name As String, ByVal em As Single, ByVal style As FontStyle, unit As GraphicsUnit) As Font
        Dim defn = New FontDefinition(name, em, style, unit)
        Dim f As Font = Nothing

        If Not mFontMap.TryGetValue(defn, f) Then
            f = New Font(name, em, style, unit)
            mFontMap(defn) = f
        End If

        Return f
    End Function

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
    Public Overloads Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            For Each f As Font In mFontMap.Values
                If f IsNot Nothing Then f.Dispose()
            Next

            mFontMap.Clear()
        End If
    End Sub


End Class
