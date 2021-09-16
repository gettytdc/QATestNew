namespace AutomateControlTester.Configurators
{
    public partial class GrippableSplitterConfigurator : BaseConfigurator
    {
        public GrippableSplitterConfigurator()
        {
            InitializeComponent();
        }

        public override string ConfigName
        {
            get { return "Grippable Splitter"; }
        }
    }
}
