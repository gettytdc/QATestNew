using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WixSharp.CommonTasks;
using WixSharp.UI.Forms;

namespace WixSharp
{
    static class Extensions
    {
        public static void AddFiles(this Dir dir, IEnumerable<File> files) =>
            Tasks.AddFiles(dir, files.ToArray());

        public static void AddDirs(this Dir dir, IEnumerable<Dir> dirs) =>
            Tasks.AddDirs(dir, dirs.ToArray());

        public static void AddMergeModules(this Dir dir, IEnumerable<Merge> merges) =>
            Tasks.AddMergeModules(dir, merges.ToArray());

        public static void AddEventSources(this Dir dir, IEnumerable<EventSource> events) =>
            Tasks.Add(dir, events.ToArray());

        public static void AddDirs(this Project project, IEnumerable<Dir> dirs) =>
            Tasks.AddDirs(project, dirs.ToArray());

        public static void AddActions(this Project project, IEnumerable<Action> actions) =>
            Tasks.AddActions(project, actions.ToArray());

        public static bool ShouldInstall(this FeatureItem feature) => feature.DisallowAbsent || feature.IsViewChecked();


        public static void AddRegKeys(this Project project, IEnumerable<RegKey> regkeys)
        {
            foreach (var k in regkeys)
                Tasks.AddRegKey(project, k);
        }
    }
}
