using System;

namespace BluePrism.DatabaseInstaller
{
    [Flags]
    public enum DatabaseInstallerOptions
    {
        None = 0,
        MigrateSessionsPre65 = 1
    }
}