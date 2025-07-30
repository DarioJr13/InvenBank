
// ===============================================
// CONFIGURACIÓN DE BASE DE DATOS
// ===============================================
namespace InvenBank.API.Configuration
{
    public class DatabaseSettings
    {
        public const string SectionName = "ConnectionStrings";

        public string DefaultConnection { get; set; } = string.Empty;
        public string SqlServerConnection { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
        public bool EnableRetryOnFailure { get; set; } = true;
        public int MaxRetryCount { get; set; } = 3;
    }
}
