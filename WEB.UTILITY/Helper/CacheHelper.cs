using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Helper;

public class CacheHelper
{
    public static async Task<string> GetCacheKey(HttpContext context)
    {
        return $"{context.Request.Path.GetHashCode()
            }:{(context.Connection.RemoteIpAddress?.ToString() ?? "unknown").GetHashCode()
            }:{context.Request.Headers.UserAgent.GetHashCode()
            }:{context.Request.Headers.Authorization.GetHashCode()
            }:{(await StreamReaderHelper.ReadRequestBodyAsync(context)).GetHashCode()}";
    }
}
