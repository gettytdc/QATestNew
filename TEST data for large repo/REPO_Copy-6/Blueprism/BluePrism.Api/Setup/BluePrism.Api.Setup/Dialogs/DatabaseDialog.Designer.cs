namespace BluePrism.Api.Setup.Dialogs
{
    partial class DatabaseDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseDialog));
            this.banner = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelTitleDetails = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this._databaseName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._trustedAuthenticationRadio = new System.Windows.Forms.RadioButton();
            this._password = new System.Windows.Forms.TextBox();
            this._sqlAuthenticationRadio = new System.Windows.Forms.RadioButton();
            this._username = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this._refreshButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._serverCombo = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // banner
            // 
            this.banner.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.banner, "banner");
            this.banner.Name = "banner";
            this.banner.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.labelTitleDetails);
            this.panel2.Controls.Add(this.labelTitle);
            this.panel2.Controls.Add(this.banner);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // labelTitleDetails
            // 
            resources.ApplyResources(this.labelTitleDetails, "labelTitleDetails");
            this.labelTitleDetails.BackColor = System.Drawing.Color.White;
            this.labelTitleDetails.Name = "labelTitleDetails";
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.BackColor = System.Drawing.Color.White;
            this.labelTitle.Name = "labelTitle";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.back);
            this.panel1.Controls.Add(this.next);
            this.panel1.Controls.Add(this.cancel);
            this.panel1.Name = "panel1";
            // 
            // back
            // 
            resources.ApplyResources(this.back, "back");
            this.back.Name = "back";
            this.back.UseVisualStyleBackColor = true;
            this.back.Click += new System.EventHandler(this.back_Click);
            // 
            // next
            // 
            resources.ApplyResources(this.next, "next");
            this.next.Name = "next";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.next_Click);
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // _databaseName
            // 
            resources.ApplyResources(this._databaseName, "_databaseName");
            this._databaseName.Name = "_databaseName";
            this._databaseName.TextChanged += new System.EventHandler(this._databaseName_TextChanged);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this._trustedAuthenticationRadio);
            this.groupBox1.Controls.Add(this._password);
            this.groupBox1.Controls.Add(this._sqlAuthenticationRadio);
            this.groupBox1.Controls.Add(this._username);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // _trustedAuthenticationRadio
            // 
            resources.ApplyResources(this._trustedAuthenticationRadio, "_trustedAuthenticationRadio");
            this._trustedAuthenticationRadio.Name = "_trustedAuthenticationRadio";
            this._trustedAuthenticationRadio.TabStop = true;
            this._trustedAuthenticationRadio.UseVisualStyleBackColor = true;
            this._trustedAuthenticationRadio.CheckedChanged += new System.EventHandler(this._trustedAuthenticationRadio_CheckedChanged);
            // 
            // _password
            // 
            resources.ApplyResources(this._password, "_password");
            this._password.Name = "_password";
            this._password.UseSystemPasswordChar = true;
            this._password.TextChanged += new System.EventHandler(this._password_TextChanged);
            // 
            // _sqlAuthenticationRadio
            // 
            resources.ApplyResources(this._sqlAuthenticationRadio, "_sqlAuthenticationRadio");
            this._sqlAuthenticationRadio.Name = "_sqlAuthenticationRadio";
            this._sqlAuthenticationRadio.TabStop = true;
            this._sqlAuthenticationRadio.UseVisualStyleBackColor = true;
            // 
            // _username
            // 
            resources.ApplyResources(this._username, "_username");
            this._username.Name = "_username";
            this._username.TextChanged += new System.EventHandler(this._username_TextChanged);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // _refreshButton
            // 
            resources.ApplyResources(this._refreshButton, "_refreshButton");
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.TabStop = false;
            this._refreshButton.UseVisualStyleBackColor = true;
            this._refreshButton.Click += new System.EventHandler(this._refreshButton_Click);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // _serverCombo
            // 
            resources.ApplyResources(this._serverCombo, "_serverCombo");
            this._serverCombo.FormattingEnabled = true;
            this._serverCombo.Name = "_serverCombo";
            this._serverCombo.TextChanged += new System.EventHandler(this._serverCombo_TextChanged);
            // 
            // DatabaseDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this._databaseName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._refreshButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._serverCombo);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "DatabaseDialog";
            this.Load += new System.EventHandler(this.dialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox banner;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelTitleDetails;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button _refreshButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _databaseName;
        private System.Windows.Forms.RadioButton _trustedAuthenticationRadio;
        private System.Windows.Forms.TextBox _password;
        private System.Windows.Forms.RadioButton _sqlAuthenticationRadio;
        private System.Windows.Forms.TextBox _username;
        private System.Windows.Forms.ComboBox _serverCombo;
    }
}
