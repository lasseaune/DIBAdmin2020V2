using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace StaticFilesAPI.Services
{
    public class CacheService : ICacheService
    {
        private readonly int CacheTimeout = (3600 * 8); // 8 hour
        private readonly IMemoryCache _cache;
        public CacheService(IMemoryCache cache)
        {
            _cache = cache;

            string targetDirectory = @"d:\_StaticFilesAPI\9D30F938-B261-4289-8CE0-126E7C007A1B";
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                FileStream f = File.OpenRead(fileName);
                string name = "9D30F938-B261-4289-8CE0-126E7C007A1B/" + fileName.Split('\\').LastOrDefault();
                _cache.Set<FileStream>(name, f);
            }
            
        }

        public T Get<T>(string cacheId, Func<T> getItemCallback) where T : class
        {
            return GetCache(cacheId, getItemCallback, CacheTimeout);
        }

        public T Get<T>(string cacheId, Func<T> getItemCallback, int timeout) where T : class
        {
            return GetCache(cacheId, getItemCallback, timeout);
        }

        public T Get<T>(string cacheId)
        {
            object value = null;

            if (!_cache.TryGetValue(cacheId, out value))
                return default(T);

            return (T)value;
        }

        public void Set<T>(string cacheId, object item) where T : class
        {
            SetCache(cacheId, item, CacheTimeout);
        }

        public void Set<T>(string cacheId, object item, int timeout) where T : class
        {
            SetCache(cacheId, item, timeout);
        }

        private T GetCache<T>(string cacheId, Func<T> getItemCallback, int timeout) where T : class
        {
            var item = _cache.Get(cacheId) as T;
            if (item == null)
            {
                item = getItemCallback();

                if (item != null)
                    SetCache(cacheId, item, timeout);
            }
            return item;
        }

        private void SetCache(string cacheId, object item, int timeout)
        {
            //HttpContext.Current._cache.Insert(cacheId, item, null, DateTime.Now.AddSeconds(timeout), _cache.NoSlidingExpiration);
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(timeout),
                AbsoluteExpiration = DateTime.Now.AddHours(24),
                Priority = CacheItemPriority.NeverRemove
            };

            _cache.Set(cacheId, item, options);
        }

        public void RemoveCache(string cacheId)
        {
            _cache.Remove(cacheId);
        }

    }
}
