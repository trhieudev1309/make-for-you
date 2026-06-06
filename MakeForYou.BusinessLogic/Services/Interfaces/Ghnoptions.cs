namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    /// <summary>
    /// Typed configuration bound from appsettings.json → "Ghn" section.
    /// </summary>
    public class GhnOptions
    {
        public const string SectionName = "Ghn";

        /// <summary>GHN API token (same for staging and production — only the BaseUrl changes).</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Default Shop ID to use when creating orders (can be overridden per-request).</summary>
        public int ShopId { get; set; }

        /// <summary>Base URL options — switch between Staging and Production via config.</summary>
        public GhnBaseUrlOptions BaseUrl { get; set; } = new();

        /// <summary>Returns the active base URL. Change "Staging" to "Production" in appsettings when going live.</summary>
        public string ActiveBaseUrl => BaseUrl.Staging; // ← swap to BaseUrl.Production for live
    }

    public class GhnBaseUrlOptions
    {
        public string Staging { get; set; } = "https://dev-online-gateway.ghn.vn";
        public string Production { get; set; } = "https://online-gateway.ghn.vn";
    }
}