using System.Configuration;

namespace Izenda.BI.CacheProvider.RedisCache
{
    /// <summary>
    /// Configuration model for the RedisCache
    /// </summary>
    public sealed class RedisCacheConfiguration : ConfigurationSection
    {
        /// <summary>
        /// The ConnectionString setting
        /// </summary>
        [ConfigurationProperty("connectionString")]
        public string ConnectionString
        {
            get { return (string)base["connectionString"]; }
            set { base["connectionString"] = value; }
        }
    }
}
