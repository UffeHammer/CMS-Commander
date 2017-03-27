using System;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace SitecoreConverter.Core.Plugins
{
    public class PluginManagerConfiguration : ConfigurationSection
    {
        public PluginManagerConfiguration()
        {
            Name = "";
            Type = "";
        }


        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }


        [ConfigurationProperty("type")]
        public string Type
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }


        [ConfigurationProperty("ItemCopyPlugins", IsDefaultCollection = false)]
        public PluginConfigurations ItemCopyPlugins
        {
            get { return (PluginConfigurations)base["ItemCopyPlugins"]; }
            set { base["ItemCopyPlugins"] = value; }
        }

        protected override void DeserializeSection(
            System.Xml.XmlReader reader)
        {
            base.DeserializeSection(reader);
            // You can add custom processing code here.
        }

        protected override string SerializeSection(
            ConfigurationElement parentElement,
            string name, ConfigurationSaveMode saveMode)
        {
            string s =
                base.SerializeSection(parentElement,
                name, saveMode);
            // You can add custom processing code here.
            return s;
        }

    }

}

