﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DSFramework.Linq
{
    public static class PredicateBuilder
    {
        /// <summary>
        ///     Represents the parameter rebinder used for rebinding the parameters
        ///     for the given expressions. This is part of the solution which solves
        ///     the expression parameter problem when going to Entity Framework.
        ///     For more information about this solution please refer to
        ///     http://blogs.msdn.com/b/meek/archive/2008/05/02/linq-to-entities-combining-predicates.aspx.
        /// </summary>
        private class RebindParameterVisitor : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            public RebindParameterVisitor(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (_map.TryGetValue(p, out var replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
                => new RebindParameterVisitor(map).Visit(exp);
        }

        public static Expression<Func<T, bool>> True<T>() => f => true;

        public static Expression<Func<T, bool>> False<T>() => f => false;

        /// <summary>
        ///     Combines two given expressions by using the AND semantics.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="first">The first part of the expression.</param>
        /// <param name="second">The second part of the expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
            => first.Compose(second, Expression.AndAlso);

        /// <summary>
        ///     Combines two given expressions by using the OR semantics.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="first">The first part of the expression.</param>
        /// <param name="second">The second part of the expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
            => first.Compose(second, Expression.OrElse);

        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = RebindParameterVisitor.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
    }
}