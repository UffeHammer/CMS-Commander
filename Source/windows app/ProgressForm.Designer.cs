namespace SitecoreConverter
{
    partial class ProgressForm
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
            this.components = new System.ComponentModel.Container();
            this.lblCopyingFrom = new System.Windows.Forms.Label();
            this.progressCurrentTask = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblCopyingTo = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbWarnings = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCopyingFrom
            // 
            this.lblCopyingFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCopyingFrom.Location = new System.Drawing.Point(31, 30);
            this.lblCopyingFrom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCopyingFrom.Name = "lblCopyingFrom";
            this.lblCopyingFrom.Size = new System.Drawing.Size(781, 16);
            this.lblCopyingFrom.TabIndex = 0;
            this.lblCopyingFrom.Text = "label1";
            this.lblCopyingFrom.Click += new System.EventHandler(this.label1_Click);
            // 
            // progressCurrentTask
            // 
            this.progressCurrentTask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressCurrentTask.Location = new System.Drawing.Point(35, 100);
            this.progressCurrentTask.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressCurrentTask.Maximum = 1000;
            this.progressCurrentTask.Name = "progressCurrentTask";
            this.progressCurrentTask.Size = new System.Drawing.Size(787, 28);
            this.progressCurrentTask.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblCopyingTo
            // 
            this.lblCopyingTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCopyingTo.Location = new System.Drawing.Point(31, 60);
            this.lblCopyingTo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCopyingTo.Name = "lblCopyingTo";
            this.lblCopyingTo.Size = new System.Drawing.Size(781, 16);
            this.lblCopyingTo.TabIndex = 2;
            this.lblCopyingTo.Text = "label1";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(376, 135);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbWarnings);
            this.groupBox1.Location = new System.Drawing.Point(35, 197);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(787, 160);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Warnings";
            // 
            // tbWarnings
            // 
            this.tbWarnings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbWarnings.Location = new System.Drawing.Point(8, 23);
            this.tbWarnings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbWarnings.Multiline = true;
            this.tbWarnings.Name = "tbWarnings";
            this.tbWarnings.Size = new System.Drawing.Size(769, 128);
            this.tbWarnings.TabIndex = 5;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(837, 182);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblCopyingTo);
            this.Controls.Add(this.progressCurrentTask);
            this.Controls.Add(this.lblCopyingFrom);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ProgressForm";
            this.Text = "ProgressForm";
            this.Load += new System.EventHandler(this.ProgressForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblCopyingFrom;
        private System.Windows.Forms.ProgressBar progressCurrentTask;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.Label lblCopyingTo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbWarnings;
    }
}