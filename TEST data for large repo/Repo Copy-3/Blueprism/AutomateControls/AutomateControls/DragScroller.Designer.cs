namespace AutomateControls
{
    partial class DragScroller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerScroll = new System.Windows.Forms.Timer(this.components);
            // 
            // timerScroll
            // 
            this.timerScroll.Tick += new System.EventHandler(this.HandleTimerTick);

        }

        #endregion

        private System.Windows.Forms.Timer timerScroll;
    }
}
