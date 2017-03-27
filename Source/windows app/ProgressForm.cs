using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SitecoreConverter.Core;

namespace SitecoreConverter
{
    public partial class ProgressForm : Form
    {
        private bool _bCopyProgess = true;
        private int _iInstallSitesCount = 0;
        private int _iProgressIncrement = 1;

        public bool CopyProgess
        {
            get             
            {
                return _bCopyProgess;
            }
            set 
            { 
                _bCopyProgess = value;
            }
        }

        public int InstallSitesCount
        {
            get
            {
                return _iInstallSitesCount;
            }
            set
            {
                _iInstallSitesCount = value;
            }
        }

        public ProgressForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_bCopyProgess)
            {
                if (lblCopyingFrom.Text != Util.CopyingFrom)
                    lblCopyingFrom.Text = Util.CopyingFrom;

                if (lblCopyingTo.Text != Util.CopyingTo)
                    lblCopyingTo.Text = Util.CopyingTo;
            }

            progressCurrentTask.Value += _iProgressIncrement;

            if ((progressCurrentTask.Value == progressCurrentTask.Maximum) || 
                (progressCurrentTask.Value == progressCurrentTask.Minimum))
                _iProgressIncrement = -_iProgressIncrement;

            if (Util.WarningList.Count > 0)
            {
                if (this.Height < 336)
                    this.Height = 336;

                int iLineCount = 0;
                string sWarnings = tbWarnings.Text;
                while (sWarnings.IndexOf("\n") > -1)
                {
                    sWarnings = sWarnings.Remove(sWarnings.IndexOf("\n"), "\n".Length);
                    iLineCount++;
                }

                // Only update if new lines have been added
                if (iLineCount != Util.WarningList.Count)
                {
                    tbWarnings.Text = "";
                    for (int t = 0; t < Util.WarningList.Count; t++)
                    {
                        tbWarnings.Text += Util.WarningList[t] + "\r\n";
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_bCopyProgess)
                Util.backgroundWorker.CancelAsync();
            else
                this.Close();
        }
    }
}
