using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SitecoreConverter.Core
{
    public enum CopyOperations { Overwrite, SkipExisting, GenerateNewItemIDs, UseNames };
    public delegate void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem);
    public delegate bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem);

    public class ConverterOptions
    {
        // Default language is english because it is required for correct operation of Sitecore, so
        // we can assume that it will always exist
        public string Language = "en";
        public bool NewID = false;
        public CopyOperations CopyOperation = CopyOperations.Overwrite;
        public bool RecursiveOperation = true;
        public bool IgnoreErrors = false;
        public CopyItemCallback CopyItem = null;
        public ShouldItemBeCopiedCallback ShouldItemBeCopied = null;
        public bool CopySecuritySettings = false;
        public bool SetItemRightsExplicitly = true;
        public bool UsersCopied = false;
        public string DefaultSecurityDomain = "";
        public string RootRole = "";
        // Login and password so that we can resume timedout sessions
        public string LoginName = "";
        public string LoginPassword = "";
        public string HostName = "";
        public string Database = "master";
        // Caching of template items 
        public Dictionary<string, IItem> ExistingTemplates = new Dictionary<string, IItem>();
        // Caching of template fields items 
        public Dictionary<string, XmlNode> ExistingTemplateFields = new Dictionary<string, XmlNode>();
    }
}
