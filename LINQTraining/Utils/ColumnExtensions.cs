using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace LINQTraining.Utils
{
    public static class ColumnExtensions
    {
        private static ConcurrentDictionary<string, Action<object>> cache = new ConcurrentDictionary<string, Action<object>>();

        public static IEnumerable<TEntity> FillColumn<TEntity>(this IEnumerable<TEntity> entity) where TEntity : notnull
        {
            return entity.Select(x => x.FillColumn());
        }

        public static TEntity FillColumn<TEntity>(this TEntity entity) where TEntity : notnull
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return (TEntity)FillColumnCore(entity);
        }
        
        private static object FillColumnCore(object entity)
        {
            var type = entity.GetType();
            var typeName = type.FullName ?? throw new InvalidOperationException($"Cannot get {type}.FullName");
            var columnFiller = cache.GetOrAdd(typeName, _ =>
            {
                var untypedExpr = Expression.Parameter(typeof(object), "untyped");
                var entityExpr = Expression.Variable(entity.GetType(), "entity");
                // entity = (TEntity)untyped
                var assignEntityExpr =
                    Expression.Assign(
                        entityExpr,
                        Expression.Convert(
                            untypedExpr,
                            type));

                // entity.Column### = "ColumnValue###"
                var properties = type.GetProperties()
                    .Where(x => Regex.IsMatch(x.Name, @"Column\d{3}"));
                var assigns = properties.Select(x =>
                    Expression.Assign(
                        Expression.Property(entityExpr, x),
                        Expression.Constant($"ColumnValue{x.Name[6..]}", typeof(string))
                    ));

                // entity = (TEntity)untyped;
                // entity.Column000 = "ColumnValue000";
                // entity.Column001 = "ColumnValue001";
                // ...
                // entity.Column099 = "ColumnValue099";
                var block = Expression.Block(
                    new[] { entityExpr },
                    assigns.Prepend(assignEntityExpr)
                );
                var lambda = Expression.Lambda<Action<object>>(block, untypedExpr);
                return lambda.Compile();
            });
            columnFiller(entity);
            return entity;
        }
    }
}
