using System;
using System.Collections.Generic;
using System.Linq;

namespace LINQTraining
{
    /// <remarks>
    /// <see cref="Enumerable.ToHashSet{TSource}"/>や
    /// <see cref="Enumerable.ToDictionary{TSource,TKey,TElement}"/>を真似しています。
    /// </remarks>
    internal static class EnumerableExtensions
    {
        #region ToSortedSet

        public static SortedSet<TSource> ToSortedSet<TSource>(this IEnumerable<TSource> source)
        {
            throw new NotImplementedException();
        }
        
        public static SortedSet<TSource> ToSortedSet<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ToSortedList

        public static SortedList<TKey, TSource> ToSortedList<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TKey : notnull
        {
            throw new NotImplementedException();
        }

        public static SortedList<TKey, TSource> ToSortedList<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            throw new NotImplementedException();
        }

        public static SortedList<TKey, TElement> ToSortedList<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector) where TKey : notnull
        {
            throw new NotImplementedException();
        }
        
        public static SortedList<TKey, TElement> ToSortedList<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ToSortedDictionary

        public static SortedDictionary<TKey, TSource> ToSortedDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TKey : notnull
        {
            throw new NotImplementedException();
        }

        public static SortedDictionary<TKey, TSource> ToSortedDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            throw new NotImplementedException();
        }

        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector) where TKey : notnull
        {
            throw new NotImplementedException();
        }
        
        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            throw new NotImplementedException();
        }

        #endregion
        
        #region DistinctBy

        public static IEnumerable<TSource> DistinctBy<TKey, TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            throw new NotImplementedException();
        }
        
        public static IEnumerable<TSource> DistinctBy<TKey, TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            throw new NotImplementedException();
        }
    }
}