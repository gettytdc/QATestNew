namespace AutomateControlTester.Configurators
{
    partial class SecurePasswordTextBoxConfigurator
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Security.SecureString secureString1 = new System.Security.SecureString();
            this.txtSecurePassword = new AutomateControls.SecurePasswordTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnReveal = new AutomateControls.Buttons.StandardStyledButton();
            this.txtReveal = new System.Windows.Forms.TextBox();
            this.txtObfuscatedPassword = new System.Windows.Forms.TextBox();
            this.btnObfuscate = new AutomateControls.Buttons.StandardStyledButton();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRevealDeobfuscate = new AutomateControls.Buttons.StandardStyledButton();
            this.txtDeobfuscatedPassword = new System.Windows.Forms.TextBox();
            this.btnDeobfuscate = new AutomateControls.Buttons.StandardStyledButton();
            this.SuspendLayout();
            // 
            // txtSecurePassword
            // 
            this.txtSecurePassword.AllowPasting = true;
            this.txtSecurePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSecurePassword.Location = new System.Drawing.Point(0, 3);
            this.txtSecurePassword.Name = "txtSecurePassword";
            this.txtSecurePassword.Size = new System.Drawing.Size(261, 20);
            this.txtSecurePassword.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(257, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Once the secure string has been revealed it will then appear in memory.";
            // 
            // btnReveal
            // 
            this.btnReveal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReveal.Location = new System.Drawing.Point(202, 23);
            this.btnReveal.Name = "btnReveal";
            this.btnReveal.Size = new System.Drawing.Size(58, 23);
            this.btnReveal.TabIndex = 2;
            this.btnReveal.Text = "Reveal";
            this.btnReveal.UseVisualStyleBackColor = true;
            // 
            // txtReveal
            // 
            this.txtReveal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReveal.Location = new System.Drawing.Point(0, 25);
            this.txtReveal.Name = "txtReveal";
            this.txtReveal.ReadOnly = true;
            this.txtReveal.Size = new System.Drawing.Size(196, 20);
            this.txtReveal.TabIndex = 3;
            // 
            // txtObfuscatedPassword
            // 
            this.txtObfuscatedPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtObfuscatedPassword.Location = new System.Drawing.Point(88, 88);
            this.txtObfuscatedPassword.Name = "txtObfuscatedPassword";
            this.txtObfuscatedPassword.ReadOnly = true;
            this.txtObfuscatedPassword.Size = new System.Drawing.Size(172, 20);
            this.txtObfuscatedPassword.TabIndex = 4;
            // 
            // btnObfuscate
            // 
            this.btnObfuscate.Location = new System.Drawing.Point(3, 86);
            this.btnObfuscate.Name = "btnObfuscate";
            this.btnObfuscate.Size = new System.Drawing.Size(79, 23);
            this.btnObfuscate.TabIndex = 5;
            this.btnObfuscate.Text = "Obfuscate";
            this.btnObfuscate.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(257, 32);
            this.label2.TabIndex = 6;
            this.label2.Text = "Obfuscated password will appear in memory once deobfuscated and revealed\r\n";
            // 
            // btnRevealDeobfuscate
            // 
            this.btnRevealDeobfuscate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRevealDeobfuscate.Location = new System.Drawing.Point(209, 134);
            this.btnRevealDeobfuscate.Name = "btnRevealDeobfuscate";
            this.btnRevealDeobfuscate.Size = new System.Drawing.Size(51, 23);
            this.btnRevealDeobfuscate.TabIndex = 8;
            this.btnRevealDeobfuscate.Text = "Reveal";
            this.btnRevealDeobfuscate.UseVisualStyleBackColor = true;
            this.btnRevealDeobfuscate.Click += new System.EventHandler(this.btnRevealDeobfuscate_Click);
            // 
            // txtDeobfuscatedPassword
            // 
            this.txtDeobfuscatedPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeobfuscatedPassword.Location = new System.Drawing.Point(3, 136);
            this.txtDeobfuscatedPassword.Name = "txtDeobfuscatedPassword";
            this.txtDeobfuscatedPassword.ReadOnly = true;
            this.txtDeobfuscatedPassword.Size = new System.Drawing.Size(200, 20);
            this.txtDeobfuscatedPassword.TabIndex = 7;
            // 
            // btnDeobfuscate
            // 
            this.btnDeobfuscate.Location = new System.Drawing.Point(3, 111);
            this.btnDeobfuscate.Name = "btnDeobfuscate";
            this.btnDeobfuscate.Size = new System.Drawing.Size(79, 23);
            this.btnDeobfuscate.TabIndex = 9;
            this.btnDeobfuscate.Text = "DeObfuscate";
            this.btnDeobfuscate.UseVisualStyleBackColor = true;
            // 
            // SecurePasswordTextBoxConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnDeobfuscate);
            this.Controls.Add(this.btnRevealDeobfuscate);
            this.Controls.Add(this.txtDeobfuscatedPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnObfuscate);
            this.Controls.Add(this.txtObfuscatedPassword);
            this.Controls.Add(this.txtReveal);
            this.Controls.Add(this.btnReveal);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSecurePassword);
            this.Name = "SecurePasswordTextBoxConfigurator";
            this.Size = new System.Drawing.Size(264, 191);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AutomateControls.SecurePasswordTextBox txtSecurePassword;
        private System.Windows.Forms.Label label1;
        private AutomateControls.Buttons.StandardStyledButton btnReveal;
        private System.Windows.Forms.TextBox txtReveal;
        private System.Windows.Forms.TextBox txtObfuscatedPassword;
        private AutomateControls.Buttons.StandardStyledButton btnObfuscate;
        private System.Windows.Forms.Label label2;
        private AutomateControls.Buttons.StandardStyledButton btnRevealDeobfuscate;
        private System.Windows.Forms.TextBox txtDeobfuscatedPassword;
        private AutomateControls.Buttons.StandardStyledButton btnDeobfuscate;








    }
}
