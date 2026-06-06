using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    /// <summary>
    /// Typed HTTP client for GHN API.
    /// Automatically sets:
    ///   • Token  header  (authentication)
    ///   • ShopId header  (required by most GHN endpoints)
    /// BaseAddress is resolved from GhnOptions.ActiveBaseUrl at startup.
    /// </summary>
    public class GhnApiClient : IGhnApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<GhnApiClient> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GhnApiClient(HttpClient http, IOptions<GhnOptions> options, ILogger<GhnApiClient> logger)
        {
            _http = http;
            _logger = logger;

            var cfg = options.Value;

            // BaseAddress set here so callers only pass relative paths
            _http.BaseAddress = new Uri(cfg.ActiveBaseUrl);
            _http.Timeout = TimeSpan.FromSeconds(30);

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // GHN uses plain header names, not Bearer
            _http.DefaultRequestHeaders.Add("Token", cfg.Token);
            _http.DefaultRequestHeaders.Add("ShopId", cfg.ShopId.ToString());
        }

        /// <inheritdoc />
        public async Task<JsonDocument> GetAsync(string path, CancellationToken ct = default)
        {
            _logger.LogDebug("[GHN] GET {Path}", path);
            var response = await _http.GetAsync(path, ct);
            return await ReadAndValidateAsync(response, "GET", path);
        }

        /// <inheritdoc />
        public async Task<JsonDocument> PostAsync(string path, object body, CancellationToken ct = default)
        {
            _logger.LogDebug("[GHN] POST {Path}", path);
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(path, content, ct);
            return await ReadAndValidateAsync(response, "POST", path);
        }

        // ── private ──────────────────────────────────────────────────────────

        private async Task<JsonDocument> ReadAndValidateAsync(
            HttpResponseMessage response, string method, string path)
        {
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[GHN] {Method} {Path} → HTTP {Status}: {Body}",
                    method, path, (int)response.StatusCode, raw);
                throw new HttpRequestException(
                    $"GHN API returned HTTP {(int)response.StatusCode} for {method} {path}: {raw}");
            }

            var doc = JsonDocument.Parse(raw);
            var code = doc.RootElement.TryGetProperty("code", out var c) ? c.GetInt32() : -1;

            if (code != 200)
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : "Unknown GHN error";

                _logger.LogError("[GHN] {Method} {Path} → code={Code}, message={Message}",
                    method, path, code, msg);
                throw new InvalidOperationException($"GHN error ({code}): {msg}");
            }

            _logger.LogDebug("[GHN] {Method} {Path} → OK", method, path);
            return doc;
        }
    }
}

