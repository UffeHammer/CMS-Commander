using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net;
using System.Xml;
using SitecoreConverter.Core;
using CustomControls;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace SitecoreConverter
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
        private System.Windows.Forms.TextBox tbLeftServerURL;
        private System.Windows.Forms.Button tbLeftConnect;
        private Button tbRightConnect;
        private TextBox tbRightServerURL;
        private ComboBox comboSiteTypeRight;
        private ComboBox comboSiteTypeLeft;
        private Button btnCopyToRight;
        private Button btnDelete;
        private TreeNode _destinationNode = null;
        private IContainer components;
        private IItem _destItem = null;
        private IItem _srcItem = null;
        private ProgressForm _progressForm = null;
        private SplitContainer splitContainer1;
        private TreeView leftTreeView;
        private TreeView rightTreeView;
        private Button btnView;

        private float _fTreeviewScale = -1;

        public delegate void AddNodeItem(object sender, TreeViewEventArgs e);
        public static AddNodeItem _myAddNodeItem;
        private ImageList imageList1;

        public delegate void ShowOverwriteDialog(IItem srcItem, IItem existingItem);
        public ShowOverwriteDialog _delegateShowOverwriteDialog;

        private TreeView _lastSelectedTreeView = null;
        private TreeNode _LastSelectedNode = null;
        private Button btnNewFolder;
        private Button btnFind;
        private IItemCopyPlugin[] _itemCopyPlugins = null;
        private Button btnCompare;
        private Button btnLeftAddressList;
        private Button btnRightAddressList;
        private Button btnInstall;
        private OpenFileDialog openFileDialog1;
        private Button btnTransferFieldsTool;
        private ComboBox comboDatabaseLeft;
        private ComboBox comboDatabaseRight;
        private Button btnLocateOnOtherSide;
        private Button btnRename;
        private Exception _globalException = null;


		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();


            Util.backgroundWorker.WorkerReportsProgress = true;
            Util.backgroundWorker.WorkerSupportsCancellation = true;
            Util.backgroundWorker.DoWork += new DoWorkEventHandler(bw_DoWork);
            Util.backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            Util.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            Util.backgroundWorkerLeftView.WorkerReportsProgress = true;
            Util.backgroundWorkerLeftView.WorkerSupportsCancellation = true;
            Util.backgroundWorkerLeftView.DoWork += new DoWorkEventHandler(bw_DoWork);
            Util.backgroundWorkerLeftView.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            Util.backgroundWorkerLeftView.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            Util.backgroundWorkerRightView.WorkerReportsProgress = true;
            Util.backgroundWorkerRightView.WorkerSupportsCancellation = true;
            Util.backgroundWorkerRightView.DoWork += new DoWorkEventHandler(bw_DoWork);
            Util.backgroundWorkerRightView.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            Util.backgroundWorkerRightView.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            

            _myAddNodeItem = new AddNodeItem(AddNode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // From 8bit ugly to 32bit beautyful
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            this.Text += " (version: " + version + ")";


            _fTreeviewScale = (float)leftTreeView.Width / (float)this.Width;
            _itemCopyPlugins = SitecoreConverter.Core.Plugins.PluginManager.ItemCopyPlugins;
            Logins loginList = LoginControl.LoadLogins();
            if ((loginList != null) && (loginList.LoginInfos.Count > 0))
            {
                tbLeftServerURL.Text = (loginList.LoginInfos[0] as LoginInfo).SiteUrl;
                comboSiteTypeLeft.Text = (loginList.LoginInfos[0] as LoginInfo).SiteType;
            }
                
            if ((loginList != null) && (loginList.LoginInfos.Count > 1))
            {
                tbRightServerURL.Text = (loginList.LoginInfos[1] as LoginInfo).SiteUrl;
                comboSiteTypeRight.Text = (loginList.LoginInfos[1] as LoginInfo).SiteType;
            }


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
        }

        private static DialogResult _DialogResult = System.Windows.Forms.DialogResult.None;
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            else
            {
                if (_DialogResult != System.Windows.Forms.DialogResult.None) 
                {
                    if (_DialogResult == System.Windows.Forms.DialogResult.Yes)
                        return true;
                    else
                        return false;
                }
                else
                {
                _DialogResult = System.Windows.Forms.MessageBox.Show("The server certificate is not valid.\nAccept?", "Certificate Validation", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
                if (_DialogResult == System.Windows.Forms.DialogResult.Yes)
                    return true;
                else
                    return false;
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logins loginList = LoginControl.LoadLogins();
            if ((loginList != null) && (loginList.LoginInfos.Count > 0))
            {
                // Move tbRightServerURL.Text to first in list (will later be moved to second)
                for (int t = 0; t < loginList.LoginInfos.Count; t++)
                {
                    LoginInfo info = loginList.LoginInfos[t] as LoginInfo;
                    if (tbRightServerURL.Text.ToLower() == info.SiteUrl.ToLower())
                    {
                        loginList.LoginInfos.RemoveAt(t);
                        loginList.LoginInfos.Insert(0, info);
                        break;
                    }
                }

                // Move tbLeftServerURL.Text to first in list (will later be moved to second)
                for (int t=0; t<loginList.LoginInfos.Count; t++)
                {
                    LoginInfo info = loginList.LoginInfos[t] as LoginInfo;
                    if (tbLeftServerURL.Text.ToLower() == info.SiteUrl.ToLower())
                    {
                        loginList.LoginInfos.RemoveAt(t);
                        loginList.LoginInfos.Insert(0, info);
                        break;
                    }
                }
            }
            LoginControl.SaveLogins(loginList);
        }


        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (_fTreeviewScale != -1)
            {
//                leftTreeView.Width = (int)Math.Round(this.Width * _fTreeviewScale);
//                rightTreeView.Width = (int)Math.Round(this.Width * _fTreeviewScale);
            }
        }


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tbLeftServerURL = new System.Windows.Forms.TextBox();
            this.tbLeftConnect = new System.Windows.Forms.Button();
            this.tbRightConnect = new System.Windows.Forms.Button();
            this.tbRightServerURL = new System.Windows.Forms.TextBox();
            this.comboSiteTypeRight = new System.Windows.Forms.ComboBox();
            this.comboSiteTypeLeft = new System.Windows.Forms.ComboBox();
            this.btnCopyToRight = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.leftTreeView = new System.Windows.Forms.TreeView();
            this.rightTreeView = new System.Windows.Forms.TreeView();
            this.btnView = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnNewFolder = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnCompare = new System.Windows.Forms.Button();
            this.btnLeftAddressList = new System.Windows.Forms.Button();
            this.btnRightAddressList = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnTransferFieldsTool = new System.Windows.Forms.Button();
            this.comboDatabaseLeft = new System.Windows.Forms.ComboBox();
            this.comboDatabaseRight = new System.Windows.Forms.ComboBox();
            this.btnLocateOnOtherSide = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbLeftServerURL
            // 
            this.tbLeftServerURL.Location = new System.Drawing.Point(198, 37);
            this.tbLeftServerURL.Name = "tbLeftServerURL";
            this.tbLeftServerURL.Size = new System.Drawing.Size(147, 20);
            this.tbLeftServerURL.TabIndex = 0;
            this.tbLeftServerURL.Text = "http://";
            this.tbLeftServerURL.TextChanged += new System.EventHandler(this.tbLeftServerURL_TextChanged);
            // 
            // tbLeftConnect
            // 
            this.tbLeftConnect.Location = new System.Drawing.Point(345, 35);
            this.tbLeftConnect.Name = "tbLeftConnect";
            this.tbLeftConnect.Size = new System.Drawing.Size(62, 20);
            this.tbLeftConnect.TabIndex = 2;
            this.tbLeftConnect.Text = "Connect";
            this.tbLeftConnect.Click += new System.EventHandler(this.tbLeftConnect_Click);
            // 
            // tbRightConnect
            // 
            this.tbRightConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRightConnect.Location = new System.Drawing.Point(1013, 34);
            this.tbRightConnect.Name = "tbRightConnect";
            this.tbRightConnect.Size = new System.Drawing.Size(60, 20);
            this.tbRightConnect.TabIndex = 4;
            this.tbRightConnect.Text = "Connect";
            this.tbRightConnect.Click += new System.EventHandler(this.tbRightConnect_Click);
            // 
            // tbRightServerURL
            // 
            this.tbRightServerURL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRightServerURL.Location = new System.Drawing.Point(868, 35);
            this.tbRightServerURL.Name = "tbRightServerURL";
            this.tbRightServerURL.Size = new System.Drawing.Size(145, 20);
            this.tbRightServerURL.TabIndex = 8;
            this.tbRightServerURL.Text = "http://";
            // 
            // comboSiteTypeRight
            // 
            this.comboSiteTypeRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSiteTypeRight.FormattingEnabled = true;
            this.comboSiteTypeRight.Items.AddRange(new object[] {
            "Sitecore43",
            "Sitecore5x",
            "Sitecore6x-8x"});
            this.comboSiteTypeRight.Location = new System.Drawing.Point(677, 36);
            this.comboSiteTypeRight.Name = "comboSiteTypeRight";
            this.comboSiteTypeRight.Size = new System.Drawing.Size(90, 21);
            this.comboSiteTypeRight.TabIndex = 9;
            this.comboSiteTypeRight.Text = "Sitecore6x-8x";
            // 
            // comboSiteTypeLeft
            // 
            this.comboSiteTypeLeft.FormattingEnabled = true;
            this.comboSiteTypeLeft.Items.AddRange(new object[] {
            "Sitecore43",
            "Sitecore5x",
            "Sitecore6x-8x"});
            this.comboSiteTypeLeft.Location = new System.Drawing.Point(8, 36);
            this.comboSiteTypeLeft.Name = "comboSiteTypeLeft";
            this.comboSiteTypeLeft.Size = new System.Drawing.Size(89, 21);
            this.comboSiteTypeLeft.TabIndex = 10;
            this.comboSiteTypeLeft.Text = "Sitecore43";
            this.comboSiteTypeLeft.SelectedIndexChanged += new System.EventHandler(this.comboSiteTypeLeft_SelectedIndexChanged);
            // 
            // btnCopyToRight
            // 
            this.btnCopyToRight.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCopyToRight.Location = new System.Drawing.Point(102, 9);
            this.btnCopyToRight.Name = "btnCopyToRight";
            this.btnCopyToRight.Size = new System.Drawing.Size(90, 20);
            this.btnCopyToRight.TabIndex = 11;
            this.btnCopyToRight.Text = "F5 Copy -->";
            this.btnCopyToRight.UseVisualStyleBackColor = true;
            this.btnCopyToRight.Click += new System.EventHandler(this.btnCopyToRight_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDelete.Location = new System.Drawing.Point(388, 8);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(88, 20);
            this.btnDelete.TabIndex = 12;
            this.btnDelete.Text = "F8 Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(8, 63);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.leftTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rightTreeView);
            this.splitContainer1.Size = new System.Drawing.Size(1087, 508);
            this.splitContainer1.SplitterDistance = 513;
            this.splitContainer1.TabIndex = 13;
            this.splitContainer1.TabStop = false;
            // 
            // leftTreeView
            // 
            this.leftTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.leftTreeView.FullRowSelect = true;
            this.leftTreeView.HideSelection = false;
            this.leftTreeView.ItemHeight = 18;
            this.leftTreeView.Location = new System.Drawing.Point(3, 5);
            this.leftTreeView.Name = "leftTreeView";
            this.leftTreeView.Size = new System.Drawing.Size(510, 503);
            this.leftTreeView.TabIndex = 7;
            this.leftTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.leftTreeView_AfterExpand);
            this.leftTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.leftTreeView_AfterSelect);
            this.leftTreeView.Enter += new System.EventHandler(this.leftTreeView_Enter);
            // 
            // rightTreeView
            // 
            this.rightTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rightTreeView.FullRowSelect = true;
            this.rightTreeView.HideSelection = false;
            this.rightTreeView.Location = new System.Drawing.Point(2, 6);
            this.rightTreeView.Name = "rightTreeView";
            this.rightTreeView.Size = new System.Drawing.Size(568, 502);
            this.rightTreeView.TabIndex = 8;
            this.rightTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.rightTreeView_AfterExpand);
            this.rightTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.rightTreeView_AfterSelect);
            this.rightTreeView.Enter += new System.EventHandler(this.rightTreeView_Enter);
            // 
            // btnView
            // 
            this.btnView.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnView.Location = new System.Drawing.Point(9, 9);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(88, 20);
            this.btnView.TabIndex = 14;
            this.btnView.Text = "F3 View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "icon_dark_sitemap_16.gif");
            this.imageList1.Images.SetKeyName(1, "icon_folder_16.gif");
            // 
            // btnNewFolder
            // 
            this.btnNewFolder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnNewFolder.Location = new System.Drawing.Point(294, 8);
            this.btnNewFolder.Name = "btnNewFolder";
            this.btnNewFolder.Size = new System.Drawing.Size(88, 20);
            this.btnNewFolder.TabIndex = 15;
            this.btnNewFolder.Text = "F7 NewFolder";
            this.btnNewFolder.UseVisualStyleBackColor = true;
            this.btnNewFolder.Click += new System.EventHandler(this.btnNewFolder_Click);
            // 
            // btnFind
            // 
            this.btnFind.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFind.Location = new System.Drawing.Point(482, 9);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(89, 20);
            this.btnFind.TabIndex = 16;
            this.btnFind.Text = "Alt + F7 Find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCompare.Location = new System.Drawing.Point(577, 8);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(88, 20);
            this.btnCompare.TabIndex = 17;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // btnLeftAddressList
            // 
            this.btnLeftAddressList.Location = new System.Drawing.Point(413, 34);
            this.btnLeftAddressList.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnLeftAddressList.Name = "btnLeftAddressList";
            this.btnLeftAddressList.Size = new System.Drawing.Size(18, 20);
            this.btnLeftAddressList.TabIndex = 18;
            this.btnLeftAddressList.Text = "*";
            this.btnLeftAddressList.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLeftAddressList.UseVisualStyleBackColor = true;
            this.btnLeftAddressList.Click += new System.EventHandler(this.btnLeftAddressList_Click);
            // 
            // btnRightAddressList
            // 
            this.btnRightAddressList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRightAddressList.Location = new System.Drawing.Point(1079, 35);
            this.btnRightAddressList.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnRightAddressList.Name = "btnRightAddressList";
            this.btnRightAddressList.Size = new System.Drawing.Size(15, 20);
            this.btnRightAddressList.TabIndex = 19;
            this.btnRightAddressList.Text = "*";
            this.btnRightAddressList.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRightAddressList.UseVisualStyleBackColor = true;
            this.btnRightAddressList.Click += new System.EventHandler(this.btnRightAddressList_Click);
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnInstall.Location = new System.Drawing.Point(1013, 8);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(88, 20);
            this.btnInstall.TabIndex = 20;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "*.zip";
            // 
            // btnTransferFieldsTool
            // 
            this.btnTransferFieldsTool.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTransferFieldsTool.Location = new System.Drawing.Point(919, 8);
            this.btnTransferFieldsTool.Name = "btnTransferFieldsTool";
            this.btnTransferFieldsTool.Size = new System.Drawing.Size(88, 20);
            this.btnTransferFieldsTool.TabIndex = 21;
            this.btnTransferFieldsTool.Text = "Transfer fields tool";
            this.btnTransferFieldsTool.UseVisualStyleBackColor = true;
            this.btnTransferFieldsTool.Visible = false;
            this.btnTransferFieldsTool.Click += new System.EventHandler(this.btnTransferFieldsTool_Click);
            // 
            // comboDatabaseLeft
            // 
            this.comboDatabaseLeft.FormattingEnabled = true;
            this.comboDatabaseLeft.Items.AddRange(new object[] {
            "master",
            "web",
            "core"});
            this.comboDatabaseLeft.Location = new System.Drawing.Point(103, 36);
            this.comboDatabaseLeft.Name = "comboDatabaseLeft";
            this.comboDatabaseLeft.Size = new System.Drawing.Size(89, 21);
            this.comboDatabaseLeft.TabIndex = 22;
            this.comboDatabaseLeft.Text = "master";
            // 
            // comboDatabaseRight
            // 
            this.comboDatabaseRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboDatabaseRight.FormattingEnabled = true;
            this.comboDatabaseRight.Items.AddRange(new object[] {
            "master",
            "web",
            "core"});
            this.comboDatabaseRight.Location = new System.Drawing.Point(773, 35);
            this.comboDatabaseRight.Name = "comboDatabaseRight";
            this.comboDatabaseRight.Size = new System.Drawing.Size(89, 21);
            this.comboDatabaseRight.TabIndex = 23;
            this.comboDatabaseRight.Text = "master";
            // 
            // btnLocateOnOtherSide
            // 
            this.btnLocateOnOtherSide.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLocateOnOtherSide.Location = new System.Drawing.Point(671, 8);
            this.btnLocateOnOtherSide.Name = "btnLocateOnOtherSide";
            this.btnLocateOnOtherSide.Size = new System.Drawing.Size(116, 20);
            this.btnLocateOnOtherSide.TabIndex = 25;
            this.btnLocateOnOtherSide.Text = "Locate opposite side";
            this.btnLocateOnOtherSide.UseVisualStyleBackColor = true;
            this.btnLocateOnOtherSide.Click += new System.EventHandler(this.btnLocateWithLeft_Click);
            // 
            // btnRename
            // 
            this.btnRename.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRename.Location = new System.Drawing.Point(198, 8);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(90, 20);
            this.btnRename.TabIndex = 26;
            this.btnRename.Text = "F2 Rename";
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1105, 583);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnLocateOnOtherSide);
            this.Controls.Add(this.comboDatabaseRight);
            this.Controls.Add(this.comboDatabaseLeft);
            this.Controls.Add(this.btnTransferFieldsTool);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnRightAddressList);
            this.Controls.Add(this.btnLeftAddressList);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.btnNewFolder);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCopyToRight);
            this.Controls.Add(this.tbLeftServerURL);
            this.Controls.Add(this.comboSiteTypeLeft);
            this.Controls.Add(this.comboSiteTypeRight);
            this.Controls.Add(this.tbRightServerURL);
            this.Controls.Add(this.tbRightConnect);
            this.Controls.Add(this.tbLeftConnect);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(583, 347);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "CMS Commander";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

        private void ConnectSitecore43(TreeView treeView, string sServerUrl, string sSiteType, string sDatabase)
        {
            SitecoreConverter.Core.Sitecore43.SitecoreClientAPI sitecoreApi = new SitecoreConverter.Core.Sitecore43.SitecoreClientAPI();
            sitecoreApi.Url = sServerUrl + "/sitecore/client/api/api.asmx";
            sitecoreApi.Credentials = CredentialCache.DefaultCredentials; //new System.Net.NetworkCredential();
            sitecoreApi.CookieContainer = new CookieContainer();
            sitecoreApi.UnsafeAuthenticatedConnectionSharing = true;
            sitecoreApi.Timeout = 120 * 1000;

            string sLoginName = null;
            string sLoginPassword = null;
            if (!sitecoreApi.IsLoggedIn())
            {
                LoginForm login = new LoginForm();
                login.SiteUrl = sServerUrl;
                login.SiteType = sSiteType;
                login.Username = "Admin";
                if (login.ShowDialog(this) == DialogResult.Cancel)
                    return;
                try
                {
                    sitecoreApi.Login(login.loginControl1.Username, login.loginControl1.Password);
                    sLoginName = login.loginControl1.Username;
                    sLoginPassword = login.loginControl1.Password;
                }
                catch
                {
                    MessageBox.Show("Error logging into site, wrong username or password?");
                    return;
                }
            }
            Sitecore43Item item = Sitecore43Item.GetItem("/sitecore", sitecoreApi);
            item.Options.LoginName = sLoginName;
            item.Options.LoginPassword = sLoginPassword;
            item.Options.HostName = sServerUrl;
            item.Options.Database = sDatabase;

            treeView.Nodes.Clear();
            TreeNode newNode = new TreeNode(item.Name);
            newNode.Tag = item;
            treeView.Nodes.Add(newNode);
            treeView.SelectedNode = newNode;
        }

        private IItem ConnectSitecore61(TreeView treeView, string sServerUrl, string sSiteType, string sDatabase)
        {
            SitecoreConverter.Core.Sitecore61.VisualSitecoreService sitecoreApi = new SitecoreConverter.Core.Sitecore61.VisualSitecoreService();
            sitecoreApi.Url = sServerUrl + "/sitecore/shell/webservice/service.asmx";
            SitecoreConverter.Core.Sitecore61.Credentials credential = new SitecoreConverter.Core.Sitecore61.Credentials();
            sitecoreApi.CookieContainer = new CookieContainer();
            // Timeout = 3000 seconds (default 10)
            sitecoreApi.Timeout = 3000000;
            sitecoreApi.EnableDecompression = true;

            try
            {
                sitecoreApi.VerifyCredentials(credential);
            }
            catch (System.Net.WebException ex)
            {
                
                if ((ex.Status == System.Net.WebExceptionStatus.ProtocolError))
                {
                    LoginForm BasicAuthLogin = new LoginForm();
                    BasicAuthLogin.SiteUrl = sServerUrl + "/Basic authentication";
                    BasicAuthLogin.SiteType = "Basic authentication";
                    BasicAuthLogin.Username = "";
                    BasicAuthLogin.Password = "";
                    if (BasicAuthLogin.ShowDialog(this) == DialogResult.Cancel)
                        return null;

                    CredentialCache credCache = new CredentialCache();
                    NetworkCredential netCred = new NetworkCredential(BasicAuthLogin.Username, BasicAuthLogin.Password);
                    credCache.Add(new Uri(sitecoreApi.Url), "Basic", netCred);
                    sitecoreApi.Credentials = credCache;
                    sitecoreApi.PreAuthenticate = true;
                }
            }

            LoginForm login = new LoginForm();
            login.SiteUrl = sServerUrl;
            login.SiteType = sSiteType;
            login.Username = "Admin";
            if (login.ShowDialog(this) == DialogResult.Cancel)
                return null;
            credential.UserName = "sitecore\\" + login.loginControl1.Username;
            credential.Password = login.loginControl1.Password;

            ConverterOptions Options = new ConverterOptions();
            Options.LoginName = credential.UserName;
            Options.LoginPassword = credential.Password;
            Options.HostName = sServerUrl;
            Options.Database = sDatabase;

            Sitecore6xItem item = null;
            try
            {
                item = Sitecore6xItem.GetItem("/sitecore", sitecoreApi, credential, Options);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error logging into site, wrong username or password?\nError message: " + ex.Message);
                return null;
            }

            if (item.ExtendedWebService != null)
            {
                try
                {
                    item.ExtendedWebService.Discover();
                }
                catch (Exception ex)
                {
                    //  \nError message:\n " + ex.Message
                    MessageBox.Show("Error connecting to custom CMS Commander webservice, is it installed?\n\n" + 
                                    "Please install it using the \"My Documents\\CMS Commander\\Sitecore webservice installer.zip\" package.");
                    return null;
                }
            }

            if (treeView != null)
            {
                treeView.Nodes.Clear();
                TreeNode newNode = new TreeNode(item.Name);
                newNode.Tag = item;
                treeView.Nodes.Add(newNode);
                treeView.SelectedNode = newNode;
            }
            return item;
        }

        private void ConnectSitecore5x(TreeView treeView, string sServerUrl, string sSiteType, string sDatabase)
        {
            SitecoreConverter.Core.Sitecore5x.VisualSitecoreService sitecoreApi = new SitecoreConverter.Core.Sitecore5x.VisualSitecoreService();
            sitecoreApi.Url = sServerUrl + "/sitecore/shell/webservice/service.asmx";
            sitecoreApi.Credentials = CredentialCache.DefaultCredentials; //new System.Net.NetworkCredential();
            SitecoreConverter.Core.Sitecore5x.Credentials credential = new SitecoreConverter.Core.Sitecore5x.Credentials();
            sitecoreApi.CookieContainer = new CookieContainer();
            sitecoreApi.UnsafeAuthenticatedConnectionSharing = true;
            // Timeout = 30 seconds (default 10)
            sitecoreApi.Timeout = 300000;



            LoginForm login = new LoginForm();
            login.SiteUrl = sServerUrl;
            login.SiteType = sSiteType;
            login.Username = "Admin";
            if (login.ShowDialog(this) == DialogResult.Cancel)
                return;
            credential.UserName = login.loginControl1.Username;
            credential.Password = login.loginControl1.Password;


            ConverterOptions Options = new ConverterOptions();
            Options.LoginName = credential.UserName;
            Options.LoginPassword = credential.Password;
            Options.HostName = sServerUrl;
            Options.Database = sDatabase;

            Sitecore5xItem item = null;
            try
            {
                item = Sitecore5xItem.GetItem("/sitecore", sitecoreApi, credential, Options);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error logging into site, wrong username or password?\nError message: " + ex.Message);
                return;
            }


            treeView.Nodes.Clear();
            TreeNode newNode = new TreeNode(item.Name);
            newNode.Tag = item;
            treeView.Nodes.Add(newNode);
            treeView.SelectedNode = newNode;
        }


        private void ConnectUmbraco6x(TreeView treeView, string sServerUrl, string sSiteType)
        {           
            LoginForm login = new LoginForm();
            login.SiteUrl = sServerUrl;
            login.SiteType = sSiteType;
            login.Username = "Admin";
            if (login.ShowDialog(this) == DialogResult.Cancel)
                return;

            Credentials credentials = new Credentials();
            credentials.UserName = login.loginControl1.Username;
            credentials.Password = login.loginControl1.Password;


            Umbraco6xItem item = null;
            try
            {
                Umbraco6xAPI umbracoAPI = new Umbraco6xAPI(sServerUrl, credentials);
                item = Umbraco6xItem.GetRoot(umbracoAPI, new ConverterOptions());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error logging into site, wrong username or password?\nError message: " + ex.Message);
                return;
            }
            item.Options.LoginName = credentials.UserName;
            item.Options.LoginPassword = credentials.Password;
            item.Options.HostName = sServerUrl;

            treeView.Nodes.Clear();
            TreeNode newNode = new TreeNode(item.Name);
            newNode.Tag = item;
            treeView.Nodes.Add(newNode);
            treeView.SelectedNode = newNode;
        }



		private void tbLeftConnect_Click(object sender, System.EventArgs e)
		{
            if (comboSiteTypeLeft.Text == "Sitecore43")
            {
                ConnectSitecore43(leftTreeView, tbLeftServerURL.Text, comboSiteTypeLeft.Text, comboDatabaseLeft.Text);
            }
            else if (comboSiteTypeLeft.Text.Contains("Sitecore6x"))
            {
                ConnectSitecore61(leftTreeView, tbLeftServerURL.Text, comboSiteTypeLeft.Text, comboDatabaseLeft.Text);
            }
            else if (comboSiteTypeLeft.Text == "Sitecore5x")
            {
                ConnectSitecore5x(leftTreeView, tbLeftServerURL.Text, comboSiteTypeLeft.Text, comboDatabaseLeft.Text);
            }            
            else if (comboSiteTypeLeft.Text == "Umbraco6x")
            {
                ConnectUmbraco6x(leftTreeView, tbLeftServerURL.Text, comboSiteTypeLeft.Text);
            }                        
		}

        private void tbRightConnect_Click(object sender, EventArgs e)
        {
            if (comboSiteTypeRight.Text == "Sitecore43")
            {
                ConnectSitecore43(rightTreeView, tbRightServerURL.Text, comboSiteTypeRight.Text, comboDatabaseRight.Text);
            }
            else if (comboSiteTypeRight.Text.Contains("Sitecore6x"))
            {
                ConnectSitecore61(rightTreeView, tbRightServerURL.Text, comboSiteTypeRight.Text, comboDatabaseRight.Text);
            }
            else if (comboSiteTypeRight.Text == "Sitecore5x")
            {
                ConnectSitecore5x(rightTreeView, tbRightServerURL.Text, comboSiteTypeRight.Text, comboDatabaseRight.Text);
            }
            else if (comboSiteTypeRight.Text == "Umbraco6x")
            {
                ConnectUmbraco6x(rightTreeView, tbRightServerURL.Text, comboSiteTypeRight.Text);
            }            
        }



        private void AddNode(object sender, TreeViewEventArgs e)
        {
            // TreeViewEventArgs e = args[0] as TreeViewEventArgs;
            // e.Node.Nodes.Add(args[1] as TreeNode);
            TreeNode node = sender as TreeNode;
            IItem item = node.Tag as IItem;
            string sIcon = item.Icon;
            if ((sIcon != "") && (sIcon.IndexOf("/") != 0) && (sIcon.IndexOf("://") == -1))
                sIcon = "/temp/IconCache/" + sIcon;

            // Set node icon
            if ((sIcon != "") && (e.Node.TreeView != null))
            {
                TreeView view = e.Node.TreeView;
                if (view.ImageList == null)
                {
                    view.ImageList = new ImageList();
                    view.ImageList.ColorDepth = ColorDepth.Depth32Bit;
                    view.ImageList.Images.Add(imageList1.Images[1]);
                }

                if (view.ImageList.Images.ContainsKey(sIcon))
                {
                    node.ImageIndex = view.ImageList.Images.IndexOfKey(sIcon);
                    node.SelectedImageIndex = node.ImageIndex;
                }
                else
                {
                    Uri host = new Uri(item.GetHostUrl());
                    try
                    {
                        Bitmap bitmap = null;
                        if (!host.Host.EndsWith("/") && (!sIcon.StartsWith("/")))
                            bitmap = Util.LoadPicture(host.Scheme + "://" + host.Host + "/" + sIcon);
                        else
                            bitmap = Util.LoadPicture(host.Scheme + "://" + host.Host + sIcon);

                        view.ImageList.Images.Add(sIcon, bitmap);
                        node.ImageIndex = view.ImageList.Images.IndexOfKey(sIcon);
                        node.SelectedImageIndex = node.ImageIndex;
                    }
                    catch
                    {
                        node.ImageIndex = -1;
                    }
                }
            }

            if (!e.Node.Nodes.ContainsKey(item.ID))
            {
                e.Node.Nodes.Add(node);
                node.EnsureVisible();
                if (item.HasChildren())
                    node.Nodes.Add("dummy", "");
            }

        }

        private void ExpandNode(object sender, TreeViewEventArgs e)
        {
            TreeNode node = sender as TreeNode;
            if (!node.IsExpanded)
                node.Expand();
        }

        private void TreeView_AfterSelectAsync(object sender, TreeViewEventArgs e)
        {
            IItem item = e.Node.Tag as IItem;
            
            IItem[] children = item.GetChildren();
            foreach (IItem child in children)
            {
                TreeNode newNode = new TreeNode(child.Name);
                newNode.Tag = child;
                newNode.Name = child.ID; 
                ((Control)sender).Invoke(new AddNodeItem(AddNode), new object[] { newNode, e });
            }
        }


        private void leftTreeView_Enter(object sender, EventArgs e)
        {
            _lastSelectedTreeView = leftTreeView;
        }

        private void leftTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _lastSelectedTreeView = leftTreeView;
            IItem item = e.Node.Tag as IItem;

            if (leftTreeView.SelectedNode.Nodes["dummy"] != null)
                leftTreeView.SelectedNode.Nodes["dummy"].Remove();

            if (leftTreeView.SelectedNode.Nodes.Count > 0)
                return;

            if (! Util.backgroundWorkerLeftView.IsBusy)
                Util.backgroundWorkerLeftView.RunWorkerAsync(new BackgroundWorkerArgument("expandnode", new TreeViewEventHandler(this.TreeView_AfterSelectAsync), e, leftTreeView));
/*
            IItem[] children = item.GetChildren();
            foreach (IItem child in children)
            {
                TreeNode newNode = new TreeNode(child.Name);
                newNode.Tag = child;
                leftTreeView.SelectedNode.Nodes.Add(newNode);
            }
 */ 
        }

        private void rightTreeView_Enter(object sender, EventArgs e)
        {
            _lastSelectedTreeView = rightTreeView;
        }

        private void rightTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            rightTreeView.SelectedNode = e.Node;
        }

        private void leftTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            leftTreeView.SelectedNode = e.Node;
        }

        private void rightTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _lastSelectedTreeView = rightTreeView;
            IItem item = e.Node.Tag as IItem;

            if (rightTreeView.SelectedNode.Nodes["dummy"] != null)
                rightTreeView.SelectedNode.Nodes["dummy"].Remove();

            if (rightTreeView.SelectedNode.Nodes.Count > 0)
                return;

            if (!Util.backgroundWorkerRightView.IsBusy)
                Util.backgroundWorkerRightView.RunWorkerAsync(new BackgroundWorkerArgument("expandnode", new TreeViewEventHandler(this.TreeView_AfterSelectAsync), e, rightTreeView));

/*
            IItem[] children = item.GetChildren();
            foreach (IItem child in children)
            {
                TreeNode newNode = new TreeNode(child.Name);
                newNode.Tag = child;
                rightTreeView.SelectedNode.Nodes.Add(newNode);
            }
 */ 
        }

        private void FindNode(TreeNode treeNode, IItem findItem, ref TreeNode foundTreeNode)
        {
            IItem item = treeNode.Tag as IItem;
            if (item.ID == findItem.ID)
            {
                foundTreeNode = treeNode;
                return;
            }
            if (foundTreeNode != null)
                return;
            
            for (int t = 0; t < treeNode.Nodes.Count; t++)
            {
                item = treeNode.Nodes[t].Tag as IItem;
                // Item might be zero because of the dummy item
                if (item == null)
                    continue;
                if (item.ID == findItem.ID)
                {
                    foundTreeNode = treeNode.Nodes[t];
                    return;
                }
                if (treeNode.Nodes[t].Nodes.Count > 0)
                    FindNode(treeNode.Nodes[t], findItem, ref foundTreeNode);
            }
        }

        /// <summary>
        /// Callback from IItem copy operation
        /// </summary>
        private void CopyItem(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)        
        {
            TreeNode newNode = new TreeNode(destinationItem.Name);
            newNode.Tag = destinationItem;
            newNode.Name = destinationItem.ID;
            
            // This is not a visual item (template?)
            if (destinationParentItem != null)
            {
                TreeNode parentTreeNode = null;
                FindNode(_destinationNode, destinationParentItem, ref parentTreeNode);
                if (parentTreeNode == null)
                    parentTreeNode = _destinationNode;
                TreeViewEventArgs e = new TreeViewEventArgs(parentTreeNode);


                if (!parentTreeNode.Nodes.ContainsKey(destinationItem.ID))
                {
                    _destinationNode.TreeView.Invoke(new AddNodeItem(AddNode), new object[] { newNode, e });
                }
                //            else
                //                _destinationNode.TreeView.Invoke(new AddNodeItem(ExpandNode), new object[] { parentTreeNode, e });            
            }

            for (int t=0; t<_itemCopyPlugins.Length; t++)
            {
                _itemCopyPlugins[t].CopyItemCallback(sourceItem, destinationParentItem, destinationItem);
            }

//            _destinationNode = newNode;
        }

        /// <summary>
        /// Callback from IItem copy operation
        /// </summary>
        private bool ShouldItemBeCopied(IItem sourceItem, IItem destinationParentItem)
        {
            bool bResult = true;
            // This is not a visual item (template?)
            if (destinationParentItem != null)
            {
                for (int t = 0; t < _itemCopyPlugins.Length; t++)
                {
                    if (!_itemCopyPlugins[t].ShouldItemBeCopiedCallback(sourceItem, destinationParentItem))
                        bResult = false;
                }
            }
            return bResult;
        }


        private void btnCopyToRight_Click(object sender, EventArgs e)
        {
            if (rightTreeView.SelectedNode == null)
            {
                MessageBox.Show("Select destination node in right window", "My Application", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                return;
            }

            if (leftTreeView.SelectedNode == null)
            {
                MessageBox.Show("Select source node in left window", "My Application", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                return;
            }


/*
            IItem destItem = rightTreeView.SelectedNode.Tag as IItem;
            IItem srcItem = leftTreeView.SelectedNode.Tag as IItem;
            destItem.CopyTo(srcItem, true);
*/

            _srcItem = leftTreeView.SelectedNode.Tag as IItem;
            _destItem = rightTreeView.SelectedNode.Tag as IItem;
            _destinationNode = rightTreeView.SelectedNode;

            CopyOptions CopyOptionsForm = new CopyOptions();
            string[] sLanguages = _srcItem.GetLanguages();
            foreach (string sLang in sLanguages)
            {
                CopyOptionsForm.comboFromLanguage.Items.Add(sLang);
            }

            sLanguages = _destItem.GetLanguages();
            foreach (string sLang in sLanguages)
            {
                CopyOptionsForm.comboToLanguage.Items.Add(sLang);
            }
            CopyOptionsForm.comboToLanguage.SelectedIndex = 0;
            CopyOptionsForm.lblToPath.Text = "(" + new Uri(_destItem.GetHostUrl()).Host + ")" + _destItem.Path;

            // Setting this AFTER the destination, enables autoselect of correct destination language
            CopyOptionsForm.comboFromLanguage.SelectedIndex = 0;
            CopyOptionsForm.lblFromPath.Text = "(" + new Uri(_srcItem.GetHostUrl()).Host + ")" + _srcItem.Path;

            // Plugins
            CopyOptionsForm._itemCopyPlugins = _itemCopyPlugins;

            // Show Copyoptions
            if (CopyOptionsForm.ShowDialog(this) == DialogResult.Cancel)
                return;

            // Get copyOptions result
            _srcItem.Options.Language = CopyOptionsForm.comboFromLanguage.Text;
            _destItem.Options.Language = CopyOptionsForm.comboToLanguage.Text;
            _destItem.Options.CopyItem = CopyItem;
            _destItem.Options.ShouldItemBeCopied = ShouldItemBeCopied;
            if (CopyOptionsForm.rbOverwrite.Checked)
                _destItem.Options.CopyOperation = CopyOperations.Overwrite;
            else if (CopyOptionsForm.rbSkipExisting.Checked)
                _destItem.Options.CopyOperation = CopyOperations.SkipExisting;
            else if (CopyOptionsForm.rbCreateNewItemIDs.Checked)
                _destItem.Options.CopyOperation = CopyOperations.GenerateNewItemIDs;
            else if (CopyOptionsForm.rbUseNames.Checked)
                _destItem.Options.CopyOperation = CopyOperations.UseNames;
            _destItem.Options.RecursiveOperation = CopyOptionsForm.cbRecursive.Checked;
            _destItem.Options.CopyOnlyChildren = CopyOptionsForm.cbOnlyChildren.Checked;
            _destItem.Options.IgnoreErrors = CopyOptionsForm.cbIgnoreErrors.Checked;

//            _srcItem.Options.Database = CopyOptionsForm.tbFromDatabase.Text;
//            _destItem.Options.Database = CopyOptionsForm.tbToDatabase.Text;
            
            // Security settings
            _srcItem.Options.CopySecuritySettings = CopyOptionsForm.cbCopySecurity.Checked;
            _destItem.Options.CopySecuritySettings = CopyOptionsForm.cbCopySecurity.Checked;
            _destItem.Options.DefaultSecurityDomain = CopyOptionsForm.tbSecurityDomain.Text;
            _destItem.Options.RootRole = CopyOptionsForm.tbRootRole.Text;
            _destItem.Options.SetItemRightsExplicitly = CopyOptionsForm.cbSetItemRightsExplicitly.Checked;

            Sitecore6xItem item = _destItem as Sitecore6xItem;
//            if (item != null)
//                item.ExtendedWebService.EnableIndexing(false);

            Util.backgroundWorker.RunWorkerAsync(new BackgroundWorkerArgument("copy", null, null, this));

            _progressForm = new ProgressForm();
            _progressForm.ShowDialog(this);

//            if (item != null)
//                item.ExtendedWebService.EnableIndexing(true);
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if ((_lastSelectedTreeView == null) || (_lastSelectedTreeView.SelectedNode == null))
            {
                MessageBox.Show("Please select an item first.");
                return;
            }

            IItem selItem = _lastSelectedTreeView.SelectedNode.Tag as IItem;
            SinglelineInputForm createFolderForm = new SinglelineInputForm();
            createFolderForm.tbResult.Text = selItem.Name;
            createFolderForm.lblInput.Text = "Enter new name of item";
            createFolderForm.ShowDialog(this);
            if (createFolderForm.OkClicked)
            {
                selItem.Rename(createFolderForm.tbResult.Text);

                _lastSelectedTreeView.SelectedNode.Text = createFolderForm.tbResult.Text;
            }

        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Cancel;
            IItem destItem = null;
            if ((leftTreeView == _lastSelectedTreeView) && (leftTreeView.SelectedNode != null))
            {
                destItem = leftTreeView.SelectedNode.Tag as IItem;
                result = MessageBox.Show("Delete item: " + destItem.Name + " from \"" + tbLeftServerURL.Text + "\"", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }

            if ((rightTreeView == _lastSelectedTreeView) &&  (rightTreeView.SelectedNode != null))
            {
                destItem = rightTreeView.SelectedNode.Tag as IItem;
                result = MessageBox.Show("Delete item: " + destItem.Name + " from \"" + tbRightServerURL.Text + "\"", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }

            if ((result == DialogResult.OK) && (destItem != null))
            {
                destItem.Delete();
                _lastSelectedTreeView.SelectedNode.Remove();
            }

        }

        private void btnNewFolder_Click(object sender, EventArgs e)
        {
            if ((_lastSelectedTreeView == null) || (_lastSelectedTreeView.SelectedNode == null))
            {
                MessageBox.Show("Please select an item first.");
                return;
            }


            SinglelineInputForm createFolderForm = new SinglelineInputForm();
            createFolderForm.lblInput.Text = "Enter name of new folder";
            createFolderForm.ShowDialog(this);
            if (createFolderForm.OkClicked)
            {
                IItem selItem = _lastSelectedTreeView.SelectedNode.Tag as IItem;
                string sID = selItem.AddFromTemplate(createFolderForm.tbResult.Text, "/sitecore/templates/common/folder");
                IItem newFolderItem = selItem.GetItem(sID);
                
                TreeViewEventArgs args = new TreeViewEventArgs(_lastSelectedTreeView.SelectedNode);
                TreeNode newNode = new TreeNode(newFolderItem.Name);
                newNode.Tag = newFolderItem;

                AddNode(newNode, args);                
            }
        }

        private void comboSiteTypeLeft_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tbLeftServerURL_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnView_Click(object sender, EventArgs e)
        {
            IItem selItem = null;
            selItem = _lastSelectedTreeView.SelectedNode.Tag as IItem;

            ItemEditForm itemEditForm = new ItemEditForm();

            string[] sLanguages = selItem.GetLanguages();
            foreach (string sLang in sLanguages)
            {
                itemEditForm.comboFromLanguage.Items.Add(sLang);
            }

            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            for (int t = 0; t < itemEditForm.comboFromLanguage.Items.Count; t++)
            {
                if (itemEditForm.comboFromLanguage.Items[t].ToString().ToLower() == currentCulture.TwoLetterISOLanguageName.ToLower())
                    itemEditForm.comboFromLanguage.SelectedIndex = t;
            }

            itemEditForm._startItem = selItem;
            itemEditForm.ShowDialog(this);

/*
            ViewTextForm viewTextForm = new ViewTextForm();
            viewTextForm.RichEdit.Text = selItem.GetOuterXml();
            viewTextForm.ShowDialog(this);

            if (viewTextForm.UpdateContent)
            {
                IField field = Util.GetFieldByName("__Default Bucket Query", selItem.Fields);
                if (field != null)
                    field.Content = "";
                selItem.Save();
            }
*/
        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                btnRename.PerformClick();
            }
            else if (e.KeyCode == Keys.F3)
            {
                btnView.PerformClick();
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnCopyToRight.PerformClick();
            }
            else if (e.KeyCode == Keys.F8)
            {
                btnDelete.PerformClick();
            }
            else if ((e.Modifiers == Keys.Alt) && (e.KeyCode == Keys.F7))
            {
                btnFind.PerformClick();
            }
            else if (e.KeyCode == Keys.Tab)
            {
                /*
                                if (leftTreeView.Focused)
                                    rightTreeView.Focus();
                                else
                                    leftTreeView.Focus();
                 */
                e.SuppressKeyPress = true;
            }
            
        }

#region Overridden Methods: [OnKeyDown()] and [ProcessTabKey()] -- Intercept/process [Keys.Tab]

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs args)
        {
            if ((args.Modifiers == System.Windows.Forms.Keys.Control)
            && (args.KeyCode == System.Windows.Forms.Keys.Tab))
            {
//                interceptTabKey = !interceptTabKey;
            }

            base.OnKeyDown(args);
        }

        private bool interceptTabKey = true;
        protected override bool ProcessTabKey(bool forward)
        {
            // We can intercept/process the [Keys.Tab] via this method.
            if (interceptTabKey)
            {
                if (forward)			// [Keys.Shift] was not used
                {
                    if (leftTreeView.Focused)
                    {
                        rightTreeView.Focus();
                    }
                    else
                    {
                        leftTreeView.Focus();
                    }
                }
                else					// [Keys.Shift] was used
                {
                }

                // [return true;]	-- To indicate that a control is selected.
                // [return false;]	-- Also, it happens that [return false;] causes the TabKey 
                //					   to be processed by the [OnKeyDown()] and related methods.
                return true;							// 'true' indicates that a control is selected
                //return false;
            }
            return base.ProcessTabKey(forward);			// One would normally do this, but we may
            // have wanted to intercept [Keys.Tab] above
        }
#endregion 


        private void ShowGlobalException()
        {
            ShowException(_globalException);
        }

        private void ShowException(Exception ex, ViewTextForm viewTextForm = null)
        {
            if (viewTextForm == null)
            {
                viewTextForm = new ViewTextForm();
                viewTextForm.RichEdit.Text = "An error occurred while copying content, exceptions are: \n";
            }

            viewTextForm.RichEdit.Text += "Exception.Message:" + ex.Message + "\n";
            viewTextForm.RichEdit.Text += "Exception.StackTrace:" + ex.StackTrace + "\n";
            viewTextForm.RichEdit.Text += "Exception.Source:" + ex.Source + "\n";
            if (ex.InnerException != null)
            {
                viewTextForm.RichEdit.Text += "\n";
                viewTextForm.RichEdit.Text += "Next InnerException:\n";
                ShowException(ex.InnerException, viewTextForm);
            }
            else
            {
                viewTextForm.ShowDialog();
                Util.WarningList.Clear();
            }
        }

        private void ShowGlobalWarninglist()
        {
            if (Util.WarningList.Count > 0)
            {
                ViewTextForm viewTextForm = new ViewTextForm();
                viewTextForm.RichEdit.Text = "A number of warnings occurred while copying content, they are: \n";
                foreach (string sWarning in Util.WarningList)
                    viewTextForm.RichEdit.Text += sWarning + "\n";
                viewTextForm.ShowDialog(this);
                Util.WarningList.Clear();
            }
        }

        private void ShowOverwriteDialogMethod(IItem srcItem, IItem existingItem)
        {
            _DialogResult = System.Windows.Forms.MessageBox.Show("The item " + srcItem.Name + " already exist at:\n " + existingItem.Path + "\nDo you want to overwrite it?", "Overwrite item", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
        }

        // *** Background worker - functions ***
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            BackgroundWorkerArgument arg = e.Argument as BackgroundWorkerArgument;

            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
            }
            else if (arg._sOperation == "copy")
            {
                try
                {
                    // Get item again in case we are copying from another language layer
                    _srcItem = _srcItem.GetItem(_srcItem.ID);

                    
                    // Ask user whether it is ok to overwrite destination
                    IItem targetItem = _destItem.GetItem(_srcItem.ID);
                    if ((targetItem != null) && 
                        ((_destItem.Options.CopyOperation == CopyOperations.Overwrite) || (_destItem.Options.CopyOperation == CopyOperations.UseNames)))
                    {
                        _delegateShowOverwriteDialog = new ShowOverwriteDialog(ShowOverwriteDialogMethod);
                        Invoke(_delegateShowOverwriteDialog, _srcItem, targetItem);
                        if (_DialogResult != System.Windows.Forms.DialogResult.Yes)
                            return;
                    }

                    _destItem.CopyTo(_srcItem, _destItem.Options.RecursiveOperation, _destItem.Options.CopyOnlyChildren);

                    // Show warnings
                    if (Util.WarningList.Count > 0)
                    {
                        Invoke(new MethodInvoker(ShowGlobalWarninglist));
                    }
                }
                catch (Exception ex)
                {
                    _globalException = ex;
                    Invoke(new MethodInvoker(ShowGlobalException));
                    throw ex;
                }
            }
            else if (arg._sOperation == "expandnode")
            {
                try
                {
                    TreeViewEventHandler treeHandler = arg._CallBackEventHandler as TreeViewEventHandler;
                    TreeViewEventArgs args = arg._CallBackEventArgs as TreeViewEventArgs;
                    treeHandler(arg._ParentControl, args);
                }
                catch (Exception ex)
                {
                    _globalException = ex;
                    Invoke(new MethodInvoker(ShowGlobalException));
                    throw ex;
                }
            }
            else
            {
                // Perform a time consuming operation and report progress.
                //                worker.ReportProgress((i * 10));
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                if (_progressForm != null)
                    _progressForm.Close();
            }
            else if (!(e.Error == null))
            {
                if (_progressForm != null)
                    _progressForm.Close();
            }

            else
            {
                if (_progressForm != null)
                    _progressForm.Close();
                //                this.tbProgress.Text = "Done!";
            }
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //            this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }


        private void btnFind_Click(object sender, EventArgs e)
        {
            IItem selItem = null;
            selItem = _lastSelectedTreeView.SelectedNode.Tag as IItem;
            _LastSelectedNode = _lastSelectedTreeView.SelectedNode;

            SearchReplaceForm searchReplaceForm = new SearchReplaceForm();
            searchReplaceForm._startItem = selItem;
            string[] sLanguages = selItem.GetLanguages();
            foreach (string sLang in sLanguages)
            {
                searchReplaceForm.comboFromLanguage.Items.Add(sLang);
            }
            searchReplaceForm.comboFromLanguage.SelectedIndex = 0;
            searchReplaceForm._myExpandNode = new SearchReplaceForm.ExpandNode(ExpandNode);

            searchReplaceForm.Show(this);
        }

        public void ExpandNode(IItem item)
        {
            ExpandNode(item, _LastSelectedNode, _lastSelectedTreeView);
        }

        public void ExpandNode(IItem item, TreeNode selectedNode, TreeView selectedTreeView)
        {
            // Does the node already exist?
            TreeNode theTreeNode = null;
            FindNode(selectedNode, item, ref theTreeNode);
            if (theTreeNode != null)
            {
//                selectedTreeView.SelectedNode = theTreeNode;
                return;
            }

            // No try and find the parent
            TreeNode parentTreeNode = null;
            FindNode(selectedNode, item.Parent, ref parentTreeNode);
            if (parentTreeNode == null)
                throw new Exception("Node not found, while trying to expand it, item.Path:" + item.Path);
            TreeViewEventArgs args = new TreeViewEventArgs(parentTreeNode);
            TreeNode newNode = new TreeNode(item.Name);
            newNode.Tag = item;
            AddNode(newNode, args);
        }


        private void btnCompare_Click(object sender, EventArgs e)
        {
            IItem selItem = null;
            selItem = _lastSelectedTreeView.SelectedNode.Tag as IItem;
            _LastSelectedNode = _lastSelectedTreeView.SelectedNode;

            CompareForm compareForm = new CompareForm();
            compareForm._leftStartItem = leftTreeView.SelectedNode.Tag as IItem;
            compareForm._rightStartItem = rightTreeView.SelectedNode.Tag as IItem;
            compareForm._leftSelectedTreeView = leftTreeView;
            compareForm._rightSelectedTreeView = rightTreeView;

            string[] sLanguages = selItem.GetLanguages();
            foreach (string sLang in sLanguages)
            {
                compareForm.comboFromLanguage.Items.Add(sLang);
            }

            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            for (int t = 0; t < compareForm.comboFromLanguage.Items.Count; t++)
            {
                if (compareForm.comboFromLanguage.Items[t].ToString().ToLower() == currentCulture.TwoLetterISOLanguageName.ToLower())
                    compareForm.comboFromLanguage.SelectedIndex = t;
            }
            compareForm._myExpandNode = new CompareForm.ExpandNode(ExpandNode);

            compareForm.Show(this);
            
        }

        private void btnLeftAddressList_Click(object sender, EventArgs e)
        {
            AddressForm addressForm = new AddressForm();
            if ((addressForm.ShowDialog() == DialogResult.OK) && (addressForm.foundLoginInfo != null))
            {
                tbLeftServerURL.Text = addressForm.foundLoginInfo.SiteUrl;
                tbLeftConnect_Click(this, new EventArgs());
            }
        }

        private void btnRightAddressList_Click(object sender, EventArgs e)
        {
            AddressForm addressForm = new AddressForm();
            if ((addressForm.ShowDialog() == DialogResult.OK) && (addressForm.foundLoginInfo != null))
            {
                tbRightServerURL.Text = addressForm.foundLoginInfo.SiteUrl;
                tbRightConnect_Click(this, new EventArgs());
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Util.WarningList.Clear();
            AddressForm addressForm = new AddressForm();
            addressForm.lbConnections.SelectionMode = SelectionMode.MultiExtended;
            if ((addressForm.ShowDialog() == DialogResult.OK) && (addressForm.foundLoginInfo != null))
            {                              
                _progressForm = new ProgressForm();
                _progressForm.CopyProgess = false;
                _progressForm.InstallSitesCount = addressForm.lbConnections.SelectedItems.Count;
                _progressForm.lblCopyingTo.Text = "";

                Logins loginList = LoginControl.LoadLogins();
                for (int d=0; d<addressForm.lbConnections.SelectedItems.Count; d++)
                {
                    LoginInfo selectedLoginInfo = null;
                    string sSelectedAddress =  addressForm.lbConnections.SelectedItems[d].ToString();
                    for (int t=0; t<loginList.LoginInfos.Count; t++)
                    {
                        if (sSelectedAddress == (loginList.LoginInfos[t] as LoginInfo).SiteUrl)
                        {
                            selectedLoginInfo = loginList.LoginInfos[t] as LoginInfo;
                            break;
                        }
                    }


                    // Connect to site
                    Sitecore6xItem item = ConnectSitecore61(null, selectedLoginInfo.SiteUrl, selectedLoginInfo.SiteType, "Master") as Sitecore6xItem;
                    // Timeout = 3000 seconds (default 10)
                    item.ExtendedWebService.Timeout = 3000000;

                    // Upload package
                    byte[] buffer = File.ReadAllBytes(openFileDialog1.FileName);
                    item.ExtendedWebService.UploadFile(Path.GetFileName(openFileDialog1.FileName), buffer, 0, item.ExtendedSitecoreAPICredentials);

                    // Install package
                    item.ExtendedWebService.InstallZipPackageCompleted += this.InstallZipPackageAsync_completed;
                    item.ExtendedWebService.InstallZipPackageAsync("/temp/" + Path.GetFileName(openFileDialog1.FileName), item.ExtendedSitecoreAPICredentials, selectedLoginInfo.SiteUrl);

                    // _progressForm.Text = "Installing package at: " + selectedLoginInfo.SiteUrl;
                    _progressForm.lblCopyingFrom.Text = "Installing package at: ";
                    if (_progressForm.lblCopyingTo.Text == "")
                        _progressForm.lblCopyingTo.Text = selectedLoginInfo.SiteUrl;
                    else
                        _progressForm.lblCopyingTo.Text += " and " + selectedLoginInfo.SiteUrl;
                    _progressForm.lblCopyingTo.Visible = true;
                    if (! _progressForm.Visible)
                        _progressForm.Show(this);
                    // _progressForm.ShowDialog(this);
                    // item.ExtendedWebService.InstallZipPackage("/temp/" + Path.GetFileName(openFileDialog1.FileName), item.ExtendedSitecoreAPICredentials);
                }
            }
        }

        void InstallZipPackageAsync_completed(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if ((_progressForm != null) && (_progressForm.Visible))
            {
                string sUrl = e.UserState as string;
                if (sUrl != null)
                {
                    Util.WarningList.Add("Finished installing package on " + sUrl);
                }

                _progressForm.InstallSitesCount--;
                if (_progressForm.InstallSitesCount <= 0)
                {
                    if (Util.WarningList.Count > 0)
                    {
                        ViewTextForm viewTextForm = new ViewTextForm();
                        viewTextForm.RichEdit.Text = "Tasks: \n";
                        foreach (string sWarning in Util.WarningList)
                            viewTextForm.RichEdit.Text += sWarning + "\n";
                        viewTextForm.ShowDialog(this);
                    }
                    _progressForm.Close();
                    // _progressForm = null;
                }
            }
        }

        private void btnTransferFieldsTool_Click(object sender, EventArgs e)
        {
            if ((rightTreeView.SelectedNode == null) || (rightTreeView.SelectedNode == null))
            {
                MessageBox.Show("Please connect to sites first.");
                return;
            }


            IItem destItem = rightTreeView.SelectedNode.Tag as IItem;
            IItem srcItem = leftTreeView.SelectedNode.Tag as IItem;

            ScriptToolForm scriptToolForm = new ScriptToolForm();
            scriptToolForm._leftStartItem = srcItem;
            scriptToolForm._rightStartItem = destItem;
            scriptToolForm.ShowDialog();

        }

        private void btnLocateWithRight_Click(object sender, EventArgs e)
        {

        }

        private TreeNode FindRootNode(TreeNode treeNode)
        {
            while (treeNode.Parent != null)
            {
                treeNode = treeNode.Parent;
            }
            return treeNode;
        }

        /// <summary>
        /// Find tree node item and expand to it
        /// </summary>
        /// <param name="node"></param>
        /// <param name="sName"></param>
        /// <param name="sPath"></param>
        /// <returns></returns>
        private void FindTreeNode(TreeNode node, IItem item, string sName, string sPath, ref TreeNode foundNode)
        {
            sName = sName.Replace("/", "");

            if (item.Name == sName)
            {
                item.GetChildren();
                int iFoundPos = sPath.IndexOf("/", 1);
                if (iFoundPos > -1)
                {
                    sName = sPath.Substring(iFoundPos + 1);
                    if (sName.IndexOf("/") > -1)
                        sName = sName.Remove(sName.IndexOf("/"));

                    sPath = sPath.Remove(0, sPath.IndexOf("/", 1));
                }
            }
//            node = node.TreeView.SelectedNode;

            foreach (IItem child in item.GetChildren())
            {
                ExpandNode(child, node, node.TreeView);

                if (child.Name == sName)
                {                    
                    if (sPath.IndexOf("/", 1) > -1)
                    {
                        sName = sPath.Substring(sPath.IndexOf("/", 1) + 1);
                        if (sName.IndexOf("/") > -1)
                            sName = sName.Remove(sName.IndexOf("/"));

                        sPath = sPath.Remove(0, sPath.IndexOf("/", 1));

                        Application.DoEvents();
                        FindTreeNode(node, child, sName, sPath, ref foundNode);
                    }
                    else
                    {
                        // Found node select and return it
                        // child.TreeView.SelectedNode = child;
                        // return child;
                        /// ExpandNode(child, node, node.TreeView);
                        // TreeNode foundNode = null;
                        FindNode(node, child, ref foundNode);
                        //if (foundNode != null)
                        //    node = foundNode;
                    }
                }
            }
        }

        private void ExpandToNode(TreeView treeView, string sPath)
        {
            if (sPath == "")
                return;

            // ExpandNode(item, _LastSelectedNode, _lastSelectedTreeView);
            string sElementName = "";
            if (sPath.IndexOf("/") > -1)
                sElementName = sPath.Substring(0, sPath.IndexOf("/", 1));

            if (treeView.SelectedNode == null)
            {
                MessageBox.Show("Connect to site first.");
                return;
            }

            TreeNode root = FindRootNode(treeView.SelectedNode);
            TreeNode foundNode = null;
            FindTreeNode(root, root.Tag as IItem, sElementName, sPath, ref foundNode);

            treeView.SelectedNode = foundNode;

        }

        private void btnLocateWithLeft_Click(object sender, EventArgs e)
        {
            if (_lastSelectedTreeView == null)
            {
                MessageBox.Show("Connect to site first.");
                return;
            }

            _progressForm = new ProgressForm();
            _progressForm.Show(this);
            _progressForm.lblCopyingTo.Text = "";
            _progressForm.lblCopyingFrom.Text = "";
            _progressForm.Text = "Locating content...";

            IItem selItem = _lastSelectedTreeView.SelectedNode.Tag as IItem;
            _LastSelectedNode = _lastSelectedTreeView.SelectedNode;
            if (_lastSelectedTreeView == rightTreeView)
                ExpandToNode(leftTreeView, selItem.Path);
            else
                ExpandToNode(rightTreeView, selItem.Path);


            _progressForm.Close();
        }
    }



    public class BackgroundWorkerArgument
    {
        public string _sOperation = "";
        public object _CallBackEventHandler = null;
        public object _CallBackEventArgs = null;
        public System.Windows.Forms.Control _ParentControl = null;

        public BackgroundWorkerArgument(string sOperation, object CallBackEventHandler, object CallBackEventArgs, System.Windows.Forms.Control ParentControl)
        {
            _sOperation = sOperation;
            _CallBackEventHandler = CallBackEventHandler;
            _CallBackEventArgs = CallBackEventArgs;
            _ParentControl = ParentControl;
        }
    }

}
