using BluePrism.BPServer.Properties;
namespace BluePrism.BPServer
{
    partial class frmEditUrlReservation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEditUrlReservation));
            this.gbAddress = new System.Windows.Forms.GroupBox();
            this.rbNoBinding = new AutomateControls.StyledRadioButton();
            this.rbSpecificAddress = new AutomateControls.StyledRadioButton();
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.btnSave = new AutomateControls.Buttons.StandardStyledButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.clbUserNames = new System.Windows.Forms.CheckedListBox();
            this.gbAddress.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAddress
            // 
            this.gbAddress.Controls.Add(this.rbNoBinding);
            this.gbAddress.Controls.Add(this.rbSpecificAddress);
            resources.ApplyResources(this.gbAddress, "gbAddress");
            this.gbAddress.Name = "gbAddress";
            this.gbAddress.TabStop = false;
            // 
            // rbNoBinding
            // 
            this.rbNoBinding.ButtonHeight = 21;
            this.rbNoBinding.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.rbNoBinding.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.rbNoBinding.FocusDiameter = 16;
            this.rbNoBinding.FocusThickness = 3;
            this.rbNoBinding.FocusYLocation = 9;
            this.rbNoBinding.ForceFocus = true;
            this.rbNoBinding.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.rbNoBinding.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.rbNoBinding, "rbNoBinding");
            this.rbNoBinding.MouseLeaveColor = System.Drawing.Color.White;
            this.rbNoBinding.Name = "rbNoBinding";
            this.rbNoBinding.RadioButtonDiameter = 12;
            this.rbNoBinding.RadioButtonThickness = 2;
            this.rbNoBinding.RadioYLocation = 7;
            this.rbNoBinding.StringYLocation = 1;
            this.rbNoBinding.TabStop = true;
            this.rbNoBinding.TextColor = System.Drawing.Color.Black;
            this.rbNoBinding.UseVisualStyleBackColor = true;
            // 
            // rbSpecificAddress
            // 
            this.rbSpecificAddress.ButtonHeight = 21;
            this.rbSpecificAddress.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.rbSpecificAddress.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.rbSpecificAddress.FocusDiameter = 16;
            this.rbSpecificAddress.FocusThickness = 3;
            this.rbSpecificAddress.FocusYLocation = 9;
            this.rbSpecificAddress.ForceFocus = true;
            this.rbSpecificAddress.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.rbSpecificAddress.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.rbSpecificAddress, "rbSpecificAddress");
            this.rbSpecificAddress.MouseLeaveColor = System.Drawing.Color.White;
            this.rbSpecificAddress.Name = "rbSpecificAddress";
            this.rbSpecificAddress.RadioButtonDiameter = 12;
            this.rbSpecificAddress.RadioButtonThickness = 2;
            this.rbSpecificAddress.RadioYLocation = 7;
            this.rbSpecificAddress.StringYLocation = 1;
            this.rbSpecificAddress.TabStop = true;
            this.rbSpecificAddress.TextColor = System.Drawing.Color.Black;
            this.rbSpecificAddress.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.HandleSave);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.clbUserNames);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // clbUserNames
            // 
            this.clbUserNames.CheckOnClick = true;
            this.clbUserNames.FormattingEnabled = true;
            resources.ApplyResources(this.clbUserNames, "clbUserNames");
            this.clbUserNames.Name = "clbUserNames";
            this.clbUserNames.SelectedIndexChanged += new System.EventHandler(this.clbUserNames_SelectedIndexChanged);
            // 
            // frmEditUrlReservation
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;           
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbAddress);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEditUrlReservation";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.gbAddress.ResumeLayout(false);
            this.gbAddress.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbAddress;
        private AutomateControls.StyledRadioButton rbNoBinding;
        private AutomateControls.StyledRadioButton rbSpecificAddress;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private AutomateControls.Buttons.StandardStyledButton btnSave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox clbUserNames;
    }
}