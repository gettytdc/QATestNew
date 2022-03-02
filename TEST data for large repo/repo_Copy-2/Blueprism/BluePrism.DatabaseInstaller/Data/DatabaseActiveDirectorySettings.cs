using System;

namespace BluePrism.DatabaseInstaller
{
    public class DatabaseActiveDirectorySettings
    {
        public string Domain { get; }
        public string AdminGroupId { get; }
        public string AdminGroupName { get; }
        public string AdminGroupPath { get; }
        public string AdminRole { get; }

        public DatabaseActiveDirectorySettings(string domain, string adminGroupId, string adminGroupName, string adminGroupPath, string adminRole)
        {
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException(nameof(domain));

            Domain = domain;
            AdminGroupId = adminGroupId;
            AdminGroupName = adminGroupName;
            AdminGroupPath = adminGroupPath;
            AdminRole = adminRole;
        }
    }
}
