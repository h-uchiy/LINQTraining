using System;
using System.Linq;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace LinqTraining_Answer
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
            return ToSortedSet(source, Comparer<TSource>.Default);
        }
        
        public static SortedSet<TSource> ToSortedSet<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            
            return new SortedSet<TSource>(source, comparer);
        }

        #endregion

        #region ToSortedList

        public static SortedList<TKey, TSource> ToSortedList<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TKey : notnull
        {
            return ToSortedList(source, keySelector, element => element, Comparer<TKey>.Default);
        }

        public static SortedList<TKey, TSource> ToSortedList<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            return ToSortedList(source, keySelector, element => element, comparer);
        }

        public static SortedList<TKey, TElement> ToSortedList<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector) where TKey : notnull
        {
            return ToSortedList(source, keySelector, elementSelector, Comparer<TKey>.Default);
        }
        
        public static SortedList<TKey, TElement> ToSortedList<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            
            var d = new SortedList<TKey, TElement>(comparer);
            foreach (var element in source)
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
        }

        #endregion

        #region ToSortedDictionary

        public static SortedDictionary<TKey, TSource> ToSortedDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TKey : notnull
        {
            return ToSortedDictionary(source, keySelector, element => element, Comparer<TKey>.Default);
        }

        public static SortedDictionary<TKey, TSource> ToSortedDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            return ToSortedDictionary(source, keySelector, element => element, comparer);
        }

        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector) where TKey : notnull
        {
            return ToSortedDictionary(source, keySelector, elementSelector, Comparer<TKey>.Default);
        }
        
        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IComparer<TKey> comparer) where TKey : notnull
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            
            var d = new SortedDictionary<TKey, TElement>(comparer);
            foreach (var element in source)
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
        }

        #endregion

        #region DistinctBy

        public static IEnumerable<TSource> DistinctBy<TKey, TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return DistinctBy(source, keySelector, EqualityComparer<TKey>.Default);
        }
        
        public static IEnumerable<TSource> DistinctBy<TKey, TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            
            var hashSet = new HashSet<TKey>(comparer);
            foreach (var item in source)
            {
                if (hashSet.Add(keySelector(item)))
                {
                    yield return item;
                }
            }
        }

        #endregion

        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));

            using var e = source.GetEnumerator();
            while (e.MoveNext())
            {
                var chunk = new TSource[size];
                chunk[0] = e.Current;

                for (var i = 1; i < size; i++)
                {
                    if (!e.MoveNext())
                    {
                        Array.Resize(ref chunk, i);
                        yield return chunk;
                        yield break;
                    }

                    chunk[i] = e.Current;
                }

                yield return chunk;
            }
        }
    }
}