Imports BluePrism.AMI
Imports System.Collections.Generic

Namespace BluePrism.ApplicationManager.AMI
    ''' <summary>
    ''' Class used to encapsulate all the data about a particular wait condition, for
    ''' passing it to clsAMI.DoWait().
    ''' </summary>
    Public Class clsWaitInfo

        ''' <summary>
        ''' Constructor. Always use it!
        ''' </summary>
        ''' <param name="elementType">The target element type.</param>
        ''' <param name="idents">The identifiers.</param>
        ''' <param name="args">The arguments.</param>
        ''' <param name="condition">The condition type to use.</param>
        ''' <param name="expectedReply">The expected reply.</param>
        ''' <param name="ComparisonType">The comparison type to use when matching the
        ''' response against the expected reply.</param>
        Public Sub New(ByVal elementType As clsElementTypeInfo, ByVal idents As List(Of clsIdentifierInfo), ByVal args As Dictionary(Of String, String), ByVal condition As clsConditionTypeInfo, ByVal expectedReply As String, ByVal ComparisonType As clsAMI.ComparisonTypes)
            mElementType = elementType
            mIdentifiers = idents
            mArguments = args
            mCondition = condition
            mExpectedReply = expectedReply
            mComparisonType = ComparisonType
        End Sub


        ''' <summary>
        ''' The target element type.
        ''' </summary>
        Public ReadOnly Property ElementType() As clsElementTypeInfo
            Get
                Return mElementType
            End Get
        End Property
        Private mElementType As clsElementTypeInfo

        ''' <summary>
        ''' The identifiers.
        ''' </summary>
        Public ReadOnly Property Identifiers() As List(Of clsIdentifierInfo)
            Get
                Return mIdentifiers
            End Get
        End Property
        Private mIdentifiers As List(Of clsIdentifierInfo)

        ''' <summary>
        ''' The arguments.
        ''' </summary>
        Public ReadOnly Property Arguments() As Dictionary(Of String, String)
            Get
                Return mArguments
            End Get
        End Property
        Private mArguments As Dictionary(Of String, String)


        ''' <summary>
        ''' The condition to use.
        ''' </summary>
        Public ReadOnly Property Condition() As clsConditionTypeInfo
            Get
                Return mCondition
            End Get
        End Property
        Private mCondition As clsConditionTypeInfo


        ''' <summary>
        ''' The expected reply from the condition check.
        ''' </summary>
        Public ReadOnly Property ExpectedReply() As String
            Get
                Return mExpectedReply
            End Get
        End Property
        Private mExpectedReply As String


        ''' <summary>
        ''' The comparison type to use when comparing the expected reply to the value
        ''' observed in the target application.
        ''' </summary>
        Public ReadOnly Property ComparisonType() As clsAMI.ComparisonTypes
            Get
                Return mComparisonType
            End Get
        End Property
        Private mComparisonType As clsAMI.ComparisonTypes

    End Class
End Namespace
