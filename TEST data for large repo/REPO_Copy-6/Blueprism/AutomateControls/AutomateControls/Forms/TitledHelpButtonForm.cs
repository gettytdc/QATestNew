using System.Drawing;

namespace AutomateControls.Forms
{
    public partial class TitledHelpButtonForm : HelpButtonForm, IEnvironmentColourManager
    {
        public virtual string Title
        {
            get => objBluebar.Title;
            set => objBluebar.Title = value;
        }

        public virtual string SubTitle
        {
            get => objBluebar.SubTitle;
            set => objBluebar.SubTitle = value;
        }

        public virtual Color EnvironmentBackColor
        {
            get => objBluebar.BackColor;
            set => objBluebar.BackColor = value;
        }

        public virtual Color EnvironmentForeColor
        {
            get => objBluebar.TitleColor;
            set => objBluebar.TitleColor = value;
        }

        public TitledHelpButtonForm()
        {
            InitializeComponent();
        }
    }
}
