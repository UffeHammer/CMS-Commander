using System;
using System.Configuration;
using System.Collections;


namespace SitecoreConverter.Core.Plugins
{
    public class PluginConfigurationElement : ConfigurationElement
    {
        // Constructor allowing name, url, and port to be specified.
        public PluginConfigurationElement(String newName,
            String newAssembly, String newPluginType)
        {
            Name = newName;
            Assembly = newAssembly;
            PluginType = newPluginType;
        }

        // Default constructor, will use default values as defined
        // below.
        public PluginConfigurationElement()
        {
        }

        // Constructor allowing name to be specified, will take the
        // default values for url and port.
        public PluginConfigurationElement(string elementName)
        {
            Name = elementName;
        }

        [ConfigurationProperty("name",
            DefaultValue = "Custom plugin",
            IsRequired = true,
            IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("assembly",
            DefaultValue = "",
            IsRequired = true,
            IsKey = true)]
        public string Assembly
        {
            get
            {
                return (string)this["assembly"];
            }
            set
            {
                this["assembly"] = value;
            }
        }

        [ConfigurationProperty("plugintype",
            DefaultValue = "",
            IsRequired = true)]
        public string PluginType
        {
            get
            {
                return (string)this["plugintype"];
            }
            set
            {
                this["plugintype"] = value;
            }
        }
/*
        [ConfigurationProperty("port",
            DefaultValue = (int)0,
            IsRequired = false)]
        [IntegerValidator(MinValue = 0,
            MaxValue = 8080, ExcludeRange = false)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }
*/
        protected override void DeserializeElement(
           System.Xml.XmlReader reader,
            bool serializeCollectionKey)
        {
            base.DeserializeElement(reader,
                serializeCollectionKey);
            // You can your custom processing code here.
        }


        protected override bool SerializeElement(
            System.Xml.XmlWriter writer,
            bool serializeCollectionKey)
        {
            bool ret = base.SerializeElement(writer,
                serializeCollectionKey);
            // You can enter your custom processing code here.
            return ret;

        }


        protected override bool IsModified()
        {
            bool ret = base.IsModified();
            // You can enter your custom processing code here.
            return ret;
        }
    }

}
