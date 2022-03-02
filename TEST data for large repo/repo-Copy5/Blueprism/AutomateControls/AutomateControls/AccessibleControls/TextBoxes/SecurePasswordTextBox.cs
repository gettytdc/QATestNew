using AutomateControls.WindowsSupport;
using BluePrism.BPCoreLib;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutomateControls.Textboxes;
using BluePrism.Common.Security;

namespace AutomateControls
{
    /// <summary>
    /// Password text box that doesn't store the password as plain text at any point.
    /// Instead the password is stored as a serializable SafeString,
    /// which is less readable and can be programatically removed from memory.
    /// </summary>
    public partial class SecurePasswordTextBox : StyledTextBox
    {
        #region - Member Variables -

        /// <summary>
        /// Flag to indicate that the key message has been manually processed
        /// </summary>
        private bool _keyMessageProcessed = false;

        /// <summary>
        /// Flag determining whether to process the set text windows message.
        /// </summary>
        private bool _handleSetText = true;

        // The string containing the password in a secure string
        private SafeString _securePassword;

        /// <summary>
        /// Flag indicating whether the placeholder is currently being displayed
        /// </summary>
        private bool _showingPlaceholder = false;



        /// <summary>
        /// The length of the masking char string used for a placeholder
        /// </summary>
        public const int PlaceHolderLength = 8;

        #endregion

        #region - Auto-properties -

        /// <summary>
        /// Get or set the character used for password masking. The default value
        /// matches the operating system's password character
        /// </summary>
        /// <remarks>
        /// Need to prevent the property from automatically being generated
        /// in the designer code (using Browsable and DesignerSerializationVisibility)
        /// otherwise the char will be hardcoded to the OS that the form was
        /// designed on
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public char PasswordMaskingCharacter { get; set; }

        /// <summary>
        /// Indicates whether the control allow pasting. If it does and a password is
        /// pasted in, it will mean that the plain text password is stored in memory
        /// and the application has no control over when it is disposed.
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(true), Description(
            "Allow the user to paste a password into this box, which may expose " +
            "the text into managed memory")]
        public bool AllowPasting { get; set; }

        /// <summary>
        /// Indicates whether the control will be initially loaded with a
        /// placeholder. This placeholder will be made up of masking chars, and will
        /// be cleared as soon as there is any interaction with the control
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false), Description(
            "Show a placeholder password, removed on any interaction, even if " +
            "the password is empty.")]
        public bool UsePlaceholder { get; set; }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new SecurePasswordBox
        /// </summary>
        public SecurePasswordTextBox()
        {
            InitializeComponent();

            //Set default property values
            SecurePassword = new SafeString();
            PasswordMaskingCharacter = BPUtil.PasswordChar;
            AllowPasting = true;
            ImeMode = ImeMode.Disable;

            //Enable shortcuts so the control detects pasting into the textbox
            this.ShortcutsEnabled = AllowPasting;
            _handleSetText = true;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets or sets the password as a SafeString
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SafeString SecurePassword
        {
            get { return _securePassword; }
            set
            {
                _securePassword = (value ?? new SafeString());
                ResetMaskingCharacters(_securePassword.Length);
            }
        }

        /// <summary>
        /// The text property of the secure password text box is used to display the
        /// password masking chars. The actual password is stored in the
        /// <see cref="SecurePassword"/> property.
        /// </summary>
        /// <remarks>This is hidden from the designer because setting the text value
        /// will not result in the setting of the secure string, so it should not
        /// really be used to set a value for this control.</remarks>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                //Prevent the Set Text windows message being processed and added to
                //the secure string when the control's text property is set
                //e.g. when the password chars are added to the control.
                _handleSetText = false;


                try
                {
                    base.Text = value;
                }
                catch
                {
                    _handleSetText = true;
                }

                // If value is null or an empty string, we won't receive a WM_SETTEXT message.
                if (string.IsNullOrEmpty(value))
                {
                    _handleSetText = true;
                }
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Override create control to ensure the placeholder is added to the text
        /// box if required.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (UsePlaceholder)
            {
                this.Text = new String(PasswordMaskingCharacter, 8);
                _showingPlaceholder = true;
            }

        }

        /// <summary>
        /// Override the function that processes key messages. If the key message has
        /// already been processed then the base function is not called and the key
        /// press is not passed through to the control.
        /// </summary>
        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (!_keyMessageProcessed)
            {
                return base.ProcessKeyMessage(ref m);
            }
            else
            {
                //Return flag to false, ready for the next key to process
                _keyMessageProcessed = false;

                //Set message to be processed so char not passed through to control
                return true;
            }
        }

        /// <summary>
        /// Override the function detecting whether the key message is an input char.
        /// If it the input is not a control/cursor key then add the char to the
        /// securestring. If the char is backspace then process the backspace.
        /// </summary>
        protected override bool IsInputChar(char charCode)
        {

            int keyCode = (int)charCode;

            //Only add a character to the secure string if it is not a control/
            //cursor key
            if (!char.IsControl(charCode) && !char.IsHighSurrogate(charCode)
                && !char.IsLowSurrogate(charCode))
            {
                ProcessNewCharacter(charCode);
                _keyMessageProcessed = true;
            }

            else if (keyCode == (int)Keys.Back)
            {
                ProcessBackspace();
                _keyMessageProcessed = true;
            }
            else if(keyCode == 127)
            {
                ProcessControlBackspace();
                _keyMessageProcessed = true;
            }

            return true;
        }

        /// <summary>
        /// Override the function detecting whether the key message is an input key.
        /// If it is the delete key then process the delete and indicate that the key
        /// message has been processed.
        /// </summary>
        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Delete) == Keys.Delete)
            {
                ProcessDelete();
                _keyMessageProcessed = true;
                return true;
            }
            else if ((keyData & Keys.Tab) == Keys.Tab)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        /// <summary>
        /// Need to override the default behaviour of <strike>two</strike> three
        /// windows messages
        /// 1. The WM_PASTE message so that we can create our own custom behaviour.
        /// This will occur if you paste using the control's context menu.
        /// 2. The WM_SETTEXT message which is used when running Blue Prism
        /// from Blue Prism i.e. in Automated Tests, and also when setting the text
        /// property of the password field i.e. when setting masking chars.
        /// 3. The EM_REPLACESEL message is used when using the new Win32
        /// 'Set Password Text' action
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WindowsMessage.WM_PASTE: ProcessPasteClipboard(); break;
                case WindowsMessage.WM_SETTEXT: ProcessSetText(ref m); break;
                case WindowsMessage.EM_REPLACESEL: ProcessReplaceSel(ref m); break;
                default: base.WndProc(ref m); break;
            }
        }

        /// <summary>
        /// Processes the REPLACESEL windows message being sent to this control
        /// </summary>
        /// <param name="m">The windows message to process</param>
        private void ProcessReplaceSel(ref Message m)
        {
            int start = SelectionStart;
            int numChars = SelectionLength;
            while (--numChars >= 0)
                SecurePassword.RemoveAt(start);

            foreach (char c in Marshal.PtrToStringUni(m.LParam))
            {
                ProcessNewCharacter(c);
            }

        }

        /// <summary>
        /// Processes any new character that is entered into the password text box
        /// and adds it to the secure string
        /// </summary>
        private void ProcessNewCharacter(char character)
        {
            //Insert new character into the secure string, and reset the masking
            //characters
            if (SelectionLength > 0)
                RemoveSelectedCharacters();

            SecurePassword.InsertAt(this.SelectionStart, character);

            ResetMaskingCharacters(this.SelectionStart + 1);
        }

        /// <summary>
        /// Remove any characters from the secure string that are currently
        /// selected in the text box
        /// </summary>
        private void RemoveSelectedCharacters()
        {
            int count = SelectionLength;
            while (--count >= 0)
            {
                SecurePassword.RemoveAt(this.SelectionStart);
            }
        }

        /// <summary>
        /// Set the number of masking characters to match length of the password, and
        /// ensure the caret stays in the same position
        /// </summary>
        private void ResetMaskingCharacters()
        {
            ResetMaskingCharacters(SelectionStart);
        }

        /// <summary>
        /// Set the number of masking characters to match length of the password,
        /// setting the caret position after the masking chars are applied.
        /// </summary>
        /// <param name="caretPosition">The desired position of the caret after the
        /// masking character have been set in the control</param>
        private void ResetMaskingCharacters(int caretPosition)
        {
            this.Text = new string(PasswordMaskingCharacter, SecurePassword.Length);
            this.SelectionStart = caretPosition;
        }

        /// <summary>
        /// Process any backspaces applied to the password text box
        /// </summary>
        private void ProcessBackspace()
        {
            if (this.SelectionLength > 0)
            {
                //If there are characters highlighted then remove these
                RemoveSelectedCharacters();
                ResetMaskingCharacters();
            }
            else if (this.SelectionStart > 0)
            {
                //When there are no characters highlighted (and the caret is not at
                //the start of the text box), then remove the character that appears
                //before the caret
                SecurePassword.RemoveAt(this.SelectionStart - 1);
                ResetMaskingCharacters(this.SelectionStart - 1);
            }
        }

        /// <summary>
        /// Calls the clear method to remove all characters from the field.
        /// </summary>
        private void ProcessControlBackspace()
        {
            this.Clear();
        }

        

        /// <summary>
        /// Process when the contents of the clipboard are being pasted into the
        /// password control
        /// </summary>
        private void ProcessPasteClipboard()
        {
            if (AllowPasting)
            {
                //Process each character from the clipboard into the SecureString
                foreach (char c in Clipboard.GetText())
                {
                    ProcessNewCharacter(c);
                }
            }
            else
            {
                //Do nothing if the control doesn't allow pasting
            }
        }

        /// <summary>
        /// Manually process the set text message that is sent to the control. This
        /// will happen when Blue Prism is running Blue Prism, and also when the
        /// password masking chars are being set.
        /// </summary>
        private void ProcessSetText(ref Message m)
        {
            if (_handleSetText)
            {
                //Remove the placeholder if it exists
                Clear();

                string password = Marshal.PtrToStringUni(m.LParam);

                //Process each character from the set text message into the
                //secure string
                foreach (char c in password)
                {
                    ProcessNewCharacter(c);
                }
            }
            else
            {
                _handleSetText = true;
                base.WndProc(ref m);
            }
        }

        /// <summary>
        /// Process when the delete button is pressed in the password text box
        /// </summary>
        private void ProcessDelete()
        {
            if (this.SelectionLength > 0)
            {
                //If there are characters highlighted then remove these
                RemoveSelectedCharacters();
            }
            else if (this.SelectionStart < this.Text.Length)
            {
                //When there are no characters highlighted (and the caret is not at
                //the end of the text box), then remove the character that appears
                //after the caret
                SecurePassword.RemoveAt(this.SelectionStart);
            }

            ResetMaskingCharacters();
        }

        /// <summary>
        /// Remove the placeholder if it is still being shown
        /// </summary>
        public void RemovePlaceholder()
        {
            if (_showingPlaceholder)
            {
                ResetMaskingCharacters(0);
                _showingPlaceholder = false;
            }
        }

        /// <summary>
        /// Ensure the placeholder is removed when you enter the password box
        /// </summary>
        protected override void OnEnter(EventArgs e)
        {
            RemovePlaceholder();
            base.OnEnter(e);
        }

        /// <summary>
        /// Resets the Text property to its default value.
        /// </summary>
        public override void ResetText()
        {
            Clear();
        }

        /// <summary>
        /// Clear the password text box
        /// </summary>
        /// <remarks>
        /// This hides the base text box Clear method, and ensures the underlying
        /// SecureString is cleared properly
        /// </remarks>
        public new void Clear()
        {
            SecurePassword.Clear();
            ResetMaskingCharacters(0);
        }

        #endregion

    }
}
