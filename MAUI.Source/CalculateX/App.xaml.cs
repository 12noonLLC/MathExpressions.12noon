using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace CalculateX;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		// Ensure that EVERY page will resize when the keyboard pops up.
		Current?.On<Microsoft.Maui.Controls
			.PlatformConfiguration.Android>()
			.UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

		InitializeComponent();

		MainPage = new AppShell();
	}
}
