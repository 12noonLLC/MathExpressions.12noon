using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Shared;

/// <summary>
/// This behavior causes the ListBox to scroll to the end on load.
/// This is especially helpful if the ListBox is in a DataTemplate
/// used by a TabControl.
/// </summary>
/// <example>
/// 1. Install NuGet Microsoft.Xaml.Behaviors.
/// 2. Add namespaces to the Window:
///	xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
///	xmlns:shared="clr-namespace:Shared"
/// 3. Add the behavior to the ListBox to get focus:
///	<DataTemplate x:Key="TemplateWorkspace">
///		<ListBox>
///			<i:Interaction.Behaviors>
///				<shared:ListBoxScrollToEndBehavior />
///			</i:Interaction.Behaviors>
///	...
///	</DataTemplate>
///	<TabControl ContentTemplate="{StaticResource TemplateWorkspace}">
///	...
///	</TabControl>
/// </example>
public class ListBoxScrollToEndBehavior : Behavior<ListBox>
{
	protected override void OnAttached()
	{
		/// Handle the FIRST time the ListBox is displayed with a collection.
		AssociatedObject.Loaded += AssociatedObject_Loaded;

		/// Handle the SUBSEQUENT times the ListBox is displayed with a collection
		/// (for the first time) and when a new item is added to ItemsSource.
		CollectionViewSource.GetDefaultView(AssociatedObject.Items).CollectionChanged += AssociatedObjectItems_CollectionChanged;
	}

	protected override void OnDetaching()
	{
		CollectionViewSource.GetDefaultView(AssociatedObject.Items).CollectionChanged -= AssociatedObjectItems_CollectionChanged;
		AssociatedObject.Loaded -= AssociatedObject_Loaded;
	}

	private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
	{
		ListBoxScrollToEnd(AssociatedObject);
	}

	private void AssociatedObjectItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		/// We want to scroll to the end on both Reset and Add.
		/// We don't care about the other actions.
		/// If we want to make some optional, we can add parameters to the behavior: Add="False"
		ListBoxScrollToEnd(AssociatedObject);
	}

	public static void ListBoxScrollToEnd(ListBox listBox)
	{
		if (listBox.Items.Count == 0)
		{
			return;
		}

		var lastItem = listBox.Items.GetItemAt(listBox.Items.Count - 1);
		listBox.ScrollIntoView(lastItem);
	}
}
