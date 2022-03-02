Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class which describes a conflict - its explanatory text, and the options
''' available to the user to resolve the conflict.
''' </summary>
Public Class ConflictDefinition

#Region " Member Variables "

    ' The unique ID of this definition
    Private mId As String

    ' The text describing this conflict

    ' The hint for resolution of this conflict

    ' The options available to resolve the conflict.
    Private mOptions As IDictionary(Of ConflictOption.UserChoice, ConflictOption)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new conflict definition with the given properties
    ''' </summary>
    ''' <param name="id">The ID of the conflict definition</param>
    ''' <param name="txt">The text, which explains, to the user, the definition of
    ''' the conflict.</param>
    ''' <param name="hint">The text explaining hinting what is required to resolve
    ''' this conflict</param>
    Public Sub New(id As String, txt As String, hint As String)
        Me.New(id, txt, hint, New ConflictOption() {})
    End Sub

    ''' <summary>
    ''' Creates a new conflict definition with the given properties
    ''' </summary>
    ''' <param name="id">The ID of the conflict definition</param>
    ''' <param name="txt">The text, which explains, to the user, the definition of
    ''' the conflict.</param>
    ''' <param name="hint">The text explaining hinting what is required to resolve
    ''' this conflict</param>
    ''' <param name="options">The options available to the user to resolve conflicts
    ''' of this definition.</param>
    Public Sub New(id As String, txt As String, hint As String, ParamArray options() As ConflictOption)
        Me.New(id, txt, hint, DirectCast(options, ICollection(Of ConflictOption)))
    End Sub

    ''' <summary>
    ''' Creates a new conflict definition with the given properties
    ''' </summary>
    ''' <param name="id">The ID of the conflict definition</param>
    ''' <param name="txt">The text, which explains, to the user, the definition of
    ''' the conflict.</param>
    ''' <param name="hint">The text explaining hinting what is required to resolve
    ''' this conflict</param>
    ''' <param name="options">The options available to the user to resolve conflicts
    ''' of this definition.</param>
    Public Sub New(id As String, txt As String, hint As String, options As ICollection(Of ConflictOption))
        mId = id
        Text = txt
        Me.Hint = hint
        mOptions = New clsOrderedDictionary(Of ConflictOption.UserChoice, ConflictOption)
        ' Add all the options into the map keyed on ID
        If Not CollectionUtil.IsNullOrEmpty(options) Then
            For Each opt As ConflictOption In options
                mOptions(opt.Choice) = opt
            Next
        End If
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The unique ID of this conflict definition.
    ''' </summary>
    Public ReadOnly Property Id() As String
        Get
            Return mId
        End Get
    End Property

    ''' <summary>
    ''' The text explaining the type of conflict and its cause
    ''' </summary>
    Public Property Text As String

    ''' <summary>
    ''' The text explaining how the user can resolve this type of conflict
    ''' </summary>
    Public Property Hint As String

    ''' <summary>
    ''' The options available to the user for resolving this conflict
    ''' </summary>
    Public Property Options() As ICollection(Of ConflictOption)
        Get
            Return mOptions.Values
        End Get
        Set(value As ICollection(Of ConflictOption))
            For Each v In value
                mOptions.Where(Function(x) x.Value.Choice = v.Choice).FirstOrDefault.Value.Text = v.Text
            Next
        End Set
    End Property

    ''' <summary>
    ''' Gets the conflict option with the given ID, or null if no such conflict
    ''' option exists within this definition.
    ''' </summary>
    ''' <param name="res">The resolution choice for the required conflict option.
    ''' </param>
    ''' <returns>The conflict option which is mapped to the given resolution or
    ''' null if no such conflict option occurs in this definition</returns>
    Default Public ReadOnly Property Item(ByVal res As ConflictOption.UserChoice) As ConflictOption
        Get
            Dim opt As ConflictOption = Nothing
            mOptions.TryGetValue(res, opt)
            Return opt
        End Get
    End Property

    ''' <summary>
    ''' Holds the default (recommended) resolution to this conflict when the release
    ''' is being imported interactively with the Blue Prism client.
    ''' </summary>
    Public Property DefaultInteractiveResolution() As ConflictOption.UserChoice

    ''' <summary>
    ''' Holds the resolution to this conflict that will be automatically applied
    ''' when the release is imported non-interactively via the AutomateC command
    ''' </summary>
    Public Property DefaultNonInteractiveResolution() As ConflictOption.UserChoice

#End Region

#Region " Methods "

    Public Sub RemoveOption(type As ConflictOption.UserChoice)
        If mOptions.ContainsKey(type) Then mOptions.Remove(type)
    End Sub

    Public Sub UpdateConflictDefinitionStrings(newConflict As ConflictDefinition)
        If newConflict IsNot Nothing Then
            Hint = newConflict.Hint
            Text = newConflict.Text
            Options = newConflict.Options
        End If
    End Sub
#End Region

End Class
