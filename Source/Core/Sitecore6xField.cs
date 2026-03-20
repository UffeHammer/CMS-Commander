using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SitecoreConverter.Core
{
    public class Sitecore6xField : IField
    {
        private string _sName = "";
        private string _sLanguageTitle = "";
        private string _sKey = "";
        private string _sSource = "";
        private string _sSection = null;
        private string _sContent = "";
        private string _sType = "";
        private string _sSortOrder = "";
        private Guid _TemplateFieldID = Guid.Empty;


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

        public string LanguageTitle
        {
            get
            {
                return _sLanguageTitle;
            }
            set
            {
                _sLanguageTitle = value;
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
                return _sSource;
            }
            set
            {
                _sSource = value;
            }
        }

        public string Section
        {
            get
            {
                return _sSection;
            }
            set
            {
                _sSection = value;
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
                return _sType;
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

        public string TemplateFieldID
        {
            get
            {
                return Util.GuidToSitecoreID(_TemplateFieldID);
            }
        }


        public Sitecore6xField(XmlNode fieldNode)
        {
            // XmlNode from GetXml function
            if (fieldNode.Attributes["tfid"] != null)
            {
                if (fieldNode.Attributes["name"] != null)
                    _sName = fieldNode.Attributes["name"].Value;
                else
                    _sName = fieldNode.Attributes["key"].Value;
                _sKey = fieldNode.Attributes["key"].Value;
                string typeValue = fieldNode.Attributes["type"].Value;
                _sType = string.IsNullOrEmpty(typeValue) ? "Single-Line Text" : typeValue;
                _TemplateFieldID = new Guid(fieldNode.Attributes["tfid"].Value);
                if (fieldNode.Attributes["sortorder"] != null)
                    _sSortOrder = fieldNode.Attributes["sortorder"].Value;
                _sContent = fieldNode.InnerText;
            }
            // XmlNode from GetItemFields function
            else
            {
                _sName = fieldNode.Attributes["name"].Value;
                _sKey = _sName.ToLower();
                string typeValue2 = fieldNode.Attributes["type"].Value;
                _sType = string.IsNullOrEmpty(typeValue2) ? "Single-Line Text" : typeValue2;
                _TemplateFieldID = new Guid(fieldNode.Attributes["fieldid"].Value);
                _sContent = fieldNode.SelectSingleNode("value").InnerText;
            }
        }

        public Sitecore6xField(string sName, string sKey, string sType, Guid TemplateFieldID, string sContent, string sSortOrder, string sSection)
        {
            _sName = sName;
            _sKey = sKey;
            _sType = string.IsNullOrEmpty(sType) ? "Single-Line Text" : sType;
            _TemplateFieldID = TemplateFieldID;
            _sContent = sContent;
            if (sSortOrder != null)
                _sSortOrder = sSortOrder;
            _sSection = sSection;
        }
    }
}
