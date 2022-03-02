Imports System.Globalization
Imports System.Windows.Data

Public Class SkillBarWidthConverter : Implements IValueConverter

    Private Const TextBoxBorderValue As Integer = 14

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        
        Dim width = 0
        If Integer.TryParse(CType(value, String), width) Then
            width -= TextBoxBorderValue
        End If
        Return If(width>0, width.ToString, value)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException
    End Function
End Class
