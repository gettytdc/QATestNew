using BluePrism.BPServer.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using BluePrism.BPServer.UrlReservations;
using BluePrism.BPServer.WindowsServices;
using BluePrism.Core.HttpConfiguration;
using BluePrism.Core.WindowsSecurity;
using NLog;

namespace BluePrism.BPServer
{
    /// <summary>
    /// Form for managing URL reservations for user accounts that run Windows
    /// services for a particular server configuration
    /// </summary>
    public partial class frmManageUrlReservations : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Details of the binding URL for the configuration being edited
        /// </summary>
        private readonly BindingProperties _binding;

        /// <summary>
        /// The Windows services currently set up
        /// </summary>
        private readonly List<WindowsServiceInfo> _currentServices;
        
        private readonly HttpConfigurationService _configurationService = new HttpConfigurationService();

        /// <summary>
        /// Creates a new instance of the form
        /// </summary>
        /// <param name="binding">Contains details of the server address</param>
        /// <param name="currentServices">Windows services that apply to the configuration</param>
        public frmManageUrlReservations(BindingProperties binding, List<WindowsServiceInfo> currentServices)
        {
            InitializeComponent();
            _binding = binding;
            _currentServices = currentServices;
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            lblIntroduction.Text += " " + _binding.BindingReservationUrl;
            DisplayUrlReservations();
        }

        /// <summary>
        /// Loads and displays list of URL reservations
        /// </summary>
        private void DisplayUrlReservations()
        {
            var all = _configurationService.GetUrlReservations();
            string url = _binding.BindingReservationUrl;
            var related = all
                .Select(x => new
                {
                    UrlReservation = x, 
                    MatchType = UrlReservationMatcher.Compare(url, x.Url)
                })
                .Where(x => x.MatchType != UrlReservationMatchType.None);

            var items2 = related
                .Select(x => UrlReservationGridItem.MapFrom(x.UrlReservation, x.MatchType))
                .ToList();

            dgvUrlReservations.AutoGenerateColumns = false;
            dgvUrlReservations.DataSource = items2;
            dgvUrlReservations.Refresh();
            dgvUrlReservations.ClearSelection();

            llEdit.Enabled = related.Any();
            llDelete.Enabled = related.Any();
        }

        /// <summary>
        /// Gets possible URL prefixes that will identify any URL reservations that 
        /// apply to or conflict with the configuration's binding address and port
        /// </summary>
        private List<string> GetPossibleUrls()
        {
            var urls = new List<string>
            {
                _binding.BindingReservationUrl
            };
            if (!_binding.IsWildcardAddress)
            {
                urls.Add(_binding.WildcardReservationUrl);
            }

            // URL reservations using http will conflict with any that need to use https
            // and vice versa
            var oppositeProtocolBinding = new BindingProperties(_binding.Address, _binding.Port, !_binding.Secure);
            urls.Add(oppositeProtocolBinding.BindingReservationUrl);
            if (!oppositeProtocolBinding.IsWildcardAddress)
            {
                urls.Add(oppositeProtocolBinding.WildcardReservationUrl);   
            }

            return urls;
        }


        /// <summary>
        /// Disable the edit link label when there is a row selected that has conflicts.
        /// This is because you cannot make changes to the address of the reservation,
        /// so cannot resolve the conflicts by editing. Instead you would need to click
        /// the New link label to overwrite the conflicting reservation.
        /// </summary>
        private void HandleGridSelectionChanged(object sender, EventArgs e)
        {
            if (dgvUrlReservations.SelectedRows.Count == 0)
            {
                llEdit.Enabled = true;
            }
            else
            {
                UrlReservationGridItem currentRow = (UrlReservationGridItem)dgvUrlReservations.CurrentRow.DataBoundItem;
                llEdit.Enabled = (currentRow.MatchType != UrlReservationMatchType.Conflict);
            }
            
        }

        /// <summary>
        /// Opens dialogue to create a new URL reservation
        /// </summary>
        private void HandleAdd(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!_currentServices.Any())
            {
                MessageBox.Show(this, Resources.UnableToCreateURLPermissionAsThereAreNoServicesWithUsersThatRequirePermissions,
                    Resources.CannotAddPermissions, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            DialogResult result;
            using (var form = new frmEditUrlReservation(_binding, _currentServices, null))
            {
                result = form.ShowDialog(this);
            }
            if (result == DialogResult.OK)
            {
                DisplayUrlReservations();
            }
        }

        /// <summary>
        /// Opens dialogue to update an editing URL reservation
        /// </summary>
        private void HandleEdit(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var urlReservation = EnsureSelectedUrlReservation(true);
            if (urlReservation != null)
            {
                DialogResult result;
                using (var form = new frmEditUrlReservation(_binding, _currentServices, urlReservation))
                {
                    result = form.ShowDialog(this);
                }
                if (result == DialogResult.OK)
                {
                    DisplayUrlReservations();
                }
            }
        }

        /// <summary>
        /// Deletes a URL reservation
        /// </summary>
        private void HandleDelete(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var urlReservation = EnsureSelectedUrlReservation(true);
            if (urlReservation != null)
            {
                try
                {
                    _configurationService.DeleteUrlReservation(urlReservation.Url);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error deleting URL reservation");
                    string message = Resources.AnUnexpectedErrorOccurredWhileDeletingTheURLReservationFullDetailsOfTheErrorHav;
                    MessageBox.Show(this, message,
                        Resources.ErrorDeletingURLReservation, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                DisplayUrlReservations();
            }
        }

        /// <summary>
        /// Returns the UrlReservation for the selected row in the URL 
        /// reservations listing, optionally displaying a warning dialogue if no item is
        /// selected.
        /// </summary>
        private UrlReservation EnsureSelectedUrlReservation(bool displayWarning)
        {
            if (dgvUrlReservations.SelectedRows.Count == 0)
            {
                if (displayWarning)
                {
                    MessageBox.Show(this, Resources.PleaseSelectAPermission, Resources.NoPermissionSelected, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return null;
            }
            var row = dgvUrlReservations.SelectedRows[0];
            return ((UrlReservationGridItem) row.DataBoundItem).Reservation;
        }

        #region Inner types

        /// <summary>
        /// Simple data structure used for grid - contains details about URL reservation
        /// with properties specific to UI 
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Public properties used for data binding")]
        private class UrlReservationGridItem
        {
            /// <summary>
            /// Creates instance from a UrlReservation 
            /// </summary>
            public static UrlReservationGridItem MapFrom(UrlReservation reservation, UrlReservationMatchType matchType)
            {
                var usersSummary = GetUsersSummary(reservation);
                return new UrlReservationGridItem
                {
                    Url = reservation.Url,
                    UsersSummary = usersSummary,
                    Reservation = reservation,
                    MatchType = matchType,
                    UrlDescription = 
                        string.Format("{0}{1}", 
                        reservation.Url, 
                        matchType == UrlReservationMatchType.Conflict ?  
                        Resources.ThisURLReservationConflictsWithTheCurrentURL
                        : "")
                };
            } 

            /// <summary>
            /// Formats text for the UsersSummary property
            /// </summary>
            private static string GetUsersSummary(UrlReservation reservation)
            {
                var entries = reservation.SecurityDescriptor.Entries;
                if (!entries.Any())
                {
                    return "";
                }

                var userNames = AccountSidTranslator.GetUserNames(entries.Select(x => x.Sid));
                var userDescriptions = from entry in entries
                    let userName = userNames[entry.Sid]
                    let listen = entry.AllowListen ? Resources.Yes : Resources.No
                    let @delegate = entry.AllowDelegate ? Resources.Yes : Resources.No
                    select string.Format(Resources._0Listen1Delegate2, userName, listen, @delegate);

                string usersSummary = string.Join(Environment.NewLine, userDescriptions);
                return usersSummary;
            }

            /// <summary>
            /// The URL prefix for the URL reservation
            /// </summary>
            public string Url { get; private set; }

            /// <summary>
            /// Summary of users based on the URL reservation's access control entries 
            /// </summary>
            public string UsersSummary { get; private set; }

            /// <summary>
            /// The UrlReservation on which this item is based
            /// </summary>
            public UrlReservation Reservation { get; private set; }

            /// <summary>
            /// Indicates whether the UrlReservation matches or conflicts with the binding address
            /// </summary>
            public UrlReservationMatchType MatchType { get; private set; }

            /// <summary>
            /// The URL prefix for the URL Reservation, plus a note stating if the url reservation
            /// conflicts with the url of the service,
            /// </summary>
            public string UrlDescription { get; private set; }

        }
        
        #endregion
        
    }
}
