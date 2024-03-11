using System.Windows;

namespace CalculateX;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private const string _appGUID = "28CCE173-97C9-4B59-B483-2A5CEA4435B9";
	private readonly Shared.SingleInstance _singleInstance = new(_appGUID);

	// This is publicly modifiable so that tests can use a different service.
	public static IAlertService MyAlertService { get; set; } = new AlertService();


	protected override void OnStartup(StartupEventArgs e)
	{
		_singleInstance.PreventAnotherInstance(applicationName: "Calculate X", typeMainWindow: typeof(MainWindow));

		base.OnStartup(e);

		Shared.ApplicationRecoveryAndRestart.Register();

#if DEBUG
		if (e.Args.Length > 0)
		{
			// If the application was restarted, Args.First() == ApplicationRecoveryAndRestart.RestartSwitch.
			System.Diagnostics.Debugger.Break();
		}
#endif
	}

	protected override void OnExit(ExitEventArgs e)
	{
		Shared.ApplicationRecoveryAndRestart.Unregister();

		base.OnExit(e);
	}
}
