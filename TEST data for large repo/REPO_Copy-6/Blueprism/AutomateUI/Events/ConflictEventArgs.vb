Imports BluePrism.AutomateAppCore

Public Class ConflictEventArgs
    Private mSource As Object
    Private mDefn As ConflictDefinition
    Private mOption As ConflictOption
    Public Sub New(ByVal src As Object)
        Me.New(src, Nothing, Nothing)
    End Sub
    Public Sub New(ByVal src As Object, ByVal defn As ConflictDefinition)
        Me.New(src, defn, Nothing)
    End Sub
    Public Sub New(ByVal src As Object, ByVal defn As ConflictDefinition, ByVal opt As ConflictOption)
        mSource = src
        mDefn = defn
        mOption = opt
    End Sub

    Public ReadOnly Property Source() As Object
        Get
            Return mSource
        End Get
    End Property
    Public ReadOnly Property Definition() As ConflictDefinition
        Get
            Return mDefn
        End Get
    End Property
    Public ReadOnly Property ConflictOption() As ConflictOption
        Get
            Return mOption
        End Get
    End Property

End Class