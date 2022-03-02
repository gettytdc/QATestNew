namespace BluePrism.CharMatching.UI
{
    partial class CharacterEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterEditor));
            this.charList = new BluePrism.CharMatching.UI.CharacterListPanel();
            this.shifter = new BluePrism.CharMatching.UI.Shifter();
            this.SuspendLayout();
            // 
            // charList
            // 
            this.charList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.charList, "charList");
            this.charList.MultiSelect = true;
            this.charList.Name = "charList";
            this.charList.SelectionChanged += new System.EventHandler(this.HandleSelectionChanged);
            // 
            // shifter
            // 
            resources.ApplyResources(this.shifter, "shifter");
            this.shifter.Name = "shifter";
            this.shifter.ShiftOperationClick += new BluePrism.CharMatching.UI.ShiftOperationEventHandler(this.HandleShiftOperation);
            // 
            // CharacterEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.charList);
            this.Controls.Add(this.shifter);
            this.Name = "CharacterEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private CharacterListPanel charList;
        private Shifter shifter;
    }
}
