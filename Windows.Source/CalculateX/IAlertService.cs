namespace CalculateX;

public interface IAlertService
{
	/// <summary>
	/// Prompt user to confirm an action.
	/// The main app will implement this to call the system UI.
	/// Unit tests will use their own service that avoids a UI.
	/// </summary>
	/// <remarks>
	/// https://stackoverflow.com/a/72439742/4858
	/// </remarks>
	/// <param name="message"></param>
	/// <param name="title"></param>
	/// <returns>True if user selects Yes; false if Cancel or closed</returns>
	public bool PromptForConfirmation(string message, string title);
}
