Imports BluePrism.AutomateProcessCore

''' <summary>
''' Arguments that are associated with a ConflictDefinition.
''' </summary>
Public Class ConflictArgument

    ''' <summary>
    ''' Construct a new instance.
    ''' </summary>
    ''' <param name="name">The name of the argument</param>
    ''' <param name="value">The associated value</param>
    Public Sub New(name As String, value As clsProcessValue)
        Me.New(name, value, Nothing)
    End Sub

    ''' <summary>
    ''' Construct a new instance.
    ''' </summary>
    ''' <param name="name">The name of the argument</param>
    ''' <param name="value">The associated value</param>
    ''' <param name="customTitle">A custom title used to display the property in 
    ''' the user interface</param>
    Public Sub New(name As String, value As clsProcessValue, customTitle As String)

        Me.Name = name
        Me.CustomTitle = customTitle
        Me.Value = value

    End Sub

    ''' <summary>
    ''' The name of the argument
    ''' </summary>
    Public ReadOnly Property Name As String

    ''' <summary>
    ''' Gets a custom title used to display the property in the user interface
    ''' </summary>
    Public ReadOnly Property CustomTitle As String

    ''' <summary>
    ''' The value of the argument.
    ''' </summary>
    Public Property Value As clsProcessValue

    ''' <summary>
    ''' Gets the value of CustomTitle if defined or the value of Name if it is not 
    ''' </summary>
    Public ReadOnly Property CustomTitleOrDefault As String
        Get
            If String.IsNullOrEmpty(CustomTitle) Then
                Return Name
            End If

            Return CustomTitle
        End Get
    End Property

    ''' <summary>
    ''' Deep clones this argument object.
    ''' </summary>
    ''' <returns>A full clone of this argument</returns>
    Public Function Clone() As ConflictArgument
        Return New ConflictArgument(Name, Value, CustomTitle)
    End Function

End Class
