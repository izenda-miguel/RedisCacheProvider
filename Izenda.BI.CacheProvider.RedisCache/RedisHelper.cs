using Izenda.BI.Utility;
using StackExchange.Redis;

namespace Izenda.BI.CacheProvider.RedisCache
{
    /// <summary>
    /// Helper class to retrieve a connection to the Redis server
    /// </summary>
    internal class RedisHelper
    {
        private static readonly RedisCacheConfiguration redisCacheConfiguration;

        static RedisHelper()
        {
            // Get redis cache configuration
            redisCacheConfiguration = WebConfigUtil.GetSection("redisCacheSettings") as RedisCacheConfiguration;
            connection = GetConnection();
            database = connection.GetDatabase();
        }

        private static IConnectionMultiplexer GetConnection()
        {
            return ConnectionMultiplexer.Connect(redisCacheConfiguration.ConnectionStringWithOptions);
        }

        private static IConnectionMultiplexer connection = null;
        public static IConnectionMultiplexer Connection
        {
            get
            {
                if (connection == null)
                {
                    connection = GetConnection();
                }

                return connection;
            }
        }

        private static IDatabase database = null;
        public static IDatabase Database
        {
            get
            {
                if (database == null)
                {
                    connection.GetDatabase();
                }

                return database;
            }
        }

        private static IServer server = null;
        public static IServer Server
        {
            get
            {
                if (server == null)
                {
                    server = connection.GetServer(redisCacheConfiguration.ConnectionString);
                }

                return server;
            }
        }
    }
}
