using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LinqTraining_Answer
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
            return ToSortedSetAsync(source, Comparer<TSource>.Default, cancellationToken);
        }

        public static async Task<SortedSet<TSource>> ToSortedSetAsync<TSource>(
            this IQueryable<TSource> source, IComparer<TSource> comparer,
            CancellationToken cancellationToken = default)
        {
            var list = new SortedSet<TSource>(comparer);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        #endregion

        #region ToSortedListAsync

        public static Task<SortedList<TKey, TSource>> ToSortedListAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
        {
            return ToSortedListAsync(source, keySelector, element => element, Comparer<TKey>.Default, cancellationToken);
        }

        public static Task<SortedList<TKey, TSource>> ToSortedListAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            return ToSortedListAsync(source, keySelector, element => element, comparer, cancellationToken);
        }

        public static Task<SortedList<TKey, TElement>> ToSortedListAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
        {
            return ToSortedListAsync(source, keySelector, elementSelector, Comparer<TKey>.Default, cancellationToken);
        }
        
        public static async Task<SortedList<TKey, TElement>> ToSortedListAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            
            var d = new SortedList<TKey, TElement>(comparer);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
        }

        #endregion

        #region ToSortedDictionaryAsync

        public static Task<SortedDictionary<TKey, TSource>> ToSortedDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
        {
            return ToSortedDictionaryAsync(source, keySelector, e => e, Comparer<TKey>.Default, cancellationToken);
        }

        public static Task<SortedDictionary<TKey, TSource>> ToSortedDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            return ToSortedDictionaryAsync(source, keySelector, e => e, comparer, cancellationToken);
        }

        public static Task<SortedDictionary<TKey, TElement>> ToSortedDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
        {
            return ToSortedDictionaryAsync(source, keySelector, elementSelector, comparer: null, cancellationToken);
        }

        public static async Task<SortedDictionary<TKey, TElement>> ToSortedDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            var d = new SortedDictionary<TKey, TElement>(comparer);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
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