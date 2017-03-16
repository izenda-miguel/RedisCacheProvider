using Xunit;

namespace Izenda.BI.CacheProvider.RedisCache.Test
{
    public class RedisCacheProviderTest
    {
        private const string Key = "test_key";
        private const string Value = "test value";

        /// <summary>
        /// Test function Add()
        /// </summary>
        [Fact]
        public void AddValueToCache_NoExpiration()
        {
            var redisCacheProvider = new RedisCacheProvider();
            redisCacheProvider.Add(Key, Value);
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }
    }
}
