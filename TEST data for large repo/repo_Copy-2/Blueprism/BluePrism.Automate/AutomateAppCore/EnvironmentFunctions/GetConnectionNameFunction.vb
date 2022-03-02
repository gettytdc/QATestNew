Imports BluePrism.AutomateAppCore.Config
Imports BluePrism.AutomateProcessCore

Namespace EnvironmentFunctions

    ''' <summary>
    ''' Function to get the name of the current connection to the database.
    ''' </summary>
    Public Class GetConnectionNameFunction : Inherits EnvironmentFunction
        
        Private ReadOnly mOptionsFactory As Func(Of IOptions)

        Public Sub New(optionsFactory As Func(Of IOptions))
            mOptionsFactory = optionsFactory
        End Sub

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.GetConnectionNameFunction_GetsTheNameOfTheCurrentBluePrismDatabaseConnection
            End Get
        End Property

        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue
            Return mOptionsFactory().CurrentConnectionName
        End Function

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetConnection"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.GetConnectionNameFunction_GetNameOfConnection
            End Get
        End Property
    End Class
End NameSpace