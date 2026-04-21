using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SitecoreConverter.Core
{
    public enum Umbraco14xItemKind
    {
        Root,
        BranchRoot,
        Content,
        Media,
        DocumentType,
        DataType,
        Member,
        MemberGroup,
        User,
        UserGroup,
        Language
    }

    public class Umbraco14xItem : IItem
    {
        // Sentinel GUIDs for synthetic nodes (random but fixed).
        private static readonly Guid ROOT_SENTINEL = new Guid("14000000-0000-0000-0000-000000000000");
        private static readonly Dictionary<string, Guid> BRANCH_SENTINELS = new Dictionary<string, Guid>
        {
            { "Content",    new Guid("14000001-0000-0000-0000-000000000000") },
            { "Media",      new Guid("14000002-0000-0000-0000-000000000000") },
            { "Templates",  new Guid("14000003-0000-0000-0000-000000000000") },
            { "DataTypes",  new Guid("14000004-0000-0000-0000-000000000000") },
            { "Members",    new Guid("14000005-0000-0000-0000-000000000000") },
            { "Roles",      new Guid("14000006-0000-0000-0000-000000000000") },
            { "Languages",  new Guid("14000007-0000-0000-0000-000000000000") }
        };

        // Branch name → ItemKind for its children
        private static readonly Dictionary<string, Umbraco14xItemKind> BRANCH_CHILD_KIND =
            new Dictionary<string, Umbraco14xItemKind>
            {
                { "Content",    Umbraco14xItemKind.Content },
                { "Media",      Umbraco14xItemKind.Media },
                { "Templates",  Umbraco14xItemKind.DocumentType },
                { "DataTypes",  Umbraco14xItemKind.DataType },
                { "Members",   Umbraco14xItemKind.Member },
                { "Roles",      Umbraco14xItemKind.UserGroup },
                { "Languages",  Umbraco14xItemKind.Language }
            };

        private readonly Umbraco14xAPI _api;
        private ConverterOptions _Options;
        private Umbraco14xItemKind _kind;

        private string _sID = "";          // GUID string
        private string _sName = "";
        private string _sKey = "";
        private string _sPath = "";
        private string _sIcon = "";
        private string _sSortOrder = "";
        private string _sParentID = "";
        private Umbraco14xItem _parent = null;
        private bool _bHasChildren = false;
        private JObject _rawPayload = null;
        private List<Umbraco14xField> _fields = null;

        // Dirty flags for Save()
        private bool _nameDirty = false;
        private bool _iconDirty = false;
        private bool _sortOrderDirty = false;

        public Umbraco14xItemKind Kind { get { return _kind; } }
        public Umbraco14xAPI Api { get { return _api; } }
        public JObject RawPayload { get { return _rawPayload; } }

        #region Construction

        public static Umbraco14xItem GetRoot(Umbraco14xAPI api, ConverterOptions options)
        {
            return new Umbraco14xItem(api, options, Umbraco14xItemKind.Root, null);
        }

        /// <summary>Synthetic root / branch constructor.</summary>
        private Umbraco14xItem(Umbraco14xAPI api, ConverterOptions options, Umbraco14xItemKind kind, string branchName)
        {
            _api = api;
            _Options = options;
            _kind = kind;
            _fields = new List<Umbraco14xField>();

            if (kind == Umbraco14xItemKind.Root)
            {
                _sID = ROOT_SENTINEL.ToString();
                _sName = "umbraco";
                _sKey = "umbraco";
                _sPath = "/umbraco";
                _bHasChildren = true;
            }
            else if (kind == Umbraco14xItemKind.BranchRoot)
            {
                _sID = BRANCH_SENTINELS[branchName].ToString();
                _sName = branchName;
                _sKey = branchName.ToLower();
                _sPath = "/umbraco/" + branchName;
                _bHasChildren = true;
            }
        }

        /// <summary>Real-item constructor.</summary>
        internal Umbraco14xItem(Umbraco14xAPI api, ConverterOptions options, Umbraco14xItemKind kind,
                                JObject payload, Umbraco14xItem parent)
        {
            _api = api;
            _Options = options;
            _kind = kind;
            _parent = parent;
            _fields = new List<Umbraco14xField>();
            LoadFromPayload(payload);
        }

        private void LoadFromPayload(JObject payload)
        {
            _rawPayload = payload;
            _sID = (string)payload["id"] ?? "";
            _bHasChildren = (bool?)payload["hasChildren"] ?? false;
            _sSortOrder = (payload["sortOrder"] ?? "").ToString();
            _sIcon = (string)payload["icon"] ?? "";

            // Name: varying items expose variants[]; invariant items expose top-level name.
            var variants = payload["variants"] as JArray;
            if (variants != null && variants.Count > 0)
            {
                var current = PickVariantForCulture(variants);
                _sName = (string)current["name"] ?? "";
            }
            else
            {
                _sName = (string)payload["name"] ?? "";
            }
            _sKey = _sName.ToLower();

            var parentRef = payload["parent"] as JObject;
            if (parentRef != null) _sParentID = (string)parentRef["id"] ?? "";

            // Path is computed lazily (see Task 5).
        }

        private JObject PickVariantForCulture(JArray variants)
        {
            var culture = _Options != null ? _Options.Language : "en";
            foreach (var v in variants.OfType<JObject>())
            {
                var vc = (string)v["culture"];
                if (vc == null) return v;  // invariant
                if (string.Equals(vc, culture, StringComparison.OrdinalIgnoreCase)) return v;
                if (vc.StartsWith(culture, StringComparison.OrdinalIgnoreCase)) return v;
            }
            return (JObject)variants[0];
        }

        #endregion

        #region IItem — skeleton stubs (filled in later tasks)

        public string ID { get { return _sID; } }
        public string Name { get { return _sName; } }
        public string Key { get { return _sKey; } }
        public string Path { get { return _sPath; } }
        public string Icon { get { return _sIcon; } set { _sIcon = value; _iconDirty = true; } }
        public string SortOrder { get { return _sSortOrder; } set { _sSortOrder = value; _sortOrderDirty = true; } }
        public IItem[] Templates { get { throw new NotImplementedException(); } }
        public IItem BaseTemplate { get { throw new NotImplementedException(); } }
        public IField[] Fields { get { return _fields.ToArray(); } }
        public IRole[] Roles { get { throw new NotImplementedException(); } }
        public IRole[] Users { get { throw new NotImplementedException(); } }
        public IItem Parent { get { return _parent; } }
        public IItem[] GetChildren() { throw new NotImplementedException(); }
        public IItem GetItem(string sItemPath) { throw new NotImplementedException(); }
        public void CopyTo(IItem CopyFrom, bool bRecursive, bool bOnlyChildren) { throw new NotImplementedException(); }
        public bool MoveTo(IItem MoveTo) { throw new NotImplementedException(); }
        public void Rename(string Name) { throw new NotImplementedException(); }
        public void Delete() { throw new NotImplementedException(); }
        public void Save() { throw new NotImplementedException(); }
        public string AddFromTemplate(string sName, string sTemplatePath) { throw new NotImplementedException(); }
        public bool HasChildren() { return _bHasChildren; }
        public string[] GetLanguages() { throw new NotImplementedException(); }
        public ConverterOptions Options { get { return _Options; } set { _Options = value; } }
        public string GetOuterXml()
        {
            if (_rawPayload == null) return "";
            var doc = new XmlDocument();
            var elem = doc.CreateElement("umbraco");
            elem.InnerText = _rawPayload.ToString(Newtonsoft.Json.Formatting.None);
            return elem.OuterXml;
        }
        public string GetHostUrl() { return _api.BaseUrl; }

        #endregion

        #region Helpers used across later tasks

        /// <summary>Map a kind to its Management API segment (`document`, `media`, ...).</summary>
        internal static string KindToSegment(Umbraco14xItemKind kind)
        {
            switch (kind)
            {
                case Umbraco14xItemKind.Content:      return "document";
                case Umbraco14xItemKind.Media:        return "media";
                case Umbraco14xItemKind.DocumentType: return "document-type";
                case Umbraco14xItemKind.DataType:     return "data-type";
                case Umbraco14xItemKind.Member:       return "member";
                case Umbraco14xItemKind.MemberGroup:  return "member-group";
                case Umbraco14xItemKind.User:         return "user";
                case Umbraco14xItemKind.UserGroup:    return "user-group";
                case Umbraco14xItemKind.Language:     return "language";
                default: throw new InvalidOperationException("No Management API segment for kind " + kind);
            }
        }

        internal static string BaseApiPath { get { return "/umbraco/management/api/v1"; } }

        #endregion
    }
}
