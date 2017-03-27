using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core.Plugins
{

    class MoveItemToDateFolderPlugin : IItemCopyPlugin
    {
        private PluginOption[] _PluginOptions = null;
        public MoveItemToDateFolderPlugin()
        {
            _PluginOptions = new PluginOption[4];
            _PluginOptions[0] = new PluginOption("Enable moving items to Date folders :", "False", PluginOptionTypes.CheckBox);
            _PluginOptions[1] = new PluginOption("First folder date format options:", "yyyy", PluginOptionTypes.TextBox);
            _PluginOptions[2] = new PluginOption("Second folder date format options (if omitted no folder will be created):", "MMMM", PluginOptionTypes.TextBox);
            _PluginOptions[3] = new PluginOption("Move items with template:", "Document", PluginOptionTypes.TextBox);
        }

        public string Name
        {
            get
            {
                return "Move item to date folder";
            }
        }

        public IPluginOption[] PluginOptions
        {
            get
            {
                return _PluginOptions;
            }
        }

        public void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            if (_PluginOptions[0].Value == "False")
                return;

            foreach (IItem sourceTemplate in sourceItem.Templates)
            {
                // Check if the correct template exists on the item
                if (sourceTemplate.Name.ToLower() != _PluginOptions[3].Value.ToLower())
                    continue;

                IField field = Util.GetFieldByName("__publish", destinationItem.Fields);
                if (field == null)
                {
                    field = Util.GetFieldByName("__valid to", destinationItem.Fields);
                    if (field == null)
                    {
                        field = Util.GetFieldByName("__created", destinationItem.Fields);
                    }
                }
                if (field == null)
                    throw new Exception("Error finding any valid date field in " + this.Name + " plugin.");


                DateTime date = Util.XsdDatetimeToDateTime(field.Content);
                string sDate = date.ToString(_PluginOptions[1].Value);

                string sYearPath = destinationParentItem.Path + "/" + sDate;
                IItem yearFolder = destinationParentItem.GetItem(sYearPath);
                if (yearFolder == null)
                {
                    string sFolderID = destinationParentItem.AddFromTemplate(sDate, "/sitecore/templates/common/folder");
                    yearFolder = destinationItem.GetItem(sFolderID);
                }
                bool bResult = false;
                if (_PluginOptions[2].Value == "")
                    bResult = destinationItem.MoveTo(yearFolder);
                else
                {
                    string sMonth = date.ToString(_PluginOptions[2].Value);
                    IItem monthFolder = destinationParentItem.GetItem(sYearPath + "/" + sMonth);
                    if (monthFolder == null)
                    {
                        string sFolderID = yearFolder.AddFromTemplate(sMonth, "/sitecore/templates/common/folder");
                        monthFolder = destinationItem.GetItem(sFolderID);
                        monthFolder.SortOrder = date.ToString("MM");
                        monthFolder.Save();
                    }
                    destinationItem.MoveTo(monthFolder);
                }
            }
        }

        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }

    }
}
