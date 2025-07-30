// ===============================================
// CONFIGURACIÓN API
// ===============================================

namespace InvenBank.API.Configuration
{
    public class ApiSettings
    {
        public const string SectionName = "ApiSettings";

        public int DefaultPageSize { get; set; } = 20;
        public int MaxPageSize { get; set; } = 100;
        public int CacheExpirationMinutes { get; set; } = 15;
        public bool EnableRequestLogging { get; set; } = true;
        public bool EnableResponseCompression { get; set; } = true;
    }

}
