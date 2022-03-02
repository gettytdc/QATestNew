Imports System.Globalization
Imports System.IO

Public Module HelpFileFinder

    ''' <summary>
    ''' Provides sequence of possible help file paths based on list of cultures. This can be used to find the most
    ''' specific help file that exists on the file system.
    ''' </summary>
    ''' <param name="cultures">The cultures in which help file should be displayed (more specific before less specific)</param>
    ''' <param name="appDirectory">The directory where the application is running</param>
    ''' <returns>Sequence of possible help file paths</returns>
    Public Function GetPossiblePaths(cultures As CultureInfo(), appDirectory As String) As IEnumerable(Of String)

        Const filename As String = "AutomateHelp.chm"
        Const fileNameRoot As String = "AutomateHelp"
        Const fileNameEnding As String = ".chm"

        Dim developmentPaths = From culture In cultures
                               Select Path.Combine(appDirectory, $"..\BluePrism.Automate\Help\l10n\{culture.Name}\{filename}")
        Dim installPaths = From culture In cultures
                           Select Path.Combine(appDirectory, $"{fileNameRoot}_{culture.Name}{fileNameEnding}")
        Dim defaultPaths = {
                               Path.Combine(appDirectory, "..\BluePrism.Automate\Help\" & filename),
                               Path.Combine(appDirectory, filename)
                           }

        Return developmentPaths.
            Concat(installPaths).
            Concat(defaultPaths)

    End Function

    Public Iterator Function GetPossibleCultures(current As CultureInfo) As IEnumerable(Of CultureInfo)
        Dim culture = current
        While Not culture.Equals(CultureInfo.InvariantCulture)
            Yield culture
            culture = culture.Parent
        End While
    End Function

End Module