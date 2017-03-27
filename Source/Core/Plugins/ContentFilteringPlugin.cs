using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core.Plugins
{
    public class ContentFilteringPlugin : IItemCopyPlugin
    {
        private PluginOption[] _PluginOptions = null;
        public ContentFilteringPlugin()
        {
            _PluginOptions = new PluginOption[4];
            _PluginOptions[0] = new PluginOption("Enable content filter:", "False", PluginOptionTypes.CheckBox);
            _PluginOptions[1] = new PluginOption("Requried field name:", "Status", PluginOptionTypes.TextBox);
            _PluginOptions[2] = new PluginOption("Requried field value containing:", "skal kopieres til nyt sdu website", PluginOptionTypes.TextBox);
            _PluginOptions[3] = new PluginOption("Field has to exist:", "False", PluginOptionTypes.CheckBox);
        }   

        public string Name
        {
            get { return "Content filtering"; }
        }

        public IPluginOption[] PluginOptions
        {
            get { return _PluginOptions; }
        }

        public void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            return;
        }


        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            if (_PluginOptions[0].Value == "False")
                return true;

            // Skip if status field is set
            IField statusField = Util.GetFieldByName(_PluginOptions[1].Value, sourceItem.Fields);
            if (statusField == null)
            {
                foreach (IItem template in sourceItem.Templates)
                {
                    // Template has standard values
                    statusField = Util.GetFieldByName(_PluginOptions[1].Value, template.Fields);
                    if (statusField != null)
                        break;
                }
            }

            if (statusField != null)
            {
                // Contains value "skal kopieres til nyt sdu website"
                if (!statusField.Content.ToLower().Contains(_PluginOptions[2].Value.ToLower()))
                    return false;
            }
            else if (_PluginOptions[3].Value == "True")
            {
                // Field has to exist
                return false;
            }

            return true;
        }
    }
}
