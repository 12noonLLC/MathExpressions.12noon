using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shared;

/// <summary>
/// This behavior causes the window to set focus to the first control on load.
/// </summary>
/// <example>
/// 1. Install NuGet Microsoft.Xaml.Behaviors.
/// 2. Add namespaces to the Window:
///	xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
///	xmlns:shared="clr-namespace:Shared"
/// 3. Add the behavior to the Window to get focus:
///	<Window>
///		<i:Interaction.Behaviors>
///			<shared:FocusFirstBehavior />
///		</i:Interaction.Behaviors>
/// </example>
public class FocusFirstBehavior : Behavior<FrameworkElement>
{
	protected override void OnAttached()
	{
		AssociatedObject.Loaded += AssociatedObject_Loaded;
	}

	protected override void OnDetaching()
	{
		AssociatedObject.Loaded -= AssociatedObject_Loaded;
	}

	private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
	{
		AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
	}
}


/// <summary>
/// This behavior causes a TabControl to set focus to the named control when tab selection changes.
/// </summary>
/// <example>
///	<TabControl>
///		<i:Interaction.Behaviors>
///			<shared:FocusTabControlSelectionChangedBehavior ElementName="SomeTextBox" />
///		</i:Interaction.Behaviors>
///		<TabControl.ContentTemplate>
///			<DataTemplate>
///				<TextBox x:Name="SomeTextBox" />
///			</DataTemplate>
///		</TabControl.ContentTemplate>
///	</TabControl>
/// </example>
public class FocusTabControlSelectionChangedBehavior : Behavior<TabControl>
{
	public string ElementName
	{
		get => (string)GetValue(ElementNameProperty);
		set => SetValue(ElementNameProperty, value);
	}
	public static readonly DependencyProperty ElementNameProperty =
		DependencyProperty.Register(nameof(ElementName), typeof(string), typeof(FocusTabControlSelectionChangedBehavior));


	protected override void OnAttached()
	{
		AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
	}

	protected override void OnDetaching()
	{
		AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
	}

	private void AssociatedObject_SelectionChanged(object /*TabControl*/ sender, RoutedEventArgs e)
	{
		if (string.IsNullOrEmpty(ElementName))
		{
			return;
		}

		TabControl tabControl = (TabControl)sender;

		// Invoke this code to give the tab time to generate the tree.
		Dispatcher.BeginInvoke(() =>
		{
			FrameworkElement? focusElement = FindDataTemplateChild(tabControl, ElementName);

			/// Set focus to this control.
			focusElement?.Focus();
		});
	}

	private static FrameworkElement? FindDataTemplateChild(DependencyObject parentControl, string elementName)
	{
		if (string.IsNullOrEmpty(elementName))
		{
			return null;
		}

		/// Get the ContentPresenter for the control.
		ContentPresenter? contentPresenter = MyExtensions.FindVisualChild<ContentPresenter>(parentControl);
		ArgumentNullException.ThrowIfNull(contentPresenter);

		/// Get the data template for this presenter.
		DataTemplate dataTemplate = contentPresenter.ContentTemplate;

		/// Use this data template to search the presenter for an element with the specified name.
		FrameworkElement foundElement = (FrameworkElement)dataTemplate.FindName(elementName, contentPresenter);

		return foundElement;
	}
}


/// <summary>
/// This behavior sets focus to the associated object when it's loaded.
/// </summary>
/// <example>
/// 1. Install NuGet Microsoft.Xaml.Behaviors.
/// 2. Add namespaces to the Window:
///	xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
///	xmlns:shared="clr-namespace:Shared"
/// 3. Add the behavior to the control to get focus:
///	<ListBox>
///		<i:Interaction.Behaviors>
///			<shared:FocusOnLoadBehavior />
///		</i:Interaction.Behaviors>
/// </example>
public class FocusOnLoadBehavior : Behavior<FrameworkElement>
{
	protected override void OnAttached()
	{
		AssociatedObject.Loaded += AssociatedObject_Loaded;
	}

	protected override void OnDetaching()
	{
		AssociatedObject.Loaded -= AssociatedObject_Loaded;
	}

	private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
	{
		Keyboard.Focus(AssociatedObject);
	}
}
