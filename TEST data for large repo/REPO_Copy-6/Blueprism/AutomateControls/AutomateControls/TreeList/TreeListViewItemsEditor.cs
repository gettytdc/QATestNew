using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// Editor for the TreeListView.Items property
    /// </summary>
    public class TreeListViewItemsEditorForm : System.Windows.Forms.Form
    {
        private TreeListViewItemCollection _items;
        /// <summary>
        /// Get the items that are edited in this form
        /// </summary>
        public TreeListViewItemCollection Items{get{return(_items);}}
        private AutomateControls.Buttons.StandardStyledButton buttonOk;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private AutomateControls.Buttons.StandardStyledButton buttonRemove;
        private AutomateControls.Buttons.StandardStyledButton buttonAdd;
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection"></param>
        public TreeListViewItemsEditorForm(TreeListViewItemCollection collection)
        {
            InitializeComponent();
            _items = collection;
            treeView1.SelectedNode = null;
            foreach(TreeListViewItem item in _items)
            {
                TreeNode node = new TreeNode(item.Text);
                node.Tag = item;
                treeView1.Nodes.Add(node);
                AddChildren(node);
                node.Expand();
            }
        }
        private void AddChildren(TreeNode node)
        {
            TreeListViewItem tlvitem = (TreeListViewItem) node.Tag;
            foreach(TreeListViewItem item in tlvitem.Items)
            {
                TreeNode child = new TreeNode(item.Text);
                child.Tag = item;
                node.Nodes.Add(child);
                AddChildren(child);
                child.Expand();
            }
        }

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeListViewItemsEditorForm));
            this.buttonOk = new AutomateControls.Buttons.StandardStyledButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.buttonRemove = new AutomateControls.Buttons.StandardStyledButton();
            this.buttonAdd = new AutomateControls.Buttons.StandardStyledButton();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.propertyGrid1);
            this.groupBox1.Controls.Add(this.buttonRemove);
            this.groupBox1.Controls.Add(this.buttonAdd);
            this.groupBox1.Controls.Add(this.splitter1);
            this.groupBox1.Controls.Add(this.treeView1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // propertyGrid1
            // 
            resources.ApplyResources(this.propertyGrid1, "propertyGrid1");
            this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // buttonRemove
            // 
            resources.ApplyResources(this.buttonRemove, "buttonRemove");
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // splitter1
            // 
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // treeView1
            // 
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // TreeListViewItemsEditorForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonOk);
            this.Name = "TreeListViewItemsEditorForm";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonOk_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            propertyGrid1.SelectedObject = (TreeListViewItem) e.Node.Tag;
        }

        private void buttonRemove_Click(object sender, System.EventArgs e)
        {
            if(treeView1.SelectedNode == null) return;
            TreeListViewItem item = (TreeListViewItem) treeView1.SelectedNode.Tag;
            item.Remove();
            treeView1.SelectedNode.Remove();
        }

        private void buttonAdd_Click(object sender, System.EventArgs e)
        {
            try
            {
                TreeListViewItem newitem = new TreeListViewItem("treeListView" + _items.Owner.ItemsCount.ToString());
                TreeNode node = new TreeNode(newitem.Text);
                node.Tag = newitem;
                if(treeView1.SelectedNode != null)
                {
                    TreeListViewItem item = (TreeListViewItem) treeView1.SelectedNode.Tag;
                    if(item.Items.Add(newitem) > -1) treeView1.SelectedNode.Nodes.Add(node);
                }
                else
                    if(_items.Add(newitem) > -1) treeView1.Nodes.Add(node);
                if(node.Index > -1) treeView1.SelectedNode = node;
            }
            catch(Exception ex){MessageBox.Show(ex.Message);}
        }

        private void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if(treeView1.SelectedNode == null) return;
            if(e.ChangedItem.Label == "Text")
                treeView1.SelectedNode.Text = (string) e.ChangedItem.Value;
        }
    }
    /// <summary>
    /// UITypeEditor for the TreeListView.Items property
    /// </summary>
    public class TreeListViewItemsEditor : UITypeEditor
    {
        private IWindowsFormsEditorService edSvc = null;
        private TreeListViewItemsEditorForm editor = null;
        /// <summary>
        /// Constructor
        /// </summary>
        public TreeListViewItemsEditor()
        {
        }
        /// <summary>
        /// Shows a dropdown icon in the property editor
        /// </summary>
        /// <param name="context">The context of the editing control</param>
        /// <returns>Returns <c>UITypeEditorEditStyle.DropDown</c></returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// Overrides the method used to provide basic behaviour for selecting editor.
        /// Shows our custom control for editing the value.
        /// </summary>
        /// <param name="context">The context of the editing control</param>
        /// <param name="provider">A valid service provider</param>
        /// <param name="value">The current value of the object to edit</param>
        /// <returns>The new value of the object</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) 
        {
            if (context != null
                && context.Instance != null
                && provider != null) 
            {
                edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if(edSvc != null)
                {
                    editor = new TreeListViewItemsEditorForm((TreeListViewItemCollection) value);
                    editor.ShowInTaskbar = false;
                    edSvc.ShowDialog(editor);
                    if(editor.DialogResult == DialogResult.OK)
                        return(editor.Items);
                }
            }
            return(value);
        }
    }
}
