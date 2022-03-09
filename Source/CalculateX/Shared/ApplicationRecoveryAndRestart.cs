using System;
using System.Runtime.InteropServices;

namespace Shared;

/// <summary>
/// Use this class to register an application for restart after update or crash.
/// </summary>
/// <example>
/// public partial class App : Application
/// {
/// 	protected override void OnStartup(StartupEventArgs e)
/// 	{
/// 		base.OnStartup(e);
/// 		Shared.ApplicationRecoveryAndRestart.Register();
/// 
///		if (e.Args.Length > 0)
///		{
///			// If the application was restarted, Args.First() == ApplicationRecoveryAndRestart.RestartSwitch.
///		}
/// 	}
/// 
/// 	protected override void OnExit(ExitEventArgs e)
/// 	{
/// 		Shared.ApplicationRecoveryAndRestart.Unregister();
/// 		base.OnExit(e);
/// 	}
/// }
/// </example>
/// <see cref="https://docs.microsoft.com/en-us/windows/win32/recovery/using-application-recovery-and-restart"/>
/// <see cref="https://www.meziantou.net/application-recovery-and-restart.htm"/>
/// <see cref="https://www.codeproject.com/articles/74249/application-recovery-and-restart-c-quick-reference"/>
public static class ApplicationRecoveryAndRestart
{
	public const string RestartSwitch = "-restart";

	/// <summary>
	/// Register the application to restart in all failure cases.
	/// The application is run with the specified arguments.
	/// </summary>
	public static void Register()
	{
		ApplicationRecoveryAndRestartNativeMethods.RegisterApplicationRestart(commandLineArgs: RestartSwitch, ApplicationRecoveryAndRestartNativeMethods.RestartRestrictions.None);
	}

	/// <summary>
	/// Unregister the application before it exits.
	/// </summary>
	public static void Unregister()
	{
		ApplicationRecoveryAndRestartNativeMethods.UnregisterApplicationRestart();
	}


	// If we ever want to use this, we'll have to pass a RecoveryDelegate (or something with our own signature that calls it).
	//public static void RegisterCallback()
	//{
	//	ApplicationRecoveryAndRestartNativeMethods.RegisterApplicationRecoveryCallback(MyRecoveryCallback, parameter: IntPtr.Zero, pingInterval: 0 /*default 5s*/, flags: 0 /*required*/);
	//}

	//public static void UnregisterCallback()
	//{
	//	ApplicationRecoveryAndRestartNativeMethods.UnregisterApplicationRecoveryCallback();
	//}

	//Note: In the callback, we can change the restart command line by calling "register" again.
	//private static int MyRecoveryCallback(IntPtr data)
	//{
	//	/// If recovery is needed, we must call "in-progress" before the registered ping interval expires.
	//	//ApplicationRecoveryAndRestartNativeMethods.ApplicationRecoveryInProgress(out bool isCanceled);
	//	//if (isCanceled)
	//	//{
	//	//	return 0;
	//	//}

	//	ApplicationRecoveryAndRestartNativeMethods.ApplicationRecoveryFinished(success: true);
	//	return 0;
	//}
}


/// <summary>
/// P/Invoke for Win32 API.
/// </summary>
internal static class ApplicationRecoveryAndRestartNativeMethods
{
	[Flags]
	public enum RestartRestrictions
	{
		None = 0,
		NotOnCrash = 1,
		NotOnHang = 2,
		NotOnPatch = 4,
		NotOnReboot = 8,
	}

	[DllImport("kernel32.dll")]
	public static extern int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string commandLineArgs, RestartRestrictions flags);

	[DllImport("kernel32.dll")]
	public static extern int UnregisterApplicationRestart();
	
	//public delegate int RecoveryDelegate(IntPtr parameterData);

	//[DllImport("kernel32.dll")]
	//public static extern int RegisterApplicationRecoveryCallback(RecoveryDelegate recoveryCallback, IntPtr parameter, uint pingInterval, uint flags);

	//[DllImport("kernel32.dll")]
	//public static extern int UnregisterApplicationRecoveryCallback();

	//[DllImport("kernel32.dll")]
	//public static extern int ApplicationRecoveryInProgress(out bool canceled);

	//[DllImport("kernel32.dll")]
	//public static extern void ApplicationRecoveryFinished(bool success);


	// https://www.devx.com/dotnet/Article/36205
	//public delegate Int32 ApplicationRecoveryCallbackDelegate(RecoveryInformation parameter);

	//[DllImport("kernel32.dll")]
	//public static extern Int32 GetApplicationRecoveryCallback(
	//	 IntPtr process,
	//	 out ApplicationRecoveryCallbackDelegate recoveryCallback,
	//	 out RecoveryInformation parameter,
	//	 out UInt32 pingInterval,
	//	 out UInt32 flags);

	//[DllImport("kernel32.dll")]
	//public static extern Int32 GetApplicationRestartSettings(
	//	 IntPtr process,
	//	 StringBuilder commandLine,
	//	 ref UInt32 size,
	//	 out UInt32 flags);
}
