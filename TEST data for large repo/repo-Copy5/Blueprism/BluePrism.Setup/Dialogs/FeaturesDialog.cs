using BluePrism.Setup.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.Forms;
using BluePrism.Core.Utility;

namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The logical equivalent of the standard Features dialog. Though it implement slightly
    /// different user experience as it has checkboxes bound to the features instead of icons context menu
    /// as MSI dialog has.
    /// </summary>
    public partial class FeaturesDialog : BaseDialog
    {
        /*https://msdn.microsoft.com/en-us/library/aa367536(v=vs.85).aspx
         * ADDLOCAL - list of features to install
         * REMOVE - list of features to uninstall
         * ADDDEFAULT - list of features to set to their default state
         * REINSTALL - list of features to repair*/

        private FeatureItem[] features;
        /// <summary>
        /// The collection of the features selected by user as the features to be installed.
        /// </summary>
        public static List<string> UserSelectedItems { get; set; }
        private bool _isAutoCheckingActive = false;
        private readonly List<string> _disabledNodes = new List<string>();
        private Helpers _helpers;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturesDialog"/> class.
        /// </summary>
        public FeaturesDialog()
        {
            InitializeComponent();
        }

        void FeaturesDialog_Load(object sender, System.EventArgs e)
        {
            //resize the subtitle to accomodate the longest subtitle whilst maintaining look and feel
            _helpers = new Helpers(Shell);
            _helpers.ApplySubtitleAppearance(Subtitle);

            BuildFeaturesHierarchy();

            ResetLayout();
        }

        void ResetLayout()
        {
            featuresTree.Nodes[0].EnsureVisible();

            var helpers = new Helpers(Shell);
            NextButton.Text = Properties.Resources.Install;
            NextButton.Image = helpers.GetUacShield(16);
        }

        void BuildFeaturesHierarchy()
        {
            featuresTree.ShowNodeToolTips = true;
            features = Runtime.Session.Features;

            //build the hierarchy tree
            var rootItems = features.Where(x => x.ParentName.IsEmpty())
                                    .OrderBy(x => x.RawDisplay)
                                    .ToArray();

            var itemsToProcess = new Queue<FeatureItem>(rootItems); //features to find the children for

            while (itemsToProcess.Any())
            {
                var item = itemsToProcess.Dequeue();
                //create the view of the feature
                var view = new ReadOnlyTreeNode
                {
                    Text = Properties.Resources.ResourceManager.GetString(item.Title),
                    Tag = item, //link view to model
                    IsReadOnly = item.DisallowAbsent,
                    DefaultChecked = item.DefaultIsToBeInstalled(),
                    Checked = FeatureChecked(item)
                };

                if (item.Name == "CitrixDriver" && !ShouldEnableCitrix(view))
                {
                    view.ToolTipText = Properties.Resources.ResourceManager.GetString("CitrixIsNotInstalledOnThisMachine");
                }

                item.View = view;

                if (item.Parent != null && item.Display != FeatureDisplay.hidden)
                {
                    (item.Parent.View as TreeNode).Nodes.Add(view); //link child view to parent view
                }

                // even if the item is hidden process all its children so the correct hierarchy is established

                // find all children
                features.Where(x => x.ParentName == item.Name)
                        .ForEach(c =>
                                 {
                                     c.Parent = item; //link child model to parent model
                                     itemsToProcess.Enqueue(c); //schedule for further processing
                                 });

                if (UserSelectedItems != null)
                {
                    view.Checked = UserSelectedItems.Contains((view.Tag as FeatureItem).Name);
                }

                if (item.Display == FeatureDisplay.expand)
                {
                    view.Expand();
                }
            }

            //add views to the treeView control
            rootItems.Where(x => x.Display != FeatureDisplay.hidden)
                     .Select(x => x.View)
                     .Cast<ReadOnlyTreeNode>()
                     .ForEach(node => featuresTree.Nodes.Add(node));

            RemoveReadonlyCheckboxes();

            _isAutoCheckingActive = true;
        }

        bool ShouldEnableCitrix(ReadOnlyTreeNode view)
        {
            if (string.IsNullOrWhiteSpace(_helpers.GetCitrixInstallFolder()))
            {
                view.ForeColor = System.Drawing.Color.DarkGray;
                _disabledNodes.Add("CitrixDriver");
                return false;
            }
            return true;
        }
       
        bool FeatureChecked(FeatureItem item)
        {
            if (item.Title != "OutlookAutomation")
            {
                return item.DefaultIsToBeInstalled();
            }

            var officeType = Type.GetTypeFromProgID("Outlook.Application");

            return officeType != null;
        }

        void SaveUserSelection()
        {
            UserSelectedItems = features.Where(x => x.IsViewChecked())
                                        .Select(x => x.Name)
                                        .ToList();
        }

        protected override void Back_Click(object sender, System.EventArgs e)
        {
            SaveUserSelection();
            Shell.GoPrev();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            var itemsToInstall = features.Where(x => x.ShouldInstall())
                                        .Select(x => x.Name)
                                        .Join(",");

            var itemsToRemove = features.Where(x => !x.ShouldInstall())
                                           .Select(x => x.Name)
                                           .Join(",");

            if (itemsToRemove.Any())
            {
                Runtime.Session["REMOVE"] = itemsToRemove;
            }

            if (itemsToInstall.Any())
            {
                Runtime.Session["ADDLOCAL"] = itemsToInstall;
            }

            SaveUserSelection();
            Shell.GoNext();
        }

        void FeaturesTree_AfterCheck(object sender, TreeViewEventArgs e)
        {

            if (_isAutoCheckingActive)
            {
                _isAutoCheckingActive = false;
                                
                if (_disabledNodes.Contains(((FeatureItem)e.Node.Tag).Name))
                {
                    e.Node.Checked = ((FeatureItem)e.Node.Tag).DefaultIsToBeInstalled();
                }

                var newState = e.Node.Checked;
                var queue = new Queue<TreeNode>();
                queue.EnqueueRange(e.Node.Nodes.ToArray());

                while (queue.Any())
                {
                    var node = queue.Dequeue();
                    node.Checked = newState;
                    queue.EnqueueRange(node.Nodes.ToArray());
                }

                if (e.Node.Checked)
                {
                    var parent = e.Node.Parent as ReadOnlyTreeNode;
                    while (parent != null)
                    {
                        if (!parent.IsReadOnly)
                        {
                            parent.Checked = true;
                        }

                        parent = parent.Parent as ReadOnlyTreeNode;
                    }
                }

                _isAutoCheckingActive = true;
            }
        }
        private void ResetButton_Click(object sender, EventArgs e)
        {
            _isAutoCheckingActive = false;
            foreach (var node in features)
            {
                node.ResetViewChecked();
            }

            RemoveReadonlyCheckboxes();

            _isAutoCheckingActive = true;
        }

        private void DiskUsageButton_Click(object sender, EventArgs e)
        {
            var installDirProperty = Runtime.Session.Property("WixSharp_UI_INSTALLDIR");
            var installDirPropertyValue = Runtime.Session.Property(installDirProperty);
            var installCost = Runtime.Session.Property("INSTALL_COST");

            using (var lcf = new DiskUsageForm(Shell, installDirPropertyValue, installCost))
            {
                lcf.StartPosition = FormStartPosition.CenterParent;
                lcf.ShowDialog(owner: this);
            }
        }

        private void RemoveReadonlyCheckboxes()
        {
            foreach (ReadOnlyTreeNode node in featuresTree.Nodes)
            {
                if (node.IsReadOnly)
                {
                    featuresTree.RemoveCheckbox(node);
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Process.Start($"{Constants.BluePrismHelp}/bp-{Constants.BluePrismVersion}/{GetHelpDocumentationCulture()}/advanced-installation.htm");
        }

        private static string GetHelpDocumentationCulture()
        {
            var parentUICulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (parentUICulture.Equals("fr"))
            {
                return "fr-fr";
            }

            if (parentUICulture.Equals("zh"))
            {
                return "zh-hans";
            }

            if (parentUICulture.Equals("ja"))
            {
                return "ja-jp";
            }

            if (parentUICulture.Equals("de"))
            {
                return "de-de";
            }

            if (CultureHelper.IsLatinAmericanSpanish())
            {
                return CultureHelper.LatinAmericanSpanishHelpCode;
            }

            return "en-us";
        }
    }
}
