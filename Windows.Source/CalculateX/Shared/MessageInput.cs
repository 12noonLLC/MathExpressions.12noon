using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Shared;

/// <summary>
/// Display a small window to prompt the user to enter a string.
/// </summary>
/// <remarks>
/// https://stackoverflow.com/questions/8103743/wpf-c-sharp-inputbox
/// </remarks>
/// <example>
/// string newName = new Shared.MessageInput(main, "Enter new name:", "Rename Thing", CurrentName).ShowDialog() ?? CurrentName;
/// if ((newName == CurrentName) || string.IsNullOrWhiteSpace(newName))
/// {
/// 	// Name hasn't changed or is blank
/// 	return;
/// }
/// CurrentName = newName;
/// </example>
public class MessageInput
{
	private readonly Window TheWindow;
	private TextBox InputField { get; init; }

	private readonly string okButtonText = "OK";
	private readonly string cancelButtonText = "Cancel";


	/// <summary>
	///
	/// </summary>
	/// <param name="owner">Window that owns this prompt so it can be centered.</param>
	/// <param name="message">Label text of field user will enter.</param>
	/// <param name="title">Window title</param>
	/// <param name="initialText">Default text of field user will enter.</param>
	public MessageInput(Window owner, string message, string title, string initialText)
	{
		Button okButton = new()
		{
			Content = okButtonText,
			IsDefault = true,
			Margin = new(10),
			Padding = new(0, 4, 0, 4),
		};
		okButton.Click += (object sender, RoutedEventArgs e) => TheWindow!.DialogResult = true;
		Button cancelButton = new()
		{
			Content = cancelButtonText,
			IsCancel = true,
			Margin = new(10),
			Padding = new(0, 4, 0, 4),
		};
		UniformGrid gridButtons = new() { Columns = 2 };
		gridButtons.Children.Add(okButton);
		gridButtons.Children.Add(cancelButton);

		StackPanel stackPanel = new()
		{
			Margin = new(5),
			Orientation = Orientation.Vertical,
		};
		stackPanel.Children.Add(new Label()
		{
			Content = message,
			Margin = new(5, 0, 5, 0),
		});
		InputField = new TextBox()
		{
			Text = initialText,
			TextWrapping = TextWrapping.Wrap,
			Margin = new(10, 5, 10, 5),
			Padding = new(0, 2, 0, 2),
		};
		stackPanel.Children.Add(InputField);
		stackPanel.Children.Add(gridButtons);

		TheWindow = new()
		{
			Content = stackPanel,
			Title = title,
			Owner = owner,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			MinWidth = 200,
			MaxWidth = 1_000,
			SizeToContent = SizeToContent.WidthAndHeight,
			ResizeMode = ResizeMode.NoResize
		};

		InputField.Focus();
	}

	public string? ShowDialog()
	{
		return TheWindow.ShowDialog().GetValueOrDefault() ? InputField.Text : null;
	}
}
