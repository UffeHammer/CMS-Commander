using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.Specialized;

namespace SitecoreConverter.Core
{
    public class Sitecore43Item : IItem
    {
        private Guid _ID = Guid.Empty;
        private string _sName = "";
        private string _sKey = "";
        private string _sPath = "";
        private string _sIcon = "";
        private Guid _TemplateID = Guid.Empty;
        private string _sTemplateName = "";
        private Sitecore43Item[] _Templates = null;
        private List<Sitecore43Field> _fields = new List<Sitecore43Field>();
        private BaseRole[] _Roles = null;
        private Sitecore43.SitecoreClientAPI _sitecoreApi = null;
        private string _sXmlNode = "";
        private IItem _Parent = null;
        private string _sSortOrder = "";
        private bool _bHasChildren = false;

        // Global variables
        private static Dictionary<string, Sitecore43Item> _existingTemplates = new Dictionary<string, Sitecore43Item>();
        private static StringDictionary _roleNames = new StringDictionary();
        private static ConverterOptions _Options = new ConverterOptions();
        // Theres only one base template in Sitecore 4.3, so we can make it static
        // and avoid fetching it more than one time.
        private static Sitecore43Item _BaseTemplate = null;

        public string ID
        {
            get
            {
                return Util.GuidToSitecoreID(_ID);
            }
        }

        public string Name 
        {
            get
            {
                return _sName;
            }
            set
            {
                _sName = value;
            }
        }
        
        public string Key 
        {
            get
            {
                return _sKey;
            }
            set
            {
                _sKey = value;
            }
        }

        public string Path 
        {
            get
            {
                return _sPath;
            }
            set
            {
                _sPath = value;
            }
        }

        public string Icon
        {
            get
            {
                return _sIcon;
            }
            set
            {
                _sIcon = value;
            }
        }

        public Guid TemplateID
        {
            get
            {
                return _TemplateID;
            }
        }

        public string TemplateName
        {
            get
            {
                return _sTemplateName;
            }
        }

        public string SortOrder
        {
            get
            {
                return _sSortOrder;
            }
            set
            {
                _sSortOrder = value;
            }
        }

        public IItem[] Templates
        {
            get
            {
                if (_Templates == null)
                {
                    _Templates = new Sitecore43Item[1];
                    // This IS basetemplate return null
                    if ((_TemplateID == Guid.Empty) && (Util.GuidToSitecoreID(_ID) == BaseTemplate.ID))
                        _Templates[0] = null;
                    // This is a template return basetemplate
                    else if ((_TemplateID == Guid.Empty) && (Util.GuidToSitecoreID(_ID) != BaseTemplate.ID))
                        _Templates[0] = _BaseTemplate;
                    // Normal item
                    else
                        _Templates[0] = GetSitecore43Item(Util.GuidToSitecoreID(_TemplateID));
                    // Cache template
                    if (_Templates[0] != null)
                        AddItemToCache(_Templates[0]);
                }
                return _Templates;
            }
        }

        public IItem BaseTemplate
        {
            get
            {
                if (_BaseTemplate == null)
                {
                    _BaseTemplate = GetSitecore43Item("/sitecore/templates/__Standard template");
                    _BaseTemplate.Name = "Sitecore43 Standard template";
                    _BaseTemplate.Key = _BaseTemplate.Name.ToLower();
                    // Cache template
                    AddItemToCache(_BaseTemplate);
                }
                return _BaseTemplate;
            }
        }

        public IField[] Fields 
        {
            get
            {
                return _fields.ToArray();
            }
        }

        public IRole[] Roles
        {
            get
            {
                if (_Roles == null)
                {
                    if (_ID != new Guid("{11111111-1111-1111-1111-111111111111}"))
                    {
                        string sSecurity = null;
                        try
                        {
                            sSecurity = _sitecoreApi.GetItemSecurity(Util.GuidToSitecoreID(_ID));
                        }
                        catch (Exception ex)
                        {
                            // We are timed out or application has recycled
                            if (ex.Message.Contains("logged in"))
                            {
                                _sitecoreApi.Login(_Options.LoginName, _Options.LoginPassword);
                                sSecurity = _sitecoreApi.GetItemSecurity(Util.GuidToSitecoreID(_ID));
                            }
                            else
                                throw ex;
                        }
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(sSecurity);
                        XmlNodeList groups = doc.SelectNodes("/security/access");
                        if (groups != null)
                        {
                            List<BaseRole> Roles = new List<BaseRole>();
                            for (int t=0; t<groups.Count; t++)
                            {
                                string sName = "";
/*
                                if (groups[t].Attributes["groupname"] != null)
                                    sName = groups[t].Attributes["groupname"].Value;
                                else
                                {
 */ 
                                    // sName = _roleNames.Single(sKey => Key == groups[t].Attributes["group"].Value).Value;
                                    if (_roleNames.ContainsKey(groups[t].Attributes["group"].Value))
                                        sName = _roleNames[groups[t].Attributes["group"].Value];
                                    else
                                    {
                                        try
                                        {
                                            IItem rightsItem = GetItem(groups[t].Attributes["group"].Value);
                                            sName = rightsItem.Name;
                                            _roleNames.Add(groups[t].Attributes["group"].Value, sName);
                                        }
                                        catch
                                        {
                                            // An exception here is typically because the group right item has been deleted
                                            continue;
                                        }
                                    }
//                                }
                                Roles.Add(BaseRole.CreateBaseRoleFromSitecore43Rights(
                                        sName, groups[t].Attributes["rights"].Value));
                            }
                            _Roles = Roles.ToArray();
                        }
                    }
                }
                return _Roles;
            }
        }

        public IRole[] Users
        {
            get
            {
                return null;
//                throw new Exception("Error Users is not implemented.");
            }
        }

        public IItem Parent
        {
            get
            {
                if (_Parent == null)
                {
                    int iLast = _sPath.LastIndexOf("/");
                    if ((iLast > -1) && (_sPath.ToLower() != "/sitecore"))
                    {
                        string sParent = _sPath.Remove(iLast, _sPath.Length - iLast);
                        _Parent = GetSitecore43Item(sParent);
                    }
                }
                return _Parent;
            }
        }        

        public IItem[] GetChildren()
        {
            if (_sitecoreApi == null)
                throw new Exception("sitecoreApi not set please call constructor with API object");

            XmlNode itemNode = _sitecoreApi.GetItemTree("{" + _ID.ToString().ToUpper() + "}", 2);

            XmlNodeList nodeList = itemNode.SelectNodes("./item");
            List<Sitecore43Item> children = new List<Sitecore43Item>();
            foreach (XmlNode node in nodeList)
            {
                // Get full-blown item
//                XmlNode item = _sitecoreApi.GetItem(node.Attributes["id"].Value);
                XmlNode itemXml = _sitecoreApi.GetItemXml(node.Attributes["id"].Value, "", this.Options.Language);
                XmlAttribute attr = itemXml.OwnerDocument.CreateAttribute("haschildren");

                // there is children
                if ((node.SelectNodes("./item") != null) && (node.SelectNodes("./item").Count > 0))
                    attr.Value = "1";
                else
                    attr.Value = "0";          
                itemXml.Attributes.Append(attr);

                children.Add(new Sitecore43Item(itemXml, this, _sitecoreApi));
            }            
            return children.ToArray();
        }


        private Sitecore43Item(XmlNode itemNode, IItem parent, Sitecore43.SitecoreClientAPI sitecoreApi)
        {
            _Parent = parent;
            _sitecoreApi = sitecoreApi;
            
            _ID = new Guid(itemNode.Attributes["id"].Value);
            _sName = itemNode.Attributes["name"].Value;
            _sKey = itemNode.Attributes["key"].Value;
            _sTemplateName = itemNode.Attributes["template"].Value;
             
            // Fetch latest field content
            XmlNode newestContentNode = _sitecoreApi.GetItem(Util.GuidToSitecoreID(_ID));

            _sPath = newestContentNode.Attributes["path"].Value;
            _sIcon = newestContentNode.Attributes["icon"].Value;

            // Only the standard template has an empty TemplateID
            // The item is an template if it's id is the same as the template id
            if ((newestContentNode.Attributes["templateid"].Value == "")  ||
                (newestContentNode.Attributes["templateid"].Value == Util.GuidToSitecoreID(_ID)))
                _TemplateID = Guid.Empty;
            else
                _TemplateID = new Guid(newestContentNode.Attributes["templateid"].Value);

            // Get TemplateID from itemNode instead
//            if ((_TemplateID == Guid.Empty) && (itemNode.Attributes["tid"] != null) && (itemNode.Attributes["tid"].Value != ""))
//                _TemplateID = new Guid(itemNode.Attributes["tid"].Value);



            int iRejected = 0;
            XmlNodeList list = itemNode.SelectNodes("field");
            foreach (XmlNode node in list)
            {
                Sitecore43Field field = new Sitecore43Field(node, _sTemplateName, _sitecoreApi);

/*                
                // Fetch latest field content
                string sContent = newestContentNode.SelectSingleNode("field[@key = '" + field.Key + "']").InnerText;
                if (sContent != "")
                    field.Content = sContent;
*/
                // Only add valid fields
                if (IsValidField(field))
                {
                    _fields.Add(field);
                }
                else
                    iRejected++;

            }
            _sXmlNode = itemNode.OuterXml;

            // Additional info
            _sSortOrder = itemNode.Attributes["sortorder"].Value;

            if ((itemNode.Attributes["haschildren"] != null) && (itemNode.Attributes["haschildren"].Value == "1"))
                _bHasChildren = true;


            // This is a template item
            if (_sPath.ToLower().IndexOf("/sitecore/templates/") > -1) 
            {
                XmlNode childrenNode = _sitecoreApi.GetItemTree("{" + _ID.ToString().ToUpper() + "}", 2);
                foreach (XmlNode node in childrenNode.SelectNodes("./item"))
                {
                    // Get full-blown item
                    XmlNode templateFieldNode = _sitecoreApi.GetItemXml(node.Attributes["id"].Value, "", this.Options.Language);

                    Sitecore43Field field = new Sitecore43Field(templateFieldNode, _sTemplateName, _sitecoreApi);

                    var existingfields = from f in _fields
                                            where f.TemplateFieldID == field.TemplateFieldID
                                            select f;
                    if ((existingfields.Count() == 0) && (IsValidField(field)))
                    {
                        _fields.Add(field);
                    }
                }
            }
        }

        private bool IsValidField(Sitecore43Field field)
        {
            string sName = field.Name.ToLower();
            if ((sName == "__layout") || (sName == "__renderings"))
                return false;
            else if (field.TemplateFieldID == Util.GuidToSitecoreID(Guid.Empty))
                return false;
            else
                return true;
        }

        public string GetOuterXml()
        {
            return _sXmlNode;
        }

        public string GetHostUrl()
        {
            return _sitecoreApi.Url;
        }

        public void SetAPIConnection(Sitecore43.SitecoreClientAPI sitecoreApi)
        {
            _sitecoreApi = sitecoreApi;
        }

        public void CopyTo(IItem CopyFrom, bool bRecursive)
        {
            throw new Exception("Error CopyTo is not implemented.");
        }

        public bool MoveTo(IItem MoveTo)
        {
            XmlNode retVal = _sitecoreApi.MoveItemTo(this.ID, MoveTo.ID);
            if (retVal.SelectSingleNode("status").InnerText != "ok")
                return false;
            else
                return true;

        }

        public void Delete()
        {
            _sitecoreApi.DeleteItem(Util.GuidToSitecoreID(_ID));
        }

        public void Save()
        {
            throw new Exception("Error Save is not implemented.");
        }

        //  sTemplatePath = "/sitecore/templates/common/folder"
        public string AddFromTemplate(string sName, string sTemplatePath)
        {
            XmlNode resultNode = _sitecoreApi.AddItemFromTemplate(sName, this.ID, sTemplatePath);
            return resultNode.Attributes["id"].Value;
        }



        public bool HasChildren()
        {
            return _bHasChildren;
        }

        public ConverterOptions Options
        {
            get
            {
                return _Options;
            }
        }

        public string[] GetLanguages()
        {
            Sitecore43Item langRoot = GetSitecore43Item("/sitecore/system/Languages");
            IItem[] languages = langRoot.GetChildren();

            string[] result = new string[languages.Length];
            for (int t = 0; t < languages.Length; t++)
            {
                IField field = Util.GetFieldByName("Iso", languages[t].Fields);
                result[t] = field.Content;
            }
            
            return result;
        }

        private void AddItemToCache(Sitecore43Item item)
        {
            // Add to cache two times because we might request it by key or by path
            if (!_existingTemplates.ContainsKey(item.ID.ToLower()))
                _existingTemplates.Add(item.ID.ToLower(), item);
            if (!_existingTemplates.ContainsKey(item.Path.ToLower()))
                _existingTemplates.Add(item.Path.ToLower(), item);
        }

        private Sitecore43Item GetSitecore43Item(string sItemPath)
        {
            if (_existingTemplates.ContainsKey(sItemPath.ToLower()))
            {
                Sitecore43Item item = null;
                _existingTemplates.TryGetValue(sItemPath.ToLower(), out item);
                return item;
            }
            XmlNode itemXml = _sitecoreApi.GetItemXml(sItemPath, "", this.Options.Language);
            return new Sitecore43Item(itemXml, null, _sitecoreApi);
        }

        public IItem GetItem(string sItemPath)
        {
            return GetSitecore43Item(sItemPath) as IItem;
        }

        public static Sitecore43Item GetItem(string sItemPath, Sitecore43.SitecoreClientAPI sitecoreApi)
        {
//            XmlNode node = sitecoreApi.GetItem(sItemPath);
            XmlNode itemXml = sitecoreApi.GetItemXml(sItemPath, "", "");
            Sitecore43Item item = new Sitecore43Item(itemXml, null, sitecoreApi);
            item.SetAPIConnection(sitecoreApi);
            return item;
        }
    }
}
