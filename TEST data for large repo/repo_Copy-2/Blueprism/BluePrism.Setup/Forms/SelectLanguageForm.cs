using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace BluePrism.Setup.Forms
{
    public partial class SelectLanguageForm : Form
    {
        public string NewLocale { get; set; }
        private bool _pseudoLocalization = false;
        private static List<LocaleListEntry> _cultureList = new List<LocaleListEntry>();
        private Font _labelFont = new Font("Segoe UI", 16, FontStyle.Regular, GraphicsUnit.Pixel);
        private Font _labelFontBold = new Font("Segoe UI", 16, FontStyle.Bold, GraphicsUnit.Pixel);
        private Helpers _helpers = new Helpers(null);
        public SelectLanguageForm(bool pseudoLocalization)
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();
            this.KeyPreview = true;
#if DEBUG
            this._pseudoLocalization = true;
#else
            this._pseudoLocalization = pseudoLocalization;
#endif
            if (_cultureList.Count == 0)
                GenerateCulturesTable();
            else
                _cultureList.FirstOrDefault(x => x.ID == 0).Text =
                    Properties.Resources.LocaleConfigForm_UsingWindowsLocaleSettings;
            _cultureList.Sort();
            PopulateLocales();
        }

        private void PopulateLocales()
        {
            ListPanel.Controls.Clear();

            var yPos = 26;
            var yImage = 0;
            var yBar = 0;
            var xImage = 126;
            var xWord = 150;
            var activeWord = "";


            if (_cultureList != null)
            {
                if (!_cultureList.Any(x => x.Active) &&
                    _cultureList.Any(x => x.Value == CultureInfo.CurrentUICulture.Name))
                    _cultureList.FirstOrDefault(x => x.Value == CultureInfo.CurrentUICulture.Name).Active = true;

                foreach (var culture in _cultureList)
                {
                    var l = new Label
                    {
                        Text = culture.Text,
                        ForeColor = Color.FromArgb(17, 126, 194),
                        AutoSize = true,
                        Name = $"L{culture.ID.ToString()}",
                        Font = _labelFont
                    };
                    l.Click += L_Click;
                    ListPanel.Controls.Add(l);
                    l.Location = new Point(xWord, yPos);
                    if (culture.Active)
                    {
                        activeWord = l.Text;
                        yImage = yPos + 4;
                        yBar = yPos + 24;
                    }

                    yPos += 32;
                }
            }

            var i = new PictureBox
            {
                Image = Properties.Resources.triangle_fwd_2x,
                Size = new Size(16, 16),
                Name = $"ActiveEntry",
                Visible = (yImage > 0)
            };
            ListPanel.Controls.Add(i);
            i.Location = new Point(xImage, yImage);

            var b = new Panel()
            {
                BackColor = Color.FromArgb(17, 126, 194),
                Height = 4,
                Width = _helpers.MeasureString(_labelFontBold, activeWord),
                Name = "UnderlineBar",
                Visible = (yBar > 0)
            };
            ListPanel.Controls.Add(b);
            b.Location = new Point(xWord + 5, yBar);

        }

        private void L_Click(object sender, EventArgs e)
        {
            var label = (Label)sender;
            var selectedLocale = label.Name.Substring(1);
            var oldCulture = _cultureList.FirstOrDefault(x => x.Active);

            if (oldCulture != null)
            {
                _cultureList.Remove(oldCulture);
                oldCulture.Active = false;
                _cultureList.Add(oldCulture);
            }

            var newCulture = _cultureList.FirstOrDefault(x => x.ID.ToString() == selectedLocale);

            if (newCulture != null)
            {
                ResetLabelFonts();
                _cultureList.Remove(newCulture);
                newCulture.Active = true;
                _cultureList.Add(newCulture);

                if (newCulture.ID.ToString() != null)
                {
                    var activeEntry = (PictureBox)ListPanel.Controls.Find("ActiveEntry", true).First();
                    activeEntry.Visible = true;
                    activeEntry.Location = new Point(126, label.Location.Y + 4);

                    var underlineBar = (Panel)ListPanel.Controls.Find("UnderlineBar", true).First();
                    underlineBar.Location = new Point(label.Location.X + 5, label.Location.Y + 24);
                    underlineBar.Width = _helpers.MeasureString(_labelFontBold, label.Text);
                    underlineBar.Visible = true;

                    label.Font = _labelFontBold;
                    label.AutoSize = true;
                }

                NewLocale = newCulture.Value;
            }

            _cultureList.Sort();
        }

        private void ResetLabelFonts()
        {
            foreach (Control c in ListPanel.Controls)
            {
                if (c is Label label)
                    label.Font = _labelFont;
            }
        }


        private class LocaleListEntry : IComparable
        {
            public int ID;
            public string Text;
            public string Value;
            public bool Active;

            public int CompareTo(object obj)
            {
                return ID.CompareTo(((LocaleListEntry)obj).ID);
            }
        }

        private class LocaleListComparer : IComparer<LocaleListEntry>
        {
            public int Compare(LocaleListEntry x, LocaleListEntry y)
            {
                return x.ID.CompareTo(y.ID);
            }
        }
        /// <summary>
        ///     ''' Generates list of supported locales
        ///     ''' </summary>
        private void GenerateCulturesTable()
        {
            _cultureList.Clear();
            var id = 0;
            var installedCulture = CultureInfo.CurrentUICulture;
            _cultureList.Add(new LocaleListEntry { ID = id, Text = Properties.Resources.LocaleConfigForm_UsingWindowsLocaleSettings, Value = installedCulture.Name, Active = installedCulture.Name == NewLocale });

            foreach (string locale in Internationalisation.Locales.SupportedLocales)
            {
                id++;
                try
                {
                    var addCulture = new CultureInfo(locale);
                    _cultureList.Add(new LocaleListEntry { ID = id, Text = addCulture.TextInfo.ToTitleCase(addCulture.NativeName), Value = addCulture.Name, Active = addCulture.Name == NewLocale });
                }
                catch
                {
                    // do nothing, just don't add to the drop down
                }
            }

            if (_pseudoLocalization)
            {
                try
                {
                    var addCulture = new CultureInfo("gsw-LI");
                    _cultureList.Add(new LocaleListEntry { ID = _cultureList.Count, Text = "i18n TEST LANGUAGE", Value = addCulture.Name, Active = addCulture.Name == NewLocale });
                }
                catch
                {
                    // do nothing, just don't add to the drop down
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

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

        #endregion

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

        private void SelectLanguageForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        SelectNextLocale(1);
                        break;
                    }

                case Keys.Up:
                    {
                        SelectNextLocale(-1);
                        break;
                    }
            }
        }

        private void SelectNextLocale(int dir)
        {
            var oldCulture = _cultureList.FirstOrDefault(x => x.Active);
            var index = oldCulture.ID + dir;
            if (ListPanel.Controls.Find($"L{index}", true).FirstOrDefault() is Label label)
            {
                ListPanel.ScrollControlIntoView(label);
                L_Click(label, EventArgs.Empty);
            }
        }
    }
}
