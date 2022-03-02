using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    /// <summary>
    /// <para>A dialog which can be used as a fairly simple method of showing a
    /// progress bar over a component.</para>
    /// 
    /// <para>The most common use will be with a BackgroundWorker object which will
    /// be used to perform the slow task. If used as such, the BackgroundWorker
    /// <em>must</em> report progress (ie. BackgroundWorker.WorkerReportsProgress
    /// must return true), otherwise an exception will be thrown.</para>
    /// 
    /// <para>See the overloaded Show() methods for descriptions on how to use it.
    /// </para>
    /// </summary>
    public partial class ProgressDialog : Form
    {
        #region - Static Show() methods -

        /// <summary>
        /// Show a default sized modal ProgressDialog, centred on the given control
        /// (or centred on the screen if no control was given) and running the given
        /// worker. It will use the given title and subtitle, or defaults if none
        /// are given, and it will create the dialog to be the given size if one is
        /// provided.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie. WorkerSupportsCancellation
        /// returns true), a Cancel button will be generated on the form.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static RunWorkerCompletedEventArgs Show(
            Control parent, BackgroundWorker worker)
        {
            return Show(parent, worker, null, null, null, null);
        }


        /// <summary>
        /// Show a default sized modal ProgressDialog, centred on the given control
        /// (or centred on the screen if no control was given) and running the given
        /// worker. It will use the given title and subtitle, or defaults if none
        /// are given, and it will create the dialog to be the given size if one is
        /// provided.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie. WorkerSupportsCancellation
        /// returns true), a Cancel button will be generated on the form.</param>
        /// <param name="arg">The argument to pass to the background worker if any.
        /// </param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static RunWorkerCompletedEventArgs Show(
            Control parent, BackgroundWorker worker, object arg)
        {
            return Show(parent, worker, arg, null, null, null);
        }

        /// <summary>
        /// Show a default sized modal ProgressDialog, centred on the given control
        /// (or centred on the screen if no control was given) and running the given
        /// worker. It will use the given title and subtitle, or defaults if none 
        /// are given.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie. WorkerSupportsCancellation
        /// returns true), a Cancel button will be generated on the form.</param>
        /// <param name="title">The title of this dialog - this is the larger text
        /// displayed on the dialog box. A null value will use the default ("Progress").
        /// An empty string will display nothing in the title's place.
        /// </param>
        /// <param name="subtitle">The subtitle of this dialog. This sits underneath
        /// the title in a smaller font. A null value will use the default
        /// ("Please wait...") and an empty string will display nothing.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static RunWorkerCompletedEventArgs Show(
            Control parent, BackgroundWorker worker, String title, String subtitle)
        {
            return Show(parent, worker, null, title, subtitle, null);
        }

        /// <summary>
        /// Show a default sized modal ProgressDialog, centred on the given control
        /// (or centred on the screen if no control was given) and running the given
        /// worker. It will use the given title and subtitle, or defaults if none 
        /// are given.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie. WorkerSupportsCancellation
        /// returns true), a Cancel button will be generated on the form.</param>
        /// <param name="arg">The argument to pass to the background worker if any.
        /// </param>
        /// <param name="title">The title of this dialog - this is the larger text
        /// displayed on the dialog box. A null value will use the default ("Progress").
        /// An empty string will display nothing in the title's place.
        /// </param>
        /// <param name="subtitle">The subtitle of this dialog. This sits underneath
        /// the title in a smaller font. A null value will use the default
        /// ("Please wait...") and an empty string will display nothing.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static RunWorkerCompletedEventArgs Show(
            Control parent, BackgroundWorker worker, Object arg, String title, String subtitle)
        {
            return Show(parent, worker, arg, title, subtitle, null);
        }

        /// <summary>
        /// Show a modal ProgressDialog, centred on the given control (or centred on the
        /// screen if no control was given) and running the given worker.
        /// It will use the default title and subtitle, and it will create the dialog 
        /// to be the given size if one is provided.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie. WorkerSupportsCancellation
        /// returns true), a Cancel button will be generated on the form.</param>
        /// <param name="dialogSize">The size of the dialog to create. If null, this
        /// will use the default size of the dialog.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static RunWorkerCompletedEventArgs Show(
            Control parent, BackgroundWorker worker, Size? dialogSize)
        {
            return Show(parent, worker, null, null, null, dialogSize);
        }

        /// <summary>
        /// Show a modal ProgressDialog, centred on the given control (or centred on the
        /// screen if no control was given) and running the given worker.
        /// It will use the given title and subtitle, or defaults if none are given,
        /// and it will create the dialog to be the given size if one is provided.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie. WorkerSupportsCancellation
        /// returns true), a Cancel button will be generated on the form.</param>
        /// <param name="arg">The argument to pass to the background worker if any.
        /// </param>
        /// <param name="title">The title of this dialog - this is the larger text
        /// displayed on the dialog box. A null value will use the default ("Progress").
        /// An empty string will display nothing in the title's place.
        /// </param>
        /// <param name="subtitle">The subtitle of this dialog. This sits underneath
        /// the title in a smaller font. A null value will use the default
        /// ("Please wait...") and an empty string will display nothing.</param>
        /// <param name="dialogSize">The size of the dialog to create. If null, this
        /// will use the default size of the dialog.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static RunWorkerCompletedEventArgs Show(
            Control parent,
            BackgroundWorker worker,
            Object arg,
            String title,
            String subtitle,
            Size? dialogSize)
        {
            IWin32Window win;
            using (ProgressDialog pd = Prepare(
                parent, worker, arg, title, subtitle, dialogSize, out win))
            {
                pd.ShowInTaskbar = false;
                pd.ShowDialog(win);

                // Make sure the parent form is activated after the progress bar
                // returns
                //Form f = win as Form;
                //if (f != null)
                //{
                //    // This works and fits and starts without BringToFront()
                //    // which appears to aid it in working consistently (with a little
                //    // flicker)
                //    f.BringToFront();
                //    f.Activate();
                //}
                return pd._completedArgs;
            }
        }

        #endregion

        #region - Static Prepare() methods -

        /// <summary>
        /// Prepares a ProgressDialog, centred on the given control (or centred on
        /// the screen if no control was given) and running the given worker.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie.
        /// WorkerSupportsCancellation returns true), a Cancel button will be
        /// generated on the form.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static ProgressDialog Prepare(
            Control parent, BackgroundWorker worker, out IWin32Window win)
        {
            return Prepare(parent, worker, null, null, null, null, out win);
        }


        /// <summary>
        /// Prepares a ProgressDialog, centred on the given control (or centred on
        /// the screen if no control was given) and running the given worker.
        /// It will use the given title and subtitle, or defaults if none are given.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie.
        /// WorkerSupportsCancellation returns true), a Cancel button will be
        /// generated on the form.</param>
        /// <param name="arg">The argument to pass to the background worker if any.
        /// </param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static ProgressDialog Prepare(Control parent,
            BackgroundWorker worker, object arg, out IWin32Window win)
        {
            return Prepare(parent, worker, arg, null, null, null, out win);
        }

        /// <summary>
        /// Prepares a ProgressDialog, centred on the given control (or centred on
        /// the screen if no control was given) and running the given worker.
        /// It will use the given title and subtitle, or defaults if none are given.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie.
        /// WorkerSupportsCancellation returns true), a Cancel button will be
        /// generated on the form.</param>
        /// <param name="title">The title of this dialog - this is the larger text
        /// displayed on the dialog box. A null value will use the default
        /// ("Progress"). An empty string will display nothing in the title's place.
        /// </param>
        /// <param name="subtitle">The subtitle of this dialog. This sits underneath
        /// the title in a smaller font. A null value will use the default
        /// ("Please wait...") and an empty string will display nothing.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static ProgressDialog Prepare(Control parent, BackgroundWorker worker,
            String title, String subtitle, out IWin32Window win)
        {
            return Prepare(parent, worker, null, title, subtitle, null, out win);
        }

        /// <summary>
        /// Prepares a ProgressDialog, centred on the given control (or centred on
        /// the screen if no control was given) and running the given worker.
        /// It will use the given title and subtitle, or defaults if none 
        /// are given.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie.
        /// WorkerSupportsCancellation returns true), a Cancel button will be
        /// generated on the form.</param>
        /// <param name="arg">The argument to pass to the background worker if any.
        /// </param>
        /// <param name="title">The title of this dialog - this is the larger text
        /// displayed on the dialog box. A null value will use the default
        /// ("Progress"). An empty string will display nothing in the title's place.
        /// </param>
        /// <param name="subtitle">The subtitle of this dialog. This sits underneath
        /// the title in a smaller font. A null value will use the default
        /// ("Please wait...") and an empty string will display nothing.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static ProgressDialog Prepare(Control parent, BackgroundWorker worker,
            Object arg, String title, String subtitle, out IWin32Window win)
        {
            return Prepare(parent, worker, arg, title, subtitle, null, out win);
        }

        /// <summary>
        /// Prepares a ProgressDialog, centred on the given control (or centred on
        /// the screen if no control was given) and running the given worker.
        /// It will use the default title and subtitle, and it will create the dialog 
        /// to be the given size if one is provided.
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie.
        /// WorkerSupportsCancellation returns true), a Cancel button will be
        /// generated on the form.</param>
        /// <param name="dialogSize">The size of the dialog to create. If null, this
        /// will use the default size of the dialog.</param>
        /// <returns>The event arguments which detail the background worker completed
        /// event.</returns>
        public static ProgressDialog Prepare(Control parent,
            BackgroundWorker worker, Size? dialogSize, out IWin32Window win)
        {
            return Prepare(parent, worker, null, null, null, dialogSize, out win);
        }

        /// <summary>
        /// Prepares the progress dialog for use with the given parameters
        /// </summary>
        /// <param name="parent">The control which the progress dialog should be
        /// centred upon.</param>
        /// <param name="worker">The worker which is responsible for doing the work
        /// in the background. This must report progress (ie. WorkerReportsProgress
        /// must return true). If it supports cancellation (ie.
        /// WorkerSupportsCancellation returns true), a Cancel button will be
        /// generated on the form.</param>
        /// <param name="arg">The argument to pass to the background worker if any.
        /// </param>
        /// <param name="title">The title of this dialog - this is the larger text
        /// displayed on the dialog box. A null value will use the default
        /// ("Progress"). An empty string will display nothing in the title's place.
        /// </param>
        /// <param name="subtitle">The subtitle of this dialog. This sits underneath
        /// the title in a smaller font. A null value will use the default
        /// ("Please wait...") and an empty string will display nothing.</param>
        /// <param name="dialogSize">The size of the dialog to create. If null, this
        /// will use the default size of the dialog.</param>
        /// <param name="win">On return, this will represent the IWin32Window which
        /// is expected to be used as the owner of the dialog. If this is not used,
        /// the <see cref="StartPosition"/> of the returned dialog may be
        /// inappropriate to how the dialog will display.</param>
        /// <returns>The progress dialog prepared from the given arguments, and
        /// ready to be shown owned by the returned <paramref name="win"/></returns>
        public static ProgressDialog Prepare(
            Control parent,
            BackgroundWorker worker,
            Object arg,
            String title,
            String subtitle,
            Size? dialogSize,
            out IWin32Window win)
        {
            if (worker == null) throw new ArgumentNullException(nameof(worker));

            ProgressDialog pd = new ProgressDialog(worker);
            pd.Argument = arg;
            pd.Size = (dialogSize.HasValue ? dialogSize.Value : pd.PreferredSize);

            if (title != null)
                pd.lblTitle.Text = title;

            if (subtitle != null)
                pd.lblSubtitle.Text = subtitle;

            if (parent == null)
            {
                pd.StartPosition = FormStartPosition.CenterScreen;
                win = null;
            }
            else
            {
                win = (parent.TopLevelControl as IWin32Window);
                pd.Owner = (win as Form);

                // try and find where to centre the dialog.

                // first, we try getting the bounds from the parent's parent...
                // if it exists.
                Control grandparent = parent.Parent;
                if (grandparent != null)
                {
                    Rectangle rect = grandparent.RectangleToScreen(parent.Bounds);
                    pd.StartPosition = FormStartPosition.Manual;
                    pd.Location = new Point(
                        rect.Left + ((rect.Width - pd.Size.Width) / 2),
                        rect.Top + ((rect.Height - pd.Size.Height) / 2)
                    );
                }
                // no parent - use the top level control's bounds (if it's a form)
                else
                {
                    pd.StartPosition = FormStartPosition.CenterParent;
                }
            }

            return pd;
        }


        #endregion

        #region - Other static declarations -

        /// <summary>
        /// The default title used in this dialog
        /// </summary>
        private const string DefaultTitle = "Progress";

        /// <summary>
        /// The default subtitle used in this dialog
        /// </summary>
        private const string DefaultSubTitle = "Please Wait...";

        #endregion

        #region - Published Events -

        /// <summary>
        /// Event called if the user cancels the progress dialog, and,
        /// by extension, cancels the underlying operation that the progress
        /// dialog is monitoring.
        /// </summary>
        public event EventHandler Cancel;

        /// <summary>
        /// Event called if the user cancels the progress dialog, and,
        /// by extension, cancels the underlying operation that the progress
        /// dialog is monitoring.
        /// </summary>
        public event EventHandler OverriddenCancel;


        /// <summary>
        /// Chained event from the contained background worker indicating that the
        /// progress has changed.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// Chained event from the contained background worker indicating that the
        /// worker has completed.
        /// </summary>
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        #endregion

        #region - Member variables -

        /// <summary>
        /// The worker object which will be asynchronously handling the work
        /// </summary>
        private BackgroundWorker _worker;

        /// <summary>
        /// The active worker which is set when the work is initiated and unset
        /// when it has completed. This allows the handling of event listeners to
        /// be done in a controller manner
        /// </summary>
        private BackgroundWorker _active;

        /// <summary>
        /// The argument to pass to the worker when started automatically when the
        /// dialog is shown
        /// </summary>
        private object _arg;

        /// <summary>
        /// The function delegate which is given the progress data and then used
        /// to populate the subtitle of the dialog. If the function returns null
        /// the subtitle is not changed.
        /// </summary>
        private Func<object, string> _progDisplayer;

        /// <summary>
        /// The completed arguments which detail the result from and how the
        /// background worker completed.
        /// </summary>
        private RunWorkerCompletedEventArgs _completedArgs;

        /// <summary>
        /// Sets the dialog to close immediately after the cancel button is pressed,
        /// even if the background worker has not yet responded to the request
        /// </summary>
        private bool _closeOnCancel;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new progress dialog with a min and max value of 0 and
        /// 100, respectively.
        /// </summary>
        public ProgressDialog() : this(0, 100, null) { }

        /// <summary>
        /// Creates a new progress dialog tied to the given worker.
        /// If the worker supports progress, the progress bar style will be left
        /// at default - otherwise a marquee style is set. If the worker supports
        /// cancellation, a button will be available on the form to allow the
        /// user to cancel the work. Otherwise, the button will be unavailable.
        /// </summary>
        /// <param name="worker">The worker which will handle the long
        /// task asynchronously.</param>
        /// <exception cref="ArgumentNullException">If the worker was null
        /// </exception>        
        public ProgressDialog(BackgroundWorker worker) : this(0, 100, worker) { }

        /// <summary>
        /// Creates a new progress dialog bounded by the given minimum and
        /// maximum values.
        /// </summary>
        /// <param name="minimum">The minimum value to use which indicates
        /// no progress</param>
        /// <param name="maximum">The maximum value to use which indicates
        /// the operation is complete.</param>
        public ProgressDialog(int minimum, int maximum)
            : this(minimum, maximum, null) { }

        /// <summary>
        /// Creates a new progress dialog bounded by the given minimum and
        /// maximum values, tied to the given worker.
        /// If the worker supports progress, the progress bar style will be left
        /// at default - otherwise a marquee style is set. If the worker supports
        /// cancellation, a button will be available on the form to allow the
        /// user to cancel the work. Otherwise, the button will be unavailable.
        /// </summary>
        /// <param name="worker">The worker which will handle the long
        /// task asynchronously.</param>
        /// <param name="minimum">The minimum value to use which indicates
        /// no progress</param>
        /// <param name="maximum">The maximum value to use which indicates
        /// the operation is complete.</param>
        /// <exception cref="ArgumentNullException">If the worker was null and the
        /// dialog was not created at design time</exception>
        public ProgressDialog(int minimum, int maximum, BackgroundWorker worker)
        {
            InitializeComponent();

            _closeOnCancel = true;

            this.Minimum = minimum;
            this.Maximum = maximum;

            btnCancel.Click += new EventHandler(HandleCancelClicked);

            if (worker != null)
            {
                if (!worker.WorkerReportsProgress)
                    Style = ProgressBarStyle.Marquee;

                if (!worker.WorkerSupportsCancellation)
                {
                    Controls.Remove(btnCancel);
                    lblTitle.Width = lblSubtitle.Width;
                    btnCancel = null;
                }

                _worker = worker;
            }
            // Only fail if this occurs at runtime - at design time it's expected
            // to not have a background worker available
            else if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                throw new ArgumentNullException(
nameof(worker), "The worker for a progress dialog cannot be null");
            }
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The active background worker - this value is only non-null while a
        /// background worker is active within this progress dialog
        /// </summary>
        public BackgroundWorker ActiveWorker
        {
            get { return _active; }
            private set
            {
                if (value != _active)
                {
                    if (_active != null)
                    {
                        _active.ProgressChanged -= HandleWorkerProgressChanged;
                        _active.RunWorkerCompleted -= HandleWorkerCompleted;
                    }
                    _active = value;
                    if (_active != null)
                    {
                        _active.ProgressChanged += HandleWorkerProgressChanged;
                        _active.RunWorkerCompleted += HandleWorkerCompleted;
                        if (!_active.IsBusy)
                            _active.RunWorkerAsync(_arg);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this dialog should close immediately on a cancel
        /// request or wait for the background worker to complete first
        /// </summary>
        [Category("Behavior"),
         Description("Close dialog immediately on user cancel request"),
         DefaultValue(true)]
        public bool CloseOnCancel
        {
            get { return _closeOnCancel; }
            set { _closeOnCancel = value; }
        }

        /// <summary>
        /// Gets or sets the title of this dialog; this also sets the 'text' of
        /// this dialog.
        /// </summary>
        [Category("Appearance"),
         Description("The title of the progress dialog"),
         DefaultValue(DefaultTitle)]
        public string Title
        {
            get { return lblTitle.Text; }
            set
            {
                if (value != Title)
                {
                    lblTitle.Text = value ?? DefaultTitle;
                    Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the subtitle of this dialog
        /// </summary>
        [Category("Appearance"),
         Description("The subtitle of the progress dialog"),
         DefaultValue(DefaultSubTitle)]
        public string SubTitle
        {
            get { return lblSubtitle.Text; }
            set { lblSubtitle.Text = value ?? DefaultSubTitle; }
        }

        /// <summary>
        /// Checks if this dialog was cancelled or not.
        /// </summary>
        /// <returns>true if this dialog completed because of a cancellation;
        /// false otherwise</returns>
        [Browsable(false)]
        public bool IsCancelled
        {
            get { return (_completedArgs != null && _completedArgs.Cancelled); }
        }

        /// <summary>
        /// Gets or sets the text of this dialog, which has the effect of setting
        /// the 'title' of this dialog.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                if (value != Text)
                {
                    base.Text = value;
                    Title = value;
                }
            }
        }

        /// <summary>
        /// The argument to pass to the background worker when starting it from
        /// within this dialog (ie. when the dialog is shown)
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Argument
        {
            get { return _arg; }
            set { _arg = value; }
        }

        /// <summary>
        /// The function to call with the progress object from progress updates
        /// given by the background worker in order to format the subtitle in the
        /// progress dialog.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, string> ProgressDisplayFunction
        {
            get { return (_progDisplayer ?? delegate (object o) { return null; }); }
            set { _progDisplayer = value; }
        }

        #endregion

        #region - Event handlers -

        /// <summary>
        /// Event handler for the registered background worker indicating a
        /// progress update.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The arguments detailing the nature of the progress
        /// update, primarily the percentage complete.</param>
        private void HandleWorkerProgressChanged(
            object sender, ProgressChangedEventArgs e)
        {
            OnProgressChanged(e);
        }

        /// <summary>
        /// Event handler to deal with the Cancel button being clicked. This
        /// ensures that the background worker (if one is registered in this
        /// dialog) is told to cancel, and fires the Cancel event for any
        /// listeners.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The arguments detailing the event.</param>
        private void HandleCancelClicked(object sender, EventArgs e)
        {
            if (!OverrideCancel)
            {
                OnCancel(e);
            }
            else
            {
                OnOverriddenCancel(e);
            }

        }

        /// <summary>
        /// Event handler for the registered background worker completing.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The args detailing the nature and specifics
        /// of the worker completing its work.</param>
        private void HandleWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            // We're no longer interested in the progress of the worker,
            // make sure we are distanced from it so it may be used again
            // without compounding the event listeners.
            ActiveWorker = null;
            OnRunWorkerCompleted(e);
        }

        /// <summary>
        /// Raises the <see cref="RunWorkerCompleted"/> event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            _completedArgs = e;

            // Set the dialog result appropriately, though really all of this is
            // available in the event args which are typically returned from
            // the Show() methods used to display this dialog
            if (e.Error != null)
                DialogResult = DialogResult.Abort;
            else if (e.Cancelled)
                DialogResult = DialogResult.Cancel;
            else
                DialogResult = DialogResult.OK;

            Close();

            RunWorkerCompletedEventHandler h = this.RunWorkerCompleted;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ProgressChanged"/> event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            this.Value = e.ProgressPercentage;
            string subtitle = ProgressDisplayFunction(e.UserState);
            if (subtitle != null)
                SubTitle = subtitle;

            ProgressChangedEventHandler h = this.ProgressChanged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Cancel"/> event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected virtual void OnCancel(EventArgs e)
        {
            BackgroundWorker active = _active;
            if (active != null)
            {
                // set the completed args to a generic 'cancelled' since the worker
                // might still be running when the dialog returns
                _completedArgs = new RunWorkerCompletedEventArgs(null, null, true);
                active.CancelAsync();
            }

            if (_closeOnCancel)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }

            EventHandler h = this.Cancel;
            if (h != null)
                h(this, e);

        }

        /// <summary>
        /// Handles this dialog being shown by starting the background worker that
        /// it is showing the progress for
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ActiveWorker = _worker;
        }

        /// <summary>
        /// Override to try and make sure that the closing of the form doesn't
        /// send the parent window into the background.
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // workaround for the closing of this window sending the parent
            // window into the background.
            // I got this solution, indirectly from a link on the page :-
            // http://social.msdn.microsoft.com/Forums/en-IE/wpf/thread/05863be7-2c37-43c3-b506-b23b7c82870d
            // though I'm not sure why it might work.
            if (!e.Cancel)
            {
                this.ShowInTaskbar = true;
                this.Owner = null;
            }
        }
        public bool UpdateCancelProgress()
        {
            return MessageBox.Show(Properties.Resources.ProgressBarAreYouSure, Properties.Resources.ProgressBarCancelUpgrade, MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        protected virtual void OnOverriddenCancel(EventArgs e)
        {
            OverriddenCancel?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region - Progress Bar-a-like Properties/Methods -

        /// <summary>
        /// The maximum value of this progress dialog.
        /// </summary>
        public int Maximum
        {
            get { return progBar.Maximum; }
            set { progBar.Maximum = value; }
        }

        /// <summary>
        /// The minimum value of this progress dialog.
        /// </summary>
        public int Minimum
        {
            get { return progBar.Minimum; }
            set { progBar.Minimum = value; }
        }

        /// <summary>
        /// The size of a step which is used when PerformStep()
        /// is called.
        /// </summary>
        public int Step
        {
            get { return progBar.Step; }
            set { progBar.Step = value; }
        }

        /// <summary>
        /// The current value of this progress dialog.
        /// </summary>
        public int Value
        {
            get { return progBar.Value; }
            set { progBar.Value = value; }
        }

        /// <summary>
        /// Allows the Cancel Event to be overridden
        /// </summary>
        public bool OverrideCancel { get; set; }

        /// <summary>
        /// Increments the progress modelled by this dialog by the
        /// given amount.
        /// </summary>
        /// <param name="amount">The amount that the Value of this dialog
        /// should be incremented by.</param>
        public void Increment(int amount)
        {
            progBar.Increment(amount);
        }

        /// <summary>
        /// Increments the value of this progress dialog by the Step
        /// amount.
        /// </summary>
        public void PerformStep()
        {
            progBar.PerformStep();
        }

        /// <summary>
        /// If the progress bar style set in this dialog is Marquee, this
        /// has the effect of setting the animation effect of the marquee
        /// </summary>
        public int MarqueeAnimationSpeed
        {
            get { return progBar.MarqueeAnimationSpeed; }
            set { progBar.MarqueeAnimationSpeed = value; }
        }

        /// <summary>
        /// The visual style of the progress bar.
        /// </summary>
        [Browsable(true)]
        public ProgressBarStyle Style
        {
            get { return progBar.Style; }
            set { progBar.Style = value; }
        }

        public void UpdateTitle(string title)
        {
            if (title != null)
                lblTitle.Text = title;
        }

        public void EnableCancel(bool enabled)
        {
            btnCancel.Enabled = enabled;
        }
        #endregion


    }
}
