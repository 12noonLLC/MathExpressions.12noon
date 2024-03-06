using System;
using System.Collections.ObjectModel;

namespace Shared;

public static class MyExtensions
{
	/// <summary>
	/// Insert an item into a sorted position in an ObservableCollection class.
	/// </summary>
	/// <remarks>
	/// This could be made much more efficient, but our collection will be small.
	/// </remarks>
	/// <param name="list">sorted collection</param>
	/// <param name="item">item being added</param>
	/// <param name="comparator">Lambda that returns true if the first item is less than the second</param>
	public static void AddSorted<T>(this ObservableCollection<T> list,
												T item,
												Func<T, T, bool> comparator)
	{
		int ixAdd = FindSortedIndex(list, item, comparator);
		list.Insert(ixAdd, item);
	}

	public static int FindSortedIndex<T>(this ObservableCollection<T> list,
														T item,
														Func<T, T, bool> comparator)
	{
		if (list.Count == 0)
		{
			return 0;
		}

		foreach (int ix in Enumerable.Range(0, list.Count))
		{
			if (comparator(item, list[ix]))
			{
				return ix;
			}
		}

		return list.Count;
	}
}
