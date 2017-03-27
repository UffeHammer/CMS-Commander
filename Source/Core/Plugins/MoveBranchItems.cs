using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core.Plugins
{
    class MoveBranchItems : IItemCopyPlugin
    {
        private PluginOption[] _PluginOptions = null;
        public MoveBranchItems()
        {
            _PluginOptions = new PluginOption[1];
            _PluginOptions[0] = new PluginOption("Enable transform masters to branch items:", "False", PluginOptionTypes.CheckBox);
        }

        public string Name
        {
            get
            {
                return "Transform master to branch items";
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

            // We are copying from masters to branches
            if ((sourceItem.Path.ToLower().IndexOf("/sitecore/masters") > -1) &&
                (destinationItem.Path.ToLower().IndexOf("/sitecore/templates/branches") > -1) &&
                (!sourceItem.Name.StartsWith("__")) && (sourceItem.Name != "masters"))
            {
                Sitecore6xItem destination6xItem = destinationItem as Sitecore6xItem;
                Sitecore6xItem destinationParent6xItem = destinationParentItem as Sitecore6xItem;

                IItem newDestinationItem = destinationParent6xItem.GetItem(destinationParent6xItem.Path + "/$name");
                if (newDestinationItem == null)
                {

                    destination6xItem.CopyTo(destinationParentItem, "$name");

                    newDestinationItem = destinationParent6xItem.GetItem(destinationParent6xItem.Path + "/$name");

                    string sPath = destinationParent6xItem.CreateTemplateItemWithSpecificID(
                                destinationParent6xItem.ID,
                                "/sitecore/templates/System/Branches/Branch",
                                sourceItem.ID,
                                sourceItem.Name);

                    Sitecore6xItem branchItem = destinationParentItem.GetItem(destinationParent6xItem.Path + "/" + sourceItem.Name) as Sitecore6xItem;

                    newDestinationItem.MoveTo(branchItem);

                    if (destination6xItem.Icon != "")
                        branchItem.Icon = destination6xItem.Icon;
                    else
                        branchItem.Icon = destination6xItem.Templates[0].Icon;

                    branchItem.Save();
                }
                // This is a child of a master, so it only needs to be moved
                else
                {
                    destinationItem.MoveTo(newDestinationItem);
                }
            }
        }

        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }
    }
}
