using System;
using System.Linq.Expressions;

namespace Core.Common.Utils
{
    public static class PropertySupport
    {
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            var expression = propertyExpression.Body as MemberExpression;
            if (expression == null)
            {
                throw new NotSupportedException("Invalid expression passed. Only property member should be selected.");
            }

            return expression.Member.Name;
        }

    }
}