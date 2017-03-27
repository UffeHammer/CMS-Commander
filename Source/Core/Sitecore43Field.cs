using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SitecoreConverter.Core
{
    class Sitecore43Field : IField
    {
        private string _sName = "";
        private string _sLanguageTitle = "";
        private string _sKey = "";
        private string _sSource = null;
        private string _sSection = null;
        private string _sContent = "";
        private string _sType = "";
        private string _sSortOrder = "";
        private Guid _TemplateFieldID = Guid.Empty;
        private string _sTemplateName = "";
        private Sitecore43.SitecoreClientAPI _sitecoreApi = null;

        public string Name 
        {
            get
            {
                return _sName;
            }
        }

        public string LanguageTitle
        {
            get
            {
                return _sLanguageTitle;
            }
        }
        
        public string Key
        {
            get
            {
                return _sKey;
            }
        }

        public string Source
        {
            get
            {
                if (_sSource == null)
                    GetExtraNodeInfo();
                return _sSource;
            }
        }

        public string Section
        {
            get
            {
                if (_sSection == null)
                    GetExtraNodeInfo();
                return _sSection;
            }
        }        

        public string Content
        {
            get
            {
                return _sContent;
            }
            set
            {
                _sContent = value;
            }
        }
        public string Type
        {
            get
            {
                if (_sType == "")
                    GetExtraNodeInfo();
                return _sType;
            }
        }

        public string SortOrder
        {
            get
            {
                return _sSortOrder;
            }
        }        

        public string TemplateFieldID
        {
            get
            {
//                if (_TemplateFieldID == Guid.Empty)
//                    GetExtraNodeInfo();
                return Util.GuidToSitecoreID(_TemplateFieldID);
            }
        }

        public string TemplateName
        {
            set
            {
                _sTemplateName = value;
            }
        }

        private void GetExtraNodeInfo()
        {
            // Get template node field 
            XmlNode templateNodeField = null;
            try
            {
                templateNodeField = _sitecoreApi.GetItem("/sitecore/templates/" + _sTemplateName + "/" + _sName);
                _TemplateFieldID = new Guid(templateNodeField.Attributes["id"].Value);
            }
            catch { }

            if (_TemplateFieldID == Guid.Empty)
            {
                try
                {
                    templateNodeField = _sitecoreApi.GetItem("/sitecore/templates/__Standard template/" + _sName);
                    _TemplateFieldID = new Guid(templateNodeField.Attributes["id"].Value);
                }
                catch { }
            }
            
            _sSource = templateNodeField.SelectSingleNode("field[@key='source']").InnerText;
            _sSection = templateNodeField.SelectSingleNode("field[@key='section']").InnerText;
            _sType = templateNodeField.SelectSingleNode("field[@key='type']").InnerText;

            // Remap id's if neeeded
            RemapTemplateIDs();
        }

        private static Guid __Publish43 = new Guid("{C8F93AFE-BFD4-4E8F-9C61-152559854661}");
        private static Guid __Unpublish43 = new Guid("{4C346442-E859-4EFD-89B2-44AEDF467D21}");
        private static Guid __NeverPublish43 = new Guid("{B8F42732-9CB8-478D-AE95-07E25345FB0F}");

        private static Guid __Publish5x = new Guid("{86FE4F77-4D9A-4EC3-9ED9-263D03BD1965}");
        private static Guid __Unpublish5x = new Guid("{7EAD6FD6-6CF1-4ACA-AC6B-B200E7BAFE88}");
        private static Guid __NeverPublish5x = new Guid("{9135200A-5626-4DD8-AB9D-D665B8C11748}");


        /// <summary>
        /// Function to remap __Publish, __Unpublish and __Never publish
        /// Unfortunately they do not have same functionality in 43 as they have in newer versions
        /// </summary>
        private void RemapTemplateIDs()
        {
            if (_TemplateFieldID == __Publish43)
                _TemplateFieldID = __Publish5x;
            else if (_TemplateFieldID == __Unpublish43)
                _TemplateFieldID = __Unpublish5x;
            else if (_TemplateFieldID == __NeverPublish43)
                _TemplateFieldID = __NeverPublish5x;
        }


        public Sitecore43Field(XmlNode fieldNode, string sTemplateName, Sitecore43.SitecoreClientAPI sitecoreApi)
        {
            _sitecoreApi = sitecoreApi;
            _sName = fieldNode.Attributes["name"].Value;
            _sKey = fieldNode.Attributes["key"].Value;
            _sSortOrder = fieldNode.Attributes["sortorder"].Value;

            // This is a template field
//            if ((fieldNode.Attributes["master"] != null) && 
//                (fieldNode.Attributes["master"].Value == "__Template field"))
            if (fieldNode.Attributes["title"] == null)
            {
                if ((fieldNode.Attributes["master"] != null) && 
                    (fieldNode.Attributes["master"].Value == "__Template"))
                    throw new Exception("Template fields cannot be handled here");

                _sLanguageTitle = Util.GetNodeFieldValue(fieldNode, "//field[@key='title']/content");
                _sSection = Util.GetNodeFieldValue(fieldNode, "//field[@key='section']/content");
                _sSource = Util.GetNodeFieldValue(fieldNode, "//field[@key='source']/content");
                _sType = Util.GetNodeFieldValue(fieldNode, "//field[@key='type']/content");
                _TemplateFieldID = new Guid(fieldNode.Attributes["id"].Value);
                // _sNoLanguage = Util.GetNodeFieldValue(fieldNode, "//field[@key='nolang']/content"); 
                // Actual values reside in masters
                _sContent = "";
            }
            else if ((fieldNode.Attributes["master"] != null) && 
                     (fieldNode.Attributes["master"].Value == "__Template"))
            {
                throw new Exception("Template fields cannot be handled here");
            }
            // Normal field
            else
            {
                _sLanguageTitle = fieldNode.Attributes["title"].Value;
                _sSection = fieldNode.Attributes["section"].Value;
                _sSource = fieldNode.Attributes["source"].Value;
                _sType = fieldNode.Attributes["type"].Value;
                if (fieldNode.Attributes["tfid"].Value != "")
                    _TemplateFieldID = new Guid(fieldNode.Attributes["tfid"].Value);

                if (fieldNode.InnerText != "")
                    _sContent = fieldNode.InnerText;
                else if ((fieldNode.SelectSingleNode("content") != null) &&
                    (fieldNode.SelectSingleNode("content").InnerXml != ""))
                    _sContent = fieldNode.SelectSingleNode("content").InnerXml;
            }


            _sTemplateName = sTemplateName;

            // Remap id's if neeeded
            RemapTemplateIDs();
        }

    }
}
