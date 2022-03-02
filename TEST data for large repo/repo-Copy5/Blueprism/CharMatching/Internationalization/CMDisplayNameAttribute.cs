using System.ComponentModel;
using System.Text.RegularExpressions;
using BluePrism.CharMatching.Properties;

namespace BluePrism.CharMatching
{
    public class CMDisplayNameAttribute : DisplayNameAttribute
    {
        public CMDisplayNameAttribute(string s) : base(s)
        {
            //just a supercall
        }

        public override string DisplayName
        {
            get
            {
                var resxKey = Regex.Replace(base.DisplayName, @"\b(\w)+\b",
                    x => x.Value[0].ToString().ToUpper() + x.Value.Substring(1));
                resxKey = "CMDisplayName_" + Regex.Replace(resxKey, @"[^a-zA-Z0-9]*", "");
                var localized = Resources.ResourceManager.GetString($"{resxKey}");

                if (localized != null) return localized;

                return base.DisplayName;
            }
        }
    }
}