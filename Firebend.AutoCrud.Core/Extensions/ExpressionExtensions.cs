using System;
using System.Linq;
using System.Linq.Expressions;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T1, TResult>> FixParam<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T2 parameterValue)
        {
            var parameterToRemove = expression.Parameters.ElementAt(1);
            var replacer = new ReplaceExpressionVisitor(parameterToRemove, Expression.Constant(parameterValue, typeof(T2)));
            return Expression.Lambda<Func<T1, TResult>>(replacer.Visit(expression.Body), expression.Parameters.Where(p => p != parameterToRemove));
        }

        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            if (expr1?.Body == null)
                return expr2;

            if (expr2?.Body == null)
                return expr1;

            var parameter = Expression.Parameter(typeof(T), "x");

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            if (left == null)
            {
                return expr2;
            }

            if (right == null)
            {
                return expr1;
            }

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right), parameter);
        }

        private class ReplaceExpressionVisitor
            : ExpressionVisitor
        {
            private readonly Expression _newValue;
            private readonly Expression _oldValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                {
                    return _newValue;
                }

                return base.Visit(node);
            }
        }
    }
}
