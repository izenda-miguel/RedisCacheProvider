using Izenda.BI.Utility;
using StackExchange.Redis;
using System;

namespace Izenda.BI.CacheProvider.RedisCache
{
    /// <summary>
    /// Helper class to retrieve a connection to the Redis server
    /// </summary>
    [Obsolete("This class has been obsolesced in favor of 'RedisHelper'", true)]
    internal class RedisConnectionHelper
    {
        private static IDatabase instance = null;
        private static readonly RedisCacheConfiguration redisCacheConfiguration;

        static RedisConnectionHelper()
        {
            // Get redis cache configuration
            redisCacheConfiguration = WebConfigUtil.GetSection("redisCacheSettings") as RedisCacheConfiguration;
        }

        public static IDatabase Instance
        {
            get
            {
                if (instance == null)
                {

                    var connection = ConnectionMultiplexer.Connect(redisCacheConfiguration.ConnectionString);
                    instance = connection.GetDatabase();
                }

                return instance;
            }
        }
    }
}
