using System.Linq.Expressions;

namespace WEB.UTILITY.Helper
{
    public static class LambdaBuilder
    {
        public static Expression<Func<TDto, object>>[] BuildNavigationExpressions<TDto>(string[] propertyNames)
        {
            var expressions = new List<Expression<Func<TDto, object>>>();

            foreach (var propName in propertyNames)
            {
                var parameter = Expression.Parameter(typeof(TDto), "x");
                var property = Expression.Property(parameter, propName);
                var converted = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<TDto, object>>(converted, parameter);
                expressions.Add(lambda);
            }

            return expressions.ToArray();
        }
    }
}