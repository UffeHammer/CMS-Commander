namespace SitecoreConverter
{
    partial class CompareForm
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
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabSearchResult = new System.Windows.Forms.TabPage();
            this.lbCompareResult = new System.Windows.Forms.ListBox();
            this.btnCompare = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbSearchingIn = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbIgnoreStandardFields = new System.Windows.Forms.CheckBox();
            this.cbMissingFields = new System.Windows.Forms.CheckBox();
            this.lblRightFromPath = new System.Windows.Forms.Label();
            this.lblLeftFromPath = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboFromLanguage = new System.Windows.Forms.ComboBox();
            this.btnSaveToLeftSide = new System.Windows.Forms.Button();
            this.btnSaveToRightSide = new System.Windows.Forms.Button();
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.tabControl2.SuspendLayout();
            this.tabSearchResult.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button2.Location = new System.Drawing.Point(581, 58);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(72, 20);
            this.button2.TabIndex = 14;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabControl2
            // 
            this.tabControl2.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabSearchResult);
            this.tabControl2.Location = new System.Drawing.Point(10, 215);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl2.Multiline = true;
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(562, 278);
            this.tabControl2.TabIndex = 13;
            // 
            // tabSearchResult
            // 
            this.tabSearchResult.Controls.Add(this.lbCompareResult);
            this.tabSearchResult.Location = new System.Drawing.Point(4, 4);
            this.tabSearchResult.Margin = new System.Windows.Forms.Padding(2);
            this.tabSearchResult.Name = "tabSearchResult";
            this.tabSearchResult.Padding = new System.Windows.Forms.Padding(2);
            this.tabSearchResult.Size = new System.Drawing.Size(554, 252);
            this.tabSearchResult.TabIndex = 0;
            this.tabSearchResult.Text = "Compare result";
            this.tabSearchResult.UseVisualStyleBackColor = true;
            // 
            // lbCompareResult
            // 
            this.lbCompareResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbCompareResult.FormattingEnabled = true;
            this.lbCompareResult.Location = new System.Drawing.Point(13, 13);
            this.lbCompareResult.Margin = new System.Windows.Forms.Padding(2);
            this.lbCompareResult.Name = "lbCompareResult";
            this.lbCompareResult.Size = new System.Drawing.Size(533, 225);
            this.lbCompareResult.TabIndex = 0;
            // 
            // btnCompare
            // 
            this.btnCompare.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCompare.Location = new System.Drawing.Point(581, 33);
            this.btnCompare.Margin = new System.Windows.Forms.Padding(2);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(72, 20);
            this.btnCompare.TabIndex = 12;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageMain);
            this.tabControl1.Location = new System.Drawing.Point(10, 11);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(566, 199);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.groupBox3);
            this.tabPageMain.Controls.Add(this.groupBox1);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(558, 173);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Main options";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.tbSearchingIn);
            this.groupBox3.Location = new System.Drawing.Point(13, 90);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(533, 73);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Currently comparing in";
            // 
            // tbSearchingIn
            // 
            this.tbSearchingIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchingIn.Location = new System.Drawing.Point(7, 20);
            this.tbSearchingIn.Margin = new System.Windows.Forms.Padding(2);
            this.tbSearchingIn.Multiline = true;
            this.tbSearchingIn.Name = "tbSearchingIn";
            this.tbSearchingIn.Size = new System.Drawing.Size(522, 40);
            this.tbSearchingIn.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbIgnoreStandardFields);
            this.groupBox1.Controls.Add(this.cbMissingFields);
            this.groupBox1.Controls.Add(this.lblRightFromPath);
            this.groupBox1.Controls.Add(this.lblLeftFromPath);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboFromLanguage);
            this.groupBox1.Location = new System.Drawing.Point(13, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(533, 79);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compare:";
            this.groupBox1.UseCompatibleTextRendering = true;
            // 
            // cbIgnoreStandardFields
            // 
            this.cbIgnoreStandardFields.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbIgnoreStandardFields.AutoSize = true;
            this.cbIgnoreStandardFields.Location = new System.Drawing.Point(395, 31);
            this.cbIgnoreStandardFields.Name = "cbIgnoreStandardFields";
            this.cbIgnoreStandardFields.Size = new System.Drawing.Size(127, 17);
            this.cbIgnoreStandardFields.TabIndex = 10;
            this.cbIgnoreStandardFields.Text = "Ignore standard fields";
            this.cbIgnoreStandardFields.UseVisualStyleBackColor = true;
            // 
            // cbMissingFields
            // 
            this.cbMissingFields.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbMissingFields.AutoSize = true;
            this.cbMissingFields.Location = new System.Drawing.Point(395, 53);
            this.cbMissingFields.Margin = new System.Windows.Forms.Padding(2);
            this.cbMissingFields.Name = "cbMissingFields";
            this.cbMissingFields.Size = new System.Drawing.Size(133, 17);
            this.cbMissingFields.TabIndex = 9;
            this.cbMissingFields.Text = "Also find missing fields.";
            this.cbMissingFields.UseVisualStyleBackColor = true;
            // 
            // lblRightFromPath
            // 
            this.lblRightFromPath.AutoSize = true;
            this.lblRightFromPath.Location = new System.Drawing.Point(79, 33);
            this.lblRightFromPath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRightFromPath.Name = "lblRightFromPath";
            this.lblRightFromPath.Size = new System.Drawing.Size(35, 13);
            this.lblRightFromPath.TabIndex = 8;
            this.lblRightFromPath.Text = "label2";
            // 
            // lblLeftFromPath
            // 
            this.lblLeftFromPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLeftFromPath.AutoSize = true;
            this.lblLeftFromPath.Location = new System.Drawing.Point(76, 15);
            this.lblLeftFromPath.Name = "lblLeftFromPath";
            this.lblLeftFromPath.Size = new System.Drawing.Size(39, 13);
            this.lblLeftFromPath.TabIndex = 7;
            this.lblLeftFromPath.Text = "lblPath";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Compare in:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Language:";
            // 
            // comboFromLanguage
            // 
            this.comboFromLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboFromLanguage.FormattingEnabled = true;
            this.comboFromLanguage.Location = new System.Drawing.Point(78, 53);
            this.comboFromLanguage.Name = "comboFromLanguage";
            this.comboFromLanguage.Size = new System.Drawing.Size(314, 21);
            this.comboFromLanguage.TabIndex = 4;
            // 
            // btnSaveToLeftSide
            // 
            this.btnSaveToLeftSide.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSaveToLeftSide.Location = new System.Drawing.Point(581, 221);
            this.btnSaveToLeftSide.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveToLeftSide.Name = "btnSaveToLeftSide";
            this.btnSaveToLeftSide.Size = new System.Drawing.Size(73, 34);
            this.btnSaveToLeftSide.TabIndex = 15;
            this.btnSaveToLeftSide.Text = "Save to left side";
            this.btnSaveToLeftSide.UseVisualStyleBackColor = true;
            this.btnSaveToLeftSide.Click += new System.EventHandler(this.btnSaveToLeftSide_Click);
            // 
            // btnSaveToRightSide
            // 
            this.btnSaveToRightSide.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSaveToRightSide.Location = new System.Drawing.Point(581, 260);
            this.btnSaveToRightSide.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveToRightSide.Name = "btnSaveToRightSide";
            this.btnSaveToRightSide.Size = new System.Drawing.Size(73, 37);
            this.btnSaveToRightSide.TabIndex = 16;
            this.btnSaveToRightSide.Text = "Save to right side";
            this.btnSaveToRightSide.UseVisualStyleBackColor = true;
            this.btnSaveToRightSide.Click += new System.EventHandler(this.btnSaveToRightSide_Click);
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnGenerateReport.Location = new System.Drawing.Point(583, 301);
            this.btnGenerateReport.Margin = new System.Windows.Forms.Padding(2);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new System.Drawing.Size(72, 34);
            this.btnGenerateReport.TabIndex = 17;
            this.btnGenerateReport.Text = "Generate report";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // CompareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 503);
            this.Controls.Add(this.btnGenerateReport);
            this.Controls.Add(this.btnSaveToRightSide);
            this.Controls.Add(this.btnSaveToLeftSide);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.tabControl1);
            this.Name = "CompareForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "CompareForm";
            this.Shown += new System.EventHandler(this.CompareForm_Shown);
            this.tabControl2.ResumeLayout(false);
            this.tabSearchResult.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabSearchResult;
        private System.Windows.Forms.ListBox lbCompareResult;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox tbSearchingIn;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label lblLeftFromPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox comboFromLanguage;
        private System.Windows.Forms.Label lblRightFromPath;
        private System.Windows.Forms.Button btnSaveToLeftSide;
        private System.Windows.Forms.Button btnSaveToRightSide;
        private System.Windows.Forms.CheckBox cbMissingFields;
        private System.Windows.Forms.Button btnGenerateReport;
        private System.Windows.Forms.CheckBox cbIgnoreStandardFields;
    }
}