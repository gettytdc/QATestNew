''' <summary>
''' Class to build and query information about forthcoming events which affect the
''' overall license entitlement.
''' </summary>
<Serializable>
Public Class LicenseChanges

#Region " Inner event class "

    Private Class LicenseEvent
        ''' <summary>
        ''' Gets or sets type of this license change event
        ''' </summary>
        Public Property EventType() As LicenseChangeEventType

        ''' <summary>
        ''' Gets or sets the date of this license change event
        ''' </summary>
        Public Property EventDate() As DateTime

        ''' <summary>
        ''' Gets or sets a value indicating whether the license related to the change is standalone
        ''' </summary>
        Public Property Standalone() As Boolean

        ''' <summary>
        ''' Gets the number of days until this license event occurs
        ''' </summary>
        Public ReadOnly Property DaysToEvent() As Integer
            Get
                Return (EventDate - DateTime.Today).Days
            End Get
        End Property
    End Class

#End Region

#Region " Constants and member variables "

    ' The number of days in the future to consider
    Public Const NumberOfDays As Integer = 31

    Public Const GracePeriodNumberOfDays As Integer = 5

    ' List of license change events
    Private mEvents As New List(Of LicenseEvent)

    ' Flag to indicate all active license keys will expire soon
    Private mAllExpireSoon As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new list of future license change events, for the passed set of
    ''' license keys.
    ''' </summary>
    Public Sub New(keys As IEnumerable(Of KeyInfo))

        ' Keep track of latest expiry and earliest start dates so we can detect
        ' if a license comes into effect before an existing one expires
        Dim lastExpiry As DateTime = DateTime.MinValue
        Dim firstStart As DateTime = DateTime.MaxValue

        mAllExpireSoon = True
        For Each k In keys

            If k.Effective Then
                If k.ExpiresSoon Then
                    ' Add expiry event
                    ' Since licenses are still valid on the expiry date, the event
                    ' date is the day after (i.e. the entitlement will only be affected
                    ' the day after the expiry date)
                    Dim eventDate = k.ExpiryDate.AddDays(1)
                    mEvents.Add(New LicenseEvent() With
                        {.EventType = LicenseChangeEventType.Expires, .EventDate = eventDate, .Standalone = k.Standalone})
                    If k.ExpiryDate > lastExpiry Then lastExpiry = eventDate
                ElseIf k.IsWithinGracePeriod AndAlso k.GracePeriodEndsSoon Then
                    ' Add grace period end event
                    mEvents.Add(New LicenseEvent() With
                        {.EventType = LicenseChangeEventType.GracePeriodEnds, .EventDate = k.GracePeriodEndDate, .Standalone = k.Standalone})
                    If k.StartDate < firstStart Then firstStart = k.StartDate
                ElseIf k.StartsSoon Then
                    ' Add start event
                    mEvents.Add(New LicenseEvent() With
                        {.EventType = LicenseChangeEventType.BecomesActive, .EventDate = k.StartDate, .Standalone = k.Standalone})
                    If k.StartDate < firstStart Then firstStart = k.StartDate
                Else
                    ' Must be valid upto the end of the period we're considering
                    mAllExpireSoon = False
                End If
            Else
                If k.StartsSoon Then
                    If k.RequiresActivation AndAlso k.ActivationResponse Is Nothing Then
                        ' Add requires activation
                        mEvents.Add(New LicenseEvent() With
                            {.EventType = LicenseChangeEventType.RequiresActivation, .EventDate = k.StartDate, .Standalone = k.Standalone})
                    Else
                        ' Add start event
                        mEvents.Add(New LicenseEvent() With
                        {.EventType = LicenseChangeEventType.BecomesActive, .EventDate = k.StartDate, .Standalone = k.Standalone})
                        If k.StartDate < firstStart Then firstStart = k.StartDate
                    End If
                ElseIf k.IsWithinStartAndEndDate AndAlso k.RequiresActivation AndAlso k.ActivationResponse Is Nothing Then
                    ' Add requires activation
                    mEvents.Add(New LicenseEvent() With
                        {.EventType = LicenseChangeEventType.RequiresActivation, .EventDate = DateTime.UtcNow, .Standalone = k.Standalone})
                End If
            End If
        Next

        ' If there are no expiries then don't report that all licenses will expire
        ' Also, if they do all expire then check for a new one coming into effect
        ' earlier
        If lastExpiry = DateTime.MinValue OrElse firstStart <= lastExpiry Then
            mAllExpireSoon = False
        End If
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the number of future license change events for this set of license keys
    ''' </summary>
    Public ReadOnly Property Count() As Integer
        Get
            Return mEvents.Count()
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of days to the last license change event (or -1 there are no
    ''' forthcoming changes).
    ''' </summary>
    Public ReadOnly Property DaysToLastChange() As Integer
        Get
            If Count = 0 Then Return -1
            Return LastEvent.DaysToEvent
        End Get
    End Property

    ''' <summary>
    ''' Gets the next license change event (or Nothing if there are no forthcoming
    ''' changes)
    ''' </summary>
    Private ReadOnly Property NextEvent() As LicenseEvent
        Get
            If Count = 0 Then Return Nothing
            Return (From e In mEvents
                    Order By e.DaysToEvent Ascending
                    Select e).First
        End Get
    End Property

    ''' <summary>
    ''' Gets the last license change event (or Nothing if there are no forthcoming
    ''' changes). Note that if all licenses are due to expire within the
    ''' period, any future license changes will not be included in this calculation.
    ''' In other words the last change date is the point at which the application
    ''' becomes unlicensed, even if other licenses are due to come into effect after
    ''' that date)
    ''' </summary>
    Private ReadOnly Property LastEvent() As LicenseEvent
        Get
            If Count = 0 Then Return Nothing
            Return (From e In mEvents
                    Where Not mAllExpireSoon OrElse e.EventType = LicenseChangeEventType.Expires
                    Order By e.DaysToEvent Descending
                    Select e).First
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of days to next license change event (or -1 if there are no
    ''' forthcoming changes)
    ''' </summary>
    Public ReadOnly Property DaysToNextChange() As Integer
        Get
            If Count = 0 Then Return -1
            Return NextEvent.DaysToEvent
        End Get
    End Property

    ''' <summary>
    ''' Gets the type of the next license change event (or Nothing if there are no
    ''' forthcoming changes)
    ''' </summary>
    Public ReadOnly Property NextChangeType() As LicenseChangeEventType
        Get
            If Count = 0 Then Return Nothing
            Return NextEvent.EventType
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of days to next license change event (or -1 if there are no
    ''' forthcoming changes)
    ''' </summary>
    Public ReadOnly Property IsNextChangeStandalone() As Boolean
        Get
            If Count = 0 Then Return False
            Return NextEvent.Standalone
        End Get
    End Property

    ''' <summary>
    ''' Indicates that all effective licenses will expire soon, resulting in the
    ''' application being unlicensed.
    ''' </summary>
    Public ReadOnly Property AllLicensesExpireSoon() As Boolean
        Get
            Return mAllExpireSoon
        End Get
    End Property

#End Region

End Class
