using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;


namespace SitecoreConverter.Core.Plugins
{
    public class PluginOption : IPluginOption
    {
        private string _sName = "";
        private string _sValue = "";
        private PluginOptionTypes _Type = PluginOptionTypes.undefined;
        private string _sInitialValue = "";

        public PluginOption(string sName, string sValue, PluginOptionTypes Type)
        {
            _sName = sName;
            _sValue = sValue;
            _Type = Type;
        }

        public PluginOption(string sName, string sValue, PluginOptionTypes Type, string sInitialValue)
        {
            _sName = sName;
            _sValue = sValue;
            _Type = Type;
            _sInitialValue = sInitialValue;
        }


        public string Name
        {
            get { return _sName; }
        }

        public string Value
        {
            get { return _sValue; }
            set { _sValue = value; }
        }

        public PluginOptionTypes Type
        {
            get { return _Type; }
        }

        public string InitialValue
        {
            get { return _sInitialValue; }
        }

    }

    public class GoogleTranslatePlugin : IItemCopyPlugin
    {
        private PluginOption[] _PluginOptions = null;
        public GoogleTranslatePlugin()
        {
            _PluginOptions = new PluginOption[2];
            _PluginOptions[0] = new PluginOption("Enable Google autotranslation:", "False", PluginOptionTypes.CheckBox);
            _PluginOptions[1] = new PluginOption("ID that is added to translated texts:", "*AUTOTRANSLATED*", PluginOptionTypes.TextBox);
        }

        public string Name
        {
            get
            {
                return "GoogleTranslate";
            }
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
            if (_PluginOptions[0].Value == "False")
                return;

            GoogleTranslator translator = new GoogleTranslator(sourceItem.Options.Language, destinationItem.Options.Language, "");
            foreach (IField field in destinationItem.Fields)
            {
                if ((field.Type == "Single-Line Text") || (field.Type == "Multi-Line Text") || (field.Type == "Rich Text"))
                {
                    string sTranslated = translator.Translate(field.Content);
                    if (sTranslated != null)
                        field.Content = sTranslated;
                }
            }

            //            throw new Exception("Hello from plugin");            
        }


        public bool ShouldItemBeCopiedCallback(IItem sourceItem, IItem destinationParentItem)
        {
            return true;
        }
    }



    [Serializable]
    public class JSONResponse
    {
        public string responseDetails = null;
        public string responseStatus = null;
    }

    // Then, a Translation object that inherits from this class: 
    [Serializable]
    public class Translation : JSONResponse
    {
        public TranslationResponseData responseData =
         new TranslationResponseData();
    }

    // This Translation class has a TranslationResponseData object that looks like this: 
    [Serializable]
    public class TranslationResponseData
    {
        public string translatedText;
    }







    public class GoogleTranslator
    {
        private string _q = "";
        private string _key = "";
        private string _langPair = "";
        private string _requestUrl = "";




        public GoogleTranslator(string sFromLanguage, string sToLanguage, string sKey)
        {
            _langPair = HttpUtility.UrlEncode(sFromLanguage + "|" + sToLanguage);
            _key = HttpUtility.UrlEncode(sKey);
        }




        public string Translate(string queryTerm)
        {
            queryTerm = queryTerm.Replace("\n", "<br>");
            _q = HttpUtility.UrlEncode(queryTerm);
            string encodedRequestUrlFragment = string.Format("?v=1.0&q={0}&langpair={1}", _q, _langPair);

            if (_key != "")
                encodedRequestUrlFragment += "&key=" + _key;

            string sTranslate = "http://ajax.googleapis.com/ajax/services/language/translate";
            _requestUrl = sTranslate + encodedRequestUrlFragment;




            try
            {

                WebRequest request = WebRequest.Create(_requestUrl);

                WebResponse response = request.GetResponse();



                StreamReader reader = new StreamReader(response.GetResponseStream());

                string json = reader.ReadLine();

                using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {

                    DataContractJsonSerializer ser =

                       new DataContractJsonSerializer(typeof(Translation));

                    Translation translation = ser.ReadObject(ms) as Translation;



                    return HttpUtility.UrlDecode(translation.responseData.translatedText);

                }

            }

            catch (Exception) { }
            return null;
        }

    }

}
