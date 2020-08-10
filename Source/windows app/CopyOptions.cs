using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SitecoreConverter.Core;
using System.Globalization;
using System.Threading;

namespace SitecoreConverter
{
    public partial class CopyOptions : Form
    {
        public IItemCopyPlugin[] _itemCopyPlugins = null;

        public CopyOptions()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            for (int d=0; d<tabControl1.TabPages.Count; d++)
            {
                TabPage page = tabControl1.TabPages[d];
                IPluginOption[] options = null;
                if (_itemCopyPlugins != null)
                {
                    for (int t = 0; t < _itemCopyPlugins.Length; t++)
                    {
                        if (page.Name == _itemCopyPlugins[t].Name)
                        {
                            options = _itemCopyPlugins[t].PluginOptions;
                            break;
                        }
                    }
                }

                // Found an option that matched a page
                if (options != null)
                {
                    for (int t = 0; t < options.Length; t++)
                    {
                        Control[] ctlResults = page.Controls.Find(options[t].Name, true);
                        if (ctlResults != null)
                        {
                            if (options[t].Type == PluginOptionTypes.CheckBox)
                            {
                                options[t].Value = (ctlResults[0] as CheckBox).Checked.ToString();
                            }
                            else if (options[t].Type == PluginOptionTypes.TextBox)
                            {
                                options[t].Value = (ctlResults[0] as TextBox).Text;
                            }
                        }
                    }

                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            tbSecurityDomain.Enabled = cbCopySecurity.Checked;
            tbRootRole.Enabled = cbCopySecurity.Checked;
            // cbSetItemRightsExplicitly.Enabled = cbCopySecurity.Checked;
            if (cbCopySecurity.Checked)
            {
                tbSecurityDomain.Text = "sitecore";
                tbRootRole.Text = "AfdelingsRod";
            }
            else
            {
                tbSecurityDomain.Text = "";
                tbRootRole.Text = "";
            }
        }

        private void CopyOptions_Load(object sender, EventArgs e)
        {

        }

        private void SetControlOptions(Control obj, Control parent, string sText, int iTop, int iLeft, int iWidth, int iHeight)
        {
            obj.Parent = parent;
            obj.Text = sText;
            obj.Top = iTop;
            obj.Left = iLeft;
            obj.Width = iWidth;
            if (iHeight !=  -1)
                obj.Height = iHeight;
        }

        private void CopyOptions_Shown(object sender, EventArgs e)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

            for (int t = 0; t < comboFromLanguage.Items.Count; t++)
            {
                if (comboFromLanguage.Items[t].ToString().ToLower() == currentCulture.TwoLetterISOLanguageName.ToLower())
                    comboFromLanguage.SelectedIndex = t;
            }
            
            if (_itemCopyPlugins != null)
            {
                for (int t=0; t<_itemCopyPlugins.Length; t++)
                {
                    if (!tabControl1.TabPages.ContainsKey(_itemCopyPlugins[t].Name))
                    {
                        const int MARGIN = 10;
                        const int CONTROLWIDTH = 220;
                        const int CONTROLHEIGHT = 30;

                        int y = MARGIN * 2;
                        int x = MARGIN;

                        tabControl1.TabPages.Add(_itemCopyPlugins[t].Name, _itemCopyPlugins[t].Name);
                        TabPage page = tabControl1.TabPages[tabControl1.TabPages.Count - 1];
                        GroupBox gbOptionsBorder = new GroupBox();
                        gbOptionsBorder.Parent = page;
                        gbOptionsBorder.Location = new Point(MARGIN, MARGIN);
                        gbOptionsBorder.Size = new Size(page.Width - (MARGIN * 2), page.Height - (MARGIN * 2));
                        gbOptionsBorder.Text = "Options";
                        gbOptionsBorder.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                        IPluginOption[] options = _itemCopyPlugins[t].PluginOptions;
                        for (int d=0; d<options.Length; d++)
                        {
                            x = MARGIN;

                            Label lbl = new Label();
                            SetControlOptions(lbl, gbOptionsBorder, options[d].Name, y, x, CONTROLWIDTH, CONTROLHEIGHT);
                            // lbl.AutoSize = true;
                            x += CONTROLWIDTH;

                            if (options[d].Type == PluginOptionTypes.TextBox)
                            {
                                TextBox tmp = new TextBox();
                                tmp.Name = options[d].Name;
                                SetControlOptions(tmp, gbOptionsBorder, options[d].Value, y, x, CONTROLWIDTH, -1);
                            }
                            else if (options[d].Type == PluginOptionTypes.CheckBox)
                            {
                                CheckBox tmp = new CheckBox();
                                tmp.Name = options[d].Name;
                                SetControlOptions(tmp, gbOptionsBorder, "Enable", y, x, CONTROLWIDTH, -1);
                                if (options[d].Value == "True")
                                    tmp.Checked = true;
                            }

                            y += CONTROLHEIGHT;
                        }

                    }
                }
                
            }
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void comboFromLanguage_SelectedValueChanged(object sender, EventArgs e)
        {
        }

        private void comboFromLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboFromLanguage.SelectedIndex > -1)
            {
                
                string sSelected = comboFromLanguage.Items[comboFromLanguage.SelectedIndex].ToString();
                if (comboToLanguage.Items.IndexOf(sSelected) > -1)
                    comboToLanguage.SelectedIndex = comboToLanguage.Items.IndexOf(sSelected);
            }

        }

    }
}
