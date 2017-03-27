using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core
{
    public enum PluginOptionTypes { undefined, TextBox, MemoBox, CheckBox};
    public interface IPluginOption
    {
        string Name { get; }
        string Value { get; set; }
        PluginOptionTypes Type  { get; }
        string InitialValue{ get;}
    }

    public interface IItemCopyPlugin
    {
        string Name { get; }
        IPluginOption[] PluginOptions { get; }
        void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem);
        bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem);
    }
}
