using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    public partial class TextEnterBox: Form
    {
        public event CancelEventHandler TextEntered;

        private bool _allowEmpty;
        private bool _autotrim;

        public TextEnterBox() : this(true, false) { }

        public TextEnterBox(bool allowEmpty, bool autotrim)
        {
            InitializeComponent();
            tbEnteredText.Validating += HandleTextValidating;
            btnOk.Click += HandleOkClick;
            btnCancel.Click += HandleCancelClick;
            _allowEmpty = allowEmpty;
            _autotrim = autotrim;
            btnOk.Enabled = allowEmpty;
        }

        public string EnteredText
        {
            get { return (_autotrim ? tbEnteredText.Text.Trim() : tbEnteredText.Text); }
            set
            {
                tbEnteredText.Text = value;
                UpdateOkButtonState();
            }
        }

        public string Prompt
        {
            get { return lblPrompt.Text; }
            set { lblPrompt.Text = value; }
        }

        public string Title
        {
            get { return Text; }
            set { Text = value; }
        }

        public bool AllowEmpty
        {
            get { return _allowEmpty; }
            set
            {
                if (_allowEmpty != value)
                {
                    _allowEmpty = value;
                    UpdateOkButtonState();
                }
            }
        }

        public bool AutoTrim
        {
            get { return _autotrim; }
            set
            {
                if (_autotrim != value)
                {
                    _autotrim = value;
                    UpdateOkButtonState();
                }
            }
        }

        void HandleTextValidating(object sender, CancelEventArgs e)
        {
            CancelEventHandler handler = TextEntered;
            // Just pass on the CancelEventArgs we've got - a cancel from our
            // listeners is just as meaningful as one from us
            if (handler != null)
                handler(this, e);
        }

        void HandleOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void HandleCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void tbEnteredText_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        void UpdateOkButtonState()
        {
            btnOk.Enabled = (_allowEmpty || EnteredText != "");
        }

        public static string Show(IWin32Window owner, string prompt)
        {
            return Show(owner, null, prompt, null, true, true);
        }

        public static string Show(string prompt)
        {
            return Show(null, null, prompt, null, true, true);
        }

        public static string Show(string title, string prompt)
        {
            return Show(null, title, prompt, null, true, true);
        }

        public static string Show(IWin32Window owner, string title, string prompt)
        {
            return Show(owner, title, prompt, null, true, true);
        }

        public static string Show(string title, string prompt, string initValue)
        {
            return Show(null, title, prompt, initValue, true, true);
        }

        public static string Show(IWin32Window owner,
            string title, string prompt, string initValue)
        {
            return Show(owner, title, prompt, initValue, true, true);
        }

        public static string Show(
            string title, string prompt, string initValue, bool allowEmpty)
        {
            return  Show(null, title, prompt, initValue, allowEmpty, true);
        }

        public static string Show(IWin32Window owner,
            string title, string prompt, string initValue, bool allowEmpty)
        {
            return Show(owner, title, prompt, initValue, allowEmpty, true);
        }

        public static string Show(IWin32Window owner,
            string title, string prompt, string initValue, bool allowEmpty,
            bool autotrim)
        {
            return Show(owner, title, prompt, initValue, allowEmpty, autotrim, null);
        }

        public static string Show(IWin32Window owner,
            string title, string prompt, string initValue, bool allowEmpty,
            bool autotrim, Func<string,bool> validator)
        {
            using (TextEnterBox box = new TextEnterBox(allowEmpty, autotrim))
            {
                if (title != null)
                    box.Title = title;
                if (prompt != null)
                    box.Prompt = prompt;
                if (initValue != null)
                    box.EnteredText = initValue;

                // Set up the TextEntered event handler with the validator we've
                // been given (if we have been given one.
                CancelEventHandler handler = null;
                if (validator != null)
                {
                    handler = delegate(object sender, CancelEventArgs e) {
                        if (!validator(box.EnteredText))
                            e.Cancel = true;
                    };
                    box.TextEntered += handler;
                }

                box.ShowInTaskbar = false;
                DialogResult res = box.ShowDialog(owner);

                // Make sure there are no dangling references
                if (handler != null)
                    box.TextEntered -= handler;

                if (res == DialogResult.OK)
                    return box.EnteredText;

                return null;
            }
        }
    }
}
