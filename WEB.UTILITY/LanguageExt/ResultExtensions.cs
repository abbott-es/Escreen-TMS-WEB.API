using LanguageExt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.LanguageExt
{
    public static class ResultExtensions
    {
        public static L Value<L>(this Result<L> result)
        {
            return result.IfFail(ex => throw new InvalidOperationException("Result<T> failed", ex));
        }
    }
}
