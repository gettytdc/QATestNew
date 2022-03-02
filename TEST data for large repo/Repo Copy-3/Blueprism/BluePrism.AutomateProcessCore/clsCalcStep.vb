Imports System.Runtime.Serialization
Imports BluePrism.Core.Expressions

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCalcStep
''' 
''' <summary>
''' This class represents a calculation step within a multiple calculation stage.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsCalcStep : Implements IExpressionHolder

    ''' <summary>
    ''' Creates a new empty calculation step within the given process stage.
    ''' </summary>
    ''' <param name="parent">The process stage that this calc step is part of
    ''' </param>
    Public Sub New(ByVal parent As clsProcessStage)
        Me.New(parent, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new calculation step with the given process stage as parent, and
    ''' with its expression and store in values copied from the given calc step.
    ''' </summary>
    ''' <param name="parent">The multiple calculation stage which this step forms a
    ''' part of.</param>
    ''' <param name="copyFrom">The calc step from which to copy the expression and
    ''' store in settings.</param>
    Public Sub New(ByVal parent As clsProcessStage, ByVal copyFrom As clsCalcStep)
        Me.New(parent, copyFrom.Expression, copyFrom.StoreIn)
    End Sub

    ''' <summary>
    ''' Creates a new calculation step with the given process stage as parent, and
    ''' the given expression and store in values.
    ''' </summary>
    ''' <param name="parent">The multiple calculation stage which this step forms a
    ''' part of.</param>
    ''' <param name="expr">The expression that forms this step.</param>
    ''' <param name="store">The stage in which to store this calculation in, in
    ''' text form.</param>
    Public Sub New(ByVal parent As clsProcessStage, _
     ByVal expr As BPExpression, ByVal store As String)
        mParent = parent
        mStoreIn = store
        Expression = expr
    End Sub

    ''' <summary>
    ''' The parent stage of the calculation step.
    ''' </summary>
    Public ReadOnly Property Parent() As clsProcessStage
        Get
            Return mParent
        End Get
    End Property
    <NonSerialized>
    Private mParent As clsProcessStage

    ''' <summary>
    ''' The expression to evaluate. This will never be null
    ''' </summary>
    Public Property Expression() As BPExpression _
     Implements IExpressionHolder.Expression
        Get
            Return mExpression
        End Get
        Set(ByVal value As BPExpression)
            mExpression = If(value, BPExpression.Empty)
        End Set
    End Property
    <DataMember>
    Private mExpression As BPExpression = BPExpression.Empty

    ''' <summary>
    ''' The data item name to store the result of the calculation in.
    ''' </summary>
    Public Property StoreIn() As String
        Get
            Return mStoreIn
        End Get
        Set(ByVal value As String)
            mStoreIn = value
        End Set
    End Property
    <DataMember>
    Private mStoreIn As String

End Class
