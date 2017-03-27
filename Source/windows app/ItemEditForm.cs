using SitecoreConverter.Core;
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
    public partial class ItemEditForm : Form
    {
        public IItem _startItem = null;

        public ItemEditForm()
        {
            InitializeComponent();
        }

        private void ItemEditForm_Load(object sender, EventArgs e)
        {
            if (_startItem == null)
                return;

            _startItem.Options.Language = comboFromLanguage.Text;
            _startItem = _startItem.GetItem(_startItem.ID);


            gbFields.Text = "Fields in: " + _startItem.Name;



            int iTop = 20;
            foreach (IItem template in _startItem.Templates)
            {
                foreach (IField templateField in template.Fields)
                {
                    Label lbl = new Label();
                    lbl.Text = templateField.Name;
                    lbl.Top = iTop;
                    lbl.Left = 20;
                    lbl.Parent = fieldsSplitContainer.Panel1;
                    lbl.Width = fieldsSplitContainer.Panel1.Width;
                    lbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                    IField itemField = Util.GetFieldByID(templateField.TemplateFieldID, _startItem.Fields);

                    string sContent = templateField.Content;
                    if (itemField != null)
                        sContent = itemField.Content;

                    TextBox textBox = new TextBox();
                    textBox.Text = sContent;
                    textBox.Name = templateField.Name;
                    if (itemField == null)
                        textBox.Enabled = false;

                    textBox.Top = iTop;
                    textBox.Left = 10;
                    textBox.Parent = fieldsSplitContainer.Panel2;
                    textBox.Width = fieldsSplitContainer.Panel2.Width - 20;
                    textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                    iTop = iTop + textBox.Height + 10;
                    fieldsSplitContainer.Panel1.Controls.Add(lbl);
                    fieldsSplitContainer.Panel2.Controls.Add(textBox);
                }
            }
        }

        private void ItemEditForm_Shown(object sender, EventArgs e)
        {
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboFromLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == comboFromLanguage)
            {
                ItemEditForm_Shown(sender, e);
            }
        }

        private void fieldsSplitContainer_Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            fieldsSplitContainer.Panel2.VerticalScroll.Value = fieldsSplitContainer.Panel1.VerticalScroll.Value;
        }

        private void fieldsSplitContainer_Panel2_Scroll(object sender, ScrollEventArgs e)
        {
            fieldsSplitContainer.Panel1.VerticalScroll.Value = fieldsSplitContainer.Panel2.VerticalScroll.Value;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            foreach (Control ctrl in fieldsSplitContainer.Panel2.Controls)
            {
                TextBox txtBox = ctrl as TextBox;
                if (txtBox != null)
                {
                    IField field = _startItem.Fields.GetFieldByName(txtBox.Name);
                    if (field != null)
                        field.Content = txtBox.Text;
                }
            }
            _startItem.Save();
            Close();
        }
    }
}
