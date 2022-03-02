namespace AutomateControlTester.Configurators
{
    partial class HighlightWindowConfigurator
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
            this.numPeriod = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnShow = new AutomateControls.Buttons.StandardStyledButton();
            this.label2 = new System.Windows.Forms.Label();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numHeight = new System.Windows.Forms.NumericUpDown();
            this.numY = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numX = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.btnShowBlocked = new AutomateControls.Buttons.StandardStyledButton();
            ((System.ComponentModel.ISupportInitialize)(this.numPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).BeginInit();
            this.SuspendLayout();
            // 
            // numPeriod
            // 
            this.numPeriod.Location = new System.Drawing.Point(64, 3);
            this.numPeriod.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPeriod.Name = "numPeriod";
            this.numPeriod.Size = new System.Drawing.Size(59, 20);
            this.numPeriod.TabIndex = 1;
            this.numPeriod.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Display (s)";
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(6, 81);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(75, 23);
            this.btnShow.TabIndex = 10;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Size";
            // 
            // numWidth
            // 
            this.numWidth.Location = new System.Drawing.Point(64, 55);
            this.numWidth.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numWidth.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(59, 20);
            this.numWidth.TabIndex = 7;
            this.numWidth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(129, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "x";
            // 
            // numHeight
            // 
            this.numHeight.Location = new System.Drawing.Point(147, 55);
            this.numHeight.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numHeight.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(59, 20);
            this.numHeight.TabIndex = 9;
            this.numHeight.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // numY
            // 
            this.numY.Location = new System.Drawing.Point(147, 29);
            this.numY.Name = "numY";
            this.numY.Size = new System.Drawing.Size(59, 20);
            this.numY.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(129, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = ",";
            // 
            // numX
            // 
            this.numX.Location = new System.Drawing.Point(64, 29);
            this.numX.Name = "numX";
            this.numX.Size = new System.Drawing.Size(59, 20);
            this.numX.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Location";
            // 
            // button1
            // 
            this.btnShowBlocked.Location = new System.Drawing.Point(87, 81);
            this.btnShowBlocked.Name = "button1";
            this.btnShowBlocked.Size = new System.Drawing.Size(98, 23);
            this.btnShowBlocked.TabIndex = 11;
            this.btnShowBlocked.Text = "Show Blocked";
            this.btnShowBlocked.UseVisualStyleBackColor = true;
            // 
            // HighlightWindowConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnShowBlocked);
            this.Controls.Add(this.numY);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numX);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numHeight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numPeriod);
            this.Name = "HighlightWindowConfigurator";
            this.Size = new System.Drawing.Size(290, 185);
            ((System.ComponentModel.ISupportInitialize)(this.numPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numPeriod;
        private System.Windows.Forms.Label label1;
        private AutomateControls.Buttons.StandardStyledButton btnShow;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.NumericUpDown numY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.Label label5;
        private AutomateControls.Buttons.StandardStyledButton btnShowBlocked;
    }
}
