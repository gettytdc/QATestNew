Imports System.Globalization
Imports System.IO
Imports System.Windows.Data
Imports System.Windows.Media.Imaging

Public Class ImageToSourceConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim imageToConvert = CType(value, Image)
        If imageToConvert Is Nothing Then Return Nothing

        Using stream = New MemoryStream()
            Dim bitmapImage = New BitmapImage()

            imageToConvert.Save(stream, imageToConvert.RawFormat)
            stream.Seek(0, SeekOrigin.Begin)
            bitmapImage.BeginInit()
            BitmapImage.StreamSource = stream
            BitmapImage.CacheOption = BitmapCacheOption.OnLoad
            BitmapImage.EndInit()
            BitmapImage.Freeze()

            Return BitmapImage
        End Using
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
