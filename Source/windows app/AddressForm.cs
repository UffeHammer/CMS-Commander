using CustomControls;
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
    public partial class AddressForm : Form
    {
        public LoginInfo _foundLoginInfo = null;

        public LoginInfo foundLoginInfo
        {
            get
            {
                string sConfigFile = Application.UserAppDataPath.Remove(Application.UserAppDataPath.LastIndexOf("\\")) + "\\LoginInfo.xml";
                Logins logins = null;
                try
                {
                    logins = disc1.XML.ObjectXMLSerializer<Logins>.Load(sConfigFile);
                }
                catch
                {
                    logins = new Logins();
                }

                _foundLoginInfo = null;
                for (int t = 0; t < logins.LoginInfos.Count; t++)
                {
                    LoginInfo tmp = (LoginInfo)logins.LoginInfos[t];
                    if (tmp.SiteUrl == lbConnections.SelectedItem.ToString())
                    {
                        _foundLoginInfo = tmp;
                        break;
                    }
                }

                return _foundLoginInfo;
            }
            set 
            {
                _foundLoginInfo = value;
            }
        }

        public AddressForm()
        {
            InitializeComponent();
        }

        private void LoadLogin()
        {
            string sConfigFile = Application.UserAppDataPath.Remove(Application.UserAppDataPath.LastIndexOf("\\")) + "\\LoginInfo.xml";
            Logins logins = null;
            try
            {
                logins = disc1.XML.ObjectXMLSerializer<Logins>.Load(sConfigFile);
            }
            catch
            {
                logins = new Logins();
            }
            for (int t = 0; t < logins.LoginInfos.Count; t++)
            {
                LoginInfo tmp = (LoginInfo)logins.LoginInfos[t];
                if (!tmp.SiteUrl.Contains("Basic authentication"))
                    lbConnections.Items.Add(tmp.SiteUrl);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

            if (foundLoginInfo != null)
                this.Close();
        }

        private void AddressForm_Load(object sender, EventArgs e)
        {
            LoadLogin();
        }

        private void lbConnections_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btnConnect_Click(sender, e);
            this.DialogResult = DialogResult.OK;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (foundLoginInfo == null)
                return;

            string sConfigFile = Application.UserAppDataPath.Remove(Application.UserAppDataPath.LastIndexOf("\\")) + "\\LoginInfo.xml";
            Logins logins = null;
            try
            {
                logins = disc1.XML.ObjectXMLSerializer<Logins>.Load(sConfigFile);
            }
            catch
            {
                logins = new Logins();
            }

            for (int t = 0; t < logins.LoginInfos.Count; t++)
            {
                LoginInfo tmp = (LoginInfo)logins.LoginInfos[t];
                if (tmp.SiteUrl == lbConnections.SelectedItem.ToString())
                {
                    logins.LoginInfos.RemoveAt(t);
                    lbConnections.Items.Remove(lbConnections.SelectedItem);
                    break;
                }
            }

//            logins.LoginInfos.Remove(foundLoginInfo);

            LoginControl.SaveLogins(logins);
            
        }
    }
}
