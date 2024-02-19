using System.Windows.Input;

namespace CalculateX.Views;

public partial class HelpPage : ContentPage
{
	public ICommand HyperlinkCommand => new Command<string>(async url => await Launcher.OpenAsync(url));

	public HelpPage()
	{
		InitializeComponent();

		BindingContext = this;
	}
}
