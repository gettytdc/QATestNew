Imports BluePrism.Skills

Public Class SkillVersionsForm

    Private mSkill As Skill

    Public Sub New(skill As Skill)

        ' This call is required by the designer.
        InitializeComponent()
        Me.Icon = Icon.FromHandle(BluePrism.Images.ComponentImages.Skill_16x16.GetHIcon())

        mSkill = skill
        ' Add any initialization after the InitializeComponent() call.
        PopulateDataGrid(skill)
    End Sub

    Private Sub PopulateDataGrid(skill As Skill)

        dgvSkillVersions.Rows.Clear()

        For Each skillVersion In skill.PreviousVersions.OrderByDescending(Function(x) x.ImportedAt)
            dgvSkillVersions.Rows.Add(
                skillVersion.Name,
                SkillCategoryExtensions.GetDescription(skillVersion.Category),
                skillVersion.VersionNumber)
        Next

    End Sub

End Class