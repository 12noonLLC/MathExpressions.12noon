namespace CalculateX.UnitTests;

public class AlertServiceUnitTests : IAlertService
{
	/// <summary>
	/// For unit tests, always confirm delete request.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="title"></param>
	/// <returns></returns>
	public bool PromptForConfirmation(string message, string title) => true;
}
