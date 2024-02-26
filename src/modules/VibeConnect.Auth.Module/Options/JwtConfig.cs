using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Auth.Module.Options
{
    public class JwtConfig
    {
        /// <summary>
        ///
        /// </summary>
        public string? Audience { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        ///
        /// </summary>
        ///
        public string SigningKey { get; set; } = string.Empty;

        /// <summary>
        ///
        /// </summary>
        public double AccessTokenValidityPeriod { get; set; }
        public double RefreshTokenValidityPeriod { get; set; }

    }
}
