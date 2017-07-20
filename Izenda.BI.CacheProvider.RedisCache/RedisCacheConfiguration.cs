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
        
        private string _connectionStringWithOptions;
        public string ConnectionStringWithOptions
        {
            get
            {
                if (_connectionStringWithOptions == null)
                {
                    var options = !string.IsNullOrEmpty(AdditionalOptions) ? $",{AdditionalOptions}" : "";
                    _connectionStringWithOptions = $"{ConnectionString}{options}";
                }

                return _connectionStringWithOptions;
            }
        }

        /// <summary>
        /// Additional options for the redis server
        /// </summary>
        [ConfigurationProperty("additionalOptions")]
        public string AdditionalOptions
        {
            get { return (string)base["additionalOptions"]; }
            set { base["additionalOptions"] = value; }
        }
    }
}
