namespace CalculateX;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// No need to register pages in the TabBar.
		Routing.RegisterRoute(nameof(Views.WorkspacePage), typeof(Views.WorkspacePage));
		Routing.RegisterRoute(nameof(Views.VariablesPage), typeof(Views.VariablesPage));
		Routing.RegisterRoute(nameof(Views.HelpPage), typeof(Views.HelpPage));
		Routing.RegisterRoute(nameof(Views.AboutPage), typeof(Views.AboutPage));
	}
}
