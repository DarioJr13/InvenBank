// ===============================================
// CONFIGURACIÓN SWAGGER
// ===============================================

namespace InvenBank.API.Configuration
{
    public class SwaggerSettings
    {
        public const string SectionName = "SwaggerSettings";

        public string Title { get; set; } = "InvenBank API";
        public string Version { get; set; } = "v1";
        public string Description { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

}
