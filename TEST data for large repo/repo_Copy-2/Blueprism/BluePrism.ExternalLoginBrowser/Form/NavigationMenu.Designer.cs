namespace BluePrism.ExternalLoginBrowser.Form
{
    partial class NavigationMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigationMenu));
            this.panNavigationMenuContainer = new System.Windows.Forms.TableLayoutPanel();
            this.panNavigationButtonsRightContainer = new System.Windows.Forms.TableLayoutPanel();
            this.btnCloseForm = new System.Windows.Forms.PictureBox();
            this.btnResizeForm = new System.Windows.Forms.PictureBox();
            this.NavigationMenuLoading = new System.Windows.Forms.TableLayoutPanel();
            this.panNavigationMenuContainer.SuspendLayout();
            this.panNavigationButtonsRightContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCloseForm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnResizeForm)).BeginInit();
            this.SuspendLayout();
            // 
            // panNavigationMenuContainer
            // 
            resources.ApplyResources(this.panNavigationMenuContainer, "panNavigationMenuContainer");
            this.panNavigationMenuContainer.Controls.Add(this.panNavigationButtonsRightContainer, 2, 0);
            this.panNavigationMenuContainer.Controls.Add(this.NavigationMenuLoading, 1, 0);
            this.panNavigationMenuContainer.Name = "panNavigationMenuContainer";
            // 
            // panNavigationButtonsRightContainer
            // 
            resources.ApplyResources(this.panNavigationButtonsRightContainer, "panNavigationButtonsRightContainer");
            this.panNavigationButtonsRightContainer.Controls.Add(this.btnCloseForm, 2, 0);
            this.panNavigationButtonsRightContainer.Controls.Add(this.btnResizeForm, 1, 0);
            this.panNavigationButtonsRightContainer.Name = "panNavigationButtonsRightContainer";
            // 
            // btnCloseForm
            // 
            resources.ApplyResources(this.btnCloseForm, "btnCloseForm");
            this.btnCloseForm.BackColor = System.Drawing.Color.White;
            this.btnCloseForm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCloseForm.Name = "btnCloseForm";
            this.btnCloseForm.TabStop = false;
            this.btnCloseForm.Click += new System.EventHandler(this.HandleCloseFormButtonClicked);
            // 
            // btnResizeForm
            // 
            resources.ApplyResources(this.btnResizeForm, "btnResizeForm");
            this.btnResizeForm.BackColor = System.Drawing.Color.White;
            this.btnResizeForm.BackgroundImage = global::BluePrism.ExternalLoginBrowser.Properties.Resources.form_maximise;
            this.btnResizeForm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResizeForm.Name = "btnResizeForm";
            this.btnResizeForm.TabStop = false;
            this.btnResizeForm.Click += new System.EventHandler(this.HandleFormResizeButtonClicked);
            // 
            // NavigationMenuLoading
            // 
            resources.ApplyResources(this.NavigationMenuLoading, "NavigationMenuLoading");
            this.NavigationMenuLoading.BackColor = System.Drawing.Color.White;
            this.NavigationMenuLoading.Name = "NavigationMenuLoading";
            // 
            // NavigationMenu
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panNavigationMenuContainer);
            this.Name = "NavigationMenu";
            this.panNavigationMenuContainer.ResumeLayout(false);
            this.panNavigationButtonsRightContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnCloseForm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnResizeForm)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panNavigationMenuContainer;
        private System.Windows.Forms.TableLayoutPanel panNavigationButtonsRightContainer;
        private System.Windows.Forms.PictureBox btnResizeForm;
        private System.Windows.Forms.PictureBox btnCloseForm;
        private System.Windows.Forms.TableLayoutPanel NavigationMenuLoading;
    }
}
