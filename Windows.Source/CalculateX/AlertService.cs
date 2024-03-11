using System.Windows;

namespace CalculateX;

public class AlertService : IAlertService
{
	public bool PromptForConfirmation(string message, string title)
	{
		return (MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
	}
}
