Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Model
Imports BluePrism.AutomateAppCore
Imports BluePrism.Skills

Namespace Controls.Widgets.ProcessStudio.Skills.ViewModel
    Public Class SkillModel : Implements IDisposable
        Private Property mSkills As New List(Of SkillViewModel)

        Public Sub New()
            LoadSkills()
        End Sub

        Private Sub LoadSkills()
            Try
                Dim skillsFromDB = gSv.GetSkills().Where(Function(s) s.Enabled AndAlso If(TryCast(s.LatestVersion, WebSkillVersion)?.WebApiEnabled, True)).ToList()
                Dim loadedSkills = New List(Of SkillViewModel)

                skillsFromDB.ForEach(Sub(s) loadedSkills.Add(New SkillViewModel(s)))
                mSkills = loadedSkills.ToList()
            Catch ex As Exception
                UserMessage.Err(ex, My.Resources.UnableToLoadSkillsFromDatabase)
            End Try
        End Sub

        Friend Function GetSkillsFor(category As SkillCategory) As IEnumerable(Of SkillViewModel)
            Return mSkills.Where(Function(x) x.Category.Equals(category))
        End Function

        Friend Function GetSkill(skillId As Guid) As SkillViewModel
            Return mSkills.FirstOrDefault(Function(x) x.ID.Equals(skillId))
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            mSkills.ForEach(Sub(x) x.Dispose())
        End Sub

        Friend ReadOnly Property HasSkills As Boolean
            Get
                Return mSkills.Any()
            End Get
        End Property
    End Class
End Namespace