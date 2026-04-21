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
            PopulateFields(payload);
        }

        private void PopulateFields(JObject payload)
        {
            _fields.Clear();

            if (_kind == Umbraco14xItemKind.DocumentType)
            {
                // Field definitions come from properties[] on the document type itself.
                var props = payload["properties"] as JArray;
                if (props != null)
                {
                    foreach (var p in props.OfType<JObject>())
                        _fields.Add(new Umbraco14xField(p));
                }
                return;
            }

            if (_kind != Umbraco14xItemKind.Content && _kind != Umbraco14xItemKind.Media) return;

            // Content / Media: values[] filtered by current culture + invariant.
            var values = payload["values"] as JArray;
            if (values == null) return;

            string culture = _Options != null ? _Options.Language : "en";
            var docTypeProps = LoadDocumentTypeProperties();

            foreach (var v in values.OfType<JObject>())
            {
                var vCulture = (string)v["culture"];
                bool accept = vCulture == null
                    || string.Equals(vCulture, culture, StringComparison.OrdinalIgnoreCase)
                    || (vCulture != null && culture != null && vCulture.StartsWith(culture, StringComparison.OrdinalIgnoreCase));
                if (!accept) continue;

                string alias = (string)v["alias"];
                JObject propDef = null;
                if (alias != null && docTypeProps.TryGetValue(alias.ToLower(), out var def)) propDef = def;
                _fields.Add(new Umbraco14xField(v, propDef));
            }
        }

        /// <summary>Returns property-definitions keyed by alias (lower-cased) for this item's document type.</summary>
        private Dictionary<string, JObject> LoadDocumentTypeProperties()
        {
            var result = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);

            var docTypeRef = _rawPayload["documentType"] as JObject ?? _rawPayload["mediaType"] as JObject;
            if (docTypeRef == null) return result;
            string docTypeId = (string)docTypeRef["id"];
            if (string.IsNullOrEmpty(docTypeId)) return result;

            JObject docTypePayload;
            if (_Options != null && _Options.ExistingTemplateFields.ContainsKey(docTypeId))
            {
                docTypePayload = JObject.Parse(_Options.ExistingTemplateFields[docTypeId].OuterXml ?? "{}");
            }
            else
            {
                string segment = _kind == Umbraco14xItemKind.Media ? "media-type" : "document-type";
                docTypePayload = _api.TryGetJson(BaseApiPath + "/" + segment + "/" + docTypeId) as JObject;
                if (docTypePayload == null) return result;
                if (_Options != null)
                {
                    // Stash as XML-wrapped JSON so the cache dictionary type fits.
                    var doc = new XmlDocument();
                    doc.LoadXml("<d>" + System.Security.SecurityElement.Escape(docTypePayload.ToString(Newtonsoft.Json.Formatting.None)) + "</d>");
                    _Options.ExistingTemplateFields[docTypeId] = doc.DocumentElement;
                }
            }

            var props = docTypePayload["properties"] as JArray;
            if (props == null) return result;
            foreach (var p in props.OfType<JObject>())
            {
                string alias = (string)p["alias"];
                if (!string.IsNullOrEmpty(alias)) result[alias.ToLower()] = p;
            }
            return result;
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
        public string Name
        {
            get { return _sName; }
            set
            {
                if (_sName != value)
                {
                    _sName = value;
                    _sKey = value.ToLower();
                    _nameDirty = true;
                }
            }
        }
        public string Key { get { return _sKey; } }
        public string Path
        {
            get
            {
                if (!string.IsNullOrEmpty(_sPath)) return _sPath;
                if (_kind == Umbraco14xItemKind.Root || _kind == Umbraco14xItemKind.BranchRoot)
                    return _sPath;
                _sPath = ComputePath();
                return _sPath;
            }
        }
        public string Icon { get { return _sIcon; } set { _sIcon = value; _iconDirty = true; } }
        public string SortOrder { get { return _sSortOrder; } set { _sSortOrder = value; _sortOrderDirty = true; } }
        public IItem[] Templates
        {
            get
            {
                if (_kind == Umbraco14xItemKind.Content || _kind == Umbraco14xItemKind.Media)
                {
                    var docTypeRef = _rawPayload["documentType"] as JObject ?? _rawPayload["mediaType"] as JObject;
                    if (docTypeRef == null) return new IItem[0];
                    var items = new List<IItem>();
                    var primary = GetItemByGuid((string)docTypeRef["id"]);
                    if (primary != null) items.Add(primary);
                    var comps = docTypeRef["compositions"] as JArray;
                    if (comps != null)
                    {
                        foreach (var c in comps.OfType<JObject>())
                        {
                            var ci = GetItemByGuid((string)c["id"]);
                            if (ci != null) items.Add(ci);
                        }
                    }
                    return items.ToArray();
                }

                if (_kind == Umbraco14xItemKind.DocumentType)
                {
                    var comps = _rawPayload["compositions"] as JArray;
                    if (comps == null) return new IItem[0];
                    var items = new List<IItem>();
                    foreach (var c in comps.OfType<JObject>())
                    {
                        var ci = GetItemByGuid((string)c["id"]);
                        if (ci != null) items.Add(ci);
                    }
                    return items.ToArray();
                }

                return new IItem[0];
            }
        }
        public IItem BaseTemplate
        {
            get
            {
                if (_kind == Umbraco14xItemKind.Content || _kind == Umbraco14xItemKind.Media)
                {
                    var docTypeRef = _rawPayload["documentType"] as JObject ?? _rawPayload["mediaType"] as JObject;
                    if (docTypeRef == null) return null;
                    return GetItemByGuid((string)docTypeRef["id"]);
                }

                if (_kind == Umbraco14xItemKind.DocumentType)
                {
                    var parent = _rawPayload["parent"] as JObject;
                    if (parent != null)
                    {
                        var p = GetItemByGuid((string)parent["id"]);
                        if (p != null) return p;
                    }
                    var comps = _rawPayload["compositions"] as JArray;
                    if (comps != null && comps.Count > 0)
                        return GetItemByGuid((string)((JObject)comps[0])["id"]);
                    return null;
                }

                return null;
            }
        }
        public IField[] Fields { get { return _fields.ToArray(); } }
        public IRole[] Roles
        {
            get
            {
                if (_kind == Umbraco14xItemKind.BranchRoot && _sName == "Roles")
                {
                    var result = new List<IRole>();
                    var resp = _api.GetJson(BaseApiPath + "/user-group?skip=0&take=500") as JObject;
                    var items = resp?["items"] as JArray;
                    if (items != null)
                    {
                        foreach (var g in items.OfType<JObject>())
                        {
                            string id = (string)g["id"] ?? "";
                            string name = (string)g["name"] ?? "";
                            result.Add(new BaseRole(name, id, "/umbraco/Roles/" + name, AccessRights.NotSet));
                        }
                    }
                    return result.ToArray();
                }

                if (_kind == Umbraco14xItemKind.Content)
                {
                    var result = new List<IRole>();
                    var resp = _api.TryGetJson(BaseApiPath + "/document/" + _sID + "/permissions") as JObject;
                    var items = resp?["items"] as JArray ?? resp?["permissions"] as JArray;
                    if (items != null)
                    {
                        foreach (var p in items.OfType<JObject>())
                        {
                            string groupId = (string)(p["userGroup"]?["id"]) ?? (string)p["userGroupId"] ?? "";
                            string name = (string)(p["userGroup"]?["name"]) ?? groupId;
                            var verbs = p["verbs"] as JArray;
                            var rights = MapUmbracoVerbsToAccessRights(verbs);
                            result.Add(new BaseRole(name, groupId, "", rights));
                        }
                    }
                    return result.ToArray();
                }

                return new IRole[0];
            }
        }
        public IRole[] Users
        {
            get
            {
                if (_kind == Umbraco14xItemKind.BranchRoot && _sName == "Members")
                {
                    var result = new List<IRole>();
                    var resp = _api.GetJson(BaseApiPath + "/member?skip=0&take=500") as JObject;
                    var items = resp?["items"] as JArray;
                    if (items != null)
                    {
                        foreach (var m in items.OfType<JObject>())
                        {
                            string id = (string)m["id"] ?? "";
                            string name = (string)m["username"] ?? (string)m["name"] ?? "";
                            result.Add(new BaseRole(name, id, "/umbraco/Members/" + name, AccessRights.NotSet));
                        }
                    }
                    return result.ToArray();
                }
                return new IRole[0];
            }
        }
        public IItem Parent { get { return _parent; } }
        public IItem[] GetChildren()
        {
            if (_kind == Umbraco14xItemKind.Root)
            {
                var names = new[] { "Content", "Media", "Templates", "DataTypes", "Members", "Roles", "Languages" };
                var items = new List<IItem>();
                foreach (var name in names)
                    items.Add(new Umbraco14xItem(_api, _Options, Umbraco14xItemKind.BranchRoot, name));
                return items.ToArray();
            }

            if (_kind == Umbraco14xItemKind.BranchRoot)
            {
                var childKind = BRANCH_CHILD_KIND[_sName];
                return LoadBranchRootChildren(childKind).ToArray();
            }

            return LoadTreeChildren(_kind, _sID).ToArray();
        }
        public IItem GetItem(string sItemPath)
        {
            if (string.IsNullOrEmpty(sItemPath)) return null;

            // GUID form → direct lookup.
            if (Guid.TryParse(sItemPath, out var g))
            {
                return GetItemByGuid(g.ToString());
            }

            // Absolute path form → walk from root.
            if (sItemPath.StartsWith("/"))
                return WalkByPath(sItemPath);

            // Bare name relative to this.
            foreach (var child in GetChildren())
            {
                if (string.Equals(child.Name, sItemPath, StringComparison.OrdinalIgnoreCase))
                    return child;
            }
            return null;
        }
        public void CopyTo(IItem CopyFrom, bool bRecursive, bool bOnlyChildren)
        {
            if (_Options.ShouldItemBeCopied != null && !_Options.ShouldItemBeCopied(CopyFrom, this))
                return;

            IItem destination;
            try
            {
                if (!bOnlyChildren)
                    destination = CopyOneItem(CopyFrom, this);
                else
                    destination = this;

                if (_Options.CopyItem != null) _Options.CopyItem(CopyFrom, this, destination);
            }
            catch (Exception ex)
            {
                if (!_Options.IgnoreErrors) throw;
                System.Diagnostics.Trace.WriteLine("Umbraco14x CopyTo error on '" + CopyFrom.Name + "' (" + CopyFrom.ID + "): " + ex.Message);
                return; // cannot recurse when item creation failed
            }

            if (!bRecursive || !CopyFrom.HasChildren()) return;

            foreach (var child in CopyFrom.GetChildren())
            {
                // Each child is recursed independently so one sibling's failure doesn't skip the rest
                // (matches Sitecore6xItem.CopyChildren precedent).
                try
                {
                    ((Umbraco14xItem)destination).CopyTo(child, true, false);
                }
                catch (Exception ex)
                {
                    if (!_Options.IgnoreErrors) throw;
                    System.Diagnostics.Trace.WriteLine("Umbraco14x CopyTo child error on '" + child.Name + "' (" + child.ID + "): " + ex.Message);
                }
            }
        }

        private Umbraco14xItem CopyOneItem(IItem source, Umbraco14xItem parent)
        {
            // Try to find an existing destination item by name under this parent.
            Umbraco14xItem existing = null;
            foreach (var c in parent.GetChildren())
            {
                if (string.Equals(c.Name, source.Name, StringComparison.OrdinalIgnoreCase) && c is Umbraco14xItem uc)
                {
                    existing = uc; break;
                }
            }

            if (existing != null)
            {
                switch (_Options.CopyOperation)
                {
                    case CopyOperations.SkipExisting:
                        return existing;
                    case CopyOperations.Overwrite:
                        MergeFields(source, existing);
                        existing.Save();
                        return existing;
                    case CopyOperations.GenerateNewItemIDs:
                    case CopyOperations.UseNames:
                        // Fall through to re-create the item alongside the existing one.
                        // Umbraco will reject a duplicate name at the same parent; callers relying
                        // on these operations must ensure uniqueness upstream or use Overwrite.
                        break;
                }
            }

            // Resolve (or create) destination document type.
            string docTypeId = ResolveOrCreateDocumentTypeFor(source);
            if (string.IsNullOrEmpty(docTypeId))
                throw new InvalidOperationException("Cannot resolve document type for source '" + source.Name + "'");

            string culture = _Options.Language;
            string createSegment = parent._kind == Umbraco14xItemKind.Media ? "media" : "document";
            Umbraco14xItemKind createKind = parent._kind == Umbraco14xItemKind.Media
                ? Umbraco14xItemKind.Media : Umbraco14xItemKind.Content;

            var valuesArray = new JArray();
            foreach (var f in source.Fields)
            {
                valuesArray.Add(new JObject
                {
                    ["editorAlias"] = f.Type,
                    ["alias"] = f.Name,
                    ["culture"] = culture,
                    ["segment"] = null,
                    ["value"] = ConvertFieldContent(f)
                });
            }

            var body = new JObject
            {
                ["parent"] = parent._kind == Umbraco14xItemKind.BranchRoot
                    ? (JToken)null
                    : new JObject { ["id"] = parent.ID },
                ["values"] = valuesArray,
                ["variants"] = new JArray(new JObject
                {
                    ["culture"] = culture,
                    ["segment"] = null,
                    ["name"] = source.Name
                })
            };
            if (createKind == Umbraco14xItemKind.Media)
                body["mediaType"] = new JObject { ["id"] = docTypeId };
            else
                body["documentType"] = new JObject { ["id"] = docTypeId };

            var resp = _api.PostJson(BaseApiPath + "/" + createSegment, body) as JObject;
            string newId = (string)resp?["id"] ?? (string)(resp?["entity"]?["id"]);
            if (string.IsNullOrEmpty(newId))
                throw new InvalidOperationException("Create returned no id for '" + source.Name + "'");

            var newPayload = _api.GetJson(BaseApiPath + "/" + createSegment + "/" + newId) as JObject;
            return new Umbraco14xItem(_api, _Options, createKind, newPayload, parent);
        }

        private JToken ConvertFieldContent(IField f)
        {
            var s = f.Content ?? "";
            if (string.IsNullOrEmpty(s)) return null;
            if (s.StartsWith("{") || s.StartsWith("["))
            {
                try { return JToken.Parse(s); } catch { /* ignore */ }
            }
            return new JValue(s);
        }

        private void MergeFields(IItem source, Umbraco14xItem destination)
        {
            foreach (var srcField in source.Fields)
            {
                var dst = destination._fields.FirstOrDefault(f =>
                    string.Equals(f.Name, srcField.Name, StringComparison.OrdinalIgnoreCase));
                if (dst != null)
                {
                    dst.Content = srcField.Content; // setter flips dirty
                }
                else
                {
                    var newField = new Umbraco14xField(new JObject
                    {
                        ["alias"] = srcField.Name,
                        ["culture"] = destination._Options.Language,
                        ["segment"] = null,
                        ["value"] = srcField.Content
                    }, null);
                    newField.IsDirty = true;
                    destination._fields.Add(newField);
                }
            }
        }
        public bool MoveTo(IItem MoveToItem)
        {
            if (_kind != Umbraco14xItemKind.Content && _kind != Umbraco14xItemKind.Media
                && _kind != Umbraco14xItemKind.DocumentType && _kind != Umbraco14xItemKind.DataType)
                throw new NotImplementedException("MoveTo is only supported for Content/Media/DocumentType/DataType items");

            string segment = KindToSegment(_kind);
            var body = new JObject
            {
                ["target"] = new JObject { ["id"] = MoveToItem.ID }
            };
            _api.PutJson(BaseApiPath + "/" + segment + "/" + _sID + "/move", body);

            _parent = MoveToItem as Umbraco14xItem;
            _sPath = ""; // force recompute on next access
            return true;
        }
        public void Rename(string Name)
        {
            if (_rawPayload == null) throw new InvalidOperationException("Cannot rename a synthetic item");

            var updated = (JObject)_rawPayload.DeepClone();
            var variants = updated["variants"] as JArray;
            string culture = _Options != null ? _Options.Language : "en";

            if (variants != null && variants.Count > 0)
            {
                bool applied = false;
                foreach (var v in variants.OfType<JObject>())
                {
                    var vc = (string)v["culture"];
                    if (vc == null || string.Equals(vc, culture, StringComparison.OrdinalIgnoreCase)
                        || (vc != null && culture != null && vc.StartsWith(culture, StringComparison.OrdinalIgnoreCase)))
                    {
                        v["name"] = Name;
                        applied = true;
                        break;
                    }
                }
                if (!applied) ((JObject)variants[0])["name"] = Name;
            }
            else
            {
                updated["name"] = Name;
            }

            string segment = KindToSegment(_kind);
            _api.PutJson(BaseApiPath + "/" + segment + "/" + _sID, updated);

            _sName = Name;
            _sKey = Name.ToLower();
            _rawPayload = updated;
        }
        public void Delete()
        {
            if (_kind == Umbraco14xItemKind.Content || _kind == Umbraco14xItemKind.Media)
            {
                string segment = KindToSegment(_kind);
                try
                {
                    // Two-step hard delete: move to recycle bin, then purge from the bin.
                    _api.PutJson(BaseApiPath + "/" + segment + "/" + _sID + "/move-to-recycle-bin", new JObject());
                    _api.DeleteJson(BaseApiPath + "/" + segment + "-recycle-bin/" + _sID);
                    return;
                }
                catch (Umbraco14xApiException)
                {
                    // Fall back to direct delete (sometimes permitted on items already in the bin or for hard-delete-enabled builds).
                    _api.DeleteJson(BaseApiPath + "/" + segment + "/" + _sID);
                    return;
                }
            }

            if (_kind == Umbraco14xItemKind.DocumentType || _kind == Umbraco14xItemKind.DataType
                || _kind == Umbraco14xItemKind.Member || _kind == Umbraco14xItemKind.UserGroup
                || _kind == Umbraco14xItemKind.Language)
            {
                string segment = KindToSegment(_kind);
                _api.DeleteJson(BaseApiPath + "/" + segment + "/" + _sID);
                return;
            }

            throw new NotImplementedException("Delete is not supported for kind " + _kind);
        }
        public void Save()
        {
            if (_kind != Umbraco14xItemKind.Content && _kind != Umbraco14xItemKind.Media
                && _kind != Umbraco14xItemKind.DocumentType)
                throw new NotImplementedException("Save is only supported for Content, Media, DocumentType");
            if (_rawPayload == null) throw new InvalidOperationException("Cannot save a synthetic item");

            var dirtyFields = _fields.Where(f => f.IsDirty).ToList();
            if (dirtyFields.Count == 0 && !_nameDirty && !_iconDirty && !_sortOrderDirty)
                return; // nothing to persist

            var updated = (JObject)_rawPayload.DeepClone();

            // Item-level edits
            if (_iconDirty) updated["icon"] = _sIcon;
            if (_sortOrderDirty) updated["sortOrder"] = int.TryParse(_sSortOrder, out var so) ? (JToken)so : _sSortOrder;

            // Name changes (content/media) go through variants[]
            if (_nameDirty)
            {
                var variants = updated["variants"] as JArray;
                string culture = _Options != null ? _Options.Language : "en";
                if (variants != null && variants.Count > 0)
                {
                    var target = (JObject)variants.OfType<JObject>()
                        .FirstOrDefault(v => (string)v["culture"] == null
                            || string.Equals((string)v["culture"], culture, StringComparison.OrdinalIgnoreCase));
                    if (target == null)
                    {
                        target = new JObject { ["culture"] = culture, ["segment"] = null, ["name"] = _sName };
                        variants.Add(target);
                    }
                    else
                    {
                        target["name"] = _sName;
                    }
                }
                else
                {
                    updated["name"] = _sName;
                }
            }

            // Field-level edits
            var values = updated["values"] as JArray;
            if (values == null) { values = new JArray(); updated["values"] = values; }

            foreach (var f in dirtyFields)
            {
                var entry = f.ToValuesEntry();
                // Upsert by (alias, culture, segment).
                var existing = values.OfType<JObject>().FirstOrDefault(v =>
                    string.Equals((string)v["alias"], (string)entry["alias"], StringComparison.OrdinalIgnoreCase) &&
                    (string)v["culture"] == (string)entry["culture"] &&
                    (string)v["segment"] == (string)entry["segment"]);
                if (existing != null)
                {
                    existing["value"] = entry["value"];
                    existing["editorAlias"] = entry["editorAlias"];
                }
                else
                {
                    values.Add(entry);
                }
            }

            string segment = KindToSegment(_kind);
            _api.PutJson(BaseApiPath + "/" + segment + "/" + _sID, updated);

            // Reset dirty flags
            foreach (var f in dirtyFields) f.IsDirty = false;
            _nameDirty = _iconDirty = _sortOrderDirty = false;
            _rawPayload = updated;
        }
        public string AddFromTemplate(string sName, string sTemplatePath)
        {
            if (_kind != Umbraco14xItemKind.Content && _kind != Umbraco14xItemKind.Media
                && _kind != Umbraco14xItemKind.BranchRoot)
                throw new NotImplementedException("AddFromTemplate is only supported under Content/Media or at their branch root");

            // Resolve sTemplatePath to a document-type GUID.
            string docTypeId = ResolveDocumentTypeId(sTemplatePath);
            if (string.IsNullOrEmpty(docTypeId))
                throw new InvalidOperationException("Could not resolve document type at path '" + sTemplatePath + "'");

            // Decide target kind + endpoint.
            Umbraco14xItemKind childKind;
            string segment;
            if (_kind == Umbraco14xItemKind.Media || (_kind == Umbraco14xItemKind.BranchRoot && _sName == "Media"))
            {
                childKind = Umbraco14xItemKind.Media;
                segment = "media";
            }
            else
            {
                childKind = Umbraco14xItemKind.Content;
                segment = "document";
            }

            string culture = _Options != null ? _Options.Language : "en";
            var body = new JObject
            {
                ["parent"] = (_kind == Umbraco14xItemKind.BranchRoot)
                    ? (JToken)null
                    : new JObject { ["id"] = _sID },
                ["documentType"] = new JObject { ["id"] = docTypeId },
                ["values"] = new JArray(),
                ["variants"] = new JArray(new JObject
                {
                    ["culture"] = culture,
                    ["segment"] = null,
                    ["name"] = sName
                })
            };
            // Media payloads use mediaType instead of documentType.
            if (childKind == Umbraco14xItemKind.Media)
            {
                body.Remove("documentType");
                body["mediaType"] = new JObject { ["id"] = docTypeId };
            }

            var resp = _api.PostJson(BaseApiPath + "/" + segment, body) as JObject;
            // Umbraco returns the new entity; id may be in top-level "id" or in a Location header (not parsed here).
            string newId = (string)resp?["id"];
            if (string.IsNullOrEmpty(newId))
            {
                // Fallback: the response might be the full entity with id nested.
                newId = (string)(resp?["entity"]?["id"]);
            }
            return newId ?? "";
        }
        public bool HasChildren() { return _bHasChildren; }
        public string[] GetLanguages()
        {
            var langs = _api.GetLanguages();
            var result = new List<string>();
            foreach (var l in langs.OfType<JObject>())
            {
                var code = (string)l["isoCode"] ?? (string)l["cultureName"];
                if (!string.IsNullOrEmpty(code)) result.Add(code);
            }
            return result.ToArray();
        }
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

        private List<IItem> LoadBranchRootChildren(Umbraco14xItemKind childKind)
        {
            string segment = KindToSegment(childKind);

            // Content / Media / DocumentType / DataType use the tree/{segment}/root paginated endpoint.
            // Member / UserGroup / Language list directly under the collection endpoint.
            switch (childKind)
            {
                case Umbraco14xItemKind.Content:
                case Umbraco14xItemKind.Media:
                case Umbraco14xItemKind.DocumentType:
                case Umbraco14xItemKind.DataType:
                    return LoadTreeChildrenRaw(segment, null, childKind);
                case Umbraco14xItemKind.Member:
                    return LoadCollection("/member?skip=0&take=500", childKind);
                case Umbraco14xItemKind.UserGroup:
                    return LoadCollection("/user-group?skip=0&take=500", childKind);
                case Umbraco14xItemKind.Language:
                    return LoadCollection("/language?skip=0&take=500", childKind);
                default:
                    return new List<IItem>();
            }
        }

        private List<IItem> LoadTreeChildren(Umbraco14xItemKind childKind, string parentId)
        {
            string segment = KindToSegment(childKind);
            return LoadTreeChildrenRaw(segment, parentId, childKind);
        }

        private List<IItem> LoadTreeChildrenRaw(string segment, string parentId, Umbraco14xItemKind childKind)
        {
            var result = new List<IItem>();
            int skip = 0;
            int take = 500;
            while (true)
            {
                string path = parentId == null
                    ? BaseApiPath + "/tree/" + segment + "/root?skip=" + skip + "&take=" + take
                    : BaseApiPath + "/tree/" + segment + "/children?parentId=" + parentId + "&skip=" + skip + "&take=" + take;
                var resp = (JObject)_api.GetJson(path);
                var items = resp["items"] as JArray ?? new JArray();
                foreach (var entry in items.OfType<JObject>())
                {
                    var detail = LoadItemPayload(childKind, (string)entry["id"]);
                    // Tree endpoints return shallow payloads; merge hasChildren from the tree entry.
                    if (detail["hasChildren"] == null && entry["hasChildren"] != null)
                        detail["hasChildren"] = entry["hasChildren"];
                    result.Add(new Umbraco14xItem(_api, _Options, childKind, detail, this));
                }
                int total = (int?)resp["total"] ?? items.Count;
                skip += items.Count;
                if (items.Count == 0 || skip >= total) break;
            }
            return result;
        }

        private List<IItem> LoadCollection(string relativePath, Umbraco14xItemKind childKind)
        {
            var result = new List<IItem>();
            var resp = (JObject)_api.GetJson(BaseApiPath + relativePath);
            var items = resp["items"] as JArray ?? new JArray();
            foreach (var entry in items.OfType<JObject>())
            {
                result.Add(new Umbraco14xItem(_api, _Options, childKind, entry, this));
            }
            return result;
        }

        private JObject LoadItemPayload(Umbraco14xItemKind kind, string id)
        {
            string segment = KindToSegment(kind);
            var detail = (JObject)_api.GetJson(BaseApiPath + "/" + segment + "/" + id);
            return detail;
        }

        private string ComputePath()
        {
            // Cache check
            if (_Options != null && _Options.ExistingTemplates.ContainsKey("path:" + _sID))
            {
                var cached = _Options.ExistingTemplates["path:" + _sID];
                return cached.Path;
            }

            // Walk ancestors via the tree endpoint.
            string branchName = BranchNameForKind(_kind);
            string segment = KindToSegment(_kind);
            var path = "/umbraco/" + branchName;

            try
            {
                var ancestorsResp = _api.TryGetJson(BaseApiPath + "/tree/" + segment + "/ancestors?descendantId=" + _sID);
                if (ancestorsResp is JArray arr)
                {
                    foreach (var a in arr.OfType<JObject>())
                        path += "/" + (string)a["name"];
                }
                else if (ancestorsResp is JObject obj && obj["items"] is JArray arr2)
                {
                    foreach (var a in arr2.OfType<JObject>())
                        path += "/" + (string)a["name"];
                }
            }
            catch { /* ancestor walk is best-effort; fall back to parent-less path */ }

            path += "/" + _sName;
            return path;
        }

        private static string BranchNameForKind(Umbraco14xItemKind kind)
        {
            switch (kind)
            {
                case Umbraco14xItemKind.Content:      return "Content";
                case Umbraco14xItemKind.Media:        return "Media";
                case Umbraco14xItemKind.DocumentType: return "Templates";
                case Umbraco14xItemKind.DataType:     return "DataTypes";
                case Umbraco14xItemKind.Member:       return "Members";
                case Umbraco14xItemKind.MemberGroup:  return "Members";
                case Umbraco14xItemKind.User:         return "Members";
                case Umbraco14xItemKind.UserGroup:    return "Roles";
                case Umbraco14xItemKind.Language:     return "Languages";
                default: return "";
            }
        }

        private IItem GetItemByGuid(string guid)
        {
            if (_Options != null && _Options.ExistingTemplates.ContainsKey(guid))
                return _Options.ExistingTemplates[guid];

            // Try the kinds in order: DocumentType most commonly queried by GUID first, then Content.
            foreach (var kind in new[] {
                Umbraco14xItemKind.DocumentType, Umbraco14xItemKind.Content,
                Umbraco14xItemKind.Media, Umbraco14xItemKind.DataType,
                Umbraco14xItemKind.Member, Umbraco14xItemKind.UserGroup })
            {
                string segment = KindToSegment(kind);
                var payload = _api.TryGetJson(BaseApiPath + "/" + segment + "/" + guid) as JObject;
                if (payload != null)
                {
                    var item = new Umbraco14xItem(_api, _Options, kind, payload, null);
                    if (_Options != null) _Options.ExistingTemplates[guid] = item;
                    return item;
                }
            }
            return null;
        }

        private IItem WalkByPath(string path)
        {
            if (_Options != null && _Options.ExistingTemplates.ContainsKey(path))
                return _Options.ExistingTemplates[path];

            var segments = path.Trim('/').Split('/');
            IItem current = GetRoot(_api, _Options);
            foreach (var seg in segments)
            {
                if (string.Equals(seg, "umbraco", StringComparison.OrdinalIgnoreCase)) continue;
                IItem next = null;
                foreach (var child in current.GetChildren())
                {
                    if (string.Equals(child.Name, seg, StringComparison.OrdinalIgnoreCase))
                    {
                        next = child; break;
                    }
                }
                if (next == null) return null;
                current = next;
            }
            if (_Options != null && current is Umbraco14xItem) _Options.ExistingTemplates[path] = current;
            return current;
        }

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

        private string ResolveDocumentTypeId(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath)) return "";

            // GUID form
            if (Guid.TryParse(templatePath, out var g)) return g.ToString();

            // Cache check
            if (_Options != null && _Options.ExistingTemplates.ContainsKey(templatePath))
                return _Options.ExistingTemplates[templatePath].ID;

            // Alias form (bare name, no slashes): hit /document-type/by-alias/{alias}
            if (!templatePath.Contains("/"))
            {
                var byAlias = _api.TryGetJson(BaseApiPath + "/document-type/by-alias/" + Uri.EscapeDataString(templatePath)) as JObject;
                if (byAlias != null) return (string)byAlias["id"];
            }

            // Path form: walk from root via GetItem
            var item = GetRoot(_api, _Options).GetItem(templatePath);
            return item?.ID ?? "";
        }

        private static AccessRights MapUmbracoVerbsToAccessRights(JArray verbs)
        {
            var rights = AccessRights.NotSet;
            if (verbs == null) return rights;
            foreach (var v in verbs)
            {
                var s = ((string)v) ?? "";
                if (s.IndexOf("browse", StringComparison.OrdinalIgnoreCase) >= 0) rights |= AccessRights.Read;
                if (s.IndexOf("update", StringComparison.OrdinalIgnoreCase) >= 0) rights |= AccessRights.Write;
                if (s.IndexOf("create", StringComparison.OrdinalIgnoreCase) >= 0) rights |= AccessRights.Create;
                if (s.IndexOf("delete", StringComparison.OrdinalIgnoreCase) >= 0) rights |= AccessRights.Delete;
                if (s.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0) rights |= AccessRights.Administer;
            }
            return rights;
        }

        private string ResolveOrCreateDocumentTypeFor(IItem source)
        {
            // Use the source's BaseTemplate name as the Umbraco document-type alias.
            var baseTpl = source.BaseTemplate;
            string desiredAlias = baseTpl != null ? SanitizeAlias(baseTpl.Name) : "defaultDocument";

            // Cache
            if (_Options.ExistingTemplates.ContainsKey("alias:" + desiredAlias))
                return _Options.ExistingTemplates["alias:" + desiredAlias].ID;

            // Lookup
            var existing = _api.TryGetJson(BaseApiPath + "/document-type/by-alias/" + Uri.EscapeDataString(desiredAlias)) as JObject;
            if (existing != null)
            {
                string id = (string)existing["id"];
                if (!string.IsNullOrEmpty(id))
                {
                    var cached = new Umbraco14xItem(_api, _Options, Umbraco14xItemKind.DocumentType, existing, null);
                    _Options.ExistingTemplates["alias:" + desiredAlias] = cached;
                    return id;
                }
            }

            if (!_Options.CopyTemplates) return "";

            // Create a minimal document type matching the source template's fields.
            var propsArray = new JArray();
            if (baseTpl != null)
            {
                foreach (var f in baseTpl.Fields)
                {
                    propsArray.Add(new JObject
                    {
                        ["alias"] = SanitizeAlias(f.Name),
                        ["name"] = f.Name,
                        ["dataType"] = new JObject { ["id"] = KnownDataTypeIdForEditor(f.Type) },
                        ["variesByCulture"] = true
                    });
                }
            }
            var createBody = new JObject
            {
                ["alias"] = desiredAlias,
                ["name"] = baseTpl != null ? baseTpl.Name : "Default",
                ["icon"] = "icon-document",
                ["allowedAsRoot"] = true,
                ["variesByCulture"] = true,
                ["properties"] = propsArray,
                ["compositions"] = new JArray()
            };
            var createdResp = _api.PostJson(BaseApiPath + "/document-type", createBody) as JObject;
            string newId = (string)createdResp?["id"] ?? (string)(createdResp?["entity"]?["id"]);
            return newId ?? "";
        }

        private static string SanitizeAlias(string name)
        {
            if (string.IsNullOrEmpty(name)) return "alias";
            var sb = new StringBuilder();
            bool upperNext = false;
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(upperNext ? char.ToUpper(ch) : (sb.Length == 0 ? char.ToLower(ch) : ch));
                    upperNext = false;
                }
                else upperNext = true;
            }
            return sb.Length == 0 ? "alias" : sb.ToString();
        }

        /// <summary>Well-known Umbraco data-type IDs. Values can be overridden on the installation;
        /// these match Umbraco's default seed data for v14+.</summary>
        private static string KnownDataTypeIdForEditor(string editorAlias)
        {
            switch (editorAlias)
            {
                case "Umbraco.RichText":       return "ca90c950-0aff-4e72-b976-a30b1ac57dad"; // Richtext editor
                case "Umbraco.TextBox":        return "0cc0eba1-9960-42c9-bf9b-60e150b429ae"; // Textstring
                case "Umbraco.TextArea":       return "c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3"; // Textarea
                case "Umbraco.Integer":        return "2e6d3631-066e-44b8-aec4-96f09099b2b5"; // Numeric
                case "Umbraco.TrueFalse":      return "92897bc6-a5f3-4ffe-ae27-f2e7e33dda49"; // True/false
                case "Umbraco.DateTime":       return "e4d66c0f-b935-4200-81f0-025f7256b89a"; // Date/time
                case "Umbraco.MediaPicker3":   return "135d60e0-2dd4-4f27-9b2b-e8f9ad3b8b8a"; // Media Picker 3
                default:                        return "0cc0eba1-9960-42c9-bf9b-60e150b429ae"; // Textstring fallback
            }
        }

        #endregion
    }
}
