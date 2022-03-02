namespace BluePrism.CharMatching.UI
{
    partial class Shifter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Shifter));
            this.label1 = new System.Windows.Forms.Label();
            this.ttShifter = new System.Windows.Forms.ToolTip(this.components);
            this.btnPadLeft = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnShiftLeft = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnTrimLeft = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnTrimRight = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnShiftRight = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnPadRight = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnPadBottom = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnShiftDown = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnTrimBottom = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnTrimTop = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnShiftUp = new BluePrism.CharMatching.UI.ShifterButton();
            this.btnPadTop = new BluePrism.CharMatching.UI.ShifterButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnPadLeft
            // 
            resources.ApplyResources(this.btnPadLeft, "btnPadLeft");
            this.btnPadLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPadLeft.FlatAppearance.BorderSize = 0;
            this.btnPadLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnPadLeft.Name = "btnPadLeft";
            this.btnPadLeft.Operation = BluePrism.CharMatching.UI.ShiftOperation.PadLeft;
            this.btnPadLeft.UseVisualStyleBackColor = true;
            this.btnPadLeft.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnShiftLeft
            // 
            resources.ApplyResources(this.btnShiftLeft, "btnShiftLeft");
            this.btnShiftLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShiftLeft.FlatAppearance.BorderSize = 0;
            this.btnShiftLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnShiftLeft.Name = "btnShiftLeft";
            this.btnShiftLeft.Operation = BluePrism.CharMatching.UI.ShiftOperation.ShiftLeft;
            this.btnShiftLeft.UseVisualStyleBackColor = true;
            this.btnShiftLeft.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnTrimLeft
            // 
            resources.ApplyResources(this.btnTrimLeft, "btnTrimLeft");
            this.btnTrimLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrimLeft.FlatAppearance.BorderSize = 0;
            this.btnTrimLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnTrimLeft.Name = "btnTrimLeft";
            this.btnTrimLeft.Operation = BluePrism.CharMatching.UI.ShiftOperation.TrimLeft;
            this.btnTrimLeft.UseVisualStyleBackColor = true;
            this.btnTrimLeft.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnTrimRight
            // 
            resources.ApplyResources(this.btnTrimRight, "btnTrimRight");
            this.btnTrimRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrimRight.FlatAppearance.BorderSize = 0;
            this.btnTrimRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnTrimRight.Name = "btnTrimRight";
            this.btnTrimRight.Operation = BluePrism.CharMatching.UI.ShiftOperation.TrimRight;
            this.btnTrimRight.UseVisualStyleBackColor = true;
            this.btnTrimRight.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnShiftRight
            // 
            resources.ApplyResources(this.btnShiftRight, "btnShiftRight");
            this.btnShiftRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShiftRight.FlatAppearance.BorderSize = 0;
            this.btnShiftRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnShiftRight.Name = "btnShiftRight";
            this.btnShiftRight.Operation = BluePrism.CharMatching.UI.ShiftOperation.ShiftRight;
            this.btnShiftRight.UseVisualStyleBackColor = true;
            this.btnShiftRight.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnPadRight
            // 
            resources.ApplyResources(this.btnPadRight, "btnPadRight");
            this.btnPadRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPadRight.FlatAppearance.BorderSize = 0;
            this.btnPadRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnPadRight.Name = "btnPadRight";
            this.btnPadRight.Operation = BluePrism.CharMatching.UI.ShiftOperation.PadRight;
            this.btnPadRight.UseVisualStyleBackColor = true;
            this.btnPadRight.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnPadBottom
            // 
            resources.ApplyResources(this.btnPadBottom, "btnPadBottom");
            this.btnPadBottom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPadBottom.FlatAppearance.BorderSize = 0;
            this.btnPadBottom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnPadBottom.Name = "btnPadBottom";
            this.btnPadBottom.Operation = BluePrism.CharMatching.UI.ShiftOperation.PadBottom;
            this.btnPadBottom.UseVisualStyleBackColor = true;
            this.btnPadBottom.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnShiftDown
            // 
            resources.ApplyResources(this.btnShiftDown, "btnShiftDown");
            this.btnShiftDown.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShiftDown.FlatAppearance.BorderSize = 0;
            this.btnShiftDown.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnShiftDown.Name = "btnShiftDown";
            this.btnShiftDown.Operation = BluePrism.CharMatching.UI.ShiftOperation.ShiftDown;
            this.btnShiftDown.UseVisualStyleBackColor = true;
            this.btnShiftDown.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnTrimBottom
            // 
            resources.ApplyResources(this.btnTrimBottom, "btnTrimBottom");
            this.btnTrimBottom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrimBottom.FlatAppearance.BorderSize = 0;
            this.btnTrimBottom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnTrimBottom.Name = "btnTrimBottom";
            this.btnTrimBottom.Operation = BluePrism.CharMatching.UI.ShiftOperation.TrimBottom;
            this.btnTrimBottom.UseVisualStyleBackColor = true;
            this.btnTrimBottom.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnTrimTop
            // 
            resources.ApplyResources(this.btnTrimTop, "btnTrimTop");
            this.btnTrimTop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrimTop.FlatAppearance.BorderSize = 0;
            this.btnTrimTop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnTrimTop.Name = "btnTrimTop";
            this.btnTrimTop.Operation = BluePrism.CharMatching.UI.ShiftOperation.TrimTop;
            this.btnTrimTop.UseVisualStyleBackColor = true;
            this.btnTrimTop.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnShiftUp
            // 
            resources.ApplyResources(this.btnShiftUp, "btnShiftUp");
            this.btnShiftUp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShiftUp.FlatAppearance.BorderSize = 0;
            this.btnShiftUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnShiftUp.Name = "btnShiftUp";
            this.btnShiftUp.Operation = BluePrism.CharMatching.UI.ShiftOperation.ShiftUp;
            this.btnShiftUp.UseVisualStyleBackColor = true;
            this.btnShiftUp.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // btnPadTop
            // 
            resources.ApplyResources(this.btnPadTop, "btnPadTop");
            this.btnPadTop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPadTop.FlatAppearance.BorderSize = 0;
            this.btnPadTop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnPadTop.Name = "btnPadTop";
            this.btnPadTop.Operation = BluePrism.CharMatching.UI.ShiftOperation.PadTop;
            this.btnPadTop.UseVisualStyleBackColor = true;
            this.btnPadTop.Click += new System.EventHandler(this.HandleShiftButtonClick);
            // 
            // Shifter
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPadLeft);
            this.Controls.Add(this.btnShiftLeft);
            this.Controls.Add(this.btnTrimLeft);
            this.Controls.Add(this.btnTrimRight);
            this.Controls.Add(this.btnShiftRight);
            this.Controls.Add(this.btnPadRight);
            this.Controls.Add(this.btnPadBottom);
            this.Controls.Add(this.btnShiftDown);
            this.Controls.Add(this.btnTrimBottom);
            this.Controls.Add(this.btnTrimTop);
            this.Controls.Add(this.btnShiftUp);
            this.Controls.Add(this.btnPadTop);
            this.Name = "Shifter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BluePrism.CharMatching.UI.ShifterButton btnPadTop;
        private BluePrism.CharMatching.UI.ShifterButton btnShiftUp;
        private BluePrism.CharMatching.UI.ShifterButton btnTrimTop;
        private BluePrism.CharMatching.UI.ShifterButton btnPadBottom;
        private BluePrism.CharMatching.UI.ShifterButton btnShiftDown;
        private BluePrism.CharMatching.UI.ShifterButton btnTrimBottom;
        private BluePrism.CharMatching.UI.ShifterButton btnPadRight;
        private BluePrism.CharMatching.UI.ShifterButton btnShiftRight;
        private BluePrism.CharMatching.UI.ShifterButton btnTrimRight;
        private BluePrism.CharMatching.UI.ShifterButton btnPadLeft;
        private BluePrism.CharMatching.UI.ShifterButton btnShiftLeft;
        private BluePrism.CharMatching.UI.ShifterButton btnTrimLeft;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip ttShifter;
    }
}
