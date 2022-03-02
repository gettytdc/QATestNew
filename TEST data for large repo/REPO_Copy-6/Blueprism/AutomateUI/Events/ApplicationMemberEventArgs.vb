Imports BluePrism.AutomateProcessCore

''' <summary>
''' Event arguments detailing an application member event.
''' </summary>
Public Class ApplicationMemberEventArgs

    ' The application member for which the event occurred
    Private mMember As clsApplicationMember

    ' Whether this event was as a result of a filter being applied or cleared
    Private mResultOfFilter As Boolean

    ''' <summary>
    ''' Creates a new event args object wrapping the given member, indicating that
    ''' this event is *not* a result of a filter being applied.
    ''' </summary>
    ''' <param name="member">The application member affected.</param>
    Public Sub New(ByVal member As clsApplicationMember)
        Me.New(member, False)
    End Sub

    ''' <summary>
    ''' Creates a new event args object wrapping the given member and indicating
    ''' whether the event is a result of a filter being applied or not.
    ''' </summary>
    ''' <param name="member">The application member affected.</param>
    ''' <param name="resultOfFilter">True to indicate that this event was a result
    ''' of a filter being applied, false to indicate otherwise.</param>
    Public Sub New(ByVal member As clsApplicationMember, ByVal resultOfFilter As Boolean)
        mMember = member
        mResultOfFilter = resultOfFilter
    End Sub

    ''' <summary>
    ''' The application member affected by this event.
    ''' </summary>
    Public ReadOnly Property Member() As clsApplicationMember
        Get
            Return mMember
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the event detailed by these args is a result of a filter
    ''' being applied or not.
    ''' </summary>
    Public ReadOnly Property IsResultOfFilter() As Boolean
        Get
            Return mResultOfFilter
        End Get
    End Property

End Class
