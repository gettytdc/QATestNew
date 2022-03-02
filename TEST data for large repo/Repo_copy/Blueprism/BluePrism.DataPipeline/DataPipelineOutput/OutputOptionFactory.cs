
namespace BluePrism.DataPipeline.DataPipelineOutput
{
    public static class OutputOptionFactory
    {
        public static OutputOption GetOutputOption(string key, string value)
        {
            switch(key)
            {
                case "credential":
                    return new AuthorizationOutputOption(key, value);
                default:
                    return new OutputOption(key, value);
            }
            
                
        }
    }
}