using Newtonsoft.Json;

namespace AutomateControls.UIState
{
    public class UIControlConfig
    {
        [JsonProperty(PropertyName = "i")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "v")]
        public object Value { get; set; }
    }
}