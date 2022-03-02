using BluePrism.CharMatching.Properties;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace BluePrism.CharMatching.UI.Designer
{
    /// <summary>
    /// Class used to provide commands for GridSpyRegions - used in the command list
    /// within the property grid
    /// </summary>
    internal class GridSpyRegionSite: ISite, IMenuCommandService
    {
        #region - Member Variables -

        private GridSpyRegion _region;
        private DesignerVerbCollection _verbs;

        #endregion

        #region - Constructors -

        public GridSpyRegionSite(GridSpyRegion region)
        {
            _region = region;
        }

        #endregion

        #region - ISite / IServiceProvider Members -

        public IComponent Component
        {
            get { return _region; }
        }

        public IContainer Container
        {
            get { return null; }
        }

        public bool DesignMode
        {
            get { return false; }
        }

        public string Name
        {
            get { return _region.Name; }
            set { _region.Name = value; }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IMenuCommandService))
                return this;
            return null;
        }

        #endregion

        #region - IMenuCommandService Members -

        #region - Not Implemented (ie. unnecessary) Methods -

        public void AddCommand(MenuCommand command)
        {
            throw new NotImplementedException();
        }

        public void AddVerb(DesignerVerb verb)
        {
            throw new NotImplementedException();
        }

        public MenuCommand FindCommand(CommandID commandID)
        {
            throw new NotImplementedException();
        }

        public bool GlobalInvoke(CommandID commandID)
        {
            throw new NotImplementedException();
        }

        public void RemoveCommand(MenuCommand command)
        {
            throw new NotImplementedException();
        }

        public void RemoveVerb(DesignerVerb verb)
        {
            throw new NotImplementedException();
        }

        public void ShowContextMenu(CommandID menuID, int x, int y)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// The collection of verbs (ie. commands) which apply to components hosted
        /// by this site.
        /// </summary>
        /// <remarks>This is the only meaningful IMenuCommandService implementation
        /// in this class - the rest aren't used by the property grid mechanisms and
        /// are there more for use elsewhere</remarks>
        public DesignerVerbCollection Verbs
        {
            get
            {
                if (_verbs == null)
                {
                    _verbs = new DesignerVerbCollection(new DesignerVerb[]{
                            new DesignerVerb(Resources.AddRow, HandleAddRow),
                            new DesignerVerb(Resources.AddColumn, HandleAddColumn),
                            new DesignerVerb(Resources.DeleteLastRow, HandleDeleteRow),
                            new DesignerVerb(Resources.DeleteLastColumn, HandleDeleteColumn),
                            new DesignerVerb(Resources.EditRowsAndColumns, HandleEdit)
                        });
                }
                return _verbs;
            }
        }

        #endregion

        #region - Command Event Handlers -

        /// <summary>
        /// Handles the 'Add Row' command being invoked
        /// </summary>
        private void HandleAddRow(object sender, EventArgs e)
        {
            _region.Schema.AddRow();
        }

        /// <summary>
        /// Handles the 'Add Column' command being invoked
        /// </summary>
        private void HandleAddColumn(object sender, EventArgs e)
        {
            _region.Schema.AddColumn();
        }

        /// <summary>
        /// Handles the 'Delete Row' command being invoked
        /// </summary>
        private void HandleDeleteRow(object sender, EventArgs e)
        {
            if (_region.Schema.RowCount > 1)
                _region.Schema.DeleteLastRow();
        }

        /// <summary>
        /// Handles the 'Delete Column' command being invoked
        /// </summary>
        private void HandleDeleteColumn(object sender, EventArgs e)
        {
            if (_region.Schema.ColumnCount > 1)
                _region.Schema.DeleteLastColumn();
        }

        /// <summary>
        /// Handles the 'Edit Rows and Columns' command being invoked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleEdit(object sender, EventArgs e)
        {
            using (GridSpyRegionSchemaEditorForm f =
                new GridSpyRegionSchemaEditorForm())
            {
                f.Schema = _region.Schema.Clone();
                if (f.ShowDialog() == DialogResult.OK)
                    _region.Schema.CopySchemaFrom(f.Schema);
            }
        }

        #endregion
    }
}
