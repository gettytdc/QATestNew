namespace BluePrism.Skills
{
    public enum SkillCategory
    {
        Unknown = 0,
        VisualPerception = 1,
        PlanningAndSequencing = 2,
        Collaboration = 3,
        KnowledgeAndInsight = 4,
        ProblemSolving = 5,
        Learning = 6
    }

    public static class SkillCategoryExtensions
    {
        public static string GetDescription(SkillCategory category)
        {
            return SkillResources.ResourceManager.GetString($"SkillCategory_{category.ToString()}");
        }
    }
}