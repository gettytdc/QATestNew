Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns


''' <summary>
''' Thrown when an expected UI Automation pattern cannot be found.
''' </summary>
''' <seealso cref="MissingPatternException" />
<Serializable>
Public Class PatternNotFoundException(Of TPattern As IAutomationPattern)
    Inherits MissingPatternException

    ''' <summary>
    ''' Gets the pattern type for the automation pattern represented by this class.
    ''' </summary>
    ''' <returns>The pattern type corresponding to automation pattern represented by
    ''' this exception</returns>
    Private Shared Function GetPatternType() As PatternType
        Return RepresentsPatternTypeAttribute.GetPatternType(GetType(TPattern))
    End Function

    ''' <summary>
    ''' Initializes a new instance of the
    ''' <see cref="PatternNotFoundException(Of TPattern)"/> class.
    ''' </summary>
    Public Sub New()
        MyBase.New(GetPatternType())
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="PatternNotFoundException(Of TPattern)"/> class.
    ''' </summary>
    ''' <param name="messageFormat">Message format string used to format the message. The
    ''' name of the type specified by TPattern will be inserted into the &quot;{0}&quot;
    ''' placeholder.</param>
    Public Sub New(messageFormat As String)
        MyBase.New(messageFormat, GetPatternType())
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the
    ''' <see cref="PatternNotFoundException(Of TPattern)"/> class.
    ''' </summary>
    ''' <param name="innerException">The inner exception.</param>
    Public Sub New(innerException As Exception)
        MyBase.New(innerException, GetPatternType())
    End Sub

End Class
