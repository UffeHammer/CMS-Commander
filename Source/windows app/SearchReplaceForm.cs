using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SitecoreConverter.Core;
using System.Threading;

namespace SitecoreConverter
{
    public partial class SearchReplaceForm : Form
    {
        public IItem _startItem = null;
        public Dictionary<string, IItem> _itemsFound = new Dictionary<string, IItem>();
        private bool _bStop = false;

        public delegate void ExpandNode(IItem item);
        public ExpandNode _myExpandNode = null;

        public SearchReplaceForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnSearch.Enabled = false;   
            _startItem.Options.Language = comboFromLanguage.Text;
            Search(_startItem, tbSearchFor.Text);
            _bStop = false;
            btnSearch.Enabled = true;
        }

        private void Search(IItem rootItem, string sFind)
        {            
            foreach (IItem item in rootItem.GetChildren())
            {
                if (_bStop)
                    return;

                tbSearchingIn.Text = item.Path;
                foreach (IField field in item.Fields)
                {
                    if (field.Content.ToLower().IndexOf(sFind.ToLower()) > -1)
                    {
                        lbSearchResult.Items.Add(item.Path);
                        lock (_itemsFound)
                        {
                            _itemsFound.Add(item.Path, item);
                        }                        
                        break;
                    }
                }
                Application.DoEvents();
                _myExpandNode(item);

                Application.DoEvents();
                if (item.HasChildren())
                {
                    Search(item, sFind);
                }
            }            
        }

        private void SearchReplaceForm_Shown(object sender, EventArgs e)
        {
            lblFromPath.Text = _startItem.Path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _bStop = true;
        }

        private void lbSearchResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string sPath = (string)lbSearchResult.SelectedItem;

            foreach (IItem item in _itemsFound.Values)
            {
                if (item.Path == sPath)
                {
                    _myExpandNode(item);
                }
            }
            
        }

    }
}
