using System.Windows.Input;

namespace CalculateX.Views;

public partial class AboutPage : ContentPage
{
	public string AppVersion { get; set; }

	public ICommand HyperlinkCommand => new Command<string>(async url => await Launcher.OpenAsync(url));

	public AboutPage()
	{
		AppVersion = VersionTracking.CurrentVersion;

		InitializeComponent();

		BindingContext = this;
	}
}
