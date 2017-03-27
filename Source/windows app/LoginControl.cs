using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;  // Does XML serializing for a class.

namespace CustomControls
{
	/// <summary> 
	/// LoginControl is a free .NET login control with cryptography support.
	/// </summary>
	public class LoginControl : System.Windows.Forms.UserControl
	{
		///<event cref="Successful">
		/// Raised after a successful Login.
		/// </event>
		public event System.EventHandler Successful;
		///<event cref="Failed">
		/// Raised after a failed Login.
		/// </event>
		public event System.EventHandler Failed;
		///<event cref="Ready">
		///    Raised after the User clicked on Login. Use this event to monitor LoginControl.
		///</event>
		/// <example> This sample shows how to use Ready.
		/// <code>
		/// private void OnEventReady(object sender, System.EventArgs e)
		/// {
		///		MessageBox.Show("Username: " + loginControl1.Username + "\nPassword: " + loginControl1.Password,"LoginControl");
		/// }
		/// </code>
		/// </example>
		public event System.EventHandler Ready;
		private bool bCheck = true;
		private bool bHash= true;
		private string theUsername;
		private string thePassword;
		private string tmpU;
		private string tmpP;
		private string ErrMsg = "Please enter the correct Username / Password combination!";
		private System.Windows.Forms.Button bLogin;
		private System.Windows.Forms.Label lPassword;
		private System.Windows.Forms.Label lUsername;
		private System.Windows.Forms.TextBox tbPassword;
		private System.Windows.Forms.Label lHeader;
		private System.Windows.Forms.Label lText;
		private System.Windows.Forms.PictureBox pbImage;
		private System.Windows.Forms.TextBox tbUsername;
		private System.Windows.Forms.Panel pHeader;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private Button bCancel;
        private CheckBox cbStoreLogin;
        private Label lblSiteInfo;
        public Label lblSiteUrl;
        public Label lblSiteType;
        private IContainer components;

		/// <summary> 
		/// The public Initializer. Nothing happening here.
		/// </summary>
		public LoginControl()
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginControl));
            this.bLogin = new System.Windows.Forms.Button();
            this.lPassword = new System.Windows.Forms.Label();
            this.lUsername = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.pHeader = new System.Windows.Forms.Panel();
            this.lHeader = new System.Windows.Forms.Label();
            this.lText = new System.Windows.Forms.Label();
            this.pbImage = new System.Windows.Forms.PictureBox();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.bCancel = new System.Windows.Forms.Button();
            this.lblSiteInfo = new System.Windows.Forms.Label();
            this.cbStoreLogin = new System.Windows.Forms.CheckBox();
            this.lblSiteUrl = new System.Windows.Forms.Label();
            this.lblSiteType = new System.Windows.Forms.Label();
            this.pHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // bLogin
            // 
            this.bLogin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bLogin.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bLogin.Location = new System.Drawing.Point(254, 245);
            this.bLogin.Name = "bLogin";
            this.bLogin.Size = new System.Drawing.Size(70, 20);
            this.bLogin.TabIndex = 9;
            this.bLogin.Text = "Login";
            this.bLogin.Click += new System.EventHandler(this.OnClickLogin);
            // 
            // lPassword
            // 
            this.lPassword.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lPassword.Location = new System.Drawing.Point(30, 186);
            this.lPassword.Name = "lPassword";
            this.lPassword.Size = new System.Drawing.Size(80, 20);
            this.lPassword.TabIndex = 11;
            this.lPassword.Text = "Password";
            this.lPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lUsername
            // 
            this.lUsername.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lUsername.Location = new System.Drawing.Point(30, 146);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(80, 20);
            this.lUsername.TabIndex = 10;
            this.lUsername.Text = "Username";
            this.lUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbPassword
            // 
            this.tbPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbPassword.Location = new System.Drawing.Point(130, 186);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(270, 22);
            this.tbPassword.TabIndex = 7;
            // 
            // pHeader
            // 
            this.pHeader.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pHeader.Controls.Add(this.lHeader);
            this.pHeader.Controls.Add(this.lText);
            this.pHeader.Controls.Add(this.pbImage);
            this.pHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pHeader.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pHeader.Location = new System.Drawing.Point(0, 0);
            this.pHeader.Name = "pHeader";
            this.pHeader.Size = new System.Drawing.Size(460, 60);
            this.pHeader.TabIndex = 8;
            // 
            // lHeader
            // 
            this.lHeader.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lHeader.Location = new System.Drawing.Point(10, 10);
            this.lHeader.Name = "lHeader";
            this.lHeader.Size = new System.Drawing.Size(360, 20);
            this.lHeader.TabIndex = 3;
            this.lHeader.Text = "Login";
            this.lHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lText
            // 
            this.lText.Location = new System.Drawing.Point(20, 30);
            this.lText.Name = "lText";
            this.lText.Size = new System.Drawing.Size(360, 20);
            this.lText.TabIndex = 4;
            this.lText.Text = "Please enter your Username and Password.";
            this.lText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbImage
            // 
            this.pbImage.Image = ((System.Drawing.Image)(resources.GetObject("pbImage.Image")));
            this.pbImage.Location = new System.Drawing.Point(388, 5);
            this.pbImage.Name = "pbImage";
            this.pbImage.Size = new System.Drawing.Size(48, 48);
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbImage.TabIndex = 5;
            this.pbImage.TabStop = false;
            // 
            // tbUsername
            // 
            this.tbUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbUsername.Location = new System.Drawing.Point(130, 146);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(270, 22);
            this.tbUsername.TabIndex = 6;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // bCancel
            // 
            this.bCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bCancel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bCancel.Location = new System.Drawing.Point(330, 245);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(70, 20);
            this.bCancel.TabIndex = 12;
            this.bCancel.Text = "Cancel";
            this.bCancel.Click += new System.EventHandler(this.OnClickCancel);
            // 
            // lblSiteInfo
            // 
            this.lblSiteInfo.Location = new System.Drawing.Point(49, 84);
            this.lblSiteInfo.Name = "lblSiteInfo";
            this.lblSiteInfo.Size = new System.Drawing.Size(116, 20);
            this.lblSiteInfo.TabIndex = 13;
            this.lblSiteInfo.Text = "Enter login for website:";
            this.lblSiteInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbStoreLogin
            // 
            this.cbStoreLogin.AutoSize = true;
            this.cbStoreLogin.Location = new System.Drawing.Point(206, 212);
            this.cbStoreLogin.Name = "cbStoreLogin";
            this.cbStoreLogin.Size = new System.Drawing.Size(254, 21);
            this.cbStoreLogin.TabIndex = 14;
            this.cbStoreLogin.Text = "Store login as unsecure as possibly";
            this.cbStoreLogin.UseVisualStyleBackColor = true;
            // 
            // lblSiteUrl
            // 
            this.lblSiteUrl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSiteUrl.Location = new System.Drawing.Point(171, 84);
            this.lblSiteUrl.Name = "lblSiteUrl";
            this.lblSiteUrl.Size = new System.Drawing.Size(229, 20);
            this.lblSiteUrl.TabIndex = 15;
            this.lblSiteUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSiteType
            // 
            this.lblSiteType.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSiteType.Location = new System.Drawing.Point(171, 113);
            this.lblSiteType.Name = "lblSiteType";
            this.lblSiteType.Size = new System.Drawing.Size(229, 20);
            this.lblSiteType.TabIndex = 16;
            this.lblSiteType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LoginControl
            // 
            this.Controls.Add(this.lblSiteType);
            this.Controls.Add(this.lblSiteUrl);
            this.Controls.Add(this.cbStoreLogin);
            this.Controls.Add(this.lblSiteInfo);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bLogin);
            this.Controls.Add(this.lPassword);
            this.Controls.Add(this.lUsername);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.pHeader);
            this.Controls.Add(this.tbUsername);
            this.Name = "LoginControl";
            this.Size = new System.Drawing.Size(460, 300);
            this.Load += new System.EventHandler(this.OnLoad);
            this.pHeader.ResumeLayout(false);
            this.pHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion


		/// <summary> 
		/// OnLoad currently only defines the value tbPassword.PasswordChar.
		/// </summary>
		private void OnLoad(object sender, System.EventArgs e)
		{
            LoadLogin();
			tbPassword.PasswordChar = '\u25CF';
			if (bCheck)
			{
				tmpU = theUsername;
				tmpP = thePassword;
			}
            tbUsername.Text = theUsername;
            tbPassword.Text = thePassword;
            bLogin.Focus();
		}

        public static Logins LoadLogins()
        {
            string sConfigFile = Application.UserAppDataPath.Remove(Application.UserAppDataPath.LastIndexOf("\\")) + "\\LoginInfo.xml";
            Logins logins = null;
            try
            {
                logins = disc1.XML.ObjectXMLSerializer<Logins>.Load(sConfigFile);
            }
            catch
            {
                logins = null;
            }
            return logins;
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
            LoginInfo foundLoginInfo = null;
            for (int t = 0; t < logins.LoginInfos.Count; t++)
            {
                LoginInfo tmp = (LoginInfo)logins.LoginInfos[t];
                if (tmp.SiteUrl == lblSiteUrl.Text)
                {
                    foundLoginInfo = tmp;
                    break;
                }
            }
            if (foundLoginInfo != null)
            {
                theUsername = foundLoginInfo.UserName;
                thePassword = foundLoginInfo.Password;
            }
        }

        private void SaveLogin()
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

            for (int t=0; t<logins.LoginInfos.Count; t++)
            {
                LoginInfo tmp = (LoginInfo)logins.LoginInfos[t];
                if (tmp.SiteUrl == lblSiteUrl.Text)
                {
                    logins.LoginInfos.RemoveAt(t);
                }
            }

            logins.LoginInfos.Insert(0, new LoginInfo(lblSiteUrl.Text, theUsername, thePassword, lblSiteType.Text));
//            logins.LoginInfos.Add(new LoginInfo(lblSiteUrl.Text, theUsername, thePassword));

            disc1.XML.ObjectXMLSerializer<Logins>.Save(logins, sConfigFile);
        }

        public static void SaveLogins(Logins logins)
        {
            string sConfigFile = Application.UserAppDataPath.Remove(Application.UserAppDataPath.LastIndexOf("\\")) + "\\LoginInfo.xml";
            disc1.XML.ObjectXMLSerializer<Logins>.Save(logins, sConfigFile);
        }

		private void OnClickLogin(object sender, System.EventArgs e)
		{
			if (!bCheck)
			{
				if (!bHash)
				{
					theUsername = tbUsername.Text;
					thePassword = tbPassword.Text;
					if (Ready != null)
					{
						Ready(this, new System.EventArgs());
					}
				}
				else
				{
					theUsername = HashString(tbUsername.Text);
					thePassword = HashString(tbPassword.Text);
					if (Ready != null)
					{
						Ready(this, new System.EventArgs());
					}
				}
                if (cbStoreLogin.Checked)
                    SaveLogin();
            }
			else
			{
				bool b = CheckCredentials(tmpU, tmpP);
				if (b)
				{
				 if (Ready != null)
				 {
					 Successful(this, new System.EventArgs());
				 }
				}
				else
				{
					if (Ready != null)
					{
						Failed(this, new System.EventArgs());
					}
				}
                if (cbStoreLogin.Checked)
                    SaveLogin();
            }
		}

        private void OnClickCancel(object sender, System.EventArgs e)                
        {
            Failed(this, new System.EventArgs());
        }
	
		/// <summary> 
		/// Public property defining the headline text.
		/// </summary>	
		public string labelHeadline
		{
			get
			{
				return lHeader.Text;
			}
			set
			{
				lHeader.Text = value;
			}
		}

		/// <summary> 
		/// Public property defining the header text.
		/// </summary>	
		public string labelHeader
		{
			get
			{
				return lText.Text;
			}
			set
			{
				lText.Text = value;
			}
		}

		/// <summary> 
		/// Public property defining the label text before the Username textbox.
		/// </summary>	
		public string labelUsername
		{
			get
			{
				return lUsername.Text;
			}
			set
			{
				lUsername.Text = value;
			}
		}

		/// <summary> 
		/// Public property defining the label text before the Password textbox.
		/// </summary>	
		public string labelPassword
		{
			get
			{
				return lPassword.Text;
			}
			set
			{
				lPassword.Text = value;
			}
		}

		/// <summary> 
		/// Public property defining the text on the login button.
		/// </summary>	
		public string labelButton
		{
			get
			{
				return bLogin.Text;
			}
			set
			{
				bLogin.Text = value;
			}
		}

		/// <summary> 
		/// Public property to transfer the Username between the Host Application and LoginControl.
		/// </summary>	
		public string Username
		{
			get
			{
				return theUsername;
			}
			set
			{
				theUsername = value;
			}
	
		}

		/// <summary> 
		/// Public property to transfer the Password between the Host Application and LoginControl.
		/// </summary>	
		public string Password
		{
			get
			{
				return thePassword;
			}
			set
			{
				thePassword = value;
			}
		}

		/// <summary>
		/// Property NoCheck. Defines if the validation of Username and Password is handled by the Host Application (True) or LoginControl (False).
		/// </summary>
		public bool Check
		{
			get
			{
				return bCheck;
			}
			set
			{
				bCheck = value;
			}
		}

		/// <summary>
		/// Property NoHash. Defines if the user data is hashed (False).
		/// </summary>
		public bool Hash
		{
			get
			{
				return bHash;
			}
			set
			{
				bHash= value;
			}
		}

		/// <summary> 
		/// If NoCheck is set to false this method checks Username and Password.
		/// </summary>
		private bool CheckCredentials(string usr, string pwd)
		{
			if (!bHash)
			{
				if (tbUsername.Text == usr)
				{
					if (tbPassword.Text == pwd)
					{
						return true;
					}
					else
					{
						errorProvider.SetError(bLogin, ErrMsg);
						return false;
					}
				}
				else
				{
					errorProvider.SetError(bLogin, ErrMsg);
					return false;
				}
			}
			else
			{
				if (HashString(tbUsername.Text) == HashString(usr))
				{
					if (HashString(tbPassword.Text) == HashString(pwd))
					{
						return true;
					}
					else
					{
						errorProvider.SetError(bLogin, ErrMsg);
						return false;
					}
				}
				else
				{
					errorProvider.SetError(bLogin, ErrMsg);
					return false;
				}
			}
		}
		
		/// <summary>
		/// If NoHash is set to false HashPassword ... well ... hashes the password.
		/// </summary>
		private string HashString(string s)
		{
			UTF8Encoding UTF8 = new UTF8Encoding();
			byte[] data = UTF8.GetBytes(s);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(data);
			return System.Text.Encoding.UTF8.GetString(result);
		}

		/// <summary>
		/// Error message for the ErrorProvider.
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return ErrMsg;
			}
			set
			{
				ErrMsg = value;
			}
		}	
	}


    // Mark class as serializable.
    [Serializable]
    public class LoginInfo
    {
        /// <summary>
        /// Default constructor for this class (required for serialization).
        /// </summary>
        public LoginInfo()
        { 
        }

        public LoginInfo(string sSiteUrl, string sUserName, string sPassword, string sSiteType)
        {
            SiteUrl = sSiteUrl;
            UserName = sUserName;
            Password = sPassword;
            SiteType = sSiteType;
        }

        // Specify that this field should be serialized as an XML attribute 
        // instead of an element to demonstrate the formatting differences in an XML file. 
        [XmlElement]
        public string SiteUrl = "";

        [XmlElement]
        public string UserName = "";

        [XmlElement]
        public string Password = "";

        [XmlElement]
        public string SiteType = "";
    }

    [XmlRootAttribute("Logins", Namespace = "", IsNullable = false)]
    public class Logins
    {
        /// <summary>
        /// Default constructor for this class (required for serialization).
        /// </summary>
        public Logins()
        {
        }

        // Serializes an ArrayList as a "EmailAddresses" array of XML elements of custom type EmailAddress named "EmailAddress".
        [XmlArray("LoginInfos"), XmlArrayItem("LoginInfo", typeof(LoginInfo))]
        public System.Collections.ArrayList LoginInfos = new System.Collections.ArrayList();

    }

}
