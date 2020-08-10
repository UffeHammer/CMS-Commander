using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SitecoreConverter.Core;
using System.IO;
using System.Diagnostics;

namespace SitecoreConverter
{
    public partial class CompareForm : Form
    {
        public delegate void ExpandNode(IItem item, TreeNode selectedNode, TreeView selectedTreeView);
        public IItem _leftStartItem = null;
        public IItem _rightStartItem = null;
        public TreeView _leftSelectedTreeView = null;
        public TreeView _rightSelectedTreeView = null;
        public TreeNode _leftSelectedTreeNode = null;
        public TreeNode _rightSelectedTreeNode = null;
        public Dictionary<string, IItem> _leftItemsFound = new Dictionary<string, IItem>();
        public Dictionary<string, IItem> _rightItemsFound = new Dictionary<string, IItem>();
        private bool _bStop = false;

        public ExpandNode _myExpandNode = null;

        public CompareForm()
        {
            InitializeComponent();
        }

        private void lbSearchResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void CompareForm_Shown(object sender, EventArgs e)
        {
            lblLeftFromPath.Text = "Left path: " + _leftStartItem.Path;
            lblRightFromPath.Text = "Right path: " + _rightStartItem.Path;
            btnSaveToLeftSide.Enabled = false;
            btnSaveToRightSide.Enabled = false;
            btnGenerateReport.Enabled = false;

            _leftSelectedTreeNode = _leftSelectedTreeView.SelectedNode;
            _rightSelectedTreeNode = _rightSelectedTreeView.SelectedNode;
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            lbCompareResult.Items.Clear();
            _rightItemsFound.Clear();
            _leftItemsFound.Clear();

            btnCompare.Enabled = false;
            _leftStartItem.Options.Language = comboFromLanguage.Text;
            _rightStartItem.Options.Language = comboFromLanguage.Text;
            Compare(_leftStartItem, _rightStartItem);
            _bStop = false;
            btnCompare.Enabled = true;
            btnSaveToRightSide.Enabled = true;
            btnSaveToLeftSide.Enabled = true;
            btnGenerateReport.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _bStop = true;
        }

        private Guid REVISIONGUID = new Guid("{8cdc337e-a112-42fb-bbb4-4143751e123f}");
        private Guid UPDATEDGUID = new Guid("{d9cf14b1-fa16-4ba6-9288-e8a174d4d522}");
        private Guid UPDATEDBYGUID = new Guid("{badd9cf9-53e0-4d0c-bcc0-2d784c282f6a}");
        private Guid CREATEDGUID = new Guid("{25bed78c-4957-4165-998a-ca1b52f67497}");
        private Guid CREATEDBYGUID = new Guid("{5dd74568-4d4b-44c1-b513-0af5f4cda34f}");
        private Guid OWNERGUID = new Guid("{52807595-0f8f-4b20-8d2a-cb71d28c6103}");
        private Guid LOCKGUID = new Guid("{001dd393-96c5-490b-924a-b0f25cd9efd8}");
        
        private void Compare(IItem leftRootItem, IItem rightRootItem)
        {            
            List <IItem> rightChildren = new List<IItem>(rightRootItem.GetChildren());
            rightChildren.Add(rightRootItem.GetItem(rightRootItem.ID));

            List<IItem> leftChildren = new List<IItem>(leftRootItem.GetChildren());
            leftChildren.Add(leftRootItem.GetItem(leftRootItem.ID));

            foreach (IItem leftItem in leftChildren)
            {
                if (_bStop)
                    return;

                tbSearchingIn.Text = leftItem.Path;

                // Find the correct right hand item
                IItem rightItem = rightChildren.Find(x =>x.ID == leftItem.ID);
                if (rightItem == null)
                {
                    lbCompareResult.Items.Add(leftItem.Path + "  --> item not found in " + rightRootItem.Options.HostName);
                    lock (_leftItemsFound)
                    {
                        _leftItemsFound.Add(leftItem.ID.ToString(), leftItem);
                    }
                    continue;
                }

//                CompareFields(leftItem.Fields, rightItem.Fields);
//                CompareFields(rightItem.Fields, leftItem.Fields);

                 
                foreach (IField leftField in leftItem.Fields)
                {
                    // Find the correct right hand field
                    IField rightField = null;
                    foreach (IField field in rightItem.Fields)
                    {
                        if (field.TemplateFieldID == leftField.TemplateFieldID)
                            rightField = field;
                    }

                    // If field is missing then that counts for a diference
                    if (rightField == null)                        
                    {
                        // Also find missing fields
                        if (cbMissingFields.Checked)
                        {

                            if (leftField.TemplateFieldID == Util.GuidToSitecoreID(LOCKGUID))
                                continue;

                            lbCompareResult.Items.Add(leftItem.Path);
                            _leftItemsFound.Add(leftItem.ID.ToString(), leftItem);
                            _rightItemsFound.Add(rightItem.ID.ToString(), rightItem);
                            break;
                        }
                        else
                            continue;
                    }

                    // Compare all other fields than the Revision, which might always be different
                    if (leftField.Content.ToLower() != rightField.Content.ToLower())
                    {

                        if ((cbIgnoreStandardFields.Checked) &&
                        ((leftField.TemplateFieldID == Util.GuidToSitecoreID(REVISIONGUID)) ||
                        (leftField.TemplateFieldID == Util.GuidToSitecoreID(UPDATEDGUID)) ||
                        (leftField.TemplateFieldID == Util.GuidToSitecoreID(UPDATEDBYGUID)) ||
                        (leftField.TemplateFieldID == Util.GuidToSitecoreID(CREATEDGUID)) ||
                        (leftField.TemplateFieldID == Util.GuidToSitecoreID(CREATEDBYGUID)) ||
                        (leftField.TemplateFieldID == Util.GuidToSitecoreID(OWNERGUID))))
                        {
                            // Ignore this field 
                        }
                        else
                        {

                            if (!lbCompareResult.Items.Contains(leftItem.Path))
                                lbCompareResult.Items.Add(leftItem.Path + " - Field: " + leftField.Name + " has different content.");
                            if (!_leftItemsFound.ContainsKey(leftItem.ID.ToString()))
                                _leftItemsFound.Add(leftItem.ID.ToString(), leftItem);

                            if (!_rightItemsFound.ContainsKey(rightItem.ID.ToString()))
                                _rightItemsFound.Add(rightItem.ID.ToString(), rightItem);

                            //break;
                        }
                    }
                }

                Application.DoEvents();
                _myExpandNode(leftItem, _leftSelectedTreeNode, _leftSelectedTreeView);
                _myExpandNode(rightItem, _rightSelectedTreeNode, _rightSelectedTreeView);

                Application.DoEvents();
                if (leftItem.HasChildren())
                {
                    Compare(leftItem, rightItem);
                }
            }

            // Find left hand source
            foreach (IItem rightItem in rightChildren)
            {
                IItem leftItem = leftChildren.Find(x => x.ID == rightItem.ID); 
                if (leftItem == null)
                {
                    lbCompareResult.Items.Add(rightItem.Path + "  --> item not found in " + leftRootItem.Options.HostName);
                    lock (_rightItemsFound)
                    {
                        _rightItemsFound.Add(rightItem.ID.ToString(), rightItem);
                    }
                }
            }
        }

        private void CompareFields(IField[] fields1, IField[] fields2)
        {

        }


        private void Reset()
        {
            _leftItemsFound.Clear();
            _rightItemsFound.Clear();
            lbCompareResult.Items.Clear();

            btnSaveToLeftSide.Enabled = false;
            btnSaveToRightSide.Enabled = false;
            btnGenerateReport.Enabled = false;
        }


        private void btnSaveToLeftSide_Click(object sender, EventArgs e)
        {
            foreach (IItem rightItem in _rightItemsFound.Values)
            {
                IItem leftItem = null;
                if (_leftItemsFound.TryGetValue(rightItem.ID.ToString(), out leftItem))
                    leftItem.CopyTo(rightItem, false, false);
                else if (_rightStartItem.GetItem(rightItem.Parent.ID.ToString()) != null)
                {
                    leftItem = _leftStartItem.GetItem(rightItem.Parent.ID.ToString());
                    if (leftItem != null)
                        leftItem.CopyTo(rightItem, true, false);
                }
            }
            Reset();
        }

        private void btnSaveToRightSide_Click(object sender, EventArgs e)
        {
            foreach (IItem leftItem in _leftItemsFound.Values)
            {
                IItem rightItem = null;
                if (_rightItemsFound.TryGetValue(leftItem.ID.ToString(), out rightItem))
                    rightItem.CopyTo(leftItem, false, false);
                else if (_rightStartItem.GetItem(leftItem.Parent.ID.ToString()) != null)
                {
                    rightItem = _rightStartItem.GetItem(leftItem.Parent.ID.ToString());
                    if (rightItem != null)
                        rightItem.CopyTo(leftItem, true, false);
                }
            }

            Reset();
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            string fileName = System.IO.Path.GetTempPath() + "Report_" + Guid.NewGuid().ToString() + ".txt";

            // create a writer and open the file
            TextWriter tw = new StreamWriter(fileName);

            for (int t = 0; t < lbCompareResult.Items.Count; t++)
            {                                
                // write a line of text to the file
                tw.WriteLine(lbCompareResult.Items[t].ToString());
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
