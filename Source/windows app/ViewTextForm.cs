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
    public partial class ViewTextForm : Form
    {
        private bool m_bUpdateContent = false;

        public bool UpdateContent
        {
            get
            {
                return m_bUpdateContent;
            }
        } 

        public ViewTextForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            m_bUpdateContent = true;
            this.Close();
        }
    }
}
