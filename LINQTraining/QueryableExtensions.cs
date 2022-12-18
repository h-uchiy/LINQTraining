using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LINQTraining
{
    /// <remarks>
    /// <see cref="EntityFrameworkQueryableExtensions.ToListAsync{TSource}"/>や
    /// <see cref="EntityFrameworkQueryableExtensions.ToDictionaryAsync{TSource,TKey}"/>を真似してください。</remarks>
    internal static class QueryableExtensions
    {
        #region ToSortedSetAsync

        public static Task<SortedSet<TSource>> ToSortedSetAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public static Task<SortedSet<TSource>> ToSortedSetAsync<TSource>(
            this IQueryable<TSource> source, IComparer<TSource> comparer,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ToSortedListAsync

        public static Task<SortedList<TKey, TSource>> ToSortedListAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public static Task<SortedList<TKey, TSource>> ToSortedListAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public static Task<SortedList<TKey, TElement>> ToSortedListAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        
        public static Task<SortedList<TKey, TElement>> ToSortedListAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ToSortedDictionaryAsync

        public static Task<SortedDictionary<TKey, TSource>> ToSortedDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public static Task<SortedDictionary<TKey, TSource>> ToSortedDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public static Task<SortedDictionary<TKey, TElement>> ToSortedDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public static Task<SortedDictionary<TKey, TElement>> ToSortedDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
        
        #region DistinctBy

        public static IQueryable<TSource> DistinctBy<TKey, TSource>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            throw new NotImplementedException();
        }
        
        public static IQueryable<TSource> DistinctBy<TKey, TSource>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static IQueryable<TSource[]> Chunk<TSource>(this IQueryable<TSource> source, int size)
        {
            throw new NotImplementedException();
        }
    }
}