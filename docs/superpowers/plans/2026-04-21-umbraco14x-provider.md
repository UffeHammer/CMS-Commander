# Umbraco14x Provider Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `Umbraco14xAPI`, `Umbraco14xItem`, `Umbraco14xField` — a full-parity REST provider for Umbraco 14+ Management API that plugs into CMS-commander's existing `IItem`/`IField` abstraction.

**Architecture:** Three new files in `Source/Core/`. `Umbraco14xAPI` wraps `HttpClient` with OAuth2 bearer-token auth and synchronous JSON helpers. `Umbraco14xItem` implements `IItem` with an internal `ItemKind` dispatch so one class handles the synthetic unified tree (Content / Media / Templates / DataTypes / Members / Roles / Languages) over separate Management API endpoints. `Umbraco14xField` implements `IField` with dirty-tracking for targeted PUTs. Adds `Newtonsoft.Json` as the project's first NuGet dependency. Adds a `Umbraco14xSmokeTest` console project for manual validation against a live instance.

**Tech Stack:** .NET Framework 4.8, C#, classic (non-SDK) MSBuild csproj, `System.Net.Http.HttpClient`, `Newtonsoft.Json`.

**Reference spec:** [`docs/superpowers/specs/2026-04-21-umbraco14x-provider-design.md`](../specs/2026-04-21-umbraco14x-provider-design.md)

**Reference implementation for parity:** [`Source/Core/Sitecore6xItem.cs`](../../../Source/Core/Sitecore6xItem.cs) and [`Source/Core/Sitecore6xField.cs`](../../../Source/Core/Sitecore6xField.cs).

---

## Notes for the implementer

- **No unit tests.** The existing `Core` project has zero automated tests — this is the established pattern. Validation is: (1) the project compiles after every task, (2) the smoke-test harness added in Task 14 exercises the round-trip end-to-end against a live Umbraco 14+ instance.
- **Synchronous public API only.** The rest of the codebase is sync; use `.GetAwaiter().GetResult()` internally.
- **Classic MSBuild csproj format.** Do not convert to SDK-style. Register new files in `<ItemGroup>` under `<Compile Include="…"/>`.
- **One concept per file.** `Umbraco14xAPI` / `Umbraco14xItem` / `Umbraco14xField` — never mix their responsibilities.
- **Follow `Sitecore6xItem.cs` patterns** for lazy loading, caching via `_Options.ExistingTemplates` / `_Options.ExistingTemplateFields`, and error propagation via `ConverterOptions.IgnoreErrors`.
- **Build command.** From `c:/Projects/CMS-commander/Source/`, run:
  ```
  msbuild SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
  ```
  If `msbuild` isn't on PATH, use the Visual Studio Developer Command Prompt or invoke `"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"` directly.
- **Commit at every task boundary** with an imperative conventional message (`feat:`, `chore:`, `fix:`).

---

## Task 1: Add Newtonsoft.Json to the Core project

**Files:**
- Create: `Source/Core/packages.config`
- Modify: `Source/Core/SitecoreConverter.Core.csproj`
- Create: `Source/packages/` (will be populated by `nuget restore`; may not need manual creation)

- [ ] **Step 1: Create `packages.config`**

Create file `Source/Core/packages.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net48" />
</packages>
```

- [ ] **Step 2: Add reference and file to csproj**

In `Source/Core/SitecoreConverter.Core.csproj`:

Add inside the existing `<ItemGroup>` containing `<Reference Include="System" />` (around line 61):

```xml
<Reference Include="Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL">
  <HintPath>..\packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll</HintPath>
  <Private>True</Private>
</Reference>
<Reference Include="System.Net.Http" />
```

Add to the `<ItemGroup>` with other `<None Include>` entries (around line 277, near `app.config`):

```xml
<None Include="packages.config" />
```

Add to the bottom of the csproj, just above the closing `</Project>` tag:

```xml
<Import Project="..\packages\Newtonsoft.Json.13.0.3\build\Newtonsoft.Json.targets" Condition="Exists('..\packages\Newtonsoft.Json.13.0.3\build\Newtonsoft.Json.targets')" />
<Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
  <PropertyGroup>
    <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them. For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}.</ErrorText>
  </PropertyGroup>
  <Error Condition="!Exists('..\packages\Newtonsoft.Json.13.0.3\build\Newtonsoft.Json.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Newtonsoft.Json.13.0.3\build\Newtonsoft.Json.targets'))" />
</Target>
```

- [ ] **Step 3: Restore and build**

From `c:/Projects/CMS-commander/Source/`:

```
nuget restore SitecoreConverter.sln
msbuild SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds, `Source/packages/Newtonsoft.Json.13.0.3/` appears on disk.

If `nuget.exe` isn't available, download it to `Source/nuget.exe` from https://dist.nuget.org/win-x86-commandline/latest/nuget.exe and invoke `.\nuget.exe restore SitecoreConverter.sln`.

- [ ] **Step 4: Commit**

```bash
git add Source/Core/packages.config Source/Core/SitecoreConverter.Core.csproj
git commit -m "chore: add Newtonsoft.Json 13.0.3 to Core project"
```

---

## Task 2: `Umbraco14xAPI` — transport layer

**Files:**
- Create: `Source/Core/Umbraco14xAPI.cs`
- Modify: `Source/Core/SitecoreConverter.Core.csproj` (register file)

- [ ] **Step 1: Create the file**

Create `Source/Core/Umbraco14xAPI.cs` with the following complete content:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SitecoreConverter.Core
{
    public class Umbraco14xApiException : Exception
    {
        public int StatusCode { get; }
        public string Path { get; }
        public string ResponseBody { get; }

        public Umbraco14xApiException(int statusCode, string path, string responseBody)
            : base("Umbraco Management API " + statusCode + " at " + path + ": " + responseBody)
        {
            StatusCode = statusCode;
            Path = path;
            ResponseBody = responseBody;
        }
    }

    public class Umbraco14xAPI
    {
        private readonly string _baseUrl;
        private readonly Credentials _credentials;
        private readonly HttpClient _http;
        private readonly object _tokenLock = new object();

        private string _bearerToken = null;
        private DateTime _bearerExpiresUtc = DateTime.MinValue;
        private JArray _cachedLanguages = null;

        public string BaseUrl { get { return _baseUrl; } }
        public Credentials Credentials { get { return _credentials; } }

        public Umbraco14xAPI(string sUrl, Credentials credentials)
        {
            _baseUrl = sUrl.TrimEnd('/');
            _credentials = credentials;

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            _http = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public JToken GetJson(string path)
        {
            return Send(HttpMethod.Get, path, null);
        }

        public JToken TryGetJson(string path)
        {
            try { return Send(HttpMethod.Get, path, null); }
            catch (Umbraco14xApiException ex) when (ex.StatusCode == 404) { return null; }
        }

        public JToken PostJson(string path, JToken body)
        {
            return Send(HttpMethod.Post, path, body);
        }

        public JToken PutJson(string path, JToken body)
        {
            return Send(HttpMethod.Put, path, body);
        }

        public void DeleteJson(string path)
        {
            Send(HttpMethod.Delete, path, null);
        }

        public JArray GetLanguages()
        {
            if (_cachedLanguages != null) return _cachedLanguages;
            var result = GetJson("/umbraco/management/api/v1/language?skip=0&take=1000");
            _cachedLanguages = (JArray)result["items"] ?? new JArray();
            return _cachedLanguages;
        }

        private JToken Send(HttpMethod method, string path, JToken body)
        {
            EnsureToken();

            var req = BuildRequest(method, path, body);
            HttpResponseMessage resp = _http.SendAsync(req).GetAwaiter().GetResult();

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token may have expired between requests; force refresh and retry once.
                lock (_tokenLock) { _bearerToken = null; }
                EnsureToken();
                req = BuildRequest(method, path, body);
                resp = _http.SendAsync(req).GetAwaiter().GetResult();
            }

            string respBody = resp.Content != null
                ? resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                : "";

            if (!resp.IsSuccessStatusCode)
            {
                throw new Umbraco14xApiException((int)resp.StatusCode, path, respBody);
            }

            if (string.IsNullOrEmpty(respBody)) return null;
            return JToken.Parse(respBody);
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string path, JToken body)
        {
            var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? path
                : _baseUrl + path;
            var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            if (body != null)
            {
                req.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
            }
            return req;
        }

        private void EnsureToken()
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_bearerToken) && DateTime.UtcNow < _bearerExpiresUtc.AddSeconds(-30))
                    return;
                AcquireTokenLocked();
            }
        }

        private void AcquireTokenLocked()
        {
            // OAuth2 password grant against the Management API back-office token endpoint.
            // Exact endpoint path is stable across Umbraco 14+: /umbraco/management/api/v1/security/back-office/token
            var tokenUrl = _baseUrl + "/umbraco/management/api/v1/security/back-office/token";

            var form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", "umbraco-back-office"),
                new KeyValuePair<string, string>("username", _credentials.UserName ?? ""),
                new KeyValuePair<string, string>("password", _credentials.Password ?? "")
            };
            var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(form)
            };

            var resp = _http.SendAsync(req).GetAwaiter().GetResult();
            var respBody = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
            {
                throw new Umbraco14xApiException((int)resp.StatusCode, tokenUrl, respBody);
            }

            var json = JObject.Parse(respBody);
            _bearerToken = (string)json["access_token"];
            int expiresIn = (int?)json["expires_in"] ?? 3600;
            _bearerExpiresUtc = DateTime.UtcNow.AddSeconds(expiresIn);

            if (string.IsNullOrEmpty(_bearerToken))
                throw new Umbraco14xApiException(500, tokenUrl, "No access_token in response: " + respBody);
        }
    }
}
```

- [ ] **Step 2: Register the file in the csproj**

In `Source/Core/SitecoreConverter.Core.csproj`, add to the `<ItemGroup>` containing `<Compile Include>` entries (near line 123, after `Sitecore6xItem.cs`):

```xml
<Compile Include="Umbraco14xAPI.cs" />
```

- [ ] **Step 3: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds, no warnings on the new file.

- [ ] **Step 4: Commit**

```bash
git add Source/Core/Umbraco14xAPI.cs Source/Core/SitecoreConverter.Core.csproj
git commit -m "feat: add Umbraco14xAPI transport layer with OAuth2 bearer auth"
```

---

## Task 3: `Umbraco14xField`

**Files:**
- Create: `Source/Core/Umbraco14xField.cs`
- Modify: `Source/Core/SitecoreConverter.Core.csproj`

- [ ] **Step 1: Create the file**

Create `Source/Core/Umbraco14xField.cs`:

```csharp
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
```

- [ ] **Step 2: Register in csproj**

Add to `Source/Core/SitecoreConverter.Core.csproj`, near the other `Umbraco` entries:

```xml
<Compile Include="Umbraco14xField.cs" />
```

- [ ] **Step 3: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 4: Commit**

```bash
git add Source/Core/Umbraco14xField.cs Source/Core/SitecoreConverter.Core.csproj
git commit -m "feat: add Umbraco14xField with dirty tracking and culture awareness"
```

---

## Task 4: `Umbraco14xItem` skeleton + `ItemKind` + synthetic roots

**Files:**
- Create: `Source/Core/Umbraco14xItem.cs`
- Modify: `Source/Core/SitecoreConverter.Core.csproj`

- [ ] **Step 1: Create skeleton**

Create `Source/Core/Umbraco14xItem.cs` with the skeleton. All `IItem` members throw `NotImplementedException` at first — later tasks fill them in.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public string GetOuterXml() { return _rawPayload == null ? "" : "<umbraco>" + _rawPayload.ToString(Formatting.None) + "</umbraco>"; }
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
```

- [ ] **Step 2: Register in csproj**

Add to `Source/Core/SitecoreConverter.Core.csproj`:

```xml
<Compile Include="Umbraco14xItem.cs" />
```

- [ ] **Step 3: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds; skeleton-only item compiles.

- [ ] **Step 4: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs Source/Core/SitecoreConverter.Core.csproj
git commit -m "feat: add Umbraco14xItem skeleton with ItemKind dispatch and synthetic roots"
```

---

## Task 5: `GetChildren` + `GetItem` — the tree walker

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

- [ ] **Step 1: Implement `GetChildren`**

Replace the `GetChildren()` stub with this implementation. Add near the `#region IItem` region:

```csharp
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

private List<IItem> LoadBranchRootChildren(Umbraco14xItemKind childKind)
{
    string segment = KindToSegment(childKind);

    // Content / Media / DocumentType / DataType / Member use the tree/{segment}/root paginated endpoint.
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
```

- [ ] **Step 2: Implement `Path` via ancestors**

Replace the read-only `Path` property with an implementation that lazy-computes on first access:

```csharp
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
```

- [ ] **Step 3: Implement `GetItem`**

Replace the `GetItem` stub:

```csharp
public IItem GetItem(string sItemPath)
{
    if (string.IsNullOrEmpty(sItemPath)) return null;

    // GUID form → direct lookup across candidate segments.
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
```

- [ ] **Step 4: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem tree traversal via GetChildren / GetItem / Path"
```

---

## Task 6: Load fields + `Templates` / `BaseTemplate`

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

- [ ] **Step 1: Extend `LoadFromPayload` to populate `_fields`**

Append at the end of `LoadFromPayload` (just before the method closes):

```csharp
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
            doc.LoadXml("<d>" + System.Security.SecurityElement.Escape(docTypePayload.ToString(Formatting.None)) + "</d>");
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
```

Note: the `ExistingTemplateFields` cache is typed as `Dictionary<string, XmlNode>` in `ConverterOptions.cs` — we wrap the JSON payload in an escaped XML element for storage and parse it out again, because widening the cache type is out of scope.

- [ ] **Step 2: Implement `Templates`**

Replace the `Templates` stub:

```csharp
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
```

- [ ] **Step 3: Implement `BaseTemplate`**

Replace the `BaseTemplate` stub:

```csharp
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
```

- [ ] **Step 4: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem field loading, Templates, BaseTemplate"
```

---

## Task 7: `GetLanguages`, `Roles`, `Users`

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

- [ ] **Step 1: Implement `GetLanguages`**

Replace the `GetLanguages` stub:

```csharp
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
```

- [ ] **Step 2: Implement `Roles`**

Replace the `Roles` stub. Returns `BaseRole[]` cast as `IRole[]`. Permission translation is basic — `Read`/`Write` only; Umbraco permission verbs map loosely onto `AccessRights`.

```csharp
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
```

- [ ] **Step 3: Implement `Users`**

Replace the `Users` stub:

```csharp
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
```

- [ ] **Step 4: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem languages, roles and users"
```

---

## Task 8: `MoveTo`, `Rename`, `Delete`

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

- [ ] **Step 1: Implement `MoveTo`**

Replace the `MoveTo` stub:

```csharp
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
```

- [ ] **Step 2: Implement `Rename`**

Replace the `Rename` stub:

```csharp
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
```

- [ ] **Step 3: Implement `Delete`**

Replace the `Delete` stub. Hard-delete semantics match `Sitecore6xItem.Delete()`.

```csharp
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
```

- [ ] **Step 4: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem MoveTo, Rename, Delete"
```

---

## Task 9: `Save` with dirty tracking

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

- [ ] **Step 1: Implement `Save`**

Replace the `Save` stub:

```csharp
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
```

- [ ] **Step 2: Wire `_nameDirty` into the `Name` setter**

Replace the `Name` property with a get/set pair:

```csharp
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
```

- [ ] **Step 3: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 4: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem Save with field + item dirty tracking"
```

---

## Task 10: `AddFromTemplate`

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

- [ ] **Step 1: Implement `AddFromTemplate`**

Replace the `AddFromTemplate` stub:

```csharp
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
```

- [ ] **Step 2: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem AddFromTemplate"
```

---

## Task 11: `CopyTo` — recursive cross-CMS copy

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

This task is the biggest one. It implements the same contract as `Sitecore6xItem.CopyItemTo` (see [Source/Core/Sitecore6xItem.cs](../../../Source/Core/Sitecore6xItem.cs) lines 1213–1770 for reference). The implementation here targets `Umbraco14xItem` as the destination and accepts any `IItem` as the source.

- [ ] **Step 1: Implement `CopyTo`**

Replace the `CopyTo` stub with this full implementation:

```csharp
public void CopyTo(IItem CopyFrom, bool bRecursive, bool bOnlyChildren)
{
    if (_Options.ShouldItemBeCopied != null && !_Options.ShouldItemBeCopied(CopyFrom, this))
        return;

    try
    {
        IItem destination = this;
        if (!bOnlyChildren)
            destination = CopyOneItem(CopyFrom, this);
        else
            destination = this;

        if (_Options.CopyItem != null) _Options.CopyItem(CopyFrom, this, destination);

        if (bRecursive && CopyFrom.HasChildren())
        {
            foreach (var child in CopyFrom.GetChildren())
            {
                // Recurse on the destination so each child copy uses the correct parent.
                ((Umbraco14xItem)destination).CopyTo(child, true, false);
            }
        }
    }
    catch (Exception ex)
    {
        if (!_Options.IgnoreErrors) throw;
        System.Diagnostics.Trace.WriteLine("Umbraco14x CopyTo error: " + ex.Message);
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
                // Fall through to creation with a unique name.
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
```

- [ ] **Step 2: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14xItem CopyTo with template resolution and field migration"
```

---

## Task 12: Media blob upload via temporary-file flow

**Files:**
- Modify: `Source/Core/Umbraco14xItem.cs`

The media create call accepts only `{ key }` references to previously-uploaded temporary files, so binary content needs a two-step upload: `POST /temporary-file` → `POST /media` with the returned key in a `umbracoFile` property.

- [ ] **Step 1: Add helper `UploadTemporaryFile`**

Inside `Umbraco14xItem` (place below `ConvertFieldContent`):

```csharp
/// <summary>
/// Uploads binary content as a temporary file and returns the key for referencing in a media create call.
/// </summary>
public string UploadTemporaryFile(byte[] bytes, string fileName, string contentType)
{
    string tempKey = Guid.NewGuid().ToString();
    var url = _api.BaseUrl + BaseApiPath + "/temporary-file";

    using (var http = new System.Net.Http.HttpClient())
    using (var content = new System.Net.Http.MultipartFormDataContent())
    {
        var fileContent = new System.Net.Http.ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType ?? "application/octet-stream");
        content.Add(fileContent, "File", fileName);
        content.Add(new System.Net.Http.StringContent(tempKey), "Id");

        // Reuse auth from main API. EnsureToken/BuildRequest logic is intentionally duplicated here
        // to avoid widening the API class's public surface — this is the only multipart call.
        var field = typeof(Umbraco14xAPI).GetMethod("EnsureToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.Invoke(_api, null);

        var tokenField = typeof(Umbraco14xAPI).GetField("_bearerToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        string token = (string)tokenField?.GetValue(_api);

        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = http.PostAsync(url, content).GetAwaiter().GetResult();
        var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!resp.IsSuccessStatusCode)
            throw new Umbraco14xApiException((int)resp.StatusCode, "/temporary-file", body);
    }

    return tempKey;
}
```

- [ ] **Step 2: Extend `CopyOneItem` to handle media blobs**

Inside `CopyOneItem`, before building the `valuesArray`, add a block that detects an Umbraco 6 source blob and uploads it:

```csharp
// Media blob handling — only when source is media and exposes a downloadable host URL.
string mediaTempKey = null;
if (createKind == Umbraco14xItemKind.Media && source.GetHostUrl() != null)
{
    try
    {
        var umbracoFile = source.Fields.FirstOrDefault(f =>
            string.Equals(f.Name, "umbracoFile", StringComparison.OrdinalIgnoreCase));
        if (umbracoFile != null && !string.IsNullOrEmpty(umbracoFile.Content))
        {
            string downloadUrl = umbracoFile.Content;
            if (downloadUrl.StartsWith("/"))
                downloadUrl = source.GetHostUrl().TrimEnd('/') + downloadUrl;
            using (var wc = new System.Net.WebClient())
            {
                var bytes = wc.DownloadData(downloadUrl);
                string fileName = System.IO.Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
                string contentType = wc.ResponseHeaders?["Content-Type"] ?? "application/octet-stream";
                mediaTempKey = UploadTemporaryFile(bytes, fileName, contentType);
            }
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Trace.WriteLine("Umbraco14x media blob copy failed for '" + source.Name + "': " + ex.Message);
        if (!_Options.IgnoreErrors) throw;
    }
}
```

After the `valuesArray` is populated but before `body` is built, substitute the media file value:

```csharp
if (mediaTempKey != null)
{
    foreach (var entry in valuesArray.OfType<JObject>())
    {
        if (string.Equals((string)entry["alias"], "umbracoFile", StringComparison.OrdinalIgnoreCase))
            entry["value"] = new JObject { ["temporaryFileId"] = mediaTempKey };
    }
}
```

- [ ] **Step 3: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds.

- [ ] **Step 4: Commit**

```bash
git add Source/Core/Umbraco14xItem.cs
git commit -m "feat: Umbraco14x media blob upload via temporary-file flow"
```

---

## Task 13: Wire into Windows app source/target selector

**Files:**
- Inspect: `Source/windows app/` — find where Sitecore6x / Umbraco6x are registered in the source/target dropdown.
- Modify: the file(s) identified above.

- [ ] **Step 1: Locate the existing Umbraco6x registration**

Run:

```
grep -r "Umbraco6xAPI\|Umbraco6xItem.GetRoot" Source/windows\ app/
```

Expected: one or more `.cs` files reference `Umbraco6xAPI` when constructing the CMS connection. Common candidates: `MainForm.cs`, a settings/config dialog, or a factory method.

- [ ] **Step 2: Add an Umbraco14x entry**

Alongside each place `Umbraco6xAPI` is instantiated, add an analogous `Umbraco14xAPI` / `Umbraco14xItem.GetRoot` branch. The exact code is codebase-specific — mirror the surrounding pattern. Example shape:

```csharp
// inside whatever switch/factory chose the CMS
else if (cmsChoice == "Umbraco 14+")
{
    var api = new Umbraco14xAPI(urlTextBox.Text, credentials);
    rootItem = Umbraco14xItem.GetRoot(api, options);
}
```

Add `"Umbraco 14+"` as an option wherever the CMS-type dropdown is populated.

- [ ] **Step 3: Build**

```
msbuild Source/SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: build succeeds; Windows app now lists Umbraco 14+ as an option.

- [ ] **Step 4: Commit**

```bash
git add Source/windows\ app/
git commit -m "feat: register Umbraco14x in Windows app source/target selector"
```

---

## Task 14: Smoke-test harness

**Files:**
- Create: `Source/Umbraco14xSmokeTest/Umbraco14xSmokeTest.csproj`
- Create: `Source/Umbraco14xSmokeTest/Program.cs`
- Create: `Source/Umbraco14xSmokeTest/App.config`
- Create: `Source/Umbraco14xSmokeTest/Properties/AssemblyInfo.cs`
- Modify: `Source/SitecoreConverter.sln`

- [ ] **Step 1: Create the csproj**

Create `Source/Umbraco14xSmokeTest/Umbraco14xSmokeTest.csproj`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{A1000000-0000-0000-0000-000000000014}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <RootNamespace>SitecoreConverter.Umbraco14xSmokeTest</RootNamespace>
    <AssemblyName>Umbraco14xSmokeTest</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Net.Http" />
    <Reference Include="Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed">
      <HintPath>..\packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <None Include="App.config" />
    <None Include="packages.config" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Core\SitecoreConverter.Core.csproj">
      <Project>{6389AD81-2400-4A67-9ACD-CF5F3C81F5BB}</Project>
      <Name>SitecoreConverter.Core</Name>
    </ProjectReference>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```

- [ ] **Step 2: Create `packages.config`**

Create `Source/Umbraco14xSmokeTest/packages.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net48" />
</packages>
```

- [ ] **Step 3: Create `App.config`**

Create `Source/Umbraco14xSmokeTest/App.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
    </startup>
</configuration>
```

- [ ] **Step 4: Create `AssemblyInfo.cs`**

Create `Source/Umbraco14xSmokeTest/Properties/AssemblyInfo.cs`:

```csharp
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Umbraco14xSmokeTest")]
[assembly: AssemblyProduct("SitecoreConverter")]
[assembly: ComVisible(false)]
[assembly: Guid("a1000000-0000-0000-0000-000000000015")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
```

- [ ] **Step 5: Create `Program.cs`**

Create `Source/Umbraco14xSmokeTest/Program.cs`:

```csharp
using System;
using System.Linq;
using SitecoreConverter.Core;

namespace SitecoreConverter.Umbraco14xSmokeTest
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                string url      = GetArg(args, "--url")       ?? Environment.GetEnvironmentVariable("UMBRACO14_URL");
                string username = GetArg(args, "--username")  ?? Environment.GetEnvironmentVariable("UMBRACO14_USERNAME");
                string password = GetArg(args, "--password")  ?? Environment.GetEnvironmentVariable("UMBRACO14_PASSWORD");

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.Error.WriteLine("Usage: Umbraco14xSmokeTest --url <base-url> --username <user> --password <pwd>");
                    Console.Error.WriteLine("       or set UMBRACO14_URL / UMBRACO14_USERNAME / UMBRACO14_PASSWORD env vars");
                    return 2;
                }

                var creds = new Credentials { UserName = username, Password = password };
                var api = new Umbraco14xAPI(url, creds);
                var options = new ConverterOptions { Language = "en-US" };
                var root = Umbraco14xItem.GetRoot(api, options);

                Console.WriteLine("Root: " + root.Name + " (" + root.ID + ")");
                foreach (var branch in root.GetChildren())
                    Console.WriteLine("  Branch: " + branch.Name);

                var content = root.GetChildren().FirstOrDefault(c => c.Name == "Content");
                if (content == null) { Console.Error.WriteLine("No Content branch"); return 3; }

                Console.WriteLine("\nTop-level Content items:");
                var topContent = content.GetChildren();
                foreach (var i in topContent) Console.WriteLine("  - " + i.Name + " (" + i.ID + ")");

                if (!topContent.Any())
                {
                    Console.WriteLine("No content items found; exiting successfully after traversal.");
                    return 0;
                }

                var first = topContent.First();
                Console.WriteLine("\nReading first content item's fields:");
                foreach (var f in first.Fields)
                    Console.WriteLine("  " + f.Name + " [" + f.Type + "] = " +
                        (f.Content?.Length > 60 ? f.Content.Substring(0, 60) + "..." : f.Content));

                var probe = first.Fields.FirstOrDefault(f =>
                    f.Type == "Umbraco.TextBox" || f.Type == "Umbraco.TextArea");
                if (probe != null)
                {
                    string originalValue = probe.Content;
                    string marker = "smoke-test-" + DateTime.UtcNow.Ticks;
                    probe.Content = originalValue + "\n" + marker;
                    Console.WriteLine("\nSaving round-trip change on field '" + probe.Name + "'...");
                    first.Save();

                    // Re-read and verify
                    var refreshed = (Umbraco14xItem)((Umbraco14xItem)content).GetItem(first.ID);
                    var refreshedField = refreshed.Fields.FirstOrDefault(f =>
                        string.Equals(f.Name, probe.Name, StringComparison.OrdinalIgnoreCase));
                    bool roundTripped = refreshedField != null && refreshedField.Content.Contains(marker);
                    Console.WriteLine("Round-trip " + (roundTripped ? "OK" : "FAILED"));

                    // Restore original
                    refreshedField.Content = originalValue;
                    refreshed.Save();
                    return roundTripped ? 0 : 4;
                }
                Console.WriteLine("\nNo text field found on first item to probe round-trip; tree + field read succeeded.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SMOKE TEST FAILED: " + ex);
                return 1;
            }
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
```

- [ ] **Step 6: Add the project to `SitecoreConverter.sln`**

In `Source/SitecoreConverter.sln`, insert before `Global` (around line 14):

```
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Umbraco14xSmokeTest", "Umbraco14xSmokeTest\Umbraco14xSmokeTest.csproj", "{A1000000-0000-0000-0000-000000000014}"
EndProject
```

And in the `GlobalSection(ProjectConfigurationPlatforms) = postSolution` block, add:

```
{A1000000-0000-0000-0000-000000000014}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
{A1000000-0000-0000-0000-000000000014}.Debug|Any CPU.Build.0 = Debug|Any CPU
{A1000000-0000-0000-0000-000000000014}.Release|Any CPU.ActiveCfg = Release|Any CPU
{A1000000-0000-0000-0000-000000000014}.Release|Any CPU.Build.0 = Release|Any CPU
```

- [ ] **Step 7: Restore + build**

From `Source/`:

```
nuget restore SitecoreConverter.sln
msbuild SitecoreConverter.sln /p:Configuration=Debug /t:Build /v:minimal
```

Expected: all six projects build successfully.

- [ ] **Step 8: Commit**

```bash
git add Source/Umbraco14xSmokeTest/ Source/SitecoreConverter.sln
git commit -m "feat: add Umbraco14xSmokeTest console harness for manual verification"
```

---

## Task 15: Manual verification against a live Umbraco 14+ instance

This task is **not code**. It runs the smoke test against a live instance to confirm the provider actually works end-to-end. Results get recorded in the commit message so the repo has a record of what was verified.

- [ ] **Step 1: Prepare a local Umbraco 14+ instance**

Either use an existing instance or spin up a fresh one:

```
dotnet new install Umbraco.Templates
dotnet new umbraco -n SmokeTestSite
cd SmokeTestSite
dotnet run
```

Visit `https://localhost:5001/umbraco` to complete the setup wizard; create an admin user and remember the credentials.

Create at least one content item with a text field populated.

- [ ] **Step 2: Run the smoke test**

From `Source/Umbraco14xSmokeTest/bin/Debug/`:

```
Umbraco14xSmokeTest.exe --url https://localhost:5001 --username admin@example.com --password <your-password>
```

Expected output (roughly):

```
Root: umbraco (14000000-...)
  Branch: Content
  Branch: Media
  ...
Top-level Content items:
  - Home (...)
Reading first content item's fields:
  title [Umbraco.TextBox] = Welcome
  ...
Saving round-trip change on field 'title'...
Round-trip OK
```

Exit code: `0`.

- [ ] **Step 3: Record verification in the repo**

Create an empty commit noting what was validated:

```bash
git commit --allow-empty -m "chore: verify Umbraco14x provider against local Umbraco <VERSION>

Smoke test run OK: authenticated, enumerated branches, read Content top
level, read fields on first item, round-tripped a text field write."
```

---

## Self-review — spec coverage check

| Spec section | Implemented in task(s) |
|---|---|
| `Umbraco14xAPI` transport, OAuth2, JSON helpers, token refresh | Task 2 |
| `Umbraco14xField` with dirty tracking and two constructors | Task 3 |
| `Umbraco14xItem` skeleton + `ItemKind` + synthetic roots + `GetHostUrl`/`GetOuterXml`/`Options`/`HasChildren` | Task 4 |
| `GetChildren` pagination, `GetItem`, `Path` via ancestors | Task 5 |
| Field loading (`values[]` + culture filter), `Templates`, `BaseTemplate`, property-definition cache | Task 6 |
| `GetLanguages`, `Roles`, `Users` | Task 7 |
| `MoveTo`, `Rename`, `Delete` (hard-delete two-step) | Task 8 |
| `Save` with dirty tracking on fields + name + icon + sortOrder | Task 9 |
| `AddFromTemplate` (GUID, alias, path forms) | Task 10 |
| `CopyTo` full parity (CopyOperation, CopyTemplates, plugin callbacks) | Task 11 |
| Media blob two-phase upload | Task 12 |
| Windows-app registration | Task 13 |
| Smoke-test harness | Task 14 |
| Live-instance verification | Task 15 |
| No unit tests (matches project precedent) | Explicit in "Notes for the implementer" |
| `packages.config` + Newtonsoft.Json 13.0.3 | Task 1 |

All spec requirements are covered.

## Self-review — placeholder scan

Reviewed the plan for `TBD` / `TODO` / "similar to above" / "add appropriate error handling" / undefined types. Task 13 does contain one legitimate exception: it says "the exact code is codebase-specific" for Windows-app integration because the file paths and surrounding code aren't visible without opening the Windows app project. The implementer is expected to `grep` for the existing registration and mirror it. This is documented explicitly in the task.

## Self-review — type consistency

- `Umbraco14xAPI` — public `BaseUrl`, `Credentials`, `GetJson`, `TryGetJson`, `PostJson`, `PutJson`, `DeleteJson`, `GetLanguages`. Private `Send`, `EnsureToken`, `AcquireTokenLocked`, `BuildRequest`. All consistent across Tasks 2–12.
- `Umbraco14xField` — `ToValuesEntry`, `IsDirty`, `Culture`, `Segment`. Referenced consistently in Task 9 (`f.ToValuesEntry()`, `f.IsDirty`) and Task 11 (`newField.IsDirty = true`).
- `Umbraco14xItem` — `GetRoot`, `Kind`, `Api`, `RawPayload`, `KindToSegment`, `BaseApiPath`. Consistent across Tasks 4–12.
- Enum `Umbraco14xItemKind` — same 11 values referenced across all item-code tasks.
- `ConverterOptions.ExistingTemplates` keyed variants used: raw GUID (Task 5, Task 11), `"path:<guid>"` (Task 5), `"alias:<alias>"` (Task 11), absolute path (Task 5). Types all `IItem` which matches the declared `Dictionary<string, IItem>`.
- `ConverterOptions.ExistingTemplateFields` is `Dictionary<string, XmlNode>` — Task 6 wraps the JSON payload in an XML element for storage, with a call-out comment. Consistent.

No type mismatches.
