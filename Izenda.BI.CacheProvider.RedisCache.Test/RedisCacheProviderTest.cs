using StackExchange.Redis;
using System;
using Xunit;

namespace Izenda.BI.CacheProvider.RedisCache.Test
{
    public class RedisCacheProviderTest
    {
        private const string Key = "test_key";
        private const string Value = "test value";
        private readonly ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("127.0.0.1:6379,abortConnect=false");

        /// <summary>
        /// Test function Add()
        /// </summary>
        [Fact]
        public void AddValueToCache_NoExpiration()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.Add(Key, Value);
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }

        /// <summary>
        /// Test function AddWithExactLifeTime()
        /// </summary>
        [Fact]
        public void AddWithExactLifeTime_ExpireIn20Seconds()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.AddWithExactLifetime(Key, Value, new TimeSpan(0, 0, 20));
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }

        /// <summary>
        /// Test function AddWithSlidingLifetime()
        /// </summary>
        [Fact]
        public void AddWithSlidingLifetime_ExpireAfter20Seconds()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.AddWithSlidingLifetime(Key, Value, new TimeSpan(0, 0, 20));
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }

        /// <summary>
        /// Test function Contain()
        /// </summary>
        [Fact]
        public void ContainKey_ReturnTrueAfterAddItem()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.Add(Key, Value);
            var containKeyInCache = redisCacheProvider.Contains(Key);

            Assert.True(containKeyInCache);
        }

        /// <summary>
        /// Test function Ensure()
        /// </summary>
        [Fact]
        public void EnsureCache_ReturnCorrectObject()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.Ensure(Key, () => { return Value; });
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }

        /// <summary>
        /// Test function EnsureCacheWithExactLifeTime()
        /// </summary>
        [Fact]
        public void EnsureCacheWithExactLifeTime_ReturnCorrectObject()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.EnsureWithExactLifetime(Key, new TimeSpan(0, 0, 20), () => { return Value; });
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }

        /// <summary>
        /// Test function TestEnsureCacheWidthSlidingLifetime()
        /// </summary>
        [Fact]
        public void EnsureCacheWidthSlidingLifetime_ReturnCorrectObject()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.EnsureWithSlidingLifetime(Key, new TimeSpan(0, 0, 20), () => { return Value; });
            var valueFromCache = redisCacheProvider.Get<string>(Key);

            Assert.Equal(Value, valueFromCache);
        }

        /// <summary>
        /// Test function Remove()
        /// </summary>
        [Fact]
        public void RemoveCache_ObjectIsRemoved()
        {
            var redisCacheProvider = new RedisCacheProvider(connection.GetDatabase());
            redisCacheProvider.Add(Key, Value);
            redisCacheProvider.Remove(Key);

            var containCacheKey = redisCacheProvider.Contains(Key);
            Assert.False(containCacheKey);
        }
    }
}
