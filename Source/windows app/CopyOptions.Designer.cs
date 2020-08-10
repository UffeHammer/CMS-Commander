namespace SitecoreConverter
{
    partial class CopyOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblFromPath = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboFromLanguage = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblToPath = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboToLanguage = new System.Windows.Forms.ComboBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbIgnoreErrors = new System.Windows.Forms.CheckBox();
            this.cbRecursive = new System.Windows.Forms.CheckBox();
            this.rbUseNames = new System.Windows.Forms.RadioButton();
            this.rbCreateNewItemIDs = new System.Windows.Forms.RadioButton();
            this.rbSkipExisting = new System.Windows.Forms.RadioButton();
            this.rbOverwrite = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbSetItemRightsExplicitly = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbRootRole = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbSecurityDomain = new System.Windows.Forms.TextBox();
            this.cbCopySecurity = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.cbOnlyChildren = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbOnlyChildren);
            this.groupBox1.Controls.Add(this.lblFromPath);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboFromLanguage);
            this.groupBox1.Location = new System.Drawing.Point(13, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(528, 69);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "From:";
            // 
            // lblFromPath
            // 
            this.lblFromPath.AutoSize = true;
            this.lblFromPath.Location = new System.Drawing.Point(64, 40);
            this.lblFromPath.Name = "lblFromPath";
            this.lblFromPath.Size = new System.Drawing.Size(35, 13);
            this.lblFromPath.TabIndex = 3;
            this.lblFromPath.Text = "label6";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Path:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Language";
            // 
            // comboFromLanguage
            // 
            this.comboFromLanguage.FormattingEnabled = true;
            this.comboFromLanguage.Location = new System.Drawing.Point(67, 13);
            this.comboFromLanguage.Name = "comboFromLanguage";
            this.comboFromLanguage.Size = new System.Drawing.Size(121, 21);
            this.comboFromLanguage.TabIndex = 0;
            this.comboFromLanguage.SelectedIndexChanged += new System.EventHandler(this.comboFromLanguage_SelectedIndexChanged);
            this.comboFromLanguage.SelectedValueChanged += new System.EventHandler(this.comboFromLanguage_SelectedValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.lblToPath);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.comboToLanguage);
            this.groupBox2.Location = new System.Drawing.Point(13, 81);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(528, 69);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "To:";
            // 
            // lblToPath
            // 
            this.lblToPath.AutoSize = true;
            this.lblToPath.Location = new System.Drawing.Point(64, 40);
            this.lblToPath.Name = "lblToPath";
            this.lblToPath.Size = new System.Drawing.Size(35, 13);
            this.lblToPath.TabIndex = 6;
            this.lblToPath.Text = "label6";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Language";
            // 
            // comboToLanguage
            // 
            this.comboToLanguage.FormattingEnabled = true;
            this.comboToLanguage.Location = new System.Drawing.Point(67, 13);
            this.comboToLanguage.Name = "comboToLanguage";
            this.comboToLanguage.Size = new System.Drawing.Size(121, 21);
            this.comboToLanguage.TabIndex = 2;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(421, 450);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.cbIgnoreErrors);
            this.groupBox3.Controls.Add(this.cbRecursive);
            this.groupBox3.Controls.Add(this.rbUseNames);
            this.groupBox3.Controls.Add(this.rbCreateNewItemIDs);
            this.groupBox3.Controls.Add(this.rbSkipExisting);
            this.groupBox3.Controls.Add(this.rbOverwrite);
            this.groupBox3.Location = new System.Drawing.Point(13, 157);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(528, 122);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Copy operation";
            // 
            // cbIgnoreErrors
            // 
            this.cbIgnoreErrors.AutoSize = true;
            this.cbIgnoreErrors.Location = new System.Drawing.Point(438, 42);
            this.cbIgnoreErrors.Margin = new System.Windows.Forms.Padding(2);
            this.cbIgnoreErrors.Name = "cbIgnoreErrors";
            this.cbIgnoreErrors.Size = new System.Drawing.Size(85, 17);
            this.cbIgnoreErrors.TabIndex = 5;
            this.cbIgnoreErrors.Text = "Ignore errors";
            this.cbIgnoreErrors.UseVisualStyleBackColor = true;
            // 
            // cbRecursive
            // 
            this.cbRecursive.AutoSize = true;
            this.cbRecursive.Checked = true;
            this.cbRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRecursive.Location = new System.Drawing.Point(438, 18);
            this.cbRecursive.Margin = new System.Windows.Forms.Padding(2);
            this.cbRecursive.Name = "cbRecursive";
            this.cbRecursive.Size = new System.Drawing.Size(74, 17);
            this.cbRecursive.TabIndex = 4;
            this.cbRecursive.Text = "Recursive";
            this.cbRecursive.UseVisualStyleBackColor = true;
            // 
            // rbUseNames
            // 
            this.rbUseNames.AutoSize = true;
            this.rbUseNames.Location = new System.Drawing.Point(9, 89);
            this.rbUseNames.Name = "rbUseNames";
            this.rbUseNames.Size = new System.Drawing.Size(409, 17);
            this.rbUseNames.TabIndex = 3;
            this.rbUseNames.Text = "Use names to identify items, instead of ID\'s (existing valid items will be overwr" +
    "itten)";
            this.rbUseNames.UseVisualStyleBackColor = true;
            // 
            // rbCreateNewItemIDs
            // 
            this.rbCreateNewItemIDs.AutoSize = true;
            this.rbCreateNewItemIDs.Location = new System.Drawing.Point(9, 65);
            this.rbCreateNewItemIDs.Name = "rbCreateNewItemIDs";
            this.rbCreateNewItemIDs.Size = new System.Drawing.Size(246, 17);
            this.rbCreateNewItemIDs.TabIndex = 2;
            this.rbCreateNewItemIDs.Text = "Create new items (New itemID\'s are generated)";
            this.rbCreateNewItemIDs.UseVisualStyleBackColor = true;
            // 
            // rbSkipExisting
            // 
            this.rbSkipExisting.AutoSize = true;
            this.rbSkipExisting.Location = new System.Drawing.Point(9, 42);
            this.rbSkipExisting.Name = "rbSkipExisting";
            this.rbSkipExisting.Size = new System.Drawing.Size(111, 17);
            this.rbSkipExisting.TabIndex = 1;
            this.rbSkipExisting.Text = "Skip existing items";
            this.rbSkipExisting.UseVisualStyleBackColor = true;
            // 
            // rbOverwrite
            // 
            this.rbOverwrite.AutoSize = true;
            this.rbOverwrite.Checked = true;
            this.rbOverwrite.Location = new System.Drawing.Point(9, 19);
            this.rbOverwrite.Name = "rbOverwrite";
            this.rbOverwrite.Size = new System.Drawing.Size(135, 17);
            this.rbOverwrite.TabIndex = 0;
            this.rbOverwrite.TabStop = true;
            this.rbOverwrite.Text = "Overwrite existing items";
            this.rbOverwrite.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(502, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.cbSetItemRightsExplicitly);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.tbRootRole);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.tbSecurityDomain);
            this.groupBox4.Controls.Add(this.cbCopySecurity);
            this.groupBox4.Location = new System.Drawing.Point(13, 285);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(528, 115);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Security";
            this.groupBox4.Enter += new System.EventHandler(this.groupBox4_Enter);
            // 
            // cbSetItemRightsExplicitly
            // 
            this.cbSetItemRightsExplicitly.AutoSize = true;
            this.cbSetItemRightsExplicitly.Enabled = false;
            this.cbSetItemRightsExplicitly.Location = new System.Drawing.Point(32, 77);
            this.cbSetItemRightsExplicitly.Name = "cbSetItemRightsExplicitly";
            this.cbSetItemRightsExplicitly.Size = new System.Drawing.Size(259, 17);
            this.cbSetItemRightsExplicitly.TabIndex = 5;
            this.cbSetItemRightsExplicitly.Text = "Set item rights explicitly (even if they are inherited)";
            this.cbSetItemRightsExplicitly.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(283, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Root Role:";
            // 
            // tbRootRole
            // 
            this.tbRootRole.Enabled = false;
            this.tbRootRole.Location = new System.Drawing.Point(347, 47);
            this.tbRootRole.Name = "tbRootRole";
            this.tbRootRole.Size = new System.Drawing.Size(87, 20);
            this.tbRootRole.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Default security domain:";
            // 
            // tbSecurityDomain
            // 
            this.tbSecurityDomain.Enabled = false;
            this.tbSecurityDomain.Location = new System.Drawing.Point(155, 47);
            this.tbSecurityDomain.Name = "tbSecurityDomain";
            this.tbSecurityDomain.Size = new System.Drawing.Size(100, 20);
            this.tbSecurityDomain.TabIndex = 1;
            // 
            // cbCopySecurity
            // 
            this.cbCopySecurity.AutoSize = true;
            this.cbCopySecurity.Location = new System.Drawing.Point(9, 26);
            this.cbCopySecurity.Name = "cbCopySecurity";
            this.cbCopySecurity.Size = new System.Drawing.Size(399, 17);
            this.cbCopySecurity.TabIndex = 0;
            this.cbCopySecurity.Text = "Copy item rights and create roles (requires custom webservice installed on v.6+)";
            this.cbCopySecurity.UseVisualStyleBackColor = true;
            this.cbCopySecurity.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageMain);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(566, 432);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.groupBox1);
            this.tabPageMain.Controls.Add(this.groupBox4);
            this.tabPageMain.Controls.Add(this.groupBox2);
            this.tabPageMain.Controls.Add(this.groupBox3);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(558, 406);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Main options";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // cbOnlyChildren
            // 
            this.cbOnlyChildren.AutoSize = true;
            this.cbOnlyChildren.Location = new System.Drawing.Point(438, 12);
            this.cbOnlyChildren.Margin = new System.Windows.Forms.Padding(2);
            this.cbOnlyChildren.Name = "cbOnlyChildren";
            this.cbOnlyChildren.Size = new System.Drawing.Size(87, 17);
            this.cbOnlyChildren.TabIndex = 5;
            this.cbOnlyChildren.Text = "Only children";
            this.cbOnlyChildren.UseVisualStyleBackColor = true;
            // 
            // CopyOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 486);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "CopyOptions";
            this.Text = "CopyOptions";
            this.Load += new System.EventHandler(this.CopyOptions_Load);
            this.Shown += new System.EventHandler(this.CopyOptions_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnOk;
        public System.Windows.Forms.ComboBox comboFromLanguage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox comboToLanguage;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.RadioButton rbSkipExisting;
        public System.Windows.Forms.RadioButton rbOverwrite;
        public System.Windows.Forms.RadioButton rbCreateNewItemIDs;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox tbSecurityDomain;
        public System.Windows.Forms.CheckBox cbCopySecurity;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label lblFromPath;
        public System.Windows.Forms.Label lblToPath;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox tbRootRole;
        public System.Windows.Forms.RadioButton rbUseNames;
        public System.Windows.Forms.CheckBox cbRecursive;
        public System.Windows.Forms.CheckBox cbSetItemRightsExplicitly;
        public System.Windows.Forms.CheckBox cbIgnoreErrors;
        public System.Windows.Forms.CheckBox cbOnlyChildren;
    }
}