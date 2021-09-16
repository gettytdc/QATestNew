using System.Windows.Forms;

namespace AutomateControlTester
{
    public interface IConfigurator
    {
        string ConfigName { get; }

        Control Control { get; }
    }
}
