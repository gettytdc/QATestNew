using System.ComponentModel;
using System.Text.RegularExpressions;
using BluePrism.CharMatching.Properties;

namespace BluePrism.CharMatching
{
    public class CMDescriptionAttribute : DescriptionAttribute
    {
        public CMDescriptionAttribute(string s) : base(s)
        {
            //just a supercall
        }

        public override string Description
        {
            get
            {
                var resxKey = Regex.Replace(base.Description, @"\b(\w)+\b",
                    x => x.Value[0].ToString().ToUpper() + x.Value.Substring(1));
                resxKey = "CMDescription_" + Regex.Replace(resxKey, @"[^a-zA-Z0-9]*", "");

                // Keys are restricted to 79 characters + "CMDescription_"
                if (resxKey.Length > 93)
                    resxKey = resxKey.Substring(0, 93);


                var localized = Resources.ResourceManager.GetString($"{resxKey}");

                if (localized != null) return localized;

                return base.Description;
            }
        }
    }
}