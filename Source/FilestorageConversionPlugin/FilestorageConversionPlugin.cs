using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SitecoreConverter.Core;
using SitecoreConverter.Core.Plugins;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;
using FilestorageConversionPlugin.Gallery;
using Sgml;

namespace SitecoreConverter.Plugins
{
    public class FilestorageConversionPlugin : IItemCopyPlugin
    {
        private PluginOption[] _PluginOptions = null;
        private string _sLastDepartmentName = "Global";
        private string _sLastDepartmentID = "";
        public FilestorageConversionPlugin()
        {
            _PluginOptions = new PluginOption[2];
            _PluginOptions[0] = new PluginOption("Enable Filestorage to DIMS conversion:", "False", PluginOptionTypes.CheckBox);
            _PluginOptions[1] = new PluginOption("Gallery (DIMS) host name:", "http://billeder.3fklub.dk" /*"http://galleryKlub.web02.cabana.dk" "http://billeder.3f.dk" */, PluginOptionTypes.TextBox); // gallery.web02.cabana.dk
        }

        public string Name
        {
            get { return "Convert Filestorage"; }
        }

        public IPluginOption[] PluginOptions
        {
            get
            {
                return _PluginOptions;
            }
        }

        public void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            try
            {
                if (_PluginOptions[0].Value == "False")
                    return;
                // No template for this item, so it is probably a template itself
                if ((sourceItem.Templates == null) || (sourceItem.Templates[0] == null))
                    return;

                Uri hostUri = new Uri(sourceItem.GetHostUrl());
                string sHost = hostUri.Scheme + "://" + hostUri.Host;

                IItem tmpItem = sourceItem;                
                while (tmpItem != null)
                {
                    if ((tmpItem.Templates[0].Name.ToLower() == "fff.departmentwebsite") ||
                        (tmpItem.Templates[0].Name.ToLower() == "fff.industrysectorclubwebsite"))
                    {
                        // Try to get real gallery folder ID
                        IField field = Util.GetFieldByName("GalleryFolderID", tmpItem.Fields);
                        // We are still at the same department, _sLastDepartmentName is still valid
                        if (field.Content.ToUpper() == _sLastDepartmentID)
                            break;

                        // Default value
                        _sLastDepartmentName = tmpItem.Name;

                        // Try to get real gallery folder name
                        IItem mediaItem = null;
                        try
                        { mediaItem = sourceItem.GetItem(field.Content.ToUpper()); }
                        catch { }
                        if (mediaItem != null)
                        {
                            _sLastDepartmentName = mediaItem.Name;
                            _sLastDepartmentID = "{" + mediaItem.ID.ToString().ToUpper() + "}";
                        }
                        break;
                    }
                    tmpItem = tmpItem.Parent;
                }

                foreach (IField field in destinationItem.Fields)
                {
                    string sContent = field.Content.ToLower();

                    if ((sContent.IndexOf("<file") > -1) || (sContent.IndexOf("&lt;file ") > -1))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(field.Content);
                        XmlNodeList files = doc.SelectNodes("//file");
                        foreach (XmlNode file in files)
                        {
                            // empty file node?
                            if ((file.Attributes["url"] == null) || (file.Attributes["title"] == null))
                                continue;

                            // Content is already in gallery format
                            if (file.Attributes["galleryformat"] != null)
                                continue;

                            string sUrl = file.Attributes["url"].Value;
                            string sTitle = file.Attributes["title"].Value;
                            string sMediaID = file.Attributes["mediaid"].Value;

                            // Content is already in gallery format, since it contains destination host
                            if (sUrl.Contains(_PluginOptions[1].Value))
                                continue;

                            //                        IItem mediaItem = sourceItem.GetItem(sMediaID);
                            int iFileSize = 0;
                            string sResult = CopyFile(sHost + sUrl, sTitle, out iFileSize);
                            if (sResult != null)
                            {
                                file.Attributes["mediaid"].Value = sResult;
                                file.Attributes["url"].Value = _PluginOptions[1].Value + string.Format("/Image.ashx?File={0}", System.Web.HttpUtility.UrlEncode(sResult));
                                file.Attributes.Append(doc.CreateAttribute("size"));
                                file.Attributes["size"].Value = iFileSize.ToString();
                            }
                        }
                        // If the content doesn't have <files> root, the doc object inserts <xml> root
                        if (doc.FirstChild.Name == "files")
                            field.Content = doc.OuterXml;
                        else
                            field.Content = doc.InnerXml;
                    }
                    // Convert filestorage fields
                    else if (sContent.IndexOf("/sitecore%20modules/filestorage/default.aspx?fileid=") > -1)
                    {
                        XmlDocument doc = Sgml.SgmlUtil.ParseHtml(sContent);
                        XmlNodeList linkNodes = doc.SelectNodes("//a");
                        foreach (XmlNode node in linkNodes)
                        {
                            string sUrl = Core.Util.GetAttributeValue(node.Attributes["href"]);
                            string sTitle = Core.Util.GetAttributeValue(node.Attributes["sc_text"]);
                            string sc_linktype = Core.Util.GetAttributeValue(node.Attributes["sc_linktype"]);
                            string sc_url = Core.Util.GetAttributeValue(node.Attributes["sc_url"]);
                            string sTarget = Core.Util.GetAttributeValue(node.Attributes["target"]);
                            if ((sc_linktype == "media") ||
                                (sUrl.IndexOf("/sitecore%20modules/filestorage/default.aspx?fileid=") > -1))
                            {
                                int iFileSize = 0;
                                if (sUrl.IndexOf("//") == -1)
                                    sUrl = sHost + sUrl;

                                string sResult = CopyFile(sUrl, sTitle, out iFileSize);
                                if (sResult != null)
                                {
                                    node.Attributes.Append(doc.CreateAttribute("mediaid"));
                                    node.Attributes["mediaid"].Value = sResult;
                                    node.Attributes["href"].Value = _PluginOptions[1].Value + string.Format("/Image.ashx?File={0}", System.Web.HttpUtility.UrlEncode(sResult));
                                    node.Attributes.Append(doc.CreateAttribute("size"));
                                    node.Attributes["size"].Value = iFileSize.ToString();
                                }
                                else
                                    Util.WarningList.Add("Error converting mediacontent from: " + sourceItem.Path);
                            }
                            node.Attributes.Remove(node.Attributes["sc_text"]);
                            node.Attributes.Remove(node.Attributes["sc_linktype"]);
                            node.Attributes.Remove(node.Attributes["sc_url"]);
                        }

                        linkNodes = doc.SelectNodes("//img");
//                        if (linkNodes.Count == 0)
//                            linkNodes = doc.SelectNodes("//image");
                        foreach (XmlNode node in linkNodes)
                        {
                            string sSrc = Core.Util.GetAttributeValue(node.Attributes["src"]);
                            string sc_mediaid = Core.Util.GetAttributeValue(node.Attributes["sc_mediaid"]);
                            if (sc_mediaid == null)
                                sc_mediaid = Core.Util.GetAttributeValue(node.Attributes["mediaid"]);
                            if (sSrc != "")
                            {
                                // This is an existing gallery item, since it has the full path
                                if (sSrc.IndexOf("//") > -1)
                                    continue;

                                string sUrl = sSrc;
                                if (sUrl.IndexOf("//") == -1)
                                    sUrl = sHost + sUrl;
                                int iFileSize = 0;
                                string sTitle = "Importeret billede";
                                IItem mediaItem = null;
                                try
                                { mediaItem = sourceItem.GetItem(sc_mediaid.ToUpper()); }
                                catch { }
                                if (mediaItem != null)
                                    sTitle = mediaItem.Name;


                                string sResult = CopyFile(sHost + sSrc, sTitle, out iFileSize);
                                if (sResult != null)
                                {
                                    node.Attributes.Append(doc.CreateAttribute("sc_mediaid"));
                                    node.Attributes["sc_mediaid"].Value = sResult;
                                    node.Attributes["src"].Value = _PluginOptions[1].Value + string.Format("/Image.ashx?File={0}", System.Web.HttpUtility.UrlEncode(sResult));
                                    node.Attributes.Append(doc.CreateAttribute("size"));
                                    node.Attributes["size"].Value = iFileSize.ToString();
                                }
                                else
                                    Util.WarningList.Add("Error converting mediacontent from: " + sourceItem.Path);
                            }
                            node.Attributes.Remove(node.Attributes["sc_mediaid"]);
                        }


                        linkNodes = doc.SelectNodes("//image");
                        foreach (XmlNode node in linkNodes)
                        {
                            string sSrc = Core.Util.GetAttributeValue(node.Attributes["src"]);
                            string sMediaid = Core.Util.GetAttributeValue(node.Attributes["mediaid"]);
                            if (sSrc != "")
                            {
                                // This is an existing gallery item, since it has the full path
                                if (sSrc.IndexOf("//") > -1)
                                    continue;

                                string sTitle = "Importeret billede";
                                IItem mediaItem = null;
                                try
                                { mediaItem = sourceItem.GetItem(sMediaid.ToUpper()); }
                                catch { }
                                if (mediaItem != null)
                                    sTitle = mediaItem.Name;
                                int iFileSize = 0;

                                string sResult = CopyFile(sHost + sSrc, sTitle, out iFileSize);
                                if (sResult != null)
                                {
                                    string sFileUrl = _PluginOptions[1].Value + string.Format("/Image.ashx?File={0}", System.Web.HttpUtility.UrlEncode(sResult));
                                    string sXml = "";
                                    sXml = "<file galleryformat=\"1.0\" "; //mediapath=\"" + sFileUrl +  "\" ";
                                    sXml += "url=\"" + sFileUrl + "\" ";
                                    sXml += "title=\"" + sTitle + "\" ";
                                    sXml += "mediaid=\"" + sResult + "\" />";
                                    sXml += "size=\"" + iFileSize.ToString() + "\" />";
                                    doc.LoadXml("<root><xml>" + sXml + "</xml></root>");                                    
                                }
                                else
                                    Util.WarningList.Add("Error converting mediacontent from: " + sourceItem.Path);
                            }
                            node.Attributes.Remove(node.Attributes["sc_mediaid"]);
                        }



                        field.Content = HttpUtility.HtmlDecode(doc.SelectSingleNode("root").InnerXml);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private string CopyFile(string sUrl, string sTitle, out int iFileSize)
        {
            string sContentDispositionHeader = "";
            string sContentType = "";
            byte[] fileData = null;
            iFileSize = 0;
            try
            {
                fileData = WebRequest(sUrl, out sContentDispositionHeader, out sContentType);
                if (fileData == null)
                    return null;
            }
            catch (Exception ex)
            {
                Util.AddWarning("Error requesting file: " + sUrl + ", file is skipped. Error message: " + ex.Message);
                return null;
            }

            try
            {
                // Use title supplied and content type instead of sContentDispositionHeader
                if (sContentDispositionHeader == null)
                {
                    if (sContentType.ToLower().IndexOf("jpeg") > -1)
                        sContentDispositionHeader = sTitle + ".jpg";
                    if (sContentType.ToLower().IndexOf("gif") > -1)
                        sContentDispositionHeader = sTitle + ".gif";
                    if (sContentType.ToLower().IndexOf("png") > -1)
                        sContentDispositionHeader = sTitle + ".png";
                    if (sContentType.ToLower().IndexOf("bmp") > -1)
                        sContentDispositionHeader = sTitle + ".bmp";
                    if (sContentType.ToLower().IndexOf("pdf") > -1)
                        sContentDispositionHeader = sTitle + ".pdf";
                    if (sContentType.ToLower().IndexOf("msword") > -1)
                        sContentDispositionHeader = sTitle + ".doc";
                }

                sContentDispositionHeader = sContentDispositionHeader.Replace("attachment; filename=", "");
                // There HAS to be an extension
                if (sContentDispositionHeader.IndexOf(".") == -1)
                    return null;

                bool bIsImage = false;
                string sExt = Path.GetExtension(sContentDispositionHeader).ToLower();
                if (sExt == ".jpg" || sExt == ".jpeg" || sExt == ".gif" || sExt == ".png" || sExt == ".bmp" || sExt == ".tif" ||
                    sExt == ".tiff" || sExt == ".psd" || sExt == ".eps")
                    bIsImage = true;


                sContentDispositionHeader = System.Web.HttpUtility.UrlDecode(sContentDispositionHeader);
                GalleryAPISoapClient galleryAPI = new GalleryAPISoapClient("GalleryAPISoap", _PluginOptions[1].Value + "/GalleryAPI.asmx");

                iFileSize = fileData.Length;
                string sHash = CRC32.ComputeCRC32(fileData);

                SearchItemStruct foundItem = null;
                SearchItemStruct[] searchItems = null;
                if (bIsImage)
                    searchItems = galleryAPI.Search("", "gallery/Private/" + _sLastDepartmentName + "/Gamle billeder/" + sContentDispositionHeader);
                else
                    searchItems = galleryAPI.Search("", "gallery/Private/" + _sLastDepartmentName + "/Dokumenter/" + sContentDispositionHeader);
                foreach (SearchItemStruct item in searchItems)
                {
                    if (item.CRC32 == sHash)
                    {
                        foundItem = item;
                        break;
                    }
                }

                if (foundItem != null)
                    return foundItem.Path;
                else
                {
                    if (bIsImage)
                        return galleryAPI.UploadFile("/sitecore/media library/gallery/Private/" + _sLastDepartmentName + "/Gamle billeder", sContentDispositionHeader, fileData);
                    else
                        return galleryAPI.UploadFile("/sitecore/media library/gallery/Private/" + _sLastDepartmentName + "/Dokumenter", sContentDispositionHeader, fileData);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        public static byte[] ReadStreamFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static byte[] WebRequest(string sUrl, out string sContentDispositionHeader, out string sContentType)
        {
            HttpWebRequest oReq = (HttpWebRequest)HttpWebRequest.Create(sUrl);

            oReq.MaximumAutomaticRedirections = 10;
            oReq.Timeout = 30000; // 30 sec
            oReq.CookieContainer = new CookieContainer();
            oReq.Credentials = CredentialCache.DefaultCredentials;
            oReq.PreAuthenticate = true;
            oReq.UserAgent = "[3F - Sitecore]";

            byte[] fileData = null;

            WebResponse oResp = oReq.GetResponse();
            using (oResp)
            {
                sContentDispositionHeader = oResp.Headers["Content-Disposition"];
                sContentType = oResp.Headers["Content-Type"];
                string length = oResp.Headers["Content-Length"];
                fileData = ReadStreamFully(oResp.GetResponseStream());
            }
            return fileData;
        }

        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }
    }

}
