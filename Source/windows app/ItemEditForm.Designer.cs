namespace SitecoreConverter
{
    partial class ItemEditForm
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
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.gbFields = new System.Windows.Forms.GroupBox();
            this.fieldsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboFromLanguage = new System.Windows.Forms.ComboBox();
            this.gbFields.SuspendLayout();
            this.fieldsSplitContainer.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.Location = new System.Drawing.Point(872, 502);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 4;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(953, 502);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // gbFields
            // 
            this.gbFields.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFields.Controls.Add(this.fieldsSplitContainer);
            this.gbFields.Location = new System.Drawing.Point(12, 69);
            this.gbFields.Name = "gbFields";
            this.gbFields.Size = new System.Drawing.Size(1016, 427);
            this.gbFields.TabIndex = 5;
            this.gbFields.TabStop = false;
            this.gbFields.Text = "Fields";
            // 
            // fieldsSplitContainer
            // 
            this.fieldsSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fieldsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.fieldsSplitContainer.Location = new System.Drawing.Point(3, 16);
            this.fieldsSplitContainer.Name = "fieldsSplitContainer";
            // 
            // fieldsSplitContainer.Panel1
            // 
            this.fieldsSplitContainer.Panel1.AutoScroll = true;
            this.fieldsSplitContainer.Panel1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.fieldsSplitContainer_Panel1_Scroll);
            // 
            // fieldsSplitContainer.Panel2
            // 
            this.fieldsSplitContainer.Panel2.AutoScroll = true;
            this.fieldsSplitContainer.Panel2.Scroll += new System.Windows.Forms.ScrollEventHandler(this.fieldsSplitContainer_Panel2_Scroll);
            this.fieldsSplitContainer.Size = new System.Drawing.Size(1010, 408);
            this.fieldsSplitContainer.SplitterDistance = 245;
            this.fieldsSplitContainer.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboFromLanguage);
            this.groupBox1.Location = new System.Drawing.Point(15, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(736, 51);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Properties";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Language:";
            // 
            // comboFromLanguage
            // 
            this.comboFromLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboFromLanguage.FormattingEnabled = true;
            this.comboFromLanguage.Location = new System.Drawing.Point(92, 19);
            this.comboFromLanguage.Name = "comboFromLanguage";
            this.comboFromLanguage.Size = new System.Drawing.Size(314, 21);
            this.comboFromLanguage.TabIndex = 6;
            this.comboFromLanguage.SelectedIndexChanged += new System.EventHandler(this.comboFromLanguage_SelectedIndexChanged);
            // 
            // ItemEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 537);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbFields);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnOk);
            this.Name = "ItemEditForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "ItemEditForm";
            this.Load += new System.EventHandler(this.ItemEditForm_Load);
            this.Shown += new System.EventHandler(this.ItemEditForm_Shown);
            this.gbFields.ResumeLayout(false);
            this.fieldsSplitContainer.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.GroupBox gbFields;
        private System.Windows.Forms.SplitContainer fieldsSplitContainer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox comboFromLanguage;
    }
}