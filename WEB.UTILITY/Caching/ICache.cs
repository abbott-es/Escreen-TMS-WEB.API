using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Caching
{
    public interface ICache
    {
        Task Set<T>(string key, T value, TimeSpan expiresIn);
        Task Set<T>(string key, T value);
        Task<Option<T>> Get<T>(string key);
        Task Remove(string key);
        Task RemoveByPrefix(string keyPrefix);
    }
}
