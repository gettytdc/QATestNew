using BluePrism.BPServer.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BluePrism.BPCoreLib;
using BluePrism.BPServer.UrlReservations;
using BluePrism.BPServer.WindowsServices;
using BluePrism.Core.HttpConfiguration;
using BluePrism.Core.WindowsSecurity;
using NLog;

namespace BluePrism.BPServer
{
    /// <summary>
    /// Form used to create or update a Url reservation for user accounts that run 
    /// Windows services for a particular server configuration
    /// </summary>
    public partial class frmEditUrlReservation : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Details of the editBinding URL for the configuration being edited
        /// </summary>
        private readonly BindingProperties _binding;

        /// <summary>
        /// The Windows services that the URL reservation applies to
        /// </summary>
        private readonly List<WindowsServiceInfo> _currentServices;

        /// <summary>
        /// The existing UrlReservation being edited - this will be null if creating a new
        /// URL reservation
        /// </summary>
        private readonly UrlReservation _reservationToEdit;

        private readonly HttpConfigurationService _configurationService = new HttpConfigurationService();

        /// <summary>
        /// Creates a new frmEditUrlReservation instance
        /// </summary>
        /// <param name="binding">Details of the editBinding URL for the configuration being edited</param>
        /// <param name="currentServices">Existing Windows service details</param>
        /// <param name="reservationToEdit">The UrlReservation being edited (null if creating a new reservation)</param>
        public frmEditUrlReservation(BindingProperties binding, List<WindowsServiceInfo> currentServices, 
            UrlReservation reservationToEdit)
        {
            InitializeComponent();
            _binding = binding;
            _currentServices = currentServices;
            _reservationToEdit = reservationToEdit;
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitialiseForm();
        }

        /// <summary>
        /// Prevents item from remaining selected in username checkboxlist
        /// </summary>
        private void clbUserNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            clbUserNames.ClearSelected();
        }

        /// <summary>
        /// Initiates saving changes
        /// </summary>
        private void HandleSave(object sender, EventArgs e)
        {
            if (clbUserNames.CheckedItems.Count == 0)
            {
                MessageBox.Show(this, Resources.PleaseSelectOneOrMoreUsers, Resources.SelectUsers, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_reservationToEdit != null)
            {
                UpdateUrlReservation();
            }
            else
            {
                CreateUrlReservation();
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitialiseForm()
        {
            if (_reservationToEdit == null)
                InitialiseFormForCreate();
            else
                InitialiseFormForEdit();
        }

        private void InitialiseFormForCreate()
        {
            Text = Resources.AddURLPermissions;       
            
            if (_binding.IsWildcardAddress)
            {
                // No address given, so only allow URL reservation using wildcard address
                rbNoBinding.Text += Resources._delimiter + _binding.WildcardReservationUrl;
                rbNoBinding.Checked = true;
                rbSpecificAddress.Enabled = false;
            }
            else
            {
                // Only allow URL reservation using specific address 
                rbSpecificAddress.Text += Resources._delimiter + _binding.BindingReservationUrl;
                rbSpecificAddress.Checked = true;
                rbNoBinding.Enabled = false;
            }
            InitialiseAccountNameOptions();
        }

        /// <summary>
        /// Sets up form for editing an existing URL reservation
        /// </summary>
        private void InitialiseFormForEdit()
        {
            Text = Resources.EditURLPermissions;

            // "Borrow" the wildcard address option to show the existing URL
            rbNoBinding.Text = string.Format(Resources.ExistingURL0, _reservationToEdit.Url);
            rbNoBinding.Checked = true;
            rbNoBinding.Enabled = true;
            rbSpecificAddress.Visible = false;

            InitialiseAccountNameOptions();
        }

        /// <summary>
        /// Sets up account names available for selection during edit or create
        /// </summary>
        private void InitialiseAccountNameOptions()
        {
            var accounts = GetAccounts();
            clbUserNames.DataSource = accounts
                .Select(x => new UserAccountCheckboxItem(x))
                .ToList();
            if (_reservationToEdit != null)
            {
                // Select existing account names
                var selectedSids = _reservationToEdit.SecurityDescriptor.Entries
                    .Select(x => x.Sid);

                foreach (var account in accounts.Select((value, index) => new { value.Sid, index }))
                {
                    bool selected = selectedSids.Contains(account.Sid);
                    clbUserNames.SetItemChecked(account.index, selected);
                }
            }
        }

        /// <summary>
        /// Gets available account names to display for selection
        /// </summary>
        /// <returns>The user account identifiers available for selection
        /// </returns>
        private ISet<UserAccountIdentifier> GetAccounts()
        {
            var accounts = new HashSet<UserAccountIdentifier>();

            // Start with account names from existing reservation if editing
            if (_reservationToEdit != null)
            {
                var entries = _reservationToEdit.SecurityDescriptor.Entries;
                accounts.UnionWith(
                    entries.Select(x => UserAccountIdentifier.CreateFromSid(x.Sid))
                );
            }
            // Account names for services
            accounts.UnionWith(_currentServices.Select(x => x.UserAccount));

            return accounts;
        }


        /// <summary>
        /// Saves new URL reservation
        /// </summary>
        private void CreateUrlReservation()
        {
            string url = rbSpecificAddress.Checked 
                ? _binding.BindingReservationUrl 
                : _binding.WildcardReservationUrl;
            var sids = clbUserNames.CheckedItems.OfType<UserAccountCheckboxItem>().Select(x => x.Account.Sid);
            var entries = sids.Select(x => new AccessControlEntry(x, true, false));
            var urlReservation = new UrlReservation(url, new SecurityDescriptor(entries));
            
            // Confirm if saving new reservation will replace conflicting / matching reservations
            bool conflicts;
            var existingUrlReservations = FindMatchingOrConflictingUrlReservations(url, out conflicts).ToList();
            if (existingUrlReservations.Any())
            {
                var message = new StringBuilder();
                message.AppendLine(Resources.ThisWillReplaceTheFollowingExistingURLPermissions);
                message.AppendLine();
                existingUrlReservations.ForEach(x => message.AppendLine(x.Url));
                message.AppendLine();
                if (conflicts)
                {
                    message.AppendLine(Resources.NoteThatItIsNotPossibleToHaveSeparatePermissionsForHttpAndHttpsURLsUsingAnyOfTh);
                }
                message.AppendLine();
                message.AppendLine(Resources.AreYouSureYouWishToContinue);
                message.AppendLine();
                message.Append(Resources.ClickYesToProceedOrNoToCancel);
                var result = MessageBox.Show(this, message.ToString(), Resources.ReplaceURLPermissions, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }
            
            // Add URL reservation
            try
            {
                existingUrlReservations.ForEach(x => _configurationService.DeleteUrlReservation(x.Url));
                _configurationService.AddUrlReservation(urlReservation);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error saving URL reservations");

                DialogResult = DialogResult.Cancel;
                string message = Resources.AnUnexpectedErrorOccuredWhileSavingTheURLPermissionsFullDetailsOfTheErrorHaveBe;
                MessageBox.Show(this, message, Resources.ErrorSavingURLPermissions, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Saves changes to existing URL reservation
        /// </summary>
        private void UpdateUrlReservation()
        {
            var sids = clbUserNames.CheckedItems.OfType<UserAccountCheckboxItem>().Select(x => x.Account.Sid);
            var existingEntries = _reservationToEdit.SecurityDescriptor.Entries;
            // Use existing ACEs if available (in case manually set up with different permissions)
            var entries = sids.Select(sid =>
            {
                var existing = existingEntries.FirstOrDefault(e => e.Sid == sid);
                if (existing != null)
                {
                    return new AccessControlEntry(sid, true, existing.AllowDelegate);
                }
                return new AccessControlEntry(sid, true, false);
            });
            var urlReservation = new UrlReservation(_reservationToEdit.Url, new SecurityDescriptor(entries));
            try
            {
                _configurationService.AddUrlReservation(urlReservation);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error updating URL reservations");

                DialogResult = DialogResult.Cancel;
                string message = Resources.AnUnexpectedErrorOccuredWhileUpdatingTheURLPermissionsFullDetailsOfTheErrorHave;
                MessageBox.Show(this, message, Resources.ErrorUpdatingURLPermissions, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads existing URL reservations and returns any that would be replaced by a
        /// new reservation using the URL specified (exact matches or conflicting URLs).
        /// Used when creating a new reservation to test the URL that will be used.
        /// </summary>
        /// <param name="url">The reservation URL to compare</param>
        /// <param name="conflicts">Indicates whether any of the existing URL reservations
        /// conflict with the new URL</param>
        /// <returns>A list of matching / conflicting URL reservations</returns>
        private IEnumerable<UrlReservation> FindMatchingOrConflictingUrlReservations(string url, out bool conflicts)
        {
            var matchingOrConflicting = _configurationService.GetUrlReservations()
                .Select(x => new { UrlReservation = x, MatchType = UrlReservationMatcher.Compare(url, x.Url) })
                .Where(x => x.MatchType == UrlReservationMatchType.ExactMatch || x.MatchType == UrlReservationMatchType.Conflict);
            conflicts = matchingOrConflicting.Any(x => x.MatchType == UrlReservationMatchType.Conflict);
            return matchingOrConflicting.Select(x => x.UrlReservation);
        }

        #region Inner types

        /// <summary>
        /// Wrapper for UserAccount that is used to control labels displayed 
        /// in the CheckedListBox control
        /// </summary>
        private class UserAccountCheckboxItem
        {
            public UserAccountCheckboxItem(UserAccountIdentifier account)
            {
                Account = account;
            }

            public UserAccountIdentifier Account { get; private set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Account.Name, Account.Sid);
            }
        }

        #endregion
    }
}
