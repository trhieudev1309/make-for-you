using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    /// <summary>
    /// Low-level HTTP client abstraction for GHN API.
    /// Every request automatically carries the Token and ShopId headers.
    /// Use this in any service that needs to call GHN — do NOT create raw HttpClients.
    /// </summary>
    public interface IGhnApiClient
    {
        /// <summary>HTTP GET — returns the parsed JsonDocument on success.</summary>
        Task<JsonDocument> GetAsync(string path, CancellationToken ct = default);

        /// <summary>HTTP POST with a JSON body — returns the parsed JsonDocument on success.</summary>
        Task<JsonDocument> PostAsync(string path, object body, CancellationToken ct = default);
    }
}