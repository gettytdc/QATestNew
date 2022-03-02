using Autofac;
using BluePrism.AutomateProcessCore.Compilation;

namespace BluePrism.StartUp.Modules
{
    /// <summary>
    /// Registers Web API components and dependencies
    /// </summary>
    public class CompilationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CodeCompiler>().As<ICodeCompiler>();
        }
    }
}
