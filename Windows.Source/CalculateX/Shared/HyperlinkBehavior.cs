using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Shared;

/// <summary>
/// 
/// </summary>
/// <example>
/// xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
/// 
/// <XXX.Resources>
/// 	<Style x:Key="StyleHyperlink">
/// 		<Style.Setters>
/// 			<Setter Property="TextBlock.TextDecorations" Value="Underline" />
/// 			<Setter Property="Control.Foreground" Value="Blue" />
/// 		</Style.Setters>
/// 	</Style>
/// </XXX.Resources>
/// 
/// <Label Style="StyleHyperlink" ...>
/// 	<i:Interaction.Behaviors>
/// 		<shared:HyperlinkBehavior Hyperlink="https://12noon.com" />
/// 	</i:Interaction.Behaviors>
/// </Label>
/// </example>
public class HyperlinkBehavior : Behavior<FrameworkElement>
{
	// Hyperlink to open when this control is clicked.
	public string Hyperlink
	{
		get => (string)GetValue(HyperlinkProperty);
		set => SetValue(HyperlinkProperty, value);
	}
	public static readonly DependencyProperty HyperlinkProperty =
		DependencyProperty.Register(nameof(Hyperlink), typeof(string), typeof(HyperlinkBehavior));


	private FrameworkElement TheControl => AssociatedObject;


	protected override void OnAttached()
	{
		TheControl.MouseLeftButtonDown += TheControl_MouseLeftButtonDown;
	}

	protected override void OnDetaching()
	{
		TheControl.MouseLeftButtonDown -= TheControl_MouseLeftButtonDown;
	}

	private void TheControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		Process.Start(new ProcessStartInfo(Hyperlink) { UseShellExecute = true });
		e.Handled = true;
	}
}


/// <summary>
/// 
/// </summary>
/// <example>
/// <Paragraph>
/// 	<Hyperlink NavigateUri="https://12noon.com">12noon
/// 		<i:Interaction.Behaviors>
/// 			<shared:HyperlinkBehaviorFlow />
/// 		</i:Interaction.Behaviors>
/// 	</Hyperlink>
/// </Paragraph>
/// </example>
public class HyperlinkBehaviorFlow : Behavior<Hyperlink>
{
	private Hyperlink TheControl => AssociatedObject;


	protected override void OnAttached()
	{
		TheControl.RequestNavigate += TheControl_RequestNavigate;
	}

	protected override void OnDetaching()
	{
		TheControl.RequestNavigate -= TheControl_RequestNavigate;
	}

	private void TheControl_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		e.Handled = true;
	}
}
