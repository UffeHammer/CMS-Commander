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
    public partial class SinglelineInputForm : Form
    {
        public SinglelineInputForm()
        {
            InitializeComponent();
        }

        private bool _bOkClicked = false;
        public bool OkClicked
        {
            get
            {
                return _bOkClicked;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (tbResult.Text == "")
            {
                MessageBox.Show("Please fill in edit field!");
            }
            else
            {
                _bOkClicked = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _bOkClicked = false;
            Close();
        }
    }
}
