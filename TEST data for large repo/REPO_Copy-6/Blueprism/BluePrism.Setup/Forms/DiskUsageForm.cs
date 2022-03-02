using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using WixSharp;

namespace BluePrism.Setup.Forms
{
    public partial class DiskUsageForm : Form
    {
        private const int MinimumInstallSpacePercent = 2;
        private readonly Font _driveLabelFont = new Font("Segoe UI", 20, FontStyle.Regular, GraphicsUnit.Pixel);
        private readonly Font _driveLabelFontBold = new Font("Segoe UI", 20, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly Font _labelFont = new Font("Segoe UI", 16, FontStyle.Regular, GraphicsUnit.Pixel);
        private readonly Font _warningFontBold = new Font("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly Helpers _helpers;

        private readonly Color _charcoalGreyColor = Color.FromArgb(67, 74, 79);
        private readonly Color _lightGreyColor = Color.FromArgb(212, 212, 212);
        private readonly Color _prismActionColor = Color.FromArgb(11, 117, 183);
        private readonly Color _prismWarningColor = Color.FromArgb(203, 98, 0);

        private readonly string _installPath;
        private readonly long _installSize;
        private readonly string[] _doNotWrapSubtitleForTheseLanguages = { "ja" };
        private string _errorDetails;
        private string _errorType;

        #region Constructors


        public DiskUsageForm(IManagedUIShell shell, string installPath, string installCost)
        {
            try
            {
                if (_helpers == null)
                {
                    _helpers = new Helpers(shell);
                }

                InitializeComponent();
                UpdateSubtitleForNonWrappingLanguage();
                if (shell == null)
                    throw new ArgumentNullException("Shell");

                if (!long.TryParse(installCost, out _installSize))
                {
                    //set to default
                    _installSize = 512000;
                }
                _installPath = installPath;

                PopulateDriveInfo();
            }
            catch (Exception e)
            {
                ShowError(e);
            }
        }

        private void UpdateSubtitleForNonWrappingLanguage()
        {
            if (Array.IndexOf(_doNotWrapSubtitleForTheseLanguages, CultureInfo.CurrentCulture.Parent.Name) < 0) return;
            Subtitle.Multiline = false;
            Subtitle.Font =
                _helpers.GetFontForWordAndWidth(Subtitle.Text, Subtitle.Font.Size, Subtitle.Font, Subtitle.Width);
        }

        #endregion Constructors

        #region Methods
        private int AddDrivePanelLabels(Control drivePanel, int percentFree, bool driveIsInstallDrive, bool enoughSpace)
        {
            var usedSpaceLabelWidth = Convert.ToInt32((drivePanel.Width - 2) / 100 * (100 - percentFree));
            var usedSpaceLabel = new Label
            {
                AutoSize = false,
                BackColor = driveIsInstallDrive && !enoughSpace ? _charcoalGreyColor : _lightGreyColor,
                BorderStyle = BorderStyle.None,
                Size = new Size(usedSpaceLabelWidth, 31)
            };

            drivePanel.Controls.Add(usedSpaceLabel);
            usedSpaceLabel.Location = new Point(1, 1);
            //Create the FreeSpace Label
            var freeSpaceLabelWidth = drivePanel.Width - 2 - usedSpaceLabelWidth;
            var freeSpaceLabel = new Label
            {
                AutoSize = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Size = new Size(freeSpaceLabelWidth, 31)
            };
            drivePanel.Controls.Add(freeSpaceLabel);
            freeSpaceLabel.Location = new Point(usedSpaceLabelWidth, 1);
            return usedSpaceLabelWidth;
        }

        private void AddRowLabels(bool installDrive, Control row, long freeSpace, long diskSize)
        {
            //Add the various size labels
            var requiredLabel = new Label
            {
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = _charcoalGreyColor,
                Font = _labelFont,
                Text = installDrive ? GetDiskDisplayValue(_installSize) : $"0{Properties.Resources.SizeStringMB}",
                Size = new Size(80, 33),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.BottomRight
            };
            row.Controls.Add(requiredLabel);
            requiredLabel.Location = new Point(329, 1);

            var availableLabel = new Label
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = _charcoalGreyColor,
                Font = _labelFont,
                Text = GetDiskDisplayValue(freeSpace),
                AutoSize = false,
                Size = new Size(80, 33),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.BottomRight
            };
            row.Controls.Add(availableLabel);
            availableLabel.Location = new Point(440, 1);

            var ofLabel = new Label
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = _charcoalGreyColor,
                Font = _labelFont,
                Text = Properties.Resources.DiskUsageOf,
                AutoSize = false,
                Size = new Size(39, 33),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.BottomCenter
            };
            row.Controls.Add(ofLabel);
            ofLabel.Location = new Point(520, 1);

            var diskSizeLabel = new Label
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = _charcoalGreyColor,
                Font = _labelFont,
                Text = GetDiskDisplayValue(diskSize),
                AutoSize = false,
                Size = new Size(80, 33),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.BottomRight
            };
            row.Controls.Add(diskSizeLabel);
            diskSizeLabel.Location = new Point(559, 1);
        }

        private void AddWarningPanel(Control row)
        {
            var warningPanel = new Panel
            {
                AutoSize = false,
                Size = new Size(551, 24),
                BorderStyle = BorderStyle.None,
                BackColor = _prismWarningColor,
                Padding = new Padding(2)
            };
            var warningImage = new PictureBox
            {
                Image = Properties.Resources.alert,
                Size = new Size(16, 16)
            };
            warningPanel.Controls.Add(warningImage);
            warningImage.Location = new Point(8, 4);

            var warningLabel = new Label
            {
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Text = Properties.Resources.SelectedDiskDoesNotHaveEnoughSpace,
                Font = _warningFontBold,
                Size = new Size(515, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            warningPanel.Controls.Add(warningLabel);
            warningLabel.Location = new Point(32, 0);

            row.Controls.Add(warningPanel);
            warningPanel.Location = new Point(88, 66);
        }

        private void AddDriveSpaceLabel(long diskSize, Control drivePanel, int usedSpaceLabelWidth)
        {
            var installSizeAsPercentage =
                Convert.ToInt32(Math.Floor((double)(100 / diskSize * _installSize)));
            installSizeAsPercentage = installSizeAsPercentage < MinimumInstallSpacePercent
                ? MinimumInstallSpacePercent
                : installSizeAsPercentage;

            var installSpaceLabelWidth =
                Convert.ToInt32(
                    Math.Floor((double)((drivePanel.Width - 1) / 100 * installSizeAsPercentage)));
            //Add the blue required label
            var installSpaceLabel = new Label
            {
                AutoSize = false,
                BackColor = _prismActionColor,
                BorderStyle = BorderStyle.None,
                Size = new Size(installSpaceLabelWidth, 31)
            };
            drivePanel.Controls.Add(installSpaceLabel);
            installSpaceLabel.Location = new Point(usedSpaceLabelWidth - installSpaceLabelWidth, 1);
            installSpaceLabel.BringToFront();
        }

        private void AddDriveLabel(DriveInfo drive, bool installDrive, Control row)
        {
            var driveLabel = new Label
            {
                Text = drive.Name.Substring(0, 1),
                ForeColor = _charcoalGreyColor,
                AutoSize = false,
                Size = new Size(24, 33),
                Font = installDrive ? _driveLabelFontBold : _driveLabelFont
            };
            row.Controls.Add(driveLabel);
            driveLabel.Location = new Point(41, 33);
        }

        private void AddDiskImage(bool installDrive, Control row)
        {
            //create a box around the diskImage
            var diskPanel = new Panel
            {
                AutoSize = false,
                BackColor = installDrive ? _prismActionColor : _lightGreyColor,
                Size = new Size(33, 33),
                Padding = new Padding(1, 1, 1, 1)
            };

            //Create the pictureBox with the disk icon
            var diskImage = new PictureBox
            {
                Image = Properties.Resources.disk_icon,
                Padding = new Padding(7, 10, 7, 10),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                AutoSize = false,
                Size = new Size(32, 32),
            };
            diskPanel.Controls.Add(diskImage);
            diskImage.Dock = DockStyle.Fill;
            row.Controls.Add(diskPanel);
            diskPanel.Location = new Point(1, 33);
        }
        private static string GetDiskDisplayValue(long spaceInKiloBytes)
        {
            var pass = 0;
            const int blockSize = 1024; //Bytes --> KB-->MB etc

            while (spaceInKiloBytes.ToString().Length > 4)
            {
                spaceInKiloBytes = spaceInKiloBytes / blockSize;
                pass++;
            }

            var sizeString = string.Empty;
            switch (pass)
            {
                case 0:
                    sizeString = Properties.Resources.SizeStringKB;
                    break;
                case 1:
                    sizeString = Properties.Resources.SizeStringMB;
                    break;
                case 2:
                    sizeString = Properties.Resources.SizeStringGB;
                    break;
                case 3:
                    sizeString = Properties.Resources.SizeStringTB;
                    break;
            }

            return $"{spaceInKiloBytes}{sizeString}";
        }
        private void PopulateDriveInfo()
        {
            ListPanel.Controls.Clear();

            var yPos = 16;

            var allDrives = DriveInfo.GetDrives();

            foreach (var drive in allDrives)
            {
                var driveIsInstallDrive = !string.IsNullOrWhiteSpace(_installPath) && _installPath.Substring(0, 1) == drive.Name.Substring(0, 1);
                if (drive.IsReady)
                {
                    //All Sizes need to be in kilobytes as the GetDiskDisplayValue() method will format appropriately
                    var diskSize = Convert.ToInt64(Math.Floor(drive.TotalSize / 1024.00));
                    var freeSpace = Convert.ToInt64(Math.Floor(drive.AvailableFreeSpace / 1024.00));

                    var percentFree = (int)Math.Floor((100.00 / diskSize) * freeSpace);
                    var enoughSpace = freeSpace - _installSize > 0;

                    var row = new Panel
                    {
                        Height = 90,
                        Width = 639,
                        BorderStyle = BorderStyle.None,
                        BackColor = Color.White
                    };

                    AddDiskImage(driveIsInstallDrive, row);

                    AddDriveLabel(drive, driveIsInstallDrive, row);

                    //Create the panel that will contain the drive labels
                    var drivePanel = new Panel
                    {
                        AutoSize = false,
                        Size = new Size(518, 33),
                        BorderStyle = BorderStyle.None,
                        BackColor = _lightGreyColor,
                        Padding = new Padding(2)
                    };

                    //Create the FreeSpace Label
                    var usedSpaceLabelWidth = AddDrivePanelLabels(drivePanel, percentFree, driveIsInstallDrive, enoughSpace);

                    row.Controls.Add(drivePanel);
                    drivePanel.Location = new Point(121, 33);

                    if (driveIsInstallDrive)
                    {
                        if (enoughSpace)
                        {
                            AddDriveSpaceLabel(diskSize, drivePanel, usedSpaceLabelWidth);
                        }
                        else
                        {
                            AddWarningPanel(row);
                        }
                    }

                    AddRowLabels(driveIsInstallDrive, row, freeSpace, diskSize);

                    ListPanel.Controls.Add(row);
                    row.Location = new Point(1, yPos);

                    yPos += 90;
                }
            }
        }

        private void ShowError(Exception e)
        {
            _errorType = e.GetType().ToString();
            _errorDetails = e.Message;
            //Lets show an error, first remove the base controls
            lblVolume.Visible = false;
            lblRequired.Visible = false;
            lblDiskSize.Visible = false;
            lblAvailabe.Visible = false;
            ListPanel.Visible = false;

            //update the titles
            Title.Text = Properties.Resources.DiskUsageErrorOoops;
            Subtitle.Text = Properties.Resources.DiskUsageErrorSubTitle;

            //Add the detective
            var detectivePictureBox = new PictureBox
            {
                Image = Properties.Resources.detective_prism,
                Location = new Point(278, 352),
                Size = new Size(234, 188)
            };

            BorderPanel.Controls.Add(detectivePictureBox);
            detectivePictureBox.BringToFront();
            ErrorDetailsBtn.Visible = true;
        }

        #endregion Methods

        #region EventHandlers

        private void DiskUsageForm_Load(object sender, EventArgs e)
        {
            _helpers.ApplySubtitleFont(Subtitle);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ErrorDetailsBtn_Click(object sender, EventArgs e)
        {
            var message = $"{Properties.Resources.DiskUsageErrorErrorType}:\n{_errorType}\n\n{Properties.Resources.DiskUsageErrorNextButton}:\n{_errorDetails}";
            MessageBox.Show(message, Properties.Resources.MessageTypeError,
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #endregion EventHandlers

        #region "Drag And Drop Window"
        private Point _MouseDownLocation;

        private void BorderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _MouseDownLocation = e.Location;
        }

        private void BorderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var form = (Form)this;
            form.Left += e.Location.X - _MouseDownLocation.X;
            form.Top += e.Location.Y - _MouseDownLocation.Y;
        }

        private void BorderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            //following on from a drag and drop of the screen the ListPanel scrolbar become unselectable (mouse scrolling works)
            //but tweaking the vertical scroll value this causes the control to become responsive again, invalidating or refreshing does not work.
            if (e.Button != MouseButtons.Left) return;

            ListPanel.VerticalScroll.Value =
                ListPanel.VerticalScroll.Value + 1 < ListPanel.VerticalScroll.Maximum
                    ? ListPanel.VerticalScroll.Value + 1
                    : ListPanel.VerticalScroll.Maximum;
            ListPanel.VerticalScroll.Value =
                ListPanel.VerticalScroll.Value - 1 > ListPanel.VerticalScroll.Minimum
                    ? ListPanel.VerticalScroll.Value - 1
                    : ListPanel.VerticalScroll.Minimum;
        }
        #endregion
    }
}
