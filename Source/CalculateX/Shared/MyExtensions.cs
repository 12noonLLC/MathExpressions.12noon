using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Shared;

/// 
/// (Note: We can't add static extension methods.)
/// 
public static class MyExtensions
{
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
