using System.Linq.Expressions;

namespace WEB.UTILITY.Specs
{
    public class AndSpec<T> : Spec<T>
    {
        private readonly Spec<T> _left;
        private readonly Spec<T> _right;

        public AndSpec(Spec<T> left, Spec<T> right)
        {
            _right = right;
            _left = left;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return _left.ToExpression().And(_right.ToExpression());
        }
    }
}
