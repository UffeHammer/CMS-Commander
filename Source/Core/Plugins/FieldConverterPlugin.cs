using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace SitecoreConverter.Core.Plugins
{
    /// <summary>
    /// Phong L. 29-01-2010
    /// 
    /// FieldConverterPlugin
    /// This plugin allow custom methods to convert fields in Sitecore content.
    /// 
    /// In order to convert, a conversion method must be added to this class.
    /// The Method definition:
    /// - Must have a Custom Attribute "FieldConverterMethod" with a Name-parameter.
    /// - Must take string value as input fieldvalue.
    /// - Must return string as converted fieldvalue.
    /// 
    /// </summary>
    public class FieldConverterPlugin : IItemCopyPlugin
    {
        #region Inner Custom Attribute
        [AttributeUsage(AttributeTargets.Method,AllowMultiple=false)]
        private class FieldConverterMethod : Attribute
        {
            private string _methodName;
            public FieldConverterMethod(string methodName)
            {
                this._methodName = methodName;
            }

            public string MethodName { get { return _methodName; } }
        }
        #endregion Inner Custom Attribute

        private PluginOption[] _PluginOptions = null;
        private Dictionary<string, MethodInfo> _MethodInfos = null;

        public FieldConverterPlugin()
        {                        
            _MethodInfos = GetConversionMethods();//Loading conversion methods using reflection.

            if (_MethodInfos != null && _MethodInfos.Count > 0)
            {
                int numberOfPluginOptions = 3 + _MethodInfos.Count;
                _PluginOptions = new PluginOption[numberOfPluginOptions];
                _PluginOptions[0] = new PluginOption("Field Converter:", "False", PluginOptionTypes.CheckBox);
                _PluginOptions[1] = new PluginOption("TemplateName(comma sep.):", "", PluginOptionTypes.TextBox);
                _PluginOptions[2] = new PluginOption("FieldName:", "", PluginOptionTypes.TextBox);

                int currentIndex = 3;
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
                    Object[] atr = mi.GetCustomAttributes(typeof(FieldConverterMethod), false);
                    if (atr != null)
                    {
                        foreach (Object pluginMethodAtr in atr)
                        {
                            lstMethods.Add(((FieldConverterMethod)pluginMethodAtr).MethodName, mi);
                        }
                    }

                }
            }

            return lstMethods;
        }

        #region IItemCopyPlugin Members

        public string Name
        {
            get { return "Field Converter"; }
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

            string templateName = _PluginOptions[1].Value;
            string fieldName = _PluginOptions[2].Value;

            string[] templateNames = templateName.ToLower().Split(',');


            foreach (IItem sourceTemplate in sourceItem.Templates)
            {
                //If Item match our Template
                //if (!String.IsNullOrEmpty(templateName) && !String.IsNullOrEmpty(fieldName) && sourceItem.Template.Key == templateName.ToLower())
                if (templateNames != null && templateNames.Length > 0 && !String.IsNullOrEmpty(fieldName) && templateNames.Contains<string>(sourceTemplate.Key))
                {
                    //Get Field that matches our FieldName. FieldName is assumed unique.
                    IField foundField = null;
                    try
                    {
                        foundField = sourceItem.Fields.Last<IField>(currentField => (currentField.Key.Equals(fieldName.ToLower())));
                    }
                    catch { }

                    if (foundField != null)
                    {
                        string inputFieldValue = foundField.Content;

                        //Looking for FieldValue-Conversion Methods
                        for (int i = 3; i < _PluginOptions.Length; i++)
                        {
                            IPluginOption pluginOption = _PluginOptions[i];
                            if (pluginOption.Type == PluginOptionTypes.CheckBox && !pluginOption.Value.Equals("False"))
                            {
                                //Get the MethodInfo
                                MethodInfo currentMethod = _MethodInfos[pluginOption.Name];
                                if (currentMethod != null)
                                {
                                    object[] arguments = new object[] { inputFieldValue };
                                    string convertedFieldValue = currentMethod.Invoke(this, arguments).ToString(); //Using conversion method to convert field

                                    //Store the converted field into destination Field. FieldName is assumed unique.
                                    IField destinationField = destinationItem.Fields.Last<IField>(currentField => (currentField.Key.Equals(fieldName.ToLower())));

                                    if (destinationField != null)
                                        destinationField.Content = convertedFieldValue;
                                }
                            }
                        }
                    }
                }
            }
        }        

        #endregion

        //[FieldConverterMethod("Sitecore Icon path converter")]
        //public string SitecoreIconPathConverter(string inputFieldValue)
        //{
        //    string convertedFieldValue = inputFieldValue;
        //    const string newPath = "/resources/3FLocal/Icons";

        //    if (!String.IsNullOrEmpty(inputFieldValue))
        //    {
        //        FileInfo file = new FileInfo(inputFieldValue);
        //        if (!inputFieldValue.ToLower().Contains("/sitecore/client/themes/standard/"))
        //        {
        //            convertedFieldValue = newPath + "/" + file.Name;
        //        }
        //    }
        //    return convertedFieldValue;
        //}

        /// <summary>
        /// This fieldconversion is used to 'uncheck' a checkbox
        /// </summary>
        /// <param name="inputFieldValue"></param>
        /// <returns></returns>
        [FieldConverterMethod("UnCheck CheckBox")]
        public string UnCheckCheckBox(string inputFieldValue)
        {            
            return "0";
        }

        /// <summary>
        /// This fieldconversion is used to 'check' a checkbox
        /// </summary>
        /// <param name="inputFieldValue"></param>
        /// <returns></returns>
        [FieldConverterMethod("Check CheckBox")]
        public string CheckCheckBox(string inputFieldValue)
        {
            return "1";
        }



        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }
    }
}
