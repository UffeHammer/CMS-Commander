using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core
{
    public interface IItem
    {
        string ID { get; }
        string Name { get; }
        string Key { get; }
        string Path { get; }
        string Icon { get; set; }
        string SortOrder { get; set; }

        IItem[] Templates { get; }
        IItem BaseTemplate { get; }
        IField[] Fields { get; }
        IRole[] Roles { get; }
        IRole[] Users { get; }

        IItem Parent { get; }
        IItem[] GetChildren();
        IItem GetItem(string sItemPath);

        void CopyTo(IItem CopyFrom, bool bRecursive, bool bOnlyChildren);
        bool MoveTo(IItem MoveTo);
        void Rename(string Name);
        void Delete();
        void Save();
        string AddFromTemplate(string sName, string sTemplatePath);
        bool HasChildren();
        string[] GetLanguages();

        ConverterOptions Options { get; }
        string GetOuterXml();
        string GetHostUrl();
    }
}
