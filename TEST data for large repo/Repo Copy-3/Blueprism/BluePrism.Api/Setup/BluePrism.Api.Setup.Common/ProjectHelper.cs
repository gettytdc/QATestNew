namespace BluePrism.Api.Setup.Common
{
    using System;

    public static class ProjectHelper
    {
        public static void ValidateAssemblyCompatibility()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (!assembly.ImageRuntimeVersion.StartsWith("v2."))
            {
                Console.WriteLine("Warning: assembly '{0}' is compiled for {1} runtime, which may not be compatible with the CLR version hosted by MSI. " +
                                  "The incompatibility is particularly possible for the EmbeddedUI scenarios. " +
                                  "The safest way to solve the problem is to compile the assembly for v3.5 Target Framework.",
                    assembly.GetName().Name, assembly.ImageRuntimeVersion);
            }
        }
    }
}
