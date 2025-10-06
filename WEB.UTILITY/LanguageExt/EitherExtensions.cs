using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.LanguageExt
{
    public static class EitherExtensions
    {
        public static L GetLeft<L, R>(this Either<L, R> either) =>
            either.Match(
                Left: l => l,
                Right: _ => throw new InvalidOperationException("Either does not contain a Left value")
            );

        public static R GetRight<L, R>(this Either<L, R> either) =>
            either.Match(
                Left: _ => throw new InvalidOperationException("Either does not contain a Right value"),
                Right: r => r
            );


        public static Either<L, R> Flatten<L, R>(this Either<L, Option<R>> value, L defaultValue)
        {
            return value.Sequence()
                .ToEither(defaultValue)
                .Flatten();
        }

        public static EitherAsync<L, R> Flatten<L, R>(this Either<L, OptionAsync<R>> value, L defaultValue)
        {
            return value.ToAsync()
                .Sequence()
                .ToEither(defaultValue)
                .Flatten();
        }
    }
}
