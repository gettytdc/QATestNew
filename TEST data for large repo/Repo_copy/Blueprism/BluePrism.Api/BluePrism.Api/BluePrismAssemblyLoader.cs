namespace BluePrism.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Func;

    public static class BluePrismAssemblyLoader
    {
        public static void LoadAssemblies(
            string parentAssemblyName,
            IList<string> loadedAssemblies = null)
        {
            if (loadedAssemblies == null)
                loadedAssemblies = new List<string>();

            if (loadedAssemblies.Contains(parentAssemblyName))
                return;

            var assembly = AppDomain.CurrentDomain.Load(parentAssemblyName);
            loadedAssemblies.Add(parentAssemblyName);
            assembly.GetReferencedAssemblies()
                .Select(x => x.FullName)
                .Where(x => x.StartsWith("BluePrism."))
                .ForEach(x => LoadAssemblies(x, loadedAssemblies))
                .Evaluate();
        }
    }
}
