namespace AutomateControls.Forms
{
    partial class YesNoCheckboxPopupForm
    {
        #region Properties
        /// <summary>
        /// Required designer variable.
        /// </summary>
        internal System.ComponentModel.IContainer Components = null;
                
        internal System.Windows.Forms.FlowLayoutPanel BorderPanel;
        internal System.Windows.Forms.Label TitleLabel;
        internal System.Windows.Forms.TableLayoutPanel LayoutPanel;
        internal AutomateControls.Buttons.StandardStyledButton YesButton;
        internal AutomateControls.Buttons.StandardStyledButton NoButton;
        internal System.Windows.Forms.Panel OptionsPanel;
        internal System.Windows.Forms.Label MessageLabel;
        internal System.Windows.Forms.CheckBox CheckBox;
        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (Components != null))
            {
                Components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BorderPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.LayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.OptionsPanel = new System.Windows.Forms.Panel();
            this.CheckBox = new System.Windows.Forms.CheckBox();
            this.YesButton = new AutomateControls.Buttons.StandardStyledButton(this.components);
            this.NoButton = new AutomateControls.Buttons.StandardStyledButton(this.components);
            this.BorderPanel.SuspendLayout();
            this.LayoutPanel.SuspendLayout();
            this.OptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.BackColor = System.Drawing.Color.White;
            this.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BorderPanel.Controls.Add(this.LayoutPanel);
            this.BorderPanel.Location = new System.Drawing.Point(0, 0);
            this.BorderPanel.Margin = new System.Windows.Forms.Padding(0);
            this.BorderPanel.Name = "BorderPanel";
            this.BorderPanel.Size = new System.Drawing.Size(533, 222);
            this.BorderPanel.TabIndex = 21;
            // 
            // LayoutPanel
            // 
            this.LayoutPanel.ColumnCount = 3;
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.LayoutPanel.Controls.Add(this.TitleLabel, 1, 0);
            this.LayoutPanel.Controls.Add(this.MessageLabel, 1, 1);
            this.LayoutPanel.Controls.Add(this.OptionsPanel, 1, 3);
            this.LayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.LayoutPanel.Name = "LayoutPanel";
            this.LayoutPanel.RowCount = 4;
            this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.LayoutPanel.Size = new System.Drawing.Size(529, 211);
            this.LayoutPanel.TabIndex = 22;
            this.LayoutPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LayoutPanel_MouseDown);
            this.LayoutPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LayoutPanel_MouseMove);
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.TitleLabel.Location = new System.Drawing.Point(20, 10);
            this.TitleLabel.Margin = new System.Windows.Forms.Padding(0, 10, 10, 10);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(54, 30);
            this.TitleLabel.TabIndex = 0;
            this.TitleLabel.Text = "Title";
            // 
            // MessageLabel
            // 
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageLabel.Location = new System.Drawing.Point(20, 58);
            this.MessageLabel.Margin = new System.Windows.Forms.Padding(0, 0, 10, 20);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(479, 17);
            this.MessageLabel.TabIndex = 19;
            this.MessageLabel.Text = "Message";
            // 
            // OptionsPanel
            // 
            this.OptionsPanel.AutoSize = true;
            this.OptionsPanel.Controls.Add(this.CheckBox);
            this.OptionsPanel.Controls.Add(this.YesButton);
            this.OptionsPanel.Controls.Add(this.NoButton);
            this.OptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OptionsPanel.Location = new System.Drawing.Point(23, 107);
            this.OptionsPanel.Name = "OptionsPanel";
            this.OptionsPanel.Size = new System.Drawing.Size(483, 101);
            this.OptionsPanel.TabIndex = 23;
            // 
            // CheckBox
            // 
            this.CheckBox.AutoSize = true;
            this.CheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.CheckBox.Location = new System.Drawing.Point(0, 79);
            this.CheckBox.Name = "CheckBox";
            this.CheckBox.Padding = new System.Windows.Forms.Padding(2, 0, 0, 5);
            this.CheckBox.Size = new System.Drawing.Size(483, 22);
            this.CheckBox.TabIndex = 7;
            this.CheckBox.Text = "CheckBox";
            this.CheckBox.UseVisualStyleBackColor = true;
            this.CheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // YesButton
            // 
            this.YesButton.BackColor = System.Drawing.Color.White;
            this.YesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.YesButton.ForeColor = System.Drawing.Color.Black;
            this.YesButton.Location = new System.Drawing.Point(1, 0);
            this.YesButton.Margin = new System.Windows.Forms.Padding(0, 10, 60, 3);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(209, 32);
            this.YesButton.TabIndex = 6;
            this.YesButton.Text = global::AutomateControls.Properties.Resources.FrmYesNo_Yes;
            this.YesButton.UseVisualStyleBackColor = false;
            this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
            // 
            // NoButton
            // 
            this.NoButton.BackColor = System.Drawing.Color.White;
            this.NoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NoButton.Location = new System.Drawing.Point(275, 0);
            this.NoButton.Margin = new System.Windows.Forms.Padding(0, 10, 0, 3);
            this.NoButton.Name = "NoButton";
            this.NoButton.Size = new System.Drawing.Size(208, 32);
            this.NoButton.TabIndex = 5;
            this.NoButton.Text = global::AutomateControls.Properties.Resources.FrmYesNo_No;
            this.NoButton.UseVisualStyleBackColor = false;
            this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
            // 
            // YesNoCheckboxPopupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(533, 222);
            this.Controls.Add(this.BorderPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "YesNoCheckboxPopupForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "YesNoPopupForm";
            this.BorderPanel.ResumeLayout(false);
            this.LayoutPanel.ResumeLayout(false);
            this.LayoutPanel.PerformLayout();
            this.OptionsPanel.ResumeLayout(false);
            this.OptionsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.ComponentModel.IContainer components;
    }
}
