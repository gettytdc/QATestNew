using AutomateControls.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.Wizard
{
    public abstract class WizardController
    {

        private bool m_Complete;
        private bool m_AllowCancel;
        protected IWizardDialog m_WizardDialog = null;
        private List<Control> m_WizardPanels;

        private int m_WizardIndex = -1;
        public WizardController()
        {
            m_WizardPanels = new List<Control>();
            m_AllowCancel = true;
        }


        public void SetDialog(Form dialog)
        {
            m_WizardDialog = dialog as IWizardDialog;
            if (m_WizardDialog == null)
            {
                throw new ArgumentNullException("Wizard dialogs must support IWizardDialog");
            }

            if (m_WizardDialog.NavigatePrevious == null)
                throw new ArgumentNullException(nameof(m_WizardDialog.NavigatePrevious),"Wizard dialogs must have a Previous Button");
            if (m_WizardDialog.NavigateNext == null)
                throw new ArgumentNullException(nameof(m_WizardDialog.NavigateNext), "Wizard dialogs must have a Next Button");
            if (m_WizardDialog.NavigateCancel == null)
                throw new ArgumentNullException(nameof(m_WizardDialog.NavigateCancel), "Wizard dialogs must have a Cancel Button");
            if (m_WizardDialog.Root == null)
                throw new ArgumentNullException(nameof(m_WizardDialog.Root), "Wizard dialogs must have a non null UI Root");

            m_WizardDialog.NavigatePrevious.Click += OnNavigatePrevious;
            m_WizardDialog.NavigateNext.Click += OnNavigateNext;
            m_WizardDialog.NavigateCancel.Click += OnNavigateCancel;
            dialog.Closing += this.OnClosing;
        }


        public void AddPanel(Control pnl)
        {
            IWizardPanel iwp = pnl as IWizardPanel;
            if (iwp == null)
            {
                throw new InvalidArgumentException("Wizard panels must support IWizardPanel");
            }
            else
            {
                iwp.Controller = this;
            }


            m_WizardPanels.Add(pnl);

        }

        public bool Complete
        {
            get { return m_Complete; }
        }

        public bool AllowCancel
        {
            set { m_AllowCancel = value; }
            get { return m_AllowCancel; }
        }

        public List<Control> Panels
        {
            get { return m_WizardPanels; }
        }

        protected virtual void OnClosing(object sender, CancelEventArgs e)
        {
            if (!(m_Complete||m_AllowCancel))
            {
                e.Cancel = true;
            }
        }

        public void Finish()
        {
            m_WizardDialog.Root.Controls.Clear();
            m_Complete = true;
            m_WizardDialog.Close();
        }

        protected virtual void OnNavigateFinish(object sender, EventArgs e)
        {
            Finish();
        }

        protected virtual void OnNavigateCancel(object sender, EventArgs e)
        {
            // this could be fired anywhere in case you have an opt out
            // early button on any of your forms.
            m_WizardDialog.Root.Controls.Clear();
            m_Complete = true;
            m_WizardDialog.Close();
        }

        public bool NavigateReady
        {
            set
            {
                m_WizardDialog.NavigateNext.Enabled = value;
            }
        }

        protected virtual void OnNavigateNext(object sender, EventArgs e)
        {
            CancelEventArgs ce=new CancelEventArgs();
            OnNavigateBegin(sender, ce);
            if (ce.Cancel)
                return;

            OnNavigateNextBegin(sender, ce);
            if (ce.Cancel)
                return;

            IWizardPanel iwp = m_WizardPanels[m_WizardIndex] as IWizardPanel;
            if ((iwp != null))
            {
                m_WizardIndex += 1;
            }

            if (m_WizardIndex == m_WizardPanels.Count - 1)
            {
                m_WizardDialog.NavigateNext.Text = Resources.WizardController_Finish;
            }

            if (m_WizardIndex == m_WizardPanels.Count)
            {
                OnNavigateFinish(sender, e);
                return;
            }

            Control newPanel = m_WizardPanels[m_WizardIndex];
            if ((newPanel != null))
            {
                InitPanel(newPanel);
            }

            OnNavigateNextEnd(sender, e);
            OnNavigateEnd(sender, e);
        }

        protected virtual void OnNavigateBegin(object sender, CancelEventArgs e)
        {
        }

        protected virtual void OnNavigateEnd(object sender, EventArgs e)
        {
        }

        protected virtual void OnNavigateNextBegin(object sender, CancelEventArgs e)
        {
        }

        protected virtual void OnNavigateNextEnd(object sender, EventArgs e)
        {
        }

        protected virtual void OnNavigatePrevious(object sender, EventArgs e)
        {
            CancelEventArgs ce = new CancelEventArgs();
            OnNavigateBegin(sender, ce);
            if (ce.Cancel)
                return;

            OnNavigatePreviousBegin(sender, ce);
            if (ce.Cancel)
                return;

            IWizardPanel iwp = m_WizardPanels[m_WizardIndex] as IWizardPanel;
            if ((iwp != null))
            {
                if (m_WizardIndex > 0)
                    m_WizardIndex -= 1;
                else
                    return;
            }

            if (!(m_WizardIndex == m_WizardPanels.Count - 1))
            {
                m_WizardDialog.NavigateNext.Text = Resources.WizardController_Next;
            }

            Control newPanel = m_WizardPanels[m_WizardIndex];
            if ((newPanel != null))
            {
                InitPanel(newPanel);
            }

            OnNavigatePreviousEnd(sender, e);
            OnNavigateEnd(sender, e);
        }

        protected virtual void OnNavigatePreviousBegin(object sender, CancelEventArgs e)
        {
        }

        protected virtual void OnNavigatePreviousEnd(object sender, EventArgs e)
        {
        }

        public Control CurrentPanel
        {
            get { return m_WizardPanels[m_WizardIndex]; }

            set { }
        }

        public virtual void StartWizard()
        {
            if (m_WizardPanels.Count == 0)
                throw new InvalidOperationException("Must add panels to the wizard");
            if (m_WizardIndex != -1 && !m_Complete)
                throw new InvalidOperationException("Wizard has already been started");

            m_Complete = false;
            m_WizardIndex = 0;

            Control startPanel = m_WizardPanels[m_WizardIndex];
            if ((startPanel != null))
            {
                InitPanel(startPanel);
            }
        }

        public void InitPanel(Control wizardPanel)
        {
            wizardPanel.Dock = DockStyle.Fill;

            IWizardPanel iwp = wizardPanel as IWizardPanel;
            if ((iwp != null))
            {
                m_WizardDialog.Title = iwp.Title;
                m_WizardDialog.NavigatePrevious.Enabled = iwp.ShowNavigatePrevious;
                m_WizardDialog.NavigateNext.Enabled = iwp.ShowNavigateNext;
            }


            m_WizardDialog.Root.Controls.Clear();
            m_WizardDialog.Root.Controls.Add(wizardPanel);
        }

        public void UpdateNavigate()
        {
            IWizardPanel iwp = m_WizardPanels[m_WizardIndex] as IWizardPanel;
            if ((iwp != null))
            {
                m_WizardDialog.NavigatePrevious.Enabled = iwp.ShowNavigatePrevious;
                m_WizardDialog.NavigateNext.Enabled = iwp.ShowNavigateNext;
            }
        }

        public void SetWizardProgressIndex(int newIndex) => m_WizardIndex = newIndex;
        
        public int GetWizardProgressIndex() => m_WizardIndex;
    }
}
