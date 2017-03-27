using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SitecoreConverter.Core.Plugins
{
    public class IconPathConverterPlugin : IItemCopyPlugin
    {
        private PluginOption[] _PluginOptions = null;

        public IconPathConverterPlugin()
        {
            _PluginOptions = new PluginOption[1];
            _PluginOptions[0] = new PluginOption("Enable IconPathConverter:", "False", PluginOptionTypes.CheckBox);
        }

        #region IItemCopyPlugin Members

        public string Name
        {
            get { return "Icon Path Converter"; }
        }

        public IPluginOption[] PluginOptions
        {
            get { return _PluginOptions; }
        }

        public void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            try
            {
                const string newPath = "/Resources/Desktop/Icons/";

                if (_PluginOptions[0].Value == "False")
                    return;

                if (!sourceItem.Icon.Contains(newPath) && !sourceItem.Icon.Trim().Equals(string.Empty))
                {
                    FileInfo fi = new FileInfo(sourceItem.Icon);
                    destinationItem.Icon = newPath + fi.Name;
                }

                
                // This is either a template or a stndard value
                if (sourceItem.Path.ToLower().IndexOf("/sitecore/templates") == 0)
                {
                    if (!sourceItem.Icon.Contains(newPath) && !sourceItem.Icon.Trim().Equals(string.Empty))
                    {
                        FileInfo fi = new FileInfo(sourceItem.Icon);
                        destinationItem.Icon = newPath + fi.Name;
                    }
                    else
                    {
                        // Try and find using the __icon using fields
                        IField sourceField = sourceItem.Fields.Last<IField>(currentField => (currentField.Key.ToLower().Equals("__icon")));
                        IField destinationField = destinationItem.Fields.Last<IField>(currentField => (currentField.Key.ToLower().Equals("__icon")));
                        if (sourceField != null && destinationField != null)
                        {
                            if (!sourceField.Content.Contains(newPath) && !sourceField.Content.Trim().Equals(string.Empty))
                            {
                                FileInfo fi = new FileInfo(sourceField.Content);
                                destinationField.Content = newPath + fi.Name;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }

        #endregion


    }
}
