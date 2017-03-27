namespace SitecoreConverter
{
    partial class LoginForm
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
            this.loginControl1 = new CustomControls.LoginControl();
            this.SuspendLayout();
            // 
            // loginControl1
            // 
            this.loginControl1.Check = true;
            this.loginControl1.ErrorMessage = "Please enter the correct Username / Password combination!";
            this.loginControl1.Hash = true;
            this.loginControl1.labelButton = "Login";
            this.loginControl1.labelHeader = "Please enter your Username and Password.";
            this.loginControl1.labelHeadline = "Login";
            this.loginControl1.labelPassword = "Password";
            this.loginControl1.labelUsername = "Username";
            this.loginControl1.Location = new System.Drawing.Point(1, -1);
            this.loginControl1.Name = "loginControl1";
            this.loginControl1.Password = null;
            this.loginControl1.Size = new System.Drawing.Size(460, 300);
            this.loginControl1.TabIndex = 0;
            this.loginControl1.Username = null;
            this.loginControl1.Load += new System.EventHandler(this.loginControl1_Load_1);
            this.loginControl1.Ready += new System.EventHandler(this.loginControl1_Ready);
            this.loginControl1.Successful += new System.EventHandler(this.loginControl1_Successful);
            this.loginControl1.Failed += new System.EventHandler(this.loginControl1_Failed);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 285);
            this.Controls.Add(this.loginControl1);
            this.Name = "LoginForm";
            this.Text = "LoginForm";
            this.ResumeLayout(false);

        }

        #endregion

        public CustomControls.LoginControl loginControl1;
    }
}