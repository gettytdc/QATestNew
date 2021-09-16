#If UNITTESTS Then

Imports NUnit.Framework

<TestFixture()> _
Public Class BreakpointTests

    <Test()> _
    Public Sub TestTransience()

        Dim bp As New clsProcessBreakpoint(Nothing)

        ' Check individual possible value
        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.None
        Assert.That(bp.IsTransient, [Is].False)

        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.WhenConditionMet
        Assert.That(bp.IsTransient, [Is].False)

        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.WhenDataValueRead
        Assert.That(bp.IsTransient, [Is].False)

        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.WhenDataValueChanged
        Assert.That(bp.IsTransient, [Is].False)

        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.HandledException
        Assert.That(bp.IsTransient, [Is].False)

        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.Transient
        Assert.That(bp.IsTransient, [Is].True)

        ' Check combinations
        bp.BreakPointType = clsProcessBreakpoint.BreakEvents.Transient Or clsProcessBreakpoint.BreakEvents.WhenConditionMet
        Assert.That(bp.IsTransient, [Is].True)

    End Sub

End Class

#End If