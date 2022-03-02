using System;
using System.Security;
using BluePrism.Common.Security;

namespace AutomateControlTester.Configurators
{
    public partial class SecurePasswordTextBoxConfigurator : BaseConfigurator
    {
        public override string ConfigName 
        { get { return "Secure Password Text Box"; } }

        public SecurePasswordTextBoxConfigurator()
        {
            InitializeComponent();
            btnReveal.Click += btnReveal_Click;
            btnObfuscate.Click += btnObfuscate_Click;
            btnDeobfuscate.Click += btnDeObfuscate_Click;
            btnRevealDeobfuscate.Click += btnRevealDeobfuscate_Click;
        }

        /// <summary>
        /// Holds the deobfuscated SecureString password
        /// </summary>
        private SecureString _deobfuscatedSecureString;
                
        /// <summary>
        /// Convert the SecureString password to plain text and display the results.
        /// Only works in debug as the AsString extension will not appear in 
        ///released code.
        /// </summary>
        private void btnReveal_Click(Object sender, EventArgs e)
        {
            #if DEBUG
            string plainText = txtSecurePassword.SecurePassword.AsString();
            txtReveal.Text = plainText;
            #endif
        }
        
        /// <summary>
        /// Obfuscate the SecureString password and display the results 
        /// in the text box
        /// </summary>
        private void btnObfuscate_Click(Object sender, EventArgs e)
        {
            SimpleObfuscator obfuscator = new SimpleObfuscator();
            string outputString
                = obfuscator.Obfuscate(txtSecurePassword.SecurePassword);
            txtObfuscatedPassword.Text = outputString;
        }

        /// <summary>
        /// Deobfuscate the obfuscated password string and store SecureString 
        /// in a private variable
        /// </summary>
        private void btnDeObfuscate_Click(Object sender, EventArgs e)
        {
            SimpleObfuscator obfuscator = new SimpleObfuscator();
            SecureString outputSecureString
                = obfuscator.DeObfuscate(txtObfuscatedPassword.Text);
            _deobfuscatedSecureString = outputSecureString;
        }               

        
        /// <summary>
        /// Converts the Deobfuscated SecureString password to a string and displays 
        /// in the text box. Only works in debug as the AsString extension will not
        /// appear in released code.
        /// </summary>
        private void btnRevealDeobfuscate_Click(object sender, EventArgs e)
        {   
            #if DEBUG
            string plainText = _deobfuscatedSecureString.AsString();
            txtDeobfuscatedPassword.Text = plainText;
            #endif
        }
        

    }
    
}
