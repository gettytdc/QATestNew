using System;

namespace BluePrism.Setup
{
    public class ApiVersion
    {
        public Version Version { get; set; }
        public string ApiHash { get; set; }

        public ApiVersion(string apiVersion)
        {
            try
            {
                var split = apiVersion.Split('-');
                Version = Version.Parse(split[0]);
                if (split.Length > 1)
                    ApiHash = split[1];
            }
            catch
            {
                Version = null;
                ApiHash = apiVersion;
            }
        }

        public static ApiVersion Parse(string apiVersion)
        {
            return new ApiVersion(apiVersion);
        }

        public override string ToString()
        {
            if (Version == null) return ApiHash;
            if (string.IsNullOrEmpty(ApiHash)) return Version.ToString();
            return $"{Version}-{ApiHash}";
        }

        public string ToFriendlyString()
        {
            if (Version == null) return ApiHash;
            return $"{Version.Major}.{Version.Minor}.{Version.Build}";
        }

        public static bool AreCompatible(string a, string b)
        {
            return AreCompatible(Parse(a), Parse(b));
        }

        public static bool AreCompatible(ApiVersion a, ApiVersion b)
        {
            return a.Version.Major == b.Version.Major && a.Version.Minor == b.Version.Minor && a.ApiHash == b.ApiHash;
        }
    }
}
