using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SitecoreConverter.Core.Umbraco6DocumentService;
using System.Xml;
using System.Net;

namespace SitecoreConverter.Core
{
    public class Umbraco6xAPI
    {
        public Umbraco6DocumentService.documentService _umbracoDocumentApi = null;
        public Umbraco6xWebService.webService _umbracoWebService = null;
        public Credentials _credentials = null;

        public Umbraco6xAPI(string sUrl, Credentials credentials)
        {
            _umbracoDocumentApi = new Umbraco6DocumentService.documentService();
            _umbracoDocumentApi.Url = sUrl + "/umbraco/webServices/api/DocumentService.asmx";
//            _umbracoDocumentApi.PreAuthenticate = true;

            _umbracoWebService = new Umbraco6xWebService.webService();
            _umbracoWebService.Url = sUrl + "/umbraco/webService.asmx";


            _credentials = credentials;
        }
    }

    public class Umbraco6xItem : IItem
    {
        private Umbraco6xAPI _umbracoAPI = null;
        private ConverterOptions _Options = null; 
        private string _sName = "";
        private int _iID = -1;
        private documentCarrier _docCarrier = null;
        private Umbraco6xField[] _Fields = null;

        #region IItem Members

        public static Umbraco6xItem GetRoot(Umbraco6xAPI umbracoAPI, ConverterOptions Options)
        {
            return new Umbraco6xItem(null, umbracoAPI, Options);
        }

        private Umbraco6xItem(string sID, Umbraco6xAPI umbracoAPI, ConverterOptions Options)
        {
            _umbracoAPI = umbracoAPI;
            _Options = Options;

//            XmlNode node = umbracoAPI._umbracoWebService.GetDocumentValidate(0, umbracoAPI._credentials.UserName, umbracoAPI._credentials.Password);
//            node = umbracoAPI._umbracoWebService.GetDocument(0, "");
            int iID = 0;
            if (! string.IsNullOrEmpty(sID))
                iID = Util.StringGuid2Int(sID);

            // Special handling of root item
            if (iID == 0)
            {
                documentCarrier[] docCarrierList = _umbracoAPI._umbracoDocumentApi.readList(iID, umbracoAPI._credentials.UserName, umbracoAPI._credentials.Password);
                iID = docCarrierList[0].Id;
            }
            
            _docCarrier = _umbracoAPI._umbracoDocumentApi.read(iID, umbracoAPI._credentials.UserName, umbracoAPI._credentials.Password);
            _Fields = new Umbraco6xField[_docCarrier.DocumentProperties.Length];
            int t = 0;
            foreach (documentProperty docprop in _docCarrier.DocumentProperties)
            {
                string sDocProp = docprop.PropertyValue as string;
                if (sDocProp != null)
                {
                    sDocProp.Split(',');
                }
                else
                {
                    int iDocProp = (docprop.PropertyValue as Nullable<int>).GetValueOrDefault(-1);
                    // _umbracoAPI._umbracoWebService.Credentials = new NetworkCredential(umbracoAPI._credentials.UserName, umbracoAPI._credentials.Password);
                    // XmlNode node = _umbracoAPI._umbracoWebService.GetDocument(iDocProp, "");
//                    documentCarrier fieldCarrier = _umbracoAPI._umbracoDocumentApi.read(iDocProp, umbracoAPI._credentials.UserName, umbracoAPI._credentials.Password);
                }

                _Fields[t++] = new Umbraco6xField(docprop.Key, docprop.PropertyValue.ToString());
            }

            _iID = _docCarrier.Id;
            _sName = _docCarrier.Name;
        }

        public string ID
        {
            get 
            {
                return Util.Int2Guid(_iID).ToString(); 
            }
        }

        public string Name
        {
            get { return _sName; }
        }

        public string Key
        {
            get { return _sName.ToLower(); }
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
        }

        public string Icon
        {
            get
            {
                return "";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string SortOrder
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IItem[] Templates
        {
            get { throw new NotImplementedException(); }
        }

        public IItem BaseTemplate
        {
            get { throw new NotImplementedException(); }
        }

        public IField[] Fields
        {
            get { throw new NotImplementedException(); }
        }

        public IRole[] Roles
        {
            get { throw new NotImplementedException(); }
        }

        public IRole[] Users
        {
            get { throw new NotImplementedException(); }
        }

        public IItem Parent
        {
            get { throw new NotImplementedException(); }
        }

        public IItem[] GetChildren()
        {
            documentCarrier[] docCarrierList = _umbracoAPI._umbracoDocumentApi.readList(_iID, _umbracoAPI._credentials.UserName, _umbracoAPI._credentials.Password);
            Umbraco6xItem[] items = new Umbraco6xItem[docCarrierList.Length];

            int t = 0;
            foreach (documentCarrier doc in docCarrierList)
            {
                items[t++] = new Umbraco6xItem(Util.Int2Guid(doc.Id).ToString(), _umbracoAPI, _Options);
            }
            return items;
        }

        public IItem GetItem(string sItemPath)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IItem CopyFrom, bool bRecursive, bool bOnlyChildren)
        {
            throw new NotImplementedException();
        }

        public bool MoveTo(IItem MoveTo)
        {
            throw new NotImplementedException();
        }

        public void Rename(string Name)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public string AddFromTemplate(string sName, string sTemplatePath)
        {
            throw new NotImplementedException();
        }

        public bool HasChildren()
        {
            return _docCarrier.HasChildren;
        }

        public string[] GetLanguages()
        {
            throw new NotImplementedException();
        }

        public ConverterOptions Options
        {
            get
            {
                return _Options;
            }
            set
            {
                _Options = value;
            }
        }

        public string GetOuterXml()
        {
            throw new NotImplementedException();
        }

        public string GetHostUrl()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
