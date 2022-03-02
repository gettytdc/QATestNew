using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AutomateControls.UIState
{
    public class UIStateManager : IUIStateManager
    {
        public static UIStateManager Instance { get; }
            = new UIStateManager();

        private List<UIControlConfig> _uiControlConfigs = new List<UIControlConfig>();

        public string UIControlConfigs
        {
            get { return JsonConvert.SerializeObject(_uiControlConfigs); }
            set 
            {
                try 
                {
                    _uiControlConfigs = JsonConvert.DeserializeObject<List<UIControlConfig>>(value);
                }
                catch
                {
                    _uiControlConfigs = new List<UIControlConfig>();
                }
            }
        }

        public UIControlConfig GetControlConfig(string id)
        {
            return _uiControlConfigs.SingleOrDefault(e => e.Id == id);
        }

        public void SetControlConfig(UIControlConfig uiControlConfig)
        {
            var existingElement = GetControlConfig(uiControlConfig.Id);

            if (existingElement != null)
                _uiControlConfigs.Remove(existingElement);

            _uiControlConfigs.Add(uiControlConfig);
        }
    }
}
