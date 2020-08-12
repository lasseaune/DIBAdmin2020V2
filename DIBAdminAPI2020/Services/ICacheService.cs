using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Services
{
    public interface ICacheService
    {
        T Get<T>(string cacheId);
        T Get<T>(string cacheId, Func<T> getItemCallback) where T : class;
        T Get<T>(string cacheId, Func<T> getItemCallback, int timeout) where T : class;
        void RemoveCache(string cacheId);
        void Set<T>(string cacheId, object item) where T : class;
        void Set<T>(string cacheId, object item, int timeout) where T : class;
    }
}
