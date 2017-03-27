using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

namespace SitecoreConverter.Core.Plugins
{

    public class PluginConfigurations : ConfigurationElementCollection
    {
        public PluginConfigurations()
        {
            // Add one configElement to the collection.  This is
            // not necessary; could leave the collection 
            // empty until items are added to it outside
            // the constructor.
//            PluginConfigurationElement configElement =
//                (PluginConfigurationElement)CreateNewElement();
//            Add(configElement);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginConfigurationElement();
        }


        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new PluginConfigurationElement(elementName);
        }


        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfigurationElement)element).Name;
        }


        public new string AddElementName
        {
            get
            { return base.AddElementName; }

            set
            { base.AddElementName = value; }

        }

        public new string ClearElementName
        {
            get
            { return base.ClearElementName; }

            set
            { base.AddElementName = value; }

        }

        public new string RemoveElementName
        {
            get
            { return base.RemoveElementName; }
        }

        public new int Count
        {
            get { return base.Count; }
        }


        public PluginConfigurationElement this[int index]
        {
            get
            {
                return (PluginConfigurationElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public PluginConfigurationElement this[string Name]
        {
            get
            {
                return (PluginConfigurationElement)BaseGet(Name);
            }
        }

        public int IndexOf(PluginConfigurationElement configElement)
        {
            return BaseIndexOf(configElement);
        }

        public void Add(PluginConfigurationElement configElement)
        {
            BaseAdd(configElement);
            // Add custom code here.
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
            // Add custom code here.
        }

        public void Remove(PluginConfigurationElement configElement)
        {
            if (BaseIndexOf(configElement) >= 0)
                BaseRemove(configElement.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
            // Add custom code here.
        }
    }

}

