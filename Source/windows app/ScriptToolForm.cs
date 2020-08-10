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
    public partial class ScriptToolForm : Form
    {
        public IItem _leftStartItem = null;
        public IItem _rightStartItem = null;

        public ScriptToolForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
        }

        private void btnTransferFields_Click(object sender, EventArgs e)
        {
            string[] sItemList = tbEditorControl.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            _leftStartItem.Options.Language = tbLanguage.Text;
            _rightStartItem.Options.Language = tbLanguage.Text;

            foreach (string sPath in sItemList)
            {
                IItem srcItem = _leftStartItem.GetItem(sPath);
                IItem dstItem = _rightStartItem.GetItem(sPath);

                if ((srcItem == null) || (dstItem == null))
                {
                    continue;
                }

                tbResult.Text += sPath + "\r\n";
                dstItem.CopyTo(srcItem, false, false);

/*
                foreach (IField field in srcItem.Fields)
                {

                    if (field.Content.Contains(tbSearchFieldValue.Text))
                    {

                        IField dstField = Util.GetFieldByName(field.Name, dstItem.Fields);
                        if (dstField != null)
                        {
                            string sTmpContent =  SitecoreConverter.Plugins.HtmlToXhtmlPlugin.FixContent(field.Content);
                            dstField.Content = sTmpContent;
                        }

                        dstItem.Save();
                    }
                }
*/
            }
            tbResult.Text += "Finished copying" + "\r\n";


            if (Util.WarningList.Count > 0)
            {
                ViewTextForm viewTextForm = new ViewTextForm();
                viewTextForm.RichEdit.Text = "A number of warnings occurred while copying content, they are: \n";
                foreach (string sWarning in Util.WarningList)
                    viewTextForm.RichEdit.Text += sWarning + "\n";
                viewTextForm.ShowDialog(this);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] sItemList = tbEditorControl.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            _leftStartItem.Options.Language = tbLanguage.Text;
            _rightStartItem.Options.Language = tbLanguage.Text;

            CopyFields(_leftStartItem, _rightStartItem);
        }

        private void CopyFields(IItem srcItem, IItem dstItem)
        {
/*
            IItem[] dstChildren = dstItem.GetChildren();
            foreach (IItem srcChild in srcItem.GetChildren())
            {
                IItem dstChild = null;
                foreach (IItem child in dstChildren)
                {
                    if (srcChild.Name == child.Name)
                    {
                        dstChild = child;
                        break;
                    }
                }

                IField dstTitleField = Util.GetFieldByName("Title", dstChild.Fields);
                IField srcTitleField = Util.GetFieldByName("Title", srcChild.Fields);
                if ((srcTitleField != null) && (srcTitleField.Content != ""))
                    dstTitleField.Content = 
            }
*/
        }
    }
}
