using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sample
{
    public enum SortType
    {
        None,
        Attack,
        Defense,
    }

    public enum OrderType
    {
        None,
        Ascending,
        Descending
    }

    public enum FilterType
    {
        None,
        Language,
        Elements,
        DoubleElements,
    }

    public static class SortLogics
    {
        public static IEnumerable<TItem> Sorting<TItem, TValue>(this IEnumerable<TItem> items, OrderType orderType, Func<TItem, TValue> predicate)
        {
            switch (orderType)
            {
                case OrderType.Ascending:
                    return items.OrderBy(predicate);
                case OrderType.Descending:
                    return items.OrderByDescending(predicate);
                default:
                    return items;
            }
        }
    }

    public static class CharacterSort
    {
        public static IEnumerable<TItem> Sorting<TItem>(this IEnumerable<TItem> items, SortType sortType, OrderType orderType)
            where TItem : ISortAndFilterCharacter
        {
            switch (sortType)
            {
                case SortType.Attack:
                    return items.Sorting(orderType, x => x.Atk);
                case SortType.Defense:
                    return items.Sorting(orderType, x => x.Def);
                case SortType.None:
                    return items.CustomSorting();
                default:
                    return items;
            }
        }

        private static IEnumerable<TItem> CustomSorting<TItem>(this IEnumerable<TItem> items)
            where TItem : ISortAndFilterCharacter
        {
            return items
                .OrderBy(x => x.Atk)
                .ThenBy(x => x.Def);
        }
    }

    public static class FilterLogics
    {
        /// <summary>
        /// 通常フィルタ
        /// </summary>
        public static IEnumerable<TItem> Filtering<TItem>(this IEnumerable<TItem> items, FilterType filterType, Dictionary<FilterType, HashSet<int>> filters, Func<TItem, int> predicate)
        {
            if (!filters.TryGetValue(filterType, out var values))
                return items;

            return items.Where(x => values.Contains(predicate(x)));
        }

        /// <summary>
        /// 範囲フィルタ
        /// </summary>
        public static IEnumerable<TItem> FilteringRange<TItem>(this IEnumerable<TItem> items, FilterType filterType, Dictionary<FilterType, (int Min, int Max)> filters, Func<TItem, int> predicate)
        {
            if (!filters.TryGetValue(filterType, out var range))
                return items;

            return items.Where(x =>
            {
                var value = predicate(x);
                return range.Min <= value && value <= range.Max;
            });
        }

        /// <summary>
        /// 完全一致
        /// </summary>
        public static IEnumerable<TItem> FilteringByExactMatch<TItem>(this IEnumerable<TItem> items, FilterType filterType, Dictionary<FilterType, HashSet<int>> filters, Func<TItem, int[]> predicate)
        {
            if (!filters.TryGetValue(filterType, out var filterValues))
                return items;

            return items.Where(x =>
            {
                var values = predicate(x);
                return values.All(y => filterValues.Contains(y));
            });
        }

        /// <summary>
        /// 部分一致
        /// </summary>
        public static IEnumerable<TItem> FilteringByPartialMatch<TItem>(this IEnumerable<TItem> items, FilterType filterType, Dictionary<FilterType, HashSet<int>> filters, Func<TItem, int[]> predicate)
        {
            if (!filters.TryGetValue(filterType, out var filterValues))
                return items;

            return items.Where(x =>
            {
                var values = predicate(x);
                return filterValues.All(y => values.Contains(y));
            });
        }
    }

    public static class CharacterFilter
    {
        public static IEnumerable<TItem> Filtering<TItem>(this IEnumerable<TItem> items, Dictionary<FilterType, HashSet<int>> filters)
            where TItem : ISortAndFilterCharacter
        {
            return items
                .Filtering(FilterType.Language, filters, x => x.Atk)
                .Filtering(FilterType.Elements, filters, x => x.Def)
                .FilteringByExactMatch(FilterType.DoubleElements, filters, x => new[] { (int)Elements.Fire, (int)Elements.Water, (int)Elements.Wind })
                .FilteringByPartialMatch(FilterType.DoubleElements, filters, x => new[] { (int)Elements.Fire, (int)Elements.Water });
        }
    }

    #region SortSample

    public class Character
    {
        public string Name { get; set; }
    }

    public interface ISortAndFilterCharacter
    {
        public Character Character { get; }
        public int Atk { get; }
        public int Def { get; }
    }

    public class CharacterData : ISortAndFilterCharacter
    {
        public Character Character { get; }

        public int Atk => 100;
        public int Def => 100;

        public CharacterData(Character character)
        {
            Character = character;
        }
    }

    public class NpcCharacterData : ISortAndFilterCharacter
    {
        public Character Character { get; }

        public int Atk => 0;

        public int Def => 0;

        public NpcCharacterData(Character character)
        {
            Character = character;
        }
    }

    public class TestResultView : MonoBehaviour
    {
        [SerializeField] private TestResultScrollView _testResultScrollView;

        [SerializeField] private JumpResultScrollView _jumpScrollView;

        private List<ISortAndFilterCharacter> _testResults;
        private List<ISortAndFilterCharacter> _jumpResults;

        private void Initialize()
        {
            _testResults.Clear();

            _jumpResults.Clear();
        }

        private void UpdateTestResultView()
        {
            var testResults = _testResults
                .Sorting(SortType.Attack, OrderType.Descending)
                .Cast<CharacterData>()
                .ToArray();
            _testResultScrollView.UpdateListView(testResults);

            var jumpResults = _jumpResults
                .Sorting(SortType.Attack, OrderType.Descending)
                .Cast<CharacterData>()
                .ToArray();
            _jumpScrollView.UpdateListView(jumpResults);
        }
    }

    public class TestResultScrollView //: ScrollViewBase 
    {
        public void UpdateListView(CharacterData[] testResults)
        {
            // リスト描画ライブラリ
            // FancyScrollView
            // EnhancedScroller
            // base.UpdateListView(testResults);
        }
    }

    public class JumpResultScrollView //: ScrollViewBase 
    {
        public void UpdateListView(CharacterData[] jumpScores)
        {
            // リスト描画ライブラリ
            // FancyScrollView
            // EnhancedScroller
            // base.UpdateListView(testResults);
        }
    }

    #endregion

    #region FilterSample

    public enum Elements
    {
        Fire = 0,
        Water = 1,
        Wind = 2,
    }

    public class TestFilter
    {
        private readonly Dictionary<FilterType, HashSet<int>> _filters = new();

        public void Initialize()
        {
            // UIダイアログなどで動的に設定する
            var elements = new HashSet<int>();
            elements.Add((int)Elements.Fire);
            elements.Add((int)Elements.Wind);

            var filters = new Dictionary<FilterType, HashSet<int>>();
            filters.Add(FilterType.Elements, elements);
        }

        // FilterType.Elements, 0
        // FilterType.Elements, 2

        // FilterType.Language 0
        // FilterType.Language 1
        public void OnChangedFilterSettings(FilterType filterType, int filterValue)
        {
            if (!_filters.TryGetValue(filterType, out var values))
            {
                _filters.Add(filterType, new HashSet<int> { filterValue });
                return;
            }

            if (!values.Add(filterValue))
            {
                values.Remove(filterValue);

                if (!values.Any())
                {
                    _filters.Remove(filterType);
                }
            }

            _filters[filterType] = values;
        }

        private void UpdateListView()
        {
            var list = new List<CharacterData>();
            list.Add(new CharacterData(new Character()));
            list.Add(new CharacterData(new Character()));
            list.Add(new CharacterData(new Character()));

            var sortedAndFiltered = list
                .Sorting(SortType.Defense, OrderType.Ascending)
                .Filtering(_filters)
                .ToArray();

            // _scrollView.UpdateListView(sortedAndFiltered);
        }
    }

    #endregion
}