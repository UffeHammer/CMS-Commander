# Umbraco14x Provider Design

**Status:** Draft for review
**Date:** 2026-04-21
**Author:** Uffe Hammer (with Claude)
**Target project:** `Source/Core/SitecoreConverter.Core.csproj`

## Purpose

Add an Umbraco 14+ provider (`Umbraco14xItem`, `Umbraco14xField`) to CMS-commander that uses the Umbraco **Management API** (REST/JSON) for both read and write, at full parity with the existing `Sitecore6xItem`/`Sitecore6xField` pair. The new provider replaces the obsolete SOAP `documentService` path used by the stub `Umbraco6xItem`, and enables Umbraco 14+ to act as either source or target in CMS migrations.

The name `Umbraco14x` matches the project convention (class named after the minimum supported version): Umbraco 14 is the first version to ship the Management API, and the provider is expected to work on all 14+ releases including 17.x.

## Non-goals

- No async public API — the rest of the codebase is synchronous; we expose sync-only.
- No block-editor value restructuring — existing `FieldConverterPlugin` handles field-level conversion; structural translation is out of scope.
- No multi-threading / parallelism — matches Sitecore6x.
- No automated unit tests for the provider itself — the precedent in this repo is manual validation against live instances (see Testing).
- No support for Umbraco's `segment` variation axis (non-null segment values are ignored on read; written as `null`).

## Architecture

### Files added

| File | Role | Approx size |
|---|---|---|
| `Source/Core/Umbraco14xAPI.cs` | Transport layer: `HttpClient` wrapper, OAuth2 token lifecycle, typed JSON helpers | ~300 lines |
| `Source/Core/Umbraco14xItem.cs` | `IItem` implementation with `ItemKind` dispatch | ~1500 lines |
| `Source/Core/Umbraco14xField.cs` | `IField` implementation | ~150 lines |

### Project file changes (classic MSBuild, not SDK-style)

- Add `packages.config` with `Newtonsoft.Json` (first NuGet package in the project).
- Reference `System.Net.Http` (present in the .NET Framework 4.8 BCL).
- Register the three new `.cs` files under `<ItemGroup>` in `SitecoreConverter.Core.csproj`.
- No new Web References — Management API is REST.

### Target framework

`.NET Framework 4.8` (unchanged). `HttpClient` is available; `Newtonsoft.Json` carries JSON parsing.

## Transport layer — `Umbraco14xAPI`

Analog of `Umbraco6xAPI`. Constructor:

```csharp
public Umbraco14xAPI(string sUrl, Credentials credentials)
```

**Responsibilities:**

- Holds base URL (e.g. `https://my-umbraco.local`), `Credentials`, a `CookieContainer`, and a single reusable `HttpClient`.
- On first authenticated call: exchanges the user's username/password for a bearer token via the Management API's back-office OAuth2 token endpoint (exact path confirmed against the target Umbraco version's Swagger at implementation time; the stable flow is `grant_type=password` with `client_id=umbraco-back-office`). Stores the resulting bearer token and its expiry.
- Attaches `Authorization: Bearer {token}` to every subsequent call.
- Refreshes the token on `401` (single retry) or just before stored expiry.
- Exposes synchronous helpers returning Newtonsoft types:
  - `JToken GetJson(string path)`
  - `JToken PostJson(string path, JToken body)`
  - `JToken PutJson(string path, JToken body)`
  - `void DeleteJson(string path)`
  - `JToken TryGetJson(string path)` — returns null on 404 instead of throwing
- Funnels raw HTTP through a single `Send(HttpMethod, string path, JToken body)` so logging/retry/auth are centralised. Implementation uses `.GetAwaiter().GetResult()` on `HttpClient` calls because callers are synchronous.
- Throws `Umbraco14xApiException(int statusCode, string path, string responseBody)` on any non-2xx not handled by `TryGetJson`.
- Exposes `public string BaseUrl` so `IItem.GetHostUrl()` works.
- Caches the `/language` result for the lifetime of the instance.

Callers never touch `HttpClient` directly — all Management API access is through `Umbraco14xAPI`.

## Virtual unified tree

The provider exposes Umbraco's segmented sections as a single synthetic tree so Sitecore↔Umbraco `CopyTo` operations work naturally.

### Tree shape

```
(Root, synthetic)
├── Content       → /tree/document + /document/{id}
├── Media         → /tree/media + /media/{id}
├── Templates     → /tree/document-type + /document-type/{id}
├── DataTypes     → /tree/data-type + /data-type/{id}
├── Members       → /member + /member-group
├── Roles         → /user-group
└── Languages     → /language
```

### ItemKind enum

Each `Umbraco14xItem` carries an internal `ItemKind`: `Root`, `BranchRoot`, `Content`, `Media`, `DocumentType`, `DataType`, `Member`, `MemberGroup`, `User`, `UserGroup`, `Language`. Every method that hits the API uses `_kind` to pick the right endpoint.

Root and BranchRoot items use well-known sentinel GUIDs so `GetItem("{guid}")` round-trips.

### ID format

`IItem.ID` returns the native GUID string directly. The `Util.Int2Guid` / `Util.GuidToSitecoreID` helpers used by Umbraco6x and Sitecore6x are **not** used — Umbraco 14+ uses real GUIDs.

### Path format

`"/Content/Home/About"` style, with the synthetic branch name as the first segment. `GetItem` accepts:

- `"{guid}"` → direct ID lookup
- `"/Content/..."` → walk by name from the synthetic root
- Bare name → resolve relative to `this`

Resolved paths are cached in `_Options.ExistingTemplates` keyed by path and by `"{guid}"`.

## `IItem` implementation matrix

| Member | Behaviour |
|---|---|
| `ID` | GUID string from the payload. Synthetic roots return fixed sentinel GUIDs. |
| `Name` (get/set) | From the payload's `name` (content) or `alias`/`name` (document types). Varying items use the current culture's name. Setter flips dirty flag. |
| `Key` | `Name.ToLower()`. |
| `Path` | Built via `GET /tree/{branch}/ancestors?descendantId={id}`, prefixed with branch name. Cached. |
| `Icon` (get/set) | `icon` field on the payload. Setter dirty-flags. |
| `SortOrder` (get/set) | `sortOrder` on the tree-node payload. Setter persists via the kind-appropriate sort endpoint. |
| `Templates` | Content item → `[documentType] ∪ compositions`. DocumentType → `compositions`. Cached in `_Options.ExistingTemplates`. |
| `BaseTemplate` | Content → the item's document type. DocumentType → explicit parent if any, else first composition, else null. |
| `Fields` | `Umbraco14xField[]` built from `values[]` filtered to current culture for content/media (plus all invariant fields). For DocumentType, built from flattened `properties[]` across all property groups. |
| `Roles` | On `Content` → `GET /document/{id}/permissions` returns granted user groups as `IRole[]`. On `BranchRoot("Roles")` → all user groups. |
| `Users` | On `BranchRoot("Members")` → all members. Other kinds throw with "users only exist under /Members". |
| `Parent` | Lazy via ancestors call; cached. Branch roots parent to synthetic Root; Root is null. |
| `GetChildren()` | Paginated `GET /tree/{kind}/children?parentId={id}&skip=…&take=…` until exhausted. Branch roots return top-level nodes for their endpoint. |
| `GetItem(path)` | Supports GUID, `/Content/...` path, or bare name relative to `this`. Uses the path cache. |
| `CopyTo(src, recursive, onlyChildren)` | Heavy method; mirrors `Sitecore6xItem.CopyItemTo`. Respects `CopyOperation` (Overwrite/SkipExisting/GenerateNewItemIDs/UseNames), clones document types when `CopyTemplates` is true, creates missing language variants, copies blobs for media via the two-phase temporary-file flow, invokes plugin callbacks. |
| `MoveTo(dest)` | `PUT /{kind}/{id}/move`; updates local path state. |
| `Rename(name)` | `PUT /{kind}/{id}` with modified `variants[].name`. Updates local `_sName`. |
| `Delete()` | `DELETE /{kind}/{id}`. Uses `?permanent=true` where supported; otherwise recycle-bin + empty in two steps. Matches Sitecore6x "hard delete" semantics. |
| `Save()` | Builds PUT payload from dirty fields for the current culture plus invariant fields, and from changed item-level properties (name, icon, sortOrder). `PUT /{kind}/{id}`. Adds missing culture entries to `variants[]`. |
| `AddFromTemplate(name, templatePath)` | Resolves `templatePath` to a document type GUID (cache → `GET /document-type/by-alias/{alias}` → path walk). `POST /document` with `{ documentType: { id }, parent: { id }, variants: [...] }`. Returns new GUID string. |
| `HasChildren()` | From `hasChildren` on the tree-node payload. |
| `GetLanguages()` | `GET /language` result cached on `Umbraco14xAPI`. Returns ISO culture codes (e.g. `"en-US"`). |
| `Options` (get/set) | Standard reference getter/setter. |
| `GetOuterXml()` | Serialises the item's raw Management API JSON wrapped in a `<umbraco>` root element — same spirit as Sitecore returning its SOAP XML. |
| `GetHostUrl()` | Returns `_api.BaseUrl`. |

### Dirty tracking

Each `Umbraco14xField` has an `IsDirty` flag flipped by the setter. `Save()` only writes dirty fields plus any changed item-level properties. This keeps PUT payloads small and avoids stomping concurrent edits.

## Languages and variants

- `ConverterOptions.Language` drives which culture is read and written (default `"en"`; Umbraco typically uses `"en-US"` — the `CopyTo` flow maps where needed).
- `GetLanguages()` returns cultures from `/language`.
- Invariant (non-varying) fields are read and written once.
- Varying fields are filtered to `ConverterOptions.Language` on read; the matching `variants[]` entry is updated on write.
- Missing culture variants are created on demand during `Save`/`CopyTo` by appending to `variants[]`.
- `segment` is always treated as `null` on both read and write. Non-null segment values on the source are ignored. A future extension can add `ConverterOptions.Segment` without breaking the interface.

## `Umbraco14xField` — `IField` implementation matrix

Two constructors, matching `Sitecore6xField`:

- **From a content/media payload** — reads `alias`, `culture`, `segment`, `value` from a `values[]` entry; enriches metadata by looking up the property definition on the item's document type (cached in `_Options.ExistingTemplateFields` by property-definition GUID).
- **From a document-type property definition** — used when the parent item *is* a document type; content is the default value.

| Member | Behaviour |
|---|---|
| `Name` | Property `alias` from the document-type property definition; falls back to `values[].alias`. |
| `LanguageTitle` | Human-readable label (`name` on the property definition). |
| `Key` | `alias.ToLower()`. |
| `Source` | Data-type configuration JSON for the property (allowed content types, datasource IDs, etc.), returned as a compact JSON string so plugins can parse it. |
| `Section` | Property group name (e.g. `"Content"`, `"SEO"`). |
| `Content` (get/set) | The `value` field. Setter flips `IsDirty`. Raw editor JSON/HTML passes through unchanged — `FieldConverterPlugin` handles format translation. |
| `Type` | Property editor alias (`Umbraco.RichText`, `Umbraco.TextBox`, `Umbraco.MediaPicker3`, ...). |
| `SortOrder` | `sortOrder` on the property definition within its group. |
| `TemplateFieldID` | GUID of the property definition on the document type. |

## Error handling

- Everything funnels through `Umbraco14xAPI.Send`.
- Non-2xx (except 404 via `TryGetJson`) throws `Umbraco14xApiException(statusCode, path, responseBody)`.
- `CopyTo` wraps per-item errors and honours `ConverterOptions.IgnoreErrors` — same contract as `Sitecore6xItem.CopyItemTo`.
- Requests and error responses log via `System.Diagnostics.Trace` (matching `traceextension.cs`) when not suppressed.

## Caching

- `ConverterOptions.ExistingTemplates` — keyed by path and by `"{guid}"`. Stores resolved `IItem` for document types and frequently-walked content items.
- `ConverterOptions.ExistingTemplateFields` — keyed by property-definition GUID. Stores the raw JSON node for field metadata enrichment.
- Per-`Umbraco14xAPI` caches: bearer token + expiry; `/language` result.

## Concurrency

Single-threaded by design. Matches Sitecore6x. `HttpClient` instance is reused; calls are synchronous.

## Testing

The existing Sitecore6x and Umbraco6x providers have zero unit tests; validation is manual against live instances through the Windows app. This provider follows the same precedent, plus adds a dedicated smoke-test harness.

### Validation steps

1. **Compile gate.** Build the `SitecoreConverter.Core` project clean with the new `Newtonsoft.Json` reference. First correctness bar.
2. **Manual smoke via the Windows app.** Wire `Umbraco14xItem.GetRoot` into the same source/target dropdown the other providers use. Walk a tree, read a content item, save a field change, copy a simple item. Document the workflow in the existing README.
3. **Umbraco14xSmokeTest console harness.** A new `Source/Umbraco14xSmokeTest/` project:
   - Console app, .NET Framework 4.8, references `SitecoreConverter.Core`.
   - Reads URL + credentials from command-line args or environment variables.
   - Authenticates, calls `GetRoot`, enumerates `/Content` and `/Media` top levels, reads one content item's fields, mutates one field locally, calls `Save`, re-reads, prints a diff of the round-trip.
   - Returns non-zero on any failure so it can be scripted.
   - Not run in CI; exists for repeatable manual sanity checks against a local Umbraco 14+ Docker instance.

### Cross-CMS copy validation

Sitecore → Umbraco14x `CopyTo` is validated against a real pair of instances, same as today's Sitecore↔Sitecore and Sitecore↔Umbraco6 validation.

## Risks

- **Management API drift across minor Umbraco versions.** The API is stable in v14 but shape changes have happened between minors. If 17.x or later breaks something, version probing may be needed. Deferred until observed.
- **Rich-text / block-editor structural translation.** Sitecore → Umbraco block editor is a structural migration, not string substitution. Field-level via `FieldConverterPlugin` is in scope; structural translation is a future plugin.
- **Media blob upload two-phase flow.** `POST /temporary-file` followed by referencing by key in the media create call. Straightforward but different from Umbraco 6's single-shot upload — worth a dedicated test in the smoke harness.

## Open questions

None identified during design review.

## Follow-up work (out of scope for this spec)

- Wire `Umbraco14xItem.GetRoot` into the Windows app's source/target selector.
- Update README with Umbraco 14+ configuration steps (base URL, credentials, required role).
- Consider a block-editor structural-translation plugin if migration needs exceed field-level conversion.
