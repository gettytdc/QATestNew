namespace BluePrism.ClientServerResources.Core.Events
{

    public delegate void SessionVariableUpdatedHandler(object sender, SessionVariableUpdatedEventArgs e);

    public class SessionVariableUpdatedEventArgs
        : BaseResourceEventArgs
    {
        private readonly string _jsonData;
        
        public string JSONData => _jsonData ?? string.Empty;


        public SessionVariableUpdatedEventArgs(string jsonData)
            : this (jsonData, string.Empty)
        {
        }

        public SessionVariableUpdatedEventArgs(string jsonData, string errorMessage)
        {
            _jsonData = string.IsNullOrEmpty(jsonData) ? null : jsonData;
            ErrorMessage = errorMessage;
        }
    }
}
