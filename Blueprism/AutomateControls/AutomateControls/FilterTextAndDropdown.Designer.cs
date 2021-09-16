
namespace AutomateControls
{
    partial class FilterTextAndDropdown
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
            this.filterTextBox = new AutomateControls.FilterTextBox();
            this.SuspendLayout();
            // 
            // filterTextBox1
            // 
            this.filterTextBox.AlwaysShowHandOnFarHover = true;
            this.filterTextBox.AlwaysShowHandOnNearHover = true;
            this.filterTextBox.BorderColor = System.Drawing.Color.Empty;
            this.filterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterTextBox.Location = new System.Drawing.Point(0, 0);
            this.filterTextBox.Name = "filterTextBox1";
            this.filterTextBox.Size = new System.Drawing.Size(100, 26);
            this.filterTextBox.TabIndex = 0;
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox1_TextChanged);
            // 
            // FilterTextAndDropdown
            // 
            this.Controls.Add(this.filterTextBox);
            this.Name = "FilterTextAndDropdown";
            this.Size = new System.Drawing.Size(243, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FilterTextBox filterTextBox;
    }
}
