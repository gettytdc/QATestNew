namespace AutomateControlTester.Configurators
{
    partial class FocusStealingConfigurator
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
            this.components = new System.ComponentModel.Container();
            this.spinnerStealSecs = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSteal = new AutomateControls.Buttons.StandardStyledButton();
            this.timerStealer = new System.Windows.Forms.Timer(this.components);
            this.txtEntry = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spinnerStealSecs)).BeginInit();
            this.SuspendLayout();
            // 
            // spinnerStealSecs
            // 
            this.spinnerStealSecs.Location = new System.Drawing.Point(38, 8);
            this.spinnerStealSecs.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.spinnerStealSecs.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spinnerStealSecs.Name = "spinnerStealSecs";
            this.spinnerStealSecs.Size = new System.Drawing.Size(54, 20);
            this.spinnerStealSecs.TabIndex = 0;
            this.spinnerStealSecs.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "After";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(98, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "seconds...";
            // 
            // btnSteal
            // 
            this.btnSteal.Location = new System.Drawing.Point(6, 34);
            this.btnSteal.Name = "btnSteal";
            this.btnSteal.Size = new System.Drawing.Size(148, 23);
            this.btnSteal.TabIndex = 3;
            this.btnSteal.Text = "Steal Focus";
            this.btnSteal.UseVisualStyleBackColor = true;
            this.btnSteal.Click += new System.EventHandler(this.btnSteal_Click);
            // 
            // timerStealer
            // 
            this.timerStealer.Tick += new System.EventHandler(this.timerStealer_Tick);
            // 
            // txtEntry
            // 
            this.txtEntry.Location = new System.Drawing.Point(6, 63);
            this.txtEntry.Name = "txtEntry";
            this.txtEntry.Size = new System.Drawing.Size(148, 20);
            this.txtEntry.TabIndex = 4;
            // 
            // FocusStealingConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtEntry);
            this.Controls.Add(this.btnSteal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.spinnerStealSecs);
            this.Name = "FocusStealingConfigurator";
            this.Size = new System.Drawing.Size(258, 158);
            ((System.ComponentModel.ISupportInitialize)(this.spinnerStealSecs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown spinnerStealSecs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private AutomateControls.Buttons.StandardStyledButton btnSteal;
        private System.Windows.Forms.Timer timerStealer;
        private System.Windows.Forms.TextBox txtEntry;
    }
}
