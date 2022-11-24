using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LINQTraining.Utils
{
    public static class BlahExtensions
    {
        private static Dictionary<string, Action<object>> cache = new Dictionary<string, Action<object>>();

        public static TEntity FillBlah<TEntity>(this TEntity entity)
        {
            return (TEntity)_(entity);
            
            static object _(object entity)
            {
                var typeName = entity.GetType().FullName;
                if (!cache.ContainsKey(typeName!))
                {
                    // fillExpression: fill = '###..#'
                    var fillExpr = Expression.Variable(typeof(string), "fill");
                    var assignFillExpr = Expression.Assign(fillExpr, Expression.Constant(new string('#', 10), typeof(string)));
                
                    // assignEntity: entity = (TEntity)untyped
                    var untypedEntityExpr = Expression.Parameter(typeof(object), "untyped");
                    var entityExpr = Expression.Variable(entity.GetType(), "entity");
                    var assignEntityExpr = Expression.Assign(entityExpr, Expression.Convert(untypedEntityExpr, entity.GetType()));
                
                    // assigns: ((TEntity)entity).BlahXXXX = fill
                    var assigns = from propInfo in entity.GetType().GetProperties()
                        where propInfo.Name.Contains("Blah")
                        select Expression.Assign(
                            Expression.Property(entityExpr, propInfo),
                            fillExpr);
                    var block = Expression.Block(
                        new[] { fillExpr, entityExpr },
                        assigns.Prepend(assignEntityExpr).Prepend(assignFillExpr)
                    );
                    var lambda = Expression.Lambda<Action<object>>(block, untypedEntityExpr);
                    cache[typeName] = lambda.Compile();
                }

                cache[typeName](entity);
                return entity;
            }
        }
    }
}
