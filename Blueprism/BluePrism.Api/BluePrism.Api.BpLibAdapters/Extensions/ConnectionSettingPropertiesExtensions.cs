namespace BluePrism.Api.BpLibAdapters.Extensions
{
    using AutomateAppCore;

    public static class ConnectionSettingPropertiesExtensions
    {
        public static clsDBConnectionSetting ToDbConnectionSetting(this ConnectionSettingProperties @this) =>
            new clsDBConnectionSetting(
                @this.ConnectionName,
                @this.ServerName,
                @this.DatabaseName,
                @this.DatabaseUsername,
                @this.DatabasePassword,
                @this.UsesWindowAuth);
    }
}
