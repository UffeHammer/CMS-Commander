using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        /// <summary>
        /// Multipart-form POST used for file uploads (temporary-file endpoint).
        /// Shares the same bearer-token lifecycle as the JSON helpers.
        /// </summary>
        internal void PostMultipart(string path, System.Net.Http.MultipartFormDataContent content)
        {
            EnsureToken();

            var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : _baseUrl + path;
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

            var resp = _http.SendAsync(req).GetAwaiter().GetResult();
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Retry once with a refreshed token; MultipartFormDataContent is single-use,
                // so this path requires the caller to hand us a fresh content object if retry is
                // needed. In practice this is rare because EnsureToken refreshes proactively.
                lock (_tokenLock) { _bearerToken = null; }
                EnsureToken();
                throw new Umbraco14xApiException(401, path, "Multipart request unauthorized; token was refreshed but caller must retry");
            }

            string respBody = resp.Content != null
                ? resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                : "";
            if (!resp.IsSuccessStatusCode)
            {
                throw new Umbraco14xApiException((int)resp.StatusCode, path, respBody);
            }
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
