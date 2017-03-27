using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core
{
    // Base class which contain Field definitions
    public class BaseField : IField
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
                return _sSource;
            }
        }

        public string Section
        {
            get
            {
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

        public BaseField(string sName, Guid TemplateFieldID, string sType)
        {
            _sName = sName;
            _TemplateFieldID = TemplateFieldID;
            _sType = sType;
        }
    }
}
