using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace SitecoreConverter.Core
{

    public class SitecoreWebAPIUtil
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public SitecoreWebAPIUtil(string host, string username, string password)
        {
            this.Host = host;
            this.Username = username;
            this.Password = password;
        }

        public string ReadWebAPIContent(string sHost, string sItemID, string sDatabase, string sLanguage = "en")
        {
            string sUrl = String.Format("{0}/-/item/v1?", sHost.TrimEnd('/'));
            sUrl += String.Format("sc_itemid={0}&sc_database={1}&language={2}"
                   , HttpUtility.UrlEncode(sItemID)
                   , HttpUtility.UrlEncode(sDatabase)
                   , HttpUtility.UrlEncode(sLanguage));

            return sUrl;
        }

        public string GetWebAPIMessage(string sUrl)
        {

            HttpWebRequest request = Util.CreateWebRequest(sUrl, 0, "GET");
            request.KeepAlive = true;
            request.Headers.Add("X-Scitemwebapi-Username", this.Username);
            request.Headers.Add("X-Scitemwebapi-Password", this.Password);
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

        public string PostMedia(string itemName, string parentId, string databaseName, Stream fileStream, string fileExtension)
        {
            //create the url
            string url = String.Format("{0}/-/item/v1/?", this.Host.TrimEnd('/'));

            //append parameters
            url += String.Format("name={0}&sc_itemid={1}&sc_database={2}&payload=content"
                    , HttpUtility.UrlEncode(itemName)
                    , HttpUtility.UrlEncode(parentId.ToString())
                    , HttpUtility.UrlEncode(databaseName));

            //create request instance
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            //headers
            request.Method = "POST";
            request.KeepAlive = false;
            request.Headers.Add("X-Scitemwebapi-Username", this.Username);
            request.Headers.Add("X-Scitemwebapi-Password", this.Password);

            //apply content type and boundary
            string boundary = "---------------------------" + DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            request.ContentType = "multipart/form-data; boundary=" + boundary;

            using (Stream stream = request.GetRequestStream())
            {
                //boundary
                stream.Write(boundaryBytes, 0, boundaryBytes.Length);

                //file header
                string header = "Content-Disposition: form-data; name=\"file\"; filename=\"" + itemName + fileExtension + "\"\r\nContent-Type: multipart/form-data\r\n\r\n";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(header);
                stream.Write(bytes, 0, bytes.Length);

                //file bytes
                byte[] buffer = new byte[32768];
                int bytesRead;
                if (stream != null)
                {
                    // upload from a given stream
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        stream.Write(buffer, 0, bytesRead);
                    }
                }

                byte[] end = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                stream.Write(end, 0, end.Length);
                stream.Close();
            }

            //send post
            // request.GetResponse();
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
    }
}
