namespace AutomateControls.UIState
{
    public interface IUIStateManager
    {
        UIControlConfig GetControlConfig(string id);
        void SetControlConfig(UIControlConfig uiControlConfig);
    }
}
