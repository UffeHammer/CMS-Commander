using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace SitecoreConverter.Core.Plugins
{
    public class CustomConverterPlugin : IItemCopyPlugin
    {
        #region Inner Custom Attribute
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        private class CustomConverterMethod : Attribute
        {
            private string _methodName;
            public CustomConverterMethod(string methodName)
            {
                this._methodName = methodName;
            }

            public string MethodName { get { return _methodName; } }
        }
        #endregion Inner Custom Attribute

        private PluginOption[] _PluginOptions = null;
        private Dictionary<string, MethodInfo> _MethodInfos = null;

        public CustomConverterPlugin()
        {                        
            _MethodInfos = GetConversionMethods();//Loading conversion methods using reflection.

            if (_MethodInfos != null && _MethodInfos.Count > 0)
            {
                int numberOfPluginOptions = 1 + _MethodInfos.Count;
                _PluginOptions = new PluginOption[numberOfPluginOptions];
                _PluginOptions[0] = new PluginOption("Custom Converter:", "False", PluginOptionTypes.CheckBox);                

                int currentIndex = 1;
                foreach (string key in _MethodInfos.Keys)
                {
                    _PluginOptions[currentIndex] = new PluginOption(key, "Enable", PluginOptionTypes.CheckBox);
                    currentIndex++;
                }
            }

        }

        /// <summary>
        /// Method to Get all conversion method by using reflection.
        /// Looking for all methods that has a "FieldConverterMethod"-Attribute attached.
        /// </summary>
        /// <returns>Dictionary with Conversion name as key and a MethodInfo object as value</returns>
        private Dictionary<string, MethodInfo> GetConversionMethods()
        {
            Dictionary<string, MethodInfo> lstMethods = new Dictionary<string, MethodInfo>();

            MethodInfo[] methods = this.GetType().GetMethods();
            if (methods != null)
            {
                foreach (MethodInfo mi in methods)
                {
                    Object[] atr = mi.GetCustomAttributes(typeof(CustomConverterMethod), false);
                    if (atr != null)
                    {
                        foreach (Object pluginMethodAtr in atr)
                        {
                            lstMethods.Add(((CustomConverterMethod)pluginMethodAtr).MethodName, mi);
                        }
                    }

                }
            }

            return lstMethods;
        }

        #region IItemCopyPlugin Members

        public string Name
        {
            get { return "Custom Converter"; }
        }

        public IPluginOption[] PluginOptions
        {
            get { return _PluginOptions; }
        }

        public void CopyItemCallback(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            if (_PluginOptions[0].Value == "False")
                return;

            if (_MethodInfos == null || (_MethodInfos != null && _MethodInfos.Count == 0))
                return;

            //Looking for FieldValue-Conversion Methods
            for (int i = 1; i < _PluginOptions.Length; i++)
            {
                IPluginOption pluginOption = _PluginOptions[i];
                if (pluginOption.Type == PluginOptionTypes.CheckBox && !pluginOption.Value.Equals("False"))
                {
                    //Get the MethodInfo
                    MethodInfo currentMethod = _MethodInfos[pluginOption.Name];
                    if (currentMethod != null)
                    {
                        object[] arguments = new object[] { sourceItem, destinationParentItem, destinationItem};
                        bool invoked = (bool)currentMethod.Invoke(this, arguments); //Calling Custom Converter Plugin                        
                    }
                }
            }

            //If Item match our Template
            //if (!String.IsNullOrEmpty(templateName) && !String.IsNullOrEmpty(fieldName) && sourceItem.Template.Key == templateName.ToLower())
            //if (templateNames != null && templateNames.Length > 0 && !String.IsNullOrEmpty(fieldName) && templateNames.Contains<string>(sourceItem.Template.Key))
            //{
            //    //Get Field that matches our FieldName. FieldName is assumed unique.
            //    IField foundField = null;
            //    try
            //    {
            //        foundField = sourceItem.Fields.Last<IField>(currentField => (currentField.Key.Equals(fieldName.ToLower())));
            //    }
            //    catch { }

            //    if (foundField != null)
            //    {
            //        string inputFieldValue = foundField.Content;

            //        //Looking for FieldValue-Conversion Methods
            //        for (int i = 3; i < _PluginOptions.Length; i++)
            //        {
            //            IPluginOption pluginOption = _PluginOptions[i];
            //            if (pluginOption.Type == PluginOptionTypes.CheckBox && !pluginOption.Value.Equals("False"))
            //            {
            //                //Get the MethodInfo
            //                MethodInfo currentMethod = _MethodInfos[pluginOption.Name];
            //                if (currentMethod != null)
            //                {
            //                    object[] arguments = new object[] { inputFieldValue };
            //                    string convertedFieldValue = currentMethod.Invoke(this, arguments).ToString(); //Using conversion method to convert field

            //                    //Store the converted field into destination Field. FieldName is assumed unique.
            //                    IField destinationField = destinationItem.Fields.Last<IField>(currentField => (currentField.Key.Equals(fieldName.ToLower())));

            //                    if (destinationField != null)
            //                        destinationField.Content = convertedFieldValue;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        /// <summary>
        /// This fieldconversion is used to 'check' a checkbox
        /// </summary>
        /// <param name="inputFieldValue"></param>
        /// <returns></returns>
        [CustomConverterMethod("Test")]
        public bool CheckCheckBox(IItem sourceItem, IItem destinationParentItem, IItem destinationItem)
        {
            IItem mainTemplateItem = GetMainTemplateItem(destinationItem);

            if (mainTemplateItem != null && mainTemplateItem.Key.Equals("fff.departmentitem") && destinationItem.Key.Equals("publishingmode"))
            {
                if (destinationParentItem != null && destinationParentItem.Key.Equals("crm synkronisering"))
                {
                    IField destinationField = destinationItem.Fields.Last<IField>(currentField => (currentField.Key.Equals("source")));
                    destinationField.Content = "Modified by Phong";
                }

            }

            //Get Field that matches our FieldName. FieldName is assumed unique.
            //IField foundField = null;
            //try
            //{
            //    foundField = sourceItem.Fields.Last<IField>(currentField => (currentField.Key.Equals("__never publish")));
            //}
            //catch { }

            //Store the converted field into destination Field. FieldName is assumed unique.
            //IField destinationField = destinationItem.Fields.Last<IField>(currentField => (currentField.Key.Equals("__never publish")));
            
//            destinationItem.Template.Fields

            //Sitecore6xItem itm = Sitecore6xItem.GetItem(destinationItem.Path, 
            //if (destinationField != null)
            //    destinationField.Content = "1";

            

            return true;
        }

        /// <summary>
        /// Get the MainItemTemplate.
        /// Fx. currentItem could be a template field and we need to find out the template item the field belongs to.
        /// </summary>
        /// <param name="currentItem"></param>
        /// <returns></returns>
        private IItem GetMainTemplateItem(IItem currentItem)
        {
            if (currentItem == null)
                return null;


            if (!currentItem.Templates[0].Key.Equals("template"))
            {
                return GetMainTemplateItem(currentItem.Parent);
            }
            else
            {
                return currentItem;
            }
        }


        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }
    }
}
