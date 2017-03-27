using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.Specialized;
using System.Web;


namespace SitecoreConverter.Core
{
    public class Sitecore5xItem : IItem
    {
        private Guid _ID = Guid.Empty;
        private string _sName = "";
        private string _sKey = "";
        private string _sPath = "";
        private string _sIcon = "";
        private Guid _TemplateID = Guid.Empty;
        private string _sTemplateName = "";
        private Sitecore5xItem[] _Templates = null;
        private string[] _sTemplateIDs = null;
        private List<Sitecore5xField> _fields = new List<Sitecore5xField>();
        private Sitecore5x.VisualSitecoreService  _sitecoreApi = null;
        private Sitecore5x.Credentials _credentials = null;
        private string _sXmlNode = "";
        private IItem _Parent = null;
        private string _sSortOrder = "";
        private bool _bHasChildren = false;
        private BaseRole[] _Roles = null;

        // Global variables
        private static Dictionary<string, Sitecore5xItem> _existingTemplates = new Dictionary<string, Sitecore5xItem>();
        private static StringDictionary _roleNames = new StringDictionary();
        private static ConverterOptions _Options = new ConverterOptions();
//        private static Sitecore5xItem _BaseTemplate = null;

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
/*
        public Guid TemplateID
        {
            get
            {
                return _TemplateID;
            }
        }
        */
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
                    _Templates = new Sitecore5xItem[_sTemplateIDs.Length];
                    for (int t = 0; t < _Templates.Length; t++ )
                    {
                        _Templates[t] = GetSitecore5xItem(_sTemplateIDs[t]);
                        // Cache templates
                        if (_Templates[t] != null)
                            AddItemToCache(_Templates[t]);
                    }                 
                }
                return _Templates;
            }
        }

        public IItem BaseTemplate
        {
            get
            {
/*
                if (_BaseTemplate == null)
                {
                    _BaseTemplate = GetSitecore5xItem("/sitecore/templates/System/Templates/Standard template");
                    _BaseTemplate.Name = "Sitecore5x Standard template";
                    _BaseTemplate.Key = _BaseTemplate.Name.ToLower();
                    AddItemToCache(_BaseTemplate);
                }
                return _BaseTemplate;
 */
                return null;
            }
        }

        public IField[] Fields 
        {
            get
            {
                return _fields.ToArray();
            }
        }

        private static ExtendedSitecoreAPI5x.Security _security = null;
        public IRole[] Roles
        {
            get
            {
                if (_Roles == null)
                {
                    if (_security == null)
                    {
                        _security = new SitecoreConverter.Core.ExtendedSitecoreAPI5x.Security();
                        Uri securityUri = new Uri(_sitecoreApi.Url);
                        _security.Url = securityUri.Scheme + "://" + securityUri.Host + "/sitecore%20modules/shell/ExtendedSitecoreAPI5x/security.asmx";
                        // Timeout = 30 seconds (default 10)
                        _security.Timeout = 300000;
                    }
                    ExtendedSitecoreAPI5x.RoleStruct[] rolesStruct = _security.GetItemSecurity(_ID.ToString());
                    _Roles = new BaseRole[rolesStruct.Length];
                    for (int t = 0; t < rolesStruct.Length; t++)
                    {
                        _Roles[t] = new BaseRole(rolesStruct[t].Name, rolesStruct[t].ID, rolesStruct[t].Path, rolesStruct[t].AccessRight);
                        // This is a user, only users have UserSettings
                        if (_Roles[t].Path.ToLower().IndexOf("/sitecore/users") > -1)
                        {
                            _Roles[t].UserSettings.Add("Email", rolesStruct[t].Email);
                            _Roles[t].UserSettings.Add("FullName", rolesStruct[t].FullName);
                            _Roles[t].UserSettings.Add("PassWord", rolesStruct[t].PassWord);
                            _Roles[t].UserSettings.Add("IsAdmin", rolesStruct[t].IsAdmin.ToString());
                            _Roles[t].UserSettings.Add("Roles", rolesStruct[t].Roles);
                        }
                    }
                }
                return _Roles;
            }
        }

        private static BaseRole[] _Users = null;
        public IRole[] Users
        {
            get
            {
                if (_Users == null)
                {
                    if (_security == null)
                    {
                        _security = new SitecoreConverter.Core.ExtendedSitecoreAPI5x.Security();
                        Uri securityUri = new Uri(_sitecoreApi.Url);
                        _security.Url = securityUri.Scheme + "://" + securityUri.Host + "/sitecore%20modules/shell/ExtendedSitecoreAPI5x/security.asmx";
                        // Timeout = 30 seconds (default 10)
                        _security.Timeout = 300000;
                    }
                    ExtendedSitecoreAPI5x.RoleStruct[] rolesStruct = _security.GetUsers("sitecore");
                    _Users = new BaseRole[rolesStruct.Length];
                    for (int t = 0; t < rolesStruct.Length; t++)
                    {
                        _Users[t] = new BaseRole(rolesStruct[t].Name, rolesStruct[t].ID, rolesStruct[t].Path, rolesStruct[t].AccessRight);
                        // This is a user, only users have UserSettings
 //                       if (_Users[t].Path.ToLower().IndexOf("/sitecore/users") > -1)
                        {
                            _Users[t].UserSettings.Add("Email", rolesStruct[t].Email);
                            _Users[t].UserSettings.Add("FullName", rolesStruct[t].FullName);
                            _Users[t].UserSettings.Add("PassWord", rolesStruct[t].PassWord);
                            _Users[t].UserSettings.Add("IsAdmin", rolesStruct[t].IsAdmin.ToString());
                            _Users[t].UserSettings.Add("Roles", rolesStruct[t].Roles);
                        }
                    }
                }
                return _Users;
            }
        }

        public IItem Parent
        {
            get
            {
                return _Parent;
            }
        }


        public IItem[] GetChildren()
        {
            if (_sitecoreApi == null)
                throw new Exception("sitecoreApi not set please call constructor with API object");

            XmlNode itemNode = _sitecoreApi.GetChildren("{" + _ID.ToString().ToUpper() + "}", Util.CurrentDatabase, _credentials);

            XmlNodeList nodeList = itemNode.SelectNodes("./item");
            List<Sitecore5xItem> children = new List<Sitecore5xItem>();
            foreach (XmlNode node in nodeList)
            {
                XmlNode item = null;
                try
                {
                    // Get full-blown item
                    item = GetItemXml(node.Attributes["id"].Value);
                }
                catch (Exception ex)
                {
                    Util.AddWarning("Error loading item: " + node.Attributes["id"].Value + " from parent with path: " + _sPath + ", message: " + ex.Message);
                }

                if (item != null)
                {
                    // there is children
                    XmlAttribute attr = item.OwnerDocument.CreateAttribute("haschildren");
                    if (node.Attributes["haschildren"].Value == "1")
                        attr.Value = "1";
                    else
                        attr.Value = "0";
                    item.Attributes.Append(attr);
                    children.Add(new Sitecore5xItem(item, this, _sitecoreApi, _credentials));
                }
                    
            }
            return children.ToArray();
        }

        private Sitecore5xItem(XmlNode itemNode, IItem parent, Sitecore5x.VisualSitecoreService sitecoreApi, Sitecore5x.Credentials credentials)
        {
            _Parent = parent;
            _sitecoreApi = sitecoreApi;
            _credentials = credentials;

            _ID = new Guid(itemNode.Attributes["id"].Value);
            _sName = itemNode.Attributes["name"].Value;
            _sKey = itemNode.Attributes["key"].Value;
            _TemplateID = new Guid(itemNode.Attributes["tid"].Value);
            _sTemplateName = itemNode.Attributes["template"].Value;
            // If basetemplate isn't defined use _TemplateID otherwise extract list of inherited templates
            XmlNode baseTemplateNode =  itemNode.SelectSingleNode("//field[@key='__base template']/content");
            if (baseTemplateNode == null)                
            {
                _sTemplateIDs = new string[1];
                // If template is "template" then there is an error in sitecore webservice and
                // we change it to "Standard template".
                if (Util.GuidToSitecoreID(_TemplateID) == "{AB86861A-6030-46C5-B394-E8F99E8B87DB}")
                    _sTemplateIDs[0] = "{1930BBEB-7805-471A-A3BE-4858AC7CF696}";
                else
                    _sTemplateIDs[0] = Util.GuidToSitecoreID(_TemplateID);                
            }
            else
                _sTemplateIDs = baseTemplateNode.InnerText.Split('|');

            // _sParentID = itemNode.Attributes["parentid"].Value;

            _sSortOrder = Util.GetNodeFieldValue(itemNode, "//field[@key='__sortorder']/content");
            if ((itemNode.Attributes["haschildren"] != null) && (itemNode.Attributes["haschildren"].Value == "1"))
                _bHasChildren = true;

            _sPath = "";
            string sLatestVersion = "1";
            XmlNodeList versions = itemNode.SelectNodes("//version[@language = '" + this.Options.Language + "']");
            if (versions != null && versions.Count > 0)
                sLatestVersion = versions[versions.Count - 1].SelectSingleNode("@version").Value;
            
            XmlNode contentNode = _sitecoreApi.GetItemFields(Util.GuidToSitecoreID(_ID), this.Options.Language, sLatestVersion, false, Util.CurrentDatabase, _credentials);
            XmlNodeList paths = contentNode.SelectNodes("/path/item");
            foreach (XmlNode path in paths)
            {
                if (_sPath == "")
                    _sPath = "/" + path.Attributes["name"].Value;
                else
                    _sPath = "/" + path.Attributes["name"].Value + _sPath;
            }


            // The program always assumes that all fields exists, so the fieldvalues could be empty in 
            // order to prevent copying to another language.
            XmlNodeList fieldList = itemNode.SelectNodes("version[@language='" + this.Options.Language + "' and @version = '"+sLatestVersion+"']/fields/field");
            foreach (XmlNode node in fieldList)
            {
                Sitecore5xField field = new Sitecore5xField(node);
                var existingfields = from f in _fields
                                     where f.TemplateFieldID == field.TemplateFieldID
                                     select f;
                if (existingfields.Count() == 0)
                {
                    // The content is cleared because it is from another language layer
                    //                    field.Content = "";

                    if (field.Name.ToLower() == "__icon")
                        _sIcon = field.Content;

                    if (field.Name.ToLower() == "__sortorder")
                        _sSortOrder = field.Content;

                    if ((field.Name.ToLower() == "__display name") && (field.Content != ""))
                        _sName = field.Content;

                    // Skip security field, it is copied in the extended sitecoreApi
                    if (field.Name.ToLower() == "__security")
                        continue;

                    // Skip standard values, it's set in the extended sitecoreApi
                    if (field.Name.ToLower() == "__standard values")
                        continue;


                    // Caching of template fields items 
                    XmlNode templateFieldNode = null;
                    if (!_Options.ExistingTemplateFields.TryGetValue(field.TemplateFieldID, out templateFieldNode))
                    {
                        try
                        {
                            templateFieldNode = GetItemXml(field.TemplateFieldID);
                            _Options.ExistingTemplateFields.Add(field.TemplateFieldID, templateFieldNode);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToLower().IndexOf("object reference not set to an instance of an object.") > -1)
                            {
                                // Do nothing this happens when there is content that is missing it's template field (probably the whole template)
                                continue;
                            }
                            else
                                throw new Exception("Error retrieving sitecore Field.", ex);
                        }
                    }
                    field.SortOrder = templateFieldNode.Attributes["sortorder"].Value;
                    field.Name = templateFieldNode.Attributes["name"].Value;

                    // Get section from contentNode
                    XmlNode fieldNode = contentNode.SelectSingleNode("/field[@fieldid='" + field.TemplateFieldID + "']");
                    if ((fieldNode != null) && (fieldNode.Attributes["section"] != null))
                        field.Section = fieldNode.Attributes["section"].Value;

                    _fields.Add(field);
                }
            }

            // This is a template item
            if ((_sPath.ToLower().IndexOf("/sitecore/templates") > -1) && (_sName != "__Standard Values"))
            {
                XmlNode childrenNode = _sitecoreApi.GetChildren("{" + _ID.ToString().ToUpper() + "}", Util.CurrentDatabase, _credentials);

                XmlNodeList nodeList = childrenNode.SelectNodes("./item");
                foreach (XmlNode node in nodeList)
                {
                    // Get full-blown item
                    XmlNode item = GetItemXml(node.Attributes["id"].Value);
                    string sSection = "";
                    if (item.Attributes["template"].Value == "template section")
                    {
                        sSection = item.Attributes["name"].Value;

                        XmlNode sectionChildNodes = _sitecoreApi.GetChildren(node.Attributes["id"].Value, Util.CurrentDatabase, _credentials);
                        foreach (XmlNode tempNode in sectionChildNodes.SelectNodes("./item"))
                        {
                            XmlNode templateFieldNode = GetItemXml(tempNode.Attributes["id"].Value);

                            Sitecore5xField field = new Sitecore5xField(
                                        templateFieldNode.Attributes["name"].Value,
                                        templateFieldNode.Attributes["name"].Value.ToLower(),
                                        Util.GetNodeFieldValue(templateFieldNode, "//field[@key='type']/content"),
                                        new Guid(templateFieldNode.Attributes["id"].Value),
                                        "",
                                        Util.GetNodeFieldValue(templateFieldNode, "//field[@key='__sortorder']/content"),
                                        sSection);
                            field.Source = Util.GetNodeFieldValue(templateFieldNode, "//field[@key='source']/content");
                            field.LanguageTitle = Util.GetNodeFieldValue(templateFieldNode, "//field[@key='title']/content");


                            var existingfields = from f in _fields
                                                 where f.TemplateFieldID == field.TemplateFieldID
                                                 select f;
                            if (existingfields.Count() == 0)
                            {
                                _fields.Add(field);
                            }
                        }
                    }
                    // Clear template items values, will be fille later with standard values
//                    foreach (Sitecore5xField field in _fields)
//                        field.Content = "";
                }

                // Fill template field values with standard values
                Sitecore5xItem standardValueItem = GetSitecore5xItem(_sPath + "/__Standard Values");
                if (standardValueItem != null)
                {
                    foreach (Sitecore5xField standardField in standardValueItem._fields)
                    {
                        // Add field values from this and inherited templates
                        Sitecore5xField curField = Util.GetFieldByID(standardField.TemplateFieldID, Fields) as Sitecore5xField;
                        if (curField == null)
                            _fields.Add(standardField);
                        else
                            curField.Content = standardField.Content;
                    }
                }
            }
            // This is a "__Standard Values" item for the current language layer
            else if ((_sPath.ToLower().IndexOf("/sitecore/templates") > -1) && (_sName == "__Standard Values"))
            {
            }

            _sXmlNode = itemNode.OuterXml;
        }

        public string GetOuterXml()
        {
            return _sXmlNode;
        }


        public string GetHostUrl()
        {
            return _sitecoreApi.Url;
        }


        public void SetAPIConnection(Sitecore5x.VisualSitecoreService sitecoreApi, Sitecore5x.Credentials credentials)
        {
            _sitecoreApi = sitecoreApi;
            _credentials = credentials;
        }


        public void CopyTo(IItem CopyFrom, bool bRecursive)
        {
            throw new Exception("Error CopyTo is not implemented.");
        }

        public bool MoveTo(IItem MoveTo)
        {           
            XmlNode retVal = _sitecoreApi.MoveTo(this.ID, MoveTo.ID, Util.CurrentDatabase, _credentials);
            if (retVal.SelectSingleNode("status").InnerText != "ok")
                return false;
            else
            {
                // The path is only changed to visually indicate the change, no need to save again
                this.Path = MoveTo.Path + "/" + this.Name;
                return true;
            }
        }


        public void Delete()
        {
            _sitecoreApi.Delete(Util.GuidToSitecoreID(_ID), true, Util.CurrentDatabase, _credentials);
        }

        public void Save()
        {
            AddFieldFromTemplate(this, "/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder", this.SortOrder);
            this.Save(this.Fields, this.Options.Language, "1");
        }

        //  sTemplatePath = "/sitecore/templates/common/folder"
        public string AddFromTemplate(string sName, string sTemplatePath)
        {
            Sitecore5xItem template = GetSitecore5xItem(sTemplatePath);
            XmlNode resultNode = _sitecoreApi.AddFromTemplate(this.ID, template.ID, sName, Util.CurrentDatabase, _credentials);
            CheckSitecoreReturnValue(resultNode);
            return resultNode.SelectSingleNode("//data/data").InnerText;
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
            XmlNode languagesNode = _sitecoreApi.GetLanguages(Util.CurrentDatabase, _credentials);            
            XmlNodeList languageList = languagesNode.SelectNodes("language");
            string[] result = new string[languageList.Count];           
            for (int t=0; t<languageList.Count; t++)
            {
                result[t] = languageList[t].Attributes["name"].Value;
            }
            return result;
        }

        private XmlNode CheckSitecoreReturnValue(XmlNode retVal)
        {
            if (retVal.SelectSingleNode("status").InnerText != "ok")
            {
                throw new Exception("Sitecore webservice returned " + retVal.SelectSingleNode("status").InnerText + ": " +
                                    retVal.SelectSingleNode("error").InnerText);
            }
            return retVal;
        }



        /// <summary>
        /// Change a templates inheritance from another template
        /// returns TemplateItem
        /// </summary>
        private Sitecore5xItem ChangeTemplateInheritance(Sitecore5xItem templateItem, string sTemplatePath, string sBaseTemplatePath)
        {
            // Get standard template
            XmlNode baseTemplate = GetItemXml(sBaseTemplatePath);
            string sBaseTemplateID = baseTemplate.Attributes["id"].Value;

            templateItem._fields.Add(new Sitecore5xField("__base template", "__base template", "tree list", new Guid("{12C33F3F-86C5-43A5-AEB4-5598CEC45116}"), sBaseTemplateID, null, ""));

            // Return changed template item
            return templateItem;
        }


        // Same as below but without ref toTemplateID
        private string CreateTemplateItemWithSpecificID(string sParentPath, string sFromTemplatePath, Guid toTemplateID, string sName)
        {
            return CreateTemplateItemWithSpecificID(sParentPath, sFromTemplatePath, ref toTemplateID, sName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sParentPath">Path of parent item</param>
        /// <param name="sFromTemplatePath">Path to template that the new item will be created from</param>
        /// <param name="toTemplateID">ID of new item</param>
        /// <param name="sName">Name of new item</param>
        /// <returns></returns>
        private string CreateTemplateItemWithSpecificID(string sParentPath, string sFromTemplatePath, ref Guid toTemplateID, string sName)
        {
            // Get standard template
            XmlNode standardTemplate = GetItemXml(sFromTemplatePath);
            string sStandardTemplateID = standardTemplate.Attributes["id"].Value;
            string sItemName = Util.MakeValidNodeName(sName);

            // Create new template from the standard template
            XmlNode resultNode = CheckSitecoreReturnValue(_sitecoreApi.AddFromTemplate(sParentPath, sStandardTemplateID, sItemName, Util.CurrentDatabase, _credentials));

            // Only set template id if one is given
            if (toTemplateID != Guid.Empty)
            {
                string sNewTemplateID = resultNode.SelectSingleNode("//data/data").InnerText;
                // Get newly created template, which will be used as a temporary template
                XmlNode newTemplateNode = GetItemXml(sNewTemplateID);
                // Modify it's ID 
                newTemplateNode.Attributes["id"].Value = Util.GuidToSitecoreID(toTemplateID);
                // Create new template with the source (CopyFrom template) id
                _sitecoreApi.InsertXML(sParentPath, newTemplateNode.OuterXml, false, Util.CurrentDatabase, _credentials);
                // Remove temporary template
                _sitecoreApi.Delete(sNewTemplateID, false, Util.CurrentDatabase, _credentials);
            }
            else
            {
                string sNewTemplateID = resultNode.SelectSingleNode("//data/data").InnerText;
                toTemplateID = new Guid(sNewTemplateID);
            }
            return sParentPath + "/" + sItemName;
        }
        

        /// <summary>
        /// Create template from standard template
        /// </summary>
        private string CreateTemplate(string sParentPath, string sTemplateName)
        {
            return CreateTemplateItemWithSpecificID(sParentPath, "/sitecore/templates/System/Templates/Template", Guid.Empty, sTemplateName);
        }


        /// <summary>
        /// Create Template from any given template
        /// </summary>
        private string CreateTemplate(string sParentPath, string sFromTemplatePath, string sTemplateName)
        {
            return CreateTemplateItemWithSpecificID(sParentPath, sFromTemplatePath, Guid.Empty, sTemplateName);
        }

        private XmlNode GetItemXml(string sPath)
        {
            XmlNode result = CheckSitecoreReturnValue(
                              _sitecoreApi.GetXML(sPath, false, Util.CurrentDatabase, _credentials));
            result = result.SelectSingleNode("//item");
            return result;
        }

        private string GetItemAttribute(string sPath, string sAttribute)
        {
            XmlNode itemNode = _sitecoreApi.GetXML(sPath, false, Util.CurrentDatabase, _credentials);
            itemNode = itemNode.SelectSingleNode("//item");
            if (itemNode == null)
                return null;
            else
                return itemNode.Attributes[sAttribute].Value;
        }


        /// <summary>
        /// Creates a new content or template field
        /// </summary>
        private XmlNode AddFieldToItem(XmlNode item, string sTfid, string sKey, string sType, string sContent)
        {
            XmlNode fields = item.SelectSingleNode("//item/version[@language = '" + this.Options.Language + "']/fields");
            XmlElement fieldElem = item.OwnerDocument.CreateElement("field");
            fieldElem.SetAttribute("tfid", sTfid);
            fieldElem.SetAttribute("key", sKey);
            fieldElem.SetAttribute("type", sType);
            fields.AppendChild(fieldElem);
            XmlElement contentElem = item.OwnerDocument.CreateElement("content");
            contentElem.InnerText = sContent;
            fieldElem.AppendChild(contentElem);
            return item;
        }

        /// <summary>
        /// Update existing item field
        /// </summary>
        private XmlNode ModifyItemField(XmlNode item, string sTfid, string sKey, string sType, string sContent)
        {
            XmlNode fieldNode = item.SelectSingleNode("//item/version[@language = '" + this.Options.Language + "']/fields/field[@tfid = '" + sTfid + "']/content");
            if (fieldNode != null)
            {
                fieldNode.InnerText = sContent;
            }
            else
            {
                item = AddFieldToItem(item, sTfid, sKey, sType, sContent);
            }
            return item;

        }

        private Sitecore5xItem AddFieldFromTemplate(Sitecore5xItem templateFieldItem, string sFieldTemplate, string sContent)
        {
            // Get sType field template
            XmlNode field = GetItemXml(sFieldTemplate);
            string sTemplateFieldID = field.Attributes["id"].Value;
            string sfieldTypename = field.SelectSingleNode("//item/version[@language = '" + this.Options.Language + "']/fields/field[@key='type']").InnerText;

            string sName = sFieldTemplate.Remove(0, sFieldTemplate.LastIndexOf("/") + 1);
            string sKey = sFieldTemplate.Remove(0, sFieldTemplate.LastIndexOf("/") + 1).ToLower();

            templateFieldItem._fields.Add(new Sitecore5xField(sName, sKey, sfieldTypename, new Guid(sTemplateFieldID), sContent, null, ""));

            return templateFieldItem;
        }


        /// <summary>
        /// Adds a new field to a template using the IField fromField object
        /// </summary>
        private void AddTemplateField(string sTemplatePath, IField fromField)
        {
            // Get "Template section" template
            XmlNode templateSection = GetItemXml("/sitecore/templates/System/Templates/Template section");
            string templateSectionID = templateSection.Attributes["id"].Value;

            string templateSectionName = "Data";
            if (fromField.Section != "")
                templateSectionName = fromField.Section;

            string dataSectionID = GetItemAttribute(sTemplatePath + "/" + templateSectionName, "id");
            if (dataSectionID == null)
            {
                // Create new "Template section" item from the "Template section" template
                XmlNode resultNode = _sitecoreApi.AddFromTemplate(sTemplatePath, templateSectionID, templateSectionName, Util.CurrentDatabase, _credentials);
                CheckSitecoreReturnValue(resultNode);
                dataSectionID = resultNode.SelectSingleNode("//data/data").InnerText;
            }

            // Create new field below Section
            CreateTemplateItemWithSpecificID(dataSectionID, "/sitecore/templates/System/Templates/Template field", new Guid(fromField.TemplateFieldID), fromField.Name);

            Sitecore5xItem templateFieldItem =  GetSitecore5xItem(fromField.TemplateFieldID);

            // Add "Sort order" to template
            AddFieldFromTemplate(templateFieldItem, "/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder", fromField.SortOrder);

            // Add "Field type" to template
            string sFromFieldTranslated = Util.TranslateToNewFieldTypes(fromField.Type);
            AddFieldFromTemplate(templateFieldItem, "/sitecore/templates/System/Templates/Template field/Data/Type", sFromFieldTranslated);

            // Add "Field source" to template
            AddFieldFromTemplate(templateFieldItem, "/sitecore/templates/System/Templates/Template field/Data/Source", fromField.Source);

            // Add "LanguageTitle" to template
            AddFieldFromTemplate(templateFieldItem, "/sitecore/templates/System/Templates/Template field/Data/Title", fromField.LanguageTitle);

            templateFieldItem.Save(templateFieldItem.Fields, this.Options.Language, "1");
        }


        /// <summary>
        /// Overwrite existing item values, item must already exist
        /// </summary>
        public void Save(IField[] fromFields, string sLanguage, string sVersion)
        {
            // Create special XmlDocument that _sitecoreApi.Save() expects, for some 
            // weird reason this is not the same as _sitecoreApi.InsertXml().
            XmlDocument doc = new XmlDocument();
            XmlElement sitecoreElem = doc.CreateElement("sitecore");
            doc.AppendChild(sitecoreElem);

            // Update existing fields
            foreach (IField field in fromFields)
            {
                // Do not save empty fields
                if (field.Content == "")
                    continue;

                XmlElement fieldElem = doc.CreateElement("field");
                fieldElem.SetAttribute("itemid", this.ID);
                fieldElem.SetAttribute("language", sLanguage);
                fieldElem.SetAttribute("version", sVersion);
                fieldElem.SetAttribute("fieldid", field.TemplateFieldID);
                sitecoreElem.AppendChild(fieldElem);

                XmlNode valueNode = doc.CreateElement("value");
                valueNode.InnerText = field.Content;
                fieldElem.AppendChild(valueNode);

            }
            // Save item 
            CheckSitecoreReturnValue(_sitecoreApi.Save(doc.OuterXml, Util.CurrentDatabase, _credentials));
        }

        /// <summary>
        /// Convert field content into sitecore 6.x format, mostly links
        /// </summary>
        private string ConvertFieldContent(IItem CopyFrom, IField fromField)
        {
            string sContent = fromField.Content;
            if ((fromField.Type == "Rich Text") || (fromField.Type == "html") ||
                (fromField.Type == "MultiLink") || (fromField.Type == "General Link") ||
                (fromField.Type == "link"))
            {
                try
                {
                    XmlDocument doc = Sgml.SgmlUtil.ParseHtml(sContent);

                    bool bModified = false;
                    // Convert links to new format
                    XmlNodeList linkNodes = doc.SelectNodes("//a");
                    foreach (XmlNode node in linkNodes)
                    {
                        int t=0;
                        if (fromField.Type == "link")
                            t++;
                        string sUrl = Util.GetAttributeValue(node.Attributes["href"]);
                        string sTitle = Util.GetAttributeValue(node.Attributes["sc_text"]);
                        string sc_linktype = Util.GetAttributeValue(node.Attributes["sc_linktype"]);
                        string sc_url = Util.GetAttributeValue(node.Attributes["sc_url"]);
                        string sTarget = Util.GetAttributeValue(node.Attributes["target"]);
                        // Possible linktypes: internal, external, mailto, anchor, javascript, media
                        if (sc_linktype == "internal")
                        {
                            IItem linkItem = CopyFrom.GetItem(sc_url);
                            if (linkItem != null)
                            {
                                sUrl = "~/link.aspx?_id=" + new Guid(linkItem.ID).ToString("N") + "&_z=z";
                            }

                        }
                        node.Attributes["href"].Value = sUrl;
                        node.Attributes.Remove(node.Attributes["sc_text"]);
                        node.Attributes.Remove(node.Attributes["sc_linktype"]);
                        node.Attributes.Remove(node.Attributes["sc_url"]);
                        if (node.Attributes["sc_anchor"] != null)
                            node.Attributes.Remove(node.Attributes["sc_anchor"]);
                        bModified = true;
                    }

                    if (bModified)
                    {
                        XmlNode root = doc.SelectSingleNode("root");
                        sContent = root.InnerXml;
                        // No tags at all, add a paraggraph tag
                        if (root.ChildNodes.Count == 1)
                            sContent = "<p>" + root.InnerXml + "</p>";
                    }
                }
                catch (Exception ex)
                {
                    Util.AddWarning("Error converting content: " + CopyFrom.Path + ", message: " + ex.Message);
                }
            }
            return HttpUtility.HtmlDecode(sContent);
        }


        private void AddItemToCache(Sitecore5xItem item)
        {
            // Add to cache two times because we might request it by key or by path
            if (!_existingTemplates.ContainsKey(item.ID.ToLower()))
                _existingTemplates.Add(item.ID.ToLower(), item);
            if (!_existingTemplates.ContainsKey(item.Path.ToLower()))
                _existingTemplates.Add(item.Path.ToLower(), item);
        }

        private Sitecore5xItem GetSitecore5xItem(string sItemPath)
        {
            return GetSitecore5xItem(sItemPath, null);
        }

        private Sitecore5xItem GetSitecore5xItem(string sItemPath, Sitecore5xItem  parentItem)
        {
            if (_existingTemplates.ContainsKey(sItemPath.ToLower()))
            {
                Sitecore5xItem item = null;
                _existingTemplates.TryGetValue(sItemPath.ToLower(), out item);
                return item;
            }

            XmlNode node = _sitecoreApi.GetXML(sItemPath, false, Util.CurrentDatabase, _credentials);
            XmlNode statusNode = node.SelectSingleNode("status");
            if (statusNode.InnerText == "failed")
                return null;

            return new Sitecore5xItem(node.SelectSingleNode("//item"), parentItem, _sitecoreApi, _credentials);
        }

        public IItem GetItem(string sItemPath)
        {
            return GetSitecore5xItem(sItemPath);
        }


        public static Sitecore5xItem GetItem(string sItemPath, Sitecore5x.VisualSitecoreService sitecoreApi, Sitecore5x.Credentials credentials)
        {
            XmlNode node = sitecoreApi.GetXML(sItemPath, false, Util.CurrentDatabase, credentials);
            Sitecore5xItem item = new Sitecore5xItem(node.SelectSingleNode("//item"), null, sitecoreApi, credentials);
            return item;
        }
    }
}
