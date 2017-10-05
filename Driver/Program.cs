using Izenda.BI.CacheProvider.RedisCache;
using StackExchange.Redis;
using Driver.Models;
using System.Collections.Generic;
using log4net;

namespace Driver
{
    /// <summary>
    /// Driver program to quickly test functionality of the RedisCache provider
    /// </summary>
    class Program
    {
        // Logger must be instantiated early in the application for it to be configured for later use.
        // From log4net documentation (http://logging.apache.org/log4net/release/manual/configuration.html): 
        // "Therefore it is imperative to make a logging call as early as possible during the application start-up..."
        private static readonly ILog logger = LogManager.GetLogger("RedisCacheLogger");

        static void Main(string[] args)
        {
            var connection = ConnectionMultiplexer.Connect("127.0.0.1:6379,abortConnect=false");
            var cache = new RedisCacheProvider(connection.GetDatabase());
            var key = "order_1";

            var order = new Models.Order
            {
                OrderId = 1,
                TotalPrice = 100,
                Products = new List<Product>
                {
                    new Product { Id = 1, Name = "Test Product", Price = 100 }
                }
            };

            cache.Add<Models.Order>(key, order);

            var cachedOrder = cache.Get<Models.Order>(key);
        }
    }
}
