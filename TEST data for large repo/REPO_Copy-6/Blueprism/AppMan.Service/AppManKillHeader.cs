using System;

namespace BluePrism.ApplicationManager.AppMan.Service
{
    public class AppManKillHeader
    {
        private const string Header = "ForceKill AppManId ";
        public Guid Id { get; }
        public AppManKillHeader(Guid id) => Id = id;

        public static AppManKillHeader Parse(string line) =>
            new AppManKillHeader(Guid.Parse(line.Substring(Header.Length)));

        public static bool ContainsHeader(string line) => !string.IsNullOrEmpty(line) && line.StartsWith(Header);

    }
}
