#If UNITTESTS Then

Imports NUnit.Framework

<TestFixture()> _
Public Class ApplicationModelTests

    <Test()> _
    Public Sub TestInsertMember()

        Dim top As New clsApplicationElement("Top")
        Dim a As New clsApplicationElement("a")
        Dim b As New clsApplicationElement("b")
        Dim c As New clsApplicationElement("c")
        Dim d As New clsApplicationElementGroup("d")
        top.AddMember(a)
        top.AddMember(b)
        top.AddMember(c)
        top.AddMember(d)

        Dim aa As New clsApplicationElement("aa")
        Dim ab As New clsApplicationElement("ab")
        Dim ac As New clsApplicationElement("ac")
        a.AddMember(aa) : a.AddMember(ab) : a.AddMember(ac)

        Dim ba As New clsApplicationElement("ba")
        b.AddMember(ba)

        Dim da As New clsApplicationElement("da")
        d.AddMember(da)

        ' So we now have:
        ' top
        '  +- a
        '  |  +- aa
        '  |  +- ab
        '  |  +- ac
        '  |
        '  +- b
        '  |  +- ba
        '  |
        '  +- c
        '  |
        '  +- d
        '     +- da

        ' Move ac to to the top of a
        a.InsertMember(0, ac)
        Assert.That(a.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {ac, aa, ab}))

        ' And back to the bottom
        a.InsertMember(2, ac)
        Assert.That(a.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {aa, ab, ac}))

        ' Move da into c
        c.InsertMember(0, da)

        ' This should:-
        ' i) add da into c's child members and...
        Assert.That(c.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {da}))

        ' ii) remove da from d's child members
        Assert.That(d.ChildMembers, [Is].Empty)

        ' Move da into b now
        b.InsertMember(1, da)
        Assert.That(b.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {ba, da}))
        Assert.That(c.ChildMembers, [Is].Empty)

        ' And to the top of b
        b.InsertMember(0, da)
        Assert.That(b.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {da, ba}))

        ' Into the root
        top.InsertMember(4, da)
        Assert.That(top.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {a, b, c, d, da}))
        Assert.That(b.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {ba}))

        ' And back to d
        d.InsertMember(0, da)
        Assert.That(top.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {a, b, c, d}))
        Assert.That(d.ChildMembers, _
         [Is].EqualTo(New clsApplicationMember() {da}))

        ' Finally, check some error conditions
        Try
            d.InsertMember(-1, da)
            Assert.Fail("InsertMember(-1, xx) was permitted; -1 is an invalid index")
        Catch aoore As ArgumentOutOfRangeException
            ' Correct
        Catch ex As Exception
            Assert.Fail("Expected ArgOutOfRange exception: actually got: {0}", ex)
        End Try

        Try
            d.InsertMember(10, da)
            Assert.Fail("InsertMember(10, xx) was permitted; Should be {0} or less", _
             d.ChildMembers.Count)
        Catch aoore As ArgumentOutOfRangeException
            ' Correct
        Catch ex As Exception
            Assert.Fail("Expected ArgOutOfRange exception: actually got: {0}", ex)
        End Try

    End Sub

End Class

#End If
