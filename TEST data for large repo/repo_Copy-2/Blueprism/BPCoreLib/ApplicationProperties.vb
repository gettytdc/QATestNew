Imports System.IO
Imports System.Reflection
Imports System.Threading

''' <summary>
''' Static class to hold a number of properties about the application
''' </summary>
Public Class ApplicationProperties

    ''' <summary>
    ''' The name of the application
    ''' </summary>
    Public Shared ApplicationName As String = My.Resources.ApplicationProperties_BluePrism

    ''' <summary>
    ''' The name of the company owning this software, as used in UI
    ''' messages offering tech support.
    ''' </summary>
    Public Shared CompanyName As String = My.Resources.ApplicationProperties_BluePrism

    ''' <summary>
    ''' The name of the form used in Object Studio used to model the target
    ''' application. Formerly known as "Integration Assistant"
    ''' </summary>
    Public Shared ApplicationModellerName As String = My.Resources.ApplicationProperties_ApplicationModeller

    ''' <summary>
    ''' The location (ie. directory) of the executing assembly
    ''' </summary>
    Public Shared ReadOnly ApplicationDirectory As String =
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

    ''' <summary>
    ''' Disable instantiation
    ''' </summary>
    Private Sub New()
        'ApplicationName = My.Resources.ApplicationProperties_BluePrism
    End Sub

    ''' <summary>
    ''' The path to the help file for this application
    ''' </summary>
    Public Shared ReadOnly Property HelpFilePath() As String
        Get
            Dim culture = Thread.CurrentThread.CurrentUICulture
            Dim cultures = HelpFileFinder.GetPossibleCultures(culture).ToArray()
            Dim paths = HelpFileFinder.GetPossiblePaths(cultures, ApplicationDirectory)
            Dim path = paths.First(Function(p) File.Exists(p))
            Return path
        End Get
    End Property

    ''' <summary>
    ''' The path to the 'automateconfig' executable.
    ''' </summary>
    Public Shared ReadOnly Property AutomateConfigPath() As String
        Get
            Return Path.Combine(ApplicationDirectory, "AutomateConfig.exe")
        End Get
    End Property

End Class
