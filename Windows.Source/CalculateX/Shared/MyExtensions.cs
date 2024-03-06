using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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


	/// <summary>
	/// Find a child of the specified object that matches the templated type.
	/// </summary>
	/// <seealso cref="http://msdn.microsoft.com/en-us/library/system.windows.frameworktemplate.findname.aspx"/>
	/// <typeparam name="TChildItem">The type of object we're looking for</typeparam>
	/// <param name="obj">The element whose children are to be searched</param>
	/// <returns>A reference to the matching child object, if any</returns>
	public static TChildItem? FindVisualChild<TChildItem>(DependencyObject obj)
		where TChildItem : DependencyObject
	{
		foreach (int i in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(obj)))
		{
			DependencyObject child = VisualTreeHelper.GetChild(obj, i);
			if (child is not null and TChildItem childItem)
			{
				return childItem;
			}

			Debug.Assert(child is not null);
			TChildItem? childOfChild = FindVisualChild<TChildItem>(child);
			if (childOfChild is not null)
			{
				return childOfChild;
			}
		}

		return null;
	}
}
