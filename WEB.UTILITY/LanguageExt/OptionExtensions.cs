using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.LanguageExt
{
    public static class OptionExtensions
    {
        public static T Value<T>(this Option<T> option)
        {
            if (!option.IsSome)
                throw new InvalidOperationException("Option does not have value");

            return option.IfNoneUnsafe(null);
        }

        public static Option<T> ToOption<T>(this T value) where T : class
        {
            return value != null ? Option<T>.Some(value) : Option<T>.None;
        }

        public static Task<Option<T>> Flatten<T>(this OptionAsync<Option<T>> value)
        {
            return value.MatchAsync(v => v, () => default);
        }
    }
}
