using BluePrism.AutomateAppCore.Config;

namespace AutomateUI.UnitTests.Classes
{
    public class Branding_WithOverrides : Branding
    {
        public string DefaultTitle_Override { get; set; }
        public string UnbrandedTitle_Override { get; set; }
        public string LicenseOverrideTitle_Override { get; set; }
        public bool? IsLicensed_Override { get; set; }

        public Branding_WithOverrides(IOptions options) : base(options) { }

        public override string GetDefaultTitle() 
            => DefaultTitle_Override ?? base.GetDefaultTitle();

        public override string GetUnbrandedTitle() 
            => UnbrandedTitle_Override ?? base.GetUnbrandedTitle();

        public override string GetLicenseTitleOverride() 
            => LicenseOverrideTitle_Override ?? base.GetLicenseTitleOverride();

        public override bool IsLicensed() 
            => IsLicensed_Override ?? base.IsLicensed();
    }
}
