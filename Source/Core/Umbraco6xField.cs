using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreConverter.Core
{
    class Umbraco6xField : IField
    {
        private string _sName = "";
        private string _sContent = "";

        public Umbraco6xField(string sName, string sContent)
        {
            _sName = sName;
            _sContent = sContent;
        }

        public string Name
        {
            get { return _sName; }
        }

        public string LanguageTitle
        {
            get { throw new NotImplementedException(); }
        }

        public string Key
        {
            get { return _sName.ToLower(); }
        }

        public string Source
        {
            get { throw new NotImplementedException(); }
        }

        public string Section
        {
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
        }

        public string SortOrder
        {
            get { return ""; }
        }

        public string TemplateFieldID
        {
            get { return _sName; }
        }
    }
}
