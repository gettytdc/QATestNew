namespace BluePrism.ExternalLoginBrowser.Form
{
    partial class NavigationButton
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigationButton));
            this.panNavigationButtonContainer = new System.Windows.Forms.TableLayoutPanel();
            this.Label = new System.Windows.Forms.Label();
            this.Image = new System.Windows.Forms.PictureBox();
            this.panNavigationButtonContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Image)).BeginInit();
            this.SuspendLayout();
            // 
            // panNavigationButtonContainer
            // 
            resources.ApplyResources(this.panNavigationButtonContainer, "panNavigationButtonContainer");
            this.panNavigationButtonContainer.BackColor = System.Drawing.Color.White;
            this.panNavigationButtonContainer.Controls.Add(this.Label, 1, 0);
            this.panNavigationButtonContainer.Controls.Add(this.Image, 0, 0);
            this.panNavigationButtonContainer.Name = "panNavigationButtonContainer";
            // 
            // Label
            // 
            resources.ApplyResources(this.Label, "Label");
            this.Label.BackColor = System.Drawing.Color.Transparent;
            this.Label.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Label.Name = "Label";
            this.Label.Click += new System.EventHandler(this.HandleButtonClicked);
            // 
            // Image
            // 
            resources.ApplyResources(this.Image, "Image");
            this.Image.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Image.Name = "Image";
            this.Image.TabStop = false;
            this.Image.Click += new System.EventHandler(this.HandleButtonClicked);
            // 
            // NavigationButton
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panNavigationButtonContainer);
            this.Name = "NavigationButton";
            this.Load += new System.EventHandler(this.NavigationButton_Load);
            this.panNavigationButtonContainer.ResumeLayout(false);
            this.panNavigationButtonContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Image)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panNavigationButtonContainer;
        private System.Windows.Forms.Label Label;
        private System.Windows.Forms.PictureBox Image;
    }
}
