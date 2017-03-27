namespace SitecoreConverter
{
    partial class ScriptToolForm
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
            this.tbEditorControl = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnTransferFields = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSearchFieldValue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLanguage = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbResult = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbEditorControl
            // 
            this.tbEditorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEditorControl.Location = new System.Drawing.Point(12, 82);
            this.tbEditorControl.MaxLength = 327670;
            this.tbEditorControl.Multiline = true;
            this.tbEditorControl.Name = "tbEditorControl";
            this.tbEditorControl.Size = new System.Drawing.Size(682, 326);
            this.tbEditorControl.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(700, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(85, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnTransferFields
            // 
            this.btnTransferFields.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTransferFields.Location = new System.Drawing.Point(700, 42);
            this.btnTransferFields.Name = "btnTransferFields";
            this.btnTransferFields.Size = new System.Drawing.Size(86, 23);
            this.btnTransferFields.TabIndex = 2;
            this.btnTransferFields.Text = "Transfer fields";
            this.btnTransferFields.UseVisualStyleBackColor = true;
            this.btnTransferFields.Click += new System.EventHandler(this.btnTransferFields_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Only transfer fields with this content:";
            // 
            // tbSearchFieldValue
            // 
            this.tbSearchFieldValue.Location = new System.Drawing.Point(12, 39);
            this.tbSearchFieldValue.Name = "tbSearchFieldValue";
            this.tbSearchFieldValue.Size = new System.Drawing.Size(293, 20);
            this.tbSearchFieldValue.TabIndex = 4;
            this.tbSearchFieldValue.Text = "<a name=";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "From these paths:";
            // 
            // tbLanguage
            // 
            this.tbLanguage.Location = new System.Drawing.Point(311, 39);
            this.tbLanguage.Name = "tbLanguage";
            this.tbLanguage.Size = new System.Drawing.Size(293, 20);
            this.tbLanguage.TabIndex = 7;
            this.tbLanguage.Text = "da";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(311, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(184, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Only transfer fields with this language:";
            // 
            // tbResult
            // 
            this.tbResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbResult.Location = new System.Drawing.Point(12, 423);
            this.tbResult.MaxLength = 327670;
            this.tbResult.Multiline = true;
            this.tbResult.Name = "tbResult";
            this.tbResult.Size = new System.Drawing.Size(682, 117);
            this.tbResult.TabIndex = 8;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(700, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Copy fields";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ScriptToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(797, 552);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbResult);
            this.Controls.Add(this.tbLanguage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbSearchFieldValue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnTransferFields);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tbEditorControl);
            this.Name = "ScriptToolForm";
            this.Text = "ScriptToolForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbEditorControl;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnTransferFields;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSearchFieldValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLanguage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.Button button1;
    }
}