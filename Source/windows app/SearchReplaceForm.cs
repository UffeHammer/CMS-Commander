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
using System.Globalization;
using System.IO;
using System.Diagnostics;

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
            Search(_startItem, tbSearchName.Text, tbSearchForContent.Text, tbSearchForTemplate.Text);
            _bStop = false;
            btnSearch.Enabled = true;
        }

        private void Search(IItem rootItem, string sFindName, string sFindContent, string sFindTemplate)
        {            
            foreach (IItem item in rootItem.GetChildren())
            {
                if (_bStop)
                    return;

                tbSearchingIn.Text = item.Path;
                if (sFindName != "")
                {
                    if (item.Name.ToLower().IndexOf(sFindName.ToLower()) > -1)
                    {
                        lbSearchResult.Items.Add(item.Path);
                        lock (_itemsFound)
                        {
                            if (!_itemsFound.ContainsKey(item.Path))
                                _itemsFound.Add(item.Path, item);
                        }
                    }
                }

                // Search for content?
                if (sFindContent != "")
                {
                    foreach (IField field in item.Fields)
                    {
                        if (field.Content.ToLower().IndexOf(sFindContent.ToLower()) > -1)
                        {
                            lbSearchResult.Items.Add(item.Path);
                            lock (_itemsFound)
                            {
                                if (!_itemsFound.ContainsKey(item.Path))
                                    _itemsFound.Add(item.Path, item);
                            }
                            break;
                        }
                    }
                }

                if (sFindTemplate != "")
                {
                    // {CE6AB190-29A4-4757-9FB2-122FBF4E78D9}
                    foreach (IItem template in item.Templates)
                    {
                        if ((template.Name.ToLower().IndexOf(sFindTemplate.ToLower()) > -1) ||
                            (template.ID.ToLower().IndexOf(sFindTemplate.ToLower()) > -1))
                        {
                            lbSearchResult.Items.Add(item.Path);
                            lock (_itemsFound)
                            {
                                if (!_itemsFound.ContainsKey(item.Path))
                                    _itemsFound.Add(item.Path, item);
                            }
                        }
                    }

                }


                Application.DoEvents();
                _myExpandNode(item);

                Application.DoEvents();
                if (item.HasChildren())
                {
                    Search(item, sFindName, sFindContent, sFindTemplate);
                }
            }            
        }

        private void SearchReplaceForm_Shown(object sender, EventArgs e)
        {
            lblFromPath.Text = _startItem.Path;

            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            for (int t = 0; t < comboFromLanguage.Items.Count; t++)
            {
                if (comboFromLanguage.Items[t].ToString().ToLower() == currentCulture.TwoLetterISOLanguageName.ToLower())
                    comboFromLanguage.SelectedIndex = t;
            }
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


        private void btnReport_Click(object sender, EventArgs e)
        {
            string fileName = System.IO.Path.GetTempPath() + "Report_" + Guid.NewGuid().ToString() + ".txt";

            // create a writer and open the file
            TextWriter tw = new StreamWriter(fileName);

            for (int t = 0; t < lbSearchResult.Items.Count; t++)
            {
                // write a line of text to the file
                tw.WriteLine(lbSearchResult.Items[t].ToString());
            }

            // close the stream
            tw.Close();

            Process notePad = new Process();

            notePad.StartInfo.FileName = "notepad.exe";
            notePad.StartInfo.Arguments = fileName;
            notePad.Start();
        }
    }
}
