using System.Windows.Forms;

namespace AutomateControlTester.Configurators
{
    /// <summary>
    /// Base configurator to use which establishes the standard of the configurator
    /// itself being the control that it serves up.
    /// </summary>
    public class BaseConfigurator : UserControl, IConfigurator
    {
        /// <summary>
        /// The name of this configurator. Should be overridden by subclasses, but
        /// can't be made abstract because it breaks the visual designer for
        /// subclasses.
        /// </summary>
        public virtual string ConfigName { get { return "<Unset>"; } }

        /// <summary>
        /// The control served by this configurator. For this class and subclasses,
        /// this is the configurator itself.
        /// </summary>
        public Control Control
        {
            get { return this; }
        }

    }
}
