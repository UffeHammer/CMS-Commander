using System;
using Newtonsoft.Json.Linq;

namespace SitecoreConverter.Core
{
    public class Umbraco14xField : IField
    {
        private string _sName = "";
        private string _sLanguageTitle = "";
        private string _sKey = "";
        private string _sSource = null;
        private string _sSection = null;
        private string _sContent = "";
        private string _sType = "";
        private string _sSortOrder = "";
        private string _sTemplateFieldID = "";
        private string _sCulture = null;   // null = invariant
        private string _sSegment = null;   // always null in this provider
        private bool _bDirty = false;

        public string Name { get { return _sName; } set { _sName = value; } }
        public string LanguageTitle { get { return _sLanguageTitle; } set { _sLanguageTitle = value; } }
        public string Key { get { return _sKey; } }
        public string Source { get { return _sSource; } set { _sSource = value; } }
        public string Section { get { return _sSection; } set { _sSection = value; } }
        public string Type { get { return _sType; } }
        public string SortOrder { get { return _sSortOrder; } set { _sSortOrder = value; } }
        public string TemplateFieldID { get { return _sTemplateFieldID; } }

        public string Content
        {
            get { return _sContent; }
            set
            {
                if (_sContent != value) _bDirty = true;
                _sContent = value;
            }
        }

        public string Culture { get { return _sCulture; } }
        public string Segment { get { return _sSegment; } }
        public bool IsDirty { get { return _bDirty; } set { _bDirty = value; } }

        /// <summary>Build a field from a content/media `values[]` entry and enrich with
        /// property-definition metadata from the item's document type.</summary>
        public Umbraco14xField(JObject valueNode, JObject propertyDefinition)
        {
            _sName = (string)valueNode["alias"] ?? "";
            _sKey = _sName.ToLower();
            _sCulture = (string)valueNode["culture"];
            _sSegment = (string)valueNode["segment"];

            var rawValue = valueNode["value"];
            _sContent = rawValue == null || rawValue.Type == JTokenType.Null
                ? ""
                : (rawValue.Type == JTokenType.String ? (string)rawValue : rawValue.ToString(Newtonsoft.Json.Formatting.None));

            EnrichFromPropertyDefinition(propertyDefinition);
        }

        /// <summary>Build a field from a document-type property definition (no value yet).</summary>
        public Umbraco14xField(JObject propertyDefinition)
        {
            _sName = (string)propertyDefinition["alias"] ?? "";
            _sKey = _sName.ToLower();
            _sContent = "";
            EnrichFromPropertyDefinition(propertyDefinition);
        }

        private void EnrichFromPropertyDefinition(JObject propertyDefinition)
        {
            if (propertyDefinition == null) return;

            _sLanguageTitle = (string)propertyDefinition["name"] ?? _sName;
            _sType = (string)propertyDefinition["propertyEditorAlias"]
                ?? (string)propertyDefinition["dataType"]?["editorAlias"]
                ?? "";
            _sSortOrder = (propertyDefinition["sortOrder"] ?? "").ToString();
            _sTemplateFieldID = (string)propertyDefinition["id"] ?? "";

            var group = propertyDefinition["container"];
            if (group != null)
                _sSection = (string)group["name"] ?? (string)group["alias"];

            var dataType = propertyDefinition["dataType"];
            if (dataType != null)
                _sSource = dataType.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>Emit this field as a `values[]` entry for a PUT payload.</summary>
        public JObject ToValuesEntry()
        {
            var entry = new JObject
            {
                ["editorAlias"] = _sType,
                ["alias"] = _sName,
                ["culture"] = _sCulture,
                ["segment"] = _sSegment
            };

            JToken valueToken;
            if (string.IsNullOrEmpty(_sContent))
            {
                valueToken = null;
            }
            else if (_sContent.StartsWith("{") || _sContent.StartsWith("["))
            {
                try { valueToken = JToken.Parse(_sContent); }
                catch { valueToken = new JValue(_sContent); }
            }
            else
            {
                valueToken = new JValue(_sContent);
            }
            entry["value"] = valueToken;
            return entry;
        }
    }
}
