using Izenda.BI.CacheProvider.RedisCache.Constants;
using Izenda.BI.CacheProvider.RedisCache.Converters;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;

namespace Izenda.BI.CacheProvider.RedisCache
{
    /// <summary>
    /// Redis cache provider
    /// </summary>
    [Export(typeof(ICacheProvider))]
    public class RedisCacheProvider : ICacheProvider, IDisposable
    {
        private bool _disposed = false;
        private JsonSerializerSettings _serializerSettings = new JsonSerializerSettings();
        private readonly ReaderWriterLockSlim _lockCache = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly IDatabase _cache;

        public RedisCacheProvider()
        {
            _cache = RedisConnectionHelper.Instance;
            InitSerializer();
        }

        public RedisCacheProvider(IDatabase cache)
        {
            _cache = cache;
            InitSerializer();
        }

        /// <summary>
        /// Initializes the JSON serializer
        /// </summary>
        private void InitSerializer()
        {
            _serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _serializerSettings.TypeNameHandling = TypeNameHandling.Objects;
            _serializerSettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            _serializerSettings.Converters.Add(new DBServerTypeSupportingConverter());
        }

        /// <summary>
        /// Serializes the obj to json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns> A json string of the object</returns>
        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _serializerSettings);
        }

        /// <summary>
        /// Deserializes the json string to the specified type
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="serialized">The serialized object</param>
        /// <returns>THe deserialized object</returns>
        private T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized, _serializerSettings);
        }

        /// <summary>
        /// Adds an item to the cache using the specified key.
        /// </summary>
        /// <param name="key"> The key </param>
        /// <param name="value"> The value </param>
        public void Add<T>(string key, T value)
        {
            try
            {
                _lockCache.EnterWriteLock();
               _cache.StringSet(key, Serialize(value));
            }
            catch (Exception ex)
            {
                Trace.Write(string.Format(AppConstants.ExceptionTemplate, ex.ToString()));
            }
            finally
            {
                _lockCache.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds an item to the cache using the specified key and sets an expiration
        /// </summary>
        /// <param name="key"> The key </param>
        /// <param name="value"> The value</param>
        /// <param name="expiration"> The expiration </param>
        public void AddWithExactLifetime(string key, object value, TimeSpan expiration)
        {  
            try
            {
                _lockCache.EnterWriteLock();
                _cache.StringSet(key, Serialize(value), expiration);
            }
            catch (Exception ex)
            {
                Trace.Write(string.Format(AppConstants.ExceptionTemplate, ex.ToString()));
            }
            finally
            {
                _lockCache.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds an item to the cache using the specified key and sets a sliding expiration
        /// </summary>
        /// <param name="key"> The key </param>
        /// <param name="value"> The value</param>
        /// <param name="expiration"> The expiration </param>
        public void AddWithSlidingLifetime(string key, object value, TimeSpan expiration)
        {
            AddWithExactLifetime(key, value, expiration);
        }

        /// <summary>
        /// Checks if the cache contains the given key
        /// </summary>
        /// <param name="key"> The key</param>
        /// <returns>true if the cache contains the key, false otherwise</returns>
        public bool Contains(string key)
        {
            return _cache.KeyExists(key);
        }

        /// <summary>
        /// Retrieves the specified key from the cache
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The key</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var result = _cache.StringGet(key);

            if (result.IsNullOrEmpty)
                return default(T);

            return Deserialize<T>(_cache.StringGet(key));
        }

        /// <summary>
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="key">The key</param>
        public void Remove(string key)
        {
            try
            {
                _lockCache.EnterWriteLock();
                _cache.KeyDelete(key);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _lockCache.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retrieves the specified key from the cache. If no value exists, a cache entry is created. 
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The key</param>
        /// <param name="executor">The function call that returns the data.</param>
        /// <returns></returns>
        public T Ensure<T>(string key, Func<T> executor)
        {
            return EnsureCache(executor, key, TimeSpan.Zero, (cacheKey, result, expiration) =>
            {
                Add(cacheKey, result);
            });
        }

        /// <summary>
        /// Retrieves the specified key from the cache. If no value exists, a cache entry is created. 
        /// </summary>
        /// <typeparam name="T">The type to convert the object to.</typeparam>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="expiration"> The expiration </param>
        /// <param name="executor">The function call that returns the data.</param>
        public T EnsureWithExactLifetime<T>(string key, TimeSpan expiration, Func<T> executor)
        {
            return EnsureCache(executor, key, expiration, (cacheKey, result, expirationTime) =>
            {
                AddWithExactLifetime(cacheKey, result, expirationTime);
            });
        }

        /// <summary>
        /// Retrieves the specified key from the cache. If no value exists, a cache entry is created. 
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="expiration"> The sliding expiration </param>
        /// <param name="executor">The function call that returns the data.</param>
        public T EnsureWithSlidingLifetime<T>(string key, TimeSpan expiration, Func<T> executor)
        {
            return EnsureWithExactLifetime<T>(key, expiration, executor);
        }

        /// <summary>
        /// Update the cache with the specified value, if the cache does not exist, it is created.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="expiration">The expiration timeout as a timespan. This is a sliding value.</param>
        /// <param name="executor">The function call that returns the data.</param>
        public T UpdateWithSlidingLifetime<T>(string key, TimeSpan expiration, Func<T> executor)
        {
            var newValue = executor();

            try
            {
                _lockCache.EnterWriteLock();

                if (newValue != null)
                {
                    _cache.StringSet(key, Serialize(newValue), expiration);
                }
            }
            catch (Exception ex)
            {
                Trace.Write(string.Format(AppConstants.ExceptionTemplate, ex.ToString()));
            }
            finally
            {
                _lockCache.ExitWriteLock();
            }

            return newValue;
        }

        /// <summary>
        /// Retrieves the specified key from the cache. If no value exists, a cache entry is created. 
        /// </summary>
        /// <typeparam name="T">The type to convert the object to.</typeparam>
        /// <param name="executor">The function call that returns the data.</param>
        /// <param name="key">The key.</param>
        /// <param name="expiration">The expiration timeout as a timespan.</param>
        private T EnsureCache<T>(Func<T> executor, string key, TimeSpan expiration, Action<string, T, TimeSpan> addItemToCache)
        {
            var result = Get<T>(key);

            if (EqualityComparer<T>.Default.Equals(result, default(T)))
            {               
                try
                {
                    _lockCache.EnterWriteLock();

                    result = Get<T>(key);

                    if (EqualityComparer<T>.Default.Equals(result, default(T)))
                    {
                        var newValue = executor();

                        result = newValue;
                    }

                    if (result != null)
                    {
                        addItemToCache(key, result, expiration);
                    }
                }
                catch (Exception ex)
                {
                    Trace.Write(string.Format(AppConstants.ExceptionTemplate, ex.ToString()));
                }
                finally
                {
                    _lockCache.ExitWriteLock();
                }
            }

            return result;
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _lockCache.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        ~RedisCacheProvider()
        {
            Dispose(false);
        }
    }
}
