namespace Tabibi.API.Configurations
{
    public sealed class CorsOptions
    {
        public const string PolicyName = "TabibiCorsPolicy";
        public const string SectionName = "Cors";

        public required string[] AllowedOrigins { get; init; }
    }
}
