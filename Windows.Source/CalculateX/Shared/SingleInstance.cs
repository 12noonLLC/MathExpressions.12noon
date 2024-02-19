using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Shared;

/// <summary>
/// Use this class to ensure that an application has only one instance.
/// </summary>
/// <example>
/// App.xaml:
///	// Delete StartupUri property.
/// 
/// App.xaml.cs:
///	// Generate unique GUID for this application.
///	private const string _appGUID = "3726B66F-A4ED-4D6C-9E4F-0FEFB2CC4B52";
/// 	private readonly Shared.SingleInstance _singleInstance = new(_appGUID);
///	
///	// Override OnStartup():
/// 	protected override void OnStartup(StartupEventArgs e)
/// 	{
/// 		_singleInstance.PreventAnotherInstance(applicationName: nameof(AlarmX), typeMainWindow: typeof(MainWindow));
/// 
/// 		base.OnStartup(e);
/// 	}
/// </example>
public class SingleInstance
{
	public bool IsFirstInstance => _createdNew;

	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification="<Instance Required>")]
	private readonly Mutex _mutexSingleInstance;
	private readonly string _guid;
	private readonly bool _createdNew;


	public SingleInstance(string guid)
	{
		_guid = guid;
		_mutexSingleInstance = new Mutex(initiallyOwned: true, name: _guid, out _createdNew);
	}


	/// <summary>
	/// Prevent multiple instances of the same executable from running.
	/// </summary>
	public void PreventAnotherInstance(string applicationName, Type typeMainWindow)
	{
		if (IsFirstInstance)
		{
			// Create the specified window and show it (which is what App's StartupUri would have done).
			Window? window = Activator.CreateInstance(typeMainWindow, nonPublic: true /*non-public ctor okay*/) as Window;
			window?.Show();
		}
		else
		{
			// If we can't reveal the other instance, just tell the user.
			if (!BringOtherInstanceToForeground())
			{
				MessageBox.Show($"{applicationName} is already running.", applicationName, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			// There is already an instance of this application with this GUID.
			Application.Current.Shutdown();
		}
	}


	/// <summary>
	/// Find another process with the same name as this one.
	/// Attempt to show/restore/foreground it.
	/// </summary>
	/// <returns>True if the other instance is visible (or there is none). False if another instance remains hidden.</returns>
	public static bool BringOtherInstanceToForeground()
	{
		Process? p = FindExistingProcess();
		if ((p is null) || (p.MainWindowHandle == IntPtr.Zero))
		{
			return true;
		}

		// restore it (in case it's minimized)
		NativeMethods.ShowWindow(p.MainWindowHandle, (int)NativeMethods.ShowWindowCommands.SW_RESTORE);

		// Tell the other instance to come to the foreground.
		NativeMethods.SetForegroundWindow(p.MainWindowHandle);

		return NativeMethods.IsWindowVisible(p.MainWindowHandle);
	}

	/// <summary>
	/// Returns a process with the same name (but not THIS process).
	/// </summary>
	/// <returns>Other running process</returns>
	private static Process? FindExistingProcess()
	{
		var processes = new List<Process>(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName));
		return processes.FirstOrDefault(p => p.Id != Environment.ProcessId);
	}
}

internal static class NativeMethods
{
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindowVisible(IntPtr hWnd);

	public enum ShowWindowCommands
	{
		SW_HIDE = 0,
		SW_SHOW = 5,
		SW_RESTORE = 9,
	}
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
