using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgml;
using SitecoreConverter.Core;
using SitecoreConverter.Core.Plugins;
using System.Xml;
using System.IO;
using System.Web;

namespace SitecoreConverter.Plugins
{
    public class HtmlToXhtmlPlugin : IItemCopyPlugin
    {

        private PluginOption[] _PluginOptions = null;
        public HtmlToXhtmlPlugin()
        {
            _PluginOptions = new PluginOption[1];
            _PluginOptions[0] = new PluginOption("Enable HTML to XHTML conversion:", "True", PluginOptionTypes.CheckBox);
        }

        public string Name
        {
            get
            {
                return "Fix HTML errors";
            }
        }

        public IPluginOption[] PluginOptions
        {
            get
            {
                return _PluginOptions;
            }        
        }

        public static string FixContent(string sContent)
        {
            XmlDocument doc = Sgml.SgmlUtil.ParseHtml(sContent);
            XmlNode root = doc.SelectSingleNode("root");

            string sNewContent = root.InnerXml;
            // No tags at all, add a paraggraph tag
            if (root.ChildNodes.Count == 1)
                sNewContent = "<p>" + root.InnerXml + "</p>";

            sNewContent = HttpUtility.HtmlDecode(sNewContent.Replace("&amp;", "&")).Replace("&amp;", "&"); //.Replace("&amp;nbsp;", "&nbsp;");
            // This removes double escaped instances
            sNewContent = sNewContent.Replace("&amp; amp;", "&");

            // Test xml validity
            try
            {
                doc.LoadXml("<root>" + sNewContent + "</root>");
            }
            catch
            {
                // Not valid revert
                sNewContent = sContent;
            }

            return sNewContent;
        }

        public void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            if (_PluginOptions[0].Value == "False")
                return;


            for (int t=0; t<destinationItem.Fields.Length; t++)
            {
                if ((destinationItem.Fields[t].Type.ToLower() == "rich text") ||
                    (destinationItem.Fields[t].Type.ToLower() == "html"))
                {
                    try
                    {
                        string sContent = destinationItem.Fields[t].Content;
                        if (sContent == "")
                            continue;

                        sContent = FixContent(sContent);

                        destinationItem.Fields[t].Content = sContent;
                    }
                    catch 
                    {
                        Util.WarningList.Add("Warning could not convert field to xhtml in: " + destinationItem.Path);
                    }
                }
            }
        }

        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }
    }
}
