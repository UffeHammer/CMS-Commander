using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SitecoreConverter
{
    public partial class LoginForm : Form
    {
        public bool Success = false;

        public LoginForm()
        {
            InitializeComponent();
        }

        public string Username
        {
            get
            {
                return loginControl1.Username;
            }
            set
            {
                loginControl1.Username = value;
            }
        }

        public string Password
        {
            get
            {
                return loginControl1.Password;
            }
            set
            {
                loginControl1.Password = value;
            }
        }

        public string SiteUrl
        {
            get
            {
                return loginControl1.lblSiteUrl.Text;
            }
            set
            {
                loginControl1.lblSiteUrl.Text = value;
            }
        }

        public string SiteType
        {
            get
            {
                return loginControl1.lblSiteType.Text;
            }
            set
            {
                loginControl1.lblSiteType.Text = value;
            }
        }

        

        private void loginControl1_Successful(object sender, EventArgs e)
        {
        }

        private void loginControl1_Ready(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Success = true;
            Close();
        }

        private void loginControl1_Load_1(object sender, EventArgs e)
        {
            loginControl1.Check = false;
            loginControl1.Hash = false;
        }

        private void loginControl1_Failed(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Success = false;
            Close();
        }
    }
}
