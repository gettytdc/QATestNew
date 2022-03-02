#If UNITTESTS Then

Imports NUnit.Framework

<TestFixture()> _
Public Class ProcessDependencyTests

    ''' <summary>
    ''' Check that the types and values are properly reflected.
    ''' </summary>
    <Test()> _
    Public Sub TestTypesAndValues()

        'Basic check of types and value names...
        Assert.That(clsProcessDependency.ExternalTypes.Contains("ProcessNameDependency"))
        Dim lst As ISet(Of String) = clsProcessDependency.ValueNames("ProcessNameDependency")
        Assert.That(lst.Count, [Is].EqualTo(1))
        Assert.That(lst.Contains("RefProcessName"))

        'Basic check of value retrieval...
        Dim dep As New clsProcessActionDependency("myproc", "myaction")
        Assert.That(dep.TypeName, [Is].EqualTo("ProcessActionDependency"))
        Dim vals As IDictionary(Of String, Object) = dep.GetValues()
        Assert.That(vals.Count(), [Is].EqualTo(2))
        Assert.That(vals("RefProcessName"), [Is].EqualTo("myproc"))
        Assert.That(vals("RefActionName"), [Is].EqualTo("myaction"))

        'Basic check of programmatic creation...
        Dim g As Guid = Guid.NewGuid
        Dim dep2 As clsProcessDependency
        dep2 = clsProcessDependency.Create("ProcessElementDependency", {1, g, "myproc"})
        Dim edep As clsProcessElementDependency = CType(dep2, clsProcessElementDependency)
        Assert.That(edep.RefProcessName, [Is].EqualTo("myproc"))
        Assert.That(edep.RefElementID, [Is].EqualTo(g))

    End Sub

End Class

#End If
