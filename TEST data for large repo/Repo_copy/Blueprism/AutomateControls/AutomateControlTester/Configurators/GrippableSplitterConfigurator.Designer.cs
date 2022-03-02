namespace AutomateControlTester.Configurators
{
    partial class GrippableSplitterConfigurator
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
            this.gripper = new AutomateControls.GrippableSplitContainer();
            this.innerGripper = new AutomateControls.GrippableSplitContainer();
            this.evenInnererGripper = new AutomateControls.GrippableSplitContainer();
            this.gripper.Panel2.SuspendLayout();
            this.gripper.SuspendLayout();
            this.innerGripper.Panel1.SuspendLayout();
            this.innerGripper.SuspendLayout();
            this.evenInnererGripper.SuspendLayout();
            this.SuspendLayout();
            // 
            // gripper
            // 
            this.gripper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gripper.Location = new System.Drawing.Point(0, 0);
            this.gripper.Name = "gripper";
            // 
            // gripper.Panel1
            // 
            this.gripper.Panel1.BackColor = System.Drawing.Color.Tomato;
            // 
            // gripper.Panel2
            // 
            this.gripper.Panel2.Controls.Add(this.innerGripper);
            this.gripper.Size = new System.Drawing.Size(459, 312);
            this.gripper.SplitterDistance = 153;
            this.gripper.TabIndex = 0;
            this.gripper.TabStop = false;
            // 
            // innerGripper
            // 
            this.innerGripper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerGripper.Location = new System.Drawing.Point(0, 0);
            this.innerGripper.Name = "innerGripper";
            this.innerGripper.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // innerGripper.Panel1
            // 
            this.innerGripper.Panel1.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.innerGripper.Panel1.Controls.Add(this.evenInnererGripper);
            // 
            // innerGripper.Panel2
            // 
            this.innerGripper.Panel2.BackColor = System.Drawing.Color.Thistle;
            this.innerGripper.Size = new System.Drawing.Size(302, 312);
            this.innerGripper.SplitLineColor = System.Drawing.SystemColors.MenuHighlight;
            this.innerGripper.SplitLineMode = AutomateControls.GrippableSplitLineMode.Double;
            this.innerGripper.SplitterDistance = 160;
            this.innerGripper.TabIndex = 0;
            this.innerGripper.TabStop = false;
            // 
            // evenInnererGripper
            // 
            this.evenInnererGripper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.evenInnererGripper.GripVisible = false;
            this.evenInnererGripper.Location = new System.Drawing.Point(0, 0);
            this.evenInnererGripper.Name = "evenInnererGripper";
            // 
            // evenInnererGripper.Panel2
            // 
            this.evenInnererGripper.Panel2.BackColor = System.Drawing.Color.Azure;
            this.evenInnererGripper.Size = new System.Drawing.Size(302, 160);
            this.evenInnererGripper.SplitLineMode = AutomateControls.GrippableSplitLineMode.Single;
            this.evenInnererGripper.SplitterDistance = 100;
            this.evenInnererGripper.TabIndex = 0;
            this.evenInnererGripper.TabStop = false;
            // 
            // GrippableSplitterConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gripper);
            this.Name = "GrippableSplitterConfigurator";
            this.Size = new System.Drawing.Size(459, 312);
            this.gripper.Panel2.ResumeLayout(false);
            this.gripper.ResumeLayout(false);
            this.innerGripper.Panel1.ResumeLayout(false);
            this.innerGripper.ResumeLayout(false);
            this.evenInnererGripper.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AutomateControls.GrippableSplitContainer gripper;
        private AutomateControls.GrippableSplitContainer innerGripper;
        private AutomateControls.GrippableSplitContainer evenInnererGripper;
    }
}
