Imports System.Globalization
Imports System.Windows.Data

Public Class ActionViewConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim viewModels = CType(value, List(Of SkillActionViewModel))

        Return viewModels.Select(Function(x) New SkillsToolbarAction() With {.DataContext = x}).ToList()
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException
    End Function
End Class
