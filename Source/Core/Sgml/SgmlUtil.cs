using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.IO;
using System.Net;

namespace Sgml
{
    public class SgmlUtil
    {
        // Creates XmlDocument from html content and return it with rootitem "<root>".
        public static XmlDocument ParseHtml(string sContent)
        {
            StringReader sr = new StringReader("<root>" + sContent + "</root>");
            SgmlReader reader = new SgmlReader();
            reader.WhitespaceHandling = WhitespaceHandling.All;
            reader.CaseFolding = Sgml.CaseFolding.ToLower;
            reader.InputStream = sr;

            StringWriter sw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(sw);
            w.Formatting = Formatting.Indented;
            w.WriteStartDocument();
            reader.Read();
            while (!reader.EOF)
            {
                w.WriteNode(reader, true);
            }
            w.Flush();
            w.Close();

            sw.Flush();

            // create document
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.XmlResolver = null;
            doc.LoadXml(sw.ToString());

            reader.Close();

            return doc;
        }
    }
}
