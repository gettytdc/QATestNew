using System.ComponentModel;
using System.Text.RegularExpressions;
using BluePrism.CharMatching.Properties;

namespace BluePrism.CharMatching
{
    public class CMCategoryAttribute : CategoryAttribute
    {
        public CMCategoryAttribute(string s) : base(s)
        {
            //just a supercall
        }

        protected override string GetLocalizedString(string cat)
        {
            var resxKey = Regex.Replace(cat, @"\b(\w)+\b",
                x => x.Value[0].ToString().ToUpper() + x.Value.Substring(1));
            resxKey = "CMCategory_" + Regex.Replace(resxKey, @"[^a-zA-Z0-9]*", "");
            var localized = Resources.ResourceManager.GetString($"{resxKey}");

            if (localized != null) return localized;

            return cat;
        }
    }
}