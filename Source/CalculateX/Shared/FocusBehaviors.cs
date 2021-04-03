using System.Windows;
using System.Windows.Input;

namespace Shared
{
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
	public class FocusFirstBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
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
	/// This behavior sets focus to the associated objet when it's loaded.
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
	public class FocusOnLoadBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
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
}
