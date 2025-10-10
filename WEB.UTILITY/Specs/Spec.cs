using System.Linq.Expressions;

namespace WEB.UTILITY.Specs
{
    public abstract class Spec<T>
    {
        public static Spec<T> True { get; } = new TrueSpec<T>();
        public abstract Expression<Func<T, bool>> ToExpression();

        public bool IsSatisfiedBy(T entity)
        {
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }

        public Spec<T> And(Spec<T> other)
        {
            return new AndSpec<T>(this, other);
        }

        public Spec<T> And(Spec<T> other, Func<bool> cond)
        {
            return cond() ? new AndSpec<T>(this, other) : this;
        }

        private class TrueSpec<U> : Spec<U>
        {
            public override Expression<Func<U, bool>> ToExpression() => (value) => true;
        }
    }
}
