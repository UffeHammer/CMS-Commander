using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Web;

namespace SitecoreConverter.Core
{
    public static class Util
    {
        public static BackgroundWorker _backgroundWorker = new BackgroundWorker();
        public static BackgroundWorker backgroundWorker
        {
            get
            {
                return _backgroundWorker;
            }
        }

        public static BackgroundWorker _backgroundWorkerLeftView = new BackgroundWorker();
        public static BackgroundWorker backgroundWorkerLeftView
        {
            get
            {
                return _backgroundWorkerLeftView;
            }
        }

        public static BackgroundWorker _backgroundWorkerRightView = new BackgroundWorker();
        public static BackgroundWorker backgroundWorkerRightView
        {
            get
            {
                return _backgroundWorkerRightView;
            }
        }


        private static string _sCopyingFrom = "";
        public static string CopyingFrom
        {
            get
            {
                return _sCopyingFrom;
            }
         }

        private static string _sCopyingTo = "";
        public static string CopyingTo
        {
            get
            {
                return _sCopyingTo;
            }
        }


        private static string _CurrentDatabase = "master";
        public static string CurrentDatabase
        {
            get
            {
                return _CurrentDatabase;
            }
            set
            {
                _CurrentDatabase = value;
            }
        }

        public static string GuidToSitecoreID(Guid srcGuid)
        {
            return "{" + srcGuid.ToString().ToUpper() + "}";
        }

        public static DateTime XsdDatetimeToDateTime(string xsdDateTime)
        {
          if(xsdDateTime != null && xsdDateTime.Length > 0)
          {
            try
            {
                xsdDateTime = xsdDateTime.Substring(0, xsdDateTime.IndexOf("T"));
                xsdDateTime = xsdDateTime.Insert(4, " ").Insert(7, " ");
                return DateTime.Parse(xsdDateTime);
            }
            catch{}
          }
          return DateTime.MinValue;
        }


        private static string _sItemNameValidation = null;

        // Function to Check for invalid characters.
        public static bool IsInvalid(string strToCheck)
        {
            if (_sItemNameValidation == null)
            {
                // \p{S} or \p{Symbol}: math symbols, currency signs, dingbats, box-drawing characters, etc..
                // \p{P} or \p{Punctuation}: any kind of punctuation character.
                _sItemNameValidation = "(\\p{S}|\\p{P})";
                if (ConfigurationManager.AppSettings["ItemNameValidation"] != null)
                    _sItemNameValidation = ConfigurationManager.AppSettings["ItemNameValidation"];
            }

            Regex regexPattern = new Regex(_sItemNameValidation);
            return !regexPattern.IsMatch(strToCheck);
        }

        public static string MakeValidNodeName(string name)
        {
            if (name != null)
            {
                // The setting InvalidItemNameChars only contains these characters: "\/:?&quot;&lt;&gt;|[]"
                // But there is a lot of other characters that aren't accepted, they are compiled here.                
                char[] replaceChars = new char[] { '%', '#', '/', '<', '>', '[', ']', '\'', '\"', '”', '?', /*'.',*/ '&', ':', /*'*',*/ '|', '–', /*'-',*/ ',', '!', /*'(', ')',*/ '\'', '´', '+', '@', '\\', '…', '§', '¾', ';', '’', '=', '`', '½' };

                foreach (char c in replaceChars)
                {
                    name = name.Replace(c, '_');
                }
                for (int t=0; t<name.Length; t++)
                {
                    // the name is only invalid if it starts with the dash - character, otherwise we don't have to remove it.
                    if (IsInvalid(name[t].ToString()) && ((t == 0) || (name[t] != '-')))
                        name = name.Replace(name[t], '_');
                }

                name = name.Trim();

                // Fix "Item name cannot end with period char
                if (name.EndsWith("."))
                    name = name.Remove(name.LastIndexOf("."));
            }
            return name;
        }

        public static string MakeValidRoleName(string name)
        {
            name = name.Replace("ø", "oe");
            name = name.Replace("Ø", "OE");
            name = name.Replace("æ", "ae");
            name = name.Replace("Æ", "AE");
            name = name.Replace("å", "aa");
            name = name.Replace("Å", "AA");
            return name;
        }

        public static bool IsTemplateOnIgnoreList(IItem[] Templates)
        {
            foreach (IItem Template in Templates)
            {
                string name = Template.Name.ToLower();
                if (name.IndexOf("fff.virtualpage") > -1)
                    return true;                
                if (name.IndexOf("fff.electunionrepresesentative") > -1)
                    return true;
                if (name.IndexOf("fff.paymentservice") > -1)
                    return true;
                if (name.IndexOf("fff.3fcontact") > -1)
                    return true;
                if (name.IndexOf("fff.industrysectorclubwebsite") > -1)
                    return true;                                
            }
            return false;
        }

        /// <summary>
        /// Returns true if templatefield is a standard template field which shouldn't be copied
        /// </summary>
        public static bool IsTemplateFieldOnIgnoreList(string sID)
        {
            // __Copy parent rights
            if (sID.IndexOf("{507ED8C6-6C2B-42B2-9461-DF4C79D919E5}") > -1)
                return true;
/*
            // __View
            else if (sID.IndexOf("{2176F66D-E7D1-45D6-B853-7381BD9535D7}") > -1)
                return true;
            // __Layout
            else if (sID.IndexOf("{E1D68787-D22B-4EA2-82B3-84C282E375EB}") > -1)
                return true;
*/
            // Toolbar buttons
            else if (sID.IndexOf("{5FA050B4-B1EC-469B-B098-83575D3B50C5}") > -1)
                return true;


            return false;
        }

        public static IField GetFieldByName(string sName, IField[] fields)
        {
            foreach (IField field in fields)
            {
                if (field.Name.ToLower() == sName.ToLower())
                {
                    return field;
                }
            }
            return null;
        }

        public static IField GetFieldByID(string ID, IField[] fields)
        {
            foreach (IField field in fields)
            {
                if (field.TemplateFieldID.ToLower() == ID.ToLower())
                {
                    return field;
                }
            }
            return null;
        }

        public static string GetItemPath(IItem item)
        {
            string sPath = "";
            while (item != null)
            {
                if (sPath == "")
                    sPath = item.Name;
                else
                    sPath = item.Name + "/" + sPath;
                item = item.Parent;
            }
            return "/" + sPath;
        }

        public static void SetStatus(string sCopyingFrom, string sCopyingTo)
        {
            _sCopyingFrom = sCopyingFrom;
            _sCopyingTo = sCopyingTo;
        }

        public static List<string> WarningList = new List<string>();
        public static void AddWarning(string sWarning)
        {
            WarningList.Add(sWarning);
        }


        public static string TranslateToNewFieldTypes(string sFieldType)
        {
            NameValueCollection deprecatedCollection = new NameValueCollection();
            deprecatedCollection.Add("text", "Single-Line Text");
            deprecatedCollection.Add("memo", "Multi-Line Text");
            deprecatedCollection.Add("html", "Rich Text");
            deprecatedCollection.Add("link", "General Link");
            // lookup cannot be converted, since it uses the name instead of id in sitecore 6x
//            deprecatedCollection.Add("lookup", "Droplist");
            deprecatedCollection.Add("valuelookup", "Name Value List");
//            deprecatedCollection.Add("tree", "");
//            deprecatedCollection.Add("tree list", "");
//            deprecatedCollection.Add("reference", "");
//            deprecatedCollection.Add("server file", "");

            deprecatedCollection.Add("datetime", "DateTime");
            deprecatedCollection.Add("date", "Date");
            deprecatedCollection.Add("checkbox", "Checkbox");
            deprecatedCollection.Add("integer", "Integer");
            deprecatedCollection.Add("multilink", "MultiLink");
            deprecatedCollection.Add("multifile", "GalleryMultiFile");
            deprecatedCollection.Add("multiimage", "GalleryVisuallist");
            deprecatedCollection.Add("imagefile", "GalleryImage");

            if (deprecatedCollection[sFieldType] != null)
                return deprecatedCollection[sFieldType];
            else
                return sFieldType;
        }


        public static Bitmap LoadPicture(string url)
        {
            HttpWebRequest wreq;
            HttpWebResponse wresp;
            Stream mystream;
            Bitmap bmp;

            bmp = null;
            mystream = null;
            wresp = null;
            try
            {
                wreq = (HttpWebRequest)WebRequest.Create(url);
                wreq.AllowWriteStreamBuffering = true;
                wreq.UseDefaultCredentials = true;
                wreq.Credentials = CredentialCache.DefaultCredentials;

                wresp = (HttpWebResponse)wreq.GetResponse();

                if ((mystream = wresp.GetResponseStream()) != null)
                    bmp = new Bitmap(mystream);
            }
            finally
            {
                if (mystream != null)
                    mystream.Close();

                if (wresp != null)
                    wresp.Close();
            }

            return (bmp);
        }


        public static string GetAttributeValue(XmlAttribute attrib)
        {
            if (attrib != null)
                return attrib.Value;
            else
                return "";
        }


        public static string GetNodeFieldValue(XmlNode node, string sXPath)
        {
            XmlNode selectedNode = node.SelectSingleNode(sXPath);
            if (selectedNode != null)
                return selectedNode.InnerText;
            else
                return "";
        }

        public static bool ExceptionContainsMessage(Exception ex, string sMsg)
        {
            if (ex.Message.Contains(sMsg))
                return true;
            if (ex.InnerException != null)
                ExceptionContainsMessage(ex.InnerException, sMsg);
            
            return false;
        }

        // subtract existing rights from the ones we want to set
        public static string SubtractRights(string sExistingRights, string sNewRights)
        {
             
            StringCollection existingRights = new StringCollection();
            existingRights.AddRange(sExistingRights.Split('|'));
            StringCollection newRights = new StringCollection();
            newRights.AddRange(sNewRights.Split('|'));
            foreach (string sExistingRight in existingRights)
            {
                if (newRights.Contains(sExistingRight))
                    newRights.Remove(sExistingRight);
            }

            string sResult = "";
            foreach (string sVal in newRights)
                sResult += sVal + "|";
            return sResult;
        }


        public static IItem FindItem(this IItem[] items, string ID)
        {
            foreach (IItem item in items)
            {
                if (item.ID.ToLower() == ID.ToLower())
                    return item;
            }
            return null;
        }

        public static Guid Int2Guid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static int Guid2Int(Guid value)
        {
            byte[] b = value.ToByteArray();
            int bint = BitConverter.ToInt32(b, 0);
            return bint;
        }

        public static int StringGuid2Int(string sValue)
        {
            Guid value = new Guid(sValue);
            return Guid2Int(value);
        }



        public static HttpWebRequest CreateWebRequest(string sUrl, Int32 contentLength, string sMethod)
        {
            var request = (HttpWebRequest)System.Net.WebRequest.Create(sUrl);

            request.Method = sMethod;
            request.ContentLength = contentLength;
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public static string CreateWebRequest(string sUrl)
        {
            HttpWebRequest request = CreateWebRequest(sUrl, 0, "GET");
            request.KeepAlive = true;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("GET failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }
                else
                {
                    StreamReader oStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    return oStream.ReadToEnd().ToString();
                }
            }
        }

        public static HttpWebResponse GetResponseNoException(this HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }

        public static Stream CreateStreamWebRequest(string sUrl)
        {
            HttpWebRequest webRequest = CreateWebRequest(sUrl, 0, "GET");
            webRequest.KeepAlive = true;
            webRequest.AllowAutoRedirect = false;  // IMPORTANT
//            webRequest.Timeout = (1000 * 60) * 1;           // timeout 1 minute
            webRequest.Accept = "text/html, application/xhtml+xml, */*";
            webRequest.UserAgent = "Sitecore commander";
//            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            // Get the response, but check for redirect.
            HttpWebResponse webResponse = GetResponseNoException(webRequest);
            // Now look to see if it's a redirect
            if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
            {
                string uriString = webResponse.Headers["Location"];
                if (sUrl != uriString)
                    sUrl = uriString;
                webResponse.Close();
            }
            else
            {
                return webResponse.GetResponseStream();
            }

            HttpWebRequest request = CreateWebRequest(sUrl, 0, "GET");
            request.KeepAlive = true;
/*
            request.AllowAutoRedirect = false;  // IMPORTANT
            request.Timeout = (1000 * 60) * 1;           // timeout 1 minute
            request.Accept = "text/html, application/xhtml+xml, * /*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
*/
            // Get the response ...
            HttpWebResponse oResp = (HttpWebResponse)request.GetResponse();
            return oResp.GetResponseStream();
        }

        // readStream is the stream you need to read, is closed afterwards
        // writeStream is the stream you want to write to, still open afterwards
        public static void CopyStream(Stream readStream, Stream writeStream)
        {
            byte[] buffer = new byte[2048];
            int bytesRead = 0;
            do
            {
                bytesRead = readStream.Read(buffer, 0, buffer.Length);
                writeStream.Write(buffer, 0, bytesRead);

            } while (bytesRead != 0);
            readStream.Close();
            writeStream.Flush();
            writeStream.Seek(0, SeekOrigin.Begin);
        }

        public static void SaveStream(string sFileName, Stream fromStream)
        {
            FileStream writeStream = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            CopyStream(fromStream, writeStream);
            writeStream.Close();
        }






    }
}
