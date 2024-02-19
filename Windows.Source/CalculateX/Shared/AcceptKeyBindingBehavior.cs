using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shared;

/// <summary>
/// This behavior is for the situation where a TextBox is preventing KeyBindings
/// from bubbling up to the containing UserControl (or TabControl, etc.).
/// This behavior previews keystrokes and, if it finds a key binding, invokes it.
/// The behavior should be added to the element that defines the KeyBindings.
/// </summary>
/// <remarks>
/// https://stackoverflow.com/questions/12941707/keybinding-in-usercontrol-doesnt-work-when-textbox-has-the-focus
/// </remarks>
/// <example>
/// <Window xmlns:i="http://schemas.microsoft.com/xaml/behaviors">
///	<KeyBindings>
///		...
///	</KeyBindings>
///	<i:Interaction.Behaviors>
///		<shared:AcceptKeyBindingBehavior />
///	</i:Interaction.Behaviors>
///	<TabControl>
///		<TextBox></TextBox>
///	</TabControl>
/// </Window>
/// </example>
public class AcceptKeyBindingBehavior : Behavior<FrameworkElement>
{
	private FrameworkElement TheControl => AssociatedObject;


	protected override void OnAttached()
	{
		TheControl.PreviewKeyDown += TheControl_PreviewKeyDown;

		/* MOUSE GESTURES
		 * Handle PreviewMouseUp and PreviewMouseDoubleClick
				InputBinding foundBinding = element
					.InputBindings
				  .OfType<MouseBinding>()
				  .FirstOrDefault(mb => mb.Gesture.Matches(sender, eventArgs));
		 *
		 * If we add support for mouse gestures, we should add two bool properties
		 * to allow the developer to enable/disable keyboard and/or mouse.
		 */
	}

	protected override void OnDetaching()
	{
		TheControl.PreviewKeyDown -= TheControl_PreviewKeyDown;
	}

	private void TheControl_PreviewKeyDown(object /*FrameworkElement*/ sender, KeyEventArgs e)
	{
#if DEBUG
		// Do not break if the user is pressing a modifier.
		if ((e.Key == Key.LeftShift) || (e.Key == Key.RightShift) ||
				(e.Key == Key.LeftCtrl) || (e.Key == Key.RightCtrl) ||
				(e.Key == Key.LeftAlt) || (e.Key == Key.RightAlt))
		{
			return;
		}
#endif

		var element = (FrameworkElement)sender;
		Debug.Assert(element == AssociatedObject);

		var foundBinding = element
			.InputBindings
			.OfType<KeyBinding>()
			.FirstOrDefault(kb => kb.Gesture.Matches(sender, e));
			// Same as ((kb.Key == e.Key) && (kb.Modifiers == e.KeyboardDevice.Modifiers))

		if (foundBinding is not null)
		{
			e.Handled = true;

			if (foundBinding.Command.CanExecute(foundBinding.CommandParameter))
			{
				foundBinding.Command.Execute(foundBinding.CommandParameter);
			}
		}
	}
}
